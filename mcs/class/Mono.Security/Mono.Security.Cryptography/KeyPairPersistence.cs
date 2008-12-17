//
// KeyPairPersistence.cs: Keypair persistence
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

using Mono.Xml;

namespace Mono.Security.Cryptography {

	/* File name
	 * [type][unique name][key number].xml
	 * 
	 * where
	 *	type		CspParameters.ProviderType
	 *	unique name	A unique name for the keypair, which is
	 *			a. default (for a provider default keypair)
	 *			b. a GUID derived from
	 *				i. random if no container name was
	 *				specified at generation time
	 *				ii. the MD5 hash of the container
	 *				name (CspParameters.KeyContainerName)
	 *	key number	CspParameters.KeyNumber
	 * 
	 * File format
	 * <KeyPair>
	 *	<Properties>
	 *		<Provider Name="" Type=""/>
	 *		<Container Name=""/>
	 *	</Properties>
	 *	<KeyValue Id="">
	 *		RSAKeyValue, DSAKeyValue ...
	 *	</KeyValue>
	 * </KeyPair>
	 */

	/* NOTES
	 * 
	 * - There's NO confidentiality / integrity built in this
	 * persistance mechanism. The container directories (both user and
	 * machine) are created with restrited ACL. The ACL is also checked
	 * when a key is accessed (so totally public keys won't be used).
	 * see /mono/mono/metadata/security.c for implementation
	 * 
	 * - As we do not use CSP we limit ourselves to provider types (not 
	 * names). This means that for a same type and container type, but 
	 * two different provider names) will return the same keypair. This
	 * should work as CspParameters always requires a csp type in its
	 * constructors.
	 * 
	 * - Assert (CAS) are used so only the OS permission will limit access
	 * to the keypair files. I.e. this will work even in high-security 
	 * scenarios where users do not have access to file system (e.g. web 
	 * application). We can allow this because the filename used is 
	 * TOTALLY under our control (no direct user input is used).
	 * 
	 * - You CAN'T changes properties of the keypair once it's been
	 * created (saved). You must remove the container than save it 
	 * back. This is the same behaviour as CSP under Windows.
	 */

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class KeyPairPersistence {
	
		private static bool _userPathExists = false; // check at 1st use
		private static string _userPath;
		
		private static bool _machinePathExists = false; // check at 1st use
		private static string _machinePath;

		private CspParameters _params;
		private string _keyvalue;
		private string _filename;
		private string _container;

		// constructors

		public KeyPairPersistence (CspParameters parameters) 
			: this (parameters, null)
		{
		}

		public KeyPairPersistence (CspParameters parameters, string keyPair) 
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			_params = Copy (parameters);
			_keyvalue = keyPair;
		}

		// properties

		public string Filename {
			get { 
				if (_filename == null) {
					_filename = String.Format (CultureInfo.InvariantCulture,
						"[{0}][{1}][{2}].xml", 
						_params.ProviderType, 
						this.ContainerName, 
						_params.KeyNumber);
					if (UseMachineKeyStore)
						_filename = Path.Combine (MachinePath, _filename);
					else
						_filename = Path.Combine (UserPath, _filename);
				}
				return _filename; 
			}
		}

		public string KeyValue {
			get { return _keyvalue; }
			set { 
				if (this.CanChange)
					_keyvalue = value; 
			}
		}

		// return a (read-only) copy
		public CspParameters Parameters {
			get { return Copy (_params); }
		}

		// methods

		public bool Load () 
		{
			// see NOTES
// FIXME		new FileIOPermission (FileIOPermissionAccess.Read, this.Filename).Assert ();

			bool result = File.Exists (this.Filename);
			if (result) {
				using (StreamReader sr = File.OpenText (this.Filename)) {
					FromXml (sr.ReadToEnd ());
				}
			}
			return result;
		}

