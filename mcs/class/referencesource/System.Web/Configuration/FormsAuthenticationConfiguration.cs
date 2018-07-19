//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*****************************************************************************
     From machine.config
        <!--
        authentication Attributes:
          mode="[Windows|Forms|Passport|None]"
        -->
        <authentication mode="Windows">

            <!--
            forms Attributes:
              name="[cookie name]" - Name of the cookie used for Forms Authentication
              loginUrl="[url]" - Url to redirect client to for Authentication
              protection="[All|None|Encryption|Validation]" - Protection mode for data in cookie
              timeout="[minutes]" - Duration of time for cookie to be valid (reset on each request)
              path="/" - Sets the path for the cookie
              requireSSL="[true|false]" - Should the forms-authentication cookie be sent only over SSL
              slidingExpiration="[true|false]" - Should the forms-authentication-cookie and ticket be re-issued if they are about to expire
              defaultUrl="string" - Page to redirect to after login, if none has been specified
              cookieless="[UseCookies|UseUri|AutoDetect|UseDeviceProfile]" - Use Cookies or the URL path to store the forms authentication ticket
              domain="string" - Domain of the cookie
            -->
            <forms
                    name=".ASPXAUTH"
                    loginUrl="login.aspx"
                    protection="All"
                    timeout="30"
                    path="/"
                    requireSSL="false"
                    slidingExpiration="true"
                    defaultUrl="default.aspx"
                    cookieless="UseDeviceProfile"
                    enableCrossAppRedirects="false" >

                <!--
                credentials Attributes:
                  passwordFormat="[Clear|SHA1|MD5]" - format of user password value stored in <user>
                -->
                <credentials passwordFormat="SHA1">
                        <!-- <user name="UserName" password="password" /> -->
                </credentials>

            </forms>

            <!--
            passport Attributes:
               redirectUrl=["url"] - Specifies the page to redirect to, if the page requires authentication, and the user has not signed on with passport
            -->
            <passport redirectUrl="internal" />

        </authentication>

        <authentication mode="Windows">
            <forms
                    name=".ASPXAUTH"
                    loginUrl="login.aspx"
                    protection="All"
                    timeout="30"
                    path="/"
                    requireSSL="false"
                    slidingExpiration="true"
                    defaultUrl="default.aspx"
                    cookieless="UseDeviceProfile"
                    enableCrossAppRedirects="false" >

                <credentials passwordFormat="SHA1">
                </credentials>
            </forms>
            <passport redirectUrl="internal" />
        </authentication>

    ******************************************************************************/

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class FormsAuthenticationConfiguration : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty = 
            new ConfigurationElementProperty(new CallbackValidator(typeof(FormsAuthenticationConfiguration), Validate));

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propCredentials =
                                        new ConfigurationProperty("credentials", 
                                        typeof(FormsAuthenticationCredentials), 
                                        null, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        ".ASPXAUTH",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propLoginUrl =
            new ConfigurationProperty("loginUrl",
                                        typeof(string),
                                        "login.aspx",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propDefaultUrl =
            new ConfigurationProperty("defaultUrl",
                                        typeof(string),
                                        "default.aspx",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propProtection =
            new ConfigurationProperty("protection", 
                                        typeof(FormsProtectionEnum), 
                                        FormsProtectionEnum.All, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propTimeout =
            new ConfigurationProperty("timeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(30.0),
                                        StdValidatorsAndConverters.TimeSpanMinutesConverter,
                                        new TimeSpanValidator(TimeSpan.FromMinutes(1), TimeSpan.MaxValue),
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propPath =
            new ConfigurationProperty("path",
                                        typeof(string),
                                        "/",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propRequireSSL =
            new ConfigurationProperty("requireSSL", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propSlidingExpiration =
            new ConfigurationProperty("slidingExpiration", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propCookieless =
            new ConfigurationProperty("cookieless", 
                                        typeof(HttpCookieMode), 
                                        HttpCookieMode.UseDeviceProfile, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propDomain =
            new ConfigurationProperty("domain", 
                                        typeof(string), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEnableCrossAppRedirects =
            new ConfigurationProperty("enableCrossAppRedirects", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propTicketCompatibilityMode =
            new ConfigurationProperty("ticketCompatibilityMode",
                                        typeof(TicketCompatibilityMode),
                                        TicketCompatibilityMode.Framework20,
                                        ConfigurationPropertyOptions.None);

        static FormsAuthenticationConfiguration() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propCredentials);
            _properties.Add(_propName);
            _properties.Add(_propLoginUrl);
            _properties.Add(_propDefaultUrl);
            _properties.Add(_propProtection);
            _properties.Add(_propTimeout);
            _properties.Add(_propPath);
            _properties.Add(_propRequireSSL);
            _properties.Add(_propSlidingExpiration);
            _properties.Add(_propCookieless);
            _properties.Add(_propDomain);
            _properties.Add(_propEnableCrossAppRedirects);
            _properties.Add(_propTicketCompatibilityMode);
        }

        public FormsAuthenticationConfiguration() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("credentials")]
        public FormsAuthenticationCredentials Credentials {
            get {
                return (FormsAuthenticationCredentials)base[_propCredentials];
            }
        }

        [ConfigurationProperty("name", DefaultValue = ".ASPXAUTH")]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    base[_propName] = _propName.DefaultValue;
                }
                else {
                    base[_propName] = value;
                }
            }
        }

        [ConfigurationProperty("loginUrl", DefaultValue = "login.aspx")]
        [StringValidator(MinLength = 1)]
        public string LoginUrl {
            get {
                return (string)base[_propLoginUrl];
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    base[_propLoginUrl] = _propLoginUrl.DefaultValue;
                }
                else {
                    base[_propLoginUrl] = value;
                }
            }
        }

        [ConfigurationProperty("defaultUrl", DefaultValue = "default.aspx")]
        [StringValidator(MinLength = 1)]
        public string DefaultUrl {
            get {
                return (string)base[_propDefaultUrl];
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    base[_propDefaultUrl] = _propDefaultUrl.DefaultValue;
                }
                else {
                    base[_propDefaultUrl] = value;
                }
            }
        }

        [ConfigurationProperty("protection", DefaultValue = FormsProtectionEnum.All)]
        public FormsProtectionEnum Protection {
            get {
                return (FormsProtectionEnum)base[_propProtection];
            }
            set {
                base[_propProtection] = value;
            }
        }

        [ConfigurationProperty("timeout", DefaultValue = "00:30:00")]
        [TimeSpanValidator(MinValueString="00:01:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanMinutesConverter))]
        public TimeSpan Timeout {
            get {
                return (TimeSpan)base[_propTimeout];
            }
            set {
                base[_propTimeout] = value;
            }
        }

        [ConfigurationProperty("path", DefaultValue = "/")]
        [StringValidator(MinLength = 1)]
        public string Path {
            get {
                return (string)base[_propPath];
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    base[_propPath] = _propPath.DefaultValue;
                }
                else {
                    base[_propPath] = value;
                }
            }
        }

        [ConfigurationProperty("requireSSL", DefaultValue = false)]
        public bool RequireSSL {
            get {
                return (bool)base[_propRequireSSL];
            }
            set {
                base[_propRequireSSL] = value;
            }
        }

        [ConfigurationProperty("slidingExpiration", DefaultValue = true)]
        public bool SlidingExpiration {
            get {
                return (bool)base[_propSlidingExpiration];
            }
            set {
                base[_propSlidingExpiration] = value;
            }
        }

        [ConfigurationProperty("enableCrossAppRedirects", DefaultValue = false)]
        public bool EnableCrossAppRedirects {
            get {
                return (bool)base[_propEnableCrossAppRedirects];
            }
            set {
                base[_propEnableCrossAppRedirects] = value;
            }
        }


        [ConfigurationProperty("cookieless", DefaultValue = HttpCookieMode.UseDeviceProfile)]
        public HttpCookieMode Cookieless {
            get {
                return (HttpCookieMode)base[_propCookieless];
            }
            set {
                base[_propCookieless] = value;
            }
        }

        [ConfigurationProperty("domain", DefaultValue = "")]
        public string Domain {
            get {
                return (string)base[_propDomain];
            }
            set {
                base[_propDomain] = value;
            }
        }

        [ConfigurationProperty("ticketCompatibilityMode", DefaultValue = TicketCompatibilityMode.Framework20)]
        public TicketCompatibilityMode TicketCompatibilityMode {
            get {
                return (TicketCompatibilityMode)base[_propTicketCompatibilityMode];
            }
            set {
                base[_propTicketCompatibilityMode] = value;
            }
        }

        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }
        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("forms");
            }

            FormsAuthenticationConfiguration elem = (FormsAuthenticationConfiguration)value;

            if (StringUtil.StringStartsWith(elem.LoginUrl, "\\\\") || 
                (elem.LoginUrl.Length > 1 && elem.LoginUrl[1] == ':')) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Auth_bad_url), 
                    elem.ElementInformation.Properties["loginUrl"].Source, 
                    elem.ElementInformation.Properties["loginUrl"].LineNumber);
            }

            if (StringUtil.StringStartsWith(elem.DefaultUrl, "\\\\") || 
                (elem.DefaultUrl.Length > 1 && elem.DefaultUrl[1] == ':')) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Auth_bad_url), 
                    elem.ElementInformation.Properties["defaultUrl"].Source, 
                    elem.ElementInformation.Properties["defaultUrl"].LineNumber);
            }
        }
    } // class FormsAuthenticationConfiguration
}
