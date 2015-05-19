//
// System.Windows.Forms.Design.Behavior.BehaviorServiceAdornerCollectionEnumerator
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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


using System.Collections;

namespace System.Windows.Forms.Design.Behavior
{
	public class BehaviorServiceAdornerCollectionEnumerator : IEnumerator
	{
		BehaviorServiceAdornerCollection mappings;
		int index, state;

		public BehaviorServiceAdornerCollectionEnumerator (BehaviorServiceAdornerCollection mappings)
		{
			if (mappings == null)
				throw new ArgumentNullException ("mappings");
			this.mappings = mappings;

			Reset ();
		}

		public Adorner Current {
			get { return index < 0 ? null : mappings [index]; }
		}

		void CheckState ()
		{
			if (mappings.State != state)
				throw new InvalidOperationException ("Collection has changed");
		}

		public bool MoveNext ()
		{
			CheckState ();
			if (index++ < mappings.Count)
				return true;
			index--;
			return false;
		}

		public void Reset ()
		{
			index = -1;
		}

		object IEnumerator.Current {
			get { return Current; }
		}

		bool IEnumerator.MoveNext ()
		{
			return MoveNext ();
		}

		void IEnumerator.Reset ()
		{
			Reset ();
		}
	}
}

