// 
// System.Web.Services.Protocols.DiscoveryDocumentSerializer.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian,Inc., 2003
//

using System;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Discovery
{
	internal class DiscoveryDocumentReader : XmlSerializationReader
	{
		public System.Web.Services.Discovery.DiscoveryDocument ReadTree ()
		{
			Reader.MoveToContent();
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

			bool b0=false;

			System.Collections.ArrayList o2 = null;
			int n1=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "discoveryRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/" && !b0) {
						if (o2 == null)
							o2 = new System.Collections.ArrayList();
						o2.Add (ReadObject_DiscoveryDocumentReference (false, true));
						n1++;
					}
					else if (Reader.LocalName == "contractRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/scl/" && !b0) {
						if (o2 == null)
							o2 = new System.Collections.ArrayList();
						o2.Add (ReadObject_ContractReference (false, true));
						n1++;
					}
					else if (Reader.LocalName == "schemaRef" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/disco/" && !b0) {
						if (o2 == null)
							o2 = new System.Collections.ArrayList();
						o2.Add (ReadObject_SchemaReference (false, true));
						n1++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ob.references = o2;

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
					ob.Ref = Reader.Value;
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
					if (t.Name != "ContractReference" || t.Namespace != "https://schemas.xmlsoap.org/disco/scl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Discovery.ContractReference ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "docRef" && Reader.NamespaceURI == "") {
					ob.DocRef = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.Ref = Reader.Value;
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
				if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.Ref = Reader.Value;
				}
				else if (Reader.LocalName == "targetNamespace" && Reader.NamespaceURI == "") {
					ob.TargetNamespace = Reader.Value;
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
		public void WriteTree (System.Web.Services.Discovery.DiscoveryDocument ob)
		{
			WriteStartDocument ();
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

			if (ob.references != null) {
				for (int n3 = 0; n3 < ob.references.Count; n3++) {
					if (ob.references[n3] is System.Web.Services.Discovery.SchemaReference) {
						WriteObject_SchemaReference (((System.Web.Services.Discovery.SchemaReference) ob.references[n3]), "schemaRef", "http://schemas.xmlsoap.org/disco/", false, false, true);
					}
					else if (ob.references[n3] is System.Web.Services.Discovery.DiscoveryDocumentReference) {
						WriteObject_DiscoveryDocumentReference (((System.Web.Services.Discovery.DiscoveryDocumentReference) ob.references[n3]), "discoveryRef", "http://schemas.xmlsoap.org/disco/", false, false, true);
					}
					else if (ob.references[n3] is System.Web.Services.Discovery.ContractReference) {
						WriteObject_ContractReference (((System.Web.Services.Discovery.ContractReference) ob.references[n3]), "contractRef", "http://schemas.xmlsoap.org/disco/", false, false, true);
					}
					else if (ob.references[n3] != null) throw CreateUnknownTypeException (ob.references[n3]);
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

			WriteAttribute ("ref", "", ob.Ref);
			if (ob.TargetNamespace != "") {
				WriteAttribute ("targetNamespace", "", ob.TargetNamespace);
			}

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

			WriteAttribute ("ref", "", ob.Ref);

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

			if (needType) WriteXsiType("ContractReference", "https://schemas.xmlsoap.org/disco/scl/");

			WriteAttribute ("docRef", "", ob.DocRef);
			WriteAttribute ("ref", "", ob.Ref);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}
}
