// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// NullEmptyStringConstraint tests whether a string is either null or empty.
    /// </summary>
    public class NullOrEmptyStringConstraint : Constraint
    {
        /// <summary>
        /// Constructs a new NullOrEmptyStringConstraint
        /// </summary>
        public NullOrEmptyStringConstraint()
        {
            this.DisplayName = "nullorempty";
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            // NOTE: Do not change this to use string.IsNullOrEmpty
            // since that won't work in earlier versions of .NET

            this.actual = actual;

            if (actual == null) return true;

            string actualAsString = actual as string;

            if (actualAsString == null)
                throw new ArgumentException("Actual value must be a string", "actual");

            return actualAsString == string.Empty;
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write("null or empty string");
        }
    }
}