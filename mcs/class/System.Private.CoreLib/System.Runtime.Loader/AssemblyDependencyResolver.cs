namespace System.Runtime.Loader
{
	public sealed class AssemblyDependencyResolver
	{
		public AssemblyDependencyResolver (string componentAssemblyPath)
		{
		}

		public string ResolveAssemblyToPath (System.Reflection.AssemblyName assemblyName)
		{
			throw new NotImplementedException ();
		}

		public string ResolveUnmanagedDllToPath (string unmanagedDllName)
		{
			throw new NotImplementedException ();
		}		
	}
}