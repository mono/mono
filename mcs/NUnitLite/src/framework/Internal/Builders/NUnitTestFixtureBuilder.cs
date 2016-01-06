// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Extensibility;

namespace NUnit.Framework.Builders
{
	/// <summary>
	/// Built-in SuiteBuilder for NUnit TestFixture
	/// </summary>
	public class NUnitTestFixtureBuilder : ISuiteBuilder
    {
        #region Static Fields
                
        static readonly string NO_TYPE_ARGS_MSG = 
            "Fixture type contains generic parameters. You must either provide " +
            "Type arguments or specify constructor arguments that allow NUnit " +
            "to deduce the Type arguments.";

        #endregion

        #region Instance Fields
        /// <summary>
		/// The NUnitTestFixture being constructed;
		/// </summary>
		private TestFixture fixture;

        private Extensibility.ITestCaseBuilder2 testBuilder;

		#endregion

        #region Constructor

        public NUnitTestFixtureBuilder()
        {
            testBuilder = new NUnitTestCaseBuilder();
        }

        #endregion

        #region ISuiteBuilder Methods
        /// <summary>
		/// Checks to see if the fixture type has the TestFixtureAttribute
		/// </summary>
		/// <param name="type">The fixture type to check</param>
		/// <returns>True if the fixture can be built, false if not</returns>
		public bool CanBuildFrom(Type type)
		{
            if ( type.IsAbstract && !type.IsSealed )
                return false;

            if (type.IsDefined(typeof(TestFixtureAttribute), true))
                return true;

#if CLR_2_0 || CLR_4_0
            // Generics must have a TestFixtureAttribute
            if (type.IsGenericTypeDefinition)
                return false;
#endif

            return Reflect.HasMethodWithAttribute(type, typeof(NUnit.Framework.TestAttribute)) ||
                   Reflect.HasMethodWithAttribute(type, typeof(NUnit.Framework.TestCaseAttribute)) ||
                   Reflect.HasMethodWithAttribute(type, typeof(NUnit.Framework.TestCaseSourceAttribute)) ||
                   Reflect.HasMethodWithAttribute(type, typeof(NUnit.Framework.TheoryAttribute));
		}

		/// <summary>
		/// Build a TestSuite from type provided.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Test BuildFrom(Type type)
		{
            TestFixtureAttribute[] attrs = GetTestFixtureAttributes(type);

#if CLR_2_0 || CLR_4_0
            if (type.IsGenericType)
                return BuildMultipleFixtures(type, attrs);
#endif

            switch (attrs.Length)
            {
                case 0:
                    return BuildSingleFixture(type, null);
                case 1:
                    object[] args = (object[])attrs[0].Arguments;
                    return args == null || args.Length == 0
                        ? BuildSingleFixture(type, attrs[0])
                        : BuildMultipleFixtures(type, attrs);
                default:
                    return BuildMultipleFixtures(type, attrs);
            }
        }
		#endregion

		#region Helper Methods

        private Test BuildMultipleFixtures(Type type, TestFixtureAttribute[] attrs)
        {
            TestSuite suite = new ParameterizedFixtureSuite(type);

            if (attrs.Length > 0)
            {
                foreach (TestFixtureAttribute attr in attrs)
                    suite.Add(BuildSingleFixture(type, attr));
            }
            else
            {
                suite.RunState = RunState.NotRunnable;
                suite.Properties.Set(PropertyNames.SkipReason, NO_TYPE_ARGS_MSG);
            }

            return suite;
        }

        private Test BuildSingleFixture(Type type, TestFixtureAttribute attr)
        {
            object[] arguments = null;

            if (attr != null)
            {
                arguments = (object[])attr.Arguments;

#if CLR_2_0 || CLR_4_0
                if (type.ContainsGenericParameters)
                {
                    Type[] typeArgs = (Type[])attr.TypeArgs;
                    if( typeArgs.Length > 0 || 
                        TypeHelper.CanDeduceTypeArgsFromArgs(type, arguments, ref typeArgs))
                    {
                        type = TypeHelper.MakeGenericType(type, typeArgs);
                    }
                }
#endif
            }

            this.fixture = new TestFixture(type, arguments);
            CheckTestFixtureIsValid(fixture);

            fixture.ApplyAttributesToTest(type);

            if (fixture.RunState == RunState.Runnable && attr != null)
            {
                if (attr.Ignore)
                {
                    fixture.RunState = RunState.Ignored;
                    fixture.Properties.Set(PropertyNames.SkipReason, attr.IgnoreReason);
                }
            }

            AddTestCases(type);

            return this.fixture;
        }

        /// <summary>
		/// Method to add test cases to the newly constructed fixture.
		/// </summary>
		/// <param name="fixtureType"></param>
		private void AddTestCases( Type fixtureType )
		{
			IList methods = fixtureType.GetMethods( 
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );

			foreach(MethodInfo method in methods)
			{
				Test test = BuildTestCase(method, this.fixture);

				if(test != null)
				{
					this.fixture.Add( test );
				}
			}
		}

		/// <summary>
		/// Method to create a test case from a MethodInfo and add
		/// it to the fixture being built. It first checks to see if
		/// any global TestCaseBuilder addin wants to build the
		/// test case. If not, it uses the internal builder
		/// collection maintained by this fixture builder. After
		/// building the test case, it applies any decorators
		/// that have been installed.
		/// 
		/// The default implementation has no test case builders.
		/// Derived classes should add builders to the collection
		/// in their constructor.
		/// </summary>
		/// <param name="method">The MethodInfo for which a test is to be created</param>
        /// <param name="suite">The test suite being built.</param>
		/// <returns>A newly constructed Test</returns>
		private Test BuildTestCase( MethodInfo method, TestSuite suite )
		{
            return testBuilder.CanBuildFrom(method, suite)
                ? testBuilder.BuildFrom(method, suite)
                : null;
		}

        private void CheckTestFixtureIsValid(TestFixture fixture)
        {
            Type fixtureType = fixture.FixtureType;

#if CLR_2_0 || CLR_4_0
            if (fixtureType.ContainsGenericParameters)
            {
                SetNotRunnable(fixture, NO_TYPE_ARGS_MSG);
                return;
            }
#endif
            if( !IsStaticClass(fixtureType)  && !HasValidConstructor(fixtureType, fixture.arguments) )
            {
                SetNotRunnable(fixture, "No suitable constructor was found");
                return;
            }

            if (!CheckSetUpTearDownMethods(fixture, fixture.SetUpMethods))
                return;
            if (!CheckSetUpTearDownMethods(fixture, fixture.TearDownMethods))
                return;
            if (!CheckSetUpTearDownMethods(fixture, Reflect.GetMethodsWithAttribute(fixture.FixtureType, typeof(TestFixtureSetUpAttribute), true)))
                return;
            CheckSetUpTearDownMethods(fixture, Reflect.GetMethodsWithAttribute(fixture.FixtureType, typeof(TestFixtureTearDownAttribute), true));
        }

        private static bool HasValidConstructor(Type fixtureType, object[] args)
        {
            Type[] argTypes;

            // Note: This could be done more simply using
            // Type.EmptyTypes and Type.GetTypeArray() but
            // they don't exist in all runtimes we support.
            if (args == null)
                argTypes = new Type[0];
            else
            {
                argTypes = new Type[args.Length];

                int index = 0;
                foreach (object arg in args)
                    argTypes[index++] = arg.GetType();
            }

            return fixtureType.GetConstructor(argTypes) != null;
        }

        private void SetNotRunnable(TestFixture fixture, string reason)
        {
            fixture.RunState = RunState.NotRunnable;
            fixture.Properties.Set(PropertyNames.SkipReason, reason);
        }

        private static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        private bool CheckSetUpTearDownMethods(TestFixture fixture, MethodInfo[] methods)
        {
            foreach (MethodInfo method in methods)
                if (method.IsAbstract ||
                     !method.IsPublic && !method.IsFamily ||
                     method.GetParameters().Length > 0 ||
                     !method.ReturnType.Equals(typeof(void)))
                {
                    SetNotRunnable(fixture, string.Format("Invalid signature for Setup or TearDown method: {0}", method.Name));
                    return false;
                }

            return true;
        }

        /// <summary>
        /// Get TestFixtureAttributes following a somewhat obscure
        /// set of rules to eliminate spurious duplication of fixtures.
        /// 1. If there are any attributes with args, they are the only
        ///    ones returned and those without args are ignored.
        /// 2. No more than one attribute without args is ever returned.
        /// </summary>
        private TestFixtureAttribute[] GetTestFixtureAttributes(Type type)
        {
            TestFixtureAttribute[] attrs = 
                (TestFixtureAttribute[])type.GetCustomAttributes(typeof(TestFixtureAttribute), true);

            // Just return - no possibility of duplication
            if (attrs.Length <= 1)
                return attrs;

            int withArgs = 0;
            bool[] hasArgs = new bool[attrs.Length];

            // Count and record those attrs with arguments            
            for (int i = 0; i < attrs.Length; i++)
            {
                TestFixtureAttribute attr = attrs[i];

                if (attr.Arguments.Length > 0 || attr.TypeArgs.Length > 0)
                {
                    withArgs++;
                    hasArgs[i] = true;
                }
            }

            // If all attributes have args, just return them
            if (withArgs == attrs.Length)
                return attrs;

            // If all attributes are without args, just return the first found
            if (withArgs == 0)
                return new TestFixtureAttribute[] { attrs[0] };

            // Some of each type, so extract those with args
            int count = 0;
            TestFixtureAttribute[] result = new TestFixtureAttribute[withArgs];
            for (int i = 0; i < attrs.Length; i++)
                if (hasArgs[i])
                    result[count++] = attrs[i];

            return result;
        }
		#endregion
	}
}