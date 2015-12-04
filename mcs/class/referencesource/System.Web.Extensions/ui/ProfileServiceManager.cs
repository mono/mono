//------------------------------------------------------------------------------
// <copyright file="ProfileServiceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Web.ApplicationServices;
    using System.Web;
    using System.Web.Profile;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.Configuration;

    [
    DefaultProperty("Path"),
    TypeConverter(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class ProfileServiceManager {
        private string[] _loadProperties;
        private string _path;

        internal static void ConfigureProfileService(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, List<ScriptManagerProxy> proxies) {
            string profileServiceUrl = null;
            ArrayList loadedProperties = null;
            ProfileServiceManager profileManager;

            if(scriptManager.HasProfileServiceManager) {
                profileManager = scriptManager.ProfileService;

                // get ScriptManager.Path
                profileServiceUrl = profileManager.Path.Trim();
                if(profileServiceUrl.Length > 0) {
                    profileServiceUrl = scriptManager.ResolveClientUrl(profileServiceUrl);
                }

                // get ScriptManager.LoadProperties
                if(profileManager.HasLoadProperties) {
                    loadedProperties = new ArrayList(profileManager._loadProperties);
                }
            }

            // combine proxy Paths (find the first one that has specified one)
            // combine loadedProperties collection (take the union of all)
            if(proxies != null) {
                foreach(ScriptManagerProxy proxy in proxies) {
                    if(proxy.HasProfileServiceManager) {
                        profileManager = proxy.ProfileService;

                        // combine urls
                        profileServiceUrl = ApplicationServiceManager.MergeServiceUrls(profileManager.Path, profileServiceUrl, proxy);

                        // combine LoadProperties
                        if(profileManager.HasLoadProperties) {
                            if(loadedProperties == null) {
                                loadedProperties = new ArrayList(profileManager._loadProperties);
                            }
                            else {
                                loadedProperties = ProfileServiceManager.MergeProperties(loadedProperties, profileManager._loadProperties);
                            }
                        }
                    }
                }
            }

            ProfileServiceManager.GenerateInitializationScript(ref sb, context, scriptManager, profileServiceUrl, loadedProperties);
        }

        private static void GenerateInitializationScript(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, string serviceUrl, ArrayList loadedProperties) {
            string defaultServicePath = null;
            bool loadProperties = loadedProperties != null && loadedProperties.Count > 0;

            if (ApplicationServiceHelper.ProfileServiceEnabled) {
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }

                // The default path points to the built-in service (if it is enabled)
                // Note that the client can't default to this path because it doesn't know what the app root is, we must tell it.
                // We must specify the default path to the proxy even if a custom path is provided, because on the client they could
                // reset the path back to the default if they want.
                defaultServicePath = scriptManager.ResolveClientUrl("~/" + System.Web.Script.Services.WebServiceData._profileServiceFileName);
                sb.Append("Sys.Services._ProfileService.DefaultWebServicePath = '");
                sb.Append(HttpUtility.JavaScriptStringEncode(defaultServicePath));
                sb.Append("';\n");
            }

            if (!String.IsNullOrEmpty(serviceUrl)) {
                // DevDiv Bug 72257:When custom path is set and loadProperties=True, we shouldn't use the default path
                // loadProperties script always retrieves the properties from default profile provider, which is not correct if ProfileService
                // points to non default path. Hence throw when non default path and loadProperties both are specified.
                if (defaultServicePath == null){
                    defaultServicePath = scriptManager.ResolveClientUrl("~/" + System.Web.Script.Services.WebServiceData._profileServiceFileName);
                }
                if (loadProperties && !String.Equals(serviceUrl, defaultServicePath, StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException(AtlasWeb.ProfileServiceManager_LoadProperitesWithNonDefaultPath);
                }
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }
                sb.Append("Sys.Services.ProfileService.set_path('");
                sb.Append(HttpUtility.JavaScriptStringEncode(serviceUrl));
                sb.Append("');\n");
            }

            if (loadProperties) {
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }
                if (scriptManager.DesignMode) {
                    // Dev10 757178: context is null at design time, so we cannot access ProfileBase.
                    // But at DesignTime this method is only important because if it produces any init script,
                    // it prompts AddFrameworkScripts to add the MicrosoftAjaxApplicationServices.js script reference. 
                    // So just append a comment to ensure at least some script is generated.
                    sb.Append("// loadProperties\n");
                }
                else if (context != null) {
                    // get values for all properties to be pre-loaded.
                    // GetSettingsProperty puts each property into either the top level settings dictionary or if its part of a group,
                    // it creates an entry for the group in the group collection and puts the setting in the dictionary for the group.
                    SortedList<string, object> topLevelSettings = new SortedList<string, object>(loadedProperties.Count);
                    SortedList<string, SortedList<string, object>> profileGroups = null;

                    ProfileBase profile = context.Profile;
                    foreach(string propertyFullName in loadedProperties) {
                        GetSettingsProperty(profile, propertyFullName, topLevelSettings, ref profileGroups, /* ensure exists */ true);
                    }

                    RenderProfileProperties(sb, topLevelSettings, profileGroups);
                }
            }
        }

        internal static ArrayList MergeProperties(ArrayList existingProperties, string[] newProperties) {
            // 





            foreach(string property in newProperties) {
                if(!String.IsNullOrEmpty(property)) {
                    string trimmedProperty = property.Trim();
                    if((trimmedProperty.Length > 0) && !existingProperties.Contains(trimmedProperty)) {
                        existingProperties.Add(trimmedProperty);
                    }
                }
            }

            return existingProperties;
        }

        internal static void GetSettingsProperty(
            ProfileBase profile,
            string fullPropertyName,
            SortedList<string, object> topLevelSettings,
            ref SortedList<string, SortedList<string, object>> profileGroups,
            bool ensureExists) {
            // Gets a setting off the profile, putting top level settings into the topLevelSettings list,
            // and putting grouped properties into a group-specific list that is contained within a sortedlist of groups.
            // if ensureExists is true and the given property name doesn't exist on the profile, an exception is thrown.

            int dotIndex = fullPropertyName.IndexOf('.');
            string groupName;
            string propertyName;
            SortedList<string, object> containingObject;

            if(dotIndex == -1) {
                groupName = null;
                propertyName = fullPropertyName;
                containingObject = topLevelSettings;
            }
            else {
                groupName = fullPropertyName.Substring(0, dotIndex);
                propertyName = fullPropertyName.Substring(dotIndex + 1);

                if(profileGroups == null) {
                    profileGroups = new SortedList<string, SortedList<string, object>>();
                    containingObject = new SortedList<string, object>();
                    profileGroups.Add(groupName, containingObject);
                }
                else {
                    containingObject = profileGroups[groupName];
                    if(containingObject == null) {
                        containingObject = new SortedList<string, object>();
                        profileGroups.Add(groupName, containingObject);
                    }
                }
            }

            bool exists = ProfileBase.Properties[fullPropertyName] != null;
            if(ensureExists && !exists) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.AppService_UnknownProfileProperty, fullPropertyName));
            }

            if(exists) {
                containingObject[propertyName] = profile == null ? null : profile[fullPropertyName];
            }
        }

        private static void RenderProfileProperties(StringBuilder sb, SortedList<string, object> topLevelSettings, SortedList<string, SortedList<string, object>> profileGroups) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // 1. render top level settings
            sb.Append("Sys.Services.ProfileService.properties = ");
            // 

            sb.Append(serializer.Serialize(topLevelSettings, JavaScriptSerializer.SerializationFormat.JavaScript));
            sb.Append(";\n");

            // 2. render each group as a ProfileGroup object
            //      These could be done as just the value of the PropertyName in topLevelSettings but the serializer wouldn't recognize PropertyGroup.
            if(profileGroups != null) {
                foreach(KeyValuePair<string, SortedList<string, object>> group in profileGroups) {
                    sb.Append("Sys.Services.ProfileService.properties.");
                    sb.Append(group.Key); // group name
                    sb.Append(" = new Sys.Services.ProfileGroup(");
                    sb.Append(serializer.Serialize(group.Value, JavaScriptSerializer.SerializationFormat.JavaScript));
                    sb.Append(");\n");
                }
            }
        }

        internal bool HasLoadProperties {
            get {
                return _loadProperties != null && _loadProperties.Length > 0;
            }
        }

        [
        DefaultValue(null),
        Category("Behavior"),
        NotifyParentProperty(true),
        TypeConverter(typeof(StringArrayConverter)),
        ResourceDescription("ProfileServiceManager_LoadProperties"),
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification="Required by ASP.NET parser.")
        ]
        public string[] LoadProperties {
            get {
                if(_loadProperties == null) {
                    _loadProperties = new string[0];
                }
                return (string[]) _loadProperties.Clone();
            }
            set {
                if(value != null) {
                    value = (string[])value.Clone();
                }
                _loadProperties = value;
            }
        }

        [
        DefaultValue(""),
        Category("Behavior"),
        NotifyParentProperty(true),
        ResourceDescription("ApplicationServiceManager_Path"),
        UrlProperty()
        ]
        public string Path {
            get {
                return _path ?? String.Empty;
	    }
            set {
                _path = value;
            }
        }
    }
}
