//
// System.Web.Security.MembershipPasswordFormat
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.Security {
	public enum MembershipPasswordFormat {
		Clear,
		Hashed,
		Encrypted
	}
}
#endif

