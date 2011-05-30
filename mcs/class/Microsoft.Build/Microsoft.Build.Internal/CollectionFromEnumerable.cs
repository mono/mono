//
// CollectionFromEnumerable.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//
// (C) 2011 Leszek Ciesielski
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
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Build.Internal
{
        internal class CollectionFromEnumerable<T> : ICollection<T>
        {
                IEnumerable<T> backingEnumerable;

                public CollectionFromEnumerable (IEnumerable<T> enumerable)
                {
                        backingEnumerable = enumerable;
                }

                public void Add (T item)
                {
                        throw new InvalidOperationException ("This collection is read-only.");
                }

                public void Clear ()
                {
                        throw new InvalidOperationException ("This collection is read-only.");
                }

                public bool Contains (T item)
                {
                        List<T> backingList = backingEnumerable as List<T>;
                        if (backingList != null)
                                return backingList.Contains (item);
                        return backingEnumerable.Contains (item);
                }

                public void CopyTo (T[] array, int arrayIndex)
                {
                        List<T> backingList = backingEnumerable as List<T>;
                        if (backingList != null) {
                                backingList.CopyTo (array, arrayIndex);
                                return;
                        }
                        int i = arrayIndex;
                        foreach (var item in backingEnumerable) {
                                array[i++] = item;
                        }
                }

                public int Count {
                        get {
                                var backingList = backingEnumerable as List<T>;
                                if(backingList == null)
                                        backingEnumerable = backingList = new List<T> (backingEnumerable);
                                return backingList.Count;
                        }
                }

                public bool IsReadOnly {
                        get { return true; }
                }

                public bool Remove (T item)
                {
                        throw new InvalidOperationException ("This collection is read-only.");
                }

                public IEnumerator<T> GetEnumerator ()
                {
                        return backingEnumerable.GetEnumerator ();
                }

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
                {
                        return backingEnumerable.GetEnumerator ();
                }
        }
}
