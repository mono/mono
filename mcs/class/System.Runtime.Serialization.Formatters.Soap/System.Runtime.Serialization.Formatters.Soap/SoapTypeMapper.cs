// created on 09/04/2003 at 18:58
//
//	System.Runtime.Serialization.Formatters.Soap.SoapTypeMapper
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
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
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml.Schema;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Globalization;

namespace System.Runtime.Serialization.Formatters.Soap {

	internal class Element
	{
		private string _prefix;
		private string _localName;
		private string _namespaceURI;

		public Element(string prefix, string localName, string namespaceURI) 
		{
			_prefix = prefix;
			_localName = localName;
			_namespaceURI = namespaceURI;
		}

		public Element(string localName, string namespaceURI): this(null, localName, namespaceURI)
		{
		}

		public string Prefix
		{
			get
			{
				return _prefix;
			}
		}

		public string LocalName
		{
			get
			{
				return _localName;
			}
		}

		public string NamespaceURI 
		{
			get 
			{
				return _namespaceURI;
			}
		}

		public override bool Equals(object obj) 
		{
			Element element = obj as Element;
			return (_localName == XmlConvert.DecodeName(element._localName) &&
				_namespaceURI == XmlConvert.DecodeName(element._namespaceURI))?true:false;
		}

		public override int GetHashCode()
		{
			return (String.Format("{0} {1}", 
				XmlConvert.DecodeName(_localName),
				XmlConvert.DecodeName(_namespaceURI))).GetHashCode();
		}

		public override string ToString() 
		{
			return string.Format("Element.Prefix = {0}, Element.LocalName = {1}, Element.NamespaceURI = {2}", this.Prefix, this.LocalName, this.NamespaceURI);
		}
	}

	internal class SoapTypeMapper {
		private static Hashtable xmlNodeToTypeTable = new Hashtable();
		private static Hashtable typeToXmlNodeTable = new Hashtable();
		public static readonly string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		public static readonly string SoapEncodingPrefix = "SOAP-ENC";
		public static readonly string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
		public static readonly string SoapEnvelopePrefix = "SOAP-ENV";
		//internal static readonly string SoapEnvelope;
		private XmlTextWriter _xmlWriter;
		private long _prefixNumber;
		private Hashtable namespaceToPrefixTable = new Hashtable();
		private SerializationBinder _binder;
		private static ArrayList _canBeValueTypeList;
		private FormatterAssemblyStyle _assemblyFormat = FormatterAssemblyStyle.Full;
		private Element elementString;


		// Constructor used by SoapReader
		public SoapTypeMapper(SerializationBinder binder) 
		{
			_binder = binder;
		}

		// Constructor used by SoapWriter
		public SoapTypeMapper(
			XmlTextWriter xmlWriter, 
			FormatterAssemblyStyle assemblyFormat,
			FormatterTypeStyle typeFormat) 
		{
			_xmlWriter = xmlWriter;
			_assemblyFormat = assemblyFormat;
			_prefixNumber = 1;
			Type elementType;
			elementType = typeof(string);
			if(typeFormat == FormatterTypeStyle.XsdString)
			{
				elementString = new Element("xsd", "string", XmlSchema.Namespace);
			}
			else
			{
				elementString = new Element(SoapEncodingPrefix, "string", SoapEncodingNamespace);
			}
//			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);
		}
		
		static SoapTypeMapper() {
//			SoapEnvelope = String.Format(
//				"<{0}:Envelope xmlns:{0}='{1}' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='{2}' xmlns:{3}='{4}' xmlns:clr='{5}' SOAP-ENV:encodingStyle='http://schemas.xmlsoap.org/soap/encoding/'/>",
//				SoapEnvelopePrefix,
//				SoapEnvelopeNamespace,
//				XmlSchema.Namespace,
//				SoapEncodingPrefix,
//				SoapEncodingNamespace,
//				SoapServices.XmlNsForClrType);
			_canBeValueTypeList = new ArrayList();
			_canBeValueTypeList.Add(typeof(DateTime).ToString());
			_canBeValueTypeList.Add(typeof(TimeSpan).ToString());
			_canBeValueTypeList.Add(typeof(string).ToString());
			_canBeValueTypeList.Add(typeof(decimal).ToString());
			_canBeValueTypeList.Sort();
			InitMappingTables();
			
		}

		public Type this[Element element]
		{
			get 
			{
				Type type = null;

				string localName = XmlConvert.DecodeName(element.LocalName);
				string namespaceURI = XmlConvert.DecodeName(element.NamespaceURI);
				string typeNamespace, assemblyName;
				SoapServices.DecodeXmlNamespaceForClrTypeNamespace(
					element.NamespaceURI, 
					out typeNamespace, 
					out assemblyName);
				string typeName = typeNamespace + Type.Delimiter + localName;

				if(assemblyName != null && assemblyName != string.Empty && _binder != null) 
				{
					type = _binder.BindToType(assemblyName, typeName);
				}
				if(type == null) 
				{
					string assemblyQualifiedName = (string)xmlNodeToTypeTable[element];
					if(assemblyQualifiedName != null)
						type = Type.GetType(assemblyQualifiedName);
					else
					{ 

						type = Type.GetType(element.LocalName);
						if(type == null) 
						{ 

							type = Type.GetType(typeName);
							if(type == null) 
							{

								if(assemblyName == null || assemblyName == String.Empty)
									throw new SerializationException(
										String.Format("Parse Error, no assembly associated with XML key {0} {1}", 
										localName, 
										namespaceURI));
								type = FormatterServices.GetTypeFromAssembly(
									Assembly.Load(assemblyName), 
									typeName);
							}
						}
					}
					if(type == null)
						throw new SerializationException();
				}
				return type;
			}
		}


