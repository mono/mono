//------------------------------------------------------------------------------
// <copyright file="PeerNearMe.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.ComponentModel;
    using System.Text;
    using System.Net.Mail;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This is the event args class we give back when 
    /// we have a peer near me change event triggered by native
    /// </summary>
    public class PeerNearMeChangedEventArgs : EventArgs
    {
        private PeerNearMe m_peerNearMe;
        private PeerChangeType m_peerChangeType;

        internal PeerNearMeChangedEventArgs(PeerNearMe peerNearMe, PeerChangeType peerChangeType)
        {
            m_peerNearMe = peerNearMe;
            m_peerChangeType = peerChangeType;
        }

        public PeerNearMe PeerNearMe
        {
            get{
                return m_peerNearMe;
            }
        }

        public PeerChangeType PeerChangeType
        {
            get{
                return m_peerChangeType;
            }
        }
    }

    /// <summary>
    /// This class contains the functionality of the people near me concept
    /// in windows collaboration i.e. people on the same subnet
    /// </summary>
    [Serializable]
    public class PeerNearMe : Peer, IEquatable<PeerNearMe>, ISerializable
    {
        private string m_nickname;
        private Guid m_id;

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static PeerNearMe(){
            CollaborationHelperFunctions.Initialize();
        }

        public PeerNearMe(){
            OnRefreshDataCompletedDelegate = new SendOrPostCallback(RefreshDataCompletedWaitCallback);
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerNearMe(SerializationInfo serializationInfo, StreamingContext streamingContext)
            :base(serializationInfo, streamingContext)
        {
            m_id = (Guid) serializationInfo.GetValue("_Id", typeof(Guid));
            m_nickname = serializationInfo.GetString("_NickName");

            OnRefreshDataCompletedDelegate = new SendOrPostCallback(RefreshDataCompletedWaitCallback);
        }

        public string Nickname
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                return m_nickname;
            }
            internal set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                m_nickname = value;
            }
        }

        internal Guid Id
        {
            get{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                return m_id;
            }
            set
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                m_id = value;
            }
        }

        //
        // Adds this peer to the contact manager
        //
        public PeerContact AddToContactManager()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddToContactManager.");
            PeerContact peerContact = null;
            try{
                peerContact = PeerCollaboration.ContactManager.CreateContact(this);
                PeerCollaboration.ContactManager.AddContact(peerContact);
            }
            catch (Exception e){
                throw new PeerToPeerException(SR.GetString(SR.Collab_AddToContactMgrFailed), (e.InnerException != null ? e.InnerException : e));
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving AddToContactManager.");
            return peerContact;
        }

        //
        // Adds this peer to the contact manager
        //
        public PeerContact AddToContactManager(string displayName, string nickname, MailAddress emailAddress)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddToContactManager with Display name: {0}" + 
                " Nickname: {1} and Email Address: {2}", displayName, nickname, emailAddress);

            PeerContact peerContact = null;

            try{
                peerContact = PeerCollaboration.ContactManager.CreateContact(this);
                PeerCollaboration.ContactManager.AddContact(peerContact);
            }
            catch (Exception e){
                throw new PeerToPeerException(SR.GetString(SR.Collab_AddToContactMgrFailed), (e.InnerException != null ? e.InnerException : e));
            }

            peerContact.DisplayName = displayName;
            peerContact.Nickname = nickname;
            peerContact.EmailAddress = emailAddress;

            try{
                PeerCollaboration.ContactManager.UpdateContact(peerContact);
            }
            catch (Exception e){
                throw new PeerToPeerException(SR.GetString(SR.Collab_AddToContactMgrFailed) + " " + SR.GetString(SR.Collab_AddToContactMgrFailedUpdate), (e.InnerException != null ? e.InnerException : e));
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving AddToContactManager.");
            return peerContact;
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerNearMe CreateFromPeerEndPoint(PeerEndPoint peerEndPoint)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering CreateFromPeerEndPoint.");
            CollaborationHelperFunctions.Initialize();

            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");
            if (peerEndPoint.EndPoint == null)
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            PeerNearMeCollection peers = PeerCollaboration.GetPeersNearMe();
            PeerNearMe peer = null;

            foreach (PeerNearMe peerNearMe in peers){
                PeerEndPointCollection peerEndPoints = peerNearMe.PeerEndPoints;
                if ((peerEndPoints != null) && (peerEndPoints.Count != 0) && (peerEndPoints[0].Equals(peerEndPoint)))
                    peer = peerNearMe;
            }
            if (peer == null){
                //
                // No peer found, throw
                //
                throw new PeerToPeerException(SR.GetString(SR.Collab_EndPointNotAPeerNearMe)); 
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving CreateFromPeerEndPoint.");
            return peer;
        }

        //
        // Checks if we need refreshing of an unsubscribed peer near me
        //
        internal override void RefreshIfNeeded()
        {
            RefreshData();
        }

        //
        // Refresh the endpoint with new data from collab
        //
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalRefreshData(Object):Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void RefreshData()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            InternalRefreshData(false);
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <ReferencesCritical Name="Local safeRefreshedEPDataEvent of type: SafeCollabEvent" Ring="1" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: InternalRefreshData(PeerEndPoint):Void" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal protected void InternalRefreshData(object state)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "InternalRefreshEndpointData called.");
            
            int errorCode = 0;
            bool isAsync = (bool)state;
            Exception exception = null;

            AutoResetEvent refreshedEPDataEvent = new AutoResetEvent(false);
            SafeCollabEvent safeRefreshedEPDataEvent;

            PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
            pcer.eventType = PeerCollabEventType.RequestStatusChanged;
            pcer.pInstance = IntPtr.Zero;

            //
            // Register to receive status changed event from collab
            //
            errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                refreshedEPDataEvent.SafeWaitHandle,
                                                                1,
                                                                ref pcer,
                                                                out safeRefreshedEPDataEvent);
            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                exception = PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ReqStatusChangedRegFailed), errorCode);
                if (!isAsync)
                    throw exception;
            }

            PeerEndPointCollection peerEndPoints = PeerEndPoints;
            
            if (peerEndPoints.Count == 0) return;

            try{
            InternalRefreshData(peerEndPoints[0]);
            }
            catch (Exception e){
                if (!isAsync)
                    throw;
                else
                    exception = e;
            }

            //
            // Wait till all the endpoints are refreshed
            //
            while (exception == null){
                refreshedEPDataEvent.WaitOne();

                SafeCollabData eventData;

                errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(safeRefreshedEPDataEvent,
                                                                                    out eventData);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                    exception = PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_RefreshDataFailed), errorCode);
                    if (!isAsync)
                        throw exception;
                    else 
                        break;
                }

                PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                            typeof(PEER_COLLAB_EVENT_DATA));
                
                if (ped.eventType == PeerCollabEventType.RequestStatusChanged){
                    PEER_EVENT_REQUEST_STATUS_CHANGED_DATA statusData = ped.requestStatusChangedData;

                    PeerEndPoint peerEndPoint = null;

                    if (statusData.pEndPoint != IntPtr.Zero){
                        PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(statusData.pEndPoint, typeof(PEER_ENDPOINT));
                        peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                    }

                    if (statusData.hrChange < 0){
                        exception = PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_RefreshDataFailed), statusData.hrChange);
                    }

                    if (exception != null){
                        //
                        // Throw exception for sync but call callback for async with exception
                        //
                        if (!isAsync)
                            throw exception;
                    }

                    //
                    // Check if this is our endpoint
                    //
                    if (PeerEndPoints[0].Equals(peerEndPoint)){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Found endpoint match in Request status changed.");

                        //
                        // For async call the callback and for sync just return
                        //
                        if (isAsync){
                            RefreshDataCompletedEventArgs args = new
                                        RefreshDataCompletedEventArgs(  peerEndPoint,
                                                                        null,
                                                                        false,
                                                                        m_refreshDataAsyncOp.UserSuppliedState);
                            
                            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Firing RefreshDataCompleted event with folloding peer endpoint.");
                                peerEndPoint.TracePeerEndPoint();
                            } 
                            
                            this.PrepareToRaiseRefreshDataCompletedEvent(m_refreshDataAsyncOp, args);
                        }

                        break;
                    }
                }
            }

            //
            // Async case with exception fire callback here
            // Sync would have already thrown this by now
            //
            if (exception != null){
                RefreshDataCompletedEventArgs args = new
                            RefreshDataCompletedEventArgs(null,
                                                            exception,
                                                            false,
                                                            m_refreshDataAsyncOp.UserSuppliedState);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Firing RefreshDataCompleted event with exception {0}.", exception);
                this.PrepareToRaiseRefreshDataCompletedEvent(m_refreshDataAsyncOp, args);
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving InternalRefreshEndpointData.");
        }

        //
        // Refreshes on endpoint 
        //

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRefreshEndpointData(System.IntPtr):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void InternalRefreshData(PeerEndPoint peerEndPoint)
        {
            int errorCode;
            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            GCHandle pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
            pep.pwzEndpointName = pepName.AddrOfPinnedObject();

            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabRefreshEndpointData(ptrPeerEP);
            }
            finally{
                if (pepName.IsAllocated) pepName.Free();
                if (peerEP.IsAllocated) peerEP.Free();
            }

            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRefreshEndpointData returned with errorcode {0}", errorCode);
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_RefreshDataFailed), errorCode);
            }
        }
        
        #region RefreshEndpoint Async variables
        AsyncOperation m_refreshDataAsyncOp;
        private object m_refreshDataAsyncOpLock;
        private object RefreshDataAsyncOpLock
        {
            get{
                if (m_refreshDataAsyncOpLock == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_refreshDataAsyncOpLock, o, null);
                }
                return m_refreshDataAsyncOpLock;
            }
        }
        SendOrPostCallback OnRefreshDataCompletedDelegate;
        #endregion

        private event EventHandler<RefreshDataCompletedEventArgs> m_refreshDataCompleted;
        public event EventHandler<RefreshDataCompletedEventArgs> RefreshDataCompleted
        {
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                
                m_refreshDataCompleted += value;
            }
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                m_refreshDataCompleted -= value;
            }
        }

        //
        // Async refresh endpoint data
        //
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalRefreshData(Object):Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void RefreshDataAsync(object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (userToken == null)
                throw new ArgumentNullException("userToken");

            lock (RefreshDataAsyncOpLock){
                if (m_refreshDataAsyncOp != null)
                    throw new PeerToPeerException(SR.GetString(SR.Collab_DuplicateRefreshAsync));
                m_refreshDataAsyncOp = AsyncOperationManager.CreateOperation(userToken);
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(InternalRefreshData), true);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving RefreshDataAsync().");
        }

        protected void OnRefreshDataCompleted(RefreshDataCompletedEventArgs e)
        {
            EventHandler<RefreshDataCompletedEventArgs> handlerCopy = m_refreshDataCompleted;

            if (handlerCopy != null){
                handlerCopy(this, e);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the refresh endpoint completed event callback.");
            }
        }

        void RefreshDataCompletedWaitCallback(object operationState)
        {
            m_refreshDataAsyncOp = null;
            OnRefreshDataCompleted((RefreshDataCompletedEventArgs)operationState);
        }

        internal void PrepareToRaiseRefreshDataCompletedEvent(AsyncOperation asyncOP, RefreshDataCompletedEventArgs args)
        {
            asyncOP.PostOperationCompleted(OnRefreshDataCompletedDelegate, args);
        }

        private static event EventHandler<PeerNearMeChangedEventArgs> s_peerNearMeChanged;
        public static event EventHandler<PeerNearMeChangedEventArgs> PeerNearMeChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddPeerNearMeChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PeerNearMeChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                AddPeerNearMeChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemovePeerNearMeChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PeerNearMeChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                RemovePeerNearMeChanged(value);
            }
        }

        #region PeerNearMe changed event variables
        private static object s_lockPNMChangedEvent;
        private static object LockPNMChangedEvent
        {
            get{
                if (s_lockPNMChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_lockPNMChangedEvent, o, null);
                }
                return s_lockPNMChangedEvent;
            }
        }
        private static RegisteredWaitHandle s_registeredPNMWaitHandle;
        private static AutoResetEvent s_peerNearMeChangedEvent;
        private static SafeCollabEvent s_safePeerNearMeChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: PeerNearMeChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: s_safePeerNearMeChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void AddPeerNearMeChanged(EventHandler<PeerNearMeChangedEventArgs> cb)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddPeerNearMeChanged().");

            lock (LockPNMChangedEvent){
                if (s_peerNearMeChanged == null){

                    s_peerNearMeChangedEvent = new AutoResetEvent(false);
                    
                    //
                    // Register callback with a wait handle
                    //

                    s_registeredPNMWaitHandle = ThreadPool.RegisterWaitForSingleObject(s_peerNearMeChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(PeerNearMeChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.PeopleNearMeChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        s_peerNearMeChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out s_safePeerNearMeChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_PeerNearMeChangedRegFailed), errorCode);
                    }
                }
                s_peerNearMeChanged += cb;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddPeerNearMeChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safePeerNearMeChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void RemovePeerNearMeChanged(EventHandler<PeerNearMeChangedEventArgs> cb)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePeerNearMeChanged() called.");
            lock (LockPNMChangedEvent){
                s_peerNearMeChanged -= cb;
                if (s_peerNearMeChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref s_registeredPNMWaitHandle,
                                                                ref s_safePeerNearMeChangedEvent,
                                                                ref s_peerNearMeChangedEvent);

                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean PeerNearMeChanged variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePeerNearMeChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: s_safePeerNearMeChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.PEER_PEOPLE_NEAR_METoPeerNearMe(System.Net.PeerToPeer.Collaboration.PEER_PEOPLE_NEAR_ME):System.Net.PeerToPeer.Collaboration.PeerNearMe" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void PeerNearMeChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerNearMeChangedCallback() called.");

            while (true){
                PeerNearMeChangedEventArgs peerNearMeChangedArgs = null;

                //
                // Get the event data for the fired event
                //

                try{
                    lock (LockPNMChangedEvent){
                        if (s_safePeerNearMeChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(s_safePeerNearMeChangedEvent,
                                                                                     out eventData);
                    }
                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetPeerNearMeChangedDataFailed), errorCode);
                    }

                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                                typeof(PEER_COLLAB_EVENT_DATA));
                    if (ped.eventType == PeerCollabEventType.PeopleNearMeChanged){
                        PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA pnmData = ped.peopleNearMeChangedData;
                        PeerNearMe peerNearMe = null;
                        if (pnmData.pPeopleNearMe != IntPtr.Zero){
                            PEER_PEOPLE_NEAR_ME pnm = (PEER_PEOPLE_NEAR_ME)Marshal.PtrToStructure(pnmData.pPeopleNearMe, typeof(PEER_PEOPLE_NEAR_ME));
                            peerNearMe = CollaborationHelperFunctions.PEER_PEOPLE_NEAR_METoPeerNearMe(pnm);
                        }

                        peerNearMeChangedArgs = new PeerNearMeChangedEventArgs(peerNearMe, pnmData.changeType);
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<PeerNearMeChangedEventArgs> handlerCopy = s_peerNearMeChanged;

                if ((peerNearMeChangedArgs != null) && (handlerCopy != null)){
                    handlerCopy(null, peerNearMeChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the peer near me changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving PeerNearMeChangedCallback().");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.InternalInviteEndPoint(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPoint,System.Net.PeerToPeer.Collaboration.PeerContact):System.Net.PeerToPeer.Collaboration.PeerInvitationResponse" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public override PeerInvitationResponse Invite(PeerApplication applicationToInvite, string message,
                                                      byte[] invitationData)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (applicationToInvite == null)
                throw new ArgumentNullException("applicationToInvite");
            if (applicationToInvite.Id == Guid.Empty)
                throw new PeerToPeerException(SR.GetString(SR.Collab_EmptyGuidError));
            
            //
            // We need at least one endpoint to send invitation to
            //
            PeerEndPointCollection peerEndPoints = PeerEndPoints;

            if ((peerEndPoints == null) || (peerEndPoints.Count == 0))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            PeerEndPoint peerEndPoint = PeerEndPoints[0];

            PeerInvitationResponse response = InternalInviteEndPoint(applicationToInvite.Id, message, invitationData,
                                                                     peerEndPoint, null);

            // throw an exception if the response type is ERROR
            CollaborationHelperFunctions.ThrowIfInvitationResponseInvalid(response);
            return response;
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.get_CurrentApplicationGuid():System.Guid" Ring="1" />
        // <ReferencesCritical Name="Method: Peer.InternalInviteEndPoint(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPoint,System.Net.PeerToPeer.Collaboration.PeerContact):System.Net.PeerToPeer.Collaboration.PeerInvitationResponse" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public override PeerInvitationResponse Invite()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            Guid appGuid = CurrentApplicationGuid;

            if (appGuid.Equals(Guid.Empty))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoGuidForCurrApp));
            
            //
            // We need at least one endpoint to send invitation to
            //
            PeerEndPointCollection peerEndPoints = PeerEndPoints;

            if ((peerEndPoints == null) || (peerEndPoints.Count == 0))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            PeerEndPoint peerEndPoint = PeerEndPoints[0];

            PeerInvitationResponse response = InternalInviteEndPoint(appGuid, null, null, peerEndPoint, null);

            // throw an exception if the response type is ERROR
            CollaborationHelperFunctions.ThrowIfInvitationResponseInvalid(response);
            return response;
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.get_CurrentApplicationGuid():System.Guid" Ring="1" />
        // <ReferencesCritical Name="Method: Peer.InternalInviteAsync(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPointCollection,System.Net.PeerToPeer.Collaboration.PeerContact,System.Object):System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public override void InviteAsync(Object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            if (userToken == null)
                throw new ArgumentException(SR.GetString(SR.NullUserToken));

            Guid appGuid = CurrentApplicationGuid;

            if (appGuid.Equals(Guid.Empty))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoGuidForCurrApp));

            //
            // We need at least one endpoint to send invitation to
            //
            PeerEndPointCollection peerEndPoints = PeerEndPoints;

            if ((peerEndPoints == null) || (peerEndPoints.Count == 0))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            InternalInviteAsync(appGuid, null, null, PeerEndPoints, null, userToken);
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: Peer.InternalInviteAsync(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPointCollection,System.Net.PeerToPeer.Collaboration.PeerContact,System.Object):System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public override void InviteAsync(   PeerApplication applicationToInvite, string message, 
                                            byte[] invitationData, Object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (applicationToInvite == null)
                throw new ArgumentNullException("applicationToInvite");
            if (applicationToInvite.Id == Guid.Empty)
                throw new PeerToPeerException(SR.GetString(SR.Collab_EmptyGuidError));
            if (userToken == null)
                throw new ArgumentException(SR.GetString(SR.NullUserToken));

            //
            // We need at least one endpoint to send invitation to
            //
            PeerEndPointCollection peerEndPoints = PeerEndPoints;

            if ((peerEndPoints == null) || (peerEndPoints.Count == 0))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            InternalInviteAsync(applicationToInvite.Id, message, invitationData, 
                                peerEndPoints, null, userToken);
        }

        public bool Equals(PeerNearMe other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return other.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerNearMe comparandPeerNearMe = obj as PeerNearMe;
            if (comparandPeerNearMe != null){
                return Guid.Equals(comparandPeerNearMe.Id, Id);
            }

            return false;
        }

        public new static bool Equals(object objA, object objB)
        {
            PeerNearMe comparandPeerNearMe1 = objA as PeerNearMe;
            PeerNearMe comparandPeerNearMe2 = objB as PeerNearMe;

            if ((comparandPeerNearMe1 != null) && (comparandPeerNearMe2 != null)){
                return Guid.Equals(comparandPeerNearMe1.Id, comparandPeerNearMe2.Id);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            return Id.GetHashCode();
        }

        public override string ToString()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            return Nickname;
        }

        private bool m_Disposed;

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed){
                try{
                    m_Disposed = true;
                }
                finally{
                    base.Dispose(disposing);
                }
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Peer.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext):System.Void" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// This is made virtual so that derived types can be implemented correctly
        /// </summary>
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Peer.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext):System.Void" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_Id", m_id);
            info.AddValue("_NickName", m_nickname);
        }

        //
        // Tracing information for Peer Near Me
        //
        internal void TracePeerNearMe()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerNearMe");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tNickname: {0}", Nickname);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tID: {0}", Id);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tNumber of Endpoints: {0}", PeerEndPoints.Count);
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose)){

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tEndPoints:");
                foreach (PeerEndPoint peerEndPoint in PeerEndPoints)
                    peerEndPoint.TracePeerEndPoint();
            }
        }


    }

    //
    // Manages collection of peer near me classes
    //
    [Serializable]
    public class PeerNearMeCollection : Collection<PeerNearMe>
    {
        internal PeerNearMeCollection() { }
        protected override void SetItem(int index, PeerNearMe item)
        {
            // nulls not allowed
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, PeerNearMe item)
        {
            // nulls not allowed
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        public override string ToString()
        {
            bool first = true;
            StringBuilder builder = new StringBuilder();

            foreach (PeerNearMe peerNearMe in this){
                if (!first){
                    builder.Append(", ");
                }
                else{
                    first = false;
                }
                builder.Append(peerNearMe.ToString());
            }
            return builder.ToString();
        }
    }

}
