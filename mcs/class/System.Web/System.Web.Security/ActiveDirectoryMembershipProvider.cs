//
// System.Web.Security.ActiveDirectoryMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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


using System.Collections.Specialized;

namespace System.Web.Security {

	[MonoTODO ("that's only a stub")]
	public class ActiveDirectoryMembershipProvider : MembershipProvider {
		
		[MonoTODO ("Not implemented")]
		public override bool ChangePassword (string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public virtual string GeneratePassword ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public override int GetNumberOfUsersOnline ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override string GetPassword (string username, string passwordAnswer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override string GetUserNameByEmail (string email)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override string ResetPassword (string username, string passwordAnswer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override void UpdateUser (MembershipUser user)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override bool ValidateUser (string username, string password)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override bool UnlockUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		public override string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO("Not implemented")]
		public ActiveDirectoryConnectionProtection CurrentConnectionProtection {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override bool EnablePasswordReset {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override bool EnablePasswordRetrieval {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO("Not implemented")]
		public bool EnableSearchMethods {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override MembershipPasswordFormat PasswordFormat {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override bool RequiresQuestionAndAnswer {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override bool RequiresUniqueEmail {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override int MaxInvalidPasswordAttempts {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override int MinRequiredNonAlphanumericCharacters {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override int MinRequiredPasswordLength {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override int PasswordAttemptWindow {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO("Not implemented")]
		public int PasswordAnswerAttemptLockoutDuration {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO("Not implemented")]
		public override string PasswordStrengthRegularExpression {
			get { throw new NotImplementedException (); }
		}
	}
}

