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
    /// AssignableFromConstraint is used to test that an object
    /// can be assigned from a given Type.
    /// </summary>
    public class AssignableFromConstraint : TypeConstraint
    {
        /// <summary>
        /// Construct an AssignableFromConstraint for the type provided
        /// </summary>
        /// <param name="type"></param>
        public AssignableFromConstraint(Type type) : base(type) { }

        /// <summary>
        /// Test whether an object can be assigned from the specified type
        /// </summary>
        /// <param name="actual">The object to be tested</param>
        /// <returns>True if the object can be assigned a value of the expected Type, otherwise false.</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            return actual != null && actual.GetType().IsAssignableFrom(expectedType);
        }

        /// <summary>
        /// Write a description of this constraint to a MessageWriter
        /// </summary>
        /// <param name="writer">The MessageWriter to use</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("assignable from");
            writer.WriteExpectedValue(expectedType);
        }
    }
}