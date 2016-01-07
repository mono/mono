// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// The PropertyNames class provides static constants for the
    /// standard property names that NUnit uses on tests.
    /// </summary>
    public class PropertyNames
    {
        /// <summary>
        /// The Description of a test
        /// </summary>
        public static readonly string Description = "Description";
        
        /// <summary>
        /// The reason a test was not run
        /// </summary>
        public static readonly string SkipReason = "_SKIPREASON";

        /// <summary>
        /// The stack trace from any data provider that threw
        /// an exception.
        /// </summary>
        public static readonly string ProviderStackTrace = "_PROVIDERSTACKTRACE";

        /// <summary>
        /// The culture to be set for a test
        /// </summary>
        public static readonly string SetCulture = "SetCulture";

        /// <summary>
        /// The UI culture to be set for a test
        /// </summary>
        public static readonly string SetUICulture = "SetUICulture";

        /// <summary>
        /// The categories applying to a test
        /// </summary>
        public static readonly string Category = "Category";

#if !NUNITLITE
        /// <summary>
        /// The ApartmentState required for running the test
        /// </summary>
        public static readonly string ApartmentState = "ApartmentState";
#endif

        /// <summary>
        /// The timeout value for the test
        /// </summary>
        public static readonly string Timeout = "Timeout";

        /// <summary>
        /// The number of times the test should be repeated
        /// </summary>
        public static readonly string RepeatCount = "Repeat";

        /// <summary>
        /// The maximum time in ms, above which the test is considered to have failed
        /// </summary>
        public static readonly string MaxTime = "MaxTime";

        /// <summary>
        /// The selected strategy for joining parameter data into test cases
        /// </summary>
        public static readonly string JoinType = "_JOINTYPE";

        /// <summary>
        /// The process ID of the executing assembly
        /// </summary>
        public static readonly string ProcessID = "_PID";
        
        /// <summary>
        /// The FriendlyName of the AppDomain in which the assembly is running
        /// </summary>
        public static readonly string AppDomain = "_APPDOMAIN";
    }
}
