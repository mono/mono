// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    RemotingConfiguration.cs
**
** Purpose: Classes for interfacing with remoting configuration 
**            settings
**
**
===========================================================*/

using System;
using System.Security;
using System.Security.Permissions;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using StackCrawlMark = System.Threading.StackCrawlMark;
using System.Runtime.Versioning;
using System.Diagnostics.Contracts;

namespace System.Runtime.Remoting 
{
    // Configuration - provides static methods interfacing with
    //   configuration settings.
    [System.Runtime.InteropServices.ComVisible(true)]
    public static class RemotingConfiguration
    {
        private static volatile bool s_ListeningForActivationRequests = false;

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [Obsolete("Use System.Runtime.Remoting.RemotingConfiguration.Configure(string fileName, bool ensureSecurity) instead.", false)]
        public static void Configure(String filename)
        {
            Configure(filename, false/*ensureSecurity*/);
        }
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static void Configure(String filename, bool ensureSecurity)       
        {           
            RemotingConfigHandler.DoConfiguration(filename, ensureSecurity);
            
            // Set a flag in the VM to mark that remoting is configured
            // This will enable us to decide if activation for MBR
            // objects should go through the managed codepath
            RemotingServices.InternalSetRemoteActivationConfigured();
            
        } // Configure

        public static String ApplicationName
        {
            get 
            {
                if (!RemotingConfigHandler.HasApplicationNameBeenSet())
                    return null;
                else
                    return RemotingConfigHandler.ApplicationName;
            }

            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                RemotingConfigHandler.ApplicationName = value;
            }
        } // ApplicationName


