//
// System.Reflection/AssemblyName.cs
//
// Authors:
//	Paolo Molaro (lupus@ximian.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Configuration.Assemblies;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace System.Reflection {

// References:
// a.	Uniform Resource Identifiers (URI): Generic Syntax
//	http://www.ietf.org/rfc/rfc2396.txt

	[Serializable]
	public sealed class AssemblyName  : ICloneable, ISerializable, IDeserializationCallback {
		string name;
		string codebase;
		int major, minor, build, revision;
		CultureInfo cultureinfo;
		AssemblyNameFlags flags;
		AssemblyHashAlgorithm hashalg;
		StrongNameKeyPair keypair;
		byte[] publicKey;
		byte[] keyToken;
		AssemblyVersionCompatibility versioncompat;
		
		public AssemblyName ()
		{
			// defaults
			versioncompat = AssemblyVersionCompatibility.SameMachine;
		}

		internal AssemblyName (SerializationInfo si, StreamingContext sc)
		{
			name = si.GetString ("_Name");
			codebase = si.GetString ("_CodeBase");
			Version = (Version)si.GetValue ("_Version", typeof (Version));
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string CodeBase {
			get { return codebase; }
			set { codebase = value; }
		}

		[MonoTODO("RFC 2396")]
		private string Escape (string url) 
		{
			// we already have code in mcs\class\System\System\Uri.cs
			// but Uri class ins't part of corlib !
			// TODO
			return url;
		}

		public string EscapedCodeBase {
			get { return Escape (codebase); }
		}

		public CultureInfo CultureInfo {
			get { return cultureinfo; }
			set { cultureinfo = value; }
		}

		public AssemblyNameFlags Flags {
			get { return flags; }
			set { flags = value; }
		}

		[MonoTODO("incomplete")]
		public string FullName {
			get {
				if (name == null)
					return null;
				StringBuilder fname = new StringBuilder ();
				fname.Append (name);
				fname.Append (", Version=");
				fname.Append (Version.ToString ());
				fname.Append (", Culture=");
				if (CultureInfo == null || CultureInfo.LCID == CultureInfo.InvariantCulture.LCID)
					fname.Append ("neutral");
				else
					fname.Append (CultureInfo.ToString ()); // ???
				if (keypair == null)
					fname.Append (", PublicKeyToken=null");
				// TODO
				return fname.ToString ();
			}
		}

		public AssemblyHashAlgorithm HashAlgorithm {
			get { return hashalg; }
			set { hashalg = value; }
		}

		public StrongNameKeyPair KeyPair {
			get { return keypair; }
			set { keypair = value; }
		}

		public Version Version {
			get {
				if (name == null)
					return null;
				return new Version (major, minor, build, revision);
			}

			set {
				major = value.Major;
				minor = value.Minor;
				build = value.Build;
				revision = value.Revision;
			}
		}

		public AssemblyVersionCompatibility VersionCompatibility {
			get { return versioncompat; }
			set { versioncompat = value; }
		}
		
/*		public override int GetHashCode ()
		{
			return name.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Reflection.AssemblyName))
				return false;

			AssemblyName an = (AssemblyName)o;

			if (an.name == this.name)
				return true;
			
			return false;
		}*/

		public override string ToString ()
		{
			string name = FullName;
			return (name != null) ? name : base.ToString ();
		}

		public byte[] GetPublicKey() 
		{
			return publicKey;
		}

		public byte[] GetPublicKeyToken() 
		{
			if (keyToken != null)
				return keyToken;
			else if (publicKey == null)
				return null;
			else {
				HashAlgorithm ha = null;
				switch (hashalg) {
					case AssemblyHashAlgorithm.MD5:
						ha = MD5.Create ();
						break;
					default:
						// None default to SHA1
						ha = SHA1.Create ();
						break;
				}
				byte[] hash = ha.ComputeHash (publicKey);
				// we need the last 8 bytes in reverse order
				keyToken = new byte [8];
				Array.Copy (hash, (hash.Length - 8), keyToken, 0, 8);
				Array.Reverse (keyToken, 0, 8);
				return keyToken;
			}
		}

		public void SetPublicKey (byte[] publicKey) 
		{
			flags = AssemblyNameFlags.PublicKey;
			this.publicKey = publicKey;
		}

		public void SetPublicKeyToken (byte[] publicKeyToken) 
		{
			keyToken = publicKeyToken;
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("_Name", name);
			info.AddValue ("_CodeBase", codebase);
			info.AddValue ("_Version", Version);
		}

		// required to implement ICloneable
		[MonoTODO()]
		public object Clone() 
		{
			return null;
		}

		// required to implement IDeserializationCallback
		[MonoTODO()]
		public void OnDeserialization (object sender) 
		{
		}

		public static AssemblyName GetAssemblyName (string assemblyFile) 
		{
			Assembly a = Assembly.LoadFrom (assemblyFile);
			return a.GetName ();
		}
	}
}
