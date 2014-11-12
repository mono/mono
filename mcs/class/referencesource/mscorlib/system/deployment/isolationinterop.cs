#if !ISOLATION_IN_MSCORLIB
#define FEATURE_COMINTEROP
#endif

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

using CMS=System.Deployment.Internal.Isolation.Manifest;

namespace System.Deployment.Internal.Isolation
{
#if FEATURE_COMINTEROP
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLOB : IDisposable
    {
        [MarshalAs(UnmanagedType.U4)] public uint Size;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr BlobData;

        [System.Security.SecuritySafeCritical]  // auto-generated
        public void Dispose()
        {
            if (BlobData != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(BlobData);
                BlobData = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IDENTITY_ATTRIBUTE
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string Namespace;
        [MarshalAs(UnmanagedType.LPWStr)] public string Name;
        [MarshalAs(UnmanagedType.LPWStr)] public string Value;
    }

    [Flags]
    internal enum STORE_ASSEMBLY_STATUS_FLAGS
    {
        STORE_ASSEMBLY_STATUS_MANIFEST_ONLY     = 0x00000001,
        STORE_ASSEMBLY_STATUS_PAYLOAD_RESIDENT  = 0x00000002,
        STORE_ASSEMBLY_STATUS_PARTIAL_INSTALL   = 0x00000004,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_ASSEMBLY
    {
        public uint Status;
        public IDefinitionIdentity DefinitionIdentity;
        [MarshalAs(UnmanagedType.LPWStr)] public string ManifestPath;
        public ulong AssemblySize;
        public ulong ChangeId;
    }

    [Flags]
    internal enum STORE_ASSEMBLY_FILE_STATUS_FLAGS
    {
        STORE_ASSEMBLY_FILE_STATUS_FLAG_PRESENT = 0x00000001,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_ASSEMBLY_FILE
    {
        public uint Size;
        public uint Flags;
        [MarshalAs(UnmanagedType.LPWStr)] public string FileName;
        public uint FileStatusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_CATEGORY
    {
        public IDefinitionIdentity DefinitionIdentity;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_CATEGORY_SUBCATEGORY
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string Subcategory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_CATEGORY_INSTANCE
    {
        public IDefinitionAppId DefinitionAppId_Application;
        [MarshalAs(UnmanagedType.LPWStr)] public string XMLSnippet;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CATEGORY
    {
        public IDefinitionIdentity DefinitionIdentity;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CATEGORY_SUBCATEGORY
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string Subcategory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CATEGORY_INSTANCE
    {
        public IDefinitionAppId DefinitionAppId_Application;
        [MarshalAs(UnmanagedType.LPWStr)] public string XMLSnippet;
    }

    [ComImport]
    [Guid("d8b1aacb-5142-4abb-bcc1-e9dc9052a89e")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] StoreApplicationReference[] rgelt
            /*[Out, Optional] out uint Fetched*/
            );
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE Clone();
    }

    [ComImport]
    [Guid("f9fd4090-93db-45c0-af87-624940f19cff")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_DEPLOYMENT_METADATA
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] IDefinitionAppId[] AppIds
            );

        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_DEPLOYMENT_METADATA Clone();
    };

    internal class StoreDeploymentMetadataEnumeration : IEnumerator
    {
        private IEnumSTORE_DEPLOYMENT_METADATA _enum = null;
        bool _fValid = false;
        IDefinitionAppId _current;

        public StoreDeploymentMetadataEnumeration(IEnumSTORE_DEPLOYMENT_METADATA pI)
        {
            _enum = pI;
        }

        private IDefinitionAppId GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public IDefinitionAppId Current { get { return GetCurrent(); } }

        public IEnumerator GetEnumerator() { return this; }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            IDefinitionAppId[] next = new IDefinitionAppId[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }

    [ComImport]
    [Guid("5fa4f590-a416-4b22-ac79-7c3f0d31f303")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] StoreOperationMetadataProperty[] AppIds
            );

        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY Clone();
    };

    internal class StoreDeploymentMetadataPropertyEnumeration : IEnumerator
    {
        private IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY _enum = null;
        bool _fValid = false;
        StoreOperationMetadataProperty _current;

        public StoreDeploymentMetadataPropertyEnumeration(IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY pI)
        {
            _enum = pI;
        }

        private StoreOperationMetadataProperty GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public StoreOperationMetadataProperty Current { get { return GetCurrent(); } }

        public IEnumerator GetEnumerator() { return this; }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            StoreOperationMetadataProperty[] next = new StoreOperationMetadataProperty[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }

    //
    // Unmanaged and managed versions of assembly enumeration
    //
    [ComImport]
    [Guid("a5c637bf-6eaa-4e5f-b535-55299657e33e")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_ASSEMBLY
    {
        [SecurityCritical]
        uint Next(
            [In] UInt32 celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] STORE_ASSEMBLY[] rgelt
            /*[Out, Optional] UInt32 *pceltFetched*/
            );
        [SecurityCritical]
        void Skip([In] UInt32 celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY Clone();
    };

    internal class StoreAssemblyEnumeration : IEnumerator
    {
        private IEnumSTORE_ASSEMBLY _enum = null;
        bool _fValid = false;
        STORE_ASSEMBLY _current;

        public StoreAssemblyEnumeration(IEnumSTORE_ASSEMBLY pI)
        {
            _enum = pI;
        }

        private STORE_ASSEMBLY GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public STORE_ASSEMBLY Current { get { return GetCurrent(); } }

        public IEnumerator GetEnumerator() { return this; }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_ASSEMBLY[] next = new STORE_ASSEMBLY[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }


    //
    // Enumerating the files in an assembly
    //
    [ComImport]
    [Guid("a5c6aaa3-03e4-478d-b9f5-2e45908d5e4f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_ASSEMBLY_FILE
    {
        [SecurityCritical]
        uint Next(
            [In] UInt32 celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] STORE_ASSEMBLY_FILE[] rgelt
            /*[Out, Optional] UInt32 *pceltFetched*/
            );
        [SecurityCritical]
        void Skip([In] UInt32 celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY_FILE Clone();
    };

    internal class StoreAssemblyFileEnumeration : IEnumerator
    {
        private IEnumSTORE_ASSEMBLY_FILE _enum = null;
        bool _fValid = false;
        STORE_ASSEMBLY_FILE _current;

        public StoreAssemblyFileEnumeration(IEnumSTORE_ASSEMBLY_FILE pI)
        {
            _enum = pI;
        }

        public IEnumerator GetEnumerator() { return this; }

        private STORE_ASSEMBLY_FILE GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public STORE_ASSEMBLY_FILE Current { get { return GetCurrent(); } }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_ASSEMBLY_FILE[] next = new STORE_ASSEMBLY_FILE[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }


    //
    // Managed and unmanaged store enumeration
    //
    [ComImport]
    [Guid("b840a2f5-a497-4a6d-9038-cd3ec2fbd222")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_CATEGORY
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] STORE_CATEGORY[] rgElements
            /*[Out] out uint Fetched*/
            );

        [SecurityCritical]
        void Skip([In] uint ulElements);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_CATEGORY Clone();
    }

    internal class StoreCategoryEnumeration : IEnumerator
    {
        private IEnumSTORE_CATEGORY _enum = null;
        bool _fValid = false;
        STORE_CATEGORY _current;

        public StoreCategoryEnumeration(IEnumSTORE_CATEGORY pI)
        {
            _enum = pI;
        }

        public IEnumerator GetEnumerator() { return this; }

        private STORE_CATEGORY GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public STORE_CATEGORY Current { get { return GetCurrent(); } }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_CATEGORY[] next = new STORE_CATEGORY[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }

    //
    // Managed and unmanaged subcategory enumeration
    //
    [ComImport]
    [Guid("19be1967-b2fc-4dc1-9627-f3cb6305d2a7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_CATEGORY_SUBCATEGORY
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] STORE_CATEGORY_SUBCATEGORY[] rgElements
            /*[Out] out uint Fetched*/
            );
        [SecurityCritical]
        void Skip([In] uint ulElements);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_CATEGORY_SUBCATEGORY Clone();
    }

    internal class StoreSubcategoryEnumeration : IEnumerator
    {
        private IEnumSTORE_CATEGORY_SUBCATEGORY _enum = null;
        bool _fValid = false;
        STORE_CATEGORY_SUBCATEGORY _current;

        public StoreSubcategoryEnumeration(IEnumSTORE_CATEGORY_SUBCATEGORY pI)
        {
            _enum = pI;
        }

        public IEnumerator GetEnumerator() { return this; }

        private STORE_CATEGORY_SUBCATEGORY GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public STORE_CATEGORY_SUBCATEGORY Current { get { return GetCurrent(); } }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_CATEGORY_SUBCATEGORY[] next = new STORE_CATEGORY_SUBCATEGORY[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }

    //
    // Enumeration of instances as well
    //
    [ComImport]
    [Guid("5ba7cb30-8508-4114-8c77-262fcda4fadb")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumSTORE_CATEGORY_INSTANCE
    {
        [SecurityCritical]
        uint Next(
            [In] uint ulElements,
            [Out, MarshalAs(UnmanagedType.LPArray)] STORE_CATEGORY_INSTANCE[] rgInstances
            /*[Out] out uint Fetched*/
            );
        [SecurityCritical]
        void Skip([In] uint ulElements);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_CATEGORY_INSTANCE Clone();
    }

    internal class StoreCategoryInstanceEnumeration : IEnumerator
    {
        private IEnumSTORE_CATEGORY_INSTANCE _enum = null;
        bool _fValid = false;
        STORE_CATEGORY_INSTANCE _current;

        public StoreCategoryInstanceEnumeration(IEnumSTORE_CATEGORY_INSTANCE pI)
        {
            _enum = pI;
        }

        public IEnumerator GetEnumerator() { return this; }

        private STORE_CATEGORY_INSTANCE GetCurrent()
        {
            if (!_fValid)
                throw new InvalidOperationException();
            return _current;
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public STORE_CATEGORY_INSTANCE Current { get { return GetCurrent(); } }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_CATEGORY_INSTANCE[] next = new STORE_CATEGORY_INSTANCE[1];
            UInt32 fetched;
            fetched=_enum.Next(1, next);
            if (fetched == 1)
                _current = next[0];
            return (_fValid = (fetched == 1));
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            _fValid = false;
            _enum.Reset();
        }
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class ReferenceIdentity
    {
        internal IReferenceIdentity _id = null;

        internal ReferenceIdentity(IReferenceIdentity i)
        {
            if (i == null)
                throw new ArgumentNullException();

            _id = i;
        }

        string GetAttribute(string ns, string n) { return _id.GetAttribute(ns, n); }
        string GetAttribute(string n) { return _id.GetAttribute(null, n); }

        void SetAttribute(string ns, string n, string v) { _id.SetAttribute(ns, n, v); }
        void SetAttribute(string n, string v) { SetAttribute(null, n, v); }

        void DeleteAttribute(string ns, string n) { SetAttribute(ns, n, null); }
        void DeleteAttribute(string n) { SetAttribute(null, n, null); }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("6eaf5ace-7917-4f3c-b129-e046a9704766")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IReferenceIdentity
    {
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GetAttribute(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Namespace,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name
            );
        [SecurityCritical]
        void SetAttribute(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Namespace,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Value
            );
        [SecurityCritical]
        IEnumIDENTITY_ATTRIBUTE EnumAttributes();
        [SecurityCritical]
        IReferenceIdentity Clone(
            [In] IntPtr /*SIZE_T*/ cDeltas,
            [In, MarshalAs(UnmanagedType.LPArray)] IDENTITY_ATTRIBUTE[] Deltas
            );
    }

    [ComImport]
    [Guid("587bf538-4d90-4a3c-9ef1-58a200a8a9e7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDefinitionIdentity
    {
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GetAttribute(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Namespace,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name
            );
        [SecurityCritical]
        void SetAttribute(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Namespace,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Value);
        [SecurityCritical]
        IEnumIDENTITY_ATTRIBUTE EnumAttributes();
        [SecurityCritical]
        IDefinitionIdentity Clone(
            [In] IntPtr /*SIZE_T*/ cDeltas,
            [In, MarshalAs(UnmanagedType.LPArray)] IDENTITY_ATTRIBUTE[] Deltas
            );
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class DefinitionIdentity
    {
        internal IDefinitionIdentity _id = null;

        internal DefinitionIdentity(IDefinitionIdentity i)
        {
            if (i == null)
                throw new ArgumentNullException();

            _id = i;
        }

        string GetAttribute(string ns, string n) { return _id.GetAttribute(ns, n); }
        string GetAttribute(string n) { return _id.GetAttribute(null, n); }

        void SetAttribute(string ns, string n, string v) { _id.SetAttribute(ns, n, v); }
        void SetAttribute(string n, string v) { SetAttribute(null, n, v); }

        void DeleteAttribute(string ns, string n) { SetAttribute(ns, n, null); }
        void DeleteAttribute(string n) { SetAttribute(null, n, null); }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("9cdaae75-246e-4b00-a26d-b9aec137a3eb")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumIDENTITY_ATTRIBUTE
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] IDENTITY_ATTRIBUTE[] rgAttributes
            /*[Out, Optional] out uint Written*/);

        [SecurityCritical]
        IntPtr CurrentIntoBuffer(
            [In] IntPtr /*SIZE_T*/ Available,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] Data
            /*[out] SIZE_T *pcbUsed*/);

        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumIDENTITY_ATTRIBUTE Clone();
    }

    [ComImport]
    [Guid("f3549d9c-fc73-4793-9c00-1cd204254c0c")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumDefinitionIdentity
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity[] DefinitionIdentity
            /*[Out] out uint Written*/);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumDefinitionIdentity Clone();
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class EnumDefinitionIdentity : IEnumerator
    {
        private IEnumDefinitionIdentity _enum = null;
        IDefinitionIdentity _current = null;
        IDefinitionIdentity[] _fetchList = new IDefinitionIdentity[1];

        internal EnumDefinitionIdentity(IEnumDefinitionIdentity e)
        {
            if (e == null)
                throw new ArgumentNullException();

            _enum = e;
        }

        private DefinitionIdentity GetCurrent() {
            if (_current == null)
                throw new InvalidOperationException();
            return new DefinitionIdentity(_current);
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public DefinitionIdentity Current { get { return GetCurrent(); } }

        public IEnumerator GetEnumerator() { return this; }

        public bool MoveNext()
        {
            if ((_enum.Next(1, _fetchList)) == 1)
            {
                _current = _fetchList[0];
                return true;
            }
            else
            {
                _current = null;
                return false;
            }
        }

        public void Reset()
        {
            _current = null;
            _enum.Reset();
        }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("b30352cf-23da-4577-9b3f-b4e6573be53b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumReferenceIdentity
    {
        [SecurityCritical]
        uint Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray)] IReferenceIdentity[] ReferenceIdentity
            /*[Out] out uint Written*/);
        [SecurityCritical]
        void Skip(uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumReferenceIdentity Clone();
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class EnumReferenceIdentity : IEnumerator
    {
        private IEnumReferenceIdentity _enum = null;
        IReferenceIdentity _current = null;
        IReferenceIdentity[] _fetchList = new IReferenceIdentity[1];

        internal EnumReferenceIdentity(IEnumReferenceIdentity e)
        {
            _enum = e;
        }

        private ReferenceIdentity GetCurrent() {
            if (_current == null)
                throw new InvalidOperationException();
            return new ReferenceIdentity(_current);
        }

        object IEnumerator.Current { get { return GetCurrent(); } }
        public ReferenceIdentity Current { get { return GetCurrent(); } }

        public IEnumerator GetEnumerator() { return this; }

        public bool MoveNext()
        {
            if ((_enum.Next(1, _fetchList)) == 1)
            {
                _current = _fetchList[0];
                return true;
            }
            else
            {
                _current = null;
                return false;
            }
        }

        public void Reset()
        {
            _current = null;
            _enum.Reset();
        }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("d91e12d8-98ed-47fa-9936-39421283d59b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDefinitionAppId
    {
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)] string get_SubscriptionId();
        void put_SubscriptionId([In, MarshalAs(UnmanagedType.LPWStr)] string Subscription);
        [SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [return:MarshalAs(UnmanagedType.LPWStr)] string get_Codebase();
        [SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        void put_Codebase([In, MarshalAs(UnmanagedType.LPWStr)] string CodeBase);
        [SecurityCritical]
        IEnumDefinitionIdentity EnumAppPath();
        [SecurityCritical]
        void SetAppPath([In] uint cIDefinitionIdentity, [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity[] DefinitionIdentity);
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class DefinitionAppId
    {
        internal IDefinitionAppId _id = null;

        internal DefinitionAppId(IDefinitionAppId id)
        {
            if (id == null)
                throw new ArgumentNullException();
            _id = id;
        }

        public string SubscriptionId
        {
            get { return _id.get_SubscriptionId(); }
            set { _id.put_SubscriptionId(value); }
        }

        public string Codebase
        {
            get { return _id.get_Codebase(); }
            set { _id.put_Codebase(value); }
        }

        public EnumDefinitionIdentity AppPath
        {
            get { return new EnumDefinitionIdentity(_id.EnumAppPath()); }
        }

        void SetAppPath(IDefinitionIdentity[] Ids)
        {
            _id.SetAppPath((uint)Ids.Length, Ids);
        }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("054f0bef-9e45-4363-8f5a-2f8e142d9a3b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IReferenceAppId
    {
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)] string get_SubscriptionId();
        void put_SubscriptionId([In, MarshalAs(UnmanagedType.LPWStr)] string Subscription);
        [SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [return:MarshalAs(UnmanagedType.LPWStr)] string get_Codebase();
        [ResourceExposure(ResourceScope.Machine)]
        void put_Codebase([In, MarshalAs(UnmanagedType.LPWStr)] string CodeBase);
        [SecurityCritical]
        IEnumReferenceIdentity EnumAppPath();
    }

#if !ISOLATION_IN_MSCORLIB
    internal sealed class ReferenceAppId
    {
        internal IReferenceAppId _id = null;

        internal ReferenceAppId(IReferenceAppId id)
        {
            if (id == null)
                throw new ArgumentNullException();
            _id = id;
        }

        public string SubscriptionId
        {
            get { return _id.get_SubscriptionId(); }
            set { _id.put_SubscriptionId(value); }
        }

        public string Codebase
        {
            get { return _id.get_Codebase(); }
            set { _id.put_Codebase(value); }
        }

        public EnumReferenceIdentity AppPath
        {
            get { return new EnumReferenceIdentity(_id.EnumAppPath()); }
        }
    }
#endif // !ISOLATION_IN_MSCORLIB

    internal enum IIDENTITYAUTHORITY_DEFINITION_IDENTITY_TO_TEXT_FLAGS
    {
        IIDENTITYAUTHORITY_DEFINITION_IDENTITY_TO_TEXT_FLAG_CANONICAL = 0x00000001,
    }

    internal enum IIDENTITYAUTHORITY_REFERENCE_IDENTITY_TO_TEXT_FLAGS
    {
        IIDENTITYAUTHORITY_REFERENCE_IDENTITY_TO_TEXT_FLAG_CANONICAL = 0x00000001,
    }

    internal enum IIDENTITYAUTHORITY_DOES_DEFINITION_MATCH_REFERENCE_FLAGS
    {
        IIDENTITYAUTHORITY_DOES_DEFINITION_MATCH_REFERENCE_FLAG_EXACT_MATCH_REQUIRED = 0x00000001,
    }

    [ComImport]
    [Guid("261a6983-c35d-4d0d-aa5b-7867259e77bc")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IIdentityAuthority
    {
        [SecurityCritical]
        IDefinitionIdentity TextToDefinition(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Identity
            );
        [SecurityCritical]
        IReferenceIdentity TextToReference(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Identity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string DefinitionToText(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity
            );
        [SecurityCritical]
        UInt32 DefinitionToTextBuffer(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] UInt32 BufferSize,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] Buffer
            /*out UInt32 cchBufferRequired*/
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string ReferenceToText(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity
            );
        [SecurityCritical]
        UInt32 ReferenceToTextBuffer(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity,
            [In] UInt32 BufferSize,
            [Out, MarshalAs(UnmanagedType.LPArray)] char[] Buffer
            /*out UInt32 cchBufferRequired*/
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreDefinitionsEqual(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity Definition1,
            [In] IDefinitionIdentity Definition2
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreReferencesEqual(
            [In] UInt32 Flags,
            [In] IReferenceIdentity Reference1,
            [In] IReferenceIdentity Reference2
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreTextualDefinitionsEqual(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityLeft,
            [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityRight
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreTextualReferencesEqual(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityLeft,
            [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityRight
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool DoesDefinitionMatchReference(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] IReferenceIdentity ReferenceIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool DoesTextualDefinitionMatchTextualReference(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Definition,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Reference
            );
        [SecurityCritical]
        UInt64 HashReference(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity
            );
        [SecurityCritical]
        UInt64 HashDefinition(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GenerateDefinitionKey(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GenerateReferenceKey(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity
            );
        [SecurityCritical]
        IDefinitionIdentity CreateDefinition();
        [SecurityCritical]
        IReferenceIdentity CreateReference();
    }

    [Flags]
    internal enum IAPPIDAUTHORITY_ARE_DEFINITIONS_EQUAL_FLAGS
    {
        IAPPIDAUTHORITY_ARE_DEFINITIONS_EQUAL_FLAG_IGNORE_VERSION = 0x00000001,
    }

    [Flags]
    internal enum IAPPIDAUTHORITY_ARE_REFERENCES_EQUAL_FLAGS
    {
        IAPPIDAUTHORITY_ARE_REFERENCES_EQUAL_FLAG_IGNORE_VERSION = 0x00000001,
    }

    [ComImport]
    [Guid("8c87810c-2541-4f75-b2d0-9af515488e23")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAppIdAuthority
    {
        [SecurityCritical]
        IDefinitionAppId TextToDefinition(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Identity
            );
        [SecurityCritical]
        IReferenceAppId TextToReference(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Identity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string DefinitionToText(
            [In] UInt32 Flags,
            [In] IDefinitionAppId DefinitionAppId
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string ReferenceToText(
            [In] UInt32 Flags,
            [In] IReferenceAppId ReferenceAppId
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreDefinitionsEqual(
            [In] UInt32 Flags,
            [In] IDefinitionAppId Definition1,
            [In] IDefinitionAppId Definition2
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreReferencesEqual(
            [In] UInt32 Flags,
            [In] IReferenceAppId Reference1,
            [In] IReferenceAppId Reference2
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreTextualDefinitionsEqual(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppIdLeft,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppIdRight
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool AreTextualReferencesEqual(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppIdLeft,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppIdRight
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool DoesDefinitionMatchReference(
            [In] UInt32 Flags,
            [In] IDefinitionAppId DefinitionIdentity,
            [In] IReferenceAppId ReferenceIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.Bool)]
        bool DoesTextualDefinitionMatchTextualReference(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Definition,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Reference
            );
        [SecurityCritical]
        UInt64 HashReference(
            [In] UInt32 Flags,
            [In] IReferenceAppId ReferenceIdentity
            );
        [SecurityCritical]
        UInt64 HashDefinition(
            [In] UInt32 Flags,
            [In] IDefinitionAppId DefinitionIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GenerateDefinitionKey(
            [In] UInt32 Flags,
            [In] IDefinitionAppId DefinitionIdentity
            );
        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string GenerateReferenceKey(
            [In] UInt32 Flags,
            [In] IReferenceAppId ReferenceIdentity
            );
        [SecurityCritical]
        IDefinitionAppId CreateDefinition();
        [SecurityCritical]
        IReferenceAppId CreateReference();
    }

    [Flags]
    internal enum ISTORE_BIND_REFERENCE_TO_ASSEMBLY_FLAGS
    {
        ISTORE_BIND_REFERENCE_TO_ASSEMBLY_FLAG_FORCE_LIBRARY_SEMANTICS = 0x00000001,
    }

    [Flags]
    internal enum ISTORE_ENUM_ASSEMBLIES_FLAGS
    {
        ISTORE_ENUM_ASSEMBLIES_FLAG_LIMIT_TO_VISIBLE_ONLY   = 0x00000001,
        ISTORE_ENUM_ASSEMBLIES_FLAG_MATCH_SERVICING         = 0x00000002,
        ISTORE_ENUM_ASSEMBLIES_FLAG_FORCE_LIBRARY_SEMANTICS = 0x00000004,
    }

    [Flags]
    internal enum ISTORE_ENUM_FILES_FLAGS
    {
        ISTORE_ENUM_FILES_FLAG_INCLUDE_INSTALLED_FILES = 0x00000001,
        ISTORE_ENUM_FILES_FLAG_INCLUDE_MISSING_FILES   = 0x00000002,
    }

    //
    // Operations available to the "transact" operation
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationStageComponent
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionIdentity Component;
        [MarshalAs(UnmanagedType.LPWStr)] public string ManifestPath;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0
        }

        public enum Disposition
        {
            Failed = 0,
            Installed = 1,
            Refreshed = 2,
            AlreadyInstalled = 3
        }

        public void Destroy() { }

        public StoreOperationStageComponent(IDefinitionAppId app, string Manifest)
            : this(app, null, Manifest)
        {
        }

        public StoreOperationStageComponent(IDefinitionAppId app, IDefinitionIdentity comp, string Manifest)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationStageComponent));
            Flags = OpFlags.Nothing;
            Application = app;
            Component = comp;
            ManifestPath = Manifest;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationStageComponentFile
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionIdentity Component;
        [MarshalAs(UnmanagedType.LPWStr)] public string ComponentRelativePath;
        [MarshalAs(UnmanagedType.LPWStr)] public string SourceFilePath;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0
        }

        public enum Disposition
        {
            Failed = 0,
            Installed = 1,
            Refreshed = 2,
            AlreadyInstalled = 3
        }

        public StoreOperationStageComponentFile(IDefinitionAppId App, string CompRelPath, string SrcFile)
            : this(App, null, CompRelPath, SrcFile)
        {
        }

        public StoreOperationStageComponentFile(IDefinitionAppId App, IDefinitionIdentity Component, string CompRelPath, string SrcFile)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationStageComponentFile));
            Flags = OpFlags.Nothing;
            this.Application = App;
            this.Component = Component;
            this.ComponentRelativePath = CompRelPath;
            this.SourceFilePath = SrcFile;
        }

        public void Destroy() { }


    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreApplicationReference
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public RefFlags Flags;
        public System.Guid GuidScheme;
        [MarshalAs(UnmanagedType.LPWStr)] public string Identifier;
        [MarshalAs(UnmanagedType.LPWStr)] public string NonCanonicalData;

        [Flags]
        public enum RefFlags
        {
            Nothing = 0
        }

        public StoreApplicationReference(System.Guid RefScheme, string Id, string NcData)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreApplicationReference));
            Flags = RefFlags.Nothing;
            GuidScheme = RefScheme;
            Identifier = Id;
            NonCanonicalData = NcData;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public IntPtr ToIntPtr()
        {
            IntPtr Reference = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
            Marshal.StructureToPtr(this, Reference, false);
            return Reference;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public static void Destroy(IntPtr ip)
        {
            if (ip != IntPtr.Zero)
            {
                Marshal.DestroyStructure(ip, typeof(StoreApplicationReference));
                Marshal.FreeCoTaskMem(ip);
            }
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationPinDeployment
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.I8)] public Int64 ExpirationTime;
        public IntPtr Reference;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0,
            NeverExpires = 0x1
        }

        public enum Disposition
        {
            Failed = 0,
            Pinned = 1
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public StoreOperationPinDeployment(IDefinitionAppId AppId, StoreApplicationReference Ref)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationPinDeployment));
            Flags = OpFlags.NeverExpires;
            Application = AppId;

            Reference = Ref.ToIntPtr();
            ExpirationTime = 0;
        }

        public StoreOperationPinDeployment(IDefinitionAppId AppId, System.DateTime Expiry, StoreApplicationReference Ref)
            : this(AppId, Ref)
        {
            Flags |= OpFlags.NeverExpires;
            // 
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Destroy()
        {
            StoreApplicationReference.Destroy(Reference);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationUnpinDeployment
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        public IntPtr Reference;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0
        }

        public enum Disposition
        {
            Failed = 0,
            Unpinned = 1
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public StoreOperationUnpinDeployment(IDefinitionAppId app, StoreApplicationReference reference)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationUnpinDeployment));
            Flags = OpFlags.Nothing;
            Application = app;
            Reference = reference.ToIntPtr();
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Destroy()
        {
            StoreApplicationReference.Destroy(Reference);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationInstallDeployment
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        public IntPtr Reference;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0,
            UninstallOthers = 0x1
        }

        public enum Disposition
        {
            Failed = 0,
            AlreadyInstalled = 1,
            Installed = 2,
        }


        public StoreOperationInstallDeployment(IDefinitionAppId App, StoreApplicationReference reference) :
            this(App, true, reference)
        {
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public StoreOperationInstallDeployment(IDefinitionAppId App, bool UninstallOthers, StoreApplicationReference reference)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationInstallDeployment));
            Flags = OpFlags.Nothing;
            Application = App;

            if (UninstallOthers)
            {
                Flags |= OpFlags.UninstallOthers;
            }

            Reference = reference.ToIntPtr();
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Destroy()
        {
            StoreApplicationReference.Destroy(Reference);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationUninstallDeployment
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Application;
        public IntPtr Reference;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0
        }

        public enum Disposition
        {
            Failed = 0,
            DidNotExist = 1,
            Uninstalled = 2
        }


        [System.Security.SecuritySafeCritical]  // auto-generated
        public StoreOperationUninstallDeployment(IDefinitionAppId appid, StoreApplicationReference AppRef)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationUninstallDeployment));
            Flags = OpFlags.Nothing;
            Application = appid;
            Reference = AppRef.ToIntPtr();
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Destroy()
        {
            StoreApplicationReference.Destroy(Reference);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationMetadataProperty
    {
        public System.Guid GuidPropertySet;
        [MarshalAs(UnmanagedType.LPWStr)] public string Name;
        [MarshalAs(UnmanagedType.SysUInt)] public IntPtr ValueSize;
        [MarshalAs(UnmanagedType.LPWStr)] public string Value;

        public StoreOperationMetadataProperty(System.Guid PropertySet, string Name)
            : this(PropertySet, Name, null)
        {
        }

        public StoreOperationMetadataProperty(System.Guid PropertySet, string Name, string Value)
        {
            this.GuidPropertySet = PropertySet;
            this.Name = Name;
            this.Value = Value;
            this.ValueSize = (Value != null) ? new IntPtr((Value.Length + 1) * 2) : IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationSetDeploymentMetadata
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)] public IDefinitionAppId Deployment;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr InstallerReference;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr /*SIZE_T*/ cPropertiesToTest;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr PropertiesToTest;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr /*SIZE_T*/ cPropertiesToSet;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr PropertiesToSet;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0,
        }

        public enum Disposition
        {
            Failed = 0,
            Set = 2
        }

        public StoreOperationSetDeploymentMetadata(IDefinitionAppId Deployment, StoreApplicationReference Reference, StoreOperationMetadataProperty[] SetProperties)
            : this(Deployment, Reference, SetProperties, null)
        {
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public StoreOperationSetDeploymentMetadata(IDefinitionAppId Deployment, StoreApplicationReference Reference, StoreOperationMetadataProperty[] SetProperties, StoreOperationMetadataProperty[] TestProperties)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationSetDeploymentMetadata));
            Flags = OpFlags.Nothing;
            this.Deployment = Deployment;

            if (SetProperties != null)
            {
                PropertiesToSet = MarshalProperties(SetProperties);
                cPropertiesToSet = new IntPtr(SetProperties.Length);
            }
            else
            {
                PropertiesToSet = IntPtr.Zero;
                cPropertiesToSet = IntPtr.Zero;
            }

            if (TestProperties != null)
            {
                PropertiesToTest = MarshalProperties(TestProperties);
                cPropertiesToTest = new IntPtr(TestProperties.Length);
            }
            else
            {
                PropertiesToTest = IntPtr.Zero;
                cPropertiesToTest = IntPtr.Zero;
            }

            InstallerReference = Reference.ToIntPtr();
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Destroy()
        {
            if (PropertiesToSet != IntPtr.Zero)
            {
                DestroyProperties(PropertiesToSet, (ulong)cPropertiesToSet.ToInt64());
                PropertiesToSet = IntPtr.Zero;
                cPropertiesToSet = IntPtr.Zero;
            }

            if (PropertiesToTest != IntPtr.Zero)
            {
                DestroyProperties(PropertiesToTest, (ulong)cPropertiesToTest.ToInt64());
                PropertiesToTest = IntPtr.Zero;
                cPropertiesToTest = IntPtr.Zero;
            }

            if (InstallerReference != IntPtr.Zero)
            {
                StoreApplicationReference.Destroy(InstallerReference);
                InstallerReference = IntPtr.Zero;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void DestroyProperties(IntPtr rgItems, ulong iItems)
        {
            if (rgItems != IntPtr.Zero)
            {
                IntPtr cursor = rgItems;
                ulong iSlotSize = (ulong)Marshal.SizeOf(typeof(StoreOperationMetadataProperty));

                for (ulong i = 0; i < iItems; i++)
                {
                    Marshal.DestroyStructure(
                        new IntPtr((long)((i * iSlotSize) + (ulong)rgItems.ToInt64())),
                        typeof(StoreOperationMetadataProperty));
                }

                Marshal.FreeCoTaskMem(rgItems);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static IntPtr MarshalProperties(StoreOperationMetadataProperty[] Props)
        {
            if ((Props == null) || (Props.Length == 0))
                return IntPtr.Zero;

            int iSlotSize = Marshal.SizeOf(typeof(StoreOperationMetadataProperty));
            IntPtr retval = Marshal.AllocCoTaskMem(iSlotSize * Props.Length);

            for (int i = 0; i != Props.Length; i++)
            {
                Marshal.StructureToPtr(
                    Props[i],
                    new IntPtr((i * iSlotSize) + retval.ToInt64()),
                    false);
            }

            return retval;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationSetCanonicalizationContext
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.LPWStr)] public string BaseAddressFilePath;
        [MarshalAs(UnmanagedType.LPWStr)] public string ExportsFilePath;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0
        }

        [System.Security.SecurityCritical]  // auto-generated
        public StoreOperationSetCanonicalizationContext(string Bases, string Exports)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationSetCanonicalizationContext));
            Flags = OpFlags.Nothing;
            this.BaseAddressFilePath = Bases;
            this.ExportsFilePath = Exports;
        }

        public void Destroy()
        {
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationScavenge
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
        [MarshalAs(UnmanagedType.U4)] public OpFlags Flags;
        [MarshalAs(UnmanagedType.U8)] public UInt64 SizeReclaimationLimit;
        [MarshalAs(UnmanagedType.U8)] public UInt64 RuntimeLimit;
        [MarshalAs(UnmanagedType.U4)] public UInt32 ComponentCountLimit;

        [Flags]
        public enum OpFlags
        {
            Nothing = 0,
            Light = 1,
            LimitSize = 2,
            LimitTime = 4,
            LimitCount = 8
        }

        public StoreOperationScavenge(bool Light, ulong SizeLimit, ulong RunLimit, uint ComponentLimit)
        {
            Size = (UInt32)Marshal.SizeOf(typeof(StoreOperationScavenge));
            Flags = OpFlags.Nothing;

            if (Light)
                Flags |= OpFlags.Light;

            this.SizeReclaimationLimit = SizeLimit;
            if (SizeLimit != 0)
                this.Flags |= OpFlags.LimitSize;

            this.RuntimeLimit = RunLimit;
            if (RunLimit != 0)
                this.Flags |= OpFlags.LimitTime;

            this.ComponentCountLimit = ComponentLimit;
            if (ComponentLimit != 0)
                this.Flags |= OpFlags.LimitCount;
        }

        public StoreOperationScavenge(bool Light) : this(Light, 0, 0, 0)
        {
        }

        public void Destroy()
        {
        }
    }

    internal enum StoreTransactionOperationType
    {
        Invalid = 0,
        SetCanonicalizationContext = 14,
        StageComponent = 20,
        PinDeployment = 21,
        UnpinDeployment = 22,
        StageComponentFile = 23,
        InstallDeployment = 24,
        UninstallDeployment = 25,
        SetDeploymentMetadata = 26,
        Scavenge = 27
    }


    //
    // The transaction operation contains an operation key and
    // a structure that contains the actual data.
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreTransactionOperation
    {
        [MarshalAs(UnmanagedType.U4)] public StoreTransactionOperationType Operation;
        public StoreTransactionData Data;
    }

    //
    // An IntPtr to a CoTaskMemAlloc'd transaction structure
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreTransactionData
    {
        public IntPtr DataPtr;
    }

    internal class Store
    {
        private IStore _pStore = null;

        public IStore InternalStore { get { return _pStore; } }

        public Store(IStore pStore)
        {
            if (pStore == null)
                throw new ArgumentNullException("pStore");

            this._pStore = pStore;
        }

        [SecuritySafeCritical]
        public uint[] Transact(StoreTransactionOperation[] operations)
        {
            if ((operations == null) || (operations.Length == 0))
                throw new ArgumentException("operations");

            uint[] rgDispositions = new uint[operations.Length];
            int[] rgResults = new int[operations.Length];

            _pStore.Transact(new IntPtr(operations.Length), operations, rgDispositions, rgResults);

            return rgDispositions;
        }

#if !ISOLATION_IN_MSCORLIB
        public void Transact(StoreTransactionOperation[] operations, uint[] rgDispositions, int[] rgResults)
        {
            if ((operations == null) || (operations.Length == 0))
                throw new ArgumentException("operations");

            _pStore.Transact(new IntPtr(operations.Length), operations, rgDispositions, rgResults);
        }
#endif // !ISOLATION_IN_MSCORLIB

        [SecuritySafeCritical]
        public IDefinitionIdentity BindReferenceToAssemblyIdentity(
            UInt32 Flags,
            IReferenceIdentity ReferenceIdentity,
            uint cDeploymentsToIgnore,
            IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore
            )
        {
            object o;
            System.Guid g = IsolationInterop.IID_IDefinitionIdentity;
            o=_pStore.BindReferenceToAssembly(
                Flags,
                ReferenceIdentity,
                cDeploymentsToIgnore,
                DefinitionIdentity_DeploymentsToIgnore,
                ref g);
            return (IDefinitionIdentity)o;
        }

        [SecuritySafeCritical]
        public void CalculateDelimiterOfDeploymentsBasedOnQuota(
            UInt32 dwFlags,
            UInt32 cDeployments,
            IDefinitionAppId[] rgpIDefinitionAppId_Deployments,
            ref StoreApplicationReference InstallerReference,
            UInt64 ulonglongQuota,
            ref UInt32 Delimiter,
            ref UInt64 SizeSharedWithExternalDeployment,
            ref UInt64 SizeConsumedByInputDeploymentArray
            )
        {
            IntPtr DelimIntPtr = IntPtr.Zero;
            
            _pStore.CalculateDelimiterOfDeploymentsBasedOnQuota(
                dwFlags,
                new IntPtr((Int64)cDeployments),
                rgpIDefinitionAppId_Deployments,
                ref InstallerReference,
                ulonglongQuota,
                ref DelimIntPtr,
                ref SizeSharedWithExternalDeployment,
                ref SizeConsumedByInputDeploymentArray);
                
            Delimiter = (UInt32)DelimIntPtr.ToInt64();

            return;
        }

        [SecuritySafeCritical]
        public CMS.ICMS BindReferenceToAssemblyManifest(
            UInt32 Flags,
            IReferenceIdentity ReferenceIdentity,
            uint cDeploymentsToIgnore,
            IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore
            )
        {
            object o;
            System.Guid g = IsolationInterop.IID_ICMS;
            o=_pStore.BindReferenceToAssembly(
                Flags,
                ReferenceIdentity,
                cDeploymentsToIgnore,
                DefinitionIdentity_DeploymentsToIgnore,
                ref g);
            return (CMS.ICMS)o;
        }

        [SecuritySafeCritical]
        public CMS.ICMS GetAssemblyManifest(
            UInt32 Flags,
            IDefinitionIdentity DefinitionIdentity
            )
        {
            object o;
            System.Guid g = IsolationInterop.IID_ICMS;
            o=_pStore.GetAssemblyInformation(
                Flags,
                DefinitionIdentity,
                ref g);
            return (CMS.ICMS)o;
        }

        /*
            What's the point of this?  We already know the identity, we're passing it
            in on the commandline.
         */
        [SecuritySafeCritical]
        public IDefinitionIdentity GetAssemblyIdentity(
            UInt32 Flags,
            IDefinitionIdentity DefinitionIdentity
            )
        {
            object o;
            System.Guid g = IsolationInterop.IID_IDefinitionIdentity;
            o=_pStore.GetAssemblyInformation(
                Flags,
                DefinitionIdentity,
                ref g);
            return (IDefinitionIdentity)o;
        }

        [Flags]
        public enum EnumAssembliesFlags
        {
            Nothing = 0,
            VisibleOnly = 0x1,
            MatchServicing = 0x2,
            ForceLibrarySemantics = 0x4
        }

        public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags)
        {
            return this.EnumAssemblies(Flags, null);
        }

        [SecuritySafeCritical]
        public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags, IReferenceIdentity refToMatch)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY));
            object o;

            o=_pStore.EnumAssemblies((UInt32)Flags, refToMatch, ref g);
            return new StoreAssemblyEnumeration((IEnumSTORE_ASSEMBLY)o);
        }

        [Flags]
        public enum EnumAssemblyFilesFlags
        {
            Nothing = 0,
            IncludeInstalled = 0x1,
            IncludeMissing = 0x2
        }

        [SecuritySafeCritical]
        public StoreAssemblyFileEnumeration EnumFiles(EnumAssemblyFilesFlags Flags, IDefinitionIdentity Assembly)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
            object o;
            o=_pStore.EnumFiles((UInt32)Flags, Assembly, ref g);
            return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE)o);
        }

        [Flags]
        public enum EnumApplicationPrivateFiles
        {
            Nothing = 0,
            IncludeInstalled = 0x1,
            IncludeMissing = 0x2
        }

        [SecuritySafeCritical]
        public StoreAssemblyFileEnumeration EnumPrivateFiles(
                EnumApplicationPrivateFiles Flags,
                IDefinitionAppId Application,
                IDefinitionIdentity Assembly)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
            object o;
            o=_pStore.EnumPrivateFiles((UInt32)Flags, Application, Assembly, ref g);
            return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE)o);
        }

        [Flags]
        public enum EnumAssemblyInstallReferenceFlags
        {
            Nothing = 0
        }

        [SecuritySafeCritical]
        public IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE EnumInstallationReferences(
                EnumAssemblyInstallReferenceFlags Flags,
                IDefinitionIdentity Assembly
                )
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE));
            object o;
            o=_pStore.EnumInstallationReferences((UInt32)Flags, Assembly, ref g);
            return (IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE)o;
        }

        public interface IPathLock : IDisposable
        {
            string Path { get ; }
        }

        private class AssemblyPathLock : IPathLock
        {
            private IStore _pSourceStore = null;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private string _path;

            public AssemblyPathLock(IStore s, IntPtr c, string path)
            {
                _pSourceStore = s;
                _pLockCookie = c;
                _path = path;
            }

            [SecuritySafeCritical]
            private void Dispose(bool fDisposing)
            {
                if (fDisposing)
                    System.GC.SuppressFinalize(this);

                if (_pLockCookie != IntPtr.Zero)
                {
                    _pSourceStore.ReleaseAssemblyPath(_pLockCookie);
                    _pLockCookie = IntPtr.Zero;
                }
            }

            ~AssemblyPathLock() { Dispose(false); }
            void IDisposable.Dispose() { Dispose(true); }

            public string Path
            {
                get
                {
                    return _path;
                }
            }
        }

        [SecuritySafeCritical]
        public IPathLock LockAssemblyPath(IDefinitionIdentity asm)
        {
            string thePath;
            IntPtr theCookie;
            thePath=_pStore.LockAssemblyPath(0, asm, out theCookie);
            return new AssemblyPathLock(_pStore, theCookie, thePath);
        }

        private class ApplicationPathLock : IPathLock
        {
            private IStore _pSourceStore = null;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private string _path;

            public ApplicationPathLock(IStore s, IntPtr c, string path)
            {
                _pSourceStore = s;
                _pLockCookie = c;
                _path = path;
            }

            [SecuritySafeCritical]
            private void Dispose(bool fDisposing)
            {
                if (fDisposing)
                    System.GC.SuppressFinalize(this);

                if (_pLockCookie != IntPtr.Zero)
                {
                    _pSourceStore.ReleaseApplicationPath(_pLockCookie);
                    _pLockCookie = IntPtr.Zero;
                }
            }

            ~ApplicationPathLock() { Dispose(false); }
            void IDisposable.Dispose() { Dispose(true); }

            public string Path
            {
                get
                {
                    return _path;
                }
            }
        }

        [SecuritySafeCritical]
        public IPathLock LockApplicationPath(IDefinitionAppId app)
        {
            string thePath;
            IntPtr theCookie;
            thePath = _pStore.LockApplicationPath(0, app, out theCookie);
            return new ApplicationPathLock(_pStore, theCookie, thePath);
        }

        [SecuritySafeCritical]
        public UInt64 QueryChangeID(IDefinitionIdentity asm)
        {
            UInt64 ChangeId;
            ChangeId=_pStore.QueryChangeID(asm);
            return ChangeId;
        }

        [Flags]
        public enum EnumCategoriesFlags
        {
            Nothing = 0
        }

        [SecuritySafeCritical]
        public StoreCategoryEnumeration EnumCategories(EnumCategoriesFlags Flags, IReferenceIdentity CategoryMatch)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY));
            object o;
            o=_pStore.EnumCategories((UInt32)Flags, CategoryMatch, ref g);
            return new StoreCategoryEnumeration((IEnumSTORE_CATEGORY)o);
        }

        [Flags]
        public enum EnumSubcategoriesFlags
        {
            Nothing = 0
        }

        public StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity CategoryMatch)
        {
            return this.EnumSubcategories(Flags, CategoryMatch, null);
        }

        [SecuritySafeCritical]
        public StoreSubcategoryEnumeration  EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity Category, string SearchPattern)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_SUBCATEGORY));
            object o;
            o=_pStore.EnumSubcategories((UInt32)Flags, Category, SearchPattern, ref g);
            return new StoreSubcategoryEnumeration((IEnumSTORE_CATEGORY_SUBCATEGORY)o);
        }

        [Flags]
        public enum EnumCategoryInstancesFlags
        {
            Nothing = 0
        }

        [SecuritySafeCritical]
        public StoreCategoryInstanceEnumeration EnumCategoryInstances(EnumCategoryInstancesFlags Flags, IDefinitionIdentity Category, string SubCat)
        {
            System.Guid g = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_INSTANCE));
            object o;
            o=_pStore.EnumCategoryInstances((UInt32)Flags, Category, SubCat, ref g);
            return new StoreCategoryInstanceEnumeration((IEnumSTORE_CATEGORY_INSTANCE)o);
        }

        [Flags]
        public enum GetPackagePropertyFlags
        {
            Nothing = 0
        }

        [System.Security.SecurityCritical]  // auto-generated
        public byte[] GetDeploymentProperty(
                GetPackagePropertyFlags Flags,
                IDefinitionAppId Deployment,
                StoreApplicationReference Reference,
                Guid PropertySet,
                string PropertyName
                )
        {
            BLOB b = new BLOB();
            byte[] retval = null;

            try
            {
                _pStore.GetDeploymentProperty(
                    (UInt32)Flags,
                    Deployment,
                    ref Reference,
                    ref PropertySet,
                    PropertyName,
                    out b);

                retval = new byte[b.Size];
                Marshal.Copy(b.BlobData, retval, 0, (int)b.Size);
            }
            finally
            {
                b.Dispose();
            }

            return retval;
        }

        [SecuritySafeCritical]
        public StoreDeploymentMetadataEnumeration EnumInstallerDeployments(
            Guid InstallerId,
            string InstallerName,
            string InstallerMetadata,
            IReferenceAppId DeploymentFilter
            )
        {
            object o = null;
            StoreApplicationReference AppReference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);

            o = _pStore.EnumInstallerDeploymentMetadata(
                0,
                ref AppReference,
                DeploymentFilter,
                ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA);

            return new StoreDeploymentMetadataEnumeration((IEnumSTORE_DEPLOYMENT_METADATA)o);
        }

        [SecuritySafeCritical]
        public StoreDeploymentMetadataPropertyEnumeration EnumInstallerDeploymentProperties(
            Guid InstallerId,
            string InstallerName,
            string InstallerMetadata,
            IDefinitionAppId Deployment
            )
        {
            object o = null;
            StoreApplicationReference AppReference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);

            o = _pStore.EnumInstallerDeploymentMetadataProperties(
                0,
                ref AppReference,
                Deployment,
                ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY);

            return new StoreDeploymentMetadataPropertyEnumeration((IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY)o);
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    struct IStore_BindingResult_BoundVersion
    {
        [MarshalAs(UnmanagedType.U2)] public UInt16 Revision;
        [MarshalAs(UnmanagedType.U2)] public UInt16 Build;
        [MarshalAs(UnmanagedType.U2)] public UInt16 Minor;
        [MarshalAs(UnmanagedType.U2)] public UInt16 Major;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IStore_BindingResult
    {
        [MarshalAs(UnmanagedType.U4)] public UInt32 Flags;
        [MarshalAs(UnmanagedType.U4)] public UInt32 Disposition;
        public IStore_BindingResult_BoundVersion Component;
        public Guid CacheCoherencyGuid;
        [MarshalAs(UnmanagedType.SysInt)] public IntPtr Reserved;
    }

    [ComImport]
    [Guid("a5c62f6d-5e3e-4cd9-b345-6b281d7a1d1e")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IStore
    {
        [SecurityCritical]
        void Transact([In] IntPtr /*SIZE_T*/ cOperation,
            [In, MarshalAs(UnmanagedType.LPArray)] StoreTransactionOperation[] rgOperations,
            [Out, MarshalAs(UnmanagedType.LPArray)] uint[] rgDispositions,
            [Out, MarshalAs(UnmanagedType.LPArray)] int[] /*HRESULT*/ rgResults
            );

        [SecurityCritical]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        object BindReferenceToAssembly(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity,
            [In] uint cDeploymentsToIgnore,
            [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore,
            [In] ref Guid riid
            );

        [SecurityCritical]
        void CalculateDelimiterOfDeploymentsBasedOnQuota(
            [In] UInt32 dwFlags,
            [In] IntPtr /*SIZE_T*/ cDeployments,
            [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionAppId[] rgpIDefinitionAppId_Deployments,
            [In] ref StoreApplicationReference InstallerReference,
            [In] UInt64 ulonglongQuota,
            [Out, In] ref IntPtr /*SIZE_T*/ Delimiter,
            [Out, In] ref UInt64 SizeSharedWithExternalDeployment,
            [Out, In] ref UInt64 SizeConsumedByInputDeploymentArray
            );

        [SecurityCritical]
        IntPtr BindDefinitions(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.SysInt)] IntPtr Count,
            [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity [] DefsToBind,
            [In] UInt32 DeploymentsToIgnore,
            [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity [] DefsToIgnore
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object GetAssemblyInformation(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumAssemblies(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity_ToMatch,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumFiles(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumInstallationReferences(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string LockAssemblyPath(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity DefinitionIdentity,
            [Out] out IntPtr Cookie
            );

        [SecurityCritical]
        void ReleaseAssemblyPath(
            [In] IntPtr Cookie
            );

        [SecurityCritical]
        UInt64 QueryChangeID(
            [In] IDefinitionIdentity DefinitionIdentity
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumCategories(
            [In] UInt32 Flags,
            [In] IReferenceIdentity ReferenceIdentity_ToMatch,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumSubcategories(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity CategoryId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string SubcategoryPathPattern,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumCategoryInstances(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity CategoryId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string SubcategoryPath,
            [In] ref Guid riid
            );

        // ISSUE - AMD64: Had to change to this because somehow returning BLOB
        // in the following crashes on amd64. Need to resolve the issue.
        [SecurityCritical]
        void GetDeploymentProperty(
            [In] UInt32 Flags,
            [In] IDefinitionAppId DeploymentInPackage,
            [In] ref StoreApplicationReference Reference,
            [In] ref Guid PropertySet,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pcwszPropertyName,
            out BLOB blob
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.LPWStr)]
        string LockApplicationPath(
            [In] UInt32 Flags,
            [In] IDefinitionAppId ApId,
            [Out] out IntPtr Cookie
            );

        [SecurityCritical]
        void ReleaseApplicationPath(
            [In] IntPtr Cookie
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumPrivateFiles(
            [In] UInt32 Flags,
            [In] IDefinitionAppId Application,
            [In] IDefinitionIdentity DefinitionIdentity,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumInstallerDeploymentMetadata(
            [In] UInt32 Flags,
            [In] ref StoreApplicationReference Reference,
            [In] IReferenceAppId Filter,
            [In] ref Guid riid
            );

        [SecurityCritical]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        object EnumInstallerDeploymentMetadataProperties(
            [In] UInt32 Flags,
            [In] ref StoreApplicationReference Reference,
            [In] IDefinitionAppId Filter,
            [In] ref Guid riid
            );

    }

    internal class StoreTransaction : IDisposable
    {
        private System.Collections.ArrayList _list = new System.Collections.ArrayList();
        private StoreTransactionOperation[] _storeOps = null;

        public void Add(StoreOperationInstallDeployment o) { _list.Add(o); }
        public void Add(StoreOperationPinDeployment o) { _list.Add(o); }
        public void Add(StoreOperationSetCanonicalizationContext o) { _list.Add(o); }
        public void Add(StoreOperationSetDeploymentMetadata o) { _list.Add(o); }
        public void Add(StoreOperationStageComponent o) { _list.Add(o); }
        public void Add(StoreOperationStageComponentFile o) { _list.Add(o); }
        public void Add(StoreOperationUninstallDeployment o) { _list.Add(o); }
        public void Add(StoreOperationUnpinDeployment o) { _list.Add(o); }
        public void Add(StoreOperationScavenge o) { _list.Add(o); }

        ~StoreTransaction()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        private void Dispose(bool fDisposing)
        {
            if (fDisposing)
            {
                System.GC.SuppressFinalize(this);
            }

            StoreTransactionOperation[] opList = _storeOps;
            _storeOps = null;

            //
            // If we had already created the operation list, then destroy the corresponding
            // objects that we'd copied out to the unmanaged GC data
            //
            if (opList != null)
            {
                for (int i = 0; i != opList.Length; i++)
                {
                    StoreTransactionOperation op = opList[i];
                    if (op.Data.DataPtr != IntPtr.Zero)
                    {
                        //
                        // Destroy the structure as appropriate
                        //
                        switch (op.Operation)
                        {
                        case StoreTransactionOperationType.StageComponent:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationStageComponent));
                            break;
                        case StoreTransactionOperationType.StageComponentFile:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationStageComponentFile));
                            break;
                        case StoreTransactionOperationType.PinDeployment:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationPinDeployment));
                            break;
                        case StoreTransactionOperationType.UninstallDeployment:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationUninstallDeployment));
                            break;
                        case StoreTransactionOperationType.UnpinDeployment:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationUnpinDeployment));
                            break;
                        case StoreTransactionOperationType.InstallDeployment:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationInstallDeployment));
                            break;
                        case StoreTransactionOperationType.SetCanonicalizationContext:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationSetCanonicalizationContext));
                            break;
                        case StoreTransactionOperationType.SetDeploymentMetadata:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationSetDeploymentMetadata));
                            break;
                        case StoreTransactionOperationType.Scavenge:
                            Marshal.DestroyStructure(op.Data.DataPtr, typeof(StoreOperationScavenge));
                            break;
                        }

                        //
                        // Free the pointer
                        //
                        Marshal.FreeCoTaskMem(op.Data.DataPtr);
                    }
                }
            }

        }

        public StoreTransactionOperation[] Operations
        {
            get
            {
                if (_storeOps == null)
                    _storeOps = GenerateStoreOpsList();
                return _storeOps;
            }
        }

        //
        // For each transaction that was lobbed in here, go and allocate/blit the transaction into
        // an unmanaged object.
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        private StoreTransactionOperation[] GenerateStoreOpsList()
        {
            StoreTransactionOperation[] txnList = new StoreTransactionOperation[_list.Count];

            for (int i = 0; i != _list.Count; i++)
            {
                object o = _list[i];
                System.Type t = o.GetType();

                txnList[i].Data.DataPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(o));
                Marshal.StructureToPtr(o, txnList[i].Data.DataPtr, false);


                if (t == typeof(StoreOperationSetCanonicalizationContext))
                {
                    txnList[i].Operation = StoreTransactionOperationType.SetCanonicalizationContext;
                }
                else if (t == typeof(StoreOperationStageComponent))
                {
                    txnList[i].Operation = StoreTransactionOperationType.StageComponent;
                }
                else if (t == typeof(StoreOperationPinDeployment))
                {
                    txnList[i].Operation = StoreTransactionOperationType.PinDeployment;
                }
                else if (t == typeof(StoreOperationUnpinDeployment))
                {
                    txnList[i].Operation = StoreTransactionOperationType.UnpinDeployment;
                }
                else if (t == typeof(StoreOperationStageComponentFile))
                {
                    txnList[i].Operation = StoreTransactionOperationType.StageComponentFile;
                }
                else if (t == typeof(StoreOperationInstallDeployment))
                {
                    txnList[i].Operation = StoreTransactionOperationType.InstallDeployment;
                }
                else if (t == typeof(StoreOperationUninstallDeployment))
                {
                    txnList[i].Operation = StoreTransactionOperationType.UninstallDeployment;
                }
                else if (t == typeof(StoreOperationSetDeploymentMetadata))
                {
                    txnList[i].Operation = StoreTransactionOperationType.SetDeploymentMetadata;
                }
                else if (t == typeof(StoreOperationScavenge))
                {
                    txnList[i].Operation = StoreTransactionOperationType.Scavenge;
                }
                else
                {
                    throw new Exception("How did you get here?");
                }
            }

            return txnList;
        }
    }

    [ComImport]
    [Guid("ace1b703-1aac-4956-ab87-90cac8b93ce6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IManifestParseErrorCallback
    {
        [SecurityCritical]
        void OnError(
            [In] UInt32 StartLine,
            [In] UInt32 nStartColumn,
            [In] UInt32 cCharacterCount,
            [In] Int32 hr,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ErrorStatusHostFile,
            [In] UInt32 ParameterCount,
            [In, MarshalAs(UnmanagedType.LPArray)] string[] Parameters);
    };




    //
    // Global isolation interop state.
    //
    internal static class IsolationInterop
    {
        private static object _synchObject = new object();
#if !ISOLATION_IN_MSCORLIB
        private static Store _userStore = null;
        private static Store _systemStore = null;
#endif // !ISOLATION_IN_MSCORLIB
        private static volatile IIdentityAuthority _idAuth = null;
        private static volatile IAppIdAuthority _appIdAuth = null;

#if ISOLATION_IN_MSCORLIB || ISOLATION_IN_CLICKONCE


#if FEATURE_MAIN_CLR_MODULE_USES_CORE_NAME
        public const String IsolationDllName = "coreclr.dll";
#else //FEATURE_MAIN_CLR_MODULE_USES_CORE_NAME
        public const String IsolationDllName = "clr.dll";
#endif //FEATURE_MAIN_CLR_MODULE_USES_CORE_NAME


#elif ISOLATION_IN_ISOWIN32
        public const String IsolationDllName = "isowin32.dll";
#elif ISOLATION_DLLNAME_IS_ISOWIN32
        public const String IsolationDllName = "isowin32.dll";
#elif ISOLATION_DLLNAME_IS_ISOMAN
        public const String IsolationDllName = "isoman.dll";
#elif ISOLATION_DLLNAME_IS_ISOLATION
        public const String IsolationDllName = "isolation.dll";
#elif ISOLATION_DLLNAME_IS_NTDLL
        public const String IsolationDllName = "ntdll.dll";
#else
        public const String IsolationDllName = "sxs.dll";
#endif

#if !ISOLATION_IN_MSCORLIB
        public static Store UserStore
        {
            get
            {
                if (_userStore == null)
                {
                    lock (_synchObject)
                    {
                        if (_userStore == null)
                            _userStore = new Store(GetUserStore(0, IntPtr.Zero, ref IID_IStore) as IStore);
                    }
                }

                return _userStore;
            }
        }
#endif // !ISOLATION_IN_MSCORLIB

        // Create a new user store object. 
        // Call into GetUserStore to get a new IStore handle.
        [SecuritySafeCritical]
        public static Store GetUserStore()
        {
            return new Store(GetUserStore(0, IntPtr.Zero, ref IID_IStore) as IStore);
        }

#if !ISOLATION_IN_MSCORLIB
        public static Store SystemStore
        {
            get
            {
                if (_systemStore == null)
                {
                    lock (_synchObject)
                    {
                        if (_systemStore == null)
                            _systemStore = new Store(GetSystemStore(0, ref IID_IStore) as IStore);
                    }
                }

                return _systemStore;
            }
        }
#endif // !ISOLATION_IN_MSCORLIB

        public static IIdentityAuthority IdentityAuthority
        {
            [SecuritySafeCritical]
            get
            {
                if (_idAuth == null)
                {
                    lock (_synchObject)
                    {
                        if (_idAuth == null)
                            _idAuth = GetIdentityAuthority();
                    }
                }

                return _idAuth;
            }
        }

        public static IAppIdAuthority AppIdAuthority
        {
            [System.Security.SecuritySafeCritical]
            get
            {
                if (_appIdAuth == null)
                {
                    lock (_synchObject)
                    {
                        if (_appIdAuth == null)
                            _appIdAuth = GetAppIdAuthority();
                    }
                }
                return _appIdAuth;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParameters
        {
            [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
            [MarshalAs(UnmanagedType.U4)] public UInt32 Flags;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr CustomStoreList;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr CultureFallbackList;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr ProcessorArchitectureList;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr Source;
            [MarshalAs(UnmanagedType.U2)] public UInt16 ProcArch;

            [Flags]
            public enum CreateFlags
            {
                Nothing = 0,
                StoreListValid = 1,
                CultureListValid = 2,
                ProcessorFallbackListValid = 4,
                ProcessorValid = 8,
                SourceValid = 16,
                IgnoreVisibility = 32
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSource
        {
            [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
            [MarshalAs(UnmanagedType.U4)] public UInt32 Flags;
            [MarshalAs(UnmanagedType.U4)] public UInt32 SourceType;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr Data;

            [Flags]
            public enum SourceFlags
            {
                Definition = 1,
                Reference = 2
            }


            [System.Security.SecurityCritical]  // auto-generated
            public IntPtr ToIntPtr()
            {
                IntPtr p = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, p, false);
                return p;
            }

            [System.Security.SecurityCritical]  // auto-generated
            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(CreateActContextParametersSource));
                Marshal.FreeCoTaskMem(p);
            }
        }

        #if !ISOLATION_IN_MSCORLIB
        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSourceReferenceAppid
        {
            [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
            [MarshalAs(UnmanagedType.U4)] public UInt32 Flags;
            public IReferenceAppId AppId;

            public IntPtr ToIntPtr()
            {
                IntPtr p = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, p, false);
                return p;
            }

            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(CreateActContextParametersSourceReferenceAppid));
                Marshal.FreeCoTaskMem(p);
            }
        }
        #endif // !ISOLATION_IN_MSCORLIB

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSourceDefinitionAppid
        {
            [MarshalAs(UnmanagedType.U4)] public UInt32 Size;
            [MarshalAs(UnmanagedType.U4)] public UInt32 Flags;
            public IDefinitionAppId AppId;

            [System.Security.SecurityCritical]  // auto-generated
            public IntPtr ToIntPtr()
            {
                IntPtr p = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, p, false);
                return p;
            }

            [System.Security.SecurityCritical]  // auto-generated
            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(CreateActContextParametersSourceDefinitionAppid));
                Marshal.FreeCoTaskMem(p);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        internal static IActContext CreateActContext(IDefinitionAppId AppId)
        {
            CreateActContextParameters Parameters;
            CreateActContextParametersSource SourceInfo;
            CreateActContextParametersSourceDefinitionAppid DefAppIdSource;

            Parameters.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParameters));
            Parameters.Flags = (UInt32)CreateActContextParameters.CreateFlags.SourceValid;
            Parameters.CustomStoreList = IntPtr.Zero;
            Parameters.CultureFallbackList = IntPtr.Zero;
            Parameters.ProcessorArchitectureList = IntPtr.Zero;
            Parameters.Source = IntPtr.Zero;
            Parameters.ProcArch = 0;

            SourceInfo.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParametersSource));
            SourceInfo.Flags = 0;
            SourceInfo.SourceType = (UInt32)CreateActContextParametersSource.SourceFlags.Definition;
            SourceInfo.Data = IntPtr.Zero;

            DefAppIdSource.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParametersSourceDefinitionAppid));
            DefAppIdSource.Flags = 0;
            DefAppIdSource.AppId = AppId;

            try
            {
                SourceInfo.Data = DefAppIdSource.ToIntPtr();
                Parameters.Source = SourceInfo.ToIntPtr();

                return CreateActContext(ref Parameters) as IActContext;
            }
            //
            // Don't care about exceptions, but we don't want to leak nonmanaged heap
            //
            finally
            {
                if (SourceInfo.Data != IntPtr.Zero)
                {
                    CreateActContextParametersSourceDefinitionAppid.Destroy(SourceInfo.Data);
                    SourceInfo.Data = IntPtr.Zero;
                }

                if (Parameters.Source != IntPtr.Zero)
                {
                    CreateActContextParametersSource.Destroy(Parameters.Source);
                    Parameters.Source = IntPtr.Zero;
                }
            }
        }

        #if !ISOLATION_IN_MSCORLIB
        internal static IActContext CreateActContext(IReferenceAppId AppId)
        {
            CreateActContextParameters Parameters;
            CreateActContextParametersSource SourceInfo;
            CreateActContextParametersSourceReferenceAppid RefAppIdSource;

            Parameters.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParameters));
            Parameters.Flags = (UInt32)CreateActContextParameters.CreateFlags.SourceValid;
            Parameters.CustomStoreList = IntPtr.Zero;
            Parameters.CultureFallbackList = IntPtr.Zero;
            Parameters.ProcessorArchitectureList = IntPtr.Zero;
            Parameters.Source = IntPtr.Zero;
            Parameters.ProcArch = 0;

            SourceInfo.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParametersSource));
            SourceInfo.Flags = 0;
            SourceInfo.SourceType = (UInt32)CreateActContextParametersSource.SourceFlags.Reference;
            SourceInfo.Data = IntPtr.Zero;

            RefAppIdSource.Size = (UInt32)Marshal.SizeOf(typeof(CreateActContextParametersSourceReferenceAppid));
            RefAppIdSource.Flags = 0;
            RefAppIdSource.AppId = AppId;

            try
            {
                SourceInfo.Data = RefAppIdSource.ToIntPtr();
                Parameters.Source = SourceInfo.ToIntPtr();

                return CreateActContext(ref Parameters) as IActContext;
            }
            //
            // Don't care about exceptions, but we don't want to leak nonmanaged heap
            //
            finally
            {
                if (SourceInfo.Data != IntPtr.Zero)
                {
                    CreateActContextParametersSourceDefinitionAppid.Destroy(SourceInfo.Data);
                    SourceInfo.Data = IntPtr.Zero;
                }

                if (Parameters.Source != IntPtr.Zero)
                {
                    CreateActContextParametersSource.Destroy(Parameters.Source);
                    Parameters.Source = IntPtr.Zero;
                }
            }
        }
        #endif // !ISOLATION_IN_MSCORLIB

        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return:MarshalAs(UnmanagedType.IUnknown)]
        internal static extern object CreateActContext(ref CreateActContextParameters Params);

        // Guids.
        public static Guid IID_ICMS = GetGuidOfType(typeof(CMS.ICMS));

        public static Guid IID_IDefinitionIdentity = GetGuidOfType(typeof(IDefinitionIdentity));
        public static Guid IID_IManifestInformation = GetGuidOfType(typeof(IManifestInformation));
        public static Guid IID_IEnumSTORE_ASSEMBLY = GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY));
        public static Guid IID_IEnumSTORE_ASSEMBLY_FILE = GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
        public static Guid IID_IEnumSTORE_CATEGORY = GetGuidOfType(typeof(IEnumSTORE_CATEGORY));
        public static Guid IID_IEnumSTORE_CATEGORY_INSTANCE = GetGuidOfType(typeof(IEnumSTORE_CATEGORY_INSTANCE));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA = GetGuidOfType(typeof(IEnumSTORE_DEPLOYMENT_METADATA));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY = GetGuidOfType(typeof(IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY));
        public static Guid IID_IStore = GetGuidOfType(typeof(IStore));

        public static Guid GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING =
            new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");

        public static Guid SXS_INSTALL_REFERENCE_SCHEME_SXS_STRONGNAME_SIGNED_PRIVATE_ASSEMBLY =
            new Guid("3ab20ac0-67e8-4512-8385-a487e35df3da");

        // ISSUE - Should change this to stdcall.
        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        internal static extern object CreateCMSFromXml(
            [In] byte[] buffer,
            [In] UInt32 bufferSize,
            [In] IManifestParseErrorCallback Callback,
            [In] ref Guid riid);

        [SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        internal static extern object ParseManifest(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszManifestPath,
            [In] IManifestParseErrorCallback pIManifestParseErrorCallback,
            [In] ref Guid riid);//            string pszManifestPath, IManifestParseErrorCallback pIManifestParseErrorCallback, ref Guid riid);

        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        private static extern object GetUserStore([In] UInt32 Flags, [In] IntPtr hToken, [In] ref Guid riid);

#if !ISOLATION_IN_MSCORLIB
        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        private static extern object GetSystemStore([In] UInt32 Flags, [In] ref Guid riid);
#endif // !ISOLATION_IN_MSCORLIB

        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.Interface)]
        private static extern IIdentityAuthority GetIdentityAuthority();

        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.Interface)]
        private static extern IAppIdAuthority GetAppIdAuthority();

