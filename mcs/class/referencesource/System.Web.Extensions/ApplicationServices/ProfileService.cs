//------------------------------------------------------------------------------
// <copyright file="ProfileService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.ApplicationServices {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web;
    using System.Web.Management;
    using System.Web.Profile;
    using System.Web.Resources;

    [ServiceContract(Namespace="http://asp.net/ApplicationServices/v200")]
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypesProvider))]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(Namespace="http://asp.net/ApplicationServices/v200", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ProfileService {


        //////////////////////////////////////////////////////////////////////
        /// <devdoc>
        ///    Raised to allow developers to validate property values being set by the client
        /// </devdoc>
        private static object _validatingPropertiesEventHandlerLock = new object();
        private static EventHandler<ValidatingPropertiesEventArgs> _validatingProperties;
        public static event EventHandler<ValidatingPropertiesEventArgs> ValidatingProperties {
            add {
                lock (_validatingPropertiesEventHandlerLock) {
                    _validatingProperties += value;
                }
            }
            remove {
                lock (_validatingPropertiesEventHandlerLock) {
                    _validatingProperties -= value;
                }
            }
        }

        /// <devdoc>
        ///    Raises the ValidatingPropertiesEvent if atleast one handler is assigned.
        /// </devdoc>
        private void OnValidatingProperties(ValidatingPropertiesEventArgs e) {
            EventHandler<ValidatingPropertiesEventArgs> handler = _validatingProperties;
            if (null != handler) {
                handler(this, e);
            }
        }

        public ProfileService() { 
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        [OperationContract]
        public Dictionary<string, object> GetPropertiesForCurrentUser(IEnumerable<string> properties, bool authenticatedUserOnly) {

            if (properties == null) {
                throw new ArgumentNullException("properties");
            }

            ApplicationServiceHelper.EnsureProfileServiceEnabled();
            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(HttpContext.Current);
            }


            Dictionary<string, object> retDict = new Dictionary<string, object>();
            ProfileBase pb = null;
            try {
                pb = GetProfileForCurrentUser(authenticatedUserOnly);
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
            if (pb == null) {
                return null;
            }

            Dictionary<string, object> allowedGet = ApplicationServiceHelper.ProfileAllowedGet;
            if (allowedGet == null || allowedGet.Count == 0) {
                // there are no readable properties
                return retDict;
            }
            foreach (string property in properties) {
                if (property == null) {
                    throw new ArgumentNullException("properties");
                }

                if (allowedGet.ContainsKey(property)) {
                    try {
                        SettingsPropertyValue value = GetPropertyValue(pb, property);
                        if (value != null) {
                            retDict.Add(property, value.PropertyValue);
                            value.IsDirty = false;
                        }
                    }
                    catch (Exception e) {
                        LogException(e);
                        throw;
                    }
                }
            }
            return retDict;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        [OperationContract]
        public Dictionary<string, object> GetAllPropertiesForCurrentUser(bool authenticatedUserOnly) {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();
            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(HttpContext.Current);
            }

            Dictionary<string, object> retDict = new Dictionary<string, object>();

            try {
                ProfileBase pb = GetProfileForCurrentUser(authenticatedUserOnly);
                if (pb == null) {
                    return null;
                }

                Dictionary<string, object> allowedGet = ApplicationServiceHelper.ProfileAllowedGet;
                if (allowedGet == null || allowedGet.Count == 0) {
                    // there are no readable properties
                    return retDict;
                }
                foreach (KeyValuePair<string, object> entry in allowedGet) {
                    string propertyName = entry.Key;
                    SettingsPropertyValue value = GetPropertyValue(pb, propertyName);
                    if (value != null) {
                        retDict.Add(propertyName, value.PropertyValue);
                        value.IsDirty = false;
                    }
                }
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
            return retDict;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        [OperationContract]
        public Collection<string> SetPropertiesForCurrentUser(IDictionary<string, object> values, bool authenticatedUserOnly) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            ApplicationServiceHelper.EnsureProfileServiceEnabled();

            if (authenticatedUserOnly) {
                ApplicationServiceHelper.EnsureAuthenticated(HttpContext.Current);
            }

            Collection<string> sc = new Collection<string>();
            try {
                ValidatingPropertiesEventArgs vp = new ValidatingPropertiesEventArgs(values);
                OnValidatingProperties(vp);

                Dictionary<string, object> allowedSet = ApplicationServiceHelper.ProfileAllowedSet;
                ProfileBase pb = GetProfileForCurrentUser(authenticatedUserOnly);
                foreach (KeyValuePair<string, object> kvp in values) {
                    string propertyName = kvp.Key;

                    if (pb == null) {
                        sc.Add(propertyName);
                        continue;
                    }
                    if (vp.FailedProperties.Contains(propertyName)) {
                        sc.Add(propertyName);
                        continue;
                    }
                    if (allowedSet == null) {
                        sc.Add(propertyName);
                        continue;
                    }
                    if (!allowedSet.ContainsKey(propertyName)) {
                        sc.Add(propertyName);
                        continue;
                    }
                    
                    SettingsProperty settingProperty = ProfileBase.Properties[propertyName];
                    if (settingProperty == null) {
                        // property not found 
                        sc.Add(propertyName);
                        continue;
                    }
                    if (settingProperty.IsReadOnly || (pb.IsAnonymous && !(bool)settingProperty.Attributes["AllowAnonymous"])) {
                        // property is readonly, or the profile is anonymous and the property isn't enabled for anonymous access
                        sc.Add(propertyName);
                        continue;
                    }

                    SettingsPropertyValue value = GetPropertyValue(pb, kvp.Key);
                    if (value == null) { // property not found 
                        sc.Add(propertyName);
                        continue;
                    }
                    else {
                        try {
                            pb[propertyName] = kvp.Value;
                        }
                        catch (System.Configuration.Provider.ProviderException) {
                            // provider specific error
                            sc.Add(propertyName);
                        }
                        catch (System.Configuration.SettingsPropertyNotFoundException) {
                            sc.Add(propertyName);
                        }
                        catch (System.Configuration.SettingsPropertyWrongTypeException) {
                            sc.Add(propertyName);
                        }
                    }
                }
                pb.Save();
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
            return sc;
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        [OperationContract]
        public ProfilePropertyMetadata[] GetPropertiesMetadata() {
            ApplicationServiceHelper.EnsureProfileServiceEnabled();

            try {
                // todo: convert to array is temporary -- this method should just return Collection<> like the other profileservice does.

                Collection<ProfilePropertyMetadata> metadatas = ApplicationServiceHelper.GetProfilePropertiesMetadata();
                ProfilePropertyMetadata[] metadatasArray = new ProfilePropertyMetadata[metadatas.Count];
                metadatas.CopyTo(metadatasArray, 0);
                
                return metadatasArray;
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private static SettingsPropertyValue GetPropertyValue(ProfileBase pb, string name) {
            SettingsProperty prop = ProfileBase.Properties[name];
            if (prop == null) {
                return null;
            }

            SettingsPropertyValue p = pb.PropertyValues[name];
            if (p == null) {
                // not fetched from provider
                pb.GetPropertyValue(name); // to force a fetch from the provider
                p = pb.PropertyValues[name];
                if (p == null) {
                    return null;
                }
            }
            return p;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private static ProfileBase GetProfileForCurrentUser(bool authenticatedUserOnly) {
            HttpContext context = HttpContext.Current;
            IPrincipal user = ApplicationServiceHelper.GetCurrentUser(context);
            string name = null;
            bool isAuthenticated = false;

            if (user == null || user.Identity == null || string.IsNullOrEmpty(user.Identity.Name)) { // anonymous user?
                isAuthenticated = false;

                if (!authenticatedUserOnly && context != null && !string.IsNullOrEmpty(context.Request.AnonymousID)) { // Use Anonymous ID?
                    name = context.Request.AnonymousID;
                }

            }
            else {
                name = user.Identity.Name;
                isAuthenticated = user.Identity.IsAuthenticated;
            }

            if (!isAuthenticated && (authenticatedUserOnly || string.IsNullOrEmpty(name))) {
                if (context != null)
                    throw new HttpException(AtlasWeb.UserIsNotAuthenticated);
                else
                    throw new Exception(AtlasWeb.UserIsNotAuthenticated);
            }

            return ProfileBase.Create(name, isAuthenticated);
        }

        private void LogException(Exception e) {
            WebServiceErrorEvent errorevent = new WebServiceErrorEvent(AtlasWeb.UnhandledExceptionEventLogMessage, this, e);
            errorevent.Raise();
        }
    }
}
