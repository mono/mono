//
// System.Xml.Query.XmlCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.IO;

namespace System.Xml.Query {
        public abstract class XmlCommand
        {
		#region Methods

		public abstract void Execute (string contextDocumentUri, XmlResolver dataSources, XmlQueryArgumentList argList, TextWriter results);
		public abstract void Execute (string contextDocumentUri, XmlResolver dataSources, XmlQueryArgumentList argList, Stream results);

		#endregion // Methods
        }
}

#endif // NET_1_2
