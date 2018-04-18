//
// System.Web.ClientServices.Providers.ClientFormsAuthenticationMembershipProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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

using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace System.Web.ClientServices.Providers
{
	public class ClientFormsAuthenticationMembershipProvider : System.Web.Security.MembershipProvider
	{
#pragma warning disable 67
		public event EventHandler <UserValidatedEventArgs> UserValidated;
#pragma warning restore 67
		
		public override string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override bool EnablePasswordReset {
			get { throw new NotImplementedException (); }
		}
		
		public override bool EnablePasswordRetrieval {
			get { throw new NotImplementedException (); }
		}
		
		public override int MaxInvalidPasswordAttempts {
			get { throw new NotImplementedException (); }
		}
		
		public override int MinRequiredNonAlphanumericCharacters {
			get { throw new NotImplementedException (); }
		}
		
		public override int MinRequiredPasswordLength {
			get { throw new NotImplementedException (); }
		}		

		public override int PasswordAttemptWindow {
			get { throw new NotImplementedException (); }
		}
		
		public override MembershipPasswordFormat PasswordFormat {
			get { throw new NotImplementedException (); }
		}
		
		public override string PasswordStrengthRegularExpression {
			get { throw new NotImplementedException (); }
		}
		
		public override bool RequiresQuestionAndAnswer {
			get { throw new NotImplementedException (); }
		}
		
		public override bool RequiresUniqueEmail {
			get { throw new NotImplementedException (); }
		}
		
		public ClientFormsAuthenticationMembershipProvider ()
		{
			throw new NotImplementedException ();
		}
		
		public static bool ValidateUser (string username, string password, string serviceUri)
		{
			throw new NotImplementedException ();
		}
		
		public override bool ChangePassword (string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException ();
		}
		
		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved,
							   object providerUserKey, out MembershipCreateStatus status)
		{
			throw new NotImplementedException ();
		}
		
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetNumberOfUsersOnline ()
		{
			throw new NotImplementedException ();
		}
		
		public override string GetPassword (string username, string answer)
		{
			throw new NotImplementedException ();
		}		
		
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException ();
		}
		
		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			throw new NotImplementedException ();
		}
		
		public override string GetUserNameByEmail (string email)
		{
			throw new NotImplementedException ();
		}
		
		public override void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		public void Logout ()
		{
			throw new NotImplementedException ();
		}
		
		public override string ResetPassword (string username, string answer)
		{
			throw new NotImplementedException ();
		}
		
		public override bool UnlockUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		public override void UpdateUser (System.Web.Security.MembershipUser user)
		{
			throw new NotImplementedException ();
		}
		
		public override bool ValidateUser (string username, string password)
		{
			throw new NotImplementedException ();
		}
		
		public bool ValidateUser (string username, string password, bool rememberMe)
		{
			throw new NotImplementedException ();
		}
	}
}
