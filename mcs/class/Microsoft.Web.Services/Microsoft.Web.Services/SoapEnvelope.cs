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
using System.Text;

namespace Microsoft.Web.Services {

	public class SoapEnvelope : XmlDocument {

		private SoapContext context;
		private XmlElement envelope;
		private XmlElement body;
		private XmlElement header;
#if WSE2
		private Encoding _encoding;
#endif

		public SoapEnvelope ()
		{
			envelope = CreateElement (Soap.Prefix, Soap.ElementNames.Envelope, Soap.NamespaceURI);
			AppendChild (envelope);
		}

		internal SoapEnvelope (SoapContext context) : this ()
		{
			this.context = context;
		}

#if WSE2
		public Encoding Encoding {
			get {
				if(_encoding == null) {
					return new UTF8Encoding (false);
				}
				return _encoding;
			}
			set {
				_encoding = value;
				if(_encoding is UTF8Encoding) {
					_encoding = new UTF8Encoding (false);
				}
			}
		}
#endif

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
#if WSE2
			base.Save (new XmlTextWriter (str, Encoding));
#else
			base.Save (str);
#endif
		}
	}
}
