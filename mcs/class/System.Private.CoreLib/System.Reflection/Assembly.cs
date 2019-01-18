namespace System.Reflection
{
	partial class Assembly
	{
#region Must match object-internals.h
#pragma warning disable 649
		internal IntPtr _mono_assembly;
#pragma warning restore 649

		private ResolveEventHolder resolve_event_holder;
		object _evidence, _minimum, _optional, _refuse, _granted, _denied;
		private bool fromByteArray;
		private string assemblyName;
#endregion

		internal class ResolveEventHolder {
#pragma warning disable 67
			public event ModuleResolveEventHandler ModuleResolve;
#pragma warning restore
		}

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