namespace NUnit.Runner 
{
	using System;
	using System.Reflection;
	using NUnit.Framework;

	/// <summary>
	/// An implementation of a TestCollector that loads
	/// all classes on the class path and tests whether
	/// it is assignable from ITest or provides a static Suite property.
	/// <see cref="ITestCollector"/>
	/// </summary>
	[Obsolete("Use StandardLoader or UnloadingLoader")]
	public class LoadingClassPathTestCollector: ClassPathTestCollector 
	{
	
		TestCaseClassLoader fLoader;
		/// <summary>
		/// 
		/// </summary>
		public LoadingClassPathTestCollector() 
		{
			fLoader= new TestCaseClassLoader();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="classFileName"></param>
		/// <returns></returns>
		protected override bool IsTestClass(string classFileName) 
		{	
			try 
			{
				if (classFileName.EndsWith(".dll") || classFileName.EndsWith(".exe")) 
				{
					Type testClass= ClassFromFile(classFileName);
					return (testClass != null); //HACK: && TestCase.IsTest(testClass);
				}
			} 
			catch (TypeLoadException) 
			{
			} 
			return false;
		}
	
		private Type ClassFromFile(string classFileName) 
		{
			string className = base.ClassNameFromFile(classFileName);
			if (!fLoader.IsExcluded(className))
				return fLoader.LoadClass(className, false);
			return null;
		}
	}
}