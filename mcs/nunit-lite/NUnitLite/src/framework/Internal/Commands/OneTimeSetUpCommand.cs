// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Reflection;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// OneTimeSetUpCommand runs any one-time setup methods for a suite,
    /// constructing the user test object if necessary.
    /// </summary>
    public class OneTimeSetUpCommand : TestCommand
    {
        private readonly TestSuite suite;
        private readonly Type fixtureType;
        private readonly object[] arguments;

        /// <summary>
        /// Constructs a OneTimeSetUpComand for a suite
        /// </summary>
        /// <param name="suite">The suite to which the command applies</param>
        public OneTimeSetUpCommand(TestSuite suite) : base(suite) 
        {
            this.suite = suite;
            this.fixtureType = suite.FixtureType;
            this.arguments = suite.arguments;
        }

        /// <summary>
        /// Overridden to run the one-time setup for a suite.
        /// </summary>
        /// <param name="context">The TestExecutionContext to be used.</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            if (fixtureType != null)
            {
                if (context.TestObject == null && !IsStaticClass(fixtureType))
                    context.TestObject = Reflect.Construct(fixtureType, arguments);

                foreach (MethodInfo method in  Reflect.GetMethodsWithAttribute(fixtureType, typeof(TestFixtureSetUpAttribute), true))
                    Reflect.InvokeMethod(method, method.IsStatic ? null : context.TestObject);
            }

            return context.CurrentResult;
        }

        private static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }
}
