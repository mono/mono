// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Framework
{
	using System;

	/// <example>
	/// [TestFixture]
	/// public class ExampleClass 
	/// {}
	/// </example>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	[Obsolete ("The NUnit framework shipped with Mono is deprecated and will be removed in a future release. It was based on NUnit 2.4 which is long outdated. Please move to the NUnit NuGet package or some other form of acquiring NUnit.", true)]
	public class TestFixtureAttribute : Attribute
	{
		private string description;

		/// <summary>
		/// Descriptive text for this fixture
		/// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}
	}
}
