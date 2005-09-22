//
// PreparingEnlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

namespace System.Transactions
{
	public class PreparingEnlistment : Enlistment
	{
		internal PreparingEnlistment ()
		{
		}

		[MonoTODO]
		public void ForceRollback ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ForceRollback (Exception ex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Prepared ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte [] RecoveryInformation ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
