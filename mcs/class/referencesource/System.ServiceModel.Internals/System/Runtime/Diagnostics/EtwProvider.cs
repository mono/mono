//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Text;
    using System.Security;
    using System.Diagnostics;
    using System.Runtime.Interop;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;

    sealed class EtwProvider : DiagnosticsEventProvider
    {
        Action invokeControllerCallback;
        bool end2EndActivityTracingEnabled;

        [Fx.Tag.SecurityNote(Critical = "Calling the base critical c'tor")]
        [SecurityCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts)]
        internal EtwProvider(Guid id)
            : base(id)
        {
        }
        
        internal Action ControllerCallBack
        {
            get
            {
                return this.invokeControllerCallback;
            }
            set
            {
                this.invokeControllerCallback = value;
            }
        }

        internal bool IsEnd2EndActivityTracingEnabled
        {
            get { return this.end2EndActivityTracingEnabled; }
        }

        protected override void OnControllerCommand()
        {
            this.end2EndActivityTracingEnabled = false;
            if (this.invokeControllerCallback != null)
            {
                this.invokeControllerCallback();
            }
        }

        internal void SetEnd2EndActivityTracingEnabled(bool isEnd2EndActivityTracingEnabled)
        {
            this.end2EndActivityTracingEnabled = isEnd2EndActivityTracingEnabled;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, string value2, string value3)
        {
            const int argumentCount = 3;
            bool status = true;

            //check all strings for null            
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);

            fixed (char* string1Bytes = value2, string2Bytes = value3)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)string1Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string2Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, string value1, string value2)
        {
            const int argumentCount = 2;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);

                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);

                status = WriteTransferEvent(ref eventDescriptor, eventTraceActivity, relatedActivityId, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2)
        {
            const int argumentCount = 2;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);            

            fixed (char* string1Bytes = value1, string2Bytes = value2)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3)
        {
            const int argumentCount = 3;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);

                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4)
        {
            const int argumentCount = 4;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5)
        {
            const int argumentCount = 5;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);

                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
                
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6)
        {
            const int argumentCount = 6;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
           
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
           
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
           
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
           
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
           
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);               

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7)
        {
            const int argumentCount = 7;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8)
        {
            const int argumentCount = 8;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8, string value9)
        {
            const int argumentCount = 9;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8, string9Bytes = value9)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string9Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10)
        {
            const int argumentCount = 10;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8, string9Bytes = value9, string10Bytes = value10)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string9Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string10Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11)
        {
            const int argumentCount = 11;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8, string9Bytes = value9, string10Bytes = value10, string11Bytes = value11)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string9Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string10Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string11Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12)
        {
            const int argumentCount = 12;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8, string9Bytes = value9, string10Bytes = value10, string11Bytes = value11, string12Bytes = value12)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                               
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string9Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string10Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string11Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
            
                eventDataPtr[11].DataPointer = (ulong)string12Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            const int argumentCount = 13;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value2 = (value2 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            value13 = (value13 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value2, string3Bytes = value3, string4Bytes = value4, string5Bytes = value5, string6Bytes = value6,
            string7Bytes = value7, string8Bytes = value8, string9Bytes = value9, string10Bytes = value10, string11Bytes = value11, string12Bytes = value12, string13Bytes = value13)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;
                                
                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);
            
                eventDataPtr[1].DataPointer = (ulong)string2Bytes;
                eventDataPtr[1].Size = (uint)(value2.Length + 1) * sizeof(char);
            
                eventDataPtr[2].DataPointer = (ulong)string3Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);
            
                eventDataPtr[3].DataPointer = (ulong)string4Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string5Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string6Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string7Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string8Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string9Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string10Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string11Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
            
                eventDataPtr[11].DataPointer = (ulong)string12Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
            
                eventDataPtr[12].DataPointer = (ulong)string13Bytes;
                eventDataPtr[12].Size = (uint)(value13.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1)
        {
            const int argumentCount = 1;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(int));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1, int value2)
        {
            const int argumentCount = 2;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(int));

                eventDataPtr[1].DataPointer = (UInt64)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(int));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1, int value2, int value3)
        {
            const int argumentCount = 3;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(int));

                eventDataPtr[1].DataPointer = (UInt64)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(int));

                eventDataPtr[2].DataPointer = (UInt64)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(int));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1)
        {
            const int argumentCount = 1;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(long));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1, long value2)
        {
            const int argumentCount = 2;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(long));

                eventDataPtr[1].DataPointer = (UInt64)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1, long value2, long value3)
        {
            const int argumentCount = 3;
            bool status = true;

            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (UInt64)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(long));

                eventDataPtr[1].DataPointer = (UInt64)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (UInt64)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        // The following methods are designed for ETW Tracking Participant
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10,
            string value11, string value12, string value13, string value14, string value15)
        {
            const int argumentCount = 15;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            value13 = (value13 ?? string.Empty);
            value14 = (value14 ?? string.Empty);
            value15 = (value15 ?? string.Empty);

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, string5Bytes = value8, string6Bytes = value9,
            string7Bytes = value10, string8Bytes = value11, string9Bytes = value12, string10Bytes = value13, string11Bytes = value14, string12Bytes = value15)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;                

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
                
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
           
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
           
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
           
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
           
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string7Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string8Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
            
                eventDataPtr[11].DataPointer = (ulong)string9Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
            
                eventDataPtr[12].DataPointer = (ulong)string10Bytes;
                eventDataPtr[12].Size = (uint)(value13.Length + 1) * sizeof(char);
            
                eventDataPtr[13].DataPointer = (ulong)string11Bytes;
                eventDataPtr[13].Size = (uint)(value14.Length + 1) * sizeof(char);
           
                eventDataPtr[14].DataPointer = (ulong)string12Bytes;
                eventDataPtr[14].Size = (uint)(value15.Length + 1) * sizeof(char);                
                
                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;

        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12,
            bool value13, string value14, string value15, string value16, string value17)
        {
            const int argumentCount = 17;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            //value13 is not string            
            value14 = (value14 ?? string.Empty);
            value15 = (value15 ?? string.Empty);
            value16 = (value16 ?? string.Empty);
            value17 = (value17 ?? string.Empty);

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, string5Bytes = value8, string6Bytes = value9,
            string7Bytes = value10, string8Bytes = value11, string9Bytes = value12, string10Bytes = value14, string11Bytes = value15, 
            string12Bytes = value16, string13Bytes = value17)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;                

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
               
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
           
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
           
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
           
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
           
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
           
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string7Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string8Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
           
                eventDataPtr[11].DataPointer = (ulong)string9Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
               
                eventDataPtr[12].DataPointer = (ulong)(&value13);
                eventDataPtr[12].Size = (uint)(sizeof(bool));
               
                eventDataPtr[13].DataPointer = (ulong)string10Bytes;
                eventDataPtr[13].Size = (uint)(value14.Length + 1) * sizeof(char);
           
                eventDataPtr[14].DataPointer = (ulong)string11Bytes;
                eventDataPtr[14].Size = (uint)(value15.Length + 1) * sizeof(char);
           
                eventDataPtr[15].DataPointer = (ulong)string12Bytes;
                eventDataPtr[15].Size = (uint)(value16.Length + 1) * sizeof(char);
           
                eventDataPtr[16].DataPointer = (ulong)string13Bytes;
                eventDataPtr[16].Size = (uint)(value17.Length + 1) * sizeof(char);
               
                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;

        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9)
        {
            const int argumentCount = 9;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);            

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, 
                string5Bytes = value8, string6Bytes = value9)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;                

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
               
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
           
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
           
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
           
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
           
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
           
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11)
        {
            const int argumentCount = 11;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);            

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, string5Bytes = value8,
                string6Bytes = value9, string7Bytes = value10, string8Bytes = value11)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;                

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
                
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
            
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string7Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string8Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);                

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            const int argumentCount = 13;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            value13 = (value13 ?? string.Empty);            

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, string5Bytes = value8,
                string6Bytes = value9, string7Bytes = value10, string8Bytes = value11, string9Bytes = value12, string10Bytes = value13)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
                
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
           
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
            
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
           
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
            
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
            
                eventDataPtr[9].DataPointer = (ulong)string7Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string8Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
           
                eventDataPtr[11].DataPointer = (ulong)string9Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
            
                eventDataPtr[12].DataPointer = (ulong)string10Bytes;
                eventDataPtr[12].Size = (uint)(value13.Length + 1) * sizeof(char);
                
                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13, string value14)
        {
            const int argumentCount = 14;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            value5 = (value5 ?? string.Empty);
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            value13 = (value13 ?? string.Empty);
            value14 = (value14 ?? string.Empty);

            fixed (char* string1Bytes = value4, string2Bytes = value5, string3Bytes = value6, string4Bytes = value7, string5Bytes = value8,
                string6Bytes = value9, string7Bytes = value10, string8Bytes = value11, string9Bytes = value12, string10Bytes = value13, string11Bytes = value14)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;                

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
                
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
           
                eventDataPtr[4].DataPointer = (ulong)string2Bytes;
                eventDataPtr[4].Size = (uint)(value5.Length + 1) * sizeof(char);
           
                eventDataPtr[5].DataPointer = (ulong)string3Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
            
                eventDataPtr[6].DataPointer = (ulong)string4Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
            
                eventDataPtr[7].DataPointer = (ulong)string5Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
           
                eventDataPtr[8].DataPointer = (ulong)string6Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
           
                eventDataPtr[9].DataPointer = (ulong)string7Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
            
                eventDataPtr[10].DataPointer = (ulong)string8Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
            
                eventDataPtr[11].DataPointer = (ulong)string9Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
            
                eventDataPtr[12].DataPointer = (ulong)string10Bytes;
                eventDataPtr[12].Size = (uint)(value13.Length + 1) * sizeof(char);
            
                eventDataPtr[13].DataPointer = (ulong)string11Bytes;
                eventDataPtr[13].Size = (uint)(value14.Length + 1) * sizeof(char);

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4,
            Guid value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            const int argumentCount = 13;
            bool status = true;

            //check all strings for null           
            value4 = (value4 ?? string.Empty);
            //value5 is not string            
            value6 = (value6 ?? string.Empty);
            value7 = (value7 ?? string.Empty);
            value8 = (value8 ?? string.Empty);
            value9 = (value9 ?? string.Empty);
            value10 = (value10 ?? string.Empty);
            value11 = (value11 ?? string.Empty);
            value12 = (value12 ?? string.Empty);
            value13 = (value13 ?? string.Empty);            

            fixed (char* string1Bytes = value4, string2Bytes = value6, string3Bytes = value7, string4Bytes = value8,
                string5Bytes = value9, string6Bytes = value10, string7Bytes = value11, string8Bytes = value12, string9Bytes = value13)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)(&value1);
                eventDataPtr[0].Size = (uint)(sizeof(Guid));

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)(&value3);
                eventDataPtr[2].Size = (uint)(sizeof(long));
               
                eventDataPtr[3].DataPointer = (ulong)string1Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);
               
                eventDataPtr[4].DataPointer = (ulong)(&value5);
                eventDataPtr[4].Size = (uint)(sizeof(Guid));
               
                eventDataPtr[5].DataPointer = (ulong)string2Bytes;
                eventDataPtr[5].Size = (uint)(value6.Length + 1) * sizeof(char);
           
                eventDataPtr[6].DataPointer = (ulong)string3Bytes;
                eventDataPtr[6].Size = (uint)(value7.Length + 1) * sizeof(char);
           
                eventDataPtr[7].DataPointer = (ulong)string4Bytes;
                eventDataPtr[7].Size = (uint)(value8.Length + 1) * sizeof(char);
           
                eventDataPtr[8].DataPointer = (ulong)string5Bytes;
                eventDataPtr[8].Size = (uint)(value9.Length + 1) * sizeof(char);
           
                eventDataPtr[9].DataPointer = (ulong)string6Bytes;
                eventDataPtr[9].Size = (uint)(value10.Length + 1) * sizeof(char);
           
                eventDataPtr[10].DataPointer = (ulong)string7Bytes;
                eventDataPtr[10].Size = (uint)(value11.Length + 1) * sizeof(char);
           
                eventDataPtr[11].DataPointer = (ulong)string8Bytes;
                eventDataPtr[11].Size = (uint)(value12.Length + 1) * sizeof(char);
           
                eventDataPtr[12].DataPointer = (ulong)string9Bytes;
                eventDataPtr[12].Size = (uint)(value13.Length + 1) * sizeof(char);
                               
                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }

        //used by app trace OperationCompleted
        [Fx.Tag.SecurityNote(Critical = "Calling Unsafe code; usage of EventDescriptor, which is protected by a LinkDemand")]
        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, long value2, string value3, string value4)
        {
            const int argumentCount = 4;
            bool status = true;

            //check all strings for null
            value1 = (value1 ?? string.Empty);
            value3 = (value3 ?? string.Empty);
            value4 = (value4 ?? string.Empty);

            fixed (char* string1Bytes = value1, string2Bytes = value3, string3Bytes = value4)
            {
                byte* eventData = stackalloc byte[sizeof(UnsafeNativeMethods.EventData) * argumentCount];
                UnsafeNativeMethods.EventData* eventDataPtr = (UnsafeNativeMethods.EventData*)eventData;

                eventDataPtr[0].DataPointer = (ulong)string1Bytes;
                eventDataPtr[0].Size = (uint)(value1.Length + 1) * sizeof(char);

                eventDataPtr[1].DataPointer = (ulong)(&value2);
                eventDataPtr[1].Size = (uint)(sizeof(long));

                eventDataPtr[2].DataPointer = (ulong)string2Bytes;
                eventDataPtr[2].Size = (uint)(value3.Length + 1) * sizeof(char);

                eventDataPtr[3].DataPointer = (ulong)string3Bytes;
                eventDataPtr[3].Size = (uint)(value4.Length + 1) * sizeof(char);

                status = WriteEvent(ref eventDescriptor, eventTraceActivity, argumentCount, (IntPtr)(eventData));
            }

            return status;
        }
    }
}
