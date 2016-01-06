// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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

using System.Collections;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// CollectionTally counts (tallies) the number of
    /// occurences of each object in one or more enumerations.
    /// </summary>
    public class CollectionTally
    {
        // Internal list used to track occurences
        private readonly ObjectList list = new ObjectList();

        private readonly NUnitEqualityComparer comparer;

        /// <summary>
        /// Construct a CollectionTally object from a comparer and a collection
        /// </summary>
        public CollectionTally(NUnitEqualityComparer comparer, IEnumerable c)
        {
            this.comparer = comparer;

            foreach (object o in c)
                list.Add(o);
        }

        /// <summary>
        /// The number of objects remaining in the tally
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        private bool ItemsEqual(object expected, object actual)
        {
            Tolerance tolerance = Tolerance.Zero;
            return comparer.AreEqual(expected, actual, ref tolerance);
        }

        /// <summary>
        /// Try to remove an object from the tally
        /// </summary>
        /// <param name="o">The object to remove</param>
        /// <returns>True if successful, false if the object was not found</returns>
        public bool TryRemove(object o)
        {
            for (int index = 0; index < list.Count; index++)
                if (ItemsEqual(list[index], o))
                {
                    list.RemoveAt(index);
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Try to remove a set of objects from the tally
        /// </summary>
        /// <param name="c">The objects to remove</param>
        /// <returns>True if successful, false if any object was not found</returns>
        public bool TryRemove(IEnumerable c)
        {
            foreach (object o in c)
                if (!TryRemove(o))
                    return false;

            return true;
        }
    }
}