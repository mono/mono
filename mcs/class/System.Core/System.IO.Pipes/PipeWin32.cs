//
// PipeWin32.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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

#if !BOOTSTRAP_BASIC

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes
{
	abstract class Win32AnonymousPipe : IPipe
	{
		protected Win32AnonymousPipe ()
		{
		}

		public abstract SafePipeHandle Handle { get; }

		public void WaitForPipeDrain ()
		{
			throw new NotImplementedException ();
		}
	}

	class Win32AnonymousPipeClient : Win32AnonymousPipe, IAnonymousPipeClient
	{
		// AnonymousPipeClientStream owner;

		public Win32AnonymousPipeClient (AnonymousPipeClientStream owner, SafePipeHandle handle)
		{
			// this.owner = owner;

			this.handle = handle;
		}

		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}
	}

	class Win32AnonymousPipeServer : Win32AnonymousPipe, IAnonymousPipeServer
	{
		// AnonymousPipeServerStream owner;

		public Win32AnonymousPipeServer (AnonymousPipeServerStream owner, PipeDirection direction, HandleInheritability inheritability, int bufferSize)
		{
			IntPtr r, w;
			SecurityAttributesHack att = new SecurityAttributesHack (inheritability == HandleInheritability.Inheritable);
			if (!Win32Marshal.CreatePipe (out r, out w, ref att, bufferSize))
				throw new Win32Exception (Marshal.GetLastWin32Error ());

			var rh = new SafePipeHandle (r, true);
			var wh = new SafePipeHandle (w, true);

			if (direction == PipeDirection.Out) {
				server_handle = wh;
				client_handle = rh;
			} else {
				server_handle = rh;
				client_handle = wh;
			}
		}

		public Win32AnonymousPipeServer (AnonymousPipeServerStream owner, SafePipeHandle serverHandle, SafePipeHandle clientHandle)
		{
			// this.owner = owner;
			this.server_handle = serverHandle;
			this.client_handle = clientHandle;
		}

		SafePipeHandle server_handle, client_handle;

		public override SafePipeHandle Handle {
			get { return server_handle; }
		}

		public SafePipeHandle ClientHandle {
			get { return client_handle; }
		}

		public void DisposeLocalCopyOfClientHandle ()
		{
			throw new NotImplementedException ();
		}
	}

	abstract class Win32NamedPipe : IPipe
	{
		string name_cache;

		public string Name {
			get {
				if (name_cache != null)
					return name_cache;

				int s, c, m, t;
				byte [] un = new byte [200];
				while (true) {
					if (!Win32Marshal.GetNamedPipeHandleState (Handle, out s, out c, out m, out t, un, un.Length)) {
						var xxx = Marshal.GetLastWin32Error ();
						throw new Win32Exception (xxx);
					}
					if (un [un.Length - 1] == 0)
						break;
					un = new byte [un.Length * 10];
				}
				name_cache = Encoding.Default.GetString (un);
				return name_cache;
			}
		}

		public abstract SafePipeHandle Handle { get; }

		public void WaitForPipeDrain ()
		{
			throw new NotImplementedException ();
		}
	}

	class Win32NamedPipeClient : Win32NamedPipe, INamedPipeClient
	{
		NamedPipeClientStream owner;

		// .ctor with existing handle
		public Win32NamedPipeClient (NamedPipeClientStream owner, SafePipeHandle safePipeHandle)
		{
			this.handle = safePipeHandle;
			this.owner = owner;

			// FIXME: retrieve is_async from state?
		}

		// .ctor without handle - create new
		public Win32NamedPipeClient (NamedPipeClientStream owner, string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, HandleInheritability inheritability)
		{
			name = String.Format ("\\\\{0}\\pipe\\{1}", serverName, pipeName);
			var att = new SecurityAttributesHack (inheritability == HandleInheritability.Inheritable);
			is_async = (options & PipeOptions.Asynchronous) != PipeOptions.None;

			opener = delegate {
				var ret = Win32Marshal.CreateFile (name, desiredAccessRights, 0, ref att, 3, 0, IntPtr.Zero);
				if (ret == new IntPtr (-1L))
					throw new Win32Exception (Marshal.GetLastWin32Error ());

				return new SafePipeHandle (ret, true);
			};
			this.owner = owner;
		}

		Func<SafePipeHandle> opener;
		bool is_async;
		string name;
		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public bool IsAsync {
			get { return is_async; }
		}

		public void Connect ()
		{
			if (owner.IsConnected)
				throw new InvalidOperationException ("The named pipe is already connected");

			handle = opener ();
		}

		public void Connect (int timeout)
		{
			if (owner.IsConnected)
				throw new InvalidOperationException ("The named pipe is already connected");

			if (!Win32Marshal.WaitNamedPipe (name, timeout))
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			Connect ();
		}

		public int NumberOfServerInstances {
			get {
				int s, c, m, t;
				byte [] un = null;
				if (!Win32Marshal.GetNamedPipeHandleState (Handle, out s, out c, out m, out t, un, 0))
					throw new Win32Exception (Marshal.GetLastWin32Error ());
				return c;
			}
		}
	}

	class Win32NamedPipeServer : Win32NamedPipe, INamedPipeServer
	{
		//NamedPipeServerStream owner;

		// .ctor with existing handle
		public Win32NamedPipeServer (NamedPipeServerStream owner, SafePipeHandle safePipeHandle)
		{
			handle = safePipeHandle;
			//this.owner = owner;
		}

		// .ctor without handle - create new
		public Win32NamedPipeServer (NamedPipeServerStream owner, string pipeName, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeAccessRights rights, PipeOptions options, int inBufferSize, int outBufferSize, HandleInheritability inheritability)
		{
			string name = String.Format ("\\\\.\\pipe\\{0}", pipeName);

			uint openMode = 0;
			if ((rights & PipeAccessRights.ReadData) != 0)
				openMode |= 1;
			if ((rights & PipeAccessRights.WriteData) != 0)
				openMode |= 2;
			if ((options & PipeOptions.WriteThrough) != 0)
				openMode |= 0x80000000;
			int pipeMode = 0;
			if ((owner.TransmissionMode & PipeTransmissionMode.Message) != 0)
				pipeMode |= 4;
			//if ((readTransmissionMode & PipeTransmissionMode.Message) != 0)
			//	pipeMode |= 2;
			if ((options & PipeOptions.Asynchronous) != 0)
				pipeMode |= 1;

			// FIXME: is nDefaultTimeout = 0 ok?
			var att = new SecurityAttributesHack (inheritability == HandleInheritability.Inheritable);
			var ret = Win32Marshal.CreateNamedPipe (name, openMode, pipeMode, maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, ref att, IntPtr.Zero);
			if (ret == new IntPtr (-1L))
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			handle = new SafePipeHandle (ret, true);
		}

		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public void Disconnect ()
		{
			Win32Marshal.DisconnectNamedPipe (Handle);
		}

		public void WaitForConnection ()
		{
			if (!Win32Marshal.ConnectNamedPipe (Handle, IntPtr.Zero))
				throw new Win32Exception (Marshal.GetLastWin32Error ());
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	struct SecurityAttributesHack
	{
		public readonly int Length;
		public readonly IntPtr SecurityDescriptor;
		public readonly bool Inheritable;

		public SecurityAttributesHack (bool inheritable)
		{
			Length = 0;
			SecurityDescriptor = IntPtr.Zero;
			Inheritable = inheritable;
		}
	}

	static class Win32Marshal
	{
		internal static bool IsWindows {
			get {
				switch (Environment.OSVersion.Platform) {
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.Win32NT:
				case PlatformID.WinCE:
					return true;
				default:
					return false;
				}
			}
		}

		// http://msdn.microsoft.com/en-us/library/aa365152%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern bool CreatePipe (out IntPtr readHandle, out IntPtr writeHandle, ref SecurityAttributesHack pipeAtts, int size);

		// http://msdn.microsoft.com/en-us/library/aa365150%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern IntPtr CreateNamedPipe (string name, uint openMode, int pipeMode, int maxInstances, int outBufferSize, int inBufferSize, int defaultTimeout, ref SecurityAttributesHack securityAttributes, IntPtr atts);

		// http://msdn.microsoft.com/en-us/library/aa365146%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern bool ConnectNamedPipe (SafePipeHandle handle, IntPtr overlapped);

		// http://msdn.microsoft.com/en-us/library/aa365166%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern bool DisconnectNamedPipe (SafePipeHandle handle);

		// http://msdn.microsoft.com/en-us/library/aa365443%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern bool GetNamedPipeHandleState (SafePipeHandle handle, out int state, out int curInstances, out int maxCollectionCount, out int collectDateTimeout, byte [] userName, int maxUserNameSize);

		// http://msdn.microsoft.com/en-us/library/aa365800%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern bool WaitNamedPipe (string name, int timeout);

		// http://msdn.microsoft.com/en-us/library/aa363858%28VS.85%29.aspx
		[DllImport ("kernel32")]
		internal static extern IntPtr CreateFile (string name, PipeAccessRights desiredAccess, FileShare fileShare, ref SecurityAttributesHack atts, int creationDisposition, int flags, IntPtr templateHandle);

	}
}

#endif
