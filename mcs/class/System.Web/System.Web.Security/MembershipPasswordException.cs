//
// System.Web.Security.MembershipPasswordException
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.Security {
	public class MembershipPasswordException : HttpException {
		public MembershipPasswordException () : base () {}
		public MembershipPasswordException (string message) : base (message) {}
		public MembershipPasswordException (string message, Exception innerException) : base (message, innerException) {}
	}
}
#endif

