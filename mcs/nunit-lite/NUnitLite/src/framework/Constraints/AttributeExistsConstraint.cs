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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// AttributeExistsConstraint tests for the presence of a
    /// specified attribute on  a Type.
    /// </summary>
    public class AttributeExistsConstraint : Constraint
    {
        private Type expectedType;

        /// <summary>
        /// Constructs an AttributeExistsConstraint for a specific attribute Type
        /// </summary>
        /// <param name="type"></param>
        public AttributeExistsConstraint(Type type)
            : base(type)
        {
            this.expectedType = type;

            if (!typeof(Attribute).IsAssignableFrom(expectedType))
                throw new ArgumentException(string.Format(
                    "Type {0} is not an attribute", expectedType), "type");
        }

        /// <summary>
        /// Tests whether the object provides the expected attribute.
        /// </summary>
        /// <param name="actual">A Type, MethodInfo, or other ICustomAttributeProvider</param>
        /// <returns>True if the expected attribute is present, otherwise false</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            System.Reflection.ICustomAttributeProvider attrProvider =
                actual as System.Reflection.ICustomAttributeProvider;

            if (attrProvider == null)
                throw new ArgumentException(string.Format("Actual value {0} does not implement ICustomAttributeProvider", actual), "actual");

            return attrProvider.GetCustomAttributes(expectedType, true).Length > 0;
        }

        /// <summary>
        /// Writes the description of the constraint to the specified writer
        /// </summary>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("type with attribute");
            writer.WriteExpectedValue(expectedType);
        }
    }
}