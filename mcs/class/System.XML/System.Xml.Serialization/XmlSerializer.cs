//
// Mono Class Libraries
// System.Xml.Serialization.XmlSerializer
//
// Authors:
//   John Donagher (john@webmeta.com)
//   Ajay kumar Dwivedi (adwiv@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//   Elan Feingold (ef10@cornell.edu)
//
// (C) 2002 John Donagher, Ajay kumar Dwivedi
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Elan Feingold
// 

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace System.Xml.Serialization {	
	/// <summary>
	/// Summary description for XmlSerializer.
	/// </summary>
	public class XmlSerializer {

		#region Fields

		Type xsertype;
		XmlAttributeOverrides overrides;
		Type[] extraTypes;
		XmlRootAttribute rootAttribute;
		string defaultNamespace;
		Hashtable typeTable;
		bool useOrder;
		bool isNullable;
		Hashtable typeMappings = new Hashtable ();

		#endregion // Fields

		#region Constructors

		protected XmlSerializer ()
		{
		}

		public XmlSerializer (Type type)
			: this (type, null, null, null, null)
		{
		}

		public XmlSerializer (XmlTypeMapping xmlTypeMapping)
		{
			typeMappings.Add (xmlTypeMapping.TypeFullName, xmlTypeMapping);
		}

		public XmlSerializer (Type type, string defaultNamespace)
			: this (type, null, null, null, defaultNamespace)
		{
		}

		public XmlSerializer (Type type, Type[] extraTypes)
			: this (type, null, extraTypes, null, null)
		{
		}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
			: this (type, overrides, null, null, null)
		{
		}

		public XmlSerializer (Type type, XmlRootAttribute root)
			: this (type, null, null, root, null)
		{
		}

		internal XmlSerializer (Hashtable typeTable)
		{
			this.typeTable = typeTable;
		}

		public XmlSerializer (Type type,
					  XmlAttributeOverrides overrides,
					  Type [] extraTypes,
					  XmlRootAttribute root,
					  string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			XmlReflectionImporter ri = new XmlReflectionImporter (overrides, defaultNamespace);
			TypeData td = TypeTranslator.GetTypeData (type);
			typeMappings.Add (td.FullTypeName, ri.ImportTypeMapping (type, root, defaultNamespace));
			ri.IncludeTypes (type);

			if (extraTypes != null) {
				foreach (Type t in extraTypes) {
					td = TypeTranslator.GetTypeData (t);
					string n = td.FullTypeName;
					typeMappings.Add (n, ri.ImportTypeMapping (type, root, defaultNamespace));
					ri.IncludeTypes (t);
				}
			}
			
			this.xsertype = type;
			this.overrides = overrides;
			this.extraTypes = (extraTypes == null ? new Type[0] : extraTypes);
		
			if (root != null)
				this.rootAttribute = root;
			else {
				object[] attributes = type.GetCustomAttributes (typeof (XmlRootAttribute), false);
				if (attributes.Length > 0)
					this.rootAttribute = (XmlRootAttribute) attributes[0];
			}
			
			this.defaultNamespace = defaultNamespace;

			if (typeTable == null)
				typeTable = new Hashtable ();

			FillTypeTable (type);
		}

		#endregion // Constructors

		#region Events

		public event XmlAttributeEventHandler UnknownAttribute;
		public event XmlElementEventHandler UnknownElement;
		public event XmlNodeEventHandler UnknownNode;
		public event UnreferencedObjectEventHandler UnreferencedObject;

		#endregion // Events

		#region Properties

		internal bool UseOrder {
			get { return useOrder; }
			set { useOrder = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual bool CanDeserialize (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}

		protected virtual XmlSerializationReader CreateReader ()
		{
			// This is what MS does!!!
			throw new NotImplementedException ();
		}

		protected virtual XmlSerializationWriter CreateWriter ()
		{
			// This is what MS does!!!
			throw new NotImplementedException ();
		}

		public object Deserialize (Stream stream)
		{
			XmlTextReader xmlReader = new XmlTextReader(stream);
			return Deserialize(xmlReader);
		}

		public object Deserialize (TextReader textReader)
		{
			XmlTextReader xmlReader = new XmlTextReader(textReader);
			return Deserialize(xmlReader);
		}

		public bool DeserializeComposite(XmlReader xmlReader, ref Object theObject)
		{
			Type objType = theObject.GetType();
			bool retVal	 = true;

			//Console.WriteLine("DeserializeComposite({0})", objType);
			
			// Are we at an empty element?
			if (xmlReader.IsEmptyElement == true)
				return retVal;

			// Read each field, counting how many we find.
			for (int numFields=0; xmlReader.Read(); )
			{
				XmlNodeType xmlNodeType = xmlReader.NodeType;
				bool		isEmpty		= xmlReader.IsEmptyElement;
				
				if (xmlNodeType == XmlNodeType.Element)
				{
					// Read the field.
					DeserializeField(xmlReader, ref theObject, xmlReader.Name);
					numFields++;
				}
				else if (xmlNodeType == XmlNodeType.EndElement)
				{
					if (numFields == 0)
					{
						//Console.WriteLine("Empty object deserialized, ignoring.");
						retVal = false;
					}
					
					return retVal;
				}
			}

			return retVal;
		}

		public void DeserializeField(XmlReader	xmlReader,
									 ref Object theObject,
									 String		fieldName)
		{
			// Get the type
			FieldInfo fieldInfo	   = theObject.GetType().GetField(fieldName);
			Type	  fieldType	   = fieldInfo.FieldType;
			Object	  value		   = null;
			bool	  isEmptyField = xmlReader.IsEmptyElement;

			//Console.WriteLine("DeserializeField({0} of type {1}", fieldName, fieldType);

			if (fieldType.IsArray && fieldType != typeof(System.Byte[]))
			{
				// Create an empty array list.
				ArrayList list = new ArrayList();

				// Call out to deserialize it.
				DeserializeArray(xmlReader, list, fieldType.GetElementType());
				value = list.ToArray(fieldType.GetElementType());
			}
			else if (isEmptyField == true && fieldType.IsArray)
			{
				// Must be a byte array, just create an empty one.
				value = new byte[0];
			}
			else if (isEmptyField == false && 
					 (IsInbuiltType(fieldType) || fieldType.IsEnum || fieldType.IsArray))
			{
				// Built in, set it.
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Text)
					{
						//Console.WriteLine(" -> value is '{0}'", xmlReader.Value);
						
						if (fieldType == typeof(Guid))
							value = XmlConvert.ToGuid(xmlReader.Value);
						else if (fieldType == typeof(Boolean))
							value = XmlConvert.ToBoolean(xmlReader.Value);
						else if (fieldType == typeof(String))
							value = xmlReader.Value;
						else if (fieldType == typeof(Int64))
							value = XmlConvert.ToInt64(xmlReader.Value);
						else if (fieldType == typeof(DateTime))
							value = XmlConvert.ToDateTime(xmlReader.Value);
						else if (fieldType.IsEnum)
							value = Enum.Parse(fieldType, xmlReader.Value);
						else if (fieldType == typeof(System.Byte[]))
							value = XmlCustomFormatter.ToByteArrayBase64(xmlReader.Value);
						else
							Console.WriteLine("Error (type is '{0})'", fieldType);
						
						break;
					}
				}
			}
			else
			{
				//Console.WriteLine("Creating new {0}", fieldType);

				// Create the new complex object.
				value = System.Activator.CreateInstance(fieldType);

				// Recurse, allowing the method to whack the object if it's empty.
				DeserializeComposite(xmlReader, ref value);
			}

			//Console.WriteLine(" Setting {0} to '{1}'", fieldName, value);

			// Set the field value.
			theObject.GetType().InvokeMember(fieldName,
											 BindingFlags.SetField, 
											 null,
											 theObject, 
											 new Object[] { value },
											 null, null, null);

			// We need to munch the end.
			if (IsInbuiltType(fieldType) || 
				fieldType.IsEnum		 || 
				fieldType == typeof(System.Byte[]))
			{
				if (isEmptyField == false)
					while (xmlReader.Read() && xmlReader.NodeType != XmlNodeType.EndElement)
						;
			}
			
		}

		public void DeserializeArray(XmlReader xmlReader, ArrayList theList, Type theType)
		{
			//Console.WriteLine(" DeserializeArray({0})", theType);

			if (xmlReader.IsEmptyElement)
			{
				//Console.WriteLine("  DeserializeArray -> empty, nothing to do here");
				return;
			}

			while (xmlReader.Read())
			{
				XmlNodeType xmlNodeType = xmlReader.NodeType;
				bool		isEmpty		= xmlReader.IsEmptyElement;

				if (xmlNodeType == XmlNodeType.Element)
				{
					// Must be an element of the array, create it.
					Object obj = System.Activator.CreateInstance(theType);

					//Console.WriteLine("  created obj of type '{0}'", obj.GetType());

					// Deserialize and add.
					if (DeserializeComposite(xmlReader, ref obj))
					{
						theList.Add(obj);
					}
				}
				
				if ((xmlNodeType == XmlNodeType.Element && isEmpty) ||
					(xmlNodeType == XmlNodeType.EndElement))
				{
					return;
				}
			}
		}

		public object Deserialize(XmlReader xmlReader)
		{
			Object obj = null;

			// Read each node in the tree.
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					// Create the top level object.
					//Console.WriteLine("Creating '{0}'", xsertype.FullName);
					obj = System.Activator.CreateInstance(xsertype);

					// Deserialize it.
					DeserializeComposite(xmlReader, ref obj);
				}
				else if (xmlReader.NodeType == XmlNodeType.EndElement)
				{
					return obj;
				}
			}				   

			return obj;
		}  

		protected virtual object Deserialize (XmlSerializationReader reader)
		{
			// This is what MS does!!!
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlSerializer [] FromMappings (XmlMapping [] mappings)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlSerializer [] FromTypes (Type [] mappings)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Serialize (object o, XmlSerializationWriter writer)
		{
			throw new NotImplementedException ();
		}

		public void Serialize (Stream stream, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}
		
		public void Serialize (TextWriter textWriter, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}
		
		public void Serialize (XmlWriter xmlWriter, object o)
		{
			Serialize (xmlWriter, o, null);
		}
		
		public void Serialize (Stream stream, object o, XmlSerializerNamespaces namespaces)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
		}
		
		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
		}

		public void Serialize (XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{	
			Type objType = xsertype;//o.GetType ();

			if (IsInbuiltType(objType)) 
			{
				if (writer.WriteState == WriteState.Start)
                                        writer.WriteStartDocument ();
				SerializeBuiltIn (writer, o);
				// Keep WriteState.Content state.
				// writer.WriteEndDocument();
				writer.Flush ();
				return;
			}

			string rootName = objType.Name;
			string rootNs = String.Empty;
			string rootPrefix = String.Empty;

			if (namespaces == null)
				namespaces = new XmlSerializerNamespaces ();

			if (namespaces.Count == 0) {
				namespaces.Add ("xsd", XmlSchema.Namespace);
				namespaces.Add ("xsi", XmlSchema.InstanceNamespace);
			}

			XmlSerializerNamespaces nss = new XmlSerializerNamespaces ();
			XmlQualifiedName[] qnames;
			
			if (writer.WriteState == WriteState.Start)
				writer.WriteStartDocument ();
			object [] memberObj = (object []) typeTable [objType];
			if (memberObj == null)
				throw new Exception ("Unknown Type " + objType +
							 " encountered during Serialization");

			Hashtable memberTable = (Hashtable) memberObj [0];
			XmlAttributes xmlAttributes = (XmlAttributes) memberTable [""];

			//If we have been passed an XmlRoot, set it on the base class
			if (rootAttribute != null)
				xmlAttributes.XmlRoot = rootAttribute;
			
			if (xmlAttributes.XmlRoot != null) {
				isNullable = xmlAttributes.XmlRoot.IsNullable;
				if (xmlAttributes.XmlRoot.ElementName != null)
					rootName = xmlAttributes.XmlRoot.ElementName;
				rootNs	= xmlAttributes.XmlRoot.Namespace;
			}

			if (namespaces != null && namespaces.GetPrefix (rootNs) != null)
				rootPrefix = namespaces.GetPrefix (rootNs);

			//XMLNS attributes in the Root
			XmlAttributes XnsAttrs = (XmlAttributes) ((object[]) typeTable[objType])[1];

			if (XnsAttrs != null) {
				MemberInfo member = XnsAttrs.MemberInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				PropertyInfo propertyInfo = member as PropertyInfo;
				XmlSerializerNamespaces xns;

				if (fieldInfo != null)
					xns = (XmlSerializerNamespaces) fieldInfo.GetValue (o);
				else
					xns = (XmlSerializerNamespaces) propertyInfo.GetValue (o, null);
				
				qnames = xns.ToArray ();

				foreach (XmlQualifiedName qname in qnames)
					nss.Add (qname.Name, qname.Namespace);
			}

			//XmlNs from the namespaces passed
			qnames = namespaces.ToArray ();
			foreach (XmlQualifiedName qname in qnames)
				if (writer.LookupPrefix (qname.Namespace) != qname.Name)
					nss.Add (qname.Name, qname.Namespace);

			writer.WriteStartElement (rootPrefix, rootName, rootNs);

			qnames = nss.ToArray();
			foreach (XmlQualifiedName qname in qnames)
				if (writer.LookupPrefix (qname.Namespace) != qname.Name)
					writer.WriteAttributeString ("xmlns", qname.Name, null, qname.Namespace);

			if (rootPrefix == String.Empty && rootNs != String.Empty && rootNs != null)
				writer.WriteAttributeString (String.Empty, "xmlns", null, rootNs);

			SerializeMembers (writer, o, true);//, namespaces);

			// Keep WriteState.Content state.
			// writer.WriteEndDocument ();
			writer.Flush ();
		}

		private void SerializeBuiltIn (XmlWriter writer, object o)
		{
			if (o is XmlNode) {
				XmlNode n = (XmlNode) o;
				XmlNodeReader nrdr = new XmlNodeReader (n);
				nrdr.Read ();
				if (nrdr.NodeType == XmlNodeType.XmlDeclaration)
					nrdr.Read ();
				do {
					writer.WriteNode (nrdr, false);
				} while (nrdr.Read ());
			}
			else {
				TypeData td = TypeTranslator.GetTypeData (o.GetType ());
				writer.WriteStartElement  (td.ElementName);
				WriteBuiltinValue(writer,o);
				writer.WriteEndElement();
			}
		}

		private void WriteNilAttribute(XmlWriter writer)
		{
			writer.WriteAttributeString("nil",XmlSchema.InstanceNamespace, "true");
		}

		private void WriteBuiltinValue(XmlWriter writer, object o)
		{
			if(o == null) 
				WriteNilAttribute(writer);
			else
				writer.WriteString (GetXmlValue(o));
		}

		private void SerializeMembers (XmlWriter writer, object o, bool isRoot)
		{
			if(o == null)
			{
				WriteNilAttribute(writer);
				return;
			}

			Type objType = o.GetType ();
			
			if (IsInbuiltType(objType)) 
			{
				SerializeBuiltIn (writer, o);
				return;
			}

			XmlAttributes nsAttributes = (XmlAttributes) ((object[]) typeTable [objType])[1];
			ArrayList attributes = (ArrayList) ((object[]) typeTable [objType])[2];
			ArrayList elements = (ArrayList) ((object[]) typeTable [objType])[3];

			if (!isRoot && nsAttributes != null) {
				MemberInfo member = nsAttributes.MemberInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				PropertyInfo propertyInfo = member as PropertyInfo;

				XmlSerializerNamespaces xns;

				if (fieldInfo != null)
					xns = (XmlSerializerNamespaces) fieldInfo.GetValue (o);
				else
					xns = (XmlSerializerNamespaces) propertyInfo.GetValue (o, null);
				
				XmlQualifiedName[] qnames = xns.ToArray ();
				foreach (XmlQualifiedName qname in qnames)
					if (writer.LookupPrefix (qname.Namespace) != qname.Name)
						writer.WriteAttributeString ("xmlns", qname.Name, null, qname.Namespace);
			}

			//Serialize the Attributes.
			foreach (XmlAttributes xmlAttributes in attributes) {
				MemberInfo member = xmlAttributes.MemberInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				PropertyInfo propertyInfo = member as PropertyInfo;

				Type attributeType;
				object attributeValue;
				string attributeValueString;
				string attributeName;
				string attributeNs;

				if (fieldInfo != null) {
					attributeType = fieldInfo.FieldType;
					attributeValue = fieldInfo.GetValue (o);
				} 
				else {
					attributeType = propertyInfo.PropertyType;
					attributeValue = propertyInfo.GetValue (o, null);
				}

				attributeName = xmlAttributes.GetAttributeName (attributeType, member.Name);
				attributeNs = xmlAttributes.GetAttributeNamespace (attributeType);

				if (attributeValue is XmlQualifiedName) {
					XmlQualifiedName qname = (XmlQualifiedName) attributeValue;

					if (qname.IsEmpty)
						continue;

					writer.WriteStartAttribute (attributeName, attributeNs);
					writer.WriteQualifiedName (qname.Name, qname.Namespace);
					writer.WriteEndAttribute ();
					continue;
				}
				else if (attributeValue is XmlQualifiedName[]) {
					XmlQualifiedName[] qnames = (XmlQualifiedName[]) attributeValue;
					writer.WriteStartAttribute (attributeName, attributeNs);
					int count = 0;
					foreach (XmlQualifiedName qname in qnames) {
						if (qname.IsEmpty)
							continue;
						if (count++ > 0)
							writer.WriteWhitespace (" ");
						writer.WriteQualifiedName (qname.Name, qname.Namespace);
					}
					writer.WriteEndAttribute ();
					continue;
				}
				else if (attributeValue is XmlAttribute[]) {
					XmlAttribute[] xmlattrs = (XmlAttribute[]) attributeValue;
					foreach (XmlAttribute xmlattr in xmlattrs)
						xmlattr.WriteTo(writer);
					continue;
				}

				attributeValueString = GetXmlValue (attributeValue);
				if (attributeValueString != GetXmlValue (xmlAttributes.XmlDefaultValue))
					writer.WriteAttributeString (attributeName, attributeNs, attributeValueString);
			}

			// Serialize Elements
			foreach (XmlAttributes xmlElements in elements) {
				MemberInfo member = xmlElements.MemberInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				PropertyInfo propertyInfo = member as PropertyInfo;

				Type elementType;
				object elementValue;
				string elementName;
				string elementNs;

				if (fieldInfo != null) {
					elementType = fieldInfo.FieldType;
					elementValue = fieldInfo.GetValue (o);
				}
				else {
					elementType	 = propertyInfo.PropertyType;
					elementValue = propertyInfo.GetValue (o, null);
				}
				elementName = xmlElements.GetElementName (elementType, member.Name);
				elementNs = xmlElements.GetElementNamespace (elementType);

				WriteElement (writer, xmlElements, elementName, elementNs, elementType, elementValue);
			}
		}

		[MonoTODO ("Remove FIXMEs")]
		private void WriteElement (XmlWriter writer, XmlAttributes attrs, string name, string ns, Type type, Object value)
		{
			//IF the element has XmlText Attribute, the name of the member is not serialized;
			if (attrs.XmlText != null && value != null) 
			{
				if (type == typeof (object[])) 
				{
					foreach(object obj in (object[]) value)
						writer.WriteRaw(""+obj);
				}
				else if (type == typeof (string[])) 
				{
					foreach(string str in (string[]) value)
						writer.WriteRaw(str);
				}
				else if (type == typeof (XmlNode)) 
				{
					((XmlNode) value).WriteTo (writer);
				}
				else if (type == typeof (XmlNode[])) 
				{
					XmlNode[] nodes = (XmlNode[]) value;
					foreach (XmlNode node in nodes)
						node.WriteTo (writer);
				}
				return;
			}

			//If not text, serialize as an element
			
			//Start the element tag
			writer.WriteStartElement  (name, ns);
			
			if (IsInbuiltType (type)) 
			{
				WriteBuiltinValue(writer,value);
			}
			else if (type.IsArray && value != null) 
			{
				SerializeArray (writer, value);
			}
			else if (value is ICollection) 
			{
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

				//Find a non indexer Count Property with return type of int
				PropertyInfo countInfo = type.GetProperty ("Count", flags, null, typeof (int), new Type[0], null);
				PropertyInfo itemInfo = type.GetProperty ("Item", flags, null, null, new Type[1] {typeof (int)}, null);
				int count = (int) countInfo.GetValue (value, null);

				if (count > 0) 
					for (int i = 0; i < count; i++) 
					{
						object itemValue = itemInfo.GetValue (value, new object[1] {i});
						Type   itemType	 = itemInfo.PropertyType;

						if (itemValue != null) 
						{
							string itemName = attrs.GetElementName (itemValue.GetType (), TypeTranslator.GetTypeData(itemType).ElementName);
							string itemNs = attrs.GetElementNamespace (itemValue.GetType ());

							writer.WriteStartElement (itemName, itemNs);
							SerializeMembers (writer, itemValue, false);
							writer.WriteEndElement ();
						}
					}
			}
			else if (value is IEnumerable) 
			{
				// FIXME
			}
			else if (type.IsEnum) 
			{
				writer.WriteString(GetXmlValue(value));
			}
			else
			{ //Complex Type
				SerializeMembers (writer, value, false);
			}

			// Close the Element
			writer.WriteEndElement();
		}

		//Does not take care of any array specific Xml Attributes
		[MonoTODO]
		private void SerializeArray (XmlWriter writer, object o)
		{
			Array arr = (o as Array);
			if(arr == null || arr.Rank != 1)
				throw new ApplicationException("Expected a single dimension Array, Got "+ o);

			Type arrayType = arr.GetType().GetElementType();
			string arrayTypeName = TypeTranslator.GetTypeData(arrayType).ElementName;
			
			TypeData td = TypeTranslator.GetTypeData (arrayType);

			// Special Treatment for Byte array
			if(arrayType.Equals(typeof(byte)))
			{
				WriteBuiltinValue(writer,o);
			}
			else
			{
				for(int i=0; i< arr.Length; i++)
				{
					writer.WriteStartElement (td.ElementName);
					object value = arr.GetValue(i);
					if (IsInbuiltType (arrayType)) 
					{
						WriteBuiltinValue(writer, value);
					}
					else
					{
						SerializeMembers(writer, value, false);
					}

					writer.WriteEndElement();
				}
			}
		}

		/// <summary>
		/// If the type is a string, valuetype or primitive type we do not populate the TypeTable.
		/// If the type is an array, we populate the TypeTable with Element type of the array.
		/// If the type implements ICollection, it is handled differently. We do not care for its members.
		/// If the type implements IEnumberable, we check that it implements Add(). Don't care for members.
		/// </summary>
		[MonoTODO ("Remove FIXMEs")]
		private void FillTypeTable (Type type)
		{
			// If it's already in the table, don't add it again.
			if (typeTable.Contains (type))
				return;

			//For value types and strings we don't need the members.
			//FIXME: We will need the enum types probably.
			if (IsInbuiltType (type))
				return;

			//Array, ICollection and IEnumberable are treated differenty
			if (type.IsArray) {
				FillArrayType (type);
				return;
			}
			else if (type.IsEnum) {
				FillEnum (type);
				return;
			}
			else {
				//There must be a public constructor
				//if (!HasDefaultConstructor (type))
				//throw new Exception ("Can't Serialize Type " + type.Name + " since it does not have default Constructor");

				if (type.GetInterface ("ICollection") == typeof (System.Collections.ICollection)) {
					FillICollectionType (type);
					return;
				}
//				if (type.GetInterface ("IDictionary") == typeof (System.Collections.IDictionary)) {
//					throw new Exception ("Can't Serialize Type " + type.Name + " since it implements IDictionary");
//				}
				if (type.GetInterface ("IEnumerable") == typeof (System.Collections.IEnumerable)) {
					//FillIEnumerableType(type);
					//return;
				}
			}

			
			//Add the Class to the hashtable.
			//Each value of the hashtable has two objects, one is the hashtable with key of membername (for deserialization)
			//Other is an Array of XmlSerializernames, Array of XmlAttributes & Array of XmlElements.
			Object[] memberObj = new Object[4];
			typeTable.Add (type,memberObj);

			Hashtable memberTable = new Hashtable ();
			memberObj[0] = memberTable;
			memberTable.Add ("", XmlAttributes.FromClass (type));

			memberObj[1] = null;

			ArrayList attributes = new ArrayList ();
			memberObj[2] = attributes;

			ArrayList elements = new ArrayList ();
			memberObj[3] = elements;

			//Get the graph of the members. Graph is nothing but the order
			//in which MS implementation serializes the members.
			MemberInfo[] minfo = GetGraph (type);

			foreach (MemberInfo member in minfo) {
				FieldInfo fieldInfo = (member as FieldInfo);
				PropertyInfo propertyInfo = (member as PropertyInfo);

				if (memberTable [member.Name] != null)
					continue;

				if (fieldInfo != null) {

					//If field is readOnly or const, do not serialize it.
					if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
						continue;

					XmlAttributes xmlAttributes = XmlAttributes.FromField (member, fieldInfo);

					//If XmlAttributes have XmlIgnore, ignore this member

					if (xmlAttributes.XmlIgnore)
						continue;

					//If this member is a XmlNs type, set the XmlNs object.
					if (xmlAttributes.Xmlns) {
						memberObj[1] = xmlAttributes;
						continue;
					}

					//If the member is a attribute Type, Add to attribute list
					if (xmlAttributes.isAttribute)
						attributes.Add (xmlAttributes);
					else //Add to elements
						elements.Add (xmlAttributes);

					//Add in the Hashtable.
					memberTable.Add (member.Name, xmlAttributes);
					
					if (xmlAttributes.XmlAnyAttribute != null  || xmlAttributes.XmlText != null)
						continue;

					if (xmlAttributes.XmlElements.Count > 0) {
						foreach (XmlElementAttribute elem in xmlAttributes.XmlElements) {
							if (elem.Type != null)
								FillTypeTable (elem.Type);
							else
								FillTypeTable (fieldInfo.FieldType);
						}
						continue;
					}

					if (!IsInbuiltType (fieldInfo.FieldType))
						FillTypeTable (fieldInfo.FieldType);
				} 
				else if (propertyInfo != null) {

					//If property is readonly or writeonly, do not serialize it.
					//Exceptions are properties whose return type is array, ICollection or IEnumerable
					//Indexers are not serialized unless the class Implements ICollection.
					if (!(propertyInfo.PropertyType.IsArray || 
						  Implements (propertyInfo.PropertyType, typeof (ICollection)) || 
						  (propertyInfo.PropertyType != typeof (string) && 
						   Implements (propertyInfo.PropertyType, typeof (IEnumerable)))))
					{
						if(!(propertyInfo.CanRead && propertyInfo.CanWrite) || propertyInfo.GetIndexParameters ().Length != 0)
							continue;
					}

					XmlAttributes xmlAttributes = XmlAttributes.FromProperty (member, propertyInfo);

					// If XmlAttributes have XmlIgnore, ignore this member
					if (xmlAttributes.XmlIgnore)
						continue;

					// If this member is a XmlNs type, set the XmlNs object.
					if (xmlAttributes.Xmlns) {
						memberObj[1] = xmlAttributes;
						continue;
					}
					// If the member is a attribute Type, Add to attribute list
					if (xmlAttributes.isAttribute)
						attributes.Add (xmlAttributes);
					else  //Add to elements
						elements.Add (xmlAttributes);

					// OtherWise add in the Hashtable.
					memberTable.Add (member.Name, xmlAttributes);

					if (xmlAttributes.XmlAnyAttribute != null || xmlAttributes.XmlText != null)
						continue;

					if (xmlAttributes.XmlElements.Count > 0) {
						foreach (XmlElementAttribute elem in xmlAttributes.XmlElements) {
							if (elem.Type != null)
								FillTypeTable (elem.Type);
							else
								FillTypeTable (propertyInfo.PropertyType);
						}
						continue;
					}

					if (!IsInbuiltType (propertyInfo.PropertyType))
						FillTypeTable (propertyInfo.PropertyType);
				}
			}

			// Sort the attributes for the members according to their Order
			// This is an extension to MS's Implementation and will be useful
			// if our reflection does not return the same order of elements
			// as MS .NET impl
			if (useOrder)
				BubbleSort (elements, XmlAttributes.attrComparer);
		}

		private void FillArrayType (Type type)
		{
			if (type.GetArrayRank () != 1)
				throw new Exception ("MultiDimensional Arrays are not Supported");

			Type arrayType = type.GetElementType ();

			if (arrayType.IsArray)
				FillArrayType (arrayType);
			else if (!IsInbuiltType (arrayType))
				FillTypeTable (arrayType);
		}

		private void FillICollectionType (Type type)
		{
			//Must have an public Indexer that takes an integer and
			//a public Count Property which returns an int.

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			
			//Find a non indexer Count Property with return type of int
			PropertyInfo countProp = type.GetProperty ("Count", flags, null, typeof (int), new Type[0], null);
			if (countProp == null || !countProp.CanRead)
				throw new Exception ("Cannot Serialize " + type + " because it implements ICollectoion, but does not implement public Count property");
			//Find a indexer Item Property which takes an int
			PropertyInfo itemProp = type.GetProperty ("Item", flags, null, null, new Type[1] {typeof (int)}, null);
			if (itemProp == null || !itemProp.CanRead || !itemProp.CanWrite)
				throw new Exception ("Cannot Serialize " + type + " because it does not have a read/write indexer property that takes an int as argument");
			FillTypeTable (itemProp.PropertyType);
		}

		[MonoTODO]
		private void FillIEnumerableType (Type type)
		{
			//Must implement a public Add method that takes a single parameter.
			//The Add method's parameter must be of the same type as is returned from 
			//the Current property on the value returned from GetEnumerator, or one of that type's bases.

			// We currently ignore enumerable types anyway, so this method was junked.
			// The code did not do what the documentation above says (if that is even possible!)
			return;
		}

		private void FillEnum (Type type)
		{
			Hashtable memberTable = new Hashtable ();
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			typeTable.Add (type, memberTable);
			string[] names = Enum.GetNames (type);

			foreach (string name in names) {
				MemberInfo[] members = type.GetMember (name);
				if (members.Length != 1)
					throw new Exception("Should never happen. Enum member not present or more than one. " + name);
				XmlAttributes xmlAttributes = new XmlAttributes (members[0]);

				if (xmlAttributes.XmlIgnore)
					continue;

				if (xmlAttributes.XmlEnum != null)
					memberTable.Add (members[0].Name, xmlAttributes.XmlEnum.Name);
				else
					memberTable.Add (members[0].Name, members[0].Name);
			}
		}

		private bool HasDefaultConstructor (Type type)
		{
			ConstructorInfo defaultConstructor = type.GetConstructor (new Type[0]);
			if (defaultConstructor == null || defaultConstructor.IsAbstract || defaultConstructor.IsStatic || !defaultConstructor.IsPublic)
				return false;
			
			return true;
		}

		private bool IsInbuiltType (Type type)
		{
			if (type.IsEnum)
				return false;
			if (/* type.IsValueType || */type == typeof (string) || type.IsPrimitive)
				return true;
			if (type == typeof (DateTime) || 
				type == typeof (Guid)	  ||
				type == typeof (XmlNode)  || 
				type.IsSubclassOf (typeof (XmlNode)))
				return true;
				
			return false;
		}

		private static MemberInfo[] GetGraph(Type type)
		{
			ArrayList typeGraph = new ArrayList ();
			GetGraph (type, typeGraph);
			return (MemberInfo[]) typeGraph.ToArray (typeof (MemberInfo));
		}

		private static void GetGraph (Type type, ArrayList typeGraph)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			if (type.BaseType == null)
				return;
			GetGraph (type.BaseType, typeGraph);

			typeGraph.AddRange (type.GetFields (flags));
			typeGraph.AddRange (type.GetProperties (flags));
		}

		private string GetXmlValue (object value)
		{
			if (value == null)
				return null;
			#region enum type
			if (value is Enum) 
			{
				Type type = value.GetType ();
				
				if (typeTable.ContainsKey (type)) {
					Hashtable memberTable = (Hashtable) (typeTable[type]);
					if (type.IsDefined (typeof (FlagsAttribute), false)) {
						//If value is exactly a single enum member
						if (memberTable.Contains (value.ToString ()))
							return (string) memberTable[value.ToString ()];

						string retval = "";
						int enumval = (int) value;
						string[] names = Enum.GetNames (type);

						foreach (string key in names) {
							if (!memberTable.ContainsKey (key))
								continue;

							//Otherwise multiple values.
							int val = (int) Enum.Parse (type, key);
							if (val != 0 && (enumval & val) == val)
								retval += " " + (string) memberTable[Enum.GetName (type, val)];
						}

						retval = retval.Trim ();

						if (retval.Length == 0)
							return null;

						return retval;
					}
					else if (memberTable.ContainsKey (value.ToString ()))
						return (string) memberTable[value.ToString()];
					else
						return null;
				}
				else
					throw new Exception ("Unknown Enumeration");
			}
			#endregion
			if (value is byte[])
				return XmlCustomFormatter.FromByteArrayBase64((byte[])value);
			if (value is Guid)
				return XmlConvert.ToString((Guid)value);
			if(value is DateTime)
				return XmlConvert.ToString((DateTime)value);
			if(value is TimeSpan)
				return XmlConvert.ToString((TimeSpan)value);
			if(value is bool)
				return XmlConvert.ToString((bool)value);
			if(value is byte)
				return XmlConvert.ToString((byte)value);
			if(value is char)
				return XmlCustomFormatter.FromChar((char)value);
			if(value is decimal)
				return XmlConvert.ToString((decimal)value);
			if(value is double)
				return XmlConvert.ToString((double)value);
			if(value is short)
				return XmlConvert.ToString((short)value);
			if(value is int)
				return XmlConvert.ToString((int)value);
			if(value is long)
				return XmlConvert.ToString((long)value);
			if(value is sbyte)
				return XmlConvert.ToString((sbyte)value);
			if(value is float)
				return XmlConvert.ToString((float)value);
			if(value is ushort)
				return XmlConvert.ToString((ushort)value);
			if(value is uint)
				return XmlConvert.ToString((uint)value);
			if(value is ulong)
				return XmlConvert.ToString((ulong)value);
			if (value is XmlQualifiedName) {
				if (((XmlQualifiedName) value).IsEmpty)
					return null;
			}
			return (value == null) ? null : value.ToString ();
		}

		[MonoTODO ("Remove FIXMEs")]
		private static void ProcessAttributes (XmlAttributes attrs, Hashtable memberTable)
		{
			if (attrs.XmlAnyAttribute != null) {
				// FIXME
			}
			foreach (XmlAnyElementAttribute anyelem in attrs.XmlAnyElements) 
				memberTable.Add (new XmlQualifiedName (anyelem.Name, anyelem.Namespace), attrs);

			if (attrs.XmlArray != null) {
				// FIXME
			}

			foreach (XmlArrayItemAttribute item in attrs.XmlArrayItems)
				memberTable.Add (new XmlQualifiedName (item.ElementName, item.Namespace), attrs);

			if (attrs.XmlAttribute != null)
				memberTable.Add (new XmlQualifiedName (attrs.XmlAttribute.AttributeName,attrs.XmlAttribute.Namespace), attrs);

			if (attrs.XmlChoiceIdentifier != null) {
				// FIXME
			}

			foreach (XmlElementAttribute elem in attrs.XmlElements)
				memberTable.Add (new XmlQualifiedName (elem.ElementName, elem.Namespace), attrs);

			if (attrs.XmlEnum != null) {
				// FIXME
			}

			if (attrs.XmlType != null)
				memberTable.Add (new XmlQualifiedName (attrs.XmlType.TypeName, attrs.XmlType.Namespace), attrs);
		}

		private bool Implements (Type type, Type interfaceType)
		{
			return (type.GetInterface (interfaceType.Name) == interfaceType);
		}

		private static void BubbleSort (ArrayList array, IComparer comparer)
		{
			int len = array.Count;
			object obj1, obj2;
			for (int i=0; i < len; i++) {
				for (int j=0; j < len -i -1; j++) {
					obj1 = array[j];
					obj2 = array[j+1];
					if (comparer.Compare (obj2 , obj1 ) < 0) {
						array[j] = obj2;
						array[j+1] = obj1;
					}
				}
			}
		}
		#endregion // Methods
	}
}
