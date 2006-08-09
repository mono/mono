//
// EnterpriseServicesInteropOption.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0

// OK, I have to say, am not interested in implementing COM dependent stuff.

namespace System.Transactions
{
	public enum EnterpriseServicesInteropOption {
		None,
		Automatic,
		Full
	}
}

#endif
