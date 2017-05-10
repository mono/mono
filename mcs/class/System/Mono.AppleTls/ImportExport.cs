#if SECURITY_DEP && MONO_FEATURE_APPLETLS
// 
// ImportExport.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//     
// Copyright 2011-2014 Xamarin Inc.
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

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ObjCRuntimeInternal;
using Mono.Net;

#if MONO_FEATURE_BTLS
using Mono.Btls;
#else
using Mono.Security.Cryptography;
#endif

namespace Mono.AppleTls {

	internal partial class SecImportExport {
		
		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode SecPKCS12Import (IntPtr pkcs12_data, IntPtr options, out IntPtr items);
		
		static public SecStatusCode ImportPkcs12 (byte[] buffer, CFDictionary options, out CFDictionary[] array)
		{
			using (CFData data = CFData.FromData (buffer)) {
				return ImportPkcs12 (data, options, out array);
			}
		}

		static public SecStatusCode ImportPkcs12 (CFData data, CFDictionary options, out CFDictionary [] array)
		{
			if (options == null)
				throw new ArgumentNullException ("options");
			
			IntPtr handle;
			SecStatusCode code = SecPKCS12Import (data.Handle, options.Handle, out handle);
			array = CFArray.ArrayFromHandle <CFDictionary> (handle, h => new CFDictionary (h, false));
			if (handle != IntPtr.Zero)
				CFObject.CFRelease (handle);
			return code;
		}

#if !MONOTOUCH
		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode SecItemImport (
			/* CFDataRef */ IntPtr importedData,
			/* CFStringRef */ IntPtr fileNameOrExtension, // optional
			/* SecExternalFormat* */ ref SecExternalFormat inputFormat, // optional, IN/OUT
			/* SecExternalItemType* */ ref SecExternalItemType itemType, // optional, IN/OUT
			/* SecItemImportExportFlags */ SecItemImportExportFlags flags,
			/* const SecItemImportExportKeyParameters* */ IntPtr keyParams, // optional
			/* SecKeychainRef */ IntPtr importKeychain, // optional
			/* CFArrayRef* */ out IntPtr outItems);

		static public CFArray ItemImport (byte[] buffer, string password)
		{
			using (var data = CFData.FromData (buffer))
			using (var pwstring = CFString.Create (password)) {
				SecItemImportExportKeyParameters keyParams = new SecItemImportExportKeyParameters ();
				keyParams.passphrase = pwstring.Handle;

				return ItemImport (data, SecExternalFormat.PKCS12, SecExternalItemType.Aggregate, SecItemImportExportFlags.None, keyParams);
			}
		}

		static CFArray ItemImport (CFData data, SecExternalFormat format, SecExternalItemType itemType,
		                           SecItemImportExportFlags flags = SecItemImportExportFlags.None,
		                           SecItemImportExportKeyParameters? keyParams = null)
		{
			return ItemImport (data, ref format, ref itemType, flags, keyParams);
		}

		static CFArray ItemImport (CFData data, ref SecExternalFormat format, ref SecExternalItemType itemType,
		                           SecItemImportExportFlags flags = SecItemImportExportFlags.None,
		                           SecItemImportExportKeyParameters? keyParams = null)
		{
			IntPtr keyParamsPtr = IntPtr.Zero;
			if (keyParams != null) {
				keyParamsPtr = Marshal.AllocHGlobal (Marshal.SizeOf (keyParams.Value));
				if (keyParamsPtr == IntPtr.Zero)
					throw new OutOfMemoryException ();
				Marshal.StructureToPtr (keyParams.Value, keyParamsPtr, false);
			}

			IntPtr result;
			var status = SecItemImport (data.Handle, IntPtr.Zero, ref format, ref itemType, flags, keyParamsPtr, IntPtr.Zero, out result);

			if (keyParamsPtr != IntPtr.Zero)
				Marshal.FreeHGlobal (keyParamsPtr);

			if (status != SecStatusCode.Success)
				throw new NotSupportedException (status.ToString ());

			return new CFArray (result, true);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* SecIdentityRef */ IntPtr SecIdentityCreate (
			/* CFAllocatorRef */ IntPtr allocator,
			/* SecCertificateRef */ IntPtr certificate,
			/* SecKeyRef */ IntPtr privateKey);

		static public SecIdentity ItemImport (X509Certificate2 certificate)
		{
			if (!certificate.HasPrivateKey)
				throw new NotSupportedException ();

			using (var key = ImportPrivateKey (certificate))
			using (var cert = new SecCertificate (certificate)) {
				var identity = SecIdentityCreate (IntPtr.Zero, cert.Handle, key.Handle);
				if (CFType.GetTypeID (identity) != SecIdentity.GetTypeID ())
					throw new InvalidOperationException ();

				return new SecIdentity (identity, true);
			}
		}

		static byte[] ExportKey (RSA key)
		{
#if MONO_FEATURE_BTLS
			using (var btlsKey = MonoBtlsKey.CreateFromRSAPrivateKey (key))
				return btlsKey.GetBytes (true);
#else
			return PKCS8.PrivateKeyInfo.Encode (key);
#endif
		}

		static SecKey ImportPrivateKey (X509Certificate2 certificate)
		{
			if (!certificate.HasPrivateKey)
				throw new NotSupportedException ();

			CFArray items;
			using (var data = CFData.FromData (ExportKey ((RSA)certificate.PrivateKey)))
				items = ItemImport (data, SecExternalFormat.OpenSSL, SecExternalItemType.PrivateKey);

			try {
				if (items.Count != 1)
					throw new InvalidOperationException ("Private key import failed.");

				var imported = items[0];
				if (CFType.GetTypeID (imported) != SecKey.GetTypeID ())
					throw new InvalidOperationException ("Private key import doesn't return SecKey.");

				return new SecKey (imported, items.Handle);
			} finally {
				items.Dispose ();
			}
		}

		const int SEC_KEY_IMPORT_EXPORT_PARAMS_VERSION = 0;

		// Native enum; don't change.
		enum SecExternalFormat : int {
			Unknown = 0,
			OpenSSL = 1,
			X509Cert = 9,
			PEMSequence = 10,
			PKCS7 = 11,
			PKCS12 = 12
		}

		// Native enum; don't change.
		enum SecExternalItemType : int {
			Unknown = 0,
			PrivateKey = 1,
			PublicKey = 2,
			SessionKey = 3,
			Certificate = 4,
			Aggregate = 5
		}

		// Native enum; don't change
		enum SecItemImportExportFlags : int {
			None,
			PemArmour = 0x00000001,   /* exported blob is PEM formatted */
		}

		// Native struct; don't change
		[StructLayout (LayoutKind.Sequential)]
		struct SecItemImportExportKeyParameters {
			public int version;            /* SEC_KEY_IMPORT_EXPORT_PARAMS_VERSION */
			public int flags;              /* SecKeyImportExportFlags bits */
			public IntPtr passphrase;      /* SecExternalFormat.PKCS12 only.  Legal types are CFStringRef and CFDataRef. */

			IntPtr alertTitle;
			IntPtr alertPrompt;

			public IntPtr accessRef;       /* SecAccessRef */

			IntPtr keyUsage;
			IntPtr keyAttributes;
		}
#endif
	}
}
#endif