#if !ISOLATION_IN_MSCORLIB
        [ResourceExposure(ResourceScope.None)]
        [DllImport(IsolationDllName, PreserveSig = false)]
        [return :MarshalAs(UnmanagedType.IUnknown)]
        internal static extern object GetUserStateManager([In] UInt32 Flags, [In] IntPtr hToken, [In] ref Guid riid);
#endif // !ISOLATION_IN_MSCORLIB

        internal static Guid GetGuidOfType(Type type)
        {
            GuidAttribute guidAttr = (GuidAttribute)Attribute.GetCustomAttribute(
                type, typeof(GuidAttribute), false);
            return new Guid(guidAttr.Value);
        }
    }

#if !ISOLATION_IN_MSCORLIB
    internal class ApplicationContext
    {
        private IActContext _appcontext = null;

        internal ApplicationContext(IActContext a)
        {
            if (a == null)
                throw new ArgumentNullException();
            _appcontext = a;
        }

        public ApplicationContext(DefinitionAppId appid)
        {
            if (appid == null)
                throw new ArgumentNullException();
            _appcontext = IsolationInterop.CreateActContext(appid._id);
        }

        public ApplicationContext(ReferenceAppId appid)
        {
            if (appid == null)
                throw new ArgumentNullException();
            _appcontext = IsolationInterop.CreateActContext(appid._id);
        }

        public DefinitionAppId Identity
        {
            get
            {
                object o;
                _appcontext.GetAppId(out o);
                return new DefinitionAppId(o as IDefinitionAppId);
            }
        }

        public string BasePath
        {
            get
            {
                string s;
                _appcontext.ApplicationBasePath(0, out s);
                return s;
            }
        }

        public string ReplaceStrings(string culture, string toreplace)
        {
            string replaced;
            _appcontext.ReplaceStringMacros(0, culture, toreplace, out replaced);
            return replaced;
        }

        internal CMS.ICMS GetComponentManifest(DefinitionIdentity component)
        {
            object o;
            _appcontext.GetComponentManifest(0, component._id, ref IsolationInterop.IID_ICMS, out o);
            return o as CMS.ICMS;
        }

        internal string GetComponentManifestPath(DefinitionIdentity component)
        {
            object o;
            string s;
            _appcontext.GetComponentManifest(0, component._id, ref IsolationInterop.IID_IManifestInformation, out o);
            ((IManifestInformation)o).get_FullPath(out s);
            return s;
        }

        public string GetComponentPath(DefinitionIdentity component)
        {
            string retval;
            _appcontext.GetComponentPayloadPath(0, component._id, out retval);
            return retval;
        }

        public DefinitionIdentity MatchReference(ReferenceIdentity TheRef)
        {
            object o;
            _appcontext.FindReferenceInContext(0, TheRef._id, out o);
            return new DefinitionIdentity(o as IDefinitionIdentity);
        }

        public EnumDefinitionIdentity Components
        {
            get
            {
                object o;
                _appcontext.EnumComponents(0, out o);
                return new EnumDefinitionIdentity(o as IEnumDefinitionIdentity);
            }
        }

        public void PrepareForExecution()
        {
            _appcontext.PrepareForExecution(IntPtr.Zero, IntPtr.Zero);
        }

        public enum ApplicationState
        {
            Undefined = 0,
            Starting = 1,
            Running = 2
        }

        public enum ApplicationStateDisposition
        {
            Undefined = 0,
            Starting = 1,
            Starting_Migrated = (1 | (1 << 16)),
            Running = 2,
            Running_FirstTime = (2 | (1 << 17)),
        }

        public ApplicationStateDisposition SetApplicationState(ApplicationState s)
        {
            UInt32 theDisposition;
            _appcontext.SetApplicationRunningState(0, (UInt32)s, out theDisposition);
            return (ApplicationStateDisposition)theDisposition;
        }

        public string StateLocation
        {
            get
            {
                string s;
                _appcontext.GetApplicationStateFilesystemLocation(0, UIntPtr.Zero, IntPtr.Zero, out s);
                return s;
            }
        }
    }
