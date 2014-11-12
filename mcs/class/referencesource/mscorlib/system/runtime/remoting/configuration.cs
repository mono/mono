// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    Configuration.cs
**
**
** Purpose: Classes used for reading and storing configuration
**
**
===========================================================*/
namespace System.Runtime.Remoting {

    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;    
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.Reflection;
    using System.Globalization;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;
    
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum WellKnownObjectMode
    {
        Singleton   = 1,
        SingleCall  = 2
    }

    // This is the class that plays the role of per-appDomain statics
    // till we have the real functionality.
    internal class DomainSpecificRemotingData
    {
        const int  ACTIVATION_INITIALIZING  = 0x00000001;
        const int  ACTIVATION_INITIALIZED   = 0x00000002;        
        const int  ACTIVATOR_LISTENING      = 0x00000004;
        
        [System.Security.SecurityCritical] // auto-generated
        LocalActivator _LocalActivator;
        ActivationListener _ActivationListener;       
        IContextProperty[]  _appDomainProperties;
        int _flags;
        Object _ConfigLock;
        ChannelServicesData _ChannelServicesData;
                LeaseManager _LeaseManager;
        ReaderWriterLock _IDTableLock;

        internal DomainSpecificRemotingData()
        {            
            _flags = 0;
            _ConfigLock = new Object();
            _ChannelServicesData = new ChannelServicesData();
            _IDTableLock = new ReaderWriterLock();

            // Add the Lifetime service property to the appdomain.
            // For now we are assuming that this is the only property
            // If there are more properties, then an existing array
                        // will need to be expanded to add this property
                        // The property needs to be added here so that the default context
                        // for an appdomain has lifetime services activated

            _appDomainProperties = new IContextProperty[1];
            _appDomainProperties[0] = new System.Runtime.Remoting.Lifetime.LeaseLifeTimeServiceProperty();
        }

        internal LeaseManager LeaseManager
        {
            get 
            { 
                return _LeaseManager; 
            }
            set 
            {  
                _LeaseManager = value; 
            }
        }
                

        // This lock object is exposed for various objects that need to synchronize
        // there configuration behavior.
        internal Object ConfigLock
        {
            get { return _ConfigLock; }
        }

        // This is the rwlock used by the uri table functions
        internal ReaderWriterLock IDTableLock
        {
            get { return _IDTableLock; }
        }


        internal LocalActivator LocalActivator
        {
            [System.Security.SecurityCritical]  // auto-generated
            get{return _LocalActivator;}
            [System.Security.SecurityCritical]  // auto-generated
            set{_LocalActivator=value;}
        }

        internal ActivationListener ActivationListener
        {
            get {return _ActivationListener;}
            set {_ActivationListener=value;}
        }

        // access to InitializingActivation, ActivationInitialized
        // and ActivatorListening should be guarded by ConfigLock
        // by the caller.
        internal bool InitializingActivation
        {
            get {return (_flags & ACTIVATION_INITIALIZING) == ACTIVATION_INITIALIZING;}
            set 
            {
                if (value == true)
                {
                    _flags = _flags | ACTIVATION_INITIALIZING;
                }
                else
                {
                    _flags = _flags & ~ACTIVATION_INITIALIZING;
                }
            }
        }

        internal bool ActivationInitialized
        {
            get {return (_flags & ACTIVATION_INITIALIZED) == ACTIVATION_INITIALIZED;}
            set 
            {
                if (value == true)
                {
                    _flags = _flags | ACTIVATION_INITIALIZED;
                }
                else
                {
                    _flags = _flags & ~ACTIVATION_INITIALIZED;
                }
            }

        }

        internal bool ActivatorListening
        {
            get {return (_flags & ACTIVATOR_LISTENING) == ACTIVATOR_LISTENING;}
            set 
            {
                if (value == true)
                {
                    _flags = _flags | ACTIVATOR_LISTENING;
                }
                else
                {
                    _flags = _flags & ~ACTIVATOR_LISTENING;
                }
            }

        }
        
        
        internal IContextProperty[] AppDomainContextProperties
        {
            get { return _appDomainProperties; }
        } 

        internal ChannelServicesData ChannelServicesData
        {
            get 
            {
                return _ChannelServicesData;
            }
        }
    } // class DomainSpecificRemotingData




    //------------------------------------------------------------------    
    //--------------------- Remoting Configuration ---------------------    
    //------------------------------------------------------------------    
    internal static class RemotingConfigHandler
    {
        static volatile String _applicationName;
        static volatile CustomErrorsModes _errorMode = CustomErrorsModes.RemoteOnly;
        static volatile bool _errorsModeSet = false;
        static volatile bool _bMachineConfigLoaded = false;
        static volatile bool _bUrlObjRefMode = false;

        static Queue _delayLoadChannelConfigQueue = new Queue(); // queue of channels we might be able to use
        

        // All functions of RemotingConfigHandler operate upon the config
        // data stored on a per appDomain basis 
        public static RemotingConfigInfo Info = new RemotingConfigInfo();

        private const String _machineConfigFilename = "machine.config";
        

        internal static String ApplicationName
        {
            get
            {
                if (_applicationName == null)
                {
                    throw new RemotingException(
                        Environment.GetResourceString(
                            "Remoting_Config_NoAppName"));
                }
                return _applicationName;
            }

            set
            {
                if (_applicationName != null)
                {
                    throw new RemotingException(
                        String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_AppNameSet"),
                         _applicationName));
                }
                
                _applicationName = value;

                // get rid of any starting or trailing slashes
                char[] slash = new char[]{'/'};
                if (_applicationName.StartsWith("/", StringComparison.Ordinal))
                    _applicationName = _applicationName.TrimStart(slash);
                if (_applicationName.EndsWith("/", StringComparison.Ordinal))
                    _applicationName = _applicationName.TrimEnd(slash);
            }
        }

        internal static bool HasApplicationNameBeenSet()
        {
            return _applicationName != null;
        }

        internal static bool UrlObjRefMode
        {
            get { return _bUrlObjRefMode; }
        }
        
