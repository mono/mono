//
// XmlDataSourceResolver.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2003 Novell inc.
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
#if NET_2_0

using System;
using System.Collections;
using System.Data;
using System.Data.SqlXml;
using System.Net;

namespace System.Xml
{
	public class XmlDataSourceResolver : XmlResolver
	{
		XmlNameTable nameTable;
		Hashtable table;

		public XmlDataSourceResolver ()
			: this (new NameTable ())
		{
		}

		public XmlDataSourceResolver (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;
			table = new Hashtable ();
		}

		public virtual int Count {
			get { return table.Count; }
		}

		public ICredentials Credentials {
			set { throw new NotImplementedException (); }
		}

		public virtual object this [string query] {
			get { return table [new Uri (query, true, true)]; }
		}

		public virtual void Add (string name, IDbConnection dbConnection)
		{
			table.Add (new Uri (name), dbConnection);
		}

		public virtual void Add (string name, IDbTransaction dbTransaction)
		{
			table.Add (new Uri (name), dbTransaction);
		}

		public virtual void Add (string name, string sourceUri)
		{
			table.Add (new Uri (name), sourceUri);
		}

		public virtual void Add (string name, XmlReader documentReader)
		{
			table.Add (new Uri (name), documentReader);
		}

		public virtual void Add (string name, XPathNavigator2 document)
		{
			table.Add (new Uri (name), document);
		}

		public virtual void Clear ()
		{
			table.Clear ();
		}

		public virtual bool Contains (string name)
		{
			return table.ContainsKey (new Uri (name, true, true));
		}

		public override object GetEntity (Uri absoluteUri,
			string role,
			Type ofObjectToReturn)
		{
			if (absoluteUri == null)
				throw new ArgumentNullException ("absoluteUri");

			if (ofObjectToReturn == null)
				throw new ArgumentNullException ("ofObjectToReturn");

			object o = table [absoluteUri];
			if (o == null)
				return null;

			Type type = o.GetType ();
			if (type == ofObjectToReturn)
				return o;
			else if (type.IsSubClassOf (ofObjectToReturn))
				return o;

			switch (ofObjectToReturn.FullName) {
			case "System.Data.IDbConnection":
				throw new NotImplementedException ();
			case "System.Xml.XPathNavigator2":
				return GetXPathNavigator (o);
			case "System.Array": // array of IXPathNavigable
				throw new NotImplementedException ();
			default:
				throw new NotSupportedException ();
			}
		}

		private XPathNavigator2 GetXPathNavigator (object o)
		{
			if (o is string)
				return new XPathDocument2 (new XmlTextReader (o as string)).CreateNavigator ();
			else if (o is XmlReader)
				return new XPathDocument2 (o as XmlReader).CreateNavigator ();
			else
				throw new NotImplementedException ();
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return table.GetEnumerator ();
		}

		public void Remove (string name)
		{
			table.Remove (new Uri (name, true, true));
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			// XmlDataSourceResolver has no concept of base URIs.
			
			// Note that this constructor uses new .NET 1.2 feature.
			return new Uri (relativeUri, true, true)
		}
	}
}

#endif
