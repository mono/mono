//
// MonoBtlsX509StoreCtx.cs
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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Btls
{
	class MonoBtlsX509StoreCtx : MonoBtlsObject
	{
		internal class BoringX509StoreCtxHandle : MonoBtlsHandle
		{
			bool dontFree;

			internal BoringX509StoreCtxHandle (IntPtr handle, bool ownsHandle = true)
				: base (handle, ownsHandle)
			{
				dontFree = !ownsHandle;
			}

			#if FIXME
			internal BoringX509StoreCtxHandle (IntPtr handle)
				: base ()
			{
				base.handle = handle;
				this.dontFree = true;
			}
			#endif

			protected override bool ReleaseHandle ()
			{
				if (!dontFree)
					mono_btls_x509_store_ctx_free (handle);
				return true;
			}
		}

		int? verifyResult;

		new internal BoringX509StoreCtxHandle Handle {
			get { return (BoringX509StoreCtxHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_new ();

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_from_ptr (IntPtr ctx);

		[DllImport (BTLS_DYLIB)]
		extern static MonoBtlsX509Error mono_btls_x509_store_ctx_get_error (IntPtr handle, out IntPtr error_string);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_ctx_get_error_depth (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_chain (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_ctx_init (IntPtr handle, IntPtr store, IntPtr chain);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_ctx_set_param (IntPtr handle, IntPtr param);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_ctx_verify_cert (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_by_subject (IntPtr handle, IntPtr name);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_current_cert (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_current_issuer (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_verify_param (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_get_untrusted (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_ctx_up_ref (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_store_ctx_free (IntPtr handle);

		internal MonoBtlsX509StoreCtx ()
			: base (new BoringX509StoreCtxHandle (mono_btls_x509_store_ctx_new ()))
		{
		}

		static BoringX509StoreCtxHandle Create_internal (IntPtr store_ctx)
		{
			var handle = mono_btls_x509_store_ctx_from_ptr (store_ctx);
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509StoreCtxHandle (handle);
		}

		internal MonoBtlsX509StoreCtx (int preverify_ok, IntPtr store_ctx)
			: base (Create_internal (store_ctx))
		{
			verifyResult = preverify_ok;
		}

		internal MonoBtlsX509StoreCtx (BoringX509StoreCtxHandle ptr, int? verifyResult)
			: base (ptr)
		{
			this.verifyResult = verifyResult;
		}

		public MonoBtlsX509Error GetError ()
		{
			IntPtr error_string_ptr;
			return mono_btls_x509_store_ctx_get_error (Handle.DangerousGetHandle (), out error_string_ptr);
		}

		public int GetErrorDepth ()
		{
			return mono_btls_x509_store_ctx_get_error_depth (Handle.DangerousGetHandle ());
		}

		public MonoBtlsX509Exception GetException ()
		{
			IntPtr error_string_ptr;
			var error = mono_btls_x509_store_ctx_get_error (Handle.DangerousGetHandle (), out error_string_ptr);
			if (error == 0)
				return null;
			if (error_string_ptr != IntPtr.Zero) {
				var error_string = Marshal.PtrToStringAnsi (error_string_ptr);
				return new MonoBtlsX509Exception (error, error_string);
			}
			return new MonoBtlsX509Exception (error, "Unknown verify error.");
		}

		public MonoBtlsX509Chain GetChain ()
		{
			var chain = mono_btls_x509_store_ctx_get_chain (Handle.DangerousGetHandle ());
			CheckError (chain != IntPtr.Zero);
			return new MonoBtlsX509Chain (new MonoBtlsX509Chain.BoringX509ChainHandle (chain));
		}

		public MonoBtlsX509Chain GetUntrusted ()
		{
			var chain = mono_btls_x509_store_ctx_get_untrusted (Handle.DangerousGetHandle ());
			CheckError (chain != IntPtr.Zero);
			return new MonoBtlsX509Chain (new MonoBtlsX509Chain.BoringX509ChainHandle (chain));
		}

		public void Initialize (MonoBtlsX509Store store, MonoBtlsX509Chain chain)
		{
			var ret = mono_btls_x509_store_ctx_init (
				Handle.DangerousGetHandle (),
				store.Handle.DangerousGetHandle (),
				chain.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public void SetVerifyParam (MonoBtlsX509VerifyParam param)
		{
			var ret = mono_btls_x509_store_ctx_set_param (
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
			verifyResult = mono_btls_x509_store_ctx_verify_cert (Handle.DangerousGetHandle ());
			return verifyResult.Value;
		}

		public MonoBtlsX509 LookupBySubject (MonoBtlsX509Name name)
		{
			var handle = mono_btls_x509_store_ctx_get_by_subject (
				Handle.DangerousGetHandle (), name.Handle.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				return null;
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (handle));
		}

		public MonoBtlsX509 GetCurrentCertificate ()
		{
			var x509 = mono_btls_x509_store_ctx_get_current_cert (Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (x509));
		}

		public MonoBtlsX509 GetCurrentIssuer ()
		{
			var x509 = mono_btls_x509_store_ctx_get_current_issuer (Handle.DangerousGetHandle ());
			if (x509 == IntPtr.Zero)
				return null;
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (x509));
		}

		public MonoBtlsX509VerifyParam GetVerifyParam ()
		{
			var param = mono_btls_x509_store_ctx_get_verify_param (Handle.DangerousGetHandle ());
			if (param == IntPtr.Zero)
				return null;
			return new MonoBtlsX509VerifyParam (new MonoBtlsX509VerifyParam.BoringX509VerifyParamHandle (param));
		}

		public MonoBtlsX509StoreCtx Copy ()
		{
			var copy = mono_btls_x509_store_ctx_up_ref (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoBtlsX509StoreCtx (new BoringX509StoreCtxHandle (copy), verifyResult);
		}
	}
}
#endif
