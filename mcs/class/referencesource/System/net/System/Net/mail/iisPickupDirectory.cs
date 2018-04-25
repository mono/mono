using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Security.Authentication;

namespace System.Net.Mail
{
    internal enum PropertyName
    {
        Invalid         = 0,
        ServerState     = 1016,
        PickupDirectory = 36880
    };

    internal enum ServerState
    {
        Starting = 1,
        Started = 2,
        Stopping = 3,
        Stopped = 4,
        Pausing = 5,
        Paused = 6,
        Continuing = 7,
    }
    
    internal enum MBErrors
    {
        DataNotFound            = unchecked( (int)0x800CC801 ),     // MD_ERROR_DATA_NOT_FOUND
        InvalidVersion          = unchecked( (int)0x800CC802 ),     // MD_ERROR_INVALID_VERSION
        DuplicateNameWarning    = unchecked( (int)0x000CC804 ),     // MD_WARNING_DUP_NAME
        InvalidDataWarning      = unchecked( (int)0x000CC805 ),     // MD_WARNING_INVALID_DATA
        AlreadyExists           = unchecked( (int)0x800700B7 ),     // RETURNCODETOHRESULT( ERROR_ALREADY_EXISTS )
        InvalidParameter        = unchecked( (int)0x80070057 ),     // E_INVALIDARG
        PathNotFound            = unchecked( (int)0x80070003 ),     // RETURNCODETOHRESULT( ERROR_PATH_NOT_FOUND )
        PathBusy                = unchecked( (int)0x80070094 ),     // RETURNCODETOHRESULT( ERROR_PATH_BUSY )
        InsufficientBuffer      = unchecked( (int)0x8007007A ),     // RETURNCODETOHRESULT( ERROR_INSUFFICIENT_BUFFER )
        NoMoreItems             = unchecked( (int)0x80070103 ),     // RETURNCODETOHRESULT( ERROR_NO_MORE_ITEMS )
        AccessDenied            = unchecked( (int)0x80070005 ),     // RETURNCODETOHRESULT( E_ACCCESS_DENIED )
    };
    
    [Flags]
    internal enum MBKeyAccess : uint
    {
        Read = 1,
        Write = 2
    };

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal unsafe struct MetadataRecord
    {
        internal UInt32 Identifier;
        internal UInt32 Attributes;
        internal UInt32 UserType;
        internal UInt32 DataType;
        internal UInt32 DataLen;
        internal IntPtr DataBuf;
        internal UInt32 DataTag;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal class _METADATA_HANDLE_INFO
    {
        _METADATA_HANDLE_INFO()
        {
            dwMDPermissions = 0;
            dwMDSystemChangeNumber = 0;
        }
        internal Int32 dwMDPermissions;
        internal Int32 dwMDSystemChangeNumber;
    };

    #region IMSadminBase itf definitions
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport, Guid("70b51430-b6ca-11d0-b9b9-00a0c922e750")]
    internal interface IMSAdminBase
    {
        [PreserveSig]
        int AddKey(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path
            );

        [PreserveSig]
        int DeleteKey(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path
            );

        void DeleteChildKeys(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path
            );

        [PreserveSig]
        int EnumKeys(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            StringBuilder Buffer,
            int EnumKeyIndex
            );

        void CopyKey(
            IntPtr source,
            [MarshalAs(UnmanagedType.LPWStr)] string SourcePath,
            IntPtr dest,
            [MarshalAs(UnmanagedType.LPWStr)] string DestPath,
            bool OverwriteFlag,
            bool CopyFlag
            );

        void RenameKey(
            IntPtr key,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            [MarshalAs(UnmanagedType.LPWStr)] string newName
            );

        [PreserveSig]
        int SetData(
            IntPtr key,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            ref MetadataRecord data
            );

        [PreserveSig]
        int GetData(
            IntPtr key,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            ref MetadataRecord data,
            [In, Out] ref uint RequiredDataLen
            );

        [PreserveSig]
        int DeleteData(
            IntPtr key,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            uint Identifier,
            uint DataType
            );

        [PreserveSig]
        int EnumData(
            IntPtr key,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            ref MetadataRecord data,
            int EnumDataIndex,
            [In, Out] ref uint RequiredDataLen
            );

