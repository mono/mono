//
// System.Web.Security.Membership
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public sealed class Membership {
		
		public static MembershipUser CreateUser (string username, string password)
		{
			return CreateUser (username, password, null);
		}
		
		public static MembershipUser CreateUser (string username, string password, string email)
		{
			MembershipCreateStatus status;
			MembershipUser usr = CreateUser (username, password, email, out status);
			if (usr == null)
				throw new MembershipCreateUserException (status);
			
			return usr;
		}
		
		public static MembershipUser CreateUser (string username, string password, string email, out MembershipCreateStatus status)
		{
			return Provider.CreateUser (username, password, email, out status);
		}
		
		public static bool DeleteUser (string username)
		{
			return Provider.DeleteUser (username);
		}
		
		[MonoTODO]
		public static string GeneratePassword (int length)
		{
			throw new NotImplementedException ();
		}
		
		public static MembershipUserCollection GetAllUsers ()
		{
			return Provider.GetAllUsers ();
		}
		
		public static int GetNumberOfUsersOnline ()
		{
			return Provider.GetNumberOfUsersOnline ();
		}
		
		public static MembershipUser GetUser ()
		{
			return GetUser (HttpContext.Current.User.Identity.Name, true);
		}
		
		public static MembershipUser GetUser (bool userIsOnline)
		{
			return GetUser (HttpContext.Current.User.Identity.Name, userIsOnline);
		}
		
		public static MembershipUser GetUser (string username)
		{
			return GetUser (username, false);
		}
		
		public static MembershipUser GetUser (string username, bool userIsOnline)
		{
			return Provider.GetUser (username, userIsOnline);
		}
		
		public static string GetUserNameByEmail (string email)
		{
			return Provider.GetUserNameByEmail (email);
		}
		
		public static void UpdateUser (MembershipUser user)
		{
			Provider.UpdateUser (user);
		}
		
		public static bool ValidateUser (string username, string password)
		{
			return Provider.ValidateUser (username, password);
		}
		
		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		public static bool EnablePasswordReset {
			get { return Provider.EnablePasswordReset; }
		}
		
		public static bool EnablePasswordRetrieval {
			get { return Provider.EnablePasswordRetrieval; }
		}
		
		public static bool RequiresQuestionAndAnswer {
			get { return Provider.RequiresQuestionAndAnswer; }
		}
		
		[MonoTODO]
		public static IMembershipProvider Provider {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static MembershipProviderCollection Providers {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static int UserIsOnlineTimeWindow {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif

