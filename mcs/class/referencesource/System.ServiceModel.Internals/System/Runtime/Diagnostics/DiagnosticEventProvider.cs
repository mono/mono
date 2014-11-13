//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.Interop;
    using System.Threading;
    using System.Security;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.CodeAnalysis;

    // This is a class defined based on CLR's internal implementation of ETW provider
    // This class should be replaced with CLR's version (whenever avaialble) that exposes callback functionality
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    abstract class DiagnosticsEventProvider : IDisposable
    {
        [SecurityCritical]
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of UnsafeNativeMethods.EtwEnableCallback")]
        UnsafeNativeMethods.EtwEnableCallback etwCallback;      // Trace Callback function
        
        long traceRegistrationHandle;                              // Trace Registration Handle
        byte currentTraceLevel;                                    // Tracing Level
        long anyKeywordMask;                                       // Trace Enable Flags
        long allKeywordMask;                                       // Match all keyword
        bool isProviderEnabled;                                    // Enabled flag from Trace callback
        Guid providerId;                                           // Control Guid 
        int isDisposed;                                            // when 1, provider has unregister        

        [ThreadStatic]
        static WriteEventErrorCode errorCode; // The last return code stored from a WriteEvent call

        const int basicTypeAllocationBufferSize = 16;
        const int etwMaxNumberArguments = 32;
        const int etwAPIMaxStringCount = 8;
        const int maxEventDataDescriptors = 128;
        const int traceEventMaximumSize = 65482;
        const int traceEventMaximumStringSize = 32724;
        const int WindowsVistaMajorNumber = 6;
        
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.NestedTypesShouldNotBeVisible)]
        public enum WriteEventErrorCode : int
        {
            NoError,
            NoFreeBuffers,
            EventTooBig
        }
                
        /// <summary>
        /// Constructs a new EventProvider.  This causes the class to be registered with the OS
        /// if an ETW controller turns on the logging then logging will start. 
        /// </summary>
        /// <param name="providerGuid">The GUID that identifies this provider to the system.</param>
        [SecurityCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        protected DiagnosticsEventProvider(Guid providerGuid)
        {
            this.providerId = providerGuid;            
            EtwRegister();
        }

        /// <summary>
        /// This method registers the controlGuid of this class with ETW.
        /// We need to be running on Vista or above. If not a 
        /// PlatformNotSupported exception will be thrown. 
        /// If for some reason the ETW EtwRegister call failed
        /// a NotSupported exception will be thrown. 
        /// </summary>        
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
            Justification = "Don't trace exceptions thrown from the initialization API.")]
        unsafe void EtwRegister()
        {
            this.etwCallback = new UnsafeNativeMethods.EtwEnableCallback(EtwEnableCallBack);
            uint etwRegistrationStatus = UnsafeNativeMethods.EventRegister(ref this.providerId, this.etwCallback, null, ref this.traceRegistrationHandle);
            if (etwRegistrationStatus != 0)
            {
                throw new InvalidOperationException(InternalSR.EtwRegistrationFailed(etwRegistrationStatus.ToString("x", CultureInfo.CurrentCulture)));
            }
        }

        //
        // implement Dispose Pattern to early deregister from ETW instead of waiting for 
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
            if ((this.isDisposed != 1) && (Interlocked.Exchange(ref this.isDisposed, 1) == 0))
            {
                this.isProviderEnabled = false;
                Deregister();
            }
        }

        /// <summary>
        /// This method deregisters the controlGuid of this class with ETW.        
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        ~DiagnosticsEventProvider()
        {
            Dispose(false);
        }

        /// <summary>
        /// This method un-registers from ETW.
        /// </summary>                
        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotIgnoreMethodResults)]
        [SecurityCritical]
        unsafe void Deregister()
        {
            // Unregister from ETW using the RegHandle saved from
            // the register call.            
            if (this.traceRegistrationHandle != 0)
            {
                UnsafeNativeMethods.EventUnregister(this.traceRegistrationHandle);
                this.traceRegistrationHandle = 0;
            }
        }
                
        [SecurityCritical]
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
            this.isProviderEnabled = (isEnabled != 0);
            this.currentTraceLevel = setLevel;
            this.anyKeywordMask = anyKeyword;
            this.allKeywordMask = allKeyword;
            OnControllerCommand();
        }

        protected abstract void OnControllerCommand();            

        /// <summary>
        /// IsEnabled, method used to test if provider is enabled
        /// </summary>
        public bool IsEnabled()
        {
            return this.isProviderEnabled;
        }

        /// <summary>
        /// IsEnabled, method used to test if event is enabled
        /// </summary>
        /// <param name="level">
        /// Level  to test
        /// </param>
        /// <param name="keywords">
        /// Keyword  to test
        /// </param>
        public bool IsEnabled(byte level, long keywords)
        {
            if (this.isProviderEnabled)
            {
                if ((level <= this.currentTraceLevel) ||
                    (this.currentTraceLevel == 0)) // This also covers the case of Level == 0.
                {                    
                    // Check if Keyword is enabled
                    if ((keywords == 0) ||
                        (((keywords & this.anyKeywordMask) != 0) &&
                         ((keywords & this.allKeywordMask) == this.allKeywordMask)))
                    {
                        return true;
                    }
                }                
            }           

            return false;
        }

        /// <summary>
        /// IsEventEnabled, method used to test if event is enabled
        /// </summary>
        /// <param name="eventDescriptor">
        /// EventDescriptor for the method to test
        /// </param>
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public bool IsEventEnabled(ref EventDescriptor eventDescriptor)
        {
            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                return UnsafeNativeMethods.EventEnabled(this.traceRegistrationHandle, ref eventDescriptor);
            }

            return false;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.UsePropertiesWhereAppropriate)]
        public static WriteEventErrorCode GetLastWriteEventError()
        {
            return errorCode;
        }

        //
        // Helper function to set the last error on the thread
        //
        static void SetLastError(int error)
        {
            switch (error)
            {
                case UnsafeNativeMethods.ERROR_ARITHMETIC_OVERFLOW:
                case UnsafeNativeMethods.ERROR_MORE_DATA:
                    errorCode = WriteEventErrorCode.EventTooBig;
                    break;
                case UnsafeNativeMethods.ERROR_NOT_ENOUGH_MEMORY:
                    errorCode = WriteEventErrorCode.NoFreeBuffers;
                    break;
            }
        }
        
        /// <summary>
        /// This routine is used by WriteEvent to unbox the object type and
        /// to fill the passed in ETW data descriptor. 
        /// </summary>
        /// <param name="data">argument to be decoded</param>
        /// <param name="dataDescriptor">pointer to the descriptor to be filled</param>
        /// <param name="dataBuffer">storage buffer for storing user data, needed because cant get the address of the object</param>
        /// <returns>null if the object is a basic type other than string. String otherwise</returns>
        
        [SecurityCritical]
        static unsafe string EncodeObject(ref object data, UnsafeNativeMethods.EventData* dataDescriptor, byte* dataBuffer)        
        {
            dataDescriptor->Reserved = 0;

            string sRet = data as string;
            if (sRet != null)
            {
                dataDescriptor->Size = (uint)((sRet.Length + 1) * 2);
                return sRet;
            }

            if (data is IntPtr)
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
                // Everything else is a just a string
                sRet = data.ToString();
                dataDescriptor->Size = (uint)((sRet.Length + 1) * 2);
                return sRet;
            }

            return null;
        }


        /// <summary>
        /// WriteMessageEvent, method to write a string with level and Keyword
        /// </summary>
        /// <param name="level">
        /// Level  to test  
        /// </param>
        /// <param name="Keyword">
        /// Keyword  to test 
        /// </param>        
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public bool WriteMessageEvent(EventTraceActivity eventTraceActivity, string eventMessage, byte eventLevel, long eventKeywords)
        {
            int status = 0;

            if (eventMessage == null)
            {
                throw Fx.Exception.AsError(new ArgumentNullException("eventMessage"));
            }

            if (eventTraceActivity != null)
            {
                SetActivityId(ref eventTraceActivity.ActivityId); 
            }

            if (IsEnabled(eventLevel, eventKeywords))
            {
                if (eventMessage.Length > traceEventMaximumStringSize)
                {
                    errorCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                unsafe
                {
                    fixed (char* pdata = eventMessage)
                    {
                        status = (int)UnsafeNativeMethods.EventWriteString(this.traceRegistrationHandle, eventLevel, eventKeywords, pdata);
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
        /// </summary>
        /// <param name="eventMessage">
        /// Message to log  
        /// </param> 
        [SecurityCritical]
        [Fx.Tag.SecurityNote(Critical = "Accesses security critical code WriteMessageEvent")]
        public bool WriteMessageEvent(EventTraceActivity eventTraceActivity, string eventMessage)
        {
            return WriteMessageEvent(eventTraceActivity, eventMessage, 0, 0);
        }

        /// <summary>
        /// WriteEvent, method to write a parameters with event schema properties
        /// </summary>
        /// <param name="EventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>        
        [SuppressMessage(FxCop.Category.Maintainability, FxCop.Rule.AvoidExcessiveComplexity, Justification = "Performance-critical code")]
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, params object[] eventPayload)
        {
            uint status = 0;

            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                int argCount = 0;

                if (eventTraceActivity != null)
                {
                    SetActivityId(ref eventTraceActivity.ActivityId);
                }

                unsafe
                {
                    if ((eventPayload == null)
                        || (eventPayload.Length == 0)
                        || (eventPayload.Length == 1))
                    {
                        string dataString = null;
                        UnsafeNativeMethods.EventData userData;

                        byte* dataBuffer = stackalloc byte[basicTypeAllocationBufferSize]; // Assume a max of 16 chars for non-string argument

                        userData.Size = 0;
                        if ((eventPayload != null) && (eventPayload.Length != 0))
                        {
                            //
                            // Figure out the type and fill the data descriptor
                            //
                            dataString = EncodeObject(ref eventPayload[0], &userData, dataBuffer);
                            argCount = 1;
                        }

                        if (userData.Size > traceEventMaximumSize)
                        {
                            //
                            // Maximum size of the event payload plus header is 64k
                            //
                            errorCode = WriteEventErrorCode.EventTooBig;
                            return false;
                        }

                        if (dataString != null)
                        {
                            fixed (char* pdata = dataString)
                            {
                                userData.DataPointer = (ulong)pdata;
                                status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint)argCount, &userData);
                            }
                        }
                        else
                        {
                            if (argCount == 0)
                            {
                                status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 0, null);
                            }
                            else
                            {
                                status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint)argCount, &userData);
                            }

                        }
                    }
                    else
                    {

                        argCount = eventPayload.Length;

                        if (argCount > etwMaxNumberArguments)
                        {
                            //
                            //too many arguments to log
                            //
                            throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload",
                                InternalSR.EtwMaxNumberArgumentsExceeded(etwMaxNumberArguments)));
                        }

                        uint totalEventSize = 0;
                        int index;
                        int stringIndex = 0;
                        int[] stringPosition = new int[etwAPIMaxStringCount];
                        string[] dataString = new string[etwAPIMaxStringCount];
                        UnsafeNativeMethods.EventData* userData = stackalloc UnsafeNativeMethods.EventData[argCount];
                        UnsafeNativeMethods.EventData* userDataPtr = (UnsafeNativeMethods.EventData*)userData;
                        byte* dataBuffer = stackalloc byte[basicTypeAllocationBufferSize * argCount]; // Assume 16 chars for non-string argument
                        byte* currentBuffer = dataBuffer;

                        //
                        // The loop below goes through all the arguments and fills in the data 
                        // descriptors. For strings save the location in the dataString array.
                        // Caculates the total size of the event by adding the data descriptor
                        // size value set in EncodeObjec method.
                        //
                        for (index = 0; index < eventPayload.Length; index++)
                        {
                            if (eventPayload[index] != null)
                            {
                                string isString;
                                isString = EncodeObject(ref eventPayload[index], userDataPtr, currentBuffer);
                                currentBuffer += basicTypeAllocationBufferSize;
                                totalEventSize += userDataPtr->Size;
                                userDataPtr++;
                                if (isString != null)
                                {
                                    if (stringIndex < etwAPIMaxStringCount)
                                    {
                                        dataString[stringIndex] = isString;
                                        stringPosition[stringIndex] = index;
                                        stringIndex++;
                                    }
                                    else
                                    {
                                        throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload",
                                            InternalSR.EtwAPIMaxStringCountExceeded(etwAPIMaxStringCount))); 
                                    }
                                }
                            }
                        }

                        if (totalEventSize > traceEventMaximumSize)
                        {
                            errorCode = WriteEventErrorCode.EventTooBig;
                            return false;
                        }

                        //
                        // now fix any string arguments and set the pointer on the data descriptor 
                        //
                        fixed (char* v0 = dataString[0], v1 = dataString[1], v2 = dataString[2], v3 = dataString[3],
                                v4 = dataString[4], v5 = dataString[5], v6 = dataString[6], v7 = dataString[7])
                        {
                            userDataPtr = (UnsafeNativeMethods.EventData*)userData;
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

                            status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint)argCount, userData);
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
        /// WriteEvent, method to write a string with event schema properties
        /// </summary>
        /// <param name="EventDescriptor">
        /// Event Descriptor for this event. 
        /// </param>
        /// <param name="data">
        /// string to log. 
        /// </param> 
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string data)
        {
            uint status = 0;
            //check all strings for null
            data = (data ?? string.Empty);

            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                if (data.Length > traceEventMaximumStringSize)
                {
                    errorCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }

                if (eventTraceActivity != null)
                {
                    SetActivityId(ref eventTraceActivity.ActivityId);
                }

                UnsafeNativeMethods.EventData userData;

                userData.Size = (uint)((data.Length + 1) * 2);
                userData.Reserved = 0;

                unsafe
                {
                    fixed (char* pdata = data)
                    {
                        userData.DataPointer = (ulong)pdata;
                        status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 1, &userData);
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
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal protected bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int dataCount, IntPtr data)
        {
            uint status = 0;

            if (eventTraceActivity != null)
            {
                SetActivityId(ref eventTraceActivity.ActivityId);
            }

            unsafe
            {
                status = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint)dataCount, (UnsafeNativeMethods.EventData*)data);
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
        [SuppressMessage(FxCop.Category.Maintainability, FxCop.Rule.AvoidExcessiveComplexity, Justification = "Performance-critical code")]
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        public bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, params object[] eventPayload)
        {
            // ActivityId is required when writing transfer event
            if (eventTraceActivity == null)
            {
                Fx.Assert(false, "eventTraceActivity should not be null for WriteTransferEvent");
                eventTraceActivity = EventTraceActivity.Empty;
            }

            uint status = 0;
            if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                unsafe
                {
                    if ((eventPayload != null) && (eventPayload.Length != 0))
                    {
                        int argCount = eventPayload.Length;
                        if (argCount > etwMaxNumberArguments)
                        {
                            //
                            //too many arguments to log
                            //
                            throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload",
                                InternalSR.EtwMaxNumberArgumentsExceeded(etwMaxNumberArguments)));
                        }

                        uint totalEventSize = 0;
                        int index;
                        int stringIndex = 0;
                        int[] stringPosition = new int[etwAPIMaxStringCount]; //used to keep the position of strings in the eventPayload parameter
                        string[] dataString = new string[etwAPIMaxStringCount]; // string arrays from the eventPayload parameter
                        UnsafeNativeMethods.EventData* userData = stackalloc UnsafeNativeMethods.EventData[argCount]; // allocation for the data descriptors
                        UnsafeNativeMethods.EventData* userDataPtr = (UnsafeNativeMethods.EventData*)userData;
                        byte* dataBuffer = stackalloc byte[basicTypeAllocationBufferSize * argCount]; // 16 byte for unboxing non-string argument
                        byte* currentBuffer = dataBuffer;

                        //
                        // The loop below goes through all the arguments and fills in the data 
                        // descriptors. For strings save the location in the dataString array.
                        // Caculates the total size of the event by adding the data descriptor
                        // size value set in EncodeObjec method.
                        //
                        for (index = 0; index < eventPayload.Length; index++)
                        {
                            if (eventPayload[index] != null)
                            {
                                string isString;
                                isString = EncodeObject(ref eventPayload[index], userDataPtr, currentBuffer);
                                currentBuffer += basicTypeAllocationBufferSize;
                                totalEventSize += userDataPtr->Size;
                                userDataPtr++;
                                if (isString != null)
                                {
                                    if (stringIndex < etwAPIMaxStringCount)
                                    {
                                        dataString[stringIndex] = isString;
                                        stringPosition[stringIndex] = index;
                                        stringIndex++;
                                    }
                                    else
                                    {                                        
                                        throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload",
                                            InternalSR.EtwAPIMaxStringCountExceeded(etwAPIMaxStringCount)));  
                                    }
                                }
                            }
                        }

                        if (totalEventSize > traceEventMaximumSize)
                        {
                            errorCode = WriteEventErrorCode.EventTooBig;
                            return false;
                        }

                        fixed (char* v0 = dataString[0], v1 = dataString[1], v2 = dataString[2], v3 = dataString[3],
                                v4 = dataString[4], v5 = dataString[5], v6 = dataString[6], v7 = dataString[7])
                        {
                            userDataPtr = (UnsafeNativeMethods.EventData*)userData;
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

                            status = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref eventTraceActivity.ActivityId, ref relatedActivityId, (uint)argCount, userData);
                        }
                    }
                    else
                    {
                        status = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref eventTraceActivity.ActivityId, ref relatedActivityId, 0, null);
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
                        
        [SecurityCritical]
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        protected bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, int dataCount, IntPtr data)
        {
            // ActivityId is required when writing transfer event
            if (eventTraceActivity == null)
            {
                throw Fx.Exception.ArgumentNull("eventTraceActivity");
            }

            uint status = 0;
            unsafe
            {
                status = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle,
                                                ref eventDescriptor,
                                                ref eventTraceActivity.ActivityId,
                                                ref relatedActivityId,
                                                (uint)dataCount,
                                                (UnsafeNativeMethods.EventData*)data);
            }

            if (status != 0)
            {
                SetLastError((int)status);
                return false;
            }
            return true;
        }
                
        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.DoNotIgnoreMethodResults, MessageId = "System.Runtime.Interop.UnsafeNativeMethods.EventActivityIdControl(System.Int32,System.Guid@)")]        
        [SecurityCritical]
        public static void SetActivityId(ref Guid id)
        {
            UnsafeNativeMethods.EventActivityIdControl((int)ActivityControl.EVENT_ACTIVITY_CTRL_SET_ID, ref id);
        }
    }
}
