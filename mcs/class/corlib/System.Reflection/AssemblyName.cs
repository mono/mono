//
// System.Reflection/AssemblyName.cs
//
// Authors:
//	Paolo Molaro (lupus@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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
using System.Configuration.Assemblies;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Reflection {

// References:
// a.	Uniform Resource Identifiers (URI): Generic Syntax
//	http://www.ietf.org/rfc/rfc2396.txt

	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
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
		Version version;
		
		public AssemblyName ()
		{
			// defaults
			versioncompat = AssemblyVersionCompatibility.SameMachine;
		}

#if NET_2_0
		public AssemblyName (string assemblyName)
		{
			name = assemblyName;
		}
#endif

		internal AssemblyName (SerializationInfo si, StreamingContext sc)
		{
			name = si.GetString ("_Name");
			codebase = si.GetString ("_CodeBase");
			version = (Version)si.GetValue ("_Version", typeof (Version));
			publicKey = (byte[])si.GetValue ("_PublicKey", typeof (byte[]));
			keyToken = (byte[])si.GetValue ("_PublicToken", typeof (byte[]));
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

		public string FullName {
			get {
				if (name == null)
					return null;
				StringBuilder fname = new StringBuilder ();
				fname.Append (name);
				fname.Append (", Version=");
				fname.Append (Version.ToString ());
				fname.Append (", Culture=");
				if ((cultureinfo == null) || (cultureinfo.LCID == CultureInfo.InvariantCulture.LCID))
					fname.Append ("neutral");
				else
					fname.Append (cultureinfo.Name);
				byte[] pub_tok = GetPublicKeyToken ();
				if (pub_tok == null || pub_tok.Length == 0)
					fname.Append (", PublicKeyToken=null");
				else {
					fname.Append (", PublicKeyToken=");
					for (int i = 0; i < pub_tok.Length; i++)
						fname.Append (pub_tok[i].ToString ("x2"));
				}
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
				if (version != null) return version;
				
				if (name == null)
					return null;
				if (build == -1)
					version = new Version (major, minor);
				else
					if (revision == -1)
						version = new Version (major, minor, build);
				else
					version = new Version (major, minor, build, revision);

				return version;
			}

			set {
				major = value.Major;
				minor = value.Minor;
				build = value.Build;
				revision = value.Revision;
				version = value;
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
			// to match MS implementation -- funny one
			if (publicKey != null)
				return publicKey;
			else if (name == null)
				return null;
			else
				return new byte [0];
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
			info.AddValue ("_PublicKey", publicKey);
			info.AddValue ("_PublicToken", keyToken);
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
			Assembly.InternalGetAssemblyName (System.IO.Path.GetFullPath (assemblyFile), aname);
			return aname;
		}
	}
}
