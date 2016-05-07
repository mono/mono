//------------------------------------------------------------------------------
// <copyright file="etwprovider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using System.Security;
using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.Eventing{

    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventProvider : IDisposable 
    {
        [SecurityCritical]
        UnsafeNativeMethods.EtwEnableCallback m_etwCallback;  // Trace Callback function
        private long m_regHandle;                       // Trace Registration Handle
        private byte m_level;                            // Tracing Level
        private long m_anyKeywordMask;                  // Trace Enable Flags
        private long m_allKeywordMask;                  // Match all keyword
        private int m_enabled;                           // Enabled flag from Trace callback
        private Guid m_providerId;                       // Control Guid 
        private int m_disposed;                          // when 1, provider has unregister
        [ThreadStatic]
        private static WriteEventErrorCode t_returnCode; // thread slot to keep last error
        private static bool s_platformNotSupported = (Environment.OSVersion.Version.Major < 6);
        private static bool s_preWin7 = (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 1));        

        private const int s_basicTypeAllocationBufferSize = 16;
        private const int s_etwMaxMumberArguments = 32;
        private const int s_etwAPIMaxStringCount = 8;
        private const int s_maxEventDataDescriptors = 128;
        private const int s_traceEventMaximumSize = 65482;
        private const int s_traceEventMaximumStringSize = 32724;

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum WriteEventErrorCode : int
        {
            //check mapping to runtime codes
            NoError = 0,
            NoFreeBuffers = 1,
            EventTooBig = 2
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct EventData
        {
            [FieldOffset(0)]
            internal ulong DataPointer;
            [FieldOffset(8)]
            internal uint Size;
            [FieldOffset(12)]
            internal int Reserved;
        }

        private enum ActivityControl : uint
        {
            EVENT_ACTIVITY_CTRL_GET_ID = 1,
            EVENT_ACTIVITY_CTRL_SET_ID = 2,
            EVENT_ACTIVITY_CTRL_CREATE_ID = 3,
            EVENT_ACTIVITY_CTRL_GET_SET_ID = 4,
            EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5
        }

        /// <summary>
        /// Constructor for EventProvider class.  
        /// </summary>
        /// <param name="ProviderGuid">
        /// Unique GUID among all trace sources running on a system
        /// </param>
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "guid")]
        public EventProvider(Guid providerGuid)
        {            
            m_providerId = providerGuid;

            //
            // EtwRegister the ProviderId with ETW
            //
            EtwRegister();
        }

        /// <summary>
        /// This method registers the controlGuid of this class with ETW.
        /// We need to be running on Vista or above. If not an 
        /// PlatformNotSupported exception will be thrown. 
        /// If for some reason the ETW EtwRegister call failed
        /// a NotSupported exception will be thrown. 
        /// </summary>
        [System.Security.SecurityCritical]
        private unsafe void EtwRegister()
        {

            uint status;

            //
            // Check only the mayor version
            //

            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException(SR.GetString(SR.NotSupported_DownLevelVista));
            }

            m_etwCallback = new UnsafeNativeMethods.EtwEnableCallback(EtwEnableCallBack);


            status = UnsafeNativeMethods.EventRegister(ref m_providerId, m_etwCallback, null, ref m_regHandle);
            if (status != 0)
            {
                throw new Win32Exception((int)status);
            }

        }

        //
        // implement Dispose Pattern to early deregister from ETW insted of waiting for 
        // the finalizer to call deregistration.
        // Once the user is done with the provider it needs to call Close() or Dispose()
        // If neither are called the finalizer will unregister the provider anyway
        //
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Security.SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            //
            // explicit cleanup is done by calling Dispose with true from 
            // Dispose() or Close(). The disposing arguement is ignored because there
            // are no unmanaged resources.
            // The finalizer calls Dispose with false.
            //

            //
            // check if the object has been allready disposed
            //
            if (m_disposed == 1) return;

            if (Interlocked.Exchange(ref m_disposed, 1) != 0)
            {
                // somebody is allready disposing the provider
                return;
            }

            //
            // Disables Tracing in the provider, then unregister
            // 

            m_enabled = 0;

            Deregister();
        }

        /// <summary>
        /// This method deregisters the controlGuid of this class with ETW.
        /// 
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        ~EventProvider()
        {
            Dispose(false);
        }

        /// <summary>
        /// This method un-registers from ETW.
        /// </summary>
        [System.Security.SecurityCritical]
        private unsafe void Deregister()
        {
            //
            // Unregister from ETW using the RegHandle saved from
            // the register call.
            //

            if (m_regHandle != 0)
            {
                UnsafeNativeMethods.EventUnregister(m_regHandle);
                m_regHandle = 0;
            }
        }

        [System.Security.SecurityCritical]
        unsafe void EtwEnableCallBack(
                        [In] ref System.Guid sourceId,
                        [In] int isEnabled,
                        [In] byte setLevel,
                        [In] long anyKeyword,
                        [In] long allKeyword,
                        [In] void* filterData,
                        [In] void* callbackContext
                        )
        {

            m_enabled = isEnabled;
            m_level = setLevel;
            m_anyKeywordMask = anyKeyword;
            m_allKeywordMask = allKeyword;
            return;
        }

        /// <summary>
        /// IsEnabled, method used to test if provider is enabled
        /// </summary>
        public bool IsEnabled()
        {
            return (m_enabled != 0) ? true : false;
        }

        /// <summary>
        /// IsEnabled, method used to test if event is enabled
        /// </summary>
        /// <param name="Lvl">
        /// Level  to test
        /// </param>
        /// <param name="Keyword">
        /// Keyword  to test
        /// </param>
        public bool IsEnabled(byte level, long keywords)
        {

            //
            // If not enabled at all, return false.
            //

            if (m_enabled == 0)
            {
                return false;
            }

            // This also covers the case of Level == 0.
            if ((level <= m_level) || 
                (m_level == 0))
            {

                //
                // Check if Keyword is enabled
                //

                if ((keywords == 0) ||
                    (((keywords & m_anyKeywordMask) != 0) &&
                     ((keywords & m_allKeywordMask) == m_allKeywordMask)))
                {
                    return true;
                }
            }

            return false;
        }

        public static WriteEventErrorCode GetLastWriteEventError()
        {
            return t_returnCode;
        } 

        //
        // Helper function to set the last error on the thread
        //
        private static void SetLastError(int error){
            switch (error)
            {
                case UnsafeNativeMethods.ERROR_ARITHMETIC_OVERFLOW:
                case UnsafeNativeMethods.ERROR_MORE_DATA:
                    t_returnCode = WriteEventErrorCode.EventTooBig;
                    break;
                case UnsafeNativeMethods.ERROR_NOT_ENOUGH_MEMORY:
                    t_returnCode = WriteEventErrorCode.NoFreeBuffers;
                    break;
            }
        }


        [System.Security.SecurityCritical]
        private static unsafe string EncodeObject(ref object data, EventData* dataDescriptor, byte* dataBuffer)
        /*++

        Routine Description:

           This routine is used by WriteEvent to unbox the object type and
           to fill the passed in ETW data descriptor. 

        Arguments:

           data - argument to be decoded

           dataDescriptor - pointer to the descriptor to be filled

           dataBuffer - storage buffer for storing user data, needed because cant get the address of the object

        Return Value:

           null if the object is a basic type other than string. String otherwise

        --*/
        {
            dataDescriptor->Reserved = 0;

            string sRet = data as string;
            if (sRet != null)
            {
                dataDescriptor->Size = (uint)((sRet.Length + 1) * 2);
                return sRet;
            }

            if (data == null)
            {
                dataDescriptor->Size = 0;
                dataDescriptor->DataPointer = 0;
            }
            else if (data is IntPtr)
            {
                dataDescriptor->Size = (uint)sizeof(IntPtr);
                IntPtr* intptrPtr = (IntPtr*)dataBuffer;
                *intptrPtr = (IntPtr)data;
                dataDescriptor->DataPointer = (ulong)intptrPtr;
            }
            else if (data is int)
            {
                dataDescriptor->Size = (uint)sizeof(int);
                int* intptrPtr = (int*)dataBuffer;
                *intptrPtr = (int)data;
                dataDescriptor->DataPointer = (ulong)intptrPtr;
            }
            else if (data is long)
            {
                dataDescriptor->Size = (uint)sizeof(long);
                long* longptr = (long*)dataBuffer;
                *longptr = (long)data;
                dataDescriptor->DataPointer = (ulong)longptr;
            }
            else if (data is uint)
            {
                dataDescriptor->Size = (uint)sizeof(uint);
                uint* uintptr = (uint*)dataBuffer;
                *uintptr = (uint)data;
                dataDescriptor->DataPointer = (ulong)uintptr;
            }
            else if (data is UInt64)
            {
                dataDescriptor->Size = (uint)sizeof(ulong);
                UInt64* ulongptr = (ulong*)dataBuffer;
                *ulongptr = (ulong)data;
                dataDescriptor->DataPointer = (ulong)ulongptr;
            }
            else if (data is char)
            {
                dataDescriptor->Size = (uint)sizeof(char);
                char* charptr = (char*)dataBuffer;
                *charptr = (char)data;
                dataDescriptor->DataPointer = (ulong)charptr;
            }
            else if (data is byte)
            {
                dataDescriptor->Size = (uint)sizeof(byte);
                byte* byteptr = (byte*)dataBuffer;
                *byteptr = (byte)data;
                dataDescriptor->DataPointer = (ulong)byteptr;
            }
            else if (data is short)
            {
                dataDescriptor->Size = (uint)sizeof(short);
                short* shortptr = (short*)dataBuffer;
                *shortptr = (short)data;
                dataDescriptor->DataPointer = (ulong)shortptr;
            }
            else if (data is sbyte)
            {
                dataDescriptor->Size = (uint)sizeof(sbyte);
                sbyte* sbyteptr = (sbyte*)dataBuffer;
                *sbyteptr = (sbyte)data;
                dataDescriptor->DataPointer = (ulong)sbyteptr;
            }
            else if (data is ushort)
            {
                dataDescriptor->Size = (uint)sizeof(ushort);
                ushort* ushortptr = (ushort*)dataBuffer;
                *ushortptr = (ushort)data;
                dataDescriptor->DataPointer = (ulong)ushortptr;
            }
            else if (data is float)
            {
                dataDescriptor->Size = (uint)sizeof(float);
                float* floatptr = (float*)dataBuffer;
                *floatptr = (float)data;
                dataDescriptor->DataPointer = (ulong)floatptr;
            }
            else if (data is double)
            {
                dataDescriptor->Size = (uint)sizeof(double);
                double* doubleptr = (double*)dataBuffer;
                *doubleptr = (double)data;
                dataDescriptor->DataPointer = (ulong)doubleptr;
            }
            else if (data is bool)
            {
                dataDescriptor->Size = (uint)sizeof(bool);
                bool* boolptr = (bool*)dataBuffer;
                *boolptr = (bool)data;
                dataDescriptor->DataPointer = (ulong)boolptr;
            }
            else if (data is Guid)
            {
                dataDescriptor->Size = (uint)sizeof(Guid);
                Guid* guidptr = (Guid*)dataBuffer;
                *guidptr = (Guid)data;
                dataDescriptor->DataPointer = (ulong)guidptr;
            }
            else if (data is decimal)
            {
                dataDescriptor->Size = (uint)sizeof(decimal);
                decimal* decimalptr = (decimal*)dataBuffer;
                *decimalptr = (decimal)data;
                dataDescriptor->DataPointer = (ulong)decimalptr;
            }
            else if (data is Boolean)
            {
                dataDescriptor->Size = (uint)sizeof(Boolean);
                Boolean* booleanptr = (Boolean*)dataBuffer;
                *booleanptr = (Boolean)data;
                dataDescriptor->DataPointer = (ulong)booleanptr;
            }
            else
            {
                //To our eyes, everything else is a just a string
                sRet = data.ToString();
                dataDescriptor->Size = (uint)((sRet.Length + 1) * 2);
                return sRet;
            }

            return null;
        }


        /// <summary>
        /// WriteMessageEvent, method to write a string with level and Keyword.
        /// The activity ID will be propagated only if the call stays on the same native thread as SetActivityId().
        /// </summary>
        /// <param name="level">
        /// Level  to test  
        /// </param>
        /// <param name="Keyword">
        /// Keyword  to test 
        /// </param>
        [System.Security.SecurityCritical]
        public bool WriteMessageEvent(string eventMessage, byte eventLevel, long eventKeywords)
        {
            int status = 0;

            if (eventMessage == null)
            {
                throw new ArgumentNullException("eventMessage");
            }

            if (IsEnabled(eventLevel, eventKeywords))
            {
                if (eventMessage.Length > s_traceEventMaximumStringSize)
                {
                    t_returnCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                unsafe
                {
                    fixed (char* pdata = eventMessage)
                    {
                        status = (int)UnsafeNativeMethods.EventWriteString(m_regHandle, eventLevel, eventKeywords, pdata);
                    }

                    if (status != 0)
                    {
                        SetLastError(status);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// WriteMessageEvent, method to write a string with level=0 and Keyword=0
        /// The activity ID will be propagated only if the call stays on the same native thread as SetActivityId().
        /// </summary>
        /// <param name="eventMessage">
        /// Message to log  
        /// </param> 
        public bool WriteMessageEvent(string eventMessage)
        {
            return WriteMessageEvent(eventMessage, 0, 0);
        }

 
        /// <summary>
        /// WriteEvent method to write parameters with event schema properties
        /// </summary>
        /// <param name="EventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>        
        public bool WriteEvent(ref EventDescriptor eventDescriptor, params  object[] eventPayload)
        {
            return WriteTransferEvent(ref eventDescriptor, Guid.Empty, eventPayload);            
        }

        /// <summary>
        /// WriteEvent, method to write a string with event schema properties
        /// </summary>
        /// <param name="EventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>
        /// <param name="dataString">
        /// string to log. 
        /// </param>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public bool WriteEvent(ref EventDescriptor eventDescriptor, string data)
        {
            uint status = 0;

            if (data == null)
            {
                throw new ArgumentNullException("dataString");
            }

            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                if (data.Length > s_traceEventMaximumStringSize)
                {
                    t_returnCode = WriteEventErrorCode.EventTooBig; 
                    return false;
                }

                EventData userData;

                userData.Size = (uint)((data.Length +1)* 2);
                userData.Reserved = 0;

                unsafe
                {
                    fixed (char* pdata = data)
                    {
                        Guid activityId = GetActivityId();
                        userData.DataPointer = (ulong)pdata;
                        if (s_preWin7)
                        {
                            status = UnsafeNativeMethods.EventWrite(m_regHandle, 
                                                                     ref eventDescriptor, 
                                                                     1, 
                                                                     &userData);
                        }
                        else
                        {
                            status = UnsafeNativeMethods.EventWriteTransfer(m_regHandle, 
                                                                             ref eventDescriptor, 
                                                                             (activityId == Guid.Empty) ? null : &activityId, 
                                                                             null, 
                                                                             1, 
                                                                             &userData);
                        }
                    }
                }
            }

            if (status != 0)
            {
                SetLastError((int)status);
                return false;
            }
            return true;
        }

        /// <summary>
        /// WriteEvent, method to be used by generated code on a derived class
        /// </summary>
        /// <param name="EventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>
        /// <param name="count">
        /// number of event descriptors 
        /// </param>
        /// <param name="data">
        /// pointer  do the event data
        /// </param> 
        [System.Security.SecurityCritical]
        protected bool WriteEvent(ref EventDescriptor eventDescriptor, int dataCount, IntPtr data)
        {
            uint status = 0;

            unsafe
            {
                if (s_preWin7)
                {
                    status = UnsafeNativeMethods.EventWrite(
                                                        m_regHandle, 
                                                        ref eventDescriptor,
                                                        (uint)dataCount, 
                                                        (void*)data);
                }
                else
                {
                    Guid activityId = GetActivityId();

                    status = UnsafeNativeMethods.EventWriteTransfer(
                                        m_regHandle, 
                                        ref eventDescriptor,
                                        (activityId == Guid.Empty) ? null : &activityId, 
                                        null, 
                                        (uint)dataCount, 
                                        (void*)data);
                }
            }

            if (status != 0)
            {
                SetLastError((int)status);
                return false;
            }
            return true;

        }


        /// <summary>
        /// WriteTransferEvent, method to write a parameters with event schema properties
        /// </summary>
        /// <param name="eventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>
        [System.Security.SecurityCritical]
        public bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, params object[] eventPayload)
        {
            uint status = 0;

            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                Guid activityId = GetActivityId();

                unsafe
                {
                    int argCount = 0;
                    EventData* userDataPtr = null;
                    
                    if ((eventPayload != null) && (eventPayload.Length != 0))
                    {
                        argCount = eventPayload.Length;
                        if (argCount > s_etwMaxMumberArguments)
                        {
                            //
                            //too many arguments to log
                            //
                            throw new ArgumentOutOfRangeException("eventPayload",
                                SR.GetString(SR.ArgumentOutOfRange_MaxArgExceeded, s_etwMaxMumberArguments));
                        }

                        uint totalEventSize = 0;
                        int index;
                        int stringIndex = 0;
                        int[] stringPosition = new int[s_etwAPIMaxStringCount]; //used to keep the position of strings in the eventPayload parameter
                        string[] dataString = new string[s_etwAPIMaxStringCount]; // string arrays from the eventPayload parameter                        
                        EventData* userData = stackalloc EventData[argCount];             // allocation for the data descriptors
                        userDataPtr = (EventData*)userData;
                        byte* dataBuffer = stackalloc byte[s_basicTypeAllocationBufferSize * argCount]; // 16 byte for unboxing non-string argument
                        byte* currentBuffer = dataBuffer;

                        //
                        // The loop below goes through all the arguments and fills in the data 
                        // descriptors. For strings save the location in the dataString array.
                        // Caculates the total size of the event by adding the data descriptor
                        // size value set in EncodeObjec method.
                        //
                        for (index = 0; index < eventPayload.Length; index++)
                        {                            
                            string isString;
                            isString = EncodeObject(ref eventPayload[index], userDataPtr, currentBuffer);
                            currentBuffer += s_basicTypeAllocationBufferSize;
                            totalEventSize += userDataPtr->Size;
                            userDataPtr++;
                            if (isString != null)
                            {
                                if (stringIndex < s_etwAPIMaxStringCount)
                                {
                                    dataString[stringIndex] = isString;
                                    stringPosition[stringIndex] = index;
                                    stringIndex++;
                                }
                                else
                                {
                                    throw new ArgumentOutOfRangeException("eventPayload",
                                        SR.GetString(SR.ArgumentOutOfRange_MaxStringsExceeded, s_etwAPIMaxStringCount));
                                }
                             }
                        }

                        if (totalEventSize > s_traceEventMaximumSize)
                        {
                            t_returnCode = WriteEventErrorCode.EventTooBig;
                            return false;
                        }

                        fixed (char* v0 = dataString[0], v1 = dataString[1], v2 = dataString[2], v3 = dataString[3],
                                v4 = dataString[4], v5 = dataString[5], v6 = dataString[6], v7 = dataString[7])
                        {
                            userDataPtr = (EventData*)userData;
                            if (dataString[0] != null)
                            {
                                userDataPtr[stringPosition[0]].DataPointer = (ulong)v0;
                            }
                            if (dataString[1] != null)
                            {
                                userDataPtr[stringPosition[1]].DataPointer = (ulong)v1;
                            }
                            if (dataString[2] != null)
                            {
                                userDataPtr[stringPosition[2]].DataPointer = (ulong)v2;
                            }
                            if (dataString[3] != null)
                            {
                                userDataPtr[stringPosition[3]].DataPointer = (ulong)v3;
                            }
                            if (dataString[4] != null)
                            {
                                userDataPtr[stringPosition[4]].DataPointer = (ulong)v4;
                            }
                            if (dataString[5] != null)
                            {
                                userDataPtr[stringPosition[5]].DataPointer = (ulong)v5;
                            }
                            if (dataString[6] != null)
                            {
                                userDataPtr[stringPosition[6]].DataPointer = (ulong)v6;
                            }
                            if (dataString[7] != null)
                            {
                                userDataPtr[stringPosition[7]].DataPointer = (ulong)v7;
                            }
                        }
                    }

                    if (relatedActivityId == Guid.Empty && s_preWin7)
                    {
                        // If relatedActivityId is Guid.Empty, this is not a real transfer: just call EventWrite().
                        // For pre-Win7 platforms we cannot set the activityId from CorrelationManager
                        // because we cannot set relatedActivityId to null (Win7 bug 116784)
                        status = UnsafeNativeMethods.EventWrite (m_regHandle, 
                                                                 ref eventDescriptor, 
                                                                 (uint)argCount, 
                                                                 userDataPtr);
                    }
                    else
                    {                    
                        status = UnsafeNativeMethods.EventWriteTransfer (m_regHandle, 
                                                                         ref eventDescriptor, 
                                                                         (activityId == Guid.Empty) ? null : &activityId, 
                                                                         (relatedActivityId == Guid.Empty && !s_preWin7)? null : &relatedActivityId,
                                                                         (uint)argCount, 
                                                                         userDataPtr);
                    }
                }
            }

            if (status != 0)
            {
                SetLastError((int)status);
                return false;
            }
            return true;
        }

        [System.Security.SecurityCritical]
        protected bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
        {
            uint status = 0;
            
            Guid activityId = GetActivityId();         
            
            unsafe
            {                
                status = UnsafeNativeMethods.EventWriteTransfer(
                                                m_regHandle, 
                                                ref eventDescriptor,
                                                (activityId == Guid.Empty) ? null : &activityId, 
                                                &relatedActivityId, 
                                                (uint)dataCount, 
                                                (void*)data);
            }

            if (status != 0)
            {
                SetLastError((int)status);
                return false;
            }
            return true;
        }

        [System.Security.SecurityCritical]
        private static Guid GetActivityId()
        {                        
            return Trace.CorrelationManager.ActivityId;
        }


        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Does not expose Trace.CorrelationManager")]
        public static void SetActivityId(ref Guid id)
        {
            Trace.CorrelationManager.ActivityId = id;
            UnsafeNativeMethods.EventActivityIdControl((int)ActivityControl.EVENT_ACTIVITY_CTRL_SET_ID, ref id);
        }

        [System.Security.SecurityCritical]
        public static Guid CreateActivityId()
        {
            Guid newId = new Guid();
            UnsafeNativeMethods.EventActivityIdControl((int)ActivityControl.EVENT_ACTIVITY_CTRL_CREATE_ID, ref newId);
            return newId;
        }

    }
    
}
