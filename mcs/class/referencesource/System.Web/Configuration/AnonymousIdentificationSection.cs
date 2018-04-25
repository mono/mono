//------------------------------------------------------------------------------
// <copyright file="AnonymousIdentificationSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Security;
    using System.ComponentModel;
    using System.Security.Permissions;

    //        <!--
    //            anonymousIdentification configuration:
    //                enabled="[true|false]"                            Feature is enabled?
    //                cookieName=".ASPXANONYMOUS"                       Cookie Name
    //                cookieTimeout="100000"                            Cookie Timeout in minutes
    //                cookiePath="/"                                    Cookie Path
    //                cookieRequireSSL="[true|false]"                   Set Secure bit in Cookie
    //                cookieSlidingExpiration="[true|false]"            Reissue expiring cookies?
    //                cookieProtection="[None|Validation|Encryption|All]"    How to protect cookies from being read/tampered
    //                cookieless="[UseCookies|UseUri|AutoDetect|UseDeviceProfile]" - Use Cookies or the URL path to store the id
    //                domain="[domain]"                                 Enables output of the "domain" cookie attribute set to the specified value
    //        -->
    //
    //        <anonymousIdentification enabled="false" cookieName=".ASPXANONYMOUS" cookieTimeout="100000"
    //                cookiePath="/" cookieRequireSSL="false" cookieSlidingExpiration="true"
    //                cookieProtection="None" cookieless="UseDeviceProfile" domain="" />

    // [SectionComment(
    //        "            anonymousIdentification configuration:" + "\r\n" +
    //        "                enabled=\"[true|false]\"                            Feature is enabled?" + "\r\n" +
    //        "                cookieName=\".ASPXANONYMOUS\"                       Cookie Name" + "\r\n" +
    //        "                cookieTimeout=\"100000\"                            Cookie Timeout in minutes" + "\r\n" +
    //        "                cookiePath=\"/\"                                    Cookie Path" + "\r\n" +
    //        "                cookieRequireSSL=\"[true|false]\"                   Set Secure bit in Cookie" + "\r\n" +
    //        "                cookieSlidingExpiration=\"[true|false]\"            Reissue expiring cookies?" + "\r\n" +
    //        "                cookieProtection=\"[None|Validation|Encryption|All]\"    How to protect cookies from being read/tampered" + "\r\n" +
    //        "                cookieless=\"[UseCookies|UseUri|AutoDetect|UseDeviceProfile]\" - Use Cookies or the URL path to store the id" + "\r\n" +
    //        "                domain=\"[domain]\"                                 Enables output of the "domain" cookie attribute set to the specified value" + "\r\n" +
    //        "        -->" + "\r\n" +
    //    )]
    public sealed class AnonymousIdentificationSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieName =
            new ConfigurationProperty("cookieName",
                                        typeof(string),
                                        ".ASPXANONYMOUS",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieTimeout =
            new ConfigurationProperty("cookieTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(100000.0),
                                        StdValidatorsAndConverters.TimeSpanMinutesOrInfiniteConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookiePath =
            new ConfigurationProperty("cookiePath",
                                        typeof(string),
                                        "/",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieRequireSSL =
            new ConfigurationProperty("cookieRequireSSL", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieSlidingExpiration =
            new ConfigurationProperty("cookieSlidingExpiration", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieProtection =
            new ConfigurationProperty("cookieProtection", typeof(CookieProtection), CookieProtection.Validation, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieless =
            new ConfigurationProperty("cookieless", typeof(HttpCookieMode), HttpCookieMode.UseCookies, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDomain =
            new ConfigurationProperty("domain", typeof(string), null, ConfigurationPropertyOptions.None);

        static AnonymousIdentificationSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propEnabled);
            _properties.Add(_propCookieName);
            _properties.Add(_propCookieTimeout);
            _properties.Add(_propCookiePath);
            _properties.Add(_propCookieRequireSSL);
            _properties.Add(_propCookieSlidingExpiration);
            _properties.Add(_propCookieProtection);
            _properties.Add(_propCookieless);
            _properties.Add(_propDomain);
        }

        public AnonymousIdentificationSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("cookieName", DefaultValue = ".ASPXANONYMOUS")]
        [StringValidator(MinLength = 1)]
        public string CookieName {
            get {
                return (string)base[_propCookieName];
            }
            set {
                base[_propCookieName] = value;
            }
        }

        [ConfigurationProperty("cookieTimeout", DefaultValue = "69.10:40:00")]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanMinutesOrInfiniteConverter))]
        public TimeSpan CookieTimeout {
            get {
                return (TimeSpan)base[_propCookieTimeout];
            }
            set {
                base[_propCookieTimeout] = value;
            }
        }

        [ConfigurationProperty("cookiePath", DefaultValue = "/")]
        [StringValidator(MinLength = 1)]
        public string CookiePath {
            get {
                return (string)base[_propCookiePath];
            }
            set {
                base[_propCookiePath] = value;
            }
        }

        [ConfigurationProperty("cookieRequireSSL", DefaultValue = false)]
        public bool CookieRequireSSL {
            get {
                return (bool)base[_propCookieRequireSSL];
            }
            set {
                base[_propCookieRequireSSL] = value;
            }
        }

        [ConfigurationProperty("cookieSlidingExpiration", DefaultValue = true)]
        public bool CookieSlidingExpiration {
            get {
                return (bool)base[_propCookieSlidingExpiration];
            }
            set {
                base[_propCookieSlidingExpiration] = value;
            }
        }

        [ConfigurationProperty("cookieProtection", DefaultValue = CookieProtection.Validation)]
        public CookieProtection CookieProtection {
            get {
                return (CookieProtection)base[_propCookieProtection];
            }
            set {
                base[_propCookieProtection] = value;
            }
        }

        [ConfigurationProperty("cookieless", DefaultValue = HttpCookieMode.UseCookies)]
        public HttpCookieMode Cookieless {
            get {
                return (HttpCookieMode)base[_propCookieless];
            }
            set {
                base[_propCookieless] = value;
            }
        }

        [ConfigurationProperty("domain")]
        public string Domain {
            get {
                return (string)base[_propDomain];
            }
            set {
                base[_propDomain] = value;
            }
        }
    }
}
