using System;
using System.IO;
using System.Reflection;


namespace Mono.Doc.Utils
{
	/// <summary>
	/// Provides a convenient way for programs that want to load assemblies
	/// both from files and from the current AppDomain to do so through
	/// the same interface.
	/// </summary>
	public sealed class AssemblyLoader
	{
		private const string INTERNAL_PREFIX = "int:";

		
		private AssemblyLoader()
		{
			// Can't instantiate this class
		}


		/// <summary>
		/// Given an assembly string, loads the specified assembly.
		/// </summary>
		/// <param name="assemblyString">
		/// The assembly to load.  If assemblyString matches "int:ASSEMBLYNAME" the
		/// assembly will be loaded through the current AppDomain rather than
		/// from a file on disk.
		/// </param>
		/// <returns>The loaded assembly</returns>
		public static Assembly Load(string assemblyString)
		{
			Assembly loadedAssembly = null;

			if (assemblyString == null || assemblyString == string.Empty) {
				throw new ArgumentException("assemblyString", "Invalid assembly specified for load.");
			}

			if (assemblyString.StartsWith(INTERNAL_PREFIX))  {
				string assemblyName = assemblyString.Substring(INTERNAL_PREFIX.Length);

				// load this assembly from the current AppDomain
				Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assem in loadedAssemblies) {
					if (assem.GetName().Name == assemblyName) {
						loadedAssembly = assem;
						break;
					}
				}

				if (loadedAssembly == null) {
					throw new AssemblyLoadException("Unable to load " + assemblyString);
				}
			} else {
				// load this assembly from a file
				try {
					loadedAssembly = Assembly.LoadFrom(assemblyString);
				} catch (FileNotFoundException) {
					throw new AssemblyLoadException("Unable to load: " + assemblyString);
				}
			}

			return loadedAssembly;
		}
	}
}
