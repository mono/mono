//
// SoapEnvelope.cs: Soap Envelope
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Xml;

namespace Microsoft.Web.Services {

	public class SoapEnvelope : XmlDocument {

		private SoapContext context;
		private XmlElement envelope;
		private XmlElement body;
		private XmlElement header;

		public SoapEnvelope ()
		{
			envelope = CreateElement (Soap.Prefix, Soap.ElementNames.Envelope, Soap.NamespaceURI);
			AppendChild (envelope);
		}

		internal SoapEnvelope (SoapContext context) : this ()
		{
			this.context = context;
		}

		public XmlElement Body {
			get {
				if (body == null) {
					XmlNodeList xnl = GetElementsByTagName (Soap.ElementNames.Body, Soap.NamespaceURI);
					body = (XmlElement)xnl[0];
				}
				return body;
			}
		}

		public SoapContext Context { 
			get { 
				if (context == null)
					context = new SoapContext (this);
				return context; 
			}
		}

		public XmlElement Envelope { 
			get { return envelope; }
		}

		public XmlElement Header { 
			get {
				if (header == null) {
					XmlNodeList xnl = GetElementsByTagName (Soap.ElementNames.Header, Soap.NamespaceURI);
					header = (XmlElement)xnl[0];
				}
				return header;
			}
		}

		public XmlElement CreateBody () 
		{
			if (body == null) {
				body = CreateElement (Soap.Prefix, Soap.ElementNames.Body, Soap.NamespaceURI);
				DocumentElement.AppendChild (body);
			}
			return body;
		}

		public XmlElement CreateHeader () 
		{
			if (header == null) {
				header = CreateElement (Soap.Prefix, Soap.ElementNames.Header, Soap.NamespaceURI);
				// be sure Header comes before the Body
				DocumentElement.PrependChild (header);
			}
			return header;
		}

		private void InvalidateCache () 
		{
			envelope = DocumentElement;
			header = null;
			body = null;
		}

		public override void Load (Stream stream) 
		{
			base.Load (stream);
			InvalidateCache ();
		}

		public override void Load (string filename)
		{
			base.Load (filename);
			InvalidateCache ();
		}

		public override void Load (TextReader txtReader) 
		{
			base.Load (txtReader);
			InvalidateCache ();
		}

		public override void Load (XmlReader xmlReader) 
		{
			base.Load (xmlReader);
			InvalidateCache ();
		}

		[MonoTODO("why?")]
		public override void Save (Stream stream) 
		{
			base.Save (stream);
		}

		[MonoTODO("why?")]
		public override void Save (string str) 
		{
			base.Save (str);
		}
	}
}
