//
// System.Web.Security.Membership
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public sealed class Membership {
		
		private Membership () {}
		
		public static MembershipUser CreateUser (string username, string password)
		{
			return CreateUser (username, password, null);
		}
		
		public static MembershipUser CreateUser (string username, string password, string email)
		{
			MembershipCreateStatus status;
			MembershipUser usr = CreateUser (username, password, email, null, null, true, out status);
			if (usr == null)
				throw new MembershipCreateUserException (status);
			
			return usr;
		}
		
		public static MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, out MembershipCreateStatus status)
		{
			return Provider.CreateUser (username, password, email, pwdQuestion, pwdAnswer, isApproved, out status);
		}
		
		public static bool DeleteUser (string username)
		{
			return Provider.DeleteUser (username, true);
		}
		
		public static bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			return Provider.DeleteUser (username, deleteAllRelatedData);
		}
		
		[MonoTODO]
		public static string GeneratePassword (int length)
		{
			throw new NotImplementedException ();
		}
		
		public static MembershipUserCollection GetAllUsers ()
		{
			int total;
			return GetAllUsers (1, int.MaxValue, out total);
		}
		
		public static MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.GetAllUsers (pageIndex, pageSize, out totalRecords);
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
		public static MembershipProvider Provider {
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

