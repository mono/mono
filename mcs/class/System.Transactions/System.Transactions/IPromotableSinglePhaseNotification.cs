//
// IPromotableSinglePhaseNotification.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public interface IPromotableSinglePhaseNotification
	{
		void Initialize ();

		Transaction Promote ();

		void Rollback (SinglePhaseEnlistment enlistment);

		void SinglePhaseCommit (SinglePhaseEnlistment enlistment);
	}
}

#endif
