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
using System.Text;

namespace System.Runtime.Serialization.Formatters.Soap {

	internal class Element
	{
		private string _prefix;
		private string _localName;
		private string _namespaceURI;
		private MethodInfo _parseMethod;

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

		public MethodInfo ParseMethod {
			get { return _parseMethod; }
			set { _parseMethod = value; }
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

	internal class SoapTypeMapper
	{
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
			//Type elementType = typeof(string);
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
		
		static string GetKey (string localName, string namespaceUri)
		{
			return localName + " " +  namespaceUri;
		}
		
/*		public Type this [Element element]
		{
		}
*/
		public Type GetType (string xmlName, string xmlNamespace)
		{
			Type type = null;

			string localName = XmlConvert.DecodeName (xmlName);
			string namespaceURI = XmlConvert.DecodeName (xmlNamespace);
			string typeNamespace, assemblyName;
			
			SoapServices.DecodeXmlNamespaceForClrTypeNamespace(
				xmlNamespace, 
				out typeNamespace, 
				out assemblyName);

			string typeName = (typeNamespace == null || typeNamespace == String.Empty) ?
								localName : typeNamespace + Type.Delimiter + localName;
			
			if(assemblyName != null && assemblyName != string.Empty && _binder != null) 
			{
				type = _binder.BindToType(assemblyName, typeName);
			}
			if(type == null) 
			{
				string assemblyQualifiedName = (string)xmlNodeToTypeTable [GetKey (xmlName, xmlNamespace)];
				if(assemblyQualifiedName != null)
					type = Type.GetType(assemblyQualifiedName);
				else
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
				if(type == null)
					throw new SerializationException();
			}
			return type;
		}

		public Element GetXmlElement (string typeFullName, string assemblyName)
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

				int i = typeName.IndexOf ("[");
				if (i != -1)
					typeName = XmlConvert.EncodeName (typeName.Substring (0, i)) + typeName.Substring (i);
				else
				{
					int j = typeName.IndexOf ("&");
					if (j != -1)
						typeName = XmlConvert.EncodeName (typeName.Substring (0, j)) + typeName.Substring (j);
					else
						typeName = XmlConvert.EncodeName (typeName);
				}

				element = new Element(
					prefix, 
					typeName, 
					namespaceURI);
			}
			return element;
		}

		public Element GetXmlElement (Type type)
		{
			if(type == typeof(string)) return elementString;
			Element element = (Element) typeToXmlNodeTable[type.AssemblyQualifiedName];
			if(element == null)
			{
				element = GetXmlElement (type.FullName, type.Assembly.FullName);
//					if(_assemblyFormat == FormatterAssemblyStyle.Full)
//						element = this[type.FullName, type.Assembly.FullName];
//					else
//						element = this[type.FullName, type.Assembly.GetName().Name];

			}
			else if (_xmlWriter != null)
			{
				element = new Element((element.Prefix == null)?_xmlWriter.LookupPrefix(element.NamespaceURI):element.Prefix, element.LocalName, element.NamespaceURI);
			}
			if(element == null)
				throw new SerializationException("Oooops");
			return element;
		}

		static void RegisterType (Type type, string name, string namspace)
		{
			RegisterType (type, name, namspace, true);
		}
		
		static Element RegisterType (Type type, string name, string namspace, bool reverseMap)
		{
			Element element = new Element (name, namspace);
			xmlNodeToTypeTable.Add (GetKey (name, namspace), type.AssemblyQualifiedName);
			if (reverseMap)
				typeToXmlNodeTable.Add (type.AssemblyQualifiedName, element);
			return element;
		}
		
		static void RegisterType (Type type)
		{
			string name = (string) type.GetProperty ("XsdType", BindingFlags.Public | BindingFlags.Static).GetValue (null, null);
			Element element = RegisterType (type, name, XmlSchema.Namespace, true);
			element.ParseMethod = type.GetMethod ("Parse", BindingFlags.Public | BindingFlags.Static);
			if (element.ParseMethod == null)
				throw new InvalidOperationException ("Parse method not found in class " + type);
		}

