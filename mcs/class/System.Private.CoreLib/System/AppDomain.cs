using System.Reflection;
using System.Runtime.InteropServices;

namespace System {

	[StructLayout (LayoutKind.Sequential)]
	public sealed partial class AppDomain : MarshalByRefObject {
        #pragma warning disable 169
        #region Sync with object-internals.h
		IntPtr _mono_app_domain;
		#endregion
        #pragma warning restore 169

		public event AssemblyLoadEventHandler AssemblyLoad;

		private void DoAssemblyLoad (Assembly assembly)
		{
			return; /* FIXME */
		}
		
		private Assembly DoAssemblyResolve (string name, Assembly requestingAssembly, bool refonly)
		{
			return null;
		}

		public event UnhandledExceptionEventHandler UnhandledException;
	}
}
