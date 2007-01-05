// 
// BasicProfileViolationEnumerator.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

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

#if NET_2_0

using System.Collections;
using System.Collections.Generic;

namespace System.Web.Services.Description
{
	public class BasicProfileViolationEnumerator : IEnumerator<BasicProfileViolation>, IDisposable
	{
		BasicProfileViolationCollection collection;
		int current = -1;
		int generation;

		public BasicProfileViolationEnumerator (BasicProfileViolationCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			this.collection = collection;
			generation = collection.Generation;
		}

		public void Dispose ()
		{
			collection = null;
		}

		public bool MoveNext ()
		{
			if (generation != collection.Generation)
				throw new InvalidOperationException ("Collection has changed during the enumeration.");
			if (current + 1 == collection.Count)
				return false;
			current++;
			return true;
		}

		public BasicProfileViolation Current {
			get { return current < 0 ? null : collection [current]; }
		}

		object IEnumerator.Current {
			get { return current < 0 ? null : collection [current]; }
		}

		void IEnumerator.Reset ()
		{
			current = -1;
		}
	}
}
#endif
