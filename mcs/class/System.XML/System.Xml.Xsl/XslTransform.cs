// System.Xml.Xsl.XslTransform
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2002 Tim Coleman
// (c) 2003 Ximian Inc. (http://www.ximian.com)
//

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
		IntPtr stylesheet;

		#endregion

		#region Constructors
		public XslTransform ()
		{
			stylesheet = IntPtr.Zero;
		}

		#endregion

		#region Properties

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
		}

		#endregion

		#region Methods

		void FreeStylesheetIfNeeded ()
		{
			if (stylesheet != IntPtr.Zero) {
				xsltFreeStylesheet (stylesheet);
				stylesheet = IntPtr.Zero;
			}
		}
		
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load (IXPathNavigable stylesheet)
		{
			Load (stylesheet.CreateNavigator ());
		}

		// Loads the XSLT stylesheet specified by a URL.
		public void Load (string url)
		{
			if (url == null)
				throw new ArgumentNullException ("url");

			FreeStylesheetIfNeeded ();
			stylesheet = xsltParseStylesheetFile (url);
			Cleanup ();
			if (stylesheet == IntPtr.Zero)
				throw new XmlException ("Error creating stylesheet");
		}

		static IntPtr GetStylesheetFromString (string xml)
		{
			IntPtr result = IntPtr.Zero;

			IntPtr xmlDoc = xmlParseDoc (xml);

			if (xmlDoc == IntPtr.Zero) {
				Cleanup ();
				throw new XmlException ("Error parsing stylesheet");
			}
				
			result = xsltParseStylesheetDoc (xmlDoc);
			Cleanup ();
			if (result == IntPtr.Zero)
				throw new XmlException ("Error creating stylesheet");

			return result;
		}

		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load (XmlReader stylesheet)
		{
			FreeStylesheetIfNeeded ();
			// Create a document for the stylesheet
			XmlDocument doc = new XmlDocument ();
			doc.Load (stylesheet);
			
			// Store the XML in a StringBuilder
			StringWriter sr = new UTF8StringWriter ();
			XmlTextWriter writer = new XmlTextWriter (sr);
			doc.Save (writer);

			this.stylesheet = GetStylesheetFromString (sr.GetStringBuilder ().ToString ());
			Cleanup ();
			if (this.stylesheet == IntPtr.Zero)
				throw new XmlException ("Error creating stylesheet");
		}

		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load (XPathNavigator stylesheet)
		{
			FreeStylesheetIfNeeded ();
			StringWriter sr = new UTF8StringWriter ();
			Save (stylesheet, sr);
			this.stylesheet = GetStylesheetFromString (sr.GetStringBuilder ().ToString ());
			Cleanup ();
			if (this.stylesheet == IntPtr.Zero)
				throw new XmlException ("Error creating stylesheet");
		}

		[MonoTODO("use the resolver")]
		// Loads the XSLT stylesheet contained in the IXPathNavigable.
		public void Load (IXPathNavigable stylesheet, XmlResolver resolver)
		{
			Load (stylesheet);
		}

		[MonoTODO("use the resolver")]
		// Loads the XSLT stylesheet specified by a URL.
		public void Load (string url, XmlResolver resolver)
		{
			Load (url);
		}

		[MonoTODO("use the resolver")]
		// Loads the XSLT stylesheet contained in the XmlReader
		public void Load (XmlReader stylesheet, XmlResolver resolver)
		{
			Load (stylesheet);
		}

		[MonoTODO("use the resolver")]
		// Loads the XSLT stylesheet contained in the XPathNavigator
		public void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			Load (stylesheet);
		}

		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Transform (input.CreateNavigator (), args);
		}

		// Transforms the XML data in the input file and outputs
		// the result to an output file.
		public void Transform (string inputfile, string outputfile)
		{
			IntPtr xmlDocument = IntPtr.Zero;
			IntPtr resultDocument = IntPtr.Zero;

			try {
				xmlDocument = xmlParseFile (inputfile);
				if (xmlDocument == IntPtr.Zero)
					throw new XmlException ("Error parsing input file");

				resultDocument = ApplyStylesheet (xmlDocument);
				/*
				* If I do this, the <?xml version=... is always present *
				if (-1 == xsltSaveResultToFilename (outputfile, resultDocument, stylesheet, 0))
					throw new XmlException ("Error xsltSaveResultToFilename");
				*/
				StreamWriter writer = new StreamWriter (File.OpenWrite (outputfile));
				writer.Write (GetStringFromDocument (resultDocument));
				writer.Close ();
			} finally {
				if (xmlDocument != IntPtr.Zero)
					xmlFreeDoc (xmlDocument);

				if (resultDocument != IntPtr.Zero)
					xmlFreeDoc (resultDocument);

				Cleanup ();
			}
		}

		IntPtr ApplyStylesheet (IntPtr doc)
		{
			if (stylesheet == IntPtr.Zero)
				throw new XmlException ("No style sheet!");

			IntPtr result = xsltApplyStylesheet (stylesheet, doc, IntPtr.Zero);
			if (result == IntPtr.Zero)
				throw new XmlException ("Error applying style sheet");

			return result;
		}

		static void Cleanup ()
		{
			xsltCleanupGlobals ();
			xmlCleanupParser ();
		}

		static string GetStringFromDocument (IntPtr doc)
		{
			IntPtr mem = IntPtr.Zero;
			int size = 0;
			xmlDocDumpMemory (doc, ref mem, ref size);
			if (mem == IntPtr.Zero)
				throw new XmlException ("Error dumping document");

			string docStr = Marshal.PtrToStringAnsi (mem, size);
			// FIXME: Using xmlFree segfaults :-???
			//xmlFree (mem);
			Marshal.FreeHGlobal (mem);
			//

			// Get rid of the <?xml...
			// FIXME: any other (faster) way that works?
			StringReader result = new StringReader (docStr);
			result.ReadLine (); // we want the semantics of line ending used here
			//
			return result.ReadToEnd ();
		}

		string ApplyStylesheetAndGetString (IntPtr doc)
		{
			IntPtr xmlOutput = ApplyStylesheet (doc);
			string strOutput = GetStringFromDocument (xmlOutput);
			xmlFreeDoc (xmlOutput);

			return strOutput;
		}

		IntPtr GetDocumentFromNavigator (XPathNavigator nav)
		{
			StringWriter sr = new UTF8StringWriter ();
			Save (nav, sr);
			IntPtr xmlInput = xmlParseDoc (sr.GetStringBuilder ().ToString ());
			if (xmlInput == IntPtr.Zero)
				throw new XmlException ("Error getting XML from input");

			return xmlInput;
		}

		[MonoTODO("args")]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform (XPathNavigator input, XsltArgumentList args)
		{
			IntPtr xmlInput = GetDocumentFromNavigator (input);
			string xslOutputString = ApplyStylesheetAndGetString (xmlInput);
			xmlFreeDoc (xmlInput);
			Cleanup ();

			return new XmlTextReader (new StringReader (xslOutputString));
		}

		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to a Stream.
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			Transform (input.CreateNavigator (), args, new StreamWriter (output));
		}

		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to a TextWriter.
		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			Transform (input.CreateNavigator (), args, output);
		}

		// Transforms the XML data in the IXPathNavigable using
		// the specified args and outputs the result to an XmlWriter.
		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			Transform (input.CreateNavigator (), args, output);
		}

		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to a Stream.
		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output)
		{
			Transform (input, args, new StreamWriter (output));
		}

		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to a TextWriter.
		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (output == null)
				throw new ArgumentNullException ("output");

			IntPtr inputDoc = GetDocumentFromNavigator (input);
			string transform = ApplyStylesheetAndGetString (inputDoc);
			xmlFreeDoc (inputDoc);
			Cleanup ();
			output.Write (transform);
		}

		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlWriter.
		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output)
		{
			StringWriter writer = new UTF8StringWriter ();
			Transform (input, args, writer);
			output.WriteRaw (writer.GetStringBuilder ().ToString ());
		}

		static void Save (XmlReader rdr, TextWriter baseWriter)
		{
			XmlTextWriter writer = new XmlTextWriter (baseWriter);
		
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

		static void Save (XPathNavigator navigator, TextWriter writer)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (writer);
			XPathNodeType type = XPathNodeType.All;

			WriteTree (navigator, xmlWriter, type);
			xmlWriter.WriteEndDocument ();
			xmlWriter.Flush ();
		}

		// Walks the XPathNavigator tree recursively 
		static void WriteTree (XPathNavigator navigator, XmlTextWriter writer, XPathNodeType type)
		{
			WriteCurrentNode (navigator, writer, ref type);

			if (navigator.MoveToFirstAttribute ()) {
				do {
					WriteCurrentNode (navigator, writer, ref type);
				} while (navigator.MoveToNextAttribute ());

				navigator.MoveToParent ();
			}

			if (navigator.MoveToFirstChild ()) {
				do {
					WriteTree (navigator, writer, type);
				} while (navigator.MoveToNext ());

				navigator.MoveToParent ();
				if (navigator.NodeType != XPathNodeType.Root)
					writer.WriteEndElement ();
			} else if (navigator.NodeType == XPathNodeType.Element) {
				writer.WriteEndElement ();
			}
		}

		// Format the output  
		static void WriteCurrentNode (XPathNavigator navigator, XmlTextWriter writer, ref XPathNodeType current_type)
		{
			switch (navigator.NodeType) {
			case XPathNodeType.Root:
				current_type = XPathNodeType.Root;
				writer.WriteStartDocument ();
				break;
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
				break;

			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
				writer.WriteWhitespace (navigator.Value);
				break;
			}
		}

		#endregion

		#region Calls to external libraries
		// libxslt
		[DllImport ("xslt")]
		static extern IntPtr xsltParseStylesheetFile (string filename);

		[DllImport ("xslt")]
		static extern IntPtr xsltParseStylesheetDoc (IntPtr docPtr);

		[DllImport ("xslt")]
		static extern IntPtr xsltApplyStylesheet (IntPtr stylePtr, IntPtr DocPtr, IntPtr notused);

		[DllImport ("xslt")]
		static extern int xsltSaveResultToFilename (string URI, IntPtr doc, IntPtr styleSheet, int compression);

		[DllImport ("xslt")]
		static extern void xsltCleanupGlobals ();

		[DllImport ("xslt")]
		static extern void xsltFreeStylesheet (IntPtr cur);

		// libxml2
		[DllImport ("xml2")]
		static extern IntPtr xmlNewDoc (string version);

		[DllImport ("xml2")]
		static extern int xmlSaveFile (string filename, IntPtr cur);

		[DllImport ("xml2")]
		static extern IntPtr xmlParseFile (string filename);

		[DllImport ("xml2")]
		static extern IntPtr xmlParseDoc (string document);

		[DllImport ("xml2")]
		static extern void xmlFreeDoc (IntPtr doc);

		[DllImport ("xml2")]
		static extern void xmlCleanupParser ();

		[DllImport ("xml2")]
		static extern void xmlDocDumpMemory (IntPtr doc, ref IntPtr mem, ref int size);

		[DllImport ("xml2")]
		static extern void xmlFree (IntPtr data);

		#endregion

		// This classes just makes the base class use 'encoding="utf-8"'
		class UTF8StringWriter : StringWriter
		{
			static Encoding encoding = new UTF8Encoding (false);

			public override Encoding Encoding {
				get {
					return encoding;
				}
			}
		}
	}
}
