namespace NUnit.Runner {

    using System;
    using System.Reflection;
    using System.IO;
    using System.Security;

    /// <summary>The standard test suite loader. It can only load the same
    /// class once.</summary>
    public class StandardTestSuiteLoader: ITestSuiteLoader {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="suiteClassName"></param>
        /// <returns></returns>
        public Type Load(string suiteClassName) {
            Type testClass;
            string[] classSpec=suiteClassName.Split(',');
            if (classSpec.Length > 1) {
                FileInfo dll=new FileInfo(classSpec[1]);
                if (!dll.Exists) 
                    throw new FileNotFoundException("File " + dll.FullName + " not found", dll.FullName);
                Assembly a = Assembly.LoadFrom(dll.FullName);
                testClass=a.GetType(classSpec[0], true);
            }
            else
                testClass = Type.GetType(suiteClassName, true);
            return testClass;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        public Type Reload(Type aClass) {
            return aClass;
        }
    }
}