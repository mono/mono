//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeClient.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Provides client connections for local Named Pipes.
    /// </summary>
    internal class NamedPipeClient
    {
        readonly string pipeName;

        public string Name 
        {
            get { return pipeName; }
        }

        /// <summary>
        /// Creates a new instance with the local Named Pipe name specified as an unique ID.
        /// </summary>
        /// <param name="uid">The Guid.</param>
        public NamedPipeClient(Guid uid) 
            : this(uid.ToString("N"))
        {
        }

        /// <summary>
        /// Creates a new instance for the specified pipe name.
        /// </summary>
        /// <param name="pipeName">The pipe name omiting the leading <c>\\.\pipe\</c></param>
        public NamedPipeClient(string pipeName)
        {
            this.pipeName = NamedPipeHelper.FormatPipeName(pipeName);
        }

        /// <summary>
        /// Connects to a local Named Pipe server.
        /// </summary>
        /// <returns>The NamedPipeSocket</returns>
        public NamedPipeSocket Connect() 
        {
            return Connect(2000);
        }

        /// <summary>
        /// Connects to a local Named Pipe server.
        /// </summary>
        /// <param name="timeout">Timeout in millisecons to wait for the connection.</param>
        /// <returns></returns>
        public NamedPipeSocket Connect(int timeout) 
        {
            while (true) 
            {
                IntPtr hPipe = NamedPipeHelper.CreateFile( 
                    pipeName,
                    NamedPipeHelper.GENERIC_READ |
                    NamedPipeHelper.GENERIC_WRITE, 
                    0,
                    IntPtr.Zero,
                    NamedPipeHelper.OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                    );

                if (hPipe.ToInt32() == NamedPipeHelper.INVALID_HANDLE_VALUE) 
                {
                    int lastError = Marshal.GetLastWin32Error();
                    if (lastError != NamedPipeHelper.ERROR_PIPE_BUSY)
                        throw new NamedPipeException(lastError);

                    if (!NamedPipeHelper.WaitNamedPipe(pipeName, timeout)) 
                    {
                        throw new NamedPipeException();
                    }
                }
                else 
                {
                    return new NamedPipeSocket(hPipe);
                }
            }
        }
    }
}

#endif
