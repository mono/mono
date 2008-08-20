//
// SyndicationElementExtensionCollection.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public sealed class SyndicationElementExtensionCollection : Collection<SyndicationElementExtension>
	{
		internal SyndicationElementExtensionCollection ()
		{
		}

		internal SyndicationElementExtensionCollection (IEnumerable<SyndicationElementExtension> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			foreach (SyndicationElementExtension item in source)
				Add (item);
		}

		public void Add (object extension)
		{
			Add (new SyndicationElementExtension (extension));
		}

		public void Add (object dataContractExtension, DataContractSerializer serializer)
		{
			Add (new SyndicationElementExtension (dataContractExtension, serializer));
		}

		public void Add (object xmlSerializerExtension, XmlSerializer serializer)
		{
			Add (new SyndicationElementExtension (xmlSerializerExtension, serializer));
		}

		public void Add (string outerName, string outerNamespace, object dataContractExtension)
		{
			Add (new SyndicationElementExtension (outerName, outerNamespace, dataContractExtension));
		}

		public void Add (string outerName, string outerNamespace, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
		{
			Add (new SyndicationElementExtension (outerName, outerNamespace, dataContractExtension, dataContractSerializer));
		}

		public void Add (XmlReader xmlReader)
		{
			Add (new SyndicationElementExtension (xmlReader));
		}

		new void Add (SyndicationElementExtension item)
		{
			base.Add (item);
		}

		[MonoTODO ("Find out what is expected here")]
		protected override void ClearItems ()
		{
			base.ClearItems ();
		}

		[MonoTODO]
		public XmlReader GetReaderAtElementExtensions ()
		{
			throw new NotImplementedException ();
		}

		protected override void InsertItem (int index, SyndicationElementExtension item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			base.InsertItem (index, item);
		}

		public Collection<TExtension> ReadElementExtensions<TExtension> (string extensionName, string extensionNamespace)
		{
			Collection<TExtension> c = new Collection<TExtension> ();
			foreach (SyndicationElementExtension e in this)
				if (e.OuterName == extensionName && e.OuterNamespace == extensionNamespace)
					c.Add (e.GetObject<TExtension> ());
			return c;
		}

		public Collection<TExtension> ReadElementExtensions<TExtension> (string extensionName, string extensionNamespace, XmlObjectSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			Collection<TExtension> c = new Collection<TExtension> ();
			foreach (SyndicationElementExtension e in this)
				if (e.OuterName == extensionName && e.OuterNamespace == extensionNamespace)
					c.Add (e.GetObject<TExtension> (serializer));
			return c;
		}

		public Collection<TExtension> ReadElementExtensions<TExtension> (string extensionName, string extensionNamespace, XmlSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			Collection<TExtension> c = new Collection<TExtension> ();
			foreach (SyndicationElementExtension e in this)
				if (e.OuterName == extensionName && e.OuterNamespace == extensionNamespace)
					c.Add (e.GetObject<TExtension> (serializer));
			return c;
		}

		[MonoTODO ("Find out what is expected here")]
		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, SyndicationElementExtension item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			base.SetItem (index, item);
		}
	}
}
