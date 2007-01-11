//
// MembershipProviderTest.cs
//	- Unit tests for System.Web.Security.MembershipProvider
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration.Provider;
using System.Security.Principal;
using System.Web.Security;
using System.Text;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;

namespace MonoTests.System.Web.Security {

	class TestMembershipProvider : MembershipProvider {

		public override string ApplicationName {
			get {
				throw new Exception ("The method or operation is not implemented.");
			}
			set {
				throw new Exception ("The method or operation is not implemented.");
			}
		}

		public override bool ChangePassword (string username, string oldPassword, string newPassword)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool EnablePasswordReset {
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override bool EnablePasswordRetrieval {
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override int GetNumberOfUsersOnline ()
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string GetPassword (string username, string answer)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string GetUserNameByEmail (string email)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new Exception ("The method or operation is not implemented."); }
		}

		public override string ResetPassword (string username, string answer)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool UnlockUser (string userName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void UpdateUser (MembershipUser user)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool ValidateUser (string username, string password)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public byte[] Decrypt (byte[] data)
		{
			return base.DecryptPassword (data);
		}

		public byte[] Encrypt (byte[] data)
		{
			return base.EncryptPassword (data);
		}
	}

	[TestFixture]
	public class MembershipProviderTest {

		[Test]
		[ExpectedException (typeof (ProviderException))]
		public void EncryptPassword ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (EncryptOnLoad));
			t.Run ();
		}

		public static void EncryptOnLoad (Page p) 
		{
			TestMembershipProvider mp = new TestMembershipProvider ();
			string password = "";

			byte [] buffer = ASCIIEncoding.Default.GetBytes (password);

			mp.Encrypt (buffer);
		}
	}
}

#endif
