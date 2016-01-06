// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Internal.WorkItems;

namespace NUnit.Framework.Internal
{
	/// <summary>
	/// The Test abstract class represents a test within the framework.
	/// </summary>
	public abstract class Test : ITest, IComparable
    {
        #region Fields

        /// <summary>
        /// Static value to seed ids. It's started at 1000 so any
        /// uninitialized ids will stand out.
        /// </summary>
        private static int nextID = 1000;

        private int id;
        private string name;
        private string fullName;
        private int seed;

        /// <summary>
		/// Indicates whether the test should be executed
		/// </summary>
		private RunState runState;

		/// <summary>
		/// Test suite containing this test, or null
		/// </summary>
		private ITest parent;
		
		/// <summary>
		/// A dictionary of properties, used to add information
		/// to tests without requiring the class to change.
		/// </summary>
		private PropertyBag properties;

        /// <summary>
        /// The System.Type of the fixture for this test, if there is one
        /// </summary>
        private Type fixtureType;

        /// <summary>
        /// The fixture object, if it has been created
        /// </summary>
        private object fixture;

        /// <summary>
        /// The SetUp methods.
        /// </summary>
        protected MethodInfo[] setUpMethods;

        /// <summary>
        /// The teardown methods
        /// </summary>
        protected MethodInfo[] tearDownMethods;

        #endregion

        #region Construction

        /// <summary>
		/// Constructs a test given its name
		/// </summary>
		/// <param name="name">The name of the test</param>
		protected Test( string name )
		{
			this.fullName = name;
			this.name = name;
            this.id = unchecked(nextID++);

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
			this.fullName = pathName == null || pathName == string.Empty 
				? name : pathName + "." + name;
			this.name = name;
            this.id = unchecked(nextID++);

            this.runState = RunState.Runnable;
		}

        /// <summary>
        ///  TODO: Documentation needed for constructor
        /// </summary>
        /// <param name="fixtureType"></param>
        protected Test(Type fixtureType) : this(fixtureType.FullName)
        {
            this.fixtureType = fixtureType;
        }

		#endregion

		#region ITest Members

        /// <summary>
        /// Gets or sets the id of the test
        /// </summary>
        /// <value></value>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets or sets the name of the test
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the fully qualified name of the test
        /// </summary>
        /// <value></value>
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        /// <summary>
        /// Gets the Type of the fixture used in running this test
        /// or null if no fixture type is associated with it.
        /// </summary>
        public Type FixtureType
        {
            get { return fixtureType; }
        }

        /// <summary>
		/// Whether or not the test should be run
		/// </summary>
        public RunState RunState
        {
            get { return runState; }
            set { runState = value; }
        }

        /// <summary>
        /// Gets the name used for the top-level element in the
        /// XML representation of this test
        /// </summary>
        public abstract string XmlElementName
        {
            get;
        }

        /// <summary>
        /// Gets a string representing the type of test. Used as an attribute
        /// value in the XML representation of a test and has no other
        /// function in the framework.
        /// </summary>
        public virtual string TestType
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
		/// Gets a count of test cases represented by
		/// or contained under this test.
		/// </summary>
		public virtual int TestCaseCount 
		{ 
			get { return 1; } 
		}

		/// <summary>
		/// Gets the properties for this test
		/// </summary>
		public IPropertyBag Properties
		{
			get 
			{
				if ( properties == null )
					properties = new PropertyBag();

				return properties; 
			}
		}

        /// <summary>
        /// Returns true if this is a TestSuite
        /// </summary>
        public bool IsSuite
        {
            get { return this is TestSuite; }
        }

        /// <summary>
        /// Gets a bool indicating whether the current test
        /// has any descendant tests.
        /// </summary>
        public abstract bool HasChildren { get; }

