//------------------------------------------------------------------------------
// <copyright file="ProfileBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ProfileBase
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web.Profile {
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Security;
    using System.Web.Compilation;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.CodeDom;
    using System.Web.Hosting;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;

    public class ProfileBase : SettingsBase {
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        // Public (instance) functions and properties


        public override object this[string propertyName] {
            get {
                if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                    // VSWhidbey 427541
                    if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                        HttpRuntime.NamedPermissionSet.PermitOnly();
                    }
                }

                return GetInternal(propertyName);
            }
            set {
                if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                    // VSWhidbey 427541
                    if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                        HttpRuntime.NamedPermissionSet.PermitOnly();
                    }
                }

                SetInternal(propertyName, value);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private object GetInternal(string propertyName) {
            return base[propertyName];
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private void SetInternal(string propertyName, object value) {
            if (!_IsAuthenticated) {
                SettingsProperty p = s_Properties[propertyName];

                if (p != null) {
                    bool fAllowAnonymous = (bool)p.Attributes["AllowAnonymous"];
                    if (!fAllowAnonymous)
                        throw new ProviderException(SR.GetString(SR.Profile_anonoymous_not_allowed_to_set_property));
                }
            }

            base[propertyName] = value;
        }

        public object GetPropertyValue(string propertyName) { return this[propertyName]; }

        public void SetPropertyValue(string propertyName, object propertyValue) { this[propertyName] = propertyValue; }

        public ProfileGroupBase GetProfileGroup(string groupName) {
            ProfileGroupBase grp = (ProfileGroupBase)_Groups[groupName];

            if (grp == null) {
                Type t = BuildManager.GetProfileType();
                if (t == null)
                    throw new ProviderException(SR.GetString(SR.Profile_group_not_found, groupName));
                t = t.Assembly.GetType("ProfileGroup" + groupName, false);
                if (t == null)
                    throw new ProviderException(SR.GetString(SR.Profile_group_not_found, groupName));

                grp = (ProfileGroupBase)Activator.CreateInstance(t);
                grp.Init(this, groupName);
            }

            return grp;
        }

        public ProfileBase() {
            if (!ProfileManager.Enabled)
                throw new ProviderException(SR.GetString(SR.Profile_not_enabled));
            if (!s_Initialized)
                InitializeStatic();
        }

        public void Initialize(string username, bool isAuthenticated) {
            if (username != null)
                _UserName = username.Trim();
            else
                _UserName = username;
            //if (string.IsNullOrEmpty(_UserName))
            //    throw new ArgumentException(SR.GetString(SR.Membership_InvalidUserName), "username");
            SettingsContext sc = new SettingsContext();
            sc.Add("UserName", _UserName);
            sc.Add("IsAuthenticated", isAuthenticated);
            _IsAuthenticated = isAuthenticated;
            base.Initialize(sc, s_Properties, ProfileManager.Providers);
        }

        public override void Save() {
            if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                // VSWhidbey 427541
                if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }

            SaveWithAssert();
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private void SaveWithAssert() {
            base.Save();
            _IsDirty = false;
            _DatesRetrieved = false;
        }

        public string UserName { get { return _UserName; } }
        public bool IsAnonymous { get { return !_IsAuthenticated; } }
        public bool IsDirty {
            get {
                if (_IsDirty)
                    return true;
                foreach (SettingsPropertyValue pv in PropertyValues) {
                    if (pv.IsDirty) {
                        _IsDirty = true;
                        return true;
                    }
                }
                return false;
            }
        }
        public DateTime LastActivityDate {
            get {
                if (!_DatesRetrieved)
                    RetrieveDates();
                return _LastActivityDate.ToLocalTime();
            }
        }
        public DateTime LastUpdatedDate {
            get {
                if (!_DatesRetrieved)
                    RetrieveDates();
                return _LastUpdatedDate.ToLocalTime();
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // static public Properties and functions


        static public ProfileBase Create(string username) {
            return Create(username, true);
        }


        static public ProfileBase Create(string username, bool isAuthenticated) {
            if (!ProfileManager.Enabled)
                throw new ProviderException(SR.GetString(SR.Profile_not_enabled));
            InitializeStatic();
            if (s_SingletonInstance != null)
                return s_SingletonInstance;
            if (s_Properties.Count == 0) {
                lock (s_InitializeLock) {
                    if (s_SingletonInstance == null)
                        s_SingletonInstance = new DefaultProfile();
                    return s_SingletonInstance;
                }
            }
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            return CreateMyInstance(username, isAuthenticated);
        }


        new static public SettingsPropertyCollection Properties {
            get {
                InitializeStatic();
                return s_Properties;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // Internal static functions and properties

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        internal static Type InheritsFromType {
            get {
                if (!ProfileManager.Enabled) {
                    return typeof(DefaultProfile);
                }
                Type t;
                if (HostingEnvironment.IsHosted)
                    t = BuildManager.GetType(InheritsFromTypeString, true, true);
                else
                    t = GetPropType(InheritsFromTypeString);

                if (!typeof(ProfileBase).IsAssignableFrom(t)) {
                    ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_profile_base_type), null, config.ElementInformation.Properties["inherits"].Source, config.ElementInformation.Properties["inherit"].LineNumber);
                }
                return t;
            }
        }
        internal static string InheritsFromTypeString {
            get {
                string defaultType = typeof(ProfileBase).ToString();

                if (!ProfileManager.Enabled)
                    return defaultType;
                ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                if (config.Inherits == null)
                    return defaultType;

                string inheritsType = config.Inherits.Trim();
                if (inheritsType.Length < 1)
                    return defaultType;

                Type t = Type.GetType(inheritsType, false, true);
                if (t == null)
                    return inheritsType;
                if (!typeof(ProfileBase).IsAssignableFrom(t))
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_profile_base_type), null, config.ElementInformation.Properties["inherits"].Source, config.ElementInformation.Properties["inherit"].LineNumber);
                return t.AssemblyQualifiedName;
            }
        }

        internal static bool InheritsFromCustomType {
            get {
                if (!ProfileManager.Enabled)
                    return false;
                ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                if (config.Inherits == null)
                    return false;

                string inheritsType = config.Inherits.Trim();

                if (inheritsType == null || inheritsType.Length < 1)
                    return false;

                Type t = Type.GetType(inheritsType, false, true);
                if (t == null || t != typeof(ProfileBase))
                    return true;
                else
                    return false;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        internal static ProfileBase SingletonInstance { get { return s_SingletonInstance; } }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        internal static Hashtable GetPropertiesForCompilation() {
            if (!ProfileManager.Enabled)
                return null;
            if (s_PropertiesForCompilation != null)
                return s_PropertiesForCompilation;

            lock (s_InitializeLock) {
                if (s_PropertiesForCompilation != null)
                    return s_PropertiesForCompilation;
                Hashtable ht = new Hashtable();
                ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                if (config.PropertySettings == null) {
                    s_PropertiesForCompilation = ht;
                    return s_PropertiesForCompilation;
                }
                AddProfilePropertySettingsForCompilation(config.PropertySettings, ht, null);
                foreach (ProfileGroupSettings pgs in config.PropertySettings.GroupSettings) {
                    AddProfilePropertySettingsForCompilation(pgs.PropertySettings, ht, pgs.Name);
                }

                AddProfilePropertySettingsForCompilation(ProfileManager.DynamicProfileProperties, ht, null);

                s_PropertiesForCompilation = ht;
            }
            return s_PropertiesForCompilation;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        internal static string GetProfileClassName() {
            Hashtable props = GetPropertiesForCompilation();
            if (props == null)
                return "System.Web.Profile.DefaultProfile";
            if (props.Count > 0 || InheritsFromCustomType)
                return "ProfileCommon";
            else
                return "System.Web.Profile.DefaultProfile";
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private static void AddProfilePropertySettingsForCompilation(ProfilePropertySettingsCollection propertyCollection, Hashtable ht, string groupName) {
            foreach (ProfilePropertySettings pps in propertyCollection) {
                ProfileNameTypeStruct prop = new ProfileNameTypeStruct();
                if (groupName != null) {
                    prop.Name = groupName + "." + pps.Name;
                }
                else {
                    prop.Name = pps.Name;
                }
                Type t = pps.TypeInternal;
                if (t == null)
                    t = ResolvePropertyTypeForCommonTypes(pps.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture));
                if (t == null)
                    t = BuildManager.GetType(pps.Type, false);
                if (t == null) {
                    prop.PropertyCodeRefType = new CodeTypeReference(pps.Type);
                }
                else {
                    prop.PropertyCodeRefType = new CodeTypeReference(t);
                }
                prop.PropertyType = t;
                pps.TypeInternal = t;
                prop.IsReadOnly = pps.ReadOnly;
                prop.LineNumber = pps.ElementInformation.Properties["name"].LineNumber;
                prop.FileName = pps.ElementInformation.Properties["name"].Source;
                ht.Add(prop.Name, prop);
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // Private static functions and properties
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        static private ProfileBase CreateMyInstance(string username, bool isAuthenticated) {
            Type t;
            if (HostingEnvironment.IsHosted)
                t = BuildManager.GetProfileType();
            else
                t = InheritsFromType;
            ProfileBase hbc = (ProfileBase)Activator.CreateInstance(t);
            hbc.Initialize(username, isAuthenticated);
            return hbc;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        static private void InitializeStatic() {
            if (!ProfileManager.Enabled || s_Initialized) {
                if (s_InitializeException != null)
                    throw s_InitializeException;
                return;
            }
            lock (s_InitializeLock) {
                if (s_Initialized) {
                    if (s_InitializeException != null)
                        throw s_InitializeException;
                    return;
                }

                try {

                    ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                    bool fAnonEnabled = (HostingEnvironment.IsHosted ? AnonymousIdentificationModule.Enabled : true);
                    Type baseType = ProfileBase.InheritsFromType;
                    bool hasLowTrust = HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low);

                    s_Properties = new SettingsPropertyCollection();

                    // Step 0: Add all dynamic profile properties set programatically during PreAppStart
                    ProfileBase.AddPropertySettingsFromConfig(baseType, fAnonEnabled, hasLowTrust, ProfileManager.DynamicProfileProperties, null);

                    //////////////////////////////////////////////////////////////////////
                    // Step 1: Add Properties from the base class (if not ProfileBase)
                    if (baseType != typeof(ProfileBase)) {
                        //////////////////////////////////////////////////////////////////////
                        // Step 2: Construct a hashtable containing a list of all the property-names in the ProfileBase type
                        PropertyInfo[] baseProps = typeof(ProfileBase).GetProperties();
                        NameValueCollection baseProperties = new NameValueCollection(baseProps.Length);
                        foreach (PropertyInfo baseProp in baseProps)
                            baseProperties.Add(baseProp.Name, String.Empty);

                        //////////////////////////////////////////////////////////////////////
                        // Step 3: For each property in the derived class, add it to the s_Properties class.
                        PropertyInfo[] props = baseType.GetProperties();
                        foreach (PropertyInfo prop in props) {
                            if (baseProperties[prop.Name] == null) { //not in the base class

                                ProfileProvider prov = hasLowTrust ? ProfileManager.Provider : null;
                                bool readOnly = false;
                                SettingsSerializeAs serializeAs = SettingsSerializeAs.ProviderSpecific;
                                string defaultValue = String.Empty;
                                bool allowAnonymous = false;
                                string customData = null;

                                //////////////////////////////////////////////////////////////////////
                                // Step 4: For the property, get the attributes
                                Attribute[] attribs = Attribute.GetCustomAttributes(prop, true);

                                foreach (Attribute attrib in attribs) {
                                    if (attrib is SettingsSerializeAsAttribute) {
                                        serializeAs = ((SettingsSerializeAsAttribute)attrib).SerializeAs;
                                    }
                                    else if (attrib is SettingsAllowAnonymousAttribute) {
                                        allowAnonymous = ((SettingsAllowAnonymousAttribute)attrib).Allow;
                                        if (!fAnonEnabled && allowAnonymous)
                                            throw new ConfigurationErrorsException(SR.GetString(SR.Annoymous_id_module_not_enabled, prop.Name), config.ElementInformation.Properties["inherits"].Source, config.ElementInformation.Properties["inherits"].LineNumber);
                                    }
                                    else if (attrib is System.ComponentModel.ReadOnlyAttribute) {
                                        readOnly = ((System.ComponentModel.ReadOnlyAttribute)attrib).IsReadOnly;
                                    }
                                    else if (attrib is DefaultSettingValueAttribute) {
                                        defaultValue = ((DefaultSettingValueAttribute)attrib).Value;
                                    }
                                    else if (attrib is CustomProviderDataAttribute) {
                                        customData = ((CustomProviderDataAttribute)attrib).CustomProviderData;
                                    }
                                    else if (hasLowTrust && attrib is ProfileProviderAttribute) {
                                        prov = ProfileManager.Providers[((ProfileProviderAttribute)attrib).ProviderName];
                                        if (prov == null)
                                            throw new ConfigurationErrorsException(SR.GetString(SR.Profile_provider_not_found, ((ProfileProviderAttribute)attrib).ProviderName), config.ElementInformation.Properties["inherits"].Source, config.ElementInformation.Properties["inherits"].LineNumber);
                                    }
                                }
                                //////////////////////////////////////////////////////////////////////
                                // Step 5: Add the property to the s_Properties
                                SettingsAttributeDictionary settings = new SettingsAttributeDictionary();
                                settings.Add("AllowAnonymous", allowAnonymous);
                                if (!string.IsNullOrEmpty(customData))
                                    settings.Add("CustomProviderData", customData);
                                SettingsProperty sp = new SettingsProperty(prop.Name, prop.PropertyType, prov, readOnly, defaultValue, serializeAs, settings, false, true);
                                s_Properties.Add(sp);
                            }
                        }
                    }

                    //////////////////////////////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////////////
                    // Step 6: Add all properties from config
                    if (config.PropertySettings != null) {
                        AddPropertySettingsFromConfig(baseType, fAnonEnabled, hasLowTrust, config.PropertySettings, null);
                        foreach (ProfileGroupSettings pgs in config.PropertySettings.GroupSettings) {
                            AddPropertySettingsFromConfig(baseType, fAnonEnabled, hasLowTrust, pgs.PropertySettings, pgs.Name);
                        }
                    }
                }
                catch (Exception e) {
                    if (s_InitializeException == null)
                        s_InitializeException = e;
                }
                // If there are no properties, create an empty collection.
                if (s_Properties == null)
                    s_Properties = new SettingsPropertyCollection();
                // Make the properties collection read-only
                s_Properties.SetReadOnly();
                s_Initialized = true;
            }

            // Throw an exception if there was an exception during initialization
            if (s_InitializeException != null)
                throw s_InitializeException;
        }

        private static void AddPropertySettingsFromConfig(Type baseType, bool fAnonEnabled, bool hasLowTrust, ProfilePropertySettingsCollection settingsCollection, string groupName) {
            foreach (ProfilePropertySettings pps in settingsCollection) {
                string name = (groupName != null) ? (groupName + "." + pps.Name) : pps.Name;
                if (baseType != typeof(ProfileBase) && s_Properties[name] != null)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Profile_property_already_added), null, pps.ElementInformation.Properties["name"].Source, pps.ElementInformation.Properties["name"].LineNumber);

                try {
                    if (pps.TypeInternal == null) {
                        pps.TypeInternal = ResolvePropertyType(pps.Type);
                    }
                }
                catch (Exception e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Profile_could_not_create_type, e.Message), e, pps.ElementInformation.Properties["type"].Source, pps.ElementInformation.Properties["type"].LineNumber);
                }
                if (!fAnonEnabled) {
                    bool fAllowAnonymous = pps.AllowAnonymous;
                    if (fAllowAnonymous)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Annoymous_id_module_not_enabled, pps.Name), pps.ElementInformation.Properties["allowAnonymous"].Source, pps.ElementInformation.Properties["allowAnonymous"].LineNumber);
                }
                if (hasLowTrust) {
                    SetProviderForProperty(pps);
                }
                else {
                    pps.ProviderInternal = null;
                }
                // Providers that use NetDataContractSerialzier do not require Serializable attributes any longer, only enforce this for the SqlProfileProvider
                bool requireSerializationCheck = pps.ProviderInternal == null || pps.ProviderInternal.GetType() == typeof(SqlProfileProvider);
                if (requireSerializationCheck && pps.SerializeAs == SerializationMode.Binary && !pps.TypeInternal.IsSerializable) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Property_not_serializable, pps.Name), pps.ElementInformation.Properties["serializeAs"].Source, pps.ElementInformation.Properties["serializeAs"].LineNumber);
                }

                SettingsAttributeDictionary settings = new SettingsAttributeDictionary();
                settings.Add("AllowAnonymous", pps.AllowAnonymous);
                if (!string.IsNullOrEmpty(pps.CustomProviderData))
                    settings.Add("CustomProviderData", pps.CustomProviderData);
                SettingsProperty sp = new SettingsProperty(name, pps.TypeInternal, pps.ProviderInternal, pps.ReadOnly, pps.DefaultValue, (SettingsSerializeAs)pps.SerializeAs, settings, false, true);
                s_Properties.Add(sp);
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        static private void SetProviderForProperty(ProfilePropertySettings pps) {
            if (pps.Provider == null || pps.Provider.Length < 1) {
                pps.ProviderInternal = ProfileManager.Provider; // Use default provider
            }
            else {
                pps.ProviderInternal = ProfileManager.Providers[pps.Provider]; // Use specified provider
            }

            // Provider not found?
            if (pps.ProviderInternal == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.Profile_provider_not_found, pps.Provider), pps.ElementInformation.Properties["provider"].Source, pps.ElementInformation.Properties["provider"].LineNumber);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        static private Type ResolvePropertyTypeForCommonTypes(string typeName) {
            switch (typeName) {
                case "string":
                    return typeof(string);

                case "byte":
                case "int8":
                    return typeof(byte);

                case "boolean":
                case "bool":
                    return typeof(bool);

                case "char":
                    return typeof(char);

                case "int":
                case "integer":
                case "int32":
                    return typeof(int);

                case "date":
                case "datetime":
                    return typeof(DateTime);

                case "decimal":
                    return typeof(decimal);

                case "double":
                case "float64":
                    return typeof(System.Double);

                case "float":
                case "float32":
                    return typeof(float);

                case "long":
                case "int64":
                    return typeof(long);

                case "short":
                case "int16":
                    return typeof(System.Int16);

                case "single":
                    return typeof(Single);

                case "uint16":
                case "ushort":
                    return typeof(UInt16);

                case "uint32":
                case "uint":
                    return typeof(uint);

                case "ulong":
                case "uint64":
                    return typeof(ulong);

                case "object":
                    return typeof(object);

                default:
                    return null;
            }
        }
        static private Type ResolvePropertyType(string typeName) {
            Type t = ResolvePropertyTypeForCommonTypes(typeName.ToLower(System.Globalization.CultureInfo.InvariantCulture));
            if (t != null)
                return t;

            if (HostingEnvironment.IsHosted)
                return BuildManager.GetType(typeName, true, true);
            else
                return GetPropType(typeName);
        }

        static private Type GetPropType(string typeName) {
            return Type.GetType(typeName, true, true);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Instance data
        private Hashtable _Groups = new Hashtable();
        private bool _IsAuthenticated = false;
        private string _UserName = null;
        private bool _IsDirty = false;
        private DateTime _LastActivityDate;
        private DateTime _LastUpdatedDate;
        private bool _DatesRetrieved;

        private void RetrieveDates() {
            if (_DatesRetrieved || ProfileManager.Provider == null)
                return;
            int totalRecords;
            ProfileInfoCollection coll = ProfileManager.Provider.FindProfilesByUserName(ProfileAuthenticationOption.All, _UserName, 0, 1, out totalRecords);
            foreach (ProfileInfo p in coll) {
                _LastActivityDate = p.LastActivityDate.ToUniversalTime();
                _LastUpdatedDate = p.LastUpdatedDate.ToUniversalTime();
                _DatesRetrieved = true;
                return;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Static data
        private static SettingsPropertyCollection s_Properties = null;
        private static object s_InitializeLock = new Object();
        private static Exception s_InitializeException = null;
        private static bool s_Initialized = false;
        private static ProfileBase s_SingletonInstance = null;
        private static Hashtable s_PropertiesForCompilation = null;
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    internal class ProfileNameTypeStruct {
        internal string Name;
        internal CodeTypeReference PropertyCodeRefType;
        internal Type PropertyType;
        internal bool IsReadOnly;
        internal int LineNumber;
        internal string FileName;
    }
}

