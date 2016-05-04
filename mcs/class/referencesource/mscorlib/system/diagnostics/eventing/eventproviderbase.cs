// Copyright (c) Microsoft Corporation.  All rights reserved
// This program uses code hyperlinks available as part of the HyperAddin Visual Studio plug-in.
// It is available from http://www.codeplex.com/hyperAddin 
#define FEATURE_MANAGED_ETW

#if !FEATURE_CORECLR || FEATURE_NETCORE
#define FEATURE_ACTIVITYSAMPLING
#endif // !FEATURE_CORECLR || FEATURE_NETCORE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Diagnostics.Tracing
{
    /// <summary>
    /// This class is meant to be inherited by a user eventSource (which provides specific events and then
    /// calls code:EventSource.WriteEvent to log them).
    /// 
    /// sealed class MinimalEventSource : EventSource
    /// {
    ///     * public void Load(long ImageBase, string Name) { WriteEvent(1, ImageBase, Name); }
    ///     * public void Unload(long ImageBase) { WriteEvent(2, ImageBase); }
    ///     * private MinimalEventSource() {}
    /// }
    /// 
    /// This functionaity is sufficient for many users.   When more control is needed over the ETW manifest
    /// that is created, that can be done by adding [Event] attributes on the  methods.
    /// 
    /// Finally for very advanced EventSources, it is possible to intercept the commands being given to the
    /// eventSource and change what filtering is done (or cause actions to be performed by the eventSource (eg
    /// dumping a data structure).  
    /// 
    /// The eventSources can be turned on with Window ETW controllers (eg logman), immediately.  It is also
    /// possible to control and intercept the data dispatcher programatically.  We code:EventListener for
    /// more.      
    /// </summary>
    public class EventSource : IDisposable
    {
        /// <summary>
        /// The human-friendly name of the eventSource.  It defaults to the simple name of the class
        /// </summary>
        public string Name { get { return m_name; } }
        /// <summary>
        /// Every eventSource is assigned a GUID to uniquely identify it to the system. 
        /// </summary>
        public Guid Guid { get { return m_guid; } }

        /// <summary>
        /// Returns true if the eventSource has been enabled at all.
        /// </summary>
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
#if !FEATURE_CORECLR
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public bool IsEnabled()
        {
            return m_eventSourceEnabled;
        }
        /// <summary>
        /// Returns true if events with >= 'level' and have one of 'keywords' set are enabled. 
        /// 
        /// Note that the result of this function only an approximiation on whether a particular
        /// event is active or not. It is only meant to be used as way of avoiding expensive
        /// computation for logging when logging is not on, therefore it sometimes returns false
        //  positives (but is always accurate when returning false).  EventSources are free to 
        /// have additional filtering.    
        /// </summary>
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        public bool IsEnabled(EventLevel level, EventKeywords keywords)
        {
            if (!m_eventSourceEnabled)
                return false;
            if (m_level != 0 && m_level < level)
                return false;
            if (m_matchAnyKeyword != 0 && (keywords & m_matchAnyKeyword) == 0)
                return false;

#if !FEATURE_ACTIVITYSAMPLING

            return true;

#else // FEATURE_ACTIVITYSAMPLING

            return true;

#if OPTIMIZE_IS_ENABLED
            //================================================================================
            // 2013/03/06 - The code below is a possible optimization for IsEnabled(level, kwd)
            //    in case activity tracing/sampling is enabled. The added complexity of this
            //    code however weighs against having it "on" until we know it's really needed.
            //    For now we'll have this #ifdef-ed out in case we see evidence this is needed.
            //================================================================================            

            // At this point we believe the event is enabled, however we now need to check
            // if we filter because of activity 

            // Optimization, all activity filters also register a delegate here, so if there 
            // is no delegate, we know there are no activity filters, which means that there
            // is no additional filtering, which means that we can return true immediately.  
            if (s_activityDying == null)
                return true;

            // if there's at least one legacy ETW listener we can't filter this
            if (m_legacySessions != null && m_legacySessions.Count > 0)
                return true;

            // if any event ID that triggers a new activity, or "transfers" activities
            // is covered by 'keywords' we can't filter this
            if (((long)keywords & m_keywordTriggers) != 0)
                return true;

            // See if all listeners have activity filters that would block the event.
            for (int perEventSourceSessionId = 0; perEventSourceSessionId < SessionMask.MAX; ++perEventSourceSessionId)
            {
                EtwSession etwSession = m_etwSessionIdMap[perEventSourceSessionId];
                if (etwSession == null)
                    continue;

                ActivityFilter activityFilter = etwSession.m_activityFilter;
                if (activityFilter == null || 
                    ActivityFilter.GetFilter(activityFilter, this) == null)
                {
                    // No activity filter for ETW, if event is active for ETW, we can't filter.  
                    for (int i = 0; i < m_eventData.Length; i++)
                        if (m_eventData[i].EnabledForETW)
                            return true;
                }
                else if (ActivityFilter.IsCurrentActivityActive(activityFilter))
                    return true;
            }

            // for regular event listeners
            var curDispatcher = m_Dispatchers;
            while (curDispatcher != null)
            {
                ActivityFilter activityFilter = curDispatcher.m_Listener.m_activityFilter;
                if (activityFilter == null)
                {
                    // See if any event is enabled.   
                    for (int i = 0; i < curDispatcher.m_EventEnabled.Length; i++)
                        if (curDispatcher.m_EventEnabled[i])
                            return true;
                }
                else if (ActivityFilter.IsCurrentActivityActive(activityFilter))
                    return true;
                curDispatcher = curDispatcher.m_Next;
            }

            // Every listener has an activity filter that is blocking writing the event, 
            // thus the event is not enabled.  
            return false;
#endif // OPTIMIZE_IS_ENABLED

#endif // FEATURE_ACTIVITYSAMPLING
        }

        // Manifest support 
        /// <summary>
        /// Returns the GUID that uniquely identifies the eventSource defined by 'eventSourceType'.  
        /// This API allows you to compute this without actually creating an instance of the EventSource.   
        /// It only needs to reflect over the type.  
        /// </summary>
        public static Guid GetGuid(Type eventSourceType)
        {
            if (eventSourceType == null)
                throw new ArgumentNullException("eventSourceType");
            Contract.EndContractBlock();

            EventSourceAttribute attrib = (EventSourceAttribute)GetCustomAttributeHelper(eventSourceType, typeof(EventSourceAttribute));
            string name = eventSourceType.Name;
            if (attrib != null)
            {
                if (attrib.Guid != null)
                {
                    Guid g = Guid.Empty;
                    if(Guid.TryParse(attrib.Guid, out g)) 
                        return g;
                }

                if (attrib.Name != null)
                    name = attrib.Name;
            }

            if (name == null)
                throw new ArgumentException("eventSourceType", Environment.GetResourceString("Argument_InvalidTypeName"));

            return GenerateGuidFromName(name.ToUpperInvariant());       // Make it case insensitive.  
        }
        /// <summary>
        /// Returns the official ETW Provider name for the eventSource defined by 'eventSourceType'.  
        /// This API allows you to compute this without actually creating an instance of the EventSource.   
        /// It only needs to reflect over the type.  
        /// </summary>
        public static string GetName(Type eventSourceType)
        {
            if (eventSourceType == null)
                throw new ArgumentNullException("eventSourceType");
            Contract.EndContractBlock();

            EventSourceAttribute attrib = (EventSourceAttribute)GetCustomAttributeHelper(eventSourceType, typeof(EventSourceAttribute));
            if (attrib != null && attrib.Name != null)
                return attrib.Name;

            return eventSourceType.Name;
        }
        /// <summary>
        /// Returns a string of the XML manifest associated with the eventSourceType. The scheme for this XML is
        /// documented at in EventManifest Schema http://msdn.microsoft.com/en-us/library/aa384043(VS.85).aspx
        /// </summary>
        /// <param name="assemblyPathForManifest">The manifest XML fragment contains the string name of the DLL name in
        /// which it is embeded.  This parameter spcifies what name will be used</param>
        /// <returns>The XML data string</returns>
        public static string GenerateManifest(Type eventSourceType, string assemblyPathToIncludeInManifest)
        {
            if (eventSourceType == null)
                throw new ArgumentNullException("eventSourceType");
            Contract.EndContractBlock();

            byte[] manifestBytes = EventSource.CreateManifestAndDescriptors(eventSourceType, assemblyPathToIncludeInManifest, null);
            return Encoding.UTF8.GetString(manifestBytes);
        }

        // EventListener support
        /// <summary>
        /// returns a list (IEnumerable) of all sources in the appdomain).  EventListners typically need this.  
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EventSource> GetSources()
        {
            var ret = new List<EventSource>();
            lock (EventListener.EventListenersLock)
            {
                foreach (WeakReference eventSourceRef in EventListener.s_EventSources)
                {
                    EventSource eventSource = eventSourceRef.Target as EventSource;
                    if (eventSource != null)
                        ret.Add(eventSource);
                }
            }
            return ret;
        }

        /// <summary>
        /// Send a command to a particular EventSource identified by 'eventSource'
        /// 
        /// Calling this routine simply forwards the command to the EventSource.OnEventCommand
        /// callback.  What the EventSource does with the command and its arguments are from that point
        /// EventSource-specific.  
        /// 
        /// The eventSource is passed the EventListener that issued the command along with the command and
        /// arguments.  The contract is that to the extent possible the eventSource should not affect other
        /// EventListeners (eg filtering events), however sometimes this simply is not possible (if the
        /// command was to provoke a GC, or a System flush etc).   
        /// </summary>
        public static void SendCommand(EventSource eventSource, EventCommand command, IDictionary<string, string> commandArguments)
        {
            if (eventSource == null)
                throw new ArgumentNullException("eventSource");

            // User-defined EventCommands should not conflict with the reserved commands.
            if ((int)command <= (int)EventCommand.Update && (int)command != (int)EventCommand.SendManifest)
                throw new ArgumentException("Invalid command value.", "command");

            eventSource.SendCommand(null, 0, 0, command, true, EventLevel.LogAlways, EventKeywords.None, commandArguments);
        }

        // ActivityID support (see also WriteEventWithRelatedActivityIdCore)
        /// <summary>
        /// When a thread starts work that is on behalf of 'something else' (typically another 
        /// thread or network request) it should mark the thread as working on that other work.
        /// This API marks the current thread as working on activity 'activityID'. This API
        /// should be used when the caller knows the thread's current activity (the one being
        /// overwritten) has completed. Otherwise, callers should prefer the overload that
        /// return the oldActivityThatWillContinue (below).
        /// 
        /// All events created with the EventSource on this thread are also tagged with the 
        /// activity ID of the thread. 
        /// 
        /// It is common, and good practice after setting the thread to an activity to log an event
        /// with a 'start' opcode to indicate that precise time/thread where the new activity 
        /// started.
        /// </summary>
        /// <param name="activityId">A Guid that represents the new activity with which to mark 
        /// the current thread</param>
        [System.Security.SecuritySafeCritical]
        public static void SetCurrentThreadActivityId(Guid activityId)
        {
#if FEATURE_ACTIVITYSAMPLING
            Guid newId = activityId;
#endif // FEATURE_ACTIVITYSAMPLING
            // We ignore errors to keep with the convention that EventSources do not throw errors.
            // Note we can't access m_throwOnWrites because this is a static method.  
            if (UnsafeNativeMethods.ManifestEtw.EventActivityIdControl(
                UnsafeNativeMethods.ManifestEtw.ActivityControl.EVENT_ACTIVITY_CTRL_GET_SET_ID,
                ref activityId) == 0)
            {
#if FEATURE_ACTIVITYSAMPLING
                var activityDying = s_activityDying;
                if (activityDying != null && newId != activityId)
                {
                    if (activityId == Guid.Empty)
                    {
                        activityId = FallbackActivityId;
                    }
                    // OutputDebugString(string.Format("Activity dying: {0} -> {1}", activityId, newId));
                    activityDying(activityId);     // This is actually the OLD activity ID.  
                }
#endif // FEATURE_ACTIVITYSAMPLING
            }
        }

        /// <summary>
        /// When a thread starts work that is on behalf of 'something else' (typically another 
        /// thread or network request) it should mark the thread as working on that other work.
        /// This API marks the current thread as working on activity 'activityID'. It returns 
        /// whatever activity the thread was previously marked with. There is a  convention that
        /// callers can assume that callees restore this activity mark before the callee returns. 
        /// To encourage this this API returns the old activity, so that it can be restored later.
        /// 
        /// All events created with the EventSource on this thread are also tagged with the 
        /// activity ID of the thread. 
        /// 
        /// It is common, and good practice after setting the thread to an activity to log an event
        /// with a 'start' opcode to indicate that precise time/thread where the new activity 
        /// started.
        /// </summary>
        /// <param name="activityId">A Guid that represents the new activity with which to mark 
        /// the current thread</param>
        /// <param name="oldActivityThatWillContinue">The Guid that represents the current activity  
        /// which will continue at some point in the future, on the current thread</param>
        [System.Security.SecuritySafeCritical]
        public static void SetCurrentThreadActivityId(Guid activityId, out Guid oldActivityThatWillContinue)
        {
            oldActivityThatWillContinue = activityId;
            // We ignore errors to keep with the convention that EventSources do not throw errors.
            // Note we can't access m_throwOnWrites because this is a static method.  
            UnsafeNativeMethods.ManifestEtw.EventActivityIdControl(
                UnsafeNativeMethods.ManifestEtw.ActivityControl.EVENT_ACTIVITY_CTRL_GET_SET_ID,
                    ref oldActivityThatWillContinue);
            // We don't call the activityDying callback here because the caller has declared that
            // it is not dying.  

        }
        public static Guid CurrentThreadActivityId
        {
            [System.Security.SecurityCritical]
            get
            {
                // We ignore errors to keep with the convention that EventSources do not throw 
                // errors. Note we can't access m_throwOnWrites because this is a static method.
                Guid retVal = new Guid();
                UnsafeNativeMethods.ManifestEtw.EventActivityIdControl(
                    UnsafeNativeMethods.ManifestEtw.ActivityControl.EVENT_ACTIVITY_CTRL_GET_ID,
                    ref retVal);
                return retVal;
            }
        }

        /// <summary>
        /// This property allows EventSource code to appropriately handle as "different" 
        /// activities started on different threads that have not had an activity created on them.
        /// </summary>
        internal static Guid InternalCurrentThreadActivityId
        {
            [System.Security.SecurityCritical]
            get
            {
                Guid retval = CurrentThreadActivityId;
                if (retval == Guid.Empty)
                {
                    retval = FallbackActivityId;
                }
                return retval;
            }
        }

        internal static Guid FallbackActivityId
        {
            [System.Security.SecurityCritical]
            get
            {
#pragma warning disable 612, 618
                // Managed thread IDs are more aggressively re-used than native thread IDs,
                // so we'll use the latter...
                return new Guid((uint) AppDomain.GetCurrentThreadId(), 
                                (ushort) s_currentPid, (ushort) (s_currentPid >> 16), 
                                0x94, 0x1b, 0x87, 0xd5, 0xa6, 0x5c, 0x36, 0x64);
#pragma warning restore 612, 618
            }
        }

        // Error APIs.  (We don't throw by default, but you can probe for status)
        /// <summary>
        /// Because
        /// 
        ///     1) Logging is often optional and thus should not generate fatal errors (exceptions)
        ///     2) EventSources are often initialized in class constructors (which propagate exceptions poorly)
        ///     
        /// The event source constructor does not throw exceptions.  Instead we remember any exception that 
        /// was generated (it is also logged to Trace.WriteLine).
        /// </summary>
        public Exception ConstructionException { get { return m_constructionException; } }

        /// <summary>
        /// Displays thew name and GUID for the eventSoruce for debugging purposes.  
        /// </summary>
        public override string ToString() { return Environment.GetResourceString("EventSource_ToString", Name, Guid); }

        #region protected
        /// <summary>
        /// This is the consturctor that most users will use to create their eventSource.   It takes 
        /// no parameters.  The ETW provider name and GUID of the EventSource are determined by the EventSource 
        /// custom attribute (so you can determine these things declaratively).   If the GUID for the eventSource
        /// is not specified in the EventSourceAttribute (recommended), it is Generated by hashing the name.
        /// If the ETW provider name of the EventSource is not given, the name of the EventSource class is used as
        /// the ETW provider name.
        /// </summary>
        protected EventSource()
            : this(false)
        {
        }

        /// <summary>
        /// By default calling the 'WriteEvent' methods do NOT throw on errors (they silently discard the event).  
        /// This is because in most cases users assume logging is not 'precious' and do NOT wish to have logging falures
        /// crash the program.   However for those applications where logging is 'precious' and if it fails the caller
        /// wishes to react, setting 'throwOnEventWriteErrors' will cause an exception to be thrown if WriteEvent
        /// fails.   Note the fact that EventWrite succeeds does not necessarily mean that the event reached its destination
        /// only that operation of writing it did not fail.   
        /// </summary>
        protected EventSource(bool throwOnEventWriteErrors)
        {
            m_throwOnEventWriteErrors = throwOnEventWriteErrors;
            try
            {
                Contract.Assert(m_lastCommandException == null);
                var myType = this.GetType();
                Initialize(GetGuid(myType), GetName(myType));
                m_constructionException = m_lastCommandException;
            }
            catch (Exception e)
            {
                Contract.Assert(m_eventData == null && m_eventSourceEnabled == false);
                ReportOutOfBandMessage("ERROR: Exception during construction of EventSource " + Name + ": "
                                + e.Message, false);
                m_eventSourceEnabled = false;       // This is insurance, it should still be off.    
                if (m_lastCommandException != null)
                    m_constructionException = m_lastCommandException;
                else
                    m_constructionException = e;
            }
        }

        // FrameworkEventSource is on the startup path for the framework, so we have this internal overload that it can use
        // to prevent the working set hit from looking at the custom attributes on the type to get the Guid.
        internal EventSource(Guid eventSourceGuid, string eventSourceName)
        {
            Initialize(eventSourceGuid, eventSourceName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "guid")]
        [SecuritySafeCritical]
        private void Initialize(Guid eventSourceGuid, string eventSourceName)
        {
            if (eventSourceGuid == Guid.Empty)
            {
                throw new ArgumentException(Environment.GetResourceString("EventSource_NeedGuid"));
            }

            if (eventSourceName == null)
            {
                throw new ArgumentException(Environment.GetResourceString("EventSource_NeedName"));
            }

            m_name = eventSourceName;
            m_guid = eventSourceGuid;
#if FEATURE_ACTIVITYSAMPLING
            m_curLiveSessions = new SessionMask(0);
            m_etwSessionIdMap = new EtwSession[SessionMask.MAX];
#endif // FEATURE_ACTIVITYSAMPLING

#if FEATURE_MANAGED_ETW
            m_provider = new OverideEventProvider(this);

            try
            {
                m_provider.Register(eventSourceGuid);
            }
            catch (ArgumentException)
            {
                // Failed to register.  Don't crash the app, just don't write events to ETW.
                m_provider = null;
            }
#endif
            // Add the eventSource to the global (weak) list.  This also sets m_id, which is the
            // index in the list. 
            EventListener.AddEventSource(this);

            // We are logically completely initialized at this point.  
            m_completelyInited = true;

            // report any possible errors
            ReportOutOfBandMessage(null, true);

#if FEATURE_ACTIVITYSAMPLING
            // we cue sending sampling info here based on whether we had to defer sending
            // the manifest
            // note: we do *not* send sampling info to any EventListeners because
            // the following common code pattern would cause an AV:
            // class MyEventSource: EventSource
            // {
            //    public static EventSource Log; 
            // }
            // class MyEventListener: EventListener
            // {
            //    protected override void OnEventWritten(...)
            //    { MyEventSource.Log.anything; } <-- AV, as the static Log was not set yet
            // }
            if (m_eventSourceEnabled && m_deferedSendManifest)
                ReportActivitySamplingInfo(null, m_curLiveSessions);
#endif // FEATURE_ACTIVITYSAMPLING

            // If we are active and we have not sent our manifest, do so now.  
            if (m_eventSourceEnabled && m_deferedSendManifest)
                SendManifest(m_rawManifest);
        }

        /// <summary>
        /// This method is called when the eventSource is updated by the controller.  
        /// </summary>
        protected virtual void OnEventCommand(EventCommandEventArgs command) { }

        internal void WriteString(string msg, SessionMask m)
        {
            if (m_eventSourceEnabled)
            {
                WriteEventString(0, (long) m.ToEventKeywords(), msg);
                WriteStringToAllListeners(msg);
            }
        }
        internal void WriteString(string msg)
        {
            WriteString(msg, SessionMask.All);
        }

        internal void WriteStringToListener(EventListener listener, string msg, SessionMask m)
        {
            Contract.Assert(listener == null || (uint)m == (uint)SessionMask.FromId(0));
            
            if (m_eventSourceEnabled)
            {
                if (listener == null)
                {
                    WriteEventString(0, (long) m.ToEventKeywords(), msg);
                }
                else
                {
                    List<object> arg = new List<object>();
                    arg.Add(msg);
                    EventWrittenEventArgs eventCallbackArgs = new EventWrittenEventArgs(this);
                    eventCallbackArgs.EventId = 0;
                    eventCallbackArgs.Payload = new ReadOnlyCollection<object>(arg);
                    listener.OnEventWritten(eventCallbackArgs);
                }
            }
        }

        // optimized for common signatures (no args)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId)
        {
            WriteEventCore(eventId, 0, null);
        }

        // optimized for common signatures (ints)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, int arg1)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[1];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 4;
                WriteEventCore(eventId, 1, descrs);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, int arg1, int arg2)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[2];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 4;
                descrs[1].DataPointer = (IntPtr)(&arg2);
                descrs[1].Size = 4;
                WriteEventCore(eventId, 2, descrs);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, int arg1, int arg2, int arg3)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 4;
                descrs[1].DataPointer = (IntPtr)(&arg2);
                descrs[1].Size = 4;
                descrs[2].DataPointer = (IntPtr)(&arg3);
                descrs[2].Size = 4;
                WriteEventCore(eventId, 3, descrs);
            }
        }

        // optimized for common signatures (longs)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, long arg1)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[1];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 8;
                WriteEventCore(eventId, 1, descrs);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, long arg1, long arg2)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[2];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 8;
                descrs[1].DataPointer = (IntPtr)(&arg2);
                descrs[1].Size = 8;
                WriteEventCore(eventId, 2, descrs);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, long arg1, long arg2, long arg3)
        {
            if (m_eventSourceEnabled)
            {
                EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                descrs[0].DataPointer = (IntPtr)(&arg1);
                descrs[0].Size = 8;
                descrs[1].DataPointer = (IntPtr)(&arg2);
                descrs[1].Size = 8;
                descrs[2].DataPointer = (IntPtr)(&arg3);
                descrs[2].Size = 8;
                WriteEventCore(eventId, 3, descrs);
            }
        }

        // optimized for common signatures (strings)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                fixed (char* string1Bytes = arg1)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[1];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    WriteEventCore(eventId, 1, descrs);
                }
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1, string arg2)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                if (arg2 == null) arg2 = "";
                fixed (char* string1Bytes = arg1)
                fixed (char* string2Bytes = arg2)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[2];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    descrs[1].DataPointer = (IntPtr)string2Bytes;
                    descrs[1].Size = ((arg2.Length + 1) * 2);
                    WriteEventCore(eventId, 2, descrs);
                }
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1, string arg2, string arg3)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                if (arg2 == null) arg2 = "";
                if (arg3 == null) arg3 = "";
                fixed (char* string1Bytes = arg1)
                fixed (char* string2Bytes = arg2)
                fixed (char* string3Bytes = arg3)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    descrs[1].DataPointer = (IntPtr)string2Bytes;
                    descrs[1].Size = ((arg2.Length + 1) * 2);
                    descrs[2].DataPointer = (IntPtr)string3Bytes;
                    descrs[2].Size = ((arg3.Length + 1) * 2);
                    WriteEventCore(eventId, 3, descrs);
                }
            }
        }

        // optimized for common signatures (string and ints)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1, int arg2)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                fixed (char* string1Bytes = arg1)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[2];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    WriteEventCore(eventId, 2, descrs);
                }
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1, int arg2, int arg3)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                fixed (char* string1Bytes = arg1)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    descrs[2].DataPointer = (IntPtr)(&arg3);
                    descrs[2].Size = 4;
                    WriteEventCore(eventId, 3, descrs);
                }
            }
        }

        // optimized for common signatures (string and longs)
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, string arg1, long arg2)
        {
            if (m_eventSourceEnabled)
            {
                if (arg1 == null) arg1 = "";
                fixed (char* string1Bytes = arg1)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[2];
                    descrs[0].DataPointer = (IntPtr)string1Bytes;
                    descrs[0].Size = ((arg1.Length + 1) * 2);
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 8;
                    WriteEventCore(eventId, 2, descrs);
                }
            }
        }

        protected internal struct EventData 
        {
            public IntPtr DataPointer { get { return (IntPtr)m_Ptr; } set { m_Ptr = (long)value; } }
            public int Size { get { return m_Size; } set { m_Size = value; } }

            #region private
            //Important, we pass this structure directly to the Win32 EventWrite API, so this structure must be layed out exactly
            // the way EventWrite wants it.  
            private long m_Ptr;
            private int m_Size;
            internal int m_Reserved;	// Used to pad the size to match the Win32 API
            #endregion
        }

        /// <summary>
        /// This routine allows you to create efficient WriteEvent helpers, however the code that you use to
        /// do this while straightfoward is unsafe.  See the bodies of the WriteEvent helpers above for its use.     
        /// </summary>
        [SecurityCritical]
        [CLSCompliant(false)]
        protected unsafe void WriteEventCore(int eventId, int eventDataCount, EventSource.EventData* data)
        {
            WriteEventWithRelatedActivityIdCore(eventId, null, eventDataCount, data);
        }

        /// <summary>
        /// This routine allows you to create efficient WriteEventCreatingChildActivity helpers, however the code 
        /// that you use to do this while straightfoward is unsafe.  See the bodies of the WriteEvent helpers above 
        /// for its use.   The only difference is that you pass the ChildAcivityID from caller through to this API
        /// </summary>
        [SecurityCritical]
        [CLSCompliant(false)]
        protected unsafe void WriteEventWithRelatedActivityIdCore(int eventId, Guid* childActivityID, int eventDataCount, EventSource.EventData* data)
        {
            if (m_eventSourceEnabled)
            {
                Contract.Assert(m_eventData != null);  // You must have initialized this if you enabled the source.
                if (childActivityID != null)
                    ValidateEventOpcodeForTransfer(ref m_eventData[eventId]);

#if FEATURE_MANAGED_ETW
                if (m_eventData[eventId].EnabledForETW)
                {
#if FEATURE_ACTIVITYSAMPLING
                    // this code should be kept in [....] with WriteEventVarargs().
                    SessionMask etwSessions = SessionMask.All;
                    // only compute etwSessions if there are *any* ETW filters enabled...
                    if ((ulong)m_curLiveSessions != 0)
                        etwSessions = GetEtwSessionMask(eventId, childActivityID);
                    // OutputDebugString(string.Format("{0}.WriteEvent(id {1}) -> to sessions {2:x}", 
                    //                   m_name, m_eventData[eventId].Name, (ulong) etwSessions));

                    if ((ulong)etwSessions != 0 || m_legacySessions != null && m_legacySessions.Count > 0)
                    {
                        if (etwSessions.IsEqualOrSupersetOf(m_curLiveSessions))
                        {
                            // OutputDebugString(string.Format("  (1) id {0}, kwd {1:x}", 
                            //                   m_eventData[eventId].Name, m_eventData[eventId].Descriptor.Keywords));
                            // by default the Descriptor.Keyword will have the perEventSourceSessionId bit 
                            // mask set to 0x0f so, when all ETW sessions want the event we don't need to 
                            // synthesize a new one
                            if (!m_provider.WriteEvent(ref m_eventData[eventId].Descriptor, childActivityID, eventDataCount, (IntPtr)data))
                                ThrowEventSourceException();
                        }
                        else
                        {
                            long origKwd = (long)((ulong) m_eventData[eventId].Descriptor.Keywords & ~(SessionMask.All.ToEventKeywords()));
                            // OutputDebugString(string.Format("  (2) id {0}, kwd {1:x}", 
                            //                   m_eventData[eventId].Name, etwSessions.ToEventKeywords() | (ulong) origKwd));
                            // only some of the ETW sessions will receive this event. Synthesize a new
                            // Descriptor whose Keywords field will have the appropriate bits set.
                            // etwSessions might be 0, if there are legacy ETW listeners that want this event
                            var desc = new System.Diagnostics.Tracing.EventDescriptor(
                                m_eventData[eventId].Descriptor.EventId,
                                m_eventData[eventId].Descriptor.Version,
                                m_eventData[eventId].Descriptor.Channel,
                                m_eventData[eventId].Descriptor.Level,
                                m_eventData[eventId].Descriptor.Opcode,
                                m_eventData[eventId].Descriptor.Task,
                                (long) etwSessions.ToEventKeywords() | origKwd);

                            if (!m_provider.WriteEvent(ref desc, childActivityID, eventDataCount, (IntPtr)data))
                                ThrowEventSourceException();
                        }
                    }
#else
                    if (!m_provider.WriteEvent(ref m_eventData[eventId].Descriptor, childActivityID, eventDataCount, (IntPtr)data))
                        ThrowEventSourceException();
#endif // FEATURE_ACTIVITYSAMPLING
                }
#endif
                if (m_Dispatchers != null && m_eventData[eventId].EnabledForAnyListener)
                    WriteToAllListeners(eventId, childActivityID, eventDataCount, data);
            }
        }

        // fallback varags helpers. 
        /// <summary>
        /// This is the varargs helper for writing an event.   It does create an array and box all the arguments so it is
        /// relatively inefficient and should only be used for relatively rare events (e.g. less than 100 / sec).  If you
        /// rates are fast than that you should be used WriteEventCore to create fast helpers for your particular method
        /// signature.   Even if you use this for rare evnets, this call should be guarded by a 'IsEnabled()' check so that 
        /// the varargs call is not made when the EventSource is not active.  
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        protected unsafe void WriteEvent(int eventId, params object[] args)
        {
            WriteEventVarargs(eventId, null, args);
        }

        /// <summary>
        /// This is the varargs helper for writing an event which also creates a child activity.  It is completely ----ygous
        /// to cooresponding WriteEvent (they share implementation).   It does create an array and box all the arguments so it is
        /// relatively inefficient and should only be used for relatively rare events (e.g. less than 100 / sec).  If you
        /// rates are fast than that you should be used WriteEventCore to create fast helpers for your particular method
        /// signature.   Even if you use this for rare evnets, this call should be guarded by a 'IsEnabled()' check so that 
        /// the varargs call is not made when the EventSource is not active. 
        /// </summary>
        [SecuritySafeCritical]
        protected unsafe void WriteEventWithRelatedActivityId(int eventId, Guid childActivityID, params object[] args)
        {
            WriteEventVarargs(eventId, &childActivityID, args);
        }

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes of an EventSource.
        /// </summary>
        /// <remarks>
        /// Called from Dispose() with disposing=true, and from the finalizer (~MeasurementBlock) with disposing=false.
        /// Guidelines:
        /// 1. We may be called more than once: do nothing after the first call.
        /// 2. Avoid throwing exceptions if disposing is false, i.e. if we're being finalized.
        /// </remarks>
        /// <param name="disposing">True if called from Dispose(), false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
