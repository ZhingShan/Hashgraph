﻿using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestAccount : IAsyncDisposable
    {
        public ReadOnlyMemory<byte> PublicKey;
        public ReadOnlyMemory<byte> PrivateKey;
        public CreateAccountParams CreateParams;
        public CreateAccountRecord Record;
        public NetworkCredentials Network;
        public Client Client;

        public static async Task<TestAccount> CreateAsync(NetworkCredentials networkCredentials, Action<TestAccount> customize = null)
        {
            var fx = new TestAccount();
            networkCredentials.Output?.WriteLine("STARTING SETUP: Test Account Instance");
            (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
            fx.Network = networkCredentials;
            fx.Client = networkCredentials.NewClient();
            fx.CreateParams = new CreateAccountParams
            {
                Endorsement = fx.PublicKey,
                InitialBalance = (ulong)Generator.Integer(10, 20),
                Memo = Generator.String(10, 20)
            };
            customize?.Invoke(fx);
            fx.Record = await fx.Client.CreateAccountWithRecordAsync(fx.CreateParams, ctx =>
             {
                 ctx.Memo = "Test Account Instance: Creating Test Account on Network";
             });
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Account Instance");
            return fx;
        }

        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Test Account Instance");
            try
            {
                await Client.DeleteAccountAsync(Record.Address, Network.Payer, PrivateKey, ctx =>
                  {
                      ctx.Memo = "Test Account Instance Teardown: Attempting to delete Account from Network (if exists)";
                  });
            }
            catch
            {
                // Try to recover any funds if possible(unit tests are getting hBar Hungry)
                try
                {
                    var balance = await Client.GetAccountBalanceAsync(Record.Address);
                    if (balance > 500)
                    {
                        await Client.TransferAsync(Record.Address, Network.Payer, (long)balance, PrivateKey);
                    }
                }
                catch
                {
                    // Nothing else to try, move on.
                }
            }
            await Client.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED: Test Account Instance");
        }

        public static implicit operator Address(TestAccount fxAccount)
        {
            return fxAccount.Record.Address;
        }

        public static implicit operator Signatory(TestAccount fxAccount)
        {
            return new Signatory(fxAccount.PrivateKey);
        }
    }
}