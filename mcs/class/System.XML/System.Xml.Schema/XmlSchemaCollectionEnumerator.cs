// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaCollectionEnumerator.
	/// </summary>
	public sealed class XmlSchemaCollectionEnumerator : IEnumerator
	{
		private IDictionaryEnumerator xenum;
		internal XmlSchemaCollectionEnumerator(Hashtable htable)
		{
			this.xenum = htable.GetEnumerator();
		}
		// Properties
		public XmlSchema Current 
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
