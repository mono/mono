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
using System.Collections.Specialized;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObjectTable.
	/// </summary>
	public class XmlSchemaObjectTable
	{
		private ListDictionary table;

		internal XmlSchemaObjectTable ()
		{
			table = new ListDictionary (); 
		}
		public int Count 
		{
			get{ return table.Count; }
		}
		public XmlSchemaObject this [XmlQualifiedName name] 
		{
			get{ return (XmlSchemaObject) table [name]; }
		}
		public ICollection Names 
		{
			get{ return table.Keys; }
		}
		public ICollection Values 
		{
			get{ return table.Values; }
		}

		public bool Contains (XmlQualifiedName name)
		{
			return table.Contains (name);
		}
		public IDictionaryEnumerator GetEnumerator ()
		{
			return new XmlSchemaObjectTableEnumerator (this);
		}

		internal void Add (XmlQualifiedName name, XmlSchemaObject value)
		{
			table [name] = value;
		}

		internal void Clear ()
		{
			table.Clear ();
		}

		internal void Set (XmlQualifiedName name, XmlSchemaObject value)
		{
			table [name] = value;
		}

		internal class XmlSchemaObjectTableEnumerator : IDictionaryEnumerator
		{
			private IDictionaryEnumerator xenum;
			IEnumerable tmp;
			internal XmlSchemaObjectTableEnumerator (XmlSchemaObjectTable table)
			{
				tmp = (IEnumerable) table.table;
				xenum = (IDictionaryEnumerator) tmp.GetEnumerator ();
			}
			// Properties
			public XmlSchemaObject Current { 
				get {
					return (XmlSchemaObject) xenum.Value; 
				}
			}
			public DictionaryEntry Entry {
				get { return xenum.Entry; }
			}
			public XmlQualifiedName Key {
				get { return (XmlQualifiedName) xenum.Key; }
			}
			public XmlSchemaObject Value {
				get { return (XmlSchemaObject) xenum.Value; }
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
				get { return xenum.Entry; }
			}
			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return xenum.Entry; }
			}
			object IDictionaryEnumerator.Key {
				get { return (XmlQualifiedName) xenum.Key; }
			}
			object IDictionaryEnumerator.Value {
				get { return (XmlSchemaObject) xenum.Value; }
			}
		}
	}
}
