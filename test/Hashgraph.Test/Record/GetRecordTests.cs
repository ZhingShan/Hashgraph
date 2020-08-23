﻿using Hashgraph.Test.Fixtures;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetRecordTests
    {
        private readonly NetworkCredentials _network;
        public GetRecordTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Record: Can get Transaction Record")]
        public async Task CanGetTransactionRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var amount = Generator.Integer(20, 30);
            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, amount);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            var record = await fx.Client.GetTransactionRecordAsync(receipt.Id);
            Assert.NotNull(record);
            Assert.Equal(receipt.Id, record.Id);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
        }
        [Fact(DisplayName = "Get Record: Empty Transaction ID throws error.")]
        public async Task EmptyTransactionIdThrowsError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetTransactionRecordAsync(null);
            });
            Assert.Equal("transaction", ane.ParamName);
            Assert.StartsWith("Transaction is missing. Please check that it is not null.", ane.Message);
        }

        [Fact(DisplayName = "NETWORK V0.7.0 REGRESSION: Get Record: Invalid Transaction ID throws error.")]
        public async Task InvalidTransactionIdThrowsErrorNetVersion070Regression()
        {
            // The following unit test used to throw a precheck exception because
            // the COST_ANSWER would error out if the record did not exist, will
            // this be restored or is this the new (wasteful) behavior?  For now
            // mark this test as a regression and we will wait and see if it changes
            // in the next version.  If not, we will need to look into changing
            // the behavior of the library in an attempt to not waste client hBars.
            var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(InvalidTransactionIdThrowsError));
            Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);

            // [Fact(DisplayName = "Get Record: Invalid Transaction ID throws error.")]
            async Task InvalidTransactionIdThrowsError()
            {
                await using var client = _network.NewClient();
                var txId = new Proto.TransactionID { AccountID = new Proto.AccountID(_network.Payer), TransactionValidStart = new Proto.Timestamp { Seconds = 500, Nanos = 100 } }.ToTxId();
                var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                {
                    await client.GetTransactionRecordAsync(txId);
                });
                Assert.Equal(ResponseCode.RecordNotFound, pex.Status);
                Assert.StartsWith("Transaction Failed Pre-Check: RecordNotFound", pex.Message);
            }
        }
        [Fact(DisplayName = "Get Record: Can Get Record for Existing but Failed Transaction")]
        public async Task CanGetRecordForFailedTransaction()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = await _network.TinybarsFromGas(400),
                    FunctionName = "not_a_real_method",
                });
            });
            var record = await fx.Client.GetTransactionRecordAsync(tex.TxId);
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.ContractRevertExecuted, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        }
        [Fact(DisplayName = "Get Record: Can get Create Topic Record")]
        public async Task CanGetCreateTopicRecord()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var topicRecord = Assert.IsType<CreateTopicRecord>(record);
            Assert.Equal(fx.Record.Id, topicRecord.Id);
            Assert.Equal(fx.Record.Status, topicRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, topicRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, topicRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), topicRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, topicRecord.Concensus);
            Assert.Equal(fx.Record.Memo, topicRecord.Memo);
            Assert.Equal(fx.Record.Fee, topicRecord.Fee);
            Assert.Equal(fx.Record.Topic, topicRecord.Topic);
        }
        [Fact(DisplayName = "Get Record: Can get Submit Message Record")]
        public async Task CanGetSubmitMessageRecord()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var record1 = await fx.Client.SubmitMessageWithRecordAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            var record2 = await fx.Client.GetTransactionRecordAsync(record1.Id);
            var submitRecord = Assert.IsType<SubmitMessageRecord>(record2);
            Assert.Equal(record1.Id, submitRecord.Id);
            Assert.Equal(record1.Status, submitRecord.Status);
            Assert.Equal(record1.CurrentExchangeRate, submitRecord.CurrentExchangeRate);
            Assert.Equal(record1.NextExchangeRate, submitRecord.NextExchangeRate);
            Assert.Equal(record1.Hash.ToArray(), submitRecord.Hash.ToArray());
            Assert.Equal(record1.Concensus, submitRecord.Concensus);
            Assert.Equal(record1.Memo, submitRecord.Memo);
            Assert.Equal(record1.Fee, submitRecord.Fee);
            Assert.Equal(record1.SequenceNumber, submitRecord.SequenceNumber);
            Assert.Equal(record1.RunningHash.ToArray(), submitRecord.RunningHash.ToArray());
            Assert.Equal(record1.RunningHashVersion, submitRecord.RunningHashVersion);
        }
        [Fact(DisplayName = "Get Record: Can get Call Contract Record")]
        public async Task CanGetCallContractRecord()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var record1 = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "greet"
            }, ctx => ctx.Memo = "Call Contract");
            var record2 = await fx.Client.GetTransactionRecordAsync(record1.Id);
            var callRecord = Assert.IsType<CallContractRecord>(record2);
            Assert.Equal(record1.Id, callRecord.Id);
            Assert.Equal(record1.Status, callRecord.Status);
            Assert.Equal(record1.CurrentExchangeRate, callRecord.CurrentExchangeRate);
            Assert.Equal(record1.NextExchangeRate, callRecord.NextExchangeRate);
            Assert.Equal(record1.Hash.ToArray(), callRecord.Hash.ToArray());
            Assert.Equal(record1.Concensus, callRecord.Concensus);
            Assert.Equal(record1.Memo, callRecord.Memo);
            Assert.Equal(record1.Fee, callRecord.Fee);
            Assert.Equal(record1.CallResult.Error, callRecord.CallResult.Error);
            Assert.Equal(record1.CallResult.Bloom, callRecord.CallResult.Bloom);
            Assert.Equal(record1.CallResult.Gas, callRecord.CallResult.Gas);
            Assert.Equal(record1.CallResult.Events, callRecord.CallResult.Events);
            Assert.Equal(record1.CallResult.CreatedContracts, callRecord.CallResult.CreatedContracts);
            Assert.Equal(record1.CallResult.Result.As<string>(), callRecord.CallResult.Result.As<string>());
        }
        [Fact(DisplayName = "Get Record: Can get Create Topic Record")]
        public async Task CanGetCreateContractRecord()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.ContractRecord.Id);
            var createRecord = Assert.IsType<CreateContractRecord>(record);
            Assert.Equal(fx.ContractRecord.Id, createRecord.Id);
            Assert.Equal(fx.ContractRecord.Status, createRecord.Status);
            Assert.Equal(fx.ContractRecord.CurrentExchangeRate, createRecord.CurrentExchangeRate);
            Assert.Equal(fx.ContractRecord.NextExchangeRate, createRecord.NextExchangeRate);
            Assert.Equal(fx.ContractRecord.Hash.ToArray(), createRecord.Hash.ToArray());
            Assert.Equal(fx.ContractRecord.Concensus, createRecord.Concensus);
            Assert.Equal(fx.ContractRecord.Memo, createRecord.Memo);
            Assert.Equal(fx.ContractRecord.Fee, createRecord.Fee);
            Assert.Equal(fx.ContractRecord.Contract, createRecord.Contract);
            Assert.Equal(fx.ContractRecord.CallResult.Error, createRecord.CallResult.Error);
            Assert.Equal(fx.ContractRecord.CallResult.Bloom, createRecord.CallResult.Bloom);
            Assert.Equal(fx.ContractRecord.CallResult.Gas, createRecord.CallResult.Gas);
            Assert.Equal(fx.ContractRecord.CallResult.Events, createRecord.CallResult.Events);
            Assert.Equal(fx.ContractRecord.CallResult.CreatedContracts, createRecord.CallResult.CreatedContracts);
            Assert.Equal(fx.ContractRecord.CallResult.Result.Size, createRecord.CallResult.Result.Size);
        }
        [Fact(DisplayName = "Get Record: Can get Create Account Record")]
        public async Task CanGetCreateAccountRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var accountRecord = Assert.IsType<CreateAccountRecord>(record);
            Assert.Equal(fx.Record.Id, accountRecord.Id);
            Assert.Equal(fx.Record.Status, accountRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, accountRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, accountRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), accountRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, accountRecord.Concensus);
            Assert.Equal(fx.Record.Memo, accountRecord.Memo);
            Assert.Equal(fx.Record.Fee, accountRecord.Fee);
            Assert.Equal(fx.Record.Address, accountRecord.Address);
        }
        [Fact(DisplayName = "Get Record: Can get Create File Record")]
        public async Task CanGetCreateFileRecord()
        {
            await using var fx = await TestFile.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var FileRecord = Assert.IsType<FileRecord>(record);
            Assert.Equal(fx.Record.Id, FileRecord.Id);
            Assert.Equal(fx.Record.Status, FileRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, FileRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, FileRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), FileRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, FileRecord.Concensus);
            Assert.Equal(fx.Record.Memo, FileRecord.Memo);
            Assert.Equal(fx.Record.Fee, FileRecord.Fee);
            Assert.Equal(fx.Record.File, FileRecord.File);
        }
    }
}
