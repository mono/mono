//
// System.Collections.ReadOnlyCollectionBase.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace System.Collections {

#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
	public abstract class ReadOnlyCollectionBase : ICollection, IEnumerable {

		// private instance properties
		private ArrayList list;
		
		// public instance properties
		public
#if NET_2_0
		virtual
#endif		
		int Count { get { return InnerList.Count; } }
		
		// Public Instance Methods
		public
#if NET_2_0
		virtual
#endif		
		IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }
		
		// Protected Instance Constructors
		protected ReadOnlyCollectionBase() {
			this.list = new ArrayList();
		}
		
		// Protected Instance Properties
		protected ArrayList InnerList {get { return this.list; } }
		
#if NET_2_0
		// ICollection/IEnumerable methods
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
#endif

		void ICollection.CopyTo(Array array, int index) {
			lock (InnerList) { InnerList.CopyTo(array, index); }
		}
		object ICollection.SyncRoot {
				get { return InnerList.SyncRoot; }
			}
		bool ICollection.IsSynchronized {
			get { return InnerList.IsSynchronized; }
		}
	}
}
