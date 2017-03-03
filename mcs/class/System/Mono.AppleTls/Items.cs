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
using XamCore.CoreFoundation;
using XamCore.Foundation;
using XamCore.ObjCRuntime;
#if !MONOMAC
using XamCore.UIKit;
#endif

namespace XamCore.Security {

	public enum SecKind {
		InternetPassword,
		GenericPassword,
		Certificate,
		Key,
		Identity
	}

	public enum SecAccessible {
		Invalid = -1,
		WhenUnlocked,
		AfterFirstUnlock,
		Always,
		WhenUnlockedThisDeviceOnly,
		AfterFirstUnlockThisDeviceOnly,
		AlwaysThisDeviceOnly,
		WhenPasscodeSetThisDeviceOnly
	}

	public enum SecProtocol {
		Invalid = -1,
		Ftp, FtpAccount, Http, Irc, Nntp, Pop3, Smtp, Socks, Imap, Ldap, AppleTalk, Afp, Telnet, Ssh,
		Ftps, Https, HttpProxy, HttpsProxy, FtpProxy, Smb, Rtsp, RtspProxy, Daap, Eppc, Ipp,
		Nntps, Ldaps, Telnets, Imaps, Ircs, Pop3s, 
	}

	public enum SecAuthenticationType {
		Invalid = -1,
		Ntlm, Msn, Dpa, Rpa, HttpBasic, HttpDigest, HtmlForm, Default
	}

#if XAMCORE_2_0
	public class SecKeyChain : INativeObject {

		internal SecKeyChain (IntPtr handle)
		{
			Handle = handle;
		}

		public IntPtr Handle { get; internal set; }
#else
	public static class SecKeyChain {
#endif

		static NSNumber SetLimit (NSMutableDictionary dict, int max)
		{
			NSNumber n = null;
			IntPtr val;
			if (max == -1)
				val = SecMatchLimit.MatchLimitAll;
			else if (max == 1)
				val = SecMatchLimit.MatchLimitOne;
			else {
				n = NSNumber.FromInt32 (max);
				val = n.Handle;
			}
			
			dict.LowlevelSetObject (val, SecItem.MatchLimit);
			return n;
		}
		
		public static NSData QueryAsData (SecRecord query, bool wantPersistentReference, out SecStatusCode status)
		{
			if (query == null)
				throw new ArgumentNullException ("query");

			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				SetLimit (copy, 1);
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnData);
				if (wantPersistentReference)
					copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnPersistentRef);
				
