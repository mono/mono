// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnitLite.Tests
{
    [TestFixture]
    class StackFilterTest
    {
        private static readonly string NL = NUnit.Env.NewLine;

        private static readonly string rawTrace1 =
    @"   at NUnit.Framework.Assert.Fail(String message) in D:\Dev\NUnitLite\NUnitLite\Framework\Assert.cs:line 56" + NL +
    @"   at NUnit.Framework.Assert.That(String label, Object actual, Matcher expectation, String message) in D:\Dev\NUnitLite\NUnitLite\Framework\Assert.cs:line 50" + NL +
    @"   at NUnit.Framework.Assert.That(Object actual, Matcher expectation) in D:\Dev\NUnitLite\NUnitLite\Framework\Assert.cs:line 19" + NL +
    @"   at NUnit.Tests.GreaterThanMatcherTest.MatchesGoodValue() in D:\Dev\NUnitLite\NUnitLiteTests\GreaterThanMatcherTest.cs:line 12" + NL;

        private static readonly string filteredTrace1 =
    @"   at NUnit.Tests.GreaterThanMatcherTest.MatchesGoodValue() in D:\Dev\NUnitLite\NUnitLiteTests\GreaterThanMatcherTest.cs:line 12" + NL;

        private static readonly string rawTrace2 =
    @"  at NUnit.Framework.Assert.Fail(String message, Object[] args)" + NL +
    @"  at MyNamespace.MyAppsTests.AssertFailTest()" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.InternalInvoke(RuntimeMethodInfo rtmi, Object obj, BindingFlags invokeAttr, Binder binder, Object parameters, CultureInfo culture, Boolean isBinderDefault, Assembly caller, Boolean verifyAccess, StackCrawlMark& stackMark)" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.InternalInvoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture, Boolean verifyAccess, StackCrawlMark& stackMark)" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)" + NL +
    @"  at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)" + NL +
    @"  at NUnitLite.ProxyTestCase.InvokeMethod(MethodInfo method, Object[] args)" + NL +
    @"  at NUnit.Framework.TestCase.RunTest()" + NL +
    @"  at NUnit.Framework.TestCase.RunBare()" + NL +
    @"  at NUnit.Framework.TestCase.Run(TestResult result, TestListener listener)" + NL +
    @"  at NUnit.Framework.TestCase.Run(TestListener listener)" + NL +
    @"  at NUnit.Framework.TestSuite.Run(TestListener listener)" + NL +
    @"  at NUnit.Framework.TestSuite.Run(TestListener listener)" + NL +
    @"  at NUnitLite.Runner.TestRunner.Run(ITest test)" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Run(ITest test)" + NL +
    @"  at NUnitLite.Runner.TestRunner.Run(Assembly assembly)" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Run()" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Main(String[] args)" + NL +
    @"  at OpenNETCF.Linq.Demo.Program.Main(String[] args)" + NL;

        private static readonly string filteredTrace2 =
    @"  at MyNamespace.MyAppsTests.AssertFailTest()" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.InternalInvoke(RuntimeMethodInfo rtmi, Object obj, BindingFlags invokeAttr, Binder binder, Object parameters, CultureInfo culture, Boolean isBinderDefault, Assembly caller, Boolean verifyAccess, StackCrawlMark& stackMark)" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.InternalInvoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture, Boolean verifyAccess, StackCrawlMark& stackMark)" + NL +
    @"  at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)" + NL +
    @"  at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)" + NL +
    @"  at NUnitLite.ProxyTestCase.InvokeMethod(MethodInfo method, Object[] args)" + NL +
    @"  at NUnit.Framework.TestCase.RunTest()" + NL +
    @"  at NUnit.Framework.TestCase.RunBare()" + NL +
    @"  at NUnit.Framework.TestCase.Run(TestResult result, TestListener listener)" + NL +
    @"  at NUnit.Framework.TestCase.Run(TestListener listener)" + NL +
    @"  at NUnit.Framework.TestSuite.Run(TestListener listener)" + NL +
    @"  at NUnit.Framework.TestSuite.Run(TestListener listener)" + NL +
    @"  at NUnitLite.Runner.TestRunner.Run(ITest test)" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Run(ITest test)" + NL +
    @"  at NUnitLite.Runner.TestRunner.Run(Assembly assembly)" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Run()" + NL +
    @"  at NUnitLite.Runner.ConsoleUI.Main(String[] args)" + NL +
    @"  at OpenNETCF.Linq.Demo.Program.Main(String[] args)" + NL;

        [Test]
        public void FilterFailureTrace1()
        {
            Assert.That(StackFilter.Filter(rawTrace1), Is.EqualTo(filteredTrace1));
        }

        [Test]
        public void FilterFailureTrace2()
        {
            Assert.That(StackFilter.Filter(rawTrace2), Is.EqualTo(filteredTrace2));
        }
    }
}
