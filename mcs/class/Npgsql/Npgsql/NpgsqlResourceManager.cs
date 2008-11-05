// NpgsqlResourceManager.cs
//
// Author:
//  Josh Cooley <jbnpgsql@tuxinthebox.net>
//
// Copyright (C) 2007, The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.Transactions;

namespace Npgsql
{
	internal interface INpgsqlResourceManager
	{
		void Enlist(INpgsqlTransactionCallbacks transactionCallbacks, byte[] txToken);
		byte[] Promote(INpgsqlTransactionCallbacks transactionCallbacks);
		void CommitWork(string txName);
        void RollbackWork(string txName);
	}

	internal class NpgsqlResourceManager : MarshalByRefObject, INpgsqlResourceManager
	{
        private readonly Dictionary<string, CommittableTransaction> _transactions = new Dictionary<string, CommittableTransaction>();

        #region INpgsqlTransactionManager Members

        public byte[] Promote(INpgsqlTransactionCallbacks callbacks)
        {
            CommittableTransaction tx = new CommittableTransaction();
            DurableResourceManager rm = new DurableResourceManager(this, callbacks, tx);
            byte[] token = TransactionInterop.GetTransmitterPropagationToken(tx);
            _transactions.Add(rm.TxName, tx);
            rm.Enlist(tx);
            return token;
        }

        public void Enlist(INpgsqlTransactionCallbacks callbacks, byte[] txToken)
        {
            DurableResourceManager rm = new DurableResourceManager(this, callbacks);
            rm.Enlist(txToken);
        }

        public void CommitWork(string txName)
        {
            CommittableTransaction tx;
            if (_transactions.TryGetValue(txName, out tx))
            {
                tx.Commit();
                _transactions.Remove(txName);
            }
        }

        public void RollbackWork(string txName)
        {
            CommittableTransaction tx;
            if (_transactions.TryGetValue(txName, out tx))
            {
                _transactions.Remove(txName);
                // you can fail to commit,
                // but you're not getting out
                // of a rollback.  Remove from list first.
                tx.Rollback();
            }
        }

        #endregion

		private class DurableResourceManager : IEnlistmentNotification
		{
			private CommittableTransaction _tx;
			private NpgsqlResourceManager _rm;
			private readonly INpgsqlTransactionCallbacks _callbacks;
			private string _txName;

			public DurableResourceManager(NpgsqlResourceManager rm, INpgsqlTransactionCallbacks callbacks)
				: this(rm, callbacks, null)
			{
			}

			public DurableResourceManager(NpgsqlResourceManager rm, INpgsqlTransactionCallbacks callbacks,
			                              CommittableTransaction tx)
			{
				_rm = rm;
				_tx = tx;
				_callbacks = callbacks;
			}

			public string TxName
			{
				get
				{
					// delay initialize since callback methods may be expensive
					if (_txName == null)
					{
						_txName = _callbacks.GetName();
					}
					return _txName;
				}
			}

			#region IEnlistmentNotification Members

			public void Commit(Enlistment enlistment)
			{
				_callbacks.CommitTransaction();
				// TODO: remove record of prepared
				enlistment.Done();
				_callbacks.Dispose();
			}

			public void InDoubt(Enlistment enlistment)
			{
				// not going to happen when enlisted durably
				throw new NotImplementedException();
			}

			public void Prepare(PreparingEnlistment preparingEnlistment)
			{
				_callbacks.PrepareTransaction();
				// TODO: record prepared
				preparingEnlistment.Prepared();
			}

			public void Rollback(Enlistment enlistment)
			{
				_callbacks.RollbackTransaction();
				// TODO: remove record of prepared
				enlistment.Done();
				_callbacks.Dispose();
			}

			#endregion

			private static readonly Guid rmGuid = new Guid("9e1b6d2d-8cdb-40ce-ac37-edfe5f880716");

			public Transaction Enlist(byte[] token)
			{
				return Enlist(TransactionInterop.GetTransactionFromTransmitterPropagationToken(token));
			}

			public Transaction Enlist(Transaction tx)
			{
				tx.EnlistDurable(rmGuid, this, EnlistmentOptions.None);
                return tx;
			}
		}
    }
}