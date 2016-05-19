//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using COMTypes = System.Runtime.InteropServices.ComTypes;

namespace System.ServiceModel.Activation
{
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct METADATA_RECORD
    {
        public uint dwMDIdentifier;
        public uint dwMDAttributes;
        public uint dwMDUserType;
        public uint dwMDDataType;
        public uint dwMDDataLen;
        [Fx.Tag.SecurityNote(Critical = "Stores a handle.")]
        [SecurityCritical]
        public IntPtr pbMDData;
        public uint dwMDDataTag;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct METADATA_HANDLE_INFO
    {
        public uint dwMDPermissions;
        public uint dwMDSystemChangeNumber;
    };

    [ComImport,
    Guid("A9E69610-B80D-11D0-B9B9-00A0C922E750")]
    [Fx.Tag.SecurityNote(Critical = "Implements a SecurityCritical interface.")]
    [SecurityCritical]
    class MSAdminBase
    {
        // These values are defined in %SDXROOT%\public\sdk\inc\iiscnfg.h.
        // The interfaces are defined in %SDXROOT%\public\sdk\inc\iadmw.h.
        internal const uint DEFAULT_METABASE_TIMEOUT = 30000;
        internal const int METADATA_MASTER_ROOT_HANDLE = 0;
        internal const int METADATA_PERMISSION_READ = 0x00000001;
        internal const int METADATA_INHERIT = 0x00000001;
        internal const int IIS_MD_UT_SERVER = 1;
        internal const int ALL_METADATA = 0;
        internal const int DWORD_METADATA = 1;
        internal const int STRING_METADATA = 2;
        internal const int BINARY_METADATA = 3;
        internal const int EXPANDSZ_METADATA = 4;
        internal const int MULTISZ_METADATA = 5;
    }

    [ComImport,
    Guid("70B51430-B6CA-11d0-B9B9-00A0C922E750"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    interface IMSAdminBase
    {
        //    virtual HRESULT STDMETHODCALLTYPE AddKey(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath) = 0;
        [PreserveSig]
        uint AddKey(
            uint hMDHandle,
            string pszMDPath);

        //    virtual HRESULT STDMETHODCALLTYPE DeleteKey(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath) = 0;
        [PreserveSig]
        uint DeleteKey(
            uint hMDHandle,
            string pszMDPath);

        //    virtual HRESULT STDMETHODCALLTYPE DeleteChildKeys(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath) = 0;
        [PreserveSig]
        uint DeleteChildKeys(
            uint hMDHandle,
            string pszMDPath);

        //    virtual HRESULT STDMETHODCALLTYPE EnumKeys(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [size_is][out] */ LPWSTR pszMDName,
        //        /* [in] */ DWORD dwMDEnumObjectIndex) = 0;
        [PreserveSig]
        uint EnumKeys(
            uint hMDHandle, 
            string pszMDPath,
            string pszMDName,
            uint dwMDEnumObjectIndex);

        //    virtual HRESULT STDMETHODCALLTYPE CopyKey(
        //        /* [in] */ METADATA_HANDLE hMDSourceHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDSourcePath,
        //        /* [in] */ METADATA_HANDLE hMDDestHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDDestPath,
        //        /* [in] */ BOOL bMDOverwriteFlag,
        //        /* [in] */ BOOL bMDCopyFlag) = 0;
        [PreserveSig]
        uint CopyKey(
            uint hMDSourceHandle,
            string pszMDSourcePath, 
            uint hMDDestHandle,
            string pszMDDestPath,
            int bMDOverwriteFlag,
            int bMDCopyFlag);

        //    virtual HRESULT STDMETHODCALLTYPE RenameKey(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [string][in][unique] */ LPCWSTR pszMDNewName) = 0;
        [PreserveSig]
        uint RenameKey(
            uint hMDHandle,
            string pszMDPath,
            string pszMDNewName);

        //    virtual /* [local] */ HRESULT STDMETHODCALLTYPE SetData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ PMETADATA_RECORD pmdrMDData) = 0;
        [Fx.Tag.SecurityNote(Critical = "Takes a SecurityCritical parameter.")]
        [SecurityCritical]
        [PreserveSig]
        uint SetData(
            uint hMDHandle,
            string pszMDPath, 
            METADATA_RECORD pmdrMDData);

        //    virtual /* [local] */ HRESULT STDMETHODCALLTYPE GetData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [out][in] */ PMETADATA_RECORD pmdrMDData,
        //        /* [out] */ DWORD *pdwMDRequiredDataLen) = 0;
        [Fx.Tag.SecurityNote(Critical = "Takes and Returns a SecurityCritical parameter.")]
        [SecurityCritical]
        [PreserveSig]
        uint GetData(uint hMDHandle,
              [MarshalAs(UnmanagedType.LPWStr)] string pszMDPath,
              ref METADATA_RECORD pmdrMDData,
              ref uint pdwMDRequiredDataLen);

        //    virtual HRESULT STDMETHODCALLTYPE DeleteData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ DWORD dwMDIdentifier,
        //        /* [in] */ DWORD dwMDDataType) = 0;
        [PreserveSig]
        uint DeleteData(
            uint hMDHandle,
            string pszMDPath,
            uint dwMDIdentifier,
            uint dwMDDataType
            );

        //    virtual /* [local] */ HRESULT STDMETHODCALLTYPE EnumData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [out][in] */ PMETADATA_RECORD pmdrMDData,
        //        /* [in] */ DWORD dwMDEnumDataIndex,
        //        /* [out] */ DWORD *pdwMDRequiredDataLen) = 0;
        [Fx.Tag.SecurityNote(Critical = "Takes a SecurityCritical parameter.")]
        [SecurityCritical]
        [PreserveSig]
        uint EnumData(
            uint hMDHandle,
            string pszMDPath,
            METADATA_RECORD pmdrMDData,
            uint dwMDEnumDataIndex,
            ref uint pdwMDRequiredDataLen);

        //    virtual /* [local] */ HRESULT STDMETHODCALLTYPE GetAllData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ DWORD dwMDAttributes,
        //        /* [in] */ DWORD dwMDUserType,
        //        /* [in] */ DWORD dwMDDataType,
        //        /* [out] */ DWORD *pdwMDNumDataEntries,
        //        /* [out] */ DWORD *pdwMDDataSetNumber,
        //        /* [in] */ DWORD dwMDBufferSize,
        //        /* [size_is][out] */ unsigned char *pbMDBuffer,
        //        /* [out] */ DWORD *pdwMDRequiredBufferSize) = 0;
        [PreserveSig]
        uint GetAllData(
            uint hMDHandle,
            string pszMDPath,
            uint dwMDAttributes,
            uint dwMDUserType,
            uint dwMDDataType,
            ref uint pdwMDNumDataEntries,
            ref uint pdwMDDataSetNumber,
            uint dwMDBufferSize,
            ref uint pdwMDRequiredBufferSize,
            IntPtr ppDataBlob);

        //    virtual HRESULT STDMETHODCALLTYPE DeleteAllData(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ DWORD dwMDUserType,
        //        /* [in] */ DWORD dwMDDataType) = 0;
        [PreserveSig]
        uint DeleteAllData(
            uint hMDHandle,
            string pszMDPath,
            uint dwMDUserType,
            uint dwMDDataType);

        //    virtual HRESULT STDMETHODCALLTYPE CopyData(
        //        /* [in] */ METADATA_HANDLE hMDSourceHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDSourcePath,
        //        /* [in] */ METADATA_HANDLE hMDDestHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDDestPath,
        //        /* [in] */ DWORD dwMDAttributes,
        //        /* [in] */ DWORD dwMDUserType,
        //        /* [in] */ DWORD dwMDDataType,
        //        /* [in] */ BOOL bMDCopyFlag) = 0;
        [PreserveSig]
        uint CopyData(
            uint hMDSourceHandle,
            string pszMDSourcePath,
            uint hMDDestHandle,
            string pszMDDestPath,
            uint dwMDAttributes,
            uint dwMDUserType,
            uint dwMDDataType,
            int bMDCopyFlag);

        //    virtual HRESULT STDMETHODCALLTYPE GetDataPaths(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ DWORD dwMDIdentifier,
        //        /* [in] */ DWORD dwMDDataType,
        //        /* [in] */ DWORD dwMDBufferSize,
        //        /* [size_is][out] */ WCHAR *pszBuffer,
        //        /* [out] */ DWORD *pdwMDRequiredBufferSize) = 0;
        [PreserveSig]
        uint GetDataPaths(
            uint hMDHandle,
            string pszMDPath,
            uint dwMDIdentifier,
            uint dwMDDataType,
            uint dwMDBufferSize,
            IntPtr pszBuffer,
            ref uint pdwMDRequiredBufferSize);

        //    virtual HRESULT STDMETHODCALLTYPE OpenKey(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ DWORD dwMDAccessRequested,
        //        /* [in] */ DWORD dwMDTimeOut,
        //        /* [out] */ PMETADATA_HANDLE phMDNewHandle) = 0;
        [PreserveSig]
        uint OpenKey(
            uint hMDHandle,
            string pszMDPath,
            uint dwMDAccessRequested,
            uint dwMDTimeOut,
            out uint phMDNewHandle);

        //    virtual HRESULT STDMETHODCALLTYPE CloseKey(
        //        /* [in] */ METADATA_HANDLE hMDHandle) = 0;
        [PreserveSig]
        uint CloseKey(
            uint hMDHandle);

        //    virtual HRESULT STDMETHODCALLTYPE ChangePermissions(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [in] */ DWORD dwMDTimeOut,
        //        /* [in] */ DWORD dwMDAccessRequested) = 0;
        [PreserveSig]
        uint ChangePermissions(
            uint hMDHandle,
            uint dwMDTimeOut,
            uint dwMDAccessRequested);

        //    virtual HRESULT STDMETHODCALLTYPE SaveData( void) = 0;
        [PreserveSig]
        uint SaveData();

        //    virtual HRESULT STDMETHODCALLTYPE GetHandleInfo(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [out] */ PMETADATA_HANDLE_INFO pmdhiInfo) = 0;
        [PreserveSig]
        uint GetHandleInfo(
            uint hMDHandle,
            METADATA_HANDLE_INFO pmdhiInfo);

        //    virtual HRESULT STDMETHODCALLTYPE GetSystemChangeNumber(
        //        /* [out] */ DWORD *pdwSystemChangeNumber) = 0;
        [PreserveSig]
        uint GetSystemChangeNumber(
            ref uint pdwSystemChangeNumber);

        //    virtual HRESULT STDMETHODCALLTYPE GetDataSetNumber(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [out] */ DWORD *pdwMDDataSetNumber) = 0;
        [PreserveSig]
        uint GetDataSetNumber(
            uint hMDHandle,
            string pszMDPath,
            ref uint pdwMDDataSetNumber);

        //    virtual HRESULT STDMETHODCALLTYPE SetLastChangeTime(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [in] */ PFILETIME pftMDLastChangeTime,
        //        /* [in] */ BOOL bLocalTime) = 0;
        [PreserveSig]
        uint SetLastChangeTime(
            uint hMDHandle,
            string pszMDPath,
            ref COMTypes.FILETIME pftMDLastChangeTime,
            int bLocalTime);

        //    virtual HRESULT STDMETHODCALLTYPE GetLastChangeTime(
        //        /* [in] */ METADATA_HANDLE hMDHandle,
        //        /* [string][in][unique] */ LPCWSTR pszMDPath,
        //        /* [out] */ PFILETIME pftMDLastChangeTime,
        //        /* [in] */ BOOL bLocalTime) = 0;
        [PreserveSig]
        uint GetLastChangeTime(
            uint hMDHandle,
            string pszMDPath,
            ref COMTypes.FILETIME pftMDLastChangeTime,
            int bLocalTime);

        //    virtual /* [restricted][local] */ HRESULT STDMETHODCALLTYPE KeyExchangePhase1( void) = 0;
        [PreserveSig]
        uint KeyExchangePhase1();

        //    virtual /* [restricted][local] */ HRESULT STDMETHODCALLTYPE KeyExchangePhase2( void) = 0;
        [PreserveSig]
        uint KeyExchangePhase2();

        //    virtual HRESULT STDMETHODCALLTYPE Backup(
        //        /* [string][in][unique] */ LPCWSTR pszMDBackupLocation,
        //        /* [in] */ DWORD dwMDVersion,
        //        /* [in] */ DWORD dwMDFlags) = 0;
        [PreserveSig]
        uint Backup(
            string pszMDBackupLocation,
            uint dwMDVersion,
            uint dwMDFlags);

        //    virtual HRESULT STDMETHODCALLTYPE Restore(
        //        /* [string][in][unique] */ LPCWSTR pszMDBackupLocation,
        //        /* [in] */ DWORD dwMDVersion,
        //        /* [in] */ DWORD dwMDFlags) = 0;
        [PreserveSig]
        uint Restore(
            string pszMDBackupLocation,
            uint dwMDVersion,
            uint dwMDFlags);

        //    virtual HRESULT STDMETHODCALLTYPE EnumBackups(
        //        /* [size_is][out][in] */ LPWSTR pszMDBackupLocation,
        //        /* [out] */ DWORD *pdwMDVersion,
        //        /* [out] */ PFILETIME pftMDBackupTime,
        //        /* [in] */ DWORD dwMDEnumIndex) = 0;
        [PreserveSig]
        uint EnumBackups(
            string pszMDBackupLocation,
            ref uint pdwMDVersion,
            ref COMTypes.FILETIME pftMDBackupTime,
            uint dwMDEnumIndex);

        //    virtual HRESULT STDMETHODCALLTYPE DeleteBackup(
        //        /* [string][in][unique] */ LPCWSTR pszMDBackupLocation,
        //        /* [in] */ DWORD dwMDVersion) = 0;
        [PreserveSig]
        uint DeleteBackup(
            string pszMDBackupLocation,
            uint dwMDVersion);

        //    virtual HRESULT STDMETHODCALLTYPE UnmarshalInterface(
        //        /* [out] */ IMSAdminBaseW **piadmbwInterface) = 0;
        [PreserveSig]
        uint UnmarshalInterface(
            ref IMSAdminBase piadmbwInterface);

        //    virtual /* [restricted][local] */ HRESULT STDMETHODCALLTYPE GetServerGuid( void) = 0;
        [PreserveSig]
        uint GetServerGuid(
            ref Guid pServerGuid);
    }
}
