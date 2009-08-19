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
	}

	class UnixNamedPipeClient : UnixNamedPipe, INamedPipeClient
	{
		// .ctor with existing handle
		public UnixNamedPipeClient (NamedPipeClientStream owner, SafePipeHandle safePipeHandle)
		{
			throw new NotImplementedException ();
		}

		// .ctor without handle - create new
		public UnixNamedPipeClient (NamedPipeClientStream owner, string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, HandleInheritability inheritability)
		{
			throw new NotImplementedException ();
		}

		bool is_async;
		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public void Connect ()
		{
			throw new NotImplementedException ();
		}

		public void Connect (int timeout)
		{
			throw new NotImplementedException ();
		}

		public bool IsAsync {
			get { return is_async; }
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
		public UnixNamedPipeServer (NamedPipeServerStream owner, string pipeName, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeAccessRights rights, PipeOptions options, int inBufferSize, int outBufferSize, HandleInheritability inheritability)
		{
			throw new NotImplementedException ();
		}

		SafePipeHandle handle;

		public override SafePipeHandle Handle {
			get { return handle; }
		}

		public void Disconnect ()
		{
			throw new NotImplementedException ();
		}

		public void WaitForConnection ()
		{
			throw new NotImplementedException ();
		}
	}
}
