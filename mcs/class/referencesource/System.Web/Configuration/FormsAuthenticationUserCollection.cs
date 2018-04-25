//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationUserCollection.cs" company="Microsoft">
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

    [ConfigurationCollection(typeof(FormsAuthenticationUser), AddItemName = "user",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class FormsAuthenticationUserCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static FormsAuthenticationUserCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public FormsAuthenticationUserCollection() {
        }
        
        // public properties
        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }
        
        public new FormsAuthenticationUser this[string name] {
            get {
                return (FormsAuthenticationUser)BaseGet(name);
            }
            // Having a setter here would be strange in that you could write
            //  collection["Name1"] = new FormsAuthenticationUser("differentName"...
            //
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public FormsAuthenticationUser this[int index] {
            get {
                return (FormsAuthenticationUser)BaseGet(index);
            }
            set {
                BaseAdd(index, value);
            }
        }
        
        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new FormsAuthenticationUser();
        }
        
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((FormsAuthenticationUser)element).Name;
        }
        
        protected override string ElementName {
            get {
                return "user";
            }
        }

        protected override bool ThrowOnDuplicate {
            get {
                return true;
            }
        }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        // public methods
        public void Add(FormsAuthenticationUser user) {
            BaseAdd(user);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        public FormsAuthenticationUser Get(int index) {
            return (FormsAuthenticationUser)BaseGet(index);
        }
        
        public FormsAuthenticationUser Get(string name) {
            return (FormsAuthenticationUser)BaseGet(name);
        }
        
        public String GetKey(int index) {
            return (String) BaseGetKey(index);
        }
        
        public void Remove(string name) {
            BaseRemove(name);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Set(FormsAuthenticationUser user) {
            BaseAdd(user, false);
        }
    }
}
