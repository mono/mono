//
// System.Web.Security.MembershipSortOptions
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_2_0
namespace System.Web.Security {
	public enum MembershipSortOptions {
		UserName,
		CreationDate,
		LastActivityDate,
		LastLoginDate,
		LastPasswordChangeDate,
		Email		
	}
}
#endif

