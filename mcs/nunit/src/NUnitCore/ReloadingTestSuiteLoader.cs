namespace NUnit.Runner 
{

	using System;

	/// <summary>A TestSuite loader that can reload classes.</summary>
	[Obsolete("Use StandardLoader or UnloadingLoader")]
	public class ReloadingTestSuiteLoader: ITestSuiteLoader 
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="suiteClassName"></param>
		/// <returns></returns>
		public Type Load(string suiteClassName) 
		{
			//            TestCaseClassLoader loader= new TestCaseClassLoader();
			//            return loader.LoadClass(suiteClassName, true);
			return Type.GetType(suiteClassName, true);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="aClass"></param>
		/// <returns></returns>
		public Type Reload(Type aClass) 
		{
			//            TestCaseClassLoader loader= new TestCaseClassLoader();
			//            return loader.LoadClass(aClass.ToString(), true);
			return aClass;
		}
	}
}