#if FEATURE_MANAGED_ETW
                if (m_provider != null)
                {
                    m_provider.Dispose();
                    m_provider = null;
                }
#endif
            }
        }
        ~EventSource()
        {
            this.Dispose(false);
        }
        #endregion

        #region private
        private static Guid GenerateGuidFromName(string name)
        {
            // The algorithm below is following the guidance of http://www.ietf.org/rfc/rfc4122.txt
            // Create a blob containing a 16 byte number representing the namespace
            // followed by the unicode bytes in the name.  
            var bytes = new byte[name.Length * 2 + 16];
            uint namespace1 = 0x482C2DB2;
            uint namespace2 = 0xC39047c8;
            uint namespace3 = 0x87F81A15;
            uint namespace4 = 0xBFC130FB;
            // Write the bytes most-significant byte first.  
            for (int i = 3; 0 <= i; --i)
            {
                bytes[i] = (byte)namespace1;
                namespace1 >>= 8;
                bytes[i + 4] = (byte)namespace2;
                namespace2 >>= 8;
                bytes[i + 8] = (byte)namespace3;
                namespace3 >>= 8;
                bytes[i + 12] = (byte)namespace4;
                namespace4 >>= 8;
            }
            // Write out  the name, most significant byte first
            for (int i = 0; i < name.Length; i++)
            {
                bytes[2 * i + 16 + 1] = (byte)name[i];
                bytes[2 * i + 16] = (byte)(name[i] >> 8);
            }

            // Compute the Sha1 hash 
            var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] hash = sha1.ComputeHash(bytes);

            // Create a GUID out of the first 16 bytes of the hash (SHA-1 create a 20 byte hash)
            int a = (((((hash[3] << 8) + hash[2]) << 8) + hash[1]) << 8) + hash[0];
            short b = (short)((hash[5] << 8) + hash[4]);
            short c = (short)((hash[7] << 8) + hash[6]);

            c = (short)((c & 0x0FFF) | 0x5000);   // Set high 4 bits of octet 7 to 5, as per RFC 4122
            Guid guid = new Guid(a, b, c, hash[8], hash[9], hash[10], hash[11], hash[12], hash[13], hash[14], hash[15]);
            return guid;
        }

        [SecurityCritical]
        private unsafe object DecodeObject(int eventId, int parameterId, IntPtr dataPointer)
        {
            Type dataType = m_eventData[eventId].Parameters[parameterId].ParameterType;

        Again:
            if (dataType == typeof(IntPtr))
            {
                return *((IntPtr*)dataPointer);
            }
            else if (dataType == typeof(int))
            {
                return *((int*)dataPointer);
            }
            else if (dataType == typeof(uint))
            {
                return *((uint*)dataPointer);
            }
            else if (dataType == typeof(long))
            {
                return *((long*)dataPointer);
            }
            else if (dataType == typeof(ulong))
            {
                return *((ulong*)dataPointer);
            }
            else if (dataType == typeof(byte))
            {
                return *((byte*)dataPointer);
            }
            else if (dataType == typeof(sbyte))
            {
                return *((sbyte*)dataPointer);
            }
            else if (dataType == typeof(short))
            {
                return *((short*)dataPointer);
            }
            else if (dataType == typeof(ushort))
            {
                return *((ushort*)dataPointer);
            }
            else if (dataType == typeof(float))
            {
                return *((float*)dataPointer);
            }
            else if (dataType == typeof(double))
            {
                return *((double*)dataPointer);
            }
            else if (dataType == typeof(decimal))
            {
                return *((decimal*)dataPointer);
            }
            else if (dataType == typeof(bool))
            {
                // The manifest defines a bool as a 32bit type (WIN32 BOOL), not 1 bit as CLR Does.
                if (*((int*)dataPointer) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (dataType == typeof(Guid))
            {
                return *((Guid*)dataPointer);
            }
            else if (dataType == typeof(char))
            {
                return *((char*)dataPointer);
            }
            else if (dataType == typeof(DateTime))
            {
                long dateTimeTicks = *((long*)dataPointer);
                return DateTime.FromFileTimeUtc(dateTimeTicks);
            }
            else
            {
                if (dataType.IsEnum)
                {
                    dataType = Enum.GetUnderlyingType(dataType);
                    goto Again;
                }

                // Everything else is marshaled as a string.
                // ETW strings are NULL-terminated, so marshal everything up to the first
                // null in the string.
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(dataPointer);
            }
        }

        // Finds the Dispatcher (which holds the filtering state), for a given dispatcher for the current
        // eventSource).  
        private EventDispatcher GetDispatcher(EventListener listener)
        {
            EventDispatcher dispatcher = m_Dispatchers;
            while (dispatcher != null)
            {
                if (dispatcher.m_Listener == listener)
                    return dispatcher;
                dispatcher = dispatcher.m_Next;
            }
            return dispatcher;
        }

        [SecurityCritical]
        private unsafe void WriteEventVarargs(int eventId, Guid* childActivityID, object[] args)
        {
            if (m_eventSourceEnabled)
            {
                Contract.Assert(m_eventData != null);  // You must have initialized this if you enabled the source.  
                if (childActivityID != null)
                    ValidateEventOpcodeForTransfer(ref m_eventData[eventId]);

#if FEATURE_MANAGED_ETW
                if (m_eventData[eventId].EnabledForETW)
                {
#if FEATURE_ACTIVITYSAMPLING
                    // this code should be kept in [....] with WriteEventWithRelatedActivityIdCore().
                    SessionMask etwSessions = SessionMask.All;
                    // only compute etwSessions if there are *any* ETW filters enabled...
                    if ((ulong)m_curLiveSessions != 0)
                        etwSessions = GetEtwSessionMask(eventId, childActivityID);

                    if ((ulong)etwSessions != 0 || m_legacySessions != null && m_legacySessions.Count > 0)
                    {
                        if (etwSessions.IsEqualOrSupersetOf(m_curLiveSessions))
                        {
                            // by default the Descriptor.Keyword will have the perEventSourceSessionId bit 
                            // mask set to 0x0f so, when all ETW sessions want the event we don't need to 
                            // synthesize a new one
                            if (!m_provider.WriteEvent(ref m_eventData[eventId].Descriptor, childActivityID, args))
                                ThrowEventSourceException();
                        }
                        else
                        {
                            long origKwd = (long)((ulong) m_eventData[eventId].Descriptor.Keywords & ~(SessionMask.All.ToEventKeywords()));
                            // only some of the ETW sessions will receive this event. Synthesize a new
                            // Descriptor whose Keywords field will have the appropriate bits set.
                            var desc = new System.Diagnostics.Tracing.EventDescriptor(
                                m_eventData[eventId].Descriptor.EventId,
                                m_eventData[eventId].Descriptor.Version,
                                m_eventData[eventId].Descriptor.Channel,
                                m_eventData[eventId].Descriptor.Level,
                                m_eventData[eventId].Descriptor.Opcode,
                                m_eventData[eventId].Descriptor.Task,
                                (long)(ulong)etwSessions | origKwd);

                            if (!m_provider.WriteEvent(ref desc, childActivityID, args))
                                ThrowEventSourceException();
                        }
                    }
#else
                    if (!m_provider.WriteEvent(ref m_eventData[eventId].Descriptor, childActivityID, args))
                        ThrowEventSourceException();
#endif // FEATURE_ACTIVITYSAMPLING
                }
#endif // FEATURE_MANAGED_ETW
                if (m_Dispatchers != null && m_eventData[eventId].EnabledForAnyListener)
                    WriteToAllListeners(eventId, childActivityID, args);
            }
        }

        [SecurityCritical]
        unsafe private void WriteToAllListeners(int eventId, Guid* childActivityID, int eventDataCount, EventSource.EventData* data)
        {
            object[] args = new object[eventDataCount];

            for (int i = 0; i < eventDataCount; i++)
                args[i] = DecodeObject(eventId, i, data[i].DataPointer);
            WriteToAllListeners(eventId, childActivityID, args);
        }

        // helper for writing to all EventListeners attached the current eventSource.  
        [SecurityCritical]
        unsafe private void WriteToAllListeners(int eventId, Guid* childActivityID, params object[] args)
        {
            EventWrittenEventArgs eventCallbackArgs = new EventWrittenEventArgs(this);
            eventCallbackArgs.EventId = eventId;
            if (childActivityID != null)
                eventCallbackArgs.RelatedActivityId = *childActivityID;
            eventCallbackArgs.Payload = new ReadOnlyCollection<object>(new List<object>(args));

            Exception lastThrownException = null;
            for (EventDispatcher dispatcher = m_Dispatchers; dispatcher != null; dispatcher = dispatcher.m_Next)
            {
                if (dispatcher.m_EventEnabled[eventId])
                {
#if FEATURE_ACTIVITYSAMPLING
                    var activityFilter = dispatcher.m_Listener.m_activityFilter;
                    // order below is important as PassesActivityFilter will "flow" active activities
                    // even when the current EventSource doesn't have filtering enabled. This allows
                    // interesting activities to be updated so that sources that do sample can get
                    // accurate data
                    if (activityFilter == null ||
                        ActivityFilter.PassesActivityFilter(activityFilter, childActivityID, 
                                                            m_eventData[eventId].TriggersActivityTracking > 0, 
                                                            this, eventId) ||
                        !dispatcher.m_activityFilteringEnabled)
#endif // FEATURE_ACTIVITYSAMPLING
                    {
                        try
                        {
                            dispatcher.m_Listener.OnEventWritten(eventCallbackArgs);
                        }
                        catch (Exception e)
                        {
                            ReportOutOfBandMessage("ERROR: Exception during EventSource.OnEventWritten: "
                                 + e.Message, false);
                            lastThrownException = e;
                        }
                    }
                }
            }

            if (lastThrownException != null)
            {
                throw new EventSourceException(lastThrownException);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        private unsafe void WriteEventString(EventLevel level, long keywords, string msg)
        {
            if (m_eventSourceEnabled)
            {
                if (m_provider != null && !m_provider.WriteEventString(level, keywords, msg))
                {
                    ThrowEventSourceException();
                }
            }
        }

        private void WriteStringToAllListeners(string msg)
        {
            EventWrittenEventArgs eventCallbackArgs = new EventWrittenEventArgs(this);
            eventCallbackArgs.EventId = 0;
            eventCallbackArgs.Message = msg;

            Exception lastThrownException = null;
            for (EventDispatcher dispatcher = m_Dispatchers; dispatcher != null; dispatcher = dispatcher.m_Next)
            {
                // if there's *any* enabled event on the dispatcher we'll write out the string
                // otherwise we'll treat the listener as disabled and skip it
                bool dispatcherEnabled = false;
                for (int evtId = 0; evtId < dispatcher.m_EventEnabled.Length; ++evtId)
                {
                    if (dispatcher.m_EventEnabled[evtId])
                    {
                        dispatcherEnabled = true;
                        break;
                    }
                }
                try
                {
                    if (dispatcherEnabled)
                        dispatcher.m_Listener.OnEventWritten(eventCallbackArgs);
                }
                catch (Exception e)
                {
                    lastThrownException = e;
                }
            }
            if (lastThrownException != null)
            {
                throw new EventSourceException(lastThrownException);
            }
        }

#if FEATURE_ACTIVITYSAMPLING
        [SecurityCritical]
        unsafe private SessionMask GetEtwSessionMask(int eventId, Guid* childActivityID)
        {
            SessionMask etwSessions = new SessionMask();

            for (int i = 0; i < SessionMask.MAX; ++i)
            {
                EtwSession etwSession = m_etwSessionIdMap[i];
                if (etwSession != null)
                {
                    ActivityFilter activityFilter = etwSession.m_activityFilter;
                    // PassesActivityFilter() will flow "interesting" activities, so make sure
                    // to perform this test first, before ORing with ~m_activityFilteringForETWEnabled
                    // (note: the first test for !m_activityFilteringForETWEnabled[i] ensures we
                    //  do not fire events indiscriminately, when no filters are specified, but only 
                    //  if, in addition, the session did not also enable ActivitySampling)
                    if (activityFilter == null && !m_activityFilteringForETWEnabled[i] || 
                        activityFilter != null && 
                            ActivityFilter.PassesActivityFilter(activityFilter, childActivityID, 
                                m_eventData[eventId].TriggersActivityTracking > 0, this, eventId) ||
                        !m_activityFilteringForETWEnabled[i])
                    {
                        etwSessions[i] = true;
                    }
                }
            }
            // flow "interesting" activities for all legacy sessions in which there's some 
            // level of activity tracing enabled (even other EventSources)
            if (m_legacySessions != null && m_legacySessions.Count > 0 && 
                (EventOpcode)m_eventData[eventId].Descriptor.Opcode == EventOpcode.Send)
            {
                // only calculate InternalCurrentThreadActivityId once
                Guid *pCurrentActivityId = null;
                Guid currentActivityId;
                foreach (var legacyEtwSession in m_legacySessions)
                {
                    if (legacyEtwSession == null)
                        continue;

                    ActivityFilter activityFilter = legacyEtwSession.m_activityFilter;
                    if (activityFilter != null)
                    {
                        if (pCurrentActivityId == null)
                        {
                            currentActivityId = InternalCurrentThreadActivityId;
                            pCurrentActivityId = &currentActivityId;
                        }
                        ActivityFilter.FlowActivityIfNeeded(activityFilter, pCurrentActivityId, childActivityID);
                    }
                }
            }

            return etwSessions;
        }
#endif // FEATURE_ACTIVITYSAMPLING

        /// <summary>
        /// Returns true if 'eventNum' is enabled if you only consider the level and matchAnyKeyword filters.
        /// It is possible that eventSources turn off the event based on additional filtering criteria.  
        /// </summary>
        private bool IsEnabledByDefault(int eventNum, bool enable, EventLevel currentLevel, EventKeywords currentMatchAnyKeyword)
        {
            if (!enable)
                return false;

            EventLevel eventLevel = (EventLevel)m_eventData[eventNum].Descriptor.Level;
            EventKeywords eventKeywords = (EventKeywords)((ulong)m_eventData[eventNum].Descriptor.Keywords & (~(SessionMask.All.ToEventKeywords())));

            if ((eventLevel <= currentLevel) || (currentLevel == 0))
            {
                if ((eventKeywords == 0) || ((eventKeywords & currentMatchAnyKeyword) != 0))
                    return true;
            }
            return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void ThrowEventSourceException()
        {
            // Only throw an error if we asked for them.  
            if (m_throwOnEventWriteErrors)
            {
                // 
                switch (EventProvider.GetLastWriteEventError())
                {
                    case EventProvider.WriteEventErrorCode.EventTooBig:
                        ReportOutOfBandMessage("EventSourceException: "+Environment.GetResourceString("EventSource_EventTooBig"), true);
                        throw new EventSourceException(Environment.GetResourceString("EventSource_EventTooBig"));
                    case EventProvider.WriteEventErrorCode.NoFreeBuffers:
                        ReportOutOfBandMessage("EventSourceException: "+Environment.GetResourceString("EventSource_NoFreeBuffers"), true);
                        throw new EventSourceException(Environment.GetResourceString("EventSource_NoFreeBuffers"));
                    case EventProvider.WriteEventErrorCode.NullInput:
                        ReportOutOfBandMessage("EventSourceException: "+Environment.GetResourceString("EventSource_NullInput"), true);
                        throw new EventSourceException(Environment.GetResourceString("EventSource_NullInput"));
                    case EventProvider.WriteEventErrorCode.TooManyArgs:
                        ReportOutOfBandMessage("EventSourceException: "+Environment.GetResourceString("EventSource_TooManyArgs"), true);
                        throw new EventSourceException(Environment.GetResourceString("EventSource_TooManyArgs"));
                    default:
                        ReportOutOfBandMessage("EventSourceException", true);
                        throw new EventSourceException();
                }
            }
        }

        private void ValidateEventOpcodeForTransfer(ref EventMetadata eventData)
        {
            if ((EventOpcode)eventData.Descriptor.Opcode != EventOpcode.Send &&
                (EventOpcode)eventData.Descriptor.Opcode != EventOpcode.Receive)
            {
                ThrowEventSourceException();
            }
        }

#if FEATURE_MANAGED_ETW
        /// <summary>
        /// This class lets us hook the 'OnEventCommand' from the eventSource.  
        /// </summary>
        private class OverideEventProvider : EventProvider
        {
            public OverideEventProvider(EventSource eventSource)
            {
                this.m_eventSource = eventSource;
            }
            protected override void OnControllerCommand(ControllerCommand command, IDictionary<string, string> arguments, 
                                                              int perEventSourceSessionId, int etwSessionId)
            {
                // We use null to represent the ETW EventListener.  
                EventListener listener = null;
                m_eventSource.SendCommand(listener, perEventSourceSessionId, etwSessionId, 
                                          (EventCommand)command, IsEnabled(), Level, MatchAnyKeyword, arguments);
            }
            private EventSource m_eventSource;
        }
#endif

        /// <summary>
        /// Used to hold all the static information about an event.  This includes everything in the event
        /// descriptor as well as some stuff we added specifically for EventSource. see the
        /// code:m_eventData for where we use this.  
        /// </summary>
        internal struct EventMetadata
        {
            public System.Diagnostics.Tracing.EventDescriptor Descriptor;
            public bool EnabledForAnyListener;      // true if any dispatcher has this event turned on
            public bool EnabledForETW;              // is this event on for the OS ETW data dispatcher?
            public byte TriggersActivityTracking;   // count of listeners that marked this event as trigger for start of activity logging.
            public string Name;                     // the name of the event
            public string Message;                  // If the event has a message associated with it, this is it.  
            public ParameterInfo[] Parameters;      // 
        };

        // This is the internal entry point that code:EventListeners call when wanting to send a command to a
        // eventSource. The logic is as follows
        // 
        // * if Command == Update
        //     * perEventSourceSessionId specifies the per-provider ETW session ID that the command applies 
        //         to (if listener != null)
        //         perEventSourceSessionId = 0 - reserved for EventListeners
        //         perEventSourceSessionId = 1..SessionMask.MAX - reserved for activity tracing aware ETW sessions
        //                  perEventSourceSessionId-1 represents the bit in the reserved field (bits 44..47) in 
        //                  Keywords that identifies the session
        //         perEventSourceSessionId = SessionMask.MAX+1 - reserved for legacy ETW sessions; these are 
        //                  discriminated by etwSessionId
        //     * etwSessionId specifies a machine-wide ETW session ID; this allows correlation of
        //         activity tracing across different providers (which might have different sessionIds
        //         for the same ETW session)
        //     * enable, level, matchAnyKeywords are used to set a default for all events for the
        //         eventSource.  In particular, if 'enabled' is false, 'level' and
        //         'matchAnyKeywords' are not used.  
        //     * OnEventCommand is invoked, which may cause calls to
        //         code:EventSource.EnableEventForDispatcher which may cause changes in the filtering
        //         depending on the logic in that routine.
        // * else (command != Update)
        //     * Simply call OnEventCommand. The expectation is that filtering is NOT changed.
        //     * The 'enabled' 'level', matchAnyKeyword' arguments are ignored (must be true, 0, 0).  
        // 
        // dispatcher == null has special meaning. It is the 'ETW' dispatcher.
        internal void SendCommand(EventListener listener, int perEventSourceSessionId, int etwSessionId,
                                  EventCommand command, bool enable, 
                                  EventLevel level, EventKeywords matchAnyKeyword, 
                                  IDictionary<string, string> commandArguments)
        {
            m_lastCommandException = null;
            bool shouldReport = (perEventSourceSessionId > 0) && (perEventSourceSessionId <= SessionMask.MAX);

            try
            {
                lock (EventListener.EventListenersLock)
                {
                    EnsureInitialized();

                    // Find the per-EventSource dispatcher cooresponding to registered dispatcher
                    EventDispatcher eventSourceDispatcher = GetDispatcher(listener);
                    if (eventSourceDispatcher == null && listener != null)     // dispatcher == null means ETW dispatcher
                        throw new ArgumentException(Environment.GetResourceString("EventSource_ListenerNotFound"));

                    if (commandArguments == null)
                        commandArguments = new Dictionary<string, string>();

                    if (command == EventCommand.Update)
                    {
                        // Set it up using the 'standard' filtering bitfields (use the "global" enable, not session specific one)
                        for (int i = 0; i < m_eventData.Length; i++)
                            EnableEventForDispatcher(eventSourceDispatcher, i, IsEnabledByDefault(i, enable, level, matchAnyKeyword));

                        if (enable)
                        {
                            if (!m_eventSourceEnabled)
                            {
                                // EventSource turned on for the first time, simply copy the bits.  
                                m_level = level;
                                m_matchAnyKeyword = matchAnyKeyword;
                            }
                            else
                            {
                                // Already enabled, make it the most verbose of the existing and new filter
                                if (level > m_level)
                                    m_level = level;
                                if (matchAnyKeyword == 0)
                                    m_matchAnyKeyword = 0;
                                else if (m_matchAnyKeyword != 0)
                                    m_matchAnyKeyword |= matchAnyKeyword;
                            }
                        }

                        // interpret perEventSourceSessionId's sign, and adjust perEventSourceSessionId to 
                        // represent 0-based positive values
                        bool bSessionEnable = (perEventSourceSessionId >= 0);
                        if (perEventSourceSessionId == 0 && enable == false)
                            bSessionEnable = false;

                        if (listener == null)
                        {
                            if (!bSessionEnable) 
                                perEventSourceSessionId = -perEventSourceSessionId;
                            // for "global" enable/disable (passed in with listener == null and
                            //  perEventSourceSessionId == 0) perEventSourceSessionId becomes -1
                            --perEventSourceSessionId;
                        }

                        command = bSessionEnable ? EventCommand.Enable : EventCommand.Disable;

                        // perEventSourceSessionId = -1 when ETW sent a notification, but the set of active sessions
                        // hasn't changed.
                        // sesisonId = SessionMask.MAX when one of the legacy ETW sessions changed
                        // 0 <= perEventSourceSessionId < SessionMask.MAX for activity-tracing aware sessions
                        Contract.Assert(perEventSourceSessionId >= -1 && perEventSourceSessionId <= SessionMask.MAX);

                        // Send the manifest if we are enabling an ETW session
                        if (bSessionEnable && eventSourceDispatcher == null)
                        {
                            // eventSourceDispatcher == null means this is the ETW manifest

                            // SendCommand can be called from the EventSource constructor as a side effect of 
                            // ETW registration.   Unfortunately when this callback is active the provider is
                            // not actually enabled (WriteEvents will fail).   Thus if we detect this condition
                            // (that we are still being constructed), we simply skip sending the manifest.  
                            // When the constructor completes we will try again and send the manifest at that time. 
                            //
                            // Note that we unconditionally send the manifest whenever we are enabled, even if
                            // we were already enabled.   This is because there may be multiple sessions active
                            // and we can't know that all the sessions have seen the manifest.  
                            if (m_completelyInited)
                                SendManifest(m_rawManifest);
                            else
                                m_deferedSendManifest = true;
                        }

#if FEATURE_ACTIVITYSAMPLING
                        if (bSessionEnable && perEventSourceSessionId != -1)
                        {
                            bool participateInSampling = false;
                            string activityFilters;
                            int sessionIdBit;

                            ParseCommandArgs(commandArguments, out participateInSampling, 
                                            out activityFilters, out sessionIdBit);

                            if (listener == null && commandArguments.Count > 0 && perEventSourceSessionId != sessionIdBit)
                            {
                                throw new ArgumentException(Environment.GetResourceString("EventSource_SessionIdError", 
                                                            perEventSourceSessionId+SessionMask.SHIFT_SESSION_TO_KEYWORD,
                                                            sessionIdBit+SessionMask.SHIFT_SESSION_TO_KEYWORD));
                            }

                            if (listener == null)
                            {
                                UpdateEtwSession(perEventSourceSessionId, etwSessionId, true, activityFilters, participateInSampling);
                            }
                            else
                            {
                                ActivityFilter.UpdateFilter(ref listener.m_activityFilter, this, 0, activityFilters);
                                eventSourceDispatcher.m_activityFilteringEnabled = participateInSampling;
                            }
                        }
                        else if (!bSessionEnable && listener == null)
                        {
                            // if we disable an ETW session, indicate that in a synthesized command argument
                            if (perEventSourceSessionId >= 0 && perEventSourceSessionId < SessionMask.MAX)
                            {
                                commandArguments["EtwSessionKeyword"] = (perEventSourceSessionId+SessionMask.SHIFT_SESSION_TO_KEYWORD).ToString(CultureInfo.InvariantCulture);
                            }
                        }
#endif // FEATURE_ACTIVITYSAMPLING

                        this.OnEventCommand(new EventCommandEventArgs(command, commandArguments, this, eventSourceDispatcher));

#if FEATURE_ACTIVITYSAMPLING
                        if (listener == null && !bSessionEnable && perEventSourceSessionId != -1)
                        {
                            // if we disable an ETW session, complete disabling it
                            UpdateEtwSession(perEventSourceSessionId, etwSessionId, false, null, false);
                        }
#endif // FEATURE_ACTIVITYSAMPLING

                        if (enable)
                        {
                            m_eventSourceEnabled = true;
                        }
                        else
                        {
                            // If we are disabling, maybe we can turn on 'quick checks' to filter
                            // quickly.  These are all just optimizations (since later checks will still filter)

#if FEATURE_ACTIVITYSAMPLING
                            // Turn off (and forget) any information about Activity Tracing.  
                            if (listener == null)
                            {
                                // reset all filtering information for activity-tracing-aware sessions
                                for (int i = 0; i < SessionMask.MAX; ++i)
                                {
                                    EtwSession etwSession = m_etwSessionIdMap[i];
                                    if (etwSession != null)
                                        ActivityFilter.DisableFilter(ref etwSession.m_activityFilter, this);
                                }
                                m_activityFilteringForETWEnabled = new SessionMask(0);
                                m_curLiveSessions = new SessionMask(0);
                                // reset activity-tracing-aware sessions
                                if (m_etwSessionIdMap != null)
                                    for (int i = 0; i < SessionMask.MAX; ++i)
                                        m_etwSessionIdMap[i] = null;
                                // reset legacy sessions
                                if (m_legacySessions != null)
                                    m_legacySessions.Clear();
                            }
                            else
                            {
                                ActivityFilter.DisableFilter(ref listener.m_activityFilter, this);
                                eventSourceDispatcher.m_activityFilteringEnabled = false;
                            }
#endif // FEATURE_ACTIVITYSAMPLING

                            // There is a good chance EnabledForAnyListener are not as accurate as
                            // they could be, go ahead and get a better estimate.  
                            for (int i = 0; i < m_eventData.Length; i++)
                            {
                                bool isEnabledForAnyListener = false;
                                for (EventDispatcher dispatcher = m_Dispatchers; dispatcher != null; dispatcher = dispatcher.m_Next)
                                {
                                    if (dispatcher.m_EventEnabled[i])
                                    {
                                        isEnabledForAnyListener = true;
                                        break;
                                    }
                                }
                                m_eventData[i].EnabledForAnyListener = isEnabledForAnyListener;
                            }

                            // If no events are enabled, disable the global enabled bit.
                            if (!AnyEventEnabled())
                            {
                                m_level = 0;
                                m_matchAnyKeyword = 0;
                                m_eventSourceEnabled = false;
                            }
                        }
#if FEATURE_ACTIVITYSAMPLING
                        UpdateKwdTriggers(enable);
#endif // FEATURE_ACTIVITYSAMPLING
                    }
                    else
                    {
                        if (command == EventCommand.SendManifest)
                            SendManifest(m_rawManifest);

                        // These are not used for non-update commands and thus should always be 'default' values
                        Contract.Assert(enable == true);
                        Contract.Assert(m_level == EventLevel.LogAlways);
                        Contract.Assert(m_matchAnyKeyword == EventKeywords.None);

                        this.OnEventCommand(new EventCommandEventArgs(command, commandArguments, null, null));
                    }

#if FEATURE_ACTIVITYSAMPLING
                    if (m_completelyInited && (listener != null || shouldReport))
                    {
                        SessionMask m = SessionMask.FromId(perEventSourceSessionId); 
                        ReportActivitySamplingInfo(listener, m);
                    }
                    OutputDebugString(string.Format(CultureInfo.InvariantCulture, "{0}.SendCommand(session {1}, cmd {2}, enable {3}, level {4}): live sessions {5:x}, sampling {6:x}", 
                                      m_name, perEventSourceSessionId, command, enable, level, 
                                      (ulong) m_curLiveSessions, (ulong) m_activityFilteringForETWEnabled));
#endif // FEATURE_ACTIVITYSAMPLING
                }
            }
            catch (Exception e)
            {
                // Remember any exception and rethrow.  
                m_lastCommandException = e;
                throw;
            }
        }

#if FEATURE_ACTIVITYSAMPLING

        internal void UpdateEtwSession(
            int sessionIdBit,
            int etwSessionId,
            bool bEnable,
            string activityFilters,
            bool participateInSampling)
        {
            if (sessionIdBit < SessionMask.MAX)
            {
            // activity-tracing-aware etw session
                if (bEnable)
                {
                    var etwSession = EtwSession.GetEtwSession(etwSessionId, true);
                    ActivityFilter.UpdateFilter(ref etwSession.m_activityFilter, this, sessionIdBit, activityFilters);
                    m_etwSessionIdMap[sessionIdBit] = etwSession;
                    m_activityFilteringForETWEnabled[sessionIdBit] = participateInSampling;
                }
                else
                {
                    var etwSession = EtwSession.GetEtwSession(etwSessionId);
                    ActivityFilter.DisableFilter(ref etwSession.m_activityFilter, this);
                    m_etwSessionIdMap[sessionIdBit] = null;
                    m_activityFilteringForETWEnabled[sessionIdBit] = false;
                    // the ETW session is going away; remove it from the global list
                    EtwSession.RemoveEtwSession(etwSession);
                }
                m_curLiveSessions[sessionIdBit] = bEnable;
            }
            else
            {
            // legacy etw session    
                if (bEnable)
                {
                    if (m_legacySessions == null)
                        m_legacySessions = new List<EtwSession>(8);
                    var etwSession = EtwSession.GetEtwSession(etwSessionId, true);
                    if (!m_legacySessions.Contains(etwSession))
                        m_legacySessions.Add(etwSession);
                }
                else
                {
                    var etwSession = EtwSession.GetEtwSession(etwSessionId);
                    if (m_legacySessions != null)
                        m_legacySessions.Remove(etwSession);
                    // the ETW session is going away; remove it from the global list
                    EtwSession.RemoveEtwSession(etwSession);
                }
            }
        }

        internal static bool ParseCommandArgs(
                        IDictionary<string, string> commandArguments,
                        out bool participateInSampling,
                        out string activityFilters,
                        out int sessionIdBit)
        {
            bool res = true;
            participateInSampling = false;
            string activityFilterString;
            if (commandArguments.TryGetValue("ActivitySamplingStartEvent", out activityFilters))
            {
                // if a start event is specified default the event source to participate in sampling
                participateInSampling = true;
            }

            if (commandArguments.TryGetValue("ActivitySampling", out activityFilterString))
            {
                if (string.Compare(activityFilterString, "false", StringComparison.OrdinalIgnoreCase) == 0 ||
                    activityFilterString == "0")
                    participateInSampling = false;
                else
                    participateInSampling = true;
            }

            string sSessionKwd;
            int sessionKwd = -1;
            if (!commandArguments.TryGetValue("EtwSessionKeyword", out sSessionKwd) ||
                !int.TryParse(sSessionKwd, out sessionKwd) ||
                sessionKwd < SessionMask.SHIFT_SESSION_TO_KEYWORD ||
                sessionKwd >= SessionMask.SHIFT_SESSION_TO_KEYWORD + SessionMask.MAX)
            {
                sessionIdBit = -1;
                res = false;
            }
            else
            {
                sessionIdBit = sessionKwd - SessionMask.SHIFT_SESSION_TO_KEYWORD;
            }
            return res;
        }

        internal void UpdateKwdTriggers(bool enable)
        {
            if (enable)
            {
                // recompute m_keywordTriggers
                ulong gKeywords = (ulong)m_matchAnyKeyword;
                if (gKeywords == 0)
                    gKeywords = 0xFFFFffffFFFFffff;

                m_keywordTriggers = 0;
                for (int sessId = 0; sessId < SessionMask.MAX; ++sessId)
                {
                    EtwSession etwSession = m_etwSessionIdMap[sessId];
                    if (etwSession == null)
                        continue;

                    ActivityFilter activityFilter = etwSession.m_activityFilter;
                    ActivityFilter.UpdateKwdTriggers(activityFilter, m_guid, this, (EventKeywords)gKeywords);
                }
            }
            else
            {
                m_keywordTriggers = 0;
            }
        }

#endif // FEATURE_ACTIVITYSAMPLING

        /// <summary>
        /// If 'value is 'true' then set the eventSource so that 'dispatcher' will recieve event with the eventId
        /// of 'eventId.  If value is 'false' disable the event for that dispatcher.   If 'eventId' is out of
        /// range return false, otherwise true.  
        /// </summary>
        internal bool EnableEventForDispatcher(EventDispatcher dispatcher, int eventId, bool value)
        {
            if (dispatcher == null)
            {
                if (eventId >= m_eventData.Length)
                    return false;
#if FEATURE_MANAGED_ETW
                if (m_provider != null)
                    m_eventData[eventId].EnabledForETW = value;
#endif
            }
            else
            {
                if (eventId >= dispatcher.m_EventEnabled.Length)
                    return false;
                dispatcher.m_EventEnabled[eventId] = value;
                if (value)
                    m_eventData[eventId].EnabledForAnyListener = true;
            }
            return true;
        }

        /// <summary>
        /// Returns true if any event at all is on.  
        /// </summary>
        private bool AnyEventEnabled()
        {
            for (int i = 0; i < m_eventData.Length; i++)
                if (m_eventData[i].EnabledForETW || m_eventData[i].EnabledForAnyListener)
                    return true;
            return false;
        }

        [SecuritySafeCritical]
        private void EnsureInitialized()
        {
            Contract.Assert(Monitor.IsEntered(EventListener.EventListenersLock));
            if (m_rawManifest == null)
            {
                Contract.Assert(m_rawManifest == null);
                m_rawManifest = CreateManifestAndDescriptors(this.GetType(), Name, this);

                // 
                foreach (WeakReference eventSourceRef in EventListener.s_EventSources)
                {
                    EventSource eventSource = eventSourceRef.Target as EventSource;
                    if (eventSource != null && eventSource.Guid == m_guid)
                    {
                        if (eventSource != this)
                            throw new ArgumentException(Environment.GetResourceString("EventSource_EventSourceGuidInUse", m_guid));
                    }
                }

                // Make certain all dispatchers are also have their array's initialized
                EventDispatcher dispatcher = m_Dispatchers;
                while (dispatcher != null)
                {
                    if (dispatcher.m_EventEnabled == null)
                        dispatcher.m_EventEnabled = new bool[m_eventData.Length];
                    dispatcher = dispatcher.m_Next;
                }
            }
            if (s_currentPid == 0)
            {
                s_currentPid = Win32Native.GetCurrentProcessId();
            }
        }

        // Send out the ETW manifest XML out to ETW
        // Today, we only send the manifest to ETW, custom listeners don't get it. 
        [SecuritySafeCritical]
        private unsafe bool SendManifest(byte[] rawManifest)
        {
            bool success = true;

#if FEATURE_MANAGED_ETW
            fixed (byte* dataPtr = rawManifest)
            {
                var manifestDescr = new System.Diagnostics.Tracing.EventDescriptor(0xFFFE, 1, 0, 0, 0xFE, 0xFFFE, -1);
                ManifestEnvelope envelope = new ManifestEnvelope();

                envelope.Format = ManifestEnvelope.ManifestFormats.SimpleXmlFormat;
                envelope.MajorVersion = 1;
                envelope.MinorVersion = 0;
                envelope.Magic = 0x5B;              // An unusual number that can be checked for consistancy. 
                int dataLeft = rawManifest.Length;
                envelope.TotalChunks = (ushort)((dataLeft + (ManifestEnvelope.MaxChunkSize - 1)) / ManifestEnvelope.MaxChunkSize);
                envelope.ChunkNumber = 0;

                EventProvider.EventData* dataDescrs = stackalloc EventProvider.EventData[2];
                dataDescrs[0].Ptr = (ulong)&envelope;
                dataDescrs[0].Size = (uint)sizeof(ManifestEnvelope);
                dataDescrs[0].Reserved = 0;

                dataDescrs[1].Ptr = (ulong)dataPtr;
                dataDescrs[1].Reserved = 0;

                int chunkSize = ManifestEnvelope.MaxChunkSize;
            TRY_AGAIN_WITH_SMALLER_CHUNK_SIZE:

                while (dataLeft > 0)
                {
                    dataDescrs[1].Size = (uint)Math.Min(dataLeft, chunkSize);
                    if (m_provider != null)
                    {
                        if (!m_provider.WriteEvent(ref manifestDescr, null, 2, (IntPtr)dataDescrs))
                        {                            
                            // Turns out that if users set the BufferSize to something less than 64K then WriteEvent
                            // can fail.   If we get this failure on the first chunk try again with something smaller
                            // The smallest BufferSize is 1K so if we get to 512, we can give up making it smaller. 
                            if (EventProvider.GetLastWriteEventError() == EventProvider.WriteEventErrorCode.EventTooBig)
                            {
                                chunkSize = chunkSize / 2;
                                if (envelope.ChunkNumber == 0 && chunkSize > 512)
                                    goto TRY_AGAIN_WITH_SMALLER_CHUNK_SIZE;
                            }
                            success = false;
                            if(m_throwOnEventWriteErrors)
                                ThrowEventSourceException();
                            }
                        }
                    dataLeft -= ManifestEnvelope.MaxChunkSize;
                    dataDescrs[1].Ptr += ManifestEnvelope.MaxChunkSize;
                    envelope.ChunkNumber++;
                }
            }
#endif

            return success;
        }


        // Helper to deal with the fact that the type we are reflecting over might be loaded in the ReflectionOnly context.
        // When that is the case, we have the build the custom assemblies on a member by hand.         
        internal static Attribute GetCustomAttributeHelper(MemberInfo member, Type attributeType)
        {
            if (!member.Module.Assembly.ReflectionOnly)
            {
                // Let the runtime to the work for us, since we can execute code in this context.
                return Attribute.GetCustomAttribute(member, attributeType, false);
            }

            // In the reflection only context, we have to do things by hand.
            string fullTypeNameToFind = attributeType.FullName;

#if EVENT_SOURCE_LEGACY_NAMESPACE_SUPPORT
            fullTypeNameToFind = fullTypeNameToFind.Replace("System.Diagnostics.Eventing", "System.Diagnostics.Tracing");
#endif

            foreach (CustomAttributeData data in CustomAttributeData.GetCustomAttributes(member))
            {
                string attributeFullTypeName = data.Constructor.ReflectedType.FullName;
#if EVENT_SOURCE_LEGACY_NAMESPACE_SUPPORT
                attributeFullTypeName = attributeFullTypeName.Replace("System.Diagnostics.Eventing", "System.Diagnostics.Tracing");
#endif

                if (String.Equals(attributeFullTypeName, fullTypeNameToFind, StringComparison.Ordinal))
                {
                    Attribute attr = null;

                    Contract.Assert(data.ConstructorArguments.Count <= 1);

                    if (data.ConstructorArguments.Count == 1)
                    {
                        attr = (Attribute)Activator.CreateInstance(attributeType, new object[] { data.ConstructorArguments[0].Value });
                    }
                    else if (data.ConstructorArguments.Count == 0)
                    {
                        attr = (Attribute)Activator.CreateInstance(attributeType);
                    }

                    if (attr != null)
                    {
                        Type t = attr.GetType();

                        foreach (CustomAttributeNamedArgument namedArgument in data.NamedArguments)
                        {
                            PropertyInfo p = t.GetProperty(namedArgument.MemberInfo.Name, BindingFlags.Public | BindingFlags.Instance);
                            object value = namedArgument.TypedValue.Value;

                            if (p.PropertyType.IsEnum)
                            {
                                value = Enum.Parse(p.PropertyType, value.ToString());
                            }

                            p.SetValue(attr, value, null);
                        }

                        return attr;
                    }
                }
            }

            return null;
        }

        // Use reflection to look at the attributes of a class, and generate a manifest for it (as UTF8) and
        // return the UTF8 bytes.  It also sets up the code:EventData structures needed to dispatch events
        // at run time.  'source' is the event source to place the descriptors.  If it is null,
        // then the descriptors are not creaed, and just the manifest is generated.  
        private static byte[] CreateManifestAndDescriptors(Type eventSourceType, string eventSourceDllName, EventSource source)
        {
            MethodInfo[] methods = eventSourceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            EventAttribute defaultEventAttribute;
            int eventId = 1;        // The number given to an event that does not have a explicitly given ID. 
            EventMetadata[] eventData = null;
            Dictionary<string, string> eventsByName = null;
            if (source != null)
                eventData = new EventMetadata[methods.Length];

            // See if we have localization information.  
            ResourceManager resources = null;
            EventSourceAttribute eventSourceAttrib = (EventSourceAttribute)GetCustomAttributeHelper(eventSourceType, typeof(EventSourceAttribute));
            if (eventSourceAttrib != null && eventSourceAttrib.LocalizationResources != null)
                resources = new ResourceManager(eventSourceAttrib.LocalizationResources, eventSourceType.Assembly);

            ManifestBuilder manifest = new ManifestBuilder(GetName(eventSourceType), GetGuid(eventSourceType), eventSourceDllName, resources);

            // Collect task, opcode, keyword and channel information
#if FEATURE_MANAGED_ETW_CHANNELS
            foreach (var providerEnumKind in new string[] { "Keywords", "Tasks", "Opcodes", "Channels" })
#else
            foreach (var providerEnumKind in new string[] { "Keywords", "Tasks", "Opcodes" })
#endif
            {
                Type nestedType = eventSourceType.GetNestedType(providerEnumKind);
                if (nestedType != null)
                {
                    foreach (FieldInfo staticField in nestedType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        AddProviderEnumKind(manifest, staticField, providerEnumKind);
                    }
                }
            }

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                ParameterInfo[] args = method.GetParameters();

                // Get the EventDescriptor (from the Custom attributes)
                EventAttribute eventAttribute = (EventAttribute)GetCustomAttributeHelper(method, typeof(EventAttribute));

                // Methods that don't return void can't be events.  
                if (method.ReturnType != typeof(void))
                {
                    if (eventAttribute != null)
                        throw new ArgumentException(Environment.GetResourceString("EventSource_AttributeOnNonVoid", method.Name));
                    continue;
                }
                if (method.IsVirtual || method.IsStatic)
                {
                    continue;
                }

                if (eventAttribute == null)
                {
                    // If we explictly mark the method as not being an event, then honor that.  
                    if (GetCustomAttributeHelper(method, typeof(NonEventAttribute)) != null)
                        continue;

                    defaultEventAttribute = new EventAttribute(eventId);
                    eventAttribute = defaultEventAttribute;
                }
                else if (eventAttribute.EventId <= 0)
                    throw new ArgumentException(Environment.GetResourceString("EventSource_NeedPositiveId"));
                else if ((ulong)eventAttribute.Keywords >= 0x0000100000000000UL)
                    throw new ArgumentException(Environment.GetResourceString("EventSource_ReservedKeywords"));
                eventId++;

                // Auto-assign tasks, starting with the highest task number and working back 
                if (eventAttribute.Opcode == EventOpcode.Info && eventAttribute.Task == EventTask.None)
                    eventAttribute.Task = (EventTask)(0xFFFE - eventAttribute.EventId);

                manifest.StartEvent(method.Name, eventAttribute);
                for (int fieldIdx = 0; fieldIdx < args.Length; fieldIdx++)
                {
                    // If the first parameter is 'RelatedActivityId' then skip it.  
                    if (fieldIdx == 0 && args[fieldIdx].ParameterType == typeof(Guid) && 
                        string.Compare(args[fieldIdx].Name, "RelatedActivityId", StringComparison.OrdinalIgnoreCase) == 0)
                        continue;
                    manifest.AddEventParameter(args[fieldIdx].ParameterType, args[fieldIdx].Name);
                }
                manifest.EndEvent();

                if (source != null)
                {
                    // Do checking for user errors (optional, but nto a big deal so we do it).  
                    DebugCheckEvent(ref eventsByName, eventData, method, eventAttribute);
                    AddEventDescriptor(ref eventData, method.Name, eventAttribute, args);
                }
            }

            if (source != null)
            {
                TrimEventDescriptors(ref eventData);
                source.m_eventData = eventData;     // officaly initialize it. We do this at most once (it is racy otherwise). 
            }

            return manifest.CreateManifest();
        }

        // adds a enumeration (keyword, opcode, task or channel) represented by 'staticField'
        // to the manifest.  
        private static void AddProviderEnumKind(ManifestBuilder manifest, FieldInfo staticField, string providerEnumKind)
        {
            Type staticFieldType = staticField.FieldType;
            if (staticFieldType == typeof(EventOpcode))
            {
                if (providerEnumKind != "Opcodes") goto Error;
                int value = (int)staticField.GetRawConstantValue();
                if (value <= 10)
                    throw new ArgumentException(Environment.GetResourceString("EventSource_ReservedOpcode"));
                manifest.AddOpcode(staticField.Name, value);
            }
            else if (staticFieldType == typeof(EventTask))
            {
                if (providerEnumKind != "Tasks") goto Error;
                manifest.AddTask(staticField.Name, (int)staticField.GetRawConstantValue());
            }
            else if (staticFieldType == typeof(EventKeywords))
            {
                if (providerEnumKind != "Keywords") goto Error;
                manifest.AddKeyword(staticField.Name, (ulong)(long)staticField.GetRawConstantValue());
            }
#if FEATURE_MANAGED_ETW_CHANNELS
            else if (staticFieldType == typeof(EventChannel))
            {
                if (providerEnumKind != "Channels") goto Error;
                var channelAttribute = (ChannelAttribute)GetCustomAttributeHelper(staticField, typeof(ChannelAttribute));
                manifest.AddChannel(staticField.Name, (byte)staticField.GetRawConstantValue(), channelAttribute);
            }
#endif
            return;
        Error:
            throw new ArgumentException(Environment.GetResourceString("EventSource_EnumKindMismatch",  staticField.FieldType.Name, providerEnumKind));
        }

        // Helper used by code:CreateManifestAndDescriptors to add a code:EventData descriptor for a method
        // with the code:EventAttribute 'eventAttribute'.  resourceManger may be null in which case we populate it
        // it is populated if we need to look up message resources
        private static void AddEventDescriptor(ref EventMetadata[] eventData, string eventName, 
                                EventAttribute eventAttribute, ParameterInfo[] eventParameters)
        {
            if (eventData == null || eventData.Length <= eventAttribute.EventId)
            {
                EventMetadata[] newValues = new EventMetadata[Math.Max(eventData.Length + 16, eventAttribute.EventId + 1)];
                Array.Copy(eventData, newValues, eventData.Length);
                eventData = newValues;
            }

            eventData[eventAttribute.EventId].Descriptor = new System.Diagnostics.Tracing.EventDescriptor(
                    eventAttribute.EventId,
                    eventAttribute.Version,
#if FEATURE_MANAGED_ETW_CHANNELS
                    (byte)eventAttribute.Channel,
#else
                    (byte)0,
#endif
                    (byte)eventAttribute.Level,
                    (byte)eventAttribute.Opcode,
                    (int)eventAttribute.Task,
                    (long)((ulong)eventAttribute.Keywords | SessionMask.All.ToEventKeywords()));

            eventData[eventAttribute.EventId].Name = eventName;
            eventData[eventAttribute.EventId].Parameters = eventParameters;
            eventData[eventAttribute.EventId].Message = eventAttribute.Message;
        }

        // Helper used by code:CreateManifestAndDescriptors that trims the m_eventData array to the correct
        // size after all event descriptors have been added. 
        private static void TrimEventDescriptors(ref EventMetadata[] eventData)
        {
            int idx = eventData.Length;
            while (0 < idx)
            {
                --idx;
                if (eventData[idx].Descriptor.EventId != 0)
                    break;
            }
            if (eventData.Length - idx > 2)      // allow one wasted slot. 
            {
                EventMetadata[] newValues = new EventMetadata[idx + 1];
                Array.Copy(eventData, newValues, newValues.Length);
                eventData = newValues;
            }
        }

        // Helper used by code:EventListener.AddEventSource and code:EventListener.EventListener
        // when a listener gets attached to a eventSource
        internal void AddListener(EventListener listener)
        {
            lock (EventListener.EventListenersLock)
            {
                bool[] enabledArray = null;
                if (m_eventData != null)
                    enabledArray = new bool[m_eventData.Length];
                m_Dispatchers = new EventDispatcher(m_Dispatchers, enabledArray, listener);
                listener.OnEventSourceCreated(this);
            }
        }

        // Helper used by code:CreateManifestAndDescriptors to find user mistakes like reusing an event
        // index for two distinct events etc.  Throws exceptions when it finds something wrong. 
        private static void DebugCheckEvent(ref Dictionary<string, string> eventsByName,
            EventMetadata[] eventData, MethodInfo method, EventAttribute eventAttribute)
        {
            int eventArg = GetHelperCallFirstArg(method);
            if (eventArg >= 0 && eventAttribute.EventId != eventArg)
            {
                throw new ArgumentException(Environment.GetResourceString("EventSource_MismatchIdToWriteEvent", method.Name, eventAttribute.EventId, eventArg));
            }

            if (eventAttribute.EventId < eventData.Length && eventData[eventAttribute.EventId].Descriptor.EventId != 0)
            {
                throw new ArgumentException(Environment.GetResourceString("EventSource_EventIdReused", method.Name, eventAttribute.EventId));
            }

            if (eventsByName == null)
                eventsByName = new Dictionary<string, string>();

            if (eventsByName.ContainsKey(method.Name))
                throw new ArgumentException(Environment.GetResourceString("EventSource_EventNameReused", method.Name));

            eventsByName[method.Name] = method.Name;
        }

        /// <summary>
        /// This method looks at the IL and tries to pattern match against the standard
        /// 'boilerplate' event body 
        /// 
        ///     { if (Enabled()) WriteEvent(#, ...) } 
        /// 
        /// If the pattern matches, it returns the literal number passed as the first parameter to
        /// the WriteEvent.  This is used to find common user errors (mismatching this
        /// number with the EventAttribute ID).  It is only used for validation.   
        /// </summary>
        /// <param name="method">The method to probe.</param>
        /// <returns>The literal value or -1 if the value could not be determined. </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch statement is clearer than alternatives")]
        [SecuritySafeCritical]
        static private int GetHelperCallFirstArg(MethodInfo method)
        {
            // we need this permission in low trust
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            // Currently searches for the following pattern
            // 
            // ...     // CAN ONLY BE THE INSTRUCTIONS BELOW
            // LDARG0
            // LDC.I4 XXX
            // ...     // CAN ONLY BE THE INSTRUCTIONS BELOW CAN'T BE A BRANCH OR A CALL
            // CALL 
            // NOP     // 0 or more times
            // RET
            // 
            // If we find this pattern we return the XXX.  Otherwise we return -1.  
            byte[] instrs = method.GetMethodBody().GetILAsByteArray();
            int retVal = -1;
            for (int idx = 0; idx < instrs.Length; )
            {
                switch (instrs[idx])
                {
                    case 0: // NOP
                    case 1: // BREAK
                    case 2: // LDARG_0
                    case 3: // LDARG_1
                    case 4: // LDARG_2
                    case 5: // LDARG_3
                    case 6: // LDLOC_0
                    case 7: // LDLOC_1
                    case 8: // LDLOC_2
                    case 9: // LDLOC_3
                    case 10: // STLOC_0
                    case 11: // STLOC_1
                    case 12: // STLOC_2
                    case 13: // STLOC_3
                        break;
                    case 14: // LDARG_S
                    case 16: // STARG_S
                        idx++;
                        break;
                    case 20: // LDNULL
                        break;
                    case 21: // LDC_I4_M1
                    case 22: // LDC_I4_0
                    case 23: // LDC_I4_1
                    case 24: // LDC_I4_2
                    case 25: // LDC_I4_3
                    case 26: // LDC_I4_4
                    case 27: // LDC_I4_5
                    case 28: // LDC_I4_6
                    case 29: // LDC_I4_7
                    case 30: // LDC_I4_8
                        if (idx > 0 && instrs[idx - 1] == 2)  // preceeded by LDARG0
                            retVal = instrs[idx] - 22;
                        break;
                    case 31: // LDC_I4_S
                        if (idx > 0 && instrs[idx - 1] == 2)  // preceeded by LDARG0
                            retVal = instrs[idx + 1];
                        idx++;
                        break;
                    case 32: // LDC_I4
                        idx += 4;
                        break;
                    case 37: // DUP
                        break;
                    case 40: // CALL
                        idx += 4;

                        if (retVal >= 0)
                        {
                            // Is this call just before return?  
                            for (int search = idx + 1; search < instrs.Length; search++)
                            {
                                if (instrs[search] == 42)  // RET
                                    return retVal;
                                if (instrs[search] != 0)   // NOP
                                    break;
                            }
                        }
                        retVal = -1;
                        break;
                    case 44: // BRFALSE_S
                    case 45: // BRTRUE_S
                        retVal = -1;
                        idx++;
                        break;
                    case 57: // BRFALSE
                    case 58: // BRTRUE
                        retVal = -1;
                        idx += 4;
                        break;
                    case 103: // CONV_I1
                    case 104: // CONV_I2
                    case 105: // CONV_I4
                    case 106: // CONV_I8
                    case 109: // CONV_U4
                    case 110: // CONV_U8
                        break;
                    case 140: // BOX
                    case 141: // NEWARR
                        idx += 4;
                        break;
                    case 162: // STELEM_REF
                        break;
                    case 254: // PREFIX
                        idx++;
                        // Covers the CEQ instructions used in debug code for some reason.
                        if (idx >= instrs.Length || instrs[idx] >= 6)
                            goto default;
                        break;
                    default:
                        /* Contract.Assert(false, "Warning: User validation code sub-optimial: Unsuported opcode " + instrs[idx] +
                            " at " + idx + " in method " + method.Name); */
                        return -1;
                }
                idx++;
            }
            return -1;
        }

        [Conditional("DEBUG")]
        internal static void OutputDebugString(string msg)
        {
            msg = msg.TrimEnd('\r', '\n') + 
                    string.Format(CultureInfo.InvariantCulture, ", Thrd({0})"+Environment.NewLine, Thread.CurrentThread.ManagedThreadId);
            System.Diagnostics.Debugger.Log(0, null, msg);
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        internal void ReportOutOfBandMessage(string msg, bool flush)
        {
            // msg == null is a signal to flush what's accumulated in the buffer
            if (msg == null && flush)
            {
                if (!string.IsNullOrEmpty(m_deferredErrorInfo))
                {
                    WriteString(m_deferredErrorInfo);
                    m_deferredErrorInfo = String.Empty;
                }
                return;
            }

            if (!msg.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                msg = msg + Environment.NewLine;

            // send message to debugger without delay
            System.Diagnostics.Debugger.Log(0, null, msg);

            m_deferredErrorInfo = m_deferredErrorInfo + msg;
            if (flush)
            {
                // send message to the ETW listener if available
                WriteString(m_deferredErrorInfo);
                m_deferredErrorInfo = String.Empty;
            }
        }

#if FEATURE_ACTIVITYSAMPLING
        private void ReportActivitySamplingInfo(EventListener listener, SessionMask sessions)
        {
            Contract.Assert(listener == null || (uint)sessions == (uint)SessionMask.FromId(0));

            for (int perEventSourceSessionId = 0; perEventSourceSessionId < SessionMask.MAX; ++perEventSourceSessionId)
            {
                if (!sessions[perEventSourceSessionId])
                    continue;

                ActivityFilter af;
                if (listener == null)
                {
                    EtwSession etwSession = m_etwSessionIdMap[perEventSourceSessionId];
                    Contract.Assert(etwSession != null);
                    af = etwSession.m_activityFilter;
                }
                else
                {
                    af = listener.m_activityFilter;
                }

                if (af == null)
                    continue;

                SessionMask m = new SessionMask(); 
                m[perEventSourceSessionId] = true;

                foreach (var t in af.GetFilterAsTuple(m_guid))
                {
                    WriteStringToListener(listener, string.Format(CultureInfo.InvariantCulture, "Session {0}: {1} = {2}", perEventSourceSessionId, t.Item1, t.Item2), m);
                }

                bool participateInSampling = (listener == null) ? 
                                               m_activityFilteringForETWEnabled[perEventSourceSessionId] : 
                                               GetDispatcher(listener).m_activityFilteringEnabled;
                WriteStringToListener(listener, string.Format(CultureInfo.InvariantCulture, "Session {0}: Activity Sampling support: {1}", 
                                          perEventSourceSessionId, participateInSampling ? "enabled" : "disabled"), m);
            }
        }
#endif // FEATURE_ACTIVITYSAMPLING

        // private instance state 
        private string m_name;                          // My friendly name (privided in ctor)
        internal int m_id;                              // A small integer that is unique to this instance.  
        private Guid m_guid;                            // GUID representing the ETW eventSource to the OS.  
        internal volatile EventMetadata[] m_eventData;  // None per-event data
        private volatile byte[] m_rawManifest;          // Bytes to send out representing the event schema
        private readonly bool m_throwOnEventWriteErrors; // If a listener throws and error, should we catch it or not

        // Enabling bits
        private bool m_eventSourceEnabled;              // am I enabled (any of my events are enabled for any dispatcher)
        internal EventLevel m_level;                    // higest level enabled by any output dispatcher
        internal EventKeywords m_matchAnyKeyword;       // the logical OR of all levels enabled by any output dispatcher (zero is a special case) meaning 'all keywords'

        // Dispatching state 
        internal volatile EventDispatcher m_Dispatchers;    // Linked list of code:EventDispatchers we write the data to (we also do ETW specially)
#if FEATURE_MANAGED_ETW
        private volatile OverideEventProvider m_provider;   // This hooks up ETW commands to our 'OnEventCommand' callback
#endif
        private bool m_completelyInited;                // The EventSource constructor has returned without exception.   
        private bool m_deferedSendManifest;             // We did not send the manifest in the startup path
        private Exception m_lastCommandException;       // If there was an exception during a command, this is it.  
        private Exception m_constructionException;      // If there was an exception construction, this is it 
        private string m_deferredErrorInfo;             // non-fatal error info accumulated during construction

        internal static uint s_currentPid;              // current process id, used in synthesizing quasi-GUIDs

#if FEATURE_ACTIVITYSAMPLING
        private SessionMask m_curLiveSessions;          // the activity-tracing aware sessions' bits
        private EtwSession[] m_etwSessionIdMap;         // the activity-tracing aware sessions
        private List<EtwSession> m_legacySessions;      // the legacy ETW sessions listening to this source
        internal long m_keywordTriggers;                // a bit is set if it corresponds to a keyword that's part of an enabled triggering event
        internal SessionMask m_activityFilteringForETWEnabled; // does THIS EventSource have activity filtering turned on for each ETW session
        static internal Action<Guid> s_activityDying;   // Fires when something calls SetCurrentThreadToActivity()
                                                        // Also used to mark that activity tracing is on for some case
#endif // FEATURE_ACTIVITYSAMPLING
        #endregion
    }

    /// <summary>
    /// An code:EventListener represents the target for all events generated by EventSources (that is
    /// subclasses of code:EventSource), in the currnet appdomain. When a new EventListener is created
    /// it is logically attached to all eventSources in that appdomain. When the EventListener is Disposed, then
    /// it is disconnected from the event eventSources. Note that there is a internal list of STRONG references
    /// to EventListeners, which means that relying on the lack of references ot EventListeners to clean up
    /// EventListeners will NOT work. You must call EventListener.Dispose explicitly when a dispatcher is no
    /// longer needed.
    /// 
    /// Once created, EventListeners can enable or disable on a per-eventSource basis using verbosity levels
    /// (code:EventLevel) and bitfields code:EventKeywords to further restrict the set of events to be sent
    /// to the dispatcher. The dispatcher can also send arbitrary commands to a particular eventSource using the
    /// 'SendCommand' method. The meaning of the commands are eventSource specific.
    /// 
    /// The Null Guid (that is (new Guid()) has special meaning as a wildcard for 'all current eventSources in
    /// the appdomain'. Thus it is relatively easy to turn on all events in the appdomain if desired.
    /// 
    /// It is possible for there to be many EventListener's defined in a single appdomain. Each dispatcher is
    /// logically independent of the other listeners. Thus when one dispatcher enables or disables events, it
    /// affects only that dispatcher (other listeners get the events they asked for). It is possible that
    /// commands sent with 'SendCommand' would do a semantic operation that would affect the other listeners
    /// (like doing a GC, or flushing data ...), but this is the exception rather than the rule.
    /// 
    /// Thus the model is that each EventSource keeps a list of EventListeners that it is sending events
    /// to. Associated with each EventSource-dispatcher pair is a set of filtering criteria that determine for
    /// that eventSource what events that dispatcher will recieve.
    /// 
    /// Listeners receive the events on their 'OnEventWritten' method. Thus subclasses of EventListener must
    /// override this method to do something useful with the data.
    /// 
    /// In addition, when new eventSources are created, the 'OnEventSourceCreate' method is called. The
    /// invariant associated with this callback is that every eventSource gets exactly one
    /// 'OnEventSourceCreate' call for ever eventSource that can potentially send it log messages. In
    /// particular when a EventListener is created, typically a series of OnEventSourceCreate' calls are
    /// made to notify the new dispatcher of all the eventSources that existed before the EventListener was
    /// created.
    /// 
    /// </summary>
    public abstract class EventListener : IDisposable
    {
        private static bool s_CreatingListener = false;
        /// <summary>
        /// Create a new EventListener in which all events start off truned off (use EnableEvents to turn
        /// them on).  
        /// </summary>
        protected EventListener()
        {
            lock (EventListenersLock)
            {
                // Disallow creating EventListener reentrancy. 
                if (s_CreatingListener)
                    throw new InvalidOperationException(Environment.GetResourceString("EventSource_ListenerCreatedInsideCallback"));

                try
                {
                    s_CreatingListener = true;

                    // Add to list of listeners in the system, do this BEFORE firing the ‘OnEventSourceCreated’ so that 
                    // Those added sources see this listener.
                    this.m_Next = s_Listeners;
                    s_Listeners = this;

                    // Find all existing eventSources call OnEventSourceCreated to 'catchup'
                    // Note that we DO have reentrancy here because 'AddListener' calls out to user code (via OnEventSourceCreated callback) 
                    // We tolerate this by iterating over a copy of the list here. New event sources will take care of adding listeners themselves
                    // EventSources are not guaranteed to be added at the end of the s_EventSource list -- We re-use slots when a new source
                    // is created.
                    WeakReference[] eventSourcesSnapshot = s_EventSources.ToArray();

                    for (int i = 0; i < eventSourcesSnapshot.Length; i++)
                    {
                        WeakReference eventSourceRef = eventSourcesSnapshot[i];
                        EventSource eventSource = eventSourceRef.Target as EventSource;
                        if (eventSource != null)
                            eventSource.AddListener(this); // This will cause the OnEventSourceCreated callback to fire. 
                    }

                    Validate();
                }
                finally
                {
                    s_CreatingListener = false;
                }
            }
        }
        /// <summary>
        /// Dispose should be called when the EventListener no longer desires 'OnEvent*' callbacks. Because
        /// there is an internal list of strong references to all EventListeners, calling 'Displose' directly
        /// is the only way to actually make the listen die. Thus it is important that users of EventListener
        /// call Dispose when they are done with their logging.
        /// </summary>
        public virtual void Dispose()
        {
            lock (EventListenersLock)
            {
                Contract.Assert(s_Listeners != null);
                if (s_Listeners != null)
                {
                    if (this == s_Listeners)
                    {
                        EventListener cur = s_Listeners;
                        s_Listeners = this.m_Next;
                        RemoveReferencesToListenerInEventSources(cur);
                    }
                    else
                    {
                        // Find 'this' from the s_Listeners linked list.  
                        EventListener prev = s_Listeners;
                        for (; ; )
                        {
                            EventListener cur = prev.m_Next;
                            if (cur == null)
                                break;
                            if (cur == this)
                            {
                                // Found our Listener, remove references to to it in the eventSources
                                prev.m_Next = cur.m_Next;       // Remove entry. 
                                RemoveReferencesToListenerInEventSources(cur);
                                break;
                            }
                            prev = cur;
                        }
                    }
                }
                Validate();
            }
        }
        // We don't expose a Dispose(bool), because the contract is that you don't have any non-syncronous
        // 'cleanup' associated with this object

        /// Enable all events from the eventSource identified by 'eventSource' to the current dispatcher that have a
        /// verbosity level of 'level' or lower.
        ///   
        /// This call can have the effect of REDUCING the number of events sent to the dispatcher if 'level'
        /// indicates a less verbose level than was previously enabled.
        /// 
        /// This call never has an effect on other EventListeners.
        ///
        /// Returns 'true' if any eventSource could be found that matches 'eventSourceGuid'
        /// </summary>
        public void EnableEvents(EventSource eventSource, EventLevel level)
        {
            EnableEvents(eventSource, level, EventKeywords.None);
        }
        /// <summary>
        /// Enable all events from the eventSource identified by 'eventSourceGuid' to the current dispatcher that have a
        /// verbosity level of 'level' or lower and have a event keyword matching any of the bits in
        /// 'machAnyKeyword'.
        /// 
        /// This call can have the effect of REDUCING the number of events sent to the dispatcher if 'level'
        /// indicates a less verbose level than was previously enabled or if 'machAnyKeyword' has fewer
        /// keywords set than where previously set.
        /// 
        /// If eventSourceGuid is Guid.Empty, then the affects all eventSources in the appdomain
        /// 
        /// If eventSourceGuid is not Guid.Empty, this call has no effect on any other eventSources in the appdomain.
        /// 
        /// This call never has an effect on other EventListeners.
        /// 
        /// Returns 'true' if any eventSource could be found that matches 'eventSourceGuid'        
        /// </summary>
        public void EnableEvents(EventSource eventSource, EventLevel level, EventKeywords matchAnyKeyword)
        {
            EnableEvents(eventSource, level, matchAnyKeyword, null);
        }
        /// <summary>
        /// Enable all events from the eventSource identified by 'eventSource' to the current dispatcher that have a
        /// verbosity level of 'level' or lower and have a event keyword matching any of the bits in
        /// 'machAnyKeyword' as well as any (eventSource specific) effect passing addingional 'key-value' arguments
        /// 'arguments' might have.  
        /// 
        /// This call can have the effect of REDUCING the number of events sent to the dispatcher if 'level'
        /// indicates a less verbose level than was previously enabled or if 'machAnyKeyword' has fewer
        /// keywords set than where previously set.
        /// 
        /// This call never has an effect on other EventListeners.
        /// </summary>       
        public void EnableEvents(EventSource eventSource, EventLevel level, EventKeywords matchAnyKeyword, IDictionary<string, string> arguments)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException("eventSource");
            }
            Contract.EndContractBlock();

            eventSource.SendCommand(this, 0, 0, EventCommand.Update, true, level, matchAnyKeyword, arguments);
        }
        /// <summary>
        /// Disables all events coming from eventSource identified by 'eventSource'.  
        /// 
        /// If eventSourceGuid is Guid.Empty, then the affects all eventSources in the appdomain
        /// 
        /// This call never has an effect on other EventListeners.      
        /// </summary>
        public void DisableEvents(EventSource eventSource)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException("eventSource");
            }
            Contract.EndContractBlock();

            eventSource.SendCommand(this, 0, 0, EventCommand.Update, false, EventLevel.LogAlways, EventKeywords.None, null);
        }

        /// <summary>
        /// This method is caleld whenever a new eventSource is 'attached' to the dispatcher.
        /// This can happen for all existing EventSources when the EventListener is created
        /// as well as for any EventSources that come into existance after the EventListener
        /// has been created.
        /// 
        /// These 'catch up' events are called during the construction of the EventListener.
        /// Subclasses need to be prepared for that.
        /// 
        /// In a multi-threaded environment, it is possible that 'OnEventWritten' callbacks
        /// for a paritcular eventSource to occur BEFORE the OnEventSourceCreated is issued.
        /// </summary>
        /// <param name="eventSource"></param>
        internal protected virtual void OnEventSourceCreated(EventSource eventSource) { }
        /// <summary>
        /// This method is called whenever an event has been written by a EventSource for which the EventListener
        /// has enabled events.  
        /// </summary>
        internal protected abstract void OnEventWritten(EventWrittenEventArgs eventData);
        /// <summary>
        /// EventSourceIndex is small non-negative integer (suitable for indexing in an array)
        /// identifying EventSource. It is unique per-appdomain. Some EventListeners might find
        /// it useful to store addditional information about each eventSource connected to it,
        /// and EventSourceIndex allows this extra infomation to be efficiently stored in a
        /// (growable) array (eg List(T)).
        /// </summary>
        static protected int EventSourceIndex(EventSource eventSource) { return eventSource.m_id; }

        #region private
        /// <summary>
        /// This routine adds newEventSource to the global list of eventSources, it also assigns the
        /// ID to the eventSource (which is simply the oridinal in the global list).
        /// 
        /// EventSources currently do not pro-actively remove themselves from this list. Instead
        /// when eventSources's are GCed, the weak handle in this list naturally gets nulled, and
        /// we will reuse the slot. Today this list never shrinks (but we do reuse entries
        /// that are in the list). This seems OK since the expectation is that EventSources
        /// tend to live for the lifetime of the appdomain anyway (they tend to be used in
        /// global variables).
        /// </summary>
        /// <param name="newEventSource"></param>
        internal static void AddEventSource(EventSource newEventSource)
        {
            lock (EventListenersLock)
            {
                if (s_EventSources == null)
                    s_EventSources = new List<WeakReference>(2);

                // Periodically search the list for existing entries to reuse, this avoids
                // unbounded memory use if we keep recycling eventSources (an unlikely thing). 
                int newIndex = -1;
                if (s_EventSources.Count % 64 == 63)   // on every block of 64, fill up the block before continuing
                {
                    int i = s_EventSources.Count;      // Work from the top down. 
                    while (0 < i)
                    {
                        --i;
                        WeakReference weakRef = s_EventSources[i];
                        if (!weakRef.IsAlive)
                        {
                            newIndex = i;
                            weakRef.Target = newEventSource;
                            break;
                        }
                    }
                }
                if (newIndex < 0)
                {
                    newIndex = s_EventSources.Count;
                    s_EventSources.Add(new WeakReference(newEventSource));
                }
                newEventSource.m_id = newIndex;

                // Add every existing dispatcher to the new EventSource
                for (EventListener listener = s_Listeners; listener != null; listener = listener.m_Next)
                    newEventSource.AddListener(listener);

                Validate();
            }
        }

        /// <summary>
        /// Helper used in code:Dispose that removes any references to 'listenerToRemove' in any of the
        /// eventSources in the appdomain.  
        /// 
        /// The EventListenersLock must be held before calling this routine. 
        /// </summary>
        private static void RemoveReferencesToListenerInEventSources(EventListener listenerToRemove)
        {
            // Foreach existing EventSource in the appdomain
            foreach (WeakReference eventSourceRef in s_EventSources)
            {
                EventSource eventSource = eventSourceRef.Target as EventSource;
                if (eventSource != null)
                {
                    // Is the first output dispatcher the dispatcher we are removing?
                    if (eventSource.m_Dispatchers.m_Listener == listenerToRemove)
                        eventSource.m_Dispatchers = eventSource.m_Dispatchers.m_Next;
                    else
                    {
                        // Remove 'listenerToRemove' from the eventSource.m_Dispatchers linked list.  
                        EventDispatcher prev = eventSource.m_Dispatchers;
                        for (; ; )
                        {
                            EventDispatcher cur = prev.m_Next;
                            if (cur == null)
                            {
                                Contract.Assert(false, "EventSource did not have a registered EventListener!");
                                break;
                            }
                            if (cur.m_Listener == listenerToRemove)
                            {
                                prev.m_Next = cur.m_Next;       // Remove entry. 
                                break;
                            }
                            prev = cur;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks internal consistancy of EventSources/Listeners. 
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Validate()
        {
            lock (EventListenersLock)
            {
                // Get all listeners 
                Dictionary<EventListener, bool> allListeners = new Dictionary<EventListener, bool>();
                EventListener cur = s_Listeners;
                while (cur != null)
                {
                    allListeners.Add(cur, true);
                    cur = cur.m_Next;
                }

                // For all eventSources 
                int id = -1;
                foreach (WeakReference eventSourceRef in s_EventSources)
                {
                    id++;
                    EventSource eventSource = eventSourceRef.Target as EventSource;
                    if (eventSource == null)
                        continue;
                    Contract.Assert(eventSource.m_id == id, "Unexpected event source ID.");

                    // None listeners on eventSources exist in the dispatcher list.   
                    EventDispatcher dispatcher = eventSource.m_Dispatchers;
                    while (dispatcher != null)
                    {
                        Contract.Assert(allListeners.ContainsKey(dispatcher.m_Listener), "EventSource has a listener not on the global list.");
                        dispatcher = dispatcher.m_Next;
                    }

                    // Every dispatcher is on Dispatcher List of every eventSource. 
                    foreach (EventListener listener in allListeners.Keys)
                    {
                        dispatcher = eventSource.m_Dispatchers;
                        for (; ; )
                        {
                            Contract.Assert(dispatcher != null, "Listener is not on all eventSources.");
                            if (dispatcher.m_Listener == listener)
                                break;
                            dispatcher = dispatcher.m_Next;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a global lock that is intended to protect the code:s_Listeners linked list and the
        /// code:s_EventSources WeakReference list.  (We happen to use the s_EventSources list as
        /// the lock object)
        /// </summary>
        internal static object EventListenersLock
        {
            get
            {
                if (s_EventSources == null)
                    Interlocked.CompareExchange(ref s_EventSources, new List<WeakReference>(2), null);
                return s_EventSources;
            }
        }

        // Instance fields
        internal volatile EventListener m_Next;                         // These form a linked list in s_Listeners
#if FEATURE_ACTIVITYSAMPLING
        internal ActivityFilter m_activityFilter;                       // If we are filtering by activity on this Listener, this keeps track of it. 
#endif // FEATURE_ACTIVITYSAMPLING

        // static fields
        internal static EventListener s_Listeners;             // list of all EventListeners in the appdomain
        internal static List<WeakReference> s_EventSources;     // all EventSources in the appdomain
        #endregion
    }

    /// <summary>
    /// Passed to the code:EventSource.OnEventCommand callback
    /// </summary>
    public class EventCommandEventArgs : EventArgs
    {
        public EventCommand Command { get; private set; }
        public IDictionary<String, String> Arguments { get; private set; }

        public bool EnableEvent(int eventId)
        {
            if (Command != EventCommand.Enable && Command != EventCommand.Disable)
                throw new InvalidOperationException();
            return eventSource.EnableEventForDispatcher(dispatcher, eventId, true);
        }
        public bool DisableEvent(int eventId)
        {
            if (Command != EventCommand.Enable && Command != EventCommand.Disable)
                throw new InvalidOperationException();
            return eventSource.EnableEventForDispatcher(dispatcher, eventId, false);
        }

        #region private

        internal EventCommandEventArgs(EventCommand command, IDictionary<string, string> arguments, EventSource eventSource, EventDispatcher dispatcher)
        {
            this.Command = command;
            this.Arguments = arguments;
            this.eventSource = eventSource;
            this.dispatcher = dispatcher;
        }

        internal EventSource eventSource;
        internal EventDispatcher dispatcher;

        #endregion
    }

    /// <summary>
    /// code:EventWrittenEventArgs is passed when the callback given in code:EventListener.OnEventWritten is
    /// fired.
    /// </summary>
    public class EventWrittenEventArgs : EventArgs
    {
        public int EventId { get; internal set; }
        public Guid ActivityId 
        { 
            [System.Security.SecurityCritical]
            get { return EventSource.CurrentThreadActivityId; } 
        }
        public Guid RelatedActivityId 
        { 
            [System.Security.SecurityCritical]
            get; 
            internal set; 
        }
        public ReadOnlyCollection<Object> Payload { get; internal set; }
        public EventSource EventSource { get { return m_eventSource; } }
        public EventKeywords Keywords { get { return (EventKeywords)m_eventSource.m_eventData[EventId].Descriptor.Keywords; } }
        public EventOpcode Opcode { get { return (EventOpcode)m_eventSource.m_eventData[EventId].Descriptor.Opcode; } }
        public EventTask Task { get { return (EventTask)m_eventSource.m_eventData[EventId].Descriptor.Task; } }
        public string Message 
        { 
            get 
            { 
                if (m_message != null)
                    return m_message;
                else
                    return m_eventSource.m_eventData[EventId].Message; 
            }
            internal set
            { 
                m_message = value; 
            }
        }

#if FEATURE_MANAGED_ETW_CHANNELS
        public EventChannel Channel { get { return (EventChannel) m_eventSource.m_eventData[EventId].Descriptor.Channel; }}
#endif
        public byte Version { get { return m_eventSource.m_eventData[EventId].Descriptor.Version; } }
        public EventLevel Level
        {
            get
            {
                if ((uint)EventId >= (uint)m_eventSource.m_eventData.Length)
                    return EventLevel.LogAlways;
                return (EventLevel)m_eventSource.m_eventData[EventId].Descriptor.Level;
            }
        }

        #region private
        internal EventWrittenEventArgs(EventSource eventSource)
        {
            m_eventSource = eventSource;
        }
        private string m_message;
        private EventSource m_eventSource;
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EventSourceAttribute : Attribute
    {
        public string Name { get; set; }
        public string Guid { get; set; }

        /// <summary>
        /// EventSources support localization of events. The names used for events, opcodes, tasks, keyworks and maps 
        /// can be localized to several languages if desired. This works by creating a ResX style string table 
        /// (by simply adding a 'Resource File' to your project). This resource file is given a name e.g. 
        /// 'DefaultNameSpace.ResourceFileName' which can be passed to the ResourceManager constructor to read the 
        /// resoruces. This name is the value of the LocalizationResources property. 
        /// 
        /// LocalizationResources property is non-null, then EventSource will look up the localized strings for events by 
        /// using the following resource naming scheme
        /// 
        ///     event_EVENTNAME
        ///     task_TASKNAME
        ///     keyword_KEYWORDNAME
        ///     map_MAPNAME
        ///     
        /// where the capitpalized name is the name of the event, task, keywork, or map value that should be localized.   
        /// Note that the localized string for an event corresponds to the Messsage string, and can have {0} values 
        /// which represent the payload values.  
        /// </summary>
        public string LocalizationResources { get; set; }
    }

    /// <summary>
    /// None instance methods in a class that subclasses code:EventSource that and return void are
    /// assumed by default to be methods that generate an event. Enough information can be deduced from the
    /// name of the method and its signature to generate basic schema information for the event. The
    /// code:EventAttribute allows you to specify additional event schema information for an event if
    /// desired.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EventAttribute : Attribute
    {
        public EventAttribute(int eventId) { this.EventId = eventId; Level = EventLevel.Informational; }
        public int EventId { get; private set; }
        public EventLevel Level { get; set; }
        public EventKeywords Keywords { get; set; }
        public EventOpcode Opcode { get; set; }
        public EventTask Task { get; set; }
#if FEATURE_MANAGED_ETW_CHANNELS
        public EventChannel Channel { get; set; }
#endif
        public byte Version { get; set; }

        /// <summary>
        /// This is also used for TraceSource compatabilty.  If code:EventSource.TraceSourceSupport is
        /// on events will also be logged a tracesource with the same name as the eventSource.  If this
        /// property is set then the payload will go to code:TraceSource.TraceEvent, and this string
        /// will be used as the message.  If this property is not set not set it goes to
        /// code:TraceSource.TraceData.   You can use standard .NET substitution operators (eg {1}) in 
        /// the string and they will be replaced with the 'ToString()' of the cooresponding part of the
        /// event payload.   
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// By default all instance methods in a class that subclasses code:EventSource that and return
    /// void are assumed to be methods that generate an event. This default can be overriden by specifying
    /// the code:NonEventAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NonEventAttribute : Attribute
    {
        public NonEventAttribute() { }
    }


    // 
#if FEATURE_MANAGED_ETW_CHANNELS
    [AttributeUsage(AttributeTargets.Field)]
    public class ChannelAttribute : Attribute
    {
        public bool Enabled { get; set; }
        public string Isolation { get; set; }
        /// <summary>
        /// Legal values are in ChannelTypes
        /// </summary>
        public string Type { get; set; }
        
        // 
        public string ImportChannel { get; set; }
        // 

    }

    // 

    public static class ChannelTypes
    {
        public const string Admin = "Admin";
        public const string Operational = "Operational";
        public const string Analytic = "Analytic";
        public const string Debug = "Debug";
    }
#endif

    public enum EventCommand
    {
        Update = 0,
        SendManifest = -1,
        Enable = -2,
        Disable = -3
    };


    #region private classes

#if FEATURE_ACTIVITYSAMPLING

    /// <summary>
    /// ActivityFilter is a helper structure that is used to keep track of run-time state
    /// associated with activity filtering. It is 1-1 with EventListeners (logically
    /// every listener has one of these, however we actually allocate them lazily), as well
    /// as 1-to-1 with tracing-aware EtwSessions.
    /// 
    /// This structure also keeps track of the sampling counts associated with 'trigger'
    /// events.  Because these trigger events are rare, and you typically only have one of
    /// them, we store them here as a linked list.
    /// </summary>
    internal sealed class ActivityFilter : IDisposable
    {
        /// <summary>
        /// Disable all activity filtering for the listener associated with 'filterList', 
        /// (in the session associated with it) that is triggered by any event in 'source'.
        /// </summary>
        public static void DisableFilter(ref ActivityFilter filterList, EventSource source)
        {
            Contract.Assert(Monitor.IsEntered(EventListener.EventListenersLock));

            if (filterList == null)
                return;

            ActivityFilter cur;
            // Remove it from anywhere in the list (except the first element, which has to 
            // be treated specially)
            ActivityFilter prev = filterList;
            cur = prev.m_next;
            while (cur != null)
            {
                if (cur.m_providerGuid == source.Guid)
                {
                    // update TriggersActivityTracking bit
                    if (cur.m_eventId >= 0 && cur.m_eventId < source.m_eventData.Length)
                        --source.m_eventData[cur.m_eventId].TriggersActivityTracking;

                    // Remove it from the linked list.
                    prev.m_next = cur.m_next;
                    // dispose of the removed node
                    cur.Dispose();
                    // update cursor
                    cur = prev.m_next;
                }
                else
                {
                    // update cursors
                    prev = cur;
                    cur = prev.m_next;
                }
            }

            // Sadly we have to treat the first element specially in linked list removal in C#
            if (filterList.m_providerGuid == source.Guid)
            {
                // update TriggersActivityTracking bit
                if (filterList.m_eventId >= 0 && filterList.m_eventId < source.m_eventData.Length)
                    --source.m_eventData[filterList.m_eventId].TriggersActivityTracking;

                // We are the first element in the list.   
                var first = filterList;
                filterList = first.m_next;
                // dispose of the removed node
                first.Dispose();
            }
            // the above might have removed the one ActivityFilter in the session that contains the 
            // cleanup delegate; re-create the delegate if needed
            if (filterList != null)
            {
                EnsureActivityCleanupDelegate(filterList);
            }
        }

        /// <summary>
        /// Currently this has "override" semantics. We first disable all filters
        /// associated with 'source', and next we add new filters for each entry in the 
        /// string 'startEvents'. participateInSampling specifies whether non-startEvents 
        /// always trigger or only trigger when current activity is 'active'.
        /// </summary>
        public static void UpdateFilter(
                                    ref ActivityFilter filterList, 
                                    EventSource source, 
                                    int perEventSourceSessionId,
                                    string startEvents)
        {
            Contract.Assert(Monitor.IsEntered(EventListener.EventListenersLock));

            // first remove all filters associated with 'source'
            DisableFilter(ref filterList, source);

            if (!string.IsNullOrEmpty(startEvents))
            {
                // ActivitySamplingStartEvents is a space-separated list of Event:Frequency pairs.
                // The Event may be specified by name or by ID. Errors in parsing such a pair 
                // result in the error being reported to the listeners, and the pair being ignored.
                // E.g. "CustomActivityStart:1000 12:10" specifies that for event CustomActivityStart
                // we should initiate activity tracing once every 1000 events, *and* for event ID 12
                // we should initiate activity tracing once every 10 events.
                string[] activityFilterStrings = startEvents.Split(' ');

                for (int i = 0; i < activityFilterStrings.Length; ++i)
                {
                    string activityFilterString = activityFilterStrings[i];
                    int sampleFreq = 1;
                    int eventId = -1;
                    int colonIdx = activityFilterString.IndexOf(':');
                    if (colonIdx < 0)
                    {
                        source.ReportOutOfBandMessage("ERROR: Invalid ActivitySamplingStartEvent specification: " + 
                            activityFilterString, false);
                        // ignore failure...
                        continue;
                    }
                    string sFreq = activityFilterString.Substring(colonIdx + 1);
                    if (!int.TryParse(sFreq, out sampleFreq))
                    {
                        source.ReportOutOfBandMessage("ERROR: Invalid sampling frequency specification: " + sFreq, false);
                        continue;
                    }
                    activityFilterString = activityFilterString.Substring(0, colonIdx);
                    if (!int.TryParse(activityFilterString, out eventId))
                    {
                        // reset eventId
                        eventId = -1;
                        // see if it's an event name
                        for (int j = 0; j < source.m_eventData.Length; j++)
                        {
                            EventSource.EventMetadata[] ed = source.m_eventData;
                            if (ed[j].Name != null && ed[j].Name.Length == activityFilterString.Length &&
                                string.Compare(ed[j].Name, activityFilterString, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                eventId = ed[j].Descriptor.EventId;
                                break;
                            }
                        }
                    }
                    if (eventId < 0 || eventId >= source.m_eventData.Length)
                    {
                        source.ReportOutOfBandMessage("ERROR: Invalid eventId specification: " + activityFilterString, false);
                        continue;
                    }
                    EnableFilter(ref filterList, source, perEventSourceSessionId, eventId, sampleFreq);
                }
            }
        }

        /// <summary>
        /// Returns the first ActivityFilter from 'filterList' corresponding to 'source'.
        /// </summary>
        public static ActivityFilter GetFilter(ActivityFilter filterList, EventSource source)
        {
            for (var af = filterList; af != null; af = af.m_next)
            {
                if (af.m_providerGuid == source.Guid && af.m_samplingFreq != -1)
                    return af;
            }
            return null;
        }

        /// <summary>
        /// Returns a session mask representing all sessions in which the activity 
        /// associated with the current thread is allowed  through the activity filter. 
        /// If 'triggeringEvent' is true the event MAY be a triggering event. Ideally 
        /// most of the time this is false as you can guarentee this event is NOT a 
        /// triggering event. If 'triggeringEvent' is true, then it checks the 
        /// 'EventSource' and 'eventID' of the event being logged to see if it is actually
        /// a trigger. If so it activates the current activity. 
        /// 
        /// If 'childActivityID' is present, it will be added to the active set if the 
        /// current activity is active.  
        /// </summary>
        [SecurityCritical]
        unsafe public static bool PassesActivityFilter(
                                    ActivityFilter filterList, 
                                    Guid* childActivityID, 
                                    bool triggeringEvent, 
                                    EventSource source, 
                                    int eventId)
        {
            Contract.Assert(filterList != null && filterList.m_activeActivities != null);
            bool shouldBeLogged = false;
            if (triggeringEvent)
            {
                for (ActivityFilter af = filterList; af != null; af = af.m_next)
                {
                    if (eventId == af.m_eventId && source.Guid == af.m_providerGuid)
                    {
                        // Update the sampling count with wrap-around
                        int curSampleCount, newSampleCount;
                        do
                        {
                            curSampleCount = af.m_curSampleCount;
                            if (curSampleCount <= 1)
                                newSampleCount = af.m_samplingFreq;        // Wrap around, counting down to 1
                            else
                                newSampleCount = curSampleCount - 1;
                        }
                        while (Interlocked.CompareExchange(ref af.m_curSampleCount, newSampleCount, curSampleCount) != curSampleCount);
                        // If we hit zero, then start tracking the activity.  
                        if (curSampleCount <= 1)
                        {
                            Guid currentActivityId = EventSource.InternalCurrentThreadActivityId;
                            Tuple<Guid, int> startId;
                            // only add current activity if it's not already a root activity
                            if (!af.m_rootActiveActivities.TryGetValue(currentActivityId, out startId))
                            {
                                // EventSource.OutputDebugString(string.Format("  PassesAF - Triggering(session {0}, evt {1})", af.m_perEventSourceSessionId, eventId));
                                shouldBeLogged = true;
                                af.m_activeActivities[currentActivityId] = Environment.TickCount;
                                af.m_rootActiveActivities[currentActivityId] = Tuple.Create(source.Guid, eventId);
                            }
                        }
                        else
                        {
                            // a start event following a triggering start event
                            Guid currentActivityId = EventSource.InternalCurrentThreadActivityId;
                            Tuple<Guid, int> startId;
                            // only remove current activity if we added it
                            if (af.m_rootActiveActivities.TryGetValue(currentActivityId, out startId) &&
                                startId.Item1 == source.Guid && startId.Item2 == eventId)
                            {
                                // EventSource.OutputDebugString(string.Format("Activity dying: {0} -> StartEvent({1})", currentActivityId, eventId));
                                // remove activity only from current logging scope (af)
                                int dummy;
                                af.m_activeActivities.TryRemove(currentActivityId, out dummy);
                            }                            
                        }
                        break;
                    }
                }
            }

            var activeActivities = GetActiveActivities(filterList);
            if (activeActivities != null)
            {
                // if we hadn't already determined this should be logged, test further
                if (!shouldBeLogged)
                {
                    shouldBeLogged = !activeActivities.IsEmpty && 
                                     activeActivities.ContainsKey(EventSource.InternalCurrentThreadActivityId);
                }
                if (shouldBeLogged && childActivityID != null && 
                    ((EventOpcode)source.m_eventData[eventId].Descriptor.Opcode == EventOpcode.Send))
                {
                    FlowActivityIfNeeded(filterList, null, childActivityID);
                    // EventSource.OutputDebugString(string.Format("  PassesAF - activity {0}", *childActivityID));
                }
            }
            // EventSource.OutputDebugString(string.Format("  PassesAF - shouldBeLogged(evt {0}) = {1:x}", eventId, shouldBeLogged));
            return shouldBeLogged;
        }

        [System.Security.SecuritySafeCritical]
        public static bool IsCurrentActivityActive(ActivityFilter filterList)
        {
            var activeActivities = GetActiveActivities(filterList);
            if (activeActivities != null && 
                activeActivities.ContainsKey(EventSource.InternalCurrentThreadActivityId))
                return true;

            return false;
        }

        /// <summary>
        /// For the EventListener/EtwSession associated with 'filterList', add 'childActivityid'
        /// to list of active activities IF 'currentActivityId' is also active. Passing in a null
        /// value for  'currentActivityid' is an indication tha caller has already verified
        /// that the current activity is active.
        /// </summary>
        [SecurityCritical]
        unsafe public static void FlowActivityIfNeeded(ActivityFilter filterList, Guid *currentActivityId, Guid *childActivityID)
        {
            Contract.Assert(childActivityID != null);

            var activeActivities = GetActiveActivities(filterList);
            Contract.Assert(activeActivities != null);

            // take currentActivityId == null to mean we *know* the current activity is "active"
            if (currentActivityId != null && !activeActivities.ContainsKey(*currentActivityId))
                return;

            if (activeActivities.Count > MaxActivityTrackCount)
            {
                TrimActiveActivityStore(activeActivities);
                // make sure current activity is still in the set:
                activeActivities[EventSource.InternalCurrentThreadActivityId] = Environment.TickCount;
            }
            // add child activity to list of actives
            activeActivities[*childActivityID] = Environment.TickCount;
            
        }

        /// <summary>
        /// </summary>
        public static void UpdateKwdTriggers(ActivityFilter activityFilter, Guid sourceGuid, EventSource source, EventKeywords sessKeywords)
        {
            for (var af = activityFilter; af != null; af = af.m_next)
            {
                if ((sourceGuid == af.m_providerGuid) && 
                    (source.m_eventData[af.m_eventId].TriggersActivityTracking > 0 ||
                    ((EventOpcode)source.m_eventData[af.m_eventId].Descriptor.Opcode == EventOpcode.Send)))
                {
                    // we could be more precise here, if we tracked 'anykeywords' per session
                    source.m_keywordTriggers |= (source.m_eventData[af.m_eventId].Descriptor.Keywords & (long)sessKeywords);
                }
            }
        }

        /// <summary>
        /// For the EventSource specified by 'sourceGuid' and the EventListener/EtwSession 
        /// associated with 'this' ActivityFilter list, return configured sequence of 
        /// [eventId, sampleFreq] pairs that defines the sampling policy.
        /// </summary>
        public IEnumerable<Tuple<int, int>> GetFilterAsTuple(Guid sourceGuid)
        {
            for (ActivityFilter af = this; af != null; af = af.m_next)
            {
                if (af.m_providerGuid == sourceGuid)
                    yield return Tuple.Create(af.m_eventId, af.m_samplingFreq);
            }
        }

        /// <summary>
        /// The cleanup being performed consists of removing the m_myActivityDelegate from
        /// the static s_activityDying, therefore allowing the ActivityFilter to be reclaimed.
        /// </summary>
        public void Dispose()
        {
            Contract.Assert(Monitor.IsEntered(EventListener.EventListenersLock));
            // m_myActivityDelegate is still alive (held by the static EventSource.s_activityDying). 
            // Therefore we are ok to take a dependency on m_myActivityDelegate being valid even 
            // during the finalization of the ActivityFilter
            if (m_myActivityDelegate != null)
            {
                EventSource.s_activityDying = (Action<Guid>)Delegate.Remove(EventSource.s_activityDying, m_myActivityDelegate);
                m_myActivityDelegate = null;
            }
        }

        #region private

        /// <summary>
        /// Creates a new ActivityFilter that is triggered by 'eventId' from 'source' ever
        /// 'samplingFreq' times the event fires. You can have several of these forming a 
        /// linked list.
        /// </summary>
        private ActivityFilter(EventSource source, int perEventSourceSessionId, int eventId, int samplingFreq, ActivityFilter existingFilter = null)
        {
            m_providerGuid = source.Guid;
            m_perEventSourceSessionId = perEventSourceSessionId;
            m_eventId = eventId;
            m_samplingFreq = samplingFreq;
            m_next = existingFilter;

            Contract.Assert(existingFilter == null ||
                            (existingFilter.m_activeActivities == null) == (existingFilter.m_rootActiveActivities == null));

            // if this is the first filter we add for this session, we need to create a new 
            // table of activities. m_activeActivities is common across EventSources in the same
            // session
            ConcurrentDictionary<Guid, int> activeActivities = null;
            if (existingFilter == null || 
                (activeActivities = GetActiveActivities(existingFilter)) == null)
            {
                m_activeActivities = new ConcurrentDictionary<Guid, int>();
                m_rootActiveActivities = new ConcurrentDictionary<Guid, Tuple<Guid, int>>();

                // Add a delegate to the 'SetCurrentThreadToActivity callback so that I remove 'dead' activities
                m_myActivityDelegate = GetActivityDyingDelegate(this);
                EventSource.s_activityDying = (Action<Guid>)Delegate.Combine(EventSource.s_activityDying, m_myActivityDelegate);
            }
            else
            {
                m_activeActivities = activeActivities;
                m_rootActiveActivities = existingFilter.m_rootActiveActivities;
            }

        }

        /// <summary>
        /// Ensure there's at least one ActivityFilter in the 'filterList' that contains an
        /// activity-removing delegate for the listener/session associated with 'filterList'.
        /// </summary>
        private static void EnsureActivityCleanupDelegate(ActivityFilter filterList)
        {
            if (filterList == null)
                return;

            for (ActivityFilter af = filterList; af != null; af = af.m_next)
            {
                if (af.m_myActivityDelegate != null)
                    return;
            }

            // we didn't find a delegate
            filterList.m_myActivityDelegate = GetActivityDyingDelegate(filterList);
            EventSource.s_activityDying = (Action<Guid>)Delegate.Combine(EventSource.s_activityDying, filterList.m_myActivityDelegate);
        }

        /// <summary>
        /// Builds the delegate to be called when an activity is dying. This is responsible
        /// for performing whatever cleanup is needed for the ActivityFilter list passed in.
        /// This gets "added" to EventSource.s_activityDying and ends up being called from
        /// EventSource.SetCurrentThreadActivityId and ActivityFilter.PassesActivityFilter.
        /// </summary>
        /// <returns>The delegate to be called when an activity is dying</returns>
        private static Action<Guid> GetActivityDyingDelegate(ActivityFilter filterList)
        {
            return (Guid oldActivity) =>
            {
                int dummy; 
                filterList.m_activeActivities.TryRemove(oldActivity, out dummy);
                Tuple<Guid, int> dummyTuple;
                filterList.m_rootActiveActivities.TryRemove(oldActivity, out dummyTuple);
            };
        }

        /// <summary>
        /// Enables activity filtering for the listener associated with 'filterList', triggering on
        /// the event 'eventID' from 'source' with a sampling frequency of 'samplingFreq'
        /// 
        /// if 'eventID' is out of range (e.g. negative), it means we are not triggering (but we are 
        /// activitySampling if something else triggered).  
        /// </summary>
        /// <returns>true if activity sampling is enabled the samplingFreq is non-zero </returns>
        private static bool EnableFilter(ref ActivityFilter filterList, EventSource source, int perEventSourceSessionId, int eventId, int samplingFreq)
        {
            Contract.Assert(Monitor.IsEntered(EventListener.EventListenersLock));
            Contract.Assert(samplingFreq > 0);
            Contract.Assert(eventId >= 0);

            filterList = new ActivityFilter(source, perEventSourceSessionId, eventId, samplingFreq, filterList);

            // Mark the 'quick Check' that indicates this is a trigger event.  
            // If eventId is out of range then this mark is not done which has the effect of ignoring 
            // the trigger.
            if (0 <= eventId && eventId < source.m_eventData.Length)
                ++source.m_eventData[eventId].TriggersActivityTracking;

            return true;
        }

        /// <summary>
        /// Normally this code never runs, it is here just to prevent run-away resource usage.  
        /// </summary>
        private static void TrimActiveActivityStore(ConcurrentDictionary<Guid, int> activities)
        {
            if (activities.Count > MaxActivityTrackCount)
            {
                // Remove half of the oldest activity ids.  
                var keyValues = activities.ToArray();
                var tickNow = Environment.TickCount;

                // Sort by age, taking into account wrap-around.   As long as x and y are within 
                // 23 days of now then (0x7FFFFFFF & (tickNow - x.Value)) is the delta (even if 
                // TickCount wraps).  I then sort by DESCENDING age.  (that is oldest value first)
                Array.Sort(keyValues, (x, y) => (0x7FFFFFFF & (tickNow - y.Value)) - (0x7FFFFFFF & (tickNow - x.Value)));
                for (int i = 0; i < keyValues.Length / 2; i++)
                {
                    int dummy;
                    activities.TryRemove(keyValues[i].Key, out dummy);
                }
            }
        }

        private static ConcurrentDictionary<Guid, int> GetActiveActivities(
                                    ActivityFilter filterList)
        {
            for (ActivityFilter af = filterList; af != null; af = af.m_next)
            {
                if (af.m_activeActivities != null)
                    return af.m_activeActivities;
            }
            return null;
        }

        // m_activeActivities always points to the sample dictionary for EVERY ActivityFilter  
        // in the m_next list. The 'int' value in the m_activities set is a timestamp 
        // (Environment.TickCount) of when the entry was put in the system and is used to 
        // remove 'old' entries that if the set gets too big.
        ConcurrentDictionary<Guid, int> m_activeActivities;

        // m_rootActiveActivities holds the "root" active activities, i.e. the activities 
        // that were marked as active because a Start event fired on them. We need to keep
        // track of these to enable sampling in the scenario of an app's main thread that 
        // never explicitly sets distinct activity IDs as it executes. To handle these
        // situations we manufacture a Guid from the thread's ID, and:
        //   (a) we consider the firing of a start event when the sampling counter reaches 
        //       zero to mark the beginning of an interesting activity, and 
        //   (b) we consider the very next firing of the same start event to mark the
        //       ending of that activity.
        // We use a ConcurrentDictionary to avoid taking explicit locks.
        //   The key (a guid) represents the activity ID of the root active activity
        //   The value is made up of the Guid of the event provider and the eventId of
        //      the start event.
        ConcurrentDictionary<Guid, Tuple<Guid, int>> m_rootActiveActivities;
        Guid m_providerGuid;        // We use the GUID rather than object identity because we don't want to keep the eventSource alive
        int m_eventId;              // triggering event
        int m_samplingFreq;         // Counter reset to this when it hits 0
        int m_curSampleCount;       // We count down to 0 and then activate the activity. 
        int m_perEventSourceSessionId;  // session ID bit for ETW, 0 for EventListeners

        const int MaxActivityTrackCount = 100000;   // maximum number of tracked activities

        ActivityFilter m_next;      // We create a linked list of these
        Action<Guid> m_myActivityDelegate;
        #endregion
    };


    /// <summary>
    /// An EtwSession instance represents an activity-tracing-aware ETW session. Since these
    /// are limited to 8 concurrent sessions per machine (currently) we're going to store
    /// the active ones in a singly linked list.
    /// </summary>
    internal class EtwSession
    {
        public static EtwSession GetEtwSession(int etwSessionId, bool bCreateIfNeeded = false)
        {
            if (etwSessionId < 0)
                return null;

            EtwSession etwSession;
            foreach(var wrEtwSession in s_etwSessions)
            {
                if (wrEtwSession.TryGetTarget(out etwSession) && etwSession.m_etwSessionId == etwSessionId)
                    return etwSession;
            }

            if (!bCreateIfNeeded)
                return null;

            if (s_etwSessions == null)
                s_etwSessions = new List<WeakReference<EtwSession>>();

            etwSession = new EtwSession(etwSessionId);
            s_etwSessions.Add(new WeakReference<EtwSession>(etwSession));
            if (s_etwSessions.Count > s_thrSessionCount)
                TrimGlobalList();

            return etwSession;

        }

        public static void RemoveEtwSession(EtwSession etwSession)
        {
            Contract.Assert(etwSession != null);
            if (s_etwSessions == null)
                return;

            s_etwSessions.RemoveAll((wrEtwSession) => 
                {
                    EtwSession session;
                    return wrEtwSession.TryGetTarget(out session) && 
                           (session.m_etwSessionId == etwSession.m_etwSessionId);
                });

            if (s_etwSessions.Count > s_thrSessionCount)
                TrimGlobalList();
        }

        private static void TrimGlobalList()
        {
            if (s_etwSessions == null)
                return;

            s_etwSessions.RemoveAll((wrEtwSession) => 
                {
                    EtwSession session;
                    return !wrEtwSession.TryGetTarget(out session);
                });
        }

        private EtwSession(int etwSessionId)
        {
            m_etwSessionId = etwSessionId;
        }

        public readonly int m_etwSessionId;        // ETW session ID (as retrieved by EventProvider)
        public ActivityFilter m_activityFilter;    // all filters enabled for this session

        private static List<WeakReference<EtwSession>> s_etwSessions = new List<WeakReference<EtwSession>>();
        private const  int s_thrSessionCount = 16;
    }

#endif // FEATURE_ACTIVITYSAMPLING

    // holds a bitfield representing a session mask
    /// <summary>
    /// A SessionMask represents a set of (at most MAX) sessions as a bit mask. The perEventSourceSessionId
    /// is the index in the SessionMask of the bit that will be set. These can translate to
    /// EventSource's reserved keywords bits using the provided ToEventKeywords() and
    /// FromEventKeywords() methods.
    /// </summary>
    internal struct SessionMask
    {
        public SessionMask(SessionMask m)
        { m_mask = m.m_mask; }

        public SessionMask(uint mask = 0)
        { m_mask = mask & MASK; }

        public bool IsEqualOrSupersetOf(SessionMask m)
        {
            return (this.m_mask | m.m_mask) == this.m_mask;
        }

        public static SessionMask All
        { 
            get { return new SessionMask(MASK); }
        }

        public static SessionMask FromId(int perEventSourceSessionId)
        {
            Contract.Assert(perEventSourceSessionId < MAX);
            return new SessionMask((uint) 1 << perEventSourceSessionId);
        }

        public ulong ToEventKeywords()
        {
            return (ulong)m_mask << SHIFT_SESSION_TO_KEYWORD;
        }

        public static SessionMask FromEventKeywords(ulong m)
        {
            return new SessionMask((uint)(m >> SHIFT_SESSION_TO_KEYWORD));
        }
        
        public bool this[int perEventSourceSessionId]
        {
            get 
            {
                Contract.Assert(perEventSourceSessionId < MAX);
                return (m_mask & (1 << perEventSourceSessionId)) != 0; 
            }
            set 
            { 
                Contract.Assert(perEventSourceSessionId < MAX);
                if (value) m_mask |= ((uint) 1 << perEventSourceSessionId);
                else m_mask &= ~((uint) 1 << perEventSourceSessionId); 
            }
        }

        public static SessionMask operator | (SessionMask m1, SessionMask m2)
        {
            return new SessionMask(m1.m_mask | m2.m_mask);
        }

        public static SessionMask operator & (SessionMask m1, SessionMask m2)
        {
            return new SessionMask(m1.m_mask & m2.m_mask);
        }

        public static SessionMask operator ^ (SessionMask m1, SessionMask m2)
        {
            return new SessionMask(m1.m_mask ^ m2.m_mask);
        }

        public static SessionMask operator ~(SessionMask m)
        {
            return new SessionMask(MASK & ~(m.m_mask));
        }

        public static explicit operator ulong(SessionMask m)
        { return m.m_mask; }

        public static explicit operator uint(SessionMask m)
        { return m.m_mask; }

        private uint m_mask;

        internal const int   SHIFT_SESSION_TO_KEYWORD = 44;         // bits 44-47 inclusive are reserved
        internal const uint  MASK                     = 0x0fU;      // the mask of 4 reserved bits
        internal const uint  MAX                      = 4;          // maximum number of simultaneous ETW sessions supported
                                                                    // (equals bitcount(MASK))
    }

    /// <summary>
    /// code:EventDispatchers are a simple 'helper' structure that holds the filtering state
    /// (m_EventEnabled) for a particular EventSource X EventListener tuple
    /// 
    /// Thus a single EventListener may have many EventDispatchers (one for every EventSource 
    /// that that EventListener has activate) and a Single EventSource may also have many
    /// event Dispatchers (one for every EventListener that has activated it). 
    /// 
    /// Logically a particular EventDispatcher belongs to exactly one EventSource and exactly  
    /// one EventListener (alhtough EventDispatcher does not 'remember' the EventSource it is
    /// associated with. 
    /// </summary>
    internal class EventDispatcher
    {
        internal EventDispatcher(EventDispatcher next, bool[] eventEnabled, EventListener listener)
        {
            m_Next = next;
            m_EventEnabled = eventEnabled;
            m_Listener = listener;
        }

        // Instance fields
        readonly internal EventListener m_Listener;   // The dispatcher this entry is for
        internal bool[] m_EventEnabled;               // For every event in a the eventSource, is it enabled?
#if FEATURE_ACTIVITYSAMPLING
        internal bool m_activityFilteringEnabled;     // does THIS EventSource have activity filtering turned on for this listener?
#endif // FEATURE_ACTIVITYSAMPLING

        // Only guarenteed to exist after a InsureInit()
        internal EventDispatcher m_Next;              // These form a linked list in code:EventSource.m_Dispatchers
        // Of all listeners for that eventSource.  
    }

    /// <summary>
    /// ManifestBuilder is designed to isolate the details of the message of the event from the
    /// rest of EventSource.  This one happens to create XML. 
    /// </summary>
    internal class ManifestBuilder
    {
        /// <summary>
        /// Build a manifest for 'providerName' with the given GUID, which will be packaged into 'dllName'.
        /// 'resources, is a resource manager.  If specified all messsages are localized using that manager.  
        /// </summary>
        public ManifestBuilder(string providerName, Guid providerGuid, string dllName, ResourceManager resources)
        {
#if FEATURE_MANAGED_ETW_CHANNELS
            this.providerName = providerName;
#endif
            this.resources = resources;
            sb = new StringBuilder();
            events = new StringBuilder();
            templates = new StringBuilder();
            opcodeTab = new Dictionary<int, string>();
            stringTab = new Dictionary<string, string>();

            sb.AppendLine("<instrumentationManifest xmlns=\"http://schemas.microsoft.com/win/2004/08/events\">");
            sb.AppendLine(" <instrumentation xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:win=\"http://manifests.microsoft.com/win/2004/08/windows/events\">");
            sb.AppendLine("  <events xmlns=\"http://schemas.microsoft.com/win/2004/08/events\">");
            sb.Append("<provider name=\"").Append(providerName).
               Append("\" guid=\"{").Append(providerGuid.ToString()).Append("}");
            if (dllName != null)
                sb.Append("\" resourceFileName=\"").Append(dllName).Append("\" messageFileName=\"").Append(dllName);

            var symbolsName = providerName.Replace("-", "");
            sb.Append("\" symbol=\"").Append(symbolsName);
            sb.Append("\">").AppendLine();
        }

        public void AddOpcode(string name, int value)
        {
            opcodeTab[value] = name;
        }
        public void AddTask(string name, int value)
        {
            if (taskTab == null)
                taskTab = new Dictionary<int, string>();
            taskTab[value] = name;
        }
        public void AddKeyword(string name, ulong value)
        {
            if ((value & (value - 1)) != 0)   // Is it a power of 2?
                throw new ArgumentException(Environment.GetResourceString("EventSource_KeywordNeedPowerOfTwo", value.ToString("x", CultureInfo.CurrentCulture), name));
            if (keywordTab == null)
                keywordTab = new Dictionary<ulong, string>();
            keywordTab[value] = name;
        }

#if FEATURE_MANAGED_ETW_CHANNELS
        /// <summary>
        /// Add a channel.  channelAttribute can be null
        /// </summary>
        public void AddChannel(string name, int value, ChannelAttribute channelAttribute)
        {
            if (channelTab == null)
                channelTab = new Dictionary<int, ChannelInfo>();
            channelTab[value] = new ChannelInfo { Name = name, Attribs = channelAttribute };
        }
#endif
        public void StartEvent(string eventName, EventAttribute eventAttribute)
        {
            Contract.Assert(numParams == 0);
            Contract.Assert(templateName == null);
            templateName = eventName + "Args";
            numParams = 0;

            events.Append("  <event").
                 Append(" value=\"").Append(eventAttribute.EventId).Append("\"").
                 Append(" version=\"").Append(eventAttribute.Version).Append("\"").
                 Append(" level=\"").Append(GetLevelName(eventAttribute.Level)).Append("\"");

            WriteMessageAttrib(events, "event", eventName, eventAttribute.Message);

            if (eventAttribute.Keywords != 0)
                events.Append(" keywords=\"").Append(GetKeywords((ulong)eventAttribute.Keywords, eventName)).Append("\"");
            if (eventAttribute.Opcode != 0)
                events.Append(" opcode=\"").Append(GetOpcodeName(eventAttribute.Opcode, eventName)).Append("\"");
            if (eventAttribute.Task != 0)
                events.Append(" task=\"").Append(GetTaskName(eventAttribute.Task, eventName)).Append("\"");
#if FEATURE_MANAGED_ETW_CHANNELS
            if (eventAttribute.Channel != 0)
                events.Append(" channel=\"").Append(GetChannelName(eventAttribute.Channel, eventName)).Append("\"");
#endif
        }

        public void AddEventParameter(Type type, string name)
        {
            if (numParams == 0)
                templates.Append("  <template tid=\"").Append(templateName).Append("\">").AppendLine();
            numParams++;
            templates.Append("   <data name=\"").Append(name).Append("\" inType=\"").Append(GetTypeName(type)).Append("\"");
            if (type.IsEnum)
            {
                templates.Append(" map=\"").Append(type.Name).Append("\"");
                if (mapsTab == null)
                    mapsTab = new Dictionary<string, Type>();
                if (!mapsTab.ContainsKey(type.Name))
                    mapsTab.Add(type.Name, type);        // Remember that we need to dump the type enumeration  
            }

            templates.Append("/>").AppendLine();
        }
        public void EndEvent()
        {
            if (numParams > 0)
            {
                templates.Append("  </template>").AppendLine();
                events.Append(" template=\"").Append(templateName).Append("\"");
            }
            events.Append("/>").AppendLine();

            templateName = null;
            numParams = 0;
        }

        public byte[] CreateManifest()
        {
            string str = CreateManifestString();
            return Encoding.UTF8.GetBytes(str);
        }
        private string CreateManifestString()
        {

#if FEATURE_MANAGED_ETW_CHANNELS
            // Write out the channels
            if (channelTab != null)
            {
                sb.Append(" <channels>").AppendLine();
                var sortedChannels = new List<int>(channelTab.Keys);
                sortedChannels.Sort();
                foreach (int channel in sortedChannels)
                {
                    var channelInfo = channelTab[channel];

                    // 
                    string channelType = null;
                    string elementName = "channel";
                    bool enabled = false;
                    string isolation = null;
                    string fullName = null;
                    if (channelInfo.Attribs != null)
                    {
                        var attribs = channelInfo.Attribs;
                        channelType = attribs.Type;
                        if (attribs.ImportChannel != null)
                        {
                            fullName = attribs.ImportChannel;
                            elementName = "importChannel";
                        }
                        enabled = attribs.Enabled;
                        isolation = attribs.Isolation;
                    }
                    if (fullName == null)
                        fullName = providerName + "/" + channelType;


                    sb.Append("  <").Append(elementName);
                    sb.Append(" chid=\"").Append(channelInfo.Name).Append("\"");
                    sb.Append(" name=\"").Append(fullName).Append("\"");
                    sb.Append(" value=\"").Append(channel).Append("\"");
                    if (elementName == "channel")   // not applicable to importChannels.  
                    {
                        if (channelType != null)
                            sb.Append(" type=\"").Append(channelType).Append("\"");
                        sb.Append(" enabled=\"").Append(enabled.ToString()).Append("\"");
                        if (isolation != null)
                            sb.Append(" isolation=\"").Append(isolation).Append("\"");
                    }
                    sb.Append("/>").AppendLine();
                }
                sb.Append(" </channels>").AppendLine();
            }
#endif

            // Write out the tasks
            if (taskTab != null)
            {

                sb.Append(" <tasks>").AppendLine();
                var sortedTasks = new List<int>(taskTab.Keys);
                sortedTasks.Sort();
                foreach (int task in sortedTasks)
                {
                    sb.Append("  <task name=\"").Append(taskTab[task]).
                        Append("\" value=\"").Append(task).
                        Append("\"/>").AppendLine();
                }
                sb.Append(" </tasks>").AppendLine();
            }

            // Write out the maps
            if (mapsTab != null)
            {
                sb.Append(" <maps>").AppendLine();
                foreach (Type enumType in mapsTab.Values)
                {
                    string mapKind = EventSource.GetCustomAttributeHelper(enumType, typeof(FlagsAttribute)) != null ? "bitMap" : "valueMap";
                    sb.Append("  <").Append(mapKind).Append(" name=\"").Append(enumType.Name).Append("\">").AppendLine();

                    // write out each enum value 
                    FieldInfo[] staticFields = enumType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo staticField in staticFields)
                    {
                        object constantValObj = staticField.GetRawConstantValue();
                        if (constantValObj != null)
                        {
                            string hexStr = null;
                            if (constantValObj is int)
                                hexStr = ((int)constantValObj).ToString("x", CultureInfo.InvariantCulture);
                            else if (constantValObj is long)
                                hexStr = ((long)constantValObj).ToString("x", CultureInfo.InvariantCulture);
                            sb.Append("   <map value=\"0x").Append(hexStr).Append("\"");
                            WriteMessageAttrib(sb, "map", enumType.Name + "." + staticField.Name, staticField.Name);
                            sb.Append("/>").AppendLine();
                        }
                    }
                    sb.Append("  </").Append(mapKind).Append(">").AppendLine();
                }
                sb.Append(" </maps>").AppendLine();
            }

            // Write out the opcodes
            sb.Append(" <opcodes>").AppendLine();
            var sortedOpcodes = new List<int>(opcodeTab.Keys);
            sortedOpcodes.Sort();
            foreach (int opcode in sortedOpcodes)
            {
                sb.Append("  <opcode");
                WriteNameAndMessageAttribs(sb, "opcode", opcodeTab[opcode]);
                sb.Append(" value=\"").Append(opcode).Append("\"/>").AppendLine();
            }
            sb.Append(" </opcodes>").AppendLine();

            // Write out the keywords
            if (keywordTab != null)
            {
                sb.Append(" <keywords>").AppendLine();
                var sortedKeywords = new List<ulong>(keywordTab.Keys);
                sortedKeywords.Sort();
                foreach (ulong keyword in sortedKeywords)
                {
                    sb.Append("  <keyword");
                    WriteNameAndMessageAttribs(sb, "keyword", keywordTab[keyword]);
                    sb.Append(" mask=\"0x").Append(keyword.ToString("x", CultureInfo.InvariantCulture)).Append("\"/>").AppendLine();
                }
                sb.Append(" </keywords>").AppendLine();
            }

            sb.Append(" <events>").AppendLine();
            sb.Append(events);
            sb.Append(" </events>").AppendLine();

            if (templates.Length > 0)
            {
                sb.Append(" <templates>").AppendLine();
                sb.Append(templates);
                sb.Append(" </templates>").AppendLine();
            }
            sb.Append("</provider>").AppendLine();
            sb.Append("</events>").AppendLine();
            sb.Append("</instrumentation>").AppendLine();

            // Output the localization information.  
            sb.Append("<localization>").AppendLine();

            // 
            sb.Append(" <resources culture=\"").Append(CultureInfo.CurrentUICulture.Name).Append("\">").AppendLine();
            sb.Append("  <stringTable>").AppendLine();

            var sortedStrings = new string[stringTab.Keys.Count];
            stringTab.Keys.CopyTo(sortedStrings, 0);
            // Avoid using public Array.Sort as that attempts to access BinaryCompatibility. Unfortunately FrameworkEventSource gets called 
            // very early in the app domain creation, when _FusionStore is not set up yet, resulting in a failure to run the static constructory
            // for BinaryCompatibility. This failure is then cached and a TypeInitializationException is thrown every time some code attampts to
            // access BinaryCompatibility.
            ArraySortHelper<string>.IntrospectiveSort(sortedStrings, 0, sortedStrings.Length, Comparer<string>.Default);
            foreach (var stringKey in sortedStrings)
                sb.Append("   <string id=\"").Append(stringKey).Append("\" value=\"").Append(stringTab[stringKey]).Append("\"/>").AppendLine();
            sb.Append("  </stringTable>").AppendLine();
            sb.Append(" </resources>").AppendLine();
            sb.Append("</localization>").AppendLine();
            sb.AppendLine("</instrumentationManifest>");
            return sb.ToString();
        }

        #region private
        private void WriteNameAndMessageAttribs(StringBuilder stringBuilder, string elementName, string name)
        {
            stringBuilder.Append(" name=\"").Append(name).Append("\" ");
            WriteMessageAttrib(sb, elementName, name, name);
        }
        private void WriteMessageAttrib(StringBuilder stringBuilder, string elementName, string name, string value)
        {
            string key = elementName + "_" + name;
            // See if the user wants things localized.  
            if (resources != null)
            {
                string localizedString = resources.GetString(key);
                if (localizedString != null)
                    value = localizedString;
            }
            if (value == null)
                return;
            if (elementName == "event")
                value = TranslateToManifestConvention(value);

            stringBuilder.Append(" message=\"$(string.").Append(key).Append(")\"");
            stringTab.Add(key, value);
        }

        private static string GetLevelName(EventLevel level)
        {
            return (((int)level >= 16) ? "" : "win:") + level.ToString();
        }

#if FEATURE_MANAGED_ETW_CHANNELS
        private string GetChannelName(EventChannel channel, string eventName)
        {
               ChannelInfo info = null;
            if (channelTab == null || !channelTab.TryGetValue((int)channel, out info))
                throw new ArgumentException(Environment.GetResourceString("EventSource_UndefinedChannel", channel, eventName));
            return info.Name;
        }
#endif
        private string GetTaskName(EventTask task, string eventName)
        {
            if (task == EventTask.None)
                return "";

            string ret;
            if (taskTab == null)
                taskTab = new Dictionary<int, string>();
            if (!taskTab.TryGetValue((int)task, out ret))
                ret = taskTab[(int)task] = eventName;
            return ret;
        }
        private string GetOpcodeName(EventOpcode opcode, string eventName)
        {
            switch (opcode)
            {
                case EventOpcode.Info:
                    return "win:Info";
                case EventOpcode.Start:
                    return "win:Start";
                case EventOpcode.Stop:
                    return "win:Stop";
                case EventOpcode.DataCollectionStart:
                    return "win:DC_Start";
                case EventOpcode.DataCollectionStop:
                    return "win:DC_Stop";
                case EventOpcode.Extension:
                    return "win:Extension";
                case EventOpcode.Reply:
                    return "win:Reply";
                case EventOpcode.Resume:
                    return "win:Resume";
                case EventOpcode.Suspend:
                    return "win:Suspend";
                case EventOpcode.Send:
                    return "win:Send";
                case EventOpcode.Receive:
                    return "win:Receive";
            }

            string ret;
            if (opcodeTab == null || !opcodeTab.TryGetValue((int)opcode, out ret))
                throw new ArgumentException(Environment.GetResourceString("EventSource_UndefinedOpcode", opcode, eventName));
            return ret;
        }
        private string GetKeywords(ulong keywords, string eventName)
        {
            string ret = "";
            for (ulong bit = 1; bit != 0; bit <<= 1)
            {
                if ((keywords & bit) != 0)
                {
                    string keyword;
                    if (keywordTab == null || !keywordTab.TryGetValue(bit, out keyword))
                        throw new ArgumentException(Environment.GetResourceString("EventSource_UndefinedKeyword", bit.ToString("x", CultureInfo.CurrentCulture), eventName));
                    if (ret.Length != 0)
                        ret = ret + " ";
                    ret = ret + keyword;
                }
            }
            return ret;
        }
        private static string GetTypeName(Type type)
        {
            if (type.IsEnum)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                var typeName = GetTypeName(fields[0].FieldType);
                return typeName.Replace("win:Int", "win:UInt"); // ETW requires enums to be unsigned.  
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "win:Boolean";
                case TypeCode.Byte:
                    return "win:UInt8";
                case TypeCode.UInt16:
                    return "win:UInt16";
                case TypeCode.UInt32:
                    return "win:UInt32";
                case TypeCode.UInt64:
                    return "win:UInt64";
                case TypeCode.SByte:
                    return "win:Int8";
                case TypeCode.Int16:
                    return "win:Int16";
                case TypeCode.Int32:
                    return "win:Int32";
                case TypeCode.Int64:
                    return "win:Int64";
                case TypeCode.String:
                    return "win:UnicodeString";
                case TypeCode.Single:
                    return "win:Float";
                case TypeCode.Double:
                    return "win:Double";
                case TypeCode.DateTime:
                    return "win:FILETIME";
                default:
                    if (type == typeof(Guid))
                        return "win:GUID";
                    throw new ArgumentException(Environment.GetResourceString("EventSource_UnsupportedEventTypeInManifest", type.Name));
            }
        }

        // Manifest messages use %N conventions for their message substitutions.   Translate from
        // .NET conventions.   We can't use RegEx for this (we are in mscorlib), so we do it 'by hand' 
        private static string TranslateToManifestConvention(string eventMessage)
        {
            StringBuilder stringBuilder = null;        // We lazily create this 
            int writtenSoFar = 0;
            int chIdx = -1;
            for (int i = 0; ; )
            {
                if (i >= eventMessage.Length)
                {
                    if (stringBuilder == null)
                        return eventMessage;
                    stringBuilder.Append(eventMessage, writtenSoFar, i - writtenSoFar);
                    return stringBuilder.ToString();
                }
                if (eventMessage[i] == '{')
                {
                    int leftBracket = i;
                    i++;
                    int argNum = 0;
                    while (i < eventMessage.Length && Char.IsDigit(eventMessage[i]))
                    {
                        argNum = argNum * 10 + eventMessage[i] - '0';
                        i++;
                    }
                    if (i < eventMessage.Length && eventMessage[i] == '}')
                    {
                        i++;
                        if (stringBuilder == null)
                            stringBuilder = new StringBuilder();
                        stringBuilder.Append(eventMessage, writtenSoFar, leftBracket - writtenSoFar);
                        stringBuilder.Append('%').Append(argNum + 1);
                        writtenSoFar = i;
                    }
                }
                else if ((chIdx = "&<>'\"".IndexOf(eventMessage[i])) >= 0)
                {
                    string[] xmlEscapes = {"&amp;", "&lt;", "&gt;", "&apos;", "&quot;"};
                    var update = new Action<char, string>(
                        (ch, escape) => {
                            if (stringBuilder == null)
                                stringBuilder = new StringBuilder();
                            stringBuilder.Append(eventMessage, writtenSoFar, i - writtenSoFar);
                            i++;
                            stringBuilder.Append(escape);
                            writtenSoFar = i;
                        });
                    update(eventMessage[i], xmlEscapes[chIdx]);
                }
                else
                    i++;
            }
        }

#if FEATURE_MANAGED_ETW_CHANNELS
        class ChannelInfo
        {
            public string Name;
            public ChannelAttribute Attribs;
        }
#endif

        Dictionary<int, string> opcodeTab;
        Dictionary<int, string> taskTab;
#if FEATURE_MANAGED_ETW_CHANNELS
        Dictionary<int, ChannelInfo> channelTab;
#endif
        Dictionary<ulong, string> keywordTab;
        Dictionary<string, Type> mapsTab;

        Dictionary<string, string> stringTab;       // Maps unlocalized strings to localized ones  

        StringBuilder sb;               // Holds the provider information. 
        StringBuilder events;           // Holds the events. 
        StringBuilder templates;

#if FEATURE_MANAGED_ETW_CHANNELS
        string providerName;
#endif
        ResourceManager resources;      // Look up localized strings here.  

        // State we track between StartEvent and EndEvent.  
        string templateName;            // Every event gets its own template (eventName + "Args") this hold it. 
        int numParams;                  // keeps track of the number of args the event has. 
        #endregion
    }

    /// <summary>
    /// Used to send the m_rawManifest into the event dispatcher as a series of events.  
    /// </summary>
    internal struct ManifestEnvelope
    {
        public const int MaxChunkSize = 0xFF00;
        public enum ManifestFormats : byte
        {
            SimpleXmlFormat = 1,          // simply dump the XML manifest as UTF8
        }

        public ManifestFormats Format;
        public byte MajorVersion;
        public byte MinorVersion;
        public byte Magic;
        public ushort TotalChunks;
        public ushort ChunkNumber;
    };

    #endregion
}

