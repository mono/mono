// System.Xml.Xsl.XslTransform
// 
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml.XPath;
using System.IO;

namespace System.Xml.Xsl
{
	public class XslTransform
	{
		#region Fields
		
		XmlResolver _xmlResolver;

		#endregion

		#region Constructors

		[MonoTODO]
		public XslTransform()
		{
		}

		#endregion

		#region Properties

		XmlResolver XmlResolver {
			set { _xmlResolver = value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load( IXPathNavigable stylesheet )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet specified by a URL.
		public void Load( string url )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load( XmlReader stylesheet )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load( XPathNavigator stylesheet )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load( 	
			IXPathNavigable stylesheet, 
			XmlResolver resolver )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet specified by a URL.
		public void Load( 	
			string url, 
			XmlResolver resolver )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load( 
			XmlReader stylesheet,
			XmlResolver resolver )
		{
		}

		[MonoTODO]
		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load( 
			XPathNavigator stylesheet,
			XmlResolver resolver )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using 
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform(
			IXPathNavigable input,
			XsltArgumentList args )
		{
			return null;
		}

		[MonoTODO]
		// Transforms the XML data in the input file and outputs 
		// the result to an output file.
		public void Transform(
			string inputfile,
			string outputfile )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using 
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform(
			XPathNavigator input,
			XsltArgumentList args )
		{
			return null;
		}
		
		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using 
		// the specified args and outputs the result to a Stream.
		public void Transform(
			IXPathNavigable input,
			XsltArgumentList args,
			Stream output )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using 
		// the specified args and outputs the result to a TextWriter.
		public void Transform(
			IXPathNavigable input,
			XsltArgumentList args,
			TextWriter output )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the IXPathNavigable using 
		// the specified args and outputs the result to an XmlWriter.
		public void Transform(
			IXPathNavigable input,
			XsltArgumentList args,
			XmlWriter output )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using 
		// the specified args and outputs the result to a Stream.
		public void Transform(
			XPathNavigator input,
			XsltArgumentList args,
			Stream output )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using 
		// the specified args and outputs the result to a TextWriter.
		public void Transform(
			XPathNavigator input,
			XsltArgumentList args,
			TextWriter output )
		{
		}

		[MonoTODO]
		// Transforms the XML data in the XPathNavigator using 
		// the specified args and outputs the result to an XmlWriter.
		public void Transform(
			XPathNavigator input,
			XsltArgumentList args,
			XmlWriter output )
		{
		}

		#endregion
	}
}
