//
// MonoOpenSSLSsl.cs
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
#if SECURITY_DEP && MONO_FEATURE_OPENSSL
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.OpenSSL
{
	delegate int MonoOpenSSLVerifyCallback (MonoOpenSSLX509StoreCtx ctx);
	delegate int MonoOpenSSLSelectCallback (string[] acceptableIssuers);

	class MonoOpenSSLSsl : MonoOpenSSLObject
	{
		internal class OpenSSLHandle : MonoOpenSSLHandle
		{
			public OpenSSLHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_openssl_ssl_destroy (handle);
				handle = IntPtr.Zero;
				return true;
			}
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_destroy (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_ssl_new (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_use_certificate (IntPtr handle, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_use_private_key (IntPtr handle, IntPtr key);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_add_chain_certificate (IntPtr handle, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_accept (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_connect (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_handshake (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_close (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_shutdown (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_set_quiet_shutdown (IntPtr handle, int mode);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_set_bio (IntPtr handle, IntPtr bio);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_read (IntPtr handle, IntPtr data, int len);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_write (IntPtr handle, IntPtr data, int len);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_get_error (IntPtr handle, int ret_code);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_get_version (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_set_min_version (IntPtr handle, int version);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_set_max_version (IntPtr handle, int version);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_get_cipher (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_get_ciphers (IntPtr handle, out IntPtr data);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_ssl_get_peer_certificate (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_set_cipher_list (IntPtr handle, IntPtr str);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_print_errors_cb (IntPtr func, IntPtr ctx);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_set_verify_param (IntPtr handle, IntPtr param);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_set_server_name (IntPtr handle, IntPtr name);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_ssl_get_server_name (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_ssl_set_renegotiate_mode (IntPtr handle, int mode);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_ssl_renegotiate_pending (IntPtr handle);

		static OpenSSLHandle Create_internal (MonoOpenSSLSslCtx ctx)
		{
			var handle = mono_openssl_ssl_new (ctx.Handle.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				throw new MonoOpenSSLException ();
			return new OpenSSLHandle (handle);
		}

		MonoOpenSSLBio bio;
		PrintErrorsCallbackFunc printErrorsFunc;
		IntPtr printErrorsFuncPtr;

		public MonoOpenSSLSsl (MonoOpenSSLSslCtx ctx)
			: base (Create_internal (ctx))
		{
			printErrorsFunc = PrintErrorsCallback;
			printErrorsFuncPtr = Marshal.GetFunctionPointerForDelegate (printErrorsFunc);
		}

		new internal OpenSSLHandle Handle {
			get { return (OpenSSLHandle)base.Handle; }
		}

		public void SetBio (MonoOpenSSLBio bio)
		{
			CheckThrow ();
			this.bio = bio;
			mono_openssl_ssl_set_bio (
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

			if (errors != null)
				throw new MonoOpenSSLException ("{0} failed: {1}.", callerName, errors);
			else
				throw new MonoOpenSSLException ("{0} failed.", callerName);
		}

		MonoOpenSSLSslError GetError (int ret_code)
		{
			CheckThrow ();
			bio.CheckLastError ();

			var error = mono_openssl_ssl_get_error (
				Handle.DangerousGetHandle (), ret_code);
			return (MonoOpenSSLSslError)error;
		}

		public void SetCertificate (MonoOpenSSLX509 x509)
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_use_certificate (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public void SetPrivateKey (MonoOpenSSLKey key)
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_use_private_key (
				Handle.DangerousGetHandle (),
				key.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public void AddIntermediateCertificate (MonoOpenSSLX509 x509)
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_add_chain_certificate (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			if (ret <= 0)
				throw ThrowError ();
		}

		public MonoOpenSSLSslError Accept ()
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_accept (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		public MonoOpenSSLSslError Connect ()
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_connect (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		public MonoOpenSSLSslError Handshake ()
		{
			CheckThrow ();

			var ret = mono_openssl_ssl_handshake (Handle.DangerousGetHandle ());

			var error = GetError (ret);
			return error;
		}

		delegate int PrintErrorsCallbackFunc (IntPtr str, IntPtr len, IntPtr ctx);

		[Mono.Util.MonoPInvokeCallback (typeof (PrintErrorsCallbackFunc))]
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
				mono_openssl_ssl_print_errors_cb (printErrorsFuncPtr, GCHandle.ToIntPtr (handle));
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

		public MonoOpenSSLSslError Read (IntPtr data, ref int dataSize)
		{
			CheckThrow ();
			var ret = mono_openssl_ssl_read (
				Handle.DangerousGetHandle (), data, dataSize);

			if (ret > 0) {
				dataSize = ret;
				return MonoOpenSSLSslError.None;
			}

			var error = GetError (ret);
			if (ret == 0 && error == MonoOpenSSLSslError.Syscall) {
				// End-of-stream
				dataSize = 0;
				return MonoOpenSSLSslError.None;
			}

			dataSize = 0;
			return error;
		}

		public MonoOpenSSLSslError Write (IntPtr data, ref int dataSize)
		{
			CheckThrow ();
			var ret = mono_openssl_ssl_write (
				Handle.DangerousGetHandle (), data, dataSize);

			if (ret >= 0) {
				dataSize = ret;
				return MonoOpenSSLSslError.None;
			}

			var error = mono_openssl_ssl_get_error (
				Handle.DangerousGetHandle (), ret);
			dataSize = 0;
			return (MonoOpenSSLSslError)error;
		}

		public int GetVersion ()
		{
			CheckThrow ();
			return mono_openssl_ssl_get_version (Handle.DangerousGetHandle ());
		}

		public void SetMinVersion (int version)
		{
			CheckThrow ();
			mono_openssl_ssl_set_min_version (Handle.DangerousGetHandle (), version);
		}

		public void SetMaxVersion (int version)
		{
			CheckThrow ();
			mono_openssl_ssl_set_max_version (Handle.DangerousGetHandle (), version);
		}

		public int GetCipher ()
		{
			CheckThrow ();
			var cipher = mono_openssl_ssl_get_cipher (Handle.DangerousGetHandle ());
			CheckError (cipher > 0);
			return cipher;
		}

		public short[] GetCiphers ()
		{
			CheckThrow ();
			IntPtr data;
			var count = mono_openssl_ssl_get_ciphers (
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
				var ret = mono_openssl_ssl_set_cipher_list (
					Handle.DangerousGetHandle (), strPtr);
				CheckError (ret);
			} finally {
				if (strPtr != IntPtr.Zero)
					Marshal.FreeHGlobal (strPtr);
			}
		}

		public MonoOpenSSLX509 GetPeerCertificate ()
		{
			CheckThrow ();
			var x509 = mono_openssl_ssl_get_peer_certificate (
				Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (x509));
		}

		public void SetVerifyParam (MonoOpenSSLX509VerifyParam param)
		{
			CheckThrow ();
			var ret = mono_openssl_ssl_set_verify_param (
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
				var ret = mono_openssl_ssl_set_server_name (
					Handle.DangerousGetHandle (), namePtr);
				CheckError (ret);
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		public string GetServerName ()
		{
			CheckThrow ();
			var namePtr = mono_openssl_ssl_get_server_name (
				Handle.DangerousGetHandle ());
			if (namePtr == IntPtr.Zero)
				return null;
			return Marshal.PtrToStringAnsi (namePtr);
		}

		public void Shutdown ()
		{
			CheckThrow ();
			var ret = mono_openssl_ssl_shutdown (Handle.DangerousGetHandle ());
			if (ret < 0)
				throw ThrowError ();
		}

		public void SetQuietShutdown ()
		{
			CheckThrow ();
			mono_openssl_ssl_set_quiet_shutdown (Handle.DangerousGetHandle (), 1);
		}

		protected override void Close ()
		{
			if (!Handle.IsInvalid)
				mono_openssl_ssl_close (Handle.DangerousGetHandle ());
		}

		public void SetRenegotiateMode (MonoOpenSSLSslRenegotiateMode mode)
		{
			CheckThrow ();
			mono_openssl_ssl_set_renegotiate_mode (Handle.DangerousGetHandle (), (int)mode);
		}

		public bool RenegotiatePending ()
		{
			return mono_openssl_ssl_renegotiate_pending (Handle.DangerousGetHandle ()) != 0;
		}
	}
}
#endif
