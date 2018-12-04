using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DrtXaml.XamlTestFramework
{
    public static class TestFinder
    {
        public static IEnumerable<Type> TestClasses(Assembly testAssembly)
        {
            Type[] types = null;
            try
            {
                types = testAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach(Type testClass in types.Where(t => t != null))
            {
                if (IsATestType(testClass))
                {
                    yield return testClass;
                }
            }
        }

        const BindingFlags methodBF = BindingFlags.Instance | BindingFlags.Static
                                    | BindingFlags.NonPublic | BindingFlags.Public;

        const BindingFlags fieldBF = BindingFlags.Instance | BindingFlags.Static
                                   | BindingFlags.NonPublic | BindingFlags.Public;

        public static IEnumerable<XamlTestInfoBlock> XamlTestBlocks(object suiteInstance, bool excludeKnownFailures)
        {
            foreach (XamlTestInfoBlock testBlocks in TestMethods(suiteInstance, excludeKnownFailures))
            {
                yield return testBlocks;
            }
            foreach (XamlTestInfoBlock testBlocks in TestStrings(suiteInstance, excludeKnownFailures))
            {
                yield return testBlocks;
            }
        }

        public static IEnumerable<XamlTestInfoBlock> TestMethods(object suiteInstance, bool excludeKnownFailures)
        {
            Type testClass = suiteInstance.GetType();

            SimpleTest setup = null;
            foreach (MethodInfo method in testClass.GetMethods(methodBF))
            {
                if (IsSetupMethod(method))
                {
                    setup = (SimpleTest)Delegate.CreateDelegate(typeof(SimpleTest), suiteInstance, method, true);
                    break; //take the first setup method it finds
                }
            }

            foreach (MethodInfo method in testClass.GetMethods(methodBF))
            {
                if (IsTestMethod(method))
                {
                    var testKnownFailureAttr = GetTestKnownFailureAttribute(method);

                    if (excludeKnownFailures && testKnownFailureAttr != null)
                    {
                        Console.WriteLine($"Excluding known failure: Method: {method.Name} Owner: {testKnownFailureAttr.Reason}");
                        continue;  // Exclude Known Failures
                    }
                    SimpleTest test = (SimpleTest)Delegate.CreateDelegate(typeof(SimpleTest), suiteInstance, method, true);
                    
                    if (setup != null)
                    {
                        test = (SimpleTest)Delegate.Combine(setup, test);
                    }

                    string name = String.Format("{0}.{1}", testClass.Name, method.Name);

                    Type expectedExceptionType = GetExpectedException(method);

                    var testBlock = new XamlTestInfoBlock(name, test, expectedExceptionType);
                    if (testKnownFailureAttr != null)
                    {
                        testBlock.IsTestKnownFailure = true;
                        testBlock.OwnerName = testKnownFailureAttr.Reason;
                    }

                    yield return testBlock;
                }
            }
        }

        public static IEnumerable<XamlTestInfoBlock> TestStrings(object suiteInstance, bool excludeKnownFailures)
        {
            Type testClass = suiteInstance.GetType();
            foreach (FieldInfo field in testClass.GetFields(fieldBF))
            {
                if (IsTestXamlString(field))
                {
                    var testKnownFailureAttr = GetTestKnownFailureAttribute(field);

                    if (excludeKnownFailures && testKnownFailureAttr != null)
                    {
                        Console.WriteLine($"Excluding known failure: XamlString:{field.Name} Owner:{testKnownFailureAttr.Reason}");
                        continue;  // Exclude Known Failures
                    }
                    string xamlString = field.GetValue(suiteInstance) as string;
                    MethodInfo loadMethod, validateMethod;
                    XamlStringParser loader = null;
                    PostTreeValidator validator = null;
                    Type expectedExceptionType;

                    if (!HasAlternateXamlLoader(field, out loadMethod))
                    {
                        loadMethod = TestFinder.StandardXamlLoadMethod(suiteInstance);
                    }
                    loader = Delegate.CreateDelegate(typeof(XamlStringParser),
                                        suiteInstance, loadMethod, true) as XamlStringParser;

                    if (HasTreeValidator(field, out validateMethod))
                    {
                        validator = Delegate.CreateDelegate(typeof(PostTreeValidator),
                                        suiteInstance, validateMethod, true) as PostTreeValidator;
                    }

                    expectedExceptionType = GetExpectedException(field);

                    string name = String.Format("{0}.{1}", testClass.Name, field.Name);

                    var testBlock = new XamlTestInfoBlock(name, xamlString, loader, validator, expectedExceptionType);
                    if (testKnownFailureAttr != null)
                    {
                        testBlock.IsTestKnownFailure = true;
                        testBlock.OwnerName = testKnownFailureAttr.Reason;
                    }
                    yield return testBlock;
                }
            }
        }

        private static Type GetExpectedException(MemberInfo member)
        {
            object[] exceptionAttrs = member.GetCustomAttributes(typeof(TestExpectedExceptionAttribute), false);
            if (exceptionAttrs.Length == 0)
            {
                return null;
            }

            // AttributeUsage is AllowMultiple=false
            return ((TestExpectedExceptionAttribute)exceptionAttrs[0]).ExpectedExceptionType;
        }

        private static MethodInfo StandardXamlLoadMethod(object suiteInstance)
        {
            Type testClass = suiteInstance.GetType();
            object[] stdLoadAttrs = testClass.GetCustomAttributes(typeof(TestStandardXamlLoaderAttribute), true);
            if (stdLoadAttrs.Length == 0)
            {
                string err = string.Format("TestSuite '{0}' is missing a Standard Xaml Loader", testClass.Name);
                throw new Exception(err);
            }

            // AttributeUsage is AllowMultiple=false
            var standardLoaderAttribute = stdLoadAttrs[0] as TestStandardXamlLoaderAttribute;
            MethodInfo method = GetXamlLoaderMethod(testClass, standardLoaderAttribute.MethodName);
            return method;
        }

        private static TestKnownFailureAttribute GetTestKnownFailureAttribute(MethodInfo method)
        {
            object[] objs = method.GetCustomAttributes(typeof(TestKnownFailureAttribute), false);
            return (objs.Length == 0) ? null : (TestKnownFailureAttribute)objs[0];
        }

        private static TestKnownFailureAttribute GetTestKnownFailureAttribute(FieldInfo field)
        {
            object[] objs = field.GetCustomAttributes(typeof(TestKnownFailureAttribute), false);
            return (objs.Length == 0) ? null : (TestKnownFailureAttribute)objs[0];
        }

        public static bool IsATestType(Type type)
        {
            if (!type.GetCustomAttributes(typeof(TestClassAttribute), false).Any())
            {
                return false;
            }

            if (type.GetCustomAttributes(typeof(TestDisabledAttribute), false).Any())
            {
                return false;
            }

            return true;
        }

        public static bool IsTestMethod(MethodInfo method)
        {
            if (!method.GetCustomAttributes(typeof(TestMethodAttribute), false).Any())
            {
                return false;
            }

            ParameterInfo[] paramInfo = method.GetParameters();

            if (paramInfo.Length != 0)
            {
                string err = string.Format("{0}.{1}() does not match the signature of 'DrtTest' delegate.",
                                    method.DeclaringType.Name, method.Name);
                throw new Exception(err);
            }

            if (method.GetCustomAttributes(typeof(TestDisabledAttribute), false).Any())
            {
                return false;
            }

            return true;
        }

        public static bool IsSetupMethod(MethodInfo method)
        {
            if (!method.GetCustomAttributes(typeof(TestSetupAttribute), false).Any())
            {
                return false;
            }

            ParameterInfo[] paramInfo = method.GetParameters();

            if (paramInfo.Length != 0)
            {
                string err = string.Format("{0}.{1}() does not match the signature of a setup method.",
                                    method.DeclaringType.Name, method.Name);
                throw new Exception(err);
            }

            return true;
        }

        public static bool IsNotKnownFailingTestMethod(MethodInfo method)
        {
            if (IsTestMethod(method))
            {
                if (!method.GetCustomAttributes(typeof(TestKnownFailureAttribute), false).Any())
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsNotKnownFailingXamlString(FieldInfo field)
        {
            if (IsTestXamlString(field))
            {
                if (!field.GetCustomAttributes(typeof(TestKnownFailureAttribute), false).Any())
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsTestXamlString(FieldInfo field)
        {
            if (!field.GetCustomAttributes(typeof(TestXamlAttribute), false).Any())
            {
                return false;
            }

            if (field.GetCustomAttributes(typeof(TestDisabledAttribute), false).Any())
            {
                return false;
            }

            if (field.FieldType != typeof(String))
            {
                return false;
            }

            return true;
        }

        public static bool HasAlternateXamlLoader(FieldInfo field, out MethodInfo method)
        {
            method = null;
            Type testClass = field.DeclaringType;

            object[] axls = field.GetCustomAttributes(typeof(TestAlternateXamlLoaderAttribute), false);
            if (axls.Length == 0)
            {
                return false;
            }

            // AttributeUsage is AllowMultiple=false
            var axl = axls[0] as TestAlternateXamlLoaderAttribute;
            method = GetXamlLoaderMethod(testClass, axl.MethodName);
            return true;
        }


        private static MethodInfo GetXamlLoaderMethod(Type testClass, string methodName)
        {
            MethodInfo method = testClass.GetMethod(methodName, methodBF);
            if (method == null)
            {
                // Error rather than return false, so it will get fixed.
                string err = string.Format("No method named {0} found on class {1}.",
                                            methodName, testClass.Name);
                throw new Exception(err);
            }

            ParameterInfo[] pInfos = method.GetParameters();
            if (pInfos.Length != 1 || pInfos[0].ParameterType != typeof(string)
                                   || method.ReturnType != typeof(object))
            {
                // Error rather than return false, so it will get fixed.
                string err = string.Format("{0}.{1}() does not match the signature of 'XamlStringParser' delegate.",
                                    testClass.Name, method.Name);
                throw new Exception(err);
            }

            return method;
        }

        public static bool HasTreeValidator(FieldInfo field, out MethodInfo method)
        {
            method = null;
            Type testClass = field.DeclaringType;
            object[] validatorAttrs = field.GetCustomAttributes(typeof(TestTreeValidatorAttribute), false);
            if (validatorAttrs.Length == 0)
            {
                return false;
            }

            // AttributeUsage is AllowMultiple=false
            var validatorAttribute = (TestTreeValidatorAttribute)validatorAttrs[0];
            method = GetValidatorMethod(testClass, validatorAttribute.MethodName);
            return true;
        }

        private static MethodInfo GetValidatorMethod(Type testClass, string methodName)
        {
            MethodInfo method = testClass.GetMethod(methodName);
            if (method == null)
            {
                // Error rather than return false, so it will get fixed.
                string err = string.Format("No method named {0} found on class {1}.",
                                    methodName, testClass.Name);
                throw new Exception(err);
            }

            ParameterInfo[] pInfos = method.GetParameters();
            if (pInfos.Length != 1 || pInfos[0].ParameterType != typeof(object)
                                   || method.ReturnType != typeof(void))
            {
                string err = string.Format("{0}.{1}() does not match the signature of 'PostTreeValidator' delegate.",
                                    testClass.Name, method.Name);
                throw new Exception(err);
            }

            return method;
        }
    }
}
