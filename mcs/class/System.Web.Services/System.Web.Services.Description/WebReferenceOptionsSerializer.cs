#if NET_2_0
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Description
{
	internal class WebReferenceOptionsReader : XmlSerializationReader
	{
		public object ReadRoot_WebReferenceOptions ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "webReferenceOptions" || Reader.NamespaceURI != "http://microsoft.com/webReference/")
				throw CreateUnknownNodeException();
			return ReadObject_webReferenceOptions (true, true);
		}

		public System.Web.Services.Description.WebReferenceOptions ReadObject_webReferenceOptions (bool isNullable, bool checkType)
		{
			System.Web.Services.Description.WebReferenceOptions ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "webReferenceOptions" || t.Namespace != "http://microsoft.com/webReference/")
					throw CreateUnknownTypeException(t);
			}

			ob = new System.Web.Services.Description.WebReferenceOptions ();

			Reader.MoveToElement();

			ob.@CodeGenerationOptions = ((System.Xml.Serialization.CodeGenerationOptions) System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync);
			ob.@Style = ((System.Web.Services.Description.ServiceDescriptionImportStyle) System.Web.Services.Description.ServiceDescriptionImportStyle.Client);
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
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "style" && Reader.NamespaceURI == "http://microsoft.com/webReference/" && !b2) {
						b2 = true;
						ob.@Style = GetEnumValue_ServiceDescriptionImportStyle (Reader.ReadElementString ());
					}
					else if (Reader.LocalName == "verbose" && Reader.NamespaceURI == "http://microsoft.com/webReference/" && !b3) {
						b3 = true;
						ob.@Verbose = XmlConvert.ToBoolean (Reader.ReadElementString ());
					}
					else if (Reader.LocalName == "codeGenerationOptions" && Reader.NamespaceURI == "http://microsoft.com/webReference/" && !b0) {
						b0 = true;
						ob.@CodeGenerationOptions = GetEnumValue_CodeGenerationOptions (Reader.ReadElementString ());
					}
					else if (Reader.LocalName == "schemaImporterExtensions" && Reader.NamespaceURI == "http://microsoft.com/webReference/" && !b1) {
						if (((object)ob.@SchemaImporterExtensions) == null)
							throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
						if (Reader.IsEmptyElement) {
							Reader.Skip();
						} else {
							int n4 = 0;
							Reader.ReadStartElement();
							Reader.MoveToContent();

							while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
							{
								if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
								{
									if (Reader.LocalName == "type" && Reader.NamespaceURI == "http://microsoft.com/webReference/") {
										if (((object)ob.@SchemaImporterExtensions) == null)
											throw CreateReadOnlyCollectionException ("System.Collections.Specialized.StringCollection");
										ob.@SchemaImporterExtensions.Add (Reader.ReadElementString ());
										n4++;
									}
									else UnknownNode (null);
								}
								else UnknownNode (null);

								Reader.MoveToContent();
							}
							ReadEndElement();
						}
						b1 = true;
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

		public System.Web.Services.Description.ServiceDescriptionImportStyle ReadObject_ServiceDescriptionImportStyle (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Web.Services.Description.ServiceDescriptionImportStyle res = GetEnumValue_ServiceDescriptionImportStyle (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Web.Services.Description.ServiceDescriptionImportStyle GetEnumValue_ServiceDescriptionImportStyle (string xmlName)
		{
			switch (xmlName)
			{
				case "client": return System.Web.Services.Description.ServiceDescriptionImportStyle.Client;
				case "server": return System.Web.Services.Description.ServiceDescriptionImportStyle.Server;
				case "serverInterface": return System.Web.Services.Description.ServiceDescriptionImportStyle.ServerInterface;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Web.Services.Description.ServiceDescriptionImportStyle));
			}
		}

		public System.Xml.Serialization.CodeGenerationOptions ReadObject_CodeGenerationOptions (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			System.Xml.Serialization.CodeGenerationOptions res = GetEnumValue_CodeGenerationOptions (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		System.Xml.Serialization.CodeGenerationOptions GetEnumValue_CodeGenerationOptions (string xmlName)
		{
			xmlName = xmlName.Trim();
			if (xmlName.Length == 0) return (System.Xml.Serialization.CodeGenerationOptions)0;
			System.Xml.Serialization.CodeGenerationOptions sb = (System.Xml.Serialization.CodeGenerationOptions)0;
			string[] enumNames = xmlName.Split (null);
			foreach (string name in enumNames)
			{
				if (name == string.Empty) continue;
				sb |= GetEnumValue_CodeGenerationOptions_Switch (name); 
			}
			return sb;
		}

		System.Xml.Serialization.CodeGenerationOptions GetEnumValue_CodeGenerationOptions_Switch (string xmlName)
		{
			switch (xmlName)
			{
				case "properties": return System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;
				case "newAsync": return System.Xml.Serialization.CodeGenerationOptions.GenerateNewAsync;
				case "oldAsync": return System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync;
				case "order": return System.Xml.Serialization.CodeGenerationOptions.GenerateOrder;
				case "enableDataBinding": return System.Xml.Serialization.CodeGenerationOptions.EnableDataBinding;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(System.Xml.Serialization.CodeGenerationOptions));
			}
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

	}

	internal class WebReferenceOptionsWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		public void WriteRoot_WebReferenceOptions (object o)
		{
			WriteStartDocument ();
			System.Web.Services.Description.WebReferenceOptions ob = (System.Web.Services.Description.WebReferenceOptions) o;
			TopLevelElement ();
			WriteObject_webReferenceOptions (ob, "webReferenceOptions", "http://microsoft.com/webReference/", true, false, true);
		}

		void WriteObject_webReferenceOptions (System.Web.Services.Description.WebReferenceOptions ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.WebReferenceOptions))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("webReferenceOptions", "http://microsoft.com/webReference/");

			if (ob.@CodeGenerationOptions != ((System.Xml.Serialization.CodeGenerationOptions) System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync)) {
				WriteElementString ("codeGenerationOptions", "http://microsoft.com/webReference/", GetEnumValue_CodeGenerationOptions (ob.@CodeGenerationOptions));
			}
			if (ob.@SchemaImporterExtensions != null) {
				WriteStartElement ("schemaImporterExtensions", "http://microsoft.com/webReference/", ob.@SchemaImporterExtensions);
				for (int n5 = 0; n5 < ob.@SchemaImporterExtensions.Count; n5++) {
					WriteElementString ("type", "http://microsoft.com/webReference/", ob.@SchemaImporterExtensions[n5]);
				}
				WriteEndElement (ob.@SchemaImporterExtensions);
			}
			if (ob.@Style != ((System.Web.Services.Description.ServiceDescriptionImportStyle) System.Web.Services.Description.ServiceDescriptionImportStyle.Client)) {
				WriteElementString ("style", "http://microsoft.com/webReference/", GetEnumValue_ServiceDescriptionImportStyle (ob.@Style));
			}
			WriteElementString ("verbose", "http://microsoft.com/webReference/", (ob.@Verbose?"true":"false"));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_CodeGenerationOptions (System.Xml.Serialization.CodeGenerationOptions ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Serialization.CodeGenerationOptions))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("CodeGenerationOptions", "http://microsoft.com/webReference/");

			Writer.WriteString (GetEnumValue_CodeGenerationOptions (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		static readonly string[] _xmlNamesCodeGenerationOptions = { "properties","newAsync","oldAsync","order","enableDataBinding" };
		static readonly long[] _valuesCodeGenerationOptions = { 1L,2L,4L,8L,16L };

		string GetEnumValue_CodeGenerationOptions (System.Xml.Serialization.CodeGenerationOptions val)
		{
			switch (val) {
				case System.Xml.Serialization.CodeGenerationOptions.GenerateProperties: return "properties";
				case System.Xml.Serialization.CodeGenerationOptions.GenerateNewAsync: return "newAsync";
				case System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync: return "oldAsync";
				case System.Xml.Serialization.CodeGenerationOptions.GenerateOrder: return "order";
				case System.Xml.Serialization.CodeGenerationOptions.EnableDataBinding: return "enableDataBinding";
				default:
					if (val.ToString () == "0") return string.Empty;
					return FromEnum ((long) val, _xmlNamesCodeGenerationOptions, _valuesCodeGenerationOptions, typeof (System.Xml.Serialization.CodeGenerationOptions).FullName);
			}
		}

		void WriteObject_ServiceDescriptionImportStyle (System.Web.Services.Description.ServiceDescriptionImportStyle ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Web.Services.Description.ServiceDescriptionImportStyle))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ServiceDescriptionImportStyle", "http://microsoft.com/webReference/");

			Writer.WriteString (GetEnumValue_ServiceDescriptionImportStyle (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_ServiceDescriptionImportStyle (System.Web.Services.Description.ServiceDescriptionImportStyle val)
		{
			switch (val) {
				case System.Web.Services.Description.ServiceDescriptionImportStyle.Client: return "client";
				case System.Web.Services.Description.ServiceDescriptionImportStyle.Server: return "server";
				case System.Web.Services.Description.ServiceDescriptionImportStyle.ServerInterface: return "serverInterface";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (System.Web.Services.Description.ServiceDescriptionImportStyle).FullName);
			}
		}

		protected override void InitCallbacks ()
		{
		}

	}


	internal class WebReferenceOptionsBaseSerializer : System.Xml.Serialization.XmlSerializer
	{
		protected override System.Xml.Serialization.XmlSerializationReader CreateReader () {
			return new WebReferenceOptionsReader ();
		}

		protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {
			return new WebReferenceOptionsWriter ();
		}

		public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {
			return true;
		}
	}

	internal sealed class webReferenceOptionsSerializer : WebReferenceOptionsBaseSerializer
	{
		protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {
			((WebReferenceOptionsWriter)writer).WriteRoot_WebReferenceOptions(obj);
		}

		protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {
			return ((WebReferenceOptionsReader)reader).ReadRoot_WebReferenceOptions();
		}
	}

	internal class WebReferenceOptionsSerializerImplementation : System.Xml.Serialization.XmlSerializerImplementation
	{
		System.Collections.Hashtable readMethods = null;
		System.Collections.Hashtable writeMethods = null;
		System.Collections.Hashtable typedSerializers = null;

		public override System.Xml.Serialization.XmlSerializationReader Reader {
			get {
				return new WebReferenceOptionsReader();
			}
		}

		public override System.Xml.Serialization.XmlSerializationWriter Writer {
			get {
				return new WebReferenceOptionsWriter();
			}
		}

		public override System.Collections.Hashtable ReadMethods {
			get {
				lock (this) {
					if (readMethods == null) {
						readMethods = new System.Collections.Hashtable ();
						readMethods.Add (@"", @"ReadRoot_WebReferenceOptions");
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
						writeMethods.Add (@"", @"WriteRoot_WebReferenceOptions");
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
						typedSerializers.Add (@"", new webReferenceOptionsSerializer());
					}
					return typedSerializers;
				}
			}
		}

		public override XmlSerializer GetSerializer (Type type)
		{
			switch (type.FullName) {
			case "System.Web.Services.Description.WebReferenceOptions":
				return (XmlSerializer) TypedSerializers [""];

			}
			return base.GetSerializer (type);
		}

		public override bool CanSerialize (System.Type type) {
			if (type == typeof(System.Web.Services.Description.WebReferenceOptions)) return true;
			return false;
		}
	}

}
#endif
