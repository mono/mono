// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// TestNode represents a single test or suite in the test hierarchy.
	/// TestNode holds common info needed about a test and represents a
	/// single node - either a test or a suite - in the hierarchy of tests.
	/// 
	/// TestNode extends TestInfo, which holds all the information with
	/// the exception of the list of child classes. When constructed from
	/// a Test, TestNodes are always fully populated with child TestNodes.
	/// 
	/// Like TestInfo, TestNode is purely a data class, and is not able
	/// to execute tests.
	/// 
	/// </summary>
	[Serializable]
	public class TestNode : TestInfo
	{
		#region Instance Variables
		private ITest parent;

		/// <summary>
		/// For a test suite, the child tests or suites
		/// Null if this is not a test suite
		/// </summary>
		private ArrayList tests;
		#endregion

		#region Constructors
		/// <summary>
		/// Construct from an ITest
		/// </summary>
		/// <param name="test">Test from which a TestNode is to be constructed</param>
		public TestNode ( ITest test ) : base( test )
		{
			if ( test.IsSuite )
			{
				this.tests = new ArrayList();
				
				foreach( ITest child in test.Tests )
				{
					TestNode node = new TestNode( child );
					this.Tests.Add( node );
					node.parent = this;
				}
			}
		}

        /// <summary>
        /// Construct a TestNode given a TestName and an
        /// array of child tests.
        /// </summary>
        /// <param name="testName">The TestName of the new test</param>
        /// <param name="tests">An array of tests to be added as children of the new test</param>
	    public TestNode ( TestName testName, ITest[] tests ) : base( testName, tests )
		{
			this.tests = new ArrayList();
			this.tests.AddRange( tests );
		}
		#endregion

		#region Properties
        /// <summary>
        /// Gets the parent test of the current test
        /// </summary>
		public override ITest Parent
		{
			get { return parent; }
		}

		/// <summary>
		/// Array of child tests, null if this is a test case.
		/// </summary>
		public override IList Tests 
		{
			get { return tests; }
		}
		#endregion
	}
}
