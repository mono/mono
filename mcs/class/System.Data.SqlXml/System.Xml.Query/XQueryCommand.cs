//
// System.Xml.Query.XQueryCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc., 2004
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

using System.Data;
using System.Data.SqlXml;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Query 
{
	public class XQueryCommand
	{
		// They are obtained via reflection
		static Type implType;
		static MethodInfo compileMethod;
		static MethodInfo executeMethod;

		static XQueryCommand ()
		{
			implType = typeof (XPathNavigator).Assembly.GetType ("Mono.Xml.XPath2.XQueryCommandImpl");
			compileMethod = implType.GetMethod ("Compile");
			executeMethod = implType.GetMethod ("Execute");
			if (compileMethod == null)
				throw new InvalidOperationException ("Should not happen: XQueryCommandImpl.Compile() was not found.");
			if (executeMethod == null)
				throw new InvalidOperationException ("Should not happen: XQueryCommandImpl.Execute() was not found.");
		}

		#region Constructor

		[MonoTODO]
		public XQueryCommand ()
		{
			impl = Activator.CreateInstance (implType);
		}

		#endregion // Constructor

		object impl;

		#region Event

		public event QueryEventHandler OnMessageEvent;

		#endregion

		#region Methods

		// Compile

		[MonoTODO ("Null Evidence allowed?")]
		public void Compile (string query)
		{
			Compile (query, null);
		}

		public void Compile (string query, Evidence evidence)
		{
			Compile (new StringReader (query), evidence);
		}

		[MonoTODO ("Null Evidence allowed?")]
		public void Compile (TextReader query)
		{
			Compile (query, null);
		}

		[MonoTODO]
		public void Compile (TextReader query, Evidence evidence)
		{
			compileMethod.Invoke (impl, new object [] {query, evidence, this});
		}

		// Execute

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlWriter results)
		{
			Execute (contextDocument, null, null, results);
		}

		[MonoTODO ("Output StartDocument?")]
		public void Execute (
			XmlResolver dataSources, 
			TextWriter results)
		{
			XmlWriter w = XmlWriter.Create (results);
			Execute (dataSources, null, w);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			XmlResolver dataSources, 
			XmlWriter results)
		{
			Execute (new XPathDocument (), dataSources, null, results);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlArgumentList args,
			XmlWriter results)
		{
			Execute (contextDocument, null, args, results);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlResolver resolver,
			XmlWriter results)
		{
			Execute (contextDocument, resolver, null, results);
		}

		[MonoTODO("Indentation?;write StartDocument?;Null args allowed?")]
		public void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			Stream results)
		{
			Execute (contextDocumentUri, dataSources, null, results);
		}

		[MonoTODO("Indentation?;write StartDocument?;Null args allowed?")]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			TextWriter results)
		{
			Execute (contextDocumentUri, dataSources, null, results);
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlWriter results)
		{
			Execute (new XPathDocument (XmlReader.Create (contextDocumentUri)), dataSources, results);
		}

		[MonoTODO]
		public void Execute (
			XmlResolver dataSources, 
			XmlArgumentList args,
			XmlWriter results)
		{
			Execute ((XPathNavigator) null, dataSources, args, results);
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument, 
			XmlResolver dataSources,
			XmlArgumentList args,
			XmlWriter results)
		{
			Execute (contextDocument != null ? contextDocument.CreateNavigator () : null, dataSources, args, results);
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			Stream results)
		{
			Execute (new XPathDocument (XmlReader.Create (contextDocumentUri)), dataSources, args, XmlWriter.Create (results));
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			TextWriter results)
		{
			Execute (new XPathDocument (XmlReader.Create (contextDocumentUri)), dataSources, args, XmlWriter.Create (results));
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			XmlWriter results)
		{
			Execute (new XPathDocument (XmlReader.Create (contextDocumentUri)), dataSources, args, results);
		}

		private void Execute (XPathNavigator context, XmlResolver ds, XmlArgumentList args, XmlWriter output)
		{
			executeMethod.Invoke (impl, new object [] {context, ds, args, output});
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
