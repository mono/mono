using System.Reflection;

namespace System.Runtime.Loader
{
	public abstract class AssemblyLoadContext
	{
		protected AssemblyLoadContext() { }
		protected AssemblyLoadContext(bool isCollectible) { }
		public bool IsCollectible { get { throw null; } }
		public void Unload() { throw null; }
		public static System.Runtime.Loader.AssemblyLoadContext Default { get { throw null; } }
		public static System.Reflection.AssemblyName GetAssemblyName(string assemblyPath) { throw null; }
		public static System.Runtime.Loader.AssemblyLoadContext GetLoadContext(System.Reflection.Assembly assembly) { throw null; }
		protected abstract System.Reflection.Assembly Load(System.Reflection.AssemblyName assemblyName);
		public System.Reflection.Assembly LoadFromAssemblyName(System.Reflection.AssemblyName assemblyName) { throw null; }
		public System.Reflection.Assembly LoadFromAssemblyPath(string assemblyPath) { throw null; }
		public System.Reflection.Assembly LoadFromNativeImagePath(string nativeImagePath, string assemblyPath) { throw null; }
		public System.Reflection.Assembly LoadFromStream(System.IO.Stream assembly) { throw null; }
		public System.Reflection.Assembly LoadFromStream(System.IO.Stream assembly, System.IO.Stream assemblySymbols) { throw null; }
		protected System.IntPtr LoadUnmanagedDllFromPath(string unmanagedDllPath) { throw null; }
		protected virtual System.IntPtr LoadUnmanagedDll(string unmanagedDllName) { throw null; }
		public void SetProfileOptimizationRoot(string directoryPath) { }
		public void StartProfileOptimization(string profile) { }
		public event Func<AssemblyLoadContext, System.Reflection.AssemblyName, System.Reflection.Assembly> Resolving;
		public event Action<AssemblyLoadContext> Unloading;
		
		internal static void OnProcessExit ()
		{
		}

		public static Assembly[] GetLoadedAssemblies() => throw new NotImplementedException();

		public static event AssemblyLoadEventHandler AssemblyLoad;
		public static event ResolveEventHandler TypeResolve;
		public static event ResolveEventHandler ResourceResolve;
		public static event ResolveEventHandler AssemblyResolve;
	}
}