using System;
using NUnit.Framework;

namespace NUnit.Runner
{
	/// <summary>
	/// Collects the names of all classes in an assembly that are tests.
	/// </summary>
	public sealed class AssemblyTestCollector : MarshalByRefObject, ITestCollector
	{
		#region Instance Variables
		private string fAssemblyName;
		private StandardLoader fLoader;
		#endregion
		
		#region Constructors
		/// <summary>
		/// Create a new AssemblyTestCollector for the specified 
		/// assembly, and uses the supplied loader to load the tests
		/// from the assembly.
		/// </summary>
		/// <param name="assemblyName">The file name of the assembly
		/// from which to load classes</param>
		/// <param name="loader">An instance if the standard loader to 
		/// use for loading tests from the assembly.</param>
		public AssemblyTestCollector(string assemblyName, 
			StandardLoader loader)
		{
			if(loader!=null)
				fLoader = loader;
			else
				throw new ArgumentNullException("loader");

			if(assemblyName != null)
			{
				fAssemblyName = assemblyName;
			}
			else
				throw new ArgumentNullException("assemblyName");

		}
		/// <summary>
		/// Create a new AssemblyTestCollector for the specified 
		/// assembly.
		/// </summary>
		/// <param name="assemblyName">The file name of the assembly
		/// from which to load classes.</param>
		public AssemblyTestCollector(string assemblyName)
			: this(assemblyName,new StandardLoader()){}
		/// <summary>
		/// returns a System.String[] of FullNames for all test classes 
		/// contained within the assembly.
		/// Implements ITestCollector.CollectTestsClassNames()
		/// </summary>
		#endregion

		#region ITestCollector Methods
		public string[] CollectTestsClassNames()
		{
			Type[] tests = fLoader.GetTestTypes(fAssemblyName);
			string[] ret = new string[tests.Length];
			int i=0;
			foreach (Type testType in tests)
			{
				ret[i] = testType.FullName;
				i++;
			}
			return ret;
		}
		#endregion
	}
}
