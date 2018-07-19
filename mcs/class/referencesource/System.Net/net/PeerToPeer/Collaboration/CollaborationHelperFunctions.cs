//------------------------------------------------------------------------------
// <copyright file="CollaborationHelperFunctions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Security.Permissions;

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Net.Mail;
    using System.Security.Cryptography.X509Certificates;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// This class contains some of the common functions needed for peer
    /// collaboration
    /// </summary>
    internal static class CollaborationHelperFunctions
    {
        private static volatile bool s_Initialized;
        private static object s_LockInitialized = new object();
        private const short c_CollabVersion = 0x0001;

        //
        // Initialise windows collab. This has to be called before any collab operation
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabStartup(System.Int16):System.Int32" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void Initialize()
        {
            if (!s_Initialized){
                lock (s_LockInitialized){
                    if (!s_Initialized){
                        if(!PeerToPeerOSHelper.SupportsP2P)
                            throw new PlatformNotSupportedException(SR.GetString(SR.P2P_NotAvailable));
                        int errorCode = UnsafeCollabNativeMethods.PeerCollabStartup(c_CollabVersion);
                        if (errorCode != 0){
                            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabStartup returned with errorcode {0}", errorCode);
                            throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_StartupFailed), errorCode);
                        }
                        s_Initialized = true;
                    }
                }
            }
        }

        //
        // Converts Guid class to GUID structure that we can pass into native
        //
        internal static GUID ConvertGuidToGUID(Guid guid)
        {
            GUID newGuid = new GUID();

            if (guid != null){
                byte[] guidBytes = guid.ToByteArray();
                string guidString = guid.ToString();

                int startVal = 0;
                int endVal = guidString.IndexOf('-');
                newGuid.data1 = (uint)(Convert.ToUInt32(guidString.Substring(startVal, endVal - startVal), 16));
                startVal = endVal + 1;
                endVal = guidString.IndexOf('-', endVal + 1);
                newGuid.data2 = (ushort)(Convert.ToUInt16(guidString.Substring(startVal, endVal - startVal), 16));
                startVal = endVal + 1;
                endVal = guidString.IndexOf('-', endVal + 1);
                newGuid.data3 = (ushort)(Convert.ToUInt16(guidString.Substring(startVal, endVal - startVal), 16));
                newGuid.data4 = guidBytes[8];
                newGuid.data5 = guidBytes[9];
                newGuid.data6 = guidBytes[10];
                newGuid.data7 = guidBytes[11];
                newGuid.data8 = guidBytes[12];
                newGuid.data9 = guidBytes[13];
                newGuid.data10 = guidBytes[14];
                newGuid.data11 = guidBytes[15];
            }
            return newGuid;
        }

        //
        // Converts native GUID structure to managed Guid class
        //
        internal static Guid ConvertGUIDToGuid(GUID guid)
        {
            byte[] bytes = new byte[8];
            bytes[0] = guid.data4;
            bytes[1] = guid.data5;
            bytes[2] = guid.data6;
            bytes[3] = guid.data7;
            bytes[4] = guid.data8;
            bytes[5] = guid.data9;
            bytes[6] = guid.data10;
            bytes[7] = guid.data11;

            return new Guid((int)guid.data1, (short)guid.data2, (short)guid.data3, bytes);
        }

        //
        // Converts native PEER_CONTACT to PeerContact class
        //
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: ConvertPEER_CONTACTToPeerContact(PEER_CONTACT, Boolean):PeerContact" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerContact ConvertPEER_CONTACTToPeerContact(PEER_CONTACT pc)
        {
            return ConvertPEER_CONTACTToPeerContact(pc, false);
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="Marshal.GetLastWin32Error():System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="X509Store..ctor(System.IntPtr)" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.CertOpenStore(System.IntPtr,System.UInt32,System.IntPtr,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_DATA&):System.Net.PeerToPeer.Collaboration.SafeCertStore" />
        // <ReferencesCritical Name="Local certHandle of type: SafeCertStore" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: PeerContact..ctor()" Ring="2" />
        // <ReferencesCritical Name="Method: MyContact..ctor()" Ring="3" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerContact ConvertPEER_CONTACTToPeerContact(PEER_CONTACT pc, bool isMyContact)
        {
            PeerContact peerContact = (isMyContact ? new MyContact(): new PeerContact());
            peerContact.PeerName = new PeerName(pc.pwzPeerName);
            peerContact.DisplayName = pc.pwzDisplayName;
            peerContact.Nickname = pc.pwzNickname;
            peerContact.EmailAddress = (pc.pwzEmailAddress != null) ? new MailAddress(pc.pwzEmailAddress) : null;
            if(!isMyContact) 
                peerContact.SubscribeAllowed = pc.WatcherPermissions;
            peerContact.IsSubscribed = (isMyContact ? true : pc.fWatch);
            byte[] data = null;

            if (pc.credentials.cbData != 0){
                data = new byte[pc.credentials.cbData];
                Marshal.Copy(pc.credentials.pbData, data, 0, (int)pc.credentials.cbData);
            }

            if (data != null){

                SafeCertStore certHandle = UnsafeCollabNativeMethods.CertOpenStore(new IntPtr(/*CERT_STORE_PROV_PKCS7*/ 5),
                                                    0x00000001/*X509_ASN_ENCODING*/| 0x00010000/*PKCS_7_ASN_ENCODING*/,
                                                    IntPtr.Zero,
                                                    0x00000001/*CERT_STORE_NO_CRYPT_RELEASE_FLAG*/,
                                                    ref pc.credentials);

                if (certHandle == null || certHandle.IsInvalid){
                    int win32ErrorCode = Marshal.GetLastWin32Error();
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_CredentialsError), win32ErrorCode);
                }
                try{
                    X509Store certStore = new X509Store(certHandle.DangerousGetHandle());
                    peerContact.Credentials = new X509Certificate2(certStore.Certificates[0]);
                }
                finally{
                    if(certHandle != null) certHandle.Dispose();
                }
            }

            return peerContact;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="Marshal.GetLastWin32Error():System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="X509Store..ctor(System.IntPtr)" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.CertOpenStore(System.IntPtr,System.UInt32,System.IntPtr,System.UInt32,System.IntPtr):System.Net.PeerToPeer.Collaboration.SafeCertStore" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.CertSaveStore(System.Net.PeerToPeer.Collaboration.SafeCertStore,System.UInt32,System.UInt32,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_DATA&,System.UInt32):System.Boolean" />
        // <ReferencesCritical Name="Local certHandle of type: SafeCertStore" Ring="1" />
        // <ReferencesCritical Name="Parameter safeCredentials of type: SafeCollabMemory" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: SafeCollabMemory..ctor(System.Int32)" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PEER_CONTACT ConvertPeerContactToPEER_CONTACT(PeerContact peerContact, ref SafeCollabMemory safeCredentials)
        {
            PEER_CONTACT pc = new PEER_CONTACT();

            pc.pwzDisplayName = peerContact.DisplayName;
            pc.pwzEmailAddress = (peerContact.EmailAddress == null) ? null : peerContact.EmailAddress.ToString();
            pc.pwzNickname = peerContact.Nickname;
            pc.pwzPeerName = peerContact.PeerName.ToString();
            pc.fWatch = peerContact.IsSubscribed;
            pc.WatcherPermissions = peerContact.SubscribeAllowed;
            PEER_DATA pd = new PEER_DATA();

            if (peerContact.Credentials != null){
                SafeCertStore certHandle = UnsafeCollabNativeMethods.CertOpenStore(new IntPtr(/*CERT_STORE_PROV_MEMORY*/ 2),
                                    0,
                                    IntPtr.Zero,
                                    0x00002000/*CERT_STORE_CREATE_NEW_FLAG*/ | 0x00000001/*CERT_STORE_NO_CRYPT_RELEASE_FLAG*/,
                                    IntPtr.Zero);
                
                if (certHandle == null || certHandle.IsInvalid){
                    int win32ErrorCode = Marshal.GetLastWin32Error();
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_CredentialsError), win32ErrorCode);
                }
                
                try{
                    X509Store certStore = new X509Store(certHandle.DangerousGetHandle());
                    certStore.Add(peerContact.Credentials as X509Certificate2);
                    bool returnCode = UnsafeCollabNativeMethods.CertSaveStore(certHandle,
                                                     0x00000001/*X509_ASN_ENCODING*/| 0x00010000/*PKCS_7_ASN_ENCODING*/,
                                                     2 /*CERT_STORE_SAVE_AS_STORE*/,
                                                     2, /*CERT_STORE_SAVE_TO_MEMORY*/
                                                     ref pd,
                                                     0);
                    
                    if ((pd.cbData != 0) && (returnCode)){
                        safeCredentials = new SafeCollabMemory((int)pd.cbData);
                        pd.pbData = safeCredentials.DangerousGetHandle();
                        returnCode = UnsafeCollabNativeMethods.CertSaveStore(certHandle,
                                                     0x00000001/*X509_ASN_ENCODING*/| 0x00010000/*PKCS_7_ASN_ENCODING*/,
                                                     2 /*CERT_STORE_SAVE_AS_STORE*/,
                                                     2, /*CERT_STORE_SAVE_TO_MEMORY*/
                                                     ref pd,// Clean up memory from here;
                                                     0);

                    }
                    else{
                        pd.cbData = 0;
                        pd.pbData = IntPtr.Zero;
                    }
                }
                finally{
                    if (certHandle != null) certHandle.Dispose();
                }
            }
            else{
                pd.cbData = 0;
                pd.pbData = IntPtr.Zero;
            }
            pc.credentials = pd;

            return pc;

        }

        //
        // Converts address bytes to a SOCKADDR_IN6 that can be passed into
        // native
        //
        internal static void ByteArrayToSin6Addr(byte[] addrBytes, ref SOCKADDR_IN6 sin6)
        {
            sin6.sin6_addr0 = addrBytes[0];
            sin6.sin6_addr1 = addrBytes[1];
            sin6.sin6_addr2 = addrBytes[2];
            sin6.sin6_addr3 = addrBytes[3];
            sin6.sin6_addr4 = addrBytes[4];
            sin6.sin6_addr5 = addrBytes[5];
            sin6.sin6_addr6 = addrBytes[6];
            sin6.sin6_addr7 = addrBytes[7];
            sin6.sin6_addr8 = addrBytes[8];
            sin6.sin6_addr9 = addrBytes[9];
            sin6.sin6_addr10 = addrBytes[10];
            sin6.sin6_addr11 = addrBytes[11];
            sin6.sin6_addr12 = addrBytes[12];
            sin6.sin6_addr13 = addrBytes[13];
            sin6.sin6_addr14 = addrBytes[14];
            sin6.sin6_addr15 = addrBytes[15];
        }

        //
        // Converts native structure PEER_PEOPLE_NEAR_ME to managed PeerNearMe class
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <ReferencesCritical Name="Method: ConvertPEER_ENDPOINTToPeerEndPoint(PEER_ENDPOINT):PeerEndPoint" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerNearMe PEER_PEOPLE_NEAR_METoPeerNearMe(PEER_PEOPLE_NEAR_ME ppnm)
        {
            PeerNearMe peerNearMe = new PeerNearMe();
            peerNearMe.Id = CollaborationHelperFunctions.ConvertGUIDToGuid(ppnm.id);
            peerNearMe.Nickname = Marshal.PtrToStringUni(ppnm.pwzNickname); ;

            PEER_ENDPOINT pe = ppnm.endpoint;
            PeerEndPoint peerEP = ConvertPEER_ENDPOINTToPeerEndPoint(pe);
            peerNearMe.PeerEndPoints.Add(peerEP);
            
            return peerNearMe;
        }

        //
        // Converts native PEER_OBJECT structure into PeerObject class
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerObject ConvertPEER_OBJECTToPeerObject(PEER_OBJECT po)
        {
            byte[] data = null;

            if (po.data.cbData != 0){
                data = new byte[po.data.cbData];
                Marshal.Copy(po.data.pbData, data, 0, (int)po.data.cbData);
            }

            return new PeerObject(ConvertGUIDToGuid(po.guid), data, (PeerScope)po.dwPublicationScope);
        }

        //
        // Converts native PEER_APPLICATION structure into PeerApplication class
        //
 
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Marshal.Copy(System.IntPtr,System.Byte[],System.Int32,System.Int32):System.Void" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerApplication ConvertPEER_APPLICATIONToPeerApplication(PEER_APPLICATION pa)
        {
            byte[] data = null;

            if (pa.data.cbData != 0){
                data = new byte[pa.data.cbData];
                Marshal.Copy(pa.data.pbData, data, 0, (int)pa.data.cbData);
            }

            return new PeerApplication( ConvertGUIDToGuid(pa.guid),
                                        Marshal.PtrToStringUni(pa.pwzDescription),
                                        data,
                                        null, null, PeerScope.None);
        }

        //
        // Converts native PEER_ENDPOINT structure into PeerEndPoint class
        //
        
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerEndPoint ConvertPEER_ENDPOINTToPeerEndPoint(PEER_ENDPOINT pe)
        {

            byte[] addrBytes = new byte[]{  pe.peerAddress.sin6.sin6_addr0, pe.peerAddress.sin6.sin6_addr1, 
                                            pe.peerAddress.sin6.sin6_addr2, pe.peerAddress.sin6.sin6_addr3, 
                                            pe.peerAddress.sin6.sin6_addr4, pe.peerAddress.sin6.sin6_addr5,
                                            pe.peerAddress.sin6.sin6_addr6, pe.peerAddress.sin6.sin6_addr7, 
                                            pe.peerAddress.sin6.sin6_addr8, pe.peerAddress.sin6.sin6_addr9, 
                                            pe.peerAddress.sin6.sin6_addr10, pe.peerAddress.sin6.sin6_addr11, 
                                            pe.peerAddress.sin6.sin6_addr12, pe.peerAddress.sin6.sin6_addr13, 
                                            pe.peerAddress.sin6.sin6_addr14, pe.peerAddress.sin6.sin6_addr15};
            IPAddress IPAddr = new IPAddress(addrBytes, (long)pe.peerAddress.sin6.sin6_scope_id);
            ushort port;
            unchecked{
                port = (ushort)IPAddress.NetworkToHostOrder((short)pe.peerAddress.sin6.sin6_port);
            }
            IPEndPoint IPEndPt = new IPEndPoint(IPAddr, port);

            return new PeerEndPoint(IPEndPt, Marshal.PtrToStringUni(pe.pwzEndpointName));
        }

        //
        // Converts IPEndpoint class into native PEER_ADDRESS structure
        //
        internal static PEER_ADDRESS ConvertIPEndpointToPEER_ADDRESS(IPEndPoint endPoint)
        {
            PEER_ADDRESS pa = new PEER_ADDRESS();
            SOCKADDR_IN6 sin = new SOCKADDR_IN6();
            sin.sin6_family = (ushort)endPoint.AddressFamily;
            sin.sin6_flowinfo = 0; // 
            unchecked{
                sin.sin6_port = (ushort)IPAddress.HostToNetworkOrder((short)endPoint.Port);
            }
            sin.sin6_scope_id = (uint)endPoint.Address.ScopeId;
            CollaborationHelperFunctions.ByteArrayToSin6Addr(endPoint.Address.GetAddressBytes(), ref sin);
            pa.dwSize = 32;
            pa.sin6 = sin;
            return pa;
        }

        //
        // Cleans up the registered handle and the wait event. Called under lock from events.
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Parameter safeEvent of type: SafeCollabEvent" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal static void CleanEventVars(ref RegisteredWaitHandle waitHandle,
                                            ref SafeCollabEvent safeEvent,
                                            ref AutoResetEvent firedEvent)
        {
            if (waitHandle != null){
                waitHandle.Unregister(null);
                waitHandle = null;
            }

            if ((safeEvent != null) && (!safeEvent.IsInvalid)){
                safeEvent.Dispose();
            }

            if (firedEvent != null){
                firedEvent.Close();
                firedEvent = null;
            }
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Parameter safePresenceChangedEvent of type: SafeCollabEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void AddMyPresenceChanged(EventHandler<PresenceChangedEventArgs> callback,
                                                        ref EventHandler<PresenceChangedEventArgs> presenceChanged,
                                                        object lockPresenceChangedEvent,
                                                        ref RegisteredWaitHandle regPresenceChangedWaitHandle,
                                                        ref AutoResetEvent presenceChangedEvent,
                                                        ref SafeCollabEvent safePresenceChangedEvent,
                                                        WaitOrTimerCallback PresenceChangedCallback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (lockPresenceChangedEvent){
                if (presenceChanged == null){

                    presenceChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    regPresenceChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(presenceChangedEvent, //Event that triggers the callback
                                            PresenceChangedCallback, //callback to be called 
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
                                                                        presenceChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out safePresenceChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_PresenceChangedRegFailed), errorCode);
                    }
                }
                presenceChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddMyPresenceChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Parameter safeAppChangedEvent of type: SafeCollabEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void AddMyApplicationChanged(EventHandler<ApplicationChangedEventArgs> callback,
                                                ref EventHandler<ApplicationChangedEventArgs> applicationChanged,
                                                object lockAppChangedEvent,
                                                ref RegisteredWaitHandle regAppChangedWaitHandle,
                                                ref AutoResetEvent appChangedEvent,
                                                ref SafeCollabEvent safeAppChangedEvent,
                                                WaitOrTimerCallback ApplicationChangedCallback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (lockAppChangedEvent){
                if (applicationChanged == null){

                    appChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    regAppChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(appChangedEvent, //Event that triggers the callback
                                            ApplicationChangedCallback, //callback to be called 
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
                                                                        appChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out safeAppChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ApplicationChangedRegFailed), errorCode);
                    }
                }
                applicationChanged += callback;
            }
            
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddApplicationChanged() successful.");
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="WaitHandle.get_SafeWaitHandle():Microsoft.Win32.SafeHandles.SafeWaitHandle" />
        // <CallsSuppressUnmanagedCode Name="UnsafeCollabNativeMethods.PeerCollabRegisterEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle,System.UInt32,System.Net.PeerToPeer.Collaboration.PEER_COLLAB_EVENT_REGISTRATION&,System.Net.PeerToPeer.Collaboration.SafeCollabEvent&):System.Int32" />
        // <ReferencesCritical Name="Parameter safeObjChangedEvent of type: SafeCollabEvent" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void AddMyObjectChanged(EventHandler<ObjectChangedEventArgs> callback,
                                        ref EventHandler<ObjectChangedEventArgs> objectChanged,
                                        object lockObjChangedEvent,
                                        ref RegisteredWaitHandle regObjChangedWaitHandle,
                                        ref AutoResetEvent objChangedEvent,
                                        ref SafeCollabEvent safeObjChangedEvent,
                                        WaitOrTimerCallback ObjectChangedCallback)
        {
            //
            // Register a wait handle if one has not been registered already
            //
            lock (lockObjChangedEvent){
                if (objectChanged == null){

                    objChangedEvent = new AutoResetEvent(false);

                    //
                    // Register callback with a wait handle
                    //

                    regObjChangedWaitHandle = ThreadPool.RegisterWaitForSingleObject(objChangedEvent, //Event that triggers the callback
                                            ObjectChangedCallback, //callback to be called 
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
                                                                        objChangedEvent.SafeWaitHandle,
                                                                        1,
                                                                        ref pcer,
                                                                        out safeObjChangedEvent);
                    if (errorCode != 0){
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "PeerCollabRegisterEvent returned with errorcode {0}", errorCode);
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Collab_ObjectChangedRegFailed), errorCode);
                    }
                }
                objectChanged += callback;
            }

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "AddObjectChanged() successful.");

        }

        internal static void ThrowIfInvitationResponseInvalid(PeerInvitationResponse response)
        {
            // throw an exception if the response from the native API was PEER_INVITATION_RESPONSE_ERROR
            if (response.PeerInvitationResponseType < PeerInvitationResponseType.Declined ||
                response.PeerInvitationResponseType > PeerInvitationResponseType.Expired)
            {
                throw new PeerToPeerException(SR.GetString(SR.Collab_InviteFailed));
            }
        }
    }
}
