// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Summary description for NoTestMethodsException.
	/// </summary>
	/// 
	[Serializable]
	public class InvalidTestFixtureException : ApplicationException
	{
		public InvalidTestFixtureException() : base() {}

		public InvalidTestFixtureException(string message) : base(message)
		{}

		public InvalidTestFixtureException(string message, Exception inner) : base(message, inner)
		{}

		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected InvalidTestFixtureException(SerializationInfo info, 
			StreamingContext context) : base(info,context){}

	}
}