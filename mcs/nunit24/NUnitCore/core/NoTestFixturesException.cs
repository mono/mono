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
	/// Summary description for NoTestFixtureException.
	/// </summary>
	[Serializable]
	public class NoTestFixturesException : ApplicationException
	{
		public NoTestFixturesException() : base () {}

		public NoTestFixturesException(string message) : base(message)
		{}

		public NoTestFixturesException(string message, Exception inner) : base(message, inner) {}

		protected NoTestFixturesException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}
}
