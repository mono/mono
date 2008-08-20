using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.ServiceModel.Description
{
	internal class MetadataSectionReaderBase : XmlSerializationReader
	{
		public object ReadRoot_MetadataSection ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "MetadataSection" || Reader.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/09/mex")
				throw CreateUnknownNodeException();
			return ReadObject_MetadataSection (true, true);
		}

		public System.ServiceModel.Description.MetadataSection ReadObject_MetadataSection (bool isNullable, bool checkType)
		{
			System.ServiceModel.Description.MetadataSection ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "MetadataSection" || t.Namespace != "http://schemas.xmlsoap.org/ws/2004/09/mex")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.ServiceModel.Description.MetadataSection ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Collections.ObjectModel.Collection<System.Xml.XmlAttribute> anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Dialect" && Reader.NamespaceURI == "") {
					ob.@Dialect = Reader.Value;
				}
				else if (Reader.LocalName == "Identifier" && Reader.NamespaceURI == "") {
					ob.@Identifier = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					if (((object)anyAttributeArray) == null)
						anyAttributeArray = new System.Collections.ObjectModel.Collection<System.Xml.XmlAttribute>();
					anyAttributeArray.Add (((System.Xml.XmlAttribute) attr));
					anyAttributeIndex++;
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

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "schema" && Reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema" && !b0) {
						b0 = true;
						ob.@Metadata = ReadObject_XmlSchema (false, true);
					}
					else if (Reader.LocalName == "Metadata" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex" && !b0) {
						b0 = true;
						ob.@Metadata = ((System.ServiceModel.Description.MetadataSet) ReadSerializable (new System.ServiceModel.Description.MetadataSet ()));
					}
					else if (Reader.LocalName == "Location" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex" && !b0) {
						b0 = true;
						ob.@Metadata = ReadObject_MetadataLocation (false, true);
					}
					else if (Reader.LocalName == "MetadataReference" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex" && !b0) {
						b0 = true;
						ob.@Metadata = ((System.ServiceModel.Description.MetadataReference) ReadSerializable (new System.ServiceModel.Description.MetadataReference ()));
					}
					else if (Reader.LocalName == "definitions" && Reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && !b0) {
						b0 = true;
						ob.@Metadata = ReadObject_ServiceDescription (false, true);
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
			ob = System.Xml.Schema.XmlSchema.Read (Reader, null); Reader.Read ();
			return ob;
		}

		public System.ServiceModel.Description.MetadataLocation ReadObject_MetadataLocation (bool isNullable, bool checkType)
		{
			System.ServiceModel.Description.MetadataLocation ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "MetadataLocation" || t.Namespace != "http://schemas.xmlsoap.org/ws/2004/09/mex")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.ServiceModel.Description.MetadataLocation ();

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

			bool b1=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA)
				{
					ob.@Location = ReadString (ob.@Location);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Description.ServiceDescription ReadObject_ServiceDescription (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.ServiceDescription ob = null;
			ob = (System.Web.Services.Description.ServiceDescription) System.Web.Services.Description.ServiceDescription.Serializer.Deserialize (Reader); 
			return ob;
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

	}

	internal class MetadataSectionWriterBase : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		public void WriteRoot_MetadataSection (object o)
		{
			WriteStartDocument ();
			System.ServiceModel.Description.MetadataSection ob = (System.ServiceModel.Description.MetadataSection) o;
			TopLevelElement ();
			WriteObject_MetadataSection (ob, "MetadataSection", "http://schemas.xmlsoap.org/ws/2004/09/mex", true, false, true);
		}

		void WriteObject_MetadataSection (System.ServiceModel.Description.MetadataSection ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.ServiceModel.Description.MetadataSection))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("MetadataSection", "http://schemas.xmlsoap.org/ws/2004/09/mex");

			ICollection o2 = ob.@Attributes;
			if (o2 != null) {
				foreach (XmlAttribute o3 in o2)
					if (o3.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o3, ob);
			}

			WriteAttribute ("Dialect", "", ob.@Dialect);
			WriteAttribute ("Identifier", "", ob.@Identifier);

			if (ob.@Metadata is System.ServiceModel.Description.MetadataReference) {
				WriteSerializable (((System.ServiceModel.Description.MetadataReference) ob.@Metadata), "MetadataReference", "http://schemas.xmlsoap.org/ws/2004/09/mex", false);
			}
			else if (ob.@Metadata is System.Web.Services.Description.ServiceDescription) {
				WriteObject_ServiceDescription (((System.Web.Services.Description.ServiceDescription) ob.@Metadata), "definitions", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
			}
			else if (ob.@Metadata is System.Xml.Schema.XmlSchema) {
				WriteObject_XmlSchema (((System.Xml.Schema.XmlSchema) ob.@Metadata), "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Metadata is System.ServiceModel.Description.MetadataSet) {
				WriteSerializable (((System.ServiceModel.Description.MetadataSet) ob.@Metadata), "Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex", false);
			}
			else if (ob.@Metadata is System.ServiceModel.Description.MetadataLocation) {
				WriteObject_MetadataLocation (((System.ServiceModel.Description.MetadataLocation) ob.@Metadata), "Location", "http://schemas.xmlsoap.org/ws/2004/09/mex", false, false, true);
			}
			else if (ob.@Metadata is System.Xml.XmlElement) {
				WriteElementLiteral (((System.Xml.XmlElement) ob.@Metadata), "", "http://schemas.xmlsoap.org/ws/2004/09/mex", false, false);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ServiceDescription (System.Web.Services.Description.ServiceDescription ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Web.Services.Description.ServiceDescription.Serializer.Serialize (Writer, ob);
		}

		void WriteObject_XmlSchema (System.Xml.Schema.XmlSchema ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			ob.Write (Writer);
		}

		void WriteObject_MetadataLocation (System.ServiceModel.Description.MetadataLocation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.ServiceModel.Description.MetadataLocation))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("MetadataLocation", "http://schemas.xmlsoap.org/ws/2004/09/mex");

			WriteValue (ob.@Location);
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

			ICollection o12 = ob.@ExtensibleAttributes;
			if (o12 != null) {
				foreach (XmlAttribute o13 in o12)
					if (o13.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o13, ob);
			}

			WriteAttribute ("location", "", ob.@Location);
			WriteAttribute ("namespace", "", ob.@Namespace);

			if (ob.@DocumentationElement != null) {
				XmlNode o14 = ob.@DocumentationElement;
				if (o14 is XmlElement) {
				if ((o14.Name == "documentation" && o14.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o14.WriteTo (Writer);
					WriteElementLiteral (o14, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o14.Name, o14.NamespaceURI);
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

			ICollection o15 = ob.@ExtensibleAttributes;
			if (o15 != null) {
				foreach (XmlAttribute o16 in o15)
					if (o16.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o16, ob);
			}

			if (ob.@DocumentationElement != null) {
				XmlNode o17 = ob.@DocumentationElement;
				if (o17 is XmlElement) {
				if ((o17.Name == "documentation" && o17.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o17.WriteTo (Writer);
					WriteElementLiteral (o17, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o17.Name, o17.NamespaceURI);
			}
			if (ob.@Schemas != null) {
				for (int n18 = 0; n18 < ob.@Schemas.Count; n18++) {
					WriteObject_XmlSchema (ob.@Schemas[n18], "schema", "http://www.w3.org/2001/XMLSchema", false, false, true);
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

			ICollection o19 = ob.@ExtensibleAttributes;
			if (o19 != null) {
				foreach (XmlAttribute o20 in o19)
					if (o20.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o20, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o21 = ob.@DocumentationElement;
				if (o21 is XmlElement) {
				if ((o21.Name == "documentation" && o21.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o21.WriteTo (Writer);
					WriteElementLiteral (o21, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o21.Name, o21.NamespaceURI);
			}
			if (ob.@Parts != null) {
				for (int n22 = 0; n22 < ob.@Parts.Count; n22++) {
					WriteObject_MessagePart (ob.@Parts[n22], "part", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			ICollection o23 = ob.@ExtensibleAttributes;
			if (o23 != null) {
				foreach (XmlAttribute o24 in o23)
					if (o24.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o24, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o25 = ob.@DocumentationElement;
				if (o25 is XmlElement) {
				if ((o25.Name == "documentation" && o25.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o25.WriteTo (Writer);
					WriteElementLiteral (o25, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o25.Name, o25.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n26 = 0; n26 < ob.@Operations.Count; n26++) {
					WriteObject_Operation (ob.@Operations[n26], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			ICollection o27 = ob.@ExtensibleAttributes;
			if (o27 != null) {
				foreach (XmlAttribute o28 in o27)
					if (o28.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o28, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

			if (ob.@DocumentationElement != null) {
				XmlNode o29 = ob.@DocumentationElement;
				if (o29 is XmlElement) {
				if ((o29.Name == "documentation" && o29.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o29.WriteTo (Writer);
					WriteElementLiteral (o29, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o29.Name, o29.NamespaceURI);
			}
			if (ob.@Operations != null) {
				for (int n30 = 0; n30 < ob.@Operations.Count; n30++) {
					WriteObject_OperationBinding (ob.@Operations[n30], "operation", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			ICollection o31 = ob.@ExtensibleAttributes;
			if (o31 != null) {
				foreach (XmlAttribute o32 in o31)
					if (o32.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o32, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o33 = ob.@DocumentationElement;
				if (o33 is XmlElement) {
				if ((o33.Name == "documentation" && o33.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o33.WriteTo (Writer);
					WriteElementLiteral (o33, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o33.Name, o33.NamespaceURI);
			}
			if (ob.@Ports != null) {
				for (int n34 = 0; n34 < ob.@Ports.Count; n34++) {
					WriteObject_Port (ob.@Ports[n34], "port", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
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

			ICollection o35 = ob.@ExtensibleAttributes;
			if (o35 != null) {
				foreach (XmlAttribute o36 in o35)
					if (o36.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o36, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("element", "", FromXmlQualifiedName (ob.@Element));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@Type));

			if (ob.@DocumentationElement != null) {
				XmlNode o37 = ob.@DocumentationElement;
				if (o37 is XmlElement) {
				if ((o37.Name == "documentation" && o37.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o37.WriteTo (Writer);
					WriteElementLiteral (o37, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o37.Name, o37.NamespaceURI);
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

			ICollection o38 = ob.@ExtensibleAttributes;
			if (o38 != null) {
				foreach (XmlAttribute o39 in o38)
					if (o39.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o39, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			if (ob.@ParameterOrderString != "") {
				WriteAttribute ("parameterOrder", "", ob.@ParameterOrderString);
			}

			if (ob.@DocumentationElement != null) {
				XmlNode o40 = ob.@DocumentationElement;
				if (o40 is XmlElement) {
				if ((o40.Name == "documentation" && o40.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o40.WriteTo (Writer);
					WriteElementLiteral (o40, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o40.Name, o40.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n41 = 0; n41 < ob.@Faults.Count; n41++) {
					WriteObject_OperationFault (ob.@Faults[n41], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
				}
			}
			if (ob.@Messages != null) {
				for (int n42 = 0; n42 < ob.@Messages.Count; n42++) {
					if (((object)ob.@Messages[n42]) == null) { }
					else if (ob.@Messages[n42].GetType() == typeof(System.Web.Services.Description.OperationInput)) {
						WriteObject_OperationInput (((System.Web.Services.Description.OperationInput) ob.@Messages[n42]), "input", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else if (ob.@Messages[n42].GetType() == typeof(System.Web.Services.Description.OperationOutput)) {
						WriteObject_OperationOutput (((System.Web.Services.Description.OperationOutput) ob.@Messages[n42]), "output", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Messages[n42]);
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

			ICollection o43 = ob.@ExtensibleAttributes;
			if (o43 != null) {
				foreach (XmlAttribute o44 in o43)
					if (o44.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o44, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o45 = ob.@DocumentationElement;
				if (o45 is XmlElement) {
				if ((o45.Name == "documentation" && o45.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o45.WriteTo (Writer);
					WriteElementLiteral (o45, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o45.Name, o45.NamespaceURI);
			}
			if (ob.@Faults != null) {
				for (int n46 = 0; n46 < ob.@Faults.Count; n46++) {
					WriteObject_FaultBinding (ob.@Faults[n46], "fault", "http://schemas.xmlsoap.org/wsdl/", false, false, true);
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

			ICollection o47 = ob.@ExtensibleAttributes;
			if (o47 != null) {
				foreach (XmlAttribute o48 in o47)
					if (o48.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o48, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("binding", "", FromXmlQualifiedName (ob.@Binding));

			if (ob.@DocumentationElement != null) {
				XmlNode o49 = ob.@DocumentationElement;
				if (o49 is XmlElement) {
				if ((o49.Name == "documentation" && o49.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o49.WriteTo (Writer);
					WriteElementLiteral (o49, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o49.Name, o49.NamespaceURI);
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

			ICollection o50 = ob.@ExtensibleAttributes;
			if (o50 != null) {
				foreach (XmlAttribute o51 in o50)
					if (o51.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o51, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			if (ob.@DocumentationElement != null) {
				XmlNode o52 = ob.@DocumentationElement;
				if (o52 is XmlElement) {
				if ((o52.Name == "documentation" && o52.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o52.WriteTo (Writer);
					WriteElementLiteral (o52, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o52.Name, o52.NamespaceURI);
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

			ICollection o53 = ob.@ExtensibleAttributes;
			if (o53 != null) {
				foreach (XmlAttribute o54 in o53)
					if (o54.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o54, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			if (ob.@DocumentationElement != null) {
				XmlNode o55 = ob.@DocumentationElement;
				if (o55 is XmlElement) {
				if ((o55.Name == "documentation" && o55.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o55.WriteTo (Writer);
					WriteElementLiteral (o55, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o55.Name, o55.NamespaceURI);
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

			ICollection o56 = ob.@ExtensibleAttributes;
			if (o56 != null) {
				foreach (XmlAttribute o57 in o56)
					if (o57.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o57, ob);
			}

			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("message", "", FromXmlQualifiedName (ob.@Message));

			if (ob.@DocumentationElement != null) {
				XmlNode o58 = ob.@DocumentationElement;
				if (o58 is XmlElement) {
				if ((o58.Name == "documentation" && o58.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o58.WriteTo (Writer);
					WriteElementLiteral (o58, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o58.Name, o58.NamespaceURI);
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

			ICollection o59 = ob.@ExtensibleAttributes;
			if (o59 != null) {
				foreach (XmlAttribute o60 in o59)
					if (o60.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o60, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o61 = ob.@DocumentationElement;
				if (o61 is XmlElement) {
				if ((o61.Name == "documentation" && o61.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o61.WriteTo (Writer);
					WriteElementLiteral (o61, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o61.Name, o61.NamespaceURI);
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

			ICollection o62 = ob.@ExtensibleAttributes;
			if (o62 != null) {
				foreach (XmlAttribute o63 in o62)
					if (o63.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o63, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o64 = ob.@DocumentationElement;
				if (o64 is XmlElement) {
				if ((o64.Name == "documentation" && o64.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o64.WriteTo (Writer);
					WriteElementLiteral (o64, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o64.Name, o64.NamespaceURI);
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

			ICollection o65 = ob.@ExtensibleAttributes;
			if (o65 != null) {
				foreach (XmlAttribute o66 in o65)
					if (o66.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o66, ob);
			}

			WriteAttribute ("name", "", ob.@Name);

			if (ob.@DocumentationElement != null) {
				XmlNode o67 = ob.@DocumentationElement;
				if (o67 is XmlElement) {
				if ((o67.Name == "documentation" && o67.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/")) {
					}
					else o67.WriteTo (Writer);
					WriteElementLiteral (o67, "", "", false, true);
				}
				else
					throw CreateUnknownAnyElementException (o67.Name, o67.NamespaceURI);
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
			return new MetadataSectionReaderBase ();
		}

		protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {
			return new MetadataSectionWriterBase ();
		}

		public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {
			return true;
		}
	}

	internal sealed class MetadataSectionSerializer : BaseXmlSerializer
	{
		protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {
			((MetadataSectionWriterBase)writer).WriteRoot_MetadataSection(obj);
		}

		protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {
			return ((MetadataSectionReaderBase)reader).ReadRoot_MetadataSection();
		}
	}

	internal class XmlSerializerContract : System.Xml.Serialization.XmlSerializerImplementation
	{
		System.Collections.Hashtable readMethods = null;
		System.Collections.Hashtable writeMethods = null;
		System.Collections.Hashtable typedSerializers = null;

		public override System.Xml.Serialization.XmlSerializationReader Reader {
			get {
				return new MetadataSectionReaderBase();
			}
		}

		public override System.Xml.Serialization.XmlSerializationWriter Writer {
			get {
				return new MetadataSectionWriterBase();
			}
		}

		public override System.Collections.Hashtable ReadMethods {
			get {
				lock (this) {
					if (readMethods == null) {
						readMethods = new System.Collections.Hashtable ();
						readMethods.Add (@"", @"ReadRoot_MetadataSection");
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
						writeMethods.Add (@"", @"WriteRoot_MetadataSection");
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
						typedSerializers.Add (@"", new MetadataSectionSerializer());
					}
					return typedSerializers;
				}
			}
		}
		public override bool CanSerialize (System.Type type) {
			if (type == typeof(System.ServiceModel.Description.MetadataSection)) return true;
			return false;
		}
	}

}

