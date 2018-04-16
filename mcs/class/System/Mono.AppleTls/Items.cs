#if SECURITY_DEP && MONO_FEATURE_APPLETLS
// 
// Items.cs: Implements the KeyChain query access APIs
//
// We use strong types and a helper SecQuery class to simplify the
// creation of the dictionary used to query the Keychain
// 
// Authors:
//	Miguel de Icaza
//	Sebastien Pouliot
//     
// Copyright 2010 Novell, Inc
// Copyright 2011-2016 Xamarin Inc
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
using System.Collections;
using System.Runtime.InteropServices;
using ObjCRuntimeInternal;
using Mono.Net;

namespace Mono.AppleTls {

	enum SecKind {
		Identity,
		Certificate
	}

#if MONOTOUCH
	static class SecKeyChain {
#else
	class SecKeyChain : INativeObject, IDisposable {
#endif
		internal static readonly IntPtr MatchLimitAll;
		internal static readonly IntPtr MatchLimitOne;
		internal static readonly IntPtr MatchLimit;

#if !MONOTOUCH
		IntPtr handle;

		internal SecKeyChain (IntPtr handle, bool owns = false)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}
#endif

		static SecKeyChain ()
		{
			var handle = CFObject.dlopen (AppleTlsContext.SecurityLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				MatchLimit = CFObject.GetIntPtr (handle, "kSecMatchLimit");
				MatchLimitAll = CFObject.GetIntPtr (handle, "kSecMatchLimitAll");
				MatchLimitOne = CFObject.GetIntPtr (handle, "kSecMatchLimitOne");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		public static SecIdentity FindIdentity (SecCertificate certificate, bool throwOnError = false)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			var identity = FindIdentity (cert => SecCertificate.Equals (certificate, cert));
			if (!throwOnError || identity != null)
				return identity;

			throw new InvalidOperationException (string.Format ("Could not find SecIdentity for certificate '{0}' in keychain.", certificate.SubjectSummary));
		}

		static SecIdentity FindIdentity (Predicate<SecCertificate> filter)
		{
			/*
			 * Unfortunately, SecItemCopyMatching() does not allow any search
			 * filters when looking up an identity.
			 * 
			 * The following lookup will return all identities from the keychain -
			 * we then need need to find the right one.
			 */
			using (var record = new SecRecord (SecKind.Identity)) {
				SecStatusCode status;
				var result = SecKeyChain.QueryAsReference (record, -1, out status);
				if (status != SecStatusCode.Success || result == null)
					return null;

				for (int i = 0; i < result.Length; i++) {
					var identity = (SecIdentity)result [i];
					if (filter (identity.Certificate))
						return identity;
				}
			}

			return null;
		}

		static INativeObject [] QueryAsReference (SecRecord query, int max, out SecStatusCode result)
		{
			if (query == null) {
				result = SecStatusCode.Param;
				return null;
			}

			using (var copy = query.QueryDict.MutableCopy ()) {
				copy.SetValue (CFBoolean.True.Handle, SecItem.ReturnRef);
				SetLimit (copy, max);
				return QueryAsReference (copy, out result);
			}
		}

		static INativeObject [] QueryAsReference (CFDictionary query, out SecStatusCode result)
		{
			if (query == null) {
				result = SecStatusCode.Param;
				return null;
			}

			IntPtr ptr;
			result = SecItem.SecItemCopyMatching (query.Handle, out ptr);
			if (result == SecStatusCode.Success && ptr != IntPtr.Zero) {
				var array = CFArray.ArrayFromHandle<INativeObject> (ptr, p => {
					IntPtr cfType = CFType.GetTypeID (p);
					if (cfType == SecCertificate.GetTypeID ())
						return new SecCertificate (p, true);
					if (cfType == SecKey.GetTypeID ())
						return new SecKey (p, true);
					if (cfType == SecIdentity.GetTypeID ())
						return new SecIdentity (p, true);
					throw new Exception (String.Format ("Unexpected type: 0x{0:x}", cfType));
				});
				return array;
			}
			return null;
		}

		internal static CFNumber SetLimit (CFMutableDictionary dict, int max)
		{
			CFNumber n = null;
			IntPtr val;
			if (max == -1)
				val = MatchLimitAll;
			else if (max == 1)
				val = MatchLimitOne;
			else {
				n = CFNumber.FromInt32 (max);
				val = n.Handle;
			}

			dict.SetValue (val, SecKeyChain.MatchLimit);
			return n;
		}

#if !MONOTOUCH
		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* OSStatus */ SecStatusCode SecKeychainCreate (/* const char * */ IntPtr pathName, uint passwordLength, /* const void * */ IntPtr password,
									      bool promptUser, /* SecAccessRef */ IntPtr initialAccess,
									      /* SecKeychainRef  _Nullable * */ out IntPtr keychain);

		internal static SecKeyChain Create (string pathName, string password)
		{
			IntPtr handle;
			var pathNamePtr = Marshal.StringToHGlobalAnsi (pathName);
			var passwordPtr = Marshal.StringToHGlobalAnsi (password);
			var result = SecKeychainCreate (pathNamePtr, (uint)password.Length, passwordPtr, false, IntPtr.Zero, out handle);
			if (result != SecStatusCode.Success)
				throw new InvalidOperationException (result.ToString ());
			return new SecKeyChain (handle, true);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* OSStatus */ SecStatusCode SecKeychainOpen (/* const char * */ IntPtr pathName, /* SecKeychainRef  _Nullable * */ out IntPtr keychain);

		internal static SecKeyChain Open (string pathName)
		{
			IntPtr handle;
			IntPtr pathNamePtr = IntPtr.Zero;
			try {
				pathNamePtr = Marshal.StringToHGlobalAnsi (pathName);
				var result = SecKeychainOpen (pathNamePtr, out handle);
				if (result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());
				return new SecKeyChain (handle, true);
			} finally {
				if (pathNamePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (pathNamePtr);
			}
		}

		internal static SecKeyChain OpenSystemRootCertificates ()
		{
			return Open ("/System/Library/Keychains/SystemRootCertificates.keychain");
		}

		~SecKeyChain ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
#endif
	}

	class SecRecord : IDisposable {

		internal static readonly IntPtr SecClassKey;
		static SecRecord ()
		{
			var handle = CFObject.dlopen (AppleTlsContext.SecurityLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {
				SecClassKey = CFObject.GetIntPtr (handle, "kSecClass");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		CFMutableDictionary _queryDict;
		internal CFMutableDictionary QueryDict {
			get {
				return _queryDict;
			}
		}

		internal void SetValue (IntPtr key, IntPtr value)
		{
			_queryDict.SetValue (key, value);
		}

		public SecRecord (SecKind secKind)
		{
			var kind = SecClass.FromSecKind (secKind);
			_queryDict = CFMutableDictionary.Create ();
			_queryDict.SetValue (SecClassKey, kind);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (_queryDict != null){
				if (disposing){
					_queryDict.Dispose ();
					_queryDict = null;
				}
			}
		}

		~SecRecord ()
		{
			Dispose (false);
		}
	}
	
	partial class SecItem {
		public static readonly IntPtr ReturnRef;
		public static readonly IntPtr MatchSearchList;
		
		static SecItem ()
		{
			var handle = CFObject.dlopen (AppleTlsContext.SecurityLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				ReturnRef = CFObject.GetIntPtr (handle, "kSecReturnRef");
				MatchSearchList = CFObject.GetIntPtr (handle, "kSecMatchSearchList");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		internal extern static SecStatusCode SecItemCopyMatching (/* CFDictionaryRef */ IntPtr query, /* CFTypeRef* */ out IntPtr result);
	}

	static partial class SecClass {
	
		public static readonly IntPtr Identity;
		public static readonly IntPtr Certificate;
		
		static SecClass ()
		{
			var handle = CFObject.dlopen (AppleTlsContext.SecurityLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {
				Identity = CFObject.GetIntPtr (handle, "kSecClassIdentity");
				Certificate = CFObject.GetIntPtr (handle, "kSecClassCertificate");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		public static IntPtr FromSecKind (SecKind secKind)
		{
			switch (secKind){
			case SecKind.Identity:
				return Identity;
			case SecKind.Certificate:
				return Certificate;
			default:
				throw new ArgumentException ("secKind");
			}
		}
	}
}
#endif
