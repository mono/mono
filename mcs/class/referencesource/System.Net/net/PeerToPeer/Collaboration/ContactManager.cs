//------------------------------------------------------------------------------
// <copyright file="ContactManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Net.Sockets;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Net.Mail;
    using System.Security.Cryptography.X509Certificates;
    using System.ComponentModel;
    using System.Threading;
    using System.Diagnostics;

    /// <summary>
    /// This is the event args class we give back when 
    /// we have completed the createcontactasync call
    /// </summary>
    public class CreateContactCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PeerContact m_peerContact;
        internal CreateContactCompletedEventArgs(   PeerContact peerContact,
                                                    Exception error,
                                                    bool cancelled,
                                                    object userToken)
            : base(error, cancelled, userToken)
        {
            m_peerContact = peerContact;
        }

        public PeerContact PeerContact
        {
            get{
                return m_peerContact;
            }
        }
    }

    /// <summary>
    /// This is the event args class we give back when 
    /// we have a subscription changed event from native 
    /// </summary>
    public class SubscriptionListChangedEventArgs : EventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        private PeerContact m_peerContact;
        private PeerChangeType m_peerChangeType;

        internal SubscriptionListChangedEventArgs(PeerEndPoint peerEndPoint, PeerContact peerContact,
                                              PeerChangeType peerChangeType)
        {
            m_peerEndPoint = peerEndPoint;
            m_peerContact = peerContact;
            m_peerChangeType = peerChangeType;
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

    }

    /// <summary>
    /// This class handles all the operation related to contacts and thier storage in the 
    /// Windows Address Book
    /// </summary>
    public sealed class ContactManager : IDisposable
    {
        private ISynchronizeInvoke m_synchronizingObject;

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal ContactManager() 
        {
            CollaborationHelperFunctions.Initialize();

            OnCreateContactCompletedDelegate = new SendOrPostCallback(CreateContactCompletedWaitCallback);
        }

        //
        // Returns the peer collaboration users' contact
        //
        public static PeerContact LocalContact
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
            // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
            // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
            // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
            // <ReferencesCritical Name="Local contact of type: SafeCollabData" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT,System.Boolean):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                SafeCollabData contact = null;
                MyContact myContact; 
                PEER_CONTACT pc;

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Get MyContact called.");
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                try{
                    //
                    // Get my contact from native with null peer name
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabGetContact(null, out contact);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0,
                                    "PeerCollabGetContact returned with errorcode {0}", errorCode);
                        return null;
                    }
                    pc = (PEER_CONTACT)Marshal.PtrToStructure(contact.DangerousGetHandle(), typeof(PEER_CONTACT));
                    myContact = (MyContact) CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc, true);
                }
                finally{
                    if (contact != null) contact.Dispose();
                }

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Get MyContact successful.");

                return myContact;
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

        private event EventHandler<SubscriptionListChangedEventArgs> m_subscriptionListChanged;
        public event EventHandler<SubscriptionListChangedEventArgs> SubscriptionListChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddSubscriptionListChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.SubscriptionListChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddSubscriptionListChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemoveSubscriptionListChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.SubscriptionListChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                RemoveSubscriptionListChanged(value);
            }
        }

        #region Subcription list changed event variables
        private object m_lockSubLstChangedEvent;
        private object LockSubLstChangedEvent
        {
            get{
                if (m_lockSubLstChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockSubLstChangedEvent, o, null);
                }
                return m_lockSubLstChangedEvent;
            }
        }

        private RegisteredWaitHandle m_regSubLstChangedWaitHandle;
        private AutoResetEvent m_subLstChangedEvent;
        private SafeCollabEvent m_safeSubLstChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: SubListChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeSubLstChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void AddSubscriptionListChanged(EventHandler<SubscriptionListChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            lock (LockSubLstChangedEvent){
                if (m_subscriptionListChanged == null){
                    m_subLstChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //
                    
                    m_regSubLstChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_subLstChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(SubListChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();

                    pcer.eventType = PeerCollabEventType.WatchListChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        m_subLstChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safeSubLstChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_SubListChangedRegFailed), errorCode);
                    }
                }
                m_subscriptionListChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddSubscriptionListChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeSubLstChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemoveSubscriptionListChanged(EventHandler<SubscriptionListChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveSubscriptionListChanged() called.");

            lock (LockSubLstChangedEvent){
                m_subscriptionListChanged -= callback;
                if (m_subscriptionListChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regSubLstChangedWaitHandle,
                                                                ref m_safeSubLstChangedEvent,
                                                                ref m_subLstChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean SubscriptionListChanged variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveSubscriptionListChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeSubLstChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void SubListChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "SubListChangedCallback() called.");
            
            while (true){
                SubscriptionListChangedEventArgs subListChangedArgs = null;

                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockSubLstChangedEvent){
                        if (m_safeSubLstChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(m_safeSubLstChangedEvent,
                                                                                     out eventData);
                    }
                    
                    if (errorCode == UnsafeCollabReturnCodes.PEER_S_NO_EVENT_DATA)
                        break;
                    else if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEventData returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetSubListChangedDataFailed), errorCode);
                    }

                    PEER_COLLAB_EVENT_DATA ped = (PEER_COLLAB_EVENT_DATA)Marshal.PtrToStructure(eventData.DangerousGetHandle(),
                                                                            typeof(PEER_COLLAB_EVENT_DATA));

                    if (ped.eventType == PeerCollabEventType.WatchListChanged){
                        PEER_EVENT_WATCHLIST_CHANGED_DATA watchlistData = ped.watchListChangedData;

                        PeerContact peerContact = null;

                        if (watchlistData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(watchlistData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }


                        subListChangedArgs = new SubscriptionListChangedEventArgs(null,
                                                                                        peerContact,
                                                                                        watchlistData.changeType);

                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<SubscriptionListChangedEventArgs> handlerCopy = m_subscriptionListChanged;

                if ((subListChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, subListChangedArgs });
                    else
                        handlerCopy(this, subListChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the subscription list changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving SubListChangedCallback().");
        }

        private event EventHandler<NameChangedEventArgs> m_nameChanged;
        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddNameChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemoveNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
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
            
            while (true){
                NameChangedEventArgs nameChangedArgs = null;

                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockNameChangedEvent){
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
                        PEER_EVENT_ENDPOINT_CHANGED_DATA endpointData = ped.endpointChangedData;

                        PeerContact peerContact = null;
                        PeerEndPoint peerEndPoint = null;
                        string newName = null;

                        if (endpointData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(endpointData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }

                        if (endpointData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(endpointData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                            newName = peerEndPoint.Name;
                        }

                        nameChangedArgs = new NameChangedEventArgs(peerEndPoint,
                                                                    peerContact,
                                                                    newName);
                    }
                }
                finally{
                    if(eventData != null) eventData.Dispose();
                }


                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<NameChangedEventArgs> handlerCopy = m_nameChanged;

                if ((nameChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, nameChangedArgs });
                    else
                        handlerCopy(this, nameChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the name changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving NameChangedCallback().");
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
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemovePresenceChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>):Void" Ring="2" />
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
        private object LockPresenceChangedEvent
        {
            get{
                if (m_lockPresenceChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_lockPresenceChangedEvent, o, null);
                }
                return m_lockPresenceChangedEvent;
            }
        }
        private RegisteredWaitHandle m_regPresenceChangedWaitHandle;
        private AutoResetEvent m_presenceChangedEvent;
        private SafeCollabEvent m_safePresenceChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: PresenceChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void AddPresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //

            lock (LockPresenceChangedEvent){
                if (m_presenceChanged == null){

                    m_presenceChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //
                    
                    m_regPresenceChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_presenceChangedEvent, //Event that triggers the callback
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

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemovePresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() called.");
            
            lock (LockPresenceChangedEvent){
                m_presenceChanged -= callback;
                if (m_presenceChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regPresenceChangedWaitHandle,
                                                                ref m_safePresenceChangedEvent,
                                                                ref m_presenceChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean PresenceChanged variables successful.");
                }
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void PresenceChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PresenceChangedCallback() called.");

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

                        PeerPresenceInfo peerPresenceInfo = null;
                        if (presenceData.pPresenceInfo != IntPtr.Zero){
                            PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(presenceData.pPresenceInfo, typeof(PEER_PRESENCE_INFO));
                            peerPresenceInfo = new PeerPresenceInfo();
                            peerPresenceInfo.PresenceStatus = ppi.status;
                            peerPresenceInfo.DescriptiveText = ppi.descText;
                        }

                        PeerContact peerContact = null;
                        PeerEndPoint peerEndPoint = null;

                        if (presenceData.pContact != IntPtr.Zero){
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(presenceData.pContact, typeof(PEER_CONTACT));
                            peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                        }

                        if (presenceData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(presenceData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                        }

                        presenceChangedArgs = new PresenceChangedEventArgs(peerEndPoint,
                                                                                        peerContact,
                                                                                        presenceData.changeType,
                                                                                        peerPresenceInfo);
                    }
                }
                finally{
                    if(eventData != null) eventData.Dispose();
                }
                
                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<PresenceChangedEventArgs> handlerCopy = m_presenceChanged;

                if ((presenceChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, presenceChangedArgs });
                    else
                        handlerCopy(this, presenceChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the presence changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving PresenceChangedCallback().");
        }

        private event EventHandler<ApplicationChangedEventArgs> m_applicationChanged;
        public event EventHandler<ApplicationChangedEventArgs> ApplicationChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: AddApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                AddApplicationChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: RemoveApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove{
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
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
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

            while (true)
            {
                ApplicationChangedEventArgs appChangedArgs = null;
                
                //
                // Get the event data for the fired event
                //
                try
                {
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
                        PEER_APPLICATION pa = (PEER_APPLICATION)Marshal.PtrToStructure(appData.pApplication, typeof(PEER_APPLICATION));

                        PeerApplication peerApplication = CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(pa); ;

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
                finally{
                    if(eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<ApplicationChangedEventArgs> handlerCopy = m_applicationChanged;

                if ((appChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, appChangedArgs });
                    else
                        handlerCopy(this, appChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the application changed event callback.");
                }
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
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
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

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChangedEvent() called.");

            lock (LockObjChangedEvent){
                if (m_objectChanged == null){

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

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void RemoveObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() called.");
            
            lock (LockObjChangedEvent){
                m_objectChanged -= callback;
                if (m_objectChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref m_regObjChangedWaitHandle,
                                                                ref m_safeObjChangedEvent,
                                                                ref m_objChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChanged variables successful.");
                }
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
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
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ObjectChangedCallback() called.");

            while (true)
            {
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
                finally{
                    if(eventData != null) eventData.Dispose();
                }
 
                //
                // Fire the callback with the marshalled event args data
                //

                EventHandler<ObjectChangedEventArgs> handlerCopy = m_objectChanged;

                if ((objectChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { this, objectChangedArgs });
                    else
                        handlerCopy(this, objectChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the object changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ObjectChangedCallback().");
        }

        //
        // Grabs all the contacts from the users windows address book
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumContacts(System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <UsesUnsafeCode Name="Local pContacts of type: IntPtr*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local contactArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerContactCollection GetContacts()
        {
            PeerContactCollection peerContactColl = new PeerContactCollection();
            SafeCollabEnum handlePeerEnum = null;
            UInt32 contactCount = 0;
            int errorCode = 0;

            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering GetContacts()");

            try{
                //
                // Get contacts array from native
                //
                
                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumContacts(out handlePeerEnum);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumContacts returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetContactsFailed), errorCode);
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref contactCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetContactsFailed), errorCode);
                }

                if (contactCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "No contacts found. \nLeaving GetContacts()");
                    return peerContactColl;
                }
                unsafe{
                    SafeCollabData contactArray = null;
                    try{
                        errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref contactCount, out contactArray);
                        if (errorCode != 0){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetNextItem returned with errorcode {0}", errorCode);
                            throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetContactsFailed), errorCode);
                        }

                        //
                        // Loop through the contacts array to build PeerContact collection
                        //

                        IntPtr pPEER_CONTACT = contactArray.DangerousGetHandle();
                        IntPtr* pContacts = (IntPtr*)pPEER_CONTACT;
                        for (ulong i = 0; i < contactCount; i++){
                            IntPtr pContactPtr = (IntPtr)pContacts[i];
                            PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(pContactPtr, typeof(PEER_CONTACT));

                            PeerContact peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                            peerContactColl.Add(peerContact);
                        }
                    }
                    finally{
                        contactArray.Dispose();
                    }
                }
            }
            finally{
                handlePeerEnum.Dispose();
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, 
                "Returning collections with {0} contacts. \nLeaving GetContacts()");

            return peerContactColl;
        }

        //
        // Gets specific contact from the users windows address book
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local safeContact of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerContact GetContact(PeerName peerName)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            if (peerName == null)
                throw new ArgumentNullException("peerName");

            int errorCode = 0;
            SafeCollabData safeContact = null;
            PeerContact peerContact = null;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, 
                "Entering GetContact() with peername {0}", peerName.ToString());

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabGetContact(peerName.ToString(),
                                                                        out safeContact);
                if (errorCode == UnsafeCollabReturnCodes.PEER_E_CONTACT_NOT_FOUND){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "Contact not found in Contact Manager");
                    throw new PeerToPeerException(SR.GetString(SR.Collab_ContactNotFound));
                }
                else if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetContact returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetContactFailed), errorCode);
                }

                if (!safeContact.DangerousGetHandle().Equals(IntPtr.Zero)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Found contact.");
                    PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(safeContact.DangerousGetHandle(), typeof(PEER_CONTACT));
                    peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                }
                else{
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "No contact found.");
                }
            }
            finally{
                safeContact.Dispose();
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving GetContact()");

            return peerContact;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabQueryContactData(System.IntPtr,System.String&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabParseContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <ReferencesCritical Name="Local contact of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerContact CreateContact(PeerNearMe peerNearMe) 
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerNearMe == null)
                throw new ArgumentNullException("peerNearMe");

            if ((peerNearMe.PeerEndPoints == null) || (peerNearMe.PeerEndPoints.Count == 0) || (peerNearMe.PeerEndPoints[0].EndPoint == null))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering CreateContact() with peernearme", peerNearMe.ToString());

            PeerEndPoint peerEndPoint = peerNearMe.PeerEndPoints[0];

            PEER_ENDPOINT pe = new PEER_ENDPOINT();
            pe.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin all the data to pass to native
            //
            GCHandle pepName = new GCHandle();

            if (peerEndPoint.Name != null){
                pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
                pe.pwzEndpointName = pepName.AddrOfPinnedObject();
            }
            
            GCHandle peerEP = GCHandle.Alloc(pe, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            string contactData = null;
            int errorCode = 0;
            
            //
            // Refresh end point data if it not subscribed
            //
            peerNearMe.RefreshData();

            errorCode = UnsafeCollabNativeMethods.PeerCollabQueryContactData(ptrPeerEP, ref contactData);
            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetContact returned with errorcode {0}", errorCode);
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_CreateContactFailed), errorCode);
            }

            SafeCollabData contact = null;
            PeerContact peerContact = null;

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabParseContact(contactData, out contact);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetContact returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_CreateContactFailed), errorCode);
                }

                PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(contact.DangerousGetHandle(), typeof(PEER_CONTACT));
                peerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                
                //
                // Mark it as just created and add the xml. This is used when adding the contact or getting
                // contact xml when contact is not added
                //
                peerContact.JustCreated = true;
                peerContact.ContactXml = contactData;
            }
            finally{
                if (contact != null) contact.Dispose();
                pepName.Free();
                peerEP.Free();
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving CreateContact().");

            return peerContact;
        }

        SendOrPostCallback OnCreateContactCompletedDelegate;
        object m_createContactAsyncListLock;
        object CreateContactAsyncListLock
        {
            get{
                if (m_createContactAsyncListLock == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref m_createContactAsyncListLock, o, null);
                }
                return m_createContactAsyncListLock;
            }
        }
        Dictionary<object, AsyncOperation> m_createContactAsyncList = new Dictionary<object, AsyncOperation>();

        private event EventHandler<CreateContactCompletedEventArgs> m_createContactCompleted;
        public event EventHandler<CreateContactCompletedEventArgs> CreateContactCompleted
        {
            add
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                m_createContactCompleted += value;
            }
            remove
            {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                m_createContactCompleted -= value;
            }
        }


        //
        // Stores state to pass to the create contact async helper
        //
        private class CreateContactAsyncState
        {
            PeerNearMe m_peerNearMe;
            Object m_userToken;
            internal CreateContactAsyncState(PeerNearMe peerNearMe, Object userToken){
                m_peerNearMe = peerNearMe;
                m_userToken = userToken;
            }
            internal PeerNearMe PeerNearMe
            {
                get { return m_peerNearMe; }
            }
            internal Object UserToken
            {
                get { return m_userToken; }
            }
        }

        public void CreateContactAsync(PeerNearMe peerNearMe, Object userToken)
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerNearMe == null)
                throw new ArgumentNullException("PeerNearMe");

            if ((peerNearMe.PeerEndPoints == null) || (peerNearMe.PeerEndPoints.Count == 0) || (peerNearMe.PeerEndPoints[0].EndPoint == null))
                throw new PeerToPeerException(SR.GetString(SR.Collab_NoEndpointFound));

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering CreateContactAsync() with token {0} and following peernearme" , userToken);
                peerNearMe.TracePeerNearMe();
            }

            //
            // Add to list of usertokens 
            //
            lock (CreateContactAsyncListLock){
                if (m_createContactAsyncList.ContainsKey(userToken)){
                    throw new ArgumentException(SR.GetString(SR.DuplicateUserToken));
                }
                AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
                m_createContactAsyncList[userToken] = asyncOp;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(CreateContactAsyncHelper), new CreateContactAsyncState(peerNearMe, userToken));

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving CreateContactAsync().");

        }

        //
        // Used to create a contact async'ally
        //
        private void CreateContactAsyncHelper(object state)
        {
            Exception ex = null;
            PeerContact peerContact = null;
            CreateContactAsyncState createAsyncState = state as CreateContactAsyncState;
            PeerNearMe peerNearMe = createAsyncState.PeerNearMe;
            object userToken = createAsyncState.UserToken;

            //
            // Call the sync version of createcontact
            //
            try{
                peerContact = CreateContact(peerNearMe);
            }
            catch (ObjectDisposedException e){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "CreateContactAsyncHelper caught error {0}", e.Message);
                ex = e;
            }
            catch (PeerToPeerException e){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "CreateContactAsyncHelper caught error {0}", e.Message);
                ex = e;
            }

            CreateContactCompletedEventArgs createContactCompletedArgs;
            if (ex == null){
                createContactCompletedArgs = new CreateContactCompletedEventArgs(peerContact, null, false, userToken);
            }
            else{
                createContactCompletedArgs = new CreateContactCompletedEventArgs(peerContact, ex, false, userToken);
            }

            PrepareToRaiseCreateContactCompletedEvent(m_createContactAsyncList[userToken], createContactCompletedArgs);
        }

        void OnCreateContactCompleted(CreateContactCompletedEventArgs e)
        {
            EventHandler<CreateContactCompletedEventArgs> handlerCopy = m_createContactCompleted;
            if (handlerCopy != null){
                handlerCopy(this, e);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the create contact completed event callback.");
            }
        }

        void CreateContactCompletedWaitCallback(object operationState)
        {
            CreateContactCompletedEventArgs args = (CreateContactCompletedEventArgs) operationState;
            //
            // Remove from usertoken list
            //
            m_createContactAsyncList.Remove(args.UserState);

            OnCreateContactCompleted(args);
        }

        internal void PrepareToRaiseCreateContactCompletedEvent(AsyncOperation asyncOP, CreateContactCompletedEventArgs args)
        {
            asyncOP.PostOperationCompleted(OnCreateContactCompletedDelegate, args);
        }

        //
        // Adds a contacts to the users windows address book
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabAddContact(System.String,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local contact of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void AddContact(PeerContact peerContact) 
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerContact == null)
                throw new ArgumentNullException("peerContact");

            string contactXml = null;

            if (peerContact.ContactXml == null){
                try{
                    contactXml = peerContact.ToXml();
                }
                catch (PeerToPeerException e){ 
                    throw new PeerToPeerException(SR.GetString(SR.Collab_AddContactFailedNoXml), e.InnerException);
                }
            }
            else{
                contactXml = peerContact.ContactXml;
            }

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddContact() with following peercontact");
                peerContact.TracePeerContact();
            }

            SafeCollabData contact = null;
            int errorCode = 0;

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabAddContact(contactXml, out contact);
            }
            finally{
                if(contact != null) contact.Dispose();
            }

            if (errorCode == UnsafeCollabReturnCodes.PEER_E_ALREADY_EXISTS){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabAddContact returned with errorcode {0}. Contact already exists.", errorCode);
                throw new ArgumentException(SR.GetString(SR.Collab_AddContactFailed) + " " + SR.GetString(SR.Collab_ContactExists));
            }
            else if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabAddContact returned with errorcode {0}", errorCode);
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_AddContactFailed), errorCode);
            }

            peerContact.JustCreated = false;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                                    "Leaving AddContact() successfully.");
            
        }

        //
        // Deletes a contact from the users windows address book
        //
        public void DeleteContact(PeerContact peerContact) 
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                            "Entering DeleteContact().");

            if (peerContact == null)
                throw new ArgumentNullException("peerContact");

            DeleteContact(peerContact.PeerName);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                            "Leaving DeleteContact() successfully.");

        }

        //
        // Deletes a contact from the users windows address book
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabDeleteContact(System.String):System.Int32" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void DeleteContact(PeerName peerName)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                        "Entering DeleteContact().");

            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            if (peerName == null)
                throw new ArgumentNullException("peerName");

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                        "Peername is {0}", peerName.ToString());

            int errorCode = UnsafeCollabNativeMethods.PeerCollabDeleteContact(peerName.ToString());

            if (errorCode == UnsafeCollabReturnCodes.PEER_E_CONTACT_NOT_FOUND){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "Contact not found in Contact Manager");
                throw new ArgumentException(SR.GetString(SR.Collab_ContactNotFound), "peerName");
            } 
            else if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabDeleteContact returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_DeleteContactFailed), errorCode));
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                        "Leaving DeleteContact() successfully.");
        }

        //
        // Updates a contact from the users windows address book
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabUpdateContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local safeCredentials of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(System.Net.PeerToPeer.Collaboration.PeerContact,System.Net.PeerToPeer.Collaboration.SafeCollabMemory&):System.Net.PeerToPeer.Collaboration.PEER_CONTACT" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void UpdateContact(PeerContact peerContact) 
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            if (peerContact == null)
                throw new ArgumentNullException("peerContact");

            if (peerContact.PeerName == null)
                throw new ArgumentException(SR.GetString(SR.Collab_NoPeerNameInContact));

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering UpdateContact() with following peercontact");
                peerContact.TracePeerContact();
            }

            SafeCollabMemory safeCredentials = null;
            int errorCode = 0;

            try{
                PEER_CONTACT pc = CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(peerContact, ref safeCredentials);

                errorCode = UnsafeCollabNativeMethods.PeerCollabUpdateContact(ref pc);
            }
            finally{
                if (safeCredentials != null) safeCredentials.Dispose();
            }

            if (errorCode == UnsafeCollabReturnCodes.PEER_E_CONTACT_NOT_FOUND){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "Contact not found in Contact Manager");
                throw new ArgumentException(SR.GetString(SR.Collab_ContactNotFound), "peerContact");
            }
            else if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabUpdateContact returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_UpdateContactFailed), errorCode));
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                                        "Leaving UpdateContact() successfully.");
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
        // <ReferencesCritical Name="Field: m_safeSubLstChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Field: m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Field: m_safeObjChangedEvent" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        void Dispose(bool disposing)
        {
            if (!m_Disposed){
                CollaborationHelperFunctions.CleanEventVars(ref m_regSubLstChangedWaitHandle,
                                                            ref m_safeSubLstChangedEvent,
                                                            ref m_subLstChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean SubscriptionListChanged variables successful.");
                
                CollaborationHelperFunctions.CleanEventVars(ref m_regNameChangedWaitHandle,
                                                            ref m_safeNameChangedEvent,
                                                            ref m_nameChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean NameChanged variables successful.");
                
                CollaborationHelperFunctions.CleanEventVars(ref m_regPresenceChangedWaitHandle,
                                                            ref m_safePresenceChangedEvent,
                                                            ref m_presenceChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean PresenceChanged variables successful.");

                CollaborationHelperFunctions.CleanEventVars(ref m_regAppChangedWaitHandle,
                                                            ref m_safeAppChangedEvent,
                                                            ref m_appChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ApplicationChanged variables successful.");

                CollaborationHelperFunctions.CleanEventVars(ref m_regObjChangedWaitHandle,
                                                            ref m_safeObjChangedEvent,
                                                            ref m_objChangedEvent);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChanged variables successful.");

                m_Disposed = true;
            }
        }
    }
}
