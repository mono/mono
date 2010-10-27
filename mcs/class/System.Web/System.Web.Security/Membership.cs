//
// System.Web.Security.Membership
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2005 Novell, inc.
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.Configuration;
using System.Configuration;
using System.Security.Cryptography;

namespace System.Web.Security
{
	public static class Membership
	{
#if TARGET_J2EE
		const string Membership_providers = "Membership.providers";
		static MembershipProviderCollection providers {
			get {
				object o = AppDomain.CurrentDomain.GetData (Membership_providers);
				if (o == null) {
					lock (AppDomain.CurrentDomain) {
						o = AppDomain.CurrentDomain.GetData (Membership_providers);
						if (o == null) {
							MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
							MembershipProviderCollection local_providers = new MembershipProviderCollection ();
							ProvidersHelper.InstantiateProviders (section.Providers, local_providers, typeof (MembershipProvider));
							AppDomain.CurrentDomain.SetData (Membership_providers, local_providers);
							o = local_providers;
						}
					}
				}

				return (MembershipProviderCollection) o;
			}
		}
		static MembershipProvider provider {
			get {
				MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
				MembershipProvider p = providers [section.DefaultProvider];
				if (p == null)
					throw new ConfigurationErrorsException ("Default Membership Provider could not be found: Cannot instantiate provider: '" + section.DefaultProvider + "'.");
				return p;
			}
		}
		static int onlineTimeWindow {
			get {
				MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
				return (int) section.UserIsOnlineTimeWindow.TotalMinutes;
			}
		}
		static string hashAlgorithmType {
			get {
				MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
				string ret = section.HashAlgorithmType;

				if (ret == String.Empty) {
					MachineKeySection mks = WebConfigurationManager.GetSection ("system.web/machineKey") as MachineKeySection;
					return mks.Validation;
				}

				return ret;
			}
		}
		
#else
		static MembershipProviderCollection providers;
		static MembershipProvider provider;
		static int onlineTimeWindow;
		static string hashAlgorithmType;
		
		static Membership ()
		{
			MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");

			providers = new MembershipProviderCollection ();

			ProvidersHelper.InstantiateProviders (section.Providers, providers, typeof (MembershipProvider));

			provider = providers[section.DefaultProvider];

			onlineTimeWindow = (int) section.UserIsOnlineTimeWindow.TotalMinutes;
			hashAlgorithmType = section.HashAlgorithmType;
			if (String.IsNullOrEmpty (hashAlgorithmType)) {
				MachineKeySection mks = WebConfigurationManager.GetSection ("system.web/machineKey") as MachineKeySection;
				MachineKeyValidationConverter cvt = new MachineKeyValidationConverter ();
				hashAlgorithmType = cvt.ConvertTo (null, null, mks.Validation, typeof (string)) as string;
			}
			
			if (String.IsNullOrEmpty (hashAlgorithmType))
				hashAlgorithmType = "SHA1";
		}
#endif

		public static MembershipUser CreateUser (string username, string password)
		{
			return CreateUser (username, password, null);
		}
		
		public static MembershipUser CreateUser (string username, string password, string email)
		{
			MembershipCreateStatus status;
			MembershipUser usr = CreateUser (username, password, email, null, null, true, out status);
			if (usr == null)
				throw new MembershipCreateUserException (status);
			
			return usr;
		}
		
		public static MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, out MembershipCreateStatus status)
		{
			return CreateUser (username, password, email, pwdQuestion, pwdAnswer, isApproved, null, out status);
		}
		
		public static MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			if (String.IsNullOrEmpty (username)) {
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}

