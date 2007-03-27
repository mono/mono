//
// Mainsoft.Web.Profile.WPProfileProvider
//
// Authors:
//	Ilya Kharmatsky (ilyak@mainsoft.com)
//
// (C) 2007 Mainsoft
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.Profile;
using System.Configuration;

using java.util;
using javax.portlet;

using Mainsoft.Web.Security;
using vmw.portlet;



namespace Mainsoft.Web.Profile
{
    public class WPProfileProvider : ProfileProvider
    {
        internal static readonly string DESCRIPTION = "WebSphere Portal Profile Provider";

        private static readonly string BIN_SERIALIZATION_PREFIX = "VMW_BIN_PREFIX:";
        private static readonly string BIN_SERIALIZATION_NULL = "#_NULL_#";

        private string _applicationName = String.Empty;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            if(_applicationName == String.Empty && config != null)
            {
                _applicationName = config["applicationName"];
                _applicationName = (_applicationName == null) ? String.Empty : _applicationName;
            }
        }

        public override string Description
        {
            get
            {
                return DESCRIPTION;
            }
        }

        public override string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();
            if (properties.Count == 0)
                return settings;

            PortletPreferences pp = PortletPreferences;
            if (pp == null)
            {
#if DEBUG
                Console.WriteLine("Cannot obtain PortletPreferences");
#endif
                return settings;
            }

            foreach (SettingsProperty property in properties)
            {
                if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                        property.SerializeAs = SettingsSerializeAs.String;
                    else
                        property.SerializeAs = SettingsSerializeAs.Xml;

                settings.Add(new SettingsPropertyValue(property));
            }

            for(java.util.Enumeration enumer = pp.getNames(); enumer.hasMoreElements();)
            {
                string name = (string)enumer.nextElement();

                SettingsPropertyValue property = settings[name];
                
                if (property == null)
                    continue;

                string value = pp.getValue(name, null);

                if (value == null)
                {
                    property.IsDirty = false;
                    property.Deserialized = true;
                    property.PropertyValue = null;
                }
                else if (value.StartsWith(BIN_SERIALIZATION_PREFIX))
                {
                    if (value.StartsWith(BIN_SERIALIZATION_PREFIX + BIN_SERIALIZATION_NULL))
                    {
                        property.SerializedValue = null;
                    }
                    else
                    {
                        string base64 = value.Substring(BIN_SERIALIZATION_PREFIX.Length);
                        byte[] serializedData = Convert.FromBase64String(base64);
                        property.SerializedValue = serializedData;
                    }
                }
                else
                {
                    property.SerializedValue = value;
                }
            }
            return settings;

        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            if (!IsInActionPhase)
            {
#if DEBUG
                Console.WriteLine("The portlet not in the process action phase");
#endif
                return;
            }
            PortletPreferences pp = PortletPreferences;
            if (pp == null)
            {
#if DEBUG
                Console.WriteLine("Cannot obtain PortletPreferences");
#endif
                return;
            }
            
            try
            {
                string username = (string)context["UserName"];
                bool authenticated = (bool)context["IsAuthenticated"];
#if DEBUG
                Console.WriteLine("The username is : " + username + " and he is authenticated: " + authenticated);
#endif
                foreach (SettingsPropertyValue spv in collection)
                {
                    if (!authenticated && !(bool)spv.Property.Attributes["AllowAnonymous"])
                        continue;

                    if (!spv.IsDirty && spv.UsingDefaultValue)
                        continue;

                    
                    string storeValue = null;

                    if (spv.Deserialized && spv.PropertyValue == null)
                    {
                        pp.setValue(spv.Name, null);
                        continue;
                    }

                    object serialized = spv.SerializedValue;
                    if (serialized == null)
                    {
                        pp.setValue(spv.Name, BIN_SERIALIZATION_PREFIX + BIN_SERIALIZATION_NULL);
                        continue;
                    }
                    
                    if (serialized is string)
                    {
                        storeValue = (string)serialized;
                    }
                    else
                    {
                        string encodedValue = Convert.ToBase64String((byte[])serialized);
                        storeValue = BIN_SERIALIZATION_PREFIX + encodedValue;
                    }
                    
                    pp.setValue(spv.Name, storeValue);

                }
            }
            finally
            {
                pp.store();
            }
        }

        #region Not Implemented Methods

        [MonoTODO]
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion

        #region Helper Methods

        private PortletPreferences PortletPreferences
        {
            get
            {
                PortletRequest pr = PortletUtils.getPortletRequest();
                if (pr == null)
                    return null;
                return pr.getPreferences();
            }
        }

        private bool IsInActionPhase
        {
            get
            {
                PortletRequest pr = PortletUtils.getPortletRequest();
                return pr is ActionRequest;
            }
        }

        #endregion
    }
}

#endif