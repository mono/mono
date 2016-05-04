//------------------------------------------------------------------------------
// <copyright file="NetworkCredential.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using Microsoft.Win32;


    /// <devdoc>
    ///    <para>Provides credentials for password-based
    ///       authentication schemes such as basic, digest, NTLM and Kerberos.</para>
    /// </devdoc>
    public class NetworkCredential : ICredentials,ICredentialsByHost {

        private static volatile EnvironmentPermission m_environmentUserNamePermission;
        private static volatile EnvironmentPermission m_environmentDomainNamePermission;
        private static readonly object lockingObject = new object();
        private string m_domain;
        private string m_userName;
#if !FEATURE_PAL
        private SecureString m_password;
#else  //FEATURE_PAL
        private string m_password;
#endif //FEATURE_PAL

        public NetworkCredential()
        : this(string.Empty, string.Empty, string.Empty) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, string password)
        : this(userName, password, string.Empty) {
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, SecureString password)
        : this(userName, password, string.Empty) {
        }
#endif //!FEATURE_PAL        
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, string password, string domain) {
            UserName = userName;
            Password = password;
            Domain = domain;
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, SecureString password, string domain) {
            UserName = userName;
            SecurePassword = password;
            Domain = domain;
        }
#endif //!FEATURE_PAL        

        void InitializePart1() {
            if (m_environmentUserNamePermission == null) {
                lock(lockingObject) {
                    if (m_environmentUserNamePermission == null) {
                        m_environmentDomainNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERDOMAIN");
                        m_environmentUserNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME");
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       The user name associated with this credential.
        ///    </para>
        /// </devdoc>
        public string UserName {
            get {
                InitializePart1();
                m_environmentUserNamePermission.Demand();
                return InternalGetUserName();
            }
            set {
                if (value == null)
                    m_userName = String.Empty;
                else
                    m_userName = value;
                // GlobalLog.Print("NetworkCredential::set_UserName: m_userName: \"" + m_userName + "\"" );
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The password for the user name.
        ///    </para>
        /// </devdoc>
        public string Password {
            get {
                ExceptionHelper.UnmanagedPermission.Demand();
                return InternalGetPassword();
            }
            set {
#if FEATURE_PAL
                if (value == null)
                    m_password = String.Empty;
                else
                    m_password = value;
//                GlobalLog.Print("NetworkCredential::set_Password: m_password: \"" + m_password + "\"" );
#else //!FEATURE_PAL
                m_password = UnsafeNclNativeMethods.SecureStringHelper.CreateSecureString(value);
//                GlobalLog.Print("NetworkCredential::set_Password: value = " + value);
//                GlobalLog.Print("NetworkCredential::set_Password: m_password:");
//                GlobalLog.Dump(m_password);
#endif //!FEATURE_PAL
            }
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       The password for the user name.
        ///    </para>
        /// </devdoc>
        public SecureString SecurePassword {
            get {
                ExceptionHelper.UnmanagedPermission.Demand();
                return InternalGetSecurePassword().Copy();
            }
            set {
                if (value == null)
                    m_password = new SecureString(); // makes 0 length string
                else
                    m_password = value.Copy();
            }
        }
#endif //!FEATURE_PAL
        
        /// <devdoc>
        ///    <para>
        ///       The machine name that verifies
        ///       the credentials. Usually this is the host machine.
        ///    </para>
        /// </devdoc>
        public string Domain {
            get {
                InitializePart1();
                m_environmentDomainNamePermission.Demand();
                return InternalGetDomain();
            }
            set {
                if (value == null)
                    m_domain = String.Empty;
                else
                    m_domain = value;
//                GlobalLog.Print("NetworkCredential::set_Domain: m_domain: \"" + m_domain + "\"" );
            }
        }

        internal string InternalGetUserName() {
            // GlobalLog.Print("NetworkCredential::get_UserName: returning \"" + m_userName + "\"");
            return m_userName;
        }

        internal string InternalGetPassword() {
#if FEATURE_PAL
            // GlobalLog.Print("NetworkCredential::get_Password: returning \"" + m_password + "\"");
            return m_password;
#else //!FEATURE_PAL
            string decryptedString = UnsafeNclNativeMethods.SecureStringHelper.CreateString(m_password);

            // GlobalLog.Print("NetworkCredential::get_Password: returning \"" + decryptedString + "\"");
            return decryptedString;
#endif //!FEATURE_PAL
        }

#if !FEATURE_PAL
        internal SecureString InternalGetSecurePassword()
        {
            return m_password;
        }
#endif //!FEATURE_PAL

        internal string InternalGetDomain()
        {
            // GlobalLog.Print("NetworkCredential::get_Domain: returning \"" + m_domain + "\"");
            return m_domain;
        }

        internal string InternalGetDomainUserName() {
            string domainUserName = InternalGetDomain();
            if (domainUserName.Length != 0)
                domainUserName += "\\";
            domainUserName += InternalGetUserName();
            return domainUserName;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns an instance of the NetworkCredential class for a Uri and
        ///       authentication type.
        ///    </para>
        /// </devdoc>
        public NetworkCredential GetCredential(Uri uri, String authType) {
            return this;
        }

        public NetworkCredential GetCredential(string host, int port, String authenticationType) {
            return this;
        }

#if DEBUG
        // this method is only called as part of an assert
        internal bool IsEqualTo(object compObject) {
            if ((object)compObject == null)
                return false;
            if ((object)this == (object)compObject)
                return true;
            NetworkCredential compCred = compObject as NetworkCredential;
            if ((object)compCred == null)
                return false;
#if FEATURE_PAL
            return(InternalGetUserName() == compCred.InternalGetUserName() &&
                   InternalGetPassword() == compCred.InternalGetPassword() &&
                   InternalGetDomain()  == compCred.InternalGetDomain());
#else //!FEATURE_PAL
            return (InternalGetUserName() == compCred.InternalGetUserName() &&
                    InternalGetDomain() == compCred.InternalGetDomain() &&
                    UnsafeNclNativeMethods.SecureStringHelper.AreEqualValues(InternalGetSecurePassword(), 
                                                                             compCred.InternalGetSecurePassword()));
#endif //!FEATURE_PAL
        }
#endif //DEBUG
    } // class NetworkCredential
} // namespace System.Net
