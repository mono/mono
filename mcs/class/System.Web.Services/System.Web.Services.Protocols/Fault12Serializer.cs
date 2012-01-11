#if NET_2_0
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Protocols
{
	internal class Soap12FaultReader : XmlSerializationReader
	{
		public object ReadRoot_Soap12Fault ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "Fault" || Reader.NamespaceURI != "http://www.w3.org/2003/05/soap-envelope")
				throw CreateUnknownNodeException();
			return ReadObject_Fault (true, true);
		}

		public System.Web.Services.Protocols.Soap12Fault ReadObject_Fault (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Soap12Fault ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Fault" || t.Namespace != "http://www.w3.org/2003/05/soap-envelope")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Protocols.Soap12Fault ();

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

			bool b0=false, b1=false, b2=false, b3=false, b4=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Role" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b3) {
						b3 = true;
						ob.@Role = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "Detail" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b4) {
						b4 = true;
						ob.@Detail = ReadObject_Detail (false, true);
					}
					else if (Reader.LocalName == "Code" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b0) {
						b0 = true;
						ob.@Code = ReadObject_Code (false, true);
					}
					else if (Reader.LocalName == "Node" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b2) {
						b2 = true;
						ob.@Node = Reader.ReadElementString ();
					}
					else if (Reader.LocalName == "Reason" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b1) {
						b1 = true;
						ob.@Reason = ReadObject_Reason (false, true);
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

		public System.Web.Services.Protocols.Soap12FaultDetail ReadObject_Detail (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Soap12FaultDetail ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Detail" || t.Namespace != "http://www.w3.org/2003/05/soap-envelope")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Protocols.Soap12FaultDetail ();

			Reader.MoveToElement();

			int anyAttributeIndex = 0;
			System.Xml.XmlAttribute[] anyAttributeArray = null;
			while (Reader.MoveToNextAttribute())
			{
				if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
					anyAttributeArray = (System.Xml.XmlAttribute[]) EnsureArrayIndex (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute));
					anyAttributeArray[anyAttributeIndex] = ((System.Xml.XmlAttribute) attr);
					anyAttributeIndex++;
				}
			}

			anyAttributeArray = (System.Xml.XmlAttribute[]) ShrinkArray (anyAttributeArray, anyAttributeIndex, typeof(System.Xml.XmlAttribute), true);
			ob.@Attributes = anyAttributeArray;

			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			System.Xml.XmlElement[] o8;
			o8 = null;
			int n7=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					o8 = (System.Xml.XmlElement[]) EnsureArrayIndex (o8, n7, typeof(System.Xml.XmlElement));
					o8[n7] = ((System.Xml.XmlElement) ReadXmlNode (false));
					n7++;
				}
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA)
				{
					ob.@Text = ReadString (ob.@Text);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o8 = (System.Xml.XmlElement[]) ShrinkArray (o8, n7, typeof(System.Xml.XmlElement), true);
			ob.@Children = o8;

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Protocols.Soap12FaultCode ReadObject_Code (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Soap12FaultCode ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Code" || t.Namespace != "http://www.w3.org/2003/05/soap-envelope")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Protocols.Soap12FaultCode ();

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

			bool b9=false, b10=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Value" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b9) {
						b9 = true;
						ob.@Value = ReadElementQualifiedName ();
					}
					else if (Reader.LocalName == "Subcode" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b10) {
						b10 = true;
						ob.@Subcode = ReadObject_Code (false, true);
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

		public System.Web.Services.Protocols.Soap12FaultReason ReadObject_Reason (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Soap12FaultReason ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Reason" || t.Namespace != "http://www.w3.org/2003/05/soap-envelope")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Protocols.Soap12FaultReason ();

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

			bool b11=false;

			System.Web.Services.Protocols.Soap12FaultReasonText[] o13;
			o13 = null;
			int n12=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Text" && Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope" && !b11) {
						o13 = (System.Web.Services.Protocols.Soap12FaultReasonText[]) EnsureArrayIndex (o13, n12, typeof(System.Web.Services.Protocols.Soap12FaultReasonText));
						o13[n12] = ReadObject_Text (false, true);
						n12++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o13 = (System.Web.Services.Protocols.Soap12FaultReasonText[]) ShrinkArray (o13, n12, typeof(System.Web.Services.Protocols.Soap12FaultReasonText), true);
			ob.@Texts = o13;

			ReadEndElement();

			return ob;
		}

		public System.Web.Services.Protocols.Soap12FaultReasonText ReadObject_Text (bool isNullable, bool checkType)
		{
			System.Web.Services.Protocols.Soap12FaultReasonText ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Text" || t.Namespace != "http://www.w3.org/2003/05/soap-envelope")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Protocols.Soap12FaultReasonText ();

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "lang" && Reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace") {
					ob.@XmlLang = Reader.Value;
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
				else if (Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA)
				{
					ob.@Value = ReadString (ob.@Value);
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

	internal class Soap12FaultWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		public void WriteRoot_Soap12Fault (object o)
		{
			WriteStartDocument ();
			System.Web.Services.Protocols.Soap12Fault ob = (System.Web.Services.Protocols.Soap12Fault) o;
			TopLevelElement ();
			WriteObject_Fault (ob, "Fault", "http://www.w3.org/2003/05/soap-envelope", true, false, true);
		}

		void WriteObject_Fault (System.Web.Services.Protocols.Soap12Fault ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Protocols.Soap12Fault))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Fault", "http://www.w3.org/2003/05/soap-envelope");

			WriteObject_Code (ob.@Code, "Code", "http://www.w3.org/2003/05/soap-envelope", false, false, true);
			WriteObject_Reason (ob.@Reason, "Reason", "http://www.w3.org/2003/05/soap-envelope", false, false, true);
			WriteElementString ("Node", "http://www.w3.org/2003/05/soap-envelope", ((ob.@Node != null) ? (ob.@Node).ToString() : null));
			WriteElementString ("Role", "http://www.w3.org/2003/05/soap-envelope", ((ob.@Role != null) ? (ob.@Role).ToString() : null));
			WriteObject_Detail (ob.@Detail, "Detail", "http://www.w3.org/2003/05/soap-envelope", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Code (System.Web.Services.Protocols.Soap12FaultCode ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Protocols.Soap12FaultCode))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Code", "http://www.w3.org/2003/05/soap-envelope");

			WriteElementQualifiedName ("Value", "http://www.w3.org/2003/05/soap-envelope", ob.@Value);
			WriteObject_Code (ob.@Subcode, "Subcode", "http://www.w3.org/2003/05/soap-envelope", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Reason (System.Web.Services.Protocols.Soap12FaultReason ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Protocols.Soap12FaultReason))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Reason", "http://www.w3.org/2003/05/soap-envelope");

			if (ob.@Texts != null) {
				for (int n15 = 0; n15 < ob.@Texts.Length; n15++) {
					WriteObject_Text (ob.@Texts[n15], "Text", "http://www.w3.org/2003/05/soap-envelope", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Detail (System.Web.Services.Protocols.Soap12FaultDetail ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Protocols.Soap12FaultDetail))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Detail", "http://www.w3.org/2003/05/soap-envelope");

			ICollection o16 = ob.@Attributes;
			if (o16 != null) {
				foreach (XmlAttribute o17 in o16)
					if (o17.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o17, ob);
			}

			if (ob.@Children != null) {
				foreach (XmlNode o18 in ob.@Children) {
					XmlNode o19 = o18;
					if (o19 is XmlElement) {
					}
					else o19.WriteTo (Writer);
					WriteElementLiteral (o19, "", "", false, true);
				}
			}
			WriteValue (ob.@Text);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Text (System.Web.Services.Protocols.Soap12FaultReasonText ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Protocols.Soap12FaultReasonText))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Text", "http://www.w3.org/2003/05/soap-envelope");

			WriteAttribute ("lang", "http://www.w3.org/XML/1998/namespace", ob.@XmlLang);

			WriteValue (ob.@Value);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}


	internal class Soap12FaultBaseSerializer : System.Xml.Serialization.XmlSerializer
	{
		protected override System.Xml.Serialization.XmlSerializationReader CreateReader () {
			return new Soap12FaultReader ();
		}

		protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {
			return new Soap12FaultWriter ();
		}

		public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {
			return true;
		}
	}

	internal sealed class Fault12Serializer : Soap12FaultBaseSerializer
	{
		protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {
			((Soap12FaultWriter)writer).WriteRoot_Soap12Fault(obj);
		}

		protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {
			return ((Soap12FaultReader)reader).ReadRoot_Soap12Fault();
		}
	}

	internal class Soap12FaultSerializerImplementation : System.Xml.Serialization.XmlSerializerImplementation
	{
		System.Collections.Hashtable readMethods = null;
		System.Collections.Hashtable writeMethods = null;
		System.Collections.Hashtable typedSerializers = null;

		public override System.Xml.Serialization.XmlSerializationReader Reader {
			get {
				return new Soap12FaultReader();
			}
		}

		public override System.Xml.Serialization.XmlSerializationWriter Writer {
			get {
				return new Soap12FaultWriter();
			}
		}

		public override System.Collections.Hashtable ReadMethods {
			get {
				lock (this) {
					if (readMethods == null) {
						readMethods = new System.Collections.Hashtable ();
						readMethods.Add (@"", @"ReadRoot_Soap12Fault");
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
						writeMethods.Add (@"", @"WriteRoot_Soap12Fault");
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
						typedSerializers.Add (@"", new FaultSerializer());
					}
					return typedSerializers;
				}
			}
		}

		public override XmlSerializer GetSerializer (Type type)
		{
			switch (type.FullName) {
			case "System.Web.Services.Protocols.Soap12Fault":
				return (XmlSerializer) TypedSerializers [""];

			}
			return base.GetSerializer (type);
		}

		public override bool CanSerialize (System.Type type) {
			if (type == typeof(System.Web.Services.Protocols.Soap12Fault)) return true;
			return false;
		}
	}

}
#endif
