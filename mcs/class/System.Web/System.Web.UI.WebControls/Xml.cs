//
// System.Web.UI.WebControls.Xml.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("DocumentSource")]
	[PersistChildren(false)]
	[ControlBuilder (typeof (XmlBuilder))]
	[Designer ("System.Web.UI.Design.WebControls.XmlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class Xml : Control
	{
		static XslTransform defaultTransform;

		XmlDocument document;
		string documentContent;
		string documentSource;
		XslTransform transform;
		XsltArgumentList transformArgumentList;
		string transformSource;
		XPathDocument xpathDoc;

		static Xml ()
		{
			XmlTextReader reader = new XmlTextReader (new StringReader("<xsl:stylesheet version='1.0' " +
			                                        "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
			                                        "<xsl:template match=\"*\">" +
			                                        "<xsl:copy-of select=\".\"/>" +
			                                        "</xsl:template>" +
			                                        "</xsl:stylesheet>"));

			defaultTransform = new XslTransform();
#if NET_1_0
			defaultTransform.Load (reader);
#else
			defaultTransform.Load (reader, null, null);
#endif
		}

		public Xml ()
		{
		}

		[MonoTODO("security")]
		private void LoadXmlDoc ()
		{
			if (documentContent != null && documentContent.Length > 0) {
				document = new XmlDocument ();
				document.LoadXml (documentContent);
				return;
			}

			if (documentSource != null && documentSource.Length != 0) {
				document = new XmlDocument ();
				document.Load (documentSource);
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("This is the XML document that is used for the XML Webcontrol.")]
		public XmlDocument Document {
			get {
				if (document == null)
					LoadXmlDoc ();
				return document;
			}
			set {
				documentSource  = null;
				documentContent = null;
				xpathDoc = null;
				document = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The XML content that is transformed for the XML Webcontrol.")]
		public string DocumentContent {
			get {
				return documentContent;
			}
			set {
				document = null;
				xpathDoc = null;
				documentContent = value;
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[Editor ("System.Web.UI.Design.XmlUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("The URL or the source of the XML content that is transformed for the XML Webcontrol.")]
		public string DocumentSource {
			get {
				if (documentSource != null)
					return documentSource;
				return String.Empty;
			}
			set {
				document = null;
				documentContent = null;
				xpathDoc = null;
				documentSource = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The XSL transform that is applied to this XML Webcontrol.")]
		public XslTransform Transform {
			get {
				return transform;
			}
			set {
				transformSource = null;
				transform = value;
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[Editor ("System.Web.UI.Design.XmlUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("An URL specifying the source that is used for the XSL transformation.")]
		public string TransformSource {
			get {
				if (transformSource != null)
					return transformSource;
				return String.Empty;
			}
			set {
				transform = null;
				transformSource = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Arguments that are used by the XSL Transform.")]
		public XsltArgumentList TransformArgumentList {
			get { return transformArgumentList; }
			set { transformArgumentList = value; }
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (obj is LiteralControl) {
				DocumentContent = ((LiteralControl) obj).Text;
				return;
			}

			throw new HttpException (HttpRuntime.FormatResourceString (
							"Cannot_Have_Children_of_Type",
							"Xml",
							GetType().Name));
		}

		private void LoadXpathDoc ()
		{
			if (documentContent != null && documentContent.Length > 0) {
				xpathDoc = new XPathDocument (new StringReader (documentContent));
				return;
			}

			if (documentSource != null && documentSource.Length != 0) {
				xpathDoc = new XPathDocument (MapPathSecure (documentSource));
				return;
			}
		}

		private void LoadTransform ()
		{
			if (transform != null)
				return;

			if (transformSource != null && transformSource.Length != 0) {
				transform = new XslTransform ();
				transform.Load (MapPathSecure (transformSource));
			}
		}

		protected override void Render(HtmlTextWriter output)
		{
			if (document == null) {
				LoadXpathDoc();
			}

			LoadTransform();
			if (document == null && xpathDoc == null) {
				return;
			}

			if (transform == null) {
				transform = defaultTransform;
			}

			if (document != null) {
#if NET_1_0
				Transform.Transform(document, transformArgumentList, output);
#else
				Transform.Transform(document, transformArgumentList, output, null);
#endif
				return;
			}
#if NET_1_0
			Transform.Transform(xpathDoc, transformArgumentList, output);
#else
			Transform.Transform(xpathDoc, transformArgumentList, output, null);
#endif
		}
	}
}

