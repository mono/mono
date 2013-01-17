//
// XmlDsigXPathTransform.cs - 
//	XmlDsigXPathTransform implementation for XML Signature
// http://www.w3.org/TR/1999/REC-xpath-19991116 
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace System.Security.Cryptography.Xml 
{

	// www.w3.org/TR/xmldsig-core/
	// see Section 6.6.3 of the XMLDSIG specification
	public class XmlDsigXPathTransform : Transform 
	{

		private Type [] input;
		private Type [] output;
		private XmlNodeList xpath;
		private XmlDocument doc;
		private XsltContext ctx;

		public XmlDsigXPathTransform () 
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigXPathTransform;
		}

		public override Type [] InputTypes {
			get {
				if (input == null) {
					input = new Type [3];
					input [0] = typeof (System.IO.Stream);
					input [1] = typeof (System.Xml.XmlDocument);
					input [2] = typeof (System.Xml.XmlNodeList);
				}
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				if (output == null) {
					// this way the result is cached if called multiple time
					output = new Type [1];
					output [0] = typeof (System.Xml.XmlNodeList);
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			if (xpath == null) {
				// default value
				XmlDocument xpdoc = new XmlDocument ();
				xpdoc.LoadXml ("<XPath xmlns=\"" + XmlSignature.NamespaceURI + "\"></XPath>");
				xpath = xpdoc.ChildNodes;
			}
			return xpath;
		}

		[MonoTODO ("Evaluation of extension function here() results in different from MS.NET (is MS.NET really correct??).")]
		public override object GetOutput () 
		{
#if NET_2_0
			if ((xpath == null) || (doc == null))
				return new XmlDsigNodeList (new ArrayList ());
#else
			if (xpath == null)
				return new XmlDsigNodeList (new ArrayList ());
#endif
			// evaluate every time since input or xpath might have changed.
			string x = null;
			for (int i = 0; i < xpath.Count; i++) {
				switch (xpath [i].NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.Element:
					x += xpath [i].InnerText;
					break;
				}
			}

			ctx = new XmlDsigXPathContext (doc);
			foreach (XmlNode n in xpath) {
				XPathNavigator nav = n.CreateNavigator ();
				XPathNodeIterator iter = nav.Select ("namespace::*");
				while (iter.MoveNext ())
					if (iter.Current.LocalName != "xml")
						ctx.AddNamespace (iter.Current.LocalName, iter.Current.Value);
			}
			return EvaluateMatch (doc, x);
		}

		public override object GetOutput (Type type) 
		{
			if (type != typeof (XmlNodeList))
				throw new ArgumentException ("type");
			return GetOutput ();
		}

		private XmlDsigNodeList EvaluateMatch (XmlNode n, string xpath)
		{
			ArrayList al = new ArrayList ();
			// Strictly to say, document node is explicitly
			// excluded by W3C spec (context node is initialized
			// to the document root and XPath expression is
			// "//. | //@* | //namespace::*)
			XPathNavigator nav = n.CreateNavigator ();
			XPathExpression exp = nav.Compile (xpath);
			exp.SetContext (ctx);
			EvaluateMatch (n, exp, al);
			return new XmlDsigNodeList (al);
		}

		private void EvaluateMatch (XmlNode n, XPathExpression exp, ArrayList al)
		{
			if (NodeMatches (n, exp))
				al.Add (n);
			if (n.Attributes != null)
				for (int i = 0; i < n.Attributes.Count; i++)
					if (NodeMatches (n.Attributes [i], exp))
						al.Add (n.Attributes [i]);
			for (int i = 0; i < n.ChildNodes.Count; i++)
				EvaluateMatch (n.ChildNodes [i], exp, al);
		}

		private bool NodeMatches (XmlNode n, XPathExpression exp)
		{
			// This looks waste of memory since it creates 
			// XPathNavigator every time, but even if we use
			//  XPathNodeIterator.Current, it also clones every time.
			object ret = n.CreateNavigator ().Evaluate (exp);
			if (ret is bool)
				return (bool) ret;
			if (ret is double) {
				double d = (double) ret;
				return !(d == 0.0 || Double.IsNaN (d));
			}
			if (ret is string)
				return ((string) ret).Length > 0;
			if (ret is XPathNodeIterator) {
				XPathNodeIterator retiter = (XPathNodeIterator) ret;
				return retiter.Count > 0;
			}
			return false;
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			if (nodeList == null)
				throw new CryptographicException ("nodeList");
			xpath = nodeList;
		}

		public override void LoadInput (object obj) 
		{
			// possible input: Stream, XmlDocument, and XmlNodeList
			if (obj is Stream) {
				doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.XmlResolver = GetResolver ();
				doc.Load (new XmlSignatureStreamReader (
					new StreamReader ((Stream) obj)));
			}
			else if (obj is XmlDocument) {
				doc = (obj as XmlDocument);
			}
			else if (obj is XmlNodeList) {
				doc = new XmlDocument ();
				doc.XmlResolver = GetResolver ();
				foreach (XmlNode xn in (obj as XmlNodeList))  {
					XmlNode importedNode = doc.ImportNode (xn, true);
					doc.AppendChild (importedNode);
				}
			}
		}

		// Internal classes to support XPath extension function here()

		internal class XmlDsigXPathContext : XsltContext
		{
			XmlDsigXPathFunctionHere here;
			public XmlDsigXPathContext (XmlNode node)
			{
				here = new XmlDsigXPathFunctionHere (node);
			}

			public override IXsltContextFunction ResolveFunction (
				string prefix, string name, XPathResultType [] argType)
			{
				// Here MS.NET incorrectly allows arbitrary
				// name e.g. "heretic()".
				if (name == "here" &&
					prefix == String.Empty &&
					argType.Length == 0)
					return here;
				else
					return null; // ????
			}

			public override bool Whitespace {
				get { return true; }
			}

			public override bool PreserveWhitespace (XPathNavigator node)
			{
				return true;
			}

			public override int CompareDocument (string s1, string s2)
			{
				return String.Compare (s1, s2);
			}

			public override IXsltContextVariable ResolveVariable (string prefix, string name)
			{
				throw new InvalidOperationException ();
			}
		}

		internal class XmlDsigXPathFunctionHere : IXsltContextFunction
		{
			// Static

			static XPathResultType [] types;
			static XmlDsigXPathFunctionHere ()
			{
				types = new XPathResultType [0];
			}

			// Instance

			XPathNodeIterator xpathNode;

			public XmlDsigXPathFunctionHere (XmlNode node)
			{
				xpathNode = node.CreateNavigator ().Select (".");
			}

			public XPathResultType [] ArgTypes {
				get { return types; }
			}
		
			public int Maxargs { get { return 0; } }
		
			public int Minargs { get { return 0; } }
		
			public XPathResultType ReturnType {
				get { return XPathResultType.NodeSet; }
			}

			public object Invoke (XsltContext ctx, object [] args, XPathNavigator docContext)
			{
				if (args.Length != 0)
					throw new ArgumentException ("Not allowed arguments for function here().", "args");

				return xpathNode.Clone ();
			}
		}
	}
}
