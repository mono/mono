namespace System.Reflection
{
	partial class Assembly
	{
		public static Assembly LoadFrom (string assemblyFile)
		{
			throw new NotImplementedException ();
		}

		public static Assembly GetCallingAssembly () { throw null; }

		public static Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		public static Assembly GetEntryAssembly ()
		{
			throw new NotImplementedException ();
		}

		public static Assembly GetExecutingAssembly () { throw null; }

		public static Assembly Load (AssemblyName assemblyRef) { throw null; }

		public static Assembly LoadFile (string path) { throw null; }

		public static Assembly LoadFrom (string assemblyFile, byte[] hashValue, System.Configuration.Assemblies.AssemblyHashAlgorithm hashAlgorithm) { throw null; }

		public static Assembly Load (string unused)
		{
			throw new NotImplementedException ();
		}
	}
}