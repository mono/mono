namespace NUnit.Framework 
{
	using System;
	using System.Reflection;

	/// <summary>A test case defines the fixture to run multiple Tests.</summary>
	/// <remarks> To define a test case
	/// <list type="number">
	/// <item><description>implement a subclass of TestCase</description></item>
	/// <item><description>define instance variables that store the state
	/// of the fixture</description></item>
	/// <item><description>initialize the fixture state by overriding
	/// <c>SetUp</c></description></item>
	/// <item><description>clean-up after a test by overriding
	/// <c>TearDown</c></description></item>
	/// </list>
	/// Each test runs in its own fixture so there can be no side effects 
	/// among test runs.
	/// <example>Here is an example:
	/// <code>
	/// public class MathTest: TestCase {
	///   protected double fValue1;
	///   protected double fValue2;
	///
	///   public MathTest(string name) : base(name) {}
	///
	///   protected override void SetUp() {
	///     fValue1= 2.0;
	///     fValue2= 3.0;
	///   }
	/// }</code>
	/// </example>
	///
	/// For each test implement a method which interacts with the fixture.
	/// Verify the expected results with Assertions specified by calling 
	/// <c>Assert</c> with a bool.
	/// <code>
	///    protected void TestAdd() {
	///        double result= fValue1 + fValue2;
	///        Assert(result == 5.0);
	///    }
	/// </code>
	/// Once the methods are defined you can run them. The framework supports
	/// both a static type safe and more dynamic way to run a test.
	/// In the static way you override the runTest method and define the method
	/// to be invoked.
	/// <code>
	/// protected class AddMathTest: TestCase {
	///   public void AddMathTest(string name) : base(name) {}
	///   protected override void RunTest() { TestAdd(); }
	/// }
	///
	/// test test= new AddMathTest("Add");
	/// test.Run();
	/// </code>
	/// The dynamic way uses reflection to implement <c>RunTest</c>. It
	/// dynamically finds and invokes a method. In this case the name of the
	/// test case has to correspond to the test method to be run.
	/// <code>
	/// test= new MathTest("TestAdd");
	/// test.Run();
	/// </code>
	/// The Tests to be run can be collected into a <see cref="TestSuite"/>.
	/// NUnit provides different test runners which can run a test suite
	/// and collect the results.
	/// A test runner either expects a static property <c>Suite</c> as the entry
	/// point to get a test to run or it will extract the suite automatically.
	/// <code>
	/// public static ITest Suite {
	///    get {
	///      suite.AddTest(new MathTest("TestAdd"));
	///      suite.AddTest(new MathTest("TestDivideByZero"));
	///      return suite;
	///    }
	///  }
	/// </code></remarks>
	/// <seealso cref="TestResult"/>
	/// <seealso cref="TestSuite"/>
	public abstract class TestCase: Assertion, ITest
	{
		#region Instance Variables
		/// <summary>the name of the test case.</summary>
		private readonly string fName;
		#endregion

		#region Constructors
		/// <summary>Constructs a test case with no name.</summary>
		public TestCase() : this(String.Empty){}
		
		/// <summary>Constructs a test case with the given name.</summary>
		public TestCase(string testName) 
		{
			this.fName = testName;
		}
		#endregion

		#region Properties
		/// <value>Counts the number of test cases executed by
		/// Run(TestResult result).</value>
		public int CountTestCases 
		{
			get { return 1; }
		}

		/// <value>The name of the test case.</value>
		public string Name 
		{
			get { return this.fName; }
		}
		#endregion

		#region Utility Methods
		/// <summary>Creates a default <see cref="TestResult"/> object.</summary>
		protected TestResult CreateResult() 
		{
			return new TestResult();
		}
		#endregion

		#region Run Methods
		/// <summary>A convenience method to run this test, collecting the
		/// results with a default <see cref="TestResult"/> object.</summary>
		public TestResult Run() 
		{
			TestResult result = this.CreateResult();
			this.Run(result);
			return result;
		}

		/// <summary>Runs the test case and collects the results in
		/// TestResult.</summary>
		public void Run(TestResult result) 
		{
			result.Run(this);
		}

		/// <summary>Runs the bare test sequence.</summary>
		public void RunBare() 
		{
			this.SetUp();
			try 
			{
				this.RunTest();
			}
			finally 
			{
				this.TearDown();
			}
		}

		/// <summary>Override to run the test and Assert its state.</summary>
		protected virtual void RunTest() 
		{
			MethodInfo runMethod = GetType().GetMethod(this.Name, new Type[0]);
			if (runMethod == null)
				Assertion.Fail("Method \""+this.Name+"\" not found");
          
			if (runMethod != null && !runMethod.IsPublic) 
			{
				Assertion.Fail("Method \""+this.Name+"\" should be public");
			}

			object[] exa = 
				runMethod.GetCustomAttributes(typeof(ExpectExceptionAttribute),true);

			try 
			{
				runMethod.Invoke(this, null);
			}
			catch (AssertionFailedError e) 
			{
				throw new NUnitException("Run Error: ", e);
			}
			catch (TargetInvocationException e) 
			{
				Exception inner = e.InnerException;
				if (inner is AssertionFailedError) 
				{
					throw new NUnitException("Run Error: ", inner);
				}
				if (exa.Length>0)
				{
					foreach (ExpectExceptionAttribute ex in exa) 
					{
						if (ex.ExceptionExpected.IsAssignableFrom(inner.GetType()))
							return;
					}
					Assertion.Fail("Unexpected Exception thrown: " + inner);
				} 
				else
				{
					throw new NUnitException("Run Error: ", inner);
				}
			}
			catch (MemberAccessException e) 
			{
				throw new NUnitException("", e);
			}

			if (exa.Length > 0) 
			{
				System.Text.StringBuilder sb =
					new System.Text.StringBuilder
					("One of these exceptions should have been thrown: ");
				bool first = true;
				foreach (ExpectExceptionAttribute ex in exa) 
				{
					if(first)
						first = false;
					else
						sb.Append(", ");
					sb.Append(ex);
				}
				Assertion.Fail(sb.ToString());
			}
		}

		/// <summary>
		/// Sets up the fixture, for example, open a network connection.
		/// This method is called before a test is executed.
		/// </summary>
		protected virtual void SetUp() {}
		/// <summary>
		/// Tears down the fixture, for example, close a network
		/// connection. This method is called after a test is executed.
		/// </summary>
		protected virtual void TearDown() {}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns a string representation of the test case.
		/// </summary>
		public override string ToString() 
		{
			return this.Name+"("+this.GetType().ToString()+")";
		}
		#endregion

	}
}