using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System
{
	[StructLayout (LayoutKind.Sequential)]
	partial class Exception
	{
		# region Keep in sync with MonoException in object-internals.h
		string _unused1;
		internal string _message;
		IDictionary _data;
		Exception _innerException;
		string _helpURL;
		object _traceIps;
		string _stackTraceString;
		string _unused3;
		int _unused4;
		object _unused5;
		int _HResult;
		string _source;
		object _unused6;
		StackTrace[] captured_traces;
		IntPtr[] native_trace_ips;
		int caught_in_unmanaged;
		#endregion

		public MethodBase TargetSite {
			get {
				StackTrace st = new StackTrace (this, true);
				if (st.FrameCount > 0)
					return st.GetFrame (0).GetMethod ();

				return null;
			}
		}

		public virtual String StackTrace => GetStackTrace (true);

		string GetStackTrace (bool needFileInfo)
		{
			if (_stackTraceString != null)
				return _stackTraceString;
			if (_traceIps == null)
				return null;

			return new StackTrace (this, needFileInfo).ToString (System.Diagnostics.StackTrace.TraceFormat.Normal);
		}

		internal readonly struct DispatchState
		{
		}

		internal DispatchState CaptureDispatchState ()
		{
			return default;
		}

		internal void RestoreDispatchState (in DispatchState state)
		{
		}

		string CreateSourceName ()
		{
			var st = new StackTrace (this, fNeedFileInfo: false);
			if (st.FrameCount > 0) {
				StackFrame sf = st.GetFrame (0);
				MethodBase method = sf.GetMethod ();

				Module module = method.Module;
				RuntimeModule rtModule = module as RuntimeModule;

				if (rtModule == null) {
					var moduleBuilder = module as System.Reflection.Emit.ModuleBuilder;
					if (moduleBuilder != null)
						throw new NotImplementedException (); // TODO: rtModule = moduleBuilder.InternalModule;
					else
						throw new ArgumentException (SR.Argument_MustBeRuntimeReflectionObject);
				}

				return rtModule.GetRuntimeAssembly ().GetName ().Name; // TODO: GetSimpleName ();
			}

			return null;
		}

		static IDictionary CreateDataContainer () => new ListDictionaryInternal ();

		static string SerializationWatsonBuckets => null;
		static string SerializationRemoteStackTraceString => null;
		static string SerializationStackTraceString => null;
	}
}
