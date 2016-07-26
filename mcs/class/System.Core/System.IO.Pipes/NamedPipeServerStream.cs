//
// NamedPipeServerStream.cs
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

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.IO.Pipes
{
	[MonoTODO ("working only on win32 right now")]
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class NamedPipeServerStream : PipeStream
	{
		public const int MaxAllowedServerInstances = -1;

		public NamedPipeServerStream (string pipeName)
			: this (pipeName, PipeDirection.InOut)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction)
			: this (pipeName, direction, 1)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances)
			: this (pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, DefaultBufferSize, DefaultBufferSize)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
#if MOBILE		
			: base (direction, inBufferSize)
		{
			throw new NotImplementedException ();
		}
#else
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, null)
		{
		}
#endif

#if !MOBILE
		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, HandleInheritability.None)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, inheritability, PipeAccessRights.ReadData | PipeAccessRights.WriteData)
		{
		}

		[MonoTODO]
		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
			: base (direction, transmissionMode, outBufferSize)
		{
			var rights = ToAccessRights (direction) | additionalAccessRights;
			// FIXME: reject some rights declarations (for ACL).

			if (IsWindows)
				impl = new Win32NamedPipeServer (this, pipeName, maxNumberOfServerInstances, transmissionMode,
								 rights, options, inBufferSize, outBufferSize,
								 pipeSecurity, inheritability);
			else
				impl = new UnixNamedPipeServer (this, pipeName, maxNumberOfServerInstances, transmissionMode,
								rights, options, inBufferSize, outBufferSize, inheritability);

			InitializeHandle (impl.Handle, false, (options & PipeOptions.Asynchronous) != PipeOptions.None);
		}
#endif

		public NamedPipeServerStream (PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
			: base (direction, DefaultBufferSize)
		{
#if MOBILE
			throw new NotImplementedException ();
#else
			if (IsWindows)
				impl = new Win32NamedPipeServer (this, safePipeHandle);
			else
				impl = new UnixNamedPipeServer (this, safePipeHandle);
			IsConnected = isConnected;
			InitializeHandle (safePipeHandle, true, isAsync);
#endif
		}

		~NamedPipeServerStream ()
		{
			// To be compatible with .net
		}

		INamedPipeServer impl;

		public void Disconnect ()
		{
			impl.Disconnect ();
		}

#if !MOBILE
		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void RunAsClient (PipeStreamImpersonationWorker impersonationWorker)
		{
			throw new NotImplementedException ();
		}
#endif

		public void WaitForConnection ()
		{
			impl.WaitForConnection ();
			IsConnected = true;
		}

		public Task WaitForConnectionAsync ()
		{
			return WaitForConnectionAsync (CancellationToken.None);
		}

		[MonoTODO]
		public Task WaitForConnectionAsync (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public string GetImpersonationUserName ()
		{
			throw new NotImplementedException ();
		}

#if !MOBILE
		// async operations

		Action wait_connect_delegate;

		[HostProtection (SecurityAction.LinkDemand, ExternalThreading = true)]
		public IAsyncResult BeginWaitForConnection (AsyncCallback callback, object state)
		{
			if (wait_connect_delegate == null)
				wait_connect_delegate = new Action (WaitForConnection);
			return wait_connect_delegate.BeginInvoke (callback, state);
		}

		public void EndWaitForConnection (IAsyncResult asyncResult)
		{
			wait_connect_delegate.EndInvoke (asyncResult);
		}
#endif
	}
}

#endif
