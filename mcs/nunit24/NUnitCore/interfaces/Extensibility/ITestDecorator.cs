// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The ITestDecorator interface is exposed by a class that knows how to
	/// enhance the functionality of a test case or suite by decorating it.
	/// </summary>
	public interface ITestDecorator
	{
		/// <summary>
		/// Examine the a Test and either return it as is, modify it
		/// or return a different TestCase.
		/// </summary>
		/// <param name="test">The Test to be decorated</param>
		/// <param name="member">The MethodInfo used to construct the test</param>
		/// <returns>The resulting Test</returns>
		Test Decorate( Test test, MemberInfo member );
	}
}
