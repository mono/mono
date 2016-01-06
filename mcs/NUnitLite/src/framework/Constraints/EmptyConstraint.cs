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
	/// EmptyConstraint tests a whether a string or collection is empty,
	/// postponing the decision about which test is applied until the
	/// type of the actual argument is known.
	/// </summary>
	public class EmptyConstraint : Constraint
	{
		private Constraint RealConstraint
		{
			get 
			{
                if (actual is string)
                    return new EmptyStringConstraint();
                else if (actual is System.IO.DirectoryInfo)
                    return new EmptyDirectoryConstraint();
                else
                    return new EmptyCollectionConstraint();
			}
		}
		
		/// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
		public override bool Matches(object actual)
		{
			this.actual = actual;

            if (actual == null)
                throw new ArgumentException("The actual value must be a non-null string, IEnumerable or DirectoryInfo", "actual");

			return this.RealConstraint.Matches( actual );
		}

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
		public override void WriteDescriptionTo(MessageWriter writer)
		{
			this.RealConstraint.WriteDescriptionTo( writer );
		}
	}
}
