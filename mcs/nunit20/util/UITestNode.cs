#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Util
{
	using System;
	using System.Collections;
	using NUnit.Core;

	/// <summary>
	/// UITestNode holds common info needed about a test
	/// in the UI, avoiding the remoting issues associated
	/// with holding an actual Test object.
	/// </summary>
	public class UITestNode : ITest
	{
		#region Instance Variables

		/// <summary>
		/// The full name of the test, including the assembly and namespaces
		/// </summary>
		private string fullName;

		/// <summary>
		/// The test name
		/// </summary>
		private string testName;

		/// <summary>
		/// Used to distinguish tests in multiple assemblies;
		/// </summary>
		private int assemblyKey;

		/// <summary>
		/// True if the test should be run
		/// </summary>
		private bool shouldRun;

		/// <summary>
		/// Reason for not running the test
		/// </summary>
		private string ignoreReason;

		/// <summary>
		/// Number of test cases in this test or suite
		/// </summary>
		private int testCaseCount;

		/// <summary>
		/// For a test suite, the child tests or suites
		/// Null if this is not a test suite
		/// </summary>
		private ArrayList tests;

		/// <summary>
		/// True if this is a suite
		/// </summary>
		private bool isSuite;

		/// <summary>
		/// Interface of the test suite from which this 
		/// object was constructed. Used for deferred 
		/// population of the object.
		/// </summary>
		private ITest testSuite;

		/// <summary>
		/// The test description
		/// </summary>
		private string description;

		private ArrayList categories = new ArrayList();

		private bool isExplicit;

		#endregion

		#region Construction and Conversion

		/// <summary>
		/// Construct from a TestInfo interface, which might be
		/// a Test or another UITestNode. Optionally, populate
		/// the array of child tests.
		/// </summary>
		/// <param name="test">TestInfo interface from which a UITestNode is to be constructed</param>
		/// <param name="populate">True if child array is to be populated</param>
		public UITestNode ( ITest test, bool populate )
		{
			fullName = test.FullName;
			testName = test.Name;
			assemblyKey = test.AssemblyKey;
			shouldRun = test.ShouldRun;
			ignoreReason = test.IgnoreReason;
			description = test.Description;
			isExplicit = test.IsExplicit;

			if (test.Categories != null) 
			{
				categories.AddRange(test.Categories);
			}

			if ( test is UITestNode )
				testCaseCount = 0;
			
			if ( test.IsSuite )
			{
				testCaseCount = 0;
				testSuite = test;
				isSuite = true;

				tests = new ArrayList();

				if ( populate ) PopulateTests();
			}
			else
			{
				testCaseCount = 1;
				isSuite = false;
			}
		}

		/// <summary>
		/// Default construction uses lazy population approach
		/// </summary>
		/// <param name="test"></param>
		public UITestNode ( ITest test ) : this( test, false ) { }

		public UITestNode ( string pathName, string testName )
			: this( pathName, testName, 0 ) { }

		public UITestNode ( string pathName, string testName, int assemblyKey )
		{
			this.fullName = pathName + "." + testName;
			this.testName = testName;
			this.assemblyKey = assemblyKey;
			this.shouldRun = true;
			this.isSuite = false;
			this.testCaseCount = 1;
		}

		/// <summary>
		/// Populate the arraylist of child Tests recursively.
		/// If already populated, it has no effect.
		/// </summary>
		public void PopulateTests()
		{
			if ( !Populated )
			{
				foreach( ITest test in testSuite.Tests )
				{
					UITestNode node = new UITestNode( test, true );
					tests.Add( node );
					testCaseCount += node.CountTestCases();
				}

				testSuite = null;
			}
		}

		/// <summary>
		/// Allow implicit conversion of a Test to a TestInfo
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		public static implicit operator UITestNode( Test test )
		{
			return new UITestNode( test );
		}

		#endregion

		#region Properties

		/// <summary>
		/// The test description 
		/// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		/// <summary>
		/// The reason for ignoring a test
		/// </summary>
		public string IgnoreReason
		{
			get { return ignoreReason; }
			set { ignoreReason = value; }
		}

		/// <summary>
		/// True if the test should be run
		/// </summary>
		public bool ShouldRun
		{
			get { return shouldRun; }
			set { shouldRun = value; }
		}

		/// <summary>
		/// Full name of the test
		/// </summary>
		public string FullName 
		{
			get { return fullName; }
		}

		/// <summary>
		/// Name of the test
		/// </summary>
		public string Name
		{
			get { return testName; }
		}

		/// <summary>
		/// Identifier for assembly containing this test
		/// </summary>
		public int AssemblyKey
		{
			get { return assemblyKey; }
			set { assemblyKey = value; }
		}

		public string UniqueName
		{
			get{ return string.Format( "[{0}]{1}", assemblyKey, fullName ); }
		}

		/// <summary>
		/// If the name is a path, this just returns the file part
		/// </summary>
		public string ShortName
		{
			get
			{
				string name = Name;
				int val = name.LastIndexOf("\\");
				if(val != -1)
					name = name.Substring(val+1);
				return name;
			}
		}

		public bool IsExplicit
		{
			get { return isExplicit; }
			set { isExplicit = value; }
		}

		public IList Categories 
		{
			get { return categories; }
		}

		public bool HasCategory( string name )
		{
			return categories != null && categories.Contains( name );
		}

		public bool HasCategory( IList names )
		{
			if ( categories == null )
				return false;

			foreach( string name in names )
				if ( categories.Contains( name ) )
					return true;

			return false;
		}

		/// <summary>
		/// Count of test cases in this test. If the suite
		/// has never been populated, it will be done now.
		/// </summary>
		public int CountTestCases()
		{ 
			if ( !Populated )
				PopulateTests();

			return testCaseCount; 
		}

		/// <summary>
		/// Array of child tests, null if this is a test case.
		/// The array is populated on access if necessary.
		/// </summary>
		public ArrayList Tests 
		{
			get 
			{
				if ( !Populated )
					PopulateTests();

				return tests;
			}
		}

		/// <summary>
		/// True if this is a suite, false if a test case
		/// </summary>
		public bool IsSuite
		{
			get { return isSuite; }
		}

		/// <summary>
		/// True if this is a test case, false if a suite
		/// </summary>
		public bool IsTestCase
		{
			get { return !isSuite; }
		}

		/// <summary>
		/// True if this is a fixture. May populate the test's
		/// children as a side effect.
		/// TODO: An easier way to tell this?
		/// </summary>
		public bool IsFixture
		{
			get
			{
				// A test case is obviously not a fixture
				if ( IsTestCase ) return false;

				// We have no way of constructing an empty suite unless it's a fixture
				if ( Tests.Count == 0 ) return true;
				
				// Any suite with children is a fixture if the children are test cases
				UITestNode firstChild = (UITestNode)Tests[0];
				return !firstChild.IsSuite;
			}
		}

		/// <summary>
		/// False for suites that have not yet been populated
		/// with their children, otherwise true - used for testing.
		/// </summary>
		public bool Populated
		{
			get { return testSuite == null; }
		}

		public TestResult Run( EventListener listener )
		{
			throw new InvalidOperationException( "Cannot use Run on a local copy of Test data" );
		}

		#endregion
	}
}
