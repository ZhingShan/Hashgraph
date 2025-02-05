﻿using Proto;
using System;
using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the CreateAccountAsync Client method call.  
    /// It represents the details concerning a Hedera Network Account, including 
    /// the public key value to use in smart contract interaction.
    /// </summary>
    public sealed record AccountInfo
    {
        /// <summary>
        /// The Hedera address of this account.
        /// </summary>
        public Address Address { get; private init; }
        /// <summary>
        /// The identity of the Hedera Account in a form to be
        /// used with smart contracts.  This can also be the
        /// ID of a smart contract instance if this is the account
        /// associated with a smart contract.
        /// </summary>
        public string SmartContractId { get; private init; }
        /// <summary>
        /// <code>True</code> if this account has been deleted.
        /// Its existence in the network will cease after the expiration
        /// date for the account lapses.  It cannot participate in
        /// transactions except to extend the expiration/removal date.
        /// </summary>
        public bool Deleted { get; private init; }
        /// <summary>
        /// The Address of the Account to which this account has staked.
        /// </summary>
        public Address Proxy { get; private init; }
        /// <summary>
        /// The total number of tinybars that are proxy staked to this account.
        /// </summary>
        public long ProxiedToAccount { get; private init; }
        /// <summary>
        /// Account's Public Key (typically a single Ed25519 key).
        /// </summary>
        public Endorsement Endorsement { get; private init; }
        /// <summary>
        /// Account Balance in Tinybars
        /// </summary>
        public ulong Balance { get; private init; }
        /// <summary>
        /// Balances of tokens and assets associated with this account.
        /// </summary>
        public ReadOnlyCollection<TokenBalance> Tokens { get; private init; }
        /// <summary>
        /// <code>True</code> if any receipt of funds require
        /// a signature from this account.
        /// </summary>
        public bool ReceiveSignatureRequired { get; private init; }
        /// <summary>
        /// Incremental period for auto-renewal of the account. If
        /// account does not have sufficient funds to renew at the
        /// expiration time, it will be renewed for a period of time
        /// the remaining funds can support.  If no funds remain, the
        /// account will be deleted.
        /// </summary>
        public TimeSpan AutoRenewPeriod { get; private init; }
        /// <summary>
        /// The account expiration time, at which it will attempt
        /// to renew if sufficient funds remain in the account.
        /// </summary>
        public DateTime Expiration { get; private init; }
        /// <summary>
        /// A short description associated with the account.
        /// </summary>
        public string Memo { get; private init; }
        /// <summary>
        /// The number of assets (non fungible tokens) held
        /// by this account.
        /// </summary>
        public long AssetCount { get; private init; }
        /// <summary>
        /// Internal Constructor from Raw Response
        /// </summary>
        internal AccountInfo(Response response)
        {
            var info = response.CryptoGetInfo.AccountInfo;
            Address = info.AccountID.AsAddress();
            SmartContractId = info.ContractAccountID;
            Deleted = info.Deleted;
            Proxy = info.ProxyAccountID.AsAddress();
            ProxiedToAccount = info.ProxyReceived;
            Endorsement = info.Key.ToEndorsement();
            Balance = info.Balance;
            Tokens = info.TokenRelationships.ToBalances();
            ReceiveSignatureRequired = info.ReceiverSigRequired;
            AutoRenewPeriod = info.AutoRenewPeriod.ToTimeSpan();
            Expiration = info.ExpirationTime.ToDateTime();
            Memo = info.Memo;
            AssetCount = info.OwnedNfts;
        }
    }
}
