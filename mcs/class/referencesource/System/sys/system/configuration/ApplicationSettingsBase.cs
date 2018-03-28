//------------------------------------------------------------------------------
// <copyright file="ApplicationSettingsBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

// These aren't valid violations - caused by HostProtectionAttribute.
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.Configuration.ApplicationSettingsBase.add_PropertyChanged(System.ComponentModel.PropertyChangedEventHandler):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.Configuration.ApplicationSettingsBase.remove_PropertyChanged(System.ComponentModel.PropertyChangedEventHandler):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.ComponentModel.TypeDescriptor.GetConverter(System.Type):System.ComponentModel.TypeConverter")]


// Reviewed and found to be safe.
[assembly: SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.Configuration.LocalFileSettingsProvider..ctor()")]

namespace System.Configuration {

    using System.ComponentModel;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    /// <devdoc>
    ///     Base settings class for client applications.
    /// </devdoc>
    public abstract class ApplicationSettingsBase : SettingsBase, INotifyPropertyChanged {
        private bool                            _explicitSerializeOnClass = false;
        private object[]                        _classAttributes;
        private IComponent                      _owner;
        private PropertyChangedEventHandler     _onPropertyChanged;
        private SettingsContext                 _context;
        private SettingsProperty                _init;
        private SettingsPropertyCollection      _settings;
        private SettingsProviderCollection      _providers;
        private SettingChangingEventHandler     _onSettingChanging;
        private SettingsLoadedEventHandler      _onSettingsLoaded;
        private SettingsSavingEventHandler      _onSettingsSaving;
        private string                          _settingsKey = String.Empty;
        private bool                            _firstLoad = true;
        private bool                            _initialized = false;
        
        /// <devdoc>
        ///     Default constructor without a concept of "owner" component.
        /// </devdoc>
        protected ApplicationSettingsBase() : base() {
        }

        /// <devdoc>
        ///     Constructor that takes an IComponent. The IComponent acts as the "owner" of this settings class. One
        ///     of the things we do is query the component's site to see if it has a SettingsProvider service. If it 
        ///     does, we allow it to override the providers specified in the metadata.
        /// </devdoc>
        protected ApplicationSettingsBase(IComponent owner) : this(owner, String.Empty) {
        }

        /// <devdoc>
        ///     Convenience overload that takes the settings key
        /// </devdoc>
        protected ApplicationSettingsBase(string settingsKey) {
            _settingsKey = settingsKey;
        }

        /// <devdoc>
        ///     Convenience overload that takes the owner component and settings key.
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ApplicationSettingsBase(IComponent owner, string settingsKey) : this(settingsKey) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            _owner = owner;

            if (owner.Site != null) {
                ISettingsProviderService provSvc = owner.Site.GetService(typeof(ISettingsProviderService)) as ISettingsProviderService;
                if (provSvc != null) {
                    // The component's site has a settings provider service. We pass each SettingsProperty to it
                    // to see if it wants to override the current provider.
                    foreach (SettingsProperty sp in Properties) {
                        SettingsProvider prov = provSvc.GetSettingsProvider(sp);
                        if (prov != null) {
                            sp.Provider = prov;
                        }
                    }

                    ResetProviders();
                }
            }
        }
        
        /// <devdoc>
        ///     The Context to pass on to the provider. Currently, this will just contain the settings group name.
        /// </devdoc>
        [Browsable(false)]
        public override SettingsContext Context {
            get {
                if (_context == null) {
                    if (IsSynchronized) {
                        lock (this) {
                            if (_context == null) {
                                _context = new SettingsContext();
                                EnsureInitialized();
                            }
                        }
                    }
                    else {
                        _context = new SettingsContext();
                        EnsureInitialized();
                    }
                    
                }

                return _context;
            }
        }
        
        /// <devdoc>
        ///     The SettingsBase class queries this to get the collection of SettingsProperty objects. We reflect over 
        ///     the properties defined on the current object's type and use the metadata on those properties to form 
        ///     this collection.
        /// </devdoc>
        [Browsable(false)]
        public override SettingsPropertyCollection Properties {
            get {
                if (_settings == null) {
                    if (IsSynchronized) {
                        lock (this) {
                            if (_settings == null) {
                                _settings = new SettingsPropertyCollection();
                                EnsureInitialized();
                            }
                        }
                    }
                    else {
                        _settings = new SettingsPropertyCollection();
                        EnsureInitialized();
                    }
                    
                }

                return _settings;
            }
        }

        /// <devdoc>
        ///     Just overriding to add attributes.
        /// </devdoc>
        [Browsable(false)]
        public override SettingsPropertyValueCollection PropertyValues {
            get {
                return base.PropertyValues;
            }
        }

        /// <devdoc>
        ///     Provider collection
        /// </devdoc>
        [Browsable(false)]
        public override SettingsProviderCollection Providers {
            get {
                if (_providers == null) {
                    if (IsSynchronized) {
                        lock (this) {
                            if (_providers == null) {
                                _providers = new SettingsProviderCollection();
                                EnsureInitialized();
                            }
                        }
                    }
                    else {
                        _providers = new SettingsProviderCollection();
                        EnsureInitialized();
                    }
                }

                return _providers;
            }
        }

        /// <devdoc>
        ///     Derived classes should use this to uniquely identify separate instances of settings classes.
        /// </devdoc>
        [Browsable(false)]
        public string SettingsKey {
            get {
                return _settingsKey;
            }
            set {
                _settingsKey = value;
                Context["SettingsKey"] = _settingsKey;
            }
        }

        /// <devdoc>
        ///     Fires when the value of a setting is changed. (INotifyPropertyChanged implementation.)
        /// </devdoc>
        public event PropertyChangedEventHandler PropertyChanged {
            add {
                _onPropertyChanged += value;
            }
            remove {
                _onPropertyChanged -= value;
            }

        }

        /// <devdoc>
        ///     Fires when the value of a setting is about to change. This is a cancellable event.
        /// </devdoc>
        public event SettingChangingEventHandler SettingChanging {
            add {
                _onSettingChanging += value;
            }
            remove {
                _onSettingChanging -= value;
            }
        }

        /// <devdoc>
        ///     Fires when settings are retrieved from a provider. It fires once for each provider.
        /// </devdoc>
        public event SettingsLoadedEventHandler SettingsLoaded {
            add {
                _onSettingsLoaded += value;
            }
            remove {
                _onSettingsLoaded -= value;
            }
        }

        /// <devdoc>
        ///     Fires when Save() is called. This is a cancellable event.
        /// </devdoc>
        public event SettingsSavingEventHandler SettingsSaving {
            add {
                _onSettingsSaving += value;
            }
            remove {
                _onSettingsSaving -= value;
            }
        }

        /// <devdoc>
        ///     Used in conjunction with Upgrade - retrieves the previous value of a setting from the provider.
        ///     Provider must implement IApplicationSettingsProvider to support this.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public object GetPreviousVersion(string propertyName) {
           if (Properties.Count == 0)
               throw new SettingsPropertyNotFoundException();

           SettingsProperty sp = Properties[propertyName];
           SettingsPropertyValue value = null;

           if (sp == null)
               throw new SettingsPropertyNotFoundException();

           IApplicationSettingsProvider clientProv = sp.Provider as IApplicationSettingsProvider;
           
           if (clientProv != null) {
               value = clientProv.GetPreviousVersion(Context, sp);
           }

           if (value != null) {
               return value.PropertyValue;
           }

           return null;
        }

        /// <devdoc>
        ///     Fires the PropertyChanged event.
        /// </devdoc>
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(_onPropertyChanged != null) {
                _onPropertyChanged(this, e); 
            }
        }

        /// <devdoc>
        ///     Fires the SettingChanging event.
        /// </devdoc>
        protected virtual void OnSettingChanging(object sender, SettingChangingEventArgs e) {
            if(_onSettingChanging != null) {
                _onSettingChanging(this, e); 
            }
        }

        /// <devdoc>
        ///     Fires the SettingsLoaded event.
        /// </devdoc>
        protected virtual void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e) {
            if(_onSettingsLoaded != null) {
                _onSettingsLoaded(this, e);
            }
        }

        /// <devdoc>
        ///     Fires the SettingsSaving event.
        /// </devdoc>
        protected virtual void OnSettingsSaving(object sender, CancelEventArgs e) {
            if(_onSettingsSaving != null) {
                _onSettingsSaving(this, e); 
            }
        }

        /// <devdoc>
        ///     Causes a reload to happen on next setting access, by clearing the cached values.
        /// </devdoc>
        public void Reload() {
            if (PropertyValues != null) {
                PropertyValues.Clear();
            }

            foreach (SettingsProperty sp in Properties) {
                PropertyChangedEventArgs pe = new PropertyChangedEventArgs(sp.Name);
                OnPropertyChanged(this, pe);
            }
        }

        /// <devdoc>
        ///     Calls Reset on the providers.
        ///     Providers must implement IApplicationSettingsProvider to support this.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Reset() {
            if (Properties != null) {
                foreach(SettingsProvider provider in Providers) {
                    IApplicationSettingsProvider clientProv = provider as IApplicationSettingsProvider;
                    if (clientProv != null) {
                        clientProv.Reset(Context);
                    }
                }
            }

            Reload();
        }

        /// <devdoc>
        ///     Overriden from SettingsBase to support validation event.
        /// </devdoc>
        public override void Save() {
            CancelEventArgs e= new CancelEventArgs(false);
            OnSettingsSaving(this, e);

            if (!e.Cancel) {
                base.Save();
            }
        }

        /// <devdoc>
        ///     Overriden from SettingsBase to support validation event.
        /// </devdoc>
        public override object this[string propertyName] {
            get {
                if (IsSynchronized) {
                    lock (this) {
                        return GetPropertyValue(propertyName);
                    }
                }
                else {
                    return GetPropertyValue(propertyName);
                }
                
            }
            set {
                SettingChangingEventArgs e = new SettingChangingEventArgs(propertyName, this.GetType().FullName, SettingsKey, value, false);
                OnSettingChanging(this, e);
        
                if (!e.Cancel) {
                    base[propertyName] = value;
                    //
                    PropertyChangedEventArgs pe = new PropertyChangedEventArgs(propertyName);
                    OnPropertyChanged(this, pe);
                }
            }
        }

        /// <devdoc>
        ///     Called when the app is upgraded so that we can instruct the providers to upgrade their settings.
        ///     Providers must implement IApplicationSettingsProvider to support this.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public virtual void Upgrade() {
            if (Properties != null) {
                foreach(SettingsProvider provider in Providers) {
                    IApplicationSettingsProvider clientProv = provider as IApplicationSettingsProvider;
                    if (clientProv != null) {
                        clientProv.Upgrade(Context, GetPropertiesForProvider(provider));
                    }
                }
            }

            Reload();
        }

        /// <devdoc>
        ///     Creates a SettingsProperty object using the metadata on the given property 
        ///     and returns it.
        ///
        ///     Implementation note: Initialization method - be careful not to access properties here
        ///                          to prevent stack overflow.
        /// </devdoc>
        private SettingsProperty CreateSetting(PropertyInfo propInfo) {
            object[] attributes = propInfo.GetCustomAttributes(false); 
            SettingsProperty sp = new SettingsProperty(Initializer);
            bool explicitSerialize = _explicitSerializeOnClass;

            sp.Name = propInfo.Name; 
            sp.PropertyType = propInfo.PropertyType;
            
            for (int i = 0; i < attributes.Length; i ++) {
                Attribute attr = attributes[i] as Attribute;
                if (attr != null) {
                    if (attr is DefaultSettingValueAttribute) {
                        sp.DefaultValue = ((DefaultSettingValueAttribute)attr).Value;
                    }
                    else if (attr is ReadOnlyAttribute) {
                        sp.IsReadOnly = true;
                    }
                    else if (attr is SettingsProviderAttribute) {
                        string providerTypeName = ((SettingsProviderAttribute)attr).ProviderTypeName;
                        Type providerType = Type.GetType(providerTypeName);
                        if (providerType != null) {
                            SettingsProvider spdr = SecurityUtils.SecureCreateInstance(providerType) as SettingsProvider;

                            if (spdr != null) {
                                spdr.Initialize(null, null);
                                spdr.ApplicationName = ConfigurationManagerInternalFactory.Instance.ExeProductName;

                                // See if we already have a provider of the same name in our collection. If so,
                                // re-use the existing instance, since we cannot have multiple providers of the same name.
                                SettingsProvider existing = _providers[spdr.Name];
                                if (existing != null) {
                                    spdr = existing;
                                }
                                
                                sp.Provider = spdr;
                            }
                            else {
                                throw new ConfigurationErrorsException(SR.GetString(SR.ProviderInstantiationFailed, providerTypeName));
                            }
                        }
                        else {
                            throw new ConfigurationErrorsException(SR.GetString(SR.ProviderTypeLoadFailed, providerTypeName));
                        }
                    }
                    else if (attr is SettingsSerializeAsAttribute) {
                        sp.SerializeAs = ((SettingsSerializeAsAttribute)attr).SerializeAs;
                        explicitSerialize = true;
                    }
                    else {
                        // This isn't an attribute we care about, so simply pass it on
                        // to the SettingsProvider.
                        // NOTE: The key is the type. So if an attribute was found at class
                        //       level and also property level, the latter overrides the former
                        //       for a given setting. This is exactly the behavior we want.
                        
                        sp.Attributes.Add(attr.GetType(), attr);
                    }
                }
            }

            if (!explicitSerialize) {
                sp.SerializeAs = GetSerializeAs(propInfo.PropertyType);
            }

            return sp;

        }

        /// <devdoc>
        ///     Ensures this class is initialized. Initialization involves reflecting over properties and building
        ///     a list of SettingsProperty's.
        /// 
        ///     Implementation note: Initialization method - be careful not to access properties here
        ///                          to prevent stack overflow.
        /// </devdoc>
        private void EnsureInitialized() {
            if (!_initialized) {
                _initialized = true;

                Type type = this.GetType();
                
                if (_context == null) {
                    _context = new SettingsContext();
                }
                _context["GroupName"] = type.FullName;
                _context["SettingsKey"] = SettingsKey;
                _context["SettingsClassType"] = type; 

                PropertyInfo[] properties = SettingsFilter(type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
                _classAttributes = type.GetCustomAttributes(false); 

                if (_settings == null) {
                    _settings = new SettingsPropertyCollection();
                }

                if (_providers == null) {
                    _providers = new SettingsProviderCollection();
                }
                
                for (int i = 0; i < properties.Length; i++) {
                    SettingsProperty sp = CreateSetting(properties[i]);
                    if (sp != null) {
                        _settings.Add(sp);

                        if (sp.Provider != null && _providers[sp.Provider.Name] == null) {
                            _providers.Add(sp.Provider);
                        }
                    }
                }
            }
        }

        /// <devdoc>
        ///     Returns a SettingsProperty used to initialize settings. We initialize a setting with values
        ///     derived from class level attributes, if present. Otherwise, we initialize to
        ///     reasonable defaults.
        ///
        ///     Implementation note: Initialization method - be careful not to access properties here
        ///                          to prevent stack overflow.
        /// </devdoc>
        private SettingsProperty Initializer {
            get {
                if (_init == null) {
                    _init = new SettingsProperty("");
                    _init.DefaultValue = null;
                    _init.IsReadOnly = false;
                    _init.PropertyType = null;

                    SettingsProvider provider = new LocalFileSettingsProvider();
                    
                    if (_classAttributes != null) {
                        for (int i = 0; i < _classAttributes.Length; i ++) {
                            Attribute attr = _classAttributes[i] as Attribute;
                            if (attr != null) {
                                if (attr is ReadOnlyAttribute) {
                                    _init.IsReadOnly = true;
                                }
                                else if (attr is SettingsGroupNameAttribute) {
                                    if (_context == null) {
                                        _context = new SettingsContext();
                                    }
                                    _context["GroupName"] = ((SettingsGroupNameAttribute)attr).GroupName;
                                }
                                else if (attr is SettingsProviderAttribute) {
                                    string providerTypeName = ((SettingsProviderAttribute)attr).ProviderTypeName;
                                    Type providerType = Type.GetType(providerTypeName);
                                    if (providerType != null) {
                                        SettingsProvider spdr = SecurityUtils.SecureCreateInstance(providerType) as SettingsProvider;
                                        if (spdr != null) {
                                            provider = spdr;
                                        }
                                        else {
                                            throw new ConfigurationErrorsException(SR.GetString(SR.ProviderInstantiationFailed, providerTypeName));
                                        }
                                    }
                                    else {
                                        throw new ConfigurationErrorsException(SR.GetString(SR.ProviderTypeLoadFailed, providerTypeName));
                                    }
                                }
                                else if (attr is SettingsSerializeAsAttribute) {
                                    _init.SerializeAs = ((SettingsSerializeAsAttribute)attr).SerializeAs;
                                    _explicitSerializeOnClass = true;
                                }
                                else {
                                    // This isn't an attribute we care about, so simply pass it on
                                    // to the SettingsProvider.
                                    // NOTE: The key is the type. So if an attribute was found at class
                                    //       level and also property level, the latter overrides the former
                                    //       for a given setting. This is exactly the behavior we want.
                                    _init.Attributes.Add(attr.GetType(), attr);
                                }
                            }
                        }
                    }

                    //Initialize the SettingsProvider
                    provider.Initialize(null, null);
                    provider.ApplicationName = ConfigurationManagerInternalFactory.Instance.ExeProductName;
                    _init.Provider = provider;

                }

                return _init;
            }
        }

        /// <devdoc>
        ///     Gets all the settings properties for this provider.
        /// </devdoc>
        private SettingsPropertyCollection GetPropertiesForProvider(SettingsProvider provider) {
           SettingsPropertyCollection properties = new SettingsPropertyCollection();
           foreach (SettingsProperty sp in Properties) {
               if (sp.Provider == provider) {
                   properties.Add(sp);
               }
           }

           return properties;
        }

        /// <devdoc>
        ///     Retrieves the value of a setting. We need this method so we can fire the SettingsLoaded event 
        ///     when settings are loaded from the providers.Ideally, this should be fired from SettingsBase, 
        ///     but unfortunately that will not happen in Whidbey. Instead, we check to see if the value has already 
        ///     been retrieved. If not, we fire the load event, since we expect SettingsBase to load all the settings 
        ///     from this setting's provider.
        /// </devdoc>
        private object GetPropertyValue(string propertyName) {
            if (PropertyValues[propertyName] == null) {

                // If this is our first load and we are part of a Clickonce app, call Upgrade.
                if (_firstLoad) {
                    _firstLoad = false;

                    if (IsFirstRunOfClickOnceApp()) {
                        Upgrade();
                    }
                }

                object temp = base[propertyName];
                SettingsProperty setting = Properties[propertyName];
                SettingsProvider provider = setting != null ? setting.Provider : null;

                Debug.Assert(provider != null, "Could not determine provider from which settings were loaded");

                SettingsLoadedEventArgs e = new SettingsLoadedEventArgs(provider);
                OnSettingsLoaded(this, e);

                // Note: we need to requery the value here in case someone changed it while
                // handling SettingsLoaded.
                return base[propertyName];
            }
            else {
                return base[propertyName];
            }
        }

        /// <devdoc>
        ///     When no explicit SerializeAs attribute is provided, this routine helps to decide how to
        ///     serialize. 
        /// </devdoc>
        private SettingsSerializeAs GetSerializeAs(Type type) {
            //First check whether this type has a TypeConverter that can convert to/from string
            //If so, that's our first choice
            TypeConverter tc = TypeDescriptor.GetConverter(type);
            bool toString = tc.CanConvertTo(typeof(string));
            bool fromString = tc.CanConvertFrom(typeof(string));
            if (toString && fromString) {
                return SettingsSerializeAs.String;
            }

            //Else fallback to Xml Serialization 
            return SettingsSerializeAs.Xml;
        }

        /// <devdoc>
        ///     Returns true if this is a clickonce deployed app and this is the first run of the app
        ///     since deployment or last upgrade.
        /// </devdoc>
        private bool IsFirstRunOfClickOnceApp() {
            // NOTE: For perf & servicing reasons, we don't want to introduce a dependency on
            //       System.Deployment.dll here. The following code is an alternative to calling
            //       ApplicationDeployment.CurrentDeployment.IsFirstRun.
         
            // First check if the app is ClickOnce deployed
            ActivationContext actCtx = AppDomain.CurrentDomain.ActivationContext;
            
            if (IsClickOnceDeployed(AppDomain.CurrentDomain)) {
                // Now check if this is the first run since deployment or last upgrade
                return System.Deployment.Internal.InternalActivationContextHelper.IsFirstRun(actCtx);
            }

            return false;
        }

        /// <devdoc>
        ///     Returns true if this is a clickonce deployed app.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static bool IsClickOnceDeployed(AppDomain appDomain) {
            // NOTE: For perf & servicing reasons, we don't want to introduce a dependency on
            //       System.Deployment.dll here. The following code is an alternative to calling
            //       ApplicationDeployment.IsNetworkDeployed.
            // Security Note: This is also why we need the security assert above.

            ActivationContext actCtx = appDomain.ActivationContext;

            // Ensures the app is running with a context from the store.
            if (actCtx != null && actCtx.Form == ActivationContext.ContextForm.StoreBounded) {
                string fullAppId = actCtx.Identity.FullName;
                if (!String.IsNullOrEmpty(fullAppId)) {
                    return true;
                }
            }

            return false;
        }

        /// <devdoc>
        ///     Only those settings class properties that have a SettingAttribute on them are 
        ///     treated as settings. This routine filters out other properties.
        /// </devdoc>
        private PropertyInfo[] SettingsFilter(PropertyInfo[] allProps) {
            ArrayList settingProps = new ArrayList();
            object[] attributes;
            Attribute attr;
            
            for (int i = 0; i < allProps.Length; i ++) {
                attributes = allProps[i].GetCustomAttributes(false); 
                for (int j = 0; j < attributes.Length; j ++) {
                    attr = attributes[j] as Attribute;
                    if (attr is SettingAttribute) {
                        settingProps.Add(allProps[i]);
                        break;
                    }
                }
            }

            return (PropertyInfo[]) settingProps.ToArray(typeof(PropertyInfo));
        }

        /// <devdoc>
        ///     Resets the provider collection. This needs to be called when providers change after
        ///     first being set. 
        /// </devdoc>
        private void ResetProviders() {
            Providers.Clear();

            foreach (SettingsProperty sp in Properties) {
                if (Providers[sp.Provider.Name] == null) {
                    Providers.Add(sp.Provider);
                }
            }
        }
    }

    /// <devdoc>
    ///     Event handler for the SettingsLoaded event.
    /// </devdoc>
    public delegate void SettingsLoadedEventHandler(object sender, SettingsLoadedEventArgs e);

    /// <devdoc>
    ///     Event handler for the SettingsSaving event.
    /// </devdoc>
    public delegate void SettingsSavingEventHandler(object sender, CancelEventArgs e);

    /// <devdoc>
    ///     Event handler for the SettingChanging event.
    /// </devdoc>
    public delegate void SettingChangingEventHandler(object sender, SettingChangingEventArgs e);

    /// <devdoc>
    ///     Event args for the SettingChanging event.
    /// </devdoc>
    public class SettingChangingEventArgs : CancelEventArgs {

        private string _settingClass;
        private string _settingName;
        private string _settingKey;
        private object _newValue;

        public SettingChangingEventArgs(string settingName, string settingClass, string settingKey, object newValue, bool cancel) : base(cancel) {
            _settingName = settingName;
            _settingClass = settingClass;
            _settingKey = settingKey;
            _newValue = newValue;
        }

        public object NewValue {
            get {
                return _newValue;
            }
        }

        public string SettingClass {
            get {
                return _settingClass;
            }
        }

        public string SettingName {
            get {
                return _settingName;
            }
        }

        public string SettingKey {
            get {
                return _settingKey;
            }
        }
    }

    /// <devdoc>
    ///     Event args for the SettingLoaded event.
    /// </devdoc>
    public class SettingsLoadedEventArgs : EventArgs {

        private SettingsProvider _provider;
        
        public SettingsLoadedEventArgs(SettingsProvider provider) {
            _provider = provider;
        }

        public SettingsProvider Provider {
            get {
                return _provider;
            }
        }
    }
}
