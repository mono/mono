//
// MonoBtlsObject.cs
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
using System.Threading;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Btls
{
	abstract class MonoBtlsObject : IDisposable
	{
		internal const string BTLS_DYLIB = "libmono-btls-shared";

		internal MonoBtlsObject (MonoBtlsHandle handle)
		{
			this.handle = handle;
		}

		protected internal abstract class MonoBtlsHandle : SafeHandle
		{
			internal MonoBtlsHandle ()
				: base (IntPtr.Zero, true)
			{
			}

			internal MonoBtlsHandle (IntPtr handle, bool ownsHandle)
				: base (handle, ownsHandle)
			{
			}

			public override bool IsInvalid {
				get { return handle == IntPtr.Zero; }
			}
		}

		internal MonoBtlsHandle Handle {
			get {
				CheckThrow ();
				return handle;
			}
		}

		public bool IsValid {
			get { return handle != null && !handle.IsInvalid; }
		}

		MonoBtlsHandle handle;
		Exception lastError;

		protected void CheckThrow ()
		{
			if (lastError != null)
				throw lastError;
			if (handle == null || handle.IsInvalid)
				throw new ObjectDisposedException ("MonoBtlsSsl");
		}

		protected Exception SetException (Exception ex)
		{
			if (lastError == null)
				lastError = ex;
			return ex;
		}

		protected void CheckError (bool ok, [CallerMemberName] string callerName = null)
		{
			if (!ok) {
				if (callerName != null)
					throw new CryptographicException ($"`{GetType ().Name}.{callerName}` failed.");
				else
					throw new CryptographicException ();
			}
		}

		protected void CheckError (int ret, [CallerMemberName] string callerName = null)
		{
			CheckError (ret == 1, callerName);
		}

		protected internal void CheckLastError ([CallerMemberName] string callerName = null)
		{
			var error = Interlocked.Exchange (ref lastError, null);
			if (error == null)
				return;

			if (error is AuthenticationException || error is NotSupportedException)
				throw error;

			string message;
			if (callerName != null)
				message = $"Caught unhandled exception in `{GetType ().Name}.{callerName}`.";
			else
				message = "Caught unhandled exception.";
			throw new CryptographicException (message, error);
		}

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_free (IntPtr data);

		protected void FreeDataPtr (IntPtr data)
		{
			mono_btls_free (data);
		}

		protected virtual void Close ()
		{
		}

		protected void Dispose (bool disposing)
		{
			if (disposing) {
				try {
					if (handle != null) {
						Close ();
						handle.Dispose ();
						handle = null;
					}
				} finally {
					var disposedExc = new ObjectDisposedException (GetType ().Name);
					Interlocked.CompareExchange (ref lastError, disposedExc, null);
				}
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~MonoBtlsObject ()
		{
			Dispose (false);
		}
	}
}
#endif
