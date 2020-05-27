//
// MonoBtlsBio.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP && MONO_FEATURE_BTLS
using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono.Btls
{
	class MonoBtlsBio : MonoBtlsObject
	{
		internal MonoBtlsBio (BoringBioHandle handle)
			: base (handle)
		{
		}

		new protected internal BoringBioHandle Handle {
			get { return (BoringBioHandle)base.Handle; }
		}

		protected internal class BoringBioHandle : MonoBtlsHandle
		{
			public BoringBioHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				if (handle != IntPtr.Zero) {
					mono_btls_bio_free (handle);
					handle = IntPtr.Zero;
				}
				return true;
			}

		}

		public static MonoBtlsBio CreateMonoStream (Stream stream)
		{
			return MonoBtlsBioMono.CreateStream (stream, false);
		}

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_read (IntPtr bio, IntPtr data, int len);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_write (IntPtr bio, IntPtr data, int len);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_flush (IntPtr bio);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_indent (IntPtr bio, uint indent, uint max_indent);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_hexdump (IntPtr bio, IntPtr data, int len, uint indent);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_bio_print_errors (IntPtr bio);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_bio_free (IntPtr handle);

		public int Read (byte[] buffer, int offset, int size)
		{
			CheckThrow ();
			var data = Marshal.AllocHGlobal (size);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				var ret = mono_btls_bio_read (Handle.DangerousGetHandle (), data, size);
				if (ret > 0)
					Marshal.Copy (data, buffer,offset, ret);
				return ret;
			} finally {
				if (release)
					Handle.DangerousRelease ();
				Marshal.FreeHGlobal (data);
			}
		}

		public int Write (byte[] buffer, int offset, int size)
		{
			CheckThrow ();
			var data = Marshal.AllocHGlobal (size);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				Marshal.Copy (buffer, offset, data, size);
				return mono_btls_bio_write (Handle.DangerousGetHandle (), data, size);
			} finally {
				if (release)
					Handle.DangerousRelease ();
				Marshal.FreeHGlobal (data);
			}
		}

		public int Flush ()
		{
			CheckThrow ();
			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				return mono_btls_bio_flush (Handle.DangerousGetHandle ());
			} finally {
				if (release)
					Handle.DangerousRelease ();
			}
		}

		public int Indent (uint indent, uint max_indent)
		{
			CheckThrow ();
			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				return mono_btls_bio_indent (Handle.DangerousGetHandle (), indent, max_indent);
			} finally {
				if (release)
					Handle.DangerousRelease ();
			}
		}

		public int HexDump (byte[] buffer, uint indent)
		{
			CheckThrow ();
			var data = Marshal.AllocHGlobal (buffer.Length);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				Marshal.Copy (buffer, 0, data, buffer.Length);
				return mono_btls_bio_hexdump (Handle.DangerousGetHandle (), data, buffer.Length, indent);
			} finally {
				if (release)
					Handle.DangerousRelease ();
				Marshal.FreeHGlobal (data);
			}
		}

		public void PrintErrors ()
		{
			CheckThrow ();
			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				mono_btls_bio_print_errors (Handle.DangerousGetHandle ());
			} finally {
				if (release)
					Handle.DangerousRelease ();
			}
		}
	}

	class MonoBtlsBioMemory : MonoBtlsBio
	{
		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_bio_mem_new ();

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_bio_mem_get_data (IntPtr handle, out IntPtr data);

		public MonoBtlsBioMemory ()
			: base (new BoringBioHandle (mono_btls_bio_mem_new ()))
		{
		}

		public byte[] GetData ()
		{
			IntPtr data;
			bool release = false;
			try {
				Handle.DangerousAddRef (ref release);
				var size = mono_btls_bio_mem_get_data (Handle.DangerousGetHandle (), out data);
				CheckError (size > 0);
				var buffer = new byte[size];
				Marshal.Copy (data, buffer, 0, size);
				return buffer;
			} finally {
				if (release)
					Handle.DangerousRelease ();
			}
		}
	}

	interface IMonoBtlsBioMono
	{
		int Read (byte[] buffer, int offset, int size, out bool wantMore);

		bool Write (byte[] buffer, int offset, int size);

		void Flush ();

		void Close ();
	}

	class MonoBtlsBioMono : MonoBtlsBio
	{
		GCHandle handle;
		IntPtr instance;
		BioReadFunc readFunc;
		BioWriteFunc writeFunc;
		BioControlFunc controlFunc;
		IntPtr readFuncPtr;
		IntPtr writeFuncPtr;
		IntPtr controlFuncPtr;
		IMonoBtlsBioMono backend;

		public MonoBtlsBioMono (IMonoBtlsBioMono backend)
			: base (new BoringBioHandle (mono_btls_bio_mono_new ()))
		{
			this.backend = backend;
			handle = GCHandle.Alloc (this);
			instance = GCHandle.ToIntPtr (handle);
			readFunc = OnRead;
			writeFunc = OnWrite;
			controlFunc = Control;
			readFuncPtr = Marshal.GetFunctionPointerForDelegate (readFunc);
			writeFuncPtr = Marshal.GetFunctionPointerForDelegate (writeFunc);
			controlFuncPtr = Marshal.GetFunctionPointerForDelegate (controlFunc);
			mono_btls_bio_mono_initialize (Handle.DangerousGetHandle (), instance, readFuncPtr, writeFuncPtr, controlFuncPtr);
		}

		public static MonoBtlsBioMono CreateStream (Stream stream, bool ownsStream)
		{
			return new MonoBtlsBioMono (new StreamBackend (stream, ownsStream));
		}

		public static MonoBtlsBioMono CreateString (StringWriter writer)
		{
			return new MonoBtlsBioMono (new StringBackend (writer));
		}

		enum ControlCommand
		{
			Flush = 1
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int BioReadFunc (IntPtr bio, IntPtr data, int dataLength, out int wantMore);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int BioWriteFunc (IntPtr bio, IntPtr data, int dataLength);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate long BioControlFunc (IntPtr bio, ControlCommand command, long arg);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_bio_mono_new ();

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_bio_mono_initialize (IntPtr handle, IntPtr instance, IntPtr readFunc, IntPtr writeFunc, IntPtr controlFunc);

		long Control (ControlCommand command, long arg)
		{
			switch (command) {
			case ControlCommand.Flush:
				backend.Flush ();
				return 1;

			default:
				throw new NotImplementedException ();
			}
		}

		int OnRead (IntPtr data, int dataLength, out int wantMore)
		{
			bool wantMoreBool;
			var buffer = new byte[dataLength];
			var ret = backend.Read (buffer, 0, dataLength, out wantMoreBool);
			wantMore = wantMoreBool ? 1 : 0;
			if (ret <= 0)
				return ret;
			Marshal.Copy (buffer, 0, data, ret);
			return ret;
		}

		[Mono.Util.MonoPInvokeCallback (typeof (BioReadFunc))]
		static int OnRead (IntPtr instance, IntPtr data, int dataLength, out int wantMore)
		{
			var c = (MonoBtlsBioMono)GCHandle.FromIntPtr (instance).Target;
			try {
				return c.OnRead (data, dataLength, out wantMore);
			} catch (Exception ex) {
				c.SetException (ex);
				wantMore = 0;
				return -1;
			}
		}

		int OnWrite (IntPtr data, int dataLength)
		{
			var buffer = new byte[dataLength];
			Marshal.Copy (data, buffer, 0, dataLength);
			var ok = backend.Write (buffer, 0, dataLength);
			return ok ? dataLength : -1;
		}

		[Mono.Util.MonoPInvokeCallback (typeof (BioWriteFunc))]
		static int OnWrite (IntPtr instance, IntPtr data, int dataLength)
		{
			var c = (MonoBtlsBioMono)GCHandle.FromIntPtr (instance).Target;
			try {
				return c.OnWrite (data, dataLength);
			} catch (Exception ex) {
				c.SetException (ex);
				return -1;
			}
		}

		[Mono.Util.MonoPInvokeCallback (typeof (BioControlFunc))]
		static long Control (IntPtr instance, ControlCommand command, long arg)
		{
			var c = (MonoBtlsBioMono)GCHandle.FromIntPtr (instance).Target;
			try {
				return c.Control (command, arg);
			} catch (Exception ex) {
				c.SetException (ex);
				return -1;
			}
		}

		protected override void Close ()
		{
			try {
				if (backend != null) {
					backend.Close ();
					backend = null;
				}
				if (handle.IsAllocated)
					handle.Free ();
			} finally {
				base.Close ();
			}
		}

		class StreamBackend : IMonoBtlsBioMono
		{
			Stream stream;
			bool ownsStream;

			public Stream InnerStream {
				get { return stream; }
			}

			public StreamBackend (Stream stream, bool ownsStream)
			{
				this.stream = stream;
				this.ownsStream = ownsStream;
			}

			public int Read (byte[] buffer, int offset, int size, out bool wantMore)
			{
				wantMore = false;
				return stream.Read (buffer, offset, size);
			}

			public bool Write (byte[] buffer, int offset, int size)
			{
				stream.Write (buffer, offset, size);
				return true;
			}

			public void Flush ()
			{
				stream.Flush ();
			}

			public void Close ()
			{
				if (ownsStream && stream != null)
					stream.Dispose ();
				stream = null;
			}
		}

		class StringBackend : IMonoBtlsBioMono
		{
			StringWriter writer;
			Encoding encoding = new UTF8Encoding ();

			public StringBackend (StringWriter writer)
			{
				this.writer = writer;
			}

			public int Read (byte[] buffer, int offset, int size, out bool wantMore)
			{
				wantMore = false;
				return -1;
			}

			public bool Write (byte[] buffer, int offset, int size)
			{
				var text = encoding.GetString (buffer, offset, size);
				writer.Write (text);
				return true;
			}

			public void Flush ()
			{
			}

			public void Close ()
			{
			}
		}
	}
}
#endif