				IntPtr ptr;
				status = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				if (status == SecStatusCode.Success)
					return new NSData (ptr, false);
				return null;
			}
		}

		public static NSData [] QueryAsData (SecRecord query, bool wantPersistentReference, int max, out SecStatusCode status)
		{
			if (query == null)
				throw new ArgumentNullException ("query");

			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				var n = SetLimit (copy, max);
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnData);
				if (wantPersistentReference)
					copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnPersistentRef);

				IntPtr ptr;
				status = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				n = null;
				if (status == SecStatusCode.Success){
					if (max == 1)
						return new NSData [] { new NSData (ptr, false) };

					var array = new NSArray (ptr);
					var records = new NSData [array.Count];
					for (uint i = 0; i < records.Length; i++)
						records [i] = new NSData (array.ValueAt (i), false);
					return records;
				}
				return null;
			}
		}
		
		public static NSData QueryAsData (SecRecord query)
		{
			SecStatusCode status;
			return QueryAsData (query, false, out status);
		}

		public static NSData [] QueryAsData (SecRecord query, int max)
		{
			SecStatusCode status;
			return QueryAsData (query, false, max, out status);
		}
		
		public static SecRecord QueryAsRecord (SecRecord query, out SecStatusCode result)
		{
			if (query == null)
				throw new ArgumentNullException ("query");
			
			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				SetLimit (copy, 1);
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnAttributes);
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnData);
				IntPtr ptr;
				result = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				if (result == SecStatusCode.Success)
					return new SecRecord (new NSMutableDictionary (ptr, false));
				return null;
			}
		}
		
		public static SecRecord [] QueryAsRecord (SecRecord query, int max, out SecStatusCode result)
		{
			if (query == null)
				throw new ArgumentNullException ("query");
			
			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnAttributes);
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnData);
				var n = SetLimit (copy, max);
				
				IntPtr ptr;
				result = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				n = null;
				if (result == SecStatusCode.Success){
					var array = new NSArray (ptr);
					var records = new SecRecord [array.Count];
					for (uint i = 0; i < records.Length; i++)
						records [i] = new SecRecord (new NSMutableDictionary (array.ValueAt (i), false));
					return records;
				}
				return null;
			}
		}

		public static INativeObject[] QueryAsReference (SecRecord query, int max, out SecStatusCode result)
		{
			if (query == null){
				result = SecStatusCode.Param;
				return null;
			}

			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnRef);
				SetLimit (copy, max);

				IntPtr ptr;
				result = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				if ((result == SecStatusCode.Success) && (ptr != IntPtr.Zero)) {
					var array = NSArray.ArrayFromHandle<INativeObject> (ptr, p => {
						nint cfType = CFType.GetTypeID (p);
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

		public static SecStatusCode Add (SecRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");
			return SecItem.SecItemAdd (record.queryDict.Handle, IntPtr.Zero);
			
		}

		public static SecStatusCode Remove (SecRecord record)
		{
			if (record == null)
				throw new ArgumentNullException ("record");
			return SecItem.SecItemDelete (record.queryDict.Handle);
		}
		
		public static SecStatusCode Update (SecRecord query, SecRecord newAttributes)
		{
			if (query == null)
				throw new ArgumentNullException ("record");
			if (newAttributes == null)
				throw new ArgumentNullException ("newAttributes");

			return SecItem.SecItemUpdate (query.queryDict.Handle, newAttributes.queryDict.Handle);

		}
#if MONOMAC
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeychainAddGenericPassword (
			IntPtr keychain,
			int serviceNameLength,
			byte[] serviceName,
			int accountNameLength,
			byte[] accountName,
			int passwordLength,
			byte[] passwordData,
			IntPtr itemRef);

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeychainFindGenericPassword (
			IntPtr keychainOrArray,
			int serviceNameLength,
			byte[] serviceName,
			int accountNameLength,
			byte[] accountName,
			out int passwordLength,
			out IntPtr passwordData,
			IntPtr itemRef);

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeychainAddInternetPassword (
			IntPtr keychain,
			int serverNameLength,
			byte[] serverName,
			int securityDomainLength,
			byte[] securityDomain,
			int accountNameLength,
			byte[] accountName,
			int pathLength,
			byte[] path,
			short port,
			IntPtr protocol,
			IntPtr authenticationType,
			int passwordLength,
			byte[] passwordData,
			IntPtr itemRef);

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeychainFindInternetPassword (
			IntPtr keychain,
			int serverNameLength,
			byte[] serverName,
			int securityDomainLength,
			byte[] securityDomain,
			int accountNameLength,
			byte[] accountName,
			int pathLength,
			byte[] path,
			short port,
			IntPtr protocol,
			IntPtr authenticationType,
			out int passwordLength,
			out IntPtr passwordData,
			IntPtr itemRef);

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeychainItemFreeContent (IntPtr attrList, IntPtr data);

		public static SecStatusCode AddInternetPassword (
			string serverName,
			string accountName,
			byte[] password,
			SecProtocol protocolType = SecProtocol.Http,
			short port = 0,
			string path = null,
			SecAuthenticationType authenticationType = SecAuthenticationType.Default,
			string securityDomain = null)
		{
			byte[] serverNameBytes = null;
			byte[] securityDomainBytes = null;
			byte[] accountNameBytes = null;
			byte[] pathBytes = null;
			
			if (!String.IsNullOrEmpty (serverName))
				serverNameBytes = System.Text.Encoding.UTF8.GetBytes (serverName);
			
			if (!String.IsNullOrEmpty (securityDomain))
				securityDomainBytes = System.Text.Encoding.UTF8.GetBytes (securityDomain);
			
			if (!String.IsNullOrEmpty (accountName))
				accountNameBytes = System.Text.Encoding.UTF8.GetBytes (accountName);
			
			if (!String.IsNullOrEmpty (path))
				pathBytes = System.Text.Encoding.UTF8.GetBytes (path);
			
			return SecKeychainAddInternetPassword (
				IntPtr.Zero,
				serverNameBytes?.Length ?? 0,
				serverNameBytes,
				securityDomainBytes?.Length ?? 0,
				securityDomainBytes,
				accountNameBytes?.Length ?? 0,
				accountNameBytes,
				pathBytes?.Length ?? 0,
				pathBytes,
				port,
				SecProtocolKeys.FromSecProtocol (protocolType),
				KeysAuthenticationType.FromSecAuthenticationType (authenticationType),
				password?.Length ?? 0,
				password,
				IntPtr.Zero);
		}
		
		
		public static SecStatusCode FindInternetPassword(
			string serverName,
			string accountName,
			out byte[] password,
			SecProtocol protocolType = SecProtocol.Http,
			short port = 0,
			string path = null,
			SecAuthenticationType authenticationType = SecAuthenticationType.Default,
			string securityDomain = null)
		{
			password = null;
			
			byte[] serverBytes = null;
			byte[] securityDomainBytes = null;
			byte[] accountNameBytes = null;
			byte[] pathBytes = null;

			IntPtr passwordPtr = IntPtr.Zero;
			
			try {
				if (!String.IsNullOrEmpty (serverName))
					serverBytes = System.Text.Encoding.UTF8.GetBytes (serverName);
				
				if (!String.IsNullOrEmpty (securityDomain))
					securityDomainBytes = System.Text.Encoding.UTF8.GetBytes (securityDomain);
				
				if (!String.IsNullOrEmpty (accountName))
					accountNameBytes = System.Text.Encoding.UTF8.GetBytes (accountName);
				
				if (!String.IsNullOrEmpty(path))
					pathBytes = System.Text.Encoding.UTF8.GetBytes (path);
				
				int passwordLength = 0;
				
				SecStatusCode code = SecKeychainFindInternetPassword(
					IntPtr.Zero,
					serverBytes?.Length ?? 0,
					serverBytes,
					securityDomainBytes?.Length ?? 0,
					securityDomainBytes,
					accountNameBytes?.Length ?? 0,
					accountNameBytes,
					pathBytes?.Length ?? 0,
					pathBytes,
					port,
					SecProtocolKeys.FromSecProtocol(protocolType),
					KeysAuthenticationType.FromSecAuthenticationType(authenticationType),
					out passwordLength,
					out passwordPtr,
					IntPtr.Zero);
				
				if (code == SecStatusCode.Success && passwordLength > 0) {
					password = new byte[passwordLength];
					Marshal.Copy(passwordPtr, password, 0, passwordLength);
				}
				
				return code;
				
			} finally {
				if (passwordPtr != IntPtr.Zero)
					SecKeychainItemFreeContent(IntPtr.Zero, passwordPtr);
			}
		}

		public static SecStatusCode AddGenericPassword (string serviceName, string accountName, byte[] password)
		{
			byte[] serviceNameBytes = null;
			byte[] accountNameBytes = null;
			
			if (!String.IsNullOrEmpty (serviceName))
				serviceNameBytes = System.Text.Encoding.UTF8.GetBytes (serviceName);

			if (!String.IsNullOrEmpty (accountName))
				accountNameBytes = System.Text.Encoding.UTF8.GetBytes (accountName);

			return SecKeychainAddGenericPassword(
				IntPtr.Zero,
				serviceNameBytes?.Length ?? 0,
				serviceNameBytes,
				accountNameBytes?.Length ?? 0,
				accountNameBytes,
				password?.Length ?? 0,
				password,
				IntPtr.Zero
				);
		}

		public static SecStatusCode FindGenericPassword(string serviceName, string accountName, out byte[] password)
		{
			password = null;

			byte[] serviceNameBytes = null;
			byte[] accountNameBytes = null;
			
			IntPtr passwordPtr = IntPtr.Zero;
			
			try {
				
				if (!String.IsNullOrEmpty (serviceName))
					serviceNameBytes = System.Text.Encoding.UTF8.GetBytes (serviceName);
				
				if (!String.IsNullOrEmpty (accountName))
					accountNameBytes = System.Text.Encoding.UTF8.GetBytes (accountName);
				
				int passwordLength = 0;
				
				var code = SecKeychainFindGenericPassword(
					IntPtr.Zero,
					serviceNameBytes?.Length ?? 0,
					serviceNameBytes,
					accountNameBytes?.Length ?? 0,
					accountNameBytes,
					out passwordLength,
					out passwordPtr,
					IntPtr.Zero
					);
				
				if (code == SecStatusCode.Success && passwordLength > 0){
					password = new byte[passwordLength];
					Marshal.Copy(passwordPtr, password, 0, passwordLength);
				}
				
				return code;
				
			} finally {
				if (passwordPtr != IntPtr.Zero)
					SecKeychainItemFreeContent(IntPtr.Zero, passwordPtr);
			}
		}
#else
		public static object QueryAsConcreteType (SecRecord query, out SecStatusCode result)
		{
			if (query == null){
				result = SecStatusCode.Param;
				return null;
			}
			
			using (var copy = NSMutableDictionary.FromDictionary (query.queryDict)){
				copy.LowlevelSetObject (CFBoolean.True.Handle, SecItem.ReturnRef);
				SetLimit (copy, 1);
				
				IntPtr ptr;
				result = SecItem.SecItemCopyMatching (copy.Handle, out ptr);
				if ((result == SecStatusCode.Success) && (ptr != IntPtr.Zero)) {
					nint cfType = CFType.GetTypeID (ptr);
					
					if (cfType == SecCertificate.GetTypeID ())
						return new SecCertificate (ptr, true);
					else if (cfType == SecKey.GetTypeID ())
						return new SecKey (ptr, true);
					else if (cfType == SecIdentity.GetTypeID ())
						return new SecIdentity (ptr, true);
					else
						throw new Exception (String.Format ("Unexpected type: 0x{0:x}", cfType));
				} 
				return null;
			}
		}
#endif

		public static void AddIdentity (SecIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			using (var record = new SecRecord ()) {
				record.SetValueRef (identity);

				SecStatusCode result = SecKeyChain.Add (record);

				if (result != SecStatusCode.DuplicateItem && result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());
			}
		}

		public static void RemoveIdentity (SecIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			using (var record = new SecRecord ()) {
				record.SetValueRef (identity);

				SecStatusCode result = SecKeyChain.Remove (record);

				if (result != SecStatusCode.ItemNotFound && result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());
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
	}
	
	public class SecRecord : IDisposable {
		// Fix <= iOS 6 Behaviour - Desk #83099
		// NSCFDictionary: mutating method sent to immutable object
		// iOS 6 returns an inmutable NSDictionary handle and when we try to set its values it goes kaboom
		// By explicitly calling `MutableCopy` we ensure we always have a mutable reference we expect that.
		NSMutableDictionary _queryDict;
		internal NSMutableDictionary queryDict 
		{ 
			get {
				return _queryDict;
			}
			set {
				_queryDict = value != null ? (NSMutableDictionary)value.MutableCopy () : null;
			}
		}

		internal SecRecord (NSMutableDictionary dict)
		{
			queryDict = dict;
		}

		// it's possible to query something without a class
		public SecRecord ()
		{
			queryDict = new NSMutableDictionary ();
		}

		public SecRecord (SecKind secKind)
		{
			var kind = SecClass.FromSecKind (secKind);
#if MONOMAC
			queryDict = NSMutableDictionary.LowlevelFromObjectAndKey (kind, SecClass.SecClassKey);
#elif WATCH
			queryDict = NSMutableDictionary.LowlevelFromObjectAndKey (kind, SecClass.SecClassKey);
#else
			// Apple changed/fixed this in iOS7 (not the only change, see comments above)
			// test suite has a test case that needs to work on both pre-7.0 and post-7.0
			if ((kind == SecClass.Identity) && !UIDevice.CurrentDevice.CheckSystemVersion (7,0))
				queryDict = new NSMutableDictionary ();
			else
				queryDict = NSMutableDictionary.LowlevelFromObjectAndKey (kind, SecClass.SecClassKey);
#endif
		}

		public SecRecord (SecCertificate certificate) : this (SecKind.Certificate)
		{
			SetCertificate (certificate);
		}

		public SecRecord (SecIdentity identity) : this (SecKind.Identity)
		{
			SetIdentity (identity);
		}

		public SecRecord (SecKey key) : this (SecKind.Key)
		{
			SetKey (key);
		}

		public SecCertificate GetCertificate ()
		{
			CheckClass (SecClass.Certificate);
			return GetValueRef <SecCertificate> ();
		}

		public SecIdentity GetIdentity ()
		{
			CheckClass (SecClass.Identity);
			return GetValueRef<SecIdentity> ();
		}

		public SecKey GetKey ()
		{
			CheckClass (SecClass.Key);
			return GetValueRef<SecKey> ();
		}

		void CheckClass (IntPtr secClass)
		{
			var kind = queryDict.LowlevelObjectForKey (SecClass.SecClassKey);
			if (kind != secClass)
				throw new InvalidOperationException ("SecRecord of incompatible SecClass");
		}

		public SecRecord Clone ()
		{
			return new SecRecord (NSMutableDictionary.FromDictionary (queryDict));
		}

		// some API are unusable without this (e.g. SecKey.GenerateKeyPair) without duplicating much of SecRecord logic
		public NSDictionary ToDictionary ()
		{
			return queryDict;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

#if XAMCORE_2_0
		protected virtual void Dispose (bool disposing)
#else
		public virtual void Dispose (bool disposing)
#endif
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
			
		IntPtr Fetch (IntPtr key)
		{
			return queryDict.LowlevelObjectForKey (key);
		}

		NSObject FetchObject (IntPtr key)
		{
			return Runtime.GetNSObject (Fetch (key));
		}

		string FetchString (IntPtr key)
		{
			return (NSString) FetchObject (key);
		}

		int FetchInt (IntPtr key)
		{
			var obj = (NSNumber) FetchObject (key);
			return obj == null ? -1 : obj.Int32Value;
		}

		bool FetchBool (IntPtr key, bool defaultValue)
		{
			var obj = (NSNumber) FetchObject (key);
			return obj == null ? defaultValue : obj.Int32Value != 0;
		}

		T Fetch<T> (IntPtr key) where T : NSObject
		{
			return (T) FetchObject (key);
		}
		

		void SetValue (NSObject val, IntPtr key)
		{
			queryDict.LowlevelSetObject (val, key);
		}

		void SetValue (IntPtr val, IntPtr key)
		{
			queryDict.LowlevelSetObject (val, key);
		}

		void SetValue (string value, IntPtr key)
		{
			// FIXME: it's not clear that we should not allow null (i.e. that null should remove entries)
			// but this is compatible with the exiting behaviour of older XI/XM
			if (value == null)
				throw new ArgumentNullException (nameof (value));
			var ptr = NSString.CreateNative (value);
			queryDict.LowlevelSetObject (ptr, key);
			NSString.ReleaseNative (ptr);
		}
		
		//
		// Attributes
		//
		public SecAccessible Accessible {
			get {
				return KeysAccessible.ToSecAccessible (Fetch (SecAttributeKey.Accessible));
			}
			
			set {
				SetValue (KeysAccessible.FromSecAccessible (value), SecAttributeKey.Accessible);
			}
		}

		public bool Synchronizable {
			get {
				return FetchBool (SecAttributeKey.Synchronizable, false);
			}
			set {
				SetValue (new NSNumber (value ? 1 : 0), SecAttributeKey.Synchronizable);
			}
		}

		public bool SynchronizableAny {
			get {
				return FetchBool (SecAttributeKey.SynchronizableAny, false);
			}
			set {
				SetValue (new NSNumber (value ? 1 : 0), SecAttributeKey.SynchronizableAny);
			}
		}

#if !MONOMAC
		[iOS (9,0)]
		public string SyncViewHint {
			get {
				return FetchString (SecAttributeKey.SyncViewHint);
			}
			set {
				SetValue (value, SecAttributeKey.SyncViewHint);
			}
		}

		[iOS (9,0)]
		public SecTokenID TokenID {
			get {
				return SecTokenIDExtensions.GetValue (Fetch<NSString> (SecAttributeKey.TokenID));
			}
			set {
				// choose wisely to avoid NSString -> string -> NSString conversion
				SetValue ((NSObject) value.GetConstant (), SecAttributeKey.TokenID);
			}
		}
#endif

		public NSDate CreationDate {
			get {
				return (NSDate) FetchObject (SecAttributeKey.CreationDate);
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecAttributeKey.CreationDate);
			}
		}

		public NSDate ModificationDate {
			get {
				return (NSDate) FetchObject (SecAttributeKey.ModificationDate);
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecAttributeKey.ModificationDate);
			}
		}

		public string Description {
			get {
				return FetchString (SecAttributeKey.Description);
			}

			set {
				SetValue (value, SecAttributeKey.Description);
			}
		}

		public string Comment {
			get {
				return FetchString (SecAttributeKey.Comment);
			}

			set {
				SetValue (value, SecAttributeKey.Comment);
			}
		}

		public int Creator {
			get {
				return FetchInt (SecAttributeKey.Creator);
			}
					
			set {
				SetValue (new NSNumber (value), SecAttributeKey.Creator);
			}
		}

		public int CreatorType {
			get {
				return FetchInt (SecAttributeKey.Type);
			}
					
			set {
				SetValue (new NSNumber (value), SecAttributeKey.Type);
			}
		}

		public string Label {
			get {
				return FetchString (SecAttributeKey.Label);
			}

			set {
				SetValue (value, SecAttributeKey.Label);
			}
		}

		public bool Invisible {
			get {
				return Fetch (SecAttributeKey.IsInvisible) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.IsInvisible);
			}
		}

		public bool IsNegative {
			get {
				return Fetch (SecAttributeKey.IsNegative) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.IsNegative);
			}
		}

		public string Account {
			get {
				return FetchString (SecAttributeKey.Account);
			}

			set {
				SetValue (value, SecAttributeKey.Account);
			}
		}

		public string Service {
			get {
				return FetchString (SecAttributeKey.Service);
			}

			set {
				SetValue (value, SecAttributeKey.Service);
			}
		}

#if !MONOMAC || !XAMCORE_2_0
		public string UseOperationPrompt {
			get {
				return FetchString (SecItem.UseOperationPrompt);
			}
			set {
				SetValue (value, SecItem.UseOperationPrompt);
			}
		}

		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0, Message = "Use AuthenticationUI property")]
		public bool UseNoAuthenticationUI {
			get {
				return Fetch (SecItem.UseNoAuthenticationUI) == CFBoolean.True.Handle;
			}
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecItem.UseNoAuthenticationUI);
			}
		}
