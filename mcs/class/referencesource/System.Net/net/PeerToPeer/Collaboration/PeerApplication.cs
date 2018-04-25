//------------------------------------------------------------------------------
// <copyright file="PeerApplication.cs" company="Microsoft">
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
    /// Peer Application
    /// </summary>
    [Serializable]
    public class PeerApplication : IDisposable, IEquatable<PeerApplication>, ISerializable
    {
        private const int c_16K = 16384;
        private Guid m_id;
        private byte[] m_data;
        private string m_description;
        private string m_path;
        private string m_commandLineArgs;
        private PeerScope m_peerScope;
        private ISynchronizeInvoke m_synchronizingObject;

        //
        // Initialize on first access of this class
        //

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static PeerApplication()
        {
            CollaborationHelperFunctions.Initialize();
        }

        public PeerApplication(){}

        public PeerApplication( Guid id, string description, byte[] data, string path, 
                                string commandLineArgs, PeerScope peerScope)
        {
            if ((data != null) && (data.Length > c_16K))
                throw new ArgumentException(SR.GetString(SR.Collab_ApplicationDataSizeFailed), "data");

            m_id = id;
            m_description = description;
            m_data = data;
            m_path = path;
            m_commandLineArgs = commandLineArgs;
            m_peerScope = peerScope;
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerApplication(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            m_id = (Guid) serializationInfo.GetValue("_Id", typeof(Guid));
            m_data = (byte []) serializationInfo.GetValue("_Data", typeof(byte[]));
            m_description = serializationInfo.GetString("_Description");
            m_path = serializationInfo.GetString("_Path");
            m_commandLineArgs = serializationInfo.GetString("_CommandLineArgs");
            m_peerScope = (PeerScope) serializationInfo.GetInt32("_Scope");
        }

        public Guid Id
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                return m_id; 
            }
            set {
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
                    throw new ArgumentException(SR.GetString(SR.Collab_ApplicationDataSizeFailed));

                m_data = value;
            }
        }

        public string Description
        {
            get{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_description;
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                m_description = value;
            }
        }

        public string Path
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_path;
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_path = value;
            }
        }

        public string CommandLineArgs
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                return m_commandLineArgs;
            }
            set {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                m_commandLineArgs = value;
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
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemoveApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="2" />
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
        private object LockAppChangedEvent
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
        private AutoResetEvent m_appChangedEvent;   
        private SafeCollabEvent m_safeAppChangedEvent;      
        #endregion 

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <ReferencesCritical Name="Method: ApplicationChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void AddApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddApplicationChanged() called.");

            lock (LockAppChangedEvent){
                if (m_applicationChanged == null){
                    if (m_id.Equals(Guid.Empty)){ 
                        throw new PeerToPeerException("No application guid defined"); }

                    m_appChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    m_regAppChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_appChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(ApplicationChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.EndPointApplicationChanged;

                    GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(m_id);
                    GCHandle guidHandle = GCHandle.Alloc(guid, GCHandleType.Pinned);

                    pcer.pInstance = guidHandle.AddrOfPinnedObject();

                    //
                    // Register event with collab
                    //

                    try{
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Registering event with App ID {0}", m_id.ToString());

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
                    finally{
                        if (guidHandle.IsAllocated)
                            guidHandle.Free();
                    }
                }
                m_applicationChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddApplicationChanged() successful.");

        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemoveApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() called.");

            lock (LockAppChangedEvent){
                m_applicationChanged -= callback;
                if (m_applicationChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regAppChangedWaitHandle,
                                                                ref m_safeAppChangedEvent,
                                                                ref m_appChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ApplicationChanged variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() successful.");
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

        //
        // Handles the callback when there is an application changed event from native collaboration
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(System.Net.PeerToPeer.Collaboration.PEER_APPLICATION):System.Net.PeerToPeer.Collaboration.PeerApplication" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void ApplicationChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ApplicationChangedCallback() called.");

            if (m_Disposed) return;

            while (true){
                ApplicationChangedEventArgs appChangedArgs = null;
                
                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockAppChangedEvent)
                    {
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
                        PEER_APPLICATION pa = (PEER_APPLICATION)Marshal.PtrToStructure(appData.pApplication, typeof(PEER_APPLICATION));

                        PeerApplication peerApplication = CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(pa); ;

                        //
                        // Check if the Guid of the fired app is indeed our guid
                        //

                        if (Guid.Equals(m_id, peerApplication.Id)){
                            PeerContact peerContact = null;
                            PeerEndPoint peerEndPoint = null;

                            if (appData.pContact != IntPtr.Zero){
                                PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(appData.pContact, typeof(PEER_CONTACT));
                                peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                            }

                            if (appData.pEndPoint != IntPtr.Zero){
                                PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(appData.pEndPoint, typeof(PEER_ENDPOINT));
                                peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            }

                            appChangedArgs = new ApplicationChangedEventArgs(peerEndPoint,
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

        public bool Equals(PeerApplication other)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (other != null){
                return Guid.Equals(other.Id, m_id);
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            PeerApplication comparandPeerApplication = obj as PeerApplication;
            if (comparandPeerApplication != null){
                return Guid.Equals(comparandPeerApplication.Id, Id);
            }

            return false;
        }

        public new static bool Equals(object objA, object objB)
        {
            PeerApplication comparandPeerApplication1 = objA as PeerApplication;
            PeerApplication comparandPeerApplication2 = objB as PeerApplication;

            if ((comparandPeerApplication1 != null) && (comparandPeerApplication2 != null)){
                return Guid.Equals(comparandPeerApplication1.Id, comparandPeerApplication2.Id);
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

            return Id.ToString() + " " + Description;
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
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed){
                CollaborationHelperFunctions.CleanEventVars(ref m_regAppChangedWaitHandle,
                                                            ref m_safeAppChangedEvent,
                                                            ref m_appChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ApplicationChanged variables successful.");
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
            info.AddValue("_Description", m_description);
            info.AddValue("_Path", m_path);
            info.AddValue("_CommandLineArgs", m_commandLineArgs);
            info.AddValue("_Scope", m_peerScope);
        }

        //
        // Tracing information for Peer Application
        //
        internal void TracePeerApplication()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerApplication");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tGuid: {0}", Id);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tDescription: {0}", Description);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tPath: {0}", Path);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tCommandLineArgs: {0}", CommandLineArgs);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tPeerScope: {0}", PeerScope);

            if (Data != null){

                if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tApplication data:");
                    Logging.DumpData(Logging.P2PTraceSource, TraceEventType.Verbose, Logging.P2PTraceSource.MaxDataSize, Data, 0, Data.Length);
                }
                else{
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tApplication data length {0}", Data.Length);
                }
            }
        }
    }

    //
    // Manages collection of peer applications
    //
    [Serializable]
    public class PeerApplicationCollection : Collection<PeerApplication>
    {
        internal PeerApplicationCollection() { }
        protected override void SetItem(int index, PeerApplication item)
        {
            // nulls not allowed
            if (item == null){
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, PeerApplication item)
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

            foreach (PeerApplication peerApplication in this){
                if(!first){
                    builder.Append(", ");
                }
                else{
                    first = false;
                }
                builder.Append(peerApplication.ToString());
            }
            return builder.ToString();
        }
    }
}
