namespace NUnit.Runner {

  using System;
  using System.Collections;
  using System.Collections.Specialized;
  using System.IO;
  using System.IO.IsolatedStorage;
  using System.Reflection;

  using NUnit.Framework;

  /// <summary>Base class for all test runners.</summary>
  /// <remarks>This class was born live on stage in Sardinia during
  /// XP2000.</remarks>
  public abstract class BaseTestRunner: ITestListener {
	  /// <summary>
	  /// 
	  /// </summary>
    public static string SUITE_PROPERTYNAME="Suite";

    static NameValueCollection fPreferences = new NameValueCollection();
	static int fgMaxMessageLength = 500;
    static bool fgFilterStack = true;
    bool fLoading = true;
	/// <summary>
	/// 
	/// </summary>
    public BaseTestRunner() {
      fPreferences = new NameValueCollection();
      fPreferences.Add("loading", "true");
      fPreferences.Add("filterstack", "true");
      ReadPreferences();
      fgMaxMessageLength = GetPreference("maxmessage", fgMaxMessageLength);
    }
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="test"></param>
	  /// <param name="t"></param>
    public abstract void AddError(ITest test, Exception t);
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="test"></param>
	  /// <param name="t"></param>
    public abstract void AddFailure(ITest test, AssertionFailedError t);
	/// <summary>
	/// Clears the status message.
	/// </summary>
    protected virtual void ClearStatus() { // Belongs in the GUI TestRunner class.
    }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="test"></param>
    public abstract void EndTest(ITest test);
	/// <summary>
	/// Returns the formatted string of the elapsed time.
	/// </summary>
    public static string ElapsedTimeAsString(long runTime) {
      return ((double)runTime/1000).ToString();
    }
    /// <summary>
    /// Extract the class name from a string in VA/Java style
    /// </summary>
    public static string ExtractClassName(string className) {
      if(className.StartsWith("Default package for")) 
        return className.Substring(className.LastIndexOf(".")+1);
      return className;
    }
    static bool FilterLine(string line) {
      string[] patterns = new string[] {
        "NUnit.Framework.TestCase",
        "NUnit.Framework.TestResult",
        "NUnit.Framework.TestSuite",
        "NUnit.Framework.Assertion." // don't filter AssertionFailure
      };
      for (int i = 0; i < patterns.Length; i++) {
        if (line.IndexOf(patterns[i]) > 0)
          return true;
      }
      return false;
    }
    /// <summary>
    /// Filters stack frames from internal NUnit classes
    /// </summary>
    public static string FilterStack(string stack) {
      string pref = GetPreference("filterstack");
      if (((pref != null) && !GetPreference("filterstack").Equals("true")) || fgFilterStack == false)
        return stack;

      StringWriter sw = new StringWriter();
      StringReader sr = new StringReader(stack);

      string line;
      try {
        while ((line = sr.ReadLine()) != null) {
          if (!FilterLine(line))
            sw.WriteLine(line);
        }
      } catch (Exception) {
        return stack; // return the stack unfiltered
      }
      return sw.ToString();
    }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
    public static string GetFilteredTrace(Exception t) {
      return BaseTestRunner.FilterStack(t.StackTrace);
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
    public static string GetPreference(string key) {
      return fPreferences.Get(key);
    }
    private static int GetPreference(String key, int dflt) 
    {
      String value= GetPreference(key);
      int intValue= dflt;
      if (value == null)
        return intValue;
      try {
        intValue= int.Parse(value);
      } 
      catch (FormatException) {
      }
      return intValue;
    } 
    private static FileStream GetPreferencesFile() {
      return new IsolatedStorageFileStream("NUnit.Prefs",
        FileMode.OpenOrCreate);
    }
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
    public virtual ITestSuiteLoader GetLoader() {
      if (UseReloadingTestSuiteLoader())
        return new ReloadingTestSuiteLoader();
      return new StandardTestSuiteLoader();
    }

    /// <summary>
    /// Returns the ITest corresponding to the given suite. This is
    /// a template method, subclasses override RunFailed(), ClearStatus().
    /// </summary>
    public ITest GetTest(string suiteClassName) {
      if (suiteClassName.Length <= 0) {
        ClearStatus();
        return null;
      }
      Type testClass= null;
      try {
        testClass = LoadSuiteClass(suiteClassName);
      } catch (TypeLoadException e) {
        RunFailed(e.Message);
        return null;
      } catch (Exception e) {
        RunFailed("Error: " + e.ToString());
        return null;
      }
      PropertyInfo suiteProperty= null;
      suiteProperty = testClass.GetProperty(SUITE_PROPERTYNAME, new Type[0]);
      if (suiteProperty == null ) {
        // try to extract a test suite automatically
        ClearStatus();
        return new TestSuite(testClass);
      }
      ITest test= null;
      try {
        // static property
        test= (ITest)suiteProperty.GetValue(null, new Type[0]); 
        if (test == null)
          return test;
      } catch(Exception e) {
        RunFailed("Could not get the Suite property. " + e);
        return null;
      }
      ClearStatus();
      return test;  
    }
  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
    public static bool InVAJava() {
      return false;
    }
    /// <summary>
    /// Returns the loaded Class for a suite name. 
    /// </summary>
    protected Type LoadSuiteClass(string suiteClassName) {
      return GetLoader().Load(suiteClassName);
    }
    private static void ReadPreferences() {
      FileStream fs= null;
      try {
        fs= GetPreferencesFile();
        fPreferences= new NameValueCollection(fPreferences);
        ReadPrefsFromFile(ref fPreferences, fs);
      } catch (IOException) {
        try {
          if (fs != null)
            fs.Close();
        } catch (IOException) {
        }
      }
    }
    private static void ReadPrefsFromFile(ref NameValueCollection prefs, FileStream fs) {
      // Real method reads name/value pairs, populates, or maybe just
      // deserializes...
    }
    /// <summary>
    /// Override to define how to handle a failed loading of a test suite.
    /// </summary>
    protected abstract void RunFailed(String message);
    /// <summary>
    /// Truncates a String to the maximum length.
    /// </summary>
    public static String Truncate(String s) {
        if (fgMaxMessageLength != -1 && s.Length > fgMaxMessageLength)
            s= s.Substring(0, fgMaxMessageLength)+"...";
      return s;
    }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="test"></param>
    public abstract void StartTest(ITest test);
	/// <summary>
	/// 
	/// </summary>
	/// <param name="args"></param>
	/// <param name="wait"></param>
	/// <returns></returns>
    protected string ProcessArguments(string[] args, ref bool wait) {
      string suiteName="";
      wait = false;
      foreach (string arg in args) {
        if (arg.Equals("/noloading"))
          SetLoading(false);
        else if (arg.Equals("/nofilterstack")) 
          fgFilterStack = false;
        else if (arg.Equals("/wait")) 
          wait = true;
        else if (arg.Equals("/c"))
          suiteName= ExtractClassName(arg);
        else if (arg.Equals("/v")){
          Console.Error.WriteLine("NUnit "+NUnit.Runner.Version.id()
            + " by Philip Craig");
          Console.Error.WriteLine("ported from JUnit 3.6 by Kent Beck"
            + " and Erich Gamma");
        } else
          suiteName = arg;
      }
      return suiteName;
    }
    /// <summary>
    /// Sets the loading behaviour of the test runner
    /// </summary>
    protected void SetLoading(bool enable) {
      fLoading = false;
    }
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
    protected bool UseReloadingTestSuiteLoader() {
      return GetPreference("loading").Equals("true") && fLoading;
    }
  }
}
