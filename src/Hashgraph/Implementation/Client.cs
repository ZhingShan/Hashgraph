﻿using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Hashgraph.Test")]
namespace Hashgraph
{
    /// <summary>
    /// Hedera Network Client
    /// </summary>
    /// <remarks>
    /// This component facilitates interaction with the Hedera Network.  
    /// It manages the communication channels with the network and 
    /// serialization of requests and responses.  This library generally 
    /// shields the client code from directly interacting with the 
    /// underlying protobuf communication layer but does provide hooks 
    /// allowing advanced low-level manipulation of messages if necessary.
    /// </remarks>
    public sealed partial class Client
    {
        private async Task<NetworkResult> ExecuteTransactionAsync(INetworkTransaction transaction, Action<IContext>? configure, bool includeRecord, params Signatory?[] extraSignatories)
        {
            var result = new NetworkResult();
            await using var context = CreateChildContext(configure);
            var gateway = context.Gateway;
            if (gateway is null)
            {
                throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
            }
            var signatory = context.GatherSignatories(extraSignatories);
            var schedule = signatory.GetSchedule();
            if (schedule is not null)
            {
                var scheduledTransactionBody = transaction.CreateSchedulableTransactionBody();
                scheduledTransactionBody.TransactionFee = (ulong)context.FeeLimit;
                transaction = new ScheduleCreateTransactionBody
                {
                    ScheduledTransactionBody = scheduledTransactionBody,
                    AdminKey = schedule.Administrator is null ? null : new Key(schedule.Administrator),
                    PayerAccountID = schedule.PendingPayer is null ? null : new AccountID(schedule.PendingPayer),
                    Memo = schedule.Memo ?? ""
                };
            }
            var transactionBody = transaction.CreateTransactionBody();
            transactionBody.TransactionFee = (ulong)context.FeeLimit;
            transactionBody.NodeAccountID = new AccountID(gateway);
            transactionBody.TransactionID = result.TransactionID = context.GetOrCreateTransactionID();
            transactionBody.TransactionValidDuration = new Duration(context.TransactionDuration);
            transactionBody.Memo = context.Memo ?? "";
            var invoice = new Invoice(transactionBody);
            await signatory.SignAsync(invoice).ConfigureAwait(false);
            var signedTransaction = new Transaction
            {
                SignedTransactionBytes = invoice.GenerateSignedTransactionFromSignatures(context.SignaturePrefixTrimLimit).ToByteString()
            };
            var precheck = await context.ExecuteSignedRequestWithRetryImplementationAsync(signedTransaction, transaction.InstantiateNetworkRequestMethod, getResponseCode).ConfigureAwait(false);
            if (precheck.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
            {
                var responseCode = (ResponseCode)precheck.NodeTransactionPrecheckCode;
                throw new PrecheckException($"Transaction Failed Pre-Check: {responseCode}", result.TransactionID.AsTxId(), responseCode, precheck.Cost);
            }
            var receipt = result.Receipt = await context.GetReceiptAsync(result.TransactionID).ConfigureAwait(false);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format(transaction.TransactionExceptionMessage, receipt.Status), result.TransactionID.AsTxId(), (ResponseCode)receipt.Status);
            }
            if (includeRecord)
            {
                result.Record = await GetTransactionRecordAsync(context, result.TransactionID).ConfigureAwait(false);
            }
            return result;

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
        private async Task<Response> ExecuteQueryAsync(INetworkQuery query, Action<IContext>? configure, long supplementalCost = 0)
        {
            await using var context = CreateChildContext(configure);
            return await ExecuteQueryInContextAsync(query, context, supplementalCost).ConfigureAwait(false);

        }
        private async Task<Response> ExecuteQueryInContextAsync(INetworkQuery query, GossipContextStack context, long supplementalCost)
        {
            var envelope = query.CreateEnvelope();
            query.SetHeader(new QueryHeader
            {
                Payment = new Transaction { SignedTransactionBytes = ByteString.Empty },
                ResponseType = ResponseType.CostAnswer
            });
            var response = await executeUnsignedAskQuery().ConfigureAwait(false);
            ulong cost = response.ResponseHeader?.Cost ?? 0UL;
            if (cost > 0)
            {
                var transactionId = context.GetOrCreateTransactionID();
                query.SetHeader(await createSignedQueryHeader((long)cost + supplementalCost, transactionId).ConfigureAwait(false));
                response = await executeSignedQuery().ConfigureAwait(false);
                response.Validate(transactionId);
            }
            return response;

            async Task<Response> executeUnsignedAskQuery()
            {
                var answer = await context.ExecuteNetworkRequestWithRetryAsync(envelope, query.InstantiateNetworkRequestMethod, shouldRetryRequest).ConfigureAwait(false);
                var code = answer.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                if (code != ResponseCodeEnum.Ok)
                {
                    throw new PrecheckException($"Transaction Failed Pre-Check: {code}", TxId.None, (ResponseCode)code, 0);
                }
                return answer;

                static bool shouldRetryRequest(Response response)
                {
                    return ResponseCodeEnum.Busy == response.ResponseHeader?.NodeTransactionPrecheckCode;
                }
            }

            async Task<QueryHeader> createSignedQueryHeader(long queryFee, TransactionID transactionId)
            {
                var payer = context.Payer;
                if (payer is null)
                {
                    throw new InvalidOperationException("The Payer address has not been configured. Please check that 'Payer' is set in the Client context.");
                }
                var signatory = context.Signatory as ISignatory;
                if (signatory is null)
                {
                    throw new InvalidOperationException("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that 'Signatory' is set in the Client context.");
                }
                var gateway = context.Gateway;
                if (gateway is null)
                {
                    throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
                }
                var feeLimit = context.FeeLimit;
                if (feeLimit < queryFee)
                {
                    throw new InvalidOperationException($"The user specified fee limit is not enough for the anticipated query required fee of {queryFee:n0} tinybars.");
                }
                var transactionBody = new TransactionBody
                {
                    TransactionID = transactionId,
                    NodeAccountID = new AccountID(gateway),
                    TransactionFee = (ulong)context.FeeLimit,
                    TransactionValidDuration = new Duration(context.TransactionDuration),
                    Memo = context.Memo ?? ""
                };
                var transfers = new TransferList();
                transfers.AccountAmounts.Add(new AccountAmount { AccountID = new AccountID(payer), Amount = -queryFee });
                transfers.AccountAmounts.Add(new AccountAmount { AccountID = new AccountID(gateway), Amount = queryFee });
                transactionBody.CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers };
                var invoice = new Invoice(transactionBody);
                await signatory.SignAsync(invoice).ConfigureAwait(false);
                var signedTransactionBytes = invoice.GenerateSignedTransactionFromSignatures(context.SignaturePrefixTrimLimit).ToByteString();
                return new QueryHeader
                {
                    Payment = new Transaction
                    {
                        SignedTransactionBytes = signedTransactionBytes
                    }
                };
            }

