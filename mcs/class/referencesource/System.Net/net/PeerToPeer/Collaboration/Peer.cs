//------------------------------------------------------------------------------
// <copyright file="Peer.cs" company="Microsoft">
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
    using System.Net.Mail;
    using System.ComponentModel;
    using System.Threading;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.IO;

    /// <summary>
    /// This is the object changed event args class we give back when 
    /// we have have an object changed event fired by native
    /// </summary>
    public class ObjectChangedEventArgs : EventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        private PeerContact m_peerContact;
        private PeerChangeType m_peerChangeType;
        private PeerObject m_peerObject;

        internal ObjectChangedEventArgs(PeerEndPoint peerEndPoint, PeerContact peerContact,
                                        PeerChangeType peerChangeType, PeerObject peerObject)
        {
            m_peerEndPoint = peerEndPoint;
            m_peerContact = peerContact;
            m_peerChangeType = peerChangeType;
            m_peerObject = peerObject;
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

        public PeerObject PeerObject
        {
            get
            {
                return m_peerObject;
            }
        }
    }

    /// <summary>
    /// This is the presence changed event args class we give back when 
    /// we have have presence changed event fired by native
    /// </summary>
    public class PresenceChangedEventArgs : EventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        private PeerContact m_peerContact;
        private PeerChangeType m_peerChangeType;
        private PeerPresenceInfo m_peerPresenceInfo;

        internal PresenceChangedEventArgs(PeerEndPoint peerEndPoint, PeerContact peerContact,
                                        PeerChangeType peerChangeType, PeerPresenceInfo peerPresenceInfo)
        {
            m_peerEndPoint = peerEndPoint;
            m_peerContact = peerContact;
            m_peerChangeType = peerChangeType;
            m_peerPresenceInfo = peerPresenceInfo;
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

        public PeerPresenceInfo PeerPresenceInfo
        {
            get
            {
                return m_peerPresenceInfo;
            }
        }
    }

    /// <summary>
    /// This is the event args class we give back when 
    /// we have completed the subscribeasync call
    /// </summary>
    public class SubscribeCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PeerNearMe m_peerNearMe;
        private PeerContact m_peerContact;

        internal SubscribeCompletedEventArgs(PeerNearMe peerNearMe,
                                        PeerContact peerContact,
                                        Exception error,
                                        bool cancelled,
                                        object userToken)
            : base(error, cancelled, userToken)
        {
            m_peerNearMe = peerNearMe;
            m_peerContact = peerContact;
        }

        public PeerNearMe PeerNearMe
        {
            get
            {
                return m_peerNearMe;
            }
        }

        public PeerContact PeerContact
        {
            get
            {
                return m_peerContact;
            }
        }
    }

    /// <summary>
    /// This is the event args class we give back when 
    /// we have completed the refreshendpoint async call
    /// </summary>
    public class RefreshDataCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PeerEndPoint m_peerEndPoint;
        internal RefreshDataCompletedEventArgs(PeerEndPoint peerEndPoint,
                                                        Exception error,
                                                        bool cancelled,
                                                        object userToken)
            : base(error, cancelled, userToken)
        {
            m_peerEndPoint = peerEndPoint;
        }

        public PeerEndPoint PeerEndPoint
        {
            get
            {
                return m_peerEndPoint;
            }
        }
    }

    /// <summary>
    /// This is the event args class we give back when 
    /// we have completed the inviteasync call
    /// </summary>
    public class InviteCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PeerInvitationResponse m_peerInvResponse;
        internal InviteCompletedEventArgs(PeerInvitationResponse peerInvResponse,
                                                        Exception error,
                                                        bool cancelled,
                                                        object userToken)
            : base(error, cancelled, userToken)
        {
            m_peerInvResponse = peerInvResponse;
        }

        public PeerInvitationResponse InviteResponse
        {
            get
            {
                return m_peerInvResponse;
            }
        }
    }

    /// <summary>
    /// Has the common interface for PeerNearMe and PeerContact which derive from it
    /// </summary>
    [Serializable]
    public abstract class Peer : IDisposable, IEquatable<Peer>, ISerializable
    {
        private PeerEndPointCollection m_peerEndPoints = new PeerEndPointCollection();
        private ISynchronizeInvoke m_synchronizingObject;

        public virtual PeerEndPointCollection PeerEndPoints
        {
            get {
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                return m_peerEndPoints; 
            }
        }

        public bool IsOnline
        {
            get{
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Get Isonline called.");
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

                bool isOnline = false;
                PeerPresenceInfo presenceInfo;

                foreach (PeerEndPoint peerEndPoint in PeerEndPoints){
                    presenceInfo = null;

                    try{
                        presenceInfo = GetPresenceInfo(peerEndPoint);
                    }
                    catch (Exception e){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "Exception thrown {0}", e.Message);
                    }
                    if ((presenceInfo != null) && (presenceInfo.PresenceStatus == PeerPresenceStatus.Online)){
                        isOnline = true;
                        break;
                    }
                }
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving Isonline called with {0}.", isOnline);
                return isOnline;
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

        static internal Guid CurrentApplicationGuid
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <SatisfiesLinkDemand Name="Process.GetCurrentProcess():System.Diagnostics.Process" />
            // <SatisfiesLinkDemand Name="Process.get_ProcessName():System.String" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get{
                Guid guid = Guid.Empty;

                //
                // Get path and args of app
                //
                string path = Path.Combine( Environment.CurrentDirectory, Process.GetCurrentProcess().ProcessName + ".exe");
                string arguments = null;
                string[] argsArray = Environment.GetCommandLineArgs();
                int length = argsArray.Length;
                if (length > 1){
                    StringBuilder argsBuilder = new StringBuilder();
                    for (int i = 1; i < length; ++i){
                        argsBuilder.Append(argsArray[i]);
                        if (i != (length - 1)) argsBuilder.Append(' ');
                    }

                    arguments = argsBuilder.ToString();
                }

                //
                // Find a matching registered application and return its guid
                //
                PeerApplicationCollection peerApplications = PeerCollaboration.GetLocalRegisteredApplications();

                foreach (PeerApplication peerApplication in peerApplications){
                    if ((peerApplication.CommandLineArgs == arguments) &&
                        (peerApplication.Path == path))
                        return peerApplication.Id;
                }
                return guid;
            }
        }

        internal Peer(){
            OnInviteCompletedDelegate = new SendOrPostCallback(InviteCompletedWaitCallback);
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected Peer(SerializationInfo serializationInfo, StreamingContext streamingContext):this()
        {
            m_peerEndPoints = (PeerEndPointCollection)serializationInfo.GetValue("_PeerEndPoints", typeof(PeerEndPointCollection));
        }

        //
        // Gets the presence info from collab for a specific endpoint
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetPresenceInfo(System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local presenceInfo of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerPresenceInfo GetPresenceInfo(PeerEndPoint peerEndPoint)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "GetPresenceInfo()called.");
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerEndPoint == null)
                throw new ArgumentNullException("peerEndPoint");

            if (peerEndPoint.EndPoint == null)
                throw new ArgumentException(SR.GetString(SR.Collab_NoEndPointInPeerEndPoint));

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Getting presence for the following endpoint.");
                peerEndPoint.TracePeerEndPoint();
            }

            SafeCollabData presenceInfo = null;
            PeerPresenceInfo peerPresenceInfo = null;
            int errorCode;

            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin all the data to pass to native
            //
            GCHandle pepName = new GCHandle();

            if (peerEndPoint.Name != null){
                pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
                pep.pwzEndpointName = pepName.AddrOfPinnedObject();
            }
            
            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            //
            // Refresh data for getting presence info
            //
            RefreshIfNeeded();

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabGetPresenceInfo(ptrPeerEP, out presenceInfo);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetPresenceInfo returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetPresenceFailed), errorCode);
                }

                IntPtr ptrPeerPresenceInfo = presenceInfo.DangerousGetHandle();
                PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(ptrPeerPresenceInfo, typeof(PEER_PRESENCE_INFO));
                peerPresenceInfo = new PeerPresenceInfo();
                peerPresenceInfo.PresenceStatus = ppi.status;
                peerPresenceInfo.DescriptiveText = ppi.descText;
            }
            finally{
                if (pepName.IsAllocated) pepName.Free();
                if (peerEP.IsAllocated) peerEP.Free();
                if (presenceInfo != null) presenceInfo.Dispose();
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving GetPresenceInfo().");

            return peerPresenceInfo;
        }

        //
        // Gets all the objects for all the endpoints
        //
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InternalGetAllObjects(Guid, Boolean):PeerObjectCollection" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerObjectCollection GetObjects()
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            return InternalGetAllObjects(Guid.Empty, false);
        }

        //
        // Gets specific object for all the endpoints
        //
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InternalGetAllObjects(Guid, Boolean):PeerObjectCollection" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerObjectCollection GetObjects(Guid objectId)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            return InternalGetAllObjects(objectId, true);
        }

        internal abstract void RefreshIfNeeded();

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalGetObjects(Guid, Boolean, PeerEndPoint):PeerObjectCollection" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private PeerObjectCollection InternalGetAllObjects(Guid objectId, bool guidSupplied)
        {

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering InternalGetAllObjects() with ObjectId {0}.", objectId);

            Dictionary<Guid, PeerObject> mergedObjects = new Dictionary<Guid, PeerObject>();
            PeerObjectCollection peerObjectCollection;

            //
            // Refresh the data at the endpoint before calling get objs
            //
            RefreshIfNeeded();

            foreach (PeerEndPoint peerEndPoint in PeerEndPoints)
            {
                peerObjectCollection = InternalGetObjects(objectId, guidSupplied, peerEndPoint);

                //
                // Special case. If we have already found an endpoint with the user guid then
                // we just return it
                //
                if (guidSupplied && peerObjectCollection.Count != 0)
                    return peerObjectCollection;

                foreach (PeerObject peerObject in peerObjectCollection)
                {
                    mergedObjects[peerObject.Id] = peerObject;
                }
            }

            //
            // Return the object collection from the dictionary
            //

            Dictionary<Guid, PeerObject>.ValueCollection objects = mergedObjects.Values;
            peerObjectCollection = new PeerObjectCollection();
            foreach (PeerObject peerObject in objects)
            {
                peerObjectCollection.Add(peerObject);
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving InternalGetAllObjects(). " +
            "Returning collection with {0} objects.", peerObjectCollection.Count);

            return peerObjectCollection;
        }

        //
        // Gets specific objects for an endpoint
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pObjects of type: IntPtr*" />
        // <UsesUnsafeCode Name="Local pPeerObject of type: PEER_OBJECT*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumObjects(System.IntPtr,System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local objectArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerObjectCollection InternalGetObjects(Guid objectId, bool guidSupplied, PeerEndPoint peerEndPoint)
        {
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering InternalGetObjects() with the following PeerEndPoint");
                peerEndPoint.TracePeerEndPoint();
            }
            
            PeerObjectCollection peerObjectColl = new PeerObjectCollection();
            SafeCollabEnum handlePeerEnum = null;
            UInt32 objectCount = 0;
            int errorCode = 0;


            GCHandle guidHandle = new GCHandle();
            IntPtr guidPtr = IntPtr.Zero;

            if (guidSupplied){
                GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(objectId);
                guidHandle = GCHandle.Alloc(guid, GCHandleType.Pinned);
                guidPtr = guidHandle.AddrOfPinnedObject();
            }

            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin data to pass to native
            //

            GCHandle pepName = new GCHandle();

            if (peerEndPoint.Name != null){
                pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
                pep.pwzEndpointName = pepName.AddrOfPinnedObject();
            }
            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            try{
                //
                // Enumerate through the objects for the endpoint
                //

                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumObjects(ptrPeerEP, guidPtr, out handlePeerEnum);

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumObjects returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetObjectsFailed), errorCode);
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref objectCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetObjectsFailed), errorCode);
                }

                if (objectCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "No PeerObjects found.");
                    return peerObjectColl;
                }
                unsafe
                {
                    SafeCollabData objectArray;
                    errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref objectCount, out objectArray);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetNextItem returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetObjectsFailed), errorCode);
                    }

                    IntPtr pPEER_OBJECT = objectArray.DangerousGetHandle();
                    IntPtr* pObjects = (IntPtr*)pPEER_OBJECT;

                    //
                    // Loop through the applications array from native
                    //
                    for (ulong i = 0; i < objectCount; i++){
                        PEER_OBJECT* pPeerObject = (PEER_OBJECT*)pObjects[i];
                        byte[] data = null;

                        if (pPeerObject->data.cbData != 0){
                            data = new byte[pPeerObject->data.cbData];
                            Marshal.Copy(pPeerObject->data.pbData, data, 0, (int)pPeerObject->data.cbData);
                        }

                        PeerObject peerObject = new PeerObject(CollaborationHelperFunctions.ConvertGUIDToGuid(pPeerObject->guid), data, (PeerScope)pPeerObject->dwPublicationScope);
                        
                        if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Retrieved following Object");
                            peerObject.TracePeerObject();
                        }
                        
                        peerObjectColl.Add(peerObject);
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
            "Returning collection with {0} objects.", peerObjectColl.Count);
            
            return peerObjectColl;
        }

        public abstract PeerInvitationResponse Invite();

        public abstract PeerInvitationResponse Invite(PeerApplication applicationToInvite, string message, byte[] invitationData);

        //
        // Invites an endpoint with passed data. Includes a contact if it was passed in.
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.Byte[],System.Int32,System.IntPtr,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabInviteContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT&,System.IntPtr,System.Net.PeerToPeer.Collaboration.PEER_INVITATION&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabInviteEndpoint(System.IntPtr,System.Net.PeerToPeer.Collaboration.PEER_INVITATION&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local data of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Local safeResponse of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Local safeCredentials of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabMemory..ctor(System.Int32)" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(System.Net.PeerToPeer.Collaboration.PeerContact,System.Net.PeerToPeer.Collaboration.SafeCollabMemory&):System.Net.PeerToPeer.Collaboration.PEER_CONTACT" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerInvitationResponse InternalInviteEndPoint(Guid applicationToInviteGuid,
                                                                string message, byte[] invitationData,
                                                                PeerEndPoint peerEndPoint, PeerContact peerContact)
        {

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering InternalInviteEndPoint() with the following information.");
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Invitation Message: ", message);
                if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose) && (invitationData != null)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tInvitation data:");
                    Logging.DumpData(Logging.P2PTraceSource, TraceEventType.Verbose, Logging.P2PTraceSource.MaxDataSize, invitationData, 0, invitationData.Length);
                }
                else
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Invitation Data length: ", (invitationData != null ? invitationData.Length : 0));

                if (peerEndPoint != null) peerEndPoint.TracePeerEndPoint();
            }

            PEER_INVITATION pi = new PEER_INVITATION();
            pi.applicationId = CollaborationHelperFunctions.ConvertGuidToGUID(applicationToInviteGuid);
            pi.pwzMessage = message;

            SafeCollabMemory data = null;
            pi.applicationData.cbData = (invitationData != null) ? (UInt32)invitationData.Length : 0;

            //
            // Marshal Invitation Data
            //
            if ((invitationData != null) && (invitationData.Length > 0))
            {
                data = new SafeCollabMemory(invitationData.Length);
                pi.applicationData.pbData = data.DangerousGetHandle();
                Marshal.Copy(invitationData, 0, pi.applicationData.pbData, invitationData.Length);
            }
            else
                pi.applicationData.pbData = IntPtr.Zero;


            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin data to pass to native
            //
            GCHandle pepName = new GCHandle();

            if (peerEndPoint.Name != null){
                pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
                pep.pwzEndpointName = pepName.AddrOfPinnedObject();
            }
            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            SafeCollabData safeResponse = null;
            PeerInvitationResponse peerInvResponse = null; 
            int errorCode;

            try{
                //
                // Make native call with endpoint with/without contact
                //
                if (peerContact != null){

                    //
                    // Generate native contact
                    //
                    SafeCollabMemory safeCredentials = null;
                    PEER_CONTACT pc = CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(peerContact, ref safeCredentials);

                    try{
                        errorCode = UnsafeCollabNativeMethods.PeerCollabInviteContact(ref pc,
                                                                                        ptrPeerEP,
                                                                                        ref pi,
                                                                                        out safeResponse);
                    }
                    finally{
                        if (safeCredentials != null) safeCredentials.Dispose();
                    }
                }
                else
                    errorCode = UnsafeCollabNativeMethods.PeerCollabInviteEndpoint(ptrPeerEP, ref pi, out safeResponse);

                if (errorCode != 0){
                    if ((errorCode == UnsafeCollabReturnCodes.PEER_E_TIMEOUT) || (errorCode == UnsafeCollabReturnCodes.ERROR_TIMEOUT)){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                            "Timed out. Leaving InternalInviteEndPoint() with InvitationResponseType expired.");
                        
                        return new PeerInvitationResponse(PeerInvitationResponseType.Expired);
                    }

                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, ((peerContact != null) ? "PeerCollabInviteContact" : "PeerCollabInviteEndpoint")
                        + " returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_InviteFailed), errorCode);
                }

                if (!safeResponse.IsInvalid){
                    PEER_INVITATION_RESPONSE pir = (PEER_INVITATION_RESPONSE)Marshal.PtrToStructure(safeResponse.DangerousGetHandle(),
                                                                                                    typeof(PEER_INVITATION_RESPONSE));
                    peerInvResponse = new PeerInvitationResponse(pir.action);
                }
            }
            finally{
                if (safeResponse != null) safeResponse.Dispose();
                if (pepName.IsAllocated) pepName.Free();
                if (peerEP.IsAllocated) peerEP.Free();
            }


            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0,
                "Leaving InternalInviteEndPoint() with InvitationResponse {0}.", peerInvResponse);

            return peerInvResponse;
        }


        private event EventHandler<InviteCompletedEventArgs> m_inviteCompleted;
        public event EventHandler<InviteCompletedEventArgs> InviteCompleted
        {
            add{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                m_inviteCompleted += value;
            }
            remove{
                if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                m_inviteCompleted -= value;
            }
        }

        public abstract void InviteAsync(Object userToken);

        public abstract void InviteAsync(   PeerApplication applicationToInvite, string message,
                                            byte[] invitationData, Object userToken);


        #region Invite Async variables
        SendOrPostCallback OnInviteCompletedDelegate;

        internal Dictionary<object, InviteAsyncHelper> m_inviteAsyncHelperList = new Dictionary<object, InviteAsyncHelper>();
        #endregion

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InviteAsyncHelper.InviteAsync():System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void InternalInviteAsync(Guid applicationToInviteGuid,
                                        string message, byte[] invitationData,
                                        PeerEndPointCollection peerEndPoints, PeerContact peerContact,
                                        Object userToken)
        {
            InviteAsyncHelper inviteAsyncHelper = null;

            // 
            //The userToken can't be duplicate of what is in the 
            //current list. These are the requriments for the new Async model 
            //that supports multiple outstanding async calls
            // 
            int newTraceEventId = NewTraceEventId;

            lock (m_inviteAsyncHelperList){
                if (m_inviteAsyncHelperList.ContainsKey(userToken)){
                    throw new ArgumentException(SR.GetString(SR.DuplicateUserToken));
                }

                inviteAsyncHelper = new InviteAsyncHelper(  peerContact, this, peerEndPoints, 
                                                            applicationToInviteGuid, message,
                                                            invitationData, userToken, newTraceEventId);
                m_inviteAsyncHelperList[userToken] = inviteAsyncHelper;
            }

            try{
                //
                //Start resolution on that resolver
                //
                inviteAsyncHelper.InviteAsync();
            }
            catch{
                //
                //If an exception happens clear the userState from the 
                //list so that that token can be reused
                //
                lock (m_inviteAsyncHelperList){
                    m_inviteAsyncHelperList.Remove(userToken);
                }
                throw;
            }

        }
        
        protected virtual void OnInviteCompleted(InviteCompletedEventArgs e)
        {
            EventHandler<InviteCompletedEventArgs> handlerCopy = m_inviteCompleted;

            if (handlerCopy != null){
                handlerCopy(this, e);
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the invite completed event callback.");
            }
        }

        void InviteCompletedWaitCallback(object operationState)
        {
            OnInviteCompleted((InviteCompletedEventArgs)operationState);
        }

        internal void PrepareToRaiseInviteCompletedEvent(AsyncOperation asyncOP, InviteCompletedEventArgs args)
        {
            lock (m_inviteAsyncHelperList){
                InviteAsyncHelper helper = m_inviteAsyncHelperList[args.UserState];
                if (helper == null){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Critical, 0, "userState for which we are about to call Completed event does not exist in the pending async list");
                }else{
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Critical, helper.TraceEventId,
                         "userState {0} is being removed from the pending async list", args.UserState.GetHashCode());
                    m_inviteAsyncHelperList.Remove(args.UserState);
                }
            }
            asyncOP.PostOperationCompleted(OnInviteCompletedDelegate, args);
        }
        
        // <SecurityKernel Critical="True" Ring="2">
        // <ReferencesCritical Name="Method: InviteAsyncHelper.CancelAsync(System.Object):System.Void" Ring="2" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void InviteAsyncCancel(Object userToken)
        {
            if (userToken == null)
                throw new ArgumentNullException("userToken");

            InviteAsyncHelper helper;
            lock (m_inviteAsyncHelperList){
                if (!m_inviteAsyncHelperList.TryGetValue(userToken, out helper)){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0, "InviteAsyncCancel called with a userState token that is not in the pending async list - returning");
                    return;
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, helper.TraceEventId,
                    "Proceeding to cancel the pending async");
            helper.CancelAsync(userToken);
        }

        //
        // Used to track inviteasynchelpers
        //
        private static int s_TraceEventId = 1;
        internal static int NewTraceEventId
        {
            get{
                Interlocked.CompareExchange(ref s_TraceEventId, 0, int.MaxValue);
                Interlocked.Increment(ref s_TraceEventId);
                return s_TraceEventId;
            }
        }        

        public bool Equals(Peer other)
        {
            if (other != null){
                if (other.PeerEndPoints != null){
                    return other.PeerEndPoints.Equals(PeerEndPoints);
                }
            }
            return false;
        }

        public override string ToString()
        {
            return PeerEndPoints.ToString();
        }

        private bool m_Disposed;

        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);
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
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_PeerEndPoints", PeerEndPoints);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed){
                m_Disposed = true;
            }
        }
    }

    /// <summary>
    /// Helps in all the async invites sent from Peer, PeerContact and PeerNearMe
    /// </summary>
    internal class InviteAsyncHelper : IDisposable
    {
        internal object m_userState;
        internal SafeCollabInvite m_SafeCollabInvite;
        internal AutoResetEvent m_InviteEvent = new AutoResetEvent(false);

        //
        //The WaitHandle that hooks up a callback to the 
        //event
        //
        internal RegisteredWaitHandle m_RegisteredWaitHandle;

        //
        //Disposed or not
        //
        internal bool m_Disposed;

        internal bool m_Cancelled;

        //
        //Async operation to ensure synchornization
        //context
        //
        AsyncOperation m_AsyncOp;

        //
        //A link to the resolver to avoid 
        //circular dependencies and enable GC 
        //
        WeakReference m_peerWeakReference;

        //
        //Lock to make sure things don't mess up stuff
        //
        object m_Lock = new Object();

        //
        //EventID or Just a tracking id
        //
        int m_TraceEventId;

        //
        // Store the latest exception
        //
        Exception m_latestException;

        //
        // Stores reponses from all endpoints
        //
        Collection<PeerInvitationResponseType> m_responses = new Collection<PeerInvitationResponseType>();

        //
        // Callback called
        //
        bool m_Completed;

        //
        // Used to ensure only on thread calls the callback
        //
        bool m_aboutToFireCallback;
        object m_aboutToFireCallbackLock = new object();

        //
        // Number of reponses received
        //
        int m_numberOfResponses;

        PeerContact m_peerContact;
        PeerEndPointCollection m_peerEndPoints;
        Guid m_applicationId;
        string m_message;
        byte[] m_inviteData;

        internal InviteAsyncHelper( PeerContact peerContact, Peer parentPeer, PeerEndPointCollection peerEndPoints,
                                    Guid applicationId, string message, byte[] inviteData,
                                    object userState, int NewTraceEventId)
        {
            m_userState = userState;
            m_peerContact = peerContact;
            m_applicationId = applicationId;
            m_message = message;
            m_inviteData = inviteData;
            m_peerEndPoints = peerEndPoints;
            m_TraceEventId = NewTraceEventId;
            m_peerWeakReference = new WeakReference(parentPeer);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "New InviteAsyncHelper created with TraceEventID {0}", m_TraceEventId);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
                "\tPeerContact: {0}, App Guid: {1}, userState {2}, ParentReference {3}",
                (m_peerContact != null ? m_peerContact.ToString() : "null"),
                applicationId.ToString(),
                userState.GetHashCode(),
                m_peerWeakReference.Target.GetHashCode()
                );

        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.Byte[],System.Int32,System.IntPtr,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsClosed():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local data of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: InviteCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabMemory..ctor(System.Int32)" Ring="1" />
        // <ReferencesCritical Name="Method: InviteAsyncEndPoint(PeerEndPoint, PEER_INVITATION):Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_SafeCollabInvite" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void InviteAsync()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
            "InviteAsync called");
            //
            //First wire up a callback
            //
            
            m_RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(m_InviteEvent, //Event that triggers the callback
                                                    new WaitOrTimerCallback(InviteCallback), //callback to be called 
                                                    null, //state to be passed
                                                    -1,   //Timeout - aplicable only for timers not for events 
                                                    false //call us everytime the event is set not just one time
                                                    );

            //
            //Now call the native API to start the resolution 
            //process save the handle for later
            //
            
            PEER_INVITATION pi = new PEER_INVITATION();
            pi.applicationId = CollaborationHelperFunctions.ConvertGuidToGUID(m_applicationId);
            pi.pwzMessage = m_message;

            SafeCollabMemory data = null;
            pi.applicationData.cbData = (m_inviteData != null) ? (UInt32)m_inviteData.Length : 0;

            if ((m_inviteData != null) && (m_inviteData.Length > 0)){
                data = new SafeCollabMemory(m_inviteData.Length);
                pi.applicationData.pbData = data.DangerousGetHandle();
                Marshal.Copy(m_inviteData, 0, pi.applicationData.pbData, m_inviteData.Length);
            }
            else
                pi.applicationData.pbData = IntPtr.Zero;

            foreach (PeerEndPoint peerEndPoint in m_peerEndPoints)
            {
                try{
                    InviteAsyncEndPoint(peerEndPoint, pi);
                }
                catch (PeerToPeerException){
                    if (!m_SafeCollabInvite.IsInvalid && !m_SafeCollabInvite.IsClosed){
                        m_SafeCollabInvite.Dispose();
                    }
                    m_RegisteredWaitHandle.Unregister(null);
                    m_RegisteredWaitHandle = null;
                    throw;
                }
            }

            //
            //Create an async operation with the given 
            //user state
            //
            m_AsyncOp = AsyncOperationManager.CreateOperation(m_userState);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId,
            "Leaving InviteAsync.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabAsyncInviteContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT&,System.IntPtr,System.Net.PeerToPeer.Collaboration.PEER_INVITATION&,Microsoft.Win32.SafeHandles.SafeWaitHandle,System.Net.PeerToPeer.Collaboration.SafeCollabInvite&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabAsyncInviteEndpoint(System.IntPtr,System.Net.PeerToPeer.Collaboration.PEER_INVITATION&,Microsoft.Win32.SafeHandles.SafeWaitHandle,System.Net.PeerToPeer.Collaboration.SafeCollabInvite&):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <ReferencesCritical Name="Local safeCredentials of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(System.Net.PeerToPeer.Collaboration.PeerContact,System.Net.PeerToPeer.Collaboration.SafeCollabMemory&):System.Net.PeerToPeer.Collaboration.PEER_CONTACT" Ring="1" />
        // <ReferencesCritical Name="Field: m_SafeCollabInvite" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void InviteAsyncEndPoint(PeerEndPoint peerEndPoint, PEER_INVITATION pi)
        {
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "InviteAsyncEndPoint() is called with the following Info");
                peerEndPoint.TracePeerEndPoint();
            } 
            
            PEER_ENDPOINT pep = new PEER_ENDPOINT();
            pep.peerAddress = CollaborationHelperFunctions.ConvertIPEndpointToPEER_ADDRESS(peerEndPoint.EndPoint);

            //
            // Pin all the data to pass to native
            //
            GCHandle pepName = new GCHandle();
            
            if (peerEndPoint.Name != null){
                pepName = GCHandle.Alloc(peerEndPoint.Name, GCHandleType.Pinned);
                pep.pwzEndpointName = pepName.AddrOfPinnedObject();
            }
            GCHandle peerEP = GCHandle.Alloc(pep, GCHandleType.Pinned);
            IntPtr ptrPeerEP = peerEP.AddrOfPinnedObject();

            int errorCode;
            try{
                //
                // Make native call with endpoint with/without contact
                //
                if (m_peerContact != null){

                    //
                    // Generate native contact
                    //
                    SafeCollabMemory safeCredentials = null;
                    PEER_CONTACT pc = CollaborationHelperFunctions.ConvertPeerContactToPEER_CONTACT(m_peerContact, ref safeCredentials);

                    try{
                        errorCode = UnsafeCollabNativeMethods.PeerCollabAsyncInviteContact(  ref pc,
                                                                                        ptrPeerEP,
                                                                                        ref pi,
                                                                                        m_InviteEvent.SafeWaitHandle,
                                                                                        out m_SafeCollabInvite);
                    }
                    finally{
                        if (safeCredentials != null) safeCredentials.Dispose();
                    }
                }
                else
                    errorCode = UnsafeCollabNativeMethods.PeerCollabAsyncInviteEndpoint(ptrPeerEP, ref pi,
                                                                                m_InviteEvent.SafeWaitHandle,
                                                                                out m_SafeCollabInvite);

            }
            finally
            {
                if (pepName.IsAllocated) pepName.Free();
                if (peerEP.IsAllocated) peerEP.Free();
            }

            if (errorCode != 0){
                throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_AsyncInviteFailed), errorCode);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Leaving InviteAsyncEndPoint.");

        }

        //
        // Invite callback. Will fire only if i has at least one accepted or when it has all the responses
        // from all the endpoints
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetInvitationResponse(System.Net.PeerToPeer.Collaboration.SafeCollabInvite,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local response of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: m_SafeCollabInvite" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal void InviteCallback(object state, bool timedOut)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Entering InviteCallback.");
            
            SafeCollabData response = null;

            int errorCode = 0;
            InviteCompletedEventArgs inviteCompletedArgs = null;
            Peer peer = null;
            bool fireCallback = false;
            PEER_INVITATION_RESPONSE pir = new PEER_INVITATION_RESPONSE();

            try
            {
                lock (m_Lock){
                    if (m_Cancelled || m_Completed){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Invite cancelled({0}) or completed({1}). Returning without doing anything.", m_Cancelled, m_Completed);
                        return;
                    }
                    errorCode = UnsafeCollabNativeMethods.PeerCollabGetInvitationResponse(m_SafeCollabInvite, out response);
                }

                if ((errorCode != 0) && (errorCode != UnsafeCollabReturnCodes.PEER_E_TIMEOUT)){
                    m_latestException = PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_AsyncInviteException), errorCode);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Got an exception {0}. Storing it in latest exception.", m_latestException);
                }
                else
                {
                    pir = (PEER_INVITATION_RESPONSE)Marshal.PtrToStructure(response.DangerousGetHandle(),
                                                    typeof(PEER_INVITATION_RESPONSE));

                    //
                    // Store the responses
                    //
                    lock (m_responses)
                        m_responses.Add(pir.action);

                    if (pir.action == PeerInvitationResponseType.Accepted){
                        inviteCompletedArgs = new InviteCompletedEventArgs(new PeerInvitationResponse(pir.action), null, false,
                                                                            m_AsyncOp.UserSuppliedState);
                        fireCallback = true;

                        //
                        // Got an accepted. unregister callback to disable all othe other endpoint callbacks
                        //
                        
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Found an accepted. About to fire callback.");

                        m_RegisteredWaitHandle.Unregister(null);
                    }
                }


                Interlocked.Increment(ref m_numberOfResponses);

                if ((!fireCallback) && (m_numberOfResponses == m_peerEndPoints.Count) && (!m_aboutToFireCallback)){
                    //
                    // Two threads can be here at the same time when all the responses have been 
                    // received and only one should be allowed to call the callback
                    //
                    lock (m_aboutToFireCallbackLock){
                        if (m_aboutToFireCallback)
                            return;

                        m_aboutToFireCallback = true;

                        bool foundDeclined = false;
                        bool foundExpired = false;

                        fireCallback = true;

                        //
                        // Got all responses; make a decision
                        //

                        foreach (PeerInvitationResponseType responseType in m_responses)
                        {
                            if (responseType == PeerInvitationResponseType.Expired){
                                foundExpired = true;
                            }
                            else if (responseType == PeerInvitationResponseType.Declined){
                                foundDeclined = true;
                                break;
                            }
                        }

                        //
                        // If at least one is declined, return declined.  
                        //

                        if (foundDeclined){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Got a declined invite response.");
                            inviteCompletedArgs = new InviteCompletedEventArgs(new PeerInvitationResponse(PeerInvitationResponseType.Declined), null, false,
                                                        m_AsyncOp.UserSuppliedState);
                        }
                        else if (foundExpired){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Got an expired invite response but no accepted or declined.");
                            inviteCompletedArgs = new InviteCompletedEventArgs(new PeerInvitationResponse(PeerInvitationResponseType.Expired), null, false,
                                                        m_AsyncOp.UserSuppliedState);
                        }
                        else{
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Got all error responses");
                            inviteCompletedArgs = new InviteCompletedEventArgs(null, (m_latestException != null ? m_latestException : new PeerToPeerException("InviteAsync failure.")), false,
                                                        m_AsyncOp.UserSuppliedState);
                        }
                    }
                }
                //
                //Last chance to prevent the callback 
                //
                if (fireCallback){
                    peer = m_peerWeakReference.Target as Peer;
                    if (!m_Completed && (peer != null)){
                        lock (m_Lock){
                            //
                            // Async op may be cancelled already
                            //
                            if (!m_Completed && !m_Cancelled){
                                
                                //
                                //Mark as completed so that this gets fired only once
                                //
                                m_Completed = true;
                                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Firing callback with response type {0}.", inviteCompletedArgs.InviteResponse);    
                                peer.PrepareToRaiseInviteCompletedEvent(m_AsyncOp, inviteCompletedArgs);
                            }
                        }
                    }
                }
            }
            finally{
                if (response != null) response.Dispose();
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Leaving InviteCallback.");

       }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabCancelInvitation(System.Net.PeerToPeer.Collaboration.SafeCollabInvite):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Field: m_SafeCollabInvite" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void ContinueCancelCallback(object state)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Entering ContineCancelCallback.");
            
            try{
                lock (m_Lock){
                    if (m_Completed) return;
                    m_Cancelled = true;

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabCancelInvitation(m_SafeCollabInvite);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabCancelInvitation returned with errorcode {0}", errorCode);
                    }

                    m_SafeCollabInvite.Dispose();
                }

                Peer peer = m_peerWeakReference.Target as Peer;
                if (peer != null){
                    InviteCompletedEventArgs e = new InviteCompletedEventArgs(null, null, true, m_AsyncOp.UserSuppliedState);
                    peer.PrepareToRaiseInviteCompletedEvent(m_AsyncOp, e);
                }
            }
            catch (ObjectDisposedException ex){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Critical, 0, "Exception while cancelling the call {0}", ex);
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, m_TraceEventId, "Leaving ContineCancelCallback.");

        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: ContinueCancelCallback(Object):Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void CancelAsync(object state)
        {
            //
            //Defer the work to a callback
            //

            ThreadPool.QueueUserWorkItem(new WaitCallback(ContinueCancelCallback), state);
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
        // <ReferencesCritical Name="Field: m_SafeCollabInvite" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Dispose(bool disposing)
        {
            if (!m_Disposed){
                if (!m_SafeCollabInvite.IsInvalid){
                    m_SafeCollabInvite.Dispose();
                }

                if (m_RegisteredWaitHandle != null){
                    m_RegisteredWaitHandle.Unregister(null);
                    m_RegisteredWaitHandle = null;
                }

                if (m_InviteEvent != null){
                    m_InviteEvent.Close();
                }
            }
            m_Disposed = true;
        }

        public override int GetHashCode()
        {
            return m_TraceEventId;
        }

        internal int TraceEventId
        {
            get{
                return m_TraceEventId;
            }
        }

    }

}
