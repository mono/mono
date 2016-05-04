//------------------------------------------------------------------------------
// <copyright file="ProfileService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Profile {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Web.ApplicationServices;
    using System.Web.Script.Serialization;
    using System.Web.Script.Services;
    using System.Web.Services;

    [ScriptService]
    internal sealed class ProfileService {
        private static JavaScriptSerializer _javaScriptSerializer;

        private static JavaScriptSerializer JavaScriptSerializer {
            get {
                if (_javaScriptSerializer == null) {
                    HttpContext context = HttpContext.Current;
                    WebServiceData webServiceData = WebServiceData.GetWebServiceData(context, context.Request.FilePath);
                    _javaScriptSerializer = webServiceData.Serializer;
                    
                    Debug.Assert(_javaScriptSerializer != null);
                }
                return _javaScriptSerializer;
            }
        }

        private static Dictionary<string, object> GetProfile(HttpContext context, IEnumerable<string> properties) {
            ProfileBase profile = context.Profile;

            if(profile == null) {
                return null;
            }

            Dictionary<string, object> allowedGet = ApplicationServiceHelper.ProfileAllowedGet;
            if (allowedGet == null || allowedGet.Count == 0) {
                // there are no readable properties
                return new Dictionary<string, object>(0);
            }

            Dictionary<string, object> dictionary = null;

            if(properties == null) {
                // initialize capacity to the exact number we will fill: the number of readAccessProperties
                dictionary = new Dictionary<string, object>(allowedGet.Count, StringComparer.OrdinalIgnoreCase);

                // Returns all profile properties defined in configuration when given properties array is null
                string propertyName;
                foreach(KeyValuePair<string,object> entry in allowedGet) {
                    // note: dont enumerate over _allowedGet.Keys since it unecessarily creates a keys collection object
                    propertyName = entry.Key;
                    dictionary.Add(propertyName, profile[propertyName]);
                }
            }
            else {
                // initialize capacity to the largest possible number of properties we may return.
                dictionary = new Dictionary<string, object>(allowedGet.Count, StringComparer.OrdinalIgnoreCase);

                // Returns the specified profile properties (if empty array, no properties returned)
                foreach(string propertyName in properties) {
                    if(allowedGet.ContainsKey(propertyName)) {
                        dictionary.Add(propertyName, profile[propertyName]);
                    }
                }
            }

            return dictionary;
        }
       
        private static Collection<string> SetProfile(HttpContext context, IDictionary<string, object> values) {
            // return collection of successfully saved settings.
            Collection<string> failedSettings = new Collection<string>();

            if (values == null || values.Count == 0) {
                // no values were given, so we're done, and there are no failures.
                return failedSettings;
            }

            ProfileBase profile = context.Profile;
            Dictionary<string, object> allowedSet = ApplicationServiceHelper.ProfileAllowedSet;
            
            // Note that profile may be null, and allowedSet may be null.
            // Even though no properties will be saved in these cases, we still iterate over the given values to be set,
            // because we must build up the failed collection anyway.
            bool profileDirty = false;
            foreach(KeyValuePair<string, object> entry in values) {
                string propertyName = entry.Key;
                if (profile != null && allowedSet != null && allowedSet.ContainsKey(propertyName)) {
                    SettingsProperty settingProperty = ProfileBase.Properties[propertyName];

                    if (settingProperty != null && !settingProperty.IsReadOnly &&
                        (!profile.IsAnonymous || (bool)settingProperty.Attributes["AllowAnonymous"])) {

                        Type propertyType = settingProperty.PropertyType;
                        object convertedValue;
                        if (ObjectConverter.TryConvertObjectToType(entry.Value, propertyType, JavaScriptSerializer, out convertedValue)) {
                            profile[propertyName] = convertedValue;
                            profileDirty = true;
                            // setting successfully saved.
                            // short circuit the foreach so only failed cases fall through.
                            continue; 
                        }
                    }
                }
                
                // Failed cases fall through to here. Possible failure reasons:
                // 1. type couldn't be converted for some reason (TryConvert returns false)
                // 2. the property doesnt exist (settingProperty == null)
                // 3. the property is read only (settingProperty.IsReadOnly)
                // 4. the current user is anonymous and the setting doesn't allow anonymous access
                // 5. profile for this user is null (profile == null)
                // 6. no properties are allowed for setting (allowedSet is null)
                // 7. *this* property is not allowed for setting (allowedSet.Contains returns false)
                failedSettings.Add(propertyName);
            }

            if (profileDirty) {
                profile.Save();
            }

            return failedSettings;
        }

        [WebMethod]
        public Dictionary<string, object> GetAllPropertiesForCurrentUser(bool authenticatedUserOnly) {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();

            HttpContext context = HttpContext.Current;
            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(context);
            }
            
            return ProfileService.GetProfile(context, null);
        }

        [WebMethod]
        public Dictionary<string, object> GetPropertiesForCurrentUser(IEnumerable<string> properties, bool authenticatedUserOnly) {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();

            HttpContext context = HttpContext.Current;
            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(context);
            }

            return ProfileService.GetProfile(context, properties);
        }

        [WebMethod]
        public Collection<ProfilePropertyMetadata> GetPropertiesMetadata() {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();
            return ApplicationServiceHelper.GetProfilePropertiesMetadata();
        }

        [WebMethod]
        public Collection<string> SetPropertiesForCurrentUser(IDictionary<string, object> values, bool authenticatedUserOnly) {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();

            HttpContext context = HttpContext.Current;
            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(context);
            }

            return ProfileService.SetProfile(context, values);
        }
    }
}
