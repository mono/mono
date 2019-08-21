// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    internal static class SocketAsyncEngine
    {
        //
        // Encapsulates a particular SocketAsyncContext object's access to a SocketAsyncEngine.  
        //
        public readonly struct Token
        {
            private readonly MyIOAsyncResult _ioares;
            private readonly IOSelectorJob _readJob;
            private readonly IOSelectorJob _writeJob;

            public Token(SocketAsyncContext context)
            {
                _ioares = new MyIOAsyncResult(context);
                _readJob = new IOSelectorJob(IOOperation.Read, ReadCallback, _ioares);
                _writeJob = new IOSelectorJob(IOOperation.Write, WriteCallback, _ioares);
            }

            private static void ReadCallback(IOAsyncResult ioares)
            {
                ((MyIOAsyncResult)ioares).CompleteRead();
            }

            private static void WriteCallback(IOAsyncResult ioares)
            {
                ((MyIOAsyncResult)ioares).CompleteWrite();
            }

            public bool WasAllocated
            {
                get { return true; }
            }

            public void Free(SafeCloseSocket socket)
            {
                IOSelector.Remove(socket.DangerousGetHandle());
            }

            public void Register(SafeCloseSocket socket)
            {
                IOSelector.Add(socket.DangerousGetHandle(), _readJob);
                IOSelector.Add(socket.DangerousGetHandle(), _writeJob);
            }

            class MyIOAsyncResult : IOAsyncResult
            {
                private readonly SocketAsyncContext _context;

                public MyIOAsyncResult(SocketAsyncContext context)
                {
                    this._context = context;
                }

                internal void CompleteRead()
                {
                    _context.HandleEvents(Interop.Sys.SocketEvents.Read);
                }
                
                internal void CompleteWrite()
                {
                    _context.HandleEvents(Interop.Sys.SocketEvents.Write);
                }

                internal override void CompleteDisposed()
                {
                    _context.HandleEvents(Interop.Sys.SocketEvents.Close);
                }
            }
        }
    }
}
