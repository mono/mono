//
// System.Web.Security.IMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Configuration.Provider;

namespace System.Web.Security {
	public interface IMembershipProvider : IProvider {
		bool ChangePassword (string name, string oldPwd, string newPwd);
		bool ChangePasswordQuestionAndAnswer (string name, string password, string newPwdQuestion, string newPwdAnswer);
		MembershipUser CreateUser (string username, string password, string email, out MembershipCreateStatus status);
		bool DeleteUser (string name);
		MembershipUserCollection GetAllUsers ();
		int GetNumberOfUsersOnline ();
		string GetPassword (string name, string answer);
		MembershipUser GetUser (string name, bool userIsOnline);
		string GetUserNameByEmail (string email);
		string ResetPassword (string name, string answer);
		void UpdateUser (MembershipUser user);
		bool ValidateUser (string name, string password);
		string ApplicationName { get; set; }
		bool EnablePasswordReset { get; }
		bool EnablePasswordRetrieval { get; }
		bool RequiresQuestionAndAnswer { get; }
	}
}
#endif

