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

using System.IO;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Query
{
	public sealed class XsltCommand
	{
		[MonoTODO]
		public XsltCommand ()
		{
		}

		[MonoTODO]
		public event QueryEventHandler OnMessageEvent;

		// Compile

		[MonoTODO]
		public void Compile (string stylesheetUri)
		{ 
			Compile (stylesheetUri, new XmlUrlResolver (), null);
		}

		[MonoTODO]
		public void Compile (string stylesheetUri, XmlResolver resolver)
		{
			Compile (stylesheetUri, resolver, null);
		}

		[MonoTODO]
		public void Compile (string stylesheetUri, XmlResolver resolver, Evidence evidence)
		{
			using (XmlReader reader = XmlReader.Create (stylesheetUri)) {
				Compile (reader, resolver, evidence);
			}
		}

		[MonoTODO]
		public void Compile (XmlReader reader)
		{
			Compile (reader, new XmlUrlResolver ());
		}

		[MonoTODO]
		public void Compile (XmlReader reader, XmlResolver resolver)
		{
			Compile (reader, resolver, null);
		}

		[MonoTODO]
		public void Compile (XmlReader reader, XmlResolver resolver, Evidence evidence)
		{
			throw new NotImplementedException ();
		}

		// Execute

		[MonoTODO]
		public void Execute (string contextDocumentUri, XmlWriter results)
		{
			Execute (contextDocumentUri, new XmlUrlResolver (), null, results);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlWriter results)
		{
			Execute (contextDocument, new XmlUrlResolver (), null, results);
		}

		[MonoTODO]
		public void Execute (string contextDocumentUri, string resultDocumentUri)
		{
			XmlWriter xw = XmlWriter.Create (resultDocumentUri);
			try {
				Execute (new XPathDocument (contextDocumentUri), xw);
			} finally {
				xw.Close ();
			}
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlResolver resolver,
			XmlArgumentList argList,
			Stream results)
		{
			Execute (contextDocument, resolver, argList, results);
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlResolver dataSources,
			XmlArgumentList argList,
			TextWriter results)
		{ 
			Execute (contextDocument, dataSources, argList, results);
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

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			Stream results)
		{
			XmlWriter w = XmlWriter.Create (results);
			Execute (contextDocumentUri, dataSources, argList, w);
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			TextWriter results)
		{
			XmlWriter w = XmlWriter.Create (results);
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
