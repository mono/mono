//------------------------------------------------------------------------------
// <copyright file="PeerCollaboration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Net.Mail;
    using System.Security.Cryptography.X509Certificates;
    using System.Diagnostics;
    using System.Threading;
    using System.ComponentModel;

    /// <summary>
    /// This class handles all the collaboration platform functions
    /// </summary>
    public static class PeerCollaboration
    {
        public static PeerPresenceInfo LocalPresenceInfo
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
            // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
            // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetPresenceInfo(System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
            // <ReferencesCritical Name="Local safePresenceInfo of type: SafeCollabData" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                SafeCollabData safePresenceInfo = null;
                PeerPresenceInfo peerPresenceInfo = null;

                try{
                    CollaborationHelperFunctions.Initialize();

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabGetPresenceInfo(IntPtr.Zero, out safePresenceInfo);

                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetPresenceInfo returned with errorcode {0}", errorCode);
                        return null;
                    }

                    IntPtr ptrPeerPresenceInfo = safePresenceInfo.DangerousGetHandle();
                    PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(ptrPeerPresenceInfo, 
                                                                                        typeof(PEER_PRESENCE_INFO));

                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Retrieved a PeerPresenceInfo with PresenceStatus {0}, Desc {1}", ppi.status, ppi.descText);

                    peerPresenceInfo = new PeerPresenceInfo();
                    peerPresenceInfo.PresenceStatus = ppi.status;
                    peerPresenceInfo.DescriptiveText = ppi.descText;
                }
                catch (Exception e){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetPresenceInfo had exception {0}", e.Message);
                    throw;
                }
                finally{
                    if(safePresenceInfo != null) safePresenceInfo.Dispose();
                }
                return peerPresenceInfo;
            }
            // <SecurityKernel Critical="True" Ring="0">
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabSetPresenceInfo(System.Net.PeerToPeer.Collaboration.PEER_PRESENCE_INFO&):System.Int32" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            set
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                CollaborationHelperFunctions.Initialize();

                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.PresenceStatus == PeerPresenceStatus.Offline)
                    throw new PeerToPeerException(SR.GetString(SR.Collab_SetPresenceOffline));

                PEER_PRESENCE_INFO ppi;
                ppi.status = value.PresenceStatus;
                ppi.descText = value.DescriptiveText;

                int errorCode = UnsafeCollabNativeMethods.PeerCollabSetPresenceInfo(ref ppi);

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSetPresenceInfo returned with errorcode {0}", errorCode);
                    throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_SetLocalPresenceFailed), errorCode));
                }

            }
        }

        public static string LocalEndPointName
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEndpointName(System.String&):System.Int32" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                string localEndPointName = null;

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "LocalEndPointName get called.");
                //
                // Call native to get users endpoint name
                //

                try{
                    int errorCode = UnsafeCollabNativeMethods.PeerCollabGetEndpointName(ref localEndPointName);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetEndpointName returned with errorcode {0}", errorCode);
                    }
                }
                catch (Exception e){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "LocalEndPointName threw exception {0}", e.ToString());
                    throw;
                }

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "LocalEndPointName get returning {0}.",
                                                                                        localEndPointName);

                return localEndPointName;
            }

            // <SecurityKernel Critical="True" Ring="0">
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabSetEndpointName(System.String):System.Int32" />
            // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            set
            {
                //
                // Call native to set users endpoint name
                //
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "LocalEndPointName set called with {0}.",
                                                                                    value);
                
                int errorCode = UnsafeCollabNativeMethods.PeerCollabSetEndpointName(value);
                
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSetEndpointName returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_SetLocalEndPointNameFailed), errorCode);
                }

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving LocalEndPointName set.");
            }
        }

        public static PeerScope SignInScope
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetSigninOptions(System.Net.PeerToPeer.Collaboration.PeerScope&):System.Int32" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                PeerScope peerScope = PeerScope.None;

                CollaborationHelperFunctions.Initialize();
                int errorCode = UnsafeCollabNativeMethods.PeerCollabGetSigninOptions(ref peerScope);

                if(errorCode != 0)
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetSigninOptions returned with errorcode {0}", errorCode);

                return peerScope;
            }
        }

        //
        // Lock to ensure only one Contact Manager is instantiated
        //
        static object s_contactManagerSyncObject;
        internal static object ContactManagerSyncObject
        {
            get{
                if (s_contactManagerSyncObject == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_contactManagerSyncObject, o, null);
                }
                return s_contactManagerSyncObject;
            }
        }

        static volatile ContactManager s_contactManager;
        //
        // Returns the contact manager instance. Only one is created.
        //
        public static ContactManager ContactManager
        {
            // <SecurityKernel Critical="True" Ring="2">
            // <ReferencesCritical Name="Method: ContactManager..ctor()" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get{
                if (s_contactManager == null){
                    lock (ContactManagerSyncObject){
                        if (s_contactManager == null){
                            s_contactManager = new ContactManager();
                        }
                    }
                }
                return s_contactManager;
            }
        }
        
        public static PeerApplicationLaunchInfo ApplicationLaunchInfo
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
            // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
            // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetAppLaunchInfo(System.Net.PeerToPeer.Collaboration.PEER_APP_LAUNCH_INFO&):System.Int32" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(System.Net.PeerToPeer.Collaboration.PEER_CONTACT):System.Net.PeerToPeer.Collaboration.PeerContact" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering GetApplicationLaunchInfo.");

                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

                CollaborationHelperFunctions.Initialize();

                PeerApplicationLaunchInfo peerAppLaunchInfo = null;
                SafeCollabData appLaunchInfoData = null;

                int errorCode = UnsafeCollabNativeMethods.PeerCollabGetAppLaunchInfo(out appLaunchInfoData);

                //
                // Special case. No Data found, return null.
                //
                if (errorCode == UnsafeCollabReturnCodes.PEER_E_NOT_FOUND){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "No Application launch info available.");
                    return peerAppLaunchInfo;
                }

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetAppLaunchInfo returned with errorcode {0}", errorCode);
                    return peerAppLaunchInfo;
                }

                //
                // Marshal individual Launch info elements
                //

                try{
                    PEER_APP_LAUNCH_INFO pali = (PEER_APP_LAUNCH_INFO)Marshal.PtrToStructure(appLaunchInfoData.DangerousGetHandle(),
                                                            typeof(PEER_APP_LAUNCH_INFO));

                    peerAppLaunchInfo = new PeerApplicationLaunchInfo();

                    if (pali.pContact != IntPtr.Zero){
                        PEER_CONTACT pc = (PEER_CONTACT)Marshal.PtrToStructure(pali.pContact, typeof(PEER_CONTACT));
                        peerAppLaunchInfo.PeerContact = CollaborationHelperFunctions.ConvertPEER_CONTACTToPeerContact(pc);
                    }

                    PEER_INVITATION pi = (PEER_INVITATION)Marshal.PtrToStructure(pali.pInvitation, typeof(PEER_INVITATION));
                    peerAppLaunchInfo.PeerApplication = new PeerApplication(CollaborationHelperFunctions.ConvertGUIDToGuid(pi.applicationId),
                                                                            null, null, null, null, PeerScope.None);
                    peerAppLaunchInfo.Message = pi.pwzMessage;
                    byte[] data = null;

                    if (pi.applicationData.cbData != 0){
                        data = new byte[pi.applicationData.cbData];
                        Marshal.Copy(pi.applicationData.pbData, data, 0, (int)pi.applicationData.cbData);
                    }

                    peerAppLaunchInfo.Data = data;

                    PEER_ENDPOINT pe = (PEER_ENDPOINT)Marshal.PtrToStructure(pali.pEndpoint, typeof(PEER_ENDPOINT));
                    peerAppLaunchInfo.PeerEndPoint = CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe);

                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "GetApplicationLaunchInfo successful.");
                }
                catch (Exception e){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "GetApplicationLaunchInfo threw error {0}.", e.ToString());
                    throw;
                }
                finally{
                    if (appLaunchInfoData != null) appLaunchInfoData.Dispose();
                }
                
                return peerAppLaunchInfo;
            }
        }

        static volatile ISynchronizeInvoke s_synchronizingObject;
        /// <summary>
        /// Gets and set the object used to marshall event handlers calls for stand alone 
        /// events
        /// </summary>
        [Browsable(false), DefaultValue(null), Description(SR.SynchronizingObject)]
        public static ISynchronizeInvoke SynchronizingObject
        {
            get{
                return s_synchronizingObject;
            }
            set{
                s_synchronizingObject = value;
            }
        }


        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabSignin(System.IntPtr,System.Net.PeerToPeer.Collaboration.PeerScope):System.Int32" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void SignIn(PeerScope peerScope)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering SignIn with peerscope {0}.", peerScope);

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if ((peerScope < PeerScope.None) || (peerScope > PeerScope.All))
                throw new ArgumentOutOfRangeException("peerScope");
            
            if (peerScope == PeerScope.None)
                throw new ArgumentException(SR.GetString(SR.Collab_SignInWithNone), "peerScope");

            CollaborationHelperFunctions.Initialize();

            int errorCode = UnsafeCollabNativeMethods.PeerCollabSignin(IntPtr.Zero, peerScope);

            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSignin returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_SignInFailed), errorCode));
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Signed into {0}.", peerScope);
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabSignout(System.Net.PeerToPeer.Collaboration.PeerScope):System.Int32" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void SignOut(PeerScope peerScope)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering SignOut with peerscope {0}.", peerScope);
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            CollaborationHelperFunctions.Initialize();

            if ((peerScope < PeerScope.None) || (peerScope > PeerScope.All))
                throw new ArgumentOutOfRangeException("peerScope");

            if (peerScope != PeerScope.None){
                int errorCode = UnsafeCollabNativeMethods.PeerCollabSignout(peerScope);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSignout returned with errorcode {0}", errorCode);
                    throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_SignOutFailed), errorCode));
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Signed out of {0}.", peerScope);
        }

        //
        // Gives you all the peers on the same subnet as you
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pPeersNearMe of type: IntPtr*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumPeopleNearMe(System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local appArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerNearMeCollection GetPeersNearMe()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering GetPeersNearMeGetPeersNearMe.");
            CollaborationHelperFunctions.Initialize();
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            PeerNearMeCollection pnmc = new PeerNearMeCollection();

            SafeCollabEnum handlePeerEnum = null;
            UInt32 pnmCount = 0;
            int errorCode = 0;

            try{
                //
                // Call native to get the enumeration of peers near
                //
                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumPeopleNearMe(out handlePeerEnum);

                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumPeopleNearMe returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetPeersNearMeFailed), errorCode);
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref pnmCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetPeersNearMeFailed), errorCode);
                }

                if (pnmCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "No peers found. \nLeaving GetPeersNearMe()");
                    return pnmc;
                }

                unsafe{
                    SafeCollabData appArray;
                    errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref pnmCount, out appArray);

                    IntPtr pPEER_PEOPLE_NEAR_ME = appArray.DangerousGetHandle();
                    IntPtr* pPeersNearMe = (IntPtr*)pPEER_PEOPLE_NEAR_ME;

                    //
                    // Loops through individual native structures and makes peers from them 
                    //
                    for (ulong i = 0; i < pnmCount; i++){
                        IntPtr pContactPtr = (IntPtr)pPeersNearMe[i];
                        PEER_PEOPLE_NEAR_ME pnm = (PEER_PEOPLE_NEAR_ME)Marshal.PtrToStructure(pContactPtr, typeof(PEER_PEOPLE_NEAR_ME));

                        PeerNearMe peerNearMe = new PeerNearMe();
                        peerNearMe.Id = CollaborationHelperFunctions.ConvertGUIDToGuid(pnm.id);
                        peerNearMe.Nickname = Marshal.PtrToStringUni(pnm.pwzNickname); ;

                        PEER_ENDPOINT pe = pnm.endpoint;
                        peerNearMe.PeerEndPoints.Add(CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(pe));

                        pnmc.Add(peerNearMe);
                    }
                }
            }
            finally{
                if (handlePeerEnum != null) handlePeerEnum.Dispose();
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving GetPeersNearMeGetPeersNearMe with {0} peers.", pnmc.Count);

            return pnmc;
        }
        
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.Byte[],System.Int32,System.IntPtr,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterApplication(System.Net.PeerToPeer.Collaboration.PEER_APPLICATION_REGISTRATION_INFO&,System.Net.PeerToPeer.Collaboration.PeerApplicationRegistrationType):System.Int32" />
        // <ReferencesCritical Name="Local data of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabMemory..ctor(System.Int32)" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void RegisterApplication(PeerApplication application, PeerApplicationRegistrationType type)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (application == null){
                throw new ArgumentNullException("application");
            }
            
            if (application.Path == null){
                throw new ArgumentException(SR.GetString(SR.Collab_AppRegNoPathError));
            }

            if ((type < PeerApplicationRegistrationType.CurrentUser) || (type > PeerApplicationRegistrationType.AllUsers)){
                throw new ArgumentOutOfRangeException("type");
            }

            CollaborationHelperFunctions.Initialize();

            int errorCode = 0;

            //
            // Convert PeerApplication.Guid into native GUID struct
            //

            PEER_APPLICATION_REGISTRATION_INFO appRegInfo = new PEER_APPLICATION_REGISTRATION_INFO();
            appRegInfo.application.guid = CollaborationHelperFunctions.ConvertGuidToGUID(application.Id);

            appRegInfo.pwzApplicationArguments = application.CommandLineArgs;
            appRegInfo.pwzApplicationToLaunch = application.Path;
            appRegInfo.dwPublicationScope = (uint)application.PeerScope;

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RegisterApplication() is called with the following Info");
                application.TracePeerApplication();
            }

            unsafe{

                SafeCollabMemory data = null;
                appRegInfo.application.data.cbData =    (application.Data!=null) ? 
                                                        (UInt32)application.Data.Length : 0;
                GCHandle descHandle = new GCHandle();

                try{
                    //
                    // Marshal any data to send to native call
                    //

                    if ((application.Data!=null) && (application.Data.Length > 0))
                    {
                        data = new SafeCollabMemory(application.Data.Length);
                        appRegInfo.application.data.pbData = data.DangerousGetHandle();
                        Marshal.Copy(application.Data, 0, appRegInfo.application.data.pbData, application.Data.Length);
                    }
                    else
                        appRegInfo.application.data.pbData = IntPtr.Zero;

                    descHandle = GCHandle.Alloc(application.Description, GCHandleType.Pinned);
                    appRegInfo.application.pwzDescription = descHandle.AddrOfPinnedObject();

                    errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterApplication(ref appRegInfo, type);

                }
                finally{
                    if (descHandle.IsAllocated) descHandle.Free();
                    if (data != null) data.Dispose();
                }
            }
            if (errorCode == UnsafeCollabReturnCodes.PEER_E_ALREADY_EXISTS){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterApplication returned with errorcode {0}. Application already registered.", errorCode);
                throw new ArgumentException(SR.GetString(SR.Collab_AppRegFailed) + " " + SR.GetString(SR.Collab_AppExists));
            }
            else if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterApplication returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_AppRegFailed), errorCode));
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "RegisterApplication successful");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabUnregisterApplication(System.Net.PeerToPeer.Collaboration.GUID&,System.Net.PeerToPeer.Collaboration.PeerApplicationRegistrationType):System.Int32" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void UnregisterApplication(PeerApplication application, PeerApplicationRegistrationType type)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (application == null){
                throw new ArgumentNullException("application");
            }

            if (application.Id.Equals(Guid.Empty)){
                throw new ArgumentException(SR.GetString(SR.Collab_EmptyGuidError));
            }

            if ((type < PeerApplicationRegistrationType.CurrentUser) || (type > PeerApplicationRegistrationType.AllUsers)){
                throw new ArgumentOutOfRangeException("type");
            }

            CollaborationHelperFunctions.Initialize();

            //
            // Convert PeerApplication.Guid into native GUID struct
            //

            GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(application.Id);

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information))
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "UnregisterApplication() is called with the following Info");
                application.TracePeerApplication();
            }

            int errorCode = UnsafeCollabNativeMethods.PeerCollabUnregisterApplication(ref guid, type);

            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabUnregisterApplication returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_AppUnregFailed), errorCode));
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "UnregisterApplication successful");
        }

        //
        // Gets all applications registered to collab on this machine
        //
        public static PeerApplicationCollection GetLocalRegisteredApplications()
        {
            PeerApplicationCollection allUsersColl = GetLocalRegisteredApplications(PeerApplicationRegistrationType.AllUsers);
            PeerApplicationCollection localUsersColl = GetLocalRegisteredApplications(PeerApplicationRegistrationType.CurrentUser);

            if ((allUsersColl == null) || (allUsersColl.Count == 0)){ 
                return localUsersColl; 
            }
            else if ((localUsersColl == null) || (localUsersColl.Count == 0)){ 
                return allUsersColl; 
            }
            else{
                //
                // Merge the two to remove dupes
                //

                foreach (PeerApplication peerApplicationLocal in localUsersColl){
                    PeerApplication peerAppToRemove = null;

                    foreach (PeerApplication peerApplicationAll in allUsersColl){
                        if (peerApplicationAll.Id.Equals(peerApplicationLocal.Id)){
                            peerAppToRemove = peerApplicationAll;
                        }
                    }
                    if (peerAppToRemove != null)
                        allUsersColl.Remove(peerAppToRemove);
                }

                foreach (PeerApplication peerApplicationAll in allUsersColl){
                    localUsersColl.Add(peerApplicationAll);
                }
                return localUsersColl;
            }
        }

        //
        // Gets all applications registered to collab for specific user on this machine
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pApps of type: IntPtr*" />
        // <UsesUnsafeCode Name="Local pPeerApp of type: PEER_APPLICATION*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumApplications(System.IntPtr,System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetApplicationRegistrationInfo(System.Net.PeerToPeer.Collaboration.GUID&,System.Net.PeerToPeer.Collaboration.PeerApplicationRegistrationType,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local appArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Local safeAppRegInfo of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabEnum..ctor()" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabData..ctor()" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerApplicationCollection GetLocalRegisteredApplications(PeerApplicationRegistrationType type)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering GetLocalRegisteredApplications.");
            
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if ((type < PeerApplicationRegistrationType.CurrentUser) || (type > PeerApplicationRegistrationType.AllUsers)){
                throw new ArgumentOutOfRangeException("type");
            }

            CollaborationHelperFunctions.Initialize();

            PeerApplicationCollection peerAppColl = new PeerApplicationCollection();
            SafeCollabEnum handlePeerEnum = new SafeCollabEnum();
            UInt32 appCount = 0;
            int errorCode = 0;

            //
            // Enumerate and get all the registered applications from native
            //

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumApplications(IntPtr.Zero, IntPtr.Zero, out handlePeerEnum);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumApplications returned with errorcode {0}", errorCode);
                    throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalAppsFailed), errorCode));
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref appCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalAppsFailed), errorCode));
                }

                if (appCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "No local registered PeerApplications found.");
                    return peerAppColl;
                }

                unsafe{
                    SafeCollabData appArray = null;
                    try{
                        errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref appCount, out appArray);
                        if (errorCode != 0){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetNextItem returned with errorcode {0}", errorCode);
                            throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalAppsFailed), errorCode));
                        }

                        //
                        // Marshal each application from the array
                        //

                        IntPtr pPEER_APLICATION = appArray.DangerousGetHandle();
                        IntPtr* pApps = (IntPtr*)pPEER_APLICATION;
                        for (ulong i = 0; i < appCount; i++){
                            PEER_APPLICATION* pPeerApp = (PEER_APPLICATION*)pApps[i];
                            string description = Marshal.PtrToStringUni(pPeerApp->pwzDescription);
                            byte[] data = null;

                            if (pPeerApp->data.cbData != 0){
                                data = new byte[pPeerApp->data.cbData];
                                Marshal.Copy(pPeerApp->data.pbData, data, 0, (int)pPeerApp->data.cbData);
                            }

                            PeerApplication peerApp = new PeerApplication(  CollaborationHelperFunctions.ConvertGUIDToGuid(pPeerApp->guid), 
                                                                            description, data, null, null, PeerScope.None);
                            peerAppColl.Add(peerApp);
                        }
                    }
                    finally{
                        appArray.Dispose();
                    }
                }
            }
            finally{
                handlePeerEnum.Dispose();
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Got all local registered Applications. Start filtering");

            PeerApplicationCollection peerAppCollFiltered = new PeerApplicationCollection();

            //
            // Filter the apps according to the Registration type the user wants
            //

            foreach (PeerApplication peerApplication in peerAppColl)
            {
                GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(peerApplication.Id);
                SafeCollabData safeAppRegInfo = new SafeCollabData();

                try{
                    errorCode = UnsafeCollabNativeMethods.PeerCollabGetApplicationRegistrationInfo(ref guid,
                                                                                                    type,
                                                                                                    out safeAppRegInfo);
                    if (errorCode != 0)
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabGetApplicationRegistrationInfo returned with errorcode {0}", errorCode);

                    if (!safeAppRegInfo.IsInvalid){
                        PEER_APPLICATION_REGISTRATION_INFO pari = (PEER_APPLICATION_REGISTRATION_INFO)
                                                                Marshal.PtrToStructure(safeAppRegInfo.DangerousGetHandle(),
                                                                                        typeof(PEER_APPLICATION_REGISTRATION_INFO));
                        peerApplication.Path = pari.pwzApplicationToLaunch;
                        peerApplication.CommandLineArgs = pari.pwzApplicationArguments;
                        peerApplication.PeerScope = (PeerScope)pari.dwPublicationScope;
                        
                        peerAppCollFiltered.Add(peerApplication);
                    }
                }
                finally{
                    safeAppRegInfo.Dispose();
                }

            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Filtering successful. Returning collection with {0} applications", peerAppCollFiltered.Count);

            return peerAppCollFiltered;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.Byte[],System.Int32,System.IntPtr,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabSetObject(System.Net.PeerToPeer.Collaboration.PEER_OBJECT&):System.Int32" />
        // <ReferencesCritical Name="Local data of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabMemory..ctor(System.Int32)" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void SetObject(PeerObject peerObject)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerObject == null){
                throw new ArgumentNullException("peerObject");
            }

            CollaborationHelperFunctions.Initialize();

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "SetObject() is called with the following info");
                peerObject.TracePeerObject();
            }

            PEER_OBJECT po = new PEER_OBJECT();
            int errorCode = 0;

            //
            // Create the native PEER_OBJECT struct
            //

            po.guid = CollaborationHelperFunctions.ConvertGuidToGUID(peerObject.Id);
            po.dwPublicationScope = (uint)peerObject.PeerScope;
            SafeCollabMemory data = null;

            try{
                if ((peerObject.Data != null) && (peerObject.Data.Length > 0)){
                    data = new SafeCollabMemory(peerObject.Data.Length);
                    po.data.pbData = data.DangerousGetHandle();
                    po.data.cbData = (UInt32)peerObject.Data.Length;

                    Marshal.Copy(peerObject.Data, 0, po.data.pbData, peerObject.Data.Length);
                }
                else{
                    po.data.pbData = IntPtr.Zero;
                    po.data.cbData = 0;
                }

                errorCode = UnsafeCollabNativeMethods.PeerCollabSetObject(ref po);
            }
            finally{
                if (data != null) data.Dispose();
            }

            if (errorCode == UnsafeCollabReturnCodes.PEER_E_ALREADY_EXISTS){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSetObject returned with errorcode {0}. Object already set.", errorCode);
                throw new ArgumentException(SR.GetString(SR.Collab_ObjectSetFailed) + " " + SR.GetString(SR.Collab_ObjectExists));
            }
            else if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabSetObject returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ObjectSetFailed), errorCode));
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Set Object successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabDeleteObject(System.Net.PeerToPeer.Collaboration.GUID&):System.Int32" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static void DeleteObject(PeerObject peerObject)
        {
            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();

            if (peerObject == null){
                throw new ArgumentNullException("peerObject");
            }

            CollaborationHelperFunctions.Initialize();

            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information)){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "DeleteObject() is called with the following info");
                peerObject.TracePeerObject();
            }

            GUID guid = CollaborationHelperFunctions.ConvertGuidToGUID(peerObject.Id);
            int errorCode = UnsafeCollabNativeMethods.PeerCollabDeleteObject(ref guid);

            if (errorCode != 0){
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabDeleteObject returned with errorcode {0}", errorCode);
                throw (PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ObjectDeleteFailed), errorCode));
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Delete Object successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Local pObjects of type: IntPtr*" />
        // <UsesUnsafeCode Name="Local pPeerObject of type: PEER_OBJECT*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabEnumObjects(System.IntPtr,System.IntPtr,System.Net.PeerToPeer.Collaboration.SafeCollabEnum&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetItemCount(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerGetNextItem(System.Net.PeerToPeer.Collaboration.SafeCollabEnum,System.UInt32&,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local handlePeerEnum of type: SafeCollabEnum" Ring="1" />
        // <ReferencesCritical Name="Local objectArray of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerObjectCollection GetLocalSetObjects()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering GetLocalSetObjects.");

            PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
            
            CollaborationHelperFunctions.Initialize();

            PeerObjectCollection peerObjectColl = new PeerObjectCollection();
            SafeCollabEnum handlePeerEnum = null;
            UInt32 objectCount = 0;
            int errorCode = 0;

            //
            // Enumerate through all the objects from native
            //

            try{
                errorCode = UnsafeCollabNativeMethods.PeerCollabEnumObjects(IntPtr.Zero, IntPtr.Zero, out handlePeerEnum);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabEnumObjects returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalObjectsFailed), errorCode);
                }

                errorCode = UnsafeCollabNativeMethods.PeerGetItemCount(handlePeerEnum, ref objectCount);
                if (errorCode != 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetItemCount returned with errorcode {0}", errorCode);
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalObjectsFailed), errorCode);
                }

                if (objectCount == 0){
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "No local PeerObjects found.");
                    return peerObjectColl;
                }

                unsafe{
                    SafeCollabData objectArray = null;
                    try{
                        errorCode = UnsafeCollabNativeMethods.PeerGetNextItem(handlePeerEnum, ref objectCount, out objectArray);
                        if (errorCode != 0){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerGetNextItem returned with errorcode {0}", errorCode);
                            throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_GetLocalObjectsFailed), errorCode);
                        }

                        //
                        // Marshal each object from the array
                        //

                        IntPtr pPEER_OBJECT = objectArray.DangerousGetHandle();
                        IntPtr* pObjects = (IntPtr*)pPEER_OBJECT;
                        for (ulong i = 0; i < objectCount; i++){
                            PEER_OBJECT* pPeerObject = (PEER_OBJECT*)pObjects[i];
                            byte[] data = null;

                            if (pPeerObject->data.cbData != 0){
                                data = new byte[pPeerObject->data.cbData];
                                Marshal.Copy(pPeerObject->data.pbData, data, 0, (int)pPeerObject->data.cbData);
                            }

                            PeerObject peerObject = new PeerObject(CollaborationHelperFunctions.ConvertGUIDToGuid(pPeerObject->guid), data, (PeerScope)pPeerObject->dwPublicationScope);
                            peerObjectColl.Add(peerObject);
                        }
                    }
                    finally{
                        objectArray.Dispose();
                    }
                }
            }
            finally{
                handlePeerEnum.Dispose();
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Returning collection with {0} objects", peerObjectColl.Count);
            return peerObjectColl;
        }

        private static event EventHandler<NameChangedEventArgs> s_nameChanged;
        public static event EventHandler<NameChangedEventArgs> LocalNameChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                AddNameChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemoveNameChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.NameChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                RemoveNameChanged(value);
            }
        }

        #region Name changed event variables
        private static object s_lockNameChangedEvent;
        private static object LockNameChangedEvent
        {
            get{
                if (s_lockNameChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_lockNameChangedEvent, o, null);
                }
                return s_lockNameChangedEvent;
            }
        }

        private static RegisteredWaitHandle s_regNameChangedWaitHandle;
        private static AutoResetEvent s_nameChangedEvent;
        private static SafeCollabEvent s_safeNameChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Method: NameChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Field: s_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void AddNameChanged(EventHandler<NameChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Entering AddNameChanged().");

            //
            // Register a wait handle if one has not been registered already
            //
            lock (LockNameChangedEvent)
            {
                if (s_nameChanged == null){
                    s_nameChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    s_regNameChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(s_nameChangedEvent, //Event that triggers the callback
                                            new WaitOrTimerCallback(NameChangedCallback), //callback to be called 
                                            null, //state to be passed
                                            -1,   //Timeout - aplicable only for timers
                                            false //call us everytime the event is set
                                            );
                    PEER_COLLAB_EVENT_REGISTRATION pcer = new PEER_COLLAB_EVENT_REGISTRATION();

                    pcer.eventType = PeerCollabEventType.MyEndPointChanged;
                    pcer.pInstance = IntPtr.Zero;

                    //
                    // Register event with collab
                    //

                    int errorCode = UnsafeCollabNativeMethods.PeerCollabRegisterEvent(
                                                                s_nameChangedEvent.SafeWaitHandle,
                                                                1,
                                                                ref pcer,
                                                                out s_safeNameChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_NameChangedRegFailed), errorCode);
                    }
                }
                s_nameChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddNameChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void RemoveNameChanged(EventHandler<NameChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveNameChanged() called.");
            lock (LockNameChangedEvent){
                s_nameChanged -= callback;
                if (s_nameChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref s_regNameChangedWaitHandle,
                                                                ref s_safeNameChangedEvent,
                                                                ref s_nameChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean NameChangedEvent variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveNameChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: s_safeNameChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private static void NameChangedCallback(object state, bool timedOut)
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
                    lock (LockNameChangedEvent)
                    {
                        if (s_safeNameChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(s_safeNameChangedEvent,
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
                    if (ped.eventType == PeerCollabEventType.MyEndPointChanged){

                        if (ped.endpointChangedData.pEndPoint != IntPtr.Zero){
                            //
                            // This means its an endpoint on my contact which is not on the local machine
                            // so we dont care
                            //
                            return;
                        }


                        string newName = PeerCollaboration.LocalEndPointName;

                        nameChangedArgs = new NameChangedEventArgs( null,
                                                                    null,
                                                                    newName);
                    }
                }
                finally{
                    if (eventData != null) eventData.Dispose();
                }

                //
                // Fire the callback with the marshalled event args data
                //
                EventHandler<NameChangedEventArgs> handlerCopy = s_nameChanged;

                if ((nameChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { null, nameChangedArgs });
                    else
                        handlerCopy(null, nameChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the name changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving NameChangedCallback().");
        }

        static event EventHandler<PresenceChangedEventArgs> s_presenceChanged;
        public static event EventHandler<PresenceChangedEventArgs> LocalPresenceChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddPresenceChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                AddPresenceChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemovePresenceChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                RemovePresenceChanged(value);
            }
        }

        #region Presence changed event variables
        static object s_lockPresenceChangedEvent;
        static object LockPresenceChangedEvent
        {
            get{
                if (s_lockPresenceChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_lockPresenceChangedEvent, o, null);
                }
                return s_lockPresenceChangedEvent;
            }
        }

        static RegisteredWaitHandle s_regPresenceChangedWaitHandle;
        static AutoResetEvent s_presenceChangedEvent;
        static SafeCollabEvent s_safePresenceChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PresenceChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.AddMyPresenceChanged(System.EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>,System.EventHandler`1<System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs>&,System.Object,System.Threading.RegisteredWaitHandle&,System.Threading.AutoResetEvent&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.WaitOrTimerCallback):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void AddPresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            CollaborationHelperFunctions.AddMyPresenceChanged(callback, ref s_presenceChanged, LockPresenceChangedEvent,
                                                                    ref s_regPresenceChangedWaitHandle, ref s_presenceChangedEvent,
                                                                    ref s_safePresenceChangedEvent, new WaitOrTimerCallback(PresenceChangedCallback));
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void RemovePresenceChanged(EventHandler<PresenceChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() called.");
            lock (LockPresenceChangedEvent){
                s_presenceChanged -= callback;
                if (s_presenceChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref s_regPresenceChangedWaitHandle,
                                                                ref s_safePresenceChangedEvent,
                                                                ref s_presenceChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean PresenceChanged variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemovePresenceChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: s_safePresenceChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void PresenceChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PresenceChangedCallback() called.");

            while (true)
            {
                PresenceChangedEventArgs presenceChangedArgs = null;
                
                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockPresenceChangedEvent){
                        if (s_safePresenceChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(s_safePresenceChangedEvent,
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
                        if (presenceData.pPresenceInfo != IntPtr.Zero){
                            PEER_PRESENCE_INFO ppi = (PEER_PRESENCE_INFO)Marshal.PtrToStructure(presenceData.pPresenceInfo, typeof(PEER_PRESENCE_INFO));
                            peerPresenceInfo = new PeerPresenceInfo();
                            peerPresenceInfo.PresenceStatus = ppi.status;
                            peerPresenceInfo.DescriptiveText = ppi.descText;
                        }

                        if (presenceData.pEndPoint != IntPtr.Zero){
                            //
                            // This means its an endpoint on my contact which is not on the local machine
                            // so we dont care
                            //
                            return;
                        }

                        presenceChangedArgs = new PresenceChangedEventArgs( null,
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

                EventHandler<PresenceChangedEventArgs> handlerCopy = s_presenceChanged;

                if ((presenceChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { null, presenceChangedArgs });
                    else
                        handlerCopy(null, presenceChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the presence changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving PresenceChangedCallback().");
        }

        static EventHandler<ObjectChangedEventArgs> s_objectChanged;

        public static event EventHandler<ObjectChangedEventArgs> LocalObjectChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddObjectChangedEvent(EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                AddObjectChangedEvent(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemoveObjectChangedEvent(EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                RemoveObjectChangedEvent(value);
            }
        }

        #region Object changed event variables
        static object s_lockObjChangedEvent;
        static object LockObjChangedEvent
        {
            get{
                if (s_lockObjChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_lockObjChangedEvent, o, null);
                }
                return s_lockObjChangedEvent;
            }
        }

        static RegisteredWaitHandle s_regObjChangedWaitHandle;
        static AutoResetEvent s_objChangedEvent;
        static SafeCollabEvent s_safeObjChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: ObjectChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.AddMyObjectChanged(System.EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>,System.EventHandler`1<System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs>&,System.Object,System.Threading.RegisteredWaitHandle&,System.Threading.AutoResetEvent&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.WaitOrTimerCallback):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void AddObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            CollaborationHelperFunctions.AddMyObjectChanged(callback, ref s_objectChanged, LockObjChangedEvent,
                                                                    ref s_regObjChangedWaitHandle, ref s_objChangedEvent,
                                                                    ref s_safeObjChangedEvent, new WaitOrTimerCallback(ObjectChangedCallback));
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void RemoveObjectChangedEvent(EventHandler<ObjectChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() called.");
            lock (LockObjChangedEvent){
                s_objectChanged -= callback;
                if (s_objectChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref s_regObjChangedWaitHandle,
                                                                ref s_safeObjChangedEvent,
                                                                ref s_objChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ObjectChangedEvent variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveObjectChangedEvent() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: s_safeObjChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_OBJECTToPeerObject(System.Net.PeerToPeer.Collaboration.PEER_OBJECT):System.Net.PeerToPeer.Collaboration.PeerObject" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_ENDPOINTToPeerEndPoint(System.Net.PeerToPeer.Collaboration.PEER_ENDPOINT):System.Net.PeerToPeer.Collaboration.PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void ObjectChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ObjectChangedCallback() called.");

            while (true)
            {
                ObjectChangedEventArgs objChangedArgs = null;

                //
                // Get the event data for the fired event
                //
                try
                {
                    lock (LockObjChangedEvent){
                        if (s_safeObjChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(s_safeObjChangedEvent,
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

                EventHandler<ObjectChangedEventArgs> handlerCopy = s_objectChanged;

                if ((objChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { null, objChangedArgs });
                    else
                        handlerCopy(null, objChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the object changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ObjectChangedCallback().");
        }

        static event EventHandler<ApplicationChangedEventArgs> s_applicationChanged;

        public static event EventHandler<ApplicationChangedEventArgs> LocalApplicationChanged
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: AddApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            add
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                AddApplicationChanged(value);
            }
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: CollaborationHelperFunctions.Initialize():System.Void" Ring="1" />
            // <ReferencesCritical Name="Method: RemoveApplicationChanged(EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>):Void" Ring="2" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            remove
            {
                PeerCollaborationPermission.UnrestrictedPeerCollaborationPermission.Demand();
                CollaborationHelperFunctions.Initialize();

                RemoveApplicationChanged(value);
            }
        }

        #region Application changed event variables
        static object s_lockAppChangedEvent;
        static object LockAppChangedEvent
        {
            get{
                if (s_lockAppChangedEvent == null){
                    object o = new object();
                    Interlocked.CompareExchange(ref s_lockAppChangedEvent, o, null);
                }
                return s_lockAppChangedEvent;
            }
        }

        static RegisteredWaitHandle s_regAppChangedWaitHandle;
        static AutoResetEvent s_appChangedEvent;
        static SafeCollabEvent s_safeAppChangedEvent;
        #endregion

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: ApplicationChangedCallback(Object, Boolean):Void" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.AddMyApplicationChanged(System.EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>,System.EventHandler`1<System.Net.PeerToPeer.Collaboration.ApplicationChangedEventArgs>&,System.Object,System.Threading.RegisteredWaitHandle&,System.Threading.AutoResetEvent&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.WaitOrTimerCallback):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void AddApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            CollaborationHelperFunctions.AddMyApplicationChanged(callback, ref s_applicationChanged, LockAppChangedEvent,
                                                                    ref s_regAppChangedWaitHandle, ref s_appChangedEvent,
                                                                    ref s_safeAppChangedEvent, new WaitOrTimerCallback(ApplicationChangedCallback));
        }

        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Field: s_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.CleanEventVars(System.Threading.RegisteredWaitHandle&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&,System.Threading.AutoResetEvent&):System.Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void RemoveApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback)
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() called.");
            lock (LockAppChangedEvent){
                s_applicationChanged -= callback;
                if (s_applicationChanged == null){
                    CollaborationHelperFunctions.CleanEventVars(ref s_regAppChangedWaitHandle,
                                                                ref s_safeAppChangedEvent,
                                                                ref s_appChangedEvent);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Clean ApplicationChangEvent variables successful.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "RemoveApplicationChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabGetEventData(System.Net.PeerToPeer.Collaboration.SafeCollabEvent,System.Net.PeerToPeer.Collaboration.SafeCollabData&):System.Int32" />
        // <ReferencesCritical Name="Local eventData of type: SafeCollabData" Ring="1" />
        // <ReferencesCritical Name="Field: s_safeAppChangedEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: CollaborationHelperFunctions.ConvertPEER_APPLICATIONToPeerApplication(System.Net.PeerToPeer.Collaboration.PEER_APPLICATION):System.Net.PeerToPeer.Collaboration.PeerApplication" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static void ApplicationChangedCallback(object state, bool timedOut)
        {
            SafeCollabData eventData = null;
            int errorCode = 0;

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "ApplicationChangedCallback() called.");

            while (true){
                ApplicationChangedEventArgs appChangedArgs = null;
                
                //
                // Get the event data for the fired event
                //
                try{
                    lock (LockAppChangedEvent){
                        if (s_safeAppChangedEvent.IsInvalid) return;
                        errorCode = UnsafeCollabNativeMethods.PeerCollabGetEventData(s_safeAppChangedEvent,
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

                        if (appData.pEndPoint != IntPtr.Zero){
                            //
                            // This means its an endpoint on my contact which is not on the local machine
                            // so we dont care
                            //
                            return;
                        }

                        appChangedArgs = new ApplicationChangedEventArgs(   null,
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
                EventHandler<ApplicationChangedEventArgs> handlerCopy = s_applicationChanged;

                if ((appChangedArgs != null) && (handlerCopy != null)){
                    if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                        SynchronizingObject.BeginInvoke(handlerCopy, new object[] { null, appChangedArgs });
                    else
                        handlerCopy(null, appChangedArgs);
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Fired the application changed event callback.");
                }
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Leaving ApplicationChangedCallback().");
        }


    }
}
