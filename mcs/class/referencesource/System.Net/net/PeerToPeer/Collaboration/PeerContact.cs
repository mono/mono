//------------------------------------------------------------------------------
// <copyright file="Peer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer.Collaboration
{
    using System.Net.Mail;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.InteropServices;
    using System.Net.PeerToPeer;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This is the event args class we give back when 
    /// we have an application change event triggered by native
    /// </summary>
    public class ApplicationChangedEventArgs : EventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        private PeerContact m_peerContact;
        private PeerChangeType m_peerChangeType;
        private PeerApplication m_peerApplication;

        internal ApplicationChangedEventArgs(PeerEndPoint peerEndPoint, PeerContact peerContact,
                                        PeerChangeType peerChangeType, PeerApplication peerApplication)
        {
            m_peerEndPoint = peerEndPoint;
            m_peerContact = peerContact;
            m_peerChangeType = peerChangeType;
            m_peerApplication = peerApplication;
        }

        public PeerEndPoint PeerEndPoint
        {
            get
            {
                return m_peerEndPoint;
            }
        }

        public PeerContact PeerContact
        {
            get
            {
                return m_peerContact;
            }
        }

        public PeerChangeType PeerChangeType
        {
            get
            {
                return m_peerChangeType;
            }
        }

        public PeerApplication PeerApplication
        {
            get
            {
                return m_peerApplication;
            }
        }
    }

    /// <summary>
    /// This class incorporates the contact functions of a peer collaboration
    /// contact
    /// </summary>
    [Serializable]
    public class PeerContact : Peer, IEquatable<PeerContact>, ISerializable
    {
        private PeerName m_peerName;
        private string m_nickname;
        private string m_displayName;
        private MailAddress m_emailAddress;
        private SubscriptionType m_subscribeAllowed;
        private bool m_isSubscribed;
        private X509Certificate2 m_credentials;
        private string m_contactXml;
        private bool m_justCreated; 

        public PeerName PeerName
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_peerName; 
            }
            internal set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_peerName = value; 
            }
        }

        public string Nickname
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_nickname; 
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_nickname = value; 
            }
        }
        
        public string DisplayName
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_displayName; 
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_displayName = value; 
            }
        }

        public MailAddress EmailAddress
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_emailAddress; 
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_emailAddress = value;
            }
        }

        public SubscriptionType SubscribeAllowed
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return InternalSubscribeAllowedGet();
            }
            set{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                InternalSubscribeAllowedSet(value);
            }
        }

        internal virtual SubscriptionType InternalSubscribeAllowedGet()
        {
            return m_subscribeAllowed;
        }

        internal virtual void InternalSubscribeAllowedSet(SubscriptionType value)
        {
            m_subscribeAllowed = value;
        }

        public bool IsSubscribed
        {
            get
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return InternalIsSubscribed();
            }
            internal set { m_isSubscribed = value; }
        }

        internal virtual bool InternalIsSubscribed()
        {
            lock (IsSubscribeLock)
                return m_isSubscribed;
        }

        private object m_isSubscribeLock;
        internal object IsSubscribeLock
        {
            get
            {
                if (m_isSubscribeLock == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_isSubscribeLock, o, null);
                }
                return m_isSubscribeLock;
            }
        }

        public X509Certificate2 Credentials
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                
                return m_credentials;
            }
            internal set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_credentials = value;
            }

        }

        internal bool JustCreated
        {
            get { return m_justCreated; }
            set { m_justCreated = value; }
        }

        internal string ContactXml
        {
            get { return m_contactXml; }
            set { m_contactXml = value; }
        }

        public override PeerEndPointCollection PeerEndPoints
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <UsesUnsafeCode Name="Local pEndPoints of type: IntPtr*" />
            // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
            // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
            // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
            // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumEndpoints(System.Net.PeerToPeer.Collaboration.PEER_CONTACT&,System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
            // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
            // <ReferencesCritical Name="Local safeCredentials of type: SafeCollabMemory" Ring="1" />
            // <ReferencesCritical Name="Local epArray of type: SafeCollabData" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(System.Net.PeerToPeer.Collaboration.PeerContact,System.Net.PeerToPeer.Collaboration.SafeCollabMemory&):System.Net.PeerToPeer.Collaboration.PEER_CONTACT" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Get PeerEndPoints called.");

                PeerEndPointCollection peerEndPoints = new PeerEndPointCollection();

                SafeCollabEnum handlePeerEnum = null;
                UInt32 pepCount = 0;
                int errorCode = 0;

                try{
                    SafeCollabMemory safeCredentials = null;
                    try{
                        //
                        // Get the Endpoint enumeration from native
                        //
                        PEER_CONTACT pc = CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(this, ref safeCredentials);

                        errorCode = UnsafeCollabNativeMethods.PeerCollabEnumEndpoints(ref pc, out handlePeerEnum);
                    }
                    finally{
                        if (safeCredentials != null) safeCredentials.Dispose();
                    }

                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumEndpoints returned with errorcode {0}", errorCode);
                        return peerEndPoints;
                    }

                    errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref pepCount);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                        return peerEndPoints;
                    }

                    if (pepCount == 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "No endpoints. Get PeerEndPoints returning.");
                        return peerEndPoints;
                    }

                    unsafe{
                        SafeCollabData epArray;
                        errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref pepCount, out epArray);

                        IntPtr pPEER_ENDPOINT = epArray.DangerousGetHandle();
                        IntPtr* pEndPoints = (IntPtr*)pPEER_ENDPOINT;

                        //
                        // Loop throught all the endpoints from native
                        //
                        for (ulong i = 0; i < pepCount; i++)
                        {
                            IntPtr pEndPointPtr = (IntPtr)pEndPoints[i];
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(pEndPointPtr, typeof(PEER_ENDPOINT));

                            PeerEndPoint peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            peerEndPoints.Add(peerEndPoint);
                        }
                    }
                }
                finally{
                    if (handlePeerEnum != null) handlePeerEnum.Dispose();
                }
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving Get PeerEndPoints with {0} endpoints.", peerEndPoints.Count);
                return peerEndPoints;
            }
        }
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static PeerContact()
        {
            CollaborationHelperFunctions.Initialize();
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal PeerContact() {
            OnSubscribeCompletedDelegate = new SendOrPostCallback(SubscribeCompletedWaitCallback);
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [System.Security.SecurityCritical]
        protected PeerContact(SerializationInfo serializationInfo, StreamingContext streamingContext):this()
        {
            m_peerName = (PeerName) serializationInfo.GetValue("_PeerName", typeof(PeerName));
            m_nickname = serializationInfo.GetString("_NickName");
            m_displayName = serializationInfo.GetString("_DisplayName");

            try{
                m_emailAddress = new MailAddress(serializationInfo.GetString("_EmailAddress"));
            }
            catch (SerializationException) { }

            m_subscribeAllowed = (SubscriptionType)serializationInfo.GetValue("_SubscribeAllowed", typeof(SubscriptionType));
            
            byte [] rawData = (byte[]) serializationInfo.GetValue("_Credentials", typeof(byte[]));
            m_credentials = new X509Certificate2(rawData); ;

            m_contactXml = serializationInfo.GetString("_ContactXml");
            m_justCreated = serializationInfo.GetBoolean("_JustCreated");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabExportContact(System.String,System.String&):System.Int32" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public string ToXml()
        {
            string xmlContact = null;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering ToXml().");

            if (JustCreated)
                return ContactXml;

            int errorCode = UnsafeCollabNativeMethods.PeerCollabExportContact(PeerName.ToString(), ref xmlContact);
            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabExportContact returned with errorcode {0}.", errorCode);
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ContactToXmlFailed), errorCode);

            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ToXml() with XML string: {0}", xmlContact);
            return xmlContact;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabParseContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local contact of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerContact FromXml(string peerContactXml)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering FromXml() with XML string: {0}", peerContactXml);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            CollaborationHelperFunctions.Initialize();

            if (peerContactXml == null)
                throw new ArgumentNullException("peerContactXml");
            
            SafeCollabData contact = null;
            PeerContact peerContact = null;
            int errorCode;

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabParseContact(peerContactXml, out contact);

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabParseContact returned with errorcode {0}. Contact already exists.", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ContactFromXmlFailed), errorCode);
                }

                PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(contact.DangerousGetHandle(), typeof(PEER_CONTACT));
                peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
            }
            finally{
                if (contact != null) contact.Dispose();
            }

            //
            // Check to see if this already in the contact manager else we can set the just created field
            // and set this xml since you cannot get XML for a contact not in the contact manager
            //

            try{
                contact = null;
                errorCode = UnsafeCollabNativeMethods.PeerCollabGetContact(peerContact.PeerName.ToString(),
                                                                                        out contact);
            }
            finally{
                if (contact != null) contact.Dispose();
            }

            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, 
                    "Error or contact not found in Contact Manager. ErrorCode {0}", errorCode);

                //
                // Mark it as just created and add the xml. This is used when adding the contact or getting
                // contact xml when contact is not in Contact manager
                //

                peerContact.JustCreated = true;
                peerContact.ContactXml = peerContactXml;
            }

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving FromXml() with following peercontact");
                peerContact.TracePeerContact();
            }
            return peerContact;
        }

        //
        // Updates the fwatch in native contact to indicate that we want to watch this 
        // Contact
        //
        public virtual void Subscribe()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering Subscribe().");
            
            if (IsSubscribed){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Already subscribed. Leaving Subscribe().");
                return;
            }

            InternalSubscribe(false);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving Subscribe().");
        }

        //
        // Handles Subscribe to a contact
        //
        private void InternalSubscribe(object isAsyncObj)
        {
            //
            // Updates the IsSubscribed of the contact in the address book
            //
            
            bool isAsync = (bool)isAsyncObj;
            Exception ex = null;

            lock (IsSubscribeLock){
                IsSubscribed = true;
                try{
                    PeerCollaboration.ContactManager.UpdateContact(this);
                }
                catch (Exception e){
                    IsSubscribed = false;
                    if (!isAsync)
                        throw;
                    ex = e;
                }
            }

            if (isAsync){
                SubscribeCompletedEventArgs subscribeArgs;
                if (ex == null){
                    subscribeArgs = new SubscribeCompletedEventArgs(null, this, null, false, AsyncOp.UserSuppliedState);
                }
                else{
                    subscribeArgs = new SubscribeCompletedEventArgs(null, null, ex, false, AsyncOp.UserSuppliedState);
                }
                this.PrepareToRaiseSubscribeCompletedEvent(AsyncOp, subscribeArgs);
            }

        }

        #region Subscribe Async variables
        private AsyncOperation m_AsyncOp;
        internal AsyncOperation AsyncOp
        {
            get{
                return m_AsyncOp;
            }
            set{
                m_AsyncOp = value;
            }
        }

        private object m_asyncOpLock;
        internal object AsyncLock
        {
            get{
                if (m_asyncOpLock == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_asyncOpLock, o, null);
                }
                return m_asyncOpLock;
            }
        }

        private SendOrPostCallback OnSubscribeCompletedDelegate;
        #endregion

        private event EventHandler<SubscribeCompletedEventArgs> m_subscribeCompleted;
        public event EventHandler<SubscribeCompletedEventArgs> SubscribeCompleted
        {
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                
                m_subscribeCompleted += value;
            }
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                
                m_subscribeCompleted -= value; 
            }
        }

        //
        // Updates the fwatch in native contact to indicate that we want to watch this 
        // Contact
        //
        public virtual void SubscribeAsync(Object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            if (userToken == null)
                throw new ArgumentNullException("userToken");

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering SubscribeAsync() with user token {0}.", userToken);
            
            lock (AsyncLock){
                if (AsyncOp != null)
                    throw new PeerToPeerException(SR.GetString(SR.Collab_DuplicateSubscribeAsync));

                AsyncOp = AsyncOperationManager.CreateOperation(userToken);
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(InternalSubscribe), true);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving SubscribeAsync().");
        }

        protected void OnSubscribeCompleted(SubscribeCompletedEventArgs e)
        {
            EventHandler<SubscribeCompletedEventArgs> handlerCopy = m_subscribeCompleted;

            if (handlerCopy != null){
                handlerCopy(this, e);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the subscribe completed event callback.");
            }
        }

        void SubscribeCompletedWaitCallback(object operationState)
        {
            AsyncOp = null;
            OnSubscribeCompleted((SubscribeCompletedEventArgs)operationState);
        }

        internal void PrepareToRaiseSubscribeCompletedEvent(AsyncOperation asyncOP, SubscribeCompletedEventArgs args)
        {
            asyncOP.PostOperationCompleted(OnSubscribeCompletedDelegate, args);
        }

        //
        // Unsubscribe from getting events from this contact
        //
        public virtual void Unsubscribe()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            if (!IsSubscribed)
                return;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering UnSubscribe().");

            lock (IsSubscribeLock){
                IsSubscribed = false;
                
                try{
                    PeerCollaboration.ContactManager.UpdateContact(this);
                }
                catch (Exception){
                    IsSubscribed = true;
                    throw;
                }
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Laaving UnSubscribe().");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.get_CurrentApplicationGuid():System.Guid" Ring="1" />
        // <ReferencesCritical Name="Method: InternalInviteAllEndPoints(PeerEndPointCollection, String, Byte[], Guid):PeerInvitationResponse" Ring="2" />
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
            return InternalInviteAllEndPoints(PeerEndPoints, null, null, appGuid); 
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InternalInviteAllEndPoints(PeerEndPointCollection, String, Byte[], Guid):PeerInvitationResponse" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public override PeerInvitationResponse Invite(  PeerApplication applicationToInvite, string message,
                                                        byte[] invitationData)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (applicationToInvite == null)
                throw new ArgumentNullException("applicationToInvite");
            if (applicationToInvite.Id == Guid.Empty)
                throw new PeerToPeerException(SR.GetString(SR.Collab_EmptyGuidError));

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            //
            // We need at least one endpoint to send invitation to
            //
            return InternalInviteAllEndPoints(PeerEndPoints, message, invitationData, applicationToInvite.Id); 
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.InternalInviteEndPoint(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPoint,System.Net.PeerToPeer.Collaboration.PeerContact):System.Net.PeerToPeer.Collaboration.PeerInvitationResponse" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private PeerInvitationResponse InternalInviteAllEndPoints(  PeerEndPointCollection peerEndPoints,
                                                                    string message, byte[] invitationData,
                                                                    Guid applicationId)
        {
            if ((peerEndPoints == null) || (peerEndPoints.Count == 0))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            bool foundDeclined = false;
            bool foundExpired = false;

            //
            // Call for each endpoint. Return first accepted or last declined.
            //
            foreach (PeerEndPoint peerEndPoint in peerEndPoints){
                PeerInvitationResponse endPointResponse = InternalInviteEndPoint(applicationId,
                                                            message, invitationData, peerEndPoint, this);

                if (endPointResponse.PeerInvitationResponseType == PeerInvitationResponseType.Accepted)
                    return endPointResponse;
                else if (endPointResponse.PeerInvitationResponseType == PeerInvitationResponseType.Declined)
                    foundDeclined = true;

                else if (endPointResponse.PeerInvitationResponseType == PeerInvitationResponseType.Expired)
                    foundExpired = true;
            }

            if (foundDeclined)
                return new PeerInvitationResponse(PeerInvitationResponseType.Declined);
            else if (foundExpired)
                return new PeerInvitationResponse(PeerInvitationResponseType.Expired);
            else
                throw new PeerToPeerException(SR.GetString(SR.Collab_InviteFailed));
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.get_CurrentApplicationGuid():System.Guid" Ring="1" />
        // <ReferencesCritical Name="Method: Peer.InternalInviteEndPoint(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPoint,System.Net.PeerToPeer.Collaboration.PeerContact):System.Net.PeerToPeer.Collaboration.PeerInvitationResponse" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerInvitationResponse Invite(PeerEndPoint peerEndPoint)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            //
            // We need at least one endpoint to send invitation to
            //
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");
            if (peerEndPoint.EndPoint == null)
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            Guid appGuid = CurrentApplicationGuid;

            if (appGuid.Equals(Guid.Empty))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoGuidForCurrApp));

            PeerInvitationResponse response = InternalInviteEndPoint(appGuid, null, null, peerEndPoint, this);

            // throw an exception if the response type is ERROR
            CollaborationHelperFunctions.ThrowIfInvitationResponseInvalid(response);
            return response;
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.InternalInviteEndPoint(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPoint,System.Net.PeerToPeer.Collaboration.PeerContact):System.Net.PeerToPeer.Collaboration.PeerInvitationResponse" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerInvitationResponse Invite(PeerEndPoint peerEndPoint, PeerApplication applicationToInvite, 
                                             string message, byte [] invitationData)
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
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");
            if (peerEndPoint.EndPoint == null)
                throw new ArgumentException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            PeerInvitationResponse response = InternalInviteEndPoint(applicationToInvite.Id, message, invitationData,
                                                                     peerEndPoint, this);

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


            //
            // Call internal async invite
            //
            InternalInviteAsync(appGuid, null, null, peerEndPoints, this, userToken);

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

            if (userToken == null)
                throw new ArgumentException(SR.GetString(SR.NullUserToken));
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

            //
            // Call internal async invite
            //
            InternalInviteAsync(applicationToInvite.Id, null, null, peerEndPoints, this, userToken);
        }


        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.get_CurrentApplicationGuid():System.Guid" Ring="1" />
        // <ReferencesCritical Name="Method: Peer.InternalInviteAsync(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPointCollection,System.Net.PeerToPeer.Collaboration.PeerContact,System.Object):System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void InviteAsync(PeerEndPoint peerEndPoint, Object userToken)
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
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");
            if (peerEndPoint.EndPoint == null)
                throw new ArgumentException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            PeerEndPointCollection peerEndPoints = new PeerEndPointCollection();
            peerEndPoints.Add(peerEndPoint);
            //
            // Call internal async invite
            //
            InternalInviteAsync(appGuid, null, null, peerEndPoints, this, userToken);
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: Peer.InternalInviteAsync(System.Guid,System.String,System.Byte[],System.Net.PeerToPeer.Collaboration.PeerEndPointCollection,System.Net.PeerToPeer.Collaboration.PeerContact,System.Object):System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void InviteAsync(PeerEndPoint peerEndPoint, string message,
                                byte [] invitationData, PeerApplication applicationToInvite,
                                Object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (userToken == null)
                throw new ArgumentException(SR.GetString(SR.NullUserToken));
            if (applicationToInvite == null)
                throw new ArgumentNullException("applicationToInvite");
            if (applicationToInvite.Id == Guid.Empty)
                throw new PeerToPeerException(SR.GetString(SR.Collab_EmptyGuidError));

            //
            // We need at least one endpoint to send invitation to
            //
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");
            if (peerEndPoint.EndPoint == null)
                throw new ArgumentException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            PeerEndPointCollection peerEndPoints = new PeerEndPointCollection();
            peerEndPoints.Add(peerEndPoint);
            //
            // Call internal async invite
            //
            InternalInviteAsync(applicationToInvite.Id, message, invitationData, peerEndPoints, this, userToken);
        }

        //
        // Gets all applications for all endpoints for this contact
        //
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InternalGetAllApplications(Guid, Boolean):PeerApplicationCollection" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerApplicationCollection GetApplications()
        {
            return InternalGetAllApplications(Guid.Empty, false);
        }

        //
        // Gets specfic application for all endpoints for this contact
        //
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InternalGetAllApplications(Guid, Boolean):PeerApplicationCollection" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerApplicationCollection GetApplications(Guid applicationId)
        {
            return InternalGetAllApplications(applicationId, true);
        }

        //
        // Gets all applications for specific endpoint for this contact
        //
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalGetApplications(Guid, Boolean, PeerEndPoint):PeerApplicationCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerApplicationCollection GetApplications(PeerEndPoint peerEndPoint)
        {
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");

            return InternalGetApplications(Guid.Empty, false, peerEndPoint);
        }

        //
        // Gets specific application for specific endpoint for this contact
        //
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalGetApplications(Guid, Boolean, PeerEndPoint):PeerApplicationCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerApplicationCollection GetApplications(PeerEndPoint peerEndPoint, Guid applicationId)
        {
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");

            return InternalGetApplications(applicationId, true, peerEndPoint);
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalGetApplications(Guid, Boolean, PeerEndPoint):PeerApplicationCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private PeerApplicationCollection InternalGetAllApplications(Guid applicationId, bool guidSupplied)
        {
            Dictionary<Guid, PeerApplication> mergedApplications = new Dictionary<Guid, PeerApplication>();
            PeerApplicationCollection peerApplicationCollection;

            PeerEndPointCollection peerEndPoints = PeerEndPoints;

            foreach (PeerEndPoint peerEndPoint in peerEndPoints)
            {
                peerApplicationCollection = InternalGetApplications(applicationId, guidSupplied, peerEndPoint);

                foreach (PeerApplication peerApplication in peerApplicationCollection)
                {
                    mergedApplications[peerApplication.Id] = peerApplication;
                }
            }

            //
            // Return the application collection from the dictionary
            //

            Dictionary<Guid, PeerApplication>.ValueCollection applications = mergedApplications.Values;
            peerApplicationCollection = new PeerApplicationCollection();
            foreach (PeerApplication peerApplication in applications)
            {
                peerApplicationCollection.Add(peerApplication);
            }

            return peerApplicationCollection;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pApps of type: IntPtr*" />
        // <UsesUnsafeCode Name="Local pPeerApp of type: PEER_APPLICATION*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumApplications(System.IntPtr,System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local appArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static PeerApplicationCollection InternalGetApplications(Guid applicationId, bool guidSupplied, PeerEndPoint peerEndPoint)
        {
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering InternalGetApplications() with the following PeerEndPoint");
                peerEndPoint.TracePeerEndPoint();
            }

            PeerApplicationCollection peerAppColl = new PeerApplicationCollection();
            SafeCollabEnum handlePeerEnum = null;
            UInt32 appCount = 0;
            int errorCode = 0;

            GCHandle guidHandle = new GCHandle();
            IntPtr guidPtr = IntPtr.Zero;

            if (guidSupplied){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Guid supplied is {0}.", applicationId.ToString());
                GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(applicationId);
                guidHandle = GCHandle.Alloc(guid, GCHandleType.Pinned);
                guidPtr = guidHandle.AddrOfPinnedObject();
            }

            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin data to pass to native
            //

            GCHandle pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
            pep.pwzEndpointName = pepName.AddrOfPinnedObject();

            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            try
            {
                //
                // Enumerate through the applications for the endpoint
                //
                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumApplications(ptrPeerEP, guidPtr, out handlePeerEnum);

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumApplications returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetAppsFailed), errorCode);
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref appCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetAppsFailed), errorCode);
                }

                if (appCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "No PeerApplications found.");
                    return peerAppColl;
                }

                unsafe
                {
                    SafeCollabData appArray;
                    errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref appCount, out appArray);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetAppsFailed), errorCode);
                    }

                    IntPtr pPEER_APLICATION = appArray.DangerousGetHandle();
                    IntPtr* pApps = (IntPtr*)pPEER_APLICATION;

                    //
                    // Loop through the applications array from native
                    //
                    for (ulong i = 0; i < appCount; i++){
                        PEER_APPLICATION* pPeerApp = (PEER_APPLICATION*)pApps[i];
                        string description = Marshal.PtrToStringUni(pPeerApp->pwzDescription);
                        byte[] data = null;

                        if (pPeerApp->data.cbData != 0)
                        {
                            data = new byte[pPeerApp->data.cbData];
                            Marshal.Copy(pPeerApp->data.pbData, data, 0, (int)pPeerApp->data.cbData);
                        }

                        PeerApplication peerApp = new PeerApplication(CollaborationHelperFunctions.ConvertGUIDToGuid(pPeerApp->guid), description, data, null, null, PeerScope.None);

                        if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Retrieved following Application");
                            peerApp.TracePeerApplication();
                        }

                        peerAppColl.Add(peerApp);
                    }
                }
            }
            finally{
                if (guidHandle.IsAllocated) guidHandle.Free();
                if (pepName.IsAllocated) pepName.Free();
                if (peerEP.IsAllocated) peerEP.Free();
                if (handlePeerEnum != null) handlePeerEnum.Dispose();
            }


            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving InternalGetApplications(). " + 
                "Returning collection with {0} applications.", peerAppColl.Count);
            return peerAppColl;
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.InternalGetObjects(System.Guid,System.Boolean,System.Net.PeerToPeer.Collaboration.PeerEndPoint):System.Net.PeerToPeer.Collaboration.PeerObjectCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerObjectCollection GetObjects(PeerEndPoint peerEndPoint)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");

            return InternalGetObjects(Guid.Empty, false, peerEndPoint);
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: Peer.InternalGetObjects(System.Guid,System.Boolean,System.Net.PeerToPeer.Collaboration.PeerEndPoint):System.Net.PeerToPeer.Collaboration.PeerObjectCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerObjectCollection GetObjects(PeerEndPoint peerEndPoint, Guid objectId)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");

            return InternalGetObjects(objectId, true, peerEndPoint);
        }

        internal override void RefreshIfNeeded()
        { }

        private event EventHandler<ApplicationChangedEventArgs> m_applicationChanged;
        public event EventHandler<ApplicationChangedEventArgs> ApplicationChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddApplicationChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="3">
            // <ReferencesCritical Name="Method: RemoveApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="3" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                RemoveApplicationChanged(value);
            }
        }

        #region Application changed event variables
        private object m_lockAppChangedEvent;
        internal object LockAppChangedEvent
        {
            get{
                if (m_lockAppChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockAppChangedEvent, o, null);
                }
                return m_lockAppChangedEvent;
            }
        }

        private RegisteredWaitHandle m_regAppChangedWaitHandle;
        internal RegisteredWaitHandle AppChangedWaitHandle
        {
            set{
                m_regAppChangedWaitHandle = value;
            }
        }

        private AutoResetEvent m_appChangedEvent;
        internal AutoResetEvent AppChangedEvent
        {
            get{
                return m_appChangedEvent;
            }
            set{
                m_appChangedEvent = value;
            }
        }

        internal SafeCollabEvent m_safeAppChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Method: ApplicationChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void AddApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddApplicationChanged().");

            lock (LockAppChangedEvent)
            {
                if (m_applicationChanged == null){

                    m_appChangedEvent = new AutoResetEvent(false);
                    
                    //
                    // Register callback with a wait handle
                    //

                    AppChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_appChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(ApplicationChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.EndPointApplicationChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //
                    
                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        m_appChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safeAppChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ApplicationChangedRegFailed), errorCode);
                    }
                }
                m_applicationChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddApplicationChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: CleanContactAppEventVars():Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void RemoveApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() called.");
            lock (LockAppChangedEvent){
                m_applicationChanged -= callback;
                if (m_applicationChanged == null){
                    CleanContactAppEventVars();
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void CleanContactAppEventVars()
        {
            CollaborationHelperFunctions.CleanEventVars(ref m_regAppChangedWaitHandle,
                                                        ref m_safeAppChangedEvent,
                                                        ref m_appChangedEvent);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ApplicationChanged variables successful.");
        }

        protected virtual void OnApplicationChanged(ApplicationChangedEventArgs appChangedArgs)
        {
            EventHandler<ApplicationChangedEventArgs> handlerCopy = m_applicationChanged;

            if (handlerCopy != null){
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                    SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, appChangedArgs });
                else
                    handlerCopy(this, appChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the application changed event callback.");
            } 
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(System.Net.PeerToPeer.Collaboration.PEER_APPLICATION):System.Net.PeerToPeer.Collaboration.PeerApplication" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void ApplicationChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ApplicationChangedCallback() called.");

            if (m_Disposed) return;
            
            while (true)
            {
                ApplicationChangedEventArgs appChangedArgs = null;

                //
                // Get the event data for the fired event
                //

                try{
                    lock (LockAppChangedEvent){
                        if (m_safeAppChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(m_safeAppChangedEvent,
                                                                                     out eventData);
                    }

                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetApplicationChangedDataFailed), errorCode);
                    }

                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                                typeof(PEER_COLLAB_EVENT_DATA));
                    if (ped.eventType == PeerCollabEventType.EndPointApplicationChanged){
                        PEER_EVENT_APPLICATION_CHANGED_DATA appData = ped.applicationChangedData;

                        PeerContact peerContact = null;

                        if (appData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(appData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }

                        if (peerContact != null && Equals(peerContact)){
                            PEER_APPLICATION pa = (PEER_APPLICATION)Marshal.PtrToStructure(appData.pApplication, typeof(PEER_APPLICATION));

                            PeerApplication peerApplication = CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(pa); ;

                            PeerEndPoint peerEndPoint = null;

                            if (appData.pEndPoint != IntPtr.Zero){
                                PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(appData.pEndPoint, typeof(PEER_ENDPOINT));
                                peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            }

                            appChangedArgs = new ApplicationChangedEventArgs(   peerEndPoint,
                                                                                peerContact,
                                                                                appData.changeType,
                                                                                peerApplication);
                        }
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //
                if(appChangedArgs != null)
                    OnApplicationChanged(appChangedArgs);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ApplicationChangedCallback().");
        }

        private event EventHandler<ObjectChangedEventArgs> m_objectChanged;
        public event EventHandler<ObjectChangedEventArgs> ObjectChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddObjectChangedEvent(EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddObjectChangedEvent(value);
            }
            // <SecurityKernel Critical="True" Ring="3">
            // <ReferencesCritical Name="Method: RemoveObjectChangedEvent(EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>):Void" Ring="3" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                RemoveObjectChangedEvent(value);
            }
        }

        #region Object changed event variables
        private object m_lockObjChangedEvent;
        internal object LockObjChangedEvent
        {
            get
            {
                if (m_lockObjChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockObjChangedEvent, o, null);
                }
                return m_lockObjChangedEvent;
            }
        }

        private RegisteredWaitHandle m_regObjChangedWaitHandle;
        internal RegisteredWaitHandle ObjChangedWaitHandle
        {
            set{
                m_regObjChangedWaitHandle = value;
            }
        }

        private AutoResetEvent m_objChangedEvent;
        internal AutoResetEvent ObjChangedEvent
        {
            get{
                return m_objChangedEvent;
            }
            set{
                m_objChangedEvent = value;
            }
        }

        internal SafeCollabEvent m_safeObjChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Method: ObjectChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void AddObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddObjectChangedEvent().");

            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockObjChangedEvent){
                if (m_objectChanged == null){

                    m_objChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    ObjChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_objChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(ObjectChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.EndPointObjectChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        m_objChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safeObjChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ObjectChangedRegFailed), errorCode);
                    }
                }
                m_objectChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: CleanContactObjEventVars():Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void RemoveObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() called.");

            lock (LockObjChangedEvent){
                m_objectChanged -= callback;
                if (m_objectChanged == null){
                    CleanContactObjEventVars();
                }
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void CleanContactObjEventVars()
        {
            CollaborationHelperFunctions.CleanEventVars(ref m_regObjChangedWaitHandle,
                                                        ref m_safeObjChangedEvent,
                                                        ref m_objChangedEvent);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChangedEvent variables successful.");
        }

        protected virtual void OnObjectChanged(ObjectChangedEventArgs objChangedArgs)
        {
            EventHandler<ObjectChangedEventArgs> handlerCopy = m_objectChanged;

            if (handlerCopy != null){
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                    SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, objChangedArgs });
                else
                    handlerCopy(this, objChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the object changed event callback.");
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(System.Net.PeerToPeer.Collaboration.PEER_OBJECT):System.Net.PeerToPeer.Collaboration.PeerObject" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void ObjectChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ObjectChangedCallback() called.");

            if (m_Disposed) return;

            while (true){
                ObjectChangedEventArgs objChangedArgs = null;

                //
                // Get the event data for the fired event
                //

                try{
                    lock (LockObjChangedEvent){
                        if (m_safeObjChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(m_safeObjChangedEvent,
                                                                                        out eventData);
                    }

                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetObjectChangedDataFailed), errorCode);
                    }

                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                                typeof(PEER_COLLAB_EVENT_DATA));
                    if (ped.eventType == PeerCollabEventType.EndPointObjectChanged){
                        PEER_EVENT_OBJECT_CHANGED_DATA objData = ped.objectChangedData;

                        PeerContact peerContact = null;

                        if (objData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(objData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }

                        if (peerContact != null && Equals(peerContact)){
                            PeerEndPoint peerEndPoint = null;

                            if (objData.pEndPoint != IntPtr.Zero)
                            {
                                PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(objData.pEndPoint, typeof(PEER_ENDPOINT));
                                peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            }

                            PEER_OBJECT po = (PEER_OBJECT)Marshal.PtrToStructure(objData.pObject, typeof(PEER_OBJECT));

                            PeerObject peerObject = CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(po); ;



                            objChangedArgs = new ObjectChangedEventArgs(peerEndPoint,
                                                                                                peerContact,
                                                                                                objData.changeType,
                                                                                                peerObject);
                        }
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //
                if(objChangedArgs != null)
                    OnObjectChanged(objChangedArgs);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ObjectChangedCallback().");
        }

        private event EventHandler<PresenceChangedEventArgs> m_presenceChanged;
        public event EventHandler<PresenceChangedEventArgs> PresenceChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddPresenceChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddPresenceChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="3">
            // <ReferencesCritical Name="Method: RemovePresenceChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>):Void" Ring="3" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                RemovePresenceChanged(value);
            }
        }

        #region Presence changed event variables
        private object m_lockPresenceChangedEvent;
        internal object LockPresenceChangedEvent
        {
            get
            {
                if (m_lockPresenceChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockPresenceChangedEvent, o, null);
                }
                return m_lockPresenceChangedEvent;
            }
        }

        private RegisteredWaitHandle m_regPresenceChangedWaitHandle;
        internal RegisteredWaitHandle PresenceChangedWaitHandle
        {
            set{
                m_regPresenceChangedWaitHandle = value;
            }
        }

        private AutoResetEvent m_presenceChangedEvent;
        internal AutoResetEvent PresenceChangedEvent
        {
            get{
                return m_presenceChangedEvent;
            }
            set{
                m_presenceChangedEvent = value;
            }
        }

        internal SafeCollabEvent m_safePresenceChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Method: PresenceChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void AddPresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddPresenceChanged().");

            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockPresenceChangedEvent){
                if (m_presenceChanged == null){

                    m_presenceChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    PresenceChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_presenceChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(PresenceChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.EndPointPresenceChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        m_presenceChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safePresenceChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_PresenceChangedRegFailed), errorCode);
                    }
                }
                m_presenceChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddPresenceChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: CleanContactPresenceEventVars():Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void RemovePresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() called.");

            lock (LockPresenceChangedEvent){
                m_presenceChanged -= callback;
                if (m_presenceChanged == null){
                    CleanContactPresenceEventVars();
                }
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void CleanContactPresenceEventVars()
        {
            CollaborationHelperFunctions.CleanEventVars(ref m_regPresenceChangedWaitHandle,
                                                        ref m_safePresenceChangedEvent,
                                                        ref m_presenceChangedEvent);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean PresenceChanged variables successful.");
        }

        protected virtual void OnPresenceChanged(PresenceChangedEventArgs presenceChangedArgs)
        {
            EventHandler<PresenceChangedEventArgs> handlerCopy = m_presenceChanged;

            if (handlerCopy != null){
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                    SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, presenceChangedArgs });
                else
                    handlerCopy(this, presenceChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the presence changed event callback.");
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal virtual void PresenceChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PresenceChangedCallback() called.");

            if (m_Disposed) return;

            while (true){
                PresenceChangedEventArgs presenceChangedArgs = null;

                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockPresenceChangedEvent){
                        if (m_safePresenceChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(m_safePresenceChangedEvent,
                                                                                     out eventData);
                    }

                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetPresenceChangedDataFailed), errorCode);
                    }
                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                                typeof(PEER_COLLAB_EVENT_DATA));
                    if (ped.eventType == PeerCollabEventType.EndPointPresenceChanged){

                        PEER_EVENT_PRESENCE_CHANGED_DATA presenceData = ped.presenceChangedData;
                        PeerContact peerContact = null;

                        if (presenceData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(presenceData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }

                        if ((peerContact != null) && Equals(peerContact)){
                            PeerEndPoint peerEndPoint = null;

                            if (presenceData.pEndPoint != IntPtr.Zero){

                                PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(presenceData.pEndPoint, typeof(PEER_ENDPOINT));
                                peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            }                            
                            
                            PeerPresenceInfo peerPresenceInfo = null;

                            if (presenceData.pPresenceInfo != IntPtr.Zero){
                                PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(presenceData.pPresenceInfo, typeof(PEER_PRESENCE_INFO));
                                peerPresenceInfo = new PeerPresenceInfo();
                                peerPresenceInfo.PresenceStatus = ppi.status;
                                peerPresenceInfo.DescriptiveText = ppi.descText;
                            }

                            presenceChangedArgs = new PresenceChangedEventArgs(peerEndPoint,
                                                                                peerContact,
                                                                                presenceData.changeType,
                                                                                peerPresenceInfo);
                        }
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //
                if (presenceChangedArgs != null)
                    OnPresenceChanged(presenceChangedArgs);
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving PresenceChangedCallback().");
        }

        public bool Equals(PeerContact other)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (other != null){
                if (other.PeerName != null){
                    return other.PeerName.Equals(PeerName);
                }
                else if (PeerName == null)
                    return true;
            }
            return false;

        }

        public override bool Equals(object obj)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerContact comparandPeerContact = obj as PeerContact;
            if (comparandPeerContact != null){
                return Equals(comparandPeerContact);
            }
            return false;
        }

        public new static bool Equals(object objA, object objB)
        {
            PeerContact comparandPeerContact1 = objA as PeerContact;
            PeerContact comparandPeerContact2 = objB as PeerContact;

            if ((comparandPeerContact1 != null) && (comparandPeerContact2 != null))
            {
                if (comparandPeerContact1.PeerName != null){
                    return comparandPeerContact1.PeerName.Equals(comparandPeerContact2.PeerName);
                }
                else if (comparandPeerContact2.PeerName == null)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            return ((PeerName == null) ? 0 : PeerName.GetHashCode());
        }

        public override string ToString()
        {
            return m_displayName;
        }

        private bool m_Disposed;

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: CleanContactAppEventVars():Void" Ring="2" />
        // <ReferencesCritical Name="Method: CleanContactObjEventVars():Void" Ring="2" />
        // <ReferencesCritical Name="Method: CleanContactPresenceEventVars():Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed){
                try{
                    CleanContactAppEventVars();
                    CleanContactObjEventVars();
                    CleanContactPresenceEventVars();
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
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_PeerName", m_peerName);
            info.AddValue("_NickName", m_nickname);
            info.AddValue("_DisplayName", m_displayName);
            if(m_emailAddress != null)
                info.AddValue("_EmailAddress", m_emailAddress.ToString());
            info.AddValue("_SubscribeAllowed", m_subscribeAllowed);
            info.AddValue("_Credentials", m_credentials.RawData);
            info.AddValue("_ContactXml", m_contactXml);
            info.AddValue("_JustCreated", m_justCreated);
        }

        //
        // Tracing information for Peer Contact
        //
        internal void TracePeerContact()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerContact");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tPeerName: {0}", PeerName);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tNickname: {0}", Nickname);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tEmailAddress: {0}", EmailAddress.ToString());
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tSubscribeAllowed: {0}", SubscribeAllowed);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tCredentials FriendlyName: {0}", Credentials.FriendlyName);
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tCredentials raw data:");
                Logging.DumpData(Logging.P2PTraceSource, TraceEventType.Verbose, Logging.P2PTraceSource.MaxDataSize, Credentials.RawData, 0, Credentials.RawData.Length);
            }
            else{
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tCredentials raw data length {0}", Credentials.RawData.Length);
            }
        }
    }

    //
    // Manages collection of peer contacts
    //
    [Serializable]
    public class PeerContactCollection : Collection<PeerContact>
    {
        internal PeerContactCollection() { }

        protected override void SetItem(int index, PeerContact item)
        {
            // nulls not allowed
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, PeerContact item)
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

            foreach (PeerContact peerContact in this)
            {
                if (!first){
                    builder.Append(", ");
                }
                else{
                    first = false;
                }
                builder.Append(peerContact.ToString());
            }
            return builder.ToString();
        }
    }

}
