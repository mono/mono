//
// System.Web.Security.MembershipUser
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.Security {
	public class MembershipUser {
		protected MembershipUser ()
		{
		}
		
		public MembershipUser (IMembershipProvider provider, string name, string email,
			string passwordQuestion, string comment, bool isApproved,
			DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate,
			DateTime lastPasswordChangedDate)
		{
			this.provider = provider;
			this.name = name;
			this.email = email;
			this.passwordQuestion = passwordQuestion;
			this.comment = comment;
			this.isApproved = isApproved;
			this.creationDate = creationDate;
			this.lastLoginDate = lastLoginDate;
			this.lastActivityDate = lastActivityDate;
			this.lastPasswordChangedDate = lastPasswordChangedDate;
		}
		
		public virtual bool ChangePassword (string oldPassword, string newPassword)
		{
			bool success = Provider.ChangePassword (Username, oldPassword, newPassword);
			if (success)
				LastPasswordChangedDate = DateTime.Now;
			
			return success;
		}
		
		public virtual bool ChangePasswordQuestionAndAnswer (string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			bool success = Provider.ChangePasswordQuestionAndAnswer (Username, password, newPasswordQuestion, newPasswordAnswer);
			if (success)
				passwordQuestion = newPasswordQuestion;
			
			return success;
		}
		
		public virtual string GetPassword ()
		{
			return GetPassword (null);
		}
		
		public virtual string GetPassword (string answer)
		{
			return Provider.GetPassword (Username, answer);
		}
		
		public virtual string ResetPassword ()
		{
			return ResetPassword (null);
		}
		
		public virtual string ResetPassword (string answer)
		{
			string newPass = Provider.ResetPassword (Username, answer);
			if (newPass != null)
				LastPasswordChangedDate = DateTime.Now;
			
			return newPass;
		}
		
		public virtual string Comment {
			get { return comment; }
			set { comment = value; }
		}
		
		public virtual DateTime CreationDate {
			get { return creationDate; }
			set { creationDate = value; }
		}
		
		public virtual string Email {
			get { return email; }
			set { email = value; }
		}
		
		public virtual bool IsApproved {
			get { return isApproved; }
			set { isApproved = value; }
		}
		
		[MonoTODO]
		public bool IsOnline {
			get { throw new NotImplementedException (); }
		}
		
		public virtual DateTime LastActivityDate {
			get { return lastActivityDate; }
			set { lastActivityDate = value; }
		}
		
		public virtual DateTime LastLoginDate {
			get { return lastLoginDate; }
			set { lastLoginDate = value; }
		}
		
		public virtual DateTime LastPasswordChangedDate {
			get { return lastPasswordChangedDate; }
			set { lastPasswordChangedDate = value; }
		}
		
		public virtual string PasswordQuestion {
			get { return passwordQuestion; }
		}
		
		public virtual IMembershipProvider Provider {
			get { return provider; }
		}
		
		public virtual string Username {
			get { return name; }
		}
		
		IMembershipProvider provider;
		string name;
		string email;
		string passwordQuestion;
		string comment;
		bool isApproved;
		DateTime creationDate;
		DateTime lastLoginDate;
		DateTime lastActivityDate;
		DateTime lastPasswordChangedDate;
	}
}
#endif