			if (String.IsNullOrEmpty (password)) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			return Provider.CreateUser (username, password, email, pwdQuestion, pwdAnswer, isApproved, providerUserKey, out status);
		}
		
		public static bool DeleteUser (string username)
		{
			return Provider.DeleteUser (username, true);
		}
		
		public static bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			return Provider.DeleteUser (username, deleteAllRelatedData);
		}
		
		public static string GeneratePassword (int length, int numberOfNonAlphanumericCharacters)
		{
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			byte[] pass_bytes = new byte[length];
			int i;
			int num_nonalpha = 0;

			rng.GetBytes (pass_bytes);
			
			for (i = 0; i < length; i ++) {
				/* convert the random bytes to ascii values 33-126 */
				pass_bytes[i] = (byte)(pass_bytes[i] % 93 + 33);

				/* and count the number of
				 * non-alphanumeric characters we have
				 * as we go */
				if ((pass_bytes[i] >= 33 && pass_bytes[i] <= 47)
				    || (pass_bytes[i] >= 58 && pass_bytes[i] <= 64)
				    || (pass_bytes[i] >= 91 && pass_bytes[i] <= 96)
				    || (pass_bytes[i] >= 123 && pass_bytes[i] <= 126))
					num_nonalpha++;

				/* get rid of any quotes in the
				 * password, just in case they cause
				 * problems */
				if (pass_bytes[i] == 34 || pass_bytes[i] == 39)
					pass_bytes[i] ++;
				else if (pass_bytes[i] == 96)
					pass_bytes[i] --;
			}

			if (num_nonalpha < numberOfNonAlphanumericCharacters) {
				/* loop over the array, converting the
				 * least number of alphanumeric
				 * characters to non-alpha */
				for (i = 0; i < length; i ++) {
					if (num_nonalpha == numberOfNonAlphanumericCharacters)
						break;
					if (pass_bytes[i] >= 48 && pass_bytes[i] <= 57) {
						pass_bytes[i] = (byte)(pass_bytes[i] - 48 + 33);
						num_nonalpha++;
					}
					else if (pass_bytes[i] >= 65 && pass_bytes[i] <= 90) {
						pass_bytes[i] = (byte)((pass_bytes[i] - 65) % 13 + 33);
						num_nonalpha++;
					}
					else if (pass_bytes[i] >= 97 && pass_bytes[i] <= 122) {
						pass_bytes[i] = (byte)((pass_bytes[i] - 97) % 13 + 33);
						num_nonalpha++;
					}

					/* and make sure we don't end up with quote characters */
					if (pass_bytes[i] == 34 || pass_bytes[i] == 39)
						pass_bytes[i]++;
					else if (pass_bytes[i] == 96)
						pass_bytes[i] --;
				}
			}

			return Encoding.ASCII.GetString (pass_bytes);
		}
		
		public static MembershipUserCollection GetAllUsers ()
		{
			int total;
			return GetAllUsers (0, int.MaxValue, out total);
		}
		
		public static MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.GetAllUsers (pageIndex, pageSize, out totalRecords);
		}
		
		public static int GetNumberOfUsersOnline ()
		{
			return Provider.GetNumberOfUsersOnline ();
		}
		
		public static MembershipUser GetUser ()
		{
			return GetUser (HttpContext.Current.User.Identity.Name, true);
		}
		
		public static MembershipUser GetUser (bool userIsOnline)
		{
			return GetUser (HttpContext.Current.User.Identity.Name, userIsOnline);
		}
		
		public static MembershipUser GetUser (string username)
		{
			return GetUser (username, false);
		}
		
		public static MembershipUser GetUser (string username, bool userIsOnline)
		{
			return Provider.GetUser (username, userIsOnline);
		}
		
		public static MembershipUser GetUser (object providerUserKey)
		{
			return GetUser (providerUserKey, false);
		}
		
		public static MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			return Provider.GetUser (providerUserKey, userIsOnline);
		}
		
		public static string GetUserNameByEmail (string email)
		{
			return Provider.GetUserNameByEmail (email);
		}
		
		public static void UpdateUser (MembershipUser user)
		{
			Provider.UpdateUser (user);
		}
		
		public static bool ValidateUser (string username, string password)
		{
			return Provider.ValidateUser (username, password);
		}
		
		public static MembershipUserCollection FindUsersByEmail (string emailToMatch)
		{
			int totalRecords;
			return Provider.FindUsersByEmail (emailToMatch, 0, int.MaxValue, out totalRecords);
		}
		
		public static MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindUsersByEmail (emailToMatch, pageIndex, pageSize, out totalRecords);
		}
		
		public static MembershipUserCollection FindUsersByName (string nameToMatch)
		{
			int totalRecords;
			return Provider.FindUsersByName (nameToMatch, 0, int.MaxValue, out totalRecords);
		}
		
		public static MembershipUserCollection FindUsersByName (string nameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindUsersByName (nameToMatch, pageIndex, pageSize, out totalRecords);
		}

		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		public static bool EnablePasswordReset {
			get { return Provider.EnablePasswordReset; }
		}
		
		public static bool EnablePasswordRetrieval {
			get { return Provider.EnablePasswordRetrieval; }
		}

		public static string HashAlgorithmType {
			get { return hashAlgorithmType; }
		}

		public static bool RequiresQuestionAndAnswer {
			get { return Provider.RequiresQuestionAndAnswer; }
		}
		
		public static int MaxInvalidPasswordAttempts {
			get { return Provider.MaxInvalidPasswordAttempts; }
		}
		
		public static int MinRequiredNonAlphanumericCharacters {
			get { return Provider.MinRequiredNonAlphanumericCharacters; }
		}
		
		public static int MinRequiredPasswordLength {
			get { return Provider.MinRequiredPasswordLength; }
		}
		
		public static int PasswordAttemptWindow {
			get { return Provider.PasswordAttemptWindow; }
		}
		
		public static string PasswordStrengthRegularExpression {
			get { return Provider.PasswordStrengthRegularExpression; }
		}
				
		public static MembershipProvider Provider {
			get { return provider; }
		}
		
		public static MembershipProviderCollection Providers {
			get { return providers; }
		}
		
		public static int UserIsOnlineTimeWindow {
			get { return onlineTimeWindow; }
		}
		
		public static event MembershipValidatePasswordEventHandler ValidatingPassword {
			add { Provider.ValidatingPassword += value; }
			remove { Provider.ValidatingPassword -= value; }
		}
	}
}
#endif

