using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
	public static class Debugger
	{
		public static readonly string DefaultCategory;
		public static bool IsAttached { get { throw null; } }
		public static void Break() { }
		public static bool IsLogging() { throw null; }
		public static bool Launch() { throw null; }
		public static void Log(int level, string category, string message) {}
		public static void NotifyOfCrossThreadDependency() { }
	}
}