            Task<Response> executeSignedQuery()
            {
                return context.ExecuteSignedRequestWithRetryImplementationAsync(envelope, query.InstantiateNetworkRequestMethod, getResponseCode);

                ResponseCodeEnum getResponseCode(Response response)
                {
                    return response.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                }
            }
        }
        /// <summary>
        /// Internal Helper function to retrieve the transaction record provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionRecord> GetTransactionRecordAsync(GossipContextStack context, TransactionID transactionRecordId)
        {
            var response = await ExecuteQueryInContextAsync(new TransactionGetRecordQuery(transactionRecordId, false), context, 0).ConfigureAwait(false);
            var precheckCode = response.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            if(precheckCode == ResponseCodeEnum.Ok)
            {
                return response.TransactionGetRecord.TransactionRecord;
            }
            else if(precheckCode == ResponseCodeEnum.InsufficientTxFee)
            {
                throw new TransactionException("The Network Changed the price of Retrieving a Record while attempting to retrieve this record, the transaction likely succeeded, please try to retrieve the record again.", transactionRecordId.AsTxId(), (ResponseCode)precheckCode);
            }
            else
            {
                throw new TransactionException("Unable to retrieve transaction record.", transactionRecordId.AsTxId(), (ResponseCode)precheckCode);
            }
        }
    }
}
