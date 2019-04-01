using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Mono;

namespace System
{
	partial class Environment
	{
		// This is the version of the corlib-runtime interface (defined in configure.ac)
		private const string mono_corlib_version = Consts.MonoCorlibVersion;

		public static int CurrentManagedThreadId => Thread.CurrentThread.ManagedThreadId;

		public extern static int ExitCode {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			set;
		}

		public static extern bool HasShutdownStarted {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public static extern int ProcessorCount {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public static string StackTrace {
			[MethodImpl (MethodImplOptions.NoInlining)] // Prevent inlining from affecting where the stacktrace starts
			get => new StackTrace (true).ToString (System.Diagnostics.StackTrace.TraceFormat.Normal);
		}

		public extern static int TickCount {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Exit (int exitCode);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string[] GetCommandLineArgs ();

		static string GetEnvironmentVariableCore (string variable)
		{
			using (var h = RuntimeMarshal.MarshalString (variable)) {
				return internalGetEnvironmentVariable_native (h.Value);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static string internalGetEnvironmentVariable_native (IntPtr variable);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetEnvironmentVariableNames ();

		public static IDictionary GetEnvironmentVariables ()
		{
			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = GetEnvironmentVariableCore (name);
			}
			return vars;
		}

		static unsafe void SetEnvironmentVariableCore (string variable, string value)
		{
			fixed (char *fixed_variable = variable)
			fixed (char *fixed_value = value)
				InternalSetEnvironmentVariable (fixed_variable, variable.Length, fixed_value, value?.Length ?? 0);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe void InternalSetEnvironmentVariable (char *variable, int variable_length, char *value, int value_length);

		public static void FailFast (string message)
		{
			throw new NotImplementedException ();
		}

		public static void FailFast(string message, Exception exception)
		{
			throw new NotImplementedException ();
		}

		public static void FailFast (string message, Exception exception, string errorMessage)
		{
			throw new NotImplementedException ();
		}
	}

#region referencesource dependencies - to be removed

	partial class Environment
	{
		internal static string GetResourceString (string key)
		{
			return key;
		}

		internal static string GetResourceString (string key, CultureInfo culture)
		{
			return key;
		}

		internal static string GetResourceString (string key, params object[] values)
		{
			return string.Format (CultureInfo.InvariantCulture, key, values);
		}
	}
#endregion
}