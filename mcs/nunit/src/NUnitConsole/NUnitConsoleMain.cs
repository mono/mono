namespace NUnit {

  using System;
  using System.Collections;
  
  using NUnit.Framework;
  using NUnit.Runner;
  using NUnit.TextUI;
  /// <summary>
  /// 
  /// </summary>
  public class Top {
	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="args"></param>
    public static void Main(string[] args) {
      TestRunner aTestRunner = new NUnit.TextUI.TestRunner();
      try {
        TestResult r = aTestRunner.Start(args);
        if (!r.WasSuccessful) 
          Environment.Exit(1);
        Environment.Exit(0);
      } catch(Exception e) {
        Console.Error.WriteLine(e.Message);
        Environment.Exit(2);
      }
    }
  }
}
