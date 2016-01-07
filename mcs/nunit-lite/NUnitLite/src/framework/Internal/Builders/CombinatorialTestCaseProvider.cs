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
using System.Reflection;
using System.Collections;
using NUnit.Framework.Api;
using NUnit.Framework.Extensibility;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Builders
{
    /// <summary>
    /// CombinatorialTestCaseProvider creates test cases from individual
    /// parameter data values, combining them using the CombiningStrategy
    /// indicated by an Attribute used on the test method.
    /// </summary>
    public class CombinatorialTestCaseProvider : ITestCaseProvider
    {
        #region Static Members
        static IParameterDataProvider dataPointProvider = new ParameterDataProviders();

        #endregion

        #region ITestCaseProvider Members

        /// <summary>
        /// Determine whether any test cases are available for a parameterized method.
        /// </summary>
        /// <param name="method">A MethodInfo representing a parameterized test</param>
        /// <returns>
        /// True if any cases are available, otherwise false.
        /// </returns>
        public bool HasTestCasesFor(System.Reflection.MethodInfo method)
        {
            if (method.GetParameters().Length == 0)
                return false;

            foreach (ParameterInfo parameter in method.GetParameters())
                if (!dataPointProvider.HasDataFor(parameter))
                    return false;

            return true;
        }

        /// <summary>
        /// Return an IEnumerable providing test cases for use in
        /// running a paramterized test.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
#if CLR_2_0 || CLR_4_0
        public System.Collections.Generic.IEnumerable<ITestCaseData> GetTestCasesFor(MethodInfo method)
#else
        public IEnumerable GetTestCasesFor(MethodInfo method)
#endif
        {
            return GetStrategy(method).GetTestCases();
        }
        #endregion

        #region GetStrategy

        /// <summary>
        /// Gets the strategy to be used in building test cases for this test.
        /// </summary>
        /// <param name="method">The method for which test cases are being built.</param>
        /// <returns></returns>
        private CombiningStrategy GetStrategy(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            IEnumerable[] sources = new IEnumerable[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                sources[i] = dataPointProvider.GetDataFor(parameters[i]);

            if (method.IsDefined(typeof(NUnit.Framework.SequentialAttribute), false))
                return new SequentialStrategy(sources);

            if (method.IsDefined(typeof(NUnit.Framework.PairwiseAttribute), false) &&
                method.GetParameters().Length > 2)
                    return new PairwiseStrategy(sources);

            return new CombinatorialStrategy(sources);
        }

        #endregion
    }
}
