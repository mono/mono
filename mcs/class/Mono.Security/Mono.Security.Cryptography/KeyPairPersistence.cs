//
// KeyPairPersistence.cs: Keypair persistence
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
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
	 * persistance mechanism. You better protect your directories
	 * ACL correctly!
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
	 * back. This is the same behaviour as windows.
	 */

	/* TODO
	 * 
	 * - Where do we store the machine keys ?
	 * - zeroize _keyvalue before setting to null !!!
	 */

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class KeyPairPersistence {

		private static bool _pathExists = false; // check at 1st use
		private static string _path;

		private CspParameters _params;
		private string _keyvalue;
		private string _filename;
		private string _container;

		// constructors

		public KeyPairPersistence (CspParameters parameters) 
			: this (parameters, null) {}

		public KeyPairPersistence (CspParameters parameters, string keypair) 
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			_params = Copy (parameters);
			_keyvalue = keypair;
		}

		~KeyPairPersistence () 
		{
			Clear ();
		}

		// properties

		public string Filename {
			get { 
				if (_filename == null) {
					_filename = String.Format ("[{0}][{1}][{2}].xml", 
						_params.ProviderType, 
						this.ContainerName, 
						_params.KeyNumber);
					_filename = System.IO.Path.Combine (Path, _filename);
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

		public void Clear () 
		{
			Zeroize (ref _keyvalue);
		}

		public bool Load () 
		{
			// see NOTES
			new FileIOPermission (FileIOPermissionAccess.Read, Path).Assert ();

			bool result = File.Exists (this.Filename);
			if (result) {
				string xml = null;
				try {
					using (StreamReader sr = File.OpenText (this.Filename)) {
						xml = sr.ReadToEnd ();
					}
					FromXml (xml);
				}
				finally {
					Zeroize (ref xml);
				}
			}
			return result;
		}

		public void Save () 
		{
			// see NOTES
			new FileIOPermission (FileIOPermissionAccess.Write, Path).Assert ();

			using (FileStream fs = File.Open (this.Filename, FileMode.Create)) {
				StreamWriter sw = new StreamWriter (fs, Encoding.UTF8);
				sw.Write (this.ToXml ());
				sw.Close ();
			}
		}

		public void Remove () 
		{
			// see NOTES
			new FileIOPermission (FileIOPermissionAccess.Write, Path).Assert ();

			Clear ();
			File.Delete (this.Filename);
			// it's now possible to change the keypair un the container
		}

		// private stuff

		private static string Path {
			get {
				if (_path == null) {
					lock (typeof (KeyPairPersistence)) {
						// TODO ? where to put machine key pairs ?
						_path = System.IO.Path.Combine (
							Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
							".mono");
						_path = System.IO.Path.Combine (_path, "keypairs");

						if (!_pathExists) {
							_pathExists = Directory.Exists (_path);
							if (!_pathExists) {
								Directory.CreateDirectory (_path);
							}
						}
					}
				}
				return _path;
			}
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
					else if ((_params.KeyContainerName == null) || (_params.KeyContainerName == String.Empty)) {
						_container = Guid.NewGuid ().ToString ();
					}
					else {
						// we don't want to trust the key container name as we don't control it
						// anyway some characters may not be compatible with the file system
						byte[] data = Encoding.UTF8.GetBytes (_params.KeyContainerName);
						MD5 hash = MD5.Create ();	// faster than SHA1, same length as GUID
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
				SecurityElement prop = root.SearchForChildByTag ("Properties");
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
			if ((_params.ProviderName != null) && (_params.ProviderName != String.Empty)) {
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

		private void Zeroize (ref string s) 
		{
			if (s != null) {
				// TODO - zeroize the private information ?
				// how can we track how it was used by other objects (copies?)
				// and/or reverting to unsafe code ?
				s = null;
			}
		}
	}
}
