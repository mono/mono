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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// Modes in which the tolerance value for a comparison can be interpreted.
    /// </summary>
    public enum ToleranceMode
    {
        /// <summary>
        /// The tolerance was created with a value, without specifying 
        /// how the value would be used. This is used to prevent setting
        /// the mode more than once and is generally changed to Linear
        /// upon execution of the test.
        /// </summary>
        None,
        /// <summary>
        /// The tolerance is used as a numeric range within which
        /// two compared values are considered to be equal.
        /// </summary>
        Linear,
        /// <summary>
        /// Interprets the tolerance as the percentage by which
        /// the two compared values my deviate from each other.
        /// </summary>
        Percent,
        /// <summary>
        /// Compares two values based in their distance in
        /// representable numbers.
        /// </summary>
        Ulps
    }
}