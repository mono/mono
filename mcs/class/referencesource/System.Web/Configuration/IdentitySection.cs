//------------------------------------------------------------------------------
// <copyright file="IdentitySection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Security.Permissions;

        /*        <!--
            identity Attributes:
              impersonate="[true|false]" - Impersonate Windows User
                userName="Windows user account to impersonate" | empty string implies impersonate the LOGON user specified by IIS
                password="password of above specified account" | empty string
            -->
            <identity impersonate="false" />

*/
    public sealed class IdentitySection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propImpersonate =
            new ConfigurationProperty("impersonate", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserName =
            new ConfigurationProperty("userName", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPassword =
            new ConfigurationProperty("password", typeof(string), String.Empty, ConfigurationPropertyOptions.None);

        private ImpersonateTokenRef _impersonateTokenRef = new ImpersonateTokenRef(IntPtr.Zero);

        private string _username;
        private string _password;
        private bool impersonateCache = false;
        private bool impersonateCached = false; // value not read yet
        private bool _credentialsValidated;
        private object _credentialsValidatedLock = new object();
        private String error = String.Empty;

        static IdentitySection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propImpersonate);
            _properties.Add(_propUserName);
            _properties.Add(_propPassword);
        }

        protected override object GetRuntimeObject() {
            // VSWhidbey 554776: The method ValidateCredentials() is not safe
            // when multiple threads are accessing it, because the method access
            // and modify member variables.  After reviewing the code,
            // _impersonateTokenRef.Handle is actually cached, so it is safe to
            // cache the validation result as a whole.  That will avoid
            // ValidateCredentials() to be called with multiple threads.
            if (!_credentialsValidated) {
                lock (_credentialsValidatedLock) {
                    if (!_credentialsValidated) {
                        ValidateCredentials();
                        _credentialsValidated = true;
                    }
                }
            }
            return base.GetRuntimeObject();
        }

        public IdentitySection() {
            impersonateCached = false;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("impersonate", DefaultValue = false)]
        public bool Impersonate {
            get {
                if (impersonateCached == false) {
                    impersonateCache = (bool)base[_propImpersonate];
                    impersonateCached = true; // value has been read now
                }
                return impersonateCache;
            }
            set {
                base[_propImpersonate] = value;
                impersonateCache = value;
            }
        }

        [ConfigurationProperty("userName", DefaultValue = "")]
        public string UserName {
            get {
                return (string)base[_propUserName];
            }
            set {
                base[_propUserName] = value;
            }
        }

        [ConfigurationProperty("password", DefaultValue = "")]
        public string Password {
            get {
                return (string)base[_propPassword];
            }
            set {
                base[_propPassword] = value;
            }
        }

        protected override void Reset(ConfigurationElement parentElement) {
            base.Reset(parentElement);
            IdentitySection parent = parentElement as IdentitySection;
            if (parent != null) {
                _impersonateTokenRef = parent._impersonateTokenRef;
                // No partial overrides
                if (Impersonate) {
                    UserName = null;
                    Password = null;
                    _impersonateTokenRef = new ImpersonateTokenRef(IntPtr.Zero);
                }
                impersonateCached = false; // We don't want to cache the parent's value!
                _credentialsValidated = false;
            }
        }

        protected override void Unmerge(ConfigurationElement sourceElement,
                                                ConfigurationElement parentElement,
                                                ConfigurationSaveMode saveMode) {
            base.Unmerge(sourceElement, parentElement, saveMode); // do this to unmerge locks
            IdentitySection source = sourceElement as IdentitySection;
            if (Impersonate != source.Impersonate) { // this will not be copied by unmerge if it is the same as parent 
                Impersonate = source.Impersonate;    // If it is different than expected make sure it is set or validation
            }                                        // will be missed
            // this section does not inherit in the same manner since partial overrides are not permitted
            if (Impersonate) // was impersonate set in the merge
            {
                if (source.ElementInformation.Properties[_propUserName.Name].IsModified ||
                    source.ElementInformation.Properties[_propPassword.Name].IsModified) {
                    UserName = source.UserName;
                    Password = source.Password;
                }
            }
        }
        private void ValidateCredentials() {
            _username = UserName;
            _password = Password;

            if (HandlerBase.CheckAndReadRegistryValue(ref _username, false) == false) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_registry_config), 
                    ElementInformation.Source, ElementInformation.LineNumber);
            }
            if (HandlerBase.CheckAndReadRegistryValue(ref _password, false) == false) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_registry_config), 
                    ElementInformation.Source, 
                    ElementInformation.LineNumber);
            }

            if (_username != null && _username.Length < 1) {
                _username = null;
            }

            if (_username != null && Impersonate) {
                if (_password == null) {
                    _password = String.Empty;
                }
            }
            else if (_password != null && _username == null && _password.Length > 0 && Impersonate) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_credentials), 
                    ElementInformation.Properties["password"].Source, 
                    ElementInformation.Properties["password"].LineNumber);
            }
            if (Impersonate && ImpersonateToken == IntPtr.Zero && _username != null) {
                if (error.Length > 0) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_credentials_2, error), 
                        ElementInformation.Properties["userName"].Source, 
                        ElementInformation.Properties["userName"].LineNumber);
                }
                else {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_credentials), 
                        ElementInformation.Properties["userName"].Source, 
                        ElementInformation.Properties["userName"].LineNumber);
                }
            }
        }

        private void InitializeToken() {
            error = String.Empty;
            IntPtr token = CreateUserToken(_username, _password, out error);

            _impersonateTokenRef = new ImpersonateTokenRef(token);

            if (_impersonateTokenRef.Handle == IntPtr.Zero) {
                if (error.Length > 0) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_credentials_2, error), 
                        ElementInformation.Properties["userName"].Source, 
                        ElementInformation.Properties["userName"].LineNumber);
                }
                else {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_credentials), 
                        ElementInformation.Properties["userName"].Source, 
                        ElementInformation.Properties["userName"].LineNumber);
                }
            }
        }

        internal IntPtr ImpersonateToken {
            get {
                if (_impersonateTokenRef.Handle == IntPtr.Zero) {
                    if (_username != null && Impersonate) {
                        InitializeToken();
                    }
                }
                return _impersonateTokenRef.Handle;
            }
        }

        internal static IntPtr CreateUserToken(String name, String password, out String error) {
            IntPtr token = IntPtr.Zero;
            // when using ASP.NET process model call back via ISAPI
            if (VersionInfo.ExeName == "aspnet_wp") {
                byte[] bOut = new byte[IntPtr.Size];
                byte[] bIn1 = System.Text.Encoding.Unicode.GetBytes(name + "\t" + password);
                byte[] bIn = new byte[bIn1.Length + 2];
                Buffer.BlockCopy(bIn1, 0, bIn, 0, bIn1.Length);

                if (UnsafeNativeMethods.PMCallISAPI(IntPtr.Zero, 
                                UnsafeNativeMethods.CallISAPIFunc.GenerateToken, 
                                bIn, 
                                bIn.Length, 
                                bOut, 
                                bOut.Length) == 1) {
                    Int64 iToken = 0;
                    for (int iter = 0; iter < IntPtr.Size; iter++) {
                        iToken = iToken * 256 + bOut[iter];
                    }
                    token = (IntPtr)iToken;

                    Debug.Trace("Token", "Token " + token + " for (" + name + "," + password + ") obtained via ISAPI");
                }
            }
            // try to create the token directly
            if (token == IntPtr.Zero) {
                StringBuilder errorBuffer = new StringBuilder(256);
                token = UnsafeNativeMethods.CreateUserToken(name, password, 1, errorBuffer, 256);
                error = errorBuffer.ToString();

                if (token != IntPtr.Zero) {
                    Debug.Trace("Token", "Token " + token + " for (" + name + "," + password + ") obtained directly");
                }
            }
            else {
                error = String.Empty;
            }

            if (token == IntPtr.Zero) {
                Debug.Trace("Token", "Failed to create token for (" + name + "," + password + ")");
            }

            return token;
        }

        internal ContextInformation ProtectedEvaluationContext {
            get {
                return this.EvaluationContext;
            }
        }
    }
}
