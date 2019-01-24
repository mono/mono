namespace System.Reflection
{
	public sealed partial class AssemblyName : System.ICloneable, System.Runtime.Serialization.IDeserializationCallback, System.Runtime.Serialization.ISerializable
	{
		public AssemblyName() { }
		public AssemblyName(string assemblyName) { }
		public string CodeBase { get { throw null; } set { } }
		public System.Reflection.AssemblyContentType ContentType { get { throw null; } set { } }
		public System.Globalization.CultureInfo CultureInfo { get { throw null; } set { } }
		public string CultureName { get { throw null; } set { } }
		public string EscapedCodeBase { get { throw null; } }
		public System.Reflection.AssemblyNameFlags Flags { get { throw null; } set { } }
		public string FullName { get { throw null; } }
		public System.Configuration.Assemblies.AssemblyHashAlgorithm HashAlgorithm { get { throw null; } set { } }
		public System.Reflection.StrongNameKeyPair KeyPair { get { throw null; } set { } }
		public string Name { get { throw null; } set { } }
		public System.Reflection.ProcessorArchitecture ProcessorArchitecture { get { throw null; } set { } }
		public System.Version Version { get { throw null; } set { } }
		public System.Configuration.Assemblies.AssemblyVersionCompatibility VersionCompatibility { get { throw null; } set { } }
		public object Clone() { throw null; }
		public static System.Reflection.AssemblyName GetAssemblyName(string assemblyFile) { throw null; }
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
		public byte[] GetPublicKey() { throw null; }
		public byte[] GetPublicKeyToken() { throw null; }
		public void OnDeserialization(object sender) { }
		public static bool ReferenceMatchesDefinition(System.Reflection.AssemblyName reference, System.Reflection.AssemblyName definition) { throw null; }
		public void SetPublicKey(byte[] publicKey) { }
		public void SetPublicKeyToken(byte[] publicKeyToken) { }
		public override string ToString() { throw null; }

		internal static string EscapeCodeBase (string codebase) { throw null; }
	}
}