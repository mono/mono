// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Framework 
{
	using System;
	using System.Runtime.Serialization;
	
	/// <summary>
	/// Thrown when an assertion failed.
	/// </summary>
	/// 
	[Serializable]
	public class AssertionException : System.Exception
	{
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		public AssertionException (string message) : base(message) 
		{}

		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		/// <param name="inner">The exception that caused the 
		/// current exception</param>
		public AssertionException(string message, Exception inner) :
			base(message, inner) 
		{}

		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected AssertionException(SerializationInfo info, 
			StreamingContext context) : base(info,context)
		{}

	}
}
