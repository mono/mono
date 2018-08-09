// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Microsoft.Win32
{
#if REGISTRY_ASSEMBLY
    public
#else
    internal
#endif
    sealed partial class RegistryKey : MarshalByRefObject, IDisposable
    {
        object hive, handle;
        static readonly UnixRegistryApi RegistryApi;
        readonly bool isRemoteRoot;	// is an instance of a remote root key?

        static RegistryKey()
        {
            RegistryApi = new UnixRegistryApi();
        }

        internal RegistryKey(RegistryHive hiveId) : this(hiveId,
            new IntPtr((int)hiveId), false)
        {
        }

        internal RegistryKey(RegistryHive hiveId, IntPtr keyHandle, bool remoteRoot)
        {
            hive = hiveId;
            handle = keyHandle;
            _keyName = GetHiveName(hiveId);
            isRemoteRoot = remoteRoot;
            _state = StateFlags.WriteAccess;
            _hkey = new SafeRegistryHandle();
        }

        internal RegistryKey(object data, string keyName, bool writable)
        {
            _hkey = new SafeRegistryHandle();
            handle = data;
            _keyName = keyName;
            if (writable)
                _state = StateFlags.WriteAccess;
        }

        private void ClosePerfDataKey()
        {
            FlushCore();
        }

        private void FlushCore()
        {
            RegistryApi.Flush(this);
        }

        private RegistryKey CreateSubKeyInternalCore(string subkey, RegistryKeyPermissionCheck permissionCheck, object registrySecurityObj, RegistryOptions registryOptions)
        {
            return RegistryApi.CreateSubKey(this, subkey, registryOptions);
        }

        private void DeleteSubKeyCore(string subkey, bool throwOnMissingSubKey)
        {
            RegistryKey child = OpenSubKey(subkey);

            if (child == null)
            {
                if (throwOnMissingSubKey)
                    throw new ArgumentException("Cannot delete a subkey tree because the subkey does not exist.");
                return;
            }

            if (child.SubKeyCount > 0)
            {
                throw new InvalidOperationException("Registry key has subkeys and recursive removes are not supported by this method.");
            }

            child.Close();

            RegistryApi.DeleteKey(this, subkey, throwOnMissingSubKey);
        }

        private void DeleteSubKeyTreeCore(string subkey)
        {
            RegistryKey child = OpenSubKey(subkey, true);
            child.DeleteChildKeysAndValues();
            child.Close();
            DeleteSubKey(subkey, false);
        }

        private void DeleteChildKeysAndValues()
        {
            if (IsRoot)
                return;

            string[] subKeys = GetSubKeyNames();
            foreach (string subKey in subKeys)
            {
                RegistryKey sub = OpenSubKey(subKey, true);
                sub.DeleteChildKeysAndValues();
                sub.Close();
                DeleteSubKey(subKey, false);
            }

            string[] values = GetValueNames();
            foreach (string value in values)
            {
                DeleteValue(value, false);
            }
        }

        private void DeleteValueCore(string name, bool throwOnMissingValue)
        {
            RegistryApi.DeleteValue(this, name, throwOnMissingValue);
        }

        private static RegistryKey OpenBaseKeyCore(RegistryHive hKey, RegistryView view)
        {
            return new RegistryKey(hKey) { _regView = view };
        }

        private static RegistryKey OpenRemoteBaseKeyCore(RegistryHive hKey, string machineName, RegistryView view)
        {
            if (machineName == null)
                throw new ArgumentNullException("machineName");
            return RegistryApi.OpenRemoteBaseKey(hKey, machineName);
        }

        private RegistryKey InternalOpenSubKeyCore(string name, RegistryKeyPermissionCheck permissionCheck, int rights, bool throwOnPermissionFailure)
        {
            return RegistryApi.OpenSubKey(this, name, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree);
        }

        private RegistryKey InternalOpenSubKeyCore(string name, bool writable, bool throwOnPermissionFailure)
        {
            return RegistryApi.OpenSubKey(this, name, writable);
        }

        internal RegistryKey InternalOpenSubKeyWithoutSecurityChecksCore(string name, bool writable)
        {
            return RegistryApi.OpenSubKey(this, name, writable);
        }

        private SafeRegistryHandle SystemKeyHandle => throw new PlatformNotSupportedException(SR.PlatformNotSupported_Registry);

        private int InternalSubKeyCountCore()
        {
            return RegistryApi.SubKeyCount(this);
        }

        private string[] InternalGetSubKeyNamesCore(int subkeys)
        {
            return RegistryApi.GetSubKeyNames(this);
        }

        private int InternalValueCountCore()
        {
            return RegistryApi.ValueCount(this);
        }

        private string[] GetValueNamesCore(int values)
        {
            return RegistryApi.GetValueNames(this);
        }

        private Object InternalGetValueCore(string name, Object defaultValue, bool doNotExpand)
        {
            return RegistryApi.GetValue(this, name, defaultValue, doNotExpand ? RegistryValueOptions.DoNotExpandEnvironmentNames : RegistryValueOptions.None);
        }

        private RegistryValueKind GetValueKindCore(string name)
        {
            return RegistryApi.GetValueKind(this, name);
        }

        private void SetValueCore(string name, Object value, RegistryValueKind valueKind)
        {
            RegistryApi.SetValue(this, name, value, valueKind);
        }

        private static int GetRegistryKeyAccess(bool isWritable)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_Registry);
        }

        private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_Registry);
        }

        internal static IOException CreateMarkedForDeletionException()
        {
            throw new IOException("Illegal operation attempted on a registry key that has been marked for deletion.");
        }
        internal static bool IsEquals(RegistryKey a, RegistryKey b)
        {
            return a.hive == b.hive && a._keyName == b._keyName && a._remoteKey == b._remoteKey && a.IsWritable() == b.IsWritable();
        }

        internal bool IsRoot => hive != null;

        internal RegistryHive Hive
        {
            get
            {
                if (!IsRoot)
                    throw new NotSupportedException();
                return (RegistryHive)hive;
            }
        }

        static string GetHiveName(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentConfig:
                    return "HKEY_CURRENT_CONFIG";
                case RegistryHive.CurrentUser:
                    return "HKEY_CURRENT_USER";
                //case RegistryHive.DynData:
                //    return "HKEY_DYN_DATA";
                case RegistryHive.LocalMachine:
                    return "HKEY_LOCAL_MACHINE";
                case RegistryHive.PerformanceData:
                    return "HKEY_PERFORMANCE_DATA";
                case RegistryHive.Users:
                    return "HKEY_USERS";
            }

            throw new NotImplementedException(string.Format(
                "Registry hive '{0}' is not implemented.", hive.ToString()));
        }

        public override int GetHashCode()
        {
            if (_keyName != null)
                return _keyName.GetHashCode();
            return base.GetHashCode();
        }
    }
}
