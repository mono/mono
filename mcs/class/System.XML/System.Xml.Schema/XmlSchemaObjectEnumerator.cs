// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObjectEnumerator.
	/// </summary>
	public sealed class XmlSchemaObjectEnumerator : IEnumerator
	{
		private IEnumerator ienum;
		internal XmlSchemaObjectEnumerator(IList list)
		{
			this.ienum = list.GetEnumerator();
		}
		// Properties
		public XmlSchemaObject Current
		{ 
			get
			{
				return (XmlSchemaObject) ienum.Current; 
			}
		}
		// Methods
		public bool MoveNext()
		{
			return ienum.MoveNext();
		}
		public void Reset()
		{
			ienum.Reset();
		}
		//Explicit Interface implementation
		bool IEnumerator.MoveNext()
		{
			return ienum.MoveNext();
		}
		void IEnumerator.Reset()
		{
			ienum.Reset();
		}
		object IEnumerator.Current
		{
			get{return (XmlSchemaObject) ienum.Current;}
		}
	}
}
