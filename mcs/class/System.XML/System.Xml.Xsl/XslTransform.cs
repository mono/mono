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

using BF = System.Reflection.BindingFlags;

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

		~XslTransform ()
		{
			FreeStylesheetIfNeeded ();
		}

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

				resultDocument = ApplyStylesheet (xmlDocument, null, null);

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

		IntPtr ApplyStylesheet (IntPtr doc, string[] argArr, System.Collections.Hashtable extobjects)
		{
			if (stylesheet == IntPtr.Zero)
				throw new XmlException ("No style sheet!");

			IntPtr result;

			if (extobjects == null || extobjects.Count == 0) {
				// If there are no extension objects, use the simple (old) method.
				result = xsltApplyStylesheet (stylesheet, doc, argArr);
			} else {
				// If there are extension objects, create a context and register the functions.

				IntPtr context = xsltNewTransformContext(stylesheet, doc);

				if (context == IntPtr.Zero) throw new XmlException("Error creating transformation context.");

				try {
					foreach (string ns in extobjects.Keys) {
						object ext = extobjects[ns];

						System.Type type;
						System.Collections.IEnumerable methods;

						// As an added bonus, if the extension object is a UseStaticMethods object
						// (defined below), then add the static methods of the specified type.
						if (ext is UseStaticMethods) {
							type = ((UseStaticMethods)ext).Type;
							methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
							ext = null;
						} else {
							type = ext.GetType();
							methods = type.GetMethods();
						}

						System.Collections.Hashtable alreadyadded = new	System.Collections.Hashtable();
						foreach (System.Reflection.MethodInfo mi in methods) {
							if (alreadyadded.ContainsKey(mi.Name)) continue; // don't add twice
							alreadyadded[mi.Name] = 1;

							// Simple extension function delegate
							ExtensionFunction func = new ExtensionFunction(new ReflectedExtensionFunction(type, ext, mi.Name).Function);

							// Delegate for libxslt library call
							libxsltXPathFunction libfunc = new libxsltXPathFunction(new ExtensionFunctionWrapper(func).Function);
	
							int ret = xsltRegisterExtFunction(context, mi.Name, ns, libfunc);
							if (ret != 0) throw new XmlException("Could not register extension function " + mi.DeclaringType.FullName + "." + mi.Name + " in " + ns);
						}
					
					}
	
					result = xsltApplyStylesheetUser(stylesheet, doc, argArr, null, IntPtr.Zero, context);
				} finally {
					xsltFreeTransformContext(context);
				}
			}


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

		string ApplyStylesheetAndGetString (IntPtr doc, string[] argArr, System.Collections.Hashtable extobjects)
		{
			IntPtr xmlOutput = ApplyStylesheet (doc, argArr, extobjects);
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

		[MonoTODO("Node Set and Node Fragment Parameters and Extension Objects")]
		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlReader.
		public XmlReader Transform (XPathNavigator input, XsltArgumentList args)
		{
			IntPtr xmlInput = GetDocumentFromNavigator (input);
			string[] argArr = null;
			if (args != null) {
				argArr = new string[args.parameters.Count * 2 + 1];
				int index = 0;
				foreach (object key in args.parameters.Keys) {
					argArr [index++] = key.ToString();
					object value = args.parameters [key];
					if (value is Boolean)
						argArr [index++] = XmlConvert.ToString((bool) value); // FIXME: How to encode it for libxslt?
					else if (value is Double)
						argArr [index++] = XmlConvert.ToString((double) value); // FIXME: How to encode infinity's and Nan?
					else
						argArr [index++] = "'" + value.ToString() + "'"; // FIXME: How to encode "'"?
				}
				argArr[index] = null;
			}
			string xslOutputString = ApplyStylesheetAndGetString (xmlInput, argArr, args.extensionObjects);
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
		[MonoTODO("Node Set and Node Fragment Parameters and Extension Objects")]
		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (output == null)
				throw new ArgumentNullException ("output");

			IntPtr inputDoc = GetDocumentFromNavigator (input);
			string[] argArr = null;
                        if (args != null) {
				argArr = new string[args.parameters.Count * 2 + 1];
				int index = 0;
				foreach (object key in args.parameters.Keys) {
					argArr [index++] = key.ToString();
					object value = args.parameters [key];
					if (value is Boolean)
						argArr [index++] = XmlConvert.ToString((bool) value); // FIXME: How to encode it for libxslt?
					else if (value is Double)
						argArr [index++] = XmlConvert.ToString((double) value); // FIXME: How to encode infinity's and Nan?
					else
						argArr [index++] = "'" + value.ToString() + "'"; // FIXME: How to encode "'"?
				}
				argArr[index] = null;
			}
			string transform = ApplyStylesheetAndGetString (inputDoc, argArr, args.extensionObjects);
			xmlFreeDoc (inputDoc);
			Cleanup ();
			output.Write (transform);
			output.Flush ();
		}

		// Transforms the XML data in the XPathNavigator using
		// the specified args and outputs the result to an XmlWriter.
		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output)
		{
			StringWriter writer = new UTF8StringWriter ();
			Transform (input, args, writer);
			output.WriteRaw (writer.GetStringBuilder ().ToString ());
			output.Flush ();
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

			WriteTree (navigator, xmlWriter);
			xmlWriter.WriteEndDocument ();
			xmlWriter.Flush ();
		}

		// Walks the XPathNavigator tree recursively 
		static void WriteTree (XPathNavigator navigator, XmlTextWriter writer)
		{
			WriteCurrentNode (navigator, writer);

			if (navigator.MoveToFirstAttribute ()) {
				do {
					WriteCurrentNode (navigator, writer);
				} while (navigator.MoveToNextAttribute ());

				navigator.MoveToParent ();
			}

			if (navigator.MoveToFirstChild ()) {
				do {
					WriteTree (navigator, writer);
				} while (navigator.MoveToNext ());

				navigator.MoveToParent ();
				if (navigator.NodeType != XPathNodeType.Root)
					writer.WriteEndElement ();
			} else if (navigator.NodeType == XPathNodeType.Element) {
				writer.WriteEndElement ();
			}
		}

		// Format the output  
		static void WriteCurrentNode (XPathNavigator navigator, XmlTextWriter writer)
		{
			switch (navigator.NodeType) {
			case XPathNodeType.Root:
				writer.WriteStartDocument ();
				break;
			case XPathNodeType.Attribute:
				writer.WriteAttributeString (navigator.Name, navigator.Value);
				break;

			case XPathNodeType.Comment:
				writer.WriteComment (navigator.Value);
				break;

			case XPathNodeType.Element:
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

		// Extension Objects

		internal delegate object ExtensionFunction(object[] args);

		// Wraps an ExtensionFunction into a function that is callable from the libxslt library.
		private unsafe class ExtensionFunctionWrapper {
			ExtensionFunction func;

			public ExtensionFunctionWrapper(ExtensionFunction func) { this.func = func; }
			
			public unsafe void Function(IntPtr xpath_ctxt, int nargs) {

				// Convert XPath arguments into "managed" arguments
				System.Collections.ArrayList args = new System.Collections.ArrayList();
				for (int i = 0; i < nargs; i++) {
					xpathobject* aptr = valuePop(xpath_ctxt);
					if (aptr->type == 2) // Booleans
						args.Add( xmlXPathCastToBoolean(aptr) == 0 ? false : true );
					else if (aptr->type == 3) // Doubles
						args.Add( xmlXPathCastToNumber(aptr));
					else if (aptr->type == 4) // Strings
						args.Add( xmlXPathCastToString(aptr));
					else if (aptr->type == 1) { // Node Sets ==> ArrayList of strings
						System.Collections.ArrayList a = new System.Collections.ArrayList();
						for (int ni = 0; ni < aptr->nodesetptr->count; ni++) {
							xpathobject *n = xmlXPathNewNodeSet(aptr->nodesetptr->nodes[ni]);
							valuePush(xpath_ctxt, n);
							xmlXPathStringFunction(xpath_ctxt, 1);
							a.Add(xmlXPathCastToString(valuePop(xpath_ctxt)));
							xmlXPathFreeObject(n);
						}
						args.Add(a);
					} else { // Anything else => string
						valuePush(xpath_ctxt, aptr);
						xmlXPathStringFunction(xpath_ctxt, 1);
						args.Add(xmlXPathCastToString(valuePop(xpath_ctxt)));
					}

					xmlXPathFreeObject(aptr);
				}

				// Call function
				args.Reverse();
				object ret = func(args.ToArray());

				// Convert the result back to an XPath object
				if (ret == null) // null => ""
					valuePush(xpath_ctxt, xmlXPathNewCString(""));
				else if (ret is Boolean) // Booleans
					valuePush(xpath_ctxt, xmlXPathNewBoolean((bool)ret ? 1 : 0));
				else if (ret is int || ret is long || ret is double || ret is float || ret is decimal)
					// Numbers
					valuePush(xpath_ctxt, xmlXPathNewFloat((double)ret));
				else // Strings
					valuePush(xpath_ctxt, xmlXPathNewCString(ret.ToString()));

			}
		}

		// Provides a delegate for calling a late-bound method of a type with a given name.
		// Determines method based on types of arguments.
		private class ReflectedExtensionFunction {
			System.Type type;
			object src;
			string methodname;
		
			public ReflectedExtensionFunction(System.Type type, object src, string methodname) { this.type = type; this.src = src; this.methodname = methodname; }
		
			public object Function(object[] args) {
				// Construct arg type array, stringified version in case of problem
				System.Type[] argtypes = new System.Type[args.Length];
				string argtypelist = null;
				for (int i = 0; i < args.Length; i++) {
					argtypes[i] = (args[i] == null ? typeof(object) : args[i].GetType() );

					if (argtypelist != null) argtypelist += ", ";
					argtypelist += argtypes[i].FullName;
				}
				if (argtypelist == null) argtypelist = "";

				// Find the method
				System.Reflection.MethodInfo mi = type.GetMethod(methodname, (src == null ? BF.Static : BF.Instance | BF.Static) | BF.Public, null, argtypes, null);

				// No method?
				if (mi == null) throw new XmlException("No applicable function for " + methodname + " takes (" + argtypelist + ")");

				// Invoke
				return mi.Invoke(src, args);
			}
		}

		// Special Mono-specific class that allows static methods of a type to
		// be bound without needing an instance of that type.  Useful for
		// registering System.Math functions, for example.
		// Usage:   args.AddExtensionObject( new XslTransform.UseStaticMethods(typeof(thetype)) );
		public sealed class UseStaticMethods {
			public readonly System.Type Type;
			public UseStaticMethods(System.Type Type) { this.Type = Type; }
		}

		#endregion

		#region Calls to external libraries
		// libxslt
		[DllImport ("xslt")]
		static extern IntPtr xsltParseStylesheetFile (string filename);

		[DllImport ("xslt")]
		static extern IntPtr xsltParseStylesheetDoc (IntPtr docPtr);

		[DllImport ("xslt")]
		static extern IntPtr xsltApplyStylesheet (IntPtr stylePtr, IntPtr DocPtr, string[] argPtr);

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

		// Functions and structures for extension objects

		[DllImport ("xslt")]
		static extern IntPtr xsltNewTransformContext (IntPtr style, IntPtr doc);

		[DllImport ("xslt")]
		static extern void xsltFreeTransformContext (IntPtr context);

		[DllImport ("xslt")]
		static extern IntPtr xsltApplyStylesheetUser (IntPtr stylePtr, IntPtr DocPtr, string[] argPtr, string output, IntPtr profile, IntPtr context);

		[DllImport ("xslt")]
		static extern int xsltRegisterExtFunction (IntPtr context, string name, string uri, libxsltXPathFunction function);

		[DllImport ("xml2")]
		unsafe static extern xpathobject* valuePop (IntPtr context);

		[DllImport ("xml2")]
		unsafe static extern void valuePush (IntPtr context, xpathobject* data);

		[DllImport("xml2")]
		unsafe static extern void xmlXPathFreeObject(xpathobject* obj);
		
		[DllImport("xml2")]
		unsafe static extern xpathobject* xmlXPathNewCString(string str);

		[DllImport("xml2")]
		unsafe static extern xpathobject* xmlXPathNewFloat(double val);

		[DllImport("xml2")]
		unsafe static extern xpathobject* xmlXPathNewBoolean(int val);

		[DllImport("xml2")]
		unsafe static extern xpathobject* xmlXPathNewNodeSet(IntPtr nodeptr);

		[DllImport("xml2")]
		unsafe static extern int xmlXPathCastToBoolean(xpathobject* val);

		[DllImport("xml2")]
		unsafe static extern double xmlXPathCastToNumber(xpathobject* val);

		[DllImport("xml2")]
		unsafe static extern string xmlXPathCastToString(xpathobject* val);

		[DllImport("xml2")]
		static extern void xmlXPathStringFunction(IntPtr context, int nargs);

		private delegate void libxsltXPathFunction(IntPtr xpath_ctxt, int nargs);

		private struct xpathobject {
			public int type;
			public xmlnodelist* nodesetptr;
		}
		private struct xmlnodelist {
			public int count;
			public int allocated;
			public IntPtr* nodes;
		}

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
