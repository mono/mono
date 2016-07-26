//
// PipeStream.cs
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
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace System.IO.Pipes
{
	[PermissionSet (SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public abstract class PipeStream : Stream
	{
		// FIXME: not precise.
		internal const int DefaultBufferSize = 0x400;

#if !MOBILE
		internal static bool IsWindows {
			get { return Win32Marshal.IsWindows; }
		}
#endif

		internal Exception ThrowACLException ()
		{
			return new NotImplementedException ("ACL is not supported in Mono");
		}

#if !MOBILE
		internal static PipeAccessRights ToAccessRights (PipeDirection direction)
		{
			switch (direction) {
			case PipeDirection.In:
				return PipeAccessRights.ReadData;
			case PipeDirection.Out:
				return PipeAccessRights.WriteData;
			case PipeDirection.InOut:
				return PipeAccessRights.ReadData | PipeAccessRights.WriteData;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		internal static PipeDirection ToDirection (PipeAccessRights rights)
		{
			bool r = (rights & PipeAccessRights.ReadData) != 0;
			bool w = (rights & PipeAccessRights.WriteData) != 0;
			if (r) {
				if (w)
					return PipeDirection.InOut;
				else
					return PipeDirection.In;
			} else {
				if (w)
					return PipeDirection.Out;
				else
					throw new ArgumentOutOfRangeException ();
			}
		}
#endif

		protected PipeStream (PipeDirection direction, int bufferSize)
			: this (direction, PipeTransmissionMode.Byte, bufferSize)
		{
		}

		protected PipeStream (PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
		{
			this.direction = direction;
			this.transmission_mode = transmissionMode;
			read_trans_mode = transmissionMode;
			if (outBufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize must be greater than 0");
			buffer_size = outBufferSize;
		}

		PipeDirection direction;
		PipeTransmissionMode transmission_mode, read_trans_mode;
		int buffer_size;
		SafePipeHandle handle;
		Stream stream;

		public override bool CanRead {
			get { return (direction & PipeDirection.In) != 0; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return (direction & PipeDirection.Out) != 0; }
		}

		public virtual int InBufferSize {
			get { return buffer_size; }
		}

		public bool IsAsync { get; private set; }

		public bool IsConnected { get; protected set; }

		internal Stream Stream {
			get {
				if (!IsConnected)
					throw new InvalidOperationException ("Pipe is not connected");
				if (stream == null)
					stream = new FileStream (handle.DangerousGetHandle (),
								 CanRead ? (CanWrite ? FileAccess.ReadWrite : FileAccess.Read)
								 	 : FileAccess.Write, true, buffer_size, IsAsync);
				return stream;
			}
			set { stream = value; }
		}

#if !MOBILE
		protected bool IsHandleExposed { get; private set; }
#endif

		[MonoTODO]
		public bool IsMessageComplete { get; private set; }

		[MonoTODO]
		public virtual int OutBufferSize {
			get { return buffer_size; }
		}

		public virtual PipeTransmissionMode ReadMode {
			get {
				CheckPipePropertyOperations ();
				return read_trans_mode;
			}
			set {
				CheckPipePropertyOperations ();
				read_trans_mode = value;
			}
		}

		public SafePipeHandle SafePipeHandle {
			get {
				CheckPipePropertyOperations ();
				return handle;
			}
		}

		public virtual PipeTransmissionMode TransmissionMode {
			get {
				CheckPipePropertyOperations ();
				return transmission_mode;
			}
		}

		// initialize/dispose/state check
#if MOBILE
		internal static void CheckPipePropertyOperations ()
		{
		}

		static void CheckReadOperations ()
		{
		}

		static void CheckWriteOperations ()
		{
		}
#else
		[MonoTODO]
		protected internal virtual void CheckPipePropertyOperations ()
		{
		}

		[MonoTODO]
		protected internal void CheckReadOperations ()
		{
			if (!IsConnected)
				throw new InvalidOperationException ("Pipe is not connected");
			if (!CanRead)
				throw new NotSupportedException ("The pipe stream does not support read operations");
		}

		[MonoTODO]
		protected internal void CheckWriteOperations ()
		{
			if (!IsConnected)
				throw new InvalidOperationException ("Pipe is not connected");
			if (!CanWrite)
				throw new NotSupportedException ("The pipe stream does not support write operations");
		}

		protected void InitializeHandle (SafePipeHandle handle, bool isExposed, bool isAsync)
		{
			this.handle = handle;
			this.IsHandleExposed = isExposed;
			this.IsAsync = isAsync;
		}
#endif

		protected override void Dispose (bool disposing)
		{
			if (handle != null && disposing)
				handle.Dispose ();
		}

		// not supported

		public override long Length {
			get { throw new NotSupportedException (); }
		}

		public override long Position {
			get { return 0; }
			set { throw new NotSupportedException (); }
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

#if !MOBILE
		public PipeSecurity GetAccessControl ()
		{
			return new PipeSecurity (SafePipeHandle,
						 AccessControlSections.Owner |
						 AccessControlSections.Group |
						 AccessControlSections.Access);
		}

		public void SetAccessControl (PipeSecurity pipeSecurity)
		{
			if (pipeSecurity == null)
				throw new ArgumentNullException ("pipeSecurity");
				
			pipeSecurity.Persist (SafePipeHandle);
		}
#endif

		// pipe I/O

		public void WaitForPipeDrain ()
		{
		}

		[MonoTODO]
		public override int Read ([In] byte [] buffer, int offset, int count)
		{
			CheckReadOperations ();

			return Stream.Read (buffer, offset, count);
		}

		[MonoTODO]
		public override int ReadByte ()
		{
			CheckReadOperations ();

			return Stream.ReadByte ();
		}

		[MonoTODO]
		public override void Write (byte [] buffer, int offset, int count)
		{
			CheckWriteOperations ();

			Stream.Write (buffer, offset, count);
		}

		[MonoTODO]
		public override void WriteByte (byte value)
		{
			CheckWriteOperations ();

			Stream.WriteByte (value);
		}

		[MonoTODO]
		public override void Flush ()
		{
			CheckWriteOperations ();

			Stream.Flush ();
		}

		// async

#if !MOBILE
		Func<byte [],int,int,int> read_delegate;

		[HostProtection (SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (read_delegate == null)
				read_delegate = new Func<byte[],int,int,int> (Read);
			return read_delegate.BeginInvoke (buffer, offset, count, callback, state);
		}

		Action<byte[],int,int> write_delegate;

		[HostProtection (SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (write_delegate == null)
				write_delegate = new Action<byte[],int,int> (Write);
			return write_delegate.BeginInvoke (buffer, offset, count, callback, state);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return read_delegate.EndInvoke (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			write_delegate.EndInvoke (asyncResult);
		}
#endif
	}
}

#endif
