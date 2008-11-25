// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core
{
	/// <summary>
	/// SuiteBuilderAttribute is used to mark custom suite builders.
	/// The class so marked must implement the ISuiteBuilder interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public sealed class SuiteBuilderAttribute : Attribute
	{}
}
