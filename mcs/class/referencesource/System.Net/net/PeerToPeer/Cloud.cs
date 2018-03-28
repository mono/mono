//------------------------------------------------------------------------------
// <copyright file="Cloud.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    
    /// <remarks>
    /// The Cloud class directly represents the native cloud concept in the P2P APIs
    /// There are two special static readonly members we support
    /// Cloud.All and Cloud.AllLinkLocal
    /// Cloud.All is really a notational convinience of null in the native world
    /// Cloud.AllLinkLocal is equivalent to PEER_PNRP_ALL_LINK_CLOUDS const in the header file (declared in p2p.h)
    /// 
    /// This class is serializable.
    /// This class is not sealed because there is no reason for it to be sealed. 
    /// </remarks>
    /// <




    [Serializable]
    public class Cloud : ISerializable, IEquatable<Cloud>
    {
        private const string PEER_PNRP_ALL_LINK_CLOUDS = "PEER_PNRP_ALL_LINKS";

        private string m_CloudName; //name of the cloud
        private PnrpScope m_PnrpScope; //scope of the cloud
        private int m_ScopeId; //scope Id of the scope 

        /// <summary>
        /// Cloud.AllAvailable is a notational convinience. The native side uses a null for cloud parameter
        /// to indicate all clouds. 
        /// </summary>
        public static readonly Cloud Available = new Cloud("AllAvailable", PnrpScope.All, -1);

        /// <summary>
        /// AllLinkLocal is a managed abstraction of the native const PEER_PNRP_ALL_LINK_CLOUDS
        /// </summary>
        public static readonly Cloud AllLinkLocal = new Cloud("AllLinkLocal", PnrpScope.LinkLocal, -1);

        /// <summary>
        /// The static constructor serves the purpose of checking the 
        /// availability of the P2P apis on this platform
        /// </summary>
        static Cloud()
        {
            //-------------------------------------------------
            //Check for the availability of the simpler PNRP APIs
            //-------------------------------------------------
            if (!PeerToPeerOSHelper.SupportsP2P)
            {
                throw new PlatformNotSupportedException(SR.GetString(SR.P2P_NotAvailable));
            }
        }


        /// <summary>
        /// Constructs an instance of a Cloud. 
        /// This is not public and accessible for internal members only
        /// </summary>
        /// <param name="name">Name of the cloud</param>
        /// <param name="pnrpScope">scope</param>
        /// <param name="scopeId">id ofthe scope</param>
        internal Cloud(string name, PnrpScope pnrpScope, int scopeId) {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Creating cloud with Name: {0}, PnrpScope: {1}, ScopeID: {2}", name, pnrpScope, scopeId);
            m_CloudName = name;
            m_PnrpScope = pnrpScope;
            m_ScopeId = scopeId;
        }

        /// <summary>
        ///     Name 
        /// </summary>
        public string Name {
            get {
                if (this == Cloud.AllLinkLocal || this == Cloud.Available)
                    return null;
                return m_CloudName;
            }
        }

        internal string InternalName
        {
            get
            {
                if (this == Cloud.AllLinkLocal)
                    return  PEER_PNRP_ALL_LINK_CLOUDS;
                else if (this == Cloud.Available)
                    return null;
                return m_CloudName;
            }
        }

        /// <summary>
        ///     Scope
        /// </summary>
        public PnrpScope Scope {
            get {
                return m_PnrpScope;
            }
        }

        /// <summary>
        ///     ScopeId
        /// </summary>
        public int ScopeId {
            get {
                return m_ScopeId;
            }
        }

        public static Cloud Global
        {
            // <SecurityKernel Critical="True" Ring="1">
            // <ReferencesCritical Name="Method: GetCloudOrClouds(String, Boolean, CloudCollection&, Cloud&):Void" Ring="1" />
            // </SecurityKernel>
            //[System.Security.SecurityCritical]
            get
            {
                //throw new PeerToPeerException(SR.GetString(SR.Collab_SubscribeLocalContactFailed));
                CloudCollection dummy = null;
                Cloud cloud = null;
                GetCloudOrClouds(null, true, out dummy, out cloud);
                return cloud;
            }
        }


        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: GetCloudOrClouds(String, Boolean, CloudCollection&, Cloud&):Void" Ring="1" />
        // </SecurityKernel>
        //[System.Security.SecurityCritical]
        public static Cloud GetCloudByName(string cloudName)
        {
            if (cloudName == null || cloudName.Length == 0)
            {
                throw new ArgumentException(SR.GetString(SR.Pnrp_CloudNameCantBeNull), "cloudName");
            }
            CloudCollection dummy = null;
            Cloud cloud = null;
            GetCloudOrClouds(cloudName, false, out dummy, out cloud);
            return cloud;
        }

        /// <summary>
        /// The static member returns the list of clouds
        /// </summary>
        /// <returns></returns>
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: GetCloudOrClouds(String, Boolean, CloudCollection&, Cloud&):Void" Ring="1" />
        // </SecurityKernel>
        //[System.Security.SecurityCritical]
        public static CloudCollection GetAvailableClouds()
        {
            CloudCollection clouds = null;
            Cloud dummy = null;
            GetCloudOrClouds(null, false, out clouds, out dummy);
            return clouds;
        }

        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpGetCloudInfo(System.UInt32&,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStructure(System.IntPtr,System.Type):System.Object" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Marshal.SizeOf(System.Type):System.Int32" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Local ArrayOfCloudInfoStructures of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Method: UnsafeP2PNativeMethods.PnrpStartup():System.Void" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security","CA2129:SecurityTransparentCodeShouldNotReferenceNonpublicSecurityCriticalCode", Justification="System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecuritySafeCritical]
        private static void GetCloudOrClouds(string cloudName, bool bGlobalCloudOnly,  out CloudCollection clouds, out Cloud cloud)
        {
            cloud = null;
            clouds = null;

            Logging.Enter(Logging.P2PTraceSource, "Cloud::GetCloudOrClouds()");

            //-------------------------------------------------
            //Demand for the Unrestricted Pnrp Permission
            //-------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            Int32 result = 0;
            UInt32 numClouds = 0;
            SafePeerData ArrayOfCloudInfoStructures = null;
            if (cloudName == null)
            {
                //-----------------------------------------
                //We need the collection only when we are not 
                //getting a specific cloud
                //-----------------------------------------
                clouds = new CloudCollection();
            }
            try
            {
                //---------------------------------------------------------------
                //No perf hit here, real native call happens only one time if it 
                //did not already happen
                //---------------------------------------------------------------
                UnsafeP2PNativeMethods.PnrpStartup();

                result = UnsafeP2PNativeMethods.PeerPnrpGetCloudInfo(out numClouds, out ArrayOfCloudInfoStructures);
                if (result != 0)
                {
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotEnumerateClouds), result);
                }
                if (numClouds != 0)
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Number of clouds returned {0}", numClouds);
                    IntPtr pPEER_PNRP_CLOUD_INFO = ArrayOfCloudInfoStructures.DangerousGetHandle();
                    for (ulong i = 0; i < numClouds; i++)
                    {
                        PEER_PNRP_CLOUD_INFO cloudinfo = (PEER_PNRP_CLOUD_INFO)Marshal.PtrToStructure(pPEER_PNRP_CLOUD_INFO, typeof(PEER_PNRP_CLOUD_INFO));
                        string nativeCloudName = Marshal.PtrToStringUni(cloudinfo.pwzCloudName);
                        pPEER_PNRP_CLOUD_INFO = (IntPtr)((long)pPEER_PNRP_CLOUD_INFO + Marshal.SizeOf(typeof(PEER_PNRP_CLOUD_INFO)));
                        Cloud c = new Cloud(nativeCloudName, (PnrpScope)((int)cloudinfo.dwScope), (int)cloudinfo.dwScopeId);
                        if (cloudName == null && !bGlobalCloudOnly)
                        {
                            clouds.Add(c);
                            continue;
                        }
                        //If a specific cloud by name is required, then test for name
                        //note that scope is PnrpScope.All but we don't test that now
                        if (cloudName != null && cloudName == nativeCloudName)
                        {
                            cloud = c;
                            break;
                        }

                         if (bGlobalCloudOnly && c.Scope == PnrpScope.Global)
                        {
                            cloud = c;
                            break;
                        }
                    }
                }
                else
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0, "No Clouds returned from the native call");
                }
            }
            finally
            {
                if (ArrayOfCloudInfoStructures != null)
                {
                    ArrayOfCloudInfoStructures.Dispose();
                }
            }
            if (cloudName != null && cloud == null)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "The specific cloud name {0} asked for is not found", cloudName);
            }
            Logging.Leave(Logging.P2PTraceSource, "Cloud::GetCloudOrClouds()");
        }






        /// <summary>
        /// Two Clouds are equal only when all of the information matches
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(Cloud other)
        {
            if (other == null) return false;
            return other.Name == Name && other.Scope == Scope && other.ScopeId == ScopeId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Cloud other = obj as Cloud;
            if (other == null)
                return false;
            return Equals(other);
            
        }
        /// <summary>
        /// The hash code comes from just the cloud name - for no partular reason.
        /// This implementation seems sufficient - since the cloud names or typically
        /// unique
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_CloudName.GetHashCode();
        }

        /// <summary>
        /// A friendly string for the Cloud object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Cloud Name:");
            sb.Append(Name);
            sb.Append(" Scope:");
            sb.Append(Scope);
            sb.Append(" ScopeId:");
            sb.Append(ScopeId);
            return sb.ToString();
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected Cloud(SerializationInfo info, StreamingContext context)
        {
            m_CloudName = info.GetString("_CloudName");
            m_PnrpScope = (PnrpScope)info.GetValue("_CloudScope", typeof(PnrpScope));
            m_ScopeId = info.GetInt32("_CloudScopeId");
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
            //Name is tricky since it can be null for AllLinkLocal and Available clouds
            //but internally we represent them with non null strings 
            //so we should use the property here
            info.AddValue("_CloudName", Name);
            info.AddValue("_CloudScope", m_PnrpScope);
            info.AddValue("_CloudScopeId", m_ScopeId);
        }
    }
}
