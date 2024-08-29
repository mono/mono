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

		internal virtual void Update()
        {
            Provider.UpdateUser(this);
            UpdateSelf(this);
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

		// ChangePassword() can throw 3 types of exception:
        // 1. ArgumentException is thrown if:
        //    A. OldPassword or NewPassword is null, empty, or longer than 128 characters
        //    B. NewPassword shorter than MinRequiredPasswordLength, or NewPassword contains
        //       less non-alphanumeric characters than MinRequiredNonAlphanumericCharacters,
        //       or NewPassword does not match PasswordStrengthRegularExpression.
        //    C. A developer adds a listener to the MembershipProvider.ValidatingPassword event,
        //       and sets e.Cancel to true, and e.FailureInformation is null.
        // 2. ProviderException is thrown if the user does not exist when the stored procedure
        //    is run.  The only way this could happen is in a race condition, where the user
        //    is deleted in the middle of the MembershipProvider.ChangePassword() method.
        // 3. It appears that MembershipProviderException currently cannot be thrown, but
        //    there is a codepath that throws this exception, so we should catch it here anyway.
        internal bool ChangePassword(string oldPassword, string newPassword, bool throwOnError) {
            bool passwordChanged = false;

            try {
                passwordChanged = ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException) {
                if (throwOnError) throw;
            }
            catch (MembershipPasswordException) {
                if (throwOnError) throw;
            }

            return passwordChanged;
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

		internal string GetPassword(bool throwOnError) {
            return GetPassword(null, /* useAnswer */ false, throwOnError);
        }

        internal string GetPassword(string answer, bool throwOnError) {
            return GetPassword(answer, /* useAnswer */ true, throwOnError);
        }

		 // GetPassword() can throw 3 types of exception:
        // 1. ArgumentException is thrown if:
        //    A. Answer is null, empty, or longer than 128 characters
        // 2. ProviderException is thrown if the user does not exist when the stored procedure
        //    is run.  The only way this could happen is in a race condition, where the user
        //    is deleted in the middle of the MembershipProvider.ChangePassword() method.
        // 3. MembershipPasswordException is thrown if the user is locked out, or the answer
        //    is incorrect.
        private string GetPassword(string answer, bool useAnswer, bool throwOnError) {
            string password = null;

            try {
                if (useAnswer) {
                    password = GetPassword(answer);
                }
                else {
                    password = GetPassword();
                }
            }
            catch (ArgumentException) {
                if (throwOnError) throw;
            }
            catch (MembershipPasswordException) {
                if (throwOnError) throw;
            }

            return password;
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

		internal string ResetPassword(bool throwOnError) {
            return ResetPassword(null, /* useAnswer */ false, throwOnError);
        }

        internal string ResetPassword(string passwordAnswer, bool throwOnError) {
            return ResetPassword(passwordAnswer, /* useAnswer */ true, throwOnError);
        }

		// MembershipProvider.ResetPassword() can throw 3 types of exception:
        // 1. ArgumentException is thrown if:
        //    A. Answer is null, empty, or longer than 128 characters
        // 2. ProviderException is thrown if:
        //    A. The user does not exist when the stored procedure is run.  The only way
        //       this could happen is in a race condition, where the user is deleted in
        //       the middle of the MembershipProvider.ChangePassword() method.
        //    B. A developer adds a listener to the MembershipProvider.ValidatingPassword event,
        //       and sets e.Cancel to true, and e.FailureInformation is null.
        // 3. MembershipPasswordException is thrown if the user is locked out, or the answer
        //    is incorrect.
        private string ResetPassword(string passwordAnswer, bool useAnswer, bool throwOnError) {
            string password = null;

            try {
                if (useAnswer) {
                    password = ResetPassword(passwordAnswer);
                }
                else {
                    password = ResetPassword();
                }
            }
            catch (ArgumentException) {
                if (throwOnError) throw;
            }
            catch (MembershipPasswordException) {
                if (throwOnError) throw;
            }

            return password;
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