#endif
		[iOS (9,0)][Mac (10,11)]
		public SecAuthenticationUI AuthenticationUI {
			get {
				var s = Fetch<NSString> (SecItem.UseAuthenticationUI);
				return s == null ? SecAuthenticationUI.NotSet : SecAuthenticationUIExtensions.GetValue (s);
			}
			set {
				SetValue ((NSObject) value.GetConstant (), SecItem.UseAuthenticationUI);
			}
		}

#if XAMCORE_2_0 && !WATCH && !TVOS
		[iOS (9, 0), Mac (10, 11)]
		public XamCore.LocalAuthentication.LAContext AuthenticationContext {
			get {
				return Fetch<XamCore.LocalAuthentication.LAContext> (SecItem.UseAuthenticationContext);
			}
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));
				SetValue (value.Handle, SecItem.UseAuthenticationContext);
			}
		}
#endif

		// Must store the _secAccessControl here, since we have no way of inspecting its values if
		// it is ever returned from a dictionary, so return what we cached.
		SecAccessControl _secAccessControl;
		public SecAccessControl AccessControl {
			get {
				return _secAccessControl;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				_secAccessControl = value;
				SetValue (value.Handle, SecAttributeKey.AccessControl);
			}
		}

		public NSData Generic {
			get {
				return Fetch<NSData> (SecAttributeKey.Generic);
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecAttributeKey.Generic);
			}
		}

		public string SecurityDomain {
			get {
				return FetchString (SecAttributeKey.SecurityDomain);
			}

			set {
				SetValue (value, SecAttributeKey.SecurityDomain);
			}
		}

		public string Server {
			get {
				return FetchString (SecAttributeKey.Server);
			}

			set {
				SetValue (value, SecAttributeKey.Server);
			}
		}

		public SecProtocol Protocol {
			get {
				return SecProtocolKeys.ToSecProtocol (Fetch (SecAttributeKey.Protocol));
			}
			
			set {
				SetValue (SecProtocolKeys.FromSecProtocol (value), SecAttributeKey.Protocol);
			}
		}

		public SecAuthenticationType AuthenticationType {
			get {
				var at = Fetch (SecAttributeKey.AuthenticationType);
				if (at == IntPtr.Zero)
					return SecAuthenticationType.Default;
				return KeysAuthenticationType.ToSecAuthenticationType (at);
			}
			
			set {
				SetValue (KeysAuthenticationType.FromSecAuthenticationType (value),
							     SecAttributeKey.AuthenticationType);
			}
		}

		public int Port {
			get {
				return FetchInt (SecAttributeKey.Port);
			}
					
			set {
				SetValue (new NSNumber (value), SecAttributeKey.Port);
			}
		}

		public string Path {
			get {
				return FetchString (SecAttributeKey.Path);
			}

			set {
				SetValue (value, SecAttributeKey.Path);
			}
		}

		// read only
		public string Subject {
			get {
				return FetchString (SecAttributeKey.Subject);
			}
		}

		// read only
		public NSData Issuer {
			get {
				return Fetch<NSData> (SecAttributeKey.Issuer);
			}
		}

		// read only
		public NSData SerialNumber {
			get {
				return Fetch<NSData> (SecAttributeKey.SerialNumber);
			}
		}

		// read only
		public NSData SubjectKeyID {
			get {
				return Fetch<NSData> (SecAttributeKey.SubjectKeyID);
			}
		}

		// read only
		public NSData PublicKeyHash {
			get {
				return Fetch<NSData> (SecAttributeKey.PublicKeyHash);
			}
		}

		// read only
		public NSNumber CertificateType {
			get {
				return Fetch<NSNumber> (SecAttributeKey.CertificateType);
			}
		}

		// read only
		public NSNumber CertificateEncoding {
			get {
				return Fetch<NSNumber> (SecAttributeKey.CertificateEncoding);
			}
		}

		public SecKeyClass KeyClass {
			get {
				var k = Fetch (SecAttributeKey.KeyClass);
				if (k == IntPtr.Zero)
					return SecKeyClass.Invalid;
				using (var s = new NSString (k))
					return SecKeyClassExtensions.GetValue (s);
			}
			set {
				var k = value.GetConstant ();
				if (k == null)
					throw new ArgumentException ("Unknown value");
				SetValue ((NSObject) k, SecAttributeKey.KeyClass);
			}
		}

		public string ApplicationLabel {
			get {
				return FetchString (SecAttributeKey.ApplicationLabel);
			}

			set {
				SetValue (value, SecAttributeKey.ApplicationLabel);
			}
		}

		public bool IsPermanent {
			get {
				return Fetch (SecAttributeKey.IsPermanent) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.IsPermanent);
			}
		}

		public NSData ApplicationTag {
			get {
				return Fetch<NSData> (SecAttributeKey.ApplicationTag);
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecAttributeKey.ApplicationTag);
			}
		}

		public SecKeyType KeyType {
			get {
				var k = Fetch (SecAttributeKey.KeyType);
				if (k == IntPtr.Zero)
					return SecKeyType.Invalid;
				using (var s = new NSString (k))
					return SecKeyTypeExtensions.GetValue (s);
			}
			
			set {
				var k = value.GetConstant ();
				if (k == null)
					throw new ArgumentException ("Unknown value");
				SetValue ((NSObject) k, SecAttributeKey.KeyType);
			}
		}

		public int KeySizeInBits {
			get {
				return FetchInt (SecAttributeKey.KeySizeInBits);
			}
					
			set {
				SetValue (new NSNumber (value), SecAttributeKey.KeySizeInBits);
			}
		}

		public int EffectiveKeySize {
			get {
				return FetchInt (SecAttributeKey.EffectiveKeySize);
			}
					
			set {
				SetValue (new NSNumber (value), SecAttributeKey.EffectiveKeySize);
			}
		}

		public bool CanEncrypt {
			get {
				return Fetch (SecAttributeKey.CanEncrypt) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanEncrypt);
			}
		}

		public bool CanDecrypt {
			get {
				return Fetch (SecAttributeKey.CanDecrypt) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanDecrypt);
			}
		}

		public bool CanDerive {
			get {
				return Fetch (SecAttributeKey.CanDerive) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanDerive);
			}
		}

		public bool CanSign {
			get {
				return Fetch (SecAttributeKey.CanSign) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanSign);
			}
		}

		public bool CanVerify {
			get {
				return Fetch (SecAttributeKey.CanVerify) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanVerify);
			}
		}

		public bool CanWrap {
			get {
				return Fetch (SecAttributeKey.CanWrap) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanWrap);
			}
		}

		public bool CanUnwrap {
			get {
				return Fetch (SecAttributeKey.CanUnwrap) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecAttributeKey.CanUnwrap);
			}
		}

		public string AccessGroup {
			get {
				return FetchString (SecAttributeKey.AccessGroup);
			}

			set {
				SetValue (value, SecAttributeKey.AccessGroup);
			}
		}

		//
		// Matches
		//

		public SecPolicy MatchPolicy {
			get {
				var pol = Fetch (SecItem.MatchPolicy);
				return (pol == IntPtr.Zero) ? null : new SecPolicy (pol);
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value.Handle, SecItem.MatchPolicy);
			}
		}

