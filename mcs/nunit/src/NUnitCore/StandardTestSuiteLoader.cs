namespace NUnit.Runner 
{
	using System;
	using System.Reflection;
	using System.IO;
	using System.Security;

	/// <summary>
	/// The standard test suite loader. It can only load the same
	/// class once.
	/// </summary>
	[Obsolete("Use StandardLoader or UnloadingLoader")]
	public class StandardTestSuiteLoader: ITestSuiteLoader 
	{
		/// <summary>
		/// Loads 
		/// </summary>
		/// <param name="testClassName"></param>
		/// <returns></returns>
		public Type Load(string testClassName) 
		{
			Type testClass;
			string[] classSpec=testClassName.Split(',');
			if (classSpec.Length > 1) 
			{
				FileInfo dll=new FileInfo(classSpec[1]);
				if (!dll.Exists) 
					throw new FileNotFoundException("File " + dll.FullName + " not found", dll.FullName);
				Assembly a = Assembly.LoadFrom(dll.FullName);
				testClass=a.GetType(classSpec[0], true);
			}
			else
				testClass = Type.GetType(testClassName, true);
			return testClass;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aClass"></param>
		/// <returns></returns>
		public Type Reload(Type aClass) 
		{
			return aClass;
		}
	}
}