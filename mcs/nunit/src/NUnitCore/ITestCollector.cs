namespace NUnit.Runner 
{
	using System;
	
	/// <summary>
	///    Collects Test classes to be presented by the TestSelector.
	///  <see foocref="TestSelector"/>
	/// </summary>
	public interface ITestCollector 
	{
		/// <summary>
		///    Returns an array of FullNames for classes that are tests.
		/// </summary>
		string[] CollectTestsClassNames();
	}
}
