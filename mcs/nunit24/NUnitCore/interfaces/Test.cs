// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Reflection;

	/// <summary>
	///		Test Class.
	/// </summary>
	public abstract class Test : ITest, IComparable
	{
		#region Fields
		/// <summary>
		/// TestName that identifies this test
		/// </summary>
		private TestName testName;

		/// <summary>
		/// Indicates whether the test should be executed
		/// </summary>
		private RunState runState;

		/// <summary>
		/// The reason for not running the test
		/// </summary>
		private string ignoreReason;
		
		/// <summary>
		/// Description for this test 
		/// </summary>
		private string description;
		
		/// <summary>
		/// Test suite containing this test, or null
		/// </summary>
		private Test parent;
		
		/// <summary>
		/// List of categories applying to this test
		/// </summary>
		private IList categories;

		/// <summary>
		/// A dictionary of properties, used to add information
		/// to tests without requiring the class to change.
		/// </summary>
		private IDictionary properties;

		/// <summary>
		/// The System.Type of the fixture for this test suite, if there is one
		/// </summary>
		private Type fixtureType;

		/// <summary>
		/// The fixture object, if it has been created
		/// </summary>
		private object fixture;

		#endregion

		#region Construction

		/// <summary>
		/// Constructs a test given its name
		/// </summary>
		/// <param name="name">The name of the test</param>
		protected Test( string name )
		{
			this.testName = new TestName();
			this.testName.FullName = name;
			this.testName.Name = name;
			this.testName.TestID = new TestID();

            this.runState = RunState.Runnable;
		}

		/// <summary>
		/// Constructs a test given the path through the
		/// test hierarchy to its parent and a name.
		/// </summary>
		/// <param name="pathName">The parent tests full name</param>
		/// <param name="name">The name of the test</param>
		protected Test( string pathName, string name ) 
		{ 
			this.testName = new TestName();
			this.testName.FullName = pathName == null || pathName == string.Empty 
				? name : pathName + "." + name;
			this.testName.Name = name;
			this.testName.TestID = new TestID();

            this.runState = RunState.Runnable;
		}

		/// <summary>
		/// Constructs a test given a TestName object
		/// </summary>
		/// <param name="testName">The TestName for this test</param>
		protected Test( TestName testName )
		{
			this.testName = testName;
			
			this.runState = RunState.Runnable;
		}

		/// <summary>
		/// Constructs a test given a fixture type
		/// </summary>
		/// <param name="fixtureType">The type to use in constructiong the test</param>
		protected Test( Type fixtureType )
		{
			this.testName = new TestName();
			this.testName.FullName = fixtureType.FullName;
			this.testName.Name = fixtureType.Namespace != null
				? TestName.FullName.Substring( TestName.FullName.LastIndexOf( '.' ) + 1 )
				: fixtureType.FullName;
			this.testName.TestID = new TestID();

			this.fixtureType = fixtureType;
			this.RunState = RunState.Runnable;
		}

		/// <summary>
		/// Construct a test given a MethodInfo
		/// </summary>
		/// <param name="method">The method to be used</param>
		protected Test( MethodInfo method )
			: this( method.ReflectedType )
		{
			this.testName.Name = method.DeclaringType == method.ReflectedType 
				? method.Name : method.DeclaringType.Name + "." + method.Name;
			this.testName.FullName = method.ReflectedType.FullName + "." + method.Name;
		}

		/// <summary>
		/// Sets the runner id of a test and optionally its children
		/// </summary>
		/// <param name="runnerID">The runner id to be used</param>
		/// <param name="recursive">True if all children should get the same id</param>
		public void SetRunnerID( int runnerID, bool recursive )
		{
			this.testName.RunnerID = runnerID;

			if ( recursive && this.Tests != null )
				foreach( Test child in this.Tests )
					child.SetRunnerID( runnerID, true );
		}

		#endregion

		#region ITest Members

        #region Properties
		/// <summary>
		/// Gets the TestName of the test
		/// </summary>
        public TestName TestName
		{
			get { return testName; }
		}

		/// <summary>
		/// Gets a string representing the kind of test
		/// that this object represents, for use in display.
		/// </summary>
		public abstract string TestType { get; }

		/// <summary>
		/// Whether or not the test should be run
		/// </summary>
        public RunState RunState
        {
            get { return runState; }
            set { runState = value; }
        }

		/// <summary>
		/// Reason for not running the test, if applicable
		/// </summary>
		public string IgnoreReason
		{
			get { return ignoreReason; }
			set { ignoreReason = value; }
		}

		/// <summary>
		/// Gets a count of test cases represented by
		/// or contained under this test.
		/// </summary>
		public virtual int TestCount 
		{ 
			get { return 1; } 
		}

		/// <summary>
		/// Gets a list of categories associated with this test.
		/// </summary>
		public IList Categories 
		{
			get { return categories; }
			set { categories = value; }
		}

		/// <summary>
		/// Gets a desctiption associated with this test.
		/// </summary>
		public String Description
		{
			get { return description; }
			set { description = value; }
		}

		/// <summary>
		/// Gets the property dictionary for this test
		/// </summary>
		public IDictionary Properties
		{
			get 
			{
				if ( properties == null )
					properties = new ListDictionary();

				return properties; 
			}
			set
			{
				properties = value;
			}
		}

		/// <summary>
		/// Indicates whether this test is a suite
		/// </summary>
		public abstract bool IsSuite { get; }

		/// <summary>
		/// Gets the parent test of this test
		/// </summary>
		ITest ITest.Parent 
		{
			get { return parent; }
		}

		/// <summary>
		/// Gets the parent as a Test object.
		/// Used by the core to set the parent.
		/// </summary>
		public Test Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Gets this test's child tests
		/// </summary>
		public abstract IList Tests { get; }

		/// <summary>
		/// Gets the Type of the fixture used in running this test
		/// </summary>
		public Type FixtureType
		{
			get { return fixtureType; }
		}

		/// <summary>
		/// Gets or sets a fixture object for running this test
		/// </summary>
		public  object Fixture
		{
			get { return fixture; }
			set { fixture = value; }
        }
        #endregion

        #region Methods
		/// <summary>
		/// Gets a count of test cases that would be run using
		/// the specified filter.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
        public abstract int CountTestCases(ITestFilter filter);
        #endregion

        #endregion

		#region Abstract Run Methods
		/// <summary>
		/// Runs the test, sending notifications to a listener.
		/// </summary>
		/// <param name="listener">An event listener to receive notifications</param>
		/// <returns>A TestResult</returns>
		public abstract TestResult Run(EventListener listener);

		/// <summary>
		/// Runs the test under a particular filter, sending
		/// notifications to a listener.
		/// </summary>
		/// <param name="listener">An event listener to receive notifications</param>
		/// <param name="filter">A filter used in running the test</param>
		/// <returns></returns>
        public abstract TestResult Run(EventListener listener, ITestFilter filter);
		#endregion
		
		#region IComparable Members
		/// <summary>
		/// Compares this test to another test for sorting purposes
		/// </summary>
		/// <param name="obj">The other test</param>
		/// <returns>Value of -1, 0 or +1 depending on whether the current test is less than, equal to or greater than the other test</returns>
		public int CompareTo(object obj)
		{
			Test other = obj as Test;
			
			if ( other == null )
				return -1;

			return this.TestName.FullName.CompareTo( other.TestName.FullName );
		}
		#endregion
	}
}
