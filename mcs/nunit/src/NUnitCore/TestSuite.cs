namespace NUnit.Framework {

  using System;
  using System.Collections;
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
  public class TestSuite: MarshalByRefObject, ITest {

    private ArrayList fTests= new ArrayList(10);
    private string fName;
    private bool fHasWarnings = false;

    /// <summary>Constructs an empty TestSuite.</summary>
    public TestSuite() {
    }

    /// <summary>Constructs a TestSuite from the given class.</summary>
    /// <remarks>Adds all the methods
    /// starting with "Test" as test cases to the suite.
    /// Parts of this method was written at 2337 meters in the Hüffihütte,
    /// Kanton Uri</remarks>
    public TestSuite(Type theClass) {
      fName= theClass.Name;
      ConstructorInfo constructor = GetConstructor(theClass);
      if (constructor == null) {
        AddTest(Warning("Class "+theClass.Name+" has no public constructor TestCase(String name)"));
        return;
      }
      if (theClass.IsNotPublic) {
        AddTest(Warning("Class "+theClass.Name+" is not public"));
        return;
      }

      Type superClass= theClass;
      ArrayList names= new ArrayList();
      while (typeof(ITest).IsAssignableFrom(superClass)) {
        MethodInfo[] methods= superClass.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
        for (int i= 0; i < methods.Length; i++) {
          AddTestMethod(methods[i], names, constructor);
        }
        superClass= superClass.BaseType;
      }
      if (fTests.Count == 0)
        AddTest(Warning("No Tests found in "+theClass.ToString()));
    }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="theClass"></param>
	/// <param name="hasNonWarningTests"></param>
    public TestSuite(Type theClass, ref bool hasNonWarningTests) {
      hasNonWarningTests = false;
      fName= theClass.ToString();    
      ConstructorInfo constructor= GetConstructor(theClass);
      if (constructor == null) {
        return;
      }
      if (theClass.IsNotPublic) {
        return;
      }

      Type superClass= theClass;
      ArrayList names= new ArrayList();
      while (typeof(ITest).IsAssignableFrom(superClass)) {
        MethodInfo[] methods= superClass.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
        for (int i= 0; i < methods.Length; i++) {
          AddTestMethod(methods[i], names, constructor);
        }
        superClass= superClass.BaseType;
      }
      if (fHasWarnings) {
          hasNonWarningTests = false;
      } else {
        hasNonWarningTests = (fTests.Count != 0);
      }
    }

    /// <summary>Constructs an empty TestSuite.</summary>
    public TestSuite(string name) {
      fName= name;
    }

    /// <summary>Adds a test to the suite.</summary>
    public void AddTest(ITest test) {
      fTests.Add(test);
    }
        
    private void AddTestMethod(MethodInfo m, ArrayList names,
                               ConstructorInfo constructor) {
      string name= m.Name;
      if (names.Contains(name)) 
        return;
      if (IsPublicTestMethod(m)) {
        names.Add(name);

        Object[] args= new Object[]{name};
        try {
          AddTest((ITest)constructor.Invoke(args));
        } catch (TypeLoadException e) {
          AddTest(Warning("Cannot instantiate test case: "+name + "( " + e.ToString() + ")"));
        } catch (TargetInvocationException e) {
          AddTest(Warning("Exception in constructor: "+name + "( " + e.ToString() + ")"));
        } 
		catch (MemberAccessException e) 
	    {
          AddTest(Warning("Cannot access test case: "+name + "( " + e.ToString() + ")"));
        }

      } else { // almost a test method
        if (IsTestMethod(m)) 
          AddTest(Warning("test method isn't public: "+m.Name));
      }
    }

    /// <summary>Adds the tests from the given class to the suite</summary>
    public void AddTestSuite(Type testClass) {
      AddTest(new TestSuite(testClass));
    }

    /// <value>The number of test cases that will be run by this test.</value>
    public int CountTestCases {
      get {
        int count= 0;
        foreach (ITest test in Tests) {
          count += test.CountTestCases;
        }
        return count;
      }
    }

    /// <summary>Gets a constructor which takes a single string as
    /// its argument.</summary>
    private ConstructorInfo GetConstructor(Type theClass) {
      Type[] args= { typeof(string) };
      return theClass.GetConstructor(args);
    }

   /// <summary>
   /// Returns the name of the suite. Not all test suites have a name
   /// and this method can return null.
   /// </summary>
    public string Name {
      get { return fName; }
    }

    private bool IsPublicTestMethod(MethodInfo m) {
      return IsTestMethod(m) && m.IsPublic;
    }

    private bool IsTestMethod(MethodInfo m) {
      string name= m.Name;            

      ParameterInfo[] parameters= m.GetParameters();
      Type returnType= m.ReturnType;
      return parameters.Length == 0 && name.ToLower().StartsWith("test")
        && returnType.Equals(typeof(void));
    }
         
    /// <summary>Runs the Tests and collects their result in a
    /// TestResult.</summary>
    public virtual void Run(TestResult result) {
      foreach (ITest test in Tests) {
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
    public virtual void RunTest(ITest test, TestResult result) {
      test.Run(result);
    }
        
    /// <value>The test at the given index.</value>
    /// <remarks>Formerly TestAt(int).</remarks>
    public ITest this[int index] {
      get {return (ITest)fTests[index]; }
    }
        
    /// <value>The number of Tests in this suite.</value>
    public int TestCount {
      get {return fTests.Count; }
    }
        
    /// <value>The Tests as an ArrayList.</value>
    public ArrayList Tests {
      get { return fTests; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      if (Name != null)
        return Name;
      return base.ToString();
    }

    private ITest Warning(string message) {
      fHasWarnings = true;
      return new WarningFail(message);
    }    

    /// <summary>Returns a test which will fail and log a warning
    /// message.</summary>
    public class WarningFail: TestCase {
      private string fMessage;
      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      public WarningFail(string message): base("warning") {
        fMessage = message;
      }
	/// <summary>
	/// 
	/// </summary>
      protected override void RunTest() {
        Fail(fMessage);
      }         
    }
  }
}
