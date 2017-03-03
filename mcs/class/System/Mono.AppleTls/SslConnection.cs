//
// SslConnection
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2014 Xamarin Inc.
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using XamCore.ObjCRuntime;

namespace XamCore.Security {

	delegate SslStatus SslReadFunc (IntPtr connection, IntPtr data, /* size_t* */ ref nint dataLength);

	delegate SslStatus SslWriteFunc (IntPtr connection, IntPtr data, /* size_t* */ ref nint dataLength);

	public abstract class SslConnection : IDisposable {

		GCHandle handle;

		protected SslConnection ()
		{
			handle = GCHandle.Alloc (this);
			ConnectionId = GCHandle.ToIntPtr (handle);
			ReadFunc = Read;
			WriteFunc = Write;
		}

		~SslConnection ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle.IsAllocated)
				handle.Free ();
		}

		public IntPtr ConnectionId { get; private set; }

		internal SslReadFunc ReadFunc { get; private set; }
		internal SslWriteFunc WriteFunc { get; private set; }

		public abstract SslStatus Read (IntPtr data, ref nint dataLength);

		public abstract SslStatus Write (IntPtr data, ref nint dataLength);

		[MonoPInvokeCallback (typeof (SslReadFunc))]
		static SslStatus Read (IntPtr connection, IntPtr data, ref nint dataLength)
		{
			var c = (SslConnection) GCHandle.FromIntPtr (connection).Target;
			return c.Read (data, ref dataLength);
		}

		[MonoPInvokeCallback (typeof (SslWriteFunc))]
		static SslStatus Write (IntPtr connection, IntPtr data, ref nint dataLength)
		{
			var c = (SslConnection) GCHandle.FromIntPtr (connection).Target;
			return c.Write (data, ref dataLength);
		}
	}

	// a concrete connection based on a managed Stream
	public class SslStreamConnection : SslConnection {

		byte[] buffer;

		public SslStreamConnection (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			InnerStream = stream;
			// a bit higher than the default maximum fragment size
			buffer = new byte [16384];
		}

		public Stream InnerStream { get; private set; }

		public override SslStatus Read (IntPtr data, ref nint dataLength)
		{
			// SSL state prevents multiple simultaneous reads (internal MAC would break)
			// so it's possible to reuse a single buffer (not re-allocate one each time)
			int len = (int) Math.Min (dataLength, buffer.Length);
			int read = InnerStream.Read (buffer, 0, len);
			Marshal.Copy (buffer, 0, data, read);
			bool block = (read < dataLength);
			dataLength = read;
			return block ? SslStatus.WouldBlock : SslStatus.Success;
		}

		public unsafe override SslStatus Write (IntPtr data, ref nint dataLength)
		{
			using (var ms = new UnmanagedMemoryStream ((byte*) data, dataLength)) {
				try {
					ms.CopyTo (InnerStream);
				}
				catch (IOException) {
					return SslStatus.ClosedGraceful;
				}
				catch {
					return SslStatus.Internal;
				}
			}
			return SslStatus.Success;
		}
	}
}