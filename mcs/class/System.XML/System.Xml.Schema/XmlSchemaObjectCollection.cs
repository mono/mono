// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObjectCollection.
	/// </summary>
	public class XmlSchemaObjectCollection : System.Collections.CollectionBase
	{
		private XmlSchemaObject parent;

		public XmlSchemaObjectCollection()
		{
		}
		public XmlSchemaObjectCollection(XmlSchemaObject parent)
		{
			this.parent = parent;
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
