#if NET_2_0
// It is automatically generated
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Description
{
	internal class ServiceDescriptionReaderBase : XmlSerializationReader
	{
		static readonly System.Reflection.MethodInfo fromBinHexStringMethod = typeof (XmlConvert).GetMethod ("FromBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (string)}, null);
		static byte [] FromBinHexString (string input)
		{
			return input == null ? null : (byte []) fromBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public object ReadRoot_ServiceDescription ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "definitions" || Reader.NamespaceURI != "http://schemas.xmlsoap.org/wsdl/")
				throw CreateUnknownNodeException();
			return ReadObject_ServiceDescription (true, true);
		}

		public System.Web.Services.Description.ServiceDescription ReadObject_ServiceDescription (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.ServiceDescription ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "ServiceDescription" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.ServiceDescription) Activator.CreateInstance(typeof(System.Web.Services.Description.ServiceDescription), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "targetNamespace" && Reader.NamespaceURI == "") {
					ob.@TargetNamespace = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b0=false, b1=false, b2=false, b3=false, b4=false, b5=false, b6=false;

			System.Web.Services.Description.ImportCollection o8;
			o8 = ob.@Imports;
			System.Web.Services.Description.MessageCollection o10;
			o10 = ob.@Messages;
			System.Web.Services.Description.PortTypeCollection o12;
			o12 = ob.@PortTypes;
			System.Web.Services.Description.BindingCollection o14;
			o14 = ob.@Bindings;
			System.Web.Services.Description.ServiceCollection o16;
			o16 = ob.@Services;
			int n7=0, n9=0, n11=0, n13=0, n15=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "types" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b2) {
						b2 = true;
						ob.@Types = ReadObject_Types (false, true);
					}
					else if (Reader.LocalName == "service" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b6) {
						if (((object)o16) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceCollection");
						o16.Add (ReadObject_Service (false, true));
						n15++;
					}
					else if (Reader.LocalName == "message" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b3) {
						if (((object)o10) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.MessageCollection");
						o10.Add (ReadObject_Message (false, true));
						n9++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b0) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "portType" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b4) {
						if (((object)o12) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.PortTypeCollection");
						o12.Add (ReadObject_PortType (false, true));
						n11++;
					}
					else if (Reader.LocalName == "import" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b1) {
						if (((object)o8) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ImportCollection");
						o8.Add (ReadObject_Import (false, true));
						n7++;
					}
					else if (Reader.LocalName == "binding" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b5) {
						if (((object)o14) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.BindingCollection");
						o14.Add (ReadObject_Binding (false, true));
						n13++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Types ReadObject_Types (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Types ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Types" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Types) Activator.CreateInstance(typeof(System.Web.Services.Description.Types), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b17=false, b18=false;

			System.Xml.Serialization.XmlSchemas o20;
			o20 = ob.@Schemas;
			int n19=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b17) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "schema" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b18) {
						if (((object)o20) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Serialization.XmlSchemas");
						o20.Add (ReadObject_XmlSchema (false, true));
						n19++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Service ReadObject_Service (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Service ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Service" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Service) Activator.CreateInstance(typeof(System.Web.Services.Description.Service), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b21=false, b22=false;

			System.Web.Services.Description.PortCollection o24;
			o24 = ob.@Ports;
			int n23=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b21) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "port" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b22) {
						if (((object)o24) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.PortCollection");
						o24.Add (ReadObject_Port (false, true));
						n23++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Message ReadObject_Message (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Message ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Message" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Message) Activator.CreateInstance(typeof(System.Web.Services.Description.Message), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b25=false, b26=false;

			System.Web.Services.Description.MessagePartCollection o28;
			o28 = ob.@Parts;
			int n27=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b25) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "part" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b26) {
						if (((object)o28) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.MessagePartCollection");
						o28.Add (ReadObject_MessagePart (false, true));
						n27++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.PortType ReadObject_PortType (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.PortType ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "PortType" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.PortType) Activator.CreateInstance(typeof(System.Web.Services.Description.PortType), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b29=false, b30=false;

			System.Web.Services.Description.OperationCollection o32;
			o32 = ob.@Operations;
			int n31=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b29) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b30) {
						if (((object)o32) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationCollection");
						o32.Add (ReadObject_Operation (false, true));
						n31++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Import ReadObject_Import (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Import ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Import" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Import) Activator.CreateInstance(typeof(System.Web.Services.Description.Import), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "location" && Reader.NamespaceURI == "") {
					ob.@Location = Reader.Value;
				}
				else if (Reader.LocalName == "namespace" && Reader.NamespaceURI == "") {
					ob.@Namespace = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b33=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b33) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Binding ReadObject_Binding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Binding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Binding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Binding) Activator.CreateInstance(typeof(System.Web.Services.Description.Binding), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@Type = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b34=false, b35=false;

			System.Web.Services.Description.OperationBindingCollection o37;
			o37 = ob.@Operations;
			int n36=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b34) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b35) {
						if (((object)o37) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationBindingCollection");
						o37.Add (ReadObject_OperationBinding (false, true));
						n36++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Xml.Schema.XmlSchema ReadObject_XmlSchema (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchema ob = null;
			ob = System.Xml.Schema.XmlSchema.Read (Reader, null); Reader.Read ();
			return ob;
		}

		public System.Web.Services.Description.Port ReadObject_Port (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Port ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Port" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Port) Activator.CreateInstance(typeof(System.Web.Services.Description.Port), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "binding" && Reader.NamespaceURI == "") {
					ob.@Binding = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b38=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b38) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.MessagePart ReadObject_MessagePart (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.MessagePart ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "MessagePart" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.MessagePart) Activator.CreateInstance(typeof(System.Web.Services.Description.MessagePart), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "element" && Reader.NamespaceURI == "") {
					ob.@Element = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@Type = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b39=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b39) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Operation ReadObject_Operation (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Operation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Operation" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.Operation) Activator.CreateInstance(typeof(System.Web.Services.Description.Operation), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "parameterOrder" && Reader.NamespaceURI == "") {
					ob.@ParameterOrderString = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b40=false, b41=false, b42=false;

			System.Web.Services.Description.OperationFaultCollection o44;
			o44 = ob.@Faults;
			System.Web.Services.Description.OperationMessageCollection o46;
			o46 = ob.@Messages;
			int n43=0, n45=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b42) {
						if (((object)o46) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o46.Add (ReadObject_OperationOutput (false, true));
						n45++;
					}
					else if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b42) {
						if (((object)o46) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o46.Add (ReadObject_OperationInput (false, true));
						n45++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b40) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b41) {
						if (((object)o44) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationFaultCollection");
						o44.Add (ReadObject_OperationFault (false, true));
						n43++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.OperationBinding ReadObject_OperationBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OperationBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "OperationBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.OperationBinding) Activator.CreateInstance(typeof(System.Web.Services.Description.OperationBinding), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b47=false, b48=false, b49=false, b50=false;

			System.Web.Services.Description.FaultBindingCollection o52;
			o52 = ob.@Faults;
			int n51=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b49) {
						b49 = true;
						ob.@Input = ReadObject_InputBinding (false, true);
					}
					else if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b50) {
						b50 = true;
						ob.@Output = ReadObject_OutputBinding (false, true);
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b47) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b48) {
						if (((object)o52) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.FaultBindingCollection");
						o52.Add (ReadObject_FaultBinding (false, true));
						n51++;
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.OperationOutput ReadObject_OperationOutput (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OperationOutput ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "OperationOutput" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.OperationOutput) Activator.CreateInstance(typeof(System.Web.Services.Description.OperationOutput), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b53=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b53) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.OperationInput ReadObject_OperationInput (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OperationInput ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "OperationInput" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.OperationInput) Activator.CreateInstance(typeof(System.Web.Services.Description.OperationInput), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b54=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b54) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.OperationFault ReadObject_OperationFault (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OperationFault ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "OperationFault" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.OperationFault) Activator.CreateInstance(typeof(System.Web.Services.Description.OperationFault), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b55=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b55) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.InputBinding ReadObject_InputBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.InputBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "InputBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.InputBinding) Activator.CreateInstance(typeof(System.Web.Services.Description.InputBinding), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b56=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b56) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.OutputBinding ReadObject_OutputBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OutputBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "OutputBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.OutputBinding) Activator.CreateInstance(typeof(System.Web.Services.Description.OutputBinding), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b57=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b57) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.FaultBinding ReadObject_FaultBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.FaultBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "FaultBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = (System.Web.Services.Description.FaultBinding) Activator.CreateInstance(typeof(System.Web.Services.Description.FaultBinding), true);

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@ExtensibleAttributes = anyAttributeArray;

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b58=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b58) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else {
						ServiceDescription.ReadExtension (Document, Reader, ob);
					}
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

	internal class ServiceDescriptionWriterBase : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		static readonly System.Reflection.MethodInfo toBinHexStringMethod = typeof (XmlConvert).GetMethod ("ToBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (byte [])}, null);
		static string ToBinHexString (byte [] input)
		{
			return input == null ? null : (string) toBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public void WriteRoot_ServiceDescription (object o)
		{
			WriteStartDocument ();
			System.Web.Services.Description.ServiceDescription ob = (System.Web.Services.Description.ServiceDescription) o;
			TopLevelElement ();
			WriteObject_ServiceDescription (ob, "definitions", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
		}

		void WriteObject_ServiceDescription (System.Web.Services.Description.ServiceDescription ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.ServiceDescription))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ServiceDescription", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o59 = ob.@ExtensibleAttributes;
			if (o59 != null) {
				foreach (XmlAttribute o60 in o59)
					if (o60.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o60, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("targetNamespace", "", ob.@TargetNamespace);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o61 = ob.@DocumentationElement;
				if (o61 is XmlElement) {
				if ((o61.LocalName == "documentation" && o61.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o61.WriteTo (Writer);
					WriteElementLiteral (o61, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o61.Name, o61.NamespaceURI);
			}
			if (ob.@Imports != null) {
				for (int n62 = 0; n62 < ob.@Imports.Count; n62++) {
					WriteObject_Import (ob.@Imports[n62], "import", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_Types (ob.@Types, "types", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (ob.@Messages != null) {
				for (int n63 = 0; n63 < ob.@Messages.Count; n63++) {
					WriteObject_Message (ob.@Messages[n63], "message", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@PortTypes != null) {
				for (int n64 = 0; n64 < ob.@PortTypes.Count; n64++) {
					WriteObject_PortType (ob.@PortTypes[n64], "portType", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Bindings != null) {
				for (int n65 = 0; n65 < ob.@Bindings.Count; n65++) {
					WriteObject_Binding (ob.@Bindings[n65], "binding", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Services != null) {
				for (int n66 = 0; n66 < ob.@Services.Count; n66++) {
					WriteObject_Service (ob.@Services[n66], "service", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Import (System.Web.Services.Description.Import ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Import))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Import", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o67 = ob.@ExtensibleAttributes;
			if (o67 != null) {
				foreach (XmlAttribute o68 in o67)
					if (o68.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o68, ob);
			}

			WriteAttribute ("location", "", ob.@Location);
			WriteAttribute ("namespace", "", ob.@Namespace);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o69 = ob.@DocumentationElement;
				if (o69 is XmlElement) {
				if ((o69.LocalName == "documentation" && o69.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o69.WriteTo (Writer);
					WriteElementLiteral (o69, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o69.Name, o69.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Types (System.Web.Services.Description.Types ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Types))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Types", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o70 = ob.@ExtensibleAttributes;
			if (o70 != null) {
				foreach (XmlAttribute o71 in o70)
					if (o71.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o71, ob);
			}

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o72 = ob.@DocumentationElement;
				if (o72 is XmlElement) {
				if ((o72.LocalName == "documentation" && o72.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o72.WriteTo (Writer);
					WriteElementLiteral (o72, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o72.Name, o72.NamespaceURI);
			}
			if (ob.@Schemas != null) {
				for (int n73 = 0; n73 < ob.@Schemas.Count; n73++) {
					WriteObject_XmlSchema (ob.@Schemas[n73], "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Message (System.Web.Services.Description.Message ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Message))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Message", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o74 = ob.@ExtensibleAttributes;
			if (o74 != null) {
				foreach (XmlAttribute o75 in o74)
					if (o75.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o75, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o76 = ob.@DocumentationElement;
				if (o76 is XmlElement) {
				if ((o76.LocalName == "documentation" && o76.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o76.WriteTo (Writer);
					WriteElementLiteral (o76, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o76.Name, o76.NamespaceURI);
			}
			if (ob.@Parts != null) {
				for (int n77 = 0; n77 < ob.@Parts.Count; n77++) {
					WriteObject_MessagePart (ob.@Parts[n77], "part", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_PortType (System.Web.Services.Description.PortType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.PortType))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("PortType", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o78 = ob.@ExtensibleAttributes;
			if (o78 != null) {
				foreach (XmlAttribute o79 in o78)
					if (o79.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o79, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o80 = ob.@DocumentationElement;
				if (o80 is XmlElement) {
				if ((o80.LocalName == "documentation" && o80.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o80.WriteTo (Writer);
					WriteElementLiteral (o80, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o80.Name, o80.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n81 = 0; n81 < ob.@Operations.Count; n81++) {
					WriteObject_Operation (ob.@Operations[n81], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Binding (System.Web.Services.Description.Binding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Binding))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Binding", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o82 = ob.@ExtensibleAttributes;
			if (o82 != null) {
				foreach (XmlAttribute o83 in o82)
					if (o83.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o83, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o84 = ob.@DocumentationElement;
				if (o84 is XmlElement) {
				if ((o84.LocalName == "documentation" && o84.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o84.WriteTo (Writer);
					WriteElementLiteral (o84, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o84.Name, o84.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n85 = 0; n85 < ob.@Operations.Count; n85++) {
					WriteObject_OperationBinding (ob.@Operations[n85], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Service (System.Web.Services.Description.Service ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Service))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Service", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o86 = ob.@ExtensibleAttributes;
			if (o86 != null) {
				foreach (XmlAttribute o87 in o86)
					if (o87.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o87, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o88 = ob.@DocumentationElement;
				if (o88 is XmlElement) {
				if ((o88.LocalName == "documentation" && o88.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o88.WriteTo (Writer);
					WriteElementLiteral (o88, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o88.Name, o88.NamespaceURI);
			}
			if (ob.@Ports != null) {
				for (int n89 = 0; n89 < ob.@Ports.Count; n89++) {
					WriteObject_Port (ob.@Ports[n89], "port", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchema (System.Xml.Schema.XmlSchema ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			ob.Write (Writer);
		}

		void WriteObject_MessagePart (System.Web.Services.Description.MessagePart ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.MessagePart))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("MessagePart", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o90 = ob.@ExtensibleAttributes;
			if (o90 != null) {
				foreach (XmlAttribute o91 in o90)
					if (o91.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o91, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("element", "", FromXmlQualifiedName (ob.@Element));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o92 = ob.@DocumentationElement;
				if (o92 is XmlElement) {
				if ((o92.LocalName == "documentation" && o92.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o92.WriteTo (Writer);
					WriteElementLiteral (o92, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o92.Name, o92.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Operation (System.Web.Services.Description.Operation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Operation))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Operation", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o93 = ob.@ExtensibleAttributes;
			if (o93 != null) {
				foreach (XmlAttribute o94 in o93)
					if (o94.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o94, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			if (ob.@ParameterOrderString != "") {
				WriteAttribute ("parameterOrder", "", ob.@ParameterOrderString);
			}

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o95 = ob.@DocumentationElement;
				if (o95 is XmlElement) {
				if ((o95.LocalName == "documentation" && o95.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o95.WriteTo (Writer);
					WriteElementLiteral (o95, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o95.Name, o95.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n96 = 0; n96 < ob.@Faults.Count; n96++) {
					WriteObject_OperationFault (ob.@Faults[n96], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Messages != null) {
				for (int n97 = 0; n97 < ob.@Messages.Count; n97++) {
					if (((object)ob.@Messages[n97]) == null) { }
					else if (ob.@Messages[n97].GetType() == typeof(System.Web.Services.Description.OperationOutput)) {
						WriteObject_OperationOutput (((System.Web.Services.Description.OperationOutput) ob.@Messages[n97]), "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.@Messages[n97].GetType() == typeof(System.Web.Services.Description.OperationInput)) {
						WriteObject_OperationInput (((System.Web.Services.Description.OperationInput) ob.@Messages[n97]), "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Messages[n97]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationBinding (System.Web.Services.Description.OperationBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OperationBinding))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OperationBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o98 = ob.@ExtensibleAttributes;
			if (o98 != null) {
				foreach (XmlAttribute o99 in o98)
					if (o99.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o99, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o100 = ob.@DocumentationElement;
				if (o100 is XmlElement) {
				if ((o100.LocalName == "documentation" && o100.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o100.WriteTo (Writer);
					WriteElementLiteral (o100, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o100.Name, o100.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n101 = 0; n101 < ob.@Faults.Count; n101++) {
					WriteObject_FaultBinding (ob.@Faults[n101], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_InputBinding (ob.@Input, "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			WriteObject_OutputBinding (ob.@Output, "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Port (System.Web.Services.Description.Port ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.Port))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Port", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o102 = ob.@ExtensibleAttributes;
			if (o102 != null) {
				foreach (XmlAttribute o103 in o102)
					if (o103.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o103, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("binding", "", FromXmlQualifiedName (ob.@Binding));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o104 = ob.@DocumentationElement;
				if (o104 is XmlElement) {
				if ((o104.LocalName == "documentation" && o104.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o104.WriteTo (Writer);
					WriteElementLiteral (o104, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o104.Name, o104.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationFault (System.Web.Services.Description.OperationFault ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OperationFault))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OperationFault", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o105 = ob.@ExtensibleAttributes;
			if (o105 != null) {
				foreach (XmlAttribute o106 in o105)
					if (o106.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o106, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o107 = ob.@DocumentationElement;
				if (o107 is XmlElement) {
				if ((o107.LocalName == "documentation" && o107.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o107.WriteTo (Writer);
					WriteElementLiteral (o107, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o107.Name, o107.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationOutput (System.Web.Services.Description.OperationOutput ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OperationOutput))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OperationOutput", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o108 = ob.@ExtensibleAttributes;
			if (o108 != null) {
				foreach (XmlAttribute o109 in o108)
					if (o109.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o109, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o110 = ob.@DocumentationElement;
				if (o110 is XmlElement) {
				if ((o110.LocalName == "documentation" && o110.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o110.WriteTo (Writer);
					WriteElementLiteral (o110, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o110.Name, o110.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationInput (System.Web.Services.Description.OperationInput ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OperationInput))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OperationInput", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o111 = ob.@ExtensibleAttributes;
			if (o111 != null) {
				foreach (XmlAttribute o112 in o111)
					if (o112.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o112, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o113 = ob.@DocumentationElement;
				if (o113 is XmlElement) {
				if ((o113.LocalName == "documentation" && o113.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o113.WriteTo (Writer);
					WriteElementLiteral (o113, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o113.Name, o113.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_FaultBinding (System.Web.Services.Description.FaultBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.FaultBinding))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("FaultBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o114 = ob.@ExtensibleAttributes;
			if (o114 != null) {
				foreach (XmlAttribute o115 in o114)
					if (o115.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o115, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o116 = ob.@DocumentationElement;
				if (o116 is XmlElement) {
				if ((o116.LocalName == "documentation" && o116.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o116.WriteTo (Writer);
					WriteElementLiteral (o116, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o116.Name, o116.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_InputBinding (System.Web.Services.Description.InputBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.InputBinding))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("InputBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o117 = ob.@ExtensibleAttributes;
			if (o117 != null) {
				foreach (XmlAttribute o118 in o117)
					if (o118.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o118, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o119 = ob.@DocumentationElement;
				if (o119 is XmlElement) {
				if ((o119.LocalName == "documentation" && o119.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o119.WriteTo (Writer);
					WriteElementLiteral (o119, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o119.Name, o119.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OutputBinding (System.Web.Services.Description.OutputBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OutputBinding))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OutputBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o120 = ob.@ExtensibleAttributes;
			if (o120 != null) {
				foreach (XmlAttribute o121 in o120)
					if (o121.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o121, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o122 = ob.@DocumentationElement;
				if (o122 is XmlElement) {
				if ((o122.LocalName == "documentation" && o122.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o122.WriteTo (Writer);
					WriteElementLiteral (o122, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o122.Name, o122.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}


	internal class BaseXmlSerializer : System.Xml.Serialization.XmlSerializer
	{
		protected override System.Xml.Serialization.XmlSerializationReader CreateReader () {
			return new ServiceDescriptionReaderBase ();
		}

		protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {
			return new ServiceDescriptionWriterBase ();
		}

		public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {
			return true;
		}
	}

	internal sealed class definitionsSerializer : BaseXmlSerializer
	{
		protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {
			((ServiceDescriptionWriterBase)writer).WriteRoot_ServiceDescription(obj);
		}

		protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {
			return ((ServiceDescriptionReaderBase)reader).ReadRoot_ServiceDescription();
		}
	}

	#if !TARGET_JVM
	internal class XmlSerializerContract : System.Xml.Serialization.XmlSerializerImplementation
	{
		System.Collections.Hashtable readMethods = null;
		System.Collections.Hashtable writeMethods = null;
		System.Collections.Hashtable typedSerializers = null;

		public override System.Xml.Serialization.XmlSerializationReader Reader {
			get {
				return new ServiceDescriptionReaderBase();
			}
		}

		public override System.Xml.Serialization.XmlSerializationWriter Writer {
			get {
				return new ServiceDescriptionWriterBase();
			}
		}

		public override System.Collections.Hashtable ReadMethods {
			get {
				lock (this) {
					if (readMethods == null) {
						readMethods = new System.Collections.Hashtable ();
						readMethods.Add (@"System.Web.Services.Description.ServiceDescription", @"ReadRoot_ServiceDescription");
					}
					return readMethods;
				}
			}
		}

		public override System.Collections.Hashtable WriteMethods {
			get {
				lock (this) {
					if (writeMethods == null) {
						writeMethods = new System.Collections.Hashtable ();
						writeMethods.Add (@"System.Web.Services.Description.ServiceDescription", @"WriteRoot_ServiceDescription");
					}
					return writeMethods;
				}
			}
		}

		public override System.Collections.Hashtable TypedSerializers {
			get {
				lock (this) {
					if (typedSerializers == null) {
						typedSerializers = new System.Collections.Hashtable ();
						typedSerializers.Add (@"System.Web.Services.Description.ServiceDescription", new definitionsSerializer());
					}
					return typedSerializers;
				}
			}
		}

		public override XmlSerializer GetSerializer (Type type)
		{
			switch (type.FullName) {
			case "System.Web.Services.Description.ServiceDescription":
				return (XmlSerializer) TypedSerializers ["System.Web.Services.Description.ServiceDescription"];

			}
			return base.GetSerializer (type);
		}

		public override bool CanSerialize (System.Type type) {
			if (type == typeof(System.Web.Services.Description.ServiceDescription)) return true;
			return false;
		}
	}

	#endif
}

#endif
