//
// System.Web.Security.MembershipUser
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
namespace System.Web.Security {
	public class MembershipUser {
		protected MembershipUser ()
		{
		}
		
		public MembershipUser (MembershipProvider provider, string name, string email,
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
		
		public virtual MembershipProvider Provider {
			get { return provider; }
		}
		
		public virtual string Username {
			get { return name; }
		}
		
		MembershipProvider provider;
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

