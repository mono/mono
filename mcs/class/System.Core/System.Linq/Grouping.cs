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
//	Alejandro Serrano "Serras" (trupill@yahoo.es)
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	class Grouping<K, T> : IGrouping<K, T>
	{
		K key;
		IEnumerable<T> group;

		public Grouping (K key, IEnumerable<T> group)
		{
			this.group = group;
			this.key = key;
		}

		public K Key {
			get { return key; }
			set { key = value; }
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return group.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return group.GetEnumerator ();
		}
	}
}