		public Element this[string typeFullName, string assemblyName]
		{
			get 
			{
				Element element;
				string typeNamespace = string.Empty;
				string typeName = typeFullName;
				if(_assemblyFormat == FormatterAssemblyStyle.Simple)
				{
					string[] items = assemblyName.Split(',');
					assemblyName = items[0];
				}
				string assemblyQualifiedName = typeFullName + ", " + assemblyName;
				element = (Element) typeToXmlNodeTable[assemblyQualifiedName];
				if(element == null)
				{
					int typeNameIndex = typeFullName.LastIndexOf('.');
					if(typeNameIndex != -1) 
					{
						typeNamespace = typeFullName.Substring(0, typeNameIndex);
						typeName = typeFullName.Substring(typeNamespace.Length + 1);
					}
					string namespaceURI = 
						SoapServices.CodeXmlNamespaceForClrTypeNamespace(
						typeNamespace, 
						(!assemblyName.StartsWith("mscorlib"))?assemblyName:String.Empty);
					string prefix = (string) namespaceToPrefixTable[namespaceURI];
					if(prefix == null || prefix == string.Empty)
					{
						prefix = "a" + (_prefixNumber++).ToString();
						namespaceToPrefixTable[namespaceURI] = prefix;

					}
					element = new Element(
						prefix, 
						XmlConvert.EncodeName(typeName), 
						namespaceURI);
				}
				return element;
			}
		}

		public Element this[Type type]
		{
			get 
			{
				if(type == typeof(string)) return elementString;
				Element element = (Element) typeToXmlNodeTable[type.AssemblyQualifiedName];
				if(element == null)
				{
					element = this[type.FullName, type.Assembly.FullName];
//					if(_assemblyFormat == FormatterAssemblyStyle.Full)
//						element = this[type.FullName, type.Assembly.FullName];
//					else
//						element = this[type.FullName, type.Assembly.GetName().Name];

				}
				else
				{
					element = new Element((element.Prefix == null)?_xmlWriter.LookupPrefix(element.NamespaceURI):element.Prefix, element.LocalName, element.NamespaceURI);
				}
				if(element == null)
					throw new SerializationException("Oooops");
				return element;
			}
		}

		public static bool CanBeValue(Type type)
		{
			if(type.IsPrimitive) return true;
			if(type.IsEnum) return true;
			if(_canBeValueTypeList.BinarySearch(type.ToString()) >= 0) 
			{
				return true;
			}
			return false;
		}

		private static void InitMappingTables() 
		{
			Element element;
			Type elementType;
			element = new Element("Array", SoapEncodingNamespace);
			elementType = typeof(System.Array);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("string", XmlSchema.Namespace);
			elementType = typeof(string);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
//			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("string", SoapEncodingNamespace);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);

			element = new Element("long", XmlSchema.Namespace);
			elementType = typeof(long);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("int", XmlSchema.Namespace);
			elementType = typeof(int);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("float", XmlSchema.Namespace);
			elementType = typeof(float);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("decimal", XmlSchema.Namespace);
			elementType = typeof(decimal);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("short", XmlSchema.Namespace);
			elementType = typeof(short);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("anyType", XmlSchema.Namespace);
			elementType = typeof(object);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("dateTime", XmlSchema.Namespace);
			elementType = typeof(DateTime);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("duration", XmlSchema.Namespace);
			elementType = typeof(TimeSpan);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("Fault", SoapEnvelopeNamespace);
			elementType = typeof(System.Runtime.Serialization.Formatters.SoapFault);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);

			element = new Element("base64", SoapEncodingNamespace);
			elementType = typeof(byte[]);
			xmlNodeToTypeTable.Add(element, elementType.AssemblyQualifiedName);
			typeToXmlNodeTable.Add(elementType.AssemblyQualifiedName, element);
		}
		
		public static string GetXsdValue (object value)
		{
			if (value is DateTime) {
				return SoapDateTime.ToString ((DateTime)value);
			}
			else if (value is decimal) {
				return ((decimal) value).ToString (CultureInfo.InvariantCulture);
			}
			else if (value is double) {
				return ((double) value).ToString (CultureInfo.InvariantCulture);
			}
			else if (value is float) {
				return ((float) value).ToString (CultureInfo.InvariantCulture);
			}
			else if (value is TimeSpan) {
				return SoapDuration.ToString ((TimeSpan)value);
			}
			else {
				return value.ToString ();
			}
		}
		
		public static object ParseXsdValue (string value, Type type)
		{
			if (type == typeof(DateTime)) {
				return SoapDateTime.Parse (value);
			}
			else if (type == typeof(decimal)) {
				return decimal.Parse (value, CultureInfo.InvariantCulture);
			}
			else if (type == typeof(double)) {
				return double.Parse (value, CultureInfo.InvariantCulture);
			}
			else if (type == typeof(float)) {
				return float.Parse (value, CultureInfo.InvariantCulture);
			}
			else if (type == typeof (TimeSpan)) {
				return SoapDuration.Parse (value);
			}
			else if(type.IsEnum) {
				return Enum.Parse(type, value);
			}
			else {
				return Convert.ChangeType (value, type, CultureInfo.InvariantCulture);
			}
		}

	}
}
