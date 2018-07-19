//------------------------------------------------------------------------------
// <copyright file="UnsafeCollabNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;
    using System.Security.Permissions;
    using System.Security.Cryptography.X509Certificates;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;
    using System.Collections.Generic;
    using System.Text;
    using System.Security;

    //
    // To manage any collaboration memory handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCollabData : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCollabData() : base(true) { }

        protected override bool ReleaseHandle()
        {
            if(!IsInvalid)
                UnsafeCollabNativeMethods.PeerFreeData(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    //
    // To manage any collaboration enumeration handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCollabEnum : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCollabEnum() : base(true) { }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                UnsafeCollabNativeMethods.PeerEndEnumeration(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    //
    // To manage any collaboration invite handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCollabInvite : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCollabInvite() : base(true) { }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                UnsafeCollabNativeMethods.PeerCollabCloseHandle(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    //
    // To manage any cert store handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCertStore : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCertStore() : base(true) { }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                UnsafeCollabNativeMethods.CertCloseStore(handle, 0);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }
    //
    // To manage any allocated memory handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCollabMemory : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool allocated;
        internal SafeCollabMemory() : base(true) { }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeCollabMemory(int cb)
            : base(true)
        {
            handle = Marshal.AllocHGlobal(cb);
            if (IntPtr.Equals(handle, IntPtr.Zero)){
                SetHandleAsInvalid();
                throw new PeerToPeerException(SR.GetString(SR.MemoryAllocFailed));
            }
            allocated = true;
        }

        protected override bool ReleaseHandle()
        {
            if (allocated && !IsInvalid)
                Marshal.FreeHGlobal(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    //
    // To manage any collaboration event handle
    //
    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeCollabEvent : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCollabEvent() : base(true) { }
        protected override bool ReleaseHandle()
        {
            UnsafeCollabNativeMethods.PeerCollabUnregisterEvent(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    //
    //
    // Definitions of structures used for passing data into native collaboration
    // functions
    //
    //

    /*
        typedef struct peer_presence_info_tag {
            PEER_PRESENCE_STATUS            status;
            PWSTR                           pwzDescriptiveText;
        } PEER_PRESENCE_INFO
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_PRESENCE_INFO
    {
        internal PeerPresenceStatus status;
        internal string descText;
    }
    
    // 
    /*
        typedef struct sockaddr_in6 {
            ADDRESS_FAMILY sin6_family; // AF_INET6.
            USHORT sin6_port;           // Transport level port number.
            ULONG  sin6_flowinfo;       // IPv6 flow information.
            IN6_ADDR sin6_addr;         // IPv6 address.
            union {
                ULONG sin6_scope_id;     // Set of interfaces for a scope.
                SCOPE_ID sin6_scope_struct; 
            };
        } SOCKADDR_IN6_LH
    */
    [StructLayout(LayoutKind.Sequential)]
    internal struct SOCKADDR_IN6
    {
        internal ushort sin6_family;
        internal ushort sin6_port;
        internal uint sin6_flowinfo;
        internal byte sin6_addr0;
        internal byte sin6_addr1;
        internal byte sin6_addr2;
        internal byte sin6_addr3; 
        internal byte sin6_addr4;
        internal byte sin6_addr5;
        internal byte sin6_addr6;
        internal byte sin6_addr7;
        internal byte sin6_addr8;
        internal byte sin6_addr9;
        internal byte sin6_addr10;
        internal byte sin6_addr11;
        internal byte sin6_addr12;
        internal byte sin6_addr13;
        internal byte sin6_addr14;
        internal byte sin6_addr15;
        internal uint sin6_scope_id;
    }

    /*
        typedef struct peer_address_tag {
            DWORD                   dwSize;
            SOCKADDR_IN6            sin6;
        } PEER_ADDRESS
    */

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PEER_ADDRESS
    {
        internal uint dwSize;
        internal SOCKADDR_IN6 sin6;
    }

    /* 
        typedef struct peer_endpoint_tag {
            PEER_ADDRESS                address;
            PWSTR                       pwzEndpointName;
        } PEER_ENDPOINT
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_ENDPOINT
    {
        internal PEER_ADDRESS peerAddress;
        internal IntPtr pwzEndpointName;
    }

    /*
        typedef struct peer_data_tag {
            ULONG cbData;
            PBYTE pbData;
        } PEER_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_DATA
    {
        internal UInt32 cbData;
        internal IntPtr pbData;
    }
    // for Guid
    /*    
        typedef struct _GUID {
            unsigned long  Data1;
            unsigned short Data2;
            unsigned short Data3;
            unsigned char  Data4[ 8 ];
        } GUID;
    */

    [StructLayout(LayoutKind.Sequential/*, Pack=1*/)]
    internal struct GUID
    {
        internal uint   data1;
        internal ushort data2;
        internal ushort data3;
        internal byte   data4;
        internal byte   data5;
        internal byte   data6;
        internal byte   data7;
        internal byte   data8;
        internal byte   data9;
        internal byte   data10;
        internal byte   data11;
    }

    /*
        typedef struct peer_object_tag {
            GUID            id;
            PEER_DATA       data;
            DWORD           dwPublicationScope;
        } PEER_OBJECT
    */
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_OBJECT
    {
        internal GUID guid;
        internal PEER_DATA data;
        internal uint dwPublicationScope;
    }

    /*
        typedef struct peer_application_tag {
        GUID            id;
        PEER_DATA       data;
        PWSTR           pwzDescription;
        } PEER_APPLICATION
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_APPLICATION
    {
        internal GUID guid;
        internal PEER_DATA data;
        internal IntPtr pwzDescription;
    }

    /*
        typedef struct peer_application_registration_info_tag {
        PEER_APPLICATION            application;
        PWSTR                       pwzApplicationToLaunch;
        PWSTR                       pwzApplicationArguments;
        DWORD                       dwPublicationScope;
        } PEER_APPLICATION_REGISTRATION_INFO
     */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_APPLICATION_REGISTRATION_INFO
    {
        internal PEER_APPLICATION application;
        internal string pwzApplicationToLaunch;
        internal string pwzApplicationArguments;
        internal uint dwPublicationScope;
    }

    /*
        typedef struct peer_contact_tag
        {
            PWSTR                               pwzPeerName;
            PWSTR                               pwzNickName;
            PWSTR                               pwzDisplayName;
            PWSTR                               pwzEmailAddress;
            BOOL                                fWatch;
            PEER_WATCH_PERMISSION               WatcherPermissions;
            PEER_DATA                           credentials;
        } PEER_CONTACT
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_CONTACT
    {
        internal string pwzPeerName;
        internal string pwzNickname;
        internal string pwzDisplayName;
        internal string pwzEmailAddress;
        internal bool fWatch;
        internal SubscriptionType WatcherPermissions;
        internal PEER_DATA credentials;
    }
    
    /*
        typedef struct peer_people_near_me_tag {
            PWSTR                       pwzNickName;
            PEER_ENDPOINT               endpoint;
            GUID                        id;
        } PEER_PEOPLE_NEAR_ME
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_PEOPLE_NEAR_ME
    {
        internal IntPtr pwzNickname;
        internal PEER_ENDPOINT endpoint;
        internal GUID id;
    }


    /*
        typedef struct peer_invitation_tag {
            GUID                applicationId;
            PEER_DATA           applicationData;
            PWSTR               pwzMessage;
        } PEER_INVITATION
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_INVITATION
    {
        internal GUID applicationId;
        internal PEER_DATA applicationData;
        internal string pwzMessage;
    }

    /*
        typedef struct peer_invitation_response_tag {
            PEER_INVITATION_RESPONSE_TYPE   action;
            PWSTR                           pwzMessage;
            HRESULT                         hrExtendedInfo;
        } PEER_INVITATION_RESPONSE
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_INVITATION_RESPONSE
    {
        internal PeerInvitationResponseType action;
        internal string pwzMessage;
        internal uint hrExtendedInfo;
    }

    /*
        typedef struct peer_app_launch_info_tag {
            PPEER_CONTACT               pContact;
            PPEER_ENDPOINT              pEndpoint;
            PPEER_INVITATION            pInvitation;
        } PEER_APP_LAUNCH_INFO
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_APP_LAUNCH_INFO
    {
        internal IntPtr pContact;
        internal IntPtr pEndpoint;
        internal IntPtr pInvitation;
    }

    /*
        typedef struct peer_collab_event_registration_tag {
            PEER_COLLAB_EVENT_TYPE     eventType;
            #ifdef MIDL_PASS
            [unique]
            #endif
            GUID                        * pInstance;
        } PEER_COLLAB_EVENT_REGISTRATION
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_COLLAB_EVENT_REGISTRATION
    {
        internal PeerCollabEventType eventType;
        internal IntPtr pInstance;
    }

    /*
        typedef struct peer_event_watchlist_changed_data_tag {
            PPEER_CONTACT           pContact;
            PEER_CHANGE_TYPE        changeType;
        } PEER_EVENT_WATCHLIST_CHANGED_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_WATCHLIST_CHANGED_DATA
    {
        internal IntPtr pContact;
        internal PeerChangeType changeType;
    }

    /*
        typedef struct peer_event_presence_changed_data_tag {
            PPEER_CONTACT           pContact;
            PPEER_ENDPOINT          pEndpoint;
            PEER_CHANGE_TYPE        changeType;
            PPEER_PRESENCE_INFO     pPresenceInfo;
        } PEER_EVENT_PRESENCE_CHANGED_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_PRESENCE_CHANGED_DATA
    {
        internal IntPtr pContact;
        internal IntPtr pEndPoint;
        internal PeerChangeType changeType;
        internal IntPtr pPresenceInfo;
    }

    /*
        typedef struct peer_event_application_changed_data_tag {
            PPEER_CONTACT           pContact;
            PPEER_ENDPOINT          pEndpoint;
            PEER_CHANGE_TYPE        changeType;
            PPEER_APPLICATION       pApplication;
        } PEER_EVENT_APPLICATION_CHANGED_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_APPLICATION_CHANGED_DATA
    {
        internal IntPtr pContact;
        internal IntPtr pEndPoint;
        internal PeerChangeType changeType;
        internal IntPtr pApplication;
    }

    /*
        typedef struct peer_event_object_changed_data_tag {
            PPEER_CONTACT           pContact;
            PPEER_ENDPOINT          pEndpoint;
            PEER_CHANGE_TYPE        changeType;
            PPEER_OBJECT            pObject;
        } PEER_EVENT_OBJECT_CHANGED_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_OBJECT_CHANGED_DATA
    {
        internal IntPtr pContact;
        internal IntPtr pEndPoint;
        internal PeerChangeType changeType;
        internal IntPtr pObject;
    }

    /*
        typedef struct peer_event_endpoint_changed_data_tag {
            PPEER_CONTACT           pContact;
            PPEER_ENDPOINT          pEndpoint;
        } PEER_EVENT_ENDPOINT_CHANGED_DATA
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_ENDPOINT_CHANGED_DATA
    {
        internal IntPtr pContact;
        internal IntPtr pEndPoint;
    }

    /*
        typedef struct peer_event_people_near_me_changed_data_tag {
            PEER_CHANGE_TYPE        changeType;
            PPEER_PEOPLE_NEAR_ME    pPeopleNearMe;
        } PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA, *PPEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA;
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA
    {
        internal PeerChangeType changeType;
        internal IntPtr pPeopleNearMe;
    }

    /*
        typedef struct peer_event_request_status_changed_data_tag {
            PPEER_ENDPOINT          pEndpoint;
            HRESULT                 hrChange;
        } PEER_EVENT_REQUEST_STATUS_CHANGED_DATA, *PPEER_EVENT_REQUEST_STATUS_CHANGED_DATA;
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_EVENT_REQUEST_STATUS_CHANGED_DATA
    {
        internal IntPtr pEndPoint;
        internal int hrChange;
    }

    /*
        typedef struct peer_collab_event_data_tag {
            PEER_COLLAB_EVENT_TYPE                     eventType;
            union {
                PEER_EVENT_WATCHLIST_CHANGED_DATA                   watchListChangedData;
                PEER_EVENT_PRESENCE_CHANGED_DATA                    presenceChangedData;
                PEER_EVENT_APPLICATION_CHANGED_DATA                 applicationChangedData;
                PEER_EVENT_OBJECT_CHANGED_DATA                      objectChangedData;
                PEER_EVENT_ENDPOINT_CHANGED_DATA                    endpointChangedData;
                PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA              peopleNearMeChangedData;
                PEER_EVENT_REQUEST_STATUS_CHANGED_DATA              requestStatusChangedData;
            };
        } PEER_COLLAB_EVENT_DATA, *PPEER_COLLAB_EVENT_DATA;
    */

    //
    // We have two different structures and one has explicit layout to be able to
    // handle the union as shown in the structure above. Two structures are used
    // instead of one because of x86 and x64 padding issues.
    //

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_COLLAB_EVENT_DATA
    {
        internal PeerCollabEventType eventType;
        PEER_COLLAB_EVENT_CHANGED_DATA changedData;

        internal PEER_EVENT_WATCHLIST_CHANGED_DATA watchListChangedData
        {
            get{
                return changedData.watchListChangedData;
            }
        }

        internal PEER_EVENT_PRESENCE_CHANGED_DATA presenceChangedData
        {
            get{
                return changedData.presenceChangedData;
            }
        }

        internal PEER_EVENT_APPLICATION_CHANGED_DATA applicationChangedData
        {
            get{
                return changedData.applicationChangedData;
            }
        }

        internal PEER_EVENT_OBJECT_CHANGED_DATA objectChangedData
        {
            get{
                return changedData.objectChangedData;
            }
        }

        internal PEER_EVENT_ENDPOINT_CHANGED_DATA endpointChangedData
        {
            get{
                return changedData.endpointChangedData;
            }
        }

        internal PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA peopleNearMeChangedData
        {
            get{
                return changedData.peopleNearMeChangedData;
            }
        }

        internal PEER_EVENT_REQUEST_STATUS_CHANGED_DATA requestStatusChangedData
        {
            get{
                return changedData.requestStatusChangedData;
            }
        } 
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    internal struct PEER_COLLAB_EVENT_CHANGED_DATA
    {
        [FieldOffset(0)]
        internal PEER_EVENT_WATCHLIST_CHANGED_DATA watchListChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_PRESENCE_CHANGED_DATA presenceChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_APPLICATION_CHANGED_DATA applicationChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_OBJECT_CHANGED_DATA objectChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_ENDPOINT_CHANGED_DATA endpointChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_PEOPLE_NEAR_ME_CHANGED_DATA peopleNearMeChangedData;
        [FieldOffset(0)]
        internal PEER_EVENT_REQUEST_STATUS_CHANGED_DATA requestStatusChangedData;

    }

    /// <summary>
    /// Stores specific error codes that we use.
    /// </summary>
    internal static class UnsafeCollabReturnCodes
    {
        private const UInt32 FACILITY_P2P = 99;
        private const UInt32 FACILITY_WIN32 = 7;
        internal const int PEER_S_NO_EVENT_DATA = (int)(((int)FACILITY_P2P << 16) | 0x0002);
        internal const int PEER_S_SUBSCRIPTION_EXISTS = (int)(((int)FACILITY_P2P << 16) | 0x6000);
        internal const int PEER_E_NOT_FOUND = (int)(((int)1 << 31) | ((int)FACILITY_WIN32 << 16) | 1168);
        internal const int PEER_E_CONTACT_NOT_FOUND = (int)(((int)1 << 31) | ((int)FACILITY_P2P << 16) | 0x6001);
        internal const int PEER_E_ALREADY_EXISTS = (int)(((int)1 << 31) | ((int)FACILITY_WIN32 << 16) | 183);
        internal const int PEER_E_TIMEOUT = (int)(((int)1 << 31) | ((int)FACILITY_P2P << 16) | 0x7005);
        internal const int ERROR_TIMEOUT = (int)(((int)1 << 31) | ((int)FACILITY_WIN32 << 16) | 0x05B4);
    }

    /// <summary>
    /// This class contains all the collab/windows native functions that are called
    /// by Collaboration namespace
    /// </summary>
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static class UnsafeCollabNativeMethods
    {
        private const string P2P = "p2p.dll";

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabStartup(short wVersionRequested);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabSignin(IntPtr hwndParent, PeerScope dwSignInOptions);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static void PeerFreeData(IntPtr dataToFree);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabSignout(PeerScope dwSignInOptions);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetSigninOptions(ref PeerScope dwSignInOptions);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabSetPresenceInfo(ref PEER_PRESENCE_INFO ppi);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetPresenceInfo(IntPtr endpoint, out SafeCollabData pPresenceInfo);

        
        //
        // Application registration functions
        //
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabRegisterApplication(ref PEER_APPLICATION_REGISTRATION_INFO appRegInfo,
                                                                    PeerApplicationRegistrationType appRegType);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabUnregisterApplication(ref GUID pApplicationId,
                                                                    PeerApplicationRegistrationType appRegType);
        //
        // Object set functions
        //
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabSetObject(ref PEER_OBJECT pcObject);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabDeleteObject(ref GUID pObjectId);

        //
        // Enumeration functions
        //
        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabEnumObjects(  IntPtr pcEndpoint,
                                                            IntPtr pObjectId,
                                                            out SafeCollabEnum phPeerEnum);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabEnumApplications( IntPtr pcEndpoint,
                                                                IntPtr pObjectId,
                                                                out SafeCollabEnum phPeerEnum);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabEnumPeopleNearMe(out SafeCollabEnum phPeerEnum);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabEnumEndpoints(ref PEER_CONTACT pcContact,
                                                out SafeCollabEnum phPeerEnum);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabEnumContacts(out SafeCollabEnum phPeerEnum);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerGetItemCount(SafeCollabEnum hPeerEnum, ref UInt32 pCount);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerGetNextItem(SafeCollabEnum hPeerEnum,
                                                    ref UInt32 pCount,
                                                    out SafeCollabData pppvItems);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerEndEnumeration(IntPtr hPeerEnum);

        //
        // Misc application functions 
        //
        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetAppLaunchInfo(out SafeCollabData ppLaunchInfo);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetApplicationRegistrationInfo(ref GUID pApplicationId,
                                                PeerApplicationRegistrationType registrationType,
                                                out SafeCollabData ppApplication);

        //
        // Contact functions
        //
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabExportContact(string pwzPeerNAme, ref string ppwzContactData);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabParseContact(string pwzContactData, out SafeCollabData ppContactData);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetContact(string pwzPeerName, out SafeCollabData ppwzContactData);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabQueryContactData(IntPtr pcEndpoint, ref string ppwzContactData);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabAddContact(string pwzContactData, out SafeCollabData ppContact);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabDeleteContact(string pwzPeerName);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabUpdateContact(ref PEER_CONTACT pc);
        
        //
        // Endpoint functions
        //
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabRefreshEndpointData(IntPtr pcEndpoint);

        //
        // Event functions
        //
        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabRegisterEvent(SafeWaitHandle hEvent, UInt32 cEventRegistration,
                                                            ref PEER_COLLAB_EVENT_REGISTRATION pEventRegistrations,
                                                            out SafeCollabEvent phPeerEvent);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetEventData(SafeCollabEvent hPeerEvent,
                                                            out SafeCollabData ppEventData);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabUnregisterEvent(IntPtr handle);


        //

        private const string CRYPT32 = "crypt32.dll";

        //
        // Certificate functions
        //
        [System.Security.SecurityCritical]
        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static SafeCertStore CertOpenStore(IntPtr lpszStoreProvider, uint dwMsgAndCertEncodingType, 
                                                            IntPtr hCryptProv, uint dwFlags, ref PEER_DATA pvPara);

        [System.Security.SecurityCritical]
        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static SafeCertStore CertOpenStore(IntPtr lpszStoreProvider, uint dwMsgAndCertEncodingType, 
                                                            IntPtr hCryptProv, uint dwFlags, IntPtr pvPara);

        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal extern static bool CertCloseStore(IntPtr hCertStore, uint dwFlags);

        [System.Security.SecurityCritical]
        [DllImport(CRYPT32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal extern static bool CertSaveStore(  SafeCertStore hCertStore, uint dwMsgAndCertEncodingType,
                                                    uint dwSaveAs, uint dwSaveTo, ref PEER_DATA pvSafeToPara, uint dwFlags);

        //
        // My Contact functions
        //
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetEndpointName(ref string ppwzEndpointName);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabSetEndpointName(string pwzEndpointName);

        //
        // Invitation functions
        //
        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabGetInvitationResponse(SafeCollabInvite hInvitation,
                                                                    out SafeCollabData ppInvitationResponse);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabCancelInvitation(SafeCollabInvite hInvitation);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabCloseHandle(IntPtr hInvitation);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabInviteContact( ref PEER_CONTACT pcContact,
                                                            IntPtr pcEndpoint,
                                                            ref PEER_INVITATION pcInvitation,
                                                            out SafeCollabData ppResponse);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabAsyncInviteContact(ref PEER_CONTACT pcContact,
                                                                IntPtr pcEndpoint,
                                                                ref PEER_INVITATION pcInvitation,
                                                                SafeWaitHandle hEvent,
                                                                out SafeCollabInvite phInvitation);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabInviteEndpoint(    IntPtr pcEndpoint,
                                                                ref PEER_INVITATION pcInvitation,
                                                                out SafeCollabData ppResponse);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static int PeerCollabAsyncInviteEndpoint(  IntPtr pcEndpoint,
                                                                    ref PEER_INVITATION pcInvitation,
                                                                    SafeWaitHandle hEvent,
                                                                    out SafeCollabInvite phInvitation);

    }
}
