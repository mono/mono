// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// This class collects static methods that build test cases.
	public class TestCaseBuilder
	{
		/// <summary>
		/// Makes a test case from a given method if any builders
		/// know how to do it and returns null otherwise.
		/// </summary>
		/// <param name="method">MethodInfo for the particular method</param>
		/// <returns>A test case or null</returns>
//		public static Test BuildFrom( MethodInfo method )
//		{
//			Test test = CoreExtensions.Host.TestBuilders.BuildFrom( method );
//
//			if ( test != null )
//				test = CoreExtensions.Host.TestDecorators.Decorate( test, method );
//
//			return test;
//		}
//
//		public static Test Decorate( test, method )
//		{
//			if ( test != null )
//				test = CoreExtensions.Host.TestDecorators.Decorate( test, method );
//
//			return test;
//		}
//
		/// <summary>
		/// Private constructor to prevent object creation
		/// </summary>
		private TestCaseBuilder() { }
	}
}
