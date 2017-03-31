//
// System.Runtime.Remoting.Channels.Ipc.Win32.NamedPipeHelper.cs
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
using System.Text;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Ipc.Win32
{
    /// <summary>
    /// Named Pipe P/Invoke declarations.
    /// </summary>
    internal sealed class NamedPipeHelper
    {
        NamedPipeHelper()
        {
        }

        /// <summary>
        /// Returns a properly formatted local pipe name.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public static string FormatPipeName(string pipeName) 
        {
            return String.Format(@"\\.\pipe\{0}", pipeName);
        }

        #region P/Invoke

        // Named pipe acces flags
        public const uint PIPE_ACCESS_INBOUND = 1;
        public const uint PIPE_ACCESS_OUTBOUND = 2;
        public const uint PIPE_ACCESS_DUPLEX = 3;

        // Named pipe wait states
        public const uint PIPE_WAIT = 0;
        public const uint PIPE_NOWAIT = 1;

        // Named pipe message types
        public const uint PIPE_TYPE_BYTE = 0;
        public const uint PIPE_TYPE_MESSAGE = 4;

        // Named pipe message read modes
        public const uint PIPE_READMODE_BYTE = 0;
        public const uint PIPE_READMODE_MESSAGE = 2;

        // Named pipe endpoints
        public const uint PIPE_CLIENT_END = 0;
        public const uint PIPE_SERVER_END = 1;

        // Named pipe misc flags
        public const uint PIPE_UNLIMITED_INSTANCES = 255;

        // Named pipe wait flags
        public const uint NMPWAIT_USE_DEFAULT_WAIT = 0;
        public const uint NMPWAIT_NOWAIT = 1;
        public const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;

        // Create flags
        public const uint CREATE_NEW        = 1;
        public const uint CREATE_ALWAYS     = 2;
        public const uint OPEN_EXISTING     = 3;
        public const uint OPEN_ALWAYS       = 4;
        public const uint TRUNCATE_EXISTING = 5;
	    public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        
        // Access flags
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_ALL = 0x10000000;

        // Error results
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PIPE_BUSY = 231;
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_PIPE_NOT_CONNECTED = 233;
        public const int ERROR_PIPE_CONNECTED = 535;
        public const int ERROR_PIPE_LISTENING = 536;
	    public const int ERROR_IO_PENDING = 997;

        public const int INVALID_HANDLE_VALUE = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateNamedPipe(
            string lpName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr pipeSecurityDescriptor
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ConnectNamedPipe(
            IntPtr hPipe,
            [In] ref NativeOverlapped lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            String lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr attr,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            IntPtr hHandle,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(
            IntPtr hHandle,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNamedPipeHandleState(
            IntPtr hPipe,
            out int lpState,
            out int lpCurInstances,
            out int lpMaxCollectionCount,
            out int lpCollectDataTimeout,
            StringBuilder lpUserName,
            int nMaxUserNameSize
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetNamedPipeHandleState(
            IntPtr hPipe,
            ref uint lpMode,
            ref uint lpMaxCollectionCount,
            ref uint lpCollectDataTimeout
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNamedPipeInfo(
            IntPtr hPipe,
            out int lpFlags,
            out int lpOutBufferSize,
            out int lpInBufferSize,
            out int lpMaxInstances
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PeekNamedPipe(
            IntPtr hPipe,
            IntPtr lpBuffer,
            uint nBufferSize,
            out uint lpBytesRead,
            out uint lpTotalBytesAvail,
            out uint lpBytesLeftThisMessage
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WaitNamedPipe(
            string name,
            int timeout
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DisconnectNamedPipe(
            IntPtr hPipe
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushFileBuffers(
            IntPtr hFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(
            IntPtr hHandle
            );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ImpersonateNamedPipeClient(
            IntPtr hPipe
            );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RevertToSelf();

        #endregion

    }

}

