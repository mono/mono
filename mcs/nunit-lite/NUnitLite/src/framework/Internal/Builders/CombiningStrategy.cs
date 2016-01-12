﻿// ***********************************************************************
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
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Extensibility;
using NUnit.Framework.Api;

namespace NUnit.Framework.Builders
{
    /// <summary>
    /// CombiningStrategy is the abstract base for classes that
    /// know how to combine values provided for individual test
    /// parameters to create a set of test cases.
    /// </summary>
    public abstract class CombiningStrategy
    {
        private IEnumerable[] sources;
        private IEnumerator[] enumerators;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombiningStrategy"/> 
        /// class using a set of parameter sources.
        /// </summary>
        /// <param name="sources">The sources.</param>
        public CombiningStrategy(IEnumerable[] sources)
        {
            this.sources = sources;
        }

        /// <summary>
        /// Gets the sources used by this strategy.
        /// </summary>
        /// <value>The sources.</value>
        public IEnumerable[] Sources
        {
            get { return sources; }
        }

        /// <summary>
        /// Gets the enumerators for the sources.
        /// </summary>
        /// <value>The enumerators.</value>
        public IEnumerator[] Enumerators
        {
            get
            {
                if (enumerators == null)
                {
                    enumerators = new IEnumerator[Sources.Length];
                    for (int i = 0; i < Sources.Length; i++)
                        enumerators[i] = Sources[i].GetEnumerator();
                }

                return enumerators;
            }
        }

        /// <summary>
        /// Gets the test cases generated by the CombiningStrategy.
        /// </summary>
        /// <returns>The test cases.</returns>
#if CLR_2_0 || CLR_4_0
        public abstract System.Collections.Generic.IEnumerable<ITestCaseData> GetTestCases();
#else
        public abstract System.Collections.IEnumerable GetTestCases();
#endif
    }
}
