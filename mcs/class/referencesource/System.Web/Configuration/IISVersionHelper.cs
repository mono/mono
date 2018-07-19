//------------------------------------------------------------------------------
// <copyright file="IISVersionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;
namespace System.Web.Configuration {
    internal class IISVersionHelper : IDisposable {
        const int IIS_PRODUCT_EXPRESS = 2;

        [ComImport, Guid("1B036F99-B240-4116-A6A0-B54EC5B2438E"), InterfaceType((short)1)]
        interface IIISVersion {
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPropertyValue([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            [return: MarshalAs(UnmanagedType.Struct)]
            object CreateObjectFromProgId([In, MarshalAs(UnmanagedType.BStr)] string bstrObjectName);
            [return: MarshalAs(UnmanagedType.Struct)]
            object CreateObjectFromClsId([In] Guid clsidObject);
            void ApplyIISEnvironmentVariables();
            void ClearIISEnvironmentVariables();
            void ApplyManifestContext();
            void ClearManifestContext();
        }

        [ComImport, InterfaceType((short)1), Guid("9CDA0717-2EB5-42b3-B5B0-16F4941B2029")]
        interface IIISVersionManager {
            [return: MarshalAs(UnmanagedType.Interface)]
            IIISVersion GetVersionObject([In, MarshalAs(UnmanagedType.BStr)] string bstrVersion, [In, MarshalAs(UnmanagedType.I4)] int productType);
            [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            IIISVersion[] GetAllVersionObjects();
        }

        IIISVersionManager _versionManager;
        IIISVersion _version;

        internal IISVersionHelper(string version) {
            // version is null if we're supposed to use the OS's IIS installation
            if (version == null)
                return;

            try {
                _versionManager = CreateVersionManager();
                _version = _versionManager.GetVersionObject(version, IIS_PRODUCT_EXPRESS);
                _version.ApplyManifestContext();
            }
            catch {
                Release();
                throw;
            }
        }

        private static IIISVersionManager CreateVersionManager() {
            Type type = Type.GetTypeFromProgID("Microsoft.IIS.VersionManager", throwOnError: true);
            return (IIISVersionManager)Activator.CreateInstance(type);
        }

        public void Dispose() {
            if (_version != null) {
                _version.ClearManifestContext();
                Release();
            }
        }

        private void Release() {
            if (_version != null) {
                Marshal.ReleaseComObject(_version);
                _version = null;
            }
            if (_versionManager != null) {
                Marshal.ReleaseComObject(_versionManager);
                _versionManager = null;
            }
        }
    }
}
