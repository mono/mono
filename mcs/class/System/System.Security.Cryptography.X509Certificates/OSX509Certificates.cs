// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if SECURITY_DEP
using System;
using System.Runtime.InteropServices;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {

	internal class OSX509Certificates {
		public const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";
		public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
	
		[DllImport (SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr nsdataRef);
		
		[DllImport (SecurityLibrary)]
		extern static int SecTrustCreateWithCertificates (IntPtr certOrCertArray, IntPtr policies, out IntPtr sectrustref);
		
		[DllImport (SecurityLibrary)]
		extern static IntPtr SecPolicyCreateSSL (int server, IntPtr cfStringHostname);
		
		[DllImport (SecurityLibrary)]
		extern static int SecTrustEvaluate (IntPtr secTrustRef, out SecTrustResult secTrustResultTime);

		[DllImport (CoreFoundationLibrary)]
		unsafe extern static IntPtr CFDataCreate (IntPtr allocator, byte *bytes, IntPtr length);

		[DllImport (CoreFoundationLibrary)]
		unsafe extern static void CFRelease (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayCreate (IntPtr allocator, IntPtr values, IntPtr numValues, IntPtr callbacks);
		
		public enum SecTrustResult {
			Invalid,
			Proceed,
			Confirm,
			Deny,
			Unspecified,
			RecoverableTrustFailure,
			FatalTrustFailure,
			ResultOtherError,
		}

		static IntPtr sslsecpolicy = SecPolicyCreateSSL (0, IntPtr.Zero);

		static IntPtr MakeCFData (byte [] data)
		{
			unsafe {
				fixed (byte *ptr = &data [0])
					return CFDataCreate (IntPtr.Zero, ptr, (IntPtr) data.Length);
			}
		}

		static unsafe IntPtr FromIntPtrs (IntPtr [] values)
		{
			fixed (IntPtr* pv = values) {
				return CFArrayCreate (
					IntPtr.Zero, 
					(IntPtr) pv,
					(IntPtr) values.Length,
					IntPtr.Zero);
			}
		}
		
		public static SecTrustResult TrustEvaluateSsl (X509CertificateCollection certificates)
		{
			try {
				return _TrustEvaluateSsl (certificates);
			} catch {
				return SecTrustResult.Deny;
			}
		}
		
		static SecTrustResult _TrustEvaluateSsl (X509CertificateCollection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			int certCount = certificates.Count;
			IntPtr [] cfDataPtrs = new IntPtr [certCount];
			IntPtr [] secCerts = new IntPtr [certCount];
			IntPtr certArray = IntPtr.Zero;
			
			try {
				for (int i = 0; i < certCount; i++)
					cfDataPtrs [i] = MakeCFData (certificates [i].RawData);
				
				for (int i = 0; i < certCount; i++){
					secCerts [i] = SecCertificateCreateWithData (IntPtr.Zero, cfDataPtrs [i]);
					if (secCerts [i] == IntPtr.Zero){
						CFRelease (cfDataPtrs [i]);
						return SecTrustResult.Deny;
					}
				}
				certArray = FromIntPtrs (secCerts);
				IntPtr sectrust;
				int code = SecTrustCreateWithCertificates (certArray, sslsecpolicy, out sectrust);
				if (code == 0){
					SecTrustResult result;
					code = SecTrustEvaluate (sectrust, out result);
					if (code != 0)
						return SecTrustResult.Deny;

					CFRelease (sectrust);
					CFRelease (sslsecpolicy);
					
					return result;
				}
				return SecTrustResult.Deny;
			} finally {
				for (int i = 0; i < certCount; i++)
					if (secCerts [i] != IntPtr.Zero)
						CFRelease (cfDataPtrs [i]);

				if (certArray != IntPtr.Zero)
					CFRelease (certArray);
			}
		}
	}
}
#endif
