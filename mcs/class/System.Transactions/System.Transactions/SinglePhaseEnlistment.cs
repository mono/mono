//
// SinglePhaseEnlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class SinglePhaseEnlistment : Enlistment
	{
		bool committed = false;
		Transaction tx;
		ISinglePhaseNotification enlisted;
		
		internal SinglePhaseEnlistment (Transaction tx, ISinglePhaseNotification enlisted)
		{
			this.tx = tx;
			this.enlisted = enlisted;
		}

		[MonoTODO]
		public void Aborted ()
		{
			Aborted (null);
		}

		[MonoTODO]
		public void Aborted (Exception e)
		{
			tx.Rollback (e, enlisted);
		}

		[MonoTODO]
		public void Committed ()
		{
			/* FIXME */
			committed = true;
		}

		[MonoTODO]
		public void InDoubt ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InDoubt (Exception e)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
