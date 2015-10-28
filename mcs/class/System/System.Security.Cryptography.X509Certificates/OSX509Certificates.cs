// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
// Copyright 2012-2014 Xamarin Inc.
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

#if MONOTOUCH || MONODROID
using MSX = Mono.Security.X509;
#else
extern alias MonoSecurity;
using MSX = MonoSecurity::Mono.Security.X509;
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography.X509Certificates {

	static class OSX509Certificates {
		public const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";
		public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
	
		[DllImport (SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr nsdataRef);
		
		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ int SecTrustCreateWithCertificates (IntPtr certOrCertArray, IntPtr policies, out IntPtr sectrustref);
		
		[DllImport (SecurityLibrary)]
		extern static IntPtr SecPolicyCreateSSL ([MarshalAs (UnmanagedType.I1)] bool server, IntPtr cfStringHostname);
		
		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ int SecTrustEvaluate (IntPtr secTrustRef, out SecTrustResult secTrustResultTime);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, /* CFIndex */ IntPtr count);

		[DllImport (CoreFoundationLibrary)]
		unsafe extern static IntPtr CFDataCreate (IntPtr allocator, byte *bytes, /* CFIndex */ IntPtr length);

		[DllImport (CoreFoundationLibrary)]
		extern static void CFRelease (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayCreate (IntPtr allocator, IntPtr values, /* CFIndex */ IntPtr numValues, IntPtr callbacks);

		// uint32_t
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

		static IList<byte[]> CreateArray (X509Certificate2Collection certificates)
		{
			var list = new List<byte[]> (certificates.Count);
			for (int i = 0; i < certificates.Count; i++)
				list.Add (certificates [i].RawData);
			return list;
		}
		
		static IList<byte[]> CreateArray (MSX.X509CertificateCollection certificates)
		{
			var list = new List<byte[]> (certificates.Count);
			for (int i = 0; i < certificates.Count; i++)
				list.Add (certificates [i].RawData);
			return list;
		}

		public static SecTrustResult TrustEvaluateSsl (MSX.X509CertificateCollection certificates, string host)
		{
			if (certificates == null)
				return SecTrustResult.Deny;

			try {
				var certArray = CreateArray (certificates);
				return _TrustEvaluateSsl (certArray, host);
			} catch {
				return SecTrustResult.Deny;
			}
		}

		public static SecTrustResult TrustEvaluateSsl (X509Certificate2Collection certificates, string host)
		{
			if (certificates == null)
				return SecTrustResult.Deny;

			try {
				var certArray = CreateArray (certificates);
				return _TrustEvaluateSsl (certArray, host);
			} catch {
				return SecTrustResult.Deny;
			}
		}

		static SecTrustResult _TrustEvaluateSsl (IList<byte[]> certificates, string hostName)
		{
			int certCount = certificates.Count;
			IntPtr [] cfDataPtrs = new IntPtr [certCount];
			IntPtr [] secCerts = new IntPtr [certCount];
			IntPtr certArray = IntPtr.Zero;
			IntPtr sslsecpolicy = IntPtr.Zero;
			IntPtr host = IntPtr.Zero;
			IntPtr sectrust = IntPtr.Zero;
			SecTrustResult result = SecTrustResult.Deny;

			try {
				for (int i = 0; i < certCount; i++)
					cfDataPtrs [i] = MakeCFData (certificates [i]);
				
				for (int i = 0; i < certCount; i++){
					secCerts [i] = SecCertificateCreateWithData (IntPtr.Zero, cfDataPtrs [i]);
					if (secCerts [i] == IntPtr.Zero)
						return SecTrustResult.Deny;
				}
				certArray = FromIntPtrs (secCerts);
				host = CFStringCreateWithCharacters (IntPtr.Zero, hostName, (IntPtr) hostName.Length);
				sslsecpolicy = SecPolicyCreateSSL (true, host);

				int code = SecTrustCreateWithCertificates (certArray, sslsecpolicy, out sectrust);
				if (code == 0)
					code = SecTrustEvaluate (sectrust, out result);
				return result;
			} finally {
				for (int i = 0; i < certCount; i++)
					if (cfDataPtrs [i] != IntPtr.Zero)
						CFRelease (cfDataPtrs [i]);

				if (certArray != IntPtr.Zero)
					CFRelease (certArray);
				
				for (int i = 0; i < certCount; i++)
					if (secCerts [i] != IntPtr.Zero)
						CFRelease (secCerts [i]);

				if (sslsecpolicy != IntPtr.Zero)
					CFRelease (sslsecpolicy);
				if (host != IntPtr.Zero)
					CFRelease (host);
				if (sectrust != IntPtr.Zero)
					CFRelease (sectrust);
			}
		}
	}
}
#endif
