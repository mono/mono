namespace NUnit.Framework 
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Thrown when an assertion failed.
	/// </summary>
	[Serializable]
	public class AssertionFailedError : ApplicationException//NUnitException
	{
		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected AssertionFailedError(SerializationInfo info, 
			StreamingContext context) : base(info,context){}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public AssertionFailedError (string message) : base(message) {}
//		public override bool IsAssertionFailure 
//		{
//			get{return true;}
//		}
	}
}