		private static void InitMappingTables() 
		{
			RegisterType (typeof(System.Array), "Array", SoapEncodingNamespace);
			RegisterType (typeof(string), "string", XmlSchema.Namespace, false);
			RegisterType (typeof(string), "string", SoapEncodingNamespace, false);
			RegisterType (typeof(bool), "boolean", XmlSchema.Namespace);
			RegisterType (typeof(sbyte), "byte", XmlSchema.Namespace);
			RegisterType (typeof(byte), "unsignedByte", XmlSchema.Namespace);
			RegisterType (typeof(long), "long", XmlSchema.Namespace);
			RegisterType (typeof(ulong), "unsignedLong", XmlSchema.Namespace);
			RegisterType (typeof(int), "int", XmlSchema.Namespace);
			RegisterType (typeof(uint), "unsignedInt", XmlSchema.Namespace);
			RegisterType (typeof(float), "float", XmlSchema.Namespace);
			RegisterType (typeof(double), "double", XmlSchema.Namespace);
			RegisterType (typeof(decimal), "decimal", XmlSchema.Namespace);
			RegisterType (typeof(short), "short", XmlSchema.Namespace);
			RegisterType (typeof(ushort), "unsignedShort", XmlSchema.Namespace);
			RegisterType (typeof(object), "anyType", XmlSchema.Namespace);
			RegisterType (typeof(DateTime), "dateTime", XmlSchema.Namespace);
			RegisterType (typeof(TimeSpan), "duration", XmlSchema.Namespace);
			RegisterType (typeof(SoapFault), "Fault", SoapEnvelopeNamespace);
			RegisterType (typeof(byte[]), "base64", SoapEncodingNamespace);
			RegisterType (typeof(MethodSignature), "methodSignature", SoapEncodingNamespace);
			RegisterType (typeof(SoapAnyUri));
			RegisterType (typeof(SoapEntity));
			RegisterType (typeof(SoapMonth));
			RegisterType (typeof(SoapNonNegativeInteger));
			RegisterType (typeof(SoapToken));
			RegisterType (typeof(SoapBase64Binary));
			RegisterType (typeof(SoapHexBinary));
			RegisterType (typeof(SoapMonthDay));
			RegisterType (typeof(SoapNonPositiveInteger));
			RegisterType (typeof(SoapYear));
			RegisterType (typeof(SoapDate));
			RegisterType (typeof(SoapId));
			RegisterType (typeof(SoapName));
			RegisterType (typeof(SoapNormalizedString));
			RegisterType (typeof(SoapYearMonth));
			RegisterType (typeof(SoapIdref));
			RegisterType (typeof(SoapNcName));
			RegisterType (typeof(SoapNotation));
			RegisterType (typeof(SoapDay));
			RegisterType (typeof(SoapIdrefs));
			RegisterType (typeof(SoapNegativeInteger));
			RegisterType (typeof(SoapPositiveInteger));
			RegisterType (typeof(SoapInteger));
			RegisterType (typeof(SoapNmtoken));
			RegisterType (typeof(SoapQName));
			RegisterType (typeof(SoapEntities));
			RegisterType (typeof(SoapLanguage));
			RegisterType (typeof(SoapNmtokens));
			RegisterType (typeof(SoapTime));
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
				return ((double) value).ToString ("G17", CultureInfo.InvariantCulture);
			}
			else if (value is float) {
				return ((float) value).ToString ("G9", CultureInfo.InvariantCulture);
			}
			else if (value is TimeSpan) {
				return SoapDuration.ToString ((TimeSpan)value);
			}
			else if (value is bool) {
				return ((bool) value) ? "true" : "false";
			}
			else if (value is MethodSignature) {
				return null;
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
		
		public static bool CanBeValue (Type type)
		{
			if(type.IsPrimitive) return true;
			if(type.IsEnum) return true;
			if(_canBeValueTypeList.BinarySearch(type.ToString()) >= 0) 
			{
				return true;
			}
			return false;
		}
		
		public bool IsInternalSoapType (Type type)
		{
			if (CanBeValue (type))
				return true;
			if (typeof(ISoapXsd).IsAssignableFrom (type))
				return true;
			if (type == typeof (MethodSignature))
				return true;
			return false;
		}
		
		public object ReadInternalSoapValue (SoapReader reader, Type type)
		{
			if (CanBeValue (type))
				return ParseXsdValue (reader.XmlReader.ReadElementString (), type);
			
			if (type == typeof(MethodSignature)) {
				return MethodSignature.ReadXmlValue (reader);
			}
			
			string val = reader.XmlReader.ReadElementString ();
			
			Element elem = GetXmlElement (type);
			if (elem.ParseMethod != null)
				return elem.ParseMethod.Invoke (null, new object[] { val });
			
			throw new SerializationException ("Can't parse type " + type);
		}
		
		public string GetInternalSoapValue (SoapWriter writer, object value)
		{
			if (CanBeValue (value.GetType()))
				return GetXsdValue (value);
			else if (value is MethodSignature)
				return ((MethodSignature)value).GetXmlValue (writer);
			else
				return value.ToString ();
		}
	}
	
	class MethodSignature
	{
		Type[] types;
		
		public MethodSignature (Type[] types)
		{
			this.types = types;
		}
		
		public static object ReadXmlValue (SoapReader reader)
		{
			reader.XmlReader.MoveToElement ();
			if (reader.XmlReader.IsEmptyElement) {
				reader.XmlReader.Skip ();
				return new Type[0];
			}
			reader.XmlReader.ReadStartElement ();
			string names = reader.XmlReader.ReadString ();
			while (reader.XmlReader.NodeType != XmlNodeType.EndElement)
				reader.XmlReader.Skip ();
				
			ArrayList types = new ArrayList ();
			string[] tns = names.Split (' ');
			foreach (string tn in tns) {
				if (tn.Length == 0) continue;
				types.Add (reader.GetTypeFromQName (tn));
			}
			reader.XmlReader.ReadEndElement ();
			return (Type[]) types.ToArray (typeof(Type));
		}
		
		public string GetXmlValue (SoapWriter writer)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (Type t in types) {
				Element elem = writer.Mapper.GetXmlElement (t);
				if (sb.Length > 0) sb.Append (' ');
				string prefix = writer.GetNamespacePrefix (elem);
				sb.Append (prefix).Append (':').Append (elem.LocalName);
			}
			return sb.ToString ();
		}
	}
}
