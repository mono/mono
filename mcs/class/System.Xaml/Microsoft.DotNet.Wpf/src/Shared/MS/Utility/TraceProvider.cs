//---------------------------------------------------------------------------
// File: TraceProvider
//
// A Managed wrapper for Event Tracing for Windows
// Based on TraceEvent.cs found in nt\base\wmi\trace.net
// Provides an internal Avalon API to replace Microsoft.Windows.EventTracing.dll
//
//---------------------------------------------------------------------------
#if !SILVERLIGHTXAML
using System;
using MS.Win32;
using MS.Internal;
using System.Runtime.InteropServices;
using System.Security;
using System.Globalization; //for CultureInfo
using System.Diagnostics;
using MS.Internal.WindowsBase;

#pragma warning disable 1634, 1691  //disable warnings about unknown pragma

#if SYSTEM_XAML
using System.Xaml;
namespace MS.Internal.Xaml
#else
namespace MS.Utility
#endif
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct EventData
    {
        [FieldOffset(0)]
        internal unsafe ulong Ptr;
        [FieldOffset(8)]
        internal uint Size;
        [FieldOffset(12)]
        internal uint Reserved;
    }

    internal abstract class TraceProvider
    {
        protected bool _enabled = false;
        protected EventTrace.Level _level = EventTrace.Level.LogAlways;
        protected EventTrace.Keyword _keywords = (EventTrace.Keyword)0; /* aka Flags */
        protected EventTrace.Keyword _matchAllKeyword = (EventTrace.Keyword)0; /*Vista only*/
        protected SecurityCriticalDataForSet<ulong> _registrationHandle;

        private const int s_basicTypeAllocationBufferSize = sizeof(decimal);
        private const int s_traceEventMaximumSize = 65482; // maximum buffer size is 64k - header size
        private const int s_etwMaxNumberArguments = 32;
        private const int s_etwAPIMaxStringCount = 8; // Arbitrary limit on the number of strings you can emit. This is just to limit allocations so raise it if necessary.
        private const int ErrorEventTooBig = 2;


        [SecurityCritical]
        internal TraceProvider()
        {
            _registrationHandle = new SecurityCriticalDataForSet<ulong>(0);
        }

        [SecurityCritical]
        internal abstract void Register(Guid providerGuid);
        [SecurityCritical]
        internal unsafe abstract uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv);

        internal uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level)
        {
            // Optimization for 0-1 arguments
            return TraceEvent(eventID, keywords, level, (object)null);
        }

        #region Properties and Structs

        //
        // Properties
        //
        internal EventTrace.Keyword Keywords
        {
            get
            {
                return _keywords;
            }
        }

        internal EventTrace.Keyword MatchAllKeywords
        {
            get
            {
                return _matchAllKeyword;
            }
        }

        internal EventTrace.Level Level
        {
            get
            {
                return _level;
            }
        }

        #endregion

        internal bool IsEnabled(EventTrace.Keyword keyword, EventTrace.Level level)
        {
           return _enabled &&
                  (level <= _level) &&
                  (keyword & _keywords) != 0 &&
                  (keyword & _matchAllKeyword) == _matchAllKeyword;
        }

        // Optimization for 0-1 arguments
        [SecurityCritical, SecurityTreatAsSafe]
        internal unsafe uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, object eventData)
        {
            // It is the responsibility of the caller to check that flags/keywords are enabled before calling this method
            Debug.Assert(IsEnabled(keywords, level));

            uint status = 0;
            int argCount = 0;

            EventData userData;
            userData.Size = 0;
            string dataString = null;
            byte* dataBuffer = stackalloc byte[s_basicTypeAllocationBufferSize];

            if (eventData != null)
            {
                dataString = EncodeObject(ref eventData, &userData, dataBuffer);
                argCount = 1;
            }

            if (userData.Size > s_traceEventMaximumSize)
            {
                return ErrorEventTooBig;
            }

            if (dataString != null)
            {
                fixed(char* pdata = dataString)
                {
                    userData.Ptr = (ulong)pdata;
                    status = EventWrite(eventID, keywords, level, argCount, &userData);
                }
            }
            else
            {
                status = EventWrite(eventID, keywords, level, argCount, &userData);
            }

            return status;
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal unsafe uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, params object[] eventPayload)
        {
            // It is the responsibility of the caller to check that flags/keywords are enabled before calling this method
            Debug.Assert(IsEnabled(keywords, level));

            int argCount = eventPayload.Length;

            Debug.Assert(argCount <= s_etwMaxNumberArguments);

            uint totalEventSize = 0;
            int stringIndex = 0;
            int[] stringPosition =  new int[s_etwAPIMaxStringCount];
            string [] dataString = new string[s_etwAPIMaxStringCount];
            EventData* userData = stackalloc EventData[argCount];
            EventData* userDataPtr = userData;
            byte* dataBuffer = stackalloc byte[s_basicTypeAllocationBufferSize * argCount];
            byte* currentBuffer = dataBuffer;


            for (int index = 0; index < argCount; index++)
            {
                if (eventPayload[index] != null)
                {
                    string isString = EncodeObject(ref eventPayload[index], userDataPtr, currentBuffer);
                    currentBuffer += s_basicTypeAllocationBufferSize;
                    totalEventSize = userDataPtr->Size;
                    userDataPtr++;
                    if (isString != null)
                    {
                        Debug.Assert(stringIndex < s_etwAPIMaxStringCount); // need to increase string count or emit fewer strings
                        dataString[stringIndex] = isString;
                        stringPosition[stringIndex] = index;
                        stringIndex++;
                    }
                }
            }

            if (totalEventSize > s_traceEventMaximumSize)
            {
                return ErrorEventTooBig;
            }

            fixed(char* s0 = dataString[0], s1 = dataString[1], s2 = dataString[2], s3 = dataString[3],
                    s4 = dataString[4], s5 = dataString[5], s6 = dataString[6], s7 = dataString[7])
            {
                userDataPtr = userData;
                if (dataString[0] != null)
                {
                    userDataPtr[stringPosition[0]].Ptr = (ulong)s0;
                }
                if (dataString[1] != null)
                {
                    userDataPtr[stringPosition[1]].Ptr = (ulong)s1;
                }
                if (dataString[2] != null)
                {
                    userDataPtr[stringPosition[2]].Ptr = (ulong)s2;
                }
                if (dataString[3] != null)
                {
                    userDataPtr[stringPosition[3]].Ptr = (ulong)s3;
                }
                if (dataString[4] != null)
                {
                    userDataPtr[stringPosition[4]].Ptr = (ulong)s4;
                }
                if (dataString[5] != null)
                {
                    userDataPtr[stringPosition[5]].Ptr = (ulong)s5;
                }
                if (dataString[6] != null)
                {
                    userDataPtr[stringPosition[6]].Ptr = (ulong)s6;
                }
                if (dataString[7] != null)
                {
                    userDataPtr[stringPosition[7]].Ptr = (ulong)s7;
                }

                return EventWrite(eventID, keywords, level, argCount, userData);
            }
        }


        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local intptrPtr of type: IntPtr*" />
        // <UsesUnsafeCode Name="Local intptrPtr of type: Int32*" />
        // <UsesUnsafeCode Name="Local longptr of type: Int64*" />
        // <UsesUnsafeCode Name="Local uintptr of type: UInt32*" />
        // <UsesUnsafeCode Name="Local ulongptr of type: UInt64*" />
        // <UsesUnsafeCode Name="Local charptr of type: Char*" />
        // <UsesUnsafeCode Name="Local byteptr of type: Byte*" />
        // <UsesUnsafeCode Name="Local shortptr of type: Int16*" />
        // <UsesUnsafeCode Name="Local sbyteptr of type: SByte*" />
        // <UsesUnsafeCode Name="Local ushortptr of type: UInt16*" />
        // <UsesUnsafeCode Name="Local floatptr of type: Single*" />
        // <UsesUnsafeCode Name="Local doubleptr of type: Double*" />
        // <UsesUnsafeCode Name="Local boolptr of type: Boolean*" />
        // <UsesUnsafeCode Name="Local guidptr of type: Guid*" />
        // <UsesUnsafeCode Name="Local decimalptr of type: Decimal*" />
        // <UsesUnsafeCode Name="Local booleanptr of type: Boolean*" />
        // <UsesUnsafeCode Name="Parameter dataDescriptor of type: EventData*" />
        // <UsesUnsafeCode Name="Parameter dataBuffer of type: Byte*" />
        // </SecurityKernel>
        [SecurityCritical]
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

            // If the data is an enum we'll convert it to it's underlying type
            Type dataType = data.GetType();
            if (dataType.IsEnum)
            {
                data = Convert.ChangeType(data, Enum.GetUnderlyingType(dataType), CultureInfo.InvariantCulture);
            }

            if (data is IntPtr)
            {
                dataDescriptor->Size = (uint)sizeof(IntPtr);
                IntPtr* intptrPtr = (IntPtr*)dataBuffer;
                *intptrPtr = (IntPtr)data;
                dataDescriptor->Ptr = (ulong)intptrPtr;
            }
            else if (data is int)
            {
                dataDescriptor->Size = (uint)sizeof(int);
                int* intptrPtr = (int*)dataBuffer;
                *intptrPtr = (int)data;
                dataDescriptor->Ptr = (ulong)intptrPtr;
            }
            else if (data is long)
            {
                dataDescriptor->Size = (uint)sizeof(long);
                long* longptr = (long*)dataBuffer;
                *longptr = (long)data;
                dataDescriptor->Ptr = (ulong)longptr;
            }
            else if (data is uint)
            {
                dataDescriptor->Size = (uint)sizeof(uint);
                uint* uintptr = (uint*)dataBuffer;
                *uintptr = (uint)data;
                dataDescriptor->Ptr = (ulong)uintptr;
            }
            else if (data is UInt64)
            {
                dataDescriptor->Size = (uint)sizeof(ulong);
                ulong* ulongptr = (ulong*)dataBuffer;
                *ulongptr = (ulong)data;
                dataDescriptor->Ptr = (ulong)ulongptr;
            }
            else if (data is char)
            {
                dataDescriptor->Size = (uint)sizeof(char);
                char* charptr = (char*)dataBuffer;
                *charptr = (char)data;
                dataDescriptor->Ptr = (ulong)charptr;
            }
            else if (data is byte)
            {
                dataDescriptor->Size = (uint)sizeof(byte);
                byte* byteptr = (byte*)dataBuffer;
                *byteptr = (byte)data;
                dataDescriptor->Ptr = (ulong)byteptr;
            }
            else if (data is short)
            {
                dataDescriptor->Size = (uint)sizeof(short);
                short* shortptr = (short*)dataBuffer;
                *shortptr = (short)data;
                dataDescriptor->Ptr = (ulong)shortptr;
            }
            else if (data is sbyte)
            {
                dataDescriptor->Size = (uint)sizeof(sbyte);
                sbyte* sbyteptr = (sbyte*)dataBuffer;
                *sbyteptr = (sbyte)data;
                dataDescriptor->Ptr = (ulong)sbyteptr;
            }
            else if (data is ushort)
            {
                dataDescriptor->Size = (uint)sizeof(ushort);
                ushort* ushortptr = (ushort*)dataBuffer;
                *ushortptr = (ushort)data;
                dataDescriptor->Ptr = (ulong)ushortptr;
            }
            else if (data is float)
            {
                dataDescriptor->Size = (uint)sizeof(float);
                float* floatptr = (float*)dataBuffer;
                *floatptr = (float)data;
                dataDescriptor->Ptr = (ulong)floatptr;
            }
            else if (data is double)
            {
                dataDescriptor->Size = (uint)sizeof(double);
                double* doubleptr = (double*)dataBuffer;
                *doubleptr = (double)data;
                dataDescriptor->Ptr = (ulong)doubleptr;
            }
            else if (data is bool)
            {
                dataDescriptor->Size = (uint)sizeof(bool);
                bool* boolptr = (bool*)dataBuffer;
                *boolptr = (bool)data;
                dataDescriptor->Ptr = (ulong)boolptr;
            }
            else if (data is Guid)
            {
                dataDescriptor->Size = (uint)sizeof(Guid);
                Guid* guidptr = (Guid*)dataBuffer;
                *guidptr = (Guid)data;
                dataDescriptor->Ptr = (ulong)guidptr;
            }
            else if (data is decimal)
            {
                dataDescriptor->Size = (uint)sizeof(decimal);
                decimal* decimalptr = (decimal*)dataBuffer;
                *decimalptr = (decimal)data;
                dataDescriptor->Ptr = (ulong)decimalptr;
            }
            else if (data is Boolean)
            {
                dataDescriptor->Size = (uint)sizeof(Boolean);
                Boolean* booleanptr = (Boolean*)dataBuffer;
                *booleanptr = (Boolean)data;
                dataDescriptor->Ptr = (ulong)booleanptr;
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
    }

    // XP
    internal sealed class ClassicTraceProvider : TraceProvider
    {
        private ulong _traceHandle = 0;
        
        /// <SecurityNote>
        ///     Critical - Field for critical type ClassicEtw.ControlCallback.
        /// </SecurityNote>
        [SecurityCritical]
        private static ClassicEtw.ControlCallback _etwProc;   // Trace Callback function

        [SecurityCritical]
        internal ClassicTraceProvider()
        {
        }

        //
        // Registers the providerGuid with an inbuilt callback
        //
        ///<SecurityNote>
        /// Critical:  This calls critical code in UnsafeNativeMethods.EtwTrace
        /// and sets critical for set field _registrationHandle and _etwProc
        ///</SecurityNote>
        [SecurityCritical]
        internal override unsafe void Register(Guid providerGuid)
        {
            ulong registrationHandle;
            ClassicEtw.TRACE_GUID_REGISTRATION guidReg;

            Guid dummyGuid = new Guid(0xb4955bf0,
                                      0x3af1,
                                      0x4740,
                                      0xb4,0x75,
                                      0x99,0x05,0x5d,0x3f,0xe9,0xaa);

            _etwProc = new ClassicEtw.ControlCallback(EtwEnableCallback);

            // This dummyGuid is there for ETW backward compat issues and is the same for all downlevel trace providers
            guidReg.Guid = &dummyGuid;
            guidReg.RegHandle = null;

            ClassicEtw.RegisterTraceGuidsW(_etwProc, IntPtr.Zero, ref providerGuid, 1, ref guidReg, null, null, out registrationHandle);
            _registrationHandle.Value = registrationHandle;
        }

        //
        // This callback function is called by ETW to enable or disable this provider
        //
        ///<SecurityNote>
        /// Critical:  This calls critical code in ClassicEtw
        ///</SecurityNote>
        [SecurityCritical]
        private unsafe uint EtwEnableCallback(ClassicEtw.WMIDPREQUESTCODE requestCode, IntPtr context, IntPtr bufferSize, ClassicEtw.WNODE_HEADER* buffer)
        {
            try
            {
                switch (requestCode)
                {
                    case ClassicEtw.WMIDPREQUESTCODE.EnableEvents:
                        _traceHandle = buffer->HistoricalContext;
                        _keywords = (EventTrace.Keyword)ClassicEtw.GetTraceEnableFlags((ulong)buffer->HistoricalContext);
                        _level = (EventTrace.Level)ClassicEtw.GetTraceEnableLevel((ulong)buffer->HistoricalContext);
                        _enabled = true;
                        break;
                    case ClassicEtw.WMIDPREQUESTCODE.DisableEvents:
                        _enabled = false;
                        _traceHandle = 0;
                        _level = EventTrace.Level.LogAlways;
                        _keywords = 0;
                        break;
                    default:
                        _enabled = false;
                        _traceHandle = 0;
                        break;
                }
                return 0;
            }
            catch(Exception e)
            {
                if (CriticalExceptions.IsCriticalException(e))
                {
                   throw;
                }
                else
                {
                    return 0;
                }
            }
        }

        ///<SecurityNote>
        /// Critical:  This calls critical code in EtwTrace
        /// TreatAsSafe: the registration handle this passes in to UnregisterTraceGuids
        /// was generated by the ETW unmanaged API and can't be tampered with from our side
        ///</SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        ~ClassicTraceProvider()
        {
            #pragma warning suppress 6031  //presharp suppression
            ClassicEtw.UnregisterTraceGuids(_registrationHandle.Value);
        }

        // pack the argv data and emit the event using TraceEvent
        [SecurityCritical]
        internal unsafe override uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv)
        {
            ClassicEtw.EVENT_HEADER header;
            header.Header.ClientContext = 0;
            header.Header.Flags = ClassicEtw.WNODE_FLAG_TRACED_GUID | ClassicEtw.WNODE_FLAG_USE_MOF_PTR;
            header.Header.Guid = EventTrace.GetGuidForEvent(eventID);
            header.Header.Level = (byte)level;
            header.Header.Type = (byte)EventTrace.GetOpcodeForEvent(eventID);
            header.Header.Version = (ushort)EventTrace.GetVersionForEvent(eventID);
            // Extra copy on XP to move argv to the end of the EVENT_HEADER
            EventData* eventData = &header.Data;

            if (argc > ClassicEtw.MAX_MOF_FIELDS)
            {
                // Data will be lost on XP
                argc = ClassicEtw.MAX_MOF_FIELDS;
            }

            header.Header.Size = (ushort) (argc * sizeof(EventData) + 48);
            for (int x = 0; x < argc; x++)
            {
                eventData[x].Ptr = argv[x].Ptr;
                eventData[x].Size = argv[x].Size;
            }

            return ClassicEtw.TraceEvent(_traceHandle, &header);
        }
    }

    // Vista and above
    internal class ManifestTraceProvider : TraceProvider
    {
        /// <SecurityNote>
        ///     Critical - Field for critical type ManifestEtw.EtwEnableCallback.
        /// </SecurityNote>
        [SecurityCritical]
        private static ManifestEtw.EtwEnableCallback _etwEnabledCallback;

        [SecurityCritical]
        internal ManifestTraceProvider()
        {
        }

        /// <SecurityNote>
        ///     Critical - Sets critical _etwEnabledCallback field
        ///              - Calls critical ManifestEtw.EventRegister
        /// </SecurityNote>
        [SecurityCritical]
        internal unsafe override void Register(Guid providerGuid)
        {
            _etwEnabledCallback =new ManifestEtw.EtwEnableCallback(EtwEnableCallback);
            ulong registrationHandle = 0;
            ManifestEtw.EventRegister(ref providerGuid, _etwEnabledCallback, null, ref registrationHandle);
            _registrationHandle.Value = registrationHandle;
        }

        /// <SecurityNote>
        ///     Critical - Accepts untrusted pointer argument
        /// </SecurityNote>
        [SecurityCritical]
        private unsafe void EtwEnableCallback(ref Guid sourceId, int isEnabled, byte level, long matchAnyKeywords, long matchAllKeywords, ManifestEtw.EVENT_FILTER_DESCRIPTOR* filterData, void* callbackContext)
        {
            _enabled = isEnabled > 0;
            _level = (EventTrace.Level)level;
            _keywords = (EventTrace.Keyword) matchAnyKeywords;
            _matchAllKeyword = (EventTrace.Keyword) matchAllKeywords;

            // todo: parse data from EVENT_FILTER_DESCRIPTOR - see CLR EventProvider::GetDataFromController
        }

        /// <SecurityNote>
        ///     Critical - Calls critical ManifestEtw.EventUnregister
        ///     TreatAsSafe: Only critical code can create this resource, 
        ///     and no input parameters are accepted to this method.
        ///     In fact, this method is not directly callable, but only as
        ///     part of the GC, and the GC ensures that no other rooted
        ///     objects are holding a reference.  This method clears the
        ///     handle and skips future calls to unregister the event, which
        ///     protects against resurrection.
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        ~ManifestTraceProvider()
        {
            if(_registrationHandle.Value != 0)
            {
                try
                {
                    ManifestEtw.EventUnregister(_registrationHandle.Value);
                }
                finally
                {
                    _registrationHandle.Value = 0;
                }
            }
        }

        /// <SecurityNote>
        ///     Critical - Accepts untrusted pointer argument
        /// </SecurityNote>
        [SecurityCritical]
        internal unsafe override uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv)
        {
            ManifestEtw.EventDescriptor eventDescriptor;
            eventDescriptor.Id = (ushort) eventID;
            eventDescriptor.Version = EventTrace.GetVersionForEvent(eventID);
            eventDescriptor.Channel = 0x10; // Since Channel isn't supported on XP we only use a single default channel.
            eventDescriptor.Level = (byte)level;
            eventDescriptor.Opcode = EventTrace.GetOpcodeForEvent(eventID);
            eventDescriptor.Task = EventTrace.GetTaskForEvent(eventID);
            eventDescriptor.Keywords = (long)keywords;
            if (argc == 0)
            {
                argv = null;
            }

            return ManifestEtw.EventWrite(_registrationHandle.Value, ref eventDescriptor, (uint)argc, argv);
        }

    }
}

#endif
