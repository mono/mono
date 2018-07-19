//------------------------------------------------------------------------------
// <copyright file="MyContact.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Net.Mail;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class handles events specific to my contact in windows collaboration and
    /// also acts as a conventional peer contact.
    /// </summary>
    [Serializable]
    internal class MyContact : PeerContact, ISerializable 
    {
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: PeerContact..ctor()" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal MyContact() { }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [System.Security.SecurityCritical]
        internal MyContact(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }

        internal override bool InternalIsSubscribed()
        {
            return true;
        }

        internal override SubscriptionType InternalSubscribeAllowedGet()
        {
            return SubscriptionType.Allowed;
        }

        internal override void InternalSubscribeAllowedSet(SubscriptionType value)
        {
            throw new PeerToPeerException(SR.GetString(SR.Collab_SubscribeLocalContactFailed));
        }
       
        //
        // Event to handle presence changed for my contact
        //
        private event EventHandler<PresenceChangedEventArgs> m_presenceChanged;

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: PeerContact.PresenceChangedCallback(System.Object,System.Boolean):System.Void" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void AddPresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockPresenceChangedEvent){
                if (m_presenceChanged == null){

                    PresenceChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    PresenceChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(PresenceChangedEvent, //Event that triggers the callback
                                                                                        new WaitOrTimerCallback(PresenceChangedCallback), //callback to be called 
                                                                                        null, //state to be passed
                                                                                        -1,   //Timeout - aplicable only for timers
                                                                                        false //call us everytime the event is set
                                                                                        );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.MyPresenceChanged;
                    pcer.pInstance = IntPtr.Zero;


                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        PresenceChangedEvent.SafeWaitHandle,
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

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddMyPresenceChanged() successful.");

        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: PeerContact.CleanContactPresenceEventVars():System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void RemovePresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
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

        protected override void OnPresenceChanged(PresenceChangedEventArgs presenceChangedArgs)
        {
            EventHandler<PresenceChangedEventArgs> handlerCopy = m_presenceChanged;

            if (handlerCopy != null){
                handlerCopy(this, presenceChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the presence changed event callback.");
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void PresenceChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PresenceChangedCallback() called.");

            if (m_Disposed) return;

            while (true)
            {
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
                    if (ped.eventType == PeerCollabEventType.MyPresenceChanged){
                        PEER_EVENT_PRESENCE_CHANGED_DATA presenceData = ped.presenceChangedData;

                        PeerPresenceInfo peerPresenceInfo = null;
                        if (presenceData.pPresenceInfo != IntPtr.Zero)
                        {
                            PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(presenceData.pPresenceInfo, typeof(PEER_PRESENCE_INFO));
                            peerPresenceInfo = new PeerPresenceInfo();
                            peerPresenceInfo.PresenceStatus = ppi.status;
                            peerPresenceInfo.DescriptiveText = ppi.descText;
                        }

                        PeerEndPoint peerEndPoint = null;
                        if (presenceData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(presenceData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                        }

                        presenceChangedArgs = new PresenceChangedEventArgs(peerEndPoint,
                                                                                        null,
                                                                                        presenceData.changeType,
                                                                                        peerPresenceInfo);
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //
                if(presenceChangedArgs!= null)
                    OnPresenceChanged(presenceChangedArgs);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving PresenceChangedCallback().");
        }

        //
        // Event to handle application changed for my contact
        //
        private event EventHandler<ApplicationChangedEventArgs> m_applicationChanged;
        
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: PeerContact.ApplicationChangedCallback(System.Object,System.Boolean):System.Void" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void AddApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockAppChangedEvent){
                if (m_applicationChanged == null){

                    AppChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    AppChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(AppChangedEvent, //Event that triggers the callback
                                                                                new WaitOrTimerCallback(ApplicationChangedCallback), //callback to be called 
                                                                                null, //state to be passed
                                                                                -1,   //Timeout - aplicable only for timers
                                                                                false //call us everytime the event is set
                                                                                );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();

                    pcer.eventType = PeerCollabEventType.MyApplicationChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        AppChangedEvent.SafeWaitHandle,
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
        // <ReferencesCritical Name="Method: PeerContact.CleanContactObjEventVars():System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void RemoveApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() called.");
            lock (LockAppChangedEvent){
                m_applicationChanged -= callback;
                if (m_applicationChanged == null){
                    CleanContactObjEventVars();
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() successful.");
        }

        protected override void OnApplicationChanged(ApplicationChangedEventArgs appChangedArgs)
        {
            EventHandler<ApplicationChangedEventArgs> handlerCopy = m_applicationChanged;

            if (handlerCopy != null){
                handlerCopy(this, appChangedArgs);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the application changed event callback.");
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(System.Net.PeerToPeer.Collaboration.PEER_APPLICATION):System.Net.PeerToPeer.Collaboration.PeerApplication" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void ApplicationChangedCallback(object state, bool timedOut)
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
                    if (ped.eventType == PeerCollabEventType.MyApplicationChanged){
                        PEER_EVENT_APPLICATION_CHANGED_DATA appData = ped.applicationChangedData;

                        PEER_APPLICATION pa = (PEER_APPLICATION)Marshal.PtrToStructure(appData.pApplication, typeof(PEER_APPLICATION));

                        PeerApplication peerApplication = CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(pa); ;

                        PeerEndPoint peerEndPoint = null;
                        if (appData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(appData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                        }

                        appChangedArgs = new ApplicationChangedEventArgs(peerEndPoint,
                                                                                                null,
                                                                                                appData.changeType,
                                                                                                peerApplication);
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

        //
        // Event to handle object changed for my contact
        //
        private event EventHandler<ObjectChangedEventArgs> m_objectChanged;
        
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <ReferencesCritical Name="Method: PeerContact.ObjectChangedCallback(System.Object,System.Boolean):System.Void" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void AddObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockObjChangedEvent)
            {
                if (m_objectChanged == null)
                {

                    ObjChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    ObjChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(ObjChangedEvent, //Event that triggers the callback
                                                                                new WaitOrTimerCallback(ObjectChangedCallback), //callback to be called 
                                                                                null, //state to be passed
                                                                                -1,   //Timeout - aplicable only for timers
                                                                                false //call us everytime the event is set
                                                                                );

                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();
                    pcer.eventType = PeerCollabEventType.MyObjectChanged;
                    pcer.pInstance = IntPtr.Zero;


                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                        ObjChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out m_safeObjChangedEvent);
                    if (errorCode != 0)
                    {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ObjectChangedRegFailed), errorCode);
                    }
                }
                m_objectChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChanged() successful.");

        }

        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: PeerContact.CleanContactObjEventVars():System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void RemoveObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
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

        protected override void OnObjectChanged(ObjectChangedEventArgs objChangedArgs)
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
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: PeerContact.m_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(System.Net.PeerToPeer.Collaboration.PEER_OBJECT):System.Net.PeerToPeer.Collaboration.PeerObject" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal override void ObjectChangedCallback(object state, bool timedOut)
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
                    if (ped.eventType == PeerCollabEventType.MyObjectChanged){
                        PEER_EVENT_OBJECT_CHANGED_DATA objData = ped.objectChangedData;

                        PEER_OBJECT po = (PEER_OBJECT)Marshal.PtrToStructure(objData.pObject, typeof(PEER_OBJECT));

                        PeerObject peerObject = CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(po); ;

                        PeerEndPoint peerEndPoint = null;
                        if (objData.pEndPoint != IntPtr.Zero){
                            PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(objData.pEndPoint, typeof(PEER_ENDPOINT));
                            peerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);
                        }

                        objChangedArgs = new ObjectChangedEventArgs(peerEndPoint,
                                                                                            null,
                                                                                            objData.changeType,
                                                                                            peerObject);
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

        //
        // My Contact is always subscribed. So this is no op
        //
        public override void Subscribe()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
        }

        public override void SubscribeAsync(object userToken)
        {
            //
            // My Contact is always subscribed. So this is no op. Just call the callback
            //
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            if (userToken == null)
                throw new ArgumentNullException("userToken");

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering SubscribeAsync() with user token {0}.", userToken);

            lock (AsyncLock){
                if (AsyncOp != null)
                    throw new PeerToPeerException(SR.GetString(SR.Collab_DuplicateSubscribeAsync));

                AsyncOp = AsyncOperationManager.CreateOperation(userToken);
            }

            this.PrepareToRaiseSubscribeCompletedEvent(AsyncOp, new SubscribeCompletedEventArgs(null, this, null, false, userToken));

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving SubscribeAsync().");
        }

        //
        // Cannot unsubscribe for the MyContact, so we always throw
        //
        public override void Unsubscribe()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            throw new PeerToPeerException(SR.GetString(SR.Collab_UnsubscribeLocalContactFail));
        }

        private bool m_Disposed;

        // <SecurityKernel Critical="True" Ring="3">
        // <ReferencesCritical Name="Method: PeerContact.Dispose(System.Boolean):System.Void" Ring="3" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
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

    }
}