#if XAMCORE_2_0
		public SecKeyChain[] MatchItemList {
			get {
				return NSArray.ArrayFromHandle<SecKeyChain> (Fetch (SecItem.MatchItemList));
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				using (var array = NSArray.FromNativeObjects (value))
					SetValue (array, SecItem.MatchItemList);
			}
		}
#else
		public NSArray MatchItemList {
			get {
				return (NSArray) Runtime.GetNSObject (Fetch (SecItem.MatchItemList));
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecItem.MatchItemList);
			}
		}
#endif

		public NSData [] MatchIssuers {
			get {
				return NSArray.ArrayFromHandle<NSData> (Fetch (SecItem.MatchIssuers));
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				SetValue (NSArray.FromNSObjects (value), SecItem.MatchIssuers);
			}
		}

		public string MatchEmailAddressIfPresent {
			get {
				return FetchString (SecItem.MatchEmailAddressIfPresent);
			}

			set {
				SetValue (value, SecItem.MatchEmailAddressIfPresent);
			}
		}

		public string MatchSubjectContains {
			get {
				return FetchString (SecItem.MatchSubjectContains);
			}

			set {
				SetValue (value, SecItem.MatchSubjectContains);
			}
		}

		public bool MatchCaseInsensitive {
			get {
				return Fetch (SecItem.MatchCaseInsensitive) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecItem.MatchCaseInsensitive);
			}
		}

		public bool MatchTrustedOnly {
			get {
				return Fetch (SecItem.MatchTrustedOnly) == CFBoolean.True.Handle;
			}
			
			set {
				SetValue (CFBoolean.FromBoolean (value).Handle, SecItem.MatchTrustedOnly);
			}
		}

		public NSDate MatchValidOnDate {
			get {
				return (NSDate) (Runtime.GetNSObject (Fetch (SecItem.MatchValidOnDate)));
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecItem.MatchValidOnDate);
			}
		}

		public NSData ValueData {
			get {
				return Fetch<NSData> (SecItem.ValueData);
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecItem.ValueData);
			}
		}

