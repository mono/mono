#if NET_2_0

using System;
using System.Web.Security;
using System.Collections.Specialized;
using System.Text;

namespace MonoTests.SystemWeb.Framework
{
	public sealed class FakeMembershipProvider : MembershipProvider
	{
		public bool requiresQuestionAndAnswer = true;

		public override void Initialize (string name, NameValueCollection config)
		{
			base.Initialize (name, config);
		}

		public override string ApplicationName
		{
			get { return "/"; }
			set { }
		}

		public override bool EnablePasswordReset
		{
			get { return true; }
		}


		public override bool EnablePasswordRetrieval
		{
			get { return true; }
		}


		public override bool RequiresQuestionAndAnswer
		{
			get { return requiresQuestionAndAnswer; }
		}


		public override bool RequiresUniqueEmail
		{
			get { return true; }
		}


		public override int MaxInvalidPasswordAttempts
		{
			get { return 10; }
		}


		public override int PasswordAttemptWindow
		{
			get { return 10; }
		}


		public override MembershipPasswordFormat PasswordFormat
		{
			get { return MembershipPasswordFormat.Clear; }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { return 10; }
		}

		public override int MinRequiredPasswordLength
		{
			get { return 10; }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { return ""; }
		}

		public override bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			if (username == "WrongUser")
				return false;
			return true;
		}

		public override bool ChangePasswordQuestionAndAnswer (string username,
			string password,
			string newPwdQuestion,
			string newPwdAnswer)
		{
			return true;
		}

		public override MembershipUser CreateUser (string username,
			string password,
			string email,
			string passwordQuestion,
			string passwordAnswer,
			bool isApproved,
			object providerUserKey,
			out MembershipCreateStatus status)
		{

			if (username == "duplicate") {
				status = MembershipCreateStatus.DuplicateUserName;
				return null;
			}

			if (username == "incorrect") {
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}
			
			if (password == "incorrect") {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (email == "incorrect") {
				status = MembershipCreateStatus.InvalidEmail;
				return null;
			}

			if (email == "duplicate") {
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}

			if (passwordQuestion == "incorrect") {
				status = MembershipCreateStatus.InvalidQuestion;
				return null;
			}

			if (passwordAnswer == "incorrect") {
				status = MembershipCreateStatus.InvalidAnswer;
				return null;
			}

			MembershipUser u = new MembershipUser ("FakeProvider", username, "", email, passwordQuestion, "", isApproved, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
			status = MembershipCreateStatus.Success;
			return u;
		}

		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			return true;
		}

		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			return new MembershipUserCollection ();
		}

		public override int GetNumberOfUsersOnline ()
		{
			return 123;
		}

		public override string GetPassword (string username, string answer)
		{
			return "123";
		}

		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			MembershipUser u = new MembershipUser ("FakeProvider", username, null, "name@email.com", "", "", true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
			return u;
		}

		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			MembershipUser u = new MembershipUser ("", "", providerUserKey, "name@email.com", "", "", true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
			return u;
		}

		public override bool UnlockUser (string username)
		{
			return true;
		}

		public override string GetUserNameByEmail (string email)
		{
			return "heh";
		}

		public override string ResetPassword (string username, string answer)
		{
			return "123";
		}

		public override void UpdateUser (MembershipUser user)
		{
		}

		public override bool ValidateUser (string username, string password)
		{
			if (password == "incorrect")
				return false;

			return true;
		}

		private bool CheckPassword (string password, string dbpassword)
		{
			return true;
		}

		private string EncodePassword (string password)
		{
			return "123";
		}

		private string UnEncodePassword (string encodedPassword)
		{
			return "123";
		}

		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			return new MembershipUserCollection ();
		}

		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			return new MembershipUserCollection ();
		}
	}
}

#endif
