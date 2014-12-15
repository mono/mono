//------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <remarks>
    /// The PeerToPeerException class encpasulates the exceptions for
    /// PeerToPeer classes. 
    /// NOTE:
    /// This class is marked serializable but does not implement 
    /// ISerializable interface. There are no private/public properties
    /// we keep track across the serialization. The base class message
    /// and inner exceptions are used. 
    /// </remarks>
    [Serializable]
    public class PeerToPeerException : Exception, ISerializable
    {
        private const UInt32 FACILITY_P2P = 99;


        public PeerToPeerException() { }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="message"></param>
        public PeerToPeerException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public PeerToPeerException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// HRESULT Structure
        ///   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
        ///   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        ///  +---+-+-+-----------------------+-------------------------------+
        ///  |Sev|C|R|     Facility          |               Code            |
        ///  +---+-+-+-----------------------+-------------------------------+
        ///
        ///  The intent here is that when we get a HRESULT from a P2P.dll, 
        ///  we need to get the message from the P2p.dll resource table. 
        ///  If the HRESULT is something like E_INVALIDARG, we need to get the 
        ///  message from the SYSTEM table. 
        ///  Apart from the native exception message, we would like to provide
        ///  a friendly message to explain the context under which the exception 
        ///  occurred from managed code developers. 
        ///  So we first try to get the message from the P2P.dll if the HRESULT 
        ///  comes with a facility ID for P2P. Otherwise we get the exception message from 
        ///  system. We then construct a Win32Exception and set this as an inner exception
        ///
        ///  If in case we can't get the message from either system or P2p, then 
        ///  we try the Marshal class and throw a exception from the HRESULT
        ///
        ///  If all else fails we simply throw an exception with no inner 
        ///  exception but still give the HRESULT
        ///
        ///  A note that we are getting the handle for P2p.dll from the LoadLibrary
        ///  we originally did for checking if P2P.dll is present on the system. 
        ///  Since we are getting the underlying handle, there is a possibility that 
        /// the Library is freed [AppDomain shutdown] and we are trying to 
        ///  use the handle. The code is in a try catch block here so that 
        ///  we catch these situations and still recover.
        /// </summary>
        /// <param name="message">The error message that we would like to set as the message for the exception</param>/// 
        /// <param name="hr">The error code</param>
        /// <returns>a PeerToPeerException</returns>
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeSystemNativeMethods.FormatMessage(System.Net.FormatMessageFlags,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr&,System.UInt32,System.IntPtr):System.UInt32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeSystemNativeMethods.LocalFree(System.IntPtr):System.UInt32" />
        // <SatisfiesLinkDemand Name="Marshal.PtrToStringUni(System.IntPtr):System.String" />
        // <SatisfiesLinkDemand Name="Win32Exception..ctor(System.Int32,System.String)" />
        // <SatisfiesLinkDemand Name="Marshal.GetExceptionForHR(System.Int32):System.Exception" />
        // <ReferencesCritical Name="Method: PeerToPeerOSHelper.get_P2PModuleHandle():System.IntPtr" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static PeerToPeerException CreateFromHr(string message, Int32 hr)
        {
            PeerToPeerException p2pEx = null;
            int facility = ((hr >> 16) & 0x1FFF);
            IntPtr NativeMessagePtr = IntPtr.Zero;
            try
            {
                UInt32 dwLength = UnsafeSystemNativeMethods.FormatMessage(
                        FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER |
                        FormatMessageFlags.FORMAT_MESSAGE_ARGUMENT_ARRAY |
                        (facility == FACILITY_P2P ? FormatMessageFlags.FORMAT_MESSAGE_FROM_HMODULE : FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM),
                        (facility == FACILITY_P2P ? PeerToPeerOSHelper.P2PModuleHandle : IntPtr.Zero),
                        (uint)(hr),
                        0,
                        ref NativeMessagePtr,
                        0,
                        IntPtr.Zero);
                if (dwLength != 0)
                {
                    string NativeMessage = Marshal.PtrToStringUni(NativeMessagePtr);
                    p2pEx = new PeerToPeerException(message, new Win32Exception(hr, NativeMessage));
                }
                else
                {
                    p2pEx = new PeerToPeerException(message, Marshal.GetExceptionForHR(hr));
                }
            }
            catch(Exception ex)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0, "Could not get the error message for error code {0} - Exception {1}", hr, ex);
                if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
                {
                    throw;
                }
            }
            finally
            {
                if (NativeMessagePtr != IntPtr.Zero)
                {
                    UnsafeSystemNativeMethods.LocalFree(NativeMessagePtr);
                }
            }
            if (p2pEx == null)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0, "Could not get the error message for error code {0}", hr);
                p2pEx = new PeerToPeerException(message + "Underlying native error " + hr);
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Error, 0, "Exception: {0}", p2pEx);
            return p2pEx;
       }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerToPeerException(SerializationInfo info, StreamingContext context) : base (info, context) {}
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext):System.Void" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            GetObjectData(info, context);
        }
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext):System.Void" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
