// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObjectCollection.
	/// </summary>
	public class XmlSchemaObjectCollection : System.Collections.CollectionBase
	{
//		private XmlSchemaObject parent;

		public XmlSchemaObjectCollection()
		{
		}

		public XmlSchemaObjectCollection(XmlSchemaObject parent)
		{
			// FIXME: how is it used publicly?
//			this.parent = parent;
		}

		// Properties
		public virtual XmlSchemaObject this[ int index ] 
		{  
			get
			{
				return (XmlSchemaObject) this.List[index];
			} 
			set
			{
				this.List[index] = value;
			}
		}

		// Methods
		public int  Add(XmlSchemaObject item)
		{
			return this.List.Add(item);
		}
		
		public bool Contains(XmlSchemaObject item)
		{
			return this.List.Contains(item);
		}
		
		public void CopyTo(XmlSchemaObject[] array, int index)
		{
			this.List.CopyTo(array,index);
		}
		
		public new XmlSchemaObjectEnumerator GetEnumerator ()
		{
			return new XmlSchemaObjectEnumerator(this.List);
		}
		
		public int IndexOf(XmlSchemaObject item)
		{
			return this.List.IndexOf(item);
		}
		
		public void Insert(int index, XmlSchemaObject item)
		{
			this.List.Insert(index, item);
		}
		
		protected override void OnClear()
		{
		}

		protected override void OnInsert(int index,object item)
		{
		}

		protected override void OnRemove(int index,object item)
		{
		}

		protected override void OnSet(int index,object oldValue,object newValue)
		{
		}

		public void Remove(XmlSchemaObject item)
		{
			this.List.Remove(item);
		}
	}
}
