namespace NUnit.Framework 
{
	/// <summary>An <c>ITest</c> can be run and collect its results.</summary>
	/// <seealso cref="TestResult"/>
	public interface ITest 
	{
		/// <summary>
		/// Counts the number of test cases that will be run by this test.
		/// </summary>
		int CountTestCases { get; }
		/// <summary>
		/// Runs a test and collects its result in a 
		/// <see cref="TestResult"/> instance.
		/// </summary>
		/// <param name="result"></param>
		void Run(TestResult result);
	}
}
