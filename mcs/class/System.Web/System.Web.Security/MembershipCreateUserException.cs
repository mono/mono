//
// System.Web.Security.MembershipCreateUserException
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.Security {
	[MonoTODO ("make strings for the messages")]
	public class MembershipCreateUserException : HttpException {
		public MembershipCreateUserException (MembershipCreateStatus statusCode) : base (statusCode.ToString ())
		{
			this.statusCode = statusCode;
		}
		
		MembershipCreateStatus statusCode;
		public MembershipCreateStatus StatusCode {
			get { return statusCode; }
		}
	}
}
#endif

