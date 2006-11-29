//
// Enlistment.cs
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
	public class Enlistment
	{
		internal bool done;

		internal Enlistment ()
		{
			done = false;
		}

		public void Done ()
		{
			done = true;
		}
	}
}

#endif
