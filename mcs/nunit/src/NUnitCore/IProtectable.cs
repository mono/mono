namespace NUnit.Framework 
{
	/// <summary>
	/// An <c>IProtectable</c> can be run and can throw an Exception.
	/// </summary>
	/// <seealso cref="TestResult"/>
	public interface IProtectable 
	{
		/// <summary>Run the the following method protected.</summary>
		void Protect();
	}
}