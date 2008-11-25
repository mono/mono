// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Framework
{
	using System;

	/// <summary>
	/// Attribute used to mark a static (shared in VB) property
	/// that returns a list of tests.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class SuiteAttribute : Attribute
	{}
}
