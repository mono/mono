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
		private IDictionaryEnumerator xenum;
		internal XmlSchemaObjectEnumerator(Hashtable htable)
		{
			this.xenum = htable.GetEnumerator();
		}
		// Properties
		public XmlSchemaObject Current
		{ 
			get
			{
				return (XmlSchema) xenum.Current; 
			}
		}
		// Methods
		public bool MoveNext()
		{
			return xenum.MoveNext();
		}
		public void Reset()
		{
			xenum.Reset();
		}
		//Explicit Interface implementation
		bool IEnumerator.MoveNext()
		{
			return xenum.MoveNext();
		}
		void IEnumerator.Reset()
		{
			xenum.Reset();
		}
		object IEnumerator.Current
		{
			get{return (XmlSchema) xenum.Current;}
		}
	}
}
