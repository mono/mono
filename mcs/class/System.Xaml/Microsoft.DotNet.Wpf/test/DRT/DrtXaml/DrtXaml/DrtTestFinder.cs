using System;
using System.Reflection;
using System.Collections.Generic;
using DRT;
using System.Linq;
using DrtXaml.XamlTestFramework;

namespace DrtXaml
{
    static class DrtTestFinder
    {
        public static XamlTestSuite[] TestSuites
        {
            get
            {
                List<XamlTestSuite> suites = new List<XamlTestSuite>();
                foreach (Assembly testAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type testClass in TestFinder.TestClasses(testAssembly))
                    {
                        if (!typeof(XamlTestSuite).IsAssignableFrom(testClass))
                        {
                            string err = String.Format("Test Class {0} is not derived from XamlTestSuite", testClass);
                            throw new Exception(err);
                        }
                        XamlTestSuite suite = Activator.CreateInstance(testClass) as XamlTestSuite;
                        suites.Add(suite);
                    }
                }
                return suites.ToArray<XamlTestSuite>();
            }
        }

        public static DrtTest DRT_MakeTest(XamlTestSuite suiteInstance, XamlTestInfoBlock testBlk)
        {
            DrtTest drtTest = null;
            string name = testBlk.Name;

            if (testBlk.TestDelegate != null)
            {
                SimpleTest test = testBlk.TestDelegate;

                drtTest = new DrtTest(() => suiteInstance.DRT_TestValidator(
                                                        test,            
                                                        testBlk.ExpectedExceptionType));
            }
            else
            {
                string xamlString = testBlk.XamlString;
                XamlStringParser loader = testBlk.StringParserDelegate;
                PostTreeValidator validator = testBlk.TreeValidatorDelegate;
                Type expectedExceptionType = testBlk.ExpectedExceptionType;

                drtTest = new DrtTest(() => suiteInstance.DRT_XamlLoader(
                                                        name,
                                                        xamlString,
                                                        loader,
                                                        expectedExceptionType,
                                                        validator));
            }
            return drtTest;
        }

        public static DrtTest[] FindTests(XamlTestSuite suiteInstance)
        {
            List<DrtTest> drts = new List<DrtTest>();
            foreach (XamlTestInfoBlock testBlk in TestFinder.XamlTestBlocks(suiteInstance, true))
            {
                DrtTest drtTest = DRT_MakeTest(suiteInstance, testBlk);
                drts.Add(drtTest);
            }
            return drts.ToArray<DrtTest>();
        }

    }
}
