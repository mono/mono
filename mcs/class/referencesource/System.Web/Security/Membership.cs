//------------------------------------------------------------------------------
// <copyright file="Membership.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Web.Configuration;
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Runtime.Serialization;
    using  System.Collections;
    using  System.Security.Cryptography;
    using  System.Configuration.Provider;
    using  System.Text;
    using  System.Configuration;
    using  System.Web.Management;
    using  System.Web.Hosting;
    using  System.Threading;
    using  System.Web.Util;
    using  System.Collections.Specialized;
    using System.Web.Compilation;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    // This has no hosting permission demands because of DevDiv Bugs 31461: ClientAppSvcs: ASP.net Provider support
    public static class Membership
    {

        public static bool   EnablePasswordRetrieval   { get { Initialize(); return Provider.EnablePasswordRetrieval;}}

        public static bool   EnablePasswordReset       { get { Initialize(); return Provider.EnablePasswordReset;}}

        public static bool   RequiresQuestionAndAnswer   { get { Initialize(); return Provider.RequiresQuestionAndAnswer;}}

        public static int    UserIsOnlineTimeWindow      { get { Initialize(); return s_UserIsOnlineTimeWindow; }}


        public static MembershipProviderCollection    Providers    { get { Initialize(); return s_Providers; }}

        public static MembershipProvider Provider {
            get {
                Initialize();
                if (s_Provider == null) {
                    throw new InvalidOperationException(SR.GetString(SR.Def_membership_provider_not_found));
                }
                return s_Provider;
            }
        }

        public static string   HashAlgorithmType { get { Initialize(); return s_HashAlgorithmType; }}
        internal static bool   IsHashAlgorithmFromMembershipConfig { get { Initialize(); return s_HashAlgorithmFromConfig; }}

        public static int MaxInvalidPasswordAttempts
        {
            get
            {
                Initialize();

                return Provider.MaxInvalidPasswordAttempts;
            }
        }

        public static int PasswordAttemptWindow
        {
            get
            {
                Initialize();

                return Provider.PasswordAttemptWindow;
            }
        }

        public static int MinRequiredPasswordLength
        {
            get
            {
                Initialize();

                return Provider.MinRequiredPasswordLength;
            }
        }

        public static int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                Initialize();

                return Provider.MinRequiredNonAlphanumericCharacters;
            }
        }

        public static string PasswordStrengthRegularExpression
        {
            get
            {
                Initialize();

                return Provider.PasswordStrengthRegularExpression;
            }
        }


        public static string ApplicationName
        {
            get { return Provider.ApplicationName; }
            set { Provider.ApplicationName = value; }
        }


        public static MembershipUser CreateUser(string username, string password)
        {
            return CreateUser(username, password, null);
        }


        public static MembershipUser CreateUser(string username, string password, string email)
        {
            MembershipCreateStatus status;
            MembershipUser u = CreateUser(username, password, email,null,null,true, out status);
            if (u == null)
                throw new MembershipCreateUserException(status);
            return u;
        }


        public static MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, out MembershipCreateStatus status)
        {
            return CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, null, out status);
        }

        public static MembershipUser CreateUser( string username, string password,  string email, string passwordQuestion,string passwordAnswer,
                                                 bool   isApproved, object providerUserKey, out MembershipCreateStatus status )
        {
            if( !SecUtility.ValidateParameter(ref username,  true,  true, true, 0))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if( !SecUtility.ValidatePasswordParameter(ref password, 0))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }


            if( !SecUtility.ValidateParameter( ref email, false, false, false, 0))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if( !SecUtility.ValidateParameter(ref passwordQuestion, false, true, false, 0))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if( !SecUtility.ValidateParameter(ref passwordAnswer, false, true, false, 0))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            return Provider.CreateUser( username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }


        public static bool ValidateUser(string username, string password)
        {
            return Provider.ValidateUser(username, password);
            /*
            if (retVal) {
                PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_SUCCESS);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditMembershipAuthenticationSuccess, username);
            }
            else {
                PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_FAIL);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditMembershipAuthenticationFailure, username);
            }

            return retVal;
             */
        }


        public static MembershipUser GetUser()
        {
            return GetUser(GetCurrentUserName(), true);
        }


        public static MembershipUser GetUser(bool userIsOnline)
        {
            return GetUser(GetCurrentUserName(), userIsOnline);
        }


        public static MembershipUser GetUser(string username)
        {
            return GetUser(username, false);
        }


        public static MembershipUser GetUser(string username, bool userIsOnline)
        {
            SecUtility.CheckParameter( ref username, true, false, true, 0, "username" );

            return Provider.GetUser(username, userIsOnline);
        }

        public static MembershipUser GetUser( object providerUserKey )
        {
            return GetUser( providerUserKey, false);
        }

        public static MembershipUser GetUser( object providerUserKey, bool userIsOnline )
        {
            if( providerUserKey == null )
            {
                throw new ArgumentNullException( "providerUserKey" );
            }


            return Provider.GetUser( providerUserKey, userIsOnline);
        }


        public static string GetUserNameByEmail( string emailToMatch )
        {
            SecUtility.CheckParameter( ref emailToMatch,
                                       false,
                                       false,
                                       false,
                                       0,
                                       "emailToMatch" );

            return Provider.GetUserNameByEmail( emailToMatch );
        }


        public static bool DeleteUser(string username)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 0, "username" );
            return Provider.DeleteUser( username, true );
        }


        public static bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 0, "username" );
            return Provider.DeleteUser( username, deleteAllRelatedData );
        }


        public static void UpdateUser( MembershipUser user )
        {
            if( user == null )
            {
                throw new ArgumentNullException( "user" );
            }

            user.Update();
        }


        public static MembershipUserCollection GetAllUsers()
        {
            int totalRecords = 0;
            return GetAllUsers( 0, Int32.MaxValue, out totalRecords);
        }

        public static MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if ( pageIndex < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            }

            if ( pageSize < 1 )
            {
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");
            }

            return Provider.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }


        public static int GetNumberOfUsersOnline() {
            return Provider.GetNumberOfUsersOnline();
        }

        private static char [] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();


        public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters) {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException(SR.GetString(SR.Membership_password_length_incorrect));
            }

            if( numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.Membership_min_required_non_alphanumeric_characters_incorrect,
                                                         "numberOfNonAlphanumericCharacters"));
            }

            string password;
            int    index;
            byte[] buf;
            char[] cBuf;
            int count;

            do {
                buf = new byte[length];
                cBuf = new char[length];
                count = 0;

                (new RNGCryptoServiceProvider()).GetBytes(buf);

                for(int iter=0; iter<length; iter++)
                {
                    int i = (int) (buf[iter] % 87);
                    if (i < 10)
                        cBuf[iter] = (char) ('0' + i);
                    else if (i < 36)
                        cBuf[iter] = (char) ('A' + i - 10);
                    else if (i < 62)
                        cBuf[iter] = (char) ('a' + i - 36);
                    else
                    {
                        cBuf[iter] = punctuations[i-62];
                        count++;
                    }
                }

                if( count < numberOfNonAlphanumericCharacters )
                {
                    int j, k;
                    Random rand = new Random();

                    for( j = 0; j < numberOfNonAlphanumericCharacters - count; j++ )
                    {
                        do
                        {
                            k = rand.Next( 0, length );
                        }
                        while( !Char.IsLetterOrDigit( cBuf[k] ) );

                        cBuf[k] = punctuations[rand.Next(0, punctuations.Length)];
                    }
                }

                password = new string(cBuf);
            }
            while(CrossSiteScriptingValidation.IsDangerousString(password, out index));

            return password;
        }

        private static void Initialize()
        {
            if (s_Initialized && s_InitializedDefaultProvider) {
                return;
            }
            if (s_InitializeException != null)
                throw s_InitializeException;

            if (HostingEnvironment.IsHosted)
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

            lock (s_lock) {
                if (s_Initialized && s_InitializedDefaultProvider) {
                    return;
                }
                if (s_InitializeException != null)
                    throw s_InitializeException;

                bool initializeGeneralSettings = !s_Initialized;
                // the default provider can be initialized once the pre start init has happened (i.e. when compilation has begun)
                // or if this is not even a hosted scenario
                bool initializeDefaultProvider = !s_InitializedDefaultProvider &&
                    (!HostingEnvironment.IsHosted || BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit);

                if (!initializeDefaultProvider && !initializeGeneralSettings) {
                    return;
                }

                bool generalSettingsInitialized;
                bool defaultProviderInitialized = false;
                try {
                    RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                    MembershipSection settings = appConfig.Membership;
                    generalSettingsInitialized = InitializeSettings(initializeGeneralSettings, appConfig, settings);
                    defaultProviderInitialized = InitializeDefaultProvider(initializeDefaultProvider, settings);
                } catch (Exception e) {
                    s_InitializeException = e;
                    throw;
                }

                // update this state only after the whole method completes to preserve the behavior where
                // the system is uninitialized if any exceptions were thrown.
                if (generalSettingsInitialized) {
                    s_Initialized = true;
                }
                if (defaultProviderInitialized) {
                    s_InitializedDefaultProvider = true;
                }
            }
        }

        private static bool InitializeSettings(bool initializeGeneralSettings, RuntimeConfig appConfig, MembershipSection settings) {
            if (!initializeGeneralSettings) {
                return false;
            }

            s_HashAlgorithmType = settings.HashAlgorithmType;
            s_HashAlgorithmFromConfig = !string.IsNullOrEmpty(s_HashAlgorithmType);
            if (!s_HashAlgorithmFromConfig) {
                // If no hash algorithm is specified, use the same as the "validation" in "<machineKey>".
                // If the validation is "3DES", switch it to use "SHA1" instead.
                MachineKeyValidation v = appConfig.MachineKey.Validation;
                if (v != MachineKeyValidation.AES && v != MachineKeyValidation.TripleDES)
                    s_HashAlgorithmType = appConfig.MachineKey.ValidationAlgorithm;
                else
                    s_HashAlgorithmType = "SHA1";
            }
            s_Providers = new MembershipProviderCollection();
            if (HostingEnvironment.IsHosted) {
                ProvidersHelper.InstantiateProviders(settings.Providers, s_Providers, typeof(MembershipProvider));
            } else {
                foreach (ProviderSettings ps in settings.Providers) {
                    Type t = Type.GetType(ps.Type, true, true);
                    if (!typeof(MembershipProvider).IsAssignableFrom(t))
                        throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, typeof(MembershipProvider).ToString()));
                    MembershipProvider provider = (MembershipProvider)Activator.CreateInstance(t);
                    NameValueCollection pars = ps.Parameters;
                    NameValueCollection cloneParams = new NameValueCollection(pars.Count, StringComparer.Ordinal);
                    foreach (string key in pars)
                        cloneParams[key] = pars[key];
                    provider.Initialize(ps.Name, cloneParams);
                    s_Providers.Add(provider);
                }
            }

            TimeSpan timeWindow = settings.UserIsOnlineTimeWindow;
            s_UserIsOnlineTimeWindow = (int)timeWindow.TotalMinutes;

            return true;
        }

        private static bool InitializeDefaultProvider(bool initializeDefaultProvider, MembershipSection settings) {
            if (!initializeDefaultProvider) {
                return false;
            }

            s_Providers.SetReadOnly();

            if (settings.DefaultProvider == null || s_Providers.Count < 1)
                throw new ProviderException(SR.GetString(SR.Def_membership_provider_not_specified));

            s_Provider = s_Providers[settings.DefaultProvider];
            if (s_Provider == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Def_membership_provider_not_found), settings.ElementInformation.Properties["defaultProvider"].Source, settings.ElementInformation.Properties["defaultProvider"].LineNumber);
            }

            return true;
        }

        public static MembershipUserCollection FindUsersByName( string usernameToMatch,
                                                                int pageIndex,
                                                                int pageSize,
                                                                out int totalRecords )
        {
            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            if ( pageIndex < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            }

            if ( pageSize < 1 )
            {
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");
            }

            return Provider.FindUsersByName( usernameToMatch,
                                             pageIndex,
                                             pageSize,
                                             out totalRecords);
        }


        public static MembershipUserCollection FindUsersByName( string usernameToMatch )
        {
            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            int totalRecords = 0;
            return Provider.FindUsersByName( usernameToMatch,
                                             0,
                                             Int32.MaxValue,
                                             out totalRecords );
        }

        public static MembershipUserCollection FindUsersByEmail( string  emailToMatch,
                                                                 int     pageIndex,
                                                                 int     pageSize,
                                                                 out int totalRecords )
        {
            SecUtility.CheckParameter( ref emailToMatch,
                                       false,
                                       false,
                                       false,
                                       0,
                                       "emailToMatch" );

            if ( pageIndex < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            }

            if ( pageSize < 1 )
            {
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");
            }

            return Provider.FindUsersByEmail( emailToMatch,
                                              pageIndex,
                                              pageSize,
                                              out totalRecords );
        }

        public static MembershipUserCollection FindUsersByEmail(string emailToMatch)
        {
            SecUtility.CheckParameter( ref emailToMatch,
                                       false,
                                       false,
                                       false,
                                       0,
                                       "emailToMatch" );

            int totalRecords = 0;
            return FindUsersByEmail(emailToMatch, 0, Int32.MaxValue, out totalRecords);
        }

        private static string GetCurrentUserName()
        {
            if (HostingEnvironment.IsHosted) {
                HttpContext cur = HttpContext.Current;
                if (cur != null)
                    return cur.User.Identity.Name;
            }
            IPrincipal user = Thread.CurrentPrincipal;
            if (user == null || user.Identity == null)
                return String.Empty;
            else
                return user.Identity.Name;
        }

        public static event MembershipValidatePasswordEventHandler ValidatingPassword
        {
            add
            {
                Provider.ValidatingPassword += value;
            }
            remove
            {
                Provider.ValidatingPassword -= value;
            }
        }

        private static MembershipProviderCollection   s_Providers;
        private static MembershipProvider             s_Provider;
        private static int                            s_UserIsOnlineTimeWindow = 15;
        private static object                         s_lock = new object();
        private static bool                           s_Initialized = false;
        private static bool                           s_InitializedDefaultProvider;
        private static Exception                      s_InitializeException = null;
        private static string                         s_HashAlgorithmType;
        private static bool                           s_HashAlgorithmFromConfig;

    }
}

