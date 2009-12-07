//
// AnonymousPipeServerStream.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes
{
	[MonoTODO ("Anonymous pipes are not working even on win32, due to some access authorization issue")]
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class AnonymousPipeServerStream : PipeStream
	{
		public AnonymousPipeServerStream ()
			: this (PipeDirection.Out)
		{
		}

		public AnonymousPipeServerStream (PipeDirection direction)
			: this (direction, HandleInheritability.None)
		{
		}

		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability)
			: this (direction, inheritability, DefaultBufferSize)
		{
		}

		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability, int bufferSize)
			: this (direction, inheritability, bufferSize, null)
		{
		}

		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
			: base (direction, bufferSize)
		{
			if (pipeSecurity != null)
				throw ThrowACLException ();

			if (direction == PipeDirection.InOut)
				throw new NotSupportedException ("Anonymous pipe direction can only be either in or out.");

			if (IsWindows)
				impl = new Win32AnonymousPipeServer (this,direction, inheritability, bufferSize);
			else
				impl = new UnixAnonymousPipeServer (this,direction, inheritability, bufferSize);

			InitializeHandle (impl.Handle, false, false);
			IsConnected = true;
		}

		[MonoTODO]
		public AnonymousPipeServerStream (PipeDirection direction, SafePipeHandle serverSafePipeHandle, SafePipeHandle clientSafePipeHandle)
			: base (direction, DefaultBufferSize)
		{
			if (serverSafePipeHandle == null)
				throw new ArgumentNullException ("serverSafePipeHandle");
			if (clientSafePipeHandle == null)
				throw new ArgumentNullException ("clientSafePipeHandle");

			if (direction == PipeDirection.InOut)
				throw new NotSupportedException ("Anonymous pipe direction can only be either in or out.");

			if (IsWindows)
				impl = new Win32AnonymousPipeServer (this, serverSafePipeHandle, clientSafePipeHandle);
			else
				impl = new UnixAnonymousPipeServer (this, serverSafePipeHandle, clientSafePipeHandle);

			InitializeHandle (serverSafePipeHandle, true, false);
			IsConnected = true;

			ClientSafePipeHandle = clientSafePipeHandle;
		}

		IAnonymousPipeServer impl;

		[MonoTODO]
		public SafePipeHandle ClientSafePipeHandle { get; private set; }

		public override PipeTransmissionMode ReadMode {
			set {
				if (value == PipeTransmissionMode.Message)
					throw new NotSupportedException ();
			}
		}

		public override PipeTransmissionMode TransmissionMode {
			get { return PipeTransmissionMode.Byte; }
		}

		[MonoTODO]
		public void DisposeLocalCopyOfClientHandle ()
		{
			impl.DisposeLocalCopyOfClientHandle ();
		}

		public string GetClientHandleAsString ()
		{
			// We use int64 for safety.
			return impl.Handle.DangerousGetHandle ().ToInt64 ().ToString (NumberFormatInfo.InvariantInfo);
		}
	}
}

#endif
