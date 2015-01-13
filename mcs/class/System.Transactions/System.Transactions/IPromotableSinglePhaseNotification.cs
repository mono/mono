//
// IPromotableSinglePhaseNotification.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	public interface IPromotableSinglePhaseNotification : ITransactionPromoter
	{
		void Initialize ();

		void Rollback (SinglePhaseEnlistment enlistment);

		void SinglePhaseCommit (SinglePhaseEnlistment enlistment);
	}
}

