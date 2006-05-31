//
// System.ComponentModel.Design.DesignerCollection.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel.Design
{
	public class DesignerCollection : ICollection, IEnumerable
	{

		private ArrayList designers;

		public DesignerCollection (IDesignerHost[] designers)
		{
			this.designers = new ArrayList (designers);
		}

		public DesignerCollection (IList designers)
		{
			this.designers = new ArrayList (designers);
		}

		int ICollection.Count {
			get { return Count; }
		}

		public int Count {
			get { return designers.Count; }
		}

		public virtual IDesignerHost this [int index] {
			get { return (IDesignerHost) designers [index]; }
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		public IEnumerator GetEnumerator()
		{
			return designers.GetEnumerator ();
		}

		bool ICollection.IsSynchronized {
			get { return designers.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return designers.SyncRoot; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			designers.CopyTo (array, index);
		}
	}
}
