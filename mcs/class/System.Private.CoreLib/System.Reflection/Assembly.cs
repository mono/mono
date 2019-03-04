using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Reflection
{
	[StructLayout (LayoutKind.Sequential)]
	partial class Assembly
	{
		internal bool IsRuntimeImplemented () => this is RuntimeAssembly;

		internal virtual IntPtr MonoAssembly {
			get {
				throw new NotImplementedException ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetExecutingAssembly ();

		internal static RuntimeAssembly GetExecutingAssembly (ref StackCrawlMark stackMark)
		{
			return (RuntimeAssembly) GetExecutingAssembly ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetCallingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetEntryAssembly ();

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static Assembly Load (string assemblyString)
		{
			if (assemblyString == null)
				throw new ArgumentNullException (nameof (assemblyString));

			var name = new AssemblyName (assemblyString);
			// TODO: trigger assemblyFromResolveEvent

			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Load (name, ref stackMark, IntPtr.Zero);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static Assembly Load (AssemblyName assemblyRef)
		{
			if (assemblyRef == null)
				throw new ArgumentNullException (nameof (assemblyRef));

			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Load (assemblyRef, ref stackMark, IntPtr.Zero);
		}

		internal static Assembly Load (AssemblyName assemblyRef, ref StackCrawlMark stackMark, IntPtr ptrLoadContextBinder)
		{
			// TODO: pass AssemblyName
			var assembly = InternalLoad (assemblyRef.FullName, ref stackMark, ptrLoadContextBinder);
			if (assembly == null)
				throw new FileNotFoundException (null, assemblyRef.Name);
			return assembly;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern Assembly InternalLoad (string assemblyName, ref StackCrawlMark stackMark, IntPtr ptrLoadContextBinder);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type InternalGetType (Module module, string name, bool throwOnError, bool ignoreCase);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void InternalGetAssemblyName (string assemblyFile, out Mono.MonoAssemblyName aname, out string codebase);

		#region to be removed

		public static Assembly LoadFrom (string assemblyFile)
		{
			throw new NotImplementedException ();
		}

		public static Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		public static Assembly LoadFile (string path) { throw null; }

		public static Assembly LoadFrom (string assemblyFile, byte[] hashValue, System.Configuration.Assemblies.AssemblyHashAlgorithm hashAlgorithm) { throw null; }
		
		#endregion
	}
}