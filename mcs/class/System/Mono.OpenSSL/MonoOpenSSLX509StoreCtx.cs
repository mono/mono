//
// MonoOpenSSLX509StoreCtx.cs
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
#if SECURITY_DEP && MONO_FEATURE_OPENSSL
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.OpenSSL
{
	class MonoOpenSSLX509StoreCtx : MonoOpenSSLObject
	{
		internal class OpenSSLX509StoreCtxHandle : MonoOpenSSLHandle
		{
			bool dontFree;

			internal OpenSSLX509StoreCtxHandle (IntPtr handle, bool ownsHandle = true)
				: base (handle, ownsHandle)
			{
				dontFree = !ownsHandle;
			}

			#if FIXME
			internal OpenSSLX509StoreCtxHandle (IntPtr handle)
				: base ()
			{
				base.handle = handle;
				this.dontFree = true;
			}
			#endif

			protected override bool ReleaseHandle ()
			{
				if (!dontFree) 
					mono_openssl_x509_store_ctx_free (handle);
				return true;
			}
		}

		int? verifyResult;

		new internal OpenSSLX509StoreCtxHandle Handle {
			get { return (OpenSSLX509StoreCtxHandle)base.Handle; }
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_new ();

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_from_ptr (IntPtr ctx);

		[DllImport (OPENSSL_DYLIB)]
		extern static MonoOpenSSLX509Error mono_openssl_x509_store_ctx_get_error (IntPtr handle, out IntPtr error_string);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_ctx_get_error_depth (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_chain (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_ctx_init (IntPtr handle, IntPtr store, IntPtr chain);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_ctx_set_param (IntPtr handle, IntPtr param);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_ctx_verify_cert (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_by_subject (IntPtr handle, IntPtr name);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_current_cert (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_current_issuer (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_verify_param (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_get_untrusted (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_ctx_up_ref (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_store_ctx_free (IntPtr handle);

		internal MonoOpenSSLX509StoreCtx ()
			: base (new OpenSSLX509StoreCtxHandle (mono_openssl_x509_store_ctx_new ()))
		{
		}

		static OpenSSLX509StoreCtxHandle Create_internal (IntPtr store_ctx)
		{
			var handle = mono_openssl_x509_store_ctx_from_ptr (store_ctx);
			if (handle == IntPtr.Zero)
				throw new MonoOpenSSLException ();
			return new OpenSSLX509StoreCtxHandle (handle);
		}

		internal MonoOpenSSLX509StoreCtx (int preverify_ok, IntPtr store_ctx)
			: base (Create_internal (store_ctx))
		{
			verifyResult = preverify_ok;
		}

		internal MonoOpenSSLX509StoreCtx (OpenSSLX509StoreCtxHandle ptr, int? verifyResult)
			: base (ptr)
		{
			this.verifyResult = verifyResult;
		}

		public MonoOpenSSLX509Error GetError ()
		{
			IntPtr error_string_ptr;
			return mono_openssl_x509_store_ctx_get_error (Handle.DangerousGetHandle (), out error_string_ptr);
		}

		public int GetErrorDepth ()
		{
			return mono_openssl_x509_store_ctx_get_error_depth (Handle.DangerousGetHandle ());
		}

		public MonoOpenSSLX509Exception GetException ()
		{
			IntPtr error_string_ptr;
			var error = mono_openssl_x509_store_ctx_get_error (Handle.DangerousGetHandle (), out error_string_ptr);
			if (error == 0)
				return null;
			if (error_string_ptr != IntPtr.Zero) {
				var error_string = Marshal.PtrToStringAnsi (error_string_ptr);
				return new MonoOpenSSLX509Exception (error, error_string);
			}
			return new MonoOpenSSLX509Exception (error, "Unknown verify error.");
		}

		public MonoOpenSSLX509Chain GetChain ()
		{
			var chain = mono_openssl_x509_store_ctx_get_chain (Handle.DangerousGetHandle ());
			CheckError (chain != IntPtr.Zero);
			return new MonoOpenSSLX509Chain (new MonoOpenSSLX509Chain.OpenSSLX509ChainHandle (chain));
		}

		public MonoOpenSSLX509Chain GetUntrusted ()
		{
			var chain = mono_openssl_x509_store_ctx_get_untrusted (Handle.DangerousGetHandle ());
			CheckError (chain != IntPtr.Zero);
			return new MonoOpenSSLX509Chain (new MonoOpenSSLX509Chain.OpenSSLX509ChainHandle (chain));
		}

		public void Initialize (MonoOpenSSLX509Store store, MonoOpenSSLX509Chain chain)
		{
			var ret = mono_openssl_x509_store_ctx_init (
				Handle.DangerousGetHandle (),
				store.Handle.DangerousGetHandle (),
				chain.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public void SetVerifyParam (MonoOpenSSLX509VerifyParam param)
		{
			var ret = mono_openssl_x509_store_ctx_set_param (
				Handle.DangerousGetHandle (),
				param.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public int VerifyResult {
			get {
				if (verifyResult == null)
					throw new InvalidOperationException ();
				return verifyResult.Value;
			}
		}

		public int Verify ()
		{
			verifyResult = mono_openssl_x509_store_ctx_verify_cert (Handle.DangerousGetHandle ());
			return verifyResult.Value;
		}

		public MonoOpenSSLX509 LookupBySubject (MonoOpenSSLX509Name name)
		{
			var handle = mono_openssl_x509_store_ctx_get_by_subject (
				Handle.DangerousGetHandle (), name.Handle.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (handle));
		}

		public MonoOpenSSLX509 GetCurrentCertificate ()
		{
			var x509 = mono_openssl_x509_store_ctx_get_current_cert (Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (x509));
		}

		public MonoOpenSSLX509 GetCurrentIssuer ()
		{
			var x509 = mono_openssl_x509_store_ctx_get_current_issuer (Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (x509));
		}

		public MonoOpenSSLX509VerifyParam GetVerifyParam ()
		{
			var param = mono_openssl_x509_store_ctx_get_verify_param (Handle.DangerousGetHandle ());
			if (param == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509VerifyParam (new MonoOpenSSLX509VerifyParam.OpenSSLX509VerifyParamHandle (param));
		}

		public MonoOpenSSLX509StoreCtx Copy ()
		{
			var copy = mono_openssl_x509_store_ctx_up_ref (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoOpenSSLX509StoreCtx (new OpenSSLX509StoreCtxHandle (copy), verifyResult);
		}
	}
}
#endif
