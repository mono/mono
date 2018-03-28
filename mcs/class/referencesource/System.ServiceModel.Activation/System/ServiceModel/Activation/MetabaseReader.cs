//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Activation.Interop;
    using System.Threading;

    // These values are copied from %SDXROOT%\public\sdk\inc\iiscnfg.h.
    enum MetabasePropertyType
    {
        ServerBindings = 1023,
        SecureBindings = 2021,
        AuthFlags = 6000,
        Realm = 6001,
        AnonymousUserName = 6020,
        AnonymousPassword = 6021,
        AccessSslFlags = 6030,
        AuthPersistence = 6031,
        AuthProviders = 6032,
    }

    [Fx.Tag.SecurityNote(Critical = "Does a bunch of unsafe and native access." +
        "Caller should guard MetabaseReader instance as well as any results.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class MetabaseReader : IDisposable
    {
        internal const string LMPath = "/LM";
        const uint E_INSUFFICIENT_BUFFER = 0x8007007A;
        const uint E_PATH_NOT_FOUND = 0x80070003;
        const uint E_DATA_NOT_FOUND = 0x800CC801;

        static IMSAdminBase adminBase;
        static object syncRoot = new object();
        int mdHandle = 0;

        [Fx.Tag.SecurityNote(Critical = "Stores a handle.")]
        [SecurityCritical]
        METADATA_RECORD record;

        [Fx.Tag.SecurityNote(Critical = "Handle to an unmanaged resource.")]
        [SecurityCritical]
        SafeHandle bufferHandle;

        uint currentBufferSize = 1024;
        bool disposed;
        [Fx.Tag.SecurityNote(Safe = "Access Critical Private Members but does not expose critical private members to callers.")]
        [SecuritySafeCritical]
        public MetabaseReader()
        {
            lock (syncRoot)
            {
                if (adminBase == null)
                {
                    adminBase = (IMSAdminBase)new MSAdminBase();
                }
            }

            uint handle;
            uint hResult = adminBase.OpenKey(MSAdminBase.METADATA_MASTER_ROOT_HANDLE, LMPath,
                MSAdminBase.METADATA_PERMISSION_READ, MSAdminBase.DEFAULT_METABASE_TIMEOUT, out handle);
            mdHandle = (int)handle;

            if (hResult != 0)
            {
                throw FxTrace.Exception.AsError(new COMException(SR.Hosting_MetabaseAccessError, (int)hResult));
            }

            bufferHandle = SafeHGlobalHandleCritical.AllocHGlobal(currentBufferSize);
        }

        ~MetabaseReader()
        {
            Dispose(false);
        }
        [Fx.Tag.SecurityNote(Safe = "Access Critical Private Members and returns a managed representation of the handle.")]
        [SecurityCritical]
        public object GetData(string path, MetabasePropertyType propertyType)
        {
            return GetData(path, (uint)propertyType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        [Fx.Tag.SecurityNote(Safe = "Access Critical Private Members but does not expose critical private members to callers.")]
        [SecuritySafeCritical]
        void EnsureRecordBuffer(uint bytes)
        {
            if (bytes <= currentBufferSize)
            {
                return;
            }

            bufferHandle.Close();
            currentBufferSize = bytes;
            bufferHandle = SafeHGlobalHandleCritical.AllocHGlobal(currentBufferSize);

            record.pbMDData = bufferHandle.DangerousGetHandle();
            record.dwMDDataLen = currentBufferSize;
        }
        [Fx.Tag.SecurityNote(Critical = "Accesses and Returns SecurityCritical private data.")]
        [SecurityCritical]
        object GetData(string path, uint type)
        {
            uint bytes = currentBufferSize;
            record.dwMDAttributes = MSAdminBase.METADATA_INHERIT;
            record.dwMDUserType = MSAdminBase.IIS_MD_UT_SERVER;
            record.dwMDDataType = MSAdminBase.ALL_METADATA;
            record.dwMDIdentifier = type;
            record.pbMDData = bufferHandle.DangerousGetHandle();
            record.dwMDDataLen = currentBufferSize;

            uint hResult = adminBase.GetData((uint)mdHandle, path, ref record, ref bytes);
            if (hResult == E_INSUFFICIENT_BUFFER)
            {
                EnsureRecordBuffer(bytes);
                hResult = adminBase.GetData((uint)mdHandle, path, ref record, ref bytes);
            }

            if (hResult == E_PATH_NOT_FOUND || hResult == E_DATA_NOT_FOUND)
            {
                return null;
            }
            else if (hResult != 0)
            {
                throw FxTrace.Exception.AsError(new COMException(SR.Hosting_MetabaseAccessError, (int)hResult));
            }

            return ConvertData();
        }
        [Fx.Tag.SecurityNote(Critical = "Accesses and Returns SecurityCritical private data.")]
        [SecurityCritical]
        object ConvertData()
        {
            switch (record.dwMDDataType)
            {
                case MSAdminBase.DWORD_METADATA:
                    return (UInt32)Marshal.ReadInt32(record.pbMDData);
                case MSAdminBase.EXPANDSZ_METADATA:
                case MSAdminBase.STRING_METADATA:
                    return Marshal.PtrToStringUni(record.pbMDData);
                case MSAdminBase.MULTISZ_METADATA:
                    return RecordToStringArray();
                default:
                    throw FxTrace.Exception.AsError(new NotSupportedException(
                        SR.Hosting_MetabaseDataTypeUnsupported(
                        record.dwMDDataType.ToString(NumberFormatInfo.CurrentInfo),
                        record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
            }
        }
        [Fx.Tag.SecurityNote(Critical = "Accesses and Returns SecurityCritical private data.")]
        [SecurityCritical]
        string[] RecordToStringArray()
        {
            List<string> list = new List<string>();
            if (record.dwMDDataType == MSAdminBase.MULTISZ_METADATA)
            {
                // Ensure that the data is an array of double-byte unicode chars.
                if ((record.dwMDDataLen & 1) != 0)
                {
                    throw FxTrace.Exception.AsError(new DataMisalignedException(
                        SR.Hosting_MetabaseDataStringsTerminate(record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
                }

                int startPos = 0;
                int endPos = 0;
                while (record.dwMDDataLen > 0)
                {
                    // Scan for a null terminator.
                    while (endPos < record.dwMDDataLen && Marshal.ReadInt16(record.pbMDData, endPos) != 0)
                    {
                        endPos += 2;
                    }

                    if (endPos == record.dwMDDataLen &&
                        Marshal.ReadInt16(record.pbMDData, endPos - 2) != 0)
                    {
                        throw FxTrace.Exception.AsError(new DataMisalignedException(
                            SR.Hosting_MetabaseDataStringsTerminate(record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
                    }

                    // End of the string.
                    if (endPos == startPos)
                    {
                        break;
                    }

                    // Convert to string.
                    list.Add(Marshal.PtrToStringUni(new IntPtr(record.pbMDData.ToInt64() + startPos),
                        (endPos - startPos) / 2));

                    // Go to next string
                    startPos = endPos += 2;
                }
            }

            return list.ToArray();
        }
        [Fx.Tag.SecurityNote(Safe = "Access Critical Private Members but does not expose critical private members to callers.")]
        [SecuritySafeCritical]
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    record.pbMDData = IntPtr.Zero;
                    currentBufferSize = 0;

                    if (bufferHandle != null)
                    {
                        bufferHandle.Close();
                    }
                }

                int handleToClose = Interlocked.Exchange(ref mdHandle, 0);
                if (handleToClose != 0)
                {
                    adminBase.CloseKey((uint)handleToClose);
                }

                //Notice we cannot assign adminBase to NULL in Dispose, because 
                //it could be shared by multiple instances of MetabaseReader.

                disposed = true;
            }
        }

    }
}
