//
// SinglePhaseEnlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class SinglePhaseEnlistment : Enlistment
	{
		internal SinglePhaseEnlistment ()
		{
		}

		[MonoTODO]
		public void Aborted ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Aborted (Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Committed ()
		{
			throw new NotImplementedException ();
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
