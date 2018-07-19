//------------------------------------------------------------------------------
// <copyright file="_AcceptOverlappedAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;

    //
    //  ConnectOverlappedAsyncResult - used to take care of storage for async Socket BeginAccept call.
    //
    internal class ConnectOverlappedAsyncResult : BaseOverlappedAsyncResult {

        private EndPoint m_EndPoint;

        internal ConnectOverlappedAsyncResult(Socket socket, EndPoint endPoint, Object asyncState, AsyncCallback asyncCallback):
            base(socket,asyncState,asyncCallback)
        {
            m_EndPoint = endPoint;
        }



        //
        // This method is called by base.CompletionPortCallback base.OverlappedCallback as part of IO completion
        //
        internal override object PostCompletion(int numBytes) {
            SocketError errorCode = (SocketError)ErrorCode;
            Socket socket = (Socket)AsyncObject;

            if (errorCode==SocketError.Success) {

                //set the socket context
                try
                {
                    errorCode = UnsafeNclNativeMethods.OSSOCK.setsockopt(
                        socket.SafeHandle,
                        SocketOptionLevel.Socket,
                        SocketOptionName.UpdateConnectContext,
                        null,
                        0);
                    if (errorCode == SocketError.SocketError) errorCode = (SocketError) Marshal.GetLastWin32Error();
                }
                catch (ObjectDisposedException)
                {
                    errorCode = SocketError.OperationAborted;
                }

                ErrorCode = (int) errorCode;
            }

            if (errorCode==SocketError.Success) {
                socket.SetToConnected();
                return socket;
            }
            return null;
        }

        internal EndPoint RemoteEndPoint {
            get { return m_EndPoint; }                
        }

    }


}
