namespace NUnit.Runner {

  using System;
  using System.Reflection;

  using NUnit.Framework;

  /// <summary>
  /// An implementation of a TestCollector that loads
  /// all classes on the class path and tests whether
  /// it is assignable from ITest or provides a static Suite property.
  /// <see cref="ITestCollector"/>
  /// </summary>
  public class LoadingClassPathTestCollector: ClassPathTestCollector {
	
    TestCaseClassLoader fLoader;
	/// <summary>
	/// 
	/// </summary>
    public LoadingClassPathTestCollector() {
      fLoader= new TestCaseClassLoader();
    }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="classFileName"></param>
	/// <returns></returns>
    protected override bool IsTestClass(string classFileName) {	
      try {
        if (classFileName.EndsWith(".dll") || classFileName.EndsWith(".exe")) {
          Type testClass= ClassFromFile(classFileName);
          return (testClass != null) && IsTestClass(testClass);
        }
      } catch (TypeLoadException) {
      } 
      return false;
    }
	
    Type ClassFromFile(string classFileName) {
      string className= ClassNameFromFile(classFileName);
      if (!fLoader.IsExcluded(className))
        return fLoader.LoadClass(className, false);
      return null;
    }
	
    bool IsTestClass(Type testClass) {
      if (HasSuiteMethod(testClass))
        return true;
      if (typeof(ITest).IsAssignableFrom(testClass) &&
        testClass.IsPublic &&
        HasPublicConstructor(testClass)) 
        return true;
      return false;
    }
	
    bool HasSuiteMethod(Type testClass) {
      return (testClass.GetProperty(BaseTestRunner.SUITE_PROPERTYNAME, new Type[0]) == null);
    }
	
    bool HasPublicConstructor(Type testClass) {
      Type[] args= { typeof(string) };
      ConstructorInfo c= null;
      try {
        c= testClass.GetConstructor(args);
      } catch(Exception) {
        return false;
      }
      return true;
    }
  }
}