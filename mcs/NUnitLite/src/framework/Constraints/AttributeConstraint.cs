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
    /// AttributeConstraint tests that a specified attribute is present
    /// on a Type or other provider and that the value of the attribute
    /// satisfies some other constraint.
    /// </summary>
    public class AttributeConstraint : PrefixConstraint
    {
        private readonly Type expectedType;
        private Attribute attrFound;

        /// <summary>
        /// Constructs an AttributeConstraint for a specified attriute
        /// Type and base constraint.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseConstraint"></param>
        public AttributeConstraint(Type type, Constraint baseConstraint)
            : base(baseConstraint)
        {
            this.expectedType = type;

            if (!typeof(Attribute).IsAssignableFrom(expectedType))
                throw new ArgumentException(string.Format(
                    "Type {0} is not an attribute", expectedType), "type");
        }

        /// <summary>
        /// Determines whether the Type or other provider has the 
        /// expected attribute and if its value matches the
        /// additional constraint specified.
        /// </summary>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            System.Reflection.ICustomAttributeProvider attrProvider =
                actual as System.Reflection.ICustomAttributeProvider;

            if (attrProvider == null)
                throw new ArgumentException(string.Format("Actual value {0} does not implement ICustomAttributeProvider", actual), "actual");

            Attribute[] attrs = (Attribute[])attrProvider.GetCustomAttributes(expectedType, true);
            if (attrs.Length == 0)
                throw new ArgumentException(string.Format("Attribute {0} was not found", expectedType), "actual");

            attrFound = attrs[0];
            return baseConstraint.Matches(attrFound);
        }

        /// <summary>
        /// Writes a description of the attribute to the specified writer.
        /// </summary>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("attribute " + expectedType.FullName);
            if (baseConstraint != null)
            {
                if (baseConstraint is EqualConstraint)
                    writer.WritePredicate("equal to");
                baseConstraint.WriteDescriptionTo(writer);
            }
        }

        /// <summary>
        /// Writes the actual value supplied to the specified writer.
        /// </summary>
        public override void WriteActualValueTo(MessageWriter writer)
        {
            writer.WriteActualValue(attrFound);
        }

        /// <summary>
        /// Returns a string representation of the constraint.
        /// </summary>
        protected override string GetStringRepresentation()
        {
            return string.Format("<attribute {0} {1}>", expectedType, baseConstraint);
        }
    }
}