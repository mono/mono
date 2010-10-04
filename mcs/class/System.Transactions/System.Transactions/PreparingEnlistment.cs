//
// PreparingEnlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

#if NET_2_0

using System.Threading;

namespace System.Transactions
{
	public class PreparingEnlistment : Enlistment
	{
		bool prepared = false;
		Transaction tx;
		IEnlistmentNotification enlisted;
		WaitHandle waitHandle;

		internal PreparingEnlistment (Transaction tx, IEnlistmentNotification enlisted)
		{
			this.tx = tx;
			this.enlisted = enlisted;
			waitHandle = new ManualResetEvent (false);
		}

		public void ForceRollback ()
		{
			ForceRollback (null);
		}

		[MonoTODO]
		public void ForceRollback (Exception ex)
		{
			tx.Rollback (ex, enlisted);
			/* See test RMFail2 */
			((ManualResetEvent) waitHandle).Set ();
		}

		[MonoTODO]
		public void Prepared ()
		{
			prepared = true;
			/* See test RMFail2 */
			((ManualResetEvent) waitHandle).Set ();
		}

		[MonoTODO]
		public byte [] RecoveryInformation ()
		{
			throw new NotImplementedException ();
		}

		internal bool IsPrepared {
			get { return prepared; }
		}

		internal WaitHandle WaitHandle {
			get { return waitHandle; }
		}

		internal IEnlistmentNotification EnlistmentNotification
		{
			get { return enlisted; }
		}
	}
}

#endif
