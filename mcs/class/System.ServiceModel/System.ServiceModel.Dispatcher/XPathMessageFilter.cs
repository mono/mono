//
// XPathMessageFilter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace System.ServiceModel.Dispatcher
{
	[MonoTODO]
	[XmlRoot ("XPathMessageFilter", Namespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter")]
	[XmlSchemaProvider ("StaticGetSchema")]
	public class XPathMessageFilter : MessageFilter, IXmlSerializable
	{
		public static XmlSchemaType StaticGetSchema (XmlSchemaSet schemas)
		{
			throw new NotImplementedException ();
		}

		XmlNamespaceManager namespaces;
		int node_quota;
		string xpath;
		XPathExpression expr;

		public XPathMessageFilter ()
		{
		}

		public XPathMessageFilter (string xpath)
		{
			Initialize (xpath, null);
		}

		public XPathMessageFilter (string xpath, XmlNamespaceManager namespaces)
		{
			Initialize (xpath, namespaces);
		}

		public XPathMessageFilter (string xpath, XsltContext context)
		{
			Initialize (xpath, context);
		}

		[MonoTODO]
		public XPathMessageFilter (XmlReader reader)
			: this (reader, (XmlNamespaceManager) null)
		{
		}

		[MonoTODO]
		public XPathMessageFilter (XmlReader reader, XmlNamespaceManager namespaces)
		{
			Initialize (reader.ReadString (), namespaces);
		}

		[MonoTODO]
		public XPathMessageFilter (XmlReader reader, XsltContext context)
		{
			Initialize (reader.ReadString (), context);
		}

		private void Initialize (string xpath, XmlNamespaceManager nsmgr)
		{
			this.xpath = xpath;
			namespaces = nsmgr;
		}

		public XmlNamespaceManager Namespaces {
			get { return namespaces; }
		}

		public int NodeQuota {
			get { return node_quota; }
			set { node_quota = value; }
		}

		public string XPath {
			get { return xpath; }
		}

		protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData> ()
		{
			throw new NotImplementedException ();
		}

		public override bool Match (Message message)
		{
			throw new NotImplementedException ();
		}

		public override bool Match (MessageBuffer messageBuffer)
		{
			throw new NotImplementedException ();
		}

		public bool Match (SeekableXPathNavigator navigator)
		{
			throw new NotImplementedException ();
		}

		public bool Match (XPathNavigator navigator)
		{
			throw new NotImplementedException ();
		}

		public void TrimToSize ()
		{
			expr = null;
			throw new NotImplementedException ();
		}

		public void WriteXPathTo (XmlWriter writer,
			string prefix, string localName, string ns,
			bool writeNamespaces)
		{
			throw new NotImplementedException ();
		}

		protected virtual XmlSchema OnGetSchema ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnWriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		protected void ReadXPath (XmlReader reader,
			XmlNamespaceManager namespaces)
		{
			throw new NotImplementedException ();
		}

		protected void WriteXPath (XmlWriter writer,
			IXmlNamespaceResolver resolver)
		{
			throw new NotImplementedException ();
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}
	}
}