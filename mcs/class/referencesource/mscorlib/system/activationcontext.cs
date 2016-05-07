using System;
using System.Collections;
using System.Deployment.Internal;
using System.Deployment.Internal.Isolation;
using System.Deployment.Internal.Isolation.Manifest;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.Versioning;
using System.Diagnostics.Contracts;
using System.Security;

namespace System
{
    // Public surface area for Whidbey:
    //   AppDomain.ActivationContext
    //   AppDomain.ActivationContext.Identity
    //   AppDomain.ActivationContext.Identity.FullName
    //   AppDomain.ActivationContext.Identity.CodeBase

    //   static ActivationContext.CreatePartialActivationContext(identity)
    //   static ActivationContext.CreatePartialActivationContext(identity, manifestPaths[])

    //   ActivationContext class
    //     ActivationContext.Identity
    //   ApplicationIdentity class
    //     ApplicationIdentity.FullName
    //     ApplicationIdentity.CodeBase

    //   + existing AppDomain.BaseDirectory: local app directory

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public sealed class ActivationContext : IDisposable, ISerializable
    {
        private ApplicationIdentity _applicationIdentity;
        // ISSUE - can use Generic lists.
        private ArrayList _definitionIdentities;
        private ArrayList _manifests;
        private string[] _manifestPaths;
        private ContextForm _form;
        private ApplicationStateDisposition _appRunState;

        private IActContext _actContext;

        private const int DefaultComponentCount = 2;

        private ActivationContext () {}

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [SecurityCritical]
        private ActivationContext (SerializationInfo info, StreamingContext context)
        {
            string fullName = (string) info.GetValue("FullName", typeof(string));
            string[] manifestPaths = (string[]) info.GetValue("ManifestPaths", typeof(string[]));
            if (manifestPaths == null)
                CreateFromName(new ApplicationIdentity(fullName));
            else
                CreateFromNameAndManifests(new ApplicationIdentity(fullName), manifestPaths);
        }

        internal ActivationContext(ApplicationIdentity applicationIdentity)
        {
            CreateFromName(applicationIdentity);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal ActivationContext(ApplicationIdentity applicationIdentity, string[] manifestPaths)
        {
            CreateFromNameAndManifests(applicationIdentity, manifestPaths);
        }

        [SecuritySafeCritical]
        private void CreateFromName (ApplicationIdentity applicationIdentity)
        {
            if (applicationIdentity == null)
                throw new ArgumentNullException("applicationIdentity");
            Contract.EndContractBlock();

            _applicationIdentity = applicationIdentity;

            IEnumDefinitionIdentity idenum = _applicationIdentity.Identity.EnumAppPath();

            _definitionIdentities = new ArrayList(DefaultComponentCount);

            IDefinitionIdentity[] asbId = new IDefinitionIdentity[1];
            while (idenum.Next(1, asbId) == 1)
            {
                _definitionIdentities.Add(asbId[0]);
            }
            _definitionIdentities.TrimToSize();
            if (_definitionIdentities.Count <= 1)
            {
#if ISOLATION_IN_MSCORLIB
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppId"));
#else
                throw new ArgumentException("Invalid identity: no deployment/app identity specified");
#endif
            }

            _manifestPaths = null;
            _manifests = null;

            // Construct real IActContext from store.
            _actContext = IsolationInterop.CreateActContext(_applicationIdentity.Identity);
            _form = ContextForm.StoreBounded;
            _appRunState = ApplicationStateDisposition.Undefined;

#if ISOLATION_IN_MSCORLIB
            Contract.Assert(_definitionIdentities.Count == 2, "An application must have exactly 1 deployment component and 1 application component in Whidbey");
#endif
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [SecuritySafeCritical]
        private void CreateFromNameAndManifests (ApplicationIdentity applicationIdentity, string[] manifestPaths)
        {
            if (applicationIdentity == null)
                throw new ArgumentNullException("applicationIdentity");
            if (manifestPaths == null)
                throw new ArgumentNullException("manifestPaths");
            Contract.EndContractBlock();

            _applicationIdentity = applicationIdentity;

            // ISSUE - need validation on manifestPaths

            IEnumDefinitionIdentity idenum = _applicationIdentity.Identity.EnumAppPath();

            _manifests = new ArrayList(DefaultComponentCount);
            _manifestPaths = new String[manifestPaths.Length];

            IDefinitionIdentity[] asbId = new IDefinitionIdentity[1];
            int i=0;
            while (idenum.Next(1, asbId) == 1)
            {
                ICMS cms = (ICMS) IsolationInterop.ParseManifest(manifestPaths[i], null, ref IsolationInterop.IID_ICMS);

                if (IsolationInterop.IdentityAuthority.AreDefinitionsEqual(0, cms.Identity, asbId[0]))
                {
                    _manifests.Add(cms);
                    _manifestPaths[i]=manifestPaths[i];
                }
                else
                {
#if ISOLATION_IN_MSCORLIB
                    throw new ArgumentException(Environment.GetResourceString("Argument_IllegalAppIdMismatch"));
#else
                    throw new ArgumentException("Application Identity does not match identity in manifests");
#endif
                }
                i++;
            }
            if (i!=manifestPaths.Length)
            {
#if ISOLATION_IN_MSCORLIB
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalAppId"));
#else
                throw new ArgumentException("Application Identity does not have same number of components as manifest paths");
#endif
            }
            _manifests.TrimToSize();
            if (_manifests.Count <= 1)
            {
#if ISOLATION_IN_MSCORLIB
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppId"));
#else
                throw new ArgumentException("Invalid identity: no deployment/app identity specified");
#endif
            }

            _definitionIdentities = null;
            _actContext = null;
            _form = ContextForm.Loose;
            _appRunState = ApplicationStateDisposition.Undefined;

#if ISOLATION_IN_MSCORLIB
            Contract.Assert(_manifests.Count == 2, "An application must have exactly 1 deployment component and 1 application component in Whidbey");
#endif
        }

        ~ActivationContext()
        {
            Dispose(false);
        }

        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity)
        {
            return new ActivationContext(identity);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static ActivationContext CreatePartialActivationContext(ApplicationIdentity identity, string[] manifestPaths)
        {
            return new ActivationContext(identity, manifestPaths);
        }

        public ApplicationIdentity Identity
        {
            get
            {
                return _applicationIdentity;
            }
        }

        public ContextForm Form
        {
            get
            {
                return _form;
            }
        }

       public byte[] ApplicationManifestBytes
        {
            get
            {
                return GetApplicationManifestBytes();
            }
        }

        public byte[] DeploymentManifestBytes
        {
            get
            {
                return GetDeploymentManifestBytes();
            }            
        }
        
        internal string[] ManifestPaths
        {
            [ResourceExposure(ResourceScope.Machine)]
            get
            {
                return _manifestPaths;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Nested Enum.
        public enum ContextForm
        {
            Loose = 0,
            StoreBounded = 1
        }

        // Internals.

        internal string ApplicationDirectory
        {
            [ResourceExposure(ResourceScope.Machine)]
            [SecurityCritical]
            get
            {
                if (_form == ContextForm.Loose)
                    return Path.GetDirectoryName(_manifestPaths[_manifestPaths.Length-1]);

                string s;
                _actContext.ApplicationBasePath(0, out s);
                return s;
            }
        }

        internal string DataDirectory
        {
            [ResourceExposure(ResourceScope.Machine)]
            [SecurityCritical]
            get
            {
                // ISSUE - what is the data directory for 'loose'?
                if (_form == ContextForm.Loose)
                    return null;

                string s;
                // Note: passing in flag == 1.
                _actContext.GetApplicationStateFilesystemLocation(1, UIntPtr.Zero, IntPtr.Zero, out s);
                return s;
            }
        }

        // These internal methods to become public in Longhorn, when manifest APIs become public

        // in LH M7 this will be come IACS instead of ICMS.
        internal ICMS ActivationContextData
        {
            [SecurityCritical]
            get
            {
                // this will be a true merge of deployment and application manifest contents (ACS, not CMS)
                // for now return only the contents of the application manifest, as most consumers only care about app manifest anyway
                return this.ApplicationComponentManifest;
            }
        }

        // Return the manifest of the first deployment component.
        internal ICMS DeploymentComponentManifest
        {
            [SecurityCritical]
            get
            {
                if (_form == ContextForm.Loose)
                    return (ICMS) _manifests[0];

                return GetComponentManifest((IDefinitionIdentity)_definitionIdentities[0]);
            }
        }

        internal ICMS ApplicationComponentManifest
        {
            [SecurityCritical]
            get
            {
                if (_form == ContextForm.Loose)
                    return (ICMS) _manifests[_manifests.Count-1];

                return GetComponentManifest((IDefinitionIdentity)_definitionIdentities[_definitionIdentities.Count-1]);
            }
        }

        internal ApplicationStateDisposition LastApplicationStateResult
        {
            get
            {
                // Return cached result.
                return _appRunState;
            }
        }

        [SecurityCritical]
        internal ICMS GetComponentManifest(IDefinitionIdentity component)
        {
            object o;
            _actContext.GetComponentManifest(0, component, ref IsolationInterop.IID_ICMS, out o);
            return o as ICMS;
        }

        [SecuritySafeCritical]
        internal byte[] GetDeploymentManifestBytes()
        {
            object o;
            string manifestPath;
            
            if (_form == ContextForm.Loose)
                manifestPath = _manifestPaths[0];
            else
            {
                _actContext.GetComponentManifest(0, (IDefinitionIdentity)_definitionIdentities[0], ref IsolationInterop.IID_IManifestInformation, out o);
                ((IManifestInformation)o).get_FullPath(out manifestPath);
                Marshal.ReleaseComObject(o);               
            }

            return ReadBytesFromFile(manifestPath);           
        }
        
        [SecuritySafeCritical]
        internal byte[] GetApplicationManifestBytes()
        {
            object o;
            string manifestPath;
            
            if (_form == ContextForm.Loose)
                manifestPath = _manifestPaths[_manifests.Count-1];
            else
            {
                _actContext.GetComponentManifest(0, (IDefinitionIdentity)_definitionIdentities[1], ref IsolationInterop.IID_IManifestInformation, out o);                           
                ((IManifestInformation)o).get_FullPath(out manifestPath);
                Marshal.ReleaseComObject(o);                               
            }

            return ReadBytesFromFile(manifestPath);
        }

        // Internal methods used exclusively by application hosting code.

        [SecuritySafeCritical]
        internal void PrepareForExecution()
        {
            if (_form == ContextForm.Loose)
                return; // Do nothing.

            _actContext.PrepareForExecution(IntPtr.Zero, IntPtr.Zero);
        }

        [SecuritySafeCritical]
        internal ApplicationStateDisposition SetApplicationState(ApplicationState s)
        {
            if (_form == ContextForm.Loose)
                return ApplicationStateDisposition.Undefined; // Do nothing.

            UInt32 disposition;
            _actContext.SetApplicationRunningState(0, (UInt32)s, out disposition);

            // Save the result.
            _appRunState = (ApplicationStateDisposition)disposition;

            return _appRunState;
        }

        // Nested Enum.
        internal enum ApplicationState
        {
            Undefined = 0,
            Starting = 1,
            Running = 2
        }

        internal enum ApplicationStateDisposition
        {
            Undefined = 0,
            Starting = 1,
            StartingMigrated = (1 | (1 << 16)),
            Running = 2,
            RunningFirstTime = (2 | (1 << 17))
        }

        // Privates.

        [System.Security.SecuritySafeCritical]  // auto-generated
        private void Dispose(bool fDisposing)
        {
            // ISSUE- should release unmanaged objects in array lists.
            _applicationIdentity = null;
            _definitionIdentities = null;
            _manifests = null;
            _manifestPaths = null;

            if (_actContext != null)
                Marshal.ReleaseComObject(_actContext);
        }

        private static byte[] ReadBytesFromFile(string manifestPath)
        {
            byte[] rawBytes = null;
            
            using (FileStream fs = new FileStream(manifestPath, FileMode.Open, FileAccess.Read))
            {
                    int bufferSize = (int)fs.Length;
     
                    // zero length file will except ultimately.
                    rawBytes = new byte[bufferSize];
     
                    if (fs.CanSeek)
                    {
                       fs.Seek(0, SeekOrigin.Begin);
                    }
     
                    // Read the file into buffer.
                    fs.Read(rawBytes, 0, bufferSize);
            }

            return rawBytes;
            
        }

        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_applicationIdentity != null)
                info.AddValue("FullName", _applicationIdentity.FullName, typeof(String));
            if (_manifestPaths != null)
                info.AddValue("ManifestPaths", _manifestPaths, typeof(String[]));
        }
    }

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public sealed class ApplicationIdentity : ISerializable {
        private IDefinitionAppId _appId;

        private ApplicationIdentity () {}
        [SecurityCritical]
        private ApplicationIdentity (SerializationInfo info, StreamingContext context)
        {
            string fullName = (string) info.GetValue("FullName", typeof(string));
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            _appId = IsolationInterop.AppIdAuthority.TextToDefinition(0, fullName);
        }

        [SecuritySafeCritical]
        public ApplicationIdentity(String applicationIdentityFullName)
        {
            if (applicationIdentityFullName == null)
                throw new ArgumentNullException("applicationIdentityFullName");
            Contract.EndContractBlock();
            _appId = IsolationInterop.AppIdAuthority.TextToDefinition(0, applicationIdentityFullName);
        }

        [SecurityCritical]
        internal ApplicationIdentity(IDefinitionAppId applicationIdentity)
        {
            // ISSUE- this should clone the IDefintionAppId.
            _appId = applicationIdentity;
        }

        public String FullName
        {
            [SecuritySafeCritical]
            get
            {
                return IsolationInterop.AppIdAuthority.DefinitionToText(0, _appId);
            }
        }

        public String CodeBase
        {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            [SecuritySafeCritical]
            get
            {
                return _appId.get_Codebase();
            }
        }

        public override string ToString()
        {
            return this.FullName;
        }

        internal IDefinitionAppId Identity
        {
            [SecurityCritical]
            get
            {
                return _appId;
            }
        }

        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FullName", FullName, typeof(String));
        }
    }
}

namespace System.Deployment.Internal
{
    // All classes in this namespace are only usable within CLR (enforced with Link demand/friend etc.)
    // This class will go away in LH, when the ApplicationDefinitionIdentity class exposes the functionality directly
    [System.Runtime.InteropServices.ComVisible(false)]
    public static class InternalApplicationIdentityHelper
    {
        [SecurityCritical]
        public static object /* really IDefinitionAppId */ GetInternalAppId(ApplicationIdentity id)
        {
            return id.Identity;
        }
    }

    // This helper class will go away in LH, when the ActivationContext class exposes the functionality directly

    [System.Runtime.InteropServices.ComVisible(false)]
    public static class InternalActivationContextHelper
    {
        [SecuritySafeCritical]
        public static object /* really ICMS */ GetActivationContextData(ActivationContext appInfo)
        {
            return appInfo.ActivationContextData;
        }

        [SecuritySafeCritical]
        public static object GetApplicationComponentManifest(ActivationContext appInfo)
        {
            return appInfo.ApplicationComponentManifest;
        }

        [SecuritySafeCritical]
        public static object GetDeploymentComponentManifest(ActivationContext appInfo)
        {
            return appInfo.DeploymentComponentManifest;
        }

        public static void PrepareForExecution(ActivationContext appInfo)
        {
            appInfo.PrepareForExecution();
        }

        public static bool IsFirstRun(ActivationContext appInfo)
        {
            return (appInfo.LastApplicationStateResult == ActivationContext.ApplicationStateDisposition.RunningFirstTime);
        }

        public static byte[] GetApplicationManifestBytes(ActivationContext appInfo)
        {
            if (appInfo == null)
                throw new ArgumentNullException("appInfo");
                
            return appInfo.GetApplicationManifestBytes();
        }


        public static byte[] GetDeploymentManifestBytes(ActivationContext appInfo)
        {
            if (appInfo == null)
                throw new ArgumentNullException("appInfo");

            return appInfo.GetDeploymentManifestBytes();
        }
  }
}

