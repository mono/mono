//------------------------------------------------------------------------------
// <copyright file="_OverlappedAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;
    using System.Collections.Generic;

    //
    //  OverlappedAsyncResult - used to take care of storage for async Socket operation
    //   from the BeginSend, BeginSendTo, BeginReceive, BeginReceiveFrom calls.
    //
    internal class OverlappedAsyncResult: BaseOverlappedAsyncResult {

        //
        // internal class members
        //

        private SocketAddress  m_SocketAddress;
        private SocketAddress  m_SocketAddressOriginal; // needed for partial BeginReceiveFrom/EndReceiveFrom completion

        // These two are used only as alternatives
        internal WSABuffer    m_SingleBuffer;
        internal WSABuffer[]  m_WSABuffers;

        //
        // the following two will be used only on WinNT to enable completion ports
        //
        //
        // Constructor. We take in the socket that's creating us, the caller's
        // state object, and the buffer on which the I/O will be performed.
        // We save the socket and state, pin the callers's buffer, and allocate
        // an event for the WaitHandle.
        //
        internal OverlappedAsyncResult(Socket socket, Object asyncState, AsyncCallback asyncCallback) :
            base(socket, asyncState, asyncCallback)
        { }

        //
        internal IntPtr GetSocketAddressPtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(m_SocketAddress.m_Buffer, 0);
        }
        //
        internal IntPtr GetSocketAddressSizePtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(m_SocketAddress.m_Buffer, m_SocketAddress.GetAddressSizeOffset());
        }
        //
        internal SocketAddress SocketAddress {
            get {
                return m_SocketAddress;
            }
        }
        //
        internal SocketAddress SocketAddressOriginal {
            get {
                return m_SocketAddressOriginal;
            }
            set {
                m_SocketAddressOriginal = value;
            }
        }

        //
        // SetUnmanagedStructures -
        // Fills in Overlapped Structures used in an Async Overlapped Winsock call
        //   these calls are outside the runtime and are unmanaged code, so we need
        //   to prepare specific structures and ints that lie in unmanaged memory
        //   since the Overlapped calls can be Async
        //

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, SocketAddress socketAddress, bool pinSocketAddress)
        {
            //
            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //
            m_SocketAddress = socketAddress;
            if (pinSocketAddress && m_SocketAddress != null)
            {
                object[]  objectsToPin = null;
                objectsToPin = new object[2];
                objectsToPin[0] = buffer;

                m_SocketAddress.CopyAddressSizeIntoBuffer();
                objectsToPin[1] = m_SocketAddress.m_Buffer;

                base.SetUnmanagedStructures(objectsToPin);
            } else {
                base.SetUnmanagedStructures(buffer);
            }

            m_SingleBuffer.Length = size;
            m_SingleBuffer.Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
        }

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, SocketAddress socketAddress, bool pinSocketAddress, ref OverlappedCache overlappedCache)
        {
            SetupCache(ref overlappedCache);
            SetUnmanagedStructures(buffer, offset, size, socketAddress, pinSocketAddress);
        }

        internal void SetUnmanagedStructures(BufferOffsetSize[] buffers)
        {

            //
            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //
            m_WSABuffers = new WSABuffer[buffers.Length];

            object[] objectsToPin = new object[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
                objectsToPin[i] = buffers[i].Buffer;

            // has to be called now to pin memory
            base.SetUnmanagedStructures(objectsToPin);

            for (int i = 0; i < buffers.Length; i++)
            {
                m_WSABuffers[i].Length = buffers[i].Size;
                m_WSABuffers[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffers[i].Buffer, buffers[i].Offset);
            }
        }

        internal void SetUnmanagedStructures(BufferOffsetSize[] buffers, ref OverlappedCache overlappedCache)
        {
            SetupCache(ref overlappedCache);
            SetUnmanagedStructures(buffers);
        }

        internal void SetUnmanagedStructures(IList<ArraySegment<byte>> buffers)
        {

            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //

            //make sure we don't let the app mess up the buffer array enough to cause
            //corruption.
            int count = buffers.Count;
            ArraySegment<byte>[] buffersCopy = new ArraySegment<byte>[count];

            for (int i=0;i<count;i++) {
                buffersCopy[i] = buffers[i];
                ValidationHelper.ValidateSegment(buffersCopy[i]);
            }
            
            m_WSABuffers = new WSABuffer[count];
            
            object[] objectsToPin = new object[count];
            for (int i = 0; i < count; i++)
                objectsToPin[i] = buffersCopy[i].Array;

            base.SetUnmanagedStructures(objectsToPin);

            for (int i = 0; i < count; i++)
            {
                m_WSABuffers[i].Length = buffersCopy[i].Count;
                m_WSABuffers[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffersCopy[i].Array, buffersCopy[i].Offset);
            }
        }

        internal void SetUnmanagedStructures(IList<ArraySegment<byte>> buffers, ref OverlappedCache overlappedCache)
        {
            SetupCache(ref overlappedCache);
            SetUnmanagedStructures(buffers);
        }

        //
        // This method is called after an asynchronous call is made for the user,
        // it checks and acts accordingly if the IO:
        // 1) completed synchronously.
        // 2) was pended.
        // 3) failed.
        //
        internal override object PostCompletion(int numBytes)
        {
            if (ErrorCode == 0) {
                if(Logging.On)LogBuffer(numBytes);
            }
            return (int)numBytes;
        }

        void LogBuffer(int size) {
            GlobalLog.Assert(Logging.On, "OverlappedAsyncResult#{0}::LogBuffer()|Logging is off!", ValidationHelper.HashString(this));
            if (size > -1) {
                if (m_WSABuffers != null) {
                    foreach (WSABuffer wsaBuffer in m_WSABuffers) {
                        Logging.Dump(Logging.Sockets, AsyncObject, "PostCompletion", wsaBuffer.Pointer, Math.Min(wsaBuffer.Length, size));
                        if ((size -= wsaBuffer.Length) <=0)
                            break;
                    }
                }
                else {
                    Logging.Dump(Logging.Sockets, AsyncObject, "PostCompletion", m_SingleBuffer.Pointer, Math.Min(m_SingleBuffer.Length, size));
               }
            }
        }

    }; // class OverlappedAsyncResult




} // namespace System.Net.Sockets
