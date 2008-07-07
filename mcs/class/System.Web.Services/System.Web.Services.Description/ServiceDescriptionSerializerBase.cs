#if !NET_2_0
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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "targetNamespace" && Reader.NamespaceURI == "") {
					ob.@TargetNamespace = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "location" && Reader.NamespaceURI == "") {
					ob.@Location = Reader.Value;
				}
				else if (Reader.LocalName == "namespace" && Reader.NamespaceURI == "") {
					ob.@Namespace = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@Type = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "binding" && Reader.NamespaceURI == "") {
					ob.@Binding = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "element" && Reader.NamespaceURI == "") {
					ob.@Element = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@Type = ToXmlQualifiedName (Reader.Value);
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "parameterOrder" && Reader.NamespaceURI == "") {
					ob.@ParameterOrderString = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.@Message = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

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

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("targetNamespace", "", ob.@TargetNamespace);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o59 = ob.@DocumentationElement;
				if (o59 is XmlElement) {
				if ((o59.LocalName == "documentation" && o59.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o59.WriteTo (Writer);
					WriteElementLiteral (o59, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o59.Name, o59.NamespaceURI);
			}
			if (ob.@Imports != null) {
				for (int n60 = 0; n60 < ob.@Imports.Count; n60++) {
					WriteObject_Import (ob.@Imports[n60], "import", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_Types (ob.@Types, "types", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (ob.@Messages != null) {
				for (int n61 = 0; n61 < ob.@Messages.Count; n61++) {
					WriteObject_Message (ob.@Messages[n61], "message", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@PortTypes != null) {
				for (int n62 = 0; n62 < ob.@PortTypes.Count; n62++) {
					WriteObject_PortType (ob.@PortTypes[n62], "portType", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Bindings != null) {
				for (int n63 = 0; n63 < ob.@Bindings.Count; n63++) {
					WriteObject_Binding (ob.@Bindings[n63], "binding", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Services != null) {
				for (int n64 = 0; n64 < ob.@Services.Count; n64++) {
					WriteObject_Service (ob.@Services[n64], "service", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("location", "", ob.@Location);
			WriteAttribute ("namespace", "", ob.@Namespace);

			if (ob.@DocumentationElement != null) {
				XmlNode o65 = ob.@DocumentationElement;
				if (o65 is XmlElement) {
				if ((o65.LocalName == "documentation" && o65.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o65.WriteTo (Writer);
					WriteElementLiteral (o65, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o65.Name, o65.NamespaceURI);
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

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o66 = ob.@DocumentationElement;
				if (o66 is XmlElement) {
				if ((o66.LocalName == "documentation" && o66.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o66.WriteTo (Writer);
					WriteElementLiteral (o66, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o66.Name, o66.NamespaceURI);
			}
			if (ob.@Schemas != null) {
				for (int n67 = 0; n67 < ob.@Schemas.Count; n67++) {
					WriteObject_XmlSchema (ob.@Schemas[n67], "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
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

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o68 = ob.@DocumentationElement;
				if (o68 is XmlElement) {
				if ((o68.LocalName == "documentation" && o68.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o68.WriteTo (Writer);
					WriteElementLiteral (o68, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o68.Name, o68.NamespaceURI);
			}
			if (ob.@Parts != null) {
				for (int n69 = 0; n69 < ob.@Parts.Count; n69++) {
					WriteObject_MessagePart (ob.@Parts[n69], "part", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o70 = ob.@DocumentationElement;
				if (o70 is XmlElement) {
				if ((o70.LocalName == "documentation" && o70.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o70.WriteTo (Writer);
					WriteElementLiteral (o70, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o70.Name, o70.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n71 = 0; n71 < ob.@Operations.Count; n71++) {
					WriteObject_Operation (ob.@Operations[n71], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

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
			if (ob.@Operations != null) {
				for (int n73 = 0; n73 < ob.@Operations.Count; n73++) {
					WriteObject_OperationBinding (ob.@Operations[n73], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o74 = ob.@DocumentationElement;
				if (o74 is XmlElement) {
				if ((o74.LocalName == "documentation" && o74.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o74.WriteTo (Writer);
					WriteElementLiteral (o74, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o74.Name, o74.NamespaceURI);
			}
			if (ob.@Ports != null) {
				for (int n75 = 0; n75 < ob.@Ports.Count; n75++) {
					WriteObject_Port (ob.@Ports[n75], "port", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("element", "", FromXmlQualifiedName (ob.@Element));
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

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

			WriteAttribute ("name", "", ob.@Name);
			if (ob.@ParameterOrderString != "") {
				WriteAttribute ("parameterOrder", "", ob.@ParameterOrderString);
			}

			if (ob.@DocumentationElement != null) {
				XmlNode o77 = ob.@DocumentationElement;
				if (o77 is XmlElement) {
				if ((o77.LocalName == "documentation" && o77.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o77.WriteTo (Writer);
					WriteElementLiteral (o77, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o77.Name, o77.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n78 = 0; n78 < ob.@Faults.Count; n78++) {
					WriteObject_OperationFault (ob.@Faults[n78], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Messages != null) {
				for (int n79 = 0; n79 < ob.@Messages.Count; n79++) {
					if (((object)ob.@Messages[n79]) == null) { }
					else if (ob.@Messages[n79].GetType() == typeof(System.Web.Services.Description.OperationOutput)) {
						WriteObject_OperationOutput (((System.Web.Services.Description.OperationOutput) ob.@Messages[n79]), "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.@Messages[n79].GetType() == typeof(System.Web.Services.Description.OperationInput)) {
						WriteObject_OperationInput (((System.Web.Services.Description.OperationInput) ob.@Messages[n79]), "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Messages[n79]);
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
			if (ob.@Faults != null) {
				for (int n81 = 0; n81 < ob.@Faults.Count; n81++) {
					WriteObject_FaultBinding (ob.@Faults[n81], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			WriteAttribute ("binding", "", FromXmlQualifiedName (ob.@Binding));
			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o82 = ob.@DocumentationElement;
				if (o82 is XmlElement) {
				if ((o82.LocalName == "documentation" && o82.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o82.WriteTo (Writer);
					WriteElementLiteral (o82, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o82.Name, o82.NamespaceURI);
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

			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));
			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o83 = ob.@DocumentationElement;
				if (o83 is XmlElement) {
				if ((o83.LocalName == "documentation" && o83.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o83.WriteTo (Writer);
					WriteElementLiteral (o83, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o83.Name, o83.NamespaceURI);
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

			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));
			WriteAttribute ("name", "", ob.@Name);

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

			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));
			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o85 = ob.@DocumentationElement;
				if (o85 is XmlElement) {
				if ((o85.LocalName == "documentation" && o85.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o85.WriteTo (Writer);
					WriteElementLiteral (o85, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o85.Name, o85.NamespaceURI);
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

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o86 = ob.@DocumentationElement;
				if (o86 is XmlElement) {
				if ((o86.LocalName == "documentation" && o86.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o86.WriteTo (Writer);
					WriteElementLiteral (o86, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o86.Name, o86.NamespaceURI);
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

			WriteAttribute ("name", "", ob.@Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.@DocumentationElement != null) {
				XmlNode o87 = ob.@DocumentationElement;
				if (o87 is XmlElement) {
				if ((o87.LocalName == "documentation" && o87.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o87.WriteTo (Writer);
					WriteElementLiteral (o87, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o87.Name, o87.NamespaceURI);
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
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}

}

#endif
