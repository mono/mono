//
// MonoCertificatePal.cs
//
// Authors:
//       Miguel de Icaza
//       Sebastien Pouliot <sebastien@xamarin.com>
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright 2010 Novell, Inc
// Copyright 2011-2014 Xamarin Inc.
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Apple;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;

namespace Mono.AppleTls
{
	static partial class MonoCertificatePal
	{
		const string SecurityLibrary = OSX509Certificates.SecurityLibrary;

		[DllImport (SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr cfData);

		public static SafeSecCertificateHandle FromOtherCertificate (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));
			return FromOtherCertificate (certificate.Impl);
		}

		public static SafeSecCertificateHandle FromOtherCertificate (X509CertificateImpl impl)
		{
			X509Helper.ThrowIfContextInvalid (impl);

			var handle = impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero)
				return new SafeSecCertificateHandle (handle, false);

			using (var data = CFData.FromData (impl.RawData)) {
				handle = SecCertificateCreateWithData (IntPtr.Zero, data.Handle);
				if (handle == IntPtr.Zero)
					throw new ArgumentException ("Not a valid DER-encoded X.509 certificate");

				return new SafeSecCertificateHandle (handle, true);
			}
		}

		[DllImport (SecurityLibrary)]
		extern static IntPtr SecIdentityGetTypeID ();

		public static bool IsSecIdentity (IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return false;
			return CFType.GetTypeID (ptr) == SecIdentityGetTypeID ();
		}

		[DllImport (SecurityLibrary)]
		public extern static IntPtr SecKeyGetTypeID ();
		
		public static bool IsSecKey (IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return false;
			return CFType.GetTypeID (ptr) == SecKeyGetTypeID ();
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SecStatusCode SecIdentityCopyCertificate (/* SecIdentityRef */ IntPtr identityRef,  /* SecCertificateRef* */ out IntPtr certificateRef);

		public static SafeSecCertificateHandle GetCertificate (SafeSecIdentityHandle identity)
		{
			if (identity == null || identity.IsInvalid)
				throw new ArgumentNullException (nameof (identity));
			var result = SecIdentityCopyCertificate (identity.DangerousGetHandle (), out var cert);
			if (result != SecStatusCode.Success)
				throw new InvalidOperationException (result.ToString ());
			return new SafeSecCertificateHandle (cert, true);
		}

		[DllImport (SecurityLibrary)]
		extern static IntPtr SecCertificateCopySubjectSummary (IntPtr cert);

		public static string GetSubjectSummary (SafeSecCertificateHandle certificate)
		{
			if (certificate == null || certificate.IsInvalid)
				throw new ArgumentNullException (nameof (certificate));

			var subjectSummaryHandle = IntPtr.Zero;
			try {
				subjectSummaryHandle = SecCertificateCopySubjectSummary (certificate.DangerousGetHandle ());
				return CFString.AsString (subjectSummaryHandle);
			} finally {
				if (subjectSummaryHandle != IntPtr.Zero)
					CFObject.CFRelease (subjectSummaryHandle);
			}
		}

		[DllImport (SecurityLibrary)]
		extern static /* CFDataRef */ IntPtr SecCertificateCopyData (/* SecCertificateRef */ IntPtr cert);

		public static byte[] GetRawData (SafeSecCertificateHandle certificate)
		{
			if (certificate == null || certificate.IsInvalid)
				throw new ArgumentNullException (nameof (certificate));

			var dataPtr = SecCertificateCopyData (certificate.DangerousGetHandle ());
			if (dataPtr == IntPtr.Zero)
				throw new ArgumentException ("Not a valid certificate");

			using (var data = new CFData (dataPtr, true)) {
				var buffer = new byte[(int)data.Length];
				Marshal.Copy (data.Bytes, buffer, 0, buffer.Length);
				return buffer;
			}
		}


		public static bool Equals (SafeSecCertificateHandle first, SafeSecCertificateHandle second)
		{
			/*
			 * This is a little bit expensive, but unfortunately there is no better API to compare two
			 * SecCertificateRef's for equality.
			 */
			if (first == null || first.IsInvalid)
				throw new ArgumentNullException (nameof (first));
			if (second == null || second.IsInvalid)
				throw new ArgumentNullException (nameof (second));
			if (first.DangerousGetHandle () == second.DangerousGetHandle ())
				return true;

			var firstDataPtr = SecCertificateCopyData (first.DangerousGetHandle ());
			var secondDataPtr = SecCertificateCopyData (first.DangerousGetHandle ());

			try {
				if (firstDataPtr == IntPtr.Zero || secondDataPtr == IntPtr.Zero)
					throw new ArgumentException ("Not a valid certificate.");
				if (firstDataPtr == secondDataPtr)
					return true;

				var firstLength = (int)CFData.CFDataGetLength (firstDataPtr);
				var secondLength = (int)CFData.CFDataGetLength (secondDataPtr);
				if (firstLength != secondLength)
					return false;

				var firstBytePtr = CFData.CFDataGetBytePtr (firstDataPtr);
				var secondBytePtr = CFData.CFDataGetBytePtr (secondDataPtr);
				if (firstBytePtr == secondBytePtr)
					return true;

				var firstBuffer = new byte[firstLength];
				var secondBuffer = new byte[secondLength];
				Marshal.Copy (firstBytePtr, firstBuffer, 0, firstBuffer.Length);
				Marshal.Copy (secondBytePtr, secondBuffer, 0, secondBuffer.Length);

				for (int i = 0; i < firstBuffer.Length; i++)
					if (firstBuffer[i] != secondBuffer[i])
						return false;

				return true;
			} finally {
				if (firstDataPtr != IntPtr.Zero)
					CFObject.CFRelease (firstDataPtr);
				if (secondDataPtr != IntPtr.Zero)
					CFObject.CFRelease (secondDataPtr);
			}
		}
	}
}
