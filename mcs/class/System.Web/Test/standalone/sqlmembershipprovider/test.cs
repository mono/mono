using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Web.Security;
using System.Security;
using System.Security.Principal;

public class Test {
	static void populate_db ()
	{
	}

	static void create_user (string username, string email, string password, string pwdQuestion, string pwdAnswer)
	{
		MembershipCreateStatus status;
		Membership.CreateUser (username, password, email, pwdQuestion, pwdAnswer, true, out status);
		Console.WriteLine ("create status: {0}", status);
	}

	static void generate_password (int length, int numNonAlphNum)
	{
		Console.WriteLine ("generated password = \"{0}\"", Membership.GeneratePassword (length, numNonAlphNum));
	}

	static void dump_list (MembershipUserCollection users)
	{
		Console.WriteLine ("{0} users", users.Count);
		foreach (MembershipUser u in users) {
			Console.WriteLine ("{0} {1} {2}", u.UserName, u.Email, u.IsLockedOut ? "lockedout" : "notlockedout");
		}
	}

	static void list_all_users ()
	{
		MembershipUserCollection users = Membership.GetAllUsers();
		dump_list (users);
	}

	static void validate_user (string username, string password)
	{
		if (Membership.ValidateUser (username, password))
			Console.WriteLine ("success.");
		else
			Console.WriteLine ("failure.");
	}

	static void unlock_user (string username)
	{
		if (Membership.Provider.UnlockUser (username))
			Console.WriteLine ("success.");
		else
			Console.WriteLine ("failure.");
	}

	static void reset_password (string username, string pwdAnswer)
	{
		string newPassword = Membership.Provider.ResetPassword (username, pwdAnswer);

		if (newPassword == null)
			Console.WriteLine ("failure.");
		else
			Console.WriteLine ("success, new password is \"{0}\"", newPassword);
	}

	static void change_password (string username, string oldPwd, string newPwd)
	{
		if (Membership.Provider.ChangePassword (username, oldPwd, newPwd))
			Console.WriteLine ("success.");
		else
			Console.WriteLine ("failure.");
	}

	static void change_question_answer (string username, string pwd, string question, string answer)
	{
		if (Membership.Provider.ChangePasswordQuestionAndAnswer (username, pwd, question, answer))
			Console.WriteLine ("success.");
		else
			Console.WriteLine ("failure.");
	}

	static void find_user_by_email (string pattern)
	{
		MembershipUserCollection users = Membership.FindUsersByEmail (pattern);
		dump_list (users);
	}

	static void find_user_by_name (string pattern)
	{
		MembershipUserCollection users = Membership.FindUsersByName (pattern);
		dump_list (users);
	}

	static void get_number_of_users_online ()
	{
		Console.WriteLine ("Number of online users: {0}", Membership.GetNumberOfUsersOnline ());
	}

	static void get_password (string username, string answer)
	{
		Console.WriteLine ("password for user {0}: {1}", username, Membership.Provider.GetPassword (username, answer));
	}

	static void dump_user (string username)
	{
		Console.WriteLine ("info for user: {0}", username);

		MembershipUser user = Membership.GetUser (username, false);
		Console.WriteLine ("comment: {0}", user.Comment);
		Console.WriteLine ("creation date: {0}", user.CreationDate);
		Console.WriteLine ("email: {0}", user.Email);
		Console.WriteLine ("isApproved: {0}", user.IsApproved);
		Console.WriteLine ("isOnline: {0}", user.IsOnline);
		Console.WriteLine ("last activity date: {0}", user.LastActivityDate);
		Console.WriteLine ("last login date: {0}", user.LastLoginDate);
		Console.WriteLine ("last password changed date: {0}", user.LastPasswordChangedDate);
		Console.WriteLine ("last lockout date: {0}", user.LastLockoutDate);
	}

	static void Usage ()
	{
		Console.WriteLine ("usage:   just look at test.cs...");
	}

	public static void Main (string[] args) {
		if (args.Length == 0) {
			Usage ();
			return;
		}

		switch (args[0]) {
		case "populate":
			populate_db ();
			break;
		case "createuser":
			create_user (args[1], args[2], args[3], args[4], args[5]);
			break;
		case "listallusers":
			list_all_users ();
			break;
		case "generatepassword":
			generate_password (Int32.Parse (args[1]), Int32.Parse (args[2]));
			break;
		case "validateuser":
			validate_user (args[1], args[2]);
			break;
		case "unlockuser":
			unlock_user (args[1]);
			break;
		case "resetpassword":
			reset_password (args[1], args[2]);
			break;
		case "changepassword":
			change_password (args[1], args[2], args[3]);
			break;
		case "changequestionanswer":
			change_question_answer (args[1], args[2], args[3], args[4]);
			break;
		case "dumpuser":
			dump_user (args[1]);
			break;
		case "findusersbyemail":
			find_user_by_email (args[1]);
			break;
		case "findusersbyname":
			find_user_by_name (args[1]);
			break;
		case "getnumberofusersonline":
			get_number_of_users_online ();
			break;
		case "getpassword":
			get_password (args[1], args[2]);
			break;
		default:
			Console.WriteLine ("unknown command {0}", args[0]);
			break;
		}
	}
}



public class ProvPoker : Toshok.Web.Security.SqlMembershipProvider {
	protected override byte[] EncryptPassword (byte[] pwd) {
		Console.WriteLine ("pwd = ({0})", Convert.ToBase64String (pwd));
		byte[] buf = base.EncryptPassword (pwd);
		Console.WriteLine ("buf = {0} bytes long ({1})", buf.Length, Convert.ToBase64String (buf));
		return buf;
	}

	protected override byte[] DecryptPassword (byte[] pwd) {
		Console.WriteLine ("pwd = ({0})", Convert.ToBase64String (pwd));
		byte[] buf = base.DecryptPassword (pwd);
		Console.WriteLine ("buf = {0} bytes long ({1})", buf.Length, Convert.ToBase64String (buf));
		return buf;
	}
}
