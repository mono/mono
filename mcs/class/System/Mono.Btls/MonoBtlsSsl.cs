//
// MonoBtlsSsl.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
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
#if SECURITY_DEP
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if MONOTOUCH
using MonoTouch;
#endif

namespace Mono.Btls
{
	delegate int MonoBtlsVerifyCallback (MonoBtlsX509StoreCtx ctx);
	delegate int MonoBtlsSelectCallback ();

	class MonoBtlsSsl : MonoBtlsObject
	{
		internal class BoringSslHandle : MonoBtlsHandle
		{
			public BoringSslHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_ssl_destroy (handle);
				return true;
			}
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_destroy (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_ssl_new (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_use_certificate (IntPtr handle, IntPtr x509);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_use_private_key (IntPtr handle, IntPtr key);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_add_chain_certificate (IntPtr handle, IntPtr x509);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_accept (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_connect (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_handshake (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_close (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_set_bio (IntPtr handle, IntPtr bio);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_read (IntPtr handle, IntPtr data, int len);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_write (IntPtr handle, IntPtr data, int len);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_get_error (IntPtr handle, int ret_code);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_get_version (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_set_min_version (IntPtr handle, int version);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_set_max_version (IntPtr handle, int version);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_get_cipher (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_get_ciphers (IntPtr handle, out IntPtr data);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_ssl_get_peer_certificate (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_set_cipher_list (IntPtr handle, IntPtr str);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_ssl_print_errors_cb (IntPtr func, IntPtr ctx);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_set_verify_param (IntPtr handle, IntPtr param);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_ssl_set_server_name (IntPtr handle, IntPtr name);

		static BoringSslHandle Create_internal (MonoBtlsSslCtx ctx)
		{
			var handle = mono_btls_ssl_new (ctx.Handle.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringSslHandle (handle);
		}

		PrintErrorsCallbackFunc printErrorsFunc;
		IntPtr printErrorsFuncPtr;

		public MonoBtlsSsl (MonoBtlsSslCtx ctx)
			: base (Create_internal (ctx))
		{
			printErrorsFunc = PrintErrorsCallback;
			printErrorsFuncPtr = Marshal.GetFunctionPointerForDelegate (printErrorsFunc);
		}

		new internal BoringSslHandle Handle {
			get { return (BoringSslHandle)base.Handle; }
		}

		public void SetBio (MonoBtlsBio bio)
		{
			CheckThrow ();
			mono_btls_ssl_set_bio (
				Handle.DangerousGetHandle (),
				bio.Handle.DangerousGetHandle ());
		}

		Exception ThrowError ([CallerMemberName] string callerName = null)
		{
			string errors;
			try {
				if (callerName == null)
					callerName = GetType ().Name;
				errors = GetErrors ();
			} catch {
				errors = null;
			}

			if (errors != null) {
				Console.Error.WriteLine ("ERROR: {0} failed: {1}", callerName, errors);
				throw new MonoBtlsException ("{0} failed: {1}.", callerName, errors);
			} else {
				Console.Error.WriteLine ("ERROR: {0} failed.", callerName);
				throw new MonoBtlsException ("{0} failed.", callerName);
			}
		}

		MonoBtlsSslError GetError (int ret_code)
		{
			CheckThrow ();
			var error = mono_btls_ssl_get_error (
				Handle.DangerousGetHandle (), ret_code);
			return (MonoBtlsSslError)error;
		}

		public void SetCertificate (MonoBtlsX509 x509)
		{
			CheckThrow ();

			var ret = mono_btls_ssl_use_certificate (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public void SetPrivateKey (MonoBtlsKey key)
		{
			CheckThrow ();

			var ret = mono_btls_ssl_use_private_key (
				Handle.DangerousGetHandle (),
				key.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public void AddIntermediateCertificate (MonoBtlsX509 x509)
		{
			CheckThrow ();

			var ret = mono_btls_ssl_add_chain_certificate (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public MonoBtlsSslError Accept ()
		{
			CheckThrow ();

			var ret = mono_btls_ssl_accept (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		public MonoBtlsSslError Connect ()
		{
			CheckThrow ();

			var ret = mono_btls_ssl_connect (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		public MonoBtlsSslError Handshake ()
		{
			CheckThrow ();

			var ret = mono_btls_ssl_handshake (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		delegate int PrintErrorsCallbackFunc (IntPtr str, IntPtr len, IntPtr ctx);

#if MONOTOUCH
		[MonoPInvokeCallback (typeof (PrintErrorsCallbackFunc))]
#endif
		static int PrintErrorsCallback (IntPtr str, IntPtr len, IntPtr ctx)
		{
			var sb = (StringBuilder)GCHandle.FromIntPtr (ctx).Target;
			try {
				var text = Marshal.PtrToStringAnsi (str, (int)len);
				sb.Append (text);
				return 1;
			} catch {
				return 0;
			}
		}

		public string GetErrors ()
		{
			var text = new StringBuilder ();
			var handle = GCHandle.Alloc (text);

			try {
				mono_btls_ssl_print_errors_cb (printErrorsFuncPtr, GCHandle.ToIntPtr (handle));
				return text.ToString ();
			} finally {
				if (handle.IsAllocated)
					handle.Free ();
			}
		}

		public void PrintErrors ()
		{
			var errors = GetErrors ();
			if (string.IsNullOrEmpty (errors))
				return;
			Console.Error.WriteLine (errors);
		}

		public MonoBtlsSslError Read (IntPtr data, ref int dataSize)
		{
			CheckThrow ();
			var ret = mono_btls_ssl_read (
				Handle.DangerousGetHandle (), data, dataSize);

			if (ret >= 0) {
				dataSize = ret;
				return MonoBtlsSslError.None;
			}

			var error = mono_btls_ssl_get_error (
				Handle.DangerousGetHandle (), ret);
			dataSize = 0;
			return (MonoBtlsSslError)error;
		}

		public MonoBtlsSslError Write (IntPtr data, ref int dataSize)
		{
			CheckThrow ();
			var ret = mono_btls_ssl_write (
				Handle.DangerousGetHandle (), data, dataSize);

			if (ret >= 0) {
				dataSize = ret;
				return MonoBtlsSslError.None;
			}

			var error = mono_btls_ssl_get_error (
				Handle.DangerousGetHandle (), ret);
			dataSize = 0;
			return (MonoBtlsSslError)error;
		}

		public int GetVersion ()
		{
			CheckThrow ();
			return mono_btls_ssl_get_version (Handle.DangerousGetHandle ());
		}

		public void SetMinVersion (int version)
		{
			CheckThrow ();
			mono_btls_ssl_set_min_version (Handle.DangerousGetHandle (), version);
		}

		public void SetMaxVersion (int version)
		{
			CheckThrow ();
			mono_btls_ssl_set_max_version (Handle.DangerousGetHandle (), version);
		}

		public int GetCipher ()
		{
			CheckThrow ();
			var cipher = mono_btls_ssl_get_cipher (Handle.DangerousGetHandle ());
			CheckError (cipher > 0);
			return cipher;
		}

		public short[] GetCiphers ()
		{
			CheckThrow ();
			IntPtr data;
			var count = mono_btls_ssl_get_ciphers (
				Handle.DangerousGetHandle (), out data);
			CheckError (count > 0);
			try {
				short[] ciphers = new short[count];
				Marshal.Copy (data, ciphers, 0, count);
				return ciphers;
			} finally {
				FreeDataPtr (data);
			}
		}

		public void SetCipherList (string str)
		{
			CheckThrow ();
			IntPtr strPtr = IntPtr.Zero;
			try {
				strPtr = Marshal.StringToHGlobalAnsi (str);
				var ret = mono_btls_ssl_set_cipher_list (
					Handle.DangerousGetHandle (), strPtr);
				CheckError (ret);
			} finally {
				if (strPtr != IntPtr.Zero)
					Marshal.FreeHGlobal (strPtr);
			}
		}

		public MonoBtlsX509 GetPeerCertificate ()
		{
			CheckThrow ();
			var x509 = mono_btls_ssl_get_peer_certificate (
				Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (x509));
		}

		public void SetVerifyParam (MonoBtlsX509VerifyParam param)
		{
			CheckThrow ();
			var ret = mono_btls_ssl_set_verify_param (
				Handle.DangerousGetHandle (),
				param.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public void SetServerName (string name)
		{
			CheckThrow ();
			IntPtr namePtr = IntPtr.Zero;
			try {
				namePtr = Marshal.StringToHGlobalAnsi (name);
				var ret = mono_btls_ssl_set_server_name (
					Handle.DangerousGetHandle (), namePtr);
				CheckError (ret);
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		protected override void Close ()
		{
			mono_btls_ssl_close (Handle.DangerousGetHandle ());
		}
	}
}
#endif
