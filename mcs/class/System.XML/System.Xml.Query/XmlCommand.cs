//
// System.Xml.Query.XmlCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc, 2004
//

#if NET_2_0

using System.IO;
using System.Xml.XPath;

namespace System.Xml.Query
{
        public abstract class XmlCommand
        {
		public event QueryEventHandler OnProcessingEvent;

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlWriter writer);

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			XmlResolver dataSource,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlResolver dataSource,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			Stream results);

		public abstract void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			XmlArgumentList argList,
			TextWriter results);

		public abstract void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			XmlArgumentList argList,
			XmlWriter results);
        }
}

#endif // NET_2_0
