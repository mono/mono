//
// NamedPipeClientStream.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes
{
	[MonoTODO ("working only on win32 right now")]
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class NamedPipeClientStream : PipeStream
	{
		public NamedPipeClientStream (string pipeName)
			: this (".", pipeName)
		{
		}

		public NamedPipeClientStream (string serverName, string pipeName)
			: this (serverName, pipeName, PipeDirection.InOut)
		{
		}

		public NamedPipeClientStream (string serverName, string pipeName, PipeDirection direction)
			: this (serverName, pipeName, direction, PipeOptions.None)
		{
		}

		public NamedPipeClientStream (string serverName, string pipeName, PipeDirection direction, PipeOptions options)
			: this (serverName, pipeName, direction, options, TokenImpersonationLevel.None)
		{
		}

		public NamedPipeClientStream (string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel)
			: this (serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None)
		{
		}

		public NamedPipeClientStream (string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
#if MOBILE
			: base (direction, DefaultBufferSize)
		{
			throw new NotImplementedException ();
		}
#else
			: this (serverName, pipeName, ToAccessRights (direction), options, impersonationLevel, inheritability)
		{
		}
#endif

		public NamedPipeClientStream (PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
			: base (direction, DefaultBufferSize)
		{
#if MOBILE
			throw new NotImplementedException ();
#else
			if (IsWindows)
				impl = new Win32NamedPipeClient (this, safePipeHandle);
			else
				impl = new UnixNamedPipeClient (this, safePipeHandle);
			IsConnected = isConnected;
			InitializeHandle (safePipeHandle, true, isAsync);
#endif
		}

#if !MOBILE
		public NamedPipeClientStream (string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
			: base (ToDirection (desiredAccessRights), DefaultBufferSize)
		{
			if (impersonationLevel != TokenImpersonationLevel.None ||
			    inheritability != HandleInheritability.None)
				throw ThrowACLException ();

			if (IsWindows)
				impl = new Win32NamedPipeClient (this, serverName, pipeName, desiredAccessRights, options, inheritability);
			else
				impl = new UnixNamedPipeClient (this, serverName, pipeName, desiredAccessRights, options, inheritability);
		}
#endif

		~NamedPipeClientStream () {
			Dispose (false);
		}
		
		INamedPipeClient impl;

		public void Connect ()
		{
#if MOBILE
			throw new NotImplementedException ();
#else
			impl.Connect ();
			InitializeHandle (impl.Handle, false, impl.IsAsync);
			IsConnected = true;
#endif
		}

		public void Connect (int timeout)
		{
#if MOBILE
			throw new NotImplementedException ();
#else			
			impl.Connect (timeout);
			InitializeHandle (impl.Handle, false, impl.IsAsync);
			IsConnected = true;
#endif
		}

		public Task ConnectAsync ()
		{
			return ConnectAsync (Timeout.Infinite, CancellationToken.None);
		}

		public Task ConnectAsync (int timeout)
		{
			return ConnectAsync (timeout, CancellationToken.None);
		}

		public Task ConnectAsync (CancellationToken cancellationToken)
		{
			return ConnectAsync (Timeout.Infinite, cancellationToken);
		}

		public Task ConnectAsync (int timeout, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public int NumberOfServerInstances {
			get {
				CheckPipePropertyOperations ();
				return impl.NumberOfServerInstances;
			}
		}
	}
}

#endif
