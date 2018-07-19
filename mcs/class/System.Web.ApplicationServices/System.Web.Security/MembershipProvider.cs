//
// System.Web.Security.MembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Configuration.Provider;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Configuration;

namespace System.Web.Security
{
	[TypeForwardedFrom ("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public abstract class MembershipProvider : ProviderBase
	{
		const string HELPER_TYPE_NAME = "System.Web.Security.MembershipHelper, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

		internal static IMembershipHelper Helper {
			get { return helper; }
		}
		static IMembershipHelper helper;
		
		static readonly object validatingPasswordEvent = new object ();
		
		EventHandlerList events = new EventHandlerList ();
		public event MembershipValidatePasswordEventHandler ValidatingPassword {
			add { events.AddHandler (validatingPasswordEvent, value); }
			remove { events.RemoveHandler (validatingPasswordEvent, value); }
		}

		static MembershipProvider ()
		{
			Type type = Type.GetType (HELPER_TYPE_NAME, false);
			if (type == null)
				return;

			try {
				helper = Activator.CreateInstance (type) as IMembershipHelper;
			} catch {
				// ignore
			}
		}

		protected MembershipProvider ()
		{
		}
		
		public abstract bool ChangePassword (string username, string oldPassword, string newPassword);
		public abstract bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer);
		public abstract MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status);
		public abstract bool DeleteUser (string username, bool deleteAllRelatedData);
		public abstract MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords);
		public abstract MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords);
		public abstract MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords);
		public abstract int GetNumberOfUsersOnline ();
		public abstract string GetPassword (string username, string answer);
		public abstract MembershipUser GetUser (string username, bool userIsOnline);
		public abstract MembershipUser GetUser (object providerUserKey, bool userIsOnline);
		public abstract string GetUserNameByEmail (string email);
		public abstract string ResetPassword (string username, string answer);
		public abstract void UpdateUser (MembershipUser user);
		public abstract bool ValidateUser (string username, string password);
		public abstract bool UnlockUser (string userName);
		
		public abstract string ApplicationName { get; set; }
		public abstract bool EnablePasswordReset { get; }
		public abstract bool EnablePasswordRetrieval { get; }
		public abstract bool RequiresQuestionAndAnswer { get; }
		public abstract int MaxInvalidPasswordAttempts { get; }
		public abstract int MinRequiredNonAlphanumericCharacters { get; }
		public abstract int MinRequiredPasswordLength { get; }
		public abstract int PasswordAttemptWindow { get; }
		public abstract MembershipPasswordFormat PasswordFormat { get; }
		public abstract string PasswordStrengthRegularExpression { get; }
		public abstract bool RequiresUniqueEmail { get; }
		
		protected virtual void OnValidatingPassword (ValidatePasswordEventArgs e)
		{
			MembershipValidatePasswordEventHandler eh = events [validatingPasswordEvent] as MembershipValidatePasswordEventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual byte [] DecryptPassword (byte [] encodedPassword)
		{
			if (helper == null)
				throw new PlatformNotSupportedException ("This method is not available.");
			return helper.DecryptPassword (encodedPassword);
		}

		protected virtual byte[] EncryptPassword (byte[] password)
		{
			return EncryptPassword (password, MembershipPasswordCompatibilityMode.Framework20);
		}
		[MonoTODO ("Discover what actually is 4.0 password compatibility mode.")]
		protected virtual byte[] EncryptPassword (byte[] password, MembershipPasswordCompatibilityMode legacyPasswordCompatibilityMode)
		{
			if (helper == null)
				throw new PlatformNotSupportedException ("This method is not available.");

			if (legacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework40)
				throw new PlatformNotSupportedException ("Framework 4.0 password encryption mode is not supported at this time.");
			
			return helper.EncryptPassword (password);
		}
	}
}



