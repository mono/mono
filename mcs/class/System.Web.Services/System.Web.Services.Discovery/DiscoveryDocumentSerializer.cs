// 
// System.Web.Services.Protocols.DiscoveryDocumentSerializer.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian,Inc., 2003
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

namespace System.Web.Services.Discovery
{
	internal class DiscoveryDocumentReader : XmlSerializationReader
	{
		public object ReadRoot_DiscoveryDocument ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "discovery" || Reader.NamespaceURI != "http://schemas.xmlsoap.org/disco/")
				throw CreateUnknownNodeException();
			return ReadObject_DiscoveryDocument (true, true);
		}

		public System.Web.Services.Discovery.DiscoveryDocument ReadObject_DiscoveryDocument (bool isNullable, bool checkType)
		{
			System.Web.Services.Discovery.DiscoveryDocument ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "DiscoveryDocument" || t.Namespace != "http://schemas.xmlsoap.org/disco/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.DiscoveryDocument ();

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

			bool b0=false, b1=false;

			System.Collections.ArrayList o3 = null;
			System.Collections.ArrayList o5 = null;
			int n2=0, n4=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "discoveryRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/" && !b0) {
						if (o3 == null)
							o3 = new System.Collections.ArrayList();
						o3.Add (ReadObject_DiscoveryDocumentReference (false, true));
						n2++;
					}
					else if (Reader.LocalName == "soap" && Reader.NamespaceURI == "http://schemas/xmlsoap.org/disco/schema/soap/" && !b1) {
						if (o5 == null)
							o5 = new System.Collections.ArrayList();
						o5.Add (ReadObject_SoapBinding (false, true));
						n4++;
					}
					else if (Reader.LocalName == "contractRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/scl/" && !b0) {
						if (o3 == null)
							o3 = new System.Collections.ArrayList();
						o3.Add (ReadObject_ContractReference (false, true));
						n2++;
					}
					else if (Reader.LocalName == "schemaRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/" && !b0) {
						if (o3 == null)
							o3 = new System.Collections.ArrayList();
						o3.Add (ReadObject_SchemaReference (false, true));
						n2++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ob.@references = o3;
			ob.@additionalInfo = o5;

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Discovery.DiscoveryDocumentReference ReadObject_DiscoveryDocumentReference (bool isNullable, bool checkType)
		{
			System.Web.Services.Discovery.DiscoveryDocumentReference ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "DiscoveryDocumentReference" || t.Namespace != "http://schemas.xmlsoap.org/disco/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.DiscoveryDocumentReference ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@Ref = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Discovery.SoapBinding ReadObject_SoapBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Discovery.SoapBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "SoapBinding" || t.Namespace != "http://schemas/xmlsoap.org/disco/schema/soap/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.SoapBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "binding" && Reader.NamespaceURI == "") {
					ob.@Binding = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "address" && Reader.NamespaceURI == "") {
					ob.@Address = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Discovery.ContractReference ReadObject_ContractReference (bool isNullable, bool checkType)
		{
			System.Web.Services.Discovery.ContractReference ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "ContractReference" || t.Namespace != "http://schemas.xmlsoap.org/disco/scl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.ContractReference ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "docRef" && Reader.NamespaceURI == "") {
					ob.@DocRef = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@Ref = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Discovery.SchemaReference ReadObject_SchemaReference (bool isNullable, bool checkType)
		{
			System.Web.Services.Discovery.SchemaReference ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "SchemaReference" || t.Namespace != "http://schemas/xmlsoap.org/disco/schema/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.SchemaReference ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "targetNamespace" && Reader.NamespaceURI == "") {
					ob.@TargetNamespace = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@Ref = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
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

	internal class DiscoveryDocumentWriter : XmlSerializationWriter
	{
		public void WriteRoot_DiscoveryDocument (object o)
		{
			WriteStartDocument ();
			System.Web.Services.Discovery.DiscoveryDocument ob = (System.Web.Services.Discovery.DiscoveryDocument) o;
			TopLevelElement ();
			WriteObject_DiscoveryDocument (ob, "discovery", "http://schemas.xmlsoap.org/disco/", true, false, true);
		}

		void WriteObject_DiscoveryDocument (System.Web.Services.Discovery.DiscoveryDocument ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("DiscoveryDocument", "http://schemas.xmlsoap.org/disco/");

			if (ob.@references != null) {
				for (int n6 = 0; n6 < ob.@references.Count; n6++) {
					if (ob.@references[n6] == null) { }
					else if (ob.@references[n6].GetType() == typeof(System.Web.Services.Discovery.SchemaReference)) {
						WriteObject_SchemaReference (((System.Web.Services.Discovery.SchemaReference) ob.@references[n6]), "schemaRef", "http://schemas.xmlsoap.org/disco/", false, false, true);
					}
					else if (ob.@references[n6].GetType() == typeof(System.Web.Services.Discovery.DiscoveryDocumentReference)) {
						WriteObject_DiscoveryDocumentReference (((System.Web.Services.Discovery.DiscoveryDocumentReference) ob.@references[n6]), "discoveryRef", "http://schemas.xmlsoap.org/disco/", false, false, true);
					}
					else if (ob.@references[n6].GetType() == typeof(System.Web.Services.Discovery.ContractReference)) {
						WriteObject_ContractReference (((System.Web.Services.Discovery.ContractReference) ob.@references[n6]), "contractRef", "http://schemas.xmlsoap.org/disco/scl/", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@references[n6]);
				}
			}
			if (ob.@additionalInfo != null) {
				for (int n7 = 0; n7 < ob.@additionalInfo.Count; n7++) {
					WriteObject_SoapBinding (((System.Web.Services.Discovery.SoapBinding) ob.@additionalInfo[n7]), "soap", "http://schemas/xmlsoap.org/disco/schema/soap/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_SchemaReference (System.Web.Services.Discovery.SchemaReference ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("SchemaReference", "http://schemas/xmlsoap.org/disco/schema/");

			if (ob.@TargetNamespace != "") {
				WriteAttribute ("targetNamespace", "", ob.@TargetNamespace);
			}
			WriteAttribute ("ref", "", ob.@Ref);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_DiscoveryDocumentReference (System.Web.Services.Discovery.DiscoveryDocumentReference ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("DiscoveryDocumentReference", "http://schemas.xmlsoap.org/disco/");

			WriteAttribute ("ref", "", ob.@Ref);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ContractReference (System.Web.Services.Discovery.ContractReference ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("ContractReference", "http://schemas.xmlsoap.org/disco/scl/");

			WriteAttribute ("docRef", "", ob.@DocRef);
			WriteAttribute ("ref", "", ob.@Ref);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_SoapBinding (System.Web.Services.Discovery.SoapBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("SoapBinding", "http://schemas/xmlsoap.org/disco/schema/soap/");

			WriteAttribute ("binding", "", FromXmlQualifiedName (ob.@Binding));
			WriteAttribute ("address", "", ob.@Address);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}

}

