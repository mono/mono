//------------------------------------------------------------------------------
// <copyright file="DBPropSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Runtime.ConstrainedExecution;

namespace System.Data.OleDb {

    sealed internal class DBPropSet : SafeHandle {

        private readonly Int32 propertySetCount;

        // VSDD 621427: stores the exception with last error.HRESULT from IDBProperties.GetProperties
        private Exception lastErrorFromProvider;

        private DBPropSet() : base(IntPtr.Zero, true) {
            propertySetCount = 0;
        }

        internal DBPropSet(int propertysetCount) : this() {
            this.propertySetCount = propertysetCount;
            IntPtr countOfBytes = (IntPtr)(propertysetCount * ODB.SizeOf_tagDBPROPSET);
            RuntimeHelpers.PrepareConstrainedRegions();
            try {} finally {
                base.handle = SafeNativeMethods.CoTaskMemAlloc(countOfBytes);
                if (ADP.PtrZero != base.handle) {
                    SafeNativeMethods.ZeroMemory(base.handle, countOfBytes);
                }
            }
            if (ADP.PtrZero == base.handle) {
                throw new OutOfMemoryException();
            }
        }

        internal DBPropSet(UnsafeNativeMethods.IDBProperties properties, PropertyIDSet propidset, out OleDbHResult hr) : this() {
            Debug.Assert(null != properties, "null IDBProperties");

            int propidsetcount = 0;
            if (null != propidset) {
                propidsetcount = propidset.Count;
            }
            Bid.Trace("<oledb.IDBProperties.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(propidsetcount, propidset, out this.propertySetCount, out base.handle);
            Bid.Trace("<oledb.IDBProperties.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);

            if (hr < 0) {
                // VSDD 621427: remember the last HRESULT. Note we do not want to raise exception now to avoid breaking change from Orcas RTM/SP1
                SetLastErrorInfo(hr);
            }
        }

        internal DBPropSet(UnsafeNativeMethods.IRowsetInfo properties, PropertyIDSet propidset, out OleDbHResult hr) : this() {
            Debug.Assert(null != properties, "null IRowsetInfo");

            int propidsetcount = 0;
            if (null != propidset) {
                propidsetcount = propidset.Count;
            }
            Bid.Trace("<oledb.IRowsetInfo.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(propidsetcount, propidset, out this.propertySetCount, out base.handle);
            Bid.Trace("<oledb.IRowsetInfo.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);

            if (hr < 0) {
                // VSDD 621427: remember the last HRESULT. Note we do not want to raise exception now to avoid breaking change from Orcas RTM/SP1
                SetLastErrorInfo(hr);
            }
        }

        internal DBPropSet(UnsafeNativeMethods.ICommandProperties properties, PropertyIDSet propidset, out OleDbHResult hr) : this() {
            Debug.Assert(null != properties, "null ICommandProperties");

            int propidsetcount = 0;
            if (null != propidset) {
                propidsetcount = propidset.Count;
            }
            Bid.Trace("<oledb.ICommandProperties.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(propidsetcount, propidset, out this.propertySetCount, out base.handle);
            Bid.Trace("<oledb.ICommandProperties.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);

            if (hr < 0) {
                // VSDD 621427: remember the last HRESULT. Note we do not want to raise exception now to avoid breaking change from Orcas RTM/SP1
                SetLastErrorInfo(hr);
            }
        }

        private void SetLastErrorInfo(OleDbHResult lastErrorHr) {
            // note: OleDbHResult is actually a simple wrapper over HRESULT with OLEDB-specific codes
            UnsafeNativeMethods.IErrorInfo errorInfo = null;
            string message = String.Empty;

            OleDbHResult errorInfoHr = UnsafeNativeMethods.GetErrorInfo(0, out errorInfo);  // 0 - IErrorInfo exists, 1 - no IErrorInfo
            if ((errorInfoHr == OleDbHResult.S_OK) && (errorInfo != null)) {
                ODB.GetErrorDescription(errorInfo, lastErrorHr, out message);
                // note that either GetErrorInfo or GetErrorDescription might fail in which case we will have only the HRESULT value in exception message
            }
            lastErrorFromProvider = new COMException(message, (int)lastErrorHr);
        }

        public override bool IsInvalid {
            get {
                return (IntPtr.Zero == base.handle);
            }
        }

         override protected bool ReleaseHandle() {
            // NOTE: The SafeHandle class guarantees this will be called exactly once and is non-interrutible.
            IntPtr ptr = base.handle;
            base.handle = IntPtr.Zero;
            if (ADP.PtrZero != ptr) {

                int count = this.propertySetCount;
                for (int i = 0, offset = 0; i < count; ++i, offset += ODB.SizeOf_tagDBPROPSET) {

                    IntPtr rgProperties = Marshal.ReadIntPtr(ptr, offset);
                    if(ADP.PtrZero != rgProperties) {
                        int cProperties = Marshal.ReadInt32(ptr, offset + ADP.PtrSize);

                        IntPtr vptr = ADP.IntPtrOffset(rgProperties, ODB.OffsetOf_tagDBPROP_Value);
                        for (int k = 0; k < cProperties; ++k, vptr = ADP.IntPtrOffset(vptr, ODB.SizeOf_tagDBPROP)) {
                            SafeNativeMethods.VariantClear(vptr);
                        }
                        SafeNativeMethods.CoTaskMemFree(rgProperties);
                    }
                }
                SafeNativeMethods.CoTaskMemFree(ptr);
            }
            return true;
        }

        internal int PropertySetCount {
            get {
                return this.propertySetCount;
            }
        }

        internal tagDBPROP[] GetPropertySet(int index, out Guid propertyset) {
            if ((index < 0) || (PropertySetCount <= index)) {
                if (lastErrorFromProvider != null)
                {
                    // VSDD 621427: add extra error information for CSS/stress troubleshooting.
                    // We need to keep same exception type to avoid breaking change with Orcas RTM/SP1.
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer, lastErrorFromProvider);
                }
                else {
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer);
                }
            }

            tagDBPROPSET propset = new tagDBPROPSET();
            tagDBPROP[] properties = null;

            bool mustRelease = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                DangerousAddRef(ref mustRelease);
                IntPtr propertySetPtr = ADP.IntPtrOffset(DangerousGetHandle(), index * ODB.SizeOf_tagDBPROPSET);
                Marshal.PtrToStructure(propertySetPtr, propset);
                propertyset = propset.guidPropertySet;

                properties = new tagDBPROP[propset.cProperties];
                for(int i = 0; i < properties.Length; ++i) {
                    properties[i] = new tagDBPROP();
                    IntPtr ptr = ADP.IntPtrOffset(propset.rgProperties, i * ODB.SizeOf_tagDBPROP);
                    Marshal.PtrToStructure(ptr, properties[i]);
                }
            }
            finally {
                if (mustRelease) {
                    DangerousRelease();
                }
            }
            return properties;
        }

        internal void SetPropertySet(int index, Guid propertySet, tagDBPROP[] properties) {
            if ((index < 0) || (PropertySetCount <= index)) {
                if (lastErrorFromProvider != null) {
                    // VSDD 621427: add extra error information for CSS/stress troubleshooting.
                    // We need to keep same exception type to avoid breaking change with Orcas RTM/SP1.
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer, lastErrorFromProvider);
                }
                else {
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer);
                }
            }
            Debug.Assert(Guid.Empty != propertySet, "invalid propertySet");
            Debug.Assert((null != properties) && (0 < properties.Length), "invalid properties");

            IntPtr countOfBytes = (IntPtr)(properties.Length * ODB.SizeOf_tagDBPROP);
            tagDBPROPSET propset = new tagDBPROPSET(properties.Length, propertySet);

            bool mustRelease = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                DangerousAddRef(ref mustRelease);

                IntPtr propsetPtr = ADP.IntPtrOffset(DangerousGetHandle(), index * ODB.SizeOf_tagDBPROPSET);

                RuntimeHelpers.PrepareConstrainedRegions();
                try {} finally {
                    // must allocate and clear the memory without interruption
                    propset.rgProperties = SafeNativeMethods.CoTaskMemAlloc(countOfBytes);
                    if (ADP.PtrZero != propset.rgProperties) {
                        // clearing is important so that we don't treat existing
                        // garbage as important information during releaseHandle
                        SafeNativeMethods.ZeroMemory(propset.rgProperties, countOfBytes);
                        
                        // writing the structure to native memory so that it knows to free the referenced pointers
                        Marshal.StructureToPtr(propset, propsetPtr, false/*deleteold*/);
                    }
                }
                if (ADP.PtrZero == propset.rgProperties) {
                    throw new OutOfMemoryException();
                }

                for(int i = 0; i < properties.Length; ++i) {
                    Debug.Assert(null != properties[i], "null tagDBPROP " + i.ToString(CultureInfo.InvariantCulture));
                    IntPtr propertyPtr = ADP.IntPtrOffset(propset.rgProperties, i * ODB.SizeOf_tagDBPROP);
                    Marshal.StructureToPtr(properties[i], propertyPtr, false/*deleteold*/);
                }
            }
            finally {
                if (mustRelease) {
                    DangerousRelease();
                }
            }
        }

        static internal DBPropSet CreateProperty(Guid propertySet, int propertyId, bool required, object value) {
            tagDBPROP dbprop = new tagDBPROP(propertyId, required, value);
            DBPropSet propertyset = new DBPropSet(1);
            propertyset.SetPropertySet(0, propertySet, new tagDBPROP[1] { dbprop });
            return propertyset;
        }
    }
}
