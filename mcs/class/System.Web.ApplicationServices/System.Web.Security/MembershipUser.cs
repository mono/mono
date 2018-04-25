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
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Web.Security
{
	[TypeForwardedFrom ("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[Serializable]
	public class MembershipUser
	{
		string providerName;
		string name;
		object providerUserKey;
		string email;
		string passwordQuestion;
		string comment;
		bool isApproved;
		bool isLockedOut;
		DateTime creationDate;
		DateTime lastLoginDate;
		DateTime lastActivityDate;
		DateTime lastPasswordChangedDate;
		DateTime lastLockoutDate;

		protected MembershipUser ()
		{
		}
		
		public MembershipUser (string providerName, string name, object providerUserKey, string email,
			string passwordQuestion, string comment, bool isApproved, bool isLockedOut,
			DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate,
			DateTime lastPasswordChangedDate, DateTime lastLockoutDate)
		{
			this.providerName = providerName;
			this.name = name;
			this.providerUserKey = providerUserKey;
			this.email = email;
			this.passwordQuestion = passwordQuestion;
			this.comment = comment;
			this.isApproved = isApproved;
			this.isLockedOut = isLockedOut;
			this.creationDate = creationDate.ToUniversalTime ();
			this.lastLoginDate = lastLoginDate.ToUniversalTime ();
			this.lastActivityDate = lastActivityDate.ToUniversalTime ();
			this.lastPasswordChangedDate = lastPasswordChangedDate.ToUniversalTime ();
			this.lastLockoutDate = lastLockoutDate.ToUniversalTime ();
		}
		
		void UpdateSelf (MembershipUser fromUser)
		{
			try { Comment = fromUser.Comment; } catch (NotSupportedException) {}
			try { creationDate = fromUser.CreationDate; } catch (NotSupportedException) {}
			try { Email = fromUser.Email; } catch (NotSupportedException) {}
			try { IsApproved = fromUser.IsApproved; } catch (NotSupportedException) {}
			try { isLockedOut = fromUser.IsLockedOut; } catch (NotSupportedException) {}
			try { LastActivityDate = fromUser.LastActivityDate; } catch (NotSupportedException) {}
			try { lastLockoutDate = fromUser.LastLockoutDate; } catch (NotSupportedException) {}
			try { LastLoginDate = fromUser.LastLoginDate; } catch (NotSupportedException) {}
			try { lastPasswordChangedDate = fromUser.LastPasswordChangedDate; } catch (NotSupportedException) {}
			try { passwordQuestion = fromUser.PasswordQuestion; } catch (NotSupportedException) {}
			try { providerUserKey = fromUser.ProviderUserKey; } catch (NotSupportedException) {}
		}

		internal void UpdateUser ()
		{
			MembershipUser newUser = Provider.GetUser (UserName, false);
			UpdateSelf (newUser);
		}

		public virtual bool ChangePassword (string oldPassword, string newPassword)
		{
			bool success = Provider.ChangePassword (UserName, oldPassword, newPassword);

			UpdateUser ();
			
			return success;
		}
		
		public virtual bool ChangePasswordQuestionAndAnswer (string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			bool success = Provider.ChangePasswordQuestionAndAnswer (UserName, password, newPasswordQuestion, newPasswordAnswer);

			UpdateUser ();
			
			return success;
		}
		
		public virtual string GetPassword ()
		{
			return GetPassword (null);
		}
		
		public virtual string GetPassword (string passwordAnswer)
		{
			return Provider.GetPassword (UserName, passwordAnswer);
		}
		
		public virtual string ResetPassword ()
		{
			return ResetPassword (null);
		}
		
		public virtual string ResetPassword (string passwordAnswer)
		{
			string newPass = Provider.ResetPassword (UserName, passwordAnswer);

			UpdateUser ();
			
			return newPass;
		}
		
		public virtual string Comment {
			get { return comment; }
			set { comment = value; }
		}
		
		public virtual DateTime CreationDate {
			get { return creationDate.ToLocalTime (); }
		}
		
		public virtual string Email {
			get { return email; }
			set { email = value; }
		}
		
		public virtual bool IsApproved {
			get { return isApproved; }
			set { isApproved = value; }
		}
		
		public virtual bool IsLockedOut {
			get { return isLockedOut; }
		}

		public virtual
		bool IsOnline {
			get {
				int minutes;
				IMembershipHelper helper = MembershipProvider.Helper;
				if (helper == null)
					throw new PlatformNotSupportedException ("The method is not available.");
				minutes = helper.UserIsOnlineTimeWindow;
				return LastActivityDate > DateTime.Now - TimeSpan.FromMinutes (minutes);
			}
		}
		
		public virtual DateTime LastActivityDate {
			get { return lastActivityDate.ToLocalTime (); }
			set { lastActivityDate = value.ToUniversalTime (); }
		}
		
		public virtual DateTime LastLoginDate {
			get { return lastLoginDate.ToLocalTime (); }
			set { lastLoginDate = value.ToUniversalTime (); }
		}
		
		public virtual DateTime LastPasswordChangedDate {
			get { return lastPasswordChangedDate.ToLocalTime (); }
		}
		
		public virtual DateTime LastLockoutDate {
			get { return lastLockoutDate.ToLocalTime (); }
		}
		
		public virtual string PasswordQuestion {
			get { return passwordQuestion; }
		}
		
		public virtual string ProviderName {
			get { return providerName; }
		}
		
		public virtual string UserName {
			get { return name; }
		}
		
		public virtual object ProviderUserKey {
			get { return providerUserKey; }
		}
		
		public override string ToString ()
		{
			return UserName;
		}
		
		public virtual bool UnlockUser ()
		{
			bool retval = Provider.UnlockUser (UserName);

			UpdateUser ();

			return retval;
		}
		
		MembershipProvider Provider {
			get {
				MembershipProvider p;				
				IMembershipHelper helper = MembershipProvider.Helper;
				if (helper == null)
					throw new PlatformNotSupportedException ("The method is not available.");
				p = helper.Providers [ProviderName];
				if (p == null)
					throw new InvalidOperationException ("Membership provider '" + ProviderName + "' not found.");
				return p;
			}
		}
	}
}


