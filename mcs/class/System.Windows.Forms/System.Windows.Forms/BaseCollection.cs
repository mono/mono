//
// System.Windows.Forms.BaseCollection
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
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

namespace System.Windows.Forms {

	/// <summary>
	/// Provides the base functionality for creating data-related collections in the System.Windows.Forms namespace.
	/// ToDo note:
	///  - Synchronization is not implemented
	///  - MarshalByRefObject members not stubbed out
	/// </summary>
	
	public class BaseCollection : MarshalByRefObject, ICollection, IEnumerable {

		ArrayList list;

		// --- Constructor ---
		public BaseCollection()
		{
			this.list = null;
		}

		// --- public and protected Properties ---
//		public virtual int ICollection.Count {
		public virtual int Count {
			get {
				return list.Count; 
			}
		}
		
		public bool IsReadOnly {
			//always false as per spec.
			get { return false; }
		}
		
		public bool IsSynchronized {
			//always false as per spec.
			get { return false; }
		}
		
		protected virtual ArrayList List {
			get { 
				return list; 
			}
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		// --- public Methods ---
		public void CopyTo (Array ar, int index) 
		{
			list.CopyTo(ar, index);
		}
		
		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator();
		}
		
	}
}
