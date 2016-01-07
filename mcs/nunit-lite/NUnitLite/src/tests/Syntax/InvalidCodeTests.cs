// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org.
// ****************************************************************

using System;
using System.Collections;
using System.CodeDom.Compiler;
using NUnit.Framework.Constraints;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Syntax
{
    [TestFixture]
    public class InvalidCodeTests : AssertionHelper
    {
        static readonly string template1 =
@"using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

class SomeClass
{
    void SomeMethod()
    {
        object c = $FRAGMENT$;
    }
}";

        [TestCase("Is.Null.Not")]
        [TestCase("Is.Not.Null.GreaterThan(10))")]
        [TestCase("Is.Null.All")]
        [TestCase("Is.And")]
        [TestCase("Is.All.And.And")]
        [TestCase("Is.Null.And.Throws")]
        public void CodeShouldNotCompile(string fragment)
        {
            string code = template1.Replace("$FRAGMENT$", fragment);
            TestCompiler compiler = new TestCompiler(
                new string[] { "system.dll", "nunit.framework.dll" },
                "test.dll");
            CompilerResults results = compiler.CompileCode(code);
            if (results.NativeCompilerReturnValue == 0)
                Assert.Fail("Code fragment \"" + fragment + "\" should not compile but it did");
        }

        static readonly string template2 =
@"using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

class SomeClass
{
    void SomeMethod()
    {
        Assert.That(42, $FRAGMENT$);
    }
}";

        [TestCase("Is.Not")]
        [TestCase("Is.All")]
        [TestCase("Is.Not.All")]
        [TestCase("Is.All.Not")]
        public void CodeShouldNotCompileAsFinishedConstraint(string fragment)
        {
            string code = template2.Replace("$FRAGMENT$", fragment);
            TestCompiler compiler = new TestCompiler(
                new string[] { "system.dll", "nunit.framework.dll" },
                "test.dll");
            CompilerResults results = compiler.CompileCode(code);
            if (results.NativeCompilerReturnValue == 0)
                Assert.Fail("Code fragment \"" + fragment + "\" should not compile as a finished constraint but it did");
        }
    }
}