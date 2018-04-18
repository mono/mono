//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeListener.cs
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


using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Listens for connections from local local Named Pipe clients.
    /// </summary>
    internal class NamedPipeListener
    {
        const uint DefaultBufferSize = 4096;
        readonly string pipeName;

        public string Name 
        {
            get { return pipeName; }
        }

        /// <summary>
        /// Creates a new listener with the local local Named Pipe name obtained form the specified UID.
        /// </summary>
        /// <param name="uid">The UID.</param>
        public NamedPipeListener(Guid uid) 
            : this(uid.ToString("N"))
        {
        }

        /// <summary>
        /// Creates a new listener with the specified name.
        /// </summary>
        /// <param name="pipeName">The pipe name omiting the leading <c>\\.\pipe\</c></param>
        public NamedPipeListener(string pipeName)
        {
            this.pipeName = NamedPipeHelper.FormatPipeName(pipeName);
        }

        /// <summary>
        /// Accepts a pending connection request
        /// </summary>
        /// <remarks>
        /// Accept is a blocking method that returns a NamedPipeSocket you can use to send and receive data. 
        /// </remarks>
        /// <returns>The NamedPipeSocket.</returns>
        public NamedPipeSocket Accept() 
        {
            IntPtr hPipe = NamedPipeHelper.CreateNamedPipe(
                pipeName,
                NamedPipeHelper.PIPE_ACCESS_DUPLEX | NamedPipeHelper.FILE_FLAG_OVERLAPPED,
                NamedPipeHelper.PIPE_TYPE_MESSAGE
                | NamedPipeHelper.PIPE_READMODE_MESSAGE
                | NamedPipeHelper.PIPE_WAIT,
                NamedPipeHelper.PIPE_UNLIMITED_INSTANCES,
                DefaultBufferSize,
                DefaultBufferSize,
                NamedPipeHelper.NMPWAIT_USE_DEFAULT_WAIT,
                IntPtr.Zero
                );

            if (hPipe.ToInt32 () == NamedPipeHelper.INVALID_HANDLE_VALUE) {
                throw new NamedPipeException (Marshal.GetLastWin32Error ());
            }

			// Connect the named pipe with overlapped structure
			// in order to make it altertable. This way we will
			// wake up when someone aborts a thread waiting 
			// for this pipe.
			NativeOverlapped overlapped = new NativeOverlapped ();
            bool canConnect = NamedPipeHelper.ConnectNamedPipe (hPipe, ref overlapped);

            int lastError = Marshal.GetLastWin32Error ();
			if (!canConnect) {
				if (lastError == NamedPipeHelper.ERROR_IO_PENDING) {
					uint bytesTransferred = 0;
					if (!GetOverlappedResultEx (hPipe, ref overlapped, out bytesTransferred, Timeout.Infinite, true)) {
						lastError = Marshal.GetLastWin32Error ();
						NamedPipeHelper.CloseHandle (hPipe);
						throw new NamedPipeException (lastError);
					}
					canConnect = true;
				} else if (lastError == NamedPipeHelper.ERROR_PIPE_CONNECTED)
					canConnect = true;
			}

			if (!canConnect) {
				NamedPipeHelper.CloseHandle (hPipe);
				throw new NamedPipeException (lastError);
			}

			return new NamedPipeSocket (hPipe);
        }

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetOverlappedResultEx (IntPtr hFile, [In] ref System.Threading.NativeOverlapped lpOverlapped,
												  out uint lpNumberOfBytesTransferred, int dwMilliseconds, bool bAltertable);

    }
}

