//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationUser.cs" company="Microsoft">
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

    public sealed class FormsAuthenticationUser : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        "",
                                        new LowerCaseStringConverter(),
                                        null,
                                        ConfigurationPropertyOptions.IsRequired | 
                                        ConfigurationPropertyOptions.IsKey);
        
        private static readonly ConfigurationProperty _propPassword =
            new ConfigurationProperty("password", 
                                        typeof(string), 
                                        "", 
                                        ConfigurationPropertyOptions.IsRequired);

        static FormsAuthenticationUser() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propPassword);
        }

        internal FormsAuthenticationUser() {
        }

        public FormsAuthenticationUser(String name, String password)
            : this() {
            Name = name.ToLower(CultureInfo.InvariantCulture);
            Password = password;
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [TypeConverter(typeof(LowerCaseStringConverter))]
        [StringValidator()]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("password", IsRequired = true, DefaultValue = "")]
        [StringValidator()]
        public string Password {
            get {
                return (string)base[_propPassword];
            }
            set {
                base[_propPassword] = value;
            }
        }

    } // class FormsAuthenticationUser
}
