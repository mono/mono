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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class BaseCollection : MarshalByRefObject, ICollection, IEnumerable {
		internal ArrayList	list;

		#region Public Constructors
		public BaseCollection ()
		{
		}		 
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual int Count {
			get {
				return List.Count;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool IsReadOnly {
			get {
				return false;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool IsSynchronized {
			get {
				return false;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public object SyncRoot {
			get {
				return this;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected virtual ArrayList List {
			get {
				if (list == null)
					list = new ArrayList ();
				return list;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void CopyTo (Array ar, int index)
		{
			List.CopyTo (ar, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return List.GetEnumerator ();
		}
		#endregion	// Public Instance Methods
	}
}
