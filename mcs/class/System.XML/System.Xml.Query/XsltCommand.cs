//
// System.Xml.Query.XsltCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Atsushi Enomoto, 2004
//

#if NET_2_0

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Query
{
	public class XsltCommand
	{
		[MonoTODO]
		public XsltCommand ()
		{
		}

		public event QueryEventHandler OnProcessingEvent;

		// Compile

		[MonoTODO]
		public void Compile (string stylesheetUri, XmlResolver resolver)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Compile (string stylesheetUri)
		{ 
			throw new NotImplementedException ();
		}

		// Execute

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlWriter results)
		{
			Execute (contextDocument, null, null, results);
		}

		public void Execute (string contextDocumentUri, string resultDocumentUri)
		{
			XmlTextWriter xw = new XmlTextWriter (resultDocumentUri, null);
			try {
				Execute (new XPathDocument (contextDocumentUri), xw);
			} finally {
				xw.Close ();
			}
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlArgumentList argList,
			XmlWriter results)
		{
			Execute (contextDocument, null, argList, results);
		}

		[MonoTODO]
		public void Execute (
			XmlResolver dataSources,
			XmlArgumentList argList,
			XmlWriter results)
		{ 
			Execute (dataSources, argList, results);
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlResolver dataSources,
			XmlArgumentList argList,
			XmlWriter results)
		{ 
			throw new NotImplementedException ();
		}

		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			Stream results)
		{
			XmlTextWriter w = new XmlTextWriter (results, null);
			Execute (contextDocumentUri, dataSources, argList, w);
		}

		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			TextWriter results)
		{
			XmlTextWriter w = new XmlTextWriter (results);
			Execute (contextDocumentUri, dataSources, argList, w);
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			XmlWriter results)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif // NET_2_0