#endif // !ISOLATION_IN_MSCORLIB

    [ComImport]
    [Guid("81c85208-fe61-4c15-b5bb-ff5ea66baad9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IManifestInformation
    {
        [SecurityCritical]
        void get_FullPath(
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string FullPath
            );
    }

    [ComImport]
    [Guid("0af57545-a72a-4fbe-813c-8554ed7d4528")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActContext
    {
        // Returns IDefinitionAppId
        [SecurityCritical]
        void GetAppId(
            [Out, MarshalAs(UnmanagedType.Interface)] out object AppId
            );

        // Returns IEnumCATEGORY
        [SecurityCritical]
        void EnumCategories(
            [In] UInt32 Flags,
            [In] IReferenceIdentity CategoryToMatch,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object EnumOut
            );

        // Returns IEnumCATEGORY_SUBCATEGORY
        [SecurityCritical]
        void EnumSubcategories(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity CategoryId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string SubcategoryPattern,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object EnumOut
            );

        // Returns IEnumCATEGORY_INSTANCE
        [SecurityCritical]
        void EnumCategoryInstances(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity CategoryId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Subcategory,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object EnumOut
            );

        [SecurityCritical]
        void ReplaceStringMacros(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Culture,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ReplacementPattern,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string Replaced
            );

        [SecurityCritical]
        void GetComponentStringTableStrings(
            [In] UInt32 Flags,
            [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr ComponentIndex,
            [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr StringCount,
            [Out, MarshalAs(UnmanagedType.LPArray)] string[] SourceStrings,
            [Out, MarshalAs(UnmanagedType.LPArray)] out string[] DestinationStrings,
            [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr CultureFallbacks
            );

        [SecurityCritical]
        void GetApplicationProperties(
            [In] UInt32 Flags,
            [In] UIntPtr cProperties,
            [In, MarshalAs(UnmanagedType.LPArray)] string[] PropertyNames,
            [Out, MarshalAs(UnmanagedType.LPArray)] out string[] PropertyValues,
            [Out, MarshalAs(UnmanagedType.LPArray)] out UIntPtr[] ComponentIndicies
            );

        [SecurityCritical]
        void ApplicationBasePath(
            [In] UInt32 Flags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ApplicationPath
            );

        // Returns either IDefinitionIdentity or ICMS
        [SecurityCritical]
        void GetComponentManifest(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity ComponentId,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ManifestInteface
            );

        [SecurityCritical]
        void GetComponentPayloadPath(
            [In] UInt32 Flags,
            [In] IDefinitionIdentity ComponentId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string PayloadPath
            );

        // Returns an IDefinitionIdentity
        [SecurityCritical]
        void FindReferenceInContext(
            [In] UInt32 dwFlags,
            [In] IReferenceIdentity Reference,
            [Out, MarshalAs(UnmanagedType.Interface)] out object MatchedDefinition
            );

        // Returns an IActContext
        [SecurityCritical]
        void CreateActContextFromCategoryInstance(
            [In] UInt32 dwFlags,
            [In] ref CATEGORY_INSTANCE CategoryInstance,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppCreatedAppContext
            );

        // Returns an IEnumDefinitionIdentity
        [SecurityCritical]
        void EnumComponents(
            [In] UInt32 dwFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppIdentityEnum
            );

        // Inputs is a pointer to an IAPP_CONTEXT_PREPARE_FOR_EXECUTION_INPUTS
        // structure, which for now should/can be NULL - pass IntPtr.Zero. Outputs
        // should point at an IAPP_CONTEXT_PREPARE_FOR_EXECUTION_OUTPUTS structure,
        // which should/can be likewise null for now.
        [SecurityCritical]
        void PrepareForExecution(
            [In, MarshalAs(UnmanagedType.SysInt)] IntPtr Inputs,
            [In, MarshalAs(UnmanagedType.SysInt)] IntPtr Outputs
            );

        [SecurityCritical]
        void SetApplicationRunningState(
            [In] UInt32 dwFlags,
            [In] UInt32 ulState,
            [Out] out UInt32 ulDisposition
            );

        // For now, the coordinate list (should be a pointer to a STATE_COORDINATE_LIST
        // should be null.
        [SecurityCritical]
        void GetApplicationStateFilesystemLocation(
            [In] UInt32 dwFlags,
            [In] UIntPtr Component,
            [In, MarshalAs(UnmanagedType.SysInt)] IntPtr pCoordinateList,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszPath
            );

        [SecurityCritical]
        void FindComponentsByDefinition(
            [In] UInt32 dwFlags,
            [In] UIntPtr ComponentCount,
            [In, MarshalAs(UnmanagedType.LPArray)] IDefinitionIdentity[] Components,
            [Out, MarshalAs(UnmanagedType.LPArray)] UIntPtr[] Indicies,
            [Out, MarshalAs(UnmanagedType.LPArray)] UInt32[] Dispositions
            );

        [SecurityCritical]
        void FindComponentsByReference(
            [In] UInt32 dwFlags,
            [In] UIntPtr Components,
            [In, MarshalAs(UnmanagedType.LPArray)] IReferenceIdentity[] References,
            [Out, MarshalAs(UnmanagedType.LPArray)] UIntPtr[] Indicies,
            [Out, MarshalAs(UnmanagedType.LPArray)] UInt32[] Dispositions
            );
    }

    enum StateManager_RunningState
    {
        Undefined = 0,
        Starting = 1,
        Running = 2
    };

    [ComImport]
    [Guid("07662534-750b-4ed5-9cfb-1c5bc5acfd07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IStateManager
    {
        [SecurityCritical]
        void PrepareApplicationState(
            [In] UIntPtr Inputs,
            ref UIntPtr Outputs
            );

        [SecurityCritical]
        void SetApplicationRunningState(
            [In] UInt32 Flags,
            [In] IActContext Context,
            [In] UInt32 RunningState,
            [Out] out UInt32 Disposition
            );

        [SecurityCritical]
        void GetApplicationStateFilesystemLocation(
            [In] UInt32 Flags,
            [In] IDefinitionAppId Appidentity,
            [In] IDefinitionIdentity ComponentIdentity,
            [In] UIntPtr Coordinates,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string Path
            );

        [SecurityCritical]
        void Scavenge(
            [In] UInt32 Flags,
            [Out] out UInt32 Disposition
            );
    };
#endif
}

