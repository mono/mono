                    
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Globalization;
    using System.Threading;
    using System.Security;
    using System.Deployment.Internal.Isolation;
    
    namespace System.Deployment.Internal.Isolation.Manifest
    {
    
                     
        internal enum CMSSECTIONID
        {
            CMSSECTIONID_FILE_SECTION = 1,
            CMSSECTIONID_CATEGORY_INSTANCE_SECTION = 2,
            CMSSECTIONID_COM_REDIRECTION_SECTION = 3,
            CMSSECTIONID_PROGID_REDIRECTION_SECTION = 4,
            CMSSECTIONID_CLR_SURROGATE_SECTION = 5,
            CMSSECTIONID_ASSEMBLY_REFERENCE_SECTION = 6,
            CMSSECTIONID_WINDOW_CLASS_SECTION = 8,
            CMSSECTIONID_STRING_SECTION = 9,
            CMSSECTIONID_ENTRYPOINT_SECTION = 10,
            CMSSECTIONID_PERMISSION_SET_SECTION = 11,
            CMSSECTIONENTRYID_METADATA = 12,
            CMSSECTIONID_ASSEMBLY_REQUEST_SECTION = 13,
            CMSSECTIONID_REGISTRY_KEY_SECTION = 16,
            CMSSECTIONID_DIRECTORY_SECTION = 17,
            CMSSECTIONID_FILE_ASSOCIATION_SECTION = 18,
            CMSSECTIONID_COMPATIBLE_FRAMEWORKS_SECTION = 19,
            CMSSECTIONID_EVENT_SECTION = 101,
            CMSSECTIONID_EVENT_MAP_SECTION = 102,
            CMSSECTIONID_EVENT_TAG_SECTION = 103,
            CMSSECTIONID_COUNTERSET_SECTION = 110,
            CMSSECTIONID_COUNTER_SECTION = 111,
        }
    
        internal enum CMS_ASSEMBLY_DEPLOYMENT_FLAG
        {
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_BEFORE_APPLICATION_STARTUP = 4,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_RUN_AFTER_INSTALL = 16,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_INSTALL = 32,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_TRUST_URL_PARAMETERS = 64,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_DISALLOW_URL_ACTIVATION = 128,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_MAP_FILE_EXTENSIONS = 256,
            CMS_ASSEMBLY_DEPLOYMENT_FLAG_CREATE_DESKTOP_SHORTCUT = 512,
        }
    
        internal enum CMS_ASSEMBLY_REFERENCE_FLAG
        {
            CMS_ASSEMBLY_REFERENCE_FLAG_OPTIONAL = 1,
            CMS_ASSEMBLY_REFERENCE_FLAG_VISIBLE = 2,
            CMS_ASSEMBLY_REFERENCE_FLAG_FOLLOW = 4,
            CMS_ASSEMBLY_REFERENCE_FLAG_IS_PLATFORM = 8,
            CMS_ASSEMBLY_REFERENCE_FLAG_CULTURE_WILDCARDED = 16,
            CMS_ASSEMBLY_REFERENCE_FLAG_PROCESSOR_ARCHITECTURE_WILDCARDED = 32,
            CMS_ASSEMBLY_REFERENCE_FLAG_PREREQUISITE = 128,
        }
    
        internal enum CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG
        {
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_OPTIONAL = 1,
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_VISIBLE = 2,
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_PREREQUISITE = 4,
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_RESOURCE_FALLBACK_CULTURE_INTERNAL = 8,
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_INSTALL = 16,
            CMS_ASSEMBLY_REFERENCE_DEPENDENT_ASSEMBLY_FLAG_ALLOW_DELAYED_BINDING = 32,
        }
    
        internal enum CMS_FILE_FLAG
        {
            CMS_FILE_FLAG_OPTIONAL = 1,
        }
    
        internal enum CMS_ENTRY_POINT_FLAG
        {
            CMS_ENTRY_POINT_FLAG_HOST_IN_BROWSER = 1,
            CMS_ENTRY_POINT_FLAG_CUSTOMHOSTSPECIFIED = 2,
	    CMS_ENTRY_POINT_FLAG_CUSTOMUX = 4,

        }
    
        internal enum CMS_COM_SERVER_FLAG
        {
            CMS_COM_SERVER_FLAG_IS_CLR_CLASS = 1,
        }
    
                    #if !ISOLATION_IN_MSCORLIB
        internal enum CMS_REGISTRY_KEY_FLAG
        {
            CMS_REGISTRY_KEY_FLAG_OWNER = 1,
            CMS_REGISTRY_KEY_FLAG_LEAF_IN_MANIFEST = 2,
        }
    
        internal enum CMS_REGISTRY_VALUE_FLAG
        {
            CMS_REGISTRY_VALUE_FLAG_OWNER = 1,
        }
    
        internal enum CMS_DIRECTORY_FLAG
        {
            CMS_DIRECTORY_FLAG_OWNER = 1,
        }
    
        internal enum CMS_MANIFEST_FLAG
        {
            CMS_MANIFEST_FLAG_ASSEMBLY = 1,
            CMS_MANIFEST_FLAG_CATEGORY = 2,
            CMS_MANIFEST_FLAG_FEATURE = 3,
            CMS_MANIFEST_FLAG_APPLICATION = 4,
            CMS_MANIFEST_FLAG_USEMANIFESTFORTRUST = 8,
        }
                    #endif
    
        internal enum CMS_USAGE_PATTERN
        {
            CMS_USAGE_PATTERN_SCOPE_APPLICATION = 1,
            CMS_USAGE_PATTERN_SCOPE_PROCESS = 2,
            CMS_USAGE_PATTERN_SCOPE_MACHINE = 3,
            CMS_USAGE_PATTERN_SCOPE_MASK = 7,
        }
    
        internal enum CMS_SCHEMA_VERSION
        {
            CMS_SCHEMA_VERSION_V1 = 1,
        }
        internal enum CMS_FILE_HASH_ALGORITHM
        {
            CMS_FILE_HASH_ALGORITHM_SHA1 = 1,
            CMS_FILE_HASH_ALGORITHM_SHA256 = 2,
            CMS_FILE_HASH_ALGORITHM_SHA384 = 3,
            CMS_FILE_HASH_ALGORITHM_SHA512 = 4,
            CMS_FILE_HASH_ALGORITHM_MD5 = 5,
            CMS_FILE_HASH_ALGORITHM_MD4 = 6,
            CMS_FILE_HASH_ALGORITHM_MD2 = 7,
        }
        internal enum CMS_TIME_UNIT_TYPE
        {
            CMS_TIME_UNIT_TYPE_HOURS = 1,
            CMS_TIME_UNIT_TYPE_DAYS = 2,
            CMS_TIME_UNIT_TYPE_WEEKS = 3,
            CMS_TIME_UNIT_TYPE_MONTHS = 4,
        }
                    #if !ISOLATION_IN_MSCORLIB
        internal enum CMS_REGISTRY_VALUE_TYPE
        {
            CMS_REGISTRY_VALUE_TYPE_NONE = 0,
            CMS_REGISTRY_VALUE_TYPE_SZ = 1,
            CMS_REGISTRY_VALUE_TYPE_EXPAND_SZ = 2,
            CMS_REGISTRY_VALUE_TYPE_MULTI_SZ = 3,
            CMS_REGISTRY_VALUE_TYPE_BINARY = 4,
            CMS_REGISTRY_VALUE_TYPE_DWORD = 5,
            CMS_REGISTRY_VALUE_TYPE_DWORD_LITTLE_ENDIAN = 6,
            CMS_REGISTRY_VALUE_TYPE_DWORD_BIG_ENDIAN = 7,
            CMS_REGISTRY_VALUE_TYPE_LINK = 8,
            CMS_REGISTRY_VALUE_TYPE_RESOURCE_LIST = 9,
            CMS_REGISTRY_VALUE_TYPE_FULL_RESOURCE_DESCRIPTOR = 10,
            CMS_REGISTRY_VALUE_TYPE_RESOURCE_REQUIREMENTS_LIST = 11,
            CMS_REGISTRY_VALUE_TYPE_QWORD = 12,
            CMS_REGISTRY_VALUE_TYPE_QWORD_LITTLE_ENDIAN = 13,
        }
        internal enum CMS_REGISTRY_VALUE_HINT
        {
            CMS_REGISTRY_VALUE_HINT_REPLACE = 1,
            CMS_REGISTRY_VALUE_HINT_APPEND = 2,
            CMS_REGISTRY_VALUE_HINT_PREPEND = 3,
        }
        internal enum CMS_SYSTEM_PROTECTION
        {
            CMS_SYSTEM_PROTECTION_READ_ONLY_IGNORE_WRITES = 1,
            CMS_SYSTEM_PROTECTION_READ_ONLY_FAIL_WRITES = 2,
            CMS_SYSTEM_PROTECTION_OS_ONLY_IGNORE_WRITES = 3,
            CMS_SYSTEM_PROTECTION_OS_ONLY_FAIL_WRITES = 4,
            CMS_SYSTEM_PROTECTION_TRANSACTED = 5,
            CMS_SYSTEM_PROTECTION_APPLICATION_VIRTUALIZED = 6,
            CMS_SYSTEM_PROTECTION_USER_VIRTUALIZED = 7,
            CMS_SYSTEM_PROTECTION_APPLICATION_AND_USER_VIRTUALIZED = 8,
            CMS_SYSTEM_PROTECTION_INHERIT = 9,
            CMS_SYSTEM_PROTECTION_NOT_PROTECTED = 10,
        }
                    #endif
        internal enum CMS_FILE_WRITABLE_TYPE
        {
            CMS_FILE_WRITABLE_TYPE_NOT_WRITABLE = 1,
            CMS_FILE_WRITABLE_TYPE_APPLICATION_DATA = 2,
        }
  
        internal enum CMS_HASH_TRANSFORM
        {
            CMS_HASH_TRANSFORM_IDENTITY = 1,
            CMS_HASH_TRANSFORM_MANIFESTINVARIANT = 2,
        }
        internal enum CMS_HASH_DIGESTMETHOD
        {
            CMS_HASH_DIGESTMETHOD_SHA1 = 1,
            CMS_HASH_DIGESTMETHOD_SHA256 = 2,
            CMS_HASH_DIGESTMETHOD_SHA384 = 3,
            CMS_HASH_DIGESTMETHOD_SHA512 = 4,
        }
        
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("a504e5b0-8ccf-4cb4-9902-c9d1b9abd033")]
        internal interface ICMS 
        {
            IDefinitionIdentity Identity { [SecurityCritical]get; }
            ISection FileSection { [SecurityCritical]get; }
            ISection CategoryMembershipSection { [SecurityCritical]get; }
            ISection COMRedirectionSection { [SecurityCritical]get; }
            ISection ProgIdRedirectionSection { [SecurityCritical]get; }
            ISection CLRSurrogateSection { [SecurityCritical]get; }
            ISection AssemblyReferenceSection { [SecurityCritical]get; }
            ISection WindowClassSection { [SecurityCritical]get; }
            ISection StringSection { [SecurityCritical]get; }
            ISection EntryPointSection { [SecurityCritical]get; }
            ISection PermissionSetSection { [SecurityCritical]get; }
            ISectionEntry MetadataSectionEntry { [SecurityCritical]get; }
            ISection AssemblyRequestSection { [SecurityCritical]get; }
            ISection RegistryKeySection { [SecurityCritical]get; }
            ISection DirectorySection { [SecurityCritical]get; }
            ISection FileAssociationSection { [SecurityCritical]get; }
            ISection CompatibleFrameworksSection { [SecurityCritical]get; }
            ISection EventSection { [SecurityCritical]get; }
            ISection EventMapSection { [SecurityCritical]get; }
            ISection EventTagSection { [SecurityCritical]get; }
            ISection CounterSetSection { [SecurityCritical]get; }
            ISection CounterSection { [SecurityCritical]get; }
        }
    
        //++! start object [MuiResourceIdLookupMap]
        [StructLayout(LayoutKind.Sequential)]
        internal class MuiResourceIdLookupMapEntry
        {
            public uint Count;
        };
    
        internal enum MuiResourceIdLookupMapEntryFieldId
        {
            MuiResourceIdLookupMap_Count,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("24abe1f7-a396-4a03-9adf-1d5b86a5569f")]
        internal interface IMuiResourceIdLookupMapEntry
        {
            MuiResourceIdLookupMapEntry AllData { [SecurityCritical]get; }

            uint Count { [SecurityCritical]get; }
        };
    
        //++! end object [MuiResourceIdLookupMap]
        //++! start object [MuiResourceTypeIdString]
        [StructLayout(LayoutKind.Sequential)]
        internal class MuiResourceTypeIdStringEntry : IDisposable
        {
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr StringIds;
            public uint StringIdsSize;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr IntegerIds;
            public uint IntegerIdsSize;
            ~MuiResourceTypeIdStringEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (StringIds != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(StringIds);
                    StringIds = IntPtr.Zero;
                }
                if (IntegerIds != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(IntegerIds);
                    IntegerIds = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum MuiResourceTypeIdStringEntryFieldId
        {
            MuiResourceTypeIdString_StringIds,
            MuiResourceTypeIdString_StringIdsSize,
            MuiResourceTypeIdString_IntegerIds,
            MuiResourceTypeIdString_IntegerIdsSize,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("11df5cad-c183-479b-9a44-3842b71639ce")]
        internal interface IMuiResourceTypeIdStringEntry
        {
            MuiResourceTypeIdStringEntry AllData { [SecurityCritical]get; }

            object StringIds { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            object IntegerIds { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
        };
    
        //++! end object [MuiResourceTypeIdString]
        //++! start object [MuiResourceTypeIdInt]
        [StructLayout(LayoutKind.Sequential)]
        internal class MuiResourceTypeIdIntEntry : IDisposable
        {
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr StringIds;
            public uint StringIdsSize;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr IntegerIds;
            public uint IntegerIdsSize;
            ~MuiResourceTypeIdIntEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (StringIds != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(StringIds);
                    StringIds = IntPtr.Zero;
                }
                if (IntegerIds != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(IntegerIds);
                    IntegerIds = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum MuiResourceTypeIdIntEntryFieldId
        {
            MuiResourceTypeIdInt_StringIds,
            MuiResourceTypeIdInt_StringIdsSize,
            MuiResourceTypeIdInt_IntegerIds,
            MuiResourceTypeIdInt_IntegerIdsSize,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("55b2dec1-d0f6-4bf4-91b1-30f73ad8e4df")]
        internal interface IMuiResourceTypeIdIntEntry
        {
            MuiResourceTypeIdIntEntry AllData { [SecurityCritical]get; }

            object StringIds { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            object IntegerIds { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
        };
    
        //++! end object [MuiResourceTypeIdInt]
        //++! start object [MuiResourceMap]
        [StructLayout(LayoutKind.Sequential)]
        internal class MuiResourceMapEntry : IDisposable
        {
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr ResourceTypeIdInt;
            public uint ResourceTypeIdIntSize;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr ResourceTypeIdString;
            public uint ResourceTypeIdStringSize;
            ~MuiResourceMapEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (ResourceTypeIdInt != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ResourceTypeIdInt);
                    ResourceTypeIdInt = IntPtr.Zero;
                }
                if (ResourceTypeIdString != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ResourceTypeIdString);
                    ResourceTypeIdString = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum MuiResourceMapEntryFieldId
        {
            MuiResourceMap_ResourceTypeIdInt,
            MuiResourceMap_ResourceTypeIdIntSize,
            MuiResourceMap_ResourceTypeIdString,
            MuiResourceMap_ResourceTypeIdStringSize,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("397927f5-10f2-4ecb-bfe1-3c264212a193")]
        internal interface IMuiResourceMapEntry
        {
            MuiResourceMapEntry AllData { [SecurityCritical]get; }

            object ResourceTypeIdInt { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            object ResourceTypeIdString { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
        };
    
        //++! end object [MuiResourceMap]

        //++! start object [HashElement]
        [StructLayout(LayoutKind.Sequential)]
        internal class HashElementEntry : IDisposable
        {
            public uint index;
            public byte Transform;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr TransformMetadata;
            public uint TransformMetadataSize;
            public byte DigestMethod;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr DigestValue;
            public uint DigestValueSize;
        [MarshalAs(UnmanagedType.LPWStr)] public string Xml;
            ~HashElementEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (TransformMetadata != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(TransformMetadata);
                    TransformMetadata = IntPtr.Zero;
                }
                if (DigestValue != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(DigestValue);
                    DigestValue = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum HashElementEntryFieldId
        {
            HashElement_Transform,
            HashElement_TransformMetadata,
            HashElement_TransformMetadataSize,
            HashElement_DigestMethod,
            HashElement_DigestValue,
            HashElement_DigestValueSize,
            HashElement_Xml,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9D46FB70-7B54-4f4f-9331-BA9E87833FF5")]
        internal interface IHashElementEntry
        {
            HashElementEntry AllData { [SecurityCritical]get; }

            uint index { [SecurityCritical]get; }
            byte Transform { [SecurityCritical]get; }
            object TransformMetadata { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            byte DigestMethod { [SecurityCritical]get; }
            object DigestValue { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            string Xml { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [HashElement]

        //++! start object [File]
        [StructLayout(LayoutKind.Sequential)]
        internal class FileEntry : IDisposable
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            public uint HashAlgorithm;
            [MarshalAs(UnmanagedType.LPWStr)] public string LoadFrom;
            [MarshalAs(UnmanagedType.LPWStr)] public string SourcePath;
            [MarshalAs(UnmanagedType.LPWStr)] public string ImportPath;
            [MarshalAs(UnmanagedType.LPWStr)] public string SourceName;
            [MarshalAs(UnmanagedType.LPWStr)] public string Location;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr HashValue;
            public uint HashValueSize;
            public ulong Size;
            [MarshalAs(UnmanagedType.LPWStr)] public string Group;
            public uint Flags;
            public MuiResourceMapEntry MuiMapping;
            public uint WritableType;
            public ISection HashElements;
            ~FileEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (HashValue != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(HashValue);
                    HashValue = IntPtr.Zero;
                }
    
                                if (fDisposing) {    
                                    if( MuiMapping != null) {
                                        MuiMapping.Dispose(true);
                                        MuiMapping = null;
                                    }
                                    
                    System.GC.SuppressFinalize(this);
                                }
            }
        };
    
        internal enum FileEntryFieldId
        {
            File_HashAlgorithm,
            File_LoadFrom,
            File_SourcePath,
            File_ImportPath,
            File_SourceName,
            File_Location,
            File_HashValue,
            File_HashValueSize,
            File_Size,
            File_Group,
            File_Flags,
            File_MuiMapping,
            File_WritableType,
            File_HashElements,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("A2A55FAD-349B-469b-BF12-ADC33D14A937")]
        internal interface IFileEntry
        {
            FileEntry AllData { [SecurityCritical]get; }

            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint HashAlgorithm { [SecurityCritical]get; }
            string LoadFrom { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string SourcePath { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string ImportPath { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string SourceName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Location { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            object HashValue { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            ulong Size { [SecurityCritical]get; }
            string Group { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint Flags { [SecurityCritical]get; }
            IMuiResourceMapEntry MuiMapping { [SecurityCritical]get; }
            uint WritableType { [SecurityCritical]get; }
            ISection HashElements { [SecurityCritical]get; }
        };
    
        //++! end object [File]
     //++! start object [FileAssociation]
     [StructLayout(LayoutKind.Sequential)]
     internal class FileAssociationEntry
     {
         [MarshalAs(UnmanagedType.LPWStr)] public string Extension;
         [MarshalAs(UnmanagedType.LPWStr)] public string Description;
         [MarshalAs(UnmanagedType.LPWStr)] public string ProgID;
         [MarshalAs(UnmanagedType.LPWStr)] public string DefaultIcon;
         [MarshalAs(UnmanagedType.LPWStr)] public string Parameter;
     };
 
     internal enum FileAssociationEntryFieldId
     {
         FileAssociation_Description,
         FileAssociation_ProgID,
         FileAssociation_DefaultIcon,
         FileAssociation_Parameter,
     };
 
     [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0C66F299-E08E-48c5-9264-7CCBEB4D5CBB")]
     internal interface IFileAssociationEntry
     {
         FileAssociationEntry AllData { [SecurityCritical]get; }

         string Extension { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
         string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
         string ProgID { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
         string DefaultIcon { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
         string Parameter { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
     };
 
     //++! end object [FileAssociation]
        //++! start object [CategoryMembershipData]
        [StructLayout(LayoutKind.Sequential)]
        internal class CategoryMembershipDataEntry
        {
            public uint index;
        [MarshalAs(UnmanagedType.LPWStr)] public string Xml;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
        };
    
        internal enum CategoryMembershipDataEntryFieldId
        {
            CategoryMembershipData_Xml,
            CategoryMembershipData_Description,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("DA0C3B27-6B6B-4b80-A8F8-6CE14F4BC0A4")]
        internal interface ICategoryMembershipDataEntry
        {
            CategoryMembershipDataEntry AllData { [SecurityCritical]get; }

            uint index { [SecurityCritical]get; }
            string Xml { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [CategoryMembershipData]
        //++! start object [SubcategoryMembership]
        [StructLayout(LayoutKind.Sequential)]
        internal class SubcategoryMembershipEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Subcategory;
            public ISection CategoryMembershipData;
        };
    
        internal enum SubcategoryMembershipEntryFieldId
        {
            SubcategoryMembership_CategoryMembershipData,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("5A7A54D7-5AD5-418e-AB7A-CF823A8D48D0")]
        internal interface ISubcategoryMembershipEntry
        {
            SubcategoryMembershipEntry AllData { [SecurityCritical]get; }

            string Subcategory { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ISection CategoryMembershipData { [SecurityCritical]get; }
        };
    
        //++! end object [SubcategoryMembership]
        //++! start object [CategoryMembership]
        [StructLayout(LayoutKind.Sequential)]
        internal class CategoryMembershipEntry
        {
            public IDefinitionIdentity Identity;
            public ISection SubcategoryMembership;
        };
    
        internal enum CategoryMembershipEntryFieldId
        {
            CategoryMembership_SubcategoryMembership,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("97FDCA77-B6F2-4718-A1EB-29D0AECE9C03")]
        internal interface ICategoryMembershipEntry
        {
            CategoryMembershipEntry AllData { [SecurityCritical]get; }

            IDefinitionIdentity Identity { [SecurityCritical]get; }
            ISection SubcategoryMembership { [SecurityCritical]get; }
        };
    
        //++! end object [CategoryMembership]
        //++! start object [COMServer]
        [StructLayout(LayoutKind.Sequential)]
        internal class COMServerEntry
        {
            public Guid Clsid;
            public uint Flags;
            public Guid ConfiguredGuid;
            public Guid ImplementedClsid;
            public Guid TypeLibrary;
            public uint ThreadingModel;
            [MarshalAs(UnmanagedType.LPWStr)] public string RuntimeVersion;
            [MarshalAs(UnmanagedType.LPWStr)] public string HostFile;
        };
    
        internal enum COMServerEntryFieldId
        {
            COMServer_Flags,
            COMServer_ConfiguredGuid,
            COMServer_ImplementedClsid,
            COMServer_TypeLibrary,
            COMServer_ThreadingModel,
            COMServer_RuntimeVersion,
            COMServer_HostFile,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3903B11B-FBE8-477c-825F-DB828B5FD174")]
        internal interface ICOMServerEntry
        {
            COMServerEntry AllData { [SecurityCritical]get; }

            Guid Clsid { [SecurityCritical]get; }
            uint Flags { [SecurityCritical]get; }
            Guid ConfiguredGuid { [SecurityCritical]get; }
            Guid ImplementedClsid { [SecurityCritical]get; }
            Guid TypeLibrary { [SecurityCritical]get; }
            uint ThreadingModel { [SecurityCritical]get; }
            string RuntimeVersion { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string HostFile { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [COMServer]
        //++! start object [ProgIdRedirection]
        [StructLayout(LayoutKind.Sequential)]
        internal class ProgIdRedirectionEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string ProgId;
            public Guid RedirectedGuid;
        };
    
        internal enum ProgIdRedirectionEntryFieldId
        {
            ProgIdRedirection_RedirectedGuid,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("54F198EC-A63A-45ea-A984-452F68D9B35B")]
        internal interface IProgIdRedirectionEntry
        {
            ProgIdRedirectionEntry AllData { [SecurityCritical]get; }

            string ProgId { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            Guid RedirectedGuid { [SecurityCritical]get; }
        };
    
        //++! end object [ProgIdRedirection]
        //++! start object [CLRSurrogate]
        [StructLayout(LayoutKind.Sequential)]
        internal class CLRSurrogateEntry
        {
            public Guid Clsid;
            [MarshalAs(UnmanagedType.LPWStr)] public string RuntimeVersion;
            [MarshalAs(UnmanagedType.LPWStr)] public string ClassName;
        };
    
        internal enum CLRSurrogateEntryFieldId
        {
            CLRSurrogate_RuntimeVersion,
            CLRSurrogate_ClassName,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1E0422A1-F0D2-44ae-914B-8A2DECCFD22B")]
        internal interface ICLRSurrogateEntry
        {
            CLRSurrogateEntry AllData { [SecurityCritical]get; }

            Guid Clsid { [SecurityCritical]get; }
            string RuntimeVersion { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string ClassName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [CLRSurrogate]

        //++! start object [AssemblyReferenceDependentAssembly]
        [StructLayout(LayoutKind.Sequential)]
        internal class AssemblyReferenceDependentAssemblyEntry : IDisposable
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Group;
            [MarshalAs(UnmanagedType.LPWStr)] public string Codebase;
            public ulong Size;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr HashValue;
            public uint HashValueSize;
            public uint HashAlgorithm;
            public uint Flags;
            [MarshalAs(UnmanagedType.LPWStr)] public string ResourceFallbackCulture;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
            [MarshalAs(UnmanagedType.LPWStr)] public string SupportUrl;
            public ISection HashElements;
            ~AssemblyReferenceDependentAssemblyEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (HashValue != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(HashValue);
                    HashValue = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum AssemblyReferenceDependentAssemblyEntryFieldId
        {
            AssemblyReferenceDependentAssembly_Group,
            AssemblyReferenceDependentAssembly_Codebase,
            AssemblyReferenceDependentAssembly_Size,
            AssemblyReferenceDependentAssembly_HashValue,
            AssemblyReferenceDependentAssembly_HashValueSize,
            AssemblyReferenceDependentAssembly_HashAlgorithm,
            AssemblyReferenceDependentAssembly_Flags,
            AssemblyReferenceDependentAssembly_ResourceFallbackCulture,
            AssemblyReferenceDependentAssembly_Description,
            AssemblyReferenceDependentAssembly_SupportUrl,
            AssemblyReferenceDependentAssembly_HashElements,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C31FF59E-CD25-47b8-9EF3-CF4433EB97CC")]
        internal interface IAssemblyReferenceDependentAssemblyEntry
        {
            AssemblyReferenceDependentAssemblyEntry AllData { [SecurityCritical]get; }

            string Group { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Codebase { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ulong Size { [SecurityCritical]get; }
            object HashValue { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            uint HashAlgorithm { [SecurityCritical]get; }
            uint Flags { [SecurityCritical]get; }
            string ResourceFallbackCulture { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string SupportUrl { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ISection HashElements { [SecurityCritical]get; }
        };
    
        //++! end object [AssemblyReferenceDependentAssembly]
        //++! start object [AssemblyReference]
        [StructLayout(LayoutKind.Sequential)]
        internal class AssemblyReferenceEntry
        {
            public IReferenceIdentity ReferenceIdentity;
            public uint Flags;
            public AssemblyReferenceDependentAssemblyEntry DependentAssembly;
        };
    
        internal enum AssemblyReferenceEntryFieldId
        {
            AssemblyReference_Flags,
            AssemblyReference_DependentAssembly,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("FD47B733-AFBC-45e4-B7C2-BBEB1D9F766C")]
        internal interface IAssemblyReferenceEntry
        {
            AssemblyReferenceEntry AllData { [SecurityCritical]get; }

            IReferenceIdentity ReferenceIdentity { [SecurityCritical]get; }
            uint Flags { [SecurityCritical]get; }
            IAssemblyReferenceDependentAssemblyEntry DependentAssembly { [SecurityCritical]get; }
        };
    
        //++! end object [AssemblyReference]

        //++! start object [WindowClass]
        [StructLayout(LayoutKind.Sequential)]
        internal class WindowClassEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string ClassName;
            [MarshalAs(UnmanagedType.LPWStr)] public string HostDll;
            public bool fVersioned;
        };
    
        internal enum WindowClassEntryFieldId
        {
            WindowClass_HostDll,
            WindowClass_fVersioned,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BA")]
        internal interface IWindowClassEntry
        {
            WindowClassEntry AllData { [SecurityCritical]get; }

            string ClassName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string HostDll { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            bool fVersioned { [SecurityCritical]get; }
        };
    
        //++! end object [WindowClass]
        //++! start object [ResourceTableMapping]
        [StructLayout(LayoutKind.Sequential)]
        internal class ResourceTableMappingEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string id;
            [MarshalAs(UnmanagedType.LPWStr)] public string FinalStringMapped;
        };
    
        internal enum ResourceTableMappingEntryFieldId
        {
            ResourceTableMapping_FinalStringMapped,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("70A4ECEE-B195-4c59-85BF-44B6ACA83F07")]
        internal interface IResourceTableMappingEntry
        {
            ResourceTableMappingEntry AllData { [SecurityCritical]get; }

            string id { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string FinalStringMapped { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [ResourceTableMapping]

        //++! start object [EntryPoint]
        [StructLayout(LayoutKind.Sequential)]
        internal class EntryPointEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string CommandLine_File;
            [MarshalAs(UnmanagedType.LPWStr)] public string CommandLine_Parameters;
            public IReferenceIdentity Identity;
            public uint Flags;
        };
    
        internal enum EntryPointEntryFieldId
        {
            EntryPoint_CommandLine_File,
            EntryPoint_CommandLine_Parameters,
            EntryPoint_Identity,
            EntryPoint_Flags,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1583EFE9-832F-4d08-B041-CAC5ACEDB948")]
        internal interface IEntryPointEntry
        {
            EntryPointEntry AllData { [SecurityCritical]get; }

            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string CommandLine_File { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string CommandLine_Parameters { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            IReferenceIdentity Identity { [SecurityCritical]get; }
            uint Flags { [SecurityCritical]get; }
        };
    
        //++! end object [EntryPoint]

        //++! start object [PermissionSet]
        [StructLayout(LayoutKind.Sequential)]
        internal class PermissionSetEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Id;
        [MarshalAs(UnmanagedType.LPWStr)] public string XmlSegment;
        };
    
        internal enum PermissionSetEntryFieldId
        {
            PermissionSet_XmlSegment,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EBE5A1ED-FEBC-42c4-A9E1-E087C6E36635")]
        internal interface IPermissionSetEntry
        {
            PermissionSetEntry AllData { [SecurityCritical]get; }

            string Id { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string XmlSegment { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [PermissionSet]
        //++! start object [AssemblyRequest]
        [StructLayout(LayoutKind.Sequential)]
        internal class AssemblyRequestEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string permissionSetID;
        };
    
        internal enum AssemblyRequestEntryFieldId
        {
            AssemblyRequest_permissionSetID,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2474ECB4-8EFD-4410-9F31-B3E7C4A07731")]
        internal interface IAssemblyRequestEntry
        {
            AssemblyRequestEntry AllData { [SecurityCritical]get; }

            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string permissionSetID { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [AssemblyRequest]

        //++! start object [DescriptionMetadata]
        [StructLayout(LayoutKind.Sequential)]
        internal class DescriptionMetadataEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Publisher;
            [MarshalAs(UnmanagedType.LPWStr)] public string Product;
            [MarshalAs(UnmanagedType.LPWStr)] public string SupportUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string IconFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string ErrorReportUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string SuiteName;

        };
    
        internal enum DescriptionMetadataEntryFieldId
        {
            DescriptionMetadata_Publisher,
            DescriptionMetadata_Product,
            DescriptionMetadata_SupportUrl,
            DescriptionMetadata_IconFile,
            DescriptionMetadata_ErrorReportUrl,
            DescriptionMetadata_SuiteName,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CB73147E-5FC2-4c31-B4E6-58D13DBE1A08")]
        internal interface IDescriptionMetadataEntry
        {
            DescriptionMetadataEntry AllData { [SecurityCritical]get; }

            string Publisher { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Product { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string SupportUrl { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string IconFile { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string ErrorReportUrl { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string SuiteName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [DescriptionMetadata]
        //++! start object [DeploymentMetadata]
        [StructLayout(LayoutKind.Sequential)]
        internal class DeploymentMetadataEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string DeploymentProviderCodebase;
            [MarshalAs(UnmanagedType.LPWStr)] public string MinimumRequiredVersion;
            public ushort MaximumAge;
            public byte MaximumAge_Unit;
            public uint DeploymentFlags;
        };
    
        internal enum DeploymentMetadataEntryFieldId
        {
            DeploymentMetadata_DeploymentProviderCodebase,
            DeploymentMetadata_MinimumRequiredVersion,
            DeploymentMetadata_MaximumAge,
            DeploymentMetadata_MaximumAge_Unit,
            DeploymentMetadata_DeploymentFlags,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CFA3F59F-334D-46bf-A5A5-5D11BB2D7EBC")]
        internal interface IDeploymentMetadataEntry
        {
            DeploymentMetadataEntry AllData { [SecurityCritical]get; }

            string DeploymentProviderCodebase { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string MinimumRequiredVersion { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ushort MaximumAge { [SecurityCritical]get; }
            byte MaximumAge_Unit { [SecurityCritical]get; }
            uint DeploymentFlags { [SecurityCritical]get; }
        };
    
        //++! end object [DeploymentMetadata]
        //++! start object [DependentOSMetadata]
        [StructLayout(LayoutKind.Sequential)]
        internal class DependentOSMetadataEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string SupportUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public ushort BuildNumber;
            public byte ServicePackMajor;
            public byte ServicePackMinor;
        };
    
        internal enum DependentOSMetadataEntryFieldId
        {
            DependentOSMetadata_SupportUrl,
            DependentOSMetadata_Description,
            DependentOSMetadata_MajorVersion,
            DependentOSMetadata_MinorVersion,
            DependentOSMetadata_BuildNumber,
            DependentOSMetadata_ServicePackMajor,
            DependentOSMetadata_ServicePackMinor,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CF168CF4-4E8F-4d92-9D2A-60E5CA21CF85")]
        internal interface IDependentOSMetadataEntry
        {
            DependentOSMetadataEntry AllData { [SecurityCritical]get; }

            string SupportUrl { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ushort MajorVersion { [SecurityCritical]get; }
            ushort MinorVersion { [SecurityCritical]get; }
            ushort BuildNumber { [SecurityCritical]get; }
            byte ServicePackMajor { [SecurityCritical]get; }
            byte ServicePackMinor { [SecurityCritical]get; }
        };
    
        //++! end object [DependentOSMetadata]
        //++! start object [CompatibleFrameworksMetadata]
        [StructLayout(LayoutKind.Sequential)]
        internal class CompatibleFrameworksMetadataEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string SupportUrl;
        };

        internal enum CompatibleFrameworksMetadataEntryFieldId
        {
            CompatibleFrameworksMetadata_SupportUrl,
        };

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("4A33D662-2210-463A-BE9F-FBDF1AA554E3")]
        internal interface ICompatibleFrameworksMetadataEntry
        {
            CompatibleFrameworksMetadataEntry AllData { [SecurityCritical]get; }

            string SupportUrl { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };

        //++! end object [CompatibleFrameworksMetadata]
        //++! start object [MetadataSection]
        [StructLayout(LayoutKind.Sequential)]
        internal class MetadataSectionEntry : IDisposable
        {
            public uint SchemaVersion;
            public uint ManifestFlags;
            public uint UsagePatterns;
            public IDefinitionIdentity CdfIdentity;
            [MarshalAs(UnmanagedType.LPWStr)] public string LocalPath;
            public uint HashAlgorithm;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr ManifestHash;
            public uint ManifestHashSize;
            [MarshalAs(UnmanagedType.LPWStr)] public string ContentType;
            [MarshalAs(UnmanagedType.LPWStr)] public string RuntimeImageVersion;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr MvidValue;
            public uint MvidValueSize;
            public DescriptionMetadataEntry DescriptionData;
            public DeploymentMetadataEntry DeploymentData;
            public DependentOSMetadataEntry DependentOSData;
            [MarshalAs(UnmanagedType.LPWStr)] public string defaultPermissionSetID;
            [MarshalAs(UnmanagedType.LPWStr)] public string RequestedExecutionLevel;
            public bool RequestedExecutionLevelUIAccess;
            public IReferenceIdentity ResourceTypeResourcesDependency;
            public IReferenceIdentity ResourceTypeManifestResourcesDependency;
            [MarshalAs(UnmanagedType.LPWStr)] public string KeyInfoElement;
            public CompatibleFrameworksMetadataEntry CompatibleFrameworksData;
            ~MetadataSectionEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }
    
            [System.Security.SecuritySafeCritical]  // auto-generated
            public void Dispose(bool fDisposing)
            {
                if (ManifestHash != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ManifestHash);
                    ManifestHash = IntPtr.Zero;
                }
                if (MvidValue != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(MvidValue);
                    MvidValue = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum MetadataSectionEntryFieldId
        {
            MetadataSection_SchemaVersion,
            MetadataSection_ManifestFlags,
            MetadataSection_UsagePatterns,
            MetadataSection_CdfIdentity,
            MetadataSection_LocalPath,
            MetadataSection_HashAlgorithm,
            MetadataSection_ManifestHash,
            MetadataSection_ManifestHashSize,
            MetadataSection_ContentType,
            MetadataSection_RuntimeImageVersion,
            MetadataSection_MvidValue,
            MetadataSection_MvidValueSize,
            MetadataSection_DescriptionData,
            MetadataSection_DeploymentData,
            MetadataSection_DependentOSData,
            MetadataSection_defaultPermissionSetID,
            MetadataSection_RequestedExecutionLevel,
            MetadataSection_RequestedExecutionLevelUIAccess,
            MetadataSection_ResourceTypeResourcesDependency,
            MetadataSection_ResourceTypeManifestResourcesDependency,
            MetadataSection_KeyInfoElement,
            MetadataSection_CompatibleFrameworksData,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("AB1ED79F-943E-407d-A80B-0744E3A95B28")]
        internal interface IMetadataSectionEntry
        {
            MetadataSectionEntry AllData { [SecurityCritical]get; }

            uint SchemaVersion { [SecurityCritical]get; }
            uint ManifestFlags { [SecurityCritical]get; }
            uint UsagePatterns { [SecurityCritical]get; }
            IDefinitionIdentity CdfIdentity { [SecurityCritical]get; }
            string LocalPath { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint HashAlgorithm { [SecurityCritical]get; }
            object ManifestHash { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            string ContentType { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string RuntimeImageVersion { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            object MvidValue { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            IDescriptionMetadataEntry DescriptionData { [SecurityCritical]get; }
            IDeploymentMetadataEntry DeploymentData { [SecurityCritical]get; }
            IDependentOSMetadataEntry DependentOSData { [SecurityCritical]get; }
            string defaultPermissionSetID { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string RequestedExecutionLevel { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            bool RequestedExecutionLevelUIAccess { [SecurityCritical]get; }
            IReferenceIdentity ResourceTypeResourcesDependency { [SecurityCritical]get; }
            IReferenceIdentity ResourceTypeManifestResourcesDependency { [SecurityCritical]get; }
            string KeyInfoElement { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            ICompatibleFrameworksMetadataEntry CompatibleFrameworksData { [SecurityCritical]get; }
        };
    
        //++! end object [MetadataSection]

        //++! start object [Event]
                       #if !ISOLATION_IN_MSCORLIB
        [StructLayout(LayoutKind.Sequential)]
        internal class EventEntry
        {
            public uint EventID;
            public uint Level;
            public uint Version;
            public Guid Guid;
            [MarshalAs(UnmanagedType.LPWStr)] public string SubTypeName;
            public uint SubTypeValue;
            [MarshalAs(UnmanagedType.LPWStr)] public string DisplayName;
            public uint EventNameMicrodomIndex;
        };
    
        internal enum EventEntryFieldId
        {
            Event_Level,
            Event_Version,
            Event_Guid,
            Event_SubTypeName,
            Event_SubTypeValue,
            Event_DisplayName,
            Event_EventNameMicrodomIndex,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BB")]
        internal interface IEventEntry
        {
            EventEntry AllData { [SecurityCritical]get; }

            uint EventID { [SecurityCritical]get; }
            uint Level { [SecurityCritical]get; }
            uint Version { [SecurityCritical]get; }
            Guid Guid { [SecurityCritical]get; }
            string SubTypeName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint SubTypeValue { [SecurityCritical]get; }
            string DisplayName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint EventNameMicrodomIndex { [SecurityCritical]get; }
        };
    
        //++! end object [Event]
        //++! start object [EventMap]
        [StructLayout(LayoutKind.Sequential)]
        internal class EventMapEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string MapName;
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            public uint Value;
            public bool IsValueMap;
        };
    
        internal enum EventMapEntryFieldId
        {
            EventMap_Name,
            EventMap_Value,
            EventMap_IsValueMap,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BC")]
        internal interface IEventMapEntry
        {
            EventMapEntry AllData { [SecurityCritical]get; }

            string MapName { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint Value { [SecurityCritical]get; }
            bool IsValueMap { [SecurityCritical]get; }
        };
    
        //++! end object [EventMap]
        //++! start object [EventTag]
        [StructLayout(LayoutKind.Sequential)]
        internal class EventTagEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string TagData;
            public uint EventID;
        };
    
        internal enum EventTagEntryFieldId
        {
            EventTag_EventID,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BD")]
        internal interface IEventTagEntry
        {
            EventTagEntry AllData { [SecurityCritical]get; }

            string TagData { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint EventID { [SecurityCritical]get; }
        };
    
        //++! end object [EventTag]
        //++! start object [RegistryValue]
        [StructLayout(LayoutKind.Sequential)]
        internal class RegistryValueEntry
        {
            public uint Flags;
            public uint OperationHint;
            public uint Type;
            [MarshalAs(UnmanagedType.LPWStr)] public string Value;
            [MarshalAs(UnmanagedType.LPWStr)] public string BuildFilter;
        };
    
        internal enum RegistryValueEntryFieldId
        {
            RegistryValue_Flags,
            RegistryValue_OperationHint,
            RegistryValue_Type,
            RegistryValue_Value,
            RegistryValue_BuildFilter,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("49e1fe8d-ebb8-4593-8c4e-3e14c845b142")]
        internal interface IRegistryValueEntry
        {
            RegistryValueEntry AllData { [SecurityCritical]get; }

            uint Flags { [SecurityCritical]get; }
            uint OperationHint { [SecurityCritical]get; }
            uint Type { [SecurityCritical]get; }
            string Value { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string BuildFilter { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [RegistryValue]
        //++! start object [RegistryKey]
        [StructLayout(LayoutKind.Sequential)]
        internal class RegistryKeyEntry : IDisposable
        {
            public uint Flags;
            public uint Protection;
            [MarshalAs(UnmanagedType.LPWStr)] public string BuildFilter;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr SecurityDescriptor;
            public uint SecurityDescriptorSize;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr Values;
            public uint ValuesSize;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr Keys;
            public uint KeysSize;
            ~RegistryKeyEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }

            [SecuritySafeCritical]
            public void Dispose(bool fDisposing)
            {
                if (SecurityDescriptor != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(SecurityDescriptor);
                    SecurityDescriptor = IntPtr.Zero;
                }
                if (Values != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(Values);
                    Values = IntPtr.Zero;
                }
                if (Keys != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(Keys);
                    Keys = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum RegistryKeyEntryFieldId
        {
            RegistryKey_Flags,
            RegistryKey_Protection,
            RegistryKey_BuildFilter,
            RegistryKey_SecurityDescriptor,
            RegistryKey_SecurityDescriptorSize,
            RegistryKey_Values,
            RegistryKey_ValuesSize,
            RegistryKey_Keys,
            RegistryKey_KeysSize,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("186685d1-6673-48c3-bc83-95859bb591df")]
        internal interface IRegistryKeyEntry
        {
            RegistryKeyEntry AllData { [SecurityCritical]get; }

            uint Flags { [SecurityCritical]get; }
            uint Protection { [SecurityCritical]get; }
            string BuildFilter { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            object SecurityDescriptor { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            object Values { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
            object Keys { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
        };
    
        //++! end object [RegistryKey]
        //++! start object [Directory]
        [StructLayout(LayoutKind.Sequential)]
        internal class DirectoryEntry : IDisposable
        {
            public uint Flags;
            public uint Protection;
            [MarshalAs(UnmanagedType.LPWStr)] public string BuildFilter;
            [MarshalAs(UnmanagedType.SysInt)] public IntPtr SecurityDescriptor;
            public uint SecurityDescriptorSize;
            ~DirectoryEntry()
            {
                Dispose(false);
            }
    
            void IDisposable.Dispose() { this.Dispose(true); }

            [SecuritySafeCritical]
            public void Dispose(bool fDisposing)
            {
                if (SecurityDescriptor != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(SecurityDescriptor);
                    SecurityDescriptor = IntPtr.Zero;
                }
    
                if (fDisposing)
                    System.GC.SuppressFinalize(this);
            }
        };
    
        internal enum DirectoryEntryFieldId
        {
            Directory_Flags,
            Directory_Protection,
            Directory_BuildFilter,
            Directory_SecurityDescriptor,
            Directory_SecurityDescriptorSize,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9f27c750-7dfb-46a1-a673-52e53e2337a9")]
        internal interface IDirectoryEntry
        {
            DirectoryEntry AllData { [SecurityCritical]get; }

            uint Flags { [SecurityCritical]get; }
            uint Protection { [SecurityCritical]get; }
            string BuildFilter { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            object SecurityDescriptor { [SecurityCritical][return: MarshalAs(UnmanagedType.Interface)] get; }
        };
    
        //++! end object [Directory]
        //++! start object [SecurityDescriptorReference]
        [StructLayout(LayoutKind.Sequential)]
        internal class SecurityDescriptorReferenceEntry
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string BuildFilter;
        };
    
        internal enum SecurityDescriptorReferenceEntryFieldId
        {
            SecurityDescriptorReference_Name,
            SecurityDescriptorReference_BuildFilter,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a75b74e9-2c00-4ebb-b3f9-62a670aaa07e")]
        internal interface ISecurityDescriptorReferenceEntry
        {
            SecurityDescriptorReferenceEntry AllData { [SecurityCritical]get; }

            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string BuildFilter { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };
    
        //++! end object [SecurityDescriptorReference]
        //++! start object [CounterSet]
        [StructLayout(LayoutKind.Sequential)]
        internal class CounterSetEntry
        {
            public Guid CounterSetGuid;
            public Guid ProviderGuid;
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
            public bool InstanceType;
        };
    
        internal enum CounterSetEntryFieldId
        {
            CounterSet_ProviderGuid,
            CounterSet_Name,
            CounterSet_Description,
            CounterSet_InstanceType,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8CD3FC85-AFD3-477a-8FD5-146C291195BB")]
        internal interface ICounterSetEntry
        {
            CounterSetEntry AllData { [SecurityCritical]get; }

            Guid CounterSetGuid { [SecurityCritical]get; }
            Guid ProviderGuid { [SecurityCritical]get; }
            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            bool InstanceType { [SecurityCritical]get; }
        };
    
        //++! end object [CounterSet]
        //++! start object [Counter]
        [StructLayout(LayoutKind.Sequential)]
        internal class CounterEntry
        {
            public Guid CounterSetGuid;
            public uint CounterId;
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
            public uint CounterType;
            public ulong Attributes;
            public uint BaseId;
            public uint DefaultScale;
        };
    
        internal enum CounterEntryFieldId
        {
            Counter_CounterId,
            Counter_Name,
            Counter_Description,
            Counter_CounterType,
            Counter_Attributes,
            Counter_BaseId,
            Counter_DefaultScale,
        };
    
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8CD3FC86-AFD3-477a-8FD5-146C291195BB")]
        internal interface ICounterEntry
        {
            CounterEntry AllData { [SecurityCritical]get; }

            Guid CounterSetGuid { [SecurityCritical]get; }
            uint CounterId { [SecurityCritical]get; }
            string Name { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Description { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            uint CounterType { [SecurityCritical]get; }
            ulong Attributes { [SecurityCritical]get; }
            uint BaseId { [SecurityCritical]get; }
            uint DefaultScale { [SecurityCritical] get; }
        };
    
        //++! end object [Counter]
        //++! start object [CompatibleFramework]
        [StructLayout(LayoutKind.Sequential)]
        internal class CompatibleFrameworkEntry
        {
            public uint index;
            [MarshalAs(UnmanagedType.LPWStr)] public string TargetVersion;
            [MarshalAs(UnmanagedType.LPWStr)] public string Profile;
            [MarshalAs(UnmanagedType.LPWStr)] public string SupportedRuntime;
        };

        internal enum CompatibleFrameworkEntryFieldId
        {
            CompatibleFramework_TargetVersion,
            CompatibleFramework_Profile,
            CompatibleFramework_SupportedRuntime,
        };

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C98BFE2A-62C9-40AD-ADCE-A9037BE2BE6C")]
        internal interface ICompatibleFrameworkEntry
        {
            CompatibleFrameworkEntry AllData { [SecurityCritical]get; }

            uint index{ [SecurityCritical]get; }
            string TargetVersion { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
            string Profile { [SecurityCritical][return:MarshalAs(UnmanagedType.LPWStr)] get; }
            string SupportedRuntime { [SecurityCritical][return: MarshalAs(UnmanagedType.LPWStr)] get; }
        };

        //++! end object [CompatibleFramework]

                    #endif
    
    }

