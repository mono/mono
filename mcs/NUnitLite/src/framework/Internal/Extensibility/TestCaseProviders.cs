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

#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Builders;

namespace NUnit.Framework.Extensibility
{
    class TestCaseProviders : ITestCaseProvider
    {
#if CLR_2_0 || CLR_4_0
        private List<ITestCaseProvider> Extensions = new List<ITestCaseProvider>();
#else
        private System.Collections.ArrayList Extensions = new System.Collections.ArrayList();
#endif

        public TestCaseProviders()
        {
            this.Extensions.Add(new DataAttributeTestCaseProvider());
            this.Extensions.Add(new CombinatorialTestCaseProvider());
        }

        #region ITestCaseProvider Members

        /// <summary>
        /// Determine whether any test cases are available for a parameterized method.
        /// </summary>
        /// <param name="method">A MethodInfo representing a parameterized test</param>
        /// <returns>True if any cases are available, otherwise false.</returns>
        public bool HasTestCasesFor(MethodInfo method)
        {
            foreach (ITestCaseProvider provider in Extensions)
                if (provider.HasTestCasesFor(method))
                    return true;

            return false;
        }

        /// <summary>
        /// Return an enumeration providing test cases for use in
        /// running a parameterized test.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
#if CLR_2_0 || CLR_4_0
        public System.Collections.Generic.IEnumerable<ITestCaseData> GetTestCasesFor(MethodInfo method)
        {
            List<ITestCaseData> testcases = new List<ITestCaseData>();
#else
        public System.Collections.IEnumerable GetTestCasesFor(MethodInfo method)
        {
            System.Collections.ArrayList testcases = new System.Collections.ArrayList();
#endif

            foreach (ITestCaseProvider provider in Extensions)
                try
                {
                    if (provider.HasTestCasesFor(method))
                        foreach (ITestCaseData testcase in provider.GetTestCasesFor(method))
                            testcases.Add(testcase);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    testcases.Add(new ParameterSet(ex.InnerException));
                }
                catch (System.Exception ex)
                {
                    testcases.Add(new ParameterSet(ex));
                }

            return testcases;
        }

        #endregion
    }
}
