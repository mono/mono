
//
// System.Reflection/AssemblyName.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Globalization;
using System.Configuration.Assemblies;
using System.Runtime.Serialization;

namespace System.Reflection {

	[Serializable]
	public sealed class AssemblyName  : ISerializable // ICloneable, , IDeserializationCallback
	{
		string name = "";
		string codebase;
		int major, minor, build, revision;
		CultureInfo cultureinfo;
		AssemblyNameFlags flags;
		AssemblyHashAlgorithm hashalg;
		StrongNameKeyPair keypair;
		AssemblyVersionCompatibility versioncompat;
		
		public AssemblyName ()
		{
		}

		internal AssemblyName (SerializationInfo si, StreamingContext sc)
		{
			name = si.GetString ("_Name");
			codebase = si.GetString ("_CodeBase");
			Version = (Version)si.GetValue ("_Version", typeof (Version));
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public string CodeBase {
			get {
				return codebase;
			}

			set {
				codebase = value;
			}
		}

		[MonoTODO]
		public string EscapedCodeBase {
			get {
				return codebase;
			}
		}

		public CultureInfo CultureInfo {
			get {
				return cultureinfo;
			}

			set {
				cultureinfo = value;
			}
		}

		public AssemblyNameFlags Flags {
			get {
				return flags;
			}

			set {
				flags = value;
			}
		}

		[MonoTODO]
		public string FullName {
			get {
				return name;
			}
		}

		public AssemblyHashAlgorithm HashAlgorithm {
			get {
				return hashalg;
			}

			set {
				hashalg = value;
			}
		}

		public StrongNameKeyPair KeyPair {
			get {
				return keypair;
			}

			set {
				keypair = value;
			}
		}

		public Version Version {
			get {
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
			get {
				return versioncompat;
			}

			set {
				versioncompat = value;
			}
		}
		
		public override int GetHashCode ()
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
		}

		public override string ToString ()
		{
			string name = FullName;
			return (name != null) ? name : base.ToString ();
		}

		[MonoTODO]
		public byte[] GetPublicKeyToken() {
			return new byte[0];
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("_Name", name);
			info.AddValue ("_CodeBase", codebase);
			info.AddValue ("_Version", Version);
		}
	}
}
