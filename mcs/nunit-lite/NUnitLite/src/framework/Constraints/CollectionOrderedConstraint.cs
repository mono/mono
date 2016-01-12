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
using System.Collections;
using System.Reflection;
using System.Text;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// CollectionOrderedConstraint is used to test whether a collection is ordered.
    /// </summary>
    public class CollectionOrderedConstraint : CollectionConstraint
    {
        private ComparisonAdapter comparer = ComparisonAdapter.Default;
        private string comparerName;
        private string propertyName;
        private bool descending;

        /// <summary>
        /// Construct a CollectionOrderedConstraint
        /// </summary>
        public CollectionOrderedConstraint()
        {
            this.DisplayName = "ordered";
        }

        ///<summary>
        /// If used performs a reverse comparison
        ///</summary>
        public CollectionOrderedConstraint Descending
        {
            get
            {
                descending = true;
                return this;
            }
        }

        /// <summary>
        /// Modifies the constraint to use an IComparer and returns self.
        /// </summary>
        public CollectionOrderedConstraint Using(IComparer comparer)
        {
            this.comparer = ComparisonAdapter.For(comparer);
            this.comparerName = comparer.GetType().FullName;
            return this;
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Modifies the constraint to use an IComparer&lt;T&gt; and returns self.
        /// </summary>
        public CollectionOrderedConstraint Using<T>(IComparer<T> comparer)
        {
            this.comparer = ComparisonAdapter.For(comparer);
            this.comparerName = comparer.GetType().FullName;
            return this;
        }

        /// <summary>
        /// Modifies the constraint to use a Comparison&lt;T&gt; and returns self.
        /// </summary>
        public CollectionOrderedConstraint Using<T>(Comparison<T> comparer)
        {
            this.comparer = ComparisonAdapter.For(comparer);
            this.comparerName = comparer.GetType().FullName;
            return this;
        }
#endif

        /// <summary>
        /// Modifies the constraint to test ordering by the value of
        /// a specified property and returns self.
        /// </summary>
        public CollectionOrderedConstraint By(string propertyName)
        {
            this.propertyName = propertyName;
            return this;
        }

        /// <summary>
        /// Test whether the collection is ordered
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        protected override bool doMatch(IEnumerable actual)
        {
            object previous = null;
            int index = 0;
            foreach (object obj in actual)
            {
                object objToCompare = obj;
                if (obj == null)
                    throw new ArgumentNullException("actual", "Null value at index " + index.ToString());

                if (this.propertyName != null)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(propertyName);
                    objToCompare = prop.GetValue(obj, null);
                    if (objToCompare == null)
                        throw new ArgumentNullException("actual", "Null property value at index " + index.ToString());
                }

                if (previous != null)
                {
                    //int comparisonResult = comparer.Compare(al[i], al[i + 1]);
                    int comparisonResult = comparer.Compare(previous, objToCompare);

                    if (descending && comparisonResult < 0)
                        return false;
                    if (!descending && comparisonResult > 0)
                        return false;
                }

                previous = objToCompare;
                index++;
            }

            return true;
        }

        /// <summary>
        /// Write a description of the constraint to a MessageWriter
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            if (propertyName == null)
                writer.Write("collection ordered");
            else
            {
                writer.WritePredicate("collection ordered by");
                writer.WriteExpectedValue(propertyName);
            }

            if (descending)
                writer.WriteModifier("descending");
        }

        /// <summary>
        /// Returns the string representation of the constraint.
        /// </summary>
        /// <returns></returns>
        protected override string GetStringRepresentation()
        {
            StringBuilder sb = new StringBuilder("<ordered");

            if (propertyName != null)
                sb.Append("by " + propertyName);
            if (descending)
                sb.Append(" descending");
            if (comparerName != null)
                sb.Append(" " + comparerName);

            sb.Append(">");

            return sb.ToString();
        }
    }
}