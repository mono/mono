using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
	public static class Debugger
	{
		public static readonly string DefaultCategory = "";

		public static bool IsAttached => IsAttached_internal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool IsAttached_internal ();

		public static void Break ()
		{
			// The JIT inserts a breakpoint on the caller.
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern bool IsLogging();

		public static bool Launch ()
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern void Log (int level, string category, string message);

		public static void NotifyOfCrossThreadDependency ()
		{
		}
	}
}
