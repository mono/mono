namespace NUnit.Runner 
{
	using System;

	/// <summary>
	/// An interface to define how a test suite should be loaded.
	/// </summary>
	[Obsolete("Use ILoader")]
	public interface ITestSuiteLoader 
	{
		/// <summary>
		/// 
		/// </summary>
		Type Load(string suiteClassName);
		//Type Reload(Type aType);
	}
}
