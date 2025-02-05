﻿using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Schedule
{
    [Collection(nameof(NetworkCredentials))]
    public class SignPendingTransactionTests
    {
        private readonly NetworkCredentials _network;
        public SignPendingTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pending Transaction Sign: Can Sign a Pending Transfer Transaction")]
        public async Task CanSignAPendingTransferTransaction()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);

            var receipt = await pendingFx.Client.SignPendingTransactionAsync(pendingFx.Record.Pending.Id, pendingFx.SendingAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.NotEqual(TxId.None, receipt.Id);
            Assert.Null(receipt.Pending);
            Assert.NotNull(receipt.CurrentExchangeRate);
            Assert.InRange(receipt.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(receipt.NextExchangeRate);
            Assert.InRange(receipt.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
        }
        [Fact(DisplayName = "Pending Transaction Sign: Can Sign a Pending Transfer Transaction and get Record")]
        public async Task CanSignAPendingTransferTransactionAndGetRecord()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);

            var record = await pendingFx.Client.SignPendingTransactionWithRecordAsync(pendingFx.Record.Pending.Id, pendingFx.SendingAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.NotEqual(TxId.None, record.Id);
            Assert.Null(record.Pending);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.InRange(record.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(record.NextExchangeRate);
            Assert.InRange(record.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        }
        [Fact(DisplayName = "Pending Transaction Sign: Signing a Transaction alters the Endorsements List")]
        public async Task SigningATransactionAltersTheEndorsementsList()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network, fx =>
            {
                fx.TransferParams.Signatory = new Signatory(
                    fx.PrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fx.PayingAccount,
                        Administrator = fx.PublicKey,
                        Memo = fx.Memo,
                    });
            });
            var info = await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Id);
            Assert.Empty(info.Endorsements);

            await pendingFx.Client.SignPendingTransactionWithRecordAsync(pendingFx.Record.Pending.Id, pendingFx.SendingAccount.PrivateKey);
            info = await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Id);
            Assert.Single(info.Endorsements);
            Assert.Null(info.Executed);

            await pendingFx.Client.SignPendingTransactionWithRecordAsync(pendingFx.Record.Pending.Id, pendingFx.PayingAccount.PrivateKey);
            info = await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Id);
            Assert.Equal(2, info.Endorsements.Length);
            Assert.NotNull(info.Executed);
        }
    }
}