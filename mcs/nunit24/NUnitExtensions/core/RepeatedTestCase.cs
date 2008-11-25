// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;

namespace NUnit.Core.Extensions
{
	/// <summary>
	/// RepeatedTestCase aggregates another test case and runs it
	/// a specified number of times.
	/// </summary>
	public class RepeatedTestCase : AbstractTestCaseDecoration
	{
		// The number of times to run the test
		int count;

		public RepeatedTestCase( TestCase testCase, int count )
			: base( testCase )
		{
			this.count = count;
		}

		public override void Run(TestCaseResult result)
		{
			// So testCase can get the fixture
			testCase.Parent = this.Parent;

			for( int i = 0; i < count; i++ )
			{
				testCase.Run( result );
				if ( result.IsFailure )
					return;
			}
		}
	}
}
