// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

namespace NUnit.Framework
{
    /// <summary>
    /// Provide the context information of the current test.
    /// This is an adapter for the internal ExecutionContext
    /// class, hiding the internals from the user test.
    /// </summary>
    public class TestContext
    {
        private TestExecutionContext ec;
        private TestAdapter test;
        private ResultAdapter result;

        #region Constructor

        /// <summary>
        /// Construct a TestContext for an ExecutionContext
        /// </summary>
        /// <param name="ec">The ExecutionContext to adapt</param>
        public TestContext(TestExecutionContext ec)
        {
            this.ec = ec;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the current test context. This is created
        /// as needed. The user may save the context for
        /// use within a test, but it should not be used
        /// outside the test for which it is created.
        /// </summary>
        public static TestContext CurrentContext
        {
            get { return new TestContext(TestExecutionContext.CurrentContext); }
        }

        /// <summary>
        /// Get a representation of the current test.
        /// </summary>
        public TestAdapter Test
        {
            get
            {
                if (test == null)
                    test = new TestAdapter(ec.CurrentTest);

                return test;
            }
        }

        /// <summary>
        /// Gets a Representation of the TestResult for the current test. 
        /// </summary>
        public ResultAdapter Result
        {
            get
            {
                if (result == null)
                    result = new ResultAdapter(ec.CurrentResult);

                return result;
            }
        }

#if !NETCF
        /// <summary>
        /// Gets the directory containing the current test assembly.
        /// </summary>
        public string TestDirectory
        {
            get
            {
                return AssemblyHelper.GetDirectoryName(ec.CurrentTest.FixtureType.Assembly);
            }
        }
#endif

        /// <summary>
        /// Gets the directory to be used for outputing files created
        /// by this test run.
        /// </summary>
        public string WorkDirectory
        {
            get
            {
				return ec.WorkDirectory;
        	}
        }

        public RandomGenerator Random
        {
            get
            {
                return ec.RandomGenerator;
            }
        }

        #endregion

        #region Nested TestAdapter Class

        /// <summary>
        /// TestAdapter adapts a Test for consumption by
        /// the user test code.
        /// </summary>
        public class TestAdapter
        {
            private Test test;

            #region Constructor

            /// <summary>
            /// Construct a TestAdapter for a Test
            /// </summary>
            /// <param name="test">The Test to be adapted</param>
            public TestAdapter(Test test)
            {
                this.test = test;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the unique Id of a test
            /// </summary>
            public int ID
            {
                get { return test.Id; }
            }

            /// <summary>
            /// The name of the test, which may or may not be 
            /// the same as the method name.
            /// </summary>
            public string Name
            {
                get
                {
                    return test.Name;
                }
            }
			
            /// <summary>
            /// The name of the method representing the test.
            /// </summary>
			public string MethodName
			{
				get
				{
					return test is TestMethod
						? ((TestMethod)test).Method.Name
						: null;
				}
			}

            /// <summary>
            /// The FullName of the test
            /// </summary>
            public string FullName
            {
                get
                {
                    return test.FullName;
                }
            }

            /// <summary>
            /// The properties of the test.
            /// </summary>
            public IPropertyBag Properties
            {
                get
                {
                    return test.Properties;
                }
            }

            #endregion
        }

        #endregion

        #region Nested ResultAdapter Class

        /// <summary>
        /// ResultAdapter adapts a TestResult for consumption by
        /// the user test code.
        /// </summary>
        public class ResultAdapter
        {
            private TestResult result;

            #region Constructor

            /// <summary>
            /// Construct a ResultAdapter for a TestResult
            /// </summary>
            /// <param name="result">The TestResult to be adapted</param>
            public ResultAdapter(TestResult result)
            {
                this.result = result;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets a ResultState representing the outcome of the test.
            /// </summary>
            public ResultState Outcome
            {
                get
                {
                    return result.ResultState;
                }
            }

            #endregion
        }
        
        #endregion
    }
}
