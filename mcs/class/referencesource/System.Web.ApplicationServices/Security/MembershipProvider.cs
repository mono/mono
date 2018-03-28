//------------------------------------------------------------------------------
// <copyright file="MembershipProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Security.Principal;
    using  System.Collections.Specialized;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Security.Cryptography;
    using  System.Runtime.CompilerServices;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Text;
    using  System.Web.Configuration;
    using  System.Web.Util;
    using  System.Diagnostics.CodeAnalysis;
   
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class MembershipProvider : ProviderBase
    {
        //
        // Property Section
        //


        // Public properties
        public abstract bool EnablePasswordRetrieval { get; }

        public abstract bool EnablePasswordReset { get; }

        public abstract bool RequiresQuestionAndAnswer { get; }

        public abstract string ApplicationName { get; set; }

        public abstract int MaxInvalidPasswordAttempts { get; }

        public abstract int PasswordAttemptWindow { get; }

        public abstract bool RequiresUniqueEmail { get; }

        public abstract MembershipPasswordFormat PasswordFormat { get; }

        public abstract int MinRequiredPasswordLength { get; }

        public abstract int MinRequiredNonAlphanumericCharacters { get; }

        public abstract string PasswordStrengthRegularExpression { get; }

        //
        // Method Section
        //


        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract MembershipUser CreateUser( string username,
                                                   string password,
                                                   string email,
                                                   string passwordQuestion,
                                                   string passwordAnswer,
                                                   bool   isApproved,
                                                   object providerUserKey,
                                                   out    MembershipCreateStatus status );


        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer);

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract string GetPassword(string username, string answer);

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract bool ChangePassword(string username, string oldPassword, string newPassword);

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract string ResetPassword(string username, string answer);

        public abstract void UpdateUser(MembershipUser user);

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract bool ValidateUser(string username, string password);


        public abstract bool UnlockUser( string userName );

        public abstract MembershipUser GetUser( object providerUserKey, bool userIsOnline );

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract MembershipUser GetUser(string username, bool userIsOnline);

        // GetUser() can throw 1 type of exception:
        // 1. ArgumentException is thrown if:
        //    A. Username is null, is empty, contains commas, or is longer than 256 characters
        internal MembershipUser GetUser(string username, bool userIsOnline, bool throwOnError) {
            MembershipUser user = null;

            try {
                user = GetUser(username, userIsOnline);
            }
            catch (ArgumentException) {
                if (throwOnError) throw;
            }

            return user;
        }

        public abstract string GetUserNameByEmail(string email);

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract bool DeleteUser(string username, bool deleteAllRelatedData);


        public abstract MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords);


        public abstract int GetNumberOfUsersOnline();


        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
        public abstract MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords);

        public abstract MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords);

        protected virtual byte[] EncryptPassword( byte[] password)
        {
            return EncryptPassword(password, MembershipPasswordCompatibilityMode.Framework20);
        }

        protected virtual byte[] EncryptPassword( byte[] password, MembershipPasswordCompatibilityMode legacyPasswordCompatibilityMode)
        {
            if (SystemWebProxy.Membership.IsDecryptionKeyAutogenerated)
                throw new ProviderException(ApplicationServicesStrings.Can_not_use_encrypted_passwords_with_autogen_keys);

            return SystemWebProxy.Membership.EncryptOrDecryptData(true, password, legacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework20);
        }

        protected virtual byte[] DecryptPassword( byte[] encodedPassword )
        {
            if (SystemWebProxy.Membership.IsDecryptionKeyAutogenerated)
                throw new ProviderException(ApplicationServicesStrings.Can_not_use_encrypted_passwords_with_autogen_keys);

            try {
                return SystemWebProxy.Membership.EncryptOrDecryptData(false, encodedPassword, false);
            } catch {
                if (!SystemWebProxy.Membership.UsingCustomEncryption)
                    throw;
            }
            return SystemWebProxy.Membership.EncryptOrDecryptData(false, encodedPassword, true);
        }

        //
        // Event Section
        //

        public event MembershipValidatePasswordEventHandler ValidatingPassword
        {
            add
            {
                _EventHandler += value;
            }
            remove
            {
                _EventHandler -= value;
            }
        }

        protected virtual void OnValidatingPassword( ValidatePasswordEventArgs e )
        {
            if( _EventHandler != null )
            {
                _EventHandler( this, e );
            }
        }

        private MembershipValidatePasswordEventHandler _EventHandler;
    }
}
