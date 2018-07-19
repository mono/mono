//------------------------------------------------------------------------------
// <copyright file="PeerNameRegistration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
   

    /// <remarks>
    /// The PeerNameRegistration class registers a peer name record 
    /// </remarks>
    [Serializable]
    public class PeerNameRegistration : IDisposable, ISerializable
    {
        //-----------------------------------------------------------------------
        //Native constant to indicate auto address selection
        //-----------------------------------------------------------------------
        private const UInt32 PEER_PNRP_AUTO_ADDRESSES  =  unchecked((UInt32)(-1));


        //-----------------------------------------------------------------------
        //Internal PeerNameRecord to hold information. 
        //-----------------------------------------------------------------------
        private PeerNameRecord m_PeerNameRecord = new PeerNameRecord();
        private int m_Port;
        private Cloud m_Cloud;

        //-----------------------------------------------------------------------
        //Flag to keep whether We registered or not
        //-----------------------------------------------------------------------
        private bool m_IsRegistered;

        //-----------------------------------------------------------------------
        //The native handle to the registration
        //-----------------------------------------------------------------------
        private SafePeerNameUnregister m_RegistrationHandle;

        //-----------------------------------------------------------------------
        //PeerName that is associated with this registation
        //If they update the PeerName in the PeerNameRecord and call update we
        //should throw
        //-----------------------------------------------------------------------
        private PeerName m_RegisteredPeerName;

        //-----------------------------------------------------------------------
        //We should support the scenario where you publish just the data 
        //but no end points. This flag tells us whether to use auto endpoint selection
        //or not
        //-----------------------------------------------------------------------
        private bool m_UseAutoEndPointSelection = true;

        static PeerNameRegistration()
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
        /// Empty constructor so that users can populate the 
        /// information later
        /// </summary>
        public PeerNameRegistration()
        {
        }

        /// <summary>
        /// This constuctor popuates the PeerNameRecord and registers automatically
        /// Registers in all clouds with automatic address selection
        /// </summary>
        /// <param name="name">PeerName to register</param>
        /// <param name="port">Port to register on</param>
        public PeerNameRegistration(PeerName name, int port) : this(name, port, null)
        {
        }
        /// <summary>
        /// This constuctor popuates the PeerNameRecord and registers automatically
        /// Registers with automatic address selection within the cloud
        /// </summary>
        /// <param name="name">PeerName to register</param>
        /// <param name="port">Port to register on</param>
        /// <param name="cloud">A specific cloud to regster in</param>/// 
        public PeerNameRegistration(PeerName name, int port, Cloud cloud)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (cloud == null)
            {
                cloud = Cloud.Available;
            }
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", SR.GetString(SR.Pnrp_PortOutOfRange));
            }


            m_PeerNameRecord.PeerName = name;
            m_Port = port;
            m_Cloud = cloud;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Created a PeerNameRegistration with PeerName {0}, Port {1}, Cloud {2} - Proceeding to register", name, port, cloud);
        }

        /// <summary>
        /// Property accessor to examine/change and perhaps call 
        /// Update() to update the registration information
        /// </summary>
        internal PeerNameRecord PeerNameRecord
        {
            get
            {
                if (m_Disposed)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                return m_PeerNameRecord;
            }
        }

        public int Port
        {
            get
            {
                return m_Port;
            }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw new ArgumentOutOfRangeException("port", SR.GetString(SR.Pnrp_PortOutOfRange));
                }

                m_Port = value;
            }
        }


        public PeerName PeerName
        {
            get
            {
                return m_PeerNameRecord.PeerName;
            }
            set
            {

                m_PeerNameRecord.PeerName = value;

            }
        }

        public IPEndPointCollection EndPointCollection
        {
            get
            {
                return m_PeerNameRecord.EndPointCollection;
            }
        }

        public Cloud Cloud
        {
            get
            {
                return m_Cloud;
            }
            set
            {
                m_Cloud = value;
            }
        }

        public string Comment
        {
            get
            {
                return m_PeerNameRecord.Comment;
            }
            set
            {
                m_PeerNameRecord.Comment = value;
            }
        }
        public byte[] Data
        {
            get
            {
                return m_PeerNameRecord.Data;
            }
            set
            {
                m_PeerNameRecord.Data = value;
            }
        }

        public bool UseAutoEndPointSelection
        {
            get
            {
                return m_UseAutoEndPointSelection;
            }
            set
            {
                m_UseAutoEndPointSelection = value;
            }
        }

        /// <summary>
        /// Return the flag to indicate whether we 
        /// registered or not
        /// </summary>
        /// <returns></returns>
        public bool IsRegistered()
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            //-------------------------------------------------
            //Demand for the Unrestricted Pnrp Permission
            //-------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            return m_IsRegistered;
        }

        /// <summary>
        /// This method is called if empty constructor is used and the 
        /// information in the PeerNameRecord is set. Register needs to 
        /// be called since we did not automatically register through the 
        /// constructor
        /// </summary>
        // <SecurityKernel Critical="True" Ring="1">
        // <ReferencesCritical Name="Method: InternalRegister():Void" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Start()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Proceeding to register through the Register method()");
            InternalRegister();
        }

        /// <summary>
        /// This is where the real registration happens
        /// </summary>
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpRegister(System.String,System.Net.PeerToPeer.PEER_PNRP_REGISTRATION_INFO&,System.Net.PeerToPeer.SafePeerNameUnregister&):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.SizeOf(System.Type):System.Int32" />
        // <SatisfiesLinkDemand Name="Marshal.AllocHGlobal(System.Int32):System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.FreeHGlobal(System.IntPtr):System.Void" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <UsesUnsafeCode Name="Local pAddress of type: IntPtr*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <ReferencesCritical Name="Method: UnsafeP2PNativeMethods.PnrpStartup():System.Void" Ring="1" />
        // <ReferencesCritical Name="Field: m_RegistrationHandle" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        private void InternalRegister()
        {
            //------------------------------------------
            //If we already registered, get out this place
            //------------------------------------------
            if (m_IsRegistered)
            {
                throw new PeerToPeerException(SR.GetString(SR.Pnrp_UseUpdateInsteadOfRegister));
            }

            //------------------------------------------
            //Make sure you have the required info
            //------------------------------------------
            if (m_PeerNameRecord.PeerName == null)
            {
                throw new ArgumentNullException("PeerName");
            }

            //------------------------------------------
            //If auto address selection is turned off
            //then there must be atleast Data or Endpoints
            //specified 
            //------------------------------------------
            if (!m_UseAutoEndPointSelection)
            {
                if ((EndPointCollection.Count == 0) &&
                     (Data == null || Data.Length <= 0))
                {
                    throw new PeerToPeerException(SR.GetString(SR.Pnrp_BlobOrEndpointListNeeded));
                }
            }


            //-------------------------------------------------
            //Demand for the Unrestricted Pnrp Permission
            //-------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            //---------------------------------------------------------------
            //No perf hit here, real native call happens only one time if it 
            //did not already happen
            //---------------------------------------------------------------
            UnsafeP2PNativeMethods.PnrpStartup();

            //---------------------------------------------------------------
            //Log trace info
            //---------------------------------------------------------------
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information))
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "InternalRegister() is called with the following Info");
                m_PeerNameRecord.TracePeerNameRecord();
            }

            PEER_PNRP_REGISTRATION_INFO regInfo = new PEER_PNRP_REGISTRATION_INFO();
            GCHandle handle;
            //-------------------------------------------------
            //Set data
            //-------------------------------------------------
            if (m_PeerNameRecord.Data != null)
            {
                regInfo.payLoad.cbPayload = (UInt32)m_PeerNameRecord.Data.Length;
                handle = GCHandle.Alloc(m_PeerNameRecord.Data, GCHandleType.Pinned);
                regInfo.payLoad.pbPayload = handle.AddrOfPinnedObject(); //m_PeerNameRecord.Data;
            }
            else
            {
                handle = new GCHandle();
            }
            //-------------------------------------------------
            //Set comment
            //-------------------------------------------------
            if (m_PeerNameRecord.Comment != null && m_PeerNameRecord.Comment.Length > 0)
            {
                regInfo.pwszComment = m_PeerNameRecord.Comment;
            }
            //-------------------------------------------------
            //Set cloud name
            //-------------------------------------------------
            if (m_Cloud != null)
            {
                regInfo.pwszCloudName = m_Cloud.InternalName;
            }
            try
            {
                if (m_PeerNameRecord.EndPointCollection.Count == 0)
                {
                    //-------------------------------------------------
                    //Set port only if the addresses are null
                    //and then set the selection to auto addresses
                    //-------------------------------------------------
                    regInfo.wport = (ushort)m_Port;

                    if(m_UseAutoEndPointSelection)
                        regInfo.cAddresses = PEER_PNRP_AUTO_ADDRESSES;

                    //-------------------------------------------------
                    //Call the native API to register
                    //-------------------------------------------------
                    int result = UnsafeP2PNativeMethods.PeerPnrpRegister(m_PeerNameRecord.PeerName.ToString(),
                                        ref regInfo,
                                        out m_RegistrationHandle);
                    if (result != 0)
                    {
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotRegisterPeerName), result);
                    }
                }
                else
                {
                    //-------------------------------------------------
                    //Set the Endpoint List
                    //-------------------------------------------------
                    int numAddresses = m_PeerNameRecord.EndPointCollection.Count;
                    int cbRequriedBytes = numAddresses * Marshal.SizeOf(typeof(IntPtr));
                    IntPtr pSocketAddrList = Marshal.AllocHGlobal(cbRequriedBytes);
                    GCHandle[] GCHandles = new GCHandle[numAddresses];
                    try
                    {
                        unsafe
                        {
                            IntPtr* pAddress = (IntPtr*)pSocketAddrList;
                            for (int i = 0; i < m_PeerNameRecord.EndPointCollection.Count; i++)
                            {
                                byte[] sockaddr = SystemNetHelpers.SOCKADDRFromIPEndPoint(m_PeerNameRecord.EndPointCollection[i]);
                                GCHandles[i] = GCHandle.Alloc(sockaddr, GCHandleType.Pinned);
                                IntPtr psockAddr = GCHandles[i].AddrOfPinnedObject();
                                pAddress[i] = psockAddr;
                            }
                        }
                        regInfo.ArrayOfSOCKADDRIN6Pointers = pSocketAddrList;
                        regInfo.cAddresses = (UInt32)numAddresses;
                        int result = UnsafeP2PNativeMethods.PeerPnrpRegister(m_PeerNameRecord.PeerName.ToString(),
                                            ref regInfo,
                                            out m_RegistrationHandle);
                        if (result != 0)
                        {
                            throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotRegisterPeerName), result);
                        }
                    }
                    finally
                    {
                        if (pSocketAddrList != IntPtr.Zero)
                            Marshal.FreeHGlobal(pSocketAddrList);

                        for (int i = 0; i < GCHandles.Length; i++)
                        {
                            if (GCHandles[i].IsAllocated)
                                GCHandles[i].Free();
                        }
                    }
                }
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }

            }
            m_RegisteredPeerName = m_PeerNameRecord.PeerName;
            m_IsRegistered = true;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Registration is successful. The handle is {0}", m_RegistrationHandle.DangerousGetHandle());
        }

        /// <summary>
        /// Update is called if an existing registration needs to be updated
        /// </summary>
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerPnrpUpdateRegistration(System.Net.PeerToPeer.SafePeerNameUnregister,System.Net.PeerToPeer.PEER_PNRP_REGISTRATION_INFO&):System.Int32" />
        // <SatisfiesLinkDemand Name="GCHandle.Alloc(System.Object,System.Runtime.InteropServices.GCHandleType):System.Runtime.InteropServices.GCHandle" />
        // <SatisfiesLinkDemand Name="GCHandle.AddrOfPinnedObject():System.IntPtr" />
        // <SatisfiesLinkDemand Name="GCHandle.Free():System.Void" />
        // <SatisfiesLinkDemand Name="Marshal.SizeOf(System.Type):System.Int32" />
        // <SatisfiesLinkDemand Name="Marshal.AllocHGlobal(System.Int32):System.IntPtr" />
        // <SatisfiesLinkDemand Name="Marshal.FreeHGlobal(System.IntPtr):System.Void" />
        // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
        // <UsesUnsafeCode Name="Local pAddress of type: IntPtr*" />
        // <UsesUnsafeCode Name="Method: IntPtr.op_Explicit(System.IntPtr):System.Void*" />
        // <ReferencesCritical Name="Field: m_RegistrationHandle" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Update()
        {
            //-------------------------------------------------
            //Check for the dead object
            //-------------------------------------------------
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            //-------------------------------------------------
            //If there is no existing registration to update
            //get out
            //-------------------------------------------------
            if (!IsRegistered())
            {
                throw new InvalidOperationException(SR.GetString(SR.Pnrp_CallRegisterBeforeUpdate));
            }

            //-------------------------------------------------
            //Check for parameters
            //-------------------------------------------------
            if (m_PeerNameRecord.PeerName == null)
            {
                throw new ArgumentNullException(SR.GetString(SR.Pnrp_InvalidPeerName));
            }

            //-------------------------------------------------
            //If the current PeerName associated with the 
            //current registration is not the same as the 
            //PeerName given now - throw
            //-------------------------------------------------
            if (!m_RegisteredPeerName.Equals(m_PeerNameRecord.PeerName))
            {
                throw new InvalidOperationException(SR.GetString(SR.Pnrp_CantChangePeerNameAfterRegistration));
            }

            //------------------------------------------
            //If auto address selection is turned off
            //then there must be atleast Data or Endpoints
            //specified 
            //------------------------------------------
            if (!m_UseAutoEndPointSelection)
            {
                if ((EndPointCollection.Count == 0) ||
                     (Data == null || Data.Length <= 0))
                {
                    throw new PeerToPeerException(SR.GetString(SR.Pnrp_BlobOrEndpointListNeeded));
                }
            }

            //-------------------------------------------------
            //Demand for the Unrestricted Pnrp Permission
            //-------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            //---------------------------------------------------------------
            //Log trace info
            //---------------------------------------------------------------
            if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Information))
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Update() is called with the following Info");
                m_PeerNameRecord.TracePeerNameRecord();
            }

            PEER_PNRP_REGISTRATION_INFO regInfo = new PEER_PNRP_REGISTRATION_INFO();
            //-------------------------------------------------
            //Set data
            //-------------------------------------------------
            if (m_PeerNameRecord.Data != null)
            {
                regInfo.payLoad.cbPayload = (UInt32)m_PeerNameRecord.Data.Length;
                //Marshal.All
                //regInfo.payLoad.pbPayload = m_PeerNameRecord.Data;
                GCHandle handle = GCHandle.Alloc(m_PeerNameRecord.Data, GCHandleType.Pinned);
                regInfo.payLoad.pbPayload = handle.AddrOfPinnedObject(); //m_PeerNameRecord.Data;
                handle.Free();
            };
            //-------------------------------------------------
            //Set comment
            //-------------------------------------------------
            if (m_PeerNameRecord.Comment != null && m_PeerNameRecord.Comment.Length > 0)
            {
                regInfo.pwszComment = m_PeerNameRecord.Comment;
            }
            //-------------------------------------------------
            //Set cloud name
            //-------------------------------------------------
            regInfo.pwszCloudName = null;
            if (m_Cloud != null)
            {
                regInfo.pwszCloudName = m_Cloud.InternalName;
            }

            if (m_PeerNameRecord.EndPointCollection.Count == 0)
            {
                //-------------------------------------------------
                //Set port only if the addresses are null
                //and then set the selection to auto addresses
                //-------------------------------------------------
                regInfo.wport = (ushort)m_Port;
                regInfo.cAddresses = PEER_PNRP_AUTO_ADDRESSES;

                int result = UnsafeP2PNativeMethods.PeerPnrpUpdateRegistration(m_RegistrationHandle, ref regInfo);
                if (result != 0)
                {
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotRegisterPeerName), result);
                }
                return;
            }
            else
            {
                //-------------------------------------------------
                //Set the Endpoint List
                //-------------------------------------------------
                int numAddresses = m_PeerNameRecord.EndPointCollection.Count;
                int cbRequriedBytes = numAddresses * Marshal.SizeOf(typeof(IntPtr));
                IntPtr pSocketAddrList = Marshal.AllocHGlobal(cbRequriedBytes);
                GCHandle[] GCHandles = new GCHandle[numAddresses];
                try
                {
                    unsafe
                    {
                        IntPtr* pAddress = (IntPtr*)pSocketAddrList;
                        for (int i = 0; i < m_PeerNameRecord.EndPointCollection.Count; i++)
                        {
                            byte[] sockaddr = SystemNetHelpers.SOCKADDRFromIPEndPoint(m_PeerNameRecord.EndPointCollection[i]);
                            GCHandles[i] = GCHandle.Alloc(sockaddr, GCHandleType.Pinned);
                            IntPtr psockAddr = GCHandles[i].AddrOfPinnedObject();
                            pAddress[i] = psockAddr;
                        }
                    }
                    regInfo.ArrayOfSOCKADDRIN6Pointers = pSocketAddrList;
                    regInfo.cAddresses = (UInt32)numAddresses;
                    int result = UnsafeP2PNativeMethods.PeerPnrpUpdateRegistration(m_RegistrationHandle, ref regInfo);
                    if (result != 0)
                    {
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotRegisterPeerName), result);
                    }
                }
                finally
                {
                    if (pSocketAddrList != IntPtr.Zero)
                        Marshal.FreeHGlobal(pSocketAddrList);

                    for (int i = 0; i < GCHandles.Length; i++)
                    {
                        if (GCHandles[i].IsAllocated)
                            GCHandles[i].Free();
                    }
                }

                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Update of existing registration is successful. The handle is {0}", m_RegistrationHandle.DangerousGetHandle());
            }

        }

        /// <summary>
        /// Unregister the existing registration. 
        /// </summary>
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsClosed():System.Boolean" />
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <ReferencesCritical Name="Field: m_RegistrationHandle" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public void Stop()
        {
            if (m_Disposed) throw new ObjectDisposedException(this.GetType().FullName);

            //-------------------------------------------------
            //No registration happened previously - throw
            //-------------------------------------------------
			if(m_RegistrationHandle.IsInvalid || m_RegistrationHandle.IsClosed)
			{
				throw new InvalidOperationException(SR.GetString(SR.Pnrp_NoRegistrationFound));
			}

            //-------------------------------------------------
            //Demand for the Unrestricted Pnrp Permission
            //-------------------------------------------------
            PnrpPermission.UnrestrictedPnrpPermission.Demand();

            m_RegistrationHandle.Dispose();

			m_PeerNameRecord = new PeerNameRecord();

            m_RegisteredPeerName = null;
			
            m_IsRegistered = false;
        }

        private bool m_Disposed;
        /// <summary>
        /// Dispose explicit
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose impl
        /// </summary>
        /// <param name="disposing"> Whether we are disposing(true) or the system is disposing (false)</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                try
                {
                    Stop();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                //rest throw since we don't expect any other exceptions
            }
            m_Disposed = true;
        }

        
        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerNameRegistration(SerializationInfo info, StreamingContext context)
        {
            m_Port = info.GetInt32("_Port");
            m_UseAutoEndPointSelection = info.GetBoolean("_UseAutoEndPointSelection");
            m_Cloud = info.GetValue("_Cloud", typeof(Cloud)) as Cloud;
            m_PeerNameRecord = info.GetValue("_PeerNameRecord", typeof(PeerNameRecord)) as PeerNameRecord;
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
            info.AddValue("_Port", m_Port);
            info.AddValue("_UseAutoEndPointSelection", m_UseAutoEndPointSelection);
            info.AddValue("_Cloud", m_Cloud);
            info.AddValue("_PeerNameRecord", m_PeerNameRecord);
        }
        

    }
}



