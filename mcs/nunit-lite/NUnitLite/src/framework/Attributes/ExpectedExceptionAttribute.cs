// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Api;

namespace NUnit.Framework
{
    /// <summary>
    /// ExpectedExceptionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited=false)]
    public class ExpectedExceptionAttribute : NUnitAttribute
    {
        private ExpectedExceptionData exceptionData = new ExpectedExceptionData();

        /// <summary>
        /// Constructor for a non-specific exception
        /// </summary>
        public ExpectedExceptionAttribute()
        {
        }

        /// <summary>
        /// Constructor for a given type of exception
        /// </summary>
        /// <param name="exceptionType">The type of the expected exception</param>
        public ExpectedExceptionAttribute(Type exceptionType)
        {
            exceptionData.ExpectedExceptionType = exceptionType;
        }

        /// <summary>
        /// Constructor for a given exception name
        /// </summary>
        /// <param name="exceptionName">The full name of the expected exception</param>
        public ExpectedExceptionAttribute(string exceptionName)
        {
            exceptionData.ExpectedExceptionName = exceptionName;
        }

        /// <summary>
        /// Gets or sets the expected exception type
        /// </summary>
        public Type ExpectedException
        {
            get { return exceptionData.ExpectedExceptionType; }
            set { exceptionData.ExpectedExceptionType = value; }
        }

        /// <summary>
        /// Gets or sets the full Type name of the expected exception
        /// </summary>
        public string ExpectedExceptionName
        {
            get { return exceptionData.ExpectedExceptionName; }
            set { exceptionData.ExpectedExceptionName = value; }
        }

        /// <summary>
        /// Gets or sets the expected message text
        /// </summary>
        public string ExpectedMessage
        {
            get { return exceptionData.ExpectedMessage; }
            set { exceptionData.ExpectedMessage = value; }
        }

        /// <summary>
        /// Gets or sets the user message displayed in case of failure
        /// </summary>
        public string UserMessage
        {
            get { return exceptionData.UserMessage; }
            set { exceptionData.UserMessage = value; }
        }

        /// <summary>
        ///  Gets or sets the type of match to be performed on the expected message
        /// </summary>
        public MessageMatch MatchType
        {
            get { return exceptionData.MatchType; }
            set { exceptionData.MatchType = value; }
        }

        /// <summary>
        ///  Gets the name of a method to be used as an exception handler
        /// </summary>
        public string Handler
        {
            get { return exceptionData.HandlerName; }
            set { exceptionData.HandlerName = value; }
        }

        /// <summary>
        /// Gets all data about the expected exception.
        /// </summary>
        public ExpectedExceptionData ExceptionData
        {
            get { return exceptionData; }
        }

        //#region IApplyToTest Members

        //void IApplyToTest.ApplyToTest(ITest test)
        //{
        //    TestMethod testMethod = test as TestMethod;
        //    if (testMethod != null)
        //        testMethod.CustomDecorators.Add(new ExpectedExceptionDecorator());
        //}

        //#endregion
    }

    /// <summary>
    /// ExpectedExceptionDecorator applies to a TestCommand and returns
    /// a success result only if the expected exception is thrown. 
    /// Otherwise, an appropriate failure result is returned.
    /// </summary>
    public class ExpectedExceptionDecorator : ICommandDecorator
    {
        private ExpectedExceptionData exceptionData;

        /// <summary>
        /// Construct an ExpectedExceptionDecorator using specified data.
        /// </summary>
        /// <param name="exceptionData">Data describing the expected exception</param>
        public ExpectedExceptionDecorator(ExpectedExceptionData exceptionData)
        {
            this.exceptionData = exceptionData;
        }

        #region ICommandDecorator Members

        CommandStage ICommandDecorator.Stage
        {
            get { return CommandStage.BelowSetUpTearDown; }
        }

        int ICommandDecorator.Priority
        {
            get { return 0; }
        }

        TestCommand ICommandDecorator.Decorate(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, exceptionData);
        }

        #endregion
    }
}