		public void Save () 
		{
			// see NOTES
// FIXME		new FileIOPermission (FileIOPermissionAccess.Write, this.Filename).Assert ();

			using (FileStream fs = File.Open (this.Filename, FileMode.Create)) {
				StreamWriter sw = new StreamWriter (fs, Encoding.UTF8);
				sw.Write (this.ToXml ());
				sw.Close ();
			}
			// apply protection to newly created files
			if (UseMachineKeyStore)
				ProtectMachine (Filename);
			else
				ProtectUser (Filename);
		}

		public void Remove () 
		{
			// see NOTES
// FIXME		new FileIOPermission (FileIOPermissionAccess.Write, this.Filename).Assert ();

			File.Delete (this.Filename);
			// it's now possible to change the keypair un the container
		}

		// private static stuff

		static object lockobj = new object ();
		
		private static string UserPath {
			get {
				lock (lockobj) {
					if ((_userPath == null) || (!_userPathExists)) {
						_userPath = Path.Combine (
							Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
							".mono");
						_userPath = Path.Combine (_userPath, "keypairs");

						_userPathExists = Directory.Exists (_userPath);
						if (!_userPathExists) {
							try {
								Directory.CreateDirectory (_userPath);
								ProtectUser (_userPath);
								_userPathExists = true;
							}
							catch (Exception e) {
								string msg = Locale.GetText ("Could not create user key store '{0}'.");
								throw new CryptographicException (String.Format (msg, _userPath), e);
							}
						}
					}
				}
				// is it properly protected ?
				if (!IsUserProtected (_userPath)) {
					string msg = Locale.GetText ("Improperly protected user's key pairs in '{0}'.");
					throw new CryptographicException (String.Format (msg, _userPath));
				}
				return _userPath;
			}
		}

		private static string MachinePath {
			get {
				lock (lockobj) {
					if ((_machinePath == null) || (!_machinePathExists)) {
						_machinePath = Path.Combine (
							Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData),
							".mono");
						_machinePath = Path.Combine (_machinePath, "keypairs");

						_machinePathExists = Directory.Exists (_machinePath);
						if (!_machinePathExists) {
							try {
								Directory.CreateDirectory (_machinePath);
								ProtectMachine (_machinePath);
								_machinePathExists = true;
							}
							catch (Exception e) {
								string msg = Locale.GetText ("Could not create machine key store '{0}'.");
								throw new CryptographicException (String.Format (msg, _machinePath), e);
							}
						}
					}
				}
				// is it properly protected ?
				if (!IsMachineProtected (_machinePath)) {
					string msg = Locale.GetText ("Improperly protected machine's key pairs in '{0}'.");
					throw new CryptographicException (String.Format (msg, _machinePath));
				}
				return _machinePath;
			}
		}

#if INSIDE_CORLIB
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool _CanSecure (string root);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool _ProtectUser (string path);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool _ProtectMachine (string path);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool _IsUserProtected (string path);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool _IsMachineProtected (string path);
#else
		// Mono.Security.dll assembly can't use the internal 
		// call (and still run with other runtimes)

		// Note: Class is only available in Mono.Security.dll as
		// a management helper (e.g. build a GUI app)

		internal static bool _CanSecure (string root) 
		{
			return true;
		}

		internal static bool _ProtectUser (string path)
		{
			return true;
		}

		internal static bool _ProtectMachine (string path)
		{
			return true;
		}

		internal static bool _IsUserProtected (string path)
		{
			return true;
		}

		internal static bool _IsMachineProtected (string path)
		{
			return true;
		}