        // The application id is prepended to object uri's.
        public static String ApplicationId
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get { return Identity.AppDomainUniqueId; }
        } // ApplicationId

        public static String ProcessId
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get { return Identity.ProcessGuid;}
        }
         
        public static CustomErrorsModes CustomErrorsMode 
        {
            get { return RemotingConfigHandler.CustomErrorsMode; }

            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                RemotingConfigHandler.CustomErrorsMode = value;
            }

        }

        public static bool CustomErrorsEnabled(bool isLocalRequest) 
        {
            switch (CustomErrorsMode) 
            {
                case CustomErrorsModes.Off:
                    return false;

                case CustomErrorsModes.On:
                    return true;

                case CustomErrorsModes.RemoteOnly:
                    return(!isLocalRequest);

                default:
                    return true;
            }
        }              

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedServiceType(Type type)
        {
            ActivatedServiceTypeEntry entry = new ActivatedServiceTypeEntry(type);
            RemotingConfiguration.RegisterActivatedServiceType(entry);
        } // RegisterActivatedServiceType


        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedServiceType(ActivatedServiceTypeEntry entry)
        {
            RemotingConfigHandler.RegisterActivatedServiceType(entry);

            // make sure we're listening for activation requests
            //  (all registrations for activated service types will come through here)
            if (!s_ListeningForActivationRequests)
            {
                s_ListeningForActivationRequests = true;
                ActivationServices.StartListeningForRemoteRequests();
            }
        } // RegisterActivatedServiceType


        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownServiceType(
            Type type, String objectUri, WellKnownObjectMode mode)
        {
            WellKnownServiceTypeEntry wke = 
                new WellKnownServiceTypeEntry(type, objectUri, mode);        
            RemotingConfiguration.RegisterWellKnownServiceType(wke); 
        } // RegisterWellKnownServiceType



        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownServiceType(WellKnownServiceTypeEntry entry)
        {
            RemotingConfigHandler.RegisterWellKnownServiceType(entry);    
        } // RegisterWellKnownServiceType


        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedClientType(Type type, String appUrl)
        {
            ActivatedClientTypeEntry acte = 
                new ActivatedClientTypeEntry(type, appUrl);
            RemotingConfiguration.RegisterActivatedClientType(acte);
        } // RegisterActivatedClientType



        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterActivatedClientType(ActivatedClientTypeEntry entry)
        {
            RemotingConfigHandler.RegisterActivatedClientType(entry);

            // all registrations for activated client types will come through here
            RemotingServices.InternalSetRemoteActivationConfigured();
        } // RegisterActivatedClientType




        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownClientType(Type type, String objectUrl)
        {
            WellKnownClientTypeEntry wke = new WellKnownClientTypeEntry(type, objectUrl);
            RemotingConfiguration.RegisterWellKnownClientType(wke);
        } // RegisterWellKnownClientType



        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterWellKnownClientType(WellKnownClientTypeEntry entry)
        {
            RemotingConfigHandler.RegisterWellKnownClientType(entry);

            // all registrations for wellknown client types will come through here
            RemotingServices.InternalSetRemoteActivationConfigured();
        } // RegisterWellKnownClientType


        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
        {
            return RemotingConfigHandler.GetRegisteredActivatedServiceTypes();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
        {
            return RemotingConfigHandler.GetRegisteredWellKnownServiceTypes();
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
        {
            return RemotingConfigHandler.GetRegisteredActivatedClientTypes();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
        {
            return RemotingConfigHandler.GetRegisteredWellKnownClientTypes();
        }
        
        
        // This is used at the client end to check if an activation needs
        // to go remote.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(Type svrType)
        {
            if (svrType == null)
                throw new ArgumentNullException("svrType");

            RuntimeType rt = svrType as RuntimeType;
            if (rt == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            return RemotingConfigHandler.IsRemotelyActivatedClientType(rt);
        }

        // This is used at the client end to check if an activation needs
        // to go remote.

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(String typeName, String assemblyName)
        {
            return RemotingConfigHandler.IsRemotelyActivatedClientType(typeName, assemblyName);
        }


        // This is used at the client end to check if a "new Foo" needs to
        // happen via a Connect() under the covers.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry IsWellKnownClientType(Type svrType)
        {
            if (svrType == null)
                throw new ArgumentNullException("svrType");

            RuntimeType rt = svrType as RuntimeType;
            if (rt == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            return RemotingConfigHandler.IsWellKnownClientType(rt);
        }

        // This is used at the client end to check if a "new Foo" needs to
        // happen via a Connect() under the covers.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static WellKnownClientTypeEntry IsWellKnownClientType(String typeName, 
                                                                       String assemblyName)
        {
            return RemotingConfigHandler.IsWellKnownClientType(typeName, assemblyName);
        }

        // This is used at the server end to check if a type being activated
        // is explicitly allowed by the server.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static bool IsActivationAllowed(Type svrType)
        {
            RuntimeType rt = svrType as RuntimeType;
            if (svrType != null && rt == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            return RemotingConfigHandler.IsActivationAllowed(rt);        
        }

    } // class Configuration



    //
    // The following classes are used to register and retrieve remoted type information
    //

    // Base class for all configuration entries
[System.Runtime.InteropServices.ComVisible(true)]
    public class TypeEntry
    {
        String _typeName;
        String _assemblyName;
        RemoteAppEntry _cachedRemoteAppEntry = null;

        protected TypeEntry()
        {
            // Forbid creation of this class by outside users...
        }

        public String TypeName { get { return _typeName; } set {_typeName = value;} }

        public String AssemblyName { get { return _assemblyName; } set {_assemblyName = value;} }
        
        internal void CacheRemoteAppEntry(RemoteAppEntry entry) {_cachedRemoteAppEntry = entry;}
        internal RemoteAppEntry GetRemoteAppEntry() { return _cachedRemoteAppEntry;}

    }

[System.Runtime.InteropServices.ComVisible(true)]
    public class ActivatedClientTypeEntry : TypeEntry
    {
        String _appUrl;  // url of application to activate the type in

        // optional data
        IContextAttribute[] _contextAttributes = null;
        

        public ActivatedClientTypeEntry(String typeName, String assemblyName, String appUrl)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            if (appUrl == null)
                throw new ArgumentNullException("appUrl");
            Contract.EndContractBlock();
        
            TypeName = typeName;
            AssemblyName = assemblyName;
            _appUrl = appUrl;
        } // ActivatedClientTypeEntry

        public ActivatedClientTypeEntry(Type type, String appUrl)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (appUrl == null)
                throw new ArgumentNullException("appUrl");
            Contract.EndContractBlock();

            RuntimeType rtType = type as RuntimeType;
            if (rtType == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));

            TypeName = type.FullName;
            AssemblyName = rtType.GetRuntimeAssembly().GetSimpleName();
            _appUrl = appUrl;
        } // ActivatedClientTypeEntry

        public String ApplicationUrl { get { return _appUrl; } }
        
        public Type ObjectType
        {            
            [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
            get {
                StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
                return RuntimeTypeHandle.GetTypeByName(TypeName + ", " + AssemblyName, ref stackMark);
            }
        }

        public IContextAttribute[] ContextAttributes
        {
            get { return _contextAttributes; }
            set { _contextAttributes = value; }
        }


        public override String ToString()
        {
            return "type='" + TypeName + ", " + AssemblyName + "'; appUrl=" + _appUrl;
        }        
        
    } // class ActivatedClientTypeEntry


[System.Runtime.InteropServices.ComVisible(true)]
    public class ActivatedServiceTypeEntry : TypeEntry
    {
        // optional data
        IContextAttribute[] _contextAttributes = null;
        

        public ActivatedServiceTypeEntry(String typeName, String assemblyName)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            Contract.EndContractBlock();
            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public ActivatedServiceTypeEntry(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            Contract.EndContractBlock();

            RuntimeType rtType = type as RuntimeType;
            if (rtType == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));

            TypeName = type.FullName;
            AssemblyName = rtType.GetRuntimeAssembly().GetSimpleName();
        }
        
        public Type ObjectType
        {            
            [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
            get {
                StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
                return RuntimeTypeHandle.GetTypeByName(TypeName + ", " + AssemblyName, ref stackMark);
            }
        }

        public IContextAttribute[] ContextAttributes
        {
            get { return _contextAttributes; }
            set { _contextAttributes = value; }
        }


        public override String ToString()
        {
            return "type='" + TypeName + ", " + AssemblyName + "'";
        }
        
    } // class ActivatedServiceTypeEntry


[System.Runtime.InteropServices.ComVisible(true)]
    public class WellKnownClientTypeEntry : TypeEntry
    {   
        String _objectUrl; 

        // optional data
        String _appUrl = null; // url of application to associate this object with
        

        public WellKnownClientTypeEntry(String typeName, String assemblyName, String objectUrl)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            if (objectUrl == null)
                throw new ArgumentNullException("objectUrl");
            Contract.EndContractBlock();
        
            TypeName = typeName;
            AssemblyName = assemblyName;
            _objectUrl = objectUrl;
        }

        public WellKnownClientTypeEntry(Type type, String objectUrl)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (objectUrl == null)
                throw new ArgumentNullException("objectUrl");
            Contract.EndContractBlock();

            RuntimeType rtType = type as RuntimeType;
            if (rtType == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            TypeName = type.FullName;
            AssemblyName = rtType.GetRuntimeAssembly().GetSimpleName();
            _objectUrl = objectUrl;
        }

        public String ObjectUrl { get { return _objectUrl; } }
        
        public Type ObjectType
        {            
            [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
            get {
                StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
                return RuntimeTypeHandle.GetTypeByName(TypeName + ", " + AssemblyName, ref stackMark);
            }
        }

        public String ApplicationUrl
        {
            get { return _appUrl; }
            set { _appUrl = value; }
        }

        public override String ToString()
        {
            String str = "type='" + TypeName + ", " + AssemblyName + "'; url=" + _objectUrl;
            if (_appUrl != null)
                str += "; appUrl=" + _appUrl;
            return str;
        }
        
    } // class WellKnownClientTypeEntry


[System.Runtime.InteropServices.ComVisible(true)]
    public class WellKnownServiceTypeEntry : TypeEntry
    {
        String _objectUri;
        WellKnownObjectMode _mode;

        // optional data
        IContextAttribute[] _contextAttributes = null;

        public WellKnownServiceTypeEntry(String typeName, String assemblyName, String objectUri,
                                         WellKnownObjectMode mode)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            if (objectUri == null)
                throw new ArgumentNullException("objectUri");
            Contract.EndContractBlock();
        
            TypeName = typeName;
            AssemblyName = assemblyName;
            _objectUri = objectUri;
            _mode = mode;
        }

        public WellKnownServiceTypeEntry(Type type, String objectUri, WellKnownObjectMode mode)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (objectUri == null)
                throw new ArgumentNullException("objectUri");
            Contract.EndContractBlock();
        
            if (!(type is RuntimeType))
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            TypeName = type.FullName;
            AssemblyName = type.Module.Assembly.FullName;
            _objectUri = objectUri;
            _mode = mode;
        }

        public String ObjectUri { get { return _objectUri; } }

        public WellKnownObjectMode Mode { get { return _mode; } }

        public Type ObjectType
        {
            [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
            get {
                StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
                return RuntimeTypeHandle.GetTypeByName(TypeName + ", " + AssemblyName, ref stackMark);
            }
        }

        public IContextAttribute[] ContextAttributes
        {
            get { return _contextAttributes; }
            set { _contextAttributes = value; }
        }


        public override String ToString()
        {
            return "type='" + TypeName + ", " + AssemblyName + "'; objectUri=" + _objectUri + 
                "; mode=" + _mode.ToString();
        }

    } // class WellKnownServiceTypeEntry

    internal class RemoteAppEntry
    {
        String _remoteAppName;
        String _remoteAppURI;
        internal RemoteAppEntry(String appName, String appURI)
        {
            Contract.Assert(appURI != null, "Bad remote app URI");
            _remoteAppName = appName;
            _remoteAppURI = appURI;
        }
        internal String GetAppURI() { return _remoteAppURI;}
    } // class RemoteAppEntry

[System.Runtime.InteropServices.ComVisible(true)]
    public enum CustomErrorsModes {
        On,
        Off,
        RemoteOnly
    }

} // namespace System.Runtime.Remoting 
