namespace NUnit.Runner 
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;

	/// <summary>
	/// A TestCollector that consults the
	/// class path. It considers all classes on the class path
	/// excluding classes in JARs. It leaves it up to subclasses
	/// to decide whether a class is a runnable Test.
	/// <see cref="ITestCollector"/>
	/// </summary>
	[Obsolete("Use StandardLoader or UnloadingLoader")]
	public abstract class ClassPathTestCollector : ITestCollector 
	{
		/// <summary>
		/// 
		/// </summary>
		public ClassPathTestCollector() {}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] CollectTestsClassNames()
		{
			string classPath = Environment.GetEnvironmentVariable("Path");
			char separator= Path.PathSeparator;
			ArrayList result = new ArrayList();
			CollectFilesInRoots(classPath.Split(separator), result);
			string[] retVal = new string[result.Count];
			result.CopyTo(retVal);
			return retVal;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="classFileName"></param>
		/// <returns></returns>
		protected string ClassNameFromFile(string classFileName) 
		{
			return classFileName;
		}

		private void CollectFilesInRoots(string[] roots, IList result)
		{
			foreach (string directory in roots)
			{
				DirectoryInfo dirInfo=new DirectoryInfo(directory);
				if (dirInfo.Exists)
				{
					string[] files=Directory.GetFiles(dirInfo.FullName);
					foreach (string file in files)
					{
						if (IsTestClass(file))
						{
							string className=ClassNameFromFile(file);
							result.Add(className);
						}
					}
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="classFileName"></param>
		/// <returns></returns>
		protected virtual bool IsTestClass(string classFileName) 
		{
			return 
				(  classFileName.EndsWith(".dll")
					|| classFileName.EndsWith(".exe"))
				&& classFileName.IndexOf("Test") > 0;
		}
	}
}