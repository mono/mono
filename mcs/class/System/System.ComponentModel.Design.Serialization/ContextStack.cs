//
// System.ComponentModel.Design.Serialization.ContextStack.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@gnome.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ivan N. Zlatev (contact@i-nz.net)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// (C) 2007 Ivan N. Zlatev
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

namespace System.ComponentModel.Design.Serialization
{
	public sealed class ContextStack
	{
		private ArrayList _contextList;

		public ContextStack () 
		{
			_contextList = new ArrayList ();
		}

		public object Current {
			get {
				int context_count = _contextList.Count;
				if (context_count > 0)
					return _contextList [context_count - 1];
				return null;
			}
		}

		public object this[Type type] {
			get {
				if (type == null)
					throw new ArgumentNullException ("type");
				for (int i = _contextList.Count - 1; i >= 0; i--) {
					object context = _contextList [i];
					if (type.IsInstanceOfType (context))
						return context;
				}
				return null;
			}
		}

		public object this[int level] {
			get {
				if (level < 0)
					throw new ArgumentOutOfRangeException ("level");
				int context_count = _contextList.Count;
				if (context_count > 0 && context_count > level)
					return _contextList [context_count - 1 - level];
				return null;
			}
		}

		public object Pop ()
		{
			object o = null;
			int context_count = _contextList.Count;
			if (context_count > 0) {
				int lastItem = context_count - 1;
				o = _contextList [lastItem];
				_contextList.RemoveAt (lastItem);
			}
			return o;
		}

		public void Push (object context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			_contextList.Add (context);
		}

		public void Append (object context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			_contextList.Insert (0, context);
		}
	}
}

