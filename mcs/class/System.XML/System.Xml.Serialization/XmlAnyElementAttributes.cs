//
// XmlAnyElementAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
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
using System.Collections.Generic;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAnyElementAttributes.
	/// </summary>
#if MOONLIGHT
	public class XmlAnyElementAttributes : IList {

		private List<XmlAnyElementAttribute> List = new List<XmlAnyElementAttribute> ();

		int IList.Add (object value)
		{
			return (List as IList).Add (value);
		}

		void IList.Clear ()
		{
			List.Clear ();
		}

		bool IList.Contains (object value)
		{
			return (List as IList).Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return (List as IList).IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			(List as IList).Insert (index, value);
		}

		bool IList.IsFixedSize {
			get { return (List as IList).IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return (List as IList).IsReadOnly; }
		}

		void IList.Remove (object value)
		{
			(List as IList).Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			List.RemoveAt (index);
		}

		object IList.this [int index] {
			get { return (List as IList) [index]; }
			set { (List as IList) [index] = value; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			(List as ICollection).CopyTo (array, index);
		}

		public int Count {
			get { return List.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return (List as ICollection).IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return (List as ICollection).SyncRoot; }
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (List as IEnumerable).GetEnumerator ();
		}
#else
	public class XmlAnyElementAttributes : CollectionBase {
#endif
		
		public XmlAnyElementAttribute this[int index] 
		{
			get 
			{
				return (XmlAnyElementAttribute)List[index];
			}
			set 
			{
				List[index] = value;
			}	
		}

		public int Add(XmlAnyElementAttribute attribute)
		{
			return (List as IList).Add (attribute);
		}

		public bool Contains(XmlAnyElementAttribute attribute)
		{
			return List.Contains(attribute);	
		}

		public int IndexOf(XmlAnyElementAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlAnyElementAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlAnyElementAttribute attribute)
		{
			List.Remove(attribute);
		}

		public void CopyTo(XmlAnyElementAttribute[] array,int index)
		{
			List.CopyTo(array, index);
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			if (Count == 0) return;
			
			sb.Append ("XAEAS ");
			for (int n=0; n<Count; n++)
				this[n].AddKeyHash (sb);
			sb.Append ('|');
		}
	}
}
