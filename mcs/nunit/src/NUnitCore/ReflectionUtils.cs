namespace NUnit.Runner {

  using System;
  using System.Collections;
  using System.Collections.Specialized;
  using System.IO;
  using System.Reflection;

  using NUnit.Framework;
/// <summary>
/// 
/// </summary>
  public class ReflectionUtils{
/// <summary>
/// 
/// </summary>
/// <param name="testClass"></param>
/// <returns></returns>
    public static bool HasTests(Type testClass) {

      PropertyInfo suiteProperty= null;
      suiteProperty = testClass.GetProperty("Suite", new Type[0]);
      if (suiteProperty == null ) {
        // try to extract a test suite automatically
        bool result = false;
        TestSuite dummy = new TestSuite(testClass, ref result);
        return result;
      }
      ITest test= null;
      try {
        // static property
        test= (ITest)suiteProperty.GetValue(null, new Type[0]); 
        if (test == null)
          return false;
      } catch(Exception) {
        return false;
      }
      return true;  
    }
/// <summary>
/// 
/// </summary>
/// <param name="assemblyName"></param>
/// <returns></returns>
    public static StringCollection GetAssemblyClasses(string assemblyName){
      StringCollection classNames = new StringCollection ();
      try {
        Assembly testAssembly = Assembly.LoadFrom(assemblyName);

        foreach(Type testType in testAssembly.GetExportedTypes()){
          if(testType.IsClass && HasTests(testType)){
            classNames.Add(testType.FullName);
          }
        }
      }catch(ReflectionTypeLoadException rcle){

        Type[] loadedTypes     = rcle.Types;
        Exception[] exceptions = rcle.LoaderExceptions;

        int exceptionCount = 0;

        for ( int i =0; i < loadedTypes.Length; i++ ){
          Console.Error.WriteLine("Unable to load a type because {0}", exceptions[exceptionCount] );
          exceptionCount++;        
        }
      }catch(FileNotFoundException fnfe){
        Console.Error.WriteLine(fnfe.Message);
      }catch(Exception e){
        Console.Error.WriteLine("Error reading file {0}: {1}", assemblyName, e.Message);
      }
      return classNames;
    }
  }
}
