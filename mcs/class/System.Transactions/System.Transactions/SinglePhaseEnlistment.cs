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
//		bool committed;
		Transaction tx;
		object abortingEnlisted;
		
		/// <summary>
		/// The empty ctor is used only for enlistments passed to resource managers when rolling back a transaction that
		///  has already been aborted by another resource manager; no need to retrigger (another) rollback.
		/// </summary>
		internal SinglePhaseEnlistment () {}

		internal SinglePhaseEnlistment (Transaction tx, object abortingEnlisted)
		{
			this.tx = tx;
			this.abortingEnlisted = abortingEnlisted;
		}

		public void Aborted ()
		{
			Aborted (null);
		}

		public void Aborted (Exception e)
		{
			if (tx != null)
				tx.Rollback (e, abortingEnlisted);
		}

		[MonoTODO]
		public void Committed ()
		{
			/* FIXME */
//			committed = true;
		}

		[MonoTODO ("Not implemented")]
		public void InDoubt ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void InDoubt (Exception e)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
