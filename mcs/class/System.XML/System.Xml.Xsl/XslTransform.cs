// System.Xml.Xsl.XslTransform
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Xml.Xsl
{
	public sealed class XslTransform
	{

		#region Fields

		XmlResolver xmlResolver;
		string stylesheet_file;
		#endregion

		#region Constructors
		public XslTransform ()
		{
			stylesheet_file = String.Empty;
		}

		#endregion

		#region Properties

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
		}

		#endregion

		#region Methods

		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load (IXPathNavigable stylesheet)
		{
			Load (stylesheet.CreateNavigator ());
		}

		// Loads the XSLT stylesheet specified by a URL.
		public void Load (string url)
		{
			stylesheet_file = url;
		}

		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load (XmlReader stylesheet)
		{
			stylesheet_file = Path.GetTempFileName ();
			Save (stylesheet, stylesheet_file);
		}

		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load (XPathNavigator stylesheet)
		{
			stylesheet_file = Path.GetTempFileName ();
			Save (stylesheet, stylesheet_file);
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

		[DllImport ("libxslt.so")]
		static extern IntPtr xsltParseStylesheetFile (string filename);

		[DllImport ("libxslt.so")]
		static extern IntPtr xsltApplyStylesheet (IntPtr stylePtr, IntPtr DocPtr, string [] parameters);

		[DllImport ("libxslt.so")]
		static extern IntPtr xmlNewDoc (string version);

		[DllImport ("libxslt.so")]
		static extern IntPtr xmlParseFile (string filename);

		[DllImport ("libxslt.so")]
		static extern int xmlSaveFile (string filename, IntPtr cur);

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
			IntPtr xmlDocument = xmlParseFile (inputfile);
			IntPtr xmlStylesheet = xsltParseStylesheetFile (stylesheet_file);
			IntPtr xmlOutput = xmlNewDoc ("1.0");
			string [] parameters = new string [] {};

			xmlOutput = xsltApplyStylesheet (xmlStylesheet, xmlDocument, parameters);
			
			xmlSaveFile (outputfile, xmlOutput);
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

		static void Save (XmlReader rdr, string filename)
		{
			XmlTextWriter writer = new XmlTextWriter (filename, new UTF8Encoding ());
		
			while (rdr.Read ()) {
				switch (rdr.NodeType) {
				
				case XmlNodeType.CDATA:
					writer.WriteCData (rdr.Value);
					break;
				
				case XmlNodeType.Comment:
					writer.WriteComment (rdr.Value);
					break;

				case XmlNodeType.DocumentType:
					writer.WriteDocType (rdr.Value, null, null, null);
					break;

				case XmlNodeType.Element:
					writer.WriteStartElement (rdr.Name, rdr.Value);
				
					while (rdr.MoveToNextAttribute ())
						writer.WriteAttributes (rdr, true);
					break;
			
				case XmlNodeType.EndElement:
					writer.WriteEndElement ();
					break;

				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction (rdr.Name, rdr.Value);
					break;

				case XmlNodeType.Text:
					writer.WriteString (rdr.Value);
					break;

				case XmlNodeType.Whitespace:
					writer.WriteWhitespace (rdr.Value);
					break;

				case XmlNodeType.XmlDeclaration:
					writer.WriteStartDocument ();
					break;
				}
			}

			writer.Close ();
		}

		static void Save (XPathNavigator navigator, string filename)
		{
			XmlTextWriter writer = new XmlTextWriter (filename, new UTF8Encoding ());
			XPathNodeType type = XPathNodeType.All;

			WriteTree (navigator, writer, type);
		}

		// Walks the XPathNavigator tree recursively 
		static void WriteTree (XPathNavigator navigator, XmlTextWriter writer, XPathNodeType type)
		{
			WriteCurrentNode (navigator, writer, ref type);

			if (navigator.HasAttributes) {
				navigator.MoveToFirstAttribute ();
				
				do {
					WriteCurrentNode (navigator, writer, ref type);
				} while ( navigator.MoveToNextAttribute ());

				navigator.MoveToParent ();
			} 

			if (navigator.HasChildren) {
				navigator.MoveToFirstChild ();

				do {
					WriteTree (navigator, writer, type);
				} while (navigator.MoveToNext ());

				navigator.MoveToParent ();
			}
		}

		// Format the output  
		static void WriteCurrentNode (XPathNavigator navigator, XmlTextWriter writer, ref XPathNodeType current_type)
		{
			switch (navigator.NodeType) {
			case XPathNodeType.Attribute:
				current_type = XPathNodeType.Attribute;
				writer.WriteAttributeString (navigator.LocalName, navigator.Value);
				break;

			case XPathNodeType.Comment:
				writer.WriteComment (navigator.Value);
				break;

			case XPathNodeType.Element:
				current_type = XPathNodeType.Element;
				writer.WriteStartElement (navigator.Name);
				break;
			
			case XPathNodeType.ProcessingInstruction:
				writer.WriteProcessingInstruction (navigator.Name, navigator.Value);
				break;

			case XPathNodeType.Text:
				writer.WriteString (navigator.Value);

				if (current_type == XPathNodeType.Element) {
					writer.WriteEndElement ();
					current_type = XPathNodeType.All;
				}

				break;

			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
				writer.WriteWhitespace (navigator.Value);
				break;
			}
		}
		#endregion
	}
}
