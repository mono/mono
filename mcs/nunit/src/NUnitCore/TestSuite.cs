namespace NUnit.Framework 
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Reflection;

	/// <summary>A <c>TestSuite</c> is a <c>Composite</c> of Tests.</summary>
	/// <remarks>It runs a collection of test cases. Here is an example using
	/// the dynamic test definition.
	/// <code>
	/// TestSuite suite= new TestSuite();
	/// suite.AddTest(new MathTest("TestAdd"));
	/// suite.AddTest(new MathTest("TestDivideByZero"));
	/// </code>
	/// Alternatively, a TestSuite can extract the Tests to be run automatically.
	/// To do so you pass the class of your TestCase class to the
	/// TestSuite constructor.
	/// <code>
	/// TestSuite suite= new TestSuite(typeof(MathTest));
	/// </code>
	/// This constructor creates a suite with all the methods
	/// starting with "Test" that take no arguments.</remarks>
	/// <seealso cref="ITest"/>
	public class TestSuite: MarshalByRefObject, ITest
	{
		#region Instance Variables
		private ArrayList fTests= new ArrayList(10);
		private string fName;
		private bool fSupressWarnings = false;
		private string fDynamicConstructionQualifiedName= string.Empty;

		#endregion

		#region Constructors
		/// <summary>Constructs an empty TestSuite with a name.</summary>
		public TestSuite(string name) 
		{
			if(name == null)
				this.fName = String.Empty;
			else
				this.fName = name;
		}

		/// <summary>Constructs an empty TestSuite with no name.</summary>
		public TestSuite() : this(String.Empty){}

		/// <summary>Constructs a TestSuite from the given class.</summary>
		/// <remarks>Adds all the methods starting with "Test" as test cases 
		/// to the suite. Parts of this method was written at 2337 meters in 
		/// the Hüffihütte, Kanton Uri</remarks>
		/// <param name="theClass"></param>
		/// <param name="supressWarnings"></param>
		public TestSuite(Type theClass, bool supressWarnings) : 
			this(theClass.FullName) 
		{
			this.fSupressWarnings = supressWarnings;
			//REFACTOR: these checks are also found in AssemblyTestCollector
			if(    theClass.IsClass
				&& (theClass.IsPublic || theClass.IsNestedPublic)
				&& !theClass.IsAbstract
				&& typeof(ITest).IsAssignableFrom(theClass)
				)
			{
				ConstructorInfo FixtureConstructor = GetConstructor(theClass);
				if(FixtureConstructor != null) 
				{
					{
						MethodInfo[] methods = theClass.GetMethods(
							BindingFlags.Public
							|BindingFlags.NonPublic
							|BindingFlags.Instance
							);
						foreach (MethodInfo method in methods) 
						{
							AddTestMethod(method, FixtureConstructor);
						}
						if (this.fTests.Count == 0)
							AddWarning("No Tests found in "+theClass.ToString());
					}
				}
				else
				{
					AddWarning("Class "+theClass.Name
						+" has no public constructor TestCase(String name)");
				}
			}
			else
			{
				AddWarning("Type '" + theClass.Name
					+"' must be a public, not abstract class that"
					+" implements ITest.");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="theClass"></param>
		public TestSuite(Type theClass) : this(theClass,false){}
		#endregion

		#region Collection Methods
		/// <summary>Adds a test to the suite.</summary>
		public void AddTest(ITest test) 
		{
			fTests.Add(test);
		}
        
		/// <summary>Adds the tests from the given class to the suite</summary>
		public void AddTestSuite(Type testClass) 
		{
			AddTest(new TestSuite(testClass));
		}
		#endregion

		#region Dynamic Test Case Creation
		//private void AddTestMethod(MethodInfo m, StringCollection names, 
		private void AddTestMethod(MethodInfo m,
			ConstructorInfo constructor) 
		{
			string name = m.Name;
			if (IsPublicTestMethod(m)) 
			{
				Object[] args= new Object[]{name};
				try 
				{
					AddTest((ITest)constructor.Invoke(args));
				} 
				catch (TypeLoadException e) 
				{
					AddWarning("Cannot instantiate test case: "+name + "( " + e.ToString() + ")");
				} 
				catch (TargetInvocationException e) 
				{
					AddWarning("Exception in constructor: "+name + "( " + e.ToString() + ")");
				} 
				catch (MemberAccessException e) 
				{
					AddWarning("Cannot access test case: "+name + "( " + e.ToString() + ")");
				}
			} 
			else 
			{ // almost a test method
				if (IsTestMethod(m)) 
					AddWarning("test method isn't public: "+m.Name);
			}
		}

		/// <summary>Gets a constructor which takes a single string as
		/// its argument.</summary>
		private ConstructorInfo GetConstructor(Type theClass) 
		{
			//REFACTOR: these checks are also found in AssemblyTestCollector
			return theClass.GetConstructor(new Type[]{typeof(string)});
		}

		private bool IsPublicTestMethod(MethodInfo methodToCheck) 
		{
			return methodToCheck.IsPublic
				&& IsTestMethod(methodToCheck);
		}

		private bool IsTestMethod(MethodInfo methodToCheck) 
		{
			return
				!methodToCheck.IsAbstract
				&& methodToCheck.GetParameters().Length == 0
				&& methodToCheck.ReturnType.Equals(typeof(void))
				&& methodToCheck.Name.ToLower().StartsWith("test")
				;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Returns the name of the suite. Not all test suites have a name
		/// and this method can return null.
		/// </summary>
		public string Name 
		{
			get { return this.fName; }
		}

		/// <summary>
		/// The number of test cases that will be run by this test.
		/// </summary>
		public int CountTestCases 
		{
			get
			{
				int count= 0;
				foreach (ITest test in this.Tests) 
				{
					count += test.CountTestCases;
				}
				return count;
			}
		}

		/// <value>The number of Tests in this suite.</value>
		public int TestCount 
		{
			get {return this.fTests.Count; }
		}

		/// <value>The test at the given index.</value>
		/// <remarks>Formerly TestAt(int).</remarks>
		public ITest this[int index] 
		{
			get {return (ITest)this.fTests[index]; }
		}
        
		/// <value>The Tests as a Test[].</value>
		public ITest[] Tests 
		{
			get {
				ITest[] ret = new ITest[this.fTests.Count];
				this.fTests.CopyTo(ret);
				return ret;
			}
		}
		#endregion

		#region Utility Methods
		private void AddWarning(string message) 
		{
			if(!this.fSupressWarnings)
				AddTest(new WarningFail(message));
		}    
		#endregion

		#region Run Methods
		/// <summary>Runs the Tests and collects their result in a
		/// TestResult.</summary>
		public virtual void Run(TestResult result) 
		{
			foreach (ITest test in Tests) 
			{
				if (result.ShouldStop )
					break;
				RunTest(test, result);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		/// <param name="result"></param>
		public virtual void RunTest(ITest test, TestResult result) 
		{
			test.Run(result);
		}
        
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		#endregion

		#region Overrides
		public override string ToString() 
		{
			return this.Name;
		}
		#endregion

		#region Nested Classes
		/// <summary>A test which will fail and log a warning
		/// message.</summary>
		public class WarningFail : TestCase 
		{
			private string fMessage;
			
			/// <summary>
			/// 
			/// </summary>
			/// <param name="message"></param>
			public WarningFail(string message): base("warning") 
			{
				this.fMessage = message;
			}

			/// <summary>
			/// 
			/// </summary>
			protected override void RunTest() 
			{
				Assertion.Fail(fMessage);
			}         
		}
		#endregion
	}
}
