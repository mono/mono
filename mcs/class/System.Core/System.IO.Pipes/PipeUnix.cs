//
// PipeUnix.cs
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
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Mono.Unix.Native;

namespace System.IO.Pipes
{
	abstract class UnixAnonymousPipe : IPipe
	{
		protected UnixAnonymousPipe ()
		{
		}

		public abstract SafePipeHandle Handle { get; }

		public void WaitForPipeDrain ()
		{
			throw new NotImplementedException ();
		}
	}

	class UnixAnonymousPipeClient : UnixAnonymousPipe, IAnonymousPipeClient
	{
		// AnonymousPipeClientStream owner;

		public UnixAnonymousPipeClient (AnonymousPipeClientStream owner, SafePipeHandle handle)
		{
			// this.owner = owner;

			this.handle = handle;
		}

		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}
	}

	class UnixAnonymousPipeServer : UnixAnonymousPipe, IAnonymousPipeServer
	{
		// AnonymousPipeServerStream owner;

		public UnixAnonymousPipeServer (AnonymousPipeServerStream owner, PipeDirection direction, HandleInheritability inheritability, int bufferSize)
		{
			// this.owner = owner;

			throw new NotImplementedException ();
		}

		public UnixAnonymousPipeServer (AnonymousPipeServerStream owner, SafePipeHandle serverHandle, SafePipeHandle clientHandle)
		{
			// this.owner = owner;

			this.server_handle = serverHandle;
			this.client_handle = clientHandle;

			throw new NotImplementedException ();
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

	abstract class UnixNamedPipe : IPipe
	{
		public abstract SafePipeHandle Handle { get; }

		public void WaitForPipeDrain ()
		{
			throw new NotImplementedException ();
		}
		
		public void EnsureTargetFile (string name)
		{
			if (!File.Exists (name)) {
				var error = Syscall.mknod (name, FilePermissions.S_IFIFO | FilePermissions.ALLPERMS, 0);
				if (error != 0)
					throw new IOException (String.Format ("Error on creating named pipe: error code {0}", error));
			}
		}

		protected void ValidateOptions (PipeOptions options, PipeTransmissionMode mode)
		{
			if ((options & PipeOptions.WriteThrough) != 0)
				throw new NotImplementedException ("WriteThrough is not supported");

			if ((mode & PipeTransmissionMode.Message) != 0)
				throw new NotImplementedException ("Message transmission mode is not supported");
			if ((options & PipeOptions.Asynchronous) != 0) // FIXME: use O_NONBLOCK?
				throw new NotImplementedException ("Asynchronous pipe mode is not supported");
		}
		
		protected string RightsToAccess (PipeAccessRights rights)
		{
			string access = null;
			if ((rights & PipeAccessRights.ReadData) != 0) {
				if ((rights & PipeAccessRights.WriteData) != 0)
					access = "r+";
				else
					access = "r";
			}
			else if ((rights & PipeAccessRights.WriteData) != 0)
				access = "w";
			else
				throw new InvalidOperationException ("The pipe must be opened to either read or write");

			return access;
		}
		
		protected FileAccess RightsToFileAccess (PipeAccessRights rights)
		{
			if ((rights & PipeAccessRights.ReadData) != 0) {
				if ((rights & PipeAccessRights.WriteData) != 0)
					return FileAccess.ReadWrite;
				else
					return FileAccess.Read;
			}
			else if ((rights & PipeAccessRights.WriteData) != 0)
				return FileAccess.Write;
			else
				throw new InvalidOperationException ("The pipe must be opened to either read or write");
		}
	}

	class UnixNamedPipeClient : UnixNamedPipe, INamedPipeClient
	{
		// .ctor with existing handle
		public UnixNamedPipeClient (NamedPipeClientStream owner, SafePipeHandle safePipeHandle)
		{
			this.owner = owner;
			this.handle = safePipeHandle;
			// FIXME: dunno how is_async could be filled.
		}

		// .ctor without handle - create new
		public UnixNamedPipeClient (NamedPipeClientStream owner, string serverName, string pipeName,
		                             PipeAccessRights desiredAccessRights, PipeOptions options, HandleInheritability inheritability)
		{
			this.owner = owner;

			if (serverName != "." && !Dns.GetHostEntry (serverName).AddressList.Contains (IPAddress.Loopback))
				throw new NotImplementedException ("Unix fifo does not support remote server connection");
			var name = Path.Combine ("/var/tmp/", pipeName);
			EnsureTargetFile (name);
			
			RightsToAccess (desiredAccessRights);
			
			ValidateOptions (options, owner.TransmissionMode);
			
			// FIXME: handle inheritability

			opener = delegate {
				var fs = new FileStream (name, FileMode.Open, RightsToFileAccess (desiredAccessRights), FileShare.ReadWrite);
				owner.Stream = fs;
				handle = new SafePipeHandle (fs.Handle, false);
			};
		}

		NamedPipeClientStream owner;
		SafePipeHandle handle;
		Action opener;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public void Connect ()
		{
			if (owner.IsConnected)
				throw new InvalidOperationException ("The named pipe is already connected");

			opener ();
		}

		public void Connect (int timeout)
		{
			AutoResetEvent waitHandle = new AutoResetEvent (false);
			opener.BeginInvoke (delegate (IAsyncResult result) {
				opener.EndInvoke (result);
				waitHandle.Set ();
				}, null);
			if (!waitHandle.WaitOne (TimeSpan.FromMilliseconds (timeout)))
				throw new TimeoutException ();
		}

		public bool IsAsync {
			get { return false; }
		}

		public int NumberOfServerInstances {
			get { throw new NotImplementedException (); }
		}
	}

	class UnixNamedPipeServer : UnixNamedPipe, INamedPipeServer
	{
		//NamedPipeServerStream owner;

		// .ctor with existing handle
		public UnixNamedPipeServer (NamedPipeServerStream owner, SafePipeHandle safePipeHandle)
		{
			this.handle = safePipeHandle;
			//this.owner = owner;
		}

		// .ctor without handle - create new
		public UnixNamedPipeServer (NamedPipeServerStream owner, string pipeName, int maxNumberOfServerInstances,
		                             PipeTransmissionMode transmissionMode, PipeAccessRights rights, PipeOptions options,
		                            int inBufferSize, int outBufferSize, HandleInheritability inheritability)
		{
			string name = Path.Combine ("/var/tmp/", pipeName);
			EnsureTargetFile (name);

			RightsToAccess (rights);

			ValidateOptions (options, owner.TransmissionMode);

			// FIXME: maxNumberOfServerInstances, modes, sizes, handle inheritability
			
			var fs = new FileStream (name, FileMode.Open, RightsToFileAccess (rights), FileShare.ReadWrite);
			handle = new SafePipeHandle (fs.Handle, false);
			owner.Stream = fs;
			should_close_handle = true;
		}

		SafePipeHandle handle;
		bool should_close_handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public void Disconnect ()
		{
			if (should_close_handle)
				Syscall.fclose (handle.DangerousGetHandle ());
		}

		public void WaitForConnection ()
		{
			// FIXME: what can I do here?
		}
	}
}

#endif
