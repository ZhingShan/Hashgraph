﻿using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates (replaces) the commissions (custom fees) associated with 
        /// a token, must be signed by the CommissionsEndorsment private key(s).
        /// </summary>
        /// <param name="token">
        /// The address of the token definition to update.
        /// </param>
        /// <param name="commissions">
        /// The list of commissions to apply to token transactions, may
        /// be a blank list or null, this list replaces the previous 
        /// list of commissions in full.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> UpdateCommissionsAsync(Address token, IEnumerable<ICommission>? commissions, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new TokenFeeScheduleUpdateTransactionBody(token, commissions), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Updates (replaces) the commissions (custom fees) associated with 
        /// a token, must be signed by the CommissionsEndorsment private key(s).
        /// </summary>
        /// <param name="token">
        /// The address of the token definition to update.
        /// </param>
        /// <param name="commissions">
        /// The list of commissions to apply to token transactions, may
        /// be a blank list or null, this list replaces the previous 
        /// list of commissions in full.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> UpdateCommissionsAsync(Address token, IEnumerable<ICommission>? commissions, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new TokenFeeScheduleUpdateTransactionBody(token, commissions), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Updates (replaces) the commissions (custom fees) associated with 
        /// a token, must be signed by the CommissionsEndorsment private key(s).
        /// </summary>
        /// <param name="token">
        /// The address of the token definition to update.
        /// </param>
        /// <param name="commissions">
        /// The list of commissions to apply to token transactions, may
        /// be a blank list or null, this list replaces the previous 
        /// list of commissions in full.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateCommissionsWithRecordAsync(Address token, IEnumerable<ICommission>? commissions, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new TokenFeeScheduleUpdateTransactionBody(token, commissions), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Updates (replaces) the commissions (custom fees) associated with 
        /// a token, must be signed by the CommissionsEndorsment private key(s).
        /// </summary>
        /// <param name="token">
        /// The address of the token definition to update.
        /// </param>
        /// <param name="commissions">
        /// The list of commissions to apply to token transactions, may
        /// be a blank list or null, this list replaces the previous 
        /// list of commissions in full.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateCommissionsWithRecordAsync(Address token, IEnumerable<ICommission>? commissions, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new TokenFeeScheduleUpdateTransactionBody(token, commissions), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