        internal static CustomErrorsModes  CustomErrorsMode 
        {
           get { 
                return _errorMode; 
           }
           set
           {
                if (_errorsModeSet)                
                    throw new RemotingException(Environment.GetResourceString("Remoting_Config_ErrorsModeSet"));                        
                
                _errorMode = value;
                _errorsModeSet = true;
           }
           
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessageSink FindDelayLoadChannelForCreateMessageSink(
            String url, Object data, out String objectURI)
        {
            LoadMachineConfigIfNecessary();
        
            objectURI = null;
            IMessageSink msgSink = null;
        
            foreach (DelayLoadClientChannelEntry entry in _delayLoadChannelConfigQueue)
            {
                IChannelSender channel = entry.Channel;

                // if the channel is null, that means it has already been registered.
                if (channel != null)
                {
                    msgSink = channel.CreateMessageSink(url, data, out objectURI);
                    if (msgSink != null)
                    {
                        entry.RegisterChannel();
                        return msgSink;
                    }
                }
            }

            return null;
        } // FindChannelForCreateMessageSink



        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        static void LoadMachineConfigIfNecessary()
        {                    
            // Load the machine.config file if we haven't already
            if (!_bMachineConfigLoaded)
            {
                lock (Info)
                {
                    if (!_bMachineConfigLoaded)
                    {
                        RemotingXmlConfigFileData configData = RemotingXmlConfigFileParser.ParseDefaultConfiguration(); 
                        if (configData != null)
                            ConfigureRemoting(configData, false/*ensureSecurity*/);

                        String machineDirectory = System.Security.Util.Config.MachineDirectory;                        
                        String longFileName = machineDirectory 
                                            + _machineConfigFilename;
                        new FileIOPermission(FileIOPermissionAccess.Read, longFileName).Assert();

                        configData = LoadConfigurationFromXmlFile(longFileName);

                        if (configData != null)
                            ConfigureRemoting(configData, false/*ensureSecurity*/);
                        
                        _bMachineConfigLoaded = true;
                    }
                }
            }
        } // LoadMachineConfigIfNecessary
                

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void DoConfiguration(String filename, bool ensureSecurity)
        {        
            LoadMachineConfigIfNecessary();
        
            // load specified config file
            RemotingXmlConfigFileData configData = LoadConfigurationFromXmlFile(filename);

            // Configure remoting based on data loaded from the config file.
            // By design, we do nothing if no remoting config information was
            // present in the file.
            if (configData != null)
                ConfigureRemoting(configData, ensureSecurity);
        }
        
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static RemotingXmlConfigFileData LoadConfigurationFromXmlFile(String filename)
        {
            try
            {
                if (filename != null)
                    return RemotingXmlConfigFileParser.ParseConfigFile(filename);
                else
                    return null;
            }
            catch (Exception e)
            {
                Exception inner =  e.InnerException as FileNotFoundException;
                if (inner != null)
                {
                    // if the file is missing, this gives a clearer message
                    e = inner;
                }
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                            "Remoting_Config_ReadFailure"),
                        filename,
                        e));
            }
        } // LoadConfigurationFromXmlFile       


        [System.Security.SecurityCritical]  // auto-generated
        private static void ConfigureRemoting(RemotingXmlConfigFileData configData, bool ensureSecurity)
        {
            try
            {
                String appName = configData.ApplicationName;
                if (appName != null)
                    ApplicationName = appName;
                
                if (configData.CustomErrors != null)
                    _errorMode = configData.CustomErrors.Mode;

                // configure channels
                ConfigureChannels(configData, ensureSecurity);
            
                // configure lifetime
                if (configData.Lifetime != null)
                {
                    if (configData.Lifetime.IsLeaseTimeSet)
                        LifetimeServices.LeaseTime = configData.Lifetime.LeaseTime;
                    if (configData.Lifetime.IsRenewOnCallTimeSet)
                        LifetimeServices.RenewOnCallTime = configData.Lifetime.RenewOnCallTime;
                    if (configData.Lifetime.IsSponsorshipTimeoutSet)    
                        LifetimeServices.SponsorshipTimeout = configData.Lifetime.SponsorshipTimeout;
                    if (configData.Lifetime.IsLeaseManagerPollTimeSet)
                        LifetimeServices.LeaseManagerPollTime = configData.Lifetime.LeaseManagerPollTime;
                }

                _bUrlObjRefMode = configData.UrlObjRefMode;

                // configure other entries
                Info.StoreRemoteAppEntries(configData);
                Info.StoreActivatedExports(configData);
                Info.StoreInteropEntries(configData);
                Info.StoreWellKnownExports(configData);

                // start up activation listener if there are any activated objects exposed
                if (configData.ServerActivatedEntries.Count > 0)
                    ActivationServices.StartListeningForRemoteRequests();                
            }
            catch (Exception e)
            {
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                            "Remoting_Config_ConfigurationFailure"),                        
                        e));
            }
        } // ConfigureRemoting
        

        // configures channels loaded from remoting config file.
        [System.Security.SecurityCritical]  // auto-generated
        private static void ConfigureChannels(RemotingXmlConfigFileData configData, bool ensureSecurity)
        {
            // Register our x-context & x-AD channels first
            RemotingServices.RegisterWellKnownChannels();
            
            foreach (RemotingXmlConfigFileData.ChannelEntry entry in configData.ChannelEntries)
            {
                if (!entry.DelayLoad)
                {
                    IChannel chnl = CreateChannelFromConfigEntry(entry);
                    ChannelServices.RegisterChannel(chnl, ensureSecurity);
                }
                else
                    _delayLoadChannelConfigQueue.Enqueue(new DelayLoadClientChannelEntry(entry, ensureSecurity));
            }
        } //  ConfigureChannels


        [System.Security.SecurityCritical]  // auto-generated
        internal static IChannel CreateChannelFromConfigEntry(
            RemotingXmlConfigFileData.ChannelEntry entry)
        {       
            Type type = RemotingConfigInfo.LoadType(entry.TypeName, entry.AssemblyName);
            
            bool isServerChannel = typeof(IChannelReceiver).IsAssignableFrom(type);
            bool isClientChannel = typeof(IChannelSender).IsAssignableFrom(type);

            IClientChannelSinkProvider clientProviderChain = null;
            IServerChannelSinkProvider serverProviderChain = null;

            if (entry.ClientSinkProviders.Count > 0)
                clientProviderChain = CreateClientChannelSinkProviderChain(entry.ClientSinkProviders);
            if (entry.ServerSinkProviders.Count > 0)
                serverProviderChain = CreateServerChannelSinkProviderChain(entry.ServerSinkProviders);

            // construct argument list
            Object[] args;
            
            if (isServerChannel && isClientChannel)
            {
                args = new Object[3];
                args[0] = entry.Properties;
                args[1] = clientProviderChain;
                args[2] = serverProviderChain;
            }
            else
            if (isServerChannel)
            {
                args = new Object[2];
                args[0] = entry.Properties;
                args[1] = serverProviderChain;
            }
            else
            if (isClientChannel)
            {
                args = new Object[2];
                args[0] = entry.Properties;
                args[1] = clientProviderChain;
            }
            else
            {
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidChannelType"), 
                    type.FullName));
            }

            IChannel channel = null;

            try
            {
                channel = (IChannel)Activator.CreateInstance(type, 
                                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, 
                                                        null, 
                                                        args, 
                                                        null, 
                                                        null);

            }
            catch (MissingMethodException)
            {
                String ctor = null;
                
                if (isServerChannel && isClientChannel)
                    ctor = "MyChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)";
                else
                if (isServerChannel)
                    ctor = "MyChannel(IDictionary properties, IServerChannelSinkProvider serverSinkProvider)";
                else
                if (isClientChannel)
                    ctor = "MyChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider)";
                
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_ChannelMissingCtor"),
                    type.FullName, ctor));
            }
            
            return channel;
        } //  CreateChannelFromEntry


        // create a client sink provider chain
        [System.Security.SecurityCritical]  // auto-generated
        private static IClientChannelSinkProvider CreateClientChannelSinkProviderChain(ArrayList entries)
        {   
            IClientChannelSinkProvider chain = null;
            IClientChannelSinkProvider current = null;
            
            foreach (RemotingXmlConfigFileData.SinkProviderEntry entry in entries)
            {
                if (chain == null)
                {
                    chain = (IClientChannelSinkProvider)CreateChannelSinkProvider(entry, false);
                    current = chain;
                }
                else
                {
                    current.Next = (IClientChannelSinkProvider)CreateChannelSinkProvider(entry, false);
                    current = current.Next;
                }
            }

            return chain;
        } // CreateClientChannelSinkProviderChain


        // create a client sink provider chain
        [System.Security.SecurityCritical]  // auto-generated
        private static IServerChannelSinkProvider CreateServerChannelSinkProviderChain(ArrayList entries)
        {   
            IServerChannelSinkProvider chain = null;
            IServerChannelSinkProvider current = null;
            
            foreach (RemotingXmlConfigFileData.SinkProviderEntry entry in entries)
            {
                if (chain == null)
                {
                    chain = (IServerChannelSinkProvider)CreateChannelSinkProvider(entry, true);
                    current = chain;
                }
                else
                {
                    current.Next = (IServerChannelSinkProvider)CreateChannelSinkProvider(entry, true);
                    current = current.Next;
                }
            }

            return chain;
        } // CreateServerChannelSinkProviderChain
            

        // create a sink provider from the config file data
        [System.Security.SecurityCritical]  // auto-generated
        private static Object CreateChannelSinkProvider(RemotingXmlConfigFileData.SinkProviderEntry entry,
                                                        bool bServer)
        {
            Object sinkProvider = null;

            Type type = RemotingConfigInfo.LoadType(entry.TypeName, entry.AssemblyName);            

            if (bServer)
            {
                // make sure this is a client provider                
                if (!typeof(IServerChannelSinkProvider).IsAssignableFrom(type))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidSinkProviderType"),
                            type.FullName,
                            "IServerChannelSinkProvider"));
                }
            }
            else
            {
                // make sure this is a server provider
                if (!typeof(IClientChannelSinkProvider).IsAssignableFrom(type))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidSinkProviderType"),
                            type.FullName,
                            "IClientChannelSinkProvider"));
                }
            }

            // check to see if something labelled as a formatter is a formatter
            if (entry.IsFormatter)
            {
                if ((bServer && !typeof(IServerFormatterSinkProvider).IsAssignableFrom(type)) ||
                    (!bServer && !typeof(IClientFormatterSinkProvider).IsAssignableFrom(type)))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_SinkProviderNotFormatter"),
                            type.FullName));
                }
            }                        
            
            // setup the argument list and call the constructor
            Object[] args = new Object[2];
            args[0] = entry.Properties;
            args[1] = entry.ProviderData;

            try
            {
                sinkProvider = Activator.CreateInstance(type, 
                                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, 
                                                        null, 
                                                        args, 
                                                        null, 
                                                        null);
            }
            catch (MissingMethodException)
            {
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_SinkProviderMissingCtor"),
                        type.FullName, 
                        "MySinkProvider(IDictionary properties, ICollection providerData)"));
            }

            return sinkProvider;
        } // CreateChannelSinkProvider
        
        // This is used at the client end to check if an activation needs
        // to go remote.
        [System.Security.SecurityCritical]  // auto-generated
        internal static ActivatedClientTypeEntry IsRemotelyActivatedClientType(RuntimeType svrType)
        {
            RemotingTypeCachedData cache = (RemotingTypeCachedData)
                InternalRemotingServices.GetReflectionCachedData(svrType);
        
            String assemblyName = cache.SimpleAssemblyName;
            ActivatedClientTypeEntry entry = Info.QueryRemoteActivate(svrType.FullName, assemblyName);

            if (entry == null)
            {
                // If not found try with the full assembly name
                String fullAssemblyName = cache.AssemblyName;
                entry = Info.QueryRemoteActivate(svrType.FullName, fullAssemblyName);
                if (entry == null){
                    // If still not found try with partial type name (without namespace)
                    entry = Info.QueryRemoteActivate(svrType.Name, assemblyName);
                }
            }
            return entry;
        } // IsRemotelyActivatedClientType

        
        // This is used at the client end to check if an activation needs
        // to go remote.
        internal static ActivatedClientTypeEntry IsRemotelyActivatedClientType(String typeName, String assemblyName)
        {
            return Info.QueryRemoteActivate(typeName, assemblyName);
        }


        // This is used at the client end to check if a "new Foo" needs to
        // happen via a Connect() under the covers.
        [System.Security.SecurityCritical]  // auto-generated
        internal static WellKnownClientTypeEntry IsWellKnownClientType(RuntimeType svrType)
        {
            RemotingTypeCachedData cache = (RemotingTypeCachedData)
                InternalRemotingServices.GetReflectionCachedData(svrType);
        
            String assemblyName = cache.SimpleAssemblyName;
            WellKnownClientTypeEntry wke = Info.QueryConnect(svrType.FullName, assemblyName);
            if (wke == null)
            {
                wke= Info.QueryConnect(svrType.Name, assemblyName);
            }
            return wke;
        }

        // This is used at the client end to check if a "new Foo" needs to
        // happen via a Connect() under the covers.
        internal static WellKnownClientTypeEntry IsWellKnownClientType(String typeName, 
                                                                       String assemblyName)
        {
            return Info.QueryConnect(typeName, assemblyName);
        }

        //
        // helper functions for processing and parsing data
        //
        private static void ParseGenericType(String typeAssem, int indexStart, out String typeName, out String assemName)
        {
            int len = typeAssem.Length;
            int depth = 1;

            int index = indexStart;
            while(depth > 0 && (++index < len - 1))
            {
                if (typeAssem[index] == '[') {
                    depth++;
                }
                else if (typeAssem[index] == ']') {
                    depth--;
                }
            }

            if (depth > 0 || index >= len) {
                typeName = null;
                assemName = null;
            }
            else {
                index = typeAssem.IndexOf(',', index);
                // comma must be present, and can't be last character
                if ((index >= 0) && (index < (len - 1)))
                {
                    typeName = typeAssem.Substring(0, index).Trim();
                    assemName = typeAssem.Substring(index + 1).Trim();
                }
                else
                {
                    typeName = null;
                    assemName = null;
                }
            }
        }

        internal static void ParseType(String typeAssem, out String typeName, out String assemName)
        {
            String value = typeAssem;
            
            int genericTypeIndex = value.IndexOf("[");
            if ((genericTypeIndex >= 0) && (genericTypeIndex < (value.Length - 1)))
            {
                ParseGenericType(value, genericTypeIndex, out typeName, out assemName);
            }
            else 
            {
                int index = value.IndexOf(",");

                // comma must be present, and can't be last character
                if ((index >= 0) && (index < (value.Length - 1)))
                {
                    typeName = value.Substring(0, index).Trim();
                    assemName = value.Substring(index + 1).Trim();
                }
                else
                {
                    typeName = null;
                    assemName = null;
                }
            }
        } // ParseType
        // This is used at the server end to check if a type being activated
        // is explicitly allowed by the server.
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool IsActivationAllowed(RuntimeType svrType)
        {
            if (svrType == null)
                return false;

            RemotingTypeCachedData cache = (RemotingTypeCachedData)
                InternalRemotingServices.GetReflectionCachedData(svrType);
        
            String assemblyName = cache.SimpleAssemblyName;

            return Info.ActivationAllowed(svrType.FullName, assemblyName);
        } // IsActivationAllowed

        // This is the flavor that we call from the activation listener
        // code path. This ensures that we don't load a type before checking
        // that it is configured for remote activation
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool IsActivationAllowed(String TypeName)
        {
            String svrTypeName = RemotingServices.InternalGetTypeNameFromQualifiedTypeName(TypeName);
            if (svrTypeName == null)
            {
                return false;
            }
            String typeName;
            String asmName;

            ParseType(svrTypeName, out typeName, out asmName);
            if (asmName == null)
                return false;
        
            int index = asmName.IndexOf(',');
            if (index != -1)
            {
                // strip off the version info
                asmName = asmName.Substring(0,index);
            }
            return Info.ActivationAllowed(typeName, asmName);
        }

        // helper for Configuration::RegisterActivatedServiceType
        internal static void RegisterActivatedServiceType(ActivatedServiceTypeEntry entry)
        {   
            Info.AddActivatedType(entry.TypeName, entry.AssemblyName, 
                                  entry.ContextAttributes);
        } // RegisterActivatedServiceType

        
        // helper for Configuration::RegisterWellKnownServiceType
        [System.Security.SecurityCritical]  // auto-generated
        internal static void RegisterWellKnownServiceType(WellKnownServiceTypeEntry entry)
        {
            BCLDebug.Trace("REMOTE", "Adding well known service type for " + entry.ObjectUri);
            // <
            String serverType = entry.TypeName;
            String asmName = entry.AssemblyName;
            String URI = entry.ObjectUri;
            WellKnownObjectMode mode = entry.Mode;
            
            lock (Info)
            {            
                // We make an entry in our config tables so as to keep
                // both the file-based and programmatic config in [....].
                Info.AddWellKnownEntry(entry);
            }
        } // RegisterWellKnownServiceType


        // helper for Configuration::RegisterActivatedClientType
        internal static void RegisterActivatedClientType(ActivatedClientTypeEntry entry)
        {
            Info.AddActivatedClientType(entry);
        }

        // helper for Configuration::RegisterWellKnownClientType
        internal static void RegisterWellKnownClientType(WellKnownClientTypeEntry entry)
        {
            Info.AddWellKnownClientType(entry);
        } 

        //helper for Configuration::GetServerTypeForUri
        [System.Security.SecurityCritical]  // auto-generated
        internal static Type GetServerTypeForUri(String URI)
        {
            URI = Identity.RemoveAppNameOrAppGuidIfNecessary(URI);
            return Info.GetServerTypeForUri(URI);
        }
        
        // helper for Configuration::GetRegisteredActivatedServiceTypes
        internal static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
        {
            return Info.GetRegisteredActivatedServiceTypes();
        } // GetRegisteredActivatedServiceTypes

        // helper for Configuration::GetRegisteredWellKnownServiceTypes
        internal static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
        {
            return Info.GetRegisteredWellKnownServiceTypes();
        } // GetRegisteredWellKnownServiceTypes

        // helper for Configuration::GetRegisteredActivatedClientTypes
        internal static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
        {
            return Info.GetRegisteredActivatedClientTypes();
        } // GetRegisteredActivatedClientTypes

        // helper for Configuration::GetRegisteredWellKnownClientTypes
        internal static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
        {
            return Info.GetRegisteredWellKnownClientTypes();
        } // GetRegisteredWellKnownClientTypes
        

        // helper for creating well known objects on demand
        [System.Security.SecurityCritical]  // auto-generated
        internal static ServerIdentity CreateWellKnownObject(String uri)
        {
            uri = Identity.RemoveAppNameOrAppGuidIfNecessary(uri);
            return Info.StartupWellKnownObject(uri);
        }
        

        internal class RemotingConfigInfo
        {
            Hashtable _exportableClasses; // list of objects that can be client-activated
                                          // (this should be a StringTable since we only use the key,
                                          //  but that type was removed from the BCL :( )
            Hashtable _remoteTypeInfo;
            Hashtable _remoteAppInfo;
            Hashtable _wellKnownExportInfo; //well known exports indexed by object URI in lower-case            
          
            static char[] SepSpace = {' '};
            static char[] SepPound = {'#'};
            static char[] SepSemiColon = {';'};
            static char[] SepEquals = {'='};

            private static Object s_wkoStartLock = new Object();
            private static PermissionSet s_fullTrust = new PermissionSet(PermissionState.Unrestricted);
            
            internal RemotingConfigInfo()
            {
                // <
                _remoteTypeInfo = Hashtable.Synchronized(new Hashtable());

                _exportableClasses = Hashtable.Synchronized(new Hashtable());

                _remoteAppInfo = Hashtable.Synchronized(new Hashtable());
                _wellKnownExportInfo = Hashtable.Synchronized(new Hashtable());
            }


            // encodes type name and assembly name into one string for purposes of
            //   indexing in lists and hash tables
            private String EncodeTypeAndAssemblyNames(String typeName, String assemblyName)
            {
                return typeName + ", " + assemblyName.ToLower(CultureInfo.InvariantCulture);
            }
            

            //
            // XML Configuration Helper Functions
            //

            internal void StoreActivatedExports(RemotingXmlConfigFileData configData)
            {
                foreach (RemotingXmlConfigFileData.TypeEntry entry in configData.ServerActivatedEntries)
                {
                    ActivatedServiceTypeEntry aste =
                        new ActivatedServiceTypeEntry(entry.TypeName, entry.AssemblyName);
                    aste.ContextAttributes = 
                        CreateContextAttributesFromConfigEntries(entry.ContextAttributes);
                
                    RemotingConfiguration.RegisterActivatedServiceType(aste);
                }
            } // StoreActivatedExports

            [System.Security.SecurityCritical]  // auto-generated
            internal void StoreInteropEntries(RemotingXmlConfigFileData configData)
            {
                // process interop xml element entries
                foreach (RemotingXmlConfigFileData.InteropXmlElementEntry entry in
                         configData.InteropXmlElementEntries)
                {
                    Assembly assembly = Assembly.Load(entry.UrtAssemblyName);
                    Type type = assembly.GetType(entry.UrtTypeName);
                    SoapServices.RegisterInteropXmlElement(entry.XmlElementName,
                                                           entry.XmlElementNamespace,
                                                           type);
                }

                // process interop xml type entries
                foreach (RemotingXmlConfigFileData.InteropXmlTypeEntry entry in
                         configData.InteropXmlTypeEntries)
                {
                    Assembly assembly = Assembly.Load(entry.UrtAssemblyName);
                    Type type = assembly.GetType(entry.UrtTypeName);
                    SoapServices.RegisterInteropXmlType(entry.XmlTypeName,
                                                        entry.XmlTypeNamespace,
                                                        type);
                }

                // process preload entries
                foreach (RemotingXmlConfigFileData.PreLoadEntry entry in configData.PreLoadEntries)
                {
                    Assembly assembly = Assembly.Load(entry.AssemblyName);

                    if (entry.TypeName != null)
                    {
                        Type type = assembly.GetType(entry.TypeName);
                        SoapServices.PreLoad(type);
                    }
                    else
                    {
                        SoapServices.PreLoad(assembly);
                    }
                }
            } // StoreInteropEntries

            internal void StoreRemoteAppEntries(RemotingXmlConfigFileData configData)
            {
                char[] slash = new char[]{'/'};
            
                // add each remote app to the table
                foreach (RemotingXmlConfigFileData.RemoteAppEntry remApp in configData.RemoteAppEntries)
                {
                    // form complete application uri by combining specified uri with app-name
                    //  (make sure appUri ends with slash, and that app name doesn't start,
                    //   with one. then make sure that the combined form has no trailing slashes).
                    String appUri = remApp.AppUri;
                    if ((appUri != null) && !appUri.EndsWith("/", StringComparison.Ordinal))
                        appUri = appUri.TrimEnd(slash);
                        
                    // add each client activated type for this remote app
                    foreach (RemotingXmlConfigFileData.TypeEntry cae in remApp.ActivatedObjects)
                    {
                        ActivatedClientTypeEntry acte = 
                            new ActivatedClientTypeEntry(cae.TypeName, cae.AssemblyName, 
                                                         appUri);
                        acte.ContextAttributes = 
                            CreateContextAttributesFromConfigEntries(cae.ContextAttributes);
                   
                        RemotingConfiguration.RegisterActivatedClientType(acte);
                    }

                    // add each well known object for this remote app
                    foreach (RemotingXmlConfigFileData.ClientWellKnownEntry cwke in remApp.WellKnownObjects)
                    {                    
                        WellKnownClientTypeEntry wke = 
                            new WellKnownClientTypeEntry(cwke.TypeName, cwke.AssemblyName, 
                                                         cwke.Url);
                        wke.ApplicationUrl = appUri;
                        
                        RemotingConfiguration.RegisterWellKnownClientType(wke);
                    }          
                }
            } // StoreRemoteAppEntries            

            [System.Security.SecurityCritical]  // auto-generated
            internal void StoreWellKnownExports(RemotingXmlConfigFileData configData)
            {
                // <
            
                foreach (RemotingXmlConfigFileData.ServerWellKnownEntry entry in configData.ServerWellKnownEntries)
                {
                    WellKnownServiceTypeEntry wke = 
                        new WellKnownServiceTypeEntry(
                            entry.TypeName, entry.AssemblyName, entry.ObjectURI, 
                            entry.ObjectMode);
                    wke.ContextAttributes = null;
                
                    // Register the well known entry but do not startup the object
                    RemotingConfigHandler.RegisterWellKnownServiceType(wke);
                }
            } // StoreWellKnownExports
            

            // helper functions for above configuration helpers

            static IContextAttribute[] CreateContextAttributesFromConfigEntries(ArrayList contextAttributes)
            {
                // create context attribute entry list
                int numAttrs = contextAttributes.Count;
                if (numAttrs == 0)
                    return null;
                
                IContextAttribute[] attrs = new IContextAttribute[numAttrs];

                int co = 0;
                foreach (RemotingXmlConfigFileData.ContextAttributeEntry cae in contextAttributes)
                {
                    Assembly asm = Assembly.Load(cae.AssemblyName);  

                    IContextAttribute attr = null;
                    Hashtable properties = cae.Properties;                    
                    if ((properties != null) && (properties.Count > 0))
                    {
                        Object[] args = new Object[1];
                        args[0] = properties;

                        // We explicitly allow the ability to create internal
                        // only attributes
                        attr = (IContextAttribute)
                            Activator.CreateInstance(
                                asm.GetType(cae.TypeName, false, false), 
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, 
                                null, 
                                args, 
                                null, 
                                null);
                    }
                    else
                    {
                        attr = (IContextAttribute)
                            Activator.CreateInstance(
                                asm.GetType(cae.TypeName, false, false), 
                                true);
                    }
                    
                    attrs[co++] = attr; 
                }

                return attrs;
            } // CreateContextAttributesFromConfigEntries

            //
            // end of XML configuration helper functions
            //

            internal bool ActivationAllowed(String typeName, String assemblyName)
            {
                // the assembly name is stored in lower-case to let it be case-insensitive
                return _exportableClasses.ContainsKey(EncodeTypeAndAssemblyNames(typeName, assemblyName));
            }

            internal ActivatedClientTypeEntry QueryRemoteActivate(String typeName, String assemblyName)
            {
                String index = EncodeTypeAndAssemblyNames(typeName, assemblyName);
            
                ActivatedClientTypeEntry typeEntry = _remoteTypeInfo[index] as ActivatedClientTypeEntry;
                if (typeEntry == null)
                    return null;         

                if (typeEntry.GetRemoteAppEntry() == null)
                {
                    RemoteAppEntry appEntry = (RemoteAppEntry)
                                            _remoteAppInfo[typeEntry.ApplicationUrl];
                    if (appEntry == null)
                    {
                        throw new RemotingException(
                         String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Activation_MissingRemoteAppEntry"),
                            typeEntry.ApplicationUrl));                            
                    }
                    typeEntry.CacheRemoteAppEntry(appEntry);
                }
                return typeEntry;
            }

            internal WellKnownClientTypeEntry QueryConnect(String typeName, String assemblyName)
            {
                String index = EncodeTypeAndAssemblyNames(typeName, assemblyName);
                
                WellKnownClientTypeEntry typeEntry = _remoteTypeInfo[index] as WellKnownClientTypeEntry;
                if (typeEntry == null)
                    return null;
                    
                return typeEntry;
            }       
          
            //
            // helper functions to retrieve registered types
            //


            internal ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
            {
                ActivatedServiceTypeEntry[] entries =
                    new ActivatedServiceTypeEntry[_exportableClasses.Count];

                int co = 0;
                foreach (DictionaryEntry dictEntry in _exportableClasses)
                {
                    entries[co++] = (ActivatedServiceTypeEntry)dictEntry.Value;
                }
                    
                return entries;
            } // GetRegisteredActivatedServiceTypes


            internal WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
            {
                WellKnownServiceTypeEntry[] entries =
                    new WellKnownServiceTypeEntry[_wellKnownExportInfo.Count];

                int co = 0;
                foreach (DictionaryEntry dictEntry in _wellKnownExportInfo)
                {
                    WellKnownServiceTypeEntry entry = (WellKnownServiceTypeEntry)dictEntry.Value;
                    
                    WellKnownServiceTypeEntry wkste =
                        new WellKnownServiceTypeEntry(
                            entry.TypeName, entry.AssemblyName,
                            entry.ObjectUri, entry.Mode);

                    wkste.ContextAttributes = entry.ContextAttributes;
                    
                    entries[co++] = wkste;
                }
                    
                return entries;
            } // GetRegisteredWellKnownServiceTypes


            internal ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
            {
                // count number of well known client types
                int count = 0;
                foreach (DictionaryEntry dictEntry in _remoteTypeInfo)
                {
                    ActivatedClientTypeEntry entry = dictEntry.Value as ActivatedClientTypeEntry;                
                    if (entry != null)
                        count++;
                }
                            
                ActivatedClientTypeEntry[] entries =
                    new ActivatedClientTypeEntry[count];

                int co = 0;
                foreach (DictionaryEntry dictEntry in _remoteTypeInfo)
                {
                    ActivatedClientTypeEntry entry = dictEntry.Value as ActivatedClientTypeEntry;

                    if (entry != null)
                    {
                        // retrieve application url
                        String appUrl = null;
                        RemoteAppEntry remApp = entry.GetRemoteAppEntry();
                        if (remApp != null)
                            appUrl = remApp.GetAppURI();  
                    
                        ActivatedClientTypeEntry wkcte =
                            new ActivatedClientTypeEntry(entry.TypeName, 
                                entry.AssemblyName, appUrl);
                        
                        // Fetch the context attributes
                        wkcte.ContextAttributes = entry.ContextAttributes;

                        entries[co++] = wkcte;
                    }
                    
                }
                   
                return entries;
            } // GetRegisteredActivatedClientTypes
            

            internal WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
            {
                // count number of well known client types
                int count = 0;
                foreach (DictionaryEntry dictEntry in _remoteTypeInfo)
                {
                    WellKnownClientTypeEntry entry = dictEntry.Value as WellKnownClientTypeEntry;                
                    if (entry != null)
                        count++;
                }
                            
                WellKnownClientTypeEntry[] entries =
                    new WellKnownClientTypeEntry[count];

                int co = 0;
                foreach (DictionaryEntry dictEntry in _remoteTypeInfo)
                {
                    WellKnownClientTypeEntry entry = dictEntry.Value as WellKnownClientTypeEntry;

                    if (entry != null)
                    {                    
                        WellKnownClientTypeEntry wkcte =
                            new WellKnownClientTypeEntry(entry.TypeName, 
                                entry.AssemblyName, entry.ObjectUrl);

                        // see if there is an associated app
                        RemoteAppEntry remApp = entry.GetRemoteAppEntry();
                        if (remApp != null)
                            wkcte.ApplicationUrl = remApp.GetAppURI();                             

                        entries[co++] = wkcte;
                    }
                    
                }
                   
                return entries;
            } // GetRegisteredWellKnownClientTypes


            //
            // end of helper functions to retrieve registered types
            //

            internal void AddActivatedType(String typeName, String assemblyName,
                                           IContextAttribute[] contextAttributes)
            {
                if (typeName == null)
                    throw new ArgumentNullException("typeName");
                if (assemblyName == null)
                    throw new ArgumentNullException("assemblyName");
                Contract.EndContractBlock();

                if (CheckForRedirectedClientType(typeName, assemblyName))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Config_CantUseRedirectedTypeForWellKnownService"),
                            typeName, assemblyName));
                }                                

                ActivatedServiceTypeEntry aste =  
                    new ActivatedServiceTypeEntry(typeName, assemblyName);
                aste.ContextAttributes = contextAttributes;
            
                //   The assembly name is stored in lowercase to let it be case-insensitive.
                String key = EncodeTypeAndAssemblyNames(typeName, assemblyName);
                _exportableClasses.Add(key, aste);
            } // AddActivatedType


            // determines if either a wellknown or activated service type entry
            //   is associated with the given type name and assembly name
            private bool CheckForServiceEntryWithType(String typeName, String asmName)
            {  
                return
                    CheckForWellKnownServiceEntryWithType(typeName, asmName) ||
                    ActivationAllowed(typeName, asmName);                 
            } // CheckForServiceEntryWithType

            private bool CheckForWellKnownServiceEntryWithType(String typeName, String asmName)
            {
                foreach (DictionaryEntry entry in _wellKnownExportInfo)
                {
                    WellKnownServiceTypeEntry svc = 
                        (WellKnownServiceTypeEntry)entry.Value;
                    if (typeName == svc.TypeName)
                    {
                        bool match = false;
                        
                        // need to ignore version while checking
                        if (asmName == svc.AssemblyName)
                            match = true;
                        else
                        {
                            // only well known service entry can have version info
                            if (String.Compare(svc.AssemblyName, 0, asmName, 0, asmName.Length, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // if asmName != svc.AssemblyName and svc.AssemblyName
                                //   starts with asmName we know that svc.AssemblyName is
                                //   longer. If the next character is a comma, then the
                                //   assembly names match except for version numbers
                                //   which is ok.
                                if (svc.AssemblyName[asmName.Length] == ',')
                                    match = true;
                            }
                        }

                        // We were trying to redirect
                        if (match)
                            return true;
                    }
                }

                return false;
            } // CheckForWellKnownServiceEntryOfType


            // returns true if activation for the type has been redirected.
            private bool CheckForRedirectedClientType(String typeName, String asmName)
            {
                // if asmName has version information, remove it.
                int index = asmName.IndexOf(",");
                if (index != -1)
                    asmName = asmName.Substring(0, index);

                return 
                    (QueryRemoteActivate(typeName, asmName) != null) ||
                    (QueryConnect(typeName, asmName) != null);
            } // CheckForRedirectedClientType
            

            internal void AddActivatedClientType(ActivatedClientTypeEntry entry)
            {
                if (CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Config_TypeAlreadyRedirected"),
                            entry.TypeName, entry.AssemblyName));
                }                 

                if (CheckForServiceEntryWithType(entry.TypeName, entry.AssemblyName))
                {
                   throw new RemotingException(
                       String.Format(
                           CultureInfo.CurrentCulture, Environment.GetResourceString(
                               "Remoting_Config_CantRedirectActivationOfWellKnownService"),
                           entry.TypeName, entry.AssemblyName));
                }
            
                String appUrl = entry.ApplicationUrl;
                RemoteAppEntry appEntry = (RemoteAppEntry)_remoteAppInfo[appUrl];
                if (appEntry == null)
                {
                    appEntry = new RemoteAppEntry(appUrl, appUrl);
                    _remoteAppInfo.Add(appUrl, appEntry);
                }
                    
                if (appEntry != null)
                {
                    entry.CacheRemoteAppEntry(appEntry);
                }
                    
                String index = EncodeTypeAndAssemblyNames(entry.TypeName, entry.AssemblyName);
                _remoteTypeInfo.Add(index, entry);
            } // AddActivatedClientType


            internal void AddWellKnownClientType(WellKnownClientTypeEntry entry)
            {
                if (CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Config_TypeAlreadyRedirected"),
                            entry.TypeName, entry.AssemblyName));
                }    

                if (CheckForServiceEntryWithType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Config_CantRedirectActivationOfWellKnownService"),
                            entry.TypeName, entry.AssemblyName));
                }
            
            
                String appUrl = entry.ApplicationUrl;

                RemoteAppEntry appEntry = null;
                if (appUrl != null)
                {
                    appEntry = (RemoteAppEntry)_remoteAppInfo[appUrl];
                    if (appEntry == null)
                    {
                        appEntry = new RemoteAppEntry(appUrl, appUrl);
                        _remoteAppInfo.Add(appUrl, appEntry);
                    }
                }
            
                if (appEntry != null)
                    entry.CacheRemoteAppEntry(appEntry);

                String index = EncodeTypeAndAssemblyNames(entry.TypeName, entry.AssemblyName);
                _remoteTypeInfo.Add(index, entry);
            } // AddWellKnownClientType
            
            

            // This is to add programmatically registered well known objects
            // so that we keep all this data in one place
            [System.Security.SecurityCritical]  // auto-generated
            internal void AddWellKnownEntry(WellKnownServiceTypeEntry entry)
            {
                AddWellKnownEntry(entry, true);                
            }

            [System.Security.SecurityCritical]  // auto-generated
            internal void AddWellKnownEntry(WellKnownServiceTypeEntry entry, bool fReplace)
            {
                if (CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Config_CantUseRedirectedTypeForWellKnownService"),
                            entry.TypeName, entry.AssemblyName));
                }
            
                String key = entry.ObjectUri.ToLower(CultureInfo.InvariantCulture);
                
                if (fReplace)
                {
                    // Registering a well known object twice replaces the old one, so
                    //   we null out the old entry in the identity table after adding
                    //   this one. The identity will be recreated the next time someone
                    //   asks for this object.
                    _wellKnownExportInfo[key] = entry;

                    IdentityHolder.RemoveIdentity(entry.ObjectUri);
                }
                else
                {
                    _wellKnownExportInfo.Add(key, entry);
                }

            }

            //This API exposes a way to get server type information wiihout booting the object
            [System.Security.SecurityCritical]  // auto-generated
            internal Type GetServerTypeForUri(String URI)
            {
                Contract.Assert(null != URI, "null != URI");

                Type serverType = null;
                String uriLower = URI.ToLower(CultureInfo.InvariantCulture);

                WellKnownServiceTypeEntry entry = 
                        (WellKnownServiceTypeEntry)_wellKnownExportInfo[uriLower];

                if(entry != null)
                {
                    serverType = LoadType(entry.TypeName, entry.AssemblyName);
                }

                return serverType;
            }
            
            [System.Security.SecurityCritical]  // auto-generated
            internal ServerIdentity StartupWellKnownObject(String URI)
            {
                Contract.Assert(null != URI, "null != URI");
                
                String uriLower = URI.ToLower(CultureInfo.InvariantCulture);
                ServerIdentity ident = null;

                WellKnownServiceTypeEntry entry = 
                    (WellKnownServiceTypeEntry)_wellKnownExportInfo[uriLower];
                if (entry != null)
                {
                    ident = StartupWellKnownObject(
                        entry.AssemblyName,
                        entry.TypeName,
                        entry.ObjectUri,
                        entry.Mode);

                }

                return ident;
            }

            [System.Security.SecurityCritical]  // auto-generated
            internal ServerIdentity StartupWellKnownObject(
                String asmName, String svrTypeName, String URI, 
                WellKnownObjectMode mode)
            {
                return StartupWellKnownObject(asmName, svrTypeName, URI, mode, false);
            }

            [System.Security.SecurityCritical]  // auto-generated
            internal ServerIdentity StartupWellKnownObject(
                String asmName, String svrTypeName, String URI, 
                WellKnownObjectMode mode,
                bool fReplace)
            {
                lock (s_wkoStartLock)
                {                
                    MarshalByRefObject obj = null;
                    ServerIdentity srvID = null;

                    // attempt to load the type                
                    Type serverType = LoadType(svrTypeName, asmName);
                
                    // make sure the well known object derives from MarshalByRefObject
                    if(!serverType.IsMarshalByRef)
                    {   
                        throw new RemotingException(
                            Environment.GetResourceString("Remoting_WellKnown_MustBeMBR",
                            svrTypeName));                         
                    }

                    // make sure that no one beat us to creating
                    // the well known object
                    srvID = (ServerIdentity)IdentityHolder.ResolveIdentity(URI);
                    if ((srvID != null) && srvID.IsRemoteDisconnected())
                    {
                        IdentityHolder.RemoveIdentity(URI);
                        srvID = null;
                    }
                                        
                    if (srvID == null)
                    {                    
                        //WellKnown type instances need to be created under full trust
                        //since the permission set might have been restricted by the channel 
                        //pipeline.           
                        //This assert is protected by Infrastructure link demands.
                        s_fullTrust.Assert();                
                        try {                    
                            obj = (MarshalByRefObject)Activator.CreateInstance(serverType, true);
                                                 
                            if (RemotingServices.IsClientProxy(obj))
                            {
                                // The wellknown type is remoted so we must wrap the proxy
                                // with a local object.

                                // The redirection proxy masquerades as an object of the appropriate
                                // type, and forwards incoming messages to the actual proxy.
                                RedirectionProxy redirectedProxy = new RedirectionProxy(obj, serverType);
                                redirectedProxy.ObjectMode = mode;
                                RemotingServices.MarshalInternal(redirectedProxy, URI, serverType);

                                srvID = (ServerIdentity)IdentityHolder.ResolveIdentity(URI);
                                Contract.Assert(null != srvID, "null != srvID");

                                // The redirection proxy handles SingleCall versus Singleton,
                                // so we always set its mode to Singleton.
                                srvID.SetSingletonObjectMode();
                            }
                            else
                            if (serverType.IsCOMObject && (mode == WellKnownObjectMode.Singleton))
                            {
                                // Singleton COM objects are wrapped, so that they will be
                                //   recreated when an RPC server not available is thrown
                                //   if dllhost.exe is killed.
                                ComRedirectionProxy comRedirectedProxy = new ComRedirectionProxy(obj, serverType);
                                RemotingServices.MarshalInternal(comRedirectedProxy, URI, serverType);

                                srvID = (ServerIdentity)IdentityHolder.ResolveIdentity(URI);
                                Contract.Assert(null != srvID, "null != srvID");

                                // Only singleton COM objects are redirected this way.
                                srvID.SetSingletonObjectMode();
                            }
                            else
                            {
                                // make sure the object didn't Marshal itself.
                                String tempUri = RemotingServices.GetObjectUri(obj);
                                if (tempUri != null)
                                {
                                    throw new RemotingException(
                                        String.Format(
                                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                                "Remoting_WellKnown_CtorCantMarshal"),
                                            URI));
                                }
                        
                                RemotingServices.MarshalInternal(obj, URI, serverType);

                                srvID = (ServerIdentity)IdentityHolder.ResolveIdentity(URI);
                                Contract.Assert(null != srvID, "null != srvID");

                                if (mode == WellKnownObjectMode.SingleCall)
                                {
                                    // We need to set a special flag in the serverId
                                    // so that every dispatch to this type creates 
                                    // a new instance of the server object
                                    srvID.SetSingleCallObjectMode();
                                }
                                else
                                {
                                    srvID.SetSingletonObjectMode();
                                }
                            }
                        }
                        catch
                        {
                            throw;
                        }
                        finally {
                            SecurityPermission.RevertAssert();
                        }
                    }
                    
                    Contract.Assert(null != srvID, "null != srvID");
                    return srvID;
                }
            } // StartupWellKnownObject


            [System.Security.SecurityCritical]  // auto-generated
            internal static Type LoadType(String typeName, String assemblyName)
            {
                Assembly asm = null;                                               
                // All the LoadType callers have been protected by 
                // Infrastructure LinkDemand, it is safe to assert
                // this permission. 
                // Assembly.Load demands FileIO when the target 
                // assembly is the same as the executable running.
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try {                    
                    asm = Assembly.Load(assemblyName);
                }
                finally {
                    CodeAccessPermission.RevertAssert();
                }
                
                if (asm == null)
                {
                    throw new RemotingException(
                        Environment.GetResourceString("Remoting_AssemblyLoadFailed",
                        assemblyName));                    
                }

                Type type = asm.GetType(typeName, false, false);
                if (type == null)
                {
                    throw new RemotingException(
                        Environment.GetResourceString("Remoting_BadType",
                        typeName + ", " + assemblyName));     
                }

                return type;
            } // LoadType


            
        }// class RemotingConfigInfo        
    } // class RemotingConfigHandler



    internal class DelayLoadClientChannelEntry
    {
        private RemotingXmlConfigFileData.ChannelEntry _entry;
        private IChannelSender _channel;
        private bool _bRegistered;
        private bool _ensureSecurity;

        internal DelayLoadClientChannelEntry(RemotingXmlConfigFileData.ChannelEntry entry, bool ensureSecurity)
        {
            _entry = entry;
            _channel = null;      
            _bRegistered = false;
            _ensureSecurity = ensureSecurity;
        }

        internal IChannelSender Channel
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                // If this method returns null, that means the channel has already been registered.
        
                // NOTE: Access to delay load client entries is synchronized at a higher level.
                if (_channel == null)
                {
                    if (!_bRegistered)
                    {
                        _channel = (IChannelSender)RemotingConfigHandler.CreateChannelFromConfigEntry(_entry);
                        _entry = null;
                    }
                }

                return _channel;
            } // get
        } // Channel

        internal void RegisterChannel()
        {
            Contract.Assert(_channel != null, "channel shouldn't be null");
        
            // NOTE: Access to delay load client entries is synchronized at a higher level.
            ChannelServices.RegisterChannel(_channel, _ensureSecurity);
            _bRegistered = true;
            _channel = null;
        } // RegisterChannel
        
    } // class DelayLoadChannelEntry

    

} // namespace