#if !XAMCORE_2_0
		[Obsolete ("Use GetValueRef<T> and SetValueRef<T> instead")]
		public NSObject ValueRef {
			get {
				return FetchObject (SecItem.ValueRef);
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				SetValue (value, SecItem.ValueRef);
			}
		}
#endif
			
		public T GetValueRef<T> () where T : class, INativeObject
		{
			return Runtime.GetINativeObject<T> (queryDict.LowlevelObjectForKey (SecItem.ValueRef), false);
		}

		// This can be used to store SecKey, SecCertificate, SecIdentity and SecKeyChainItem (not bound yet, and not availble on iOS)
		public void SetValueRef (INativeObject value)
		{
			SetValue (value == null ? IntPtr.Zero : value.Handle, SecItem.ValueRef);
		}

		public void SetCertificate (SecCertificate cert) => SetValueRef (cert);
		public void SetIdentity (SecIdentity identity) => SetValueRef (identity);
		public void SetKey (SecKey key) => SetValueRef (key);

	}
	
	internal partial class SecItem {

		[DllImport (Constants.SecurityLibrary)]
		internal extern static SecStatusCode SecItemCopyMatching (/* CFDictionaryRef */ IntPtr query, /* CFTypeRef* */ out IntPtr result);

		[DllImport (Constants.SecurityLibrary)]
		internal extern static SecStatusCode SecItemAdd (/* CFDictionaryRef */ IntPtr attributes, /* CFTypeRef* */ IntPtr result);

		[DllImport (Constants.SecurityLibrary)]
		internal extern static SecStatusCode SecItemDelete (/* CFDictionaryRef */ IntPtr query);

		[DllImport (Constants.SecurityLibrary)]
		internal extern static SecStatusCode SecItemUpdate (/* CFDictionaryRef */ IntPtr query, /* CFDictionaryRef */ IntPtr attributesToUpdate);
	}

	internal static partial class SecClass {
	
		public static IntPtr FromSecKind (SecKind secKind)
		{
			switch (secKind){
			case SecKind.InternetPassword:
				return InternetPassword;
			case SecKind.GenericPassword:
				return GenericPassword;
			case SecKind.Certificate:
				return Certificate;
			case SecKind.Key:
				return Key;
			case SecKind.Identity:
				return Identity;
			default:
				throw new ArgumentException ("secKind");
			}
		}
	}
	
	internal static partial class KeysAccessible {
		public static IntPtr FromSecAccessible (SecAccessible accessible)
		{
			switch (accessible){
			case SecAccessible.WhenUnlocked:
				return WhenUnlocked;
			case SecAccessible.AfterFirstUnlock:
				return AfterFirstUnlock;
			case SecAccessible.Always:
				return Always;
			case SecAccessible.WhenUnlockedThisDeviceOnly:
				return WhenUnlockedThisDeviceOnly;
			case SecAccessible.AfterFirstUnlockThisDeviceOnly:
				return AfterFirstUnlockThisDeviceOnly;
			case SecAccessible.AlwaysThisDeviceOnly:
				return AlwaysThisDeviceOnly;
			case SecAccessible.WhenPasscodeSetThisDeviceOnly:
				return WhenPasscodeSetThisDeviceOnly;
			default:
				throw new ArgumentException ("accessible");
			}
		}
			
		// note: we're comparing pointers - but it's an (even if opaque) CFType
		// and it turns out to be using CFString - so we need to use CFTypeEqual
		public static SecAccessible ToSecAccessible (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return SecAccessible.Invalid;
			if (CFType.Equal (handle, WhenUnlocked))
				return SecAccessible.WhenUnlocked;
			if (CFType.Equal (handle, AfterFirstUnlock))
				return SecAccessible.AfterFirstUnlock;
			if (CFType.Equal (handle, Always))
				return SecAccessible.Always;
			if (CFType.Equal (handle, WhenUnlockedThisDeviceOnly))
				return SecAccessible.WhenUnlockedThisDeviceOnly;
			if (CFType.Equal (handle, AfterFirstUnlockThisDeviceOnly))
				return SecAccessible.AfterFirstUnlockThisDeviceOnly;
			if (CFType.Equal (handle, AlwaysThisDeviceOnly))
				return SecAccessible.AlwaysThisDeviceOnly;
			if (CFType.Equal (handle, WhenUnlockedThisDeviceOnly))
				return SecAccessible.WhenUnlockedThisDeviceOnly;
			return SecAccessible.Invalid;
		}
	}
	
	internal static partial class SecProtocolKeys {
		public static IntPtr FromSecProtocol (SecProtocol protocol)
		{
			switch (protocol){
			case SecProtocol.Ftp: return FTP;
			case SecProtocol.FtpAccount: return FTPAccount;
			case SecProtocol.Http: return HTTP;
			case SecProtocol.Irc: return IRC;
			case SecProtocol.Nntp: return NNTP;
			case SecProtocol.Pop3: return POP3;
			case SecProtocol.Smtp: return SMTP;
			case SecProtocol.Socks:return SOCKS;
			case SecProtocol.Imap:return IMAP;
			case SecProtocol.Ldap:return LDAP;
			case SecProtocol.AppleTalk:return AppleTalk;
			case SecProtocol.Afp:return AFP;
			case SecProtocol.Telnet:return Telnet;
			case SecProtocol.Ssh:return SSH;
			case SecProtocol.Ftps:return FTPS;
			case SecProtocol.Https:return HTTPS;
			case SecProtocol.HttpProxy:return HTTPProxy;
			case SecProtocol.HttpsProxy:return HTTPSProxy;
			case SecProtocol.FtpProxy:return FTPProxy;
			case SecProtocol.Smb:return SMB;
			case SecProtocol.Rtsp:return RTSP;
			case SecProtocol.RtspProxy:return RTSPProxy;
			case SecProtocol.Daap:return DAAP;
			case SecProtocol.Eppc:return EPPC;
			case SecProtocol.Ipp:return IPP;
			case SecProtocol.Nntps:return NNTPS;
			case SecProtocol.Ldaps:return LDAPS;
			case SecProtocol.Telnets:return TelnetS;
			case SecProtocol.Imaps:return IMAPS;
			case SecProtocol.Ircs:return IRCS;
			case SecProtocol.Pop3s: return POP3S;
			}
			throw new ArgumentException ("protocol");
		}

		public static SecProtocol ToSecProtocol (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return SecProtocol.Invalid;
			if (CFType.Equal (handle, FTP))
				return SecProtocol.Ftp;
			if (CFType.Equal (handle, FTPAccount))
				return SecProtocol.FtpAccount;
			if (CFType.Equal (handle, HTTP))
				return SecProtocol.Http;
			if (CFType.Equal (handle, IRC))
				return SecProtocol.Irc;
			if (CFType.Equal (handle, NNTP))
				return SecProtocol.Nntp;
			if (CFType.Equal (handle, POP3))
				return SecProtocol.Pop3;
			if (CFType.Equal (handle, SMTP))
				return SecProtocol.Smtp;
			if (CFType.Equal (handle, SOCKS))
				return SecProtocol.Socks;
			if (CFType.Equal (handle, IMAP))
				return SecProtocol.Imap;
			if (CFType.Equal (handle, LDAP))
				return SecProtocol.Ldap;
			if (CFType.Equal (handle, AppleTalk))
				return SecProtocol.AppleTalk;
			if (CFType.Equal (handle, AFP))
				return SecProtocol.Afp;
			if (CFType.Equal (handle, Telnet))
				return SecProtocol.Telnet;
			if (CFType.Equal (handle, SSH))
				return SecProtocol.Ssh;
			if (CFType.Equal (handle, FTPS))
				return SecProtocol.Ftps;
			if (CFType.Equal (handle, HTTPS))
				return SecProtocol.Https;
			if (CFType.Equal (handle, HTTPProxy))
				return SecProtocol.HttpProxy;
			if (CFType.Equal (handle, HTTPSProxy))
				return SecProtocol.HttpsProxy;
			if (CFType.Equal (handle, FTPProxy))
				return SecProtocol.FtpProxy;
			if (CFType.Equal (handle, SMB))
				return SecProtocol.Smb;
			if (CFType.Equal (handle, RTSP))
				return SecProtocol.Rtsp;
			if (CFType.Equal (handle, RTSPProxy))
				return SecProtocol.RtspProxy;
			if (CFType.Equal (handle, DAAP))
				return SecProtocol.Daap;
			if (CFType.Equal (handle, EPPC))
				return SecProtocol.Eppc;
			if (CFType.Equal (handle, IPP))
				return SecProtocol.Ipp;
			if (CFType.Equal (handle, NNTPS))
				return SecProtocol.Nntps;
			if (CFType.Equal (handle, LDAPS))
				return SecProtocol.Ldaps;
			if (CFType.Equal (handle, TelnetS))
				return SecProtocol.Telnets;
			if (CFType.Equal (handle, IMAPS))
				return SecProtocol.Imaps;
			if (CFType.Equal (handle, IRCS))
				return SecProtocol.Ircs;
			if (CFType.Equal (handle, POP3S))
				return SecProtocol.Pop3s;
			return SecProtocol.Invalid;
		}
	}
	
	internal static partial class KeysAuthenticationType {
		public static SecAuthenticationType ToSecAuthenticationType (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return SecAuthenticationType.Invalid;
			if (CFType.Equal (handle, NTLM))
				return SecAuthenticationType.Ntlm;
			if (CFType.Equal (handle, MSN))
				return SecAuthenticationType.Msn;
			if (CFType.Equal (handle, DPA))
				return SecAuthenticationType.Dpa;
			if (CFType.Equal (handle, RPA))
				return SecAuthenticationType.Rpa;
			if (CFType.Equal (handle, HTTPBasic))
				return SecAuthenticationType.HttpBasic;
			if (CFType.Equal (handle, HTTPDigest))
				return SecAuthenticationType.HttpDigest;
			if (CFType.Equal (handle, HTMLForm))
				return SecAuthenticationType.HtmlForm;
			if (CFType.Equal (handle, Default))
				return SecAuthenticationType.Default;
			return SecAuthenticationType.Invalid;
		}

		public static IntPtr FromSecAuthenticationType (SecAuthenticationType type)
		{
			switch (type){
			case SecAuthenticationType.Ntlm:
				return NTLM;
			case SecAuthenticationType.Msn:
				return MSN;
			case SecAuthenticationType.Dpa:
				return DPA;
			case SecAuthenticationType.Rpa:
				return RPA;
			case SecAuthenticationType.HttpBasic:
				return HTTPBasic;
			case SecAuthenticationType.HttpDigest:
				return HTTPDigest;
			case SecAuthenticationType.HtmlForm:
				return HTMLForm;
			case SecAuthenticationType.Default:
				return Default;
			default:
				throw new ArgumentException ("type");
			}
		}
	}
	
	public class SecurityException : Exception {
		static string ToMessage (SecStatusCode code)
		{
			
			switch (code){
			case SecStatusCode.Success: 
			case SecStatusCode.Unimplemented: 
			case SecStatusCode.Param:
			case SecStatusCode.Allocate:
			case SecStatusCode.NotAvailable:
			case SecStatusCode.DuplicateItem:
			case SecStatusCode.ItemNotFound:
			case SecStatusCode.InteractionNotAllowed:
			case SecStatusCode.Decode:
				return code.ToString ();
			}
			return String.Format ("Unknown error: 0x{0:x}", code);
		}
		
		public SecurityException (SecStatusCode code) : base (ToMessage (code))
		{
		}
	}
}
