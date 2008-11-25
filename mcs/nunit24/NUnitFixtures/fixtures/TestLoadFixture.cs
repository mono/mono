// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections;
using NUnit.Core;
using NUnit.Util;

namespace NUnit.Fixtures
{
	/// <summary>
	/// Abstract base class for fixtures that load and run a test assembly.
	/// </summary>
	public abstract class TestLoadFixture : fit.ColumnFixture
	{
		protected TestRunner testRunner;
		protected TestResult testResult;
		protected ResultSummarizer testSummary;

		protected void LoadAndRunTestAssembly( fit.Parse cell, string testAssembly )
		{
			testRunner = new TestDomain();

			if ( !testRunner.Load( new TestPackage(testAssembly) ) )
			{
				this.wrong(cell);
				cell.addToBody( string.Format( 
					"<font size=-1 color=\"#c08080\"> <i>Failed to load {0}</i></font>", testAssembly ) );

				return;
			}

			testResult = testRunner.Run(NullListener.NULL);
			testSummary = new ResultSummarizer( testResult );

			this.right( cell );
		}

		public override void wrong(fit.Parse cell)
		{
			string body = cell.body;
			base.wrong (cell);
			cell.body = body;
		}

		public int Skipped()
		{
			return testRunner.Test.TestCount - testSummary.ResultCount - testSummary.IgnoreCount;
		}

		public int Tests()
		{
			return testRunner.Test.TestCount;
		}

		public int Run()
		{
			return testSummary.ResultCount;
		}

		public int Failures()
		{
			return testSummary.FailureCount;
		}

		public int Ignored()
		{
			return testSummary.IgnoreCount;
		}
	}
}
