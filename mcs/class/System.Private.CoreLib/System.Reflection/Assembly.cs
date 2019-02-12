using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[StructLayout (LayoutKind.Sequential)]
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

		internal bool IsRuntimeImplemented () => throw new NotImplementedException ();

		internal virtual IntPtr MonoAssembly {
			get {
				throw new NotImplementedException ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type InternalGetType (Module module, String name, Boolean throwOnError, Boolean ignoreCase);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern unsafe static void InternalGetAssemblyName (string assemblyFile, out Mono.MonoAssemblyName aname, out string codebase);
	}
}