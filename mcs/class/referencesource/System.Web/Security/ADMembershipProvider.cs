//------------------------------------------------------------------------------
// <copyright file="ADMembershipProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security
{
    using  System.Net;
    using  System.Web;
    using  System.Text;
    using  System.Text.RegularExpressions;
    using  System.Security;
    using  System.Collections;
    using  System.Globalization;
    using  System.Configuration;
    using  System.DirectoryServices;
    using  System.DirectoryServices.ActiveDirectory;
    using  System.DirectoryServices.Protocols;
    using  System.Web.Hosting;
    using  System.Security.Cryptography;
    using  System.Web.Configuration;
    using  System.Security.Permissions;
    using  System.Collections.Specialized;
    using  System.Runtime.InteropServices;
    using  System.Security.Principal;
    using  System.Web.DataAccess;
    using  System.Web.Util;
    using  System.Reflection;
    using  System.Configuration.Provider;
    using  System.Web.Management;
    
    public enum ActiveDirectoryConnectionProtection
    {
        None		= 0,
        Ssl			= 1,
        SignAndSeal	= 2
    }

    internal enum DirectoryType
    {
        AD = 0,
        ADAM = 1,
        Unknown = 2
    }

    internal enum CredentialsType
    {
        Windows = 0,
        NonWindows = 1
    }

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class ActiveDirectoryMembershipProvider : MembershipProvider
    {

        //
        // keeps track of whether the provider has already been initialized
        //
        private bool initialized = false;

        //
        // configuration parameters common to all membership providers
        //

        private string  adConnectionString;
        private bool enablePasswordRetrieval = false;
        private bool enablePasswordReset;
        private bool enableSearchMethods;
        private bool requiresQuestionAndAnswer;
        private string appName;
        private bool requiresUniqueEmail;
        private int maxInvalidPasswordAttempts;
        private int passwordAttemptWindow;
        private int passwordAnswerAttemptLockoutDuration;
        private int minRequiredPasswordLength;
        private int minRequiredNonalphanumericCharacters;
        private string passwordStrengthRegularExpression;
        private MembershipPasswordCompatibilityMode _LegacyPasswordCompatibilityMode = MembershipPasswordCompatibilityMode.Framework20;
        private int? passwordStrengthRegexTimeout;

        //
        // configuration parameters specific to the AD membership provider
        // and related to the directory connection are stored within the DirectoryInformation class
        //
        DirectoryInformation directoryInfo = null;

        //
        // custom schema mappings (and their default values)
        //
        private string attributeMapUsername = "userPrincipalName";
        private string attributeMapEmail = "mail";
        private string attributeMapPasswordQuestion = null;
        private string attributeMapPasswordAnswer = null;
        private string attributeMapFailedPasswordAnswerCount = null;
        private string attributeMapFailedPasswordAnswerTime= null;
        private string attributeMapFailedPasswordAnswerLockoutTime = null;

        //
        // maximum lengths for the different string properties
        //
        private int maxUsernameLength = 256;
        private int maxUsernameLengthForCreation = 64;
        private int maxPasswordLength = 128;
        private int maxCommentLength = 1024;
        private int maxEmailLength = 256;
        private int maxPasswordQuestionLength = 256;
        private int maxPasswordAnswerLength = 128;

        //
        // user account flags
        //
        private const int UF_ACCOUNT_DISABLED =0x2;
        private const int UF_LOCKOUT=0x10;
        private readonly DateTime DefaultLastLockoutDate = new DateTime(1754, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const int AD_SALT_SIZE_IN_BYTES = 16;

        //
        // table containing the valid syntaxes for various attribute mappings
        //
        Hashtable syntaxes = new Hashtable();
        Hashtable attributesInUse = new Hashtable(StringComparer.OrdinalIgnoreCase);
        Hashtable userObjectAttributes = null;

        //
        // auth type to be used for validation
        //
        AuthType authTypeForValidation;
        LdapConnection connection;
        bool usernameIsSAMAccountName = false;
        bool usernameIsUPN = true;

        //
        // password size for autogenerating password
        //
        private const int PASSWORD_SIZE      = 14;

        public override string ApplicationName
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return appName;
            }
            set
            {
                throw new NotSupportedException(SR.GetString(SR.ADMembership_Setting_ApplicationName_not_supported));
            }
        }

        public ActiveDirectoryConnectionProtection CurrentConnectionProtection
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return directoryInfo.ConnectionProtection;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                //
                // AD membership provider does not support password retrieval
                // (regardless of the settings). As a result the provider operates as
                // if the password was effectively hashed.
                //
                return MembershipPasswordFormat.Hashed;
            }
        }

        public override bool  EnablePasswordRetrieval
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return enablePasswordRetrieval;
             }
        }

        public override bool  EnablePasswordReset
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return enablePasswordReset;
            }
        }

        public bool  EnableSearchMethods
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return enableSearchMethods;
            }
        }

        public override bool  RequiresQuestionAndAnswer
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return requiresQuestionAndAnswer;
            }
        }

        public override bool  RequiresUniqueEmail
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return requiresUniqueEmail;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return maxInvalidPasswordAttempts;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return passwordAttemptWindow;
            }
        }

        public int PasswordAnswerAttemptLockoutDuration
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return passwordAnswerAttemptLockoutDuration;
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return minRequiredPasswordLength;
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return minRequiredNonalphanumericCharacters;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                if (!initialized)
                    throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

                return passwordStrengthRegularExpression;
            }
        }

        //
        // NOTE: In every method of the provider we need to demand DirectoryServicesPermission (irrespective of
        //           whether the underlying calls to S.DS/S.DS.Protocols result in full demand or link demand for that permission.
        //           Moreover, once we demand the permission, we should also assert it so that S.DS/S.DS.Protocols does not make the
        //           same demand (if we do not assert then in the case of S.DS/S.DS.Protocols making a full demand we would have two stack walks)
        //
        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Initialize(string name, NameValueCollection config)
        {
            if (System.Web.Hosting.HostingEnvironment.IsHosted)
                HttpRuntime.CheckAspNetHostingPermission (AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

            if (initialized)
                return;

            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "AspNetActiveDirectoryMembershipProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.ADMembership_Description));
            }

            base.Initialize(name, config);

            appName = config["applicationName"];

            if (string.IsNullOrEmpty(appName))
                appName = SecUtility.GetDefaultAppName();

            if( appName.Length > 256 )
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));

            string temp = config["connectionStringName"];
            if (String.IsNullOrEmpty(temp))
                throw new ProviderException(SR.GetString(SR.Connection_name_not_specified));

            adConnectionString = GetConnectionString(temp, true);
            if (String.IsNullOrEmpty(adConnectionString))
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, temp));

            //
            // Get the provider specific configuration settings
            //

            // connectionProtection
            string connProtection = config["connectionProtection"];
            if (connProtection == null)
                connProtection = "Secure";
            else
            {
                if ((String.Compare(connProtection, "Secure", StringComparison.Ordinal) != 0) &&
                    (String.Compare(connProtection, "None", StringComparison.Ordinal) != 0))
                    throw new ProviderException(SR.GetString(SR.ADMembership_InvalidConnectionProtection, connProtection));
            }

            //
            // credentials
            // username and password if specified must not be empty, moreover if one is specified the other must
            // be specified as well
            //
            string username = config["connectionUsername"];
            if (username != null && username.Length == 0)
                throw new ProviderException(SR.GetString(SR.ADMembership_Connection_username_must_not_be_empty));

            string password = config["connectionPassword"];
            if (password != null && password.Length == 0)
                throw new ProviderException(SR.GetString(SR.ADMembership_Connection_password_must_not_be_empty));

            if ((username != null && password == null) || (password != null && username == null))
                throw new ProviderException(SR.GetString(SR.ADMembership_Username_and_password_reqd));

            NetworkCredential credential = new NetworkCredential(username, password);

            int clientSearchTimeout = SecUtility.GetIntValue(config, "clientSearchTimeout", -1, false, 0);
            int serverSearchTimeout = SecUtility.GetIntValue(config, "serverSearchTimeout", -1, false, 0);
            TimeUnit timeoutUnit = SecUtility.GetTimeoutUnit(config, "timeoutUnit", TimeUnit.Minutes);
            passwordStrengthRegexTimeout = SecUtility.GetNullableIntValue(config, "passwordStrengthRegexTimeout");

            enableSearchMethods = SecUtility.GetBooleanValue(config, "enableSearchMethods", false);
            requiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", false);
            enablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", false);
            requiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            minRequiredPasswordLength = SecUtility.GetIntValue( config, "minRequiredPasswordLength", 7, false, 128 );
            minRequiredNonalphanumericCharacters = SecUtility.GetIntValue( config, "minRequiredNonalphanumericCharacters", 1, true, 128 );
          
            passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if( passwordStrengthRegularExpression != null )
            {
                passwordStrengthRegularExpression = passwordStrengthRegularExpression.Trim();
                if( passwordStrengthRegularExpression.Length != 0 )
                {
                    try
                    {
                        Regex regex = new Regex( passwordStrengthRegularExpression );
                    }
                    catch( ArgumentException e )
                    {
                        throw new ProviderException( e.Message, e );
                    }
                }
            }
            else
            {
                passwordStrengthRegularExpression = string.Empty;
            }
            if (minRequiredNonalphanumericCharacters > minRequiredPasswordLength)
                throw new HttpException(SR.GetString(SR.MinRequiredNonalphanumericCharacters_can_not_be_more_than_MinRequiredPasswordLength));


            using (new ApplicationImpersonationContext())
            {
                //
                //  This will make some checks regarding whether the connectionProtection is valid (choose the right
                //  connectionprotection if necessary, make sure credentials are valid, container exists and the directory is
                //  either AD or ADAM)
                //
                directoryInfo = new DirectoryInformation(adConnectionString, credential, connProtection, clientSearchTimeout, serverSearchTimeout, enablePasswordReset, timeoutUnit);

                //
                // initialize the syntaxes table
                //
                syntaxes.Add("attributeMapUsername", "DirectoryString");
                syntaxes.Add("attributeMapEmail", "DirectoryString");
                syntaxes.Add("attributeMapPasswordQuestion", "DirectoryString");
                syntaxes.Add("attributeMapPasswordAnswer", "DirectoryString");
                syntaxes.Add("attributeMapFailedPasswordAnswerCount", "Integer");
                syntaxes.Add("attributeMapFailedPasswordAnswerTime", "Integer8");
                syntaxes.Add("attributeMapFailedPasswordAnswerLockoutTime", "Integer8");

                //
                // initialize the in use attributes list
                //
                attributesInUse.Add("objectclass", null);
                attributesInUse.Add("objectsid", null);
                attributesInUse.Add("comment", null);
                attributesInUse.Add("whencreated", null);
                attributesInUse.Add("pwdlastset", null);
                attributesInUse.Add("msds-user-account-control-computed", null);
                attributesInUse.Add("lockouttime", null);
                if (directoryInfo.DirectoryType == DirectoryType.AD)
                    attributesInUse.Add("useraccountcontrol", null);
                else
                    attributesInUse.Add("msds-useraccountdisabled", null);

                //
                // initialize the user attributes list
                //
                userObjectAttributes = GetUserObjectAttributes();

                //
                // get the username/email schema mappings
                //
                int maxLength;
                string attrMapping = GetAttributeMapping(config, "attributeMapUsername", out maxLength);
                if (attrMapping != null)
                {
                    attributeMapUsername = attrMapping;
                    if (maxLength != -1)
                    {
                        if (maxLength < maxUsernameLength)
                            maxUsernameLength = maxLength;
                        if (maxLength < maxUsernameLengthForCreation)
                            maxUsernameLengthForCreation = maxLength;
                    }
                }
                attributesInUse.Add(attributeMapUsername, null);
                if (StringUtil.EqualsIgnoreCase(attributeMapUsername, "sAMAccountName"))
                {
                    usernameIsSAMAccountName = true;
                    usernameIsUPN = false;
                }

                attrMapping = GetAttributeMapping(config, "attributeMapEmail", out maxLength);
                if (attrMapping != null)
                {
                    attributeMapEmail = attrMapping;
                    if (maxLength != -1 && maxLength < maxEmailLength)
                        maxEmailLength = maxLength;
                }
                attributesInUse.Add(attributeMapEmail, null);

                //
                // get max length of "comment" attribute
                //
                maxLength = GetRangeUpperForSchemaAttribute("comment");
                if (maxLength != -1 && maxLength < maxCommentLength)
                    maxCommentLength = maxLength;

                //
                // enablePasswordReset and requiresQuestionAndAnswer should match
                //
                if (enablePasswordReset)
                {
                    //
                    // AD membership provider does not support password reset without question and answer
                    //
                    if (!requiresQuestionAndAnswer)
                        throw new ProviderException(SR.GetString(SR.ADMembership_PasswordReset_without_question_not_supported));

                    //
                    // Other password reset related attributes
                    //
                    maxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
                    passwordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
                    passwordAnswerAttemptLockoutDuration = SecUtility.GetIntValue(config, "passwordAnswerAttemptLockoutDuration", 30, false, 0);

                    //
                    // some more schema mappings that must be specified for Password Reset
                    //
                    attributeMapFailedPasswordAnswerCount = GetAttributeMapping(config, "attributeMapFailedPasswordAnswerCount", out maxLength /* ignored */);
                    if (attributeMapFailedPasswordAnswerCount != null)
                        attributesInUse.Add(attributeMapFailedPasswordAnswerCount, null);

                    attributeMapFailedPasswordAnswerTime = GetAttributeMapping(config, "attributeMapFailedPasswordAnswerTime", out maxLength /* ignored */);
                    if (attributeMapFailedPasswordAnswerTime != null)
                        attributesInUse.Add(attributeMapFailedPasswordAnswerTime, null);

                    attributeMapFailedPasswordAnswerLockoutTime = GetAttributeMapping(config, "attributeMapFailedPasswordAnswerLockoutTime", out maxLength /* ignored */);
                    if (attributeMapFailedPasswordAnswerLockoutTime != null)
                        attributesInUse.Add(attributeMapFailedPasswordAnswerLockoutTime, null);

                    if (attributeMapFailedPasswordAnswerCount == null || attributeMapFailedPasswordAnswerTime == null ||
                            attributeMapFailedPasswordAnswerLockoutTime == null)
                        throw new ProviderException(SR.GetString(SR.ADMembership_BadPasswordAnswerMappings_not_specified));
                }

                //
                // Password Q&A mappings
                //
                attributeMapPasswordQuestion = GetAttributeMapping(config, "attributeMapPasswordQuestion", out maxLength);
                if (attributeMapPasswordQuestion != null)
                {
                    if (maxLength != -1 && maxLength < maxPasswordQuestionLength)
                        maxPasswordQuestionLength = maxLength;

                    attributesInUse.Add(attributeMapPasswordQuestion, null);
                }

                attributeMapPasswordAnswer = GetAttributeMapping(config, "attributeMapPasswordAnswer", out maxLength);
                if (attributeMapPasswordAnswer != null)
                {
                    if (maxLength != -1 && maxLength < maxPasswordAnswerLength)
                        maxPasswordAnswerLength = maxLength;

                    attributesInUse.Add(attributeMapPasswordAnswer, null);
                }

                if (requiresQuestionAndAnswer)
                {
                    //
                    // We also need to check that the password question and answer attributes are mapped
                    //
                    if (attributeMapPasswordQuestion == null || attributeMapPasswordAnswer == null)
                        throw new ProviderException(SR.GetString(SR.ADMembership_PasswordQuestionAnswerMapping_not_specified));
                }

                //
                // the auth type to be used for validation is determined as follows:
                // if directory is ADAM: authType = AuthType.Basic
                // if directory is AD: authType is based on connectionProtection (None, SSL: AuthType.Basic; SignAndSeal: AuthType.Negotiate)
                //
                if (directoryInfo.DirectoryType == DirectoryType.ADAM)
                    authTypeForValidation = AuthType.Basic;
                else
                    authTypeForValidation = directoryInfo.GetLdapAuthenticationTypes(directoryInfo.ConnectionProtection, CredentialsType.NonWindows);

                if (directoryInfo.DirectoryType == DirectoryType.AD)
                {
                    //
                    // if password reset is enabled we should perform all operations on a single server
                    //
                    if (enablePasswordReset)
                        directoryInfo.SelectServer();

                    //
                    // if the username is mapped to upn we need to do  forest wide search to check the uniqueness of the upn.
                    // and if the username is mapped to samAccountName then we need to append the domain name in the username for reliable validation
                    //
                    directoryInfo.InitializeDomainAndForestName();

                }
            }

            //
            // Create a new common ldap connection for validation
            //
            connection = directoryInfo.CreateNewLdapConnection(authTypeForValidation);

            temp = config["passwordCompatMode"];
            if (!string.IsNullOrEmpty(temp))
                _LegacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode) Enum.Parse(typeof(MembershipPasswordCompatibilityMode), temp);

            config.Remove("name");
            config.Remove("applicationName");
            config.Remove("connectionStringName");
            config.Remove("requiresUniqueEmail");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("attributeMapPasswordQuestion");
            config.Remove("attributeMapPasswordAnswer");
            config.Remove("attributeMapUsername");
            config.Remove("attributeMapEmail");
            config.Remove("connectionProtection");
            config.Remove("connectionUsername");
            config.Remove("connectionPassword");
            config.Remove("clientSearchTimeout");
            config.Remove("serverSearchTimeout");
            config.Remove("timeoutUnit");
            config.Remove("enableSearchMethods");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordAnswerAttemptLockoutDuration");
            config.Remove("attributeMapFailedPasswordAnswerCount");
            config.Remove("attributeMapFailedPasswordAnswerTime");
            config.Remove("attributeMapFailedPasswordAnswerLockoutTime");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("passwordCompatMode");
            config.Remove("passwordStrengthRegexTimeout");

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }

            initialized = true;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUser CreateUser(string username,
                                                        string password,
                                                        string email,
                                                        string passwordQuestion,
                                                        string passwordAnswer,
                                                        bool   isApproved,
                                                        object providerUserKey,
                                                        out    MembershipCreateStatus status)
        {
            status = (MembershipCreateStatus) 0;
            MembershipUser user = null;

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if (providerUserKey != null)
                throw new NotSupportedException(SR.GetString(SR.ADMembership_Setting_UserId_not_supported));

            if ((passwordQuestion != null) && (attributeMapPasswordQuestion == null))
                throw new NotSupportedException(SR.GetString(SR.ADMembership_PasswordQ_not_supported));

            if ((passwordAnswer != null) && (attributeMapPasswordAnswer == null))
                throw new NotSupportedException(SR.GetString(SR.ADMembership_PasswordA_not_supported));

            if(!SecUtility.ValidateParameter(ref username, true, true, true, maxUsernameLengthForCreation))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            //
            // if username is mapped to UPN, it should not contain '\'
            //
            if (usernameIsUPN && (username.IndexOf('\\') != -1))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if(!ValidatePassword(password, maxPasswordLength))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if(!SecUtility.ValidateParameter(ref email, RequiresUniqueEmail, true, false, maxEmailLength))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if(!SecUtility.ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, maxPasswordQuestionLength))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            // validate the parameter before encoding the password answer
            if(!SecUtility.ValidateParameter(ref passwordAnswer, RequiresQuestionAndAnswer, true, false, maxPasswordAnswerLength))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            string encodedPasswordAnswer;
            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                encodedPasswordAnswer = Encrypt(passwordAnswer);

                // check length of encoded password answer
                if (maxPasswordAnswerLength > 0 && encodedPasswordAnswer.Length > maxPasswordAnswerLength)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
            }
            else
                encodedPasswordAnswer = passwordAnswer;

            if( password.Length < MinRequiredPasswordLength )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            int count = 0;

            for( int i = 0; i < password.Length; i++ )
            {
                if( !char.IsLetterOrDigit( password, i ) )
                {
                    count++;
                }
            }

            if( count < MinRequiredNonAlphanumericCharacters )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if( PasswordStrengthRegularExpression.Length > 0 )
            {
                if( !RegexUtil.IsMatch( password, PasswordStrengthRegularExpression, RegexOptions.None, passwordStrengthRegexTimeout ) )
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(e);

            if(e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            try
            {
                //
                // Get the directory entry for the container and create a user object under it
                //
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.CreationContainerDN, true /* revertImpersonation */);

                DirectoryEntry containerEntry = null;
                DirectoryEntry userEntry = null;

                try
                {
                    containerEntry = connection.DirectoryEntry;
                    // to avoid unnecessary searches (for better performance)
                    containerEntry.AuthenticationType |= AuthenticationTypes.FastBind;

                    //
                    // we set the username as the cn
                    //
                    userEntry = containerEntry.Children.Add(GetEscapedRdn("CN=" + username), "user");

                    //
                    // if we are talking to Active Directory
                    // set the sAMAccountName (if username is not mapped to this attribute, we need to autogenerate it)
                    // (NOTE: We do not need to do this if the domain controller functionality is Windows 2003 (dcLevel = 2))
                    //
                    if (directoryInfo.DirectoryType == DirectoryType.AD)
                    {
                        string sAMAccountName= null;
                        bool setSAMAccountName = false;

                        if (usernameIsSAMAccountName)
                        {
                            sAMAccountName = username;
                            setSAMAccountName = true;
                        }
                        else
                        {
                            int dcLevel = GetDomainControllerLevel(containerEntry.Options.GetCurrentServerName());

                            if (dcLevel != 2)
                            {
                                sAMAccountName = GenerateAccountName();
                                setSAMAccountName = true;
                            }
                        }

                        if (setSAMAccountName)
                            userEntry.Properties["sAMAccountName"].Value = sAMAccountName;
                    }

                    //
                    // if username is mapped to userPrincipalName and we are talking to AD, we need to do
                    // a GC search to find if the same upn already exists or not
                    // On ADAM, uniqueness of userPrincipalName is enforced on the server itself across all partitions
                    //
                    if (usernameIsUPN)
                    {
                        if (directoryInfo.DirectoryType == DirectoryType.AD && !IsUpnUnique(username))
                        {
                            status = MembershipCreateStatus.DuplicateUserName;
                            return null;
                        }

                        userEntry.Properties["userPrincipalName"].Value = username;
                    }

                    //
                    // set other attributes
                    //
                    if (email != null)
                    {
                        if (RequiresUniqueEmail && !IsEmailUnique(containerEntry, username, email, false /* existing */))
                        {
                            status = MembershipCreateStatus.DuplicateEmail;
                            return null;
                        }
                        userEntry.Properties[attributeMapEmail].Value = email;
                    }

                    if (passwordQuestion != null)
                        userEntry.Properties[attributeMapPasswordQuestion].Value = passwordQuestion;

                    if (passwordAnswer != null)
                        userEntry.Properties[attributeMapPasswordAnswer].Value = encodedPasswordAnswer;

                    //
                    // commit the user object
                    //
                    try
                    {
                        userEntry.CommitChanges();
                    }
                    catch (COMException e1)
                    {
                        if ((e1.ErrorCode == unchecked((int) 0x80071392)) || (e1.ErrorCode == unchecked((int) 0x8007200d)))
                        {
                            status = MembershipCreateStatus.DuplicateUserName;
                            return null;
                        }
                        else if ((e1.ErrorCode == unchecked((int) 0x8007001f)) && (e1 is DirectoryServicesCOMException))
                        {
                            //
                            // this error corresponds to LDAP_OTHER
                            // if username was mapped to sAMAccountName and the name is too long
                            // then the extended error should be 1315 (ERROR_INVALID_ACCOUNT_NAME)
                            //
                            DirectoryServicesCOMException dsce = e1 as DirectoryServicesCOMException;
                            if (dsce.ExtendedError == 1315)
                            {
                                status = MembershipCreateStatus.InvalidUserName;
                                return null;
                            }
                            else
                                throw;
                        }
                        else
                            throw;
                    }

                    //
                    // set the password
                    //
                    try
                    {
                        SetPasswordPortIfApplicable(userEntry);

                        //
                        // Set the password
                        //
                        userEntry.Invoke("SetPassword", new object[]{ password });

                        //
                        // if the user is approved then we need to enable the account (disabled dy default)
                        //
                        if (isApproved)
                        {
                            if (directoryInfo.DirectoryType ==  DirectoryType.AD)
                            {
                                const int UF_ACCOUNT_DISABLED =0x2;
                                const int UF_PASSWD_NOTREQD = 0x20;

                                int val = (int)PropertyManager.GetPropertyValue(userEntry, "userAccountControl");
                                val &= ~(UF_ACCOUNT_DISABLED | UF_PASSWD_NOTREQD);
                                userEntry.Properties["userAccountControl"].Value = val;
                            }
                            else
                            {
                                // ADAM case
                                userEntry.Properties["msDS-UserAccountDisabled"].Value = false;
                            }
                            userEntry.CommitChanges();
                        }
                        else
                        {
                            //
                            // For ADAM the user may be created as enabled in some cases
                            // so we need to explicitly disable it
                            //
                            if (directoryInfo.DirectoryType ==  DirectoryType.ADAM)
                            {
                                userEntry.Properties["msDS-UserAccountDisabled"].Value = true;
                                userEntry.CommitChanges();
                            }
                        }

                        //
                        // For ADAM users, we need to add the user to the Readers group in that
                        // partition
                        //
                        if (directoryInfo.DirectoryType == DirectoryType.ADAM)
                        {
                            DirectoryEntry readersEntry = new DirectoryEntry(directoryInfo.GetADsPath("CN=Readers,CN=Roles," + directoryInfo.ADAMPartitionDN), directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);
                            readersEntry.Properties["member"].Add(PropertyManager.GetPropertyValue(userEntry, "distinguishedName"));
                            readersEntry.CommitChanges();
                        }
                    }
                    //
                    // At this point we have already created the user object in AD/ADAM but we
                    // have failed in either SetPassword or while enabling/disabling the user, so we try to delete the user object
                    //
                    catch (COMException)
                    {
                        containerEntry.Children.Remove(userEntry);
                        throw;
                    }
                    catch (ProviderException)
                    {
                        containerEntry.Children.Remove(userEntry);
                        throw;
                    }
                    catch (TargetInvocationException tie)
                    {
                        containerEntry.Children.Remove(userEntry);

                        if (tie.InnerException is COMException)
                        {
                            COMException ce = (COMException) tie.InnerException;
                            int errorCode = ce.ErrorCode;

                            //
                            // if the exception is due to password not meeting complexity requirements, then return
                            // status as InvalidPassword
                            //
                            if ((errorCode == unchecked((int) 0x800708c5)) || (errorCode == unchecked((int) 0x8007202f)) || (errorCode == unchecked((int) 0x8007052d)) || (errorCode == unchecked((int) 0x8007052f)))
                            {
                                status = MembershipCreateStatus.InvalidPassword;
                                return null;
                            }
                            // if the target is ADAM and the exception is due to property not found, this indicates that a secure
                            // connection could not be setup for changing the password and ADSI is falling back to kerberos which does not work for ADAM
                            // so we will provide a clearer exception
                            //
                            else if ((errorCode == unchecked((int) 0x8000500d) && (directoryInfo.DirectoryType == DirectoryType.ADAM)))
                                throw new ProviderException(SR.GetString(SR.ADMembership_No_secure_conn_for_password));
                            else
                                throw;
                        }
                        else
                            throw;
                    }

                    //
                    // Create a user object
                    //
                    DirectoryEntry dummyEntry = null;
                    bool dummyFlag = false;
                    string dummyString;
                    user = FindUser(userEntry, "(objectClass=*)", System.DirectoryServices.SearchScope.Base, false /*retrieveSAMAccountName */, out dummyEntry, out dummyFlag, out dummyString);
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();

                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return user;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool ChangePasswordQuestionAndAnswer(string username,
                                                        string password,
                                                        string newPasswordQuestion,
                                                        string newPasswordAnswer)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            //
            // if there are no mappings for password question and answer, we should throw a NotSupportedException
            //
            if ((newPasswordQuestion != null) && (attributeMapPasswordQuestion == null))
                throw new NotSupportedException(SR.GetString(SR.ADMembership_PasswordQ_not_supported));

            if ((newPasswordAnswer != null) && (attributeMapPasswordAnswer == null))
                throw new NotSupportedException(SR.GetString(SR.ADMembership_PasswordA_not_supported));


            CheckUserName( ref username, maxUsernameLength, "username" );
            CheckPassword(password, maxPasswordLength, "password");

            SecUtility.CheckParameter(
                            ref newPasswordQuestion,
                            RequiresQuestionAndAnswer,
                            true,
                            false,
                            maxPasswordQuestionLength,
                            "newPasswordQuestion" );

            // validate the parameter before encoding the password answer
            CheckPasswordAnswer(ref newPasswordAnswer, RequiresQuestionAndAnswer, maxPasswordAnswerLength, "newPasswordAnswer");

            string encodedPasswordAnswer;
            if (!string.IsNullOrEmpty(newPasswordAnswer))
            {
                encodedPasswordAnswer = Encrypt(newPasswordAnswer);

                // check length of encoded password answer
                if (maxPasswordAnswerLength > 0 && encodedPasswordAnswer.Length > maxPasswordAnswerLength)
                    throw new ArgumentException(SR.GetString(SR.ADMembership_Parameter_too_long, "newPasswordAnswer"), "newPasswordAnswer");
            }
            else
                encodedPasswordAnswer = newPasswordAnswer;

            try
            {
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string usernameForAuthentication = null;

                try
                {
                    if (EnablePasswordReset)
                    {
                        //
                        // get the user's directory entry
                        // NOTE: If the username is mapped to userPrincipalName and the username does not contain '@' in it, then simple bind will fail as it needs domain information.
                        //           To workaround this whenever we are talking to AD, username is mapped to userPrincipalName and does not contain '@', we will get the sAMAccountName
                        //           while getting the user object and use that for authenticating the user.
                        //
                        MembershipUser user = null;
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = FindUserAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            user = FindUser(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (user == null)
                            return false;

                        //
                        // here we want to check if the user is already unlocked due to bad password answer (or bad password)
                        //
                        if (user.IsLockedOut)
                            return false;

                    }
                    else
                    {
                        //
                        // get the user's directory entry
                        //
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            userEntry = FindUserEntryAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")");
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (userEntry == null)
                            return false;
                    }

                    //
                    // validate the user's credentials
                    //
                    if (!ValidateCredentials(usernameForAuthentication, password))
                        return false;

                    if (EnablePasswordReset && resetBadPasswordAnswerAttributes)
                    {
                        //
                        // user supplied correct password, so we need to reset the password answer tracking info
                        // (NOTE: The reason we do not call the Reset method here is so that we can make all the modifications in one transaction)
                        //
                        userEntry.Properties[attributeMapFailedPasswordAnswerCount].Value = 0;
                        userEntry.Properties[attributeMapFailedPasswordAnswerTime].Value = 0;
                        userEntry.Properties[attributeMapFailedPasswordAnswerLockoutTime].Value = 0;
                    }

                    if (newPasswordQuestion == null)
                    {
                        // set it to null only if it already exists
                        if ((attributeMapPasswordQuestion != null) && (userEntry.Properties.Contains(attributeMapPasswordQuestion)))
                            userEntry.Properties[attributeMapPasswordQuestion].Clear();
                    }
                    else
                        userEntry.Properties[attributeMapPasswordQuestion].Value = newPasswordQuestion;

                    if (newPasswordAnswer == null)
                    {
                        // set it to null only if it already exists
                        if ((attributeMapPasswordAnswer != null) && (userEntry.Properties.Contains(attributeMapPasswordAnswer)))
                            userEntry.Properties[attributeMapPasswordAnswer].Clear();
                    }
                    else
                        userEntry.Properties[attributeMapPasswordAnswer].Value = encodedPasswordAnswer;

                    userEntry.CommitChanges();

                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            //
            // Password question and answer changed successfully
            //
            return true;
        }

        public override string GetPassword(string username, string passwordAnswer)
        {
            //
            // ADMembership Provider does not support password retrieval
            //
            throw new NotSupportedException(SR.GetString(SR.ADMembership_PasswordRetrieval_not_supported_AD));
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool ChangePassword(string username,
                                                        string oldPassword,
                                                        string newPassword)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            CheckUserName(ref username, maxUsernameLength, "username" );

            CheckPassword(oldPassword, maxPasswordLength, "oldPassword");

            CheckPassword(newPassword, maxPasswordLength, "newPassword");

            if( newPassword.Length < MinRequiredPasswordLength )
            {
                throw new ArgumentException(SR.GetString(SR.Password_too_short,
                              "newPassword",
                              MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture)));
            }

            int count = 0;

            for( int i = 0; i < newPassword.Length; i++ )
            {
                if( !char.IsLetterOrDigit( newPassword, i ) )
                {
                    count++;
                }
            }

            if( count < MinRequiredNonAlphanumericCharacters )
            {
                throw new ArgumentException(SR.GetString(SR.Password_need_more_non_alpha_numeric_chars,
                              "newPassword",
                              MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)));
            }

            if( PasswordStrengthRegularExpression.Length > 0 )
            {
                if( !RegexUtil.IsMatch( newPassword, PasswordStrengthRegularExpression, RegexOptions.None, passwordStrengthRegexTimeout ) )
                {
                    throw new ArgumentException(SR.GetString(SR.Password_does_not_match_regular_expression,
                                                             "newPassword"));
                }
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs( username, newPassword, false );
            OnValidatingPassword(e);

            if(e.Cancel)
            {
                if(e.FailureInformation != null)
                    throw e.FailureInformation;
                else
                    throw new ArgumentException(SR.GetString(SR.Membership_Custom_Password_Validation_Failure), "newPassword");
            }

            try
            {
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string usernameForAuthentication = null;

                try
                {
                    if (EnablePasswordReset)
                    {
                        //
                        // get the user's directory entry
                        // NOTE: If the username is mapped to userPrincipalName and the username does not contain '@' in it, the S.DS(adsi) will pass NULL
                        //           domain name to the underlying wldap32 layer. This results in authentication failure even for valid credentials. To workaround this
                        //           whenever we are talking to AD, username is mapped to userPrincipalName and does not contain '@', we will get the sAMAccountName
                        //           while getting the user object and use that for changing the password.
                        //
                        MembershipUser user = null;
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = FindUserAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            user = FindUser(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (user == null)
                            return false;

                        //
                        // here we want to check if the user is already unlocked due to bad password answer (or bad password)
                        //
                        if (user.IsLockedOut)
                            return false;
                    }
                    else
                    {
                        //
                        // get the user's directory entry (Also get sAMAccountName if needed)
                        //
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            userEntry = FindUserEntryAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")");
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (userEntry == null)
                            return false;
                    }

                    //
                    // associate the user's context with the directory entry
                    //
                    userEntry.Username = (usernameIsSAMAccountName) ? directoryInfo.DomainName + "\\" + usernameForAuthentication : usernameForAuthentication;
                    userEntry.Password = oldPassword;
                    userEntry.AuthenticationType = directoryInfo.GetAuthenticationTypes(directoryInfo.ConnectionProtection, (directoryInfo.DirectoryType == DirectoryType.AD) ? CredentialsType.Windows : CredentialsType.NonWindows);

                    try
                    {
                        SetPasswordPortIfApplicable(userEntry);

                        //
                        // Change the password
                        //
                        userEntry.Invoke("ChangePassword", new object[]{ oldPassword, newPassword });
                    }
                    catch (COMException e2)
                    {
                        if (e2.ErrorCode == unchecked((int) 0x8007052e))
                            return false;
                        else
                            throw;
                    }
                    catch (TargetInvocationException tie)
                    {
                        if (tie.InnerException is COMException)
                        {
                            COMException ce = (COMException) tie.InnerException;
                            int errorCode = ce.ErrorCode;

                            //
                            // if the exception is due to password not meeting complexity requirements, then return
                            // MembershipPasswordException
                            //
                            if ((errorCode == unchecked((int) 0x800708c5)) || (errorCode == unchecked((int) 0x8007202f))  || (errorCode == unchecked((int) 0x8007052d)) || (errorCode == unchecked((int) 0x8007052f)))
                                throw new MembershipPasswordException(SR.GetString(SR.Membership_InvalidPassword), ce);
                            //
                            // if the target is ADAM and the exception is due to property not found, this indicates that a secure
                            // connection could not be setup for changing the password and ADSI is falling back to kerberos which does not work for ADAM
                            // so we will provide a clearer exception
                            //
                            else if ((errorCode == unchecked((int) 0x8000500d) && (directoryInfo.DirectoryType == DirectoryType.ADAM)))
                                throw new ProviderException(SR.GetString(SR.ADMembership_No_secure_conn_for_password));
                            else
                                throw;
                        }
                        else
                            throw;
                    }

                    if (EnablePasswordReset && resetBadPasswordAnswerAttributes)
                    {
                        //
                        // associate the process context with the directory entry
                        //
                        userEntry.Username = directoryInfo.GetUsername();
                        userEntry.Password = directoryInfo.GetPassword();
                        userEntry.AuthenticationType = directoryInfo.AuthenticationTypes;

                        //
                        // user supplied correct password, so we need to reset the password answer tracking info
                        //
                        ResetBadPasswordAnswerAttributes(userEntry);
                    }
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            //
            // Password changed successfully
            //
            return true;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override string ResetPassword(string username, string passwordAnswer)
        {
            string newPassword = null;

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if (!EnablePasswordReset)
                throw new NotSupportedException(SR.GetString(SR.Not_configured_to_support_password_resets));

            CheckUserName(ref username, maxUsernameLength, "username");

            CheckPasswordAnswer(ref passwordAnswer, RequiresQuestionAndAnswer, maxPasswordAnswerLength, "passwordAnswer");

            try
            {
                //
                // validate the password answer
                //
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;

                try
                {
                    //
                    // get the user's directory entry
                    //
                    MembershipUser user = FindUser(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);

                    //
                    // user does not exist, throw exception
                    //
                    if (user == null)
                        throw new ProviderException(SR.GetString(SR.Membership_UserNotFound));

                    //
                    // if user is locked, throw an exception
                    //
                    if (user.IsLockedOut)
                        throw new MembershipPasswordException(SR.GetString(SR.Membership_AccountLockOut));

                    string storedPasswordAnswer = Decrypt((string) PropertyManager.GetPropertyValue(userEntry, attributeMapPasswordAnswer));
                    if (!StringUtil.EqualsIgnoreCase(passwordAnswer, storedPasswordAnswer))
                    {
                        UpdateBadPasswordAnswerAttributes(userEntry);
                        throw new MembershipPasswordException(SR.GetString(SR.Membership_WrongAnswer));
                    }
                    else
                    {
                        if (resetBadPasswordAnswerAttributes)
                            ResetBadPasswordAnswerAttributes(userEntry);
                    }

                    SetPasswordPortIfApplicable(userEntry);

                    //
                    // Reset  the password (generating a random new password)
                    //
                    newPassword = GeneratePassword();

                    ValidatePasswordEventArgs e = new ValidatePasswordEventArgs( username, newPassword, false );
                    OnValidatingPassword(e);

                    if(e.Cancel)
                    {
                        if(e.FailureInformation != null)
                            throw e.FailureInformation;
                        else
                            throw new ProviderException(SR.GetString(SR.Membership_Custom_Password_Validation_Failure));
                    }

                    userEntry.Invoke("SetPassword", new object[]{ newPassword });

                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException is COMException)
                    {
                        COMException ce = (COMException) tie.InnerException;
                        int errorCode = ce.ErrorCode;

                        //
                        // if the exception is due to password not meeting complexity requirements, then return
                        // ProviderException
                        //
                        if ((errorCode == unchecked((int) 0x800708c5)) || (errorCode == unchecked((int) 0x8007202f))  || (errorCode == unchecked((int) 0x8007052d)) || (errorCode == unchecked((int) 0x8007052f)))
                            throw new ProviderException(SR.GetString(SR.ADMembership_Generated_password_not_complex), ce);
                        //
                        // if the target is ADAM and the exception is due to property not found, this indicates that a secure
                        // connection could not be setup for changing the password and ADSI is falling back to kerberos which does not work for ADAM
                        // so we will provide a clearer exception
                        //
                        if ((errorCode == unchecked((int) 0x8000500d) && (directoryInfo.DirectoryType == DirectoryType.ADAM)))
                            throw new ProviderException(SR.GetString(SR.ADMembership_No_secure_conn_for_password));
                        else
                            throw;
                    }
                    else
                        throw;
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            //
            // Password was reset successfully, return the generated password
            //
            return newPassword;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool UnlockUser(string username)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            CheckUserName( ref username, maxUsernameLength, "username" );

            try
            {
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;

                try
                {
                    //
                    // get the user's directory entry
                    //
                    userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")");

                    //
                    // user does not exist, return false
                    //
                    if (userEntry == null)
                        return false;

                    userEntry.Properties["lockoutTime"].Value = 0;

                    if (EnablePasswordReset)
                    {
                        userEntry.Properties[attributeMapFailedPasswordAnswerCount].Value = 0;
                        userEntry.Properties[attributeMapFailedPasswordAnswerTime].Value = 0;
                        userEntry.Properties[attributeMapFailedPasswordAnswerLockoutTime].Value = 0;
                    }

                    userEntry.CommitChanges();
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            //
            // user unlocked successfully, return true
            //
            return true;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void UpdateUser(MembershipUser user)
        {
            bool emailModified = true;
            bool commentModified = true;
            bool isApprovedModified = true;

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if( user == null )
            {
                throw new ArgumentNullException("user" );
            }

            ActiveDirectoryMembershipUser adUser = user as ActiveDirectoryMembershipUser;

            if (adUser != null)
            {
                //
                // check which fields have really been modified
                //
                emailModified = adUser.emailModified;
                commentModified = adUser.commentModified;
                isApprovedModified = adUser.isApprovedModified;
            }

            string temp = user.UserName;
            CheckUserName( ref temp, maxUsernameLength, "UserName" );

            string email = user.Email;
            if (emailModified)
                SecUtility.CheckParameter( ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, maxEmailLength, "Email");

            if (commentModified && user.Comment != null)
            {
                if (user.Comment.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty, "Comment"), "Comment");

                if (maxCommentLength > 0 && user.Comment.Length > maxCommentLength)
                    throw new ArgumentException(SR.GetString(SR.Parameter_too_long, "Comment", maxCommentLength.ToString(CultureInfo.InvariantCulture)), "Comment");
            }

            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;

                try
                {
                    //
                    // get the user's directory entry
                    //
                    userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(user.UserName) + ")");

                    if (userEntry == null)
                        throw new ProviderException(SR.GetString(SR.Membership_UserNotFound));

                    if (!((emailModified) || (commentModified) || (isApprovedModified)))
                        // nothing has been modified
                        return;

                    //
                    // update the email
                    // if enableUniqueEmail is specified, we need to ensure that the email is unique
                    //
                    if (emailModified)
                    {
                        if (email == null)
                        {
                            // set the email to null only if email already exists
                            if (userEntry.Properties.Contains(attributeMapEmail))
                                userEntry.Properties[attributeMapEmail].Clear();
                        }
                        else
                        {
                            if (RequiresUniqueEmail && !IsEmailUnique(null, user.UserName, email, true /* existing */))
                                throw new ProviderException(SR.GetString(SR.Membership_DuplicateEmail));

                            userEntry.Properties[attributeMapEmail].Value = email;
                        }
                    }

                    //
                    // update the comment
                    //
                    if (commentModified)
                    {
                        if (user.Comment == null)
                        {
                            // set the comment to null only if comment already exists
                            if (userEntry.Properties.Contains("comment"))
                                userEntry.Properties["comment"].Clear();
                        }
                        else
                        {
                            //
                            // we use the original value ("user.Comment") to preserve all white space
                            // (including leading and trailing white space)
                            userEntry.Properties["comment"].Value = user.Comment;
                        }
                    }

                    //
                    // update the IsApproved field
                    //
                    if (isApprovedModified)
                    {
                        if (directoryInfo.DirectoryType == DirectoryType.AD)
                        {
                            // userAccountControl attribute
                            const int UF_ACCOUNT_DISABLED =0x2;

                            int val = (int)PropertyManager.GetPropertyValue(userEntry, "userAccountControl");

                            if (user.IsApproved)
                                val &= ~UF_ACCOUNT_DISABLED;
                            else
                                val |= UF_ACCOUNT_DISABLED;
                            userEntry.Properties["userAccountControl"].Value = val;
                        }
                        else
                        {
                            // different attribute for ADAM
                            userEntry.Properties["msDS-UserAccountDisabled"].Value = !(user.IsApproved);
                        }
                    }

                    userEntry.CommitChanges();

                    if (adUser != null)
                    {
                        adUser.emailModified = false;
                        adUser.commentModified = false;
                        adUser.isApprovedModified = false;
                    }

                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override bool ValidateUser(string username, string password)
        {
            if( ValidateUserCore(username, password))
            {
                PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_SUCCESS);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditMembershipAuthenticationSuccess, username);
                return true;
            } else {
                PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_FAIL);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditMembershipAuthenticationFailure, username);
                return false;
            }
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        private bool ValidateUserCore(string username, string password)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if(!SecUtility.ValidateParameter(ref username, true, true, true, maxUsernameLength))
            {
                return false;
            }

            //
            // if username is mapped to UPN, it should not contain '\'
            //
            if (usernameIsUPN && (username.IndexOf('\\') != -1))
            {
                return false;
            }

            if( !ValidatePassword(password, maxPasswordLength))
            {
                return false;
            }

            bool result = false;
            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string usernameForAuthentication = null;

                try
                {
                    if (EnablePasswordReset)
                    {
                        //
                        // get the user's directory entry
                        // NOTE: If the username is mapped to userPrincipalName and the username does not contain '@' in it, then simple bind will fail as it needs domain information.
                        //           To workaround this whenever we are talking to AD, username is mapped to userPrincipalName and does not contain '@', we will get the sAMAccountName
                        //           while getting the user object and use that for authenticating the user.
                        //
                        MembershipUser user = null;
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = FindUserAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            user = FindUser(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (user == null)
                            return false;

                        //
                        // here we want to check if the user is already unlocked due to bad password answer (or bad password)
                        //
                        if (user.IsLockedOut)
                            return false;
                    }
                    else
                    {
                        //
                        // get the user's directory entry
                        //
                        if ((directoryInfo.DirectoryType == DirectoryType.AD) && (usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            userEntry = FindUserEntryAndSAMAccountName(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out sAMAccountName);
                            usernameForAuthentication = directoryInfo.DomainName + "\\" + sAMAccountName;
                        }
                        else
                        {
                            userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")");
                            usernameForAuthentication = username;
                        }

                        //
                        // user does not exist, return false
                        //
                        if (userEntry == null)
                            return false;
                    }

                    result = ValidateCredentials(usernameForAuthentication, password);

                    if (EnablePasswordReset && result && resetBadPasswordAnswerAttributes)
                    {
                        //
                        // user supplied correct password, so we need to reset the password answer tracking info
                        //
                        ResetBadPasswordAnswerAttributes(userEntry);
                    }

                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return result;

        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser user = null;

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if( providerUserKey == null )
            {
                throw new ArgumentNullException( "providerUserKey" );
            }

            if ( !( providerUserKey is SecurityIdentifier) )
            {
                throw new ArgumentException( SR.GetString(SR.ADMembership_InvalidProviderUserKey) , "providerUserKey" );
            }

            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;

                try
                {
                    //
                    // Search for the user and return a MembershipUser object
                    //
                    SecurityIdentifier sid = providerUserKey as SecurityIdentifier;
                    StringBuilder sidHexValueStr = new StringBuilder();
                    int binaryLength = sid.BinaryLength;
                    byte[] sidBinaryForm = new byte[binaryLength];
                    sid.GetBinaryForm(sidBinaryForm, 0);

                    for (int i = 0; i < binaryLength; i++)
                    {
                        sidHexValueStr.Append("\\");
                        sidHexValueStr.Append(sidBinaryForm[i].ToString("x2", NumberFormatInfo.InvariantInfo));
                    }

                    DirectoryEntry dummyEntry;
                    bool resetBadPasswordAnswerAttributes = false;
                    user = FindUser(containerEntry, "(" + attributeMapUsername + "=*)(objectSid=" + sidHexValueStr.ToString() + ")", out dummyEntry /* ignored */, out resetBadPasswordAnswerAttributes /* ignored */);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return user;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser user = null;

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            CheckUserName(ref username, maxUsernameLength, "username" );

            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;

                try
                {
                    //
                    // Search for the user and return a MembershipUser object
                    //
                    DirectoryEntry dummyEntry;
                    bool resetBadPasswordAnswerAttributes = false;
                    user = FindUser(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", out dummyEntry /*ignored */, out resetBadPasswordAnswerAttributes /* ignored */);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return user;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override string GetUserNameByEmail(string email)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            SecUtility.CheckParameter(ref email, false, true, false, maxEmailLength,  "email");

            string username = null;
            try
            {
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                SearchResultCollection resCol = null;

                try
                {
                    DirectorySearcher searcher = new DirectorySearcher(containerEntry);
                    if (email != null)
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + attributeMapUsername + "=*)(" + attributeMapEmail + "=" + GetEscapedFilterValue(email) +"))";
                    else
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + attributeMapUsername + "=*)(!(" + attributeMapEmail + "=" +"*)))";
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add(attributeMapUsername);

                    if (directoryInfo.ClientSearchTimeout != -1)
                        searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
                    if (directoryInfo.ServerSearchTimeout != -1)
                        searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

                    resCol = searcher.FindAll();
                    bool userFound = false;

                    foreach (SearchResult res in resCol)
                    {
                        if (!userFound)
                        {
                            username = (string) PropertyManager.GetSearchResultPropertyValue(res, attributeMapUsername);
                            userFound = true;

                            if (!RequiresUniqueEmail)
                                break;
                        }
                        else
                        {
                            if (RequiresUniqueEmail)
                            {
                                // there is a duplicate entry, so we need to throw an ProviderException
                                throw new ProviderException(SR.GetString(SR.Membership_more_than_one_user_with_email));
                            }
                            else
                                // we should never get here
                                break;
                        }
                    }
                }
                finally
                {
                    if (resCol != null)
                        resCol.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return username;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {

            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            CheckUserName(ref username, maxUsernameLength, "username");

            try
            {
                //
                // Get the Directory Entry for the container
                //
                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.CreationContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;
                // to avoid unnecessary searches (for better performance)
                containerEntry.AuthenticationType |= AuthenticationTypes.FastBind;
                DirectoryEntry userEntry = null;

                try
                {
                    //
                    // Get the directory entry for the user
                    //
                    string dummyString;
                    userEntry = FindUserEntry(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(username) + ")", System.DirectoryServices.SearchScope.OneLevel, false /* retrieveSAMAccountName */, out dummyString);

                    if (userEntry == null)
                        return false;

                    //
                    // Remove the entry from the container
                    //
                    containerEntry.Children.Remove(userEntry);

                }
                catch (COMException e)
                {
                    if (e.ErrorCode == unchecked((int) 0x80072030))
                    {
                        //
                        // incase some one else deleted the object just before this
                        //
                        return false;
                    }
                    else
                        throw;
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose();
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

            return true;
        }

        public virtual string GeneratePassword()
        {
            //
            // 




            return Membership.GeneratePassword(
                      MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength,
                      MinRequiredNonAlphanumericCharacters);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUserCollection GetAllUsers(int pageIndex,
                                                        int pageSize,
                                                        out int totalRecords)
        {
            return FindUsersByName("*", pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            //
            // ADMembershipProvider does not support the notion of online users
            //
            throw new NotSupportedException(SR.GetString(SR.ADMembership_OnlineUsers_not_supported));
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUserCollection FindUsersByName(string usernameToMatch,
                                                        int pageIndex,
                                                        int pageSize,
                                                        out int totalRecords)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if (!EnableSearchMethods)
                throw new NotSupportedException(SR.GetString(SR.ADMembership_Provider_SearchMethods_not_supported));

            SecUtility.CheckParameter( ref usernameToMatch, true, true, true, maxUsernameLength, "usernameToMatch" );

            if ( pageIndex < 0 )
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            if ( pageSize < 1 )
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if ( upperBound > Int32.MaxValue )
                throw new ArgumentException(SR.GetString(SR.PageIndex_PageSize_bad), "pageIndex and pageSize");

            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;

                try
                {
                    totalRecords = 0;
                    return FindUsers(containerEntry, "(" + attributeMapUsername + "=" + GetEscapedFilterValue(usernameToMatch, false) + ")", attributeMapUsername, pageIndex, pageSize, out totalRecords);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!initialized)
                throw new InvalidOperationException(SR.GetString(SR.ADMembership_Provider_not_initialized));

            if (!EnableSearchMethods)
                throw new NotSupportedException(SR.GetString(SR.ADMembership_Provider_SearchMethods_not_supported));

            SecUtility.CheckParameter(ref emailToMatch, false, true, false, maxEmailLength, "emailToMatch");

            if ( pageIndex < 0 )
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            if ( pageSize < 1 )
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if ( upperBound > Int32.MaxValue )
                throw new ArgumentException(SR.GetString(SR.PageIndex_PageSize_bad), "pageIndex and pageSize");

            try
            {

                DirectoryEntryHolder connection = ActiveDirectoryConnectionHelper.GetDirectoryEntry(directoryInfo, directoryInfo.ContainerDN, true /* revertImpersonation */);
                DirectoryEntry containerEntry = connection.DirectoryEntry;

                try
                {
                    totalRecords = 0;
                    string filter = null;
                    if (emailToMatch != null)
                        filter = "(" + attributeMapUsername + "=*)(" + attributeMapEmail + "=" + GetEscapedFilterValue(emailToMatch, false) +")";
                    else
                        filter = "(" + attributeMapUsername + "=*)(!(" + attributeMapEmail + "=" +"*))";
                    return FindUsers(containerEntry, filter, attributeMapEmail, pageIndex, pageSize, out totalRecords);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch
            {
                //
                // this outer try-catch is to mitigate the exception filter attack (since we maybe suspending impersonation)
                //
                throw;
            }

        }

        private bool ValidateCredentials(string username, string password)
        {
            bool result = false;
            NetworkCredential credentialForValidation = (usernameIsSAMAccountName) ? new NetworkCredential(username, password, directoryInfo.DomainName)
                                                                                                                                    : DirectoryInformation.GetCredentialsWithDomain(new NetworkCredential(username, password));

            //
            // NOTE: we do not need to revert context here since this method is always
            //           called with explicit credentials
            //

            //
            // if this is concurrent bind (use the common connection)
            //

            if (directoryInfo.ConcurrentBindSupported)
            {
                try
                {
                    connection.Bind(credentialForValidation);
                    result = true;
                }
                catch (LdapException e)
                {
                    if (e.ErrorCode == 0x31)
                    {
                        //
                        // authentication failure, invalid user
                        //
                        result = false;
                    }
                    else
                    {
                        //
                        // some other failure
                        //
                        throw;
                    }
                }
            }
            else
            {
                //
                // create a new ldap connection
                //
                LdapConnection newConnection = directoryInfo.CreateNewLdapConnection(authTypeForValidation);

                try
                {
                    newConnection.Bind(credentialForValidation);
                    result = true;
                }
                catch (LdapException e2)
                {
                    if (e2.ErrorCode == 0x31)
                    {
                        //
                        // authentication failure, invalid user
                        //
                        result = false;
                    }
                    else
                    {
                        //
                        // some other failure
                        //
                        throw;
                    }
                }
                finally
                {
                    newConnection.Dispose();
                }
            }

            return result;
        }

        private DirectoryEntry FindUserEntryAndSAMAccountName(DirectoryEntry containerEntry, string filter, out string sAMAccountName)
        {
            return FindUserEntry(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, true /*retrieveSAMAccountName */, out sAMAccountName);
        }

        private DirectoryEntry FindUserEntry(DirectoryEntry containerEntry, string filter)
        {
            string dummyString;
            return FindUserEntry(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, false /*retrieveSAMAccountName */, out dummyString);
        }

        private DirectoryEntry FindUserEntry(DirectoryEntry containerEntry, string filter, System.DirectoryServices.SearchScope searchScope, bool retrieveSAMAccountName, out string sAMAccountName)
        {
            Debug.Assert(containerEntry != null);
            DirectorySearcher searcher = new DirectorySearcher(containerEntry);

            searcher.SearchScope = searchScope;
            searcher.Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")";

            if (directoryInfo.ClientSearchTimeout != -1)
                searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
            if (directoryInfo.ServerSearchTimeout != -1)
                searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

            if (retrieveSAMAccountName)
                searcher.PropertiesToLoad.Add("sAMAccountName");

            SearchResult res = searcher.FindOne();

            sAMAccountName = null;
            if (res != null)
            {
                if (retrieveSAMAccountName)
                    sAMAccountName = (string) PropertyManager.GetSearchResultPropertyValue(res, "sAMAccountName");
                return res.GetDirectoryEntry();
            }
            else
                return null;

        }

        private MembershipUser FindUserAndSAMAccountName(DirectoryEntry containerEntry, string filter, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes, out string sAMAccountName)
        {
            return FindUser(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, true /* retrieveSAMAccountName */, out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
        }

        private MembershipUser FindUser(DirectoryEntry containerEntry, string filter, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes)
        {
            string dummyString;
            return FindUser(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, false /* retrieveSAMAccountName */, out userEntry, out resetBadPasswordAnswerAttributes, out dummyString);
        }

        private MembershipUser FindUser(DirectoryEntry containerEntry, string filter, System.DirectoryServices.SearchScope searchScope,  bool retrieveSAMAccountName, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes, out string sAMAccountName)
        {
            Debug.Assert(containerEntry != null);
            MembershipUser user = null;
            DirectorySearcher searcher = new DirectorySearcher(containerEntry);

            searcher.SearchScope = searchScope;
            searcher.Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")";

            if (directoryInfo.ClientSearchTimeout != -1)
                searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
            if (directoryInfo.ServerSearchTimeout != -1)
                searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

            //
            // load all the attributes needed to create a MembershipUser object
            //
            searcher.PropertiesToLoad.Add(attributeMapUsername);
            searcher.PropertiesToLoad.Add("objectSid");
            searcher.PropertiesToLoad.Add(attributeMapEmail);
            searcher.PropertiesToLoad.Add("comment");
            searcher.PropertiesToLoad.Add("whenCreated");
            searcher.PropertiesToLoad.Add("pwdLastSet");
            searcher.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            searcher.PropertiesToLoad.Add("lockoutTime");

            if (retrieveSAMAccountName)
                searcher.PropertiesToLoad.Add("sAMAccountName");

            if (attributeMapPasswordQuestion != null)
                searcher.PropertiesToLoad.Add(attributeMapPasswordQuestion);

            if (directoryInfo.DirectoryType == DirectoryType.AD)
                searcher.PropertiesToLoad.Add("userAccountControl");
            else
                searcher.PropertiesToLoad.Add("msDS-UserAccountDisabled");

            if (EnablePasswordReset)
            {
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerCount);
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerTime);
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerLockoutTime);
            }


            SearchResult res = searcher.FindOne();
            resetBadPasswordAnswerAttributes = false;
            sAMAccountName = null;
            if (res != null)
            {
                user = GetMembershipUserFromSearchResult(res);
                userEntry = res.GetDirectoryEntry();

                if (retrieveSAMAccountName)
                    sAMAccountName = (string) PropertyManager.GetSearchResultPropertyValue(res, "sAMAccountName");

                if ((EnablePasswordReset) && res.Properties.Contains(attributeMapFailedPasswordAnswerCount))
                    resetBadPasswordAnswerAttributes = ((int) PropertyManager.GetSearchResultPropertyValue(res, attributeMapFailedPasswordAnswerCount) > 0);
            }
            else
            {
                userEntry = null;
            }

            return user;

        }

        private MembershipUserCollection FindUsers(DirectoryEntry containerEntry, string filter, string sortKey, int pageIndex, int pageSize, out int totalRecords)
        {
            Debug.Assert(containerEntry != null);
            MembershipUserCollection col = new MembershipUserCollection();
            int lastOffset = (pageIndex + 1) * pageSize;
            int startOffset = lastOffset -pageSize + 1;

            DirectorySearcher searcher = new DirectorySearcher(containerEntry);
            searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
            searcher.Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")";

            if (directoryInfo.ClientSearchTimeout != -1)
                searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
            if (directoryInfo.ServerSearchTimeout != -1)
                searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

            //
            // load all the attributes needed to create a MembershipUser object
            //
            searcher.PropertiesToLoad.Add(attributeMapUsername);
            searcher.PropertiesToLoad.Add("objectSid");
            searcher.PropertiesToLoad.Add(attributeMapEmail);
            searcher.PropertiesToLoad.Add("comment");
            searcher.PropertiesToLoad.Add("whenCreated");
            searcher.PropertiesToLoad.Add("pwdLastSet");
            searcher.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            searcher.PropertiesToLoad.Add("lockoutTime");

            if (attributeMapPasswordQuestion != null)
                searcher.PropertiesToLoad.Add(attributeMapPasswordQuestion);

            if (directoryInfo.DirectoryType == DirectoryType.AD)
                searcher.PropertiesToLoad.Add("userAccountControl");
            else
                searcher.PropertiesToLoad.Add("msDS-UserAccountDisabled");

            if (EnablePasswordReset)
            {
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerCount);
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerTime);
                searcher.PropertiesToLoad.Add(attributeMapFailedPasswordAnswerLockoutTime);
            }

            //
            // turn on paging
            //
            searcher.PageSize = 512;

            //
            // need to sort the users based on the attribute that is mapped to the username
            //
            searcher.Sort = new SortOption(sortKey, SortDirection.Ascending);

            SearchResultCollection resCol = searcher.FindAll();

            try
            {
                int count = 0;
                totalRecords = 0;

                foreach(SearchResult res in resCol)
                {
                    count++;

                    //
                    // add only the requested window of the result set
                    //
                    if (count >= startOffset && count <= lastOffset)
                    {
                        col.Add(GetMembershipUserFromSearchResult(res));
                    }
                }
                totalRecords = count;
            }
            finally
            {
                resCol.Dispose();
            }

            return col;

        }

        private void CheckPasswordAnswer(ref string passwordAnswer, bool checkForNull, int maxSize,string paramName)
        {
            if (passwordAnswer == null)
            {
                if (checkForNull)
                    throw new ArgumentNullException(paramName);
                return;
            }

            passwordAnswer = passwordAnswer.Trim();

            if (passwordAnswer.Length < 1)
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty, paramName), paramName);

            if (maxSize > 0 && passwordAnswer.Length > maxSize)
                throw new ArgumentException(SR.GetString(SR.ADMembership_Parameter_too_long, paramName), paramName);
        }

        private bool ValidatePassword(string password, int maxSize)
        {
            if (password == null)
                return false;

            if (password.Trim().Length < 1)
                return false;

            if (maxSize > 0 && password.Length > maxSize)
                return false;

            return true;
        }

        private void CheckPassword(string password, int maxSize, string paramName)
        {
            if (password == null)
                throw new ArgumentNullException(paramName);

            if (password.Trim().Length < 1)
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty, paramName), paramName);

            if (maxSize > 0 && password.Length > maxSize)
                throw new ArgumentException(SR.GetString(SR.Parameter_too_long, paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
        }

        private void CheckUserName(ref string username, int maxSize, string paramName)
        {
            SecUtility.CheckParameter( ref username, true, true, true, maxSize, paramName );

            //
            // if username is mapped to UPN, it should not contain '\'
            //
            if (usernameIsUPN && (username.IndexOf('\\') != -1))
                throw new ArgumentException(SR.GetString(SR.ADMembership_UPN_contains_backslash, paramName), paramName);
        }

        private int GetDomainControllerLevel(string serverName)
        {
            int dcLevel = 0;

            DirectoryEntry rootdse = new DirectoryEntry("LDAP://" + serverName + "/RootDSE", directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);
            string dcLevelString = (string) rootdse.Properties["domainControllerFunctionality"].Value;
            if (dcLevelString != null)
                dcLevel = Int32.Parse(dcLevelString, NumberFormatInfo.InvariantInfo);

            return dcLevel;
        }

        private void UpdateBadPasswordAnswerAttributes(DirectoryEntry userEntry)
        {

            //
            // get the password answer tracking related attributes to determine if we are still in an
            // active window for bad password answer attempts
            //
            int badPasswordAttemptCount = 0;
            bool inActiveWindow = false;


            DateTime currentTime = DateTime.UtcNow;
            if (userEntry.Properties.Contains(attributeMapFailedPasswordAnswerTime))
            {
                DateTime lastBadPasswordAnswerTime = GetDateTimeFromLargeInteger((NativeComInterfaces.IAdsLargeInteger) PropertyManager.GetPropertyValue(userEntry, attributeMapFailedPasswordAnswerTime));
                TimeSpan diffTime = currentTime.Subtract(lastBadPasswordAnswerTime);
                inActiveWindow = (diffTime <= new TimeSpan(0, PasswordAttemptWindow, 0));
            }

            // get the current bad password count
            int currentBadPasswordAttemptCount = 0;
            if (userEntry.Properties.Contains(attributeMapFailedPasswordAnswerCount))
                currentBadPasswordAttemptCount = (int) PropertyManager.GetPropertyValue(userEntry, attributeMapFailedPasswordAnswerCount);

            if (inActiveWindow && (currentBadPasswordAttemptCount > 0))
            {
                // within an active window for bad password answer attempts (increment count, if greater than 0)
                badPasswordAttemptCount =  currentBadPasswordAttemptCount + 1;
            }
            else
            {
                // start a new active window (set count = 1)
                badPasswordAttemptCount = 1;
            }

            // set the bad password attempt count and time
            userEntry.Properties[attributeMapFailedPasswordAnswerCount].Value = badPasswordAttemptCount;
            userEntry.Properties[attributeMapFailedPasswordAnswerTime].Value = GetLargeIntegerFromDateTime(currentTime);

            if (badPasswordAttemptCount >= maxInvalidPasswordAttempts)
            {
                //
                // user needs to be locked out due to too many bad password answer attempts
                //
                userEntry.Properties[attributeMapFailedPasswordAnswerLockoutTime].Value = GetLargeIntegerFromDateTime(currentTime);
            }

            userEntry.CommitChanges();
        }


        private void ResetBadPasswordAnswerAttributes(DirectoryEntry userEntry)
        {
            //
            // clear the password answer tracking related attributes (reset the window)
            //
            userEntry.Properties[attributeMapFailedPasswordAnswerCount].Value = 0;
            userEntry.Properties[attributeMapFailedPasswordAnswerTime].Value = 0;
            userEntry.Properties[attributeMapFailedPasswordAnswerLockoutTime].Value = 0;

            userEntry.CommitChanges();
        }

        private MembershipUser GetMembershipUserFromSearchResult(SearchResult res)
        {
            // username
            string username = (string) PropertyManager.GetSearchResultPropertyValue(res, attributeMapUsername);

            // providerUserKey is the SID of the user
            byte[] sidBinaryForm = (byte[]) PropertyManager.GetSearchResultPropertyValue(res, "objectSid");
            object providerUserKey = new SecurityIdentifier(sidBinaryForm, 0);

            // email (optional)
            string email = (res.Properties.Contains(attributeMapEmail)) ? (string) res.Properties[attributeMapEmail][0] : null;

            // passwordQuestion
            string passwordQuestion = null;
            if ((attributeMapPasswordQuestion != null) && (res.Properties.Contains(attributeMapPasswordQuestion)))
                passwordQuestion = (string) PropertyManager.GetSearchResultPropertyValue(res, attributeMapPasswordQuestion);

            //comment (optional)
            string comment = (res.Properties.Contains("comment")) ? (string) res.Properties["comment"][0] : null;

            //isApproved and isLockedOut
            bool isApproved;
            bool isLockedOut = false;
            if (directoryInfo.DirectoryType == DirectoryType.AD)
            {
                int val = (int) PropertyManager.GetSearchResultPropertyValue(res, "userAccountControl");
                if ((val & UF_ACCOUNT_DISABLED) == 0)
                    isApproved = true;
                else
                    isApproved = false;

                //
                // the "msDS-User-Account-Control-Computed" is the correct attribute to determine if  the
                // user is locked out or not. This attribute does not exist in W2K schema, so if we do not see this attribute in the result set
                // we will use the "lockoutTime". Note, if the user is not locked out and the schema is W2K3, this attribute will exist in the result
                // and have value 0 (since it's constructed), therefore absence of the attribute signifies that schema is W2K.
                //
                if (res.Properties.Contains("msDS-User-Account-Control-Computed"))
                {
                    int val2 = (int) PropertyManager.GetSearchResultPropertyValue(res, "msDS-User-Account-Control-Computed");
                    if ((val2 & UF_LOCKOUT) != 0)
                        isLockedOut = true;
                }
                else if (res.Properties.Contains("lockoutTime"))
                {
                    // NOTE: all date-time computation is done in UTC time though the values returned are in local time
                    DateTime lockoutTime = DateTime.FromFileTimeUtc((Int64) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"));
                    DateTime currentTime = DateTime.UtcNow;
                    TimeSpan diffTime = currentTime.Subtract(lockoutTime);
                    isLockedOut = (diffTime <= directoryInfo.ADLockoutDuration);
                }

            }
            else
            {
                isApproved = true; // if the msDS-UserAccountDisabled attribute if not present then the user is enabled

                if (res.Properties.Contains("msDS-UserAccountDisabled"))
                    isApproved = !((bool) PropertyManager.GetSearchResultPropertyValue(res, "msDS-UserAccountDisabled"));

                //
                // ADAM schema contains the "msDS-User-Account-Control-Computed" attribute, therefore it is used to determine the
                // lockout status of the user
                //
                int val2 = (int) PropertyManager.GetSearchResultPropertyValue(res, "msDS-User-Account-Control-Computed");
                if ((val2 & UF_LOCKOUT) != 0)
                    isLockedOut = true;
            }

            // lastLockoutDate (DateTime.FromFileTime cnoverts to Local time)
            DateTime lastLockoutDate = DefaultLastLockoutDate;
            if (isLockedOut)
                lastLockoutDate = DateTime.FromFileTime((Int64) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"));

            //
            // if password reset is enabled, we need to check if user is locked out due to bad password answer (and set/change the last lockout date)
            //
            if ((EnablePasswordReset) && (res.Properties.Contains(attributeMapFailedPasswordAnswerLockoutTime)))
            {
                // NOTE: all date-time computation is done in UTC time though the values returned are in local time
                DateTime badPasswordAnswerLockoutTime = DateTime.FromFileTimeUtc((Int64) PropertyManager.GetSearchResultPropertyValue(res, attributeMapFailedPasswordAnswerLockoutTime));
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan diffTime = currentTime.Subtract(badPasswordAnswerLockoutTime);
                bool isLockedOutByBadPasswordAnswer = (diffTime <= new TimeSpan(0, PasswordAnswerAttemptLockoutDuration, 0));

                if (isLockedOutByBadPasswordAnswer)
                {
                    if (isLockedOut)
                    {
                        //
                        // The account is locked both due to bad password and bad password answer, so we have two lockout dates
                        // Taking the later one.
                        //
                        if (DateTime.Compare(badPasswordAnswerLockoutTime, DateTime.FromFileTimeUtc((Int64) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"))) > 0)
                            lastLockoutDate = DateTime.FromFileTime((Int64) PropertyManager.GetSearchResultPropertyValue(res, attributeMapFailedPasswordAnswerLockoutTime));
                    }
                    else
                    {
                        //
                        // Account is locked out only due to bad password answer
                        //
                        isLockedOut = true;
                        lastLockoutDate = DateTime.FromFileTime((Int64) PropertyManager.GetSearchResultPropertyValue(res, attributeMapFailedPasswordAnswerLockoutTime));
                    }
                }
            }

            //createTimeStamp
            DateTime whenCreated =  ((DateTime) PropertyManager.GetSearchResultPropertyValue(res, "whenCreated")).ToLocalTime();

            //lastLogon (not supported)
            DateTime lastLogon = DateTime.MinValue;

            //lastActivity (not supported)
            DateTime lastActivity = DateTime.MinValue;

            //lastpwdchange (DateTime.FromFileTime cnoverts to Local time)
            DateTime lastPasswordChange = DateTime.FromFileTime((Int64) PropertyManager.GetSearchResultPropertyValue(res, "pwdLastSet"));

            return new ActiveDirectoryMembershipUser(Name, username, sidBinaryForm, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, whenCreated, lastLogon, lastActivity, lastPasswordChange, lastLockoutDate, true /* valuesAreUpdated */);
        }

        private string GetEscapedRdn(string rdn)
        {
            NativeComInterfaces.IAdsPathname pathCracker = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            return pathCracker.GetEscapedElement(0, rdn);
        }

        //
        // Generates an escaped name that may be used in an LDAP query. The characters
        // ( ) * \ must be escaped when used in an LDAP query per RFC 2254.
        //

        internal string GetEscapedFilterValue(string filterValue)
        {
            return GetEscapedFilterValue(filterValue, true /* escapeWildChar */);
        }

        internal string GetEscapedFilterValue(string filterValue, bool escapeWildChar)
        {
            int index = -1;
            char[] specialCharacters = new char[] { '(', ')', '*', '\\' };
            char[] specialCharactersWithoutWildChar = new char[] { '(', ')', '\\' };

            index = escapeWildChar ? filterValue.IndexOfAny(specialCharacters) : filterValue.IndexOfAny(specialCharactersWithoutWildChar);
            if (index != -1)
            {

                //
                // if it contains any of the special characters then we
                // need to escape those
                //

                StringBuilder str = new StringBuilder(2 * filterValue.Length);
                str.Append(filterValue.Substring(0, index));

                for (int i = index; i < filterValue.Length; i++) {

                switch (filterValue[i]) {
                    case ('(') : {
                        str.Append("\\28");
                        break;
                    }

                    case (')') : {
                        str.Append("\\29");
                        break;
                    }

                    case ('*') : {
                        if (escapeWildChar)
                            str.Append("\\2A");
                        else
                            str.Append("*");
                        break;
                    }

                    case ('\\') : {
                        // this may be the escaped version of '*', i.e. "\2A" or "\2a"
                        if ((escapeWildChar) || (!(((filterValue.Length - i) >= 3) && (filterValue[i + 1] == '2') && ((filterValue[i + 2] == 'A') || (filterValue[i + 2] == 'a')))))
                            str.Append("\\5C");
                        else
                            str.Append("\\");
                        break;
                    }

                    default : {
                        str.Append(filterValue[i]);
                        break;
                    }
                }
            }

            return str.ToString();
            }
            else
            {
                //
                // just return the original string
                //

                return filterValue;
            }
        }

        //
        // 



        private string GenerateAccountName()
        {
            char[] accountNameEncodingTable = new char[] {'0','1','2','3','4','5','6','7',
                                                                                '8','9','A','B','C','D','E','F',
                                                                                'G','H','I','J','K','L','M','N',
                                                                                'O','P','Q','R','S','T','U','V' };
            //
            // account name will be 20 characters long;
            //
            char[] accountName = new char[20];

            //
            // Generate a 64 bit random quantity
            //
            byte[] random = new byte[12];

            //RNGCryptoServiceProvider is an implementation of a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(random); // The array is now filled with cryptographically strong random bytes.

            // create a 32 bit random numbers from this
            uint random32a = 0;
            uint random32b = 0;
            uint random32c = 0;

            for (int i = 0; i < 4; i++)
            {
                random32a = random32a | unchecked((uint)(random[i] << (8 * i)));
            }
            for (int i = 0; i < 4; i++)
            {
                random32b = random32b | unchecked((uint)(random[4 + i] << (8 * i)));
            }
            for (int i = 0; i < 4; i++)
            {
                random32c = random32c | unchecked((uint)(random[8 + i] << (8 * i)));
            }

            //
            // The first character in the account name is a $ sign
            //

            accountName[0] = '$';

            //
            // The next 6 chars are the least 30 bits of random32a (base 32 encoded)
            //
            for (int i=1;i<=6;i++)
            {

                 //
                 // Lookup the char corresponding to the last 5 bits of
                 // random32a
                 //

                 accountName[i] = accountNameEncodingTable[(random32a & 0x1F)];

                 //
                 // Shift random32a right by 5 places
                 //

                 random32a = random32a >> 5;
            }

            //
            // The next char is a "-" to make the name more readable
            //

            accountName[7] = '-';

            //
            // The next 12 chars are formed by base 32 encoding the last 30
            // bits of random32b and random32c.
            //

            for (int i=8;i<=13;i++)
            {
                 //
                 // Lookup the char corresponding to the last 5 bits
                 //

                 accountName[i] = accountNameEncodingTable[(random32b & 0x1F)];

                 //
                 // Shift  right by 5 places
                 //
                 random32b = random32b >> 5;
            }

            for (int i=13;i<=19;i++)
            {
                 //
                 // Lookup the char corresponding to the last 5 bits
                 //

                 accountName[i] = accountNameEncodingTable[(random32c & 0x1F)];

                 //
                 // Shift  right by 5 places
                 //
                 random32c = random32c >> 5;
            }

            return new String(accountName);
        }

        private void SetPasswordPortIfApplicable(DirectoryEntry userEntry)
        {
            //
            // For ADAM, if the port is specified and we are using Ssl for connection protection,
            // we should set the password port.
            //
            if (directoryInfo.DirectoryType == DirectoryType.ADAM)
            {

                try
                {
                    if ((directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.Ssl) && (directoryInfo.PortSpecified))
                    {
                        userEntry.Options.PasswordPort = directoryInfo.Port;
                        userEntry.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingSsl;
                    }
                    else if ((directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal) || (directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.None))
                    {
                        userEntry.Options.PasswordPort = directoryInfo.Port;
                        userEntry.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingClear;
                    }
                }
                catch (COMException e)
                {

                    if (e.ErrorCode == unchecked((int) 0x80005008))
                    {
                        //
                        // If ADSI returns E_ADS_BAD_PARAMETER, it means we are running
                        // on a platform where ADSI does not support setting of the password port
                        // Since ADSI will set the password port to 636 and password method to Ssl, we can
                        // ignore this error only if that is what we are trying to set
                        //
                        if (!((directoryInfo.Port == DirectoryInformation.SSL_PORT) &&
                            (directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.Ssl)))
                            throw new ProviderException(SR.GetString(SR.ADMembership_unable_to_set_password_port));
                    }
                    else
                        throw;

                }
            }
        }

        private bool IsUpnUnique(string username)
        {

            //
            // NOTE: we do not need to revert context here since this method is always
            //           called after reverting any impersonated context
            //

            DirectoryEntry rootEntry = new DirectoryEntry("GC://" + directoryInfo.ForestName, directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);

            DirectorySearcher searcher = new DirectorySearcher(rootEntry);
            searcher.Filter = "(&(objectCategory=person)(objectClass=user)(userPrincipalName=" + GetEscapedFilterValue(username) + "))";
            searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

            if (directoryInfo.ClientSearchTimeout != -1)
                searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
            if (directoryInfo.ServerSearchTimeout != -1)
                searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

            bool result;
            try
            {
                result = (searcher.FindOne() == null);
            }
            finally
            {
                rootEntry.Dispose();
            }

            return result;

        }

        private bool IsEmailUnique(DirectoryEntry containerEntry, string username, string email, bool existing)
        {
            bool disposeContainerEntry = false;

            if (containerEntry == null)
            {
                //
                // NOTE: we do not need to revert context here since this method is always
                //           called after reverting any impersonated context
                //
                containerEntry = new DirectoryEntry(directoryInfo.GetADsPath(directoryInfo.ContainerDN), directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);
                disposeContainerEntry = true;
            }

            DirectorySearcher searcher = new DirectorySearcher(containerEntry);
            if (existing)
                searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + attributeMapUsername + "=*)(" + attributeMapEmail + "=" + GetEscapedFilterValue(email) + ")(!(" + GetEscapedRdn("cn=" + GetEscapedFilterValue(username)) + ")))";
            else
                searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + attributeMapUsername + "=*)(" + attributeMapEmail + "=" + GetEscapedFilterValue(email) + "))";
            searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

            if (directoryInfo.ClientSearchTimeout != -1)
                searcher.ClientTimeout = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ClientSearchTimeout, directoryInfo.TimeoutUnit);
            if (directoryInfo.ServerSearchTimeout != -1)
                searcher.ServerPageTimeLimit = DateTimeUtil.GetTimeoutFromTimeUnit(directoryInfo.ServerSearchTimeout, directoryInfo.TimeoutUnit);

            bool result;
            try
            {
                result = (searcher.FindOne() == null);
            }
            finally
            {
                if (disposeContainerEntry)
                {
                    containerEntry.Dispose();
                    containerEntry = null;
                }
            }

            return result;
        }

        private string GetConnectionString(string connectionStringName, bool appLevel)
        {
            if (String.IsNullOrEmpty(connectionStringName))
                return null;

            RuntimeConfig config = (appLevel) ? RuntimeConfig.GetAppConfig() : RuntimeConfig.GetConfig();
            ConnectionStringSettings connObj = config.ConnectionStrings.ConnectionStrings[connectionStringName];

            if (connObj == null)
            {
                //
                // No connection string by the specified name
                //
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, connectionStringName));
            }

            return connObj.ConnectionString;
        }

        private string GetAttributeMapping(NameValueCollection config, string valueName, out int maxLength)
        {
            string sValue = config[valueName];
            maxLength = -1;

            if (sValue == null)
                return null;

            sValue = sValue.Trim();

            if (sValue.Length == 0)
                throw new ProviderException(SR.GetString(SR.ADMembership_Schema_mappings_must_not_be_empty, valueName));

            return GetValidatedSchemaMapping(valueName, sValue, out maxLength);
        }

        private string GetValidatedSchemaMapping(string valueName, string attributeName, out int maxLength)
        {
            if (String.Compare(valueName, "attributeMapUsername", StringComparison.Ordinal) == 0)
            {
                if (directoryInfo.DirectoryType == DirectoryType.AD)
                {
                    //
                    // username can only be mapped to "sAMAccountName", "userPrincipalName"
                    //

                    if ((!StringUtil.EqualsIgnoreCase(attributeName, "sAMAccountName"))
                        && (!StringUtil.EqualsIgnoreCase(attributeName, "userPrincipalName")))
                        throw new ProviderException(SR.GetString(SR.ADMembership_Username_mapping_invalid));
                }
                else
                {
                    //
                    // for ADAM, username can only be mapped to "userPrincipalName"
                    //
                    if (!StringUtil.EqualsIgnoreCase(attributeName, "userPrincipalName"))
                        throw new ProviderException(SR.GetString(SR.ADMembership_Username_mapping_invalid_ADAM));

                }
            }
            else
            {
                //
                // ensure that we are not already using this attribute
                //
                if (attributesInUse.Contains(attributeName))
                    throw new ProviderException(SR.GetString(SR.ADMembership_mapping_not_unique, valueName, attributeName));

                //
                // ensure that the attribute exists on the user object
                //
                if (!userObjectAttributes.Contains(attributeName))
                    throw new ProviderException(SR.GetString(SR.ADMembership_MappedAttribute_does_not_exist_on_user, attributeName, valueName));
            }

            try
            {
                //
                // verify that this is an existing property and it's syntax is correct
                //
                DirectoryEntry propertyEntry = new DirectoryEntry(directoryInfo.GetADsPath("schema") + "/"  + attributeName, directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);

                //
                // to get the syntax we need to invoke the "syntax" property
                //
                string syntax = (string) propertyEntry.InvokeGet("Syntax");

                //
                // check that the syntax is as per the syntaxes table
                //
                if (!StringUtil.EqualsIgnoreCase(syntax, (string) syntaxes[valueName]))
                    throw new ProviderException(SR.GetString(SR.ADMembership_Wrong_syntax, valueName, (string) syntaxes[valueName]));

                //
                // if the type is "DirectoryString", then set the maxLength value if any
                //
                maxLength = -1;
                if (StringUtil.EqualsIgnoreCase(syntax, "DirectoryString"))
                {
                    try
                    {
                        maxLength = (int) propertyEntry.InvokeGet("MaxRange");
                    }
                    catch (TargetInvocationException e)
                    {
                        //
                        // if the inner exception is a comexception with error code 0x8007500d, then the max range is not set
                        // so we ignore that exception
                        //
                        if (!((e.InnerException is COMException) && (((COMException)e.InnerException).ErrorCode == unchecked((int) 0x8000500d))))
                            throw;
                    }
                }

                //
                // unless this is the username (which we already know is mapped
                // to a single valued attribute), the attribute should be single valued
                //
                if (String.Compare(valueName, "attributeMapUsername", StringComparison.Ordinal) != 0)
                {
                    bool isMultiValued = (bool) propertyEntry.InvokeGet("MultiValued");

                    if (isMultiValued)
                        throw new ProviderException(SR.GetString(SR.ADMembership_attribute_not_single_valued, valueName));
                }

            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x80005000))
                    throw new ProviderException(SR.GetString(SR.ADMembership_MappedAttribute_does_not_exist, attributeName, valueName), e);
                else
                    throw;
            }

            //
            // add the attribute name (lower cased) to the in use attributes list
            //
            return attributeName;
        }

        private int GetRangeUpperForSchemaAttribute(string attributeName)
        {
            int rangeUpper = -1;
            DirectoryEntry propertyEntry = new DirectoryEntry(directoryInfo.GetADsPath("schema") + "/"  + attributeName, directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);

            try
            {
                rangeUpper = (int) propertyEntry.InvokeGet("MaxRange");
            }
            catch (TargetInvocationException e)
            {
                //
                // if the inner exception is a comexception with error code 0x8007500d, then the max range is not set
                // so we ignore that exception
                //
                if (!((e.InnerException is COMException) && (((COMException)e.InnerException).ErrorCode == unchecked((int) 0x8000500d))))
                    throw;
            }

            return rangeUpper;
        }

        private Hashtable GetUserObjectAttributes()
        {
            DirectoryEntry de = new DirectoryEntry(directoryInfo.GetADsPath("schema") + "/user", directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes);
            object value = null;
            bool listEmpty = false;
            Hashtable attributes = new Hashtable(StringComparer.OrdinalIgnoreCase);

            try
            {
                value = de.InvokeGet("MandatoryProperties");
            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x8000500D))
                {
                    listEmpty = true;
                }
                else
                    throw;
            }

            if (!listEmpty)
            {
                if (value is ICollection)
                {
                    foreach (string attribute in (ICollection) value)
                    {
                        if (!attributes.Contains(attribute))
                            attributes.Add(attribute, null);
                     }
                }
                else
                {
                    // single value

                    if (!attributes.Contains(value))
                        attributes.Add(value, null);
                }
            }

            listEmpty = false;
            try
            {
                value = de.InvokeGet("OptionalProperties");
            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x8000500D))
                {
                    listEmpty = true;
                }
                else
                    throw;
            }

            if (!listEmpty)
            {
                if (value is ICollection)
                {
                    foreach (string attribute in (ICollection) value)
                    {
                        if (!attributes.Contains(attribute))
                            attributes.Add(attribute, null);
                     }
                }
                else
                {
                    // single value
                    if (!attributes.Contains(value))
                        attributes.Add(value, null);
                }
            }

            return attributes;

        }

        private DateTime GetDateTimeFromLargeInteger(NativeComInterfaces.IAdsLargeInteger largeIntValue)
        {
            //
            // Convert large integer to int64 value
            //
            Int64 int64Value = largeIntValue.HighPart * 0x100000000 + (uint) largeIntValue.LowPart;

            //
            // Return the DateTime in utc
            //
            return DateTime.FromFileTimeUtc(int64Value);

        }

        private NativeComInterfaces.IAdsLargeInteger GetLargeIntegerFromDateTime(DateTime dateTimeValue)
        {
            //
            // Convert DateTime value to utc file time
            //
            Int64 int64Value = dateTimeValue.ToFileTimeUtc();

            //
            // convert to large integer
            //
            NativeComInterfaces.IAdsLargeInteger largeIntValue = (NativeComInterfaces.IAdsLargeInteger) new NativeComInterfaces.LargeInteger();
            largeIntValue.HighPart = (int) (int64Value >> 32);
            largeIntValue.LowPart = (int) (int64Value & 0xFFFFFFFF);

            return largeIntValue;
        }

        private string Encrypt(string clearTextString)
        {
            // we should never be getting null input here
            Debug.Assert(clearTextString != null);

            byte[] bIn = Encoding.Unicode.GetBytes(clearTextString);

            byte[] bSalt = new byte[AD_SALT_SIZE_IN_BYTES];
            (new RNGCryptoServiceProvider()).GetBytes(bSalt);

            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);

            return Convert.ToBase64String(EncryptPassword(bAll, _LegacyPasswordCompatibilityMode));
        }

        private string Decrypt(string encryptedString)
        {
            // we should never be getting null input here
            Debug.Assert(encryptedString != null);

            byte[] bEncryptedData = Convert.FromBase64String(encryptedString);

            byte[] bAll = DecryptPassword(bEncryptedData);

            return Encoding.Unicode.GetString(bAll, AD_SALT_SIZE_IN_BYTES, bAll.Length - AD_SALT_SIZE_IN_BYTES);
        }

    }

    internal sealed class DirectoryInformation
    {
        private string serverName = null;
        private string containerDN = null;
        private string creationContainerDN = null;
        private string adspath = null;
        private int port = 389;
        private bool portSpecified = false;
        private DirectoryType directoryType = DirectoryType.Unknown;
        private ActiveDirectoryConnectionProtection connectionProtection = ActiveDirectoryConnectionProtection.None;
        private bool concurrentBindSupported = false;
        private int clientSearchTimeout = -1;
        private int serverSearchTimeout = -1;
        private TimeUnit timeUnit = TimeUnit.Unknown;
        private DirectoryEntry rootdse = null;
        private NetworkCredential credentials = null;
        private AuthenticationTypes authenticationType = AuthenticationTypes.None;
        private AuthType ldapAuthType = AuthType.Basic;
        private string adamPartitionDN = null;
        private TimeSpan adLockoutDuration;
        private string forestName = null;
        private string domainName = null;
        private bool isServer = false;

        private const string LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID ="1.2.840.113556.1.4.1851";
        private const string LDAP_CAP_ACTIVE_DIRECTORY_OID ="1.2.840.113556.1.4.800";
        private const string LDAP_SERVER_FAST_BIND_OID = "1.2.840.113556.1.4.1781";
        internal const int SSL_PORT = 636;
        private const int GC_PORT = 3268;
        private const int GC_SSL_PORT = 3269;
        private const string GUID_USERS_CONTAINER_W = "a9d1ca15768811d1aded00c04fd8d5cd";

        //
        // authentication types for S.DS and S.DS.Protocols (rows are indexed by connection protection
        // columns are indexed by type of credentials (see CredentialType enum)
        //
        AuthenticationTypes[,] authTypes = new AuthenticationTypes[,]
                    {{AuthenticationTypes.None, AuthenticationTypes.None},
                      {AuthenticationTypes.Secure | AuthenticationTypes.SecureSocketsLayer , AuthenticationTypes.SecureSocketsLayer },
                      {AuthenticationTypes.Secure | AuthenticationTypes.Signing | AuthenticationTypes.Sealing, AuthenticationTypes.Secure | AuthenticationTypes.Signing | AuthenticationTypes.Sealing}};

        AuthType[,] ldapAuthTypes = new AuthType[,]
                     {{AuthType.Negotiate, AuthType.Basic},
                      {AuthType.Negotiate, AuthType.Basic},
                      {AuthType.Negotiate, AuthType.Negotiate}};

        internal DirectoryInformation(string adspath,
                                                            NetworkCredential credentials,
                                                            string connProtection,
                                                            int clientSearchTimeout,
                                                            int serverSearchTimeout,
                                                            bool enablePasswordReset,
                                                            TimeUnit timeUnit)
        {

           //
           // all parameters have already been validated at this point
           //

            this.adspath = adspath;
            this.credentials = credentials;
            this.clientSearchTimeout = clientSearchTimeout;
            this.serverSearchTimeout = serverSearchTimeout;
            this.timeUnit = timeUnit;

            Debug.Assert(adspath != null);
            Debug.Assert(adspath.Length > 0);

            //
            // Provider must be LDAP
            //
            if (!(adspath.StartsWith("LDAP", StringComparison.Ordinal)))
                throw new ProviderException(SR.GetString(SR.ADMembership_OnlyLdap_supported));

            //
            // Parse out the server/domain information
            //
            NativeComInterfaces.IAdsPathname pathCracker = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();

            try {
                pathCracker.Set(adspath, NativeComInterfaces.ADS_SETTYPE_FULL);
            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x80005000))
                    throw new ProviderException(SR.GetString(SR.ADMembership_invalid_path));
                else
                    throw;
            }

            // Get the server and container names
            try
            {
                serverName = pathCracker.Retrieve(NativeComInterfaces.ADS_FORMAT_SERVER);
            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x80005000))
                    throw new ProviderException(SR.GetString(SR.ADMembership_ServerlessADsPath_not_supported));
                else
                    throw;
            }
            Debug.Assert(serverName != null);

            creationContainerDN = containerDN = pathCracker.Retrieve(NativeComInterfaces.ADS_FORMAT_X500_DN);

            //
            // Parse out the port number if specified
            //
            int index = serverName.IndexOf(':');
            if (index != -1)
            {
                string tempStr = serverName;

                serverName = tempStr.Substring(0, index);

                Debug.Assert(tempStr.Length > index);
                port = Int32.Parse(tempStr.Substring(index + 1), NumberFormatInfo.InvariantInfo);
                portSpecified = true;
            }

            if (String.Compare(connProtection, "Secure", StringComparison.Ordinal) == 0)
            {
                //
                // The logic is as follows:
                // 1. Try Ssl first and check if concurrent binds are possible for validating users
                // 2. If Ssl is not supported, try signing and sealing
                // 3. If both the above are not supported, then we will fail
                //

                bool trySignAndSeal = false;
                bool trySslWithSecureAuth = false;

                // first try with simple bind
                if (!IsDefaultCredential())
                {

                    authenticationType = GetAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.NonWindows);
                    ldapAuthType = GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.NonWindows);

                    try
                    {
                        rootdse = new DirectoryEntry(GetADsPath("rootdse"), GetUsername(), GetPassword(), authenticationType);
                        // this will force a bind
                        rootdse.RefreshCache();
                        this.connectionProtection = ActiveDirectoryConnectionProtection.Ssl;
                        if (!portSpecified)
                        {
                            port = SSL_PORT;
                            portSpecified = true;
                        }
                    }
                    catch (COMException ce)
                    {

                        if (ce.ErrorCode == unchecked((int) 0x8007052e))
                        {
                            //
                            // this could be an ADAM target with windows user (in that case simple bind will not work)
                            //
                            trySslWithSecureAuth = true;
                        }
                        else if (ce.ErrorCode == unchecked((int) 0x8007203a))
                        {
                            // server is not operational error, do nothing, we need to fall back to SignAndSeal
                            trySignAndSeal = true;
                        }
                        else
                            throw;
                     }
                }
                else
                {
                    // default credentials, so we have to do secure bind
                    trySslWithSecureAuth = true;
                }

                if (trySslWithSecureAuth)
                {

                    authenticationType = GetAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.Windows);
                    ldapAuthType = GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.Windows);

                    try
                    {
                        rootdse = new DirectoryEntry(GetADsPath("rootdse"), GetUsername(), GetPassword(), authenticationType);
                        // this will force a bind
                        rootdse.RefreshCache();
                        this.connectionProtection = ActiveDirectoryConnectionProtection.Ssl;
                        if (!portSpecified)
                        {
                            port = SSL_PORT;
                            portSpecified = true;
                        }

                    }
                    catch (COMException ce)
                    {
                        if (ce.ErrorCode == unchecked((int) 0x8007203a))
                        {
                            // server is not operational error, do nothing, we need to fall back to SignAndSeal
                            trySignAndSeal = true;
                        }
                        else
                            throw;
                     }

                }

                if (trySignAndSeal)
                {
                    authenticationType = GetAuthenticationTypes(ActiveDirectoryConnectionProtection.SignAndSeal, CredentialsType.Windows);
                    ldapAuthType = GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.SignAndSeal, CredentialsType.Windows);

                    try
                    {
                        rootdse = new DirectoryEntry(GetADsPath("rootdse"), GetUsername(), GetPassword(), authenticationType);
                        rootdse.RefreshCache();
                        this.connectionProtection = ActiveDirectoryConnectionProtection.SignAndSeal;
                    }
                    catch (COMException e)
                    {
                        throw new ProviderException(SR.GetString(SR.ADMembership_Secure_connection_not_established, e.Message), e);
                    }
                }
            }
            else
            {
                //
                // No connection protection
                //

                //
                // we will do a simple bind but we must ensure that the credentials are explicitly specified
                // since in the case of default credentials we cannot honor it (default credentials become anonymous in the case of
                // simple bind)
                //
                if (IsDefaultCredential())
                    throw new NotSupportedException(SR.GetString(SR.ADMembership_Default_Creds_not_supported));

                // simple bind
                authenticationType = GetAuthenticationTypes(connectionProtection, CredentialsType.NonWindows);
                ldapAuthType = GetLdapAuthenticationTypes(connectionProtection, CredentialsType.NonWindows);

                rootdse = new DirectoryEntry(GetADsPath("rootdse"), GetUsername(), GetPassword(), authenticationType);

            }

            //
            // Determine whether this is AD or ADAM by binding to the rootdse and
            // checking the supported capabilities
            //
            if (rootdse == null)
                rootdse = new DirectoryEntry(GetADsPath("RootDSE"), GetUsername(), GetPassword(), authenticationType);
            directoryType = GetDirectoryType();

            //
            // if the directory type is ADAM and the conntectionProtection was selected
            // as sign and seal, then we should throw an ProviderException. This is becuase validate user will always fail for ADAM
            // because ADAM does not support secure authentication for ADAM users.
            //
            if ((directoryType == DirectoryType.ADAM) && (this.connectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal))
                throw new ProviderException(SR.GetString(SR.ADMembership_Ssl_connection_not_established));

            //
            // for AD, we need to block the GC ports
            //
            if ((directoryType == DirectoryType.AD) && ((port == GC_PORT) || (port == GC_SSL_PORT)))
                throw new ProviderException(SR.GetString(SR.ADMembership_GCPortsNotSupported));

            //
            // if container dn is null, we need to get the default naming context
            // (containerDN cannot be null for ADAM)
            //
            if (String.IsNullOrEmpty(containerDN))
            {
                if (directoryType == DirectoryType.AD)
                {
                    containerDN = (string)rootdse.Properties["defaultNamingContext"].Value;
                    if (containerDN == null)
                        throw new ProviderException(SR.GetString(SR.ADMembership_DefContainer_not_specified));

                    //
                    // we will create users in the default users container, check that it exists
                    //
                    string wkUsersContainerPath = GetADsPath("<WKGUID=" + GUID_USERS_CONTAINER_W + "," + containerDN + ">");
                    DirectoryEntry containerEntry = new DirectoryEntry(wkUsersContainerPath, GetUsername(), GetPassword(), authenticationType);

                    try
                    {
                        creationContainerDN = (string) PropertyManager.GetPropertyValue(containerEntry, "distinguishedName");
                    }
                    catch (COMException ce)
                    {
                        if (ce.ErrorCode == unchecked((int) 0x80072030))
                            throw new ProviderException(SR.GetString(SR.ADMembership_DefContainer_does_not_exist));
                        else
                            throw;
                    }
                }
                else
                {
                    // container must be specified for ADAM
                    throw new ProviderException(SR.GetString(SR.ADMembership_Container_must_be_specified));
                }
            }
            else
            {
                //
                // Normalize the container name (incase it was specified as GUID or WKGUID)
                //
                DirectoryEntry containerEntry = new DirectoryEntry(GetADsPath(containerDN), GetUsername(), GetPassword(), authenticationType);

                try
                {
                    creationContainerDN = containerDN = (string) PropertyManager.GetPropertyValue(containerEntry, "distinguishedName");
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == unchecked((int) 0x80072030))
                        throw new ProviderException(SR.GetString(SR.ADMembership_Container_does_not_exist));
                    else
                        throw;
                }
            }

            //
            // Check if the specified path(container) exists on the specified server/domain
            // (NOTE: We need to do this using S.DS.Protocols rather than S.DS because we need to
            //            bypass the referral chasing which is automatic in S.DS)
            //

            LdapConnection tempConnection = new LdapConnection(new LdapDirectoryIdentifier(serverName + ":" + port), GetCredentialsWithDomain(credentials), ldapAuthType);
            tempConnection.SessionOptions.ProtocolVersion = 3;

            try
            {
                tempConnection.SessionOptions.ReferralChasing = System.DirectoryServices.Protocols.ReferralChasingOptions.None;
                SetSessionOptionsForSecureConnection(tempConnection, false /*useConcurrentBind */);
                tempConnection.Bind();


                SearchRequest request = new SearchRequest();
                request.DistinguishedName = containerDN;
                request.Filter = "(objectClass=*)";
                request.Scope = System.DirectoryServices.Protocols.SearchScope.Base;
                request.Attributes.Add("distinguishedName");
                request.Attributes.Add("objectClass");

                if (ServerSearchTimeout != -1)
                    request.TimeLimit = new TimeSpan(0, ServerSearchTimeout, 0);

                SearchResponse response;
                try
                {
                    response = (SearchResponse) tempConnection.SendRequest(request);
                    if (response.ResultCode == ResultCode.Referral || response.ResultCode ==  ResultCode.NoSuchObject)
                        throw new ProviderException(SR.GetString(SR.ADMembership_Container_does_not_exist));
                    else if (response.ResultCode != ResultCode.Success)
                        throw new ProviderException(response.ErrorMessage);
                }
                catch (DirectoryOperationException oe)
                {
                    SearchResponse errorResponse = (SearchResponse) oe.Response;
                    if (errorResponse.ResultCode == ResultCode.NoSuchObject)
                        throw new ProviderException(SR.GetString(SR.ADMembership_Container_does_not_exist));
                    else throw;
                }

                //
                // check that the container is of an object type that can be a superior of a user object
                //
                DirectoryAttribute objectClass = response.Entries[0].Attributes["objectClass"];
                if (!ContainerIsSuperiorOfUser(objectClass))
                    throw new ProviderException(SR.GetString(SR.ADMembership_Container_not_superior));

                //
                // Determine whether concurrent bind is supported
                //
                if ((connectionProtection == ActiveDirectoryConnectionProtection.None) || (connectionProtection == ActiveDirectoryConnectionProtection.Ssl))
                {
                    this.concurrentBindSupported = IsConcurrentBindSupported(tempConnection);
                }

            }
            finally
            {
                tempConnection.Dispose();
            }

            //
            // if this is ADAM, get the partition DN
            //
            if (directoryType == DirectoryType.ADAM)
            {
                adamPartitionDN = GetADAMPartitionFromContainer();
            }
            else
            {
                if (enablePasswordReset)
                {
                    // for AD, get the lockout duration for user account auto unlock
                    DirectoryEntry de = new DirectoryEntry(GetADsPath((string) PropertyManager.GetPropertyValue(rootdse, "defaultNamingContext")), GetUsername(), GetPassword(), AuthenticationTypes);
                    NativeComInterfaces.IAdsLargeInteger largeIntValue = (NativeComInterfaces.IAdsLargeInteger) PropertyManager.GetPropertyValue(de, "lockoutDuration");
                    Int64 int64Value = largeIntValue.HighPart * 0x100000000 + (uint) largeIntValue.LowPart;

                    // int64Value is the negative of the number of 100 nanoseconds interval that makes up the lockout duration
                    adLockoutDuration = new TimeSpan(-int64Value);
                }
            }
        }

        internal bool ConcurrentBindSupported
        {
            get { return concurrentBindSupported; }
        }

        internal string ContainerDN
        {
            get { return containerDN; }
        }

        internal string CreationContainerDN
        {
            get { return creationContainerDN; }
        }

        internal int Port
        {
            get { return port; }
        }

        internal bool PortSpecified
        {
            get { return portSpecified; }
        }

        internal DirectoryType DirectoryType
        {
            get { return directoryType; }
        }

        internal ActiveDirectoryConnectionProtection ConnectionProtection
        {
            get { return connectionProtection; }
        }

        internal AuthenticationTypes AuthenticationTypes
        {
            get { return authenticationType; }
        }

        internal int ClientSearchTimeout
        {
            get { return clientSearchTimeout; }
        }

        internal int ServerSearchTimeout
        {
            get { return serverSearchTimeout; }
        }

        internal TimeUnit TimeoutUnit
        {
            get { return timeUnit; }
        }

        internal string ADAMPartitionDN
        {
            get { return adamPartitionDN; }
        }

        internal TimeSpan ADLockoutDuration
        {
            get { return adLockoutDuration; }
        }

        internal string ForestName
        {
            get { return forestName; }
        }

        internal string DomainName
        {
            get { return domainName; }
        }

        internal void InitializeDomainAndForestName()
        {
            if (!isServer)
            {
                DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, serverName, GetUsername(), GetPassword());
                try
                {
                    Domain domain = Domain.GetDomain(context);
                    domainName = GetNetbiosDomainNameIfAvailable(domain.Name);
                    forestName = domain.Forest.Name;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    // the serverName may be the name of the server rather than domain
                    isServer = true;
                }
            }

            if (isServer)
            {
                DirectoryContext context = new DirectoryContext(DirectoryContextType.DirectoryServer, serverName, GetUsername(), GetPassword());
                try
                {
                    Domain domain = Domain.GetDomain(context);
                    domainName = GetNetbiosDomainNameIfAvailable(domain.Name);
                    forestName = domain.Forest.Name;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    // we were unable to contact the domain or server
                    throw new ProviderException(SR.GetString(SR.ADMembership_unable_to_contact_domain));
                }
            }
        }

        internal void SelectServer()
        {
            //
            // if the name specified in the target is a domain name, then we should
            // perform all operations on the PDC. If the name is not a domain name
            // then it would be the name of a server. In that case we perform all
            // operations on that server
            //
            serverName = GetPdcIfDomain(serverName);
            isServer = true;
        }

        //
        // Creates a new ldap connection with the specified auth types
        // (the session options are set based on the connection protection that was
        // determined during the initialize method)
        //
        internal LdapConnection CreateNewLdapConnection(AuthType authType)
        {
            LdapConnection newConnection = null;

            newConnection = new LdapConnection(new LdapDirectoryIdentifier(serverName + ":" + port));
            newConnection.AuthType = authType;
            newConnection.SessionOptions.ProtocolVersion = 3;
            SetSessionOptionsForSecureConnection(newConnection, true /* useConcurrentBind */);

            return newConnection;
        }

        //
        // this method returns the ADsPath for the given DN
        //
        internal string GetADsPath(string dn)
        {
            string path = null;

            //
            // provider and server information
            //
            Debug.Assert(serverName != null);
            path = "LDAP://" + serverName;

            //
            // port info if specified
            //
            if (portSpecified)
                path = path + ":" + port;

            //
            // DN of the object
            //
            Debug.Assert(dn != null);
            NativeComInterfaces.IAdsPathname pathCracker = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            pathCracker.Set(dn, NativeComInterfaces.ADS_SETTYPE_DN);
            pathCracker.EscapedMode = NativeComInterfaces.ADS_ESCAPEDMODE_ON;
            path = path + "/" + pathCracker.Retrieve(NativeComInterfaces.ADS_FORMAT_X500_DN);

            return path;

        }

        internal void SetSessionOptionsForSecureConnection(LdapConnection connection, bool useConcurrentBind)
        {

            if (connectionProtection == ActiveDirectoryConnectionProtection.Ssl) {
                connection.SessionOptions.SecureSocketLayer = true;
            }
            else if (connectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal)
            {
                connection.SessionOptions.Signing = true;
                connection.SessionOptions.Sealing = true;
            }

            if (useConcurrentBind && this.concurrentBindSupported)
            {
                try
                {
                    connection.SessionOptions.FastConcurrentBind();
                }
                catch (PlatformNotSupportedException)
                {
                    //
                    // concurrent bind is not supported by the client, (continue without it and don't try to set it next time)
                    //
                    this.concurrentBindSupported = false;
                }
                catch (DirectoryOperationException)
                {
                    // Dev10 Bug# 623663:
                    // concurrent bind is not supported when a client certificate is specified, (continue without it and don't try to set it next time)

                    this.concurrentBindSupported = false;
                }
            }
        }

        [EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal string GetUsername()
        {
            if (credentials == null)
                return null;

            if (credentials.UserName == null)
                return null;

            if (credentials.UserName.Length == 0 && (credentials.Password == null || credentials.Password.Length == 0))
                return null;

            return this.credentials.UserName;
        }

        [EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal string GetPassword()
        {
            if (credentials == null)
                return null;

            if (credentials.Password == null)
                return null;

            if (credentials.Password.Length == 0 && (credentials.UserName == null || credentials.UserName.Length == 0))
                return null;

            return this.credentials.Password;
        }

        internal AuthenticationTypes GetAuthenticationTypes(ActiveDirectoryConnectionProtection connectionProtection, CredentialsType type)
        {
            return authTypes[(int) connectionProtection, (int) type];
        }

        internal AuthType GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection connectionProtection, CredentialsType type)
        {
            return ldapAuthTypes[(int) connectionProtection, (int) type];
        }

        [EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal bool IsDefaultCredential()
        {
            if ((credentials.UserName == null || credentials.UserName.Length == 0) && (credentials.Password == null || credentials.Password.Length == 0))
                return true;

            return false;
        }

        [EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static NetworkCredential GetCredentialsWithDomain(NetworkCredential credentials)
        {
            NetworkCredential credentialsWithDomain;

            if (credentials == null)
                credentialsWithDomain = new NetworkCredential(null, "");
            else
            {
                string tempUsername = credentials.UserName;
                string username = null;
                string password = null;
                string domainName = null;

                if (!String.IsNullOrEmpty(tempUsername))
                {
                    int index = tempUsername.IndexOf('\\');
                    if (index != -1)
                    {
                        domainName = tempUsername.Substring(0, index);
                        username = tempUsername.Substring(index + 1);
                    }
                    else
                        username = tempUsername;

                    password = credentials.Password;
                }
                credentialsWithDomain = new NetworkCredential(username, password, domainName);
            }

            return credentialsWithDomain;
        }

        private bool IsConcurrentBindSupported(LdapConnection ldapConnection)
        {
            bool result = false;

            Debug.Assert(ldapConnection != null);

            //
            // supportedExtension is a constructed attribute so we need to search and load that attribute explicitly
            //
            SearchRequest request = new SearchRequest();
            request.Scope = System.DirectoryServices.Protocols.SearchScope.Base;
            request.Attributes.Add("supportedExtension");

            if (ServerSearchTimeout != -1)
                request.TimeLimit = new TimeSpan(0, ServerSearchTimeout, 0);

            SearchResponse response = (SearchResponse) ldapConnection.SendRequest(request);
            if (response.ResultCode != ResultCode.Success)
                throw new ProviderException(response.ErrorMessage);

            foreach (string supportedExtension in response.Entries[0].Attributes["supportedExtension"].GetValues(typeof(string)))
            {
                if (StringUtil.EqualsIgnoreCase(supportedExtension, LDAP_SERVER_FAST_BIND_OID))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        //
        // This function goes through each of the naming contexts on the server
        // and determines which one is the longest postfix of the container DN.
        // That will give the DN of partition that the container lives in.
        //
        //
        private string GetADAMPartitionFromContainer()
        {
            string partitionName = null;
            int startsAt = Int32.MaxValue;

            foreach(string namingContext in rootdse.Properties["namingContexts"])
            {
                bool endsWith = containerDN.EndsWith(namingContext, StringComparison.Ordinal);
                int lastIndexOf = containerDN.LastIndexOf(namingContext, StringComparison.Ordinal);

                if (endsWith && (lastIndexOf != -1) && (lastIndexOf < startsAt))
                {
                    partitionName = namingContext;
                    startsAt = lastIndexOf;
                }
            }

            if (partitionName == null)
                throw new ProviderException(SR.GetString(SR.ADMembership_No_ADAM_Partition));

            return partitionName;
        }

        //
        // This function goes through each of the object class values for the container to determine
        // whether the object class is one of the possible superiors of the user object
        //
        private bool ContainerIsSuperiorOfUser(DirectoryAttribute objectClass)
        {
            ArrayList possibleSuperiorsList = new ArrayList();

            //
            // first get a list of all the classes from which the user class is derived
            //
            DirectoryEntry de = new DirectoryEntry(GetADsPath("schema") + "/user", GetUsername(), GetPassword(), AuthenticationTypes);
            ArrayList classesList = new ArrayList();
            bool derivedFromlistEmpty = false;
            object value = null;

            try
            {
                value = de.InvokeGet("DerivedFrom");
            }
            catch (COMException e)
            {
                if (e.ErrorCode == unchecked((int) 0x8000500D))
                {
                    derivedFromlistEmpty = true;
                }
                else
                    throw;
            }

            if (!derivedFromlistEmpty)
            {
                if (value is ICollection)
                {
                    classesList.AddRange((ICollection) value);
                }
                else
                {
                    // single value
                    classesList.Add((string) value);
                }
            }

            //
            // we will use this list to create a filter of all the classSchema objects that we need to determine the recursive list
            // of "possibleSecuperiors". We need to add the user class also.
            //
            classesList.Add("user");

            //
            // Now search under the schema naming context for all these classes and get the "possSuperiors" and "systemPossSuperiors" attributes
            //
            DirectoryEntry schemaNC = new DirectoryEntry(GetADsPath((string) rootdse.Properties["schemaNamingContext"].Value), GetUsername(), GetPassword(), AuthenticationTypes);
            DirectorySearcher searcher = new DirectorySearcher(schemaNC);

            searcher.Filter = "(&(objectClass=classSchema)(|";
            foreach(string supClass in classesList)
                searcher.Filter += "(ldapDisplayName=" + supClass + ")";
            searcher.Filter += "))";

            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;
            searcher.PropertiesToLoad.Add("possSuperiors");
            searcher.PropertiesToLoad.Add("systemPossSuperiors");

            SearchResultCollection resCol = searcher.FindAll();

            try
            {
                foreach (SearchResult res in resCol)
                {
                    possibleSuperiorsList.AddRange(res.Properties["possSuperiors"]);
                    possibleSuperiorsList.AddRange(res.Properties["systemPossSuperiors"]);
                }
            }
            finally
            {
                resCol.Dispose();
            }

            //
            // Now we have the list of all the possible superiors, check if the objectClass that was specified as a parameter
            // to this function is one of these values, if so, return true else false
            //
            foreach (string objectClassValue in objectClass.GetValues(typeof(string)))
            {
                if (possibleSuperiorsList.Contains(objectClassValue))
                    return true;
            }

            return false;
        }

        //
        // This method determines whether the server we are talking to
        // is an AD domain controller or an ADAM instance
        //
        private DirectoryType GetDirectoryType()
        {
            DirectoryType directoryType = DirectoryType.Unknown;

            foreach (string supportedCapability in rootdse.Properties["supportedCapabilities"])
            {
                if (StringUtil.EqualsIgnoreCase(supportedCapability, LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID))
                {
                    directoryType = DirectoryType.ADAM;
                    break;
                }
                else if (StringUtil.EqualsIgnoreCase(supportedCapability, LDAP_CAP_ACTIVE_DIRECTORY_OID))
                {
                    directoryType = DirectoryType.AD;
                    break;
                }
            }

            if (directoryType == DirectoryType.Unknown)
                throw new ProviderException(SR.GetString(SR.ADMembership_Valid_Targets));

            return directoryType;
        }

        //
        // This method returns the dns name of the primary domain controller if the specified name is a domain,
        // else is just returns the name as is
        //
        internal string GetPdcIfDomain(string name)
        {
            IntPtr pDomainControllerInfo = IntPtr.Zero;

            /* DS_DIRECTORY_SERVICE_REQUIRED   0x00000010
                 DS_RETURN_DNS_NAME              0x40000000
                 DS_PDC_REQUIRED                 0x00000080 */
            uint flags = 0x00000010 | 0x40000000 | 0x00000080;
            string pdc = null;

            int ERROR_NO_SUCH_DOMAIN = 1355;

            int result = NativeMethods.DsGetDcName(null, name, IntPtr.Zero, null,  flags, out pDomainControllerInfo);

            try {
                if (result == 0)
                {
                    // success case
                    DomainControllerInfo domainControllerInfo = new DomainControllerInfo();
                    Marshal.PtrToStructure(pDomainControllerInfo, domainControllerInfo);

                    Debug.Assert(domainControllerInfo != null);
                    Debug.Assert(domainControllerInfo.DomainControllerName != null);
                    Debug.Assert(domainControllerInfo.DomainControllerName.Length > 2);

                    // domain controller name is in the format "\\server", so we need to strip the back slashes
                    pdc = domainControllerInfo.DomainControllerName.Substring(2);
                }
                else if (result == ERROR_NO_SUCH_DOMAIN)
                    pdc = name;
                else
                    throw new ProviderException(GetErrorMessage(result));
            }
            finally
            {
                // free the buffer
                if (pDomainControllerInfo != IntPtr.Zero) {
                    NativeMethods.NetApiBufferFree(pDomainControllerInfo);
                }
            }

            return pdc;
        }

        internal string GetNetbiosDomainNameIfAvailable(string dnsDomainName)
        {
            string result = null;

            //
            // Get the netbios name from the "nETBIOSName" attribute on the crossRef object for this domain
            //
            DirectoryEntry partitionsEntry = new DirectoryEntry(GetADsPath("CN=Partitions," + (string) PropertyManager.GetPropertyValue(rootdse, "configurationNamingContext")), GetUsername(), GetPassword());
            DirectorySearcher searcher = new DirectorySearcher(partitionsEntry);
            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;

            StringBuilder str = new StringBuilder(15);
            str.Append("(&(objectCategory=crossRef)(dnsRoot=");
            str.Append(dnsDomainName);
            str.Append(")(systemFlags:1.2.840.113556.1.4.804:=1)(systemFlags:1.2.840.113556.1.4.804:=2))");

            searcher.Filter = str.ToString();
            searcher.PropertiesToLoad.Add("nETBIOSName");

            SearchResult res = searcher.FindOne();
            if ((res == null) || (!(res.Properties.Contains("nETBIOSName"))))
                // return the dns name
                result = dnsDomainName;
            else
                // return the netbios name
                result = (string) PropertyManager.GetSearchResultPropertyValue(res, "nETBIOSName");

            return result;
        }

        private static string GetErrorMessage(int errorCode)
        {
            uint temp = (uint) errorCode;
            temp = ( (((temp) & 0x0000FFFF) | (7 << 16) | 0x80000000));

            string errorMsg = String.Empty;
            StringBuilder sb = new StringBuilder(256);
            int result = NativeMethods.FormatMessageW(NativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS |
                                       NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM |
                                       NativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                                       0, (int)temp, 0, sb, sb.Capacity + 1, 0);
            if (result != 0) {
                errorMsg = sb.ToString(0, result);
            }
            else {
                errorMsg = SR.GetString(SR.ADMembership_Unknown_Error, string.Format(CultureInfo.InvariantCulture, "{0}", errorCode));
            }

            return errorMsg;
        }

    }

    internal static class PropertyManager
    {
        public static object GetPropertyValue(DirectoryEntry directoryEntry, string propertyName)
        {

            Debug.Assert(directoryEntry != null, "PropertyManager::GetPropertyValue - directoryEntry is null");
            Debug.Assert(propertyName != null, "PropertyManager::GetPropertyValue - propertyName is null");

            if (directoryEntry.Properties[propertyName].Count == 0)
            {
                if (directoryEntry.Properties["distinguishedName"].Count != 0)
                    throw new ProviderException(SR.GetString(SR.ADMembership_Property_not_found_on_object, propertyName, (string) directoryEntry.Properties["distinguishedName"].Value ));
                else
                    throw new ProviderException(SR.GetString(SR.ADMembership_Property_not_found, propertyName));
            }

            return directoryEntry.Properties[propertyName].Value;
        }

        public static object GetSearchResultPropertyValue(SearchResult res, string propertyName)
        {

            Debug.Assert(res != null, "PropertyManager::GetSearchResultPropertyValue - res is null");
            Debug.Assert(propertyName != null, "PropertyManager::GetSearchResultPropertyValue - propertyName is null");

            ResultPropertyValueCollection propertyValues = null;

            propertyValues = res.Properties[propertyName];
            if ((propertyValues == null) || (propertyValues.Count < 1))
                throw new ProviderException(SR.GetString(SR.ADMembership_Property_not_found,  propertyName));

            return propertyValues[0];
        }
    }

    /*typedef struct _DOMAIN_CONTROLLER_INFO {
		LPTSTR DomainControllerName;
		LPTSTR DomainControllerAddress;
		ULONG DomainControllerAddressType;
		GUID DomainGuid;
		LPTSTR DomainName;
		LPTSTR DnsForestName;
		ULONG Flags;
		LPTSTR DcSiteName;
		LPTSTR ClientSiteName;
	} DOMAIN_CONTROLLER_INFO, *PDOMAIN_CONTROLLER_INFO; */
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	internal sealed class DomainControllerInfo {
	#pragma warning disable 0649
		public string DomainControllerName;
		public string DomainControllerAddress;
		public int DomainControllerAddressType;
		public Guid DomainGuid;
		public string DomainName;
		public string DnsForestName;
		public int Flags;
		public string DcSiteName;
		public string ClientSiteName;
       #pragma warning restore 0649

              public DomainControllerInfo() {}
	}

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal static class NativeMethods
    {
        internal const int ERROR_NO_SUCH_DOMAIN = 1355;
        internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;

        /*DWORD DsGetDcName(
                        LPCTSTR ComputerName,
                        LPCTSTR DomainName,
                        GUID* DomainGuid,
                        LPCTSTR SiteName,
                        ULONG Flags,
                        PDOMAIN_CONTROLLER_INFO* DomainControllerInfo
                        );*/
        [DllImport("Netapi32.dll", CallingConvention=CallingConvention.StdCall, EntryPoint="DsGetDcNameW", CharSet=CharSet.Unicode)]
        internal static extern int DsGetDcName(
            [In] string computerName,
            [In] string domainName,
            [In] IntPtr domainGuid,
            [In] string siteName,
            [In] uint flags,
            [Out] out IntPtr domainControllerInfo);

        /*NET_API_STATUS NetApiBufferFree(
                        LPVOID Buffer
                        );*/
        [DllImport("Netapi32.dll")]
        internal static extern int NetApiBufferFree(
            [In] IntPtr buffer);

        [DllImport("kernel32.dll", CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int FormatMessageW(
            [In] int dwFlags,
            [In] int lpSource,
            [In] int dwMessageId,
            [In] int dwLanguageId,
            [Out] StringBuilder lpBuffer,
            [In] int nSize,
            [In] int arguments);
    }

    [
        ComVisible(false),
        SuppressUnmanagedCodeSecurityAttribute()
    ]
    internal static class NativeComInterfaces
    {

        /*typedef enum {
           ADS_SETTYPE_FULL=1,
           ADS_SETTYPE_PROVIDER=2,
           ADS_SETTYPE_SERVER=3,
           ADS_SETTYPE_DN=4
        } ADS_SETTYPE_ENUM;

        typedef enum {
           ADS_FORMAT_WINDOWS=1,
           ADS_FORMAT_WINDOWS_NO_SERVER=2,
           ADS_FORMAT_WINDOWS_DN=3,
           ADS_FORMAT_WINDOWS_PARENT=4,
           ADS_FORMAT_X500=5,
           ADS_FORMAT_X500_NO_SERVER=6,
           ADS_FORMAT_X500_DN=7,
           ADS_FORMAT_X500_PARENT=8,
           ADS_FORMAT_SERVER=9,
           ADS_FORMAT_PROVIDER=10,
           ADS_FORMAT_LEAF=11
        } ADS_FORMAT_ENUM;

        typedef enum {
           ADS_ESCAPEDMODE_DEFAULT=1,
           ADS_ESCAPEDMODE_ON=2,
           ADS_ESCAPEDMODE_OFF=3,
           ADS_ESCAPEDMODE_OFF_EX=4
        } ADS_ESCAPE_MODE_ENUM;*/

        internal const int ADS_SETTYPE_FULL = 1;
        internal const int ADS_SETTYPE_DN = 4;
        internal const int ADS_FORMAT_PROVIDER = 10;
        internal const int ADS_FORMAT_SERVER = 9;
        internal const int ADS_FORMAT_X500_DN = 7;
        internal const int ADS_ESCAPEDMODE_ON = 2;
        internal const int ADS_ESCAPEDMODE_OFF = 3;

        //
        // Pathname as a co-class that implements the IAdsPathname interface
        //
        [ComImport, Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
        internal class Pathname
        {
        }


        [ComImport, Guid("D592AED4-F420-11D0-A36E-00C04FB950DC"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
        internal interface IAdsPathname
        {

            // HRESULT Set([in] BSTR bstrADsPath,  [in] long lnSetType);
            [SuppressUnmanagedCodeSecurityAttribute()]
            int Set([In, MarshalAs(UnmanagedType.BStr)] string bstrADsPath, [In, MarshalAs(UnmanagedType.U4)] int lnSetType);

            // HRESULT SetDisplayType([in] long lnDisplayType);
            int SetDisplayType([In, MarshalAs(UnmanagedType.U4)] int lnDisplayType);

            // HRESULT Retrieve([in] long lnFormatType,  [out, retval] BSTR* pbstrADsPath);
            [return: MarshalAs(UnmanagedType.BStr)][SuppressUnmanagedCodeSecurityAttribute()]
            string Retrieve([In, MarshalAs(UnmanagedType.U4)] int lnFormatType);

            // HRESULT GetNumElements([out, retval] long* plnNumPathElements);
            [return: MarshalAs(UnmanagedType.U4)]
            int GetNumElements();

            // HRESULT GetElement([in]  long lnElementIndex,  [out, retval] BSTR* pbstrElement);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetElement([In, MarshalAs(UnmanagedType.U4)] int lnElementIndex);

            // HRESULT AddLeafElement([in] BSTR bstrLeafElement);
            void AddLeafElement([In, MarshalAs(UnmanagedType.BStr)] string bstrLeafElement);

            // HRESULT RemoveLeafElement();
            void RemoveLeafElement();

            // HRESULT CopyPath([out, retval] IDispatch** ppAdsPath);
            [return: MarshalAs(UnmanagedType.Interface)]
            object CopyPath();

            // HRESULT GetEscapedElement([in] long lnReserved, [in] BSTR bstrInStr, [out, retval] BSTR*  pbstrOutStr );
            [return: MarshalAs(UnmanagedType.BStr)][SuppressUnmanagedCodeSecurityAttribute()]
            string GetEscapedElement([In, MarshalAs(UnmanagedType.U4)] int lnReserved, [In, MarshalAs(UnmanagedType.BStr)] string bstrInStr);

            int EscapedMode {
                get;
                [SuppressUnmanagedCodeSecurityAttribute()]
                set;
            }

        }

        //
        // LargeInteger as a co-class that implements the IAdsLargeInteger  interface
        //
        [ComImport, Guid("927971f5-0939-11d1-8be1-00c04fd8d503")]
        internal class LargeInteger
        {
        }

        [ComImport, Guid("9068270b-0939-11d1-8be1-00c04fd8d503"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
        internal interface IAdsLargeInteger
        {
            long HighPart {
                [SuppressUnmanagedCodeSecurityAttribute()]
                get;
                [SuppressUnmanagedCodeSecurityAttribute()]
                set;
            }

            long LowPart {
                [SuppressUnmanagedCodeSecurityAttribute()]
                get;
                [SuppressUnmanagedCodeSecurityAttribute()]
                set;
            }
        }

    }

}
