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
using System.Reflection;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// ExpectedExceptionData is a struct used within the framework
    /// to encapsulate information about an expected exception.
    /// </summary>
    public struct ExpectedExceptionData
    {
        #region Fields

        private Type expectedExceptionType;
        private string expectedExceptionName;
        private string expectedMessage;
        private MessageMatch matchType;
        private string userMessage;
        private string handlerName;
        private MethodInfo exceptionHandler;

        #endregion

        #region Properties

        /// <summary>
        /// The Type of any exception that is expected.
        /// </summary>
        public Type ExpectedExceptionType
        {
            get { return expectedExceptionType; }
            set 
            { 
                expectedExceptionType = value;
                expectedExceptionName = value.FullName;
            }
        }

        /// <summary>
        /// The FullName of any exception that is expected
        /// </summary>
        public string ExpectedExceptionName
        {
            get { return expectedExceptionName; }
            set 
            { 
                expectedExceptionName = value;
                expectedExceptionType = null;
            }
        }

        /// <summary>
        /// The Message of any exception that is expected
        /// </summary>
        public string ExpectedMessage
        {
            get { return expectedMessage; }
            set { expectedMessage = value; }
        }

        /// <summary>
        ///  The type of match to be performed on the expected message
        /// </summary>
        public MessageMatch MatchType
        {
            get { return matchType; }
            set { matchType = value; }
        }

        /// <summary>
        /// A user message to be issued in case of error
        /// </summary>
        public string UserMessage
        {
            get { return userMessage; }
            set { userMessage = value; }
        }

        /// <summary>
        /// The name of an alternate exception handler to be
        /// used to validate the exception.
        /// </summary>
        public string HandlerName
        {
            get { return handlerName; }
            set 
            { 
                handlerName = value;
                exceptionHandler = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a MethodInfo for the handler to be used to
        /// validate any exception thrown.
        /// </summary>
        /// <param name="fixtureType">The Type of the fixture.</param>
        /// <returns>A MethodInfo.</returns>
        public MethodInfo GetExceptionHandler(Type fixtureType)
        {
            if (exceptionHandler == null && handlerName != null)
            {
                exceptionHandler = fixtureType.GetMethod(
                    handlerName,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(System.Exception) },
                    null);
            }

            return exceptionHandler;
        }

        #endregion
    };
}
