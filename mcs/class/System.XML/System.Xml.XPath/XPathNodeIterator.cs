//
// System.Xml.XPath.XPathNodeIterator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
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

using System;
using System.Collections;

namespace System.Xml.XPath
{
	public abstract class XPathNodeIterator : ICloneable, IEnumerable
	{
		private int _count = -1;

		#region Constructor

		protected XPathNodeIterator ()
		{
		}

		#endregion

		#region Properties

		public virtual int Count
		{
			get
			{
				if (_count == -1)
				{
					// compute and cache the count
					XPathNodeIterator tmp = Clone ();
					while (tmp.MoveNext ())
						;
					_count = tmp.CurrentPosition;
				}
				return _count;
			}
		}

		public abstract XPathNavigator Current { get; }

		public abstract int CurrentPosition { get; }

		#endregion

		#region Methods

		public abstract XPathNodeIterator Clone ();

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return new XPathNodeIteratorEnumerator (this);
		}

		public abstract bool MoveNext ();
		
		#endregion

		struct XPathNodeIteratorEnumerator : IEnumerator
		{
			XPathNodeIterator source;
			XPathNavigator current;
			public XPathNodeIteratorEnumerator (XPathNodeIterator source)
			{
				this.source = source.Clone ();
				current = null;
			}

			public bool MoveNext ()
			{
				if (!source.MoveNext ())
					return false;
				current = source.Current.Clone ();
				return true;
			}
			
			public object Current {
				get { return current; }
			}

			public void Reset ()
			{
				throw new NotSupportedException ();
			}
		}
	}
}
