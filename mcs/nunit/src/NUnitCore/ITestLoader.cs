namespace NUnit.Runner
{
	using System;
	using NUnit.Framework;
	using System.Runtime.Serialization;

	/// <summary>
	/// Basic contract governing loading of tests
	/// </summary>
	public interface ITestLoader
	{
		/// <summary>
		/// Loads an instance of the test class specified by the name.
		/// Loadable in most cases will be an assembly qualified name.
		/// 
		/// Other loaders could dynamically construct a test case from
		/// an XML file or a database record.
		/// </summary>
		ITest LoadTest(string loadableName);

		/// <summary>
		/// Return the name used by the loader to load an instance 
		/// of the supplied test
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		string GetLoadName(ITest test);
	}
	
	/// <summary>
	/// Error thrown during assembly and class loading problems
	/// </summary>
	[Serializable]
	public class LoaderException : NUnitException
	{
		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected LoaderException(SerializationInfo info, 
			StreamingContext context) : base(info,context){}
		/// <summary>
		/// Standard constructor
		/// </summary>
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		/// <param name="innerException">The exception that caused the 
		/// current exception</param>
		public LoaderException(string message, Exception innerException) :
			base(message, innerException) {}
		/// <summary>
		/// Standard constructor
		/// </summary>
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		public LoaderException(string message) :
			base(message) {}
	}
}