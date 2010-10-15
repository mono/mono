//
// System.Reflection/AssemblyName.cs
//
// Authors:
//	Paolo Molaro (lupus@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Configuration.Assemblies;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection {

// References:
// a.	Uniform Resource Identifiers (URI): Generic Syntax
//	http://www.ietf.org/rfc/rfc2396.txt

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_AssemblyName))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	public sealed class AssemblyName  : ICloneable, ISerializable, IDeserializationCallback, _AssemblyName {

#pragma warning disable 169
		#region Synch with object-internals.h
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
		Version version;
		ProcessorArchitecture processor_architecture = ProcessorArchitecture.None;
		#endregion
#pragma warning restore 169		
		
		public AssemblyName ()
		{
			// defaults
			versioncompat = AssemblyVersionCompatibility.SameMachine;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern bool ParseName (AssemblyName aname, string assemblyName);
		
		public AssemblyName (string assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");
			if (assemblyName.Length < 1)
				throw new ArgumentException ("assemblyName cannot have zero length.");
				
			if (!ParseName (this, assemblyName))
				throw new FileLoadException ("The assembly name is invalid.");
		}
		
		[MonoLimitation ("Not used, as the values are too limited;  Mono supports more")]
		public ProcessorArchitecture ProcessorArchitecture {
			get {
				return processor_architecture;
			}
			set {
				processor_architecture = value;
			}
		}

		internal AssemblyName (SerializationInfo si, StreamingContext sc)
		{
			name = si.GetString ("_Name");
			codebase = si.GetString ("_CodeBase");
			version = (Version)si.GetValue ("_Version", typeof (Version));
			publicKey = (byte[])si.GetValue ("_PublicKey", typeof (byte[]));
			keyToken = (byte[])si.GetValue ("_PublicKeyToken", typeof (byte[]));
			hashalg = (AssemblyHashAlgorithm)si.GetValue ("_HashAlgorithm", typeof (AssemblyHashAlgorithm));
			keypair = (StrongNameKeyPair)si.GetValue ("_StrongNameKeyPair", typeof (StrongNameKeyPair));
			versioncompat = (AssemblyVersionCompatibility)si.GetValue ("_VersionCompatibility", typeof (AssemblyVersionCompatibility));
			flags = (AssemblyNameFlags)si.GetValue ("_Flags", typeof (AssemblyNameFlags));
			int lcid = si.GetInt32 ("_CultureInfo");
			if (lcid != -1) cultureinfo = new CultureInfo (lcid);
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string CodeBase {
			get { return codebase; }
			set { codebase = value; }
		}

		public string EscapedCodeBase {
			get {
				if (codebase == null)
					return null;
				return Uri.EscapeString (codebase, false, true, true);
			}
		}

		public CultureInfo CultureInfo {
			get { return cultureinfo; }
			set { cultureinfo = value; }
		}

		public AssemblyNameFlags Flags {
			get { return flags; }
			set { flags = value; }
		}

		public string FullName {
			get {
				if (name == null)
					return string.Empty;
				StringBuilder fname = new StringBuilder ();
				if (Char.IsWhiteSpace (name [0]))
					fname.Append ("\"" + name + "\"");
				else
					fname.Append (name);
				if (Version != null) {
					fname.Append (", Version=");
					fname.Append (Version.ToString ());
				}
				if (cultureinfo != null) {
					fname.Append (", Culture=");
					if (cultureinfo.LCID == CultureInfo.InvariantCulture.LCID)
						fname.Append ("neutral");
					else
						fname.Append (cultureinfo.Name);
				}
				byte [] pub_tok = InternalGetPublicKeyToken ();
				if (pub_tok != null) {
					if (pub_tok.Length == 0)
						fname.Append (", PublicKeyToken=null");
					else {
						fname.Append (", PublicKeyToken=");
						for (int i = 0; i < pub_tok.Length; i++)
							fname.Append (pub_tok[i].ToString ("x2"));
					}
				}

				if ((Flags & AssemblyNameFlags.Retargetable) != 0)
					fname.Append (", Retargetable=Yes");

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
				return version;
			}

			set {
				version = value;
				if (value == null)
					major = minor = build = revision = 0;
				else {
					major = value.Major;
					minor = value.Minor;
					build = value.Build;
					revision = value.Revision;
				}
			}
		}

		public AssemblyVersionCompatibility VersionCompatibility {
			get { return versioncompat; }
			set { versioncompat = value; }
		}
		
		public override string ToString ()
		{
			string name = FullName;
			return (name != null) ? name : base.ToString ();
		}

		public byte[] GetPublicKey()
		{
			return publicKey;
		}

		public byte[] GetPublicKeyToken ()
		{
			if (keyToken != null)
				return keyToken;
			else if (publicKey == null)
				return null;
			else {
				if (publicKey.Length == 0)
					return new byte [0];

				if (!IsPublicKeyValid)
					throw new  SecurityException ("The public key is not valid.");

				keyToken = ComputePublicKeyToken ();
				return keyToken;
			}
		}

		private bool IsPublicKeyValid {
			get {
				// check for ECMA key
				if (publicKey.Length == 16) {
					int i = 0;
					int sum = 0;
					while (i < publicKey.Length)
						sum += publicKey [i++];
					if (sum == 4)
						return true;
				}

				switch (publicKey [0]) {
				case 0x00: // public key inside a header
					if (publicKey.Length > 12 && publicKey [12] == 0x06) {
						try {
							CryptoConvert.FromCapiPublicKeyBlob (
								publicKey, 12);
							return true;
						} catch (CryptographicException) {
						}
					}
					break;
				case 0x06: // public key
					try {
						CryptoConvert.FromCapiPublicKeyBlob (publicKey);
						return true;
					} catch (CryptographicException) {
					}
					break;
				case 0x07: // private key
					break;
				}

				return false;
			}
		}

		private byte [] InternalGetPublicKeyToken ()
		{
			if (keyToken != null)
				return keyToken;

			if (publicKey == null)
				return null;

			if (publicKey.Length == 0)
				return new byte [0];

			if (!IsPublicKeyValid)
				throw new  SecurityException ("The public key is not valid.");

			return ComputePublicKeyToken ();
		}

		private byte [] ComputePublicKeyToken ()
		{
			HashAlgorithm ha = SHA1.Create ();
			byte [] hash = ha.ComputeHash (publicKey);
			// we need the last 8 bytes in reverse order
			byte [] token = new byte [8];
			Array.Copy (hash, (hash.Length - 8), token, 0, 8);
			Array.Reverse (token, 0, 8);
			return token;
		}

		[MonoTODO]
		public static bool ReferenceMatchesDefinition (AssemblyName reference, AssemblyName definition)
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			if (definition == null)
				throw new ArgumentNullException ("definition");
			if (reference.Name != definition.Name)
				return false;
			throw new NotImplementedException ();
		}

		public void SetPublicKey (byte[] publicKey) 
		{
			if (publicKey == null)
				flags ^= AssemblyNameFlags.PublicKey;
			else
				flags |= AssemblyNameFlags.PublicKey;
			this.publicKey = publicKey;
		}

		public void SetPublicKeyToken (byte[] publicKeyToken) 
		{
			keyToken = publicKeyToken;
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("_Name", name);
			info.AddValue ("_PublicKey", publicKey);
			info.AddValue ("_PublicKeyToken", keyToken);
			info.AddValue ("_CultureInfo", cultureinfo != null ? cultureinfo.LCID : -1);
			info.AddValue ("_CodeBase", codebase);
			info.AddValue ("_Version", Version);
			info.AddValue ("_HashAlgorithm", hashalg);
			info.AddValue ("_HashAlgorithmForControl", AssemblyHashAlgorithm.None);
			info.AddValue ("_StrongNameKeyPair", keypair);
			info.AddValue ("_VersionCompatibility", versioncompat);
			info.AddValue ("_Flags", flags);
			info.AddValue ("_HashForControl", null);
		}

		public object Clone() 
		{
			AssemblyName an = new AssemblyName ();
			an.name = name;
			an.codebase = codebase;
			an.major = major;
			an.minor = minor;
			an.build = build;
			an.revision = revision;
			an.version = version;
			an.cultureinfo = cultureinfo;
			an.flags = flags;
			an.hashalg = hashalg;
			an.keypair = keypair;
			an.publicKey = publicKey;
			an.keyToken = keyToken;
			an.versioncompat = versioncompat;
			return an;
		}

		public void OnDeserialization (object sender) 
		{
			Version = version;
		}

		public static AssemblyName GetAssemblyName (string assemblyFile) 
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			AssemblyName aname = new AssemblyName ();
			Assembly.InternalGetAssemblyName (Path.GetFullPath (assemblyFile), aname);
			return aname;
		}

		void _AssemblyName.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyName.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyName.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyName.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
