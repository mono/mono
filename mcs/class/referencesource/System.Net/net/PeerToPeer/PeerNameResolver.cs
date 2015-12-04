//------------------------------------------------------------------------------
// <copyright file="PeerNameResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Net;
    using System.Net.Sockets;
    using System.Diagnostics;
    
    /// <summary>
    /// This is the event args class we give back each time when 
    /// we have incremental resolution results
    /// </summary>
    public class ResolveProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private PeerNameRecord m_PeerNameRecord;

        /// <summary>
        /// We use progress percentage of **0** all times sice
        /// we will not no upfront how many records we are going to get
        /// </summary>
        /// <param name="peerNameRecord"></param>
        /// <param name="userToken"></param>
        public ResolveProgressChangedEventArgs(PeerNameRecord peerNameRecord,
                                                object userToken) : base(0, userToken) 
        {
            m_PeerNameRecord = peerNameRecord;
        }
        public PeerNameRecord PeerNameRecord
        {
            get
            {
                return m_PeerNameRecord;
            }
        }
    }

    /// <summary>
    /// When the resolution completes, we invoke the callback with this event args instance
    /// </summary>
    public class ResolveCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PeerNameRecordCollection m_PeerNameRecordCollection;
        public ResolveCompletedEventArgs(
                                                PeerNameRecordCollection peerNameRecordCollection,
                                                Exception error,
                                                bool canceled,
                                                object userToken)
            : base(error, canceled, userToken)
        {
            m_PeerNameRecordCollection = peerNameRecordCollection;
        }
        public PeerNameRecordCollection PeerNameRecordCollection
        {
            get
            {
                return m_PeerNameRecordCollection;
            }
        }
    }

    
    internal class PeerNameResolverHelper : IDisposable
    {
        private const UInt32 FACILITY_P2P = 99;
        private const UInt32 NO_MORE_RECORDS = 0x4003;
        private const int PEER_E_NO_MORE = (int)(((int)1 << 31) | ((int)FACILITY_P2P << 16) | NO_MORE_RECORDS); 
     

        //------------------------------------------
        //userState the user has supplied
        //------------------------------------------
        internal object m_userState;

        //------------------------------------------
        //Handle to the resolution process
        //------------------------------------------
        internal SafePeerNameEndResolve m_SafePeerNameEndResolve;

        //------------------------------------------
        //Event that the native API sets to indicate that 
        //information is available and that we should call 
        //the PeerPnrpGetEndPoint() to get the end point
        //------------------------------------------
        internal AutoResetEvent m_EndPointInfoAvailableEvent = new AutoResetEvent(false);

        //------------------------------------------
        //The WaitHandle that hooks up a callback to the 
        //event
        //------------------------------------------
        internal RegisteredWaitHandle m_RegisteredWaitHandle;

        //------------------------------------------
        //PeerName that is being resolved
        //------------------------------------------
        internal PeerName m_PeerName;

        //------------------------------------------
        //Cloud in which the resolution must occur
        //------------------------------------------
        internal Cloud m_Cloud;

        //------------------------------------------
        //Max number of records to resolve
        //------------------------------------------
        internal int m_MaxRecords;

        //------------------------------------------
        //Disposed or not
        //------------------------------------------
        internal bool m_Disposed;


        //-----------------------------------------
        //Flag to indicate completed or an exception
        //happened. If you set this flag you own
        //calling the callback
        //-----------------------------------------
        internal bool m_CompletedOrException;

        //-----------------------------------------
        //Flag to indicate that the call is canceled
        //If you set this flag you own calling the callback
        //-----------------------------------------
        internal bool m_Cancelled;

        //------------------------------------------
        //A place to save the incremental results 
        //so that we can invoke the completed 
        //handler with all the results at once
        //------------------------------------------
        PeerNameRecordCollection m_PeerNameRecordCollection = new PeerNameRecordCollection();

        //------------------------------------------
        //Async operation to ensure synchornization
        //context
        //------------------------------------------
        AsyncOperation m_AsyncOp;

        //------------------------------------------
        //A link to the resolver to avoid 
        //circular dependencies and enable GC 
        //------------------------------------------
        WeakReference m_PeerNameResolverWeakReference;

        //------------------------------------------
        //Lock to make sure things don't mess up stuff
        //------------------------------------------
        object m_Lock = new Object();

        //------------------------------------------
        //EventID or Just a trackig id
        //------------------------------------------
        int m_TraceEventId;

        internal PeerNameResolverHelper(PeerName peerName, Cloud cloud, int MaxRecords, object userState, PeerNameResolver parent, int NewTraceEventId)
        {
            m_userState = userState;
            m_PeerName = peerName;
            m_Cloud = cloud;
            m_MaxRecords = MaxRecords;
            m_PeerNameResolverWeakReference = new WeakReference(parent);
            m_TraceEventId = NewTraceEventId;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "New PeerNameResolverHelper created with TraceEventID {0}", m_TraceEventId);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                "\tPeerName: {0}, Cloud: {1}, MaxRecords: {2}, userState {3}, ParentReference {4}",
                m_PeerName, 
                m_Cloud, 
                m_MaxRecords, 
                userState.GetHashCode(), 
                m_PeerNameResolverWeakReference.Target.GetHashCode()
                );
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsClosed():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpStartResolve(System.String,System.String,System.UInt32,Microsoft.Win32.SafeHandles.SafeWaitHandle,System.Net.PeerToPeer.SafePeerNameEndResolve&):System.Int32" />
        // <ReferencesCritical Name="Method: EndPointInfoAvailableCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_SafePeerNameEndResolve" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void StartAsyncResolve()
        {
            //------------------------------------------
            //Check for disposal
            //------------------------------------------
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            //------------------------------------------
            //First wire up a callback
            //------------------------------------------
            m_RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_EndPointInfoAvailableEvent, //Event that triggers the callback
                                                    new WaitOrTimerCallback(EndPointInfoAvailableCallback), //callback to be called 
                                                    null, //state to be passed
                                                    -1,   //Timeout - aplicable only for timers not for events 
                                                    false //call us everytime the event is set not just one time
                                                    );

            //------------------------------------------
            //Now call the native API to start the resolution 
            //process save the handle for later
            //------------------------------------------
            Int32 result = UnsafeP2PNativeMethods.PeerPnrpStartResolve(m_PeerName.ToString(),
                                                        m_Cloud.InternalName,
                                                        (UInt32)m_MaxRecords,
                                                        m_EndPointInfoAvailableEvent.SafeWaitHandle, 
                                                        out m_SafePeerNameEndResolve);
            if (result != 0)
            {
                if (!m_SafePeerNameEndResolve.IsInvalid && !m_SafePeerNameEndResolve.IsClosed)
                {
                    m_SafePeerNameEndResolve.Dispose();
                }
                m_RegisteredWaitHandle.Unregister(null);
                m_RegisteredWaitHandle = null;
                PeerToPeerException ex = PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotStartNameResolution), result);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, m_TraceEventId,
                            "Exception occurred while starting async resolve");
                throw ex;
            }

            //------------------------------------------
            //Create an async operation with the given 
            //user state
            //------------------------------------------
            m_AsyncOp = AsyncOperationManager.CreateOperation(m_userState);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                        "Successfully started the async resolve. The native handle is {0}", m_SafePeerNameEndResolve.DangerousGetHandle());

        }

        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pEndPointInfo of type: PEER_PNRP_ENDPOINT_INFO*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="Marshal.ReadIntPtr(System.IntPtr):System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.SizeOf(System.Type):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpGetEndpoint(System.IntPtr,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <ReferencesCritical Name="Local shEndPointInfo of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Field: m_SafePeerNameEndResolve" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void EndPointInfoAvailableCallback(object state, bool timedOut)
        {
            //------------------------------------------
            //This callback is called whenever there is an endpoint info
            //available or the resultion is completed
            //------------------------------------------
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                        "EndPointInfoAvailableCallback called");
            PeerNameRecord record = null;
            SafePeerData shEndPointInfo;
            Int32 result = 0;
            PeerNameResolver parent = null;
            if (m_Cancelled)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                            "Detected that the async operation is already canceled  - before entering the lock");
                return;
            }
            lock (m_Lock)
            {
                if (m_Cancelled)
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                "Detected that the async operation is already canceled - after entering the lock");
                    return;
                }
                result = UnsafeP2PNativeMethods.PeerPnrpGetEndpoint(m_SafePeerNameEndResolve.DangerousGetHandle(), out shEndPointInfo);
                if (result != 0)
                {
                    if (result == PEER_E_NO_MORE)
                    {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                    "Native API returned that there are no more records - resolve completed successfully");
                    }
                    m_CompletedOrException = true;
                    m_SafePeerNameEndResolve.Dispose();
                }
                else
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                "Proceeding to retrieve the endpoint information from incremental resolve");
                    try
                    {
                        unsafe
                        {
                            PEER_PNRP_ENDPOINT_INFO* pEndPointInfo = (PEER_PNRP_ENDPOINT_INFO*)shEndPointInfo.DangerousGetHandle();
                            record = new PeerNameRecord();
                            record.PeerName = new PeerName(Marshal.PtrToStringUni(pEndPointInfo->pwszPeerName));
                            string comment = Marshal.PtrToStringUni(pEndPointInfo->pwszComment);
                            if (comment != null && comment.Length > 0)
                            {
                                record.Comment = comment;
                            }
                            if (pEndPointInfo->payLoad.cbPayload != 0)
                            {
                                record.Data = new byte[pEndPointInfo->payLoad.cbPayload];
                                Marshal.Copy(pEndPointInfo->payLoad.pbPayload, record.Data, 0, (int)pEndPointInfo->payLoad.cbPayload);
                            }
                            //record.EndPointList = new IPEndPoint[pEndPointInfo->cAddresses];
                            IntPtr ppSOCKADDRs = pEndPointInfo->ArrayOfSOCKADDRIN6Pointers;
                            for (UInt32 j = 0; j < pEndPointInfo->cAddresses; j++)
                            {
                                IntPtr pSOCKADDR = Marshal.ReadIntPtr(ppSOCKADDRs);

                                byte[] AddressFamilyBuffer = new byte[2];
                                Marshal.Copy(pSOCKADDR, AddressFamilyBuffer, 0, 2);
                                int addressFamily = 0;
#if BIGENDIAN
                            addressFamily = AddressFamilyBuffer[1] + ((int)AddressFamilyBuffer[0] << 8);
#else
                                addressFamily = AddressFamilyBuffer[0] + ((int)AddressFamilyBuffer[1] << 8);
#endif
                                byte[] buffer = new byte[((AddressFamily)addressFamily == AddressFamily.InterNetwork) ? SystemNetHelpers.IPv4AddressSize : SystemNetHelpers.IPv6AddressSize];
                                Marshal.Copy(pSOCKADDR, buffer, 0, buffer.Length);
                                IPEndPoint ipe = SystemNetHelpers.IPEndPointFromSOCKADDRBuffer(buffer);
                                record.EndPointCollection.Add(ipe);
                                ppSOCKADDRs = (IntPtr)((long)ppSOCKADDRs + Marshal.SizeOf(typeof(IntPtr)));
                            }
                        }
                    }
                    finally
                    {
                        shEndPointInfo.Dispose();
                    }
                    record.TracePeerNameRecord();
                    m_PeerNameRecordCollection.Add(record);

                    ResolveProgressChangedEventArgs resolveProgressChangedEventArgs = new ResolveProgressChangedEventArgs(
                                                                            record, m_AsyncOp.UserSuppliedState);


                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                "Proceeding to call progress changed event callback");
                    parent = m_PeerNameResolverWeakReference.Target as PeerNameResolver;
                    if (parent != null)
                    {
                        parent.PrepareToRaiseProgressChangedEvent(m_AsyncOp, resolveProgressChangedEventArgs);
                    }
                    return;
                }
            }

            ResolveCompletedEventArgs resolveCompletedEventArgs;
            if (result == PEER_E_NO_MORE)
            {
                resolveCompletedEventArgs = new ResolveCompletedEventArgs(m_PeerNameRecordCollection,
                                                       null, false, m_AsyncOp.UserSuppliedState);
            }
            else
            {
                PeerToPeerException ex = PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_ExceptionWhileResolvingAPeerName), result);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                            "Exception occurred when the native API is called to harvest an incremental resolve notification");
                resolveCompletedEventArgs = new ResolveCompletedEventArgs(null,
                                                       ex, false, m_AsyncOp.UserSuppliedState);

            }
            parent = m_PeerNameResolverWeakReference.Target as PeerNameResolver;
            if (parent != null)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                            "Proceeding to call the ResolveCompleted callback");
                parent.PrepareToRaiseCompletedEvent(m_AsyncOp, resolveCompletedEventArgs);
            }
            return;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Field: m_SafePeerNameEndResolve" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void ContineCancelCallback(object state)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                        "ContineCancelCallback called");
            try
            {
                if (m_CompletedOrException)
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                        "ContinueCancelCallback detected (before acquiring lock) that another thread has already called completed event - so returning without calling cancel");
                    return;
                }
                lock (m_Lock)
                {
                    if (m_Cancelled)
                    {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                            "ContinueCancelCallback detected (after acquiring lock) that cancel has already been called");
                        return;

                    }
                    if (m_CompletedOrException)
                    {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                                            "ContinueCancelCallback detected (after acquiring lock) that another thread has already called completed event - so returning without calling cancel");
                        return;
                    }
                    else
                    {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, 
                                            "ContinueCancelCallback is proceeding to close the handle and call the Completed callback with Cancelled = true");
                    }
                    m_Cancelled = true;
                    m_SafePeerNameEndResolve.Dispose();
                }
                PeerNameResolver parent = m_PeerNameResolverWeakReference.Target as PeerNameResolver;
                if (parent != null)
                {
                    ResolveCompletedEventArgs e = new ResolveCompletedEventArgs(null, null, true, m_AsyncOp.UserSuppliedState);
                    parent.PrepareToRaiseCompletedEvent(m_AsyncOp, e);
                }
            }
            catch
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Critical, m_TraceEventId, "Exception while cancelling the call ");
                throw;
            }
        }
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: ContineCancelCallback(Object):Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void CancelAsync()
        {
            //Defer the work to a callback
            ThreadPool.QueueUserWorkItem(new WaitCallback(ContineCancelCallback));
        }
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Dispose(Boolean):Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Field: m_SafePeerNameEndResolve" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (!m_SafePeerNameEndResolve.IsInvalid)
                {
                    m_SafePeerNameEndResolve.Dispose();
                }
                if (m_RegisteredWaitHandle != null)
                    m_RegisteredWaitHandle.Unregister(null);
                m_RegisteredWaitHandle = null;
                m_EndPointInfoAvailableEvent.Close();
            }
            m_Disposed = true;
        }

        internal int TraceEventId
        {
            get
            {
                return m_TraceEventId;
            }
        }
    }
    

    /// <summary>
    /// PeerNameResolver does [....] and async resolves. 
    /// PeerNameResolver supports multiple outstanding async calls
    /// </summary>
    public class PeerNameResolver
    {
        static PeerNameResolver()
        {
            //-------------------------------------------------
            //Check for the availability of the simpler PNRP APIs
            //-------------------------------------------------
            if (!PeerToPeerOSHelper.SupportsP2P)
            {
                throw new PlatformNotSupportedException(SR.GetString(SR.P2P_NotAvailable));
            }
        }

        private event EventHandler<ResolveProgressChangedEventArgs> m_ResolveProgressChanged;
        /// <summary>
        /// When an event handler is hooked up or removed, we demand the permissions. 
        /// In partial trust cases, this will avoid the security risk of just hooking up an existing instance 
        /// of the PeerNameResolver and then receiving all notification of 
        /// in resolution that is happening 
        /// </summary>
        public event EventHandler<ResolveProgressChangedEventArgs> ResolveProgressChanged
        {
            add
            {
                PnrpPermission.UnrestrictedPnrpPermission.Demand();
                m_ResolveProgressChanged += value;
            }
            remove
            {
                PnrpPermission.UnrestrictedPnrpPermission.Demand();
                m_ResolveProgressChanged -= value;
            }
        }

        private event EventHandler<ResolveCompletedEventArgs> m_ResolveCompleted;

        /// <summary>
        /// When an event handler is hooked up or removed, we demand the permissions. 
        /// In partial trust cases, this will avoid the security risk of just hooking up an existing instance 
        /// of the PeerNameResolver and then receiving all notification of 
        /// in resolution that is happening 
        /// </summary>
        public event EventHandler<ResolveCompletedEventArgs> ResolveCompleted
        {
            add
            {
                PnrpPermission.UnrestrictedPnrpPermission.Demand();
                m_ResolveCompleted += value;
            }
            remove
            {
                PnrpPermission.UnrestrictedPnrpPermission.Demand();
                m_ResolveCompleted -= value;
            }
        }

        SendOrPostCallback OnResolveProgressChangedDelegate;
        SendOrPostCallback OnResolveCompletedDelegate;

        /// <summary>
        /// The following lock and the Sorted Dictionary served
        /// the purpose of keeping an account of the multiple outstanding async
        /// resolutions. Each outstanding async operation is 
        /// keyed based on the userState parameter passed in 
        /// </summary>
        private object m_PeerNameResolverHelperListLock = new object();
        private Dictionary<object, PeerNameResolverHelper> m_PeerNameResolverHelperList = new Dictionary<object, PeerNameResolverHelper>();

        
        public PeerNameResolver()
        {
            OnResolveProgressChangedDelegate = new SendOrPostCallback(ResolveProgressChangedWaitCallback);
            OnResolveCompletedDelegate = new SendOrPostCallback(ResolveCompletedWaitCallback);
        }
        public PeerNameRecordCollection Resolve(PeerName peerName)
        {
            return Resolve(peerName, Cloud.Available, int.MaxValue);
        }
        public PeerNameRecordCollection Resolve(PeerName peerName, Cloud cloud)
        {
            return Resolve(peerName, cloud, int.MaxValue);
        }
        public PeerNameRecordCollection Resolve(PeerName peerName, int maxRecords)
        {
            return Resolve(peerName, Cloud.Available, maxRecords);
        }

        /// <summary>
        /// Implements [....] resolve of the PeerName in the cloud given
        /// </summary>
        /// <param name="peerName"></param>
        /// <param name="cloud"></param>
        /// <param name="MaxRecords"></param>
        /// <returns></returns>
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pEndPoints of type: PEER_PNRP_ENDPOINT_INFO*" />
        // <UsesUnsafeCode Name="Local pEndPointInfo of type: PEER_PNRP_ENDPOINT_INFO*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="Marshal.ReadIntPtr(System.IntPtr):System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.SizeOf(System.Type):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpResolve(System.String,System.String,System.UInt32&,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <ReferencesCritical Name="Local shEndPointInfoArray of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Method: UnsafeP2PNativeMethods.PnrpStartup():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerNameRecordCollection Resolve(PeerName peerName, Cloud cloud, int maxRecords)
        {
            
            //---------------------------------------------------
            //Check arguments
            //---------------------------------------------------
            if (peerName == null)
            {
                throw new ArgumentNullException(SR.GetString(SR.Pnrp_PeerNameCantBeNull), "peerName");
            }

            if (maxRecords <= 0)
            {
                throw new ArgumentOutOfRangeException("maxRecords", SR.GetString(SR.Pnrp_MaxRecordsParameterMustBeGreaterThanZero));
            }

            //---------------------------------------------------
            //Assume all clouds if the clould passed is null?
            //---------------------------------------------------
            if (cloud == null)
            {
                cloud = Cloud.Available;
            }
            
            //---------------------------------------------------
            //Demand CAS permissions
            //---------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            //---------------------------------------------------------------
            //No perf hit here, real native call happens only one time if it 
            //did not already happen
            //---------------------------------------------------------------
            UnsafeP2PNativeMethods.PnrpStartup();

            //---------------------------------------------------------------
            //Trace log
            //---------------------------------------------------------------
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "[....] Resolve called with PeerName: {0}, Cloud: {1}, MaxRecords {2}", peerName, cloud, maxRecords);

            SafePeerData shEndPointInfoArray;
            string NativeCloudName = cloud.InternalName;
            UInt32 ActualCountOfEndPoints = (UInt32)maxRecords;
            int result = UnsafeP2PNativeMethods.PeerPnrpResolve(peerName.ToString(), 
                                                                NativeCloudName, 
                                                                ref ActualCountOfEndPoints, 
                                                                out shEndPointInfoArray);
            if (result != 0)
            {
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotStartNameResolution), result);
            }

            //---------------------------------------------------
            //If there are no endpoints returned, return 
            //an empty PeerNameRecord Collection
            //---------------------------------------------------
            PeerNameRecordCollection PeerNameRecords = new PeerNameRecordCollection();
            if (ActualCountOfEndPoints != 0)
            {
                try
                {
                    unsafe
                    {
                        IntPtr pEndPointInfoArray = shEndPointInfoArray.DangerousGetHandle();
                        PEER_PNRP_ENDPOINT_INFO* pEndPoints = (PEER_PNRP_ENDPOINT_INFO*)pEndPointInfoArray;
                        for (int i = 0; i < ActualCountOfEndPoints; i++)
                        {
                            PeerNameRecord record = new PeerNameRecord();
                            PEER_PNRP_ENDPOINT_INFO* pEndPointInfo = &pEndPoints[i];
                            record.PeerName = new PeerName(Marshal.PtrToStringUni(pEndPointInfo->pwszPeerName));
                            string comment = Marshal.PtrToStringUni(pEndPointInfo->pwszComment);
                            if (comment != null && comment.Length > 0)
                            {
                                record.Comment = comment;
                            }
    
                            if (pEndPointInfo->payLoad.cbPayload != 0)
                            {
                                record.Data = new byte[pEndPointInfo->payLoad.cbPayload];
                                Marshal.Copy(pEndPointInfo->payLoad.pbPayload, record.Data, 0, (int)pEndPointInfo->payLoad.cbPayload);
    
                            }
                            //record.EndPointList = new IPEndPoint[pEndPointInfo->cAddresses];
                            IntPtr ppSOCKADDRs = pEndPointInfo->ArrayOfSOCKADDRIN6Pointers;
                            for (UInt32 j = 0; j < pEndPointInfo->cAddresses; j++)
                            {
                                IntPtr pSOCKADDR = Marshal.ReadIntPtr(ppSOCKADDRs);
    
                                byte[] AddressFamilyBuffer = new byte[2];
                                Marshal.Copy(pSOCKADDR, AddressFamilyBuffer, 0, 2);
                                int addressFamily = 0;
    #if BIGENDIAN
                                addressFamily = AddressFamilyBuffer[1] + ((int)AddressFamilyBuffer[0] << 8);
    #else
                                addressFamily = AddressFamilyBuffer[0] + ((int)AddressFamilyBuffer[1] << 8);
    #endif
                                byte[] buffer = new byte[((AddressFamily)addressFamily == AddressFamily.InterNetwork) ? SystemNetHelpers.IPv4AddressSize : SystemNetHelpers.IPv6AddressSize];
                                Marshal.Copy(pSOCKADDR, buffer, 0, buffer.Length);
                                IPEndPoint ipe = SystemNetHelpers.IPEndPointFromSOCKADDRBuffer(buffer);
                                record.EndPointCollection.Add(ipe);
                                ppSOCKADDRs = (IntPtr)((long)ppSOCKADDRs + Marshal.SizeOf(typeof(IntPtr)));
                            }
                            //----------------------------------
                            //Dump for trace
                            //----------------------------------
                            record.TracePeerNameRecord();
                            //----------------------------------                
                            //Add to collection
                            //----------------------------------
                            PeerNameRecords.Add(record);
                        }
                    }
                }
                finally
                {
                    shEndPointInfoArray.Dispose();
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "[....] Resolve returnig with PeerNameRecord count :{0}", PeerNameRecords.Count);
            return PeerNameRecords;
        }
        
        [HostProtection(ExternalThreading = true)]
        public void ResolveAsync(PeerName peerName, object userState)
        {
            ResolveAsync(peerName, Cloud.Available, Int32.MaxValue, userState);
        }
        [HostProtection(ExternalThreading = true)]
        public void ResolveAsync(PeerName peerName, Cloud cloud, object userState)
        {
            ResolveAsync(peerName, cloud, Int32.MaxValue, userState);
        }
        [HostProtection(ExternalThreading = true)]
        public void ResolveAsync(PeerName peerName, int maxRecords, object userState)
        {
            ResolveAsync(peerName, Cloud.Available, maxRecords, userState);
        }
        
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: UnsafeP2PNativeMethods.PnrpStartup():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerNameResolverHelper.StartAsyncResolve():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        [HostProtection(ExternalThreading = true)]
        public void ResolveAsync(PeerName peerName, Cloud cloud, int maxRecords, object userState)
        {
            //-------------------------------------------------
            //Check arguments
            //-------------------------------------------------
            if (peerName == null)
            {
                throw new ArgumentNullException(SR.GetString(SR.Pnrp_PeerNameCantBeNull), "peerName");
            }
            if (cloud == null)
            {
                cloud = Cloud.Available;
            }
            if (maxRecords <= 0)
            {
                throw new ArgumentOutOfRangeException("maxRecords", SR.GetString(SR.Pnrp_MaxRecordsParameterMustBeGreaterThanZero));
            }

            if (m_ResolveCompleted == null)
            {
                throw new PeerToPeerException(SR.GetString(SR.Pnrp_AtleastOneEvenHandlerNeeded));
            }
            //---------------------------------------------------
            //Demand CAS permissions
            //---------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            //---------------------------------------------------------------
            //No perf hit here, real native call happens only one time if it 
            //did not already happen
            //---------------------------------------------------------------
            UnsafeP2PNativeMethods.PnrpStartup();

            //----------------------------------------------------
            //userToken can't be null
            //----------------------------------------------------
            if (userState == null)
            {
                throw new ArgumentNullException(SR.GetString(SR.NullUserToken), "userState");
            }

            PeerNameResolverHelper peerNameResolverHelper = null;
            //---------------------------------------------------
            //The userToken can't be duplicate of what is in the 
            //current list. These are the requriments for the new Async model 
            //that supports multiple outstanding async calls
            //---------------------------------------------------
            int newTraceEventId  = NewTraceEventId;
            lock (m_PeerNameResolverHelperListLock)
            {
                if (m_PeerNameResolverHelperList.ContainsKey(userState))
                {
                    throw new ArgumentException(SR.GetString(SR.DuplicateUserToken));
                }
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, newTraceEventId, 
                                                "PeerNameResolverHelper is being created with TraceEventId {0}", newTraceEventId);
                peerNameResolverHelper = new PeerNameResolverHelper(peerName, cloud, maxRecords, userState, this, newTraceEventId);
                m_PeerNameResolverHelperList[userState] = peerNameResolverHelper;
            }

            try
            {
                //---------------------------------------------------
                //Start resolution on that resolver
                //---------------------------------------------------
                peerNameResolverHelper.StartAsyncResolve();
            }
            catch
            {
                //---------------------------------------------------
                //If an exception happens clear the userState from the 
                //list so that that token can be reused
                //---------------------------------------------------
                lock (m_PeerNameResolverHelperListLock)
                {
                    m_PeerNameResolverHelperList.Remove(userState);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, newTraceEventId,
                        "Removing userState token from pending list {0}", userState.GetHashCode());
                }
                throw;
            }
        }
        
        protected void OnResolveProgressChanged(ResolveProgressChangedEventArgs e)
        {
            if (m_ResolveProgressChanged != null)
            {
                m_ResolveProgressChanged(this, e);
            }
        }
        void ResolveProgressChangedWaitCallback(object operationState)
        {
            OnResolveProgressChanged((ResolveProgressChangedEventArgs)operationState);
        }
        internal void PrepareToRaiseProgressChangedEvent(AsyncOperation asyncOP, ResolveProgressChangedEventArgs args)
        {
            asyncOP.Post(OnResolveProgressChangedDelegate, args);
        }

        protected void OnResolveCompleted(ResolveCompletedEventArgs e)
        {
            if (m_ResolveCompleted != null)
            {
                m_ResolveCompleted(this, e);
            }
        }
        void ResolveCompletedWaitCallback(object operationState)
        {
            OnResolveCompleted((ResolveCompletedEventArgs)operationState);
        }
        internal void PrepareToRaiseCompletedEvent(AsyncOperation asyncOP, ResolveCompletedEventArgs args)
        {
            asyncOP.PostOperationCompleted(OnResolveCompletedDelegate, args);
            lock (m_PeerNameResolverHelperListLock)
            {
                PeerNameResolverHelper helper = m_PeerNameResolverHelperList[args.UserState];
                if (helper == null)
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Critical, 0, "userState for which we are about to call Completed event does not exist in the pending async list");
                }
                else
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, helper.TraceEventId, 
                        "userState {0} is being removed from the pending async list", args.UserState.GetHashCode());
                    m_PeerNameResolverHelperList.Remove(args.UserState);
                }

            }
        }
        

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: PeerNameResolverHelper.CancelAsync():System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void ResolveAsyncCancel(object userState)
        {
            PnrpPermission.UnrestrictedPnrpPermission.Demand();
            if (userState == null)
            {
                return;
            }
            PeerNameResolverHelper helper;
            lock (m_PeerNameResolverHelperListLock)
            {
                if (!m_PeerNameResolverHelperList.TryGetValue(userState, out helper))
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0, "ResolveAsyncCancel called with a userState token that is not in the pending async list - returning");
                    return;
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, helper.TraceEventId, 
                    "Proceeding to cancel the pending async");
            helper.CancelAsync();
        }


        private static int s_TraceEventId;
        private static int NewTraceEventId
        {
            get
            {
                Interlocked.CompareExchange(ref s_TraceEventId, 0, int.MaxValue);
                Interlocked.Increment(ref s_TraceEventId);
                return s_TraceEventId;
            }
        }        

    }
}
