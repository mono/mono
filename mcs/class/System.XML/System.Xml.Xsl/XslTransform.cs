// System.Xml.Xsl.XslTransform
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml.XPath;
using System.IO;

namespace System.Xml.Xsl
{
	public sealed class XslTransform
	{
		#region Fields

		XmlResolver xmlResolver;

		#endregion

		#region Constructors

		[MonoTODO]
		public XslTransform ()
		{
		}

		#endregion

		#region Properties

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load (IXPathNavigable stylesheet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet specified by a URL.
		public void Load (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load (XmlReader stylesheet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load (XPathNavigator stylesheet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load (IXPathNavigable stylesheet, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet specified by a URL.
		public void Load (string url, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load (XmlReader stylesheet, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the input file and outputs
		// the result to an output file.
		public void Transform (string inputfile, string outputfile)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform (XPathNavigator input, XsltArgumentList args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to a Stream.
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to a TextWriter.
		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to an XmlWriter.
		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to a Stream.
		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to a TextWriter.
		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlWriter.
		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
