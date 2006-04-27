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

namespace System.Web.Services.Description
{
	internal class ServiceDescriptionReaderBase : XmlSerializationReader
	{
		public System.Web.Services.Description.ServiceDescription ReadTree ()
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

			ob = new System.Web.Services.Description.ServiceDescription ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b0) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
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
					else if (Reader.LocalName == "types" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b2) {
						b2 = true;
						ob.@Types = ReadObject_Types (false, true);
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
				if (t == null)
				{ }
				else if (t.Name != "Service" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.Service ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b17=false, b18=false;

			System.Web.Services.Description.PortCollection o20;
			o20 = ob.@Ports;
			int n19=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b17) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "port" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b18) {
						if (((object)o20) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.PortCollection");
						o20.Add (ReadObject_Port (false, true));
						n19++;
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
				if (t == null)
				{ }
				else if (t.Name != "Message" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.Message ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b21=false, b22=false;

			System.Web.Services.Description.MessagePartCollection o24;
			o24 = ob.@Parts;
			int n23=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b21) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "part" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b22) {
						if (((object)o24) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.MessagePartCollection");
						o24.Add (ReadObject_MessagePart (false, true));
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

			ob = new System.Web.Services.Description.PortType ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b25=false, b26=false;

			System.Web.Services.Description.OperationCollection o28;
			o28 = ob.@Operations;
			int n27=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b25) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b26) {
						if (((object)o28) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationCollection");
						o28.Add (ReadObject_Operation (false, true));
						n27++;
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
				if (t == null)
				{ }
				else if (t.Name != "Import" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.Import ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b29=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b29) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
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

			ob = new System.Web.Services.Description.Binding ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b30=false, b31=false;

			System.Web.Services.Description.OperationBindingCollection o33;
			o33 = ob.@Operations;
			int n32=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b30) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "operation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b31) {
						if (((object)o33) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationBindingCollection");
						o33.Add (ReadObject_OperationBinding (false, true));
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

			ob = new System.Web.Services.Description.Types ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b34=false, b35=false;

			System.Xml.Serialization.XmlSchemas o37;
			o37 = ob.@Schemas;
			int n36=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b34) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "schema" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b35) {
						if (((object)o37) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Serialization.XmlSchemas");
						o37.Add (ReadObject_XmlSchema (false, true));
						n36++;
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
				if (t == null)
				{ }
				else if (t.Name != "Port" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.Port ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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
				if (t == null)
				{ }
				else if (t.Name != "MessagePart" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.MessagePart ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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

			ob = new System.Web.Services.Description.Operation ();

			Reader.MoveToElement();

			ob.@ParameterOrderString = "";
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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b40) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b41) {
						if (((object)o44) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationFaultCollection");
						o44.Add (ReadObject_OperationFault (false, true));
						n43++;
					}
					else if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b42) {
						if (((object)o46) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o46.Add (ReadObject_OperationInput (false, true));
						n45++;
					}
					else if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b42) {
						if (((object)o46) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.OperationMessageCollection");
						o46.Add (ReadObject_OperationOutput (false, true));
						n45++;
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
				if (t == null)
				{ }
				else if (t.Name != "OperationBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.OperationBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b47) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "fault" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b48) {
						if (((object)o52) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.FaultBindingCollection");
						o52.Add (ReadObject_FaultBinding (false, true));
						n51++;
					}
					else if (Reader.LocalName == "input" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b49) {
						b49 = true;
						ob.@Input = ReadObject_InputBinding (false, true);
					}
					else if (Reader.LocalName == "output" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b50) {
						b50 = true;
						ob.@Output = ReadObject_OutputBinding (false, true);
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

		public System.Xml.Schema.XmlSchema ReadObject_XmlSchema (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchema ob = null;
			ob = System.Xml.Schema.XmlSchema.Read (Reader, null); Reader.Read ();
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

			ob = new System.Web.Services.Description.OperationFault ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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

			ob = new System.Web.Services.Description.OperationInput ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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
				if (t == null)
				{ }
				else if (t.Name != "OperationOutput" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.OperationOutput ();

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
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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

			ob = new System.Web.Services.Description.FaultBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b56=false, b57=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b56) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "Extensions" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b57) {
						if (((object)ob.@Extensions) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n58 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "anyType" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/") {
										if (((object)ob.@Extensions) == null)
											throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
										ob.@Extensions.Add (ReadObject_anyType (true, true));
										n58++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b57 = true;
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

			ob = new System.Web.Services.Description.InputBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b59=false, b60=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b59) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "Extensions" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b60) {
						if (((object)ob.@Extensions) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n61 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "anyType" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/") {
										if (((object)ob.@Extensions) == null)
											throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
										ob.@Extensions.Add (ReadObject_anyType (true, true));
										n61++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b60 = true;
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
				if (t == null)
				{ }
				else if (t.Name != "OutputBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.OutputBinding ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b62=false, b63=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b62) {
						ob.@DocumentationElement = ((System.Xml.XmlElement) ReadXmlNode (false));
					}
					else if (Reader.LocalName == "Extensions" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b63) {
						if (((object)ob.@Extensions) == null)
							throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n64 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "anyType" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/") {
										if (((object)ob.@Extensions) == null)
											throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
										ob.@Extensions.Add (ReadObject_anyType (true, true));
										n64++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b63 = true;
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

		public System.Object ReadObject_anyType (bool isNullable, bool checkType)
		{
			System.Object ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
					return ((System.Object) ReadTypedPrimitive (new System.Xml.XmlQualifiedName("anyType", System.Xml.Schema.XmlSchema.Namespace)));
				else if (t.Name == "DocumentableItem" && t.Namespace == "")
					return ReadObject_DocumentableItem (isNullable, checkType);
				else if (t.Name == "DocumentableItem" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_DocumentableItem0 (isNullable, checkType);
				else if (t.Name == "Import" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Import (isNullable, checkType);
				else if (t.Name == "XmlSchemaObject" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaObject (isNullable, checkType);
				else if (t.Name == "XmlSchemaForm" && t.Namespace == "")
					return ReadObject_XmlSchemaForm (isNullable, checkType);
				else if (t.Name == "XmlSchemaDerivationMethod" && t.Namespace == "")
					return ReadObject_XmlSchemaDerivationMethod (isNullable, checkType);
				else if (t.Name == "XmlSchemaExternal" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaExternal (isNullable, checkType);
				else if (t.Name == "XmlSchemaDocumentation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaDocumentation (isNullable, checkType);
				else if (t.Name == "XmlSchemaAppInfo" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAppInfo (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnnotation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnnotation (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnnotated" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnnotated (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttributeGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaType (isNullable, checkType);
				else if (t.Name == "ArrayOfQName" && t.Namespace == "")
					return ReadObject_ArrayOfQName (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeUnion" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeUnion (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeList" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeList (isNullable, checkType);
				else if (t.Name == "XmlSchemaFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaPatternFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaPatternFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaWhiteSpaceFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaWhiteSpaceFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaEnumerationFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaEnumerationFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaNumericFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNumericFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaFractionDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFractionDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaTotalDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaTotalDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleType (isNullable, checkType);
				else if (t.Name == "XmlSchemaUse" && t.Namespace == "")
					return ReadObject_XmlSchemaUse (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaContentProcessing" && t.Namespace == "")
					return ReadObject_XmlSchemaContentProcessing (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnyAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnyAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttributeGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaParticle" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaParticle (isNullable, checkType);
				else if (t.Name == "XmlSchemaAny" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAny (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupBase" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupBase (isNullable, checkType);
				else if (t.Name == "XmlSchemaAll" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAll (isNullable, checkType);
				else if (t.Name == "XmlSchemaContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaContentModel" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContentModel (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexType (isNullable, checkType);
				else if (t.Name == "XmlSchemaXPath" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaXPath (isNullable, checkType);
				else if (t.Name == "XmlSchemaIdentityConstraint" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaIdentityConstraint (isNullable, checkType);
				else if (t.Name == "XmlSchemaKeyref" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKeyref (isNullable, checkType);
				else if (t.Name == "XmlSchemaKey" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKey (isNullable, checkType);
				else if (t.Name == "XmlSchemaUnique" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaUnique (isNullable, checkType);
				else if (t.Name == "XmlSchemaElement" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaElement (isNullable, checkType);
				else if (t.Name == "XmlSchemaChoice" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaChoice (isNullable, checkType);
				else if (t.Name == "XmlSchemaSequence" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSequence (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaRedefine" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaRedefine (isNullable, checkType);
				else if (t.Name == "XmlSchemaImport" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaImport (isNullable, checkType);
				else if (t.Name == "XmlSchemaInclude" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaInclude (isNullable, checkType);
				else if (t.Name == "XmlSchemaNotation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNotation (isNullable, checkType);
				else if (t.Name == "XmlSchema" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchema (isNullable, checkType);
				else if (t.Name == "Types" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Types (isNullable, checkType);
				else if (t.Name == "MessagePart" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_MessagePart (isNullable, checkType);
				else if (t.Name == "Message" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Message (isNullable, checkType);
				else if (t.Name == "OperationMessage" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationMessage (isNullable, checkType);
				else if (t.Name == "OperationFault" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationFault (isNullable, checkType);
				else if (t.Name == "OperationInput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationInput (isNullable, checkType);
				else if (t.Name == "OperationOutput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationOutput (isNullable, checkType);
				else if (t.Name == "Operation" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Operation (isNullable, checkType);
				else if (t.Name == "PortType" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_PortType (isNullable, checkType);
				else if (t.Name == "ArrayOfAnyType" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_ArrayOfAnyType (isNullable, checkType);
				else if (t.Name == "MessageBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_MessageBinding (isNullable, checkType);
				else if (t.Name == "FaultBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_FaultBinding (isNullable, checkType);
				else if (t.Name == "InputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_InputBinding (isNullable, checkType);
				else if (t.Name == "OutputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OutputBinding (isNullable, checkType);
				else if (t.Name == "OperationBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationBinding (isNullable, checkType);
				else if (t.Name == "Binding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Binding (isNullable, checkType);
				else if (t.Name == "Port" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Port (isNullable, checkType);
				else if (t.Name == "Service" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Service (isNullable, checkType);
				else if (t.Name == "ServiceDescription" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_ServiceDescription (isNullable, checkType);
				else if (t.Name != "anyType" || t.Namespace != "")
					return ((System.Object) ReadTypedPrimitive (t));
			}

			ob = new System.Object ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
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

		public System.Web.Services.Description.DocumentableItem ReadObject_DocumentableItem (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.DocumentableItem ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "ServiceDescription" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_ServiceDescription (isNullable, checkType);
				else if (t.Name != "DocumentableItem" || t.Namespace != "")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Web.Services.Description.DocumentableItem ReadObject_DocumentableItem0 (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.DocumentableItem ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "Import" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Import (isNullable, checkType);
				else if (t.Name == "Types" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Types (isNullable, checkType);
				else if (t.Name == "MessagePart" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_MessagePart (isNullable, checkType);
				else if (t.Name == "Message" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Message (isNullable, checkType);
				else if (t.Name == "OperationMessage" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationMessage (isNullable, checkType);
				else if (t.Name == "OperationFault" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationFault (isNullable, checkType);
				else if (t.Name == "OperationInput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationInput (isNullable, checkType);
				else if (t.Name == "OperationOutput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationOutput (isNullable, checkType);
				else if (t.Name == "Operation" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Operation (isNullable, checkType);
				else if (t.Name == "PortType" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_PortType (isNullable, checkType);
				else if (t.Name == "MessageBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_MessageBinding (isNullable, checkType);
				else if (t.Name == "FaultBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_FaultBinding (isNullable, checkType);
				else if (t.Name == "InputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_InputBinding (isNullable, checkType);
				else if (t.Name == "OutputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OutputBinding (isNullable, checkType);
				else if (t.Name == "OperationBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationBinding (isNullable, checkType);
				else if (t.Name == "Binding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Binding (isNullable, checkType);
				else if (t.Name == "Port" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Port (isNullable, checkType);
				else if (t.Name == "Service" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_Service (isNullable, checkType);
				else if (t.Name != "DocumentableItem" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaObject ReadObject_XmlSchemaObject (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaObject ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaExternal" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaExternal (isNullable, checkType);
				else if (t.Name == "XmlSchemaDocumentation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaDocumentation (isNullable, checkType);
				else if (t.Name == "XmlSchemaAppInfo" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAppInfo (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnnotation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnnotation (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnnotated" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnnotated (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttributeGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaType (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeUnion" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeUnion (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeList" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeList (isNullable, checkType);
				else if (t.Name == "XmlSchemaFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaPatternFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaPatternFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaWhiteSpaceFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaWhiteSpaceFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaEnumerationFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaEnumerationFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaNumericFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNumericFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaFractionDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFractionDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaTotalDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaTotalDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleType (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnyAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnyAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttributeGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaParticle" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaParticle (isNullable, checkType);
				else if (t.Name == "XmlSchemaAny" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAny (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupBase" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupBase (isNullable, checkType);
				else if (t.Name == "XmlSchemaAll" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAll (isNullable, checkType);
				else if (t.Name == "XmlSchemaContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaContentModel" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContentModel (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexType (isNullable, checkType);
				else if (t.Name == "XmlSchemaXPath" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaXPath (isNullable, checkType);
				else if (t.Name == "XmlSchemaIdentityConstraint" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaIdentityConstraint (isNullable, checkType);
				else if (t.Name == "XmlSchemaKeyref" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKeyref (isNullable, checkType);
				else if (t.Name == "XmlSchemaKey" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKey (isNullable, checkType);
				else if (t.Name == "XmlSchemaUnique" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaUnique (isNullable, checkType);
				else if (t.Name == "XmlSchemaElement" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaElement (isNullable, checkType);
				else if (t.Name == "XmlSchemaChoice" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaChoice (isNullable, checkType);
				else if (t.Name == "XmlSchemaSequence" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSequence (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaRedefine" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaRedefine (isNullable, checkType);
				else if (t.Name == "XmlSchemaImport" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaImport (isNullable, checkType);
				else if (t.Name == "XmlSchemaInclude" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaInclude (isNullable, checkType);
				else if (t.Name == "XmlSchemaNotation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNotation (isNullable, checkType);
				else if (t.Name == "XmlSchema" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchema (isNullable, checkType);
				else if (t.Name != "XmlSchemaObject" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaForm ReadObject_XmlSchemaForm (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Xml.Schema.XmlSchemaForm res = GetEnumValue_XmlSchemaForm (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Xml.Schema.XmlSchemaForm GetEnumValue_XmlSchemaForm (string xmlName)
		{
			switch (xmlName)
			{
				case "qualified": return System.Xml.Schema.XmlSchemaForm.Qualified;
				case "unqualified": return System.Xml.Schema.XmlSchemaForm.Unqualified;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Xml.Schema.XmlSchemaForm));
			}
		}

		public System.Xml.Schema.XmlSchemaDerivationMethod ReadObject_XmlSchemaDerivationMethod (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Xml.Schema.XmlSchemaDerivationMethod res = GetEnumValue_XmlSchemaDerivationMethod (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Xml.Schema.XmlSchemaDerivationMethod GetEnumValue_XmlSchemaDerivationMethod (string xmlName)
		{
			xmlName = xmlName.Trim();
			if (xmlName.Length == 0) return (System.Xml.Schema.XmlSchemaDerivationMethod)0;
			if (xmlName.IndexOf (' ') != -1)
			{
				System.Xml.Schema.XmlSchemaDerivationMethod sb = (System.Xml.Schema.XmlSchemaDerivationMethod)0;
				string[] enumNames = xmlName.ToString().Split (' ');
				foreach (string name in enumNames)
				{
					if (name == string.Empty) continue;
					sb |= GetEnumValue_XmlSchemaDerivationMethod_Switch (name); 
				}
				return sb;
			}
			else
				return GetEnumValue_XmlSchemaDerivationMethod_Switch (xmlName);
		}

		System.Xml.Schema.XmlSchemaDerivationMethod GetEnumValue_XmlSchemaDerivationMethod_Switch (string xmlName)
		{
			switch (xmlName)
			{
				case "": return System.Xml.Schema.XmlSchemaDerivationMethod.Empty;
				case "substitution": return System.Xml.Schema.XmlSchemaDerivationMethod.Substitution;
				case "extension": return System.Xml.Schema.XmlSchemaDerivationMethod.Extension;
				case "restriction": return System.Xml.Schema.XmlSchemaDerivationMethod.Restriction;
				case "list": return System.Xml.Schema.XmlSchemaDerivationMethod.List;
				case "union": return System.Xml.Schema.XmlSchemaDerivationMethod.Union;
				case "#all": return System.Xml.Schema.XmlSchemaDerivationMethod.All;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Xml.Schema.XmlSchemaDerivationMethod));
			}
		}

		public System.Xml.Schema.XmlSchemaExternal ReadObject_XmlSchemaExternal (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaExternal ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaRedefine" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaRedefine (isNullable, checkType);
				else if (t.Name == "XmlSchemaImport" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaImport (isNullable, checkType);
				else if (t.Name == "XmlSchemaInclude" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaInclude (isNullable, checkType);
				else if (t.Name != "XmlSchemaExternal" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaDocumentation ReadObject_XmlSchemaDocumentation (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaDocumentation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaDocumentation" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaDocumentation ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "source" && Reader.NamespaceURI == "") {
					ob.@Source = Reader.Value;
				}
				else if (Reader.LocalName == "xml_x003A_lang" && Reader.NamespaceURI == "") {
					ob.@Language = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b65=false;

			System.Xml.XmlNode[] o67;
			o67 = null;
			int n66=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					o67 = (System.Xml.XmlNode[]) EnsureArrayIndex (o67, n66, typeof(System.Xml.XmlNode));
					o67[n66] = ReadXmlNode (false);
					n66++;
				}
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text)
				{
					o67 = (System.Xml.XmlNode[]) EnsureArrayIndex (o67, n66, typeof(System.Xml.XmlNode));
					o67[n66] = ReadXmlNode (false);
					n66++;
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o67 = (System.Xml.XmlNode[]) ShrinkArray (o67, n66, typeof(System.Xml.XmlNode), true);
			ob.@Markup = o67;

			ReadEndElement();

			return ob;
		}

		public System.Xml.Schema.XmlSchemaAppInfo ReadObject_XmlSchemaAppInfo (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAppInfo ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAppInfo" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAppInfo ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "source" && Reader.NamespaceURI == "") {
					ob.@Source = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
					if (ob.@Namespaces == null) ob.@Namespaces = new XmlSerializerNamespaces ();
					if (Reader.Prefix == "xmlns")
						ob.@Namespaces.Add (Reader.LocalName, Reader.Value);
					else
						ob.@Namespaces.Add ("", Reader.Value);
				}
				else {
					#if NET_2_0
					ServiceDescription.AddUnknownAttribute ((XmlAttribute) ReadXmlNode (false));
					#else
					UnknownNode (ob);
					#endif
				}
			}

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b68=false;

			System.Xml.XmlNode[] o70;
			o70 = null;
			int n69=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					o70 = (System.Xml.XmlNode[]) EnsureArrayIndex (o70, n69, typeof(System.Xml.XmlNode));
					o70[n69] = ReadXmlNode (false);
					n69++;
				}
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text)
				{
					o70 = (System.Xml.XmlNode[]) EnsureArrayIndex (o70, n69, typeof(System.Xml.XmlNode));
					o70[n69] = ReadXmlNode (false);
					n69++;
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o70 = (System.Xml.XmlNode[]) ShrinkArray (o70, n69, typeof(System.Xml.XmlNode), true);
			ob.@Markup = o70;

			ReadEndElement();

			return ob;
		}

		public System.Xml.Schema.XmlSchemaAnnotation ReadObject_XmlSchemaAnnotation (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAnnotation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAnnotation" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAnnotation ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b71=false;

			System.Xml.Schema.XmlSchemaObjectCollection o73;
			o73 = ob.@Items;
			int n72=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "documentation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b71) {
						if (((object)o73) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o73.Add (ReadObject_XmlSchemaDocumentation (false, true));
						n72++;
					}
					else if (Reader.LocalName == "appinfo" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b71) {
						if (((object)o73) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o73.Add (ReadObject_XmlSchemaAppInfo (false, true));
						n72++;
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

		public System.Xml.Schema.XmlSchemaAnnotated ReadObject_XmlSchemaAnnotated (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAnnotated ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaAttributeGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaType (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeUnion" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeUnion (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeList" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeList (isNullable, checkType);
				else if (t.Name == "XmlSchemaFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaPatternFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaPatternFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaWhiteSpaceFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaWhiteSpaceFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaEnumerationFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaEnumerationFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaNumericFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNumericFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaFractionDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFractionDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaTotalDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaTotalDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleType (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaAnyAttribute" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAnyAttribute (isNullable, checkType);
				else if (t.Name == "XmlSchemaAttributeGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAttributeGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaParticle" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaParticle (isNullable, checkType);
				else if (t.Name == "XmlSchemaAny" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAny (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupBase" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupBase (isNullable, checkType);
				else if (t.Name == "XmlSchemaAll" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAll (isNullable, checkType);
				else if (t.Name == "XmlSchemaContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaContentModel" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaContentModel (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexType (isNullable, checkType);
				else if (t.Name == "XmlSchemaXPath" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaXPath (isNullable, checkType);
				else if (t.Name == "XmlSchemaIdentityConstraint" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaIdentityConstraint (isNullable, checkType);
				else if (t.Name == "XmlSchemaKeyref" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKeyref (isNullable, checkType);
				else if (t.Name == "XmlSchemaKey" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKey (isNullable, checkType);
				else if (t.Name == "XmlSchemaUnique" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaUnique (isNullable, checkType);
				else if (t.Name == "XmlSchemaElement" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaElement (isNullable, checkType);
				else if (t.Name == "XmlSchemaChoice" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaChoice (isNullable, checkType);
				else if (t.Name == "XmlSchemaSequence" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSequence (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroup" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroup (isNullable, checkType);
				else if (t.Name == "XmlSchemaNotation" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNotation (isNullable, checkType);
				else if (t.Name != "XmlSchemaAnnotated" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAnnotated ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b74=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b74) {
						b74 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaAttributeGroupRef ReadObject_XmlSchemaAttributeGroupRef (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAttributeGroupRef ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAttributeGroupRef" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAttributeGroupRef ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@RefName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b75=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b75) {
						b75 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaType ReadObject_XmlSchemaType (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaType ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaSimpleType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleType (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexType" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexType (isNullable, checkType);
				else if (t.Name != "XmlSchemaType" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaType ();

			Reader.MoveToElement();

			ob.@Final = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "final" && Reader.NamespaceURI == "") {
					ob.@Final = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b76=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b76) {
						b76 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.XmlQualifiedName[] ReadObject_ArrayOfQName (bool isNullable, bool checkType)
		{
			System.Xml.XmlQualifiedName[] o77 = null;
			if (!ReadNull()) {
				o77 = (System.Xml.XmlQualifiedName[]) EnsureArrayIndex (null, 0, typeof(System.Xml.XmlQualifiedName));
				if (Reader.IsEmptyElement) {
					Reader.Skip();
					o77 = (System.Xml.XmlQualifiedName[]) ShrinkArray (o77, 0, typeof(System.Xml.XmlQualifiedName), false);
				} else {
					int n78 = 0;
					Reader.ReadStartElement();
					Reader.MoveToContent();

					while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
					{
						if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
						{
							if (Reader.LocalName == "QName" && Reader.NamespaceURI == "") {
								o77 = (System.Xml.XmlQualifiedName[]) EnsureArrayIndex (o77, n78, typeof(System.Xml.XmlQualifiedName));
								o77[n78] = ReadElementQualifiedName ();
								n78++;
							}
							else UnknownNode (null);
						}
						else UnknownNode (null);

						Reader.MoveToContent();
					}
					ReadEndElement();
					o77 = (System.Xml.XmlQualifiedName[]) ShrinkArray (o77, n78, typeof(System.Xml.XmlQualifiedName), false);
				}
			}
			return o77;
		}

		public System.Xml.Schema.XmlSchemaSimpleTypeContent ReadObject_XmlSchemaSimpleTypeContent (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleTypeContent ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaSimpleTypeUnion" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeUnion (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeList" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeList (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleTypeRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleTypeRestriction (isNullable, checkType);
				else if (t.Name != "XmlSchemaSimpleTypeContent" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaSimpleTypeUnion ReadObject_XmlSchemaSimpleTypeUnion (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleTypeUnion ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleTypeUnion" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleTypeUnion ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "memberTypes" && Reader.NamespaceURI == "") {
					System.Xml.XmlQualifiedName[] o79;
					string s80 = Reader.Value.Trim();
					if (s80 != string.Empty) {
						string[] o81 = s80.Split (' ');
						o79 = new System.Xml.XmlQualifiedName [o81.Length];
						for (int n82 = 0; n82 < o81.Length; n82++)
							o79[n82] = ToXmlQualifiedName (o81[n82]);
					}
					else
						o79 = new System.Xml.XmlQualifiedName [0];
					ob.@MemberTypes = o79;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b83=false, b84=false;

			System.Xml.Schema.XmlSchemaObjectCollection o86;
			o86 = ob.@BaseTypes;
			int n85=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b84) {
						if (((object)o86) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o86.Add (ReadObject_XmlSchemaSimpleType (false, true));
						n85++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b83) {
						b83 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaSimpleTypeList ReadObject_XmlSchemaSimpleTypeList (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleTypeList ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleTypeList" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleTypeList ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "itemType" && Reader.NamespaceURI == "") {
					ob.@ItemTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b87=false, b88=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b88) {
						b88 = true;
						ob.@ItemType = ReadObject_XmlSchemaSimpleType (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b87) {
						b87 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaFacet ReadObject_XmlSchemaFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaPatternFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaPatternFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaWhiteSpaceFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaWhiteSpaceFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaEnumerationFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaEnumerationFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaNumericFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaNumericFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaFractionDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFractionDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaTotalDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaTotalDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMaxExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxExclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinInclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinInclusiveFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinExclusiveFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinExclusiveFacet (isNullable, checkType);
				else if (t.Name != "XmlSchemaFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaPatternFacet ReadObject_XmlSchemaPatternFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaPatternFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaPatternFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaPatternFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b89=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b89) {
						b89 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaWhiteSpaceFacet ReadObject_XmlSchemaWhiteSpaceFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaWhiteSpaceFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaWhiteSpaceFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaWhiteSpaceFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b90=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b90) {
						b90 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaEnumerationFacet ReadObject_XmlSchemaEnumerationFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaEnumerationFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaEnumerationFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaEnumerationFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b91=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b91) {
						b91 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaNumericFacet ReadObject_XmlSchemaNumericFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaNumericFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaMaxLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMaxLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaMinLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaMinLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaLengthFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaLengthFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaFractionDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaFractionDigitsFacet (isNullable, checkType);
				else if (t.Name == "XmlSchemaTotalDigitsFacet" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaTotalDigitsFacet (isNullable, checkType);
				else if (t.Name != "XmlSchemaNumericFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaMaxLengthFacet ReadObject_XmlSchemaMaxLengthFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMaxLengthFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMaxLengthFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMaxLengthFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b92=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b92) {
						b92 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaMinLengthFacet ReadObject_XmlSchemaMinLengthFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMinLengthFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMinLengthFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMinLengthFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b93=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b93) {
						b93 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaLengthFacet ReadObject_XmlSchemaLengthFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaLengthFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaLengthFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaLengthFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b94=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b94) {
						b94 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaFractionDigitsFacet ReadObject_XmlSchemaFractionDigitsFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaFractionDigitsFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaFractionDigitsFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaFractionDigitsFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b95=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b95) {
						b95 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaTotalDigitsFacet ReadObject_XmlSchemaTotalDigitsFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaTotalDigitsFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaTotalDigitsFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaTotalDigitsFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b96=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b96) {
						b96 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaMaxInclusiveFacet ReadObject_XmlSchemaMaxInclusiveFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMaxInclusiveFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMaxInclusiveFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMaxInclusiveFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b97=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b97) {
						b97 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaMaxExclusiveFacet ReadObject_XmlSchemaMaxExclusiveFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMaxExclusiveFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMaxExclusiveFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMaxExclusiveFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b98=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b98) {
						b98 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaMinInclusiveFacet ReadObject_XmlSchemaMinInclusiveFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMinInclusiveFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMinInclusiveFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMinInclusiveFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b99=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b99) {
						b99 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaMinExclusiveFacet ReadObject_XmlSchemaMinExclusiveFacet (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaMinExclusiveFacet ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaMinExclusiveFacet" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaMinExclusiveFacet ();

			Reader.MoveToElement();

			ob.@IsFixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "value" && Reader.NamespaceURI == "") {
					ob.@Value = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@IsFixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b100=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b100) {
						b100 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaSimpleTypeRestriction ReadObject_XmlSchemaSimpleTypeRestriction (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleTypeRestriction ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleTypeRestriction" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleTypeRestriction ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "base" && Reader.NamespaceURI == "") {
					ob.@BaseTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b101=false, b102=false, b103=false;

			System.Xml.Schema.XmlSchemaObjectCollection o105;
			o105 = ob.@Facets;
			int n104=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "enumeration" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaEnumerationFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "pattern" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaPatternFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "maxInclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMaxInclusiveFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "minLength" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMinLengthFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "minInclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMinInclusiveFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "length" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaLengthFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b101) {
						b101 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "totalDigits" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaTotalDigitsFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "maxLength" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMaxLengthFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "minExclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMinExclusiveFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "fractionDigits" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaFractionDigitsFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b102) {
						b102 = true;
						ob.@BaseType = ReadObject_XmlSchemaSimpleType (false, true);
					}
					else if (Reader.LocalName == "whiteSpace" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaWhiteSpaceFacet (false, true));
						n104++;
					}
					else if (Reader.LocalName == "maxExclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b103) {
						if (((object)o105) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o105.Add (ReadObject_XmlSchemaMaxExclusiveFacet (false, true));
						n104++;
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

		public System.Xml.Schema.XmlSchemaSimpleType ReadObject_XmlSchemaSimpleType (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleType ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleType" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleType ();

			Reader.MoveToElement();

			ob.@Final = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "final" && Reader.NamespaceURI == "") {
					ob.@Final = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b106=false, b107=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "restriction" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b107) {
						b107 = true;
						ob.@Content = ReadObject_XmlSchemaSimpleTypeRestriction (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b106) {
						b106 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "union" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b107) {
						b107 = true;
						ob.@Content = ReadObject_XmlSchemaSimpleTypeUnion (false, true);
					}
					else if (Reader.LocalName == "list" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b107) {
						b107 = true;
						ob.@Content = ReadObject_XmlSchemaSimpleTypeList (false, true);
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

		public System.Xml.Schema.XmlSchemaUse ReadObject_XmlSchemaUse (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Xml.Schema.XmlSchemaUse res = GetEnumValue_XmlSchemaUse (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Xml.Schema.XmlSchemaUse GetEnumValue_XmlSchemaUse (string xmlName)
		{
			switch (xmlName)
			{
				case "optional": return System.Xml.Schema.XmlSchemaUse.Optional;
				case "prohibited": return System.Xml.Schema.XmlSchemaUse.Prohibited;
				case "required": return System.Xml.Schema.XmlSchemaUse.Required;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Xml.Schema.XmlSchemaUse));
			}
		}

		public System.Xml.Schema.XmlSchemaAttribute ReadObject_XmlSchemaAttribute (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAttribute ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAttribute" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAttribute ();

			Reader.MoveToElement();

			ob.@Form = ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None);
			ob.@Use = ((System.Xml.Schema.XmlSchemaUse) System.Xml.Schema.XmlSchemaUse.None);
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "default" && Reader.NamespaceURI == "") {
					ob.@DefaultValue = Reader.Value;
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@FixedValue = Reader.Value;
				}
				else if (Reader.LocalName == "form" && Reader.NamespaceURI == "") {
					ob.@Form = GetEnumValue_XmlSchemaForm (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@RefName = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@SchemaTypeName = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "use" && Reader.NamespaceURI == "") {
					ob.@Use = GetEnumValue_XmlSchemaUse (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b108=false, b109=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b109) {
						b109 = true;
						ob.@SchemaType = ReadObject_XmlSchemaSimpleType (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b108) {
						b108 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaContentProcessing ReadObject_XmlSchemaContentProcessing (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Xml.Schema.XmlSchemaContentProcessing res = GetEnumValue_XmlSchemaContentProcessing (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Xml.Schema.XmlSchemaContentProcessing GetEnumValue_XmlSchemaContentProcessing (string xmlName)
		{
			switch (xmlName)
			{
				case "skip": return System.Xml.Schema.XmlSchemaContentProcessing.Skip;
				case "lax": return System.Xml.Schema.XmlSchemaContentProcessing.Lax;
				case "strict": return System.Xml.Schema.XmlSchemaContentProcessing.Strict;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Xml.Schema.XmlSchemaContentProcessing));
			}
		}

		public System.Xml.Schema.XmlSchemaAnyAttribute ReadObject_XmlSchemaAnyAttribute (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAnyAttribute ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAnyAttribute" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAnyAttribute ();

			Reader.MoveToElement();

			ob.@ProcessContents = ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None);
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "namespace" && Reader.NamespaceURI == "") {
					ob.@Namespace = Reader.Value;
				}
				else if (Reader.LocalName == "processContents" && Reader.NamespaceURI == "") {
					ob.@ProcessContents = GetEnumValue_XmlSchemaContentProcessing (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b110=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b110) {
						b110 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaAttributeGroup ReadObject_XmlSchemaAttributeGroup (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAttributeGroup ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAttributeGroup" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAttributeGroup ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b111=false, b112=false, b113=false;

			System.Xml.Schema.XmlSchemaObjectCollection o115;
			o115 = ob.@Attributes;
			int n114=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b111) {
						b111 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b113) {
						b113 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b112) {
						if (((object)o115) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o115.Add (ReadObject_XmlSchemaAttribute (false, true));
						n114++;
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b112) {
						if (((object)o115) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o115.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n114++;
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

		public System.Xml.Schema.XmlSchemaParticle ReadObject_XmlSchemaParticle (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaParticle ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaAny" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAny (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupRef" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupRef (isNullable, checkType);
				else if (t.Name == "XmlSchemaGroupBase" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaGroupBase (isNullable, checkType);
				else if (t.Name == "XmlSchemaAll" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAll (isNullable, checkType);
				else if (t.Name == "XmlSchemaElement" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaElement (isNullable, checkType);
				else if (t.Name == "XmlSchemaChoice" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaChoice (isNullable, checkType);
				else if (t.Name == "XmlSchemaSequence" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSequence (isNullable, checkType);
				else if (t.Name != "XmlSchemaParticle" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaAny ReadObject_XmlSchemaAny (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAny ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAny" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAny ();

			Reader.MoveToElement();

			ob.@ProcessContents = ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None);
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "namespace" && Reader.NamespaceURI == "") {
					ob.@Namespace = Reader.Value;
				}
				else if (Reader.LocalName == "processContents" && Reader.NamespaceURI == "") {
					ob.@ProcessContents = GetEnumValue_XmlSchemaContentProcessing (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b116=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b116) {
						b116 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaGroupRef ReadObject_XmlSchemaGroupRef (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaGroupRef ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaGroupRef" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaGroupRef ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@RefName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b117=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b117) {
						b117 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaGroupBase ReadObject_XmlSchemaGroupBase (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaGroupBase ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaAll" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaAll (isNullable, checkType);
				else if (t.Name == "XmlSchemaChoice" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaChoice (isNullable, checkType);
				else if (t.Name == "XmlSchemaSequence" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSequence (isNullable, checkType);
				else if (t.Name != "XmlSchemaGroupBase" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaAll ReadObject_XmlSchemaAll (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaAll ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaAll" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaAll ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b118=false, b119=false;

			System.Xml.Schema.XmlSchemaObjectCollection o121;
			o121 = ob.@Items;
			int n120=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b118) {
						b118 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "element" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b119) {
						if (((object)o121) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o121.Add (ReadObject_XmlSchemaElement (false, true));
						n120++;
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

		public System.Xml.Schema.XmlSchemaContent ReadObject_XmlSchemaContent (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaContent ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaComplexContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaComplexContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContentRestriction (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentExtension" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentExtension (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContentRestriction" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContentRestriction (isNullable, checkType);
				else if (t.Name != "XmlSchemaContent" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaComplexContentExtension ReadObject_XmlSchemaComplexContentExtension (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaComplexContentExtension ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaComplexContentExtension" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaComplexContentExtension ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "base" && Reader.NamespaceURI == "") {
					ob.@BaseTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b122=false, b123=false, b124=false, b125=false;

			System.Xml.Schema.XmlSchemaObjectCollection o127;
			o127 = ob.@Attributes;
			int n126=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b123) {
						b123 = true;
						ob.@Particle = ReadObject_XmlSchemaChoice (false, true);
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b124) {
						if (((object)o127) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o127.Add (ReadObject_XmlSchemaAttribute (false, true));
						n126++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b122) {
						b122 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "all" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b123) {
						b123 = true;
						ob.@Particle = ReadObject_XmlSchemaAll (false, true);
					}
					else if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b123) {
						b123 = true;
						ob.@Particle = ReadObject_XmlSchemaSequence (false, true);
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b125) {
						b125 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b124) {
						if (((object)o127) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o127.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n126++;
					}
					else if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b123) {
						b123 = true;
						ob.@Particle = ReadObject_XmlSchemaGroupRef (false, true);
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

		public System.Xml.Schema.XmlSchemaComplexContentRestriction ReadObject_XmlSchemaComplexContentRestriction (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaComplexContentRestriction ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaComplexContentRestriction" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaComplexContentRestriction ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "base" && Reader.NamespaceURI == "") {
					ob.@BaseTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b128=false, b129=false, b130=false, b131=false;

			System.Xml.Schema.XmlSchemaObjectCollection o133;
			o133 = ob.@Attributes;
			int n132=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b129) {
						b129 = true;
						ob.@Particle = ReadObject_XmlSchemaChoice (false, true);
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b130) {
						if (((object)o133) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o133.Add (ReadObject_XmlSchemaAttribute (false, true));
						n132++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b128) {
						b128 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "all" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b129) {
						b129 = true;
						ob.@Particle = ReadObject_XmlSchemaAll (false, true);
					}
					else if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b129) {
						b129 = true;
						ob.@Particle = ReadObject_XmlSchemaSequence (false, true);
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b131) {
						b131 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b130) {
						if (((object)o133) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o133.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n132++;
					}
					else if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b129) {
						b129 = true;
						ob.@Particle = ReadObject_XmlSchemaGroupRef (false, true);
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

		public System.Xml.Schema.XmlSchemaContentModel ReadObject_XmlSchemaContentModel (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaContentModel ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaComplexContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaComplexContent (isNullable, checkType);
				else if (t.Name == "XmlSchemaSimpleContent" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaSimpleContent (isNullable, checkType);
				else if (t.Name != "XmlSchemaContentModel" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Xml.Schema.XmlSchemaComplexContent ReadObject_XmlSchemaComplexContent (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaComplexContent ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaComplexContent" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaComplexContent ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "mixed" && Reader.NamespaceURI == "") {
					ob.@IsMixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b134=false, b135=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "extension" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b135) {
						b135 = true;
						ob.@Content = ReadObject_XmlSchemaComplexContentExtension (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b134) {
						b134 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "restriction" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b135) {
						b135 = true;
						ob.@Content = ReadObject_XmlSchemaComplexContentRestriction (false, true);
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

		public System.Xml.Schema.XmlSchemaSimpleContentExtension ReadObject_XmlSchemaSimpleContentExtension (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleContentExtension ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleContentExtension" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleContentExtension ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "base" && Reader.NamespaceURI == "") {
					ob.@BaseTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b136=false, b137=false, b138=false;

			System.Xml.Schema.XmlSchemaObjectCollection o140;
			o140 = ob.@Attributes;
			int n139=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b136) {
						b136 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b138) {
						b138 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b137) {
						if (((object)o140) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o140.Add (ReadObject_XmlSchemaAttribute (false, true));
						n139++;
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b137) {
						if (((object)o140) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o140.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n139++;
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

		public System.Xml.Schema.XmlSchemaSimpleContentRestriction ReadObject_XmlSchemaSimpleContentRestriction (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleContentRestriction ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleContentRestriction" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleContentRestriction ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "base" && Reader.NamespaceURI == "") {
					ob.@BaseTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b141=false, b142=false, b143=false, b144=false, b145=false;

			System.Xml.Schema.XmlSchemaObjectCollection o147;
			o147 = ob.@Facets;
			System.Xml.Schema.XmlSchemaObjectCollection o149;
			o149 = ob.@Attributes;
			int n146=0, n148=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "enumeration" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaEnumerationFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b144) {
						if (((object)o149) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o149.Add (ReadObject_XmlSchemaAttribute (false, true));
						n148++;
					}
					else if (Reader.LocalName == "pattern" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaPatternFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b145) {
						b145 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "maxInclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMaxInclusiveFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "minLength" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMinLengthFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "minInclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMinInclusiveFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "length" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaLengthFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b141) {
						b141 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b144) {
						if (((object)o149) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o149.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n148++;
					}
					else if (Reader.LocalName == "totalDigits" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaTotalDigitsFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "maxLength" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMaxLengthFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "minExclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMinExclusiveFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "fractionDigits" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaFractionDigitsFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b142) {
						b142 = true;
						ob.@BaseType = ReadObject_XmlSchemaSimpleType (false, true);
					}
					else if (Reader.LocalName == "whiteSpace" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaWhiteSpaceFacet (false, true));
						n146++;
					}
					else if (Reader.LocalName == "maxExclusive" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b143) {
						if (((object)o147) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o147.Add (ReadObject_XmlSchemaMaxExclusiveFacet (false, true));
						n146++;
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

		public System.Xml.Schema.XmlSchemaSimpleContent ReadObject_XmlSchemaSimpleContent (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSimpleContent ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSimpleContent" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSimpleContent ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b150=false, b151=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "extension" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b151) {
						b151 = true;
						ob.@Content = ReadObject_XmlSchemaSimpleContentExtension (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b150) {
						b150 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "restriction" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b151) {
						b151 = true;
						ob.@Content = ReadObject_XmlSchemaSimpleContentRestriction (false, true);
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

		public System.Xml.Schema.XmlSchemaComplexType ReadObject_XmlSchemaComplexType (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaComplexType ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaComplexType" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaComplexType ();

			Reader.MoveToElement();

			ob.@Final = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			ob.@IsAbstract = false;
			ob.@Block = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			ob.@IsMixed = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "final" && Reader.NamespaceURI == "") {
					ob.@Final = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
				}
				else if (Reader.LocalName == "abstract" && Reader.NamespaceURI == "") {
					ob.@IsAbstract = XmlConvert.ToBoolean (Reader.Value);
				}
				else if (Reader.LocalName == "block" && Reader.NamespaceURI == "") {
					ob.@Block = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
				}
				else if (Reader.LocalName == "mixed" && Reader.NamespaceURI == "") {
					ob.@IsMixed = XmlConvert.ToBoolean (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b152=false, b153=false, b154=false, b155=false, b156=false;

			System.Xml.Schema.XmlSchemaObjectCollection o158;
			o158 = ob.@Attributes;
			int n157=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b154) {
						b154 = true;
						ob.@Particle = ReadObject_XmlSchemaSequence (false, true);
					}
					else if (Reader.LocalName == "simpleContent" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b153) {
						b153 = true;
						ob.@ContentModel = ReadObject_XmlSchemaSimpleContent (false, true);
					}
					else if (Reader.LocalName == "attribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b155) {
						if (((object)o158) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o158.Add (ReadObject_XmlSchemaAttribute (false, true));
						n157++;
					}
					else if (Reader.LocalName == "anyAttribute" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b156) {
						b156 = true;
						ob.@AnyAttribute = ReadObject_XmlSchemaAnyAttribute (false, true);
					}
					else if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b154) {
						b154 = true;
						ob.@Particle = ReadObject_XmlSchemaChoice (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b152) {
						b152 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b155) {
						if (((object)o158) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o158.Add (ReadObject_XmlSchemaAttributeGroupRef (false, true));
						n157++;
					}
					else if (Reader.LocalName == "complexContent" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b153) {
						b153 = true;
						ob.@ContentModel = ReadObject_XmlSchemaComplexContent (false, true);
					}
					else if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b154) {
						b154 = true;
						ob.@Particle = ReadObject_XmlSchemaGroupRef (false, true);
					}
					else if (Reader.LocalName == "all" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b154) {
						b154 = true;
						ob.@Particle = ReadObject_XmlSchemaAll (false, true);
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

		public System.Xml.Schema.XmlSchemaXPath ReadObject_XmlSchemaXPath (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaXPath ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaXPath" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaXPath ();

			Reader.MoveToElement();

			ob.@XPath = "";
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "xpath" && Reader.NamespaceURI == "") {
					ob.@XPath = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b159=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b159) {
						b159 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaIdentityConstraint ReadObject_XmlSchemaIdentityConstraint (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaIdentityConstraint ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "XmlSchemaKeyref" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKeyref (isNullable, checkType);
				else if (t.Name == "XmlSchemaKey" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaKey (isNullable, checkType);
				else if (t.Name == "XmlSchemaUnique" && t.Namespace == "http://www.w3.org/2001/XMLSchema")
					return ReadObject_XmlSchemaUnique (isNullable, checkType);
				else if (t.Name != "XmlSchemaIdentityConstraint" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaIdentityConstraint ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b160=false, b161=false, b162=false;

			System.Xml.Schema.XmlSchemaObjectCollection o164;
			o164 = ob.@Fields;
			int n163=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b160) {
						b160 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "selector" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b161) {
						b161 = true;
						ob.@Selector = ReadObject_XmlSchemaXPath (false, true);
					}
					else if (Reader.LocalName == "field" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b162) {
						if (((object)o164) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o164.Add (ReadObject_XmlSchemaXPath (false, true));
						n163++;
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

		public System.Xml.Schema.XmlSchemaKeyref ReadObject_XmlSchemaKeyref (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaKeyref ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaKeyref" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaKeyref ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "refer" && Reader.NamespaceURI == "") {
					ob.@Refer = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b165=false, b166=false, b167=false;

			System.Xml.Schema.XmlSchemaObjectCollection o169;
			o169 = ob.@Fields;
			int n168=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b165) {
						b165 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "selector" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b166) {
						b166 = true;
						ob.@Selector = ReadObject_XmlSchemaXPath (false, true);
					}
					else if (Reader.LocalName == "field" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b167) {
						if (((object)o169) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o169.Add (ReadObject_XmlSchemaXPath (false, true));
						n168++;
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

		public System.Xml.Schema.XmlSchemaKey ReadObject_XmlSchemaKey (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaKey ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaKey" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaKey ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b170=false, b171=false, b172=false;

			System.Xml.Schema.XmlSchemaObjectCollection o174;
			o174 = ob.@Fields;
			int n173=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b170) {
						b170 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "selector" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b171) {
						b171 = true;
						ob.@Selector = ReadObject_XmlSchemaXPath (false, true);
					}
					else if (Reader.LocalName == "field" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b172) {
						if (((object)o174) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o174.Add (ReadObject_XmlSchemaXPath (false, true));
						n173++;
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

		public System.Xml.Schema.XmlSchemaUnique ReadObject_XmlSchemaUnique (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaUnique ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaUnique" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaUnique ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b175=false, b176=false, b177=false;

			System.Xml.Schema.XmlSchemaObjectCollection o179;
			o179 = ob.@Fields;
			int n178=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b175) {
						b175 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "selector" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b176) {
						b176 = true;
						ob.@Selector = ReadObject_XmlSchemaXPath (false, true);
					}
					else if (Reader.LocalName == "field" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b177) {
						if (((object)o179) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o179.Add (ReadObject_XmlSchemaXPath (false, true));
						n178++;
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

		public System.Xml.Schema.XmlSchemaElement ReadObject_XmlSchemaElement (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaElement ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaElement" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaElement ();

			Reader.MoveToElement();

			ob.@IsAbstract = false;
			ob.@Block = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			ob.@Final = ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None);
			ob.@Form = ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None);
			ob.@Name = "";
			ob.@IsNillable = false;
			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "abstract" && Reader.NamespaceURI == "") {
					ob.@IsAbstract = XmlConvert.ToBoolean (Reader.Value);
				}
				else if (Reader.LocalName == "block" && Reader.NamespaceURI == "") {
					ob.@Block = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
				}
				else if (Reader.LocalName == "default" && Reader.NamespaceURI == "") {
					ob.@DefaultValue = Reader.Value;
				}
				else if (Reader.LocalName == "final" && Reader.NamespaceURI == "") {
					ob.@Final = GetEnumValue_XmlSchemaDerivationMethod (Reader.Value);
				}
				else if (Reader.LocalName == "fixed" && Reader.NamespaceURI == "") {
					ob.@FixedValue = Reader.Value;
				}
				else if (Reader.LocalName == "form" && Reader.NamespaceURI == "") {
					ob.@Form = GetEnumValue_XmlSchemaForm (Reader.Value);
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "nillable" && Reader.NamespaceURI == "") {
					ob.@IsNillable = XmlConvert.ToBoolean (Reader.Value);
				}
				else if (Reader.LocalName == "ref" && Reader.NamespaceURI == "") {
					ob.@RefName = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "substitutionGroup" && Reader.NamespaceURI == "") {
					ob.@SubstitutionGroup = ToXmlQualifiedName (Reader.Value);
				}
				else if (Reader.LocalName == "type" && Reader.NamespaceURI == "") {
					ob.@SchemaTypeName = ToXmlQualifiedName (Reader.Value);
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b180=false, b181=false, b182=false;

			System.Xml.Schema.XmlSchemaObjectCollection o184;
			o184 = ob.@Constraints;
			int n183=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b181) {
						b181 = true;
						ob.@SchemaType = ReadObject_XmlSchemaSimpleType (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b180) {
						b180 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "key" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b182) {
						if (((object)o184) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o184.Add (ReadObject_XmlSchemaKey (false, true));
						n183++;
					}
					else if (Reader.LocalName == "complexType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b181) {
						b181 = true;
						ob.@SchemaType = ReadObject_XmlSchemaComplexType (false, true);
					}
					else if (Reader.LocalName == "keyref" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b182) {
						if (((object)o184) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o184.Add (ReadObject_XmlSchemaKeyref (false, true));
						n183++;
					}
					else if (Reader.LocalName == "unique" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b182) {
						if (((object)o184) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o184.Add (ReadObject_XmlSchemaUnique (false, true));
						n183++;
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

		public System.Xml.Schema.XmlSchemaChoice ReadObject_XmlSchemaChoice (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaChoice ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaChoice" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaChoice ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b185=false, b186=false;

			System.Xml.Schema.XmlSchemaObjectCollection o188;
			o188 = ob.@Items;
			int n187=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b186) {
						if (((object)o188) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o188.Add (ReadObject_XmlSchemaChoice (false, true));
						n187++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b185) {
						b185 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b186) {
						if (((object)o188) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o188.Add (ReadObject_XmlSchemaSequence (false, true));
						n187++;
					}
					else if (Reader.LocalName == "any" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b186) {
						if (((object)o188) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o188.Add (ReadObject_XmlSchemaAny (false, true));
						n187++;
					}
					else if (Reader.LocalName == "element" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b186) {
						if (((object)o188) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o188.Add (ReadObject_XmlSchemaElement (false, true));
						n187++;
					}
					else if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b186) {
						if (((object)o188) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o188.Add (ReadObject_XmlSchemaGroupRef (false, true));
						n187++;
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

		public System.Xml.Schema.XmlSchemaSequence ReadObject_XmlSchemaSequence (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaSequence ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaSequence" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaSequence ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "minOccurs" && Reader.NamespaceURI == "") {
					ob.@MinOccursString = Reader.Value;
				}
				else if (Reader.LocalName == "maxOccurs" && Reader.NamespaceURI == "") {
					ob.@MaxOccursString = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b189=false, b190=false;

			System.Xml.Schema.XmlSchemaObjectCollection o192;
			o192 = ob.@Items;
			int n191=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b190) {
						if (((object)o192) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o192.Add (ReadObject_XmlSchemaChoice (false, true));
						n191++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b189) {
						b189 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b190) {
						if (((object)o192) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o192.Add (ReadObject_XmlSchemaSequence (false, true));
						n191++;
					}
					else if (Reader.LocalName == "any" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b190) {
						if (((object)o192) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o192.Add (ReadObject_XmlSchemaAny (false, true));
						n191++;
					}
					else if (Reader.LocalName == "element" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b190) {
						if (((object)o192) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o192.Add (ReadObject_XmlSchemaElement (false, true));
						n191++;
					}
					else if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b190) {
						if (((object)o192) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o192.Add (ReadObject_XmlSchemaGroupRef (false, true));
						n191++;
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

		public System.Xml.Schema.XmlSchemaGroup ReadObject_XmlSchemaGroup (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaGroup ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaGroup" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaGroup ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b193=false, b194=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "choice" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b194) {
						b194 = true;
						ob.@Particle = ReadObject_XmlSchemaChoice (false, true);
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b193) {
						b193 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
					}
					else if (Reader.LocalName == "all" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b194) {
						b194 = true;
						ob.@Particle = ReadObject_XmlSchemaAll (false, true);
					}
					else if (Reader.LocalName == "sequence" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b194) {
						b194 = true;
						ob.@Particle = ReadObject_XmlSchemaSequence (false, true);
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

		public System.Xml.Schema.XmlSchemaRedefine ReadObject_XmlSchemaRedefine (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaRedefine ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaRedefine" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaRedefine ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "schemaLocation" && Reader.NamespaceURI == "") {
					ob.@SchemaLocation = Reader.Value;
				}
				else if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b195=false;

			System.Xml.Schema.XmlSchemaObjectCollection o197;
			o197 = ob.@Items;
			int n196=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "group" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b195) {
						if (((object)o197) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o197.Add (ReadObject_XmlSchemaGroup (false, true));
						n196++;
					}
					else if (Reader.LocalName == "simpleType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b195) {
						if (((object)o197) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o197.Add (ReadObject_XmlSchemaSimpleType (false, true));
						n196++;
					}
					else if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b195) {
						if (((object)o197) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o197.Add (ReadObject_XmlSchemaAnnotation (false, true));
						n196++;
					}
					else if (Reader.LocalName == "complexType" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b195) {
						if (((object)o197) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o197.Add (ReadObject_XmlSchemaComplexType (false, true));
						n196++;
					}
					else if (Reader.LocalName == "attributeGroup" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b195) {
						if (((object)o197) == null)
							throw CreateReadOnlyCollectionException ("System.Xml.Schema.XmlSchemaObjectCollection");
						o197.Add (ReadObject_XmlSchemaAttributeGroup (false, true));
						n196++;
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

		public System.Xml.Schema.XmlSchemaImport ReadObject_XmlSchemaImport (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaImport ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaImport" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaImport ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "schemaLocation" && Reader.NamespaceURI == "") {
					ob.@SchemaLocation = Reader.Value;
				}
				else if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b198=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b198) {
						b198 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaInclude ReadObject_XmlSchemaInclude (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaInclude ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaInclude" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaInclude ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "schemaLocation" && Reader.NamespaceURI == "") {
					ob.@SchemaLocation = Reader.Value;
				}
				else if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
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
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b199=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b199) {
						b199 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Xml.Schema.XmlSchemaNotation ReadObject_XmlSchemaNotation (bool isNullable, bool checkType)
		{
			System.Xml.Schema.XmlSchemaNotation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "XmlSchemaNotation" || t.Namespace != "http://www.w3.org/2001/XMLSchema")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Xml.Schema.XmlSchemaNotation ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "public" && Reader.NamespaceURI == "") {
					ob.@Public = Reader.Value;
				}
				else if (Reader.LocalName == "system" && Reader.NamespaceURI == "") {
					ob.@System = Reader.Value;
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
					ParseWsdlArrayType (attr);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@UnhandledAttributes = anyAttributeArray;

			#if NET_2_0
			ServiceDescription.SetExtensibleAttributes (ob);
			#endif
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b200=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "annotation" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b200) {
						b200 = true;
						ob.@Annotation = ReadObject_XmlSchemaAnnotation (false, true);
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

		public System.Web.Services.Description.OperationMessage ReadObject_OperationMessage (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.OperationMessage ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "OperationFault" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationFault (isNullable, checkType);
				else if (t.Name == "OperationInput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationInput (isNullable, checkType);
				else if (t.Name == "OperationOutput" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OperationOutput (isNullable, checkType);
				else if (t.Name != "OperationMessage" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}
			return ob;
		}

		public System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection ReadObject_ArrayOfAnyType (bool isNullable, bool checkType)
		{
			throw CreateReadOnlyCollectionException ("System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection");
		}

		public System.Web.Services.Description.MessageBinding ReadObject_MessageBinding (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.MessageBinding ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name == "FaultBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_FaultBinding (isNullable, checkType);
				else if (t.Name == "InputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_InputBinding (isNullable, checkType);
				else if (t.Name == "OutputBinding" && t.Namespace == "http://schemas.xmlsoap.org/wsdl/")
					return ReadObject_OutputBinding (isNullable, checkType);
				else if (t.Name != "MessageBinding" || t.Namespace != "http://schemas.xmlsoap.org/wsdl/")
					throw CreateUnknownTypeException(t);
			}
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
		public void WriteTree (System.Web.Services.Description.ServiceDescription ob)
		{
			WriteStartDocument ();
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
				XmlNode o201 = ob.@DocumentationElement;
				if (o201 is XmlElement) {
				if ((o201.Name == "documentation" && o201.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o201.WriteTo (Writer);
					WriteElementLiteral (o201, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o201.Name, o201.NamespaceURI);
			}
			if (ob.@Imports != null) {
				for (int n202 = 0; n202 < ob.@Imports.Count; n202++) {
					WriteObject_Import (ob.@Imports[n202], "import", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			WriteObject_Types (ob.@Types, "types", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			if (ob.@Messages != null) {
				for (int n203 = 0; n203 < ob.@Messages.Count; n203++) {
					WriteObject_Message (ob.@Messages[n203], "message", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@PortTypes != null) {
				for (int n204 = 0; n204 < ob.@PortTypes.Count; n204++) {
					WriteObject_PortType (ob.@PortTypes[n204], "portType", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Bindings != null) {
				for (int n205 = 0; n205 < ob.@Bindings.Count; n205++) {
					WriteObject_Binding (ob.@Bindings[n205], "binding", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Services != null) {
				for (int n206 = 0; n206 < ob.@Services.Count; n206++) {
					WriteObject_Service (ob.@Services[n206], "service", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o207 = ob.@DocumentationElement;
				if (o207 is XmlElement) {
				if ((o207.Name == "documentation" && o207.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o207.WriteTo (Writer);
					WriteElementLiteral (o207, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o207.Name, o207.NamespaceURI);
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
				XmlNode o208 = ob.@DocumentationElement;
				if (o208 is XmlElement) {
				if ((o208.Name == "documentation" && o208.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o208.WriteTo (Writer);
					WriteElementLiteral (o208, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o208.Name, o208.NamespaceURI);
			}
			if (ob.@Schemas != null) {
				for (int n209 = 0; n209 < ob.@Schemas.Count; n209++) {
					WriteObject_XmlSchema (ob.@Schemas[n209], "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
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
				XmlNode o210 = ob.@DocumentationElement;
				if (o210 is XmlElement) {
				if ((o210.Name == "documentation" && o210.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o210.WriteTo (Writer);
					WriteElementLiteral (o210, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o210.Name, o210.NamespaceURI);
			}
			if (ob.@Parts != null) {
				for (int n211 = 0; n211 < ob.@Parts.Count; n211++) {
					WriteObject_MessagePart (ob.@Parts[n211], "part", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o212 = ob.@DocumentationElement;
				if (o212 is XmlElement) {
				if ((o212.Name == "documentation" && o212.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o212.WriteTo (Writer);
					WriteElementLiteral (o212, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o212.Name, o212.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n213 = 0; n213 < ob.@Operations.Count; n213++) {
					WriteObject_Operation (ob.@Operations[n213], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o214 = ob.@DocumentationElement;
				if (o214 is XmlElement) {
				if ((o214.Name == "documentation" && o214.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o214.WriteTo (Writer);
					WriteElementLiteral (o214, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o214.Name, o214.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n215 = 0; n215 < ob.@Operations.Count; n215++) {
					WriteObject_OperationBinding (ob.@Operations[n215], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o216 = ob.@DocumentationElement;
				if (o216 is XmlElement) {
				if ((o216.Name == "documentation" && o216.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o216.WriteTo (Writer);
					WriteElementLiteral (o216, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o216.Name, o216.NamespaceURI);
			}
			if (ob.@Ports != null) {
				for (int n217 = 0; n217 < ob.@Ports.Count; n217++) {
					WriteObject_Port (ob.@Ports[n217], "port", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o218 = ob.@DocumentationElement;
				if (o218 is XmlElement) {
				if ((o218.Name == "documentation" && o218.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o218.WriteTo (Writer);
					WriteElementLiteral (o218, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o218.Name, o218.NamespaceURI);
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
				XmlNode o219 = ob.@DocumentationElement;
				if (o219 is XmlElement) {
				if ((o219.Name == "documentation" && o219.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o219.WriteTo (Writer);
					WriteElementLiteral (o219, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o219.Name, o219.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n220 = 0; n220 < ob.@Faults.Count; n220++) {
					WriteObject_OperationFault (ob.@Faults[n220], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Messages != null) {
				for (int n221 = 0; n221 < ob.@Messages.Count; n221++) {
					if (((object)ob.@Messages[n221]) == null) { }
					else if (ob.@Messages[n221].GetType() == typeof(System.Web.Services.Description.OperationInput)) {
						WriteObject_OperationInput (((System.Web.Services.Description.OperationInput) ob.@Messages[n221]), "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.@Messages[n221].GetType() == typeof(System.Web.Services.Description.OperationOutput)) {
						WriteObject_OperationOutput (((System.Web.Services.Description.OperationOutput) ob.@Messages[n221]), "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Messages[n221]);
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
				XmlNode o222 = ob.@DocumentationElement;
				if (o222 is XmlElement) {
				if ((o222.Name == "documentation" && o222.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o222.WriteTo (Writer);
					WriteElementLiteral (o222, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o222.Name, o222.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n223 = 0; n223 < ob.@Faults.Count; n223++) {
					WriteObject_FaultBinding (ob.@Faults[n223], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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
				XmlNode o224 = ob.@DocumentationElement;
				if (o224 is XmlElement) {
				if ((o224.Name == "documentation" && o224.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o224.WriteTo (Writer);
					WriteElementLiteral (o224, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o224.Name, o224.NamespaceURI);
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
				XmlNode o225 = ob.@DocumentationElement;
				if (o225 is XmlElement) {
				if ((o225.Name == "documentation" && o225.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o225.WriteTo (Writer);
					WriteElementLiteral (o225, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o225.Name, o225.NamespaceURI);
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
				XmlNode o226 = ob.@DocumentationElement;
				if (o226 is XmlElement) {
				if ((o226.Name == "documentation" && o226.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o226.WriteTo (Writer);
					WriteElementLiteral (o226, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o226.Name, o226.NamespaceURI);
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
				XmlNode o227 = ob.@DocumentationElement;
				if (o227 is XmlElement) {
				if ((o227.Name == "documentation" && o227.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o227.WriteTo (Writer);
					WriteElementLiteral (o227, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o227.Name, o227.NamespaceURI);
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
				XmlNode o228 = ob.@DocumentationElement;
				if (o228 is XmlElement) {
				if ((o228.Name == "documentation" && o228.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o228.WriteTo (Writer);
					WriteElementLiteral (o228, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o228.Name, o228.NamespaceURI);
			}
			if (ob.@Extensions != null) {
				WriteStartElement ("Extensions", "http://schemas.xmlsoap.org/wsdl/", ob.@Extensions);
				for (int n229 = 0; n229 < ob.@Extensions.Count; n229++) {
					WriteObject_anyType (ob.@Extensions[n229], "anyType", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
				}
				WriteEndElement (ob.@Extensions);
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
				XmlNode o230 = ob.@DocumentationElement;
				if (o230 is XmlElement) {
				if ((o230.Name == "documentation" && o230.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o230.WriteTo (Writer);
					WriteElementLiteral (o230, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o230.Name, o230.NamespaceURI);
			}
			if (ob.@Extensions != null) {
				WriteStartElement ("Extensions", "http://schemas.xmlsoap.org/wsdl/", ob.@Extensions);
				for (int n231 = 0; n231 < ob.@Extensions.Count; n231++) {
					WriteObject_anyType (ob.@Extensions[n231], "anyType", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
				}
				WriteEndElement (ob.@Extensions);
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
				XmlNode o232 = ob.@DocumentationElement;
				if (o232 is XmlElement) {
				if ((o232.Name == "documentation" && o232.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o232.WriteTo (Writer);
					WriteElementLiteral (o232, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o232.Name, o232.NamespaceURI);
			}
			if (ob.@Extensions != null) {
				WriteStartElement ("Extensions", "http://schemas.xmlsoap.org/wsdl/", ob.@Extensions);
				for (int n233 = 0; n233 < ob.@Extensions.Count; n233++) {
					WriteObject_anyType (ob.@Extensions[n233], "anyType", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
				}
				WriteEndElement (ob.@Extensions);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_anyType (System.Object ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Object))
			{ }
			else if (type == typeof(System.Web.Services.Description.DocumentableItem)) { 
				WriteObject_DocumentableItem((System.Web.Services.Description.DocumentableItem)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.DocumentableItem)) { 
				WriteObject_DocumentableItem1((System.Web.Services.Description.DocumentableItem)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Import)) { 
				WriteObject_Import((System.Web.Services.Description.Import)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaObject)) { 
				WriteObject_XmlSchemaObject((System.Xml.Schema.XmlSchemaObject)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaForm)) { 
				WriteObject_XmlSchemaForm((System.Xml.Schema.XmlSchemaForm)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaDerivationMethod)) { 
				WriteObject_XmlSchemaDerivationMethod((System.Xml.Schema.XmlSchemaDerivationMethod)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaExternal)) { 
				WriteObject_XmlSchemaExternal((System.Xml.Schema.XmlSchemaExternal)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaDocumentation)) { 
				WriteObject_XmlSchemaDocumentation((System.Xml.Schema.XmlSchemaDocumentation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAppInfo)) { 
				WriteObject_XmlSchemaAppInfo((System.Xml.Schema.XmlSchemaAppInfo)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnnotation)) { 
				WriteObject_XmlSchemaAnnotation((System.Xml.Schema.XmlSchemaAnnotation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnnotated)) { 
				WriteObject_XmlSchemaAnnotated((System.Xml.Schema.XmlSchemaAnnotated)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) { 
				WriteObject_XmlSchemaAttributeGroupRef((System.Xml.Schema.XmlSchemaAttributeGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaType)) { 
				WriteObject_XmlSchemaType((System.Xml.Schema.XmlSchemaType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.XmlQualifiedName[])) { 
				WriteObject_ArrayOfQName((System.Xml.XmlQualifiedName[])ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeContent)) { 
				WriteObject_XmlSchemaSimpleTypeContent((System.Xml.Schema.XmlSchemaSimpleTypeContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion)) { 
				WriteObject_XmlSchemaSimpleTypeUnion((System.Xml.Schema.XmlSchemaSimpleTypeUnion)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList)) { 
				WriteObject_XmlSchemaSimpleTypeList((System.Xml.Schema.XmlSchemaSimpleTypeList)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFacet)) { 
				WriteObject_XmlSchemaFacet((System.Xml.Schema.XmlSchemaFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) { 
				WriteObject_XmlSchemaPatternFacet((System.Xml.Schema.XmlSchemaPatternFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) { 
				WriteObject_XmlSchemaWhiteSpaceFacet((System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) { 
				WriteObject_XmlSchemaEnumerationFacet((System.Xml.Schema.XmlSchemaEnumerationFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNumericFacet)) { 
				WriteObject_XmlSchemaNumericFacet((System.Xml.Schema.XmlSchemaNumericFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) { 
				WriteObject_XmlSchemaMaxLengthFacet((System.Xml.Schema.XmlSchemaMaxLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) { 
				WriteObject_XmlSchemaMinLengthFacet((System.Xml.Schema.XmlSchemaMinLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) { 
				WriteObject_XmlSchemaLengthFacet((System.Xml.Schema.XmlSchemaLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) { 
				WriteObject_XmlSchemaFractionDigitsFacet((System.Xml.Schema.XmlSchemaFractionDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) { 
				WriteObject_XmlSchemaTotalDigitsFacet((System.Xml.Schema.XmlSchemaTotalDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) { 
				WriteObject_XmlSchemaMaxInclusiveFacet((System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) { 
				WriteObject_XmlSchemaMaxExclusiveFacet((System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) { 
				WriteObject_XmlSchemaMinInclusiveFacet((System.Xml.Schema.XmlSchemaMinInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) { 
				WriteObject_XmlSchemaMinExclusiveFacet((System.Xml.Schema.XmlSchemaMinExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction)) { 
				WriteObject_XmlSchemaSimpleTypeRestriction((System.Xml.Schema.XmlSchemaSimpleTypeRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType)) { 
				WriteObject_XmlSchemaSimpleType((System.Xml.Schema.XmlSchemaSimpleType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaUse)) { 
				WriteObject_XmlSchemaUse((System.Xml.Schema.XmlSchemaUse)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttribute)) { 
				WriteObject_XmlSchemaAttribute((System.Xml.Schema.XmlSchemaAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContentProcessing)) { 
				WriteObject_XmlSchemaContentProcessing((System.Xml.Schema.XmlSchemaContentProcessing)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnyAttribute)) { 
				WriteObject_XmlSchemaAnyAttribute((System.Xml.Schema.XmlSchemaAnyAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) { 
				WriteObject_XmlSchemaAttributeGroup((System.Xml.Schema.XmlSchemaAttributeGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaParticle)) { 
				WriteObject_XmlSchemaParticle((System.Xml.Schema.XmlSchemaParticle)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAny)) { 
				WriteObject_XmlSchemaAny((System.Xml.Schema.XmlSchemaAny)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef)) { 
				WriteObject_XmlSchemaGroupRef((System.Xml.Schema.XmlSchemaGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupBase)) { 
				WriteObject_XmlSchemaGroupBase((System.Xml.Schema.XmlSchemaGroupBase)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAll)) { 
				WriteObject_XmlSchemaAll((System.Xml.Schema.XmlSchemaAll)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContent)) { 
				WriteObject_XmlSchemaContent((System.Xml.Schema.XmlSchemaContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension)) { 
				WriteObject_XmlSchemaComplexContentExtension((System.Xml.Schema.XmlSchemaComplexContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction)) { 
				WriteObject_XmlSchemaComplexContentRestriction((System.Xml.Schema.XmlSchemaComplexContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContentModel)) { 
				WriteObject_XmlSchemaContentModel((System.Xml.Schema.XmlSchemaContentModel)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent)) { 
				WriteObject_XmlSchemaComplexContent((System.Xml.Schema.XmlSchemaComplexContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension)) { 
				WriteObject_XmlSchemaSimpleContentExtension((System.Xml.Schema.XmlSchemaSimpleContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction)) { 
				WriteObject_XmlSchemaSimpleContentRestriction((System.Xml.Schema.XmlSchemaSimpleContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent)) { 
				WriteObject_XmlSchemaSimpleContent((System.Xml.Schema.XmlSchemaSimpleContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexType)) { 
				WriteObject_XmlSchemaComplexType((System.Xml.Schema.XmlSchemaComplexType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaXPath)) { 
				WriteObject_XmlSchemaXPath((System.Xml.Schema.XmlSchemaXPath)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaIdentityConstraint)) { 
				WriteObject_XmlSchemaIdentityConstraint((System.Xml.Schema.XmlSchemaIdentityConstraint)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKeyref)) { 
				WriteObject_XmlSchemaKeyref((System.Xml.Schema.XmlSchemaKeyref)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKey)) { 
				WriteObject_XmlSchemaKey((System.Xml.Schema.XmlSchemaKey)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaUnique)) { 
				WriteObject_XmlSchemaUnique((System.Xml.Schema.XmlSchemaUnique)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaElement)) { 
				WriteObject_XmlSchemaElement((System.Xml.Schema.XmlSchemaElement)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaChoice)) { 
				WriteObject_XmlSchemaChoice((System.Xml.Schema.XmlSchemaChoice)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSequence)) { 
				WriteObject_XmlSchemaSequence((System.Xml.Schema.XmlSchemaSequence)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroup)) { 
				WriteObject_XmlSchemaGroup((System.Xml.Schema.XmlSchemaGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaRedefine)) { 
				WriteObject_XmlSchemaRedefine((System.Xml.Schema.XmlSchemaRedefine)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaImport)) { 
				WriteObject_XmlSchemaImport((System.Xml.Schema.XmlSchemaImport)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaInclude)) { 
				WriteObject_XmlSchemaInclude((System.Xml.Schema.XmlSchemaInclude)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNotation)) { 
				WriteObject_XmlSchemaNotation((System.Xml.Schema.XmlSchemaNotation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchema)) { 
				WriteObject_XmlSchema((System.Xml.Schema.XmlSchema)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Types)) { 
				WriteObject_Types((System.Web.Services.Description.Types)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.MessagePart)) { 
				WriteObject_MessagePart((System.Web.Services.Description.MessagePart)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Message)) { 
				WriteObject_Message((System.Web.Services.Description.Message)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationMessage)) { 
				WriteObject_OperationMessage((System.Web.Services.Description.OperationMessage)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationFault)) { 
				WriteObject_OperationFault((System.Web.Services.Description.OperationFault)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationInput)) { 
				WriteObject_OperationInput((System.Web.Services.Description.OperationInput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationOutput)) { 
				WriteObject_OperationOutput((System.Web.Services.Description.OperationOutput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Operation)) { 
				WriteObject_Operation((System.Web.Services.Description.Operation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.PortType)) { 
				WriteObject_PortType((System.Web.Services.Description.PortType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)) { 
				WriteObject_ArrayOfAnyType((System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.MessageBinding)) { 
				WriteObject_MessageBinding((System.Web.Services.Description.MessageBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.FaultBinding)) { 
				WriteObject_FaultBinding((System.Web.Services.Description.FaultBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.InputBinding)) { 
				WriteObject_InputBinding((System.Web.Services.Description.InputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OutputBinding)) { 
				WriteObject_OutputBinding((System.Web.Services.Description.OutputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationBinding)) { 
				WriteObject_OperationBinding((System.Web.Services.Description.OperationBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Binding)) { 
				WriteObject_Binding((System.Web.Services.Description.Binding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Port)) { 
				WriteObject_Port((System.Web.Services.Description.Port)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Service)) { 
				WriteObject_Service((System.Web.Services.Description.Service)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.ServiceDescription)) { 
				WriteObject_ServiceDescription((System.Web.Services.Description.ServiceDescription)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				WriteTypedPrimitive (element, namesp, ob, true);
				return;
			}
		}

		void WriteObject_DocumentableItem (System.Web.Services.Description.DocumentableItem ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.DocumentableItem))
			{ }
			else if (type == typeof(System.Web.Services.Description.ServiceDescription)) { 
				WriteObject_ServiceDescription((System.Web.Services.Description.ServiceDescription)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("DocumentableItem", "");

			if (ob.@DocumentationElement != null) {
				XmlNode o234 = ob.@DocumentationElement;
				if (o234 is XmlElement) {
				if ((o234.Name == "documentation" && o234.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o234.WriteTo (Writer);
					WriteElementLiteral (o234, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o234.Name, o234.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_DocumentableItem1 (System.Web.Services.Description.DocumentableItem ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.DocumentableItem))
			{ }
			else if (type == typeof(System.Web.Services.Description.Import)) { 
				WriteObject_Import((System.Web.Services.Description.Import)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Types)) { 
				WriteObject_Types((System.Web.Services.Description.Types)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.MessagePart)) { 
				WriteObject_MessagePart((System.Web.Services.Description.MessagePart)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Message)) { 
				WriteObject_Message((System.Web.Services.Description.Message)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationMessage)) { 
				WriteObject_OperationMessage((System.Web.Services.Description.OperationMessage)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationFault)) { 
				WriteObject_OperationFault((System.Web.Services.Description.OperationFault)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationInput)) { 
				WriteObject_OperationInput((System.Web.Services.Description.OperationInput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationOutput)) { 
				WriteObject_OperationOutput((System.Web.Services.Description.OperationOutput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Operation)) { 
				WriteObject_Operation((System.Web.Services.Description.Operation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.PortType)) { 
				WriteObject_PortType((System.Web.Services.Description.PortType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.MessageBinding)) { 
				WriteObject_MessageBinding((System.Web.Services.Description.MessageBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.FaultBinding)) { 
				WriteObject_FaultBinding((System.Web.Services.Description.FaultBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.InputBinding)) { 
				WriteObject_InputBinding((System.Web.Services.Description.InputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OutputBinding)) { 
				WriteObject_OutputBinding((System.Web.Services.Description.OutputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationBinding)) { 
				WriteObject_OperationBinding((System.Web.Services.Description.OperationBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Binding)) { 
				WriteObject_Binding((System.Web.Services.Description.Binding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Port)) { 
				WriteObject_Port((System.Web.Services.Description.Port)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.Service)) { 
				WriteObject_Service((System.Web.Services.Description.Service)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("DocumentableItem", "http://schemas.xmlsoap.org/wsdl/");

			if (ob.@DocumentationElement != null) {
				XmlNode o235 = ob.@DocumentationElement;
				if (o235 is XmlElement) {
				if ((o235.Name == "documentation" && o235.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o235.WriteTo (Writer);
					WriteElementLiteral (o235, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o235.Name, o235.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaObject (System.Xml.Schema.XmlSchemaObject ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaObject))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaExternal)) { 
				WriteObject_XmlSchemaExternal((System.Xml.Schema.XmlSchemaExternal)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaDocumentation)) { 
				WriteObject_XmlSchemaDocumentation((System.Xml.Schema.XmlSchemaDocumentation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAppInfo)) { 
				WriteObject_XmlSchemaAppInfo((System.Xml.Schema.XmlSchemaAppInfo)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnnotation)) { 
				WriteObject_XmlSchemaAnnotation((System.Xml.Schema.XmlSchemaAnnotation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnnotated)) { 
				WriteObject_XmlSchemaAnnotated((System.Xml.Schema.XmlSchemaAnnotated)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) { 
				WriteObject_XmlSchemaAttributeGroupRef((System.Xml.Schema.XmlSchemaAttributeGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaType)) { 
				WriteObject_XmlSchemaType((System.Xml.Schema.XmlSchemaType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeContent)) { 
				WriteObject_XmlSchemaSimpleTypeContent((System.Xml.Schema.XmlSchemaSimpleTypeContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion)) { 
				WriteObject_XmlSchemaSimpleTypeUnion((System.Xml.Schema.XmlSchemaSimpleTypeUnion)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList)) { 
				WriteObject_XmlSchemaSimpleTypeList((System.Xml.Schema.XmlSchemaSimpleTypeList)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFacet)) { 
				WriteObject_XmlSchemaFacet((System.Xml.Schema.XmlSchemaFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) { 
				WriteObject_XmlSchemaPatternFacet((System.Xml.Schema.XmlSchemaPatternFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) { 
				WriteObject_XmlSchemaWhiteSpaceFacet((System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) { 
				WriteObject_XmlSchemaEnumerationFacet((System.Xml.Schema.XmlSchemaEnumerationFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNumericFacet)) { 
				WriteObject_XmlSchemaNumericFacet((System.Xml.Schema.XmlSchemaNumericFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) { 
				WriteObject_XmlSchemaMaxLengthFacet((System.Xml.Schema.XmlSchemaMaxLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) { 
				WriteObject_XmlSchemaMinLengthFacet((System.Xml.Schema.XmlSchemaMinLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) { 
				WriteObject_XmlSchemaLengthFacet((System.Xml.Schema.XmlSchemaLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) { 
				WriteObject_XmlSchemaFractionDigitsFacet((System.Xml.Schema.XmlSchemaFractionDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) { 
				WriteObject_XmlSchemaTotalDigitsFacet((System.Xml.Schema.XmlSchemaTotalDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) { 
				WriteObject_XmlSchemaMaxInclusiveFacet((System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) { 
				WriteObject_XmlSchemaMaxExclusiveFacet((System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) { 
				WriteObject_XmlSchemaMinInclusiveFacet((System.Xml.Schema.XmlSchemaMinInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) { 
				WriteObject_XmlSchemaMinExclusiveFacet((System.Xml.Schema.XmlSchemaMinExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction)) { 
				WriteObject_XmlSchemaSimpleTypeRestriction((System.Xml.Schema.XmlSchemaSimpleTypeRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType)) { 
				WriteObject_XmlSchemaSimpleType((System.Xml.Schema.XmlSchemaSimpleType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttribute)) { 
				WriteObject_XmlSchemaAttribute((System.Xml.Schema.XmlSchemaAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnyAttribute)) { 
				WriteObject_XmlSchemaAnyAttribute((System.Xml.Schema.XmlSchemaAnyAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) { 
				WriteObject_XmlSchemaAttributeGroup((System.Xml.Schema.XmlSchemaAttributeGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaParticle)) { 
				WriteObject_XmlSchemaParticle((System.Xml.Schema.XmlSchemaParticle)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAny)) { 
				WriteObject_XmlSchemaAny((System.Xml.Schema.XmlSchemaAny)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef)) { 
				WriteObject_XmlSchemaGroupRef((System.Xml.Schema.XmlSchemaGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupBase)) { 
				WriteObject_XmlSchemaGroupBase((System.Xml.Schema.XmlSchemaGroupBase)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAll)) { 
				WriteObject_XmlSchemaAll((System.Xml.Schema.XmlSchemaAll)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContent)) { 
				WriteObject_XmlSchemaContent((System.Xml.Schema.XmlSchemaContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension)) { 
				WriteObject_XmlSchemaComplexContentExtension((System.Xml.Schema.XmlSchemaComplexContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction)) { 
				WriteObject_XmlSchemaComplexContentRestriction((System.Xml.Schema.XmlSchemaComplexContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContentModel)) { 
				WriteObject_XmlSchemaContentModel((System.Xml.Schema.XmlSchemaContentModel)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent)) { 
				WriteObject_XmlSchemaComplexContent((System.Xml.Schema.XmlSchemaComplexContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension)) { 
				WriteObject_XmlSchemaSimpleContentExtension((System.Xml.Schema.XmlSchemaSimpleContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction)) { 
				WriteObject_XmlSchemaSimpleContentRestriction((System.Xml.Schema.XmlSchemaSimpleContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent)) { 
				WriteObject_XmlSchemaSimpleContent((System.Xml.Schema.XmlSchemaSimpleContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexType)) { 
				WriteObject_XmlSchemaComplexType((System.Xml.Schema.XmlSchemaComplexType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaXPath)) { 
				WriteObject_XmlSchemaXPath((System.Xml.Schema.XmlSchemaXPath)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaIdentityConstraint)) { 
				WriteObject_XmlSchemaIdentityConstraint((System.Xml.Schema.XmlSchemaIdentityConstraint)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKeyref)) { 
				WriteObject_XmlSchemaKeyref((System.Xml.Schema.XmlSchemaKeyref)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKey)) { 
				WriteObject_XmlSchemaKey((System.Xml.Schema.XmlSchemaKey)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaUnique)) { 
				WriteObject_XmlSchemaUnique((System.Xml.Schema.XmlSchemaUnique)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaElement)) { 
				WriteObject_XmlSchemaElement((System.Xml.Schema.XmlSchemaElement)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaChoice)) { 
				WriteObject_XmlSchemaChoice((System.Xml.Schema.XmlSchemaChoice)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSequence)) { 
				WriteObject_XmlSchemaSequence((System.Xml.Schema.XmlSchemaSequence)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroup)) { 
				WriteObject_XmlSchemaGroup((System.Xml.Schema.XmlSchemaGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaRedefine)) { 
				WriteObject_XmlSchemaRedefine((System.Xml.Schema.XmlSchemaRedefine)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaImport)) { 
				WriteObject_XmlSchemaImport((System.Xml.Schema.XmlSchemaImport)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaInclude)) { 
				WriteObject_XmlSchemaInclude((System.Xml.Schema.XmlSchemaInclude)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNotation)) { 
				WriteObject_XmlSchemaNotation((System.Xml.Schema.XmlSchemaNotation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchema)) { 
				WriteObject_XmlSchema((System.Xml.Schema.XmlSchema)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaObject", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaForm (System.Xml.Schema.XmlSchemaForm ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaForm))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaForm", "");

			Writer.WriteString (GetEnumValue_XmlSchemaForm (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_XmlSchemaForm (System.Xml.Schema.XmlSchemaForm val)
		{
			switch (val) {
				case System.Xml.Schema.XmlSchemaForm.Qualified: return "qualified";
				case System.Xml.Schema.XmlSchemaForm.Unqualified: return "unqualified";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}

		void WriteObject_XmlSchemaDerivationMethod (System.Xml.Schema.XmlSchemaDerivationMethod ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaDerivationMethod))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaDerivationMethod", "");

			Writer.WriteString (GetEnumValue_XmlSchemaDerivationMethod (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		static readonly string[] _xmlNamesXmlSchemaDerivationMethod = { "","substitution","extension","restriction","list","union","#all" };
		static readonly long[] _valuesXmlSchemaDerivationMethod = { 0L,1L,2L,4L,8L,16L,255L };

		string GetEnumValue_XmlSchemaDerivationMethod (System.Xml.Schema.XmlSchemaDerivationMethod val)
		{
			switch (val) {
				case System.Xml.Schema.XmlSchemaDerivationMethod.Empty: return "";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Substitution: return "substitution";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Extension: return "extension";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Restriction: return "restriction";
				case System.Xml.Schema.XmlSchemaDerivationMethod.List: return "list";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Union: return "union";
				case System.Xml.Schema.XmlSchemaDerivationMethod.All: return "#all";
				default:
					if (val.ToString () == "0") return string.Empty;
					return FromEnum ((long) val, _xmlNamesXmlSchemaDerivationMethod, _valuesXmlSchemaDerivationMethod);
			}
		}

		void WriteObject_XmlSchemaExternal (System.Xml.Schema.XmlSchemaExternal ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaExternal))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaRedefine)) { 
				WriteObject_XmlSchemaRedefine((System.Xml.Schema.XmlSchemaRedefine)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaImport)) { 
				WriteObject_XmlSchemaImport((System.Xml.Schema.XmlSchemaImport)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaInclude)) { 
				WriteObject_XmlSchemaInclude((System.Xml.Schema.XmlSchemaInclude)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaExternal", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o236 = ob.@UnhandledAttributes;
			if (o236 != null) {
				foreach (XmlAttribute o237 in o236)
					if (o237.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o237, ob);
			}

			WriteAttribute ("schemaLocation", "", ((ob.@SchemaLocation != null) ? (ob.@SchemaLocation).ToString() : null));
			WriteAttribute ("id", "", ob.@Id);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaDocumentation (System.Xml.Schema.XmlSchemaDocumentation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaDocumentation))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaDocumentation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			WriteAttribute ("source", "", ((ob.@Source != null) ? (ob.@Source).ToString() : null));
			WriteAttribute ("xml_x003A_lang", "", ob.@Language);

			if (ob.@Markup != null) {
				foreach (XmlNode o238 in ob.@Markup) {
					XmlNode o239 = o238;
					if (o239 is XmlElement) {
					}
					else o239.WriteTo (Writer);
					WriteElementLiteral (o239, "", "", false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAppInfo (System.Xml.Schema.XmlSchemaAppInfo ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAppInfo))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAppInfo", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			WriteAttribute ("source", "", ((ob.@Source != null) ? (ob.@Source).ToString() : null));

			if (ob.@Markup != null) {
				foreach (XmlNode o240 in ob.@Markup) {
					XmlNode o241 = o240;
					if (o241 is XmlElement) {
					}
					else o241.WriteTo (Writer);
					WriteElementLiteral (o241, "", "", false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAnnotation (System.Xml.Schema.XmlSchemaAnnotation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAnnotation))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAnnotation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o242 = ob.@UnhandledAttributes;
			if (o242 != null) {
				foreach (XmlAttribute o243 in o242)
					if (o243.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o243, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			if (ob.@Items != null) {
				for (int n244 = 0; n244 < ob.@Items.Count; n244++) {
					if (((object)ob.@Items[n244]) == null) { }
					else if (ob.@Items[n244].GetType() == typeof(System.Xml.Schema.XmlSchemaDocumentation)) {
						WriteObject_XmlSchemaDocumentation (((System.Xml.Schema.XmlSchemaDocumentation) ob.@Items[n244]), "documentation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n244].GetType() == typeof(System.Xml.Schema.XmlSchemaAppInfo)) {
						WriteObject_XmlSchemaAppInfo (((System.Xml.Schema.XmlSchemaAppInfo) ob.@Items[n244]), "appinfo", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n244]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAnnotated (System.Xml.Schema.XmlSchemaAnnotated ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAnnotated))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) { 
				WriteObject_XmlSchemaAttributeGroupRef((System.Xml.Schema.XmlSchemaAttributeGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaType)) { 
				WriteObject_XmlSchemaType((System.Xml.Schema.XmlSchemaType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeContent)) { 
				WriteObject_XmlSchemaSimpleTypeContent((System.Xml.Schema.XmlSchemaSimpleTypeContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion)) { 
				WriteObject_XmlSchemaSimpleTypeUnion((System.Xml.Schema.XmlSchemaSimpleTypeUnion)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList)) { 
				WriteObject_XmlSchemaSimpleTypeList((System.Xml.Schema.XmlSchemaSimpleTypeList)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFacet)) { 
				WriteObject_XmlSchemaFacet((System.Xml.Schema.XmlSchemaFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) { 
				WriteObject_XmlSchemaPatternFacet((System.Xml.Schema.XmlSchemaPatternFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) { 
				WriteObject_XmlSchemaWhiteSpaceFacet((System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) { 
				WriteObject_XmlSchemaEnumerationFacet((System.Xml.Schema.XmlSchemaEnumerationFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNumericFacet)) { 
				WriteObject_XmlSchemaNumericFacet((System.Xml.Schema.XmlSchemaNumericFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) { 
				WriteObject_XmlSchemaMaxLengthFacet((System.Xml.Schema.XmlSchemaMaxLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) { 
				WriteObject_XmlSchemaMinLengthFacet((System.Xml.Schema.XmlSchemaMinLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) { 
				WriteObject_XmlSchemaLengthFacet((System.Xml.Schema.XmlSchemaLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) { 
				WriteObject_XmlSchemaFractionDigitsFacet((System.Xml.Schema.XmlSchemaFractionDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) { 
				WriteObject_XmlSchemaTotalDigitsFacet((System.Xml.Schema.XmlSchemaTotalDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) { 
				WriteObject_XmlSchemaMaxInclusiveFacet((System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) { 
				WriteObject_XmlSchemaMaxExclusiveFacet((System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) { 
				WriteObject_XmlSchemaMinInclusiveFacet((System.Xml.Schema.XmlSchemaMinInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) { 
				WriteObject_XmlSchemaMinExclusiveFacet((System.Xml.Schema.XmlSchemaMinExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction)) { 
				WriteObject_XmlSchemaSimpleTypeRestriction((System.Xml.Schema.XmlSchemaSimpleTypeRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType)) { 
				WriteObject_XmlSchemaSimpleType((System.Xml.Schema.XmlSchemaSimpleType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttribute)) { 
				WriteObject_XmlSchemaAttribute((System.Xml.Schema.XmlSchemaAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAnyAttribute)) { 
				WriteObject_XmlSchemaAnyAttribute((System.Xml.Schema.XmlSchemaAnyAttribute)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) { 
				WriteObject_XmlSchemaAttributeGroup((System.Xml.Schema.XmlSchemaAttributeGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaParticle)) { 
				WriteObject_XmlSchemaParticle((System.Xml.Schema.XmlSchemaParticle)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAny)) { 
				WriteObject_XmlSchemaAny((System.Xml.Schema.XmlSchemaAny)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef)) { 
				WriteObject_XmlSchemaGroupRef((System.Xml.Schema.XmlSchemaGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupBase)) { 
				WriteObject_XmlSchemaGroupBase((System.Xml.Schema.XmlSchemaGroupBase)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAll)) { 
				WriteObject_XmlSchemaAll((System.Xml.Schema.XmlSchemaAll)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContent)) { 
				WriteObject_XmlSchemaContent((System.Xml.Schema.XmlSchemaContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension)) { 
				WriteObject_XmlSchemaComplexContentExtension((System.Xml.Schema.XmlSchemaComplexContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction)) { 
				WriteObject_XmlSchemaComplexContentRestriction((System.Xml.Schema.XmlSchemaComplexContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaContentModel)) { 
				WriteObject_XmlSchemaContentModel((System.Xml.Schema.XmlSchemaContentModel)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent)) { 
				WriteObject_XmlSchemaComplexContent((System.Xml.Schema.XmlSchemaComplexContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension)) { 
				WriteObject_XmlSchemaSimpleContentExtension((System.Xml.Schema.XmlSchemaSimpleContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction)) { 
				WriteObject_XmlSchemaSimpleContentRestriction((System.Xml.Schema.XmlSchemaSimpleContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent)) { 
				WriteObject_XmlSchemaSimpleContent((System.Xml.Schema.XmlSchemaSimpleContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexType)) { 
				WriteObject_XmlSchemaComplexType((System.Xml.Schema.XmlSchemaComplexType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaXPath)) { 
				WriteObject_XmlSchemaXPath((System.Xml.Schema.XmlSchemaXPath)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaIdentityConstraint)) { 
				WriteObject_XmlSchemaIdentityConstraint((System.Xml.Schema.XmlSchemaIdentityConstraint)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKeyref)) { 
				WriteObject_XmlSchemaKeyref((System.Xml.Schema.XmlSchemaKeyref)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKey)) { 
				WriteObject_XmlSchemaKey((System.Xml.Schema.XmlSchemaKey)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaUnique)) { 
				WriteObject_XmlSchemaUnique((System.Xml.Schema.XmlSchemaUnique)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaElement)) { 
				WriteObject_XmlSchemaElement((System.Xml.Schema.XmlSchemaElement)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaChoice)) { 
				WriteObject_XmlSchemaChoice((System.Xml.Schema.XmlSchemaChoice)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSequence)) { 
				WriteObject_XmlSchemaSequence((System.Xml.Schema.XmlSchemaSequence)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroup)) { 
				WriteObject_XmlSchemaGroup((System.Xml.Schema.XmlSchemaGroup)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNotation)) { 
				WriteObject_XmlSchemaNotation((System.Xml.Schema.XmlSchemaNotation)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAnnotated", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o245 = ob.@UnhandledAttributes;
			if (o245 != null) {
				foreach (XmlAttribute o246 in o245)
					if (o246.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o246, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAttributeGroupRef (System.Xml.Schema.XmlSchemaAttributeGroupRef ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttributeGroupRef", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o247 = ob.@UnhandledAttributes;
			if (o247 != null) {
				foreach (XmlAttribute o248 in o247)
					if (o248.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o248, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaType (System.Xml.Schema.XmlSchemaType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaType))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType)) { 
				WriteObject_XmlSchemaSimpleType((System.Xml.Schema.XmlSchemaSimpleType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexType)) { 
				WriteObject_XmlSchemaComplexType((System.Xml.Schema.XmlSchemaComplexType)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaType", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o249 = ob.@UnhandledAttributes;
			if (o249 != null) {
				foreach (XmlAttribute o250 in o249)
					if (o250.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o250, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ArrayOfQName (System.Xml.XmlQualifiedName[] ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.XmlQualifiedName[]))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ArrayOfQName", "");

			for (int n251 = 0; n251 < ob.Length; n251++) {
				WriteElementQualifiedName ("QName", "", ob[n251]);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeContent (System.Xml.Schema.XmlSchemaSimpleTypeContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeContent))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion)) { 
				WriteObject_XmlSchemaSimpleTypeUnion((System.Xml.Schema.XmlSchemaSimpleTypeUnion)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList)) { 
				WriteObject_XmlSchemaSimpleTypeList((System.Xml.Schema.XmlSchemaSimpleTypeList)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction)) { 
				WriteObject_XmlSchemaSimpleTypeRestriction((System.Xml.Schema.XmlSchemaSimpleTypeRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o252 = ob.@UnhandledAttributes;
			if (o252 != null) {
				foreach (XmlAttribute o253 in o252)
					if (o253.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o253, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeUnion (System.Xml.Schema.XmlSchemaSimpleTypeUnion ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeUnion", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o254 = ob.@UnhandledAttributes;
			if (o254 != null) {
				foreach (XmlAttribute o255 in o254)
					if (o255.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o255, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			string s256 = null;
			if (ob.@MemberTypes != null) {
				System.Text.StringBuilder s257 = new System.Text.StringBuilder();
				for (int n258 = 0; n258 < ob.@MemberTypes.Length; n258++) {
					s257.Append (FromXmlQualifiedName (ob.@MemberTypes[n258])).Append (" ");
				}
				s256 = s257.ToString ().Trim ();
			}
			WriteAttribute ("memberTypes", "", s256);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@BaseTypes != null) {
				for (int n259 = 0; n259 < ob.@BaseTypes.Count; n259++) {
					WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@BaseTypes[n259]), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeList (System.Xml.Schema.XmlSchemaSimpleTypeList ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeList", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o260 = ob.@UnhandledAttributes;
			if (o260 != null) {
				foreach (XmlAttribute o261 in o260)
					if (o261.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o261, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("itemType", "", FromXmlQualifiedName (ob.@ItemTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@ItemType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaFacet (System.Xml.Schema.XmlSchemaFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaFacet))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) { 
				WriteObject_XmlSchemaPatternFacet((System.Xml.Schema.XmlSchemaPatternFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) { 
				WriteObject_XmlSchemaWhiteSpaceFacet((System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) { 
				WriteObject_XmlSchemaEnumerationFacet((System.Xml.Schema.XmlSchemaEnumerationFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaNumericFacet)) { 
				WriteObject_XmlSchemaNumericFacet((System.Xml.Schema.XmlSchemaNumericFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) { 
				WriteObject_XmlSchemaMaxLengthFacet((System.Xml.Schema.XmlSchemaMaxLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) { 
				WriteObject_XmlSchemaMinLengthFacet((System.Xml.Schema.XmlSchemaMinLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) { 
				WriteObject_XmlSchemaLengthFacet((System.Xml.Schema.XmlSchemaLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) { 
				WriteObject_XmlSchemaFractionDigitsFacet((System.Xml.Schema.XmlSchemaFractionDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) { 
				WriteObject_XmlSchemaTotalDigitsFacet((System.Xml.Schema.XmlSchemaTotalDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) { 
				WriteObject_XmlSchemaMaxInclusiveFacet((System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) { 
				WriteObject_XmlSchemaMaxExclusiveFacet((System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) { 
				WriteObject_XmlSchemaMinInclusiveFacet((System.Xml.Schema.XmlSchemaMinInclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) { 
				WriteObject_XmlSchemaMinExclusiveFacet((System.Xml.Schema.XmlSchemaMinExclusiveFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o262 = ob.@UnhandledAttributes;
			if (o262 != null) {
				foreach (XmlAttribute o263 in o262)
					if (o263.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o263, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaPatternFacet (System.Xml.Schema.XmlSchemaPatternFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaPatternFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o264 = ob.@UnhandledAttributes;
			if (o264 != null) {
				foreach (XmlAttribute o265 in o264)
					if (o265.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o265, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaWhiteSpaceFacet (System.Xml.Schema.XmlSchemaWhiteSpaceFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaWhiteSpaceFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o266 = ob.@UnhandledAttributes;
			if (o266 != null) {
				foreach (XmlAttribute o267 in o266)
					if (o267.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o267, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaEnumerationFacet (System.Xml.Schema.XmlSchemaEnumerationFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaEnumerationFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o268 = ob.@UnhandledAttributes;
			if (o268 != null) {
				foreach (XmlAttribute o269 in o268)
					if (o269.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o269, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaNumericFacet (System.Xml.Schema.XmlSchemaNumericFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaNumericFacet))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) { 
				WriteObject_XmlSchemaMaxLengthFacet((System.Xml.Schema.XmlSchemaMaxLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) { 
				WriteObject_XmlSchemaMinLengthFacet((System.Xml.Schema.XmlSchemaMinLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) { 
				WriteObject_XmlSchemaLengthFacet((System.Xml.Schema.XmlSchemaLengthFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) { 
				WriteObject_XmlSchemaFractionDigitsFacet((System.Xml.Schema.XmlSchemaFractionDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) { 
				WriteObject_XmlSchemaTotalDigitsFacet((System.Xml.Schema.XmlSchemaTotalDigitsFacet)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaNumericFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o270 = ob.@UnhandledAttributes;
			if (o270 != null) {
				foreach (XmlAttribute o271 in o270)
					if (o271.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o271, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxLengthFacet (System.Xml.Schema.XmlSchemaMaxLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o272 = ob.@UnhandledAttributes;
			if (o272 != null) {
				foreach (XmlAttribute o273 in o272)
					if (o273.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o273, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinLengthFacet (System.Xml.Schema.XmlSchemaMinLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o274 = ob.@UnhandledAttributes;
			if (o274 != null) {
				foreach (XmlAttribute o275 in o274)
					if (o275.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o275, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaLengthFacet (System.Xml.Schema.XmlSchemaLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o276 = ob.@UnhandledAttributes;
			if (o276 != null) {
				foreach (XmlAttribute o277 in o276)
					if (o277.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o277, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaFractionDigitsFacet (System.Xml.Schema.XmlSchemaFractionDigitsFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaFractionDigitsFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o278 = ob.@UnhandledAttributes;
			if (o278 != null) {
				foreach (XmlAttribute o279 in o278)
					if (o279.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o279, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaTotalDigitsFacet (System.Xml.Schema.XmlSchemaTotalDigitsFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaTotalDigitsFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o280 = ob.@UnhandledAttributes;
			if (o280 != null) {
				foreach (XmlAttribute o281 in o280)
					if (o281.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o281, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxInclusiveFacet (System.Xml.Schema.XmlSchemaMaxInclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxInclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o282 = ob.@UnhandledAttributes;
			if (o282 != null) {
				foreach (XmlAttribute o283 in o282)
					if (o283.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o283, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxExclusiveFacet (System.Xml.Schema.XmlSchemaMaxExclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxExclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o284 = ob.@UnhandledAttributes;
			if (o284 != null) {
				foreach (XmlAttribute o285 in o284)
					if (o285.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o285, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinInclusiveFacet (System.Xml.Schema.XmlSchemaMinInclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinInclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o286 = ob.@UnhandledAttributes;
			if (o286 != null) {
				foreach (XmlAttribute o287 in o286)
					if (o287.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o287, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinExclusiveFacet (System.Xml.Schema.XmlSchemaMinExclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinExclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o288 = ob.@UnhandledAttributes;
			if (o288 != null) {
				foreach (XmlAttribute o289 in o288)
					if (o289.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o289, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeRestriction (System.Xml.Schema.XmlSchemaSimpleTypeRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o290 = ob.@UnhandledAttributes;
			if (o290 != null) {
				foreach (XmlAttribute o291 in o290)
					if (o291.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o291, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@BaseType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Facets != null) {
				for (int n292 = 0; n292 < ob.@Facets.Count; n292++) {
					if (((object)ob.@Facets[n292]) == null) { }
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) {
						WriteObject_XmlSchemaPatternFacet (((System.Xml.Schema.XmlSchemaPatternFacet) ob.@Facets[n292]), "pattern", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) {
						WriteObject_XmlSchemaWhiteSpaceFacet (((System.Xml.Schema.XmlSchemaWhiteSpaceFacet) ob.@Facets[n292]), "whiteSpace", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) {
						WriteObject_XmlSchemaEnumerationFacet (((System.Xml.Schema.XmlSchemaEnumerationFacet) ob.@Facets[n292]), "enumeration", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) {
						WriteObject_XmlSchemaMaxLengthFacet (((System.Xml.Schema.XmlSchemaMaxLengthFacet) ob.@Facets[n292]), "maxLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) {
						WriteObject_XmlSchemaMinLengthFacet (((System.Xml.Schema.XmlSchemaMinLengthFacet) ob.@Facets[n292]), "minLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) {
						WriteObject_XmlSchemaLengthFacet (((System.Xml.Schema.XmlSchemaLengthFacet) ob.@Facets[n292]), "length", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) {
						WriteObject_XmlSchemaFractionDigitsFacet (((System.Xml.Schema.XmlSchemaFractionDigitsFacet) ob.@Facets[n292]), "fractionDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) {
						WriteObject_XmlSchemaTotalDigitsFacet (((System.Xml.Schema.XmlSchemaTotalDigitsFacet) ob.@Facets[n292]), "totalDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) {
						WriteObject_XmlSchemaMaxInclusiveFacet (((System.Xml.Schema.XmlSchemaMaxInclusiveFacet) ob.@Facets[n292]), "maxInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) {
						WriteObject_XmlSchemaMaxExclusiveFacet (((System.Xml.Schema.XmlSchemaMaxExclusiveFacet) ob.@Facets[n292]), "maxExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) {
						WriteObject_XmlSchemaMinInclusiveFacet (((System.Xml.Schema.XmlSchemaMinInclusiveFacet) ob.@Facets[n292]), "minInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n292].GetType() == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) {
						WriteObject_XmlSchemaMinExclusiveFacet (((System.Xml.Schema.XmlSchemaMinExclusiveFacet) ob.@Facets[n292]), "minExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Facets[n292]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleType (System.Xml.Schema.XmlSchemaSimpleType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleType", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o293 = ob.@UnhandledAttributes;
			if (o293 != null) {
				foreach (XmlAttribute o294 in o293)
					if (o294.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o294, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeUnion) {
				WriteObject_XmlSchemaSimpleTypeUnion (((System.Xml.Schema.XmlSchemaSimpleTypeUnion) ob.@Content), "union", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeList) {
				WriteObject_XmlSchemaSimpleTypeList (((System.Xml.Schema.XmlSchemaSimpleTypeList) ob.@Content), "list", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeRestriction) {
				WriteObject_XmlSchemaSimpleTypeRestriction (((System.Xml.Schema.XmlSchemaSimpleTypeRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaUse (System.Xml.Schema.XmlSchemaUse ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaUse))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaUse", "");

			Writer.WriteString (GetEnumValue_XmlSchemaUse (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_XmlSchemaUse (System.Xml.Schema.XmlSchemaUse val)
		{
			switch (val) {
				case System.Xml.Schema.XmlSchemaUse.Optional: return "optional";
				case System.Xml.Schema.XmlSchemaUse.Prohibited: return "prohibited";
				case System.Xml.Schema.XmlSchemaUse.Required: return "required";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}

		void WriteObject_XmlSchemaAttribute (System.Xml.Schema.XmlSchemaAttribute ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttribute))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttribute", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o295 = ob.@UnhandledAttributes;
			if (o295 != null) {
				foreach (XmlAttribute o296 in o295)
					if (o296.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o296, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			if (ob.@DefaultValue != null) {
				WriteAttribute ("default", "", ob.@DefaultValue);
			}
			if (ob.@FixedValue != null) {
				WriteAttribute ("fixed", "", ob.@FixedValue);
			}
			if (ob.@Form != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("form", "", GetEnumValue_XmlSchemaForm (ob.@Form));
			}
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@SchemaTypeName));
			if (ob.@Use != ((System.Xml.Schema.XmlSchemaUse) System.Xml.Schema.XmlSchemaUse.None)) {
				WriteAttribute ("use", "", GetEnumValue_XmlSchemaUse (ob.@Use));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@SchemaType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaContentProcessing (System.Xml.Schema.XmlSchemaContentProcessing ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaContentProcessing))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaContentProcessing", "");

			Writer.WriteString (GetEnumValue_XmlSchemaContentProcessing (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_XmlSchemaContentProcessing (System.Xml.Schema.XmlSchemaContentProcessing val)
		{
			switch (val) {
				case System.Xml.Schema.XmlSchemaContentProcessing.Skip: return "skip";
				case System.Xml.Schema.XmlSchemaContentProcessing.Lax: return "lax";
				case System.Xml.Schema.XmlSchemaContentProcessing.Strict: return "strict";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}

		void WriteObject_XmlSchemaAnyAttribute (System.Xml.Schema.XmlSchemaAnyAttribute ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAnyAttribute))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAnyAttribute", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o297 = ob.@UnhandledAttributes;
			if (o297 != null) {
				foreach (XmlAttribute o298 in o297)
					if (o298.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o298, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("namespace", "", ob.@Namespace);
			if (ob.@ProcessContents != ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None)) {
				WriteAttribute ("processContents", "", GetEnumValue_XmlSchemaContentProcessing (ob.@ProcessContents));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAttributeGroup (System.Xml.Schema.XmlSchemaAttributeGroup ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroup))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttributeGroup", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o299 = ob.@UnhandledAttributes;
			if (o299 != null) {
				foreach (XmlAttribute o300 in o299)
					if (o300.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o300, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Attributes != null) {
				for (int n301 = 0; n301 < ob.@Attributes.Count; n301++) {
					if (((object)ob.@Attributes[n301]) == null) { }
					else if (ob.@Attributes[n301].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n301]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n301].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n301]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n301]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaParticle (System.Xml.Schema.XmlSchemaParticle ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaParticle))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaAny)) { 
				WriteObject_XmlSchemaAny((System.Xml.Schema.XmlSchemaAny)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef)) { 
				WriteObject_XmlSchemaGroupRef((System.Xml.Schema.XmlSchemaGroupRef)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaGroupBase)) { 
				WriteObject_XmlSchemaGroupBase((System.Xml.Schema.XmlSchemaGroupBase)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaAll)) { 
				WriteObject_XmlSchemaAll((System.Xml.Schema.XmlSchemaAll)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaElement)) { 
				WriteObject_XmlSchemaElement((System.Xml.Schema.XmlSchemaElement)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaChoice)) { 
				WriteObject_XmlSchemaChoice((System.Xml.Schema.XmlSchemaChoice)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSequence)) { 
				WriteObject_XmlSchemaSequence((System.Xml.Schema.XmlSchemaSequence)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaParticle", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o302 = ob.@UnhandledAttributes;
			if (o302 != null) {
				foreach (XmlAttribute o303 in o302)
					if (o303.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o303, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAny (System.Xml.Schema.XmlSchemaAny ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAny))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAny", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o304 = ob.@UnhandledAttributes;
			if (o304 != null) {
				foreach (XmlAttribute o305 in o304)
					if (o305.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o305, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			WriteAttribute ("namespace", "", ob.@Namespace);
			if (ob.@ProcessContents != ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None)) {
				WriteAttribute ("processContents", "", GetEnumValue_XmlSchemaContentProcessing (ob.@ProcessContents));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaGroupRef (System.Xml.Schema.XmlSchemaGroupRef ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaGroupRef", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o306 = ob.@UnhandledAttributes;
			if (o306 != null) {
				foreach (XmlAttribute o307 in o306)
					if (o307.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o307, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaGroupBase (System.Xml.Schema.XmlSchemaGroupBase ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaGroupBase))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaAll)) { 
				WriteObject_XmlSchemaAll((System.Xml.Schema.XmlSchemaAll)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaChoice)) { 
				WriteObject_XmlSchemaChoice((System.Xml.Schema.XmlSchemaChoice)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSequence)) { 
				WriteObject_XmlSchemaSequence((System.Xml.Schema.XmlSchemaSequence)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaGroupBase", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o308 = ob.@UnhandledAttributes;
			if (o308 != null) {
				foreach (XmlAttribute o309 in o308)
					if (o309.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o309, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAll (System.Xml.Schema.XmlSchemaAll ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAll))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAll", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o310 = ob.@UnhandledAttributes;
			if (o310 != null) {
				foreach (XmlAttribute o311 in o310)
					if (o311.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o311, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n312 = 0; n312 < ob.@Items.Count; n312++) {
					WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n312]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaContent (System.Xml.Schema.XmlSchemaContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaContent))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension)) { 
				WriteObject_XmlSchemaComplexContentExtension((System.Xml.Schema.XmlSchemaComplexContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction)) { 
				WriteObject_XmlSchemaComplexContentRestriction((System.Xml.Schema.XmlSchemaComplexContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension)) { 
				WriteObject_XmlSchemaSimpleContentExtension((System.Xml.Schema.XmlSchemaSimpleContentExtension)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction)) { 
				WriteObject_XmlSchemaSimpleContentRestriction((System.Xml.Schema.XmlSchemaSimpleContentRestriction)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o313 = ob.@UnhandledAttributes;
			if (o313 != null) {
				foreach (XmlAttribute o314 in o313)
					if (o314.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o314, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContentExtension (System.Xml.Schema.XmlSchemaComplexContentExtension ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContentExtension", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o315 = ob.@UnhandledAttributes;
			if (o315 != null) {
				foreach (XmlAttribute o316 in o315)
					if (o316.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o316, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n317 = 0; n317 < ob.@Attributes.Count; n317++) {
					if (((object)ob.@Attributes[n317]) == null) { }
					else if (ob.@Attributes[n317].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n317]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n317].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n317]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n317]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContentRestriction (System.Xml.Schema.XmlSchemaComplexContentRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContentRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o318 = ob.@UnhandledAttributes;
			if (o318 != null) {
				foreach (XmlAttribute o319 in o318)
					if (o319.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o319, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n320 = 0; n320 < ob.@Attributes.Count; n320++) {
					if (((object)ob.@Attributes[n320]) == null) { }
					else if (ob.@Attributes[n320].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n320]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n320].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n320]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n320]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaContentModel (System.Xml.Schema.XmlSchemaContentModel ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaContentModel))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent)) { 
				WriteObject_XmlSchemaComplexContent((System.Xml.Schema.XmlSchemaComplexContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent)) { 
				WriteObject_XmlSchemaSimpleContent((System.Xml.Schema.XmlSchemaSimpleContent)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaContentModel", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o321 = ob.@UnhandledAttributes;
			if (o321 != null) {
				foreach (XmlAttribute o322 in o321)
					if (o322.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o322, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContent (System.Xml.Schema.XmlSchemaComplexContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o323 = ob.@UnhandledAttributes;
			if (o323 != null) {
				foreach (XmlAttribute o324 in o323)
					if (o324.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o324, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("mixed", "", (ob.@IsMixed?"true":"false"));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaComplexContentExtension) {
				WriteObject_XmlSchemaComplexContentExtension (((System.Xml.Schema.XmlSchemaComplexContentExtension) ob.@Content), "extension", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaComplexContentRestriction) {
				WriteObject_XmlSchemaComplexContentRestriction (((System.Xml.Schema.XmlSchemaComplexContentRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContentExtension (System.Xml.Schema.XmlSchemaSimpleContentExtension ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContentExtension", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o325 = ob.@UnhandledAttributes;
			if (o325 != null) {
				foreach (XmlAttribute o326 in o325)
					if (o326.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o326, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Attributes != null) {
				for (int n327 = 0; n327 < ob.@Attributes.Count; n327++) {
					if (((object)ob.@Attributes[n327]) == null) { }
					else if (ob.@Attributes[n327].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n327]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n327].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n327]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n327]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContentRestriction (System.Xml.Schema.XmlSchemaSimpleContentRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContentRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o328 = ob.@UnhandledAttributes;
			if (o328 != null) {
				foreach (XmlAttribute o329 in o328)
					if (o329.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o329, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@BaseType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Facets != null) {
				for (int n330 = 0; n330 < ob.@Facets.Count; n330++) {
					if (((object)ob.@Facets[n330]) == null) { }
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) {
						WriteObject_XmlSchemaPatternFacet (((System.Xml.Schema.XmlSchemaPatternFacet) ob.@Facets[n330]), "pattern", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) {
						WriteObject_XmlSchemaWhiteSpaceFacet (((System.Xml.Schema.XmlSchemaWhiteSpaceFacet) ob.@Facets[n330]), "whiteSpace", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) {
						WriteObject_XmlSchemaEnumerationFacet (((System.Xml.Schema.XmlSchemaEnumerationFacet) ob.@Facets[n330]), "enumeration", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) {
						WriteObject_XmlSchemaMaxLengthFacet (((System.Xml.Schema.XmlSchemaMaxLengthFacet) ob.@Facets[n330]), "maxLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) {
						WriteObject_XmlSchemaMinLengthFacet (((System.Xml.Schema.XmlSchemaMinLengthFacet) ob.@Facets[n330]), "minLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) {
						WriteObject_XmlSchemaLengthFacet (((System.Xml.Schema.XmlSchemaLengthFacet) ob.@Facets[n330]), "length", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) {
						WriteObject_XmlSchemaFractionDigitsFacet (((System.Xml.Schema.XmlSchemaFractionDigitsFacet) ob.@Facets[n330]), "fractionDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) {
						WriteObject_XmlSchemaTotalDigitsFacet (((System.Xml.Schema.XmlSchemaTotalDigitsFacet) ob.@Facets[n330]), "totalDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) {
						WriteObject_XmlSchemaMaxInclusiveFacet (((System.Xml.Schema.XmlSchemaMaxInclusiveFacet) ob.@Facets[n330]), "maxInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) {
						WriteObject_XmlSchemaMaxExclusiveFacet (((System.Xml.Schema.XmlSchemaMaxExclusiveFacet) ob.@Facets[n330]), "maxExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) {
						WriteObject_XmlSchemaMinInclusiveFacet (((System.Xml.Schema.XmlSchemaMinInclusiveFacet) ob.@Facets[n330]), "minInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n330].GetType() == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) {
						WriteObject_XmlSchemaMinExclusiveFacet (((System.Xml.Schema.XmlSchemaMinExclusiveFacet) ob.@Facets[n330]), "minExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Facets[n330]);
				}
			}
			if (ob.@Attributes != null) {
				for (int n331 = 0; n331 < ob.@Attributes.Count; n331++) {
					if (((object)ob.@Attributes[n331]) == null) { }
					else if (ob.@Attributes[n331].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n331]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n331].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n331]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n331]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContent (System.Xml.Schema.XmlSchemaSimpleContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o332 = ob.@UnhandledAttributes;
			if (o332 != null) {
				foreach (XmlAttribute o333 in o332)
					if (o333.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o333, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleContentExtension) {
				WriteObject_XmlSchemaSimpleContentExtension (((System.Xml.Schema.XmlSchemaSimpleContentExtension) ob.@Content), "extension", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleContentRestriction) {
				WriteObject_XmlSchemaSimpleContentRestriction (((System.Xml.Schema.XmlSchemaSimpleContentRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexType (System.Xml.Schema.XmlSchemaComplexType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexType))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexType", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o334 = ob.@UnhandledAttributes;
			if (o334 != null) {
				foreach (XmlAttribute o335 in o334)
					if (o335.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o335, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}
			if (ob.@IsAbstract != false) {
				WriteAttribute ("abstract", "", (ob.@IsAbstract?"true":"false"));
			}
			if (ob.@Block != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("block", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Block));
			}
			if (ob.@IsMixed != false) {
				WriteAttribute ("mixed", "", (ob.@IsMixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@ContentModel is System.Xml.Schema.XmlSchemaComplexContent) {
				WriteObject_XmlSchemaComplexContent (((System.Xml.Schema.XmlSchemaComplexContent) ob.@ContentModel), "complexContent", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@ContentModel is System.Xml.Schema.XmlSchemaSimpleContent) {
				WriteObject_XmlSchemaSimpleContent (((System.Xml.Schema.XmlSchemaSimpleContent) ob.@ContentModel), "simpleContent", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n336 = 0; n336 < ob.@Attributes.Count; n336++) {
					if (((object)ob.@Attributes[n336]) == null) { }
					else if (ob.@Attributes[n336].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n336]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n336].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n336]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n336]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaXPath (System.Xml.Schema.XmlSchemaXPath ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaXPath))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaXPath", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o337 = ob.@UnhandledAttributes;
			if (o337 != null) {
				foreach (XmlAttribute o338 in o337)
					if (o338.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o338, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			if (ob.@XPath != "") {
				WriteAttribute ("xpath", "", ob.@XPath);
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaIdentityConstraint (System.Xml.Schema.XmlSchemaIdentityConstraint ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaIdentityConstraint))
			{ }
			else if (type == typeof(System.Xml.Schema.XmlSchemaKeyref)) { 
				WriteObject_XmlSchemaKeyref((System.Xml.Schema.XmlSchemaKeyref)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaKey)) { 
				WriteObject_XmlSchemaKey((System.Xml.Schema.XmlSchemaKey)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Xml.Schema.XmlSchemaUnique)) { 
				WriteObject_XmlSchemaUnique((System.Xml.Schema.XmlSchemaUnique)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaIdentityConstraint", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o339 = ob.@UnhandledAttributes;
			if (o339 != null) {
				foreach (XmlAttribute o340 in o339)
					if (o340.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o340, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n341 = 0; n341 < ob.@Fields.Count; n341++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n341]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaKeyref (System.Xml.Schema.XmlSchemaKeyref ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaKeyref))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaKeyref", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o342 = ob.@UnhandledAttributes;
			if (o342 != null) {
				foreach (XmlAttribute o343 in o342)
					if (o343.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o343, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("refer", "", FromXmlQualifiedName (ob.@Refer));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n344 = 0; n344 < ob.@Fields.Count; n344++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n344]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaKey (System.Xml.Schema.XmlSchemaKey ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaKey))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaKey", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o345 = ob.@UnhandledAttributes;
			if (o345 != null) {
				foreach (XmlAttribute o346 in o345)
					if (o346.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o346, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n347 = 0; n347 < ob.@Fields.Count; n347++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n347]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaUnique (System.Xml.Schema.XmlSchemaUnique ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaUnique))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaUnique", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o348 = ob.@UnhandledAttributes;
			if (o348 != null) {
				foreach (XmlAttribute o349 in o348)
					if (o349.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o349, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n350 = 0; n350 < ob.@Fields.Count; n350++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n350]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaElement (System.Xml.Schema.XmlSchemaElement ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaElement))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaElement", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o351 = ob.@UnhandledAttributes;
			if (o351 != null) {
				foreach (XmlAttribute o352 in o351)
					if (o352.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o352, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			if (ob.@IsAbstract != false) {
				WriteAttribute ("abstract", "", (ob.@IsAbstract?"true":"false"));
			}
			if (ob.@Block != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("block", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Block));
			}
			if (ob.@DefaultValue != null) {
				WriteAttribute ("default", "", ob.@DefaultValue);
			}
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}
			if (ob.@FixedValue != null) {
				WriteAttribute ("fixed", "", ob.@FixedValue);
			}
			if (ob.@Form != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("form", "", GetEnumValue_XmlSchemaForm (ob.@Form));
			}
			if (ob.@Name != "") {
				WriteAttribute ("name", "", ob.@Name);
			}
			if (ob.@IsNillable != false) {
				WriteAttribute ("nillable", "", (ob.@IsNillable?"true":"false"));
			}
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));
			WriteAttribute ("substitutionGroup", "", FromXmlQualifiedName (ob.@SubstitutionGroup));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@SchemaTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@SchemaType is System.Xml.Schema.XmlSchemaComplexType) {
				WriteObject_XmlSchemaComplexType (((System.Xml.Schema.XmlSchemaComplexType) ob.@SchemaType), "complexType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@SchemaType is System.Xml.Schema.XmlSchemaSimpleType) {
				WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@SchemaType), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Constraints != null) {
				for (int n353 = 0; n353 < ob.@Constraints.Count; n353++) {
					if (((object)ob.@Constraints[n353]) == null) { }
					else if (ob.@Constraints[n353].GetType() == typeof(System.Xml.Schema.XmlSchemaKeyref)) {
						WriteObject_XmlSchemaKeyref (((System.Xml.Schema.XmlSchemaKeyref) ob.@Constraints[n353]), "keyref", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Constraints[n353].GetType() == typeof(System.Xml.Schema.XmlSchemaKey)) {
						WriteObject_XmlSchemaKey (((System.Xml.Schema.XmlSchemaKey) ob.@Constraints[n353]), "key", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Constraints[n353].GetType() == typeof(System.Xml.Schema.XmlSchemaUnique)) {
						WriteObject_XmlSchemaUnique (((System.Xml.Schema.XmlSchemaUnique) ob.@Constraints[n353]), "unique", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Constraints[n353]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaChoice (System.Xml.Schema.XmlSchemaChoice ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaChoice))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaChoice", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o354 = ob.@UnhandledAttributes;
			if (o354 != null) {
				foreach (XmlAttribute o355 in o354)
					if (o355.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o355, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n356 = 0; n356 < ob.@Items.Count; n356++) {
					if (((object)ob.@Items[n356]) == null) { }
					else if (ob.@Items[n356].GetType() == typeof(System.Xml.Schema.XmlSchemaAny)) {
						WriteObject_XmlSchemaAny (((System.Xml.Schema.XmlSchemaAny) ob.@Items[n356]), "any", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n356].GetType() == typeof(System.Xml.Schema.XmlSchemaSequence)) {
						WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Items[n356]), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n356].GetType() == typeof(System.Xml.Schema.XmlSchemaChoice)) {
						WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Items[n356]), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n356].GetType() == typeof(System.Xml.Schema.XmlSchemaGroupRef)) {
						WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Items[n356]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n356].GetType() == typeof(System.Xml.Schema.XmlSchemaElement)) {
						WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n356]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n356]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSequence (System.Xml.Schema.XmlSchemaSequence ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSequence))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSequence", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o357 = ob.@UnhandledAttributes;
			if (o357 != null) {
				foreach (XmlAttribute o358 in o357)
					if (o358.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o358, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n359 = 0; n359 < ob.@Items.Count; n359++) {
					if (((object)ob.@Items[n359]) == null) { }
					else if (ob.@Items[n359].GetType() == typeof(System.Xml.Schema.XmlSchemaAny)) {
						WriteObject_XmlSchemaAny (((System.Xml.Schema.XmlSchemaAny) ob.@Items[n359]), "any", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n359].GetType() == typeof(System.Xml.Schema.XmlSchemaSequence)) {
						WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Items[n359]), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n359].GetType() == typeof(System.Xml.Schema.XmlSchemaChoice)) {
						WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Items[n359]), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n359].GetType() == typeof(System.Xml.Schema.XmlSchemaGroupRef)) {
						WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Items[n359]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n359].GetType() == typeof(System.Xml.Schema.XmlSchemaElement)) {
						WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n359]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n359]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaGroup (System.Xml.Schema.XmlSchemaGroup ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaGroup))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaGroup", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o360 = ob.@UnhandledAttributes;
			if (o360 != null) {
				foreach (XmlAttribute o361 in o360)
					if (o361.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o361, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaRedefine (System.Xml.Schema.XmlSchemaRedefine ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaRedefine))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaRedefine", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o362 = ob.@UnhandledAttributes;
			if (o362 != null) {
				foreach (XmlAttribute o363 in o362)
					if (o363.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o363, ob);
			}

			WriteAttribute ("schemaLocation", "", ((ob.@SchemaLocation != null) ? (ob.@SchemaLocation).ToString() : null));
			WriteAttribute ("id", "", ob.@Id);

			if (ob.@Items != null) {
				for (int n364 = 0; n364 < ob.@Items.Count; n364++) {
					if (((object)ob.@Items[n364]) == null) { }
					else if (ob.@Items[n364].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) {
						WriteObject_XmlSchemaAttributeGroup (((System.Xml.Schema.XmlSchemaAttributeGroup) ob.@Items[n364]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n364].GetType() == typeof(System.Xml.Schema.XmlSchemaGroup)) {
						WriteObject_XmlSchemaGroup (((System.Xml.Schema.XmlSchemaGroup) ob.@Items[n364]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n364].GetType() == typeof(System.Xml.Schema.XmlSchemaComplexType)) {
						WriteObject_XmlSchemaComplexType (((System.Xml.Schema.XmlSchemaComplexType) ob.@Items[n364]), "complexType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n364].GetType() == typeof(System.Xml.Schema.XmlSchemaSimpleType)) {
						WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@Items[n364]), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n364].GetType() == typeof(System.Xml.Schema.XmlSchemaAnnotation)) {
						WriteObject_XmlSchemaAnnotation (((System.Xml.Schema.XmlSchemaAnnotation) ob.@Items[n364]), "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n364]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaImport (System.Xml.Schema.XmlSchemaImport ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaImport))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaImport", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o365 = ob.@UnhandledAttributes;
			if (o365 != null) {
				foreach (XmlAttribute o366 in o365)
					if (o366.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o366, ob);
			}

			WriteAttribute ("schemaLocation", "", ((ob.@SchemaLocation != null) ? (ob.@SchemaLocation).ToString() : null));
			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("namespace", "", ((ob.@Namespace != null) ? (ob.@Namespace).ToString() : null));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaInclude (System.Xml.Schema.XmlSchemaInclude ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaInclude))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaInclude", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o367 = ob.@UnhandledAttributes;
			if (o367 != null) {
				foreach (XmlAttribute o368 in o367)
					if (o368.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o368, ob);
			}

			WriteAttribute ("schemaLocation", "", ((ob.@SchemaLocation != null) ? (ob.@SchemaLocation).ToString() : null));
			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaNotation (System.Xml.Schema.XmlSchemaNotation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaNotation))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaNotation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o369 = ob.@UnhandledAttributes;
			if (o369 != null) {
				foreach (XmlAttribute o370 in o369)
					if (o370.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o370, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("public", "", ob.@Public);
			WriteAttribute ("system", "", ob.@System);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_OperationMessage (System.Web.Services.Description.OperationMessage ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.OperationMessage))
			{ }
			else if (type == typeof(System.Web.Services.Description.OperationFault)) { 
				WriteObject_OperationFault((System.Web.Services.Description.OperationFault)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationInput)) { 
				WriteObject_OperationInput((System.Web.Services.Description.OperationInput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OperationOutput)) { 
				WriteObject_OperationOutput((System.Web.Services.Description.OperationOutput)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("OperationMessage", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));
			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o371 = ob.@DocumentationElement;
				if (o371 is XmlElement) {
				if ((o371.Name == "documentation" && o371.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o371.WriteTo (Writer);
					WriteElementLiteral (o371, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o371.Name, o371.NamespaceURI);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ArrayOfAnyType (System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ArrayOfAnyType", "http://schemas.xmlsoap.org/wsdl/");

			for (int n372 = 0; n372 < ob.Count; n372++) {
				WriteObject_anyType (ob[n372], "anyType", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_MessageBinding (System.Web.Services.Description.MessageBinding ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.MessageBinding))
			{ }
			else if (type == typeof(System.Web.Services.Description.FaultBinding)) { 
				WriteObject_FaultBinding((System.Web.Services.Description.FaultBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.InputBinding)) { 
				WriteObject_InputBinding((System.Web.Services.Description.InputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else if (type == typeof(System.Web.Services.Description.OutputBinding)) { 
				WriteObject_OutputBinding((System.Web.Services.Description.OutputBinding)ob, element, namesp, isNullable, true, writeWrappingElem);
				return;
			}
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("MessageBinding", "http://schemas.xmlsoap.org/wsdl/");

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o373 = ob.@DocumentationElement;
				if (o373 is XmlElement) {
				if ((o373.Name == "documentation" && o373.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o373.WriteTo (Writer);
					WriteElementLiteral (o373, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o373.Name, o373.NamespaceURI);
			}
			if (ob.@Extensions != null) {
				WriteStartElement ("Extensions", "http://schemas.xmlsoap.org/wsdl/", ob.@Extensions);
				for (int n374 = 0; n374 < ob.@Extensions.Count; n374++) {
					WriteObject_anyType (ob.@Extensions[n374], "anyType", "http://schemas.xmlsoap.org/wsdl/", true, false, true);
				}
				WriteEndElement (ob.@Extensions);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}

}

