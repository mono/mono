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
		public XmlSchemaObjectCollection()
		{
		}
		[MonoTODO]
		public XmlSchemaObjectCollection(XmlSchemaObject parent)
		{
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
		public int Add(System.Xml.Schema.XmlSchemaObject item)
		{
			return this.List.Add(item);
		}
		
		public bool Contains(System.Xml.Schema.XmlSchemaObject item)
		{
			return this.List.Contains(item);
		}
		
		public void CopyTo(System.Xml.Schema.XmlSchemaObject[] array, int index)
		{
			this.List.CopyTo(array,index);
		}
		
		[MonoTODO]
		public new XmlSchemaObjectEnumerator GetEnumerator()
		{
			return (XmlSchemaObjectEnumerator) new object();
		}
		
		public int IndexOf(System.Xml.Schema.XmlSchemaObject item)
		{
			return this.List.IndexOf(item);
		}
		
		public void Insert(int index, System.Xml.Schema.XmlSchemaObject item)
		{
			this.List.Insert(index, item);
		}
		
		[MonoTODO]
		protected override void OnClear(){}
		[MonoTODO]
		protected override void OnInsert(int index,object item){}
		[MonoTODO]
		protected override void OnRemove(int index,object item){}
		[MonoTODO]
		protected override void OnSet(int index,object oldValue,object newValue){}

		public void Remove(System.Xml.Schema.XmlSchemaObject item)
		{
			this.List.Remove(item);
		}
	}
}
