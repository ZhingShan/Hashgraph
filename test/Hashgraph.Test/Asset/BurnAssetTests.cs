﻿using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens
{
    [Collection(nameof(NetworkCredentials))]
    public class BurnAssetTests
    {
        private readonly NetworkCredentials _network;
        public BurnAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Asset Coins")]
        public async Task CanBurnAssetsAsync()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var amountToDestory = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestory);
            var serialNumbers = Enumerable.Range(1, amountToDestory).Select(i => (long)i);

            var receipt = await fxAsset.Client.BurnAssetsAsync(fxAsset.Record.Token, serialNumbers, fxAsset.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(expectedCirculation, receipt.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0ul, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);

            Assert.Empty(info.Commissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Asset Coins and get Record")]
        public async Task CanBurnAssetsAsyncAndGetRecord()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var amountToDestory = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestory);
            var serialNumbers = Enumerable.Range(1, amountToDestory).Select(i => (long)i);

            var record = await fxAsset.Client.BurnAssetsWithRecordAsync(fxAsset, serialNumbers, fxAsset.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(expectedCirculation, record.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Single Asset")]
        public async Task CanBurnSingleAsset()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - 1);
            var serialNumbers = fxAsset.Metadata[0];
            var asset = new Asset(fxAsset, 1);

            var receipt = await fxAsset.Client.BurnAssetAsync(asset, fxAsset.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(expectedCirculation, receipt.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0ul, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Commissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Single Asset and get Record")]
        public async Task CanBurnSingleAssetAsyncAndGetRecord()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - 1);
            var serialNumbers = fxAsset.Metadata[0];
            var asset = new Asset(fxAsset, 1);

            var record = await fxAsset.Client.BurnAssetWithRecordAsync(asset, fxAsset.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(expectedCirculation, record.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Asset Coins and get Record Without Extra Signatory")]
        public async Task CanBurnAssetsAsyncAndGetRecordWithoutExtraSignatory()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var amountToDestory = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestory);
            var serialNumbers = Enumerable.Range(1, amountToDestory).Select(i => (long)i);

            var record = await fxAsset.Client.BurnAssetsWithRecordAsync(fxAsset, serialNumbers, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.SupplyPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(expectedCirculation, record.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Burn Assets: Can Burn Asset Coins from Any Account with Supply Key")]
        public async Task CanBurnAssetCoinsFromAnyAccountWithSupplyKey()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 60_000_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var amountToDestory = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestory);
            var serialNumbers = Enumerable.Range(1, amountToDestory).Select(i => (long)i);

            var receipt = await fxAccount.Client.BurnAssetsAsync(fxAsset, serialNumbers, fxAsset.SupplyPrivateKey, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(expectedCirculation, receipt.Circulation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(expectedCirculation, info.Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Burn Asset Record Includes Asset Transfers")]
        public async Task BurnAssetRecordIncludesAssetTransfers()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var amountToDestroy = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestroy);
            var serialNumbers = Enumerable.Range(1, amountToDestroy).Select(i => (long)i);

            var record = await fxAsset.Client.BurnAssetsWithRecordAsync(fxAsset, serialNumbers, fxAsset.SupplyPrivateKey);
            Assert.Empty(record.TokenTransfers);
            Assert.Equal(amountToDestroy, record.AssetTransfers.Count);
            Assert.Empty(record.Commissions);
            Assert.Equal(expectedCirculation, record.Circulation);

            for (int ssn = 1; ssn <= amountToDestroy; ssn++)
            {
                var asset = new Asset(fxAsset.Record.Token, ssn);
                var xfer = record.AssetTransfers.FirstOrDefault(x => x.Asset == asset);
                Assert.NotNull(xfer);
                Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xfer.From);
                Assert.Equal(Address.None, xfer.To);
            }
            Assert.Equal(expectedCirculation, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(expectedCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Can Not Burn More Assets than are in Circulation")]
        public async Task CanNotBurnMoreAssetsThanAreInCirculation()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

            var serialNumbers = Enumerable.Range(1, fxAsset.Metadata.Length + 1).Select(i => (long)i);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.BurnAssetsAsync(fxAsset, serialNumbers, fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidNftId, tex.Status);
            Assert.StartsWith("Unable to Burn Token Coins, status: InvalidNftId", tex.Message);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Burning Coins Requires Supply Key to Sign Transaction")]
        public async Task BurningCoinsRequiresSupplyKeyToSignTransaction()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

            var amountToDestroy = fxAsset.Metadata.Length / 3 + 1;
            var serialNumbers = Enumerable.Range(1, amountToDestroy).Select(i => (long)i);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.BurnAssetsAsync(fxAsset, serialNumbers);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Burn Token Coins, status: InvalidSignature", tex.Message);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Can Not Burn More Assets than Treasury Has")]
        public async Task CanNotBurnMoreAssetsThanTreasuryHas()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var amountToDestroy = 2 * fxAsset.Metadata.Length / 3;
            var amountToTransfer = amountToDestroy;
            var expectedTreasury = fxAsset.Metadata.Length - amountToTransfer;
            var serialNumbersDestroyed = Enumerable.Range(1, amountToDestroy).Select(i => (long)i);
            var serialNumbersTransfered = Enumerable.Range(amountToDestroy / 2, amountToTransfer).Select(i => (long)i);

            var transferParams = new TransferParams
            {
                AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
                Signatory = fxAsset.TreasuryAccount
            };

            await fxAsset.Client.TransferAsync(transferParams);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.BurnAssetsAsync(fxAsset, serialNumbersDestroyed, fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.TreasuryMustOwnBurnedNft, tex.Status);
            Assert.StartsWith("Unable to Burn Token Coins, status: TreasuryMustOwnBurnedNft", tex.Message);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Can Not Burn An Asset The Treasury Does Not Own")]
        public async Task CanBurnAnAssetTheTreasuryDoesNotOwn()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var amountToTransfer = 2 * fxAsset.Metadata.Length / 3;
            var expectedTreasury = fxAsset.Metadata.Length - amountToTransfer;
            var serialNumbersTransfered = Enumerable.Range(1, amountToTransfer).Select(i => (long)i);

            var transferParams = new TransferParams
            {
                AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
                Signatory = fxAsset.TreasuryAccount
            };

            await fxAsset.Client.TransferAsync(transferParams);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.BurnAssetsAsync(fxAsset, new long[] { 1 }, fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.TreasuryMustOwnBurnedNft, tex.Status);
            Assert.StartsWith("Unable to Burn Token Coins, status: TreasuryMustOwnBurnedNft", tex.Message);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Can Not Burn Single Asset The Treasury Does Not Own")]
        public async Task CanNotBurnSingleAssetTheTreasuryDoesNotOwn()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var amountToTransfer = 2 * fxAsset.Metadata.Length / 3;
            var expectedTreasury = fxAsset.Metadata.Length - amountToTransfer;
            var serialNumbersTransfered = Enumerable.Range(1, amountToTransfer).Select(i => (long)i);

            var transferParams = new TransferParams
            {
                AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
                Signatory = fxAsset.TreasuryAccount
            };

            await fxAsset.Client.TransferAsync(transferParams);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.BurnAssetAsync(new Asset(fxAsset.Record.Token, 1), fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.TreasuryMustOwnBurnedNft, tex.Status);
            Assert.StartsWith("Unable to Burn Token Coins, status: TreasuryMustOwnBurnedNft", tex.Message);

            Assert.Equal((ulong)amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal((ulong)expectedTreasury, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Burn Assets: Can Schedule Burn Asset Coins")]
        public async Task CanScheduleBurnAssetCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 40_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var amountToDestory = fxAsset.Metadata.Length / 3 + 1;
            var expectedCirculation = (ulong)(fxAsset.Metadata.Length - amountToDestory);
            var serialNumbers = Enumerable.Range(1, amountToDestory).Select(i => (long)i);

            var pendingReceipt = await fxAsset.Client.BurnAssetsAsync(
                fxAsset.Record.Token,
                serialNumbers,
                new Signatory(
                    fxAsset.SupplyPrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    }));

            Assert.Equal(ResponseCode.Success, pendingReceipt.Status);
            // This should be considered a network bug.
            Assert.Equal(0UL, pendingReceipt.Circulation);

            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length);

            var signingReceipt = await fxPayer.Client.SignPendingTransactionAsync(pendingReceipt.Pending.Id, fxPayer.PrivateKey); // As TokenReceipt
            Assert.Equal(ResponseCode.Success, signingReceipt.Status);

            var executedReceipt = await fxPayer.Client.GetReceiptAsync(pendingReceipt.Pending.TxId) as TokenReceipt;
            Assert.Equal(ResponseCode.Success, executedReceipt.Status);
            Assert.Equal(expectedCirculation, executedReceipt.Circulation);

            var executedRecord = await fxPayer.Client.GetTransactionRecordAsync(pendingReceipt.Pending.TxId) as TokenRecord;
            Assert.Equal(ResponseCode.Success, executedRecord.Status);
            Assert.Equal(expectedCirculation, executedRecord.Circulation);

            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, expectedCirculation);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(expectedCirculation, info.Circulation);
            Assert.Equal(0ul, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);

            Assert.Empty(info.Commissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
    }
}
