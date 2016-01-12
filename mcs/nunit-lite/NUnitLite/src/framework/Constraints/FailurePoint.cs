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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// FailurePoint class represents one point of failure
    /// in an equality test.
    /// </summary>
    public class FailurePoint
    {
        /// <summary>
        /// The location of the failure
        /// </summary>
        public int Position;

        /// <summary>
        /// The expected value
        /// </summary>
        public object ExpectedValue;

        /// <summary>
        /// The actual value
        /// </summary>
        public object ActualValue;

        /// <summary>
        /// Indicates whether the expected value is valid
        /// </summary>
        public bool ExpectedHasData;

        /// <summary>
        /// Indicates whether the actual value is valid
        /// </summary>
        public bool ActualHasData;
    }

    /// <summary>
    /// FailurePointList represents a set of FailurePoints
    /// in a cross-platform way.
    /// </summary>
#if CLR_2_0 || CLR_4_0
    class FailurePointList : System.Collections.Generic.List<FailurePoint> { }
#else
    class FailurePointList : System.Collections.ArrayList { }
#endif

}