        /// <summary>
        /// Gets the parent as a Test object.
        /// Used by the core to set the parent.
        /// </summary>
        public ITest Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets or Sets the Int value representing the seed for the RandomGenerator
        /// </summary>
        /// <value></value>
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        /// <summary>
        /// Gets this test's child tests
        /// </summary>
        /// <value>A list of child tests</value>
        
#if CLR_2_0 || CLR_4_0
        public abstract System.Collections.Generic.IList<ITest> Tests { get; }
#else
        public abstract System.Collections.IList Tests { get; }
#endif

        #endregion

        #region IXmlNodeBuilder Members

        /// <summary>
        /// Returns the Xml representation of the test
        /// </summary>
        /// <param name="recursive">If true, include child tests recursively</param>
        /// <returns></returns>
        public XmlNode ToXml(bool recursive)
        {
            XmlNode topNode = XmlNode.CreateTopLevelElement("dummy");

            XmlNode thisNode = AddToXml(topNode, recursive);

            return thisNode;
        }

        /// <summary>
        /// Returns an XmlNode representing the current result after
        /// adding it as a child of the supplied parent node.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="recursive">If true, descendant results are included</param>
        /// <returns></returns>
        public abstract XmlNode AddToXml(XmlNode parentNode, bool recursive);

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

            if (other == null)
                return -1;

            return this.FullName.CompareTo(other.FullName);
        }

        #endregion

        #region Other Public Methods

        /// <summary>
        /// Creates a TestResult for this test.
        /// </summary>
        /// <returns>A TestResult suitable for this type of test.</returns>
        public abstract TestResult MakeTestResult();

        /// <summary>
        /// Creates a WorkItem for executing this test.
        /// </summary>
        /// <param name="childFilter">A filter to be used in selecting child tests</param>
        /// <returns>A new WorkItem</returns>
        public abstract WorkItem CreateWorkItem(ITestFilter childFilter);

        /// <summary>
        /// Modify a newly constructed test by applying any of NUnit's common
        /// attributes, based on a supplied ICustomAttributeProvider, which is
        /// usually the reflection element from which the test was constructed,
        /// but may not be in some instances. The attributes retrieved are 
        /// saved for use in subsequent operations.
        /// </summary>
        /// <param name="provider">An object implementing ICustomAttributeProvider</param>
        public void ApplyAttributesToTest(ICustomAttributeProvider provider)
        {
            foreach (IApplyToTest iApply in provider.GetCustomAttributes(typeof(IApplyToTest), true))
                iApply.ApplyToTest(this);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Add standard attributes and members to a test node.
        /// </summary>
        /// <param name="thisNode"></param>
        /// <param name="recursive"></param>
        protected void PopulateTestNode(XmlNode thisNode, bool recursive)
        {
            thisNode.AddAttribute("id", this.Id.ToString());
            thisNode.AddAttribute("name", this.Name);
            thisNode.AddAttribute("fullname", this.FullName);

            if (Properties.Count > 0)
                Properties.AddToXml(thisNode, recursive);
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets or sets a fixture object for running this test.
        /// Provided for use by LegacySuiteBuilder.
        /// </summary>
        public object Fixture
        {
            get { return fixture; }
            set { fixture = value; }
        }

        /// <summary>
        /// Gets the set up methods.
        /// </summary>
        /// <returns></returns>
        internal virtual MethodInfo[] SetUpMethods
        {
            get
            {
                if (setUpMethods == null && this.Parent != null)
                {
                    TestSuite suite = this.Parent as TestSuite;
                    if (suite != null)
                        setUpMethods = suite.SetUpMethods;
                }

                return setUpMethods;
            }
        }

        /// <summary>
        /// Gets the tear down methods.
        /// </summary>
        /// <returns></returns>
        internal virtual MethodInfo[] TearDownMethods
        {
            get
            {
                if (tearDownMethods == null && this.Parent != null)
                {
                    TestSuite suite = this.Parent as TestSuite;
                    if (suite != null)
                        tearDownMethods = suite.TearDownMethods;
                }

                return tearDownMethods;
            }
        }

        #endregion
    }
}