#endif
		// private stuff

		private static bool CanSecure (string path) 
		{
			// we assume POSIX filesystems can always be secured

			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128) || (platform == 6))
				return true;

			// while we ask the runtime for Windows OS
			return _CanSecure (Path.GetPathRoot (path));
		}

		private static bool ProtectUser (string path)
		{
			// we cannot protect on some filsystem (like FAT)
			if (CanSecure (path)) {
				return _ProtectUser (path);
			}
			// but Mono still needs to run on them :(
			return true;
		}

		private static bool ProtectMachine (string path)
		{
			// we cannot protect on some filsystem (like FAT)
			if (CanSecure (path)) {
				return _ProtectMachine (path);
			}
			// but Mono still needs to run on them :(
			return true;
		}

		private static bool IsUserProtected (string path)
		{
			// we cannot protect on some filsystem (like FAT)
			if (CanSecure (path)) {
				return _IsUserProtected (path);
			}
			// but Mono still needs to run on them :(
			return true;
		}

		private static bool IsMachineProtected (string path)
		{
			// we cannot protect on some filsystem (like FAT)
			if (CanSecure (path)) {
				return _IsMachineProtected (path);
			}
			// but Mono still needs to run on them :(
			return true;
		}
		
		private bool CanChange {
			get { return (_keyvalue == null); }
		}

		private bool UseDefaultKeyContainer {
			get { return ((_params.Flags & CspProviderFlags.UseDefaultKeyContainer) == CspProviderFlags.UseDefaultKeyContainer); }
		}

		private bool UseMachineKeyStore {
			get { return ((_params.Flags & CspProviderFlags.UseMachineKeyStore) == CspProviderFlags.UseMachineKeyStore); }
		}

		private string ContainerName {
			get {
				if (_container == null) {
					if (UseDefaultKeyContainer) {
						// easy to spot
						_container = "default";
					}
					else if ((_params.KeyContainerName == null) || (_params.KeyContainerName.Length == 0)) {
						_container = Guid.NewGuid ().ToString ();
					}
					else {
						// we don't want to trust the key container name as we don't control it
						// anyway some characters may not be compatible with the file system
						byte[] data = Encoding.UTF8.GetBytes (_params.KeyContainerName);
						// Note: We use MD5 as it is faster than SHA1 and has the same length 
						// as a GUID. Recent problems found in MD5 (like collisions) aren't a
						// problem in this case.
						MD5 hash = MD5.Create ();
						byte[] result = hash.ComputeHash (data);
						_container = new Guid (result).ToString ();
					}
				}
				return _container;
			}
		}

		// we do not want any changes after receiving the csp informations
		private CspParameters Copy (CspParameters p) 
		{
			CspParameters copy = new CspParameters (p.ProviderType, p.ProviderName, p.KeyContainerName);
			copy.KeyNumber = p.KeyNumber;
			copy.Flags = p.Flags;
			return copy;
		}

		private void FromXml (string xml) 
		{
			SecurityParser sp = new SecurityParser ();
			sp.LoadXml (xml);

			SecurityElement root = sp.ToXml ();
			if (root.Tag == "KeyPair") {
				//SecurityElement prop = root.SearchForChildByTag ("Properties");
				SecurityElement keyv = root.SearchForChildByTag ("KeyValue");
				if (keyv.Children.Count > 0)
					_keyvalue = keyv.Children [0].ToString ();
				// Note: we do not read other stuff because 
				// it can't be changed after key creation
			}
		}

		private string ToXml () 
		{
			// note: we do not use SecurityElement here because the
			// keypair is a XML string (requiring parsing)
			StringBuilder xml = new StringBuilder ();
			xml.AppendFormat ("<KeyPair>{0}\t<Properties>{0}\t\t<Provider ", Environment.NewLine);
			if ((_params.ProviderName != null) && (_params.ProviderName.Length != 0)) {
				xml.AppendFormat ("Name=\"{0}\" ", _params.ProviderName);
			}
			xml.AppendFormat ("Type=\"{0}\" />{1}\t\t<Container ", _params.ProviderType, Environment.NewLine);
			xml.AppendFormat ("Name=\"{0}\" />{1}\t</Properties>{1}\t<KeyValue", this.ContainerName, Environment.NewLine);
			if (_params.KeyNumber != -1) {
				xml.AppendFormat (" Id=\"{0}\" ", _params.KeyNumber);
			}
			xml.AppendFormat (">{1}\t\t{0}{1}\t</KeyValue>{1}</KeyPair>{1}", this.KeyValue, Environment.NewLine);
			return xml.ToString ();
		}
	}
}
