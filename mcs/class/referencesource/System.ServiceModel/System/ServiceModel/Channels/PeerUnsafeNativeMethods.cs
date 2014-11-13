//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    static class PeerWinsock
    {
        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "WSAIoctl")]
        internal static extern int WSAIoctl(
                [In] IntPtr socketHandle,
                [In] int ioControlCode,
                [In] IntPtr inBuffer,
                [In] int inBufferSize,
                [Out] IntPtr outBuffer,
                [In] int outBufferSize,
                [Out] out int bytesTransferred,
                [In] IntPtr overlapped,
                [In] IntPtr completionRoutine);
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    struct SocketAddress
    {
        IntPtr sockAddr;
        int sockAddrLength;

        public IntPtr SockAddr { get { return sockAddr; } }
        public int SockAddrLength { get { return sockAddrLength; } }

        public void InitializeFromCriticalAllocHandleSocketAddress(CriticalAllocHandleSocketAddress sockAddr)
        {
            this.sockAddr = (IntPtr)sockAddr;
            this.sockAddrLength = sockAddr.Size;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SocketAddressList
    {
        int count;
        internal const int maxAddresses = 50;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxAddresses)]
        SocketAddress[] addresses;

        public SocketAddress[] Addresses { get { return addresses; } }
        public int Count { get { return count; } }

        public SocketAddressList(SocketAddress[] addresses, int count)
        {
            this.addresses = addresses;
            this.count = count;
        }

        public static ReadOnlyCollection<IPAddress> SortAddresses(Socket socket, IPAddress listenAddress, ReadOnlyCollection<IPAddress> addresses)
        {
            ReadOnlyCollection<IPAddress> sortedAddresses = null;

            // Skip sort if ipv6 isn't installed or if address array has a single address
            if (socket == null || addresses.Count <= 1)
            {
                sortedAddresses = addresses;
            }
            else
            {
                CriticalAllocHandleSocketAddressList inputBuffer = null;
                CriticalAllocHandleSocketAddressList outputBuffer = null;
                try
                {
                    inputBuffer = CriticalAllocHandleSocketAddressList.FromAddressList(addresses);
                    outputBuffer = CriticalAllocHandleSocketAddressList.FromAddressCount(0);
                    // Invoke ioctl to sort the addresses
                    int realOutputBufferSize;
                    int error = UnsafeNativeMethods.ERROR_SUCCESS;
                    int errorCode = PeerWinsock.WSAIoctl(socket.Handle,
                                                         unchecked((int)IOControlCode.AddressListSort),
                                                         (IntPtr)inputBuffer,
                                                         inputBuffer.Size,
                                                         (IntPtr)outputBuffer,
                                                         outputBuffer.Size,
                                                         out realOutputBufferSize,
                                                         IntPtr.Zero,
                                                         IntPtr.Zero);
                    if (errorCode == -1)
                    {
                        // Get the Win32 error code before doing anything else
                        error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(error));
                    }

                    // Marshal the sorted SOCKET_ADDRESS_LIST into IPAddresses
                    sortedAddresses = outputBuffer.ToAddresses();
                }
                finally
                {
                    if (inputBuffer != null) inputBuffer.Dispose();
                    if (outputBuffer != null) outputBuffer.Dispose();
                }
            }
            return sortedAddresses;
        }
    }

    // Type that converts from sockaddr_in6 to IPAddress and vice-versa
    [Serializable, StructLayout(LayoutKind.Sequential)]
    struct sockaddr_in6
    {
        short sin6_family;
        ushort sin6_port;
        uint sin6_flowinfo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = addrByteCount)]
        byte[] sin6_addr;
        uint sin6_scope_id;

        const int addrByteCount = 16;

        // if the addr is v4-mapped-v6, 10th and 11th byte contain 0xFF. the last 4 bytes contain the ipv4 address
        const int v4MapIndex = 10;
        const int v4Index = v4MapIndex + 2;

        public sockaddr_in6(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.sin6_addr = address.GetAddressBytes();
                this.sin6_scope_id = (uint)address.ScopeId;
            }
            else
            {
                // Map v4 address to v4-mapped v6 addr (i.e., ::FFFF:XX.XX.XX.XX)
                byte[] v4AddressBytes = address.GetAddressBytes();

                this.sin6_addr = new byte[addrByteCount];
                for (int i = 0; i < v4MapIndex; i++)
                    this.sin6_addr[i] = 0;
                this.sin6_addr[v4MapIndex] = 0xff;
                this.sin6_addr[v4MapIndex + 1] = 0xff;
                for (int i = v4Index; i < addrByteCount; i++)
                    this.sin6_addr[i] = v4AddressBytes[i - v4Index];

                this.sin6_scope_id = 0;     // V4 address doesn't have a scope ID
            }

            this.sin6_family = (short)AddressFamily.InterNetworkV6;
            this.sin6_port = 0;
            this.sin6_flowinfo = 0;
        }

        public short Family { get { return this.sin6_family; } }
        public uint FlowInfo { get { return this.sin6_flowinfo; } }

        // Returns true if the address is a v4-mapped v6 address
        // Adapted from ws2ipdef.w's IN6_IS_ADDR_V4MAPPED macro
        private bool IsV4Mapped
        {
            get
            {
                // A v4-mapped v6 address will have the last 4 bytes contain the IPv4 address. 
                // The preceding 2 bytes contain 0xFFFF. All others are 0.
                if (sin6_addr[v4MapIndex] != 0xff || sin6_addr[v4MapIndex + 1] != 0xff)
                    return false;

                for (int i = 0; i < v4MapIndex; i++)
                    if (sin6_addr[i] != 0)
                        return false;

                return true;
            }
        }

        public ushort Port { get { return this.sin6_port; } }

        // Converts a sockaddr_in6 to IPAddress
        // A v4 mapped v6 address is converted to a v4 address
        public IPAddress ToIPAddress()
        {
            if (!(this.sin6_family == (short)AddressFamily.InterNetworkV6))
            {
                throw Fx.AssertAndThrow("AddressFamily expected to be InterNetworkV6");
            }

            if (IsV4Mapped)
            {
                byte[] addr = 
                {
                    this.sin6_addr[v4Index],
                    this.sin6_addr[v4Index + 1],
                    this.sin6_addr[v4Index + 2],
                    this.sin6_addr[v4Index + 3] 
                };
                return new IPAddress(addr);
            }
            else
            {
                return new IPAddress(this.sin6_addr, this.sin6_scope_id);
            }
        }
    }

    class CriticalAllocHandleSocketAddressList : CriticalAllocHandle
    {
        int count;
        int size;
        CriticalAllocHandleSocketAddress[] socketHandles;

        public int Count { get { return count; } }
        public int Size { get { return size; } }

        public static CriticalAllocHandleSocketAddressList FromAddressList(ICollection<IPAddress> addresses)
        {
            if (addresses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addresses");
            }
            int count = addresses.Count;

            CriticalAllocHandleSocketAddress[] socketHandles = new CriticalAllocHandleSocketAddress[SocketAddressList.maxAddresses];
            SocketAddressList socketAddressList = new SocketAddressList(new SocketAddress[SocketAddressList.maxAddresses], count);
            int i = 0;
            foreach (IPAddress address in addresses)
            {
                if (i == SocketAddressList.maxAddresses) break; // due to Marshalling fixed sized array of SocketAddresses.
                socketHandles[i] = CriticalAllocHandleSocketAddress.FromIPAddress(address);
                socketAddressList.Addresses[i].InitializeFromCriticalAllocHandleSocketAddress(socketHandles[i]);
                ++i;
            }

            int size = Marshal.SizeOf(socketAddressList);
            CriticalAllocHandleSocketAddressList result = CriticalAllocHandleSocketAddressList.FromSize(size);
            result.count = count;
            result.socketHandles = socketHandles;
            Marshal.StructureToPtr(socketAddressList, result, false);
            return result;
        }

        public static CriticalAllocHandleSocketAddressList FromAddressCount(int count)
        {
            SocketAddressList socketAddressList = new SocketAddressList(new SocketAddress[SocketAddressList.maxAddresses], 0);
            int size = Marshal.SizeOf(socketAddressList);
            CriticalAllocHandleSocketAddressList result = CriticalAllocHandleSocketAddressList.FromSize(size);
            result.count = count;
            Marshal.StructureToPtr(socketAddressList, result, false);
            return result;
        }

        static new CriticalAllocHandleSocketAddressList FromSize(int size)
        {
            CriticalAllocHandleSocketAddressList result = new CriticalAllocHandleSocketAddressList();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                result.SetHandle(Marshal.AllocHGlobal(size));
                result.size = size;
            }
            return result;
        }

        public ReadOnlyCollection<IPAddress> ToAddresses()
        {
            SocketAddressList socketAddressList = (SocketAddressList)Marshal.PtrToStructure(this, typeof(SocketAddressList));
            IPAddress[] addresses = new IPAddress[socketAddressList.Count];
            for (int i = 0; i < addresses.Length; i++)
            {
                if (!(socketAddressList.Addresses[i].SockAddrLength == Marshal.SizeOf(typeof(sockaddr_in6))))
                {
                    throw Fx.AssertAndThrow("sockAddressLength in SOCKET_ADDRESS expected to be valid");
                }
                sockaddr_in6 sockAddr = (sockaddr_in6)Marshal.PtrToStructure(socketAddressList.Addresses[i].SockAddr, typeof(sockaddr_in6));
                addresses[i] = sockAddr.ToIPAddress();
            }

            return Array.AsReadOnly<IPAddress>(addresses);
        }
    }

    class CriticalAllocHandleSocketAddress : CriticalAllocHandle
    {
        int size;

        public int Size { get { return size; } }

        public static CriticalAllocHandleSocketAddress FromIPAddress(IPAddress input)
        {
            if (input == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("input");
            }

            CriticalAllocHandleSocketAddress result = null;
            int size = Marshal.SizeOf(typeof(sockaddr_in6));
            result = CriticalAllocHandleSocketAddress.FromSize(size);
            sockaddr_in6 sa = new sockaddr_in6(input);
            Marshal.StructureToPtr(sa, (IntPtr)result, false);
            return result;
        }

        public static new CriticalAllocHandleSocketAddress FromSize(int size)
        {
            CriticalAllocHandleSocketAddress result = new CriticalAllocHandleSocketAddress();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                result.SetHandle(Marshal.AllocHGlobal(size));
                result.size = size;
            }
            return result;
        }
    }

    class CriticalAllocHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static implicit operator IntPtr(CriticalAllocHandle safeHandle)
        {
            return (safeHandle != null) ? safeHandle.handle : (IntPtr)null;
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public static CriticalAllocHandle FromSize(int size)
        {
            CriticalAllocHandle result = new CriticalAllocHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                result.SetHandle(Marshal.AllocHGlobal(size));
            }
            return result;
        }
    }

    class CriticalAllocHandleBlob : CriticalAllocHandle
    {
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public static CriticalAllocHandle FromBlob<T>(T id)
        {
            int size = Marshal.SizeOf(typeof(T));
            CriticalAllocHandle result = CriticalAllocHandle.FromSize(size);
            Marshal.StructureToPtr(id, (IntPtr)result, false);
            return result;
        }
    }

    class CriticalAllocHandleGuid : CriticalAllocHandle
    {
        public static CriticalAllocHandle FromGuid(Guid input)
        {
            int guidSize = Marshal.SizeOf(typeof(Guid));
            CriticalAllocHandle result = CriticalAllocHandle.FromSize(guidSize);
            Marshal.Copy(input.ToByteArray(), 0, (IntPtr)result, guidSize);
            return result;
        }
    }
}
