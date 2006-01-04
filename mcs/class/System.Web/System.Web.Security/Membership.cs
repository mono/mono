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

namespace System.Web.Security
{
	public static class Membership
	{
		static MembershipProviderCollection providers;
		static MembershipProvider provider;
		static int onlineTimeWindow;
		
		static Membership ()
		{
#if CONFIGURATION_2_0
			MembershipSection section = (MembershipSection) WebConfigurationManager.GetWebApplicationSection ("system.web/membership");
#endif
			providers = new MembershipProviderCollection ();

#if CONFIGURATION_2_0
			foreach (ProviderSettings prov in section.Providers) {
				Type t = Type.GetType (prov.Type);
				if (t == null)
					throw new ConfigurationException ("Cannot find type: " + prov.Type);
				if (!typeof(MembershipProvider).IsAssignableFrom (t))
					throw new ConfigurationException ("The provided type is not a MembershipProvider subclass: " + prov.Type);
				
				MembershipProvider pr = (MembershipProvider) Activator.CreateInstance (t);
				pr.Initialize (prov.Name, prov.Parameters);
				
				if (provider == null || prov.Name == section.DefaultProvider)
					provider = pr;

				providers.Add (pr);
			}
#endif

			if (providers.Count == 0) {
				provider = new SqlMembershipProvider ();
				NameValueCollection attributes = new NameValueCollection ();
				provider.Initialize ("AspNetSqlMembershipProvider", attributes);
				providers.Add (provider);
			}
#if CONFIGURATION_2_0
			onlineTimeWindow = (int) section.UserIsOnlineTimeWindow.TotalMinutes;
#endif
		}
		
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
			return Provider.CreateUser (username, password, email, pwdQuestion, pwdAnswer, isApproved, null, out status);
		}
		
		public static MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
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
		
		[MonoTODO]
		public static string GeneratePassword (int length, int numberOfNonAlphanumericCharacters)
		{
			throw new NotImplementedException ();
		}
		
		public static MembershipUserCollection GetAllUsers ()
		{
			int total;
			return GetAllUsers (1, int.MaxValue, out total);
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
		
		[MonoTODO ("Fire it")]
		public static event MembershipValidatePasswordEventHandler ValidatingPassword {
			add { Provider.ValidatingPassword += value; }
			remove { Provider.ValidatingPassword -= value; }
		}
	}
}
#endif

