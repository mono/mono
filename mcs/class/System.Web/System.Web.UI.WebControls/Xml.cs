/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Xml
 *
 * Author:  Gaurav Vaish, Gonzalo Paniagua Javier
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>, <gonzalo@ximian.com>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 * (c) 2002 Ximian, Inc. (http://www.ximian.com)
 */

using System;
using System.ComponentModel;
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
	public class Xml : Control
	{
		private XmlDocument      document;
		private string           documentContent;
		private string           documentSource;
		private XslTransform     transform;
		private XsltArgumentList transformArgumentList;
		private string           transformSource;

		private XPathDocument xpathDoc;

		private static XslTransform defaultTransform;

		static Xml()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<xsl:stylesheet version='1.0' " +
			                                        "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
			                                        "<xsl:template match=\"\">" +
			                                        "<xsl:copy-of select=\".\"/>" +
			                                        "</xsl:template>" +
			                                        "</xsl:stylesheet>"));
			defaultTransform = new XslTransform();
			defaultTransform.Load(reader);
		}

		public Xml(): base()
		{
		}

		[MonoTODO("security")]
		private void LoadXmlDoc ()
		{
			if (documentContent != null && documentContent.Length > 0) {
				document = new XmlDocument();
				document.LoadXml (documentContent);
				return;
			}

			if (documentSource != null && documentSource.Length != 0) {
				document = new XmlDocument();
				document.Load (documentSource);
			}
		}

		public XmlDocument Document
		{
			get
			{
				if(document == null)
					LoadXmlDoc();
				return document;
			}
			set
			{
				documentSource  = null;
				documentContent = null;
				xpathDoc        = null;
				document        = value;
			}
		}

		public string DocumentContent
		{
			get
			{
				return String.Empty;
			}
			set
			{
				document        = null;
				xpathDoc        = null;
				documentContent = value;
			}
		}

		public string DocumentSource
		{
			get
			{
				if(documentSource != null)
					return documentSource;
				return String.Empty;
			}
			set
			{
				document        = null;
				documentContent = null;
				xpathDoc        = null;
				documentSource  = value;
			}
		}

		public XslTransform Transform
		{
			get
			{
				return transform;
			}
			set
			{
				transformSource = null;
				transform       = value;
			}
		}

		public string TransformSource
		{
			get
			{
				if(transformSource != null)
					return transformSource;
				return String.Empty;
			}
			set
			{
				transform       = null;
				transformSource = value;
			}
		}

		public XsltArgumentList TransformArgumentList
		{
			get
			{
				return transformArgumentList;
			}
			set
			{
				transformArgumentList = value;
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(obj is LiteralControl)
			{
				DocumentContent = ((LiteralControl)obj).Text;
				return;
			}
			throw new HttpException (HttpRuntime.FormatResourceString (
							"Cannot_Have_Children_of_Type",
							"Xml",
							GetType().Name));
		}

		[MonoTODO("security")]
		private void LoadXpathDoc ()
		{
			if(documentContent != null && documentContent.Length > 0) {
				xpathDoc = new XPathDocument (new StringReader (documentContent));
				return;
			}

			if (documentSource != null && documentSource.Length != 0) {
				xpathDoc = new XPathDocument (MapPathSecure (documentSource));
				return;
			}
		}

		[MonoTODO("security")]
		private void LoadTransform ()
		{
			if (transform != null)
				return;

			if (transformSource != null && transformSource.Length != 0) {
				transform = new XslTransform ();
				transform.Load (transformSource);
			}
		}

		protected override void Render(HtmlTextWriter output)
		{
			if(document == null)
			{
				LoadXpathDoc();
			}

			LoadTransform();
			if(document == null || xpathDoc == null)
			{
				return;
			}
			if(transform == null)
			{
				transform = defaultTransform;
			}
			if(document != null)
			{
				Transform.Transform(document, transformArgumentList, output);
				return;
			}
			Transform.Transform(xpathDoc, transformArgumentList, output);
		}
	}
}

