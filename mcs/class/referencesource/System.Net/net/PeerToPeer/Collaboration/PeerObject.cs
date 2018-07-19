//------------------------------------------------------------------------------
// <copyright file="PeerObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This class handles all the functionality and events associated with the Collaboration
    /// Peer Object
    /// </summary>
    [Serializable]
    public class PeerObject : IDisposable, IEquatable<PeerObject>, ISerializable
    {
        private const int c_16K = 16384;
        private Guid m_id;
        private byte[] m_data;
        private PeerScope m_peerScope;
        private ISynchronizeInvoke m_synchronizingObject;

        //
        // Initialize on first access of this class
        //

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static PeerObject()
        {
            CollaborationHelperFunctions.Initialize();
        }

        public PeerObject()
        {
            m_id = Guid.NewGuid();
        }

        public PeerObject(Guid Id, byte[] data, PeerScope peerScope)
        {
            if ((data != null) && (data.Length > c_16K))
                throw new ArgumentException(SR.GetString(SR.Collab_ObjectDataSizeFailed), "data");

            m_id = Id;
            m_peerScope = peerScope;
            m_data = data;
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerObject(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            m_id = (Guid) serializationInfo.GetValue("_Id", typeof(Guid));
            m_data = (byte[])serializationInfo.GetValue("_Data", typeof(byte[]));
            m_peerScope = (PeerScope) serializationInfo.GetInt32("_Scope");
        }

        public Guid Id
        {
            get{ 
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_id; 
            }
            set{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_id = value;
            }
        }

        public byte[] Data
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_data; 
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                if ((value != null) && (value.Length > c_16K))
                    throw new ArgumentException(SR.GetString(SR.Collab_ObjectDataSizeFailed));

                m_data = value; 
            }
        }

        public PeerScope PeerScope
        {
            get { 
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_peerScope; 
            }
            set { 
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_peerScope = value; 
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
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemoveObjectChangedEvent(EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>):Void" Ring="2" />
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
        private object LockObjChangedEvent
        {
            get{
                if (m_lockObjChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockObjChangedEvent, o, null);
                }
                return m_lockObjChangedEvent;
            }
        }
        private RegisteredWaitHandle m_regObjChangedWaitHandle;
        private AutoResetEvent m_objChangedEvent;
        private SafeCollabEvent m_safeObjChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Method: ObjectChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void AddObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChanged() called.");

            lock (LockObjChangedEvent){
                if (m_objectChanged == null){
                    if (m_id.Equals(Guid.Empty))
                        throw (new PeerToPeerException(SR.GetString(SR.Collab_EmptyGuidError)));

                    m_objChangedEvent = new AutoResetEvent(false);
                    
                    //
                    // Register callback with a wait handle
                    //

                    m_regObjChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_objChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(ObjectChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.EndPointObjectChanged;

                    GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(m_id);
                    GCHandle guidHandle = GCHandle.Alloc(guid, GCHandleType.Pinned);

                    //
                    // Register event with collab
                    //

                    pcer.pInstance = guidHandle.AddrOfPinnedObject();
                    try{
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
                    finally{
                        if (guidHandle.IsAllocated)
                            guidHandle.Free();
                    }
                }
                m_objectChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemoveObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChanged() called.");

            lock (LockObjChangedEvent){
                m_objectChanged -= callback;
                if (m_objectChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regObjChangedWaitHandle,
                                                                ref m_safeObjChangedEvent,
                                                                ref m_objChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChangedEvent variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChanged() successful.");
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

        //
        // Handles the callback when there is an object changed event from native collaboration
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(System.Net.PeerToPeer.Collaboration.PEER_OBJECT):System.Net.PeerToPeer.Collaboration.PeerObject" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void ObjectChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null ;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ObjectChangedCallback() called.");

            while (true){
                ObjectChangedEventArgs objectChangedArgs = null;

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
                        PEER_OBJECT po = (PEER_OBJECT)Marshal.PtrToStructure(objData.pObject, typeof(PEER_OBJECT));

                        PeerObject peerObject = CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(po);

                        //
                        // Check if the Guid of the fired app is indeed our guid
                        //

                        if (Guid.Equals(m_id, peerObject.Id)){
                            PeerContact peerContact = null;
                            PeerEndPoint peerEndPoint = null;

                            if (objData.pContact != IntPtr.Zero){
                                PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(objData.pContact, typeof(PEER_CONTACT));
                                peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                            }

                            if (objData.pEndPoint != IntPtr.Zero){
                                PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(objData.pEndPoint, typeof(PEER_ENDPOINT));
                                peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            }

                            objectChangedArgs = new ObjectChangedEventArgs(peerEndPoint,
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
                if(objectChangedArgs != null)
                    OnObjectChanged(objectChangedArgs);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ObjectChangedCallback().");
        }

        public bool Equals(PeerObject other)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (other != null){
                return other.Id.Equals(Id);
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerObject comparandPeerObject = obj as PeerObject;
            
            if (comparandPeerObject != null){
                return comparandPeerObject.Id.Equals(Id);
            }
            return false;
        }

        public new static bool Equals(object objA, object objB)
        {
            PeerObject comparandPeerObject1 = objA as PeerObject;
            PeerObject comparandPeerObject2 = objB as PeerObject;

            if ((comparandPeerObject1 != null) && (comparandPeerObject2 != null)){
                return Guid.Equals(comparandPeerObject1.Id, comparandPeerObject2.Id);
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
            return Id.ToString();
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
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed){
                CollaborationHelperFunctions.CleanEventVars(ref m_regObjChangedWaitHandle,
                                                            ref m_safeObjChangedEvent,
                                                            ref m_objChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChangedEvent variables successful.");
                m_Disposed = true;
            }
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
            info.AddValue("_Id", m_id);
            info.AddValue("_Data", m_data);
            info.AddValue("_Scope", m_peerScope);
        }

        internal void TracePeerObject()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerObject");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tGuid: {0}", Id);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tPeerScope: {0}", PeerScope);
            
            if (Data != null){
                if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tObject data:");
                    Logging.DumpData(Logging.P2PTraceSource, TraceEventType.Verbose, Logging.P2PTraceSource.MaxDataSize, Data, 0, Data.Length);
                }
                else{
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tObject data length {0}", Data.Length);
                }
            }
        }
    }

    //
    // Manages collection of peer objects
    //
    [Serializable]
    public class PeerObjectCollection : Collection<PeerObject>
    {
        internal PeerObjectCollection() { }

        protected override void SetItem(int index, PeerObject item)
        {
            // nulls not allowed
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, PeerObject item)
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

            foreach (PeerObject peerObject in this)
            {
                if (!first){
                    builder.Append(", ");
                }
                else{
                    first = false;
                }
                builder.Append(peerObject.ToString());
            }
            return builder.ToString();
        }
    }
}
