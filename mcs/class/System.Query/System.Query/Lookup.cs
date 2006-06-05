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
//
// Authors:
//        Alejandro Serrano "Serras" (trupill@yahoo.es)
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Query
{
        public class Lookup<K, T> : IEnumerable<IGrouping<K, T>>
        {
                Dictionary<K, IGrouping<K, T>> groups;
                
                internal Lookup (Dictionary<K, List<T>> groups)
                {
                        this.groups = new Dictionary<K, IGrouping<K, T>> ();
                        foreach (KeyValuePair<K, List<T>> group in groups)
                                this.groups.Add (group.Key, new Grouping<K, T>(group.Key, group.Value));
                }
                
                public int Count {
                        get { return groups.Count; }
                }
                
                public bool Contains (K key)
                {
                        return groups.ContainsKey (key);
                }
                
                public IEnumerator<IGrouping<K, T>> GetEnumerator ()
                {
                        return groups.Values.GetEnumerator ();
                }
                
                IEnumerator IEnumerable.GetEnumerator ()
                {
                        return groups.Values.GetEnumerator ();
                }
        }
}
