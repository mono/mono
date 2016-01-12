// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using System.Collections.Specialized;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

// TODO: Remove conditional code
namespace NUnit.Framework
{
    /// <summary>
    /// The TestCaseData class represents a set of arguments
    /// and other parameter info to be used for a parameterized
    /// test case. It provides a number of instance modifiers
    /// for use in initializing the test case.
    /// 
    /// Note: Instance modifiers are getters that return
    /// the same instance after modifying it's state.
    /// </summary>
    public class TestCaseData : ITestCaseData
    {

        #region Instance Fields

        /// <summary>
        /// The argument list to be provided to the test
        /// </summary>
        private object[] arguments;

        /// <summary>
        /// The expected result to be returned
        /// </summary>
        private object expectedResult;

        /// <summary>
        /// Data about any expected exception.
        /// </summary>
        private ExpectedExceptionData exceptionData;

        /// <summary>
        /// A dictionary of properties, used to add information
        /// to tests without requiring the class to change.
        /// </summary>
        private IPropertyBag properties;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TestCaseData"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public TestCaseData(params object[] args)
        {
			this.RunState = RunState.Runnable;

			if (args == null)
                this.arguments = new object[] { null };
            else
                this.arguments = args;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TestCaseData"/> class.
        /// </summary>
        /// <param name="arg">The argument.</param>
        public TestCaseData(object arg)
        {
			this.RunState = RunState.Runnable;
            this.arguments = new object[] { arg };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TestCaseData"/> class.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        public TestCaseData(object arg1, object arg2)
        {
			this.RunState = RunState.Runnable;
            this.arguments = new object[] { arg1, arg2 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TestCaseData"/> class.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        public TestCaseData(object arg1, object arg2, object arg3)
        {
			this.RunState = RunState.Runnable;
            this.arguments = new object[] { arg1, arg2, arg3 };
        }

        #endregion

        #region ITestCaseData Members

        /// <summary>
        /// Gets the argument list to be provided to the test
        /// </summary>
        public object[] Arguments
        {
            get { return arguments; }
        }

        /// <summary>
        /// Gets the expected result
        /// </summary>
        public object ExpectedResult
        {
            get { return expectedResult; }
			set
			{
				expectedResult = value;
				HasExpectedResult = true;
			}
        }

        private bool hasExpectedResult;
        /// <summary>
        /// Returns true if the expected result has been set
        /// </summary>
        public bool HasExpectedResult 
        {
            get { return hasExpectedResult; }
            set { hasExpectedResult = value; }
        }

        /// <summary>
        /// Gets data about any expected exception.
        /// </summary>
        public ExpectedExceptionData ExceptionData
        {
            get { return exceptionData; }
        }

        private string testName;
        /// <summary>
        /// Gets the name to be used for the test
        /// </summary>
        public string TestName 
        {
            get { return testName; }
            set { testName = value; }
        }

        private RunState runState;
        /// <summary>
		/// Gets the RunState for this test case.
		/// </summary>
		public RunState RunState 
        {
            get { return runState; }
            set { runState = value; }
        }

        /// <summary>
        /// Gets the property dictionary for this test
        /// </summary>
        public IPropertyBag Properties
        {
            get
            {
                if (properties == null)
                    properties = new NUnit.Framework.Internal.PropertyBag();

                return properties;
            }
        }

        #endregion

        #region Public Properties - NUnit 2.6 Compatibility

        /// <summary>
        /// Gets the expected result.
        /// </summary>
        public object Result
        {
            get { return expectedResult; }
        }

        /// <summary>
        /// The type of exception expected
        /// </summary>
        public Type ExpectedException
        {
            get { return exceptionData.ExpectedExceptionType; }
        }

        /// <summary>
        /// The full name of the expected exception type
        /// </summary>
        public string ExpectedExceptionName
        {
            get { return exceptionData.ExpectedExceptionName; }
        }

        /// <summary>
        /// The message to expect on any exception
        /// </summary>
        public string ExpectedExceptionMessage
        {
            get { return exceptionData.ExpectedMessage; }
        }

        /// <summary>
        /// The type of match to be made on the message
        /// </summary>
        public MessageMatch MatchType
        {
            get { return exceptionData.MatchType; }
        }

        #endregion

        #region Fluent Instance Modifiers

        /// <summary>
        /// Sets the expected result for the test
        /// </summary>
        /// <param name="result">The expected result</param>
        /// <returns>A modified TestCaseData</returns>
        public TestCaseData Returns(object result)
        {
            this.ExpectedResult = result;
            return this;
        }

        /// <summary>
        /// Sets the expected exception type for the test
        /// </summary>
        /// <param name="exceptionType">Type of the expected exception.</param>
        /// <returns>The modified TestCaseData instance</returns>
        public TestCaseData Throws(Type exceptionType)
        {
            //this.expectedExceptionType = exceptionType;
            exceptionData.ExpectedExceptionName = exceptionType.FullName;
            return this;
        }

        /// <summary>
        /// Sets the expected exception type for the test
        /// </summary>
        /// <param name="exceptionName">FullName of the expected exception.</param>
        /// <returns>The modified TestCaseData instance</returns>
        public TestCaseData Throws(string exceptionName)
        {
            exceptionData.ExpectedExceptionName = exceptionName;
            return this;
        }

        /// <summary>
        /// Sets the name of the test case
        /// </summary>
        /// <returns>The modified TestCaseData instance</returns>
        public TestCaseData SetName(string name)
        {
            this.TestName = name;
            return this;
        }

        /// <summary>
        /// Sets the description for the test case
        /// being constructed.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>The modified TestCaseData instance.</returns>
        public TestCaseData SetDescription(string description)
        {
            this.Properties.Set(PropertyNames.Description, description);
            return this;
        }

        /// <summary>
        /// Applies a category to the test
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public TestCaseData SetCategory(string category)
        {
            this.Properties.Add(PropertyNames.Category, category);
            return this;
        }

        /// <summary>
        /// Applies a named property to the test
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propValue"></param>
        /// <returns></returns>
        public TestCaseData SetProperty(string propName, string propValue)
        {
            this.Properties.Add(propName, propValue);
            return this;
        }

        /// <summary>
        /// Applies a named property to the test
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propValue"></param>
        /// <returns></returns>
        public TestCaseData SetProperty(string propName, int propValue)
        {
            this.Properties.Add(propName, propValue);
            return this;
        }

        /// <summary>
        /// Applies a named property to the test
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propValue"></param>
        /// <returns></returns>
        public TestCaseData SetProperty(string propName, double propValue)
        {
            this.Properties.Add(propName, propValue);
            return this;
        }

        /// <summary>
        /// Ignores this TestCase.
        /// </summary>
        /// <returns></returns>
        public TestCaseData Ignore()
        {
            this.RunState = RunState.Ignored;
            return this;
        }
		
		/// <summary>
		/// Marks the test case as explicit.
		/// </summary>
		public TestCaseData Explicit()	{
			this.RunState = RunState.Explicit;
			return this;
		}

		/// <summary>
		/// Marks the test case as explicit, specifying the reason.
		/// </summary>
		public TestCaseData Explicit(string reason)
		{
			this.RunState = RunState.Explicit;
            this.Properties.Set(PropertyNames.SkipReason, reason);
			return this;
		}

        /// <summary>
        /// Ignores this TestCase, specifying the reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public TestCaseData Ignore(string reason)
        {
            this.RunState = RunState.Ignored;
            this.Properties.Set(PropertyNames.SkipReason, reason);
            return this;
        }

        #endregion
    }
}
