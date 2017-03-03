#if MONO_FEATURE_APPLETLS
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
using ObjCRuntime;
using Mono.Net;

namespace Mono.AppleTls {

	enum SecKind {
		Identity
	}

	static class SecKeyChain {
		static readonly IntPtr MatchLimitAll;
		static readonly IntPtr MatchLimitOne;
		static readonly IntPtr MatchLimit;

		static SecKeyChain ()
		{
			var handle = CFObject.dlopen ("/System/Library/Frameworks/Security.framework/Security", 0);
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
		
		public static INativeObject[] QueryAsReference (SecRecord query, int max, out SecStatusCode result)
		{
			if (query == null){
				result = SecStatusCode.Param;
				return null;
			}

			using (var copy = query.queryDict.MutableCopy ()) {
				copy.SetValue (CFBoolean.True.Handle, SecItem.ReturnRef);
				SetLimit (copy, max);

				IntPtr ptr;
				result = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				if ((result == SecStatusCode.Success) && (ptr != IntPtr.Zero)) {
					var array = CFArray.ArrayFromHandle<INativeObject> (ptr, p => {
						IntPtr cfType = CFType.GetTypeID (p);
						if (cfType == SecCertificate.GetTypeID ())
							return new SecCertificate (p, true);
						else if (cfType == SecKey.GetTypeID ())
							return new SecKey (p, true);
						else if (cfType == SecIdentity.GetTypeID ())
							return new SecIdentity (p, true);
						else
							throw new Exception (String.Format ("Unexpected type: 0x{0:x}", cfType));
					});
					return array;
				}
				return null;
			}
		}

		static CFNumber SetLimit (CFMutableDictionary dict, int max)
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
	}
	
	class SecRecord : IDisposable {

		static readonly IntPtr SecClassKey;
		static SecRecord ()
		{
			var handle = CFObject.dlopen ("/System/Library/Frameworks/Security.framework/Security", 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				SecClassKey = CFObject.GetIntPtr (handle, "kSecClassKey");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		// Fix <= iOS 6 Behaviour - Desk #83099
		// NSCFDictionary: mutating method sent to immutable object
		// iOS 6 returns an inmutable NSDictionary handle and when we try to set its values it goes kaboom
		// By explicitly calling `MutableCopy` we ensure we always have a mutable reference we expect that.
		CFDictionary _queryDict;
		internal CFDictionary queryDict 
		{ 
			get {
				return _queryDict;
			}
			set {
				_queryDict = value != null ? value.Copy () : null;
			}
		}

		public SecRecord (SecKind secKind)
		{
			var kind = SecClass.FromSecKind (secKind);
			queryDict = CFDictionary.FromObjectAndKey (kind, SecClassKey);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (queryDict != null){
				if (disposing){
					queryDict.Dispose ();
					queryDict = null;
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
		
		static SecItem ()
		{
			var handle = CFObject.dlopen ("/System/Library/Frameworks/Security.framework/Security", 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				ReturnRef = CFObject.GetIntPtr (handle, "kSecReturnRef");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		[DllImport ("/System/Library/Frameworks/Security.framework/Security")]
		internal extern static SecStatusCode SecItemCopyMatching (/* CFDictionaryRef */ IntPtr query, /* CFTypeRef* */ out IntPtr result);
	}

	static partial class SecClass {
	
		public static readonly IntPtr Identity;
		
		static SecClass ()
		{
			var handle = CFObject.dlopen ("/System/Library/Frameworks/Security.framework/Security", 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				Identity = CFObject.GetIntPtr (handle, "kSecClassIdentity");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		public static IntPtr FromSecKind (SecKind secKind)
		{
			switch (secKind){
			case SecKind.Identity:
				return Identity;
			default:
				throw new ArgumentException ("secKind");
			}
		}
	}
}
#endif
