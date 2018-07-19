//------------------------------------------------------------------------------
// <copyright file="PeerEndPoint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;
    using System.ComponentModel;
    using System.Threading;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This is the event args class we give back when 
    /// we have have a name changed event fired by native
    /// </summary>
    public class NameChangedEventArgs : EventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        private PeerContact m_peerContact;
        private string m_name;

        internal NameChangedEventArgs(PeerEndPoint peerEndPoint, PeerContact peerContact,
                                       string name)
        {
            m_peerEndPoint = peerEndPoint;
            m_peerContact = peerContact;
            m_name = name;
        }

        public PeerEndPoint PeerEndPoint
        {
            get{
                return m_peerEndPoint;
            }
        }

        public PeerContact PeerContact
        {
            get{
                return m_peerContact;
            }
        }

        public string Name
        {
            get{
                return m_name;
            }
        }
    }

    /// <summary>
    /// PeerEndpoint class encapsulates the functionality of an 
    /// endpoint in the peer collaboration scope.
    /// </summary>
    [Serializable]
    public class PeerEndPoint : IDisposable, IEquatable<PeerEndPoint>, ISerializable
    {
        private string m_endPointName;
        private IPEndPoint m_endPoint;
        private ISynchronizeInvoke m_synchronizingObject;

        public PeerEndPoint() { }

        public PeerEndPoint(IPEndPoint endPoint):this(endPoint, null) 
        { }

        public PeerEndPoint(IPEndPoint endPoint, string endPointName) 
        {
            if (endPoint == null){
                throw new ArgumentNullException("endPoint");
            }

            //
            // Validate that this is an IPv6 address
            //
            if ((m_endPoint != null) && (m_endPoint.AddressFamily != AddressFamily.InterNetworkV6)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerEndPoint Endpoint set parameter is not IPv6.");
                throw new ArgumentException("endPoint", SR.GetString(SR.Collab_EndPointNotIPv6Error));
            }

            m_endPoint = endPoint;
            m_endPointName = endPointName;
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerEndPoint(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            m_endPointName = serializationInfo.GetString("_EndPointName");
            m_endPoint = (IPEndPoint)serializationInfo.GetValue("_EndPoint", typeof(IPEndPoint));
        }

        public string Name
        {
            get { return m_endPointName; }
            set { m_endPointName = value; }
        }

        public IPEndPoint EndPoint
        {
            get { return m_endPoint; }
            set { 
                //
                // Validate that this is an IPv6 address
                //
                if ((m_endPoint != null) && (m_endPoint.AddressFamily != AddressFamily.InterNetworkV6)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerEndPoint Endpoint set parameter is not IPv6.");
                    throw new PeerToPeerException(SR.GetString(SR.Collab_EndPointNotIPv6Error));
                }
                m_endPoint = value; 
            }
        }

        /// <summary>
        /// Gets and set the object used to marshall event handlers calls for stand alone 
        /// events
        /// </summary>
        [Browsable(false), DefaultValue(null), Description(SR.SynchronizingObject)]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_synchronizingObject;
            }
            set
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_synchronizingObject = value;
            }
        }

        private event EventHandler<NameChangedEventArgs> m_nameChanged;
        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                CollaborationHelperFunctions.Initialize();

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddNameChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemoveNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                CollaborationHelperFunctions.Initialize();

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                
                RemoveNameChanged(value);
            }
        }

        #region Name changed event variables
        private object m_lockNameChangedEvent;
        private object LockNameChangedEvent
        {
            get{
                if (m_lockNameChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockNameChangedEvent, o, null);
                }
                return m_lockNameChangedEvent;
            }
        }
        private RegisteredWaitHandle m_regNameChangedWaitHandle;
        private AutoResetEvent m_nameChangedEvent;
        private SafeCollabEvent m_safeNameChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: NameChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void AddNameChanged(EventHandler<NameChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddNameChanged().");
            
            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockNameChangedEvent){
                if (m_nameChanged == null){

                    m_nameChangedEvent = new AutoResetEvent(false);
                    
                    //
                    // Register callback with a wait handle
                    //

                    m_regNameChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_nameChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(NameChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();

                    pcer.eventType = PeerCollabEventType.EndPointChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        m_nameChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safeNameChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_NameChangedRegFailed), errorCode);
                    }
                }
                m_nameChanged += callback;
            }


            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddNameChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemoveNameChanged(EventHandler<NameChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveNameChanged() called.");
            lock (LockNameChangedEvent){
                m_nameChanged -= callback;
                if (m_nameChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regNameChangedWaitHandle,
                                                                ref m_safeNameChangedEvent,
                                                                ref m_nameChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean NameChanged variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveNameChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void NameChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "NameChangedCallback() called.");

            if (m_Disposed) return;

            while (true){
                NameChangedEventArgs nameChangedArgs = null;
                
                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockNameChangedEvent)
                    {
                        if (m_safeNameChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(m_safeNameChangedEvent,
                                                                                        out eventData);
                    }
                    
                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetNameChangedDataFailed), errorCode);
                    }

                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                                                typeof(PEER_COLLAB_EVENT_DATA));
                    if (ped.eventType == PeerCollabEventType.EndPointChanged){
                        PEER_EVENT_ENDPOINT_CHANGED_DATA epData = ped.endpointChangedData;
                        PeerEndPoint peerEndPoint = null;

                        if (epData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(epData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                        }

                        if ((peerEndPoint != null) && Equals(peerEndPoint)){
                            PeerContact peerContact = null;

                            if (epData.pContact != IntPtr.Zero){
                                PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(epData.pContact, typeof(PEER_CONTACT));
                                peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                            }


                            nameChangedArgs = new NameChangedEventArgs(peerEndPoint,
                                                                        peerContact,
                                                                        peerEndPoint.Name);
                        }
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //

                if (nameChangedArgs != null){
                    OnNameChanged(nameChangedArgs);
                    
                    //
                    // Change the name with the new name
                    //
                    Name = nameChangedArgs.PeerEndPoint.Name;
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving NameChangedCallback().");
        }

        protected void OnNameChanged(NameChangedEventArgs nameChangedArgs)
        {
            EventHandler<NameChangedEventArgs> handlerCopy = m_nameChanged;

            if (handlerCopy != null){
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                    SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, nameChangedArgs });
                else
                    handlerCopy(this, nameChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the name changed event callback.");
            }
        }
        
        public bool Equals(PeerEndPoint other)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            //
            // Equality means same ipendpoints
            //

            if (other != null){
                return other.EndPoint.Equals(EndPoint);
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            PeerEndPoint comparandPeerEndPoint = obj as PeerEndPoint;
            if (comparandPeerEndPoint != null){
                return comparandPeerEndPoint.EndPoint.Equals(EndPoint);
            }
            return false;
        }

        public new static bool Equals(object objA, object objB)
        {
            PeerEndPoint comparandPeerEndPoint1 = objA as PeerEndPoint;
            PeerEndPoint comparandPeerEndPoint2 = objB as PeerEndPoint;

            if ((comparandPeerEndPoint1 != null) && (comparandPeerEndPoint2 != null)){
                return comparandPeerEndPoint1.EndPoint.Equals(comparandPeerEndPoint2.EndPoint);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            return EndPoint.GetHashCode();
        }

        public override string ToString()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            return m_endPointName;
        }

        private bool m_Disposed;

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: Dispose(Boolean):Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed){
                CollaborationHelperFunctions.CleanEventVars(ref m_regNameChangedWaitHandle,
                                                            ref m_safeNameChangedEvent,
                                                            ref m_nameChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean NameChanged variables successful.");
            }
            m_Disposed = true;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="GetObjectData(SerializationInfo, StreamingContext):Void" />
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
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_EndPointName", m_endPointName);
            info.AddValue("_EndPoint", m_endPoint);
        }

        internal void TracePeerEndPoint()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerEndPoint");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tEndPoint: {0}", (EndPoint != null? EndPoint.ToString(): null));
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tDescription: {0}", Name);
        }

    }

    /// <summary>
    /// Represents a collection of PeerEndPoints
    /// </summary>
    [Serializable]
    public class PeerEndPointCollection : Collection<PeerEndPoint>, IEquatable<PeerEndPointCollection>
    {
        internal PeerEndPointCollection() { }

        protected override void SetItem(int index, PeerEndPoint item)
        {
            //
            // Null peerendpoints not allowed
            //
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, PeerEndPoint item)
        {
            //
            // Null peerendpoints not allowed
            //
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        public override string ToString()
        {
            bool first = true;
            StringBuilder builder = new StringBuilder();

            foreach (PeerEndPoint peerEndPoint in this){
                if (!first){
                    builder.Append(", ");
                }
                else{
                    first = false;
                }
                builder.Append(peerEndPoint.ToString());
            }
            return builder.ToString();
        }

        public bool Equals(PeerEndPointCollection other)
        {
            bool equal = false;

            if (other != null){
                foreach (PeerEndPoint peerEndPoint1 in other)
                    foreach (PeerEndPoint peerEndPoint2 in this)
                        if (!peerEndPoint1.Equals(peerEndPoint2)){
                            return equal;
                        }
                equal = true;
            }

            return equal;
        }
    }

}
