// 
// System.Web.Services.Protocols.Fault.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Protocols
{
	internal class Fault
	{
		static XmlSerializer serializer;
		
		static Fault ()
		{
			serializer = new FaultSerializer ();
		}

		public Fault () {}
		
		public Fault (SoapException ex) 
		{
			faultcode = ex.Code;
			faultstring = ex.Message;
			faultactor = ex.Actor;
			detail = ex.Detail;
		}

		[XmlElement (Namespace="")]
		public XmlQualifiedName faultcode;
		
		[XmlElement (Namespace="")]
		public string faultstring;
		
		[XmlElement (Namespace="")]
		public string faultactor;
		
		[SoapIgnore]
		public XmlNode detail;
		
		public static XmlSerializer Serializer
		{
			get { return serializer; }
		}
	}

	internal class FaultSerializer : XmlSerializer 
	{
		protected override void Serialize (object o, XmlSerializationWriter writer)
		{
			FaultWriter xsWriter = writer as FaultWriter;
			xsWriter.WriteRoot_Fault (o);
		}
		
		protected override object Deserialize (XmlSerializationReader reader)
		{
			FaultReader xsReader = reader as FaultReader;
			return xsReader.ReadRoot_Fault ();
		}
		
		protected override XmlSerializationWriter CreateWriter ()
		{
			return new FaultWriter ();
		}
		
		protected override XmlSerializationReader CreateReader ()
		{
			return new FaultReader ();
		}
	}	
	
	internal class FaultReader : XmlSerializationReader
	{
		public object ReadRoot_Fault ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "Fault" || Reader.NamespaceURI != WebServiceHelper.SoapEnvelopeNamespace)
				throw CreateUnknownNodeException();
			return ReadObject_Fault (true, true);
		}

		public System.Web.Services.Protocols.Fault ReadObject_Fault (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Fault ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "Fault" || t.Namespace != WebServiceHelper.SoapEnvelopeNamespace)
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Protocols.Fault ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b0=false, b1=false, b2=false, b3=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
					if (Reader.NamespaceURI == string.Empty || Reader.NamespaceURI == WebServiceHelper.SoapEnvelopeNamespace) {
						if (Reader.LocalName == "faultcode" && !b0) {
							b0 = true;
							ob.@faultcode = ReadElementQualifiedName ();
						} else if (Reader.LocalName == "faultstring" && !b1) {
							b1 = true;
							ob.@faultstring = Reader.ReadElementString ();
						} else if (Reader.LocalName == "detail" && !b3) {
							b3 = true;
							ob.@detail = ReadXmlNode (false);
						} else if (Reader.LocalName == "faultactor" && !b2) {
							b2 = true;
							ob.@faultactor = Reader.ReadElementString ();
						} else {
							UnknownNode (ob);
						}
					} else {
						UnknownNode (ob);
					}
				} else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}
	}

	internal class FaultWriter : XmlSerializationWriter
	{
		public void WriteRoot_Fault (object o)
		{
			WriteStartDocument ();
			System.Web.Services.Protocols.Fault ob = (System.Web.Services.Protocols.Fault) o;
			TopLevelElement ();
			WriteObject_Fault (ob, "Fault", WebServiceHelper.SoapEnvelopeNamespace, true, false, true);
		}

		void WriteObject_Fault (System.Web.Services.Protocols.Fault ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType ("Fault", WebServiceHelper.SoapEnvelopeNamespace);

			WriteElementQualifiedName ("faultcode", "", ob.@faultcode);
			WriteElementString ("faultstring", "", ob.@faultstring);
			WriteElementString ("faultactor", "", ob.@faultactor);
			WriteElementLiteral (ob.@detail, "detail", "", false, false);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}
	}
}