        [PreserveSig] 
        int GetAllData(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            UInt32 Attributes,
            UInt32 UserType,
            UInt32 DataType,
            [In, Out] ref UInt32 NumDataEntries,
            [In, Out] ref UInt32 DataSetNumber,
            UInt32 BufferSize,
            //          [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=7)] out byte[] Buffer,
            IntPtr buffer,
            [In,Out] ref UInt32 RequiredBufferSize
            );

        void DeleteAllData(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            uint UserType,
            uint DataType
            );

        [PreserveSig] 
        int CopyData(
            IntPtr sourcehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string SourcePath,
            IntPtr desthandle,
            [MarshalAs(UnmanagedType.LPWStr)] string DestPath,
            int Attributes,
            int UserType,
            int DataType,
            [MarshalAs(UnmanagedType.Bool)] bool CopyFlag
            );

        [PreserveSig] 
        void GetDataPaths(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            int Identifier,
            int DataType,
            int BufferSize,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex=4)] out char[] Buffer,
            [In, Out, MarshalAs(UnmanagedType.U4)] ref int RequiredBufferSize
            );

        [PreserveSig]
        int OpenKey(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            [MarshalAs(UnmanagedType.U4)] MBKeyAccess AccessRequested,
            int TimeOut,
            [In, Out] ref IntPtr NewHandle
            );

        [PreserveSig]
        int CloseKey(
            IntPtr handle
            );

        void ChangePermissions(
            IntPtr handle,
            int TimeOut,
            [MarshalAs(UnmanagedType.U4)] MBKeyAccess AccessRequested
            );

        void SaveData(
            );

        [PreserveSig] 
        void GetHandleInfo(
            IntPtr handle,
            [In, Out] ref _METADATA_HANDLE_INFO Info
            );

        [PreserveSig] 
        void GetSystemChangeNumber(
            [In, Out, MarshalAs(UnmanagedType.U4)] ref uint SystemChangeNumber
            );

        [PreserveSig] 
        void GetDataSetNumber(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            [In, Out] ref uint DataSetNumber
            );
        
        [PreserveSig] 
        void SetLastChangeTime(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            [Out] out System.Runtime.InteropServices.ComTypes.FILETIME LastChangeTime,
            bool LocalTime
            );

        [PreserveSig] 
        int GetLastChangeTime(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            [In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME LastChangeTime,
            bool LocalTime
            );
        
        [PreserveSig] 
        int KeyExchangePhase1(
            );

        [PreserveSig] 
        int KeyExchangePhase2(
            );

        [PreserveSig] 
        int Backup(
            [MarshalAs(UnmanagedType.LPWStr)] string Location,
            int Version,
            int Flags
            );

        [PreserveSig] 
        int Restore(
            [MarshalAs(UnmanagedType.LPWStr)] string Location,
            int Version,
            int Flags
            );

        [PreserveSig] 
        void EnumBackups(
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst=256)] out string Location,
            [Out, MarshalAs(UnmanagedType.U4)] out uint Version,
            [Out] out System.Runtime.InteropServices.ComTypes.FILETIME BackupTime,
            uint EnumIndex
            );

        [PreserveSig] 
        void DeleteBackup(
            [MarshalAs(UnmanagedType.LPWStr)] string Location,
            int Version
            );

        [PreserveSig] 
        int UnmarshalInterface(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMSAdminBase interf
            );

        [PreserveSig] 
        int GetServerGuid(
            );
    }

    [ClassInterface(ClassInterfaceType.None)]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [ComImport, Guid("a9e69610-b80d-11d0-b9b9-00a0c922e750")]
    internal class MSAdminBase
    {
    }
    #endregion

    internal enum MBDataType : byte
    {
        All             = 0,
        Dword           = 1,
        String          = 2,
        Binary          = 3,
        StringExpand    = 4,
        MultiString     = 5
    };

    internal enum MBUserType : byte
    {
        Other           = 0,
        Asp             = 101,  // ASP_MD_UT_APP,
        File            = 2,    // IIS_MD_UT_FILE,
        Server          = 1,    // IIS_MD_UT_SERVER,
        Wam             = 100   // IIS_MD_UT_WAM
    };

    [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal static class IisPickupDirectory
    {
        const int MaxPathSize        = 260;
        const int InfiniteTimeout    = -1;
        const int MetadataMaxNameLen = 256;

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal unsafe static string GetPickupDirectory()
        {
            int            hr;
            UInt32         reqLength=0;
            Int32          serverState;
            string         pickupDirectory=string.Empty;
            IMSAdminBase   adminBase = null;
            IntPtr         ptrKey = IntPtr.Zero;
            StringBuilder  keySuffix = new StringBuilder(MetadataMaxNameLen);
            uint           bufferLen = MaxPathSize * 4;
            byte[]         buffer = new byte[bufferLen];
            
            try {
                adminBase = new MSAdminBase() as IMSAdminBase;
                hr = adminBase.OpenKey(IntPtr.Zero, "LM/SmtpSvc", MBKeyAccess.Read, InfiniteTimeout, ref ptrKey);            
                if (hr < 0)
                    goto Exit;

                MetadataRecord rec = new MetadataRecord();

                fixed( byte* bufferPtr = buffer)
                {
                    for (int index=0; ; index++)
                    {
                        hr = adminBase.EnumKeys(ptrKey, "", keySuffix, index);
                        if (hr == unchecked((int)MBErrors.NoMoreItems))
                            break;
                        if (hr < 0)
                            goto Exit;
                        
                        rec.Identifier      = (UInt32) PropertyName.ServerState;
                        rec.Attributes      = 0;
                        rec.UserType        = (UInt32) MBUserType.Server;
                        rec.DataType        = (UInt32) MBDataType.Dword;
                        rec.DataTag         = 0;
                        rec.DataBuf         = (IntPtr) bufferPtr;
                        rec.DataLen         = bufferLen;
                    
                        hr = adminBase.GetData(ptrKey, keySuffix.ToString(), ref rec, ref reqLength);
                        if (hr < 0)
                        {
                            if (hr == unchecked((int)MBErrors.DataNotFound) || 
                                hr == unchecked((int)MBErrors.AccessDenied))
                                continue;
                            else
                                goto Exit;
                        }
                        serverState = Marshal.ReadInt32((IntPtr)bufferPtr);

                        if (serverState == (Int32) ServerState.Started)
                        {
                            rec.Identifier      = (UInt32) PropertyName.PickupDirectory;
                            rec.Attributes      = 0;
                            rec.UserType        = (UInt32) MBUserType.Server;
                            rec.DataType        = (UInt32) MBDataType.String;
                            rec.DataTag         = 0;
                            rec.DataBuf         = (IntPtr) bufferPtr;
                            rec.DataLen         = bufferLen;
                    
                            hr = adminBase.GetData(ptrKey, keySuffix.ToString(), ref rec, ref reqLength);
                            if (hr < 0)
                                goto Exit;

                            pickupDirectory = Marshal.PtrToStringUni((IntPtr)bufferPtr);
                            break;
                        }
                    }

                    if (hr == unchecked((int)MBErrors.NoMoreItems))
                    {

                        for (int index=0; ; index++)
                        {
                            hr = adminBase.EnumKeys(ptrKey, "", keySuffix, index);
                            if (hr == unchecked((int)MBErrors.NoMoreItems))
                                break;
                            if (hr < 0)
                                goto Exit;

                            rec.Identifier = (UInt32) PropertyName.PickupDirectory;
                            rec.Attributes = 0;
                            rec.UserType   = (UInt32) MBUserType.Server;
                            rec.DataType   = (UInt32) MBDataType.String;
                            rec.DataTag    = 0;
                            rec.DataBuf    = (IntPtr) bufferPtr;
                            rec.DataLen    = bufferLen;
                    
                            hr = adminBase.GetData(ptrKey, keySuffix.ToString(), ref rec, ref reqLength);
                            if (hr < 0)
                            {
                                if (hr == unchecked((int)MBErrors.DataNotFound) || 
                                    hr == unchecked((int)MBErrors.AccessDenied))
                                    continue;
                                else
                                    goto Exit;
                            }

                            pickupDirectory = Marshal.PtrToStringUni((IntPtr)bufferPtr);
                            if (Directory.Exists(pickupDirectory))
                                break;
                            else
                                pickupDirectory = string.Empty;
                        }
                    }
                }
Exit:
                ;
            }
            catch (Exception exception) {
                if (exception is SecurityException || 
                    exception is AuthenticationException ||
                    exception is SmtpException)
                    throw;
                throw new SmtpException(SR.GetString(SR.SmtpGetIisPickupDirectoryFailed));
            }
            finally {
                if (adminBase != null)
                    if (ptrKey != IntPtr.Zero)
                        adminBase.CloseKey(ptrKey);
            }

            if (pickupDirectory == string.Empty)
                throw new SmtpException(SR.GetString(SR.SmtpGetIisPickupDirectoryFailed));

            return pickupDirectory;
        }
    }
}

