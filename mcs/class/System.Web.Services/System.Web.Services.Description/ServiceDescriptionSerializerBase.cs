using System;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Description
{
	internal class ServiceDescriptionReaderBase : XmlSerializationReader
	{
		public System.Web.Services.Description.ServiceDescription ReadTree ()
		{
			Reader.MoveToContent();
			return ReadObject_ServiceDescription (true, true);
		}

		public System.Web.Services.Description.ServiceDescription ReadObject_ServiceDescription (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.ServiceDescription ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "ServiceDescription" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.ServiceDescription ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b0=false, b1=false, b2=false, b3=false, b4=false, b5=false, b6=false;

			System.Web.Services.Description.ImportCollection o8 = ob.Imports;
			System.Web.Services.Description.MessageCollection o10 = ob.Messages;
			System.Web.Services.Description.PortTypeCollection o12 = ob.PortTypes;
			System.Web.Services.Description.BindingCollection o14 = ob.Bindings;
			System.Web.Services.Description.ServiceCollection o16 = ob.Services;
			int n7=0, n9=0, n11=0, n13=0, n15=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "binding" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b4) {
						if (o14 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.BindingCollection");
						o14.Add (ReadObject_Binding (false, true));
						n13++;
					}
					else if (Reader.LocalName == "service" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b5) {
						if (o16 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceCollection");
						o16.Add (ReadObject_Service (false, true));
						n15++;
					}
					else if (Reader.LocalName == "import" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b0) {
						if (o8 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ImportCollection");
						o8.Add (ReadObject_Import (false, true));
						n7++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b6) {
						b6 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "message" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b2) {
						if (o10 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.MessageCollection");
						o10.Add (ReadObject_Message (false, true));
						n9++;
					}
					else if (Reader.LocalName == "types" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b1) {
						b1 = true;
						ob.Types = ReadObject_Types (false, true);
					}
					else if (Reader.LocalName == "portType" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b3) {
						if (o12 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.PortTypeCollection");
						o12.Add (ReadObject_PortType (false, true));
						n11++;
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "Binding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Binding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.Type = ToXmlQualifiedName (Reader.Value);
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

			bool b17=false, b18=false;

			System.Web.Services.Description.OperationBindingCollection o20 = ob.Operations;
			int n19=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b17) {
						if (o20 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationBindingCollection");
						o20.Add (ReadObject_OperationBinding (false, true));
						n19++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b18) {
						b18 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "Service" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Service ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b21=false, b22=false;

			System.Web.Services.Description.PortCollection o24 = ob.Ports;
			int n23=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b22) {
						b22 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "port" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b21) {
						if (o24 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.PortCollection");
						o24.Add (ReadObject_Port (false, true));
						n23++;
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "Import" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Import ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "namespace" && Reader.NamespaceURI == "") {
					ob.Namespace = Reader.Value;
				}
				else if (Reader.LocalName == "location" && Reader.NamespaceURI == "") {
					ob.Location = Reader.Value;
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

			bool b25=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b25) {
						b25 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "Message" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Message ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b26=false, b27=false;

			System.Web.Services.Description.MessagePartCollection o29 = ob.Parts;
			int n28=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b27) {
						b27 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "part" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b26) {
						if (o29 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.MessagePartCollection");
						o29.Add (ReadObject_MessagePart (false, true));
						n28++;
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "Types" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Types ();

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

			bool b30=false, b31=false;

			System.Xml.Serialization.XmlSchemas o33 = ob.Schemas;
			int n32=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b31) {
						b31 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "schema" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b30) {
						if (o33 == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Serialization.XmlSchemas");
						o33.Add (ReadObject_XmlSchema (false, true));
						n32++;
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "PortType" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.PortType ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b34=false, b35=false;

			System.Web.Services.Description.OperationCollection o37 = ob.Operations;
			int n36=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b34) {
						if (o37 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationCollection");
						o37.Add (ReadObject_Operation (false, true));
						n36++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b35) {
						b35 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "OperationBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.OperationBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b38=false, b39=false, b40=false, b41=false;

			System.Web.Services.Description.FaultBindingCollection o43 = ob.Faults;
			int n42=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b39) {
						b39 = true;
						ob.Input = ReadObject_InputBinding (false, true);
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b38) {
						if (o43 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.FaultBindingCollection");
						o43.Add (ReadObject_FaultBinding (false, true));
						n42++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b41) {
						b41 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b40) {
						b40 = true;
						ob.Output = ReadObject_OutputBinding (false, true);
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}


			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.Port ReadObject_Port (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Port ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "Port" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Port ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "binding" && Reader.NamespaceURI == "") {
					ob.Binding = ToXmlQualifiedName (Reader.Value);
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

			bool b44=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b44) {
						b44 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "MessagePart" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.MessagePart ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "element" && Reader.NamespaceURI == "") {
					ob.Element = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.Type = ToXmlQualifiedName (Reader.Value);
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

			bool b45=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b45) {
						b45 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
			ob = System.Xml.Schema.XmlSchema.Read (Reader, null);
			return ob;
		}

		public System.Web.Services.Description.Operation ReadObject_Operation (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.Operation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t != null) 
				{
					if (t.Name != "Operation" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.Operation ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "parameterOrder" && Reader.NamespaceURI == "") {
					ob.ParameterOrderString = Reader.Value;
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

			bool b46=false, b47=false, b48=false;

			System.Web.Services.Description.OperationFaultCollection o50 = ob.Faults;
			System.Web.Services.Description.OperationMessageCollection o52 = ob.Messages;
			int n49=0, n51=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b47) {
						if (o52 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o52.Add (ReadObject_OperationInput (false, true));
						n51++;
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b46) {
						if (o50 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationFaultCollection");
						o50.Add (ReadObject_OperationFault (false, true));
						n49++;
					}
					else if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b48) {
						b48 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b47) {
						if (o52 == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o52.Add (ReadObject_OperationOutput (false, true));
						n51++;
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "InputBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.InputBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b53=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b53) {
						b53 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "FaultBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.FaultBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b54=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b54) {
						b54 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "OutputBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.OutputBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
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

			bool b55=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b55) {
						b55 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						ServiceDescription.ReadExtension (Reader, ob);
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
				if (t != null) 
				{
					if (t.Name != "OperationInput" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.OperationInput ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.Message = ToXmlQualifiedName (Reader.Value);
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

			bool b56=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b56) {
						b56 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "OperationFault" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.OperationFault ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.Message = ToXmlQualifiedName (Reader.Value);
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

			bool b57=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b57) {
						b57 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
				if (t != null) 
				{
					if (t.Name != "OperationOutput" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
						throw CreateUnknownTypeException(t);
				}
			}

			ob = new System.Web.Services.Description.OperationOutput ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.Name = Reader.Value;
				}
				else if (Reader.LocalName == "message" && Reader.NamespaceURI == "") {
					ob.Message = ToXmlQualifiedName (Reader.Value);
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

			bool b58=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b58) {
						b58 = true;
						ob.Documentation = Reader.ReadElementString ();
					}
					else {
						UnknownNode (ob);
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
		public void WriteTree (System.Web.Services.Description.ServiceDescription ob)
		{
			WriteStartDocument ();
			TopLevelElement ();
			WriteObject_ServiceDescription (ob, "definitions", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
		}

		void WriteObject_ServiceDescription (System.Web.Services.Description.ServiceDescription ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("ServiceDescription", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("targetNamespace", "", ob.TargetNamespace);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Imports != null) {
				for (int n59 = 0; n59 < ob.Imports.Count; n59++) {
					WriteObject_Import (ob.Imports[n59], "import", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_Types (ob.Types, "types", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (ob.Messages != null) {
				for (int n60 = 0; n60 < ob.Messages.Count; n60++) {
					WriteObject_Message (ob.Messages[n60], "message", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.PortTypes != null) {
				for (int n61 = 0; n61 < ob.PortTypes.Count; n61++) {
					WriteObject_PortType (ob.PortTypes[n61], "portType", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Bindings != null) {
				for (int n62 = 0; n62 < ob.Bindings.Count; n62++) {
					WriteObject_Binding (ob.Bindings[n62], "binding", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Services != null) {
				for (int n63 = 0; n63 < ob.Services.Count; n63++) {
					WriteObject_Service (ob.Services[n63], "service", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Import (System.Web.Services.Description.Import ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Import", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("namespace", "", ob.Namespace);
			WriteAttribute ("location", "", ob.Location);

			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Types (System.Web.Services.Description.Types ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Types", "http://schemas.xmlsoap.org/wsdl/");

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Schemas != null) {
				for (int n64 = 0; n64 < ob.Schemas.Count; n64++) {
					WriteObject_XmlSchema (ob.Schemas[n64], "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Message (System.Web.Services.Description.Message ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Message", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);

			if (ob.Parts != null) {
				for (int n65 = 0; n65 < ob.Parts.Count; n65++) {
					WriteObject_MessagePart (ob.Parts[n65], "part", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_PortType (System.Web.Services.Description.PortType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("PortType", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);

			if (ob.Operations != null) {
				for (int n66 = 0; n66 < ob.Operations.Count; n66++) {
					WriteObject_Operation (ob.Operations[n66], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Binding (System.Web.Services.Description.Binding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Binding", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.Type));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Operations != null) {
				for (int n67 = 0; n67 < ob.Operations.Count; n67++) {
					WriteObject_OperationBinding (ob.Operations[n67], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Service (System.Web.Services.Description.Service ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Service", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);

			if (ob.Ports != null) {
				for (int n68 = 0; n68 < ob.Ports.Count; n68++) {
					WriteObject_Port (ob.Ports[n68], "port", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchema (System.Xml.Schema.XmlSchema ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			ob.Write (Writer);
		}

		void WriteObject_MessagePart (System.Web.Services.Description.MessagePart ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("MessagePart", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("element", "", FromXmlQualifiedName (ob.Element));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.Type));

			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Operation (System.Web.Services.Description.Operation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Operation", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			if (ob.ParameterOrderString != "") {
				WriteAttribute ("parameterOrder", "", ob.ParameterOrderString);
			}

			if (ob.Faults != null) {
				for (int n69 = 0; n69 < ob.Faults.Count; n69++) {
					WriteObject_OperationFault (ob.Faults[n69], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.Messages != null) {
				for (int n70 = 0; n70 < ob.Messages.Count; n70++) {
					if (ob.Messages[n70] is System.Web.Services.Description.OperationInput) {
						WriteObject_OperationInput (((System.Web.Services.Description.OperationInput) ob.Messages[n70]), "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.Messages[n70] is System.Web.Services.Description.OperationOutput) {
						WriteObject_OperationOutput (((System.Web.Services.Description.OperationOutput) ob.Messages[n70]), "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.Messages[n70] != null) throw CreateUnknownTypeException (ob.Messages[n70]);
				}
			}
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationBinding (System.Web.Services.Description.OperationBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("OperationBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Faults != null) {
				for (int n71 = 0; n71 < ob.Faults.Count; n71++) {
					WriteObject_FaultBinding (ob.Faults[n71], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_InputBinding (ob.Input, "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			WriteObject_OutputBinding (ob.Output, "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Port (System.Web.Services.Description.Port ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("Port", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("binding", "", FromXmlQualifiedName (ob.Binding));

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationFault (System.Web.Services.Description.OperationFault ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("OperationFault", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.Message));

			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationInput (System.Web.Services.Description.OperationInput ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("OperationInput", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.Message));

			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationOutput (System.Web.Services.Description.OperationOutput ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("OperationOutput", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.Message));

			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_FaultBinding (System.Web.Services.Description.FaultBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("FaultBinding", "http://schemas.xmlsoap.org/wsdl/");

			if (ob.Name != "") {
				WriteAttribute ("name", "", ob.Name);
			}

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_InputBinding (System.Web.Services.Description.InputBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("InputBinding", "http://schemas.xmlsoap.org/wsdl/");

			if (ob.Name != "") {
				WriteAttribute ("name", "", ob.Name);
			}

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OutputBinding (System.Web.Services.Description.OutputBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
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

			if (needType) WriteXsiType("OutputBinding", "http://schemas.xmlsoap.org/wsdl/");

			if (ob.Name != "") {
				WriteAttribute ("name", "", ob.Name);
			}

			ServiceDescription.WriteExtensions (Writer, ob);
			if (ob.Documentation != "") {
				WriteElementString ("documentation", "http://schemas.xmlsoap.org/wsdl/", ob.Documentation);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}
}
