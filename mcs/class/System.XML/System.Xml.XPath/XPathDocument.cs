//
// System.Xml.XPath.XPathDocument
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright 2002 Tim Coleman
//

using System.IO;
using System.Xml;

namespace System.Xml.XPath
{
	[MonoTODO]
	public class XPathDocument : IXPathNavigable
	{
		XmlDocument _doc = new XmlDocument ();

#region Constructors

		public XPathDocument (Stream stream)
		{
			_doc.Load (stream);
		}

		public XPathDocument (string uri)
		{
			_doc.Load (uri);
		}

		public XPathDocument (TextReader reader)
		{
			_doc.Load (reader);
		}

		public XPathDocument (XmlReader reader)
		{
			_doc.Load (reader);
		}

		public XPathDocument (string uri, XmlSpace space)
		{
			if (space == XmlSpace.Preserve)
				_doc.PreserveWhitespace = true;
			_doc.Load (uri);
		}

		public XPathDocument (XmlReader reader, XmlSpace space)
		{
			if (space == XmlSpace.Preserve)
				_doc.PreserveWhitespace = true;
			_doc.Load (reader);
		}

#endregion

#region Methods

		public XPathNavigator CreateNavigator ()
		{
			return _doc.CreateNavigator ();
		}

#endregion
	}
}

