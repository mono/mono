//
// Mono Class Libraries
// System.Xml.Serialization.XmlSerializer
//
// Authors:
//   John Donagher (john@webmeta.com)
//   Ajay kumar Dwivedi (adwiv@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) 2002 John Donagher, Ajay kumar Dwivedi
// Copyright (C) Tim Coleman, 2002
// 

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.Serialization {	
	/// <summary>
	/// Summary description for XmlSerializer.
	/// </summary>
	public class XmlSerializer {

		#region Fields

		Type type;
		XmlAttributeOverrides overrides;
		Type[] extraTypes;
		XmlRootAttribute rootAttribute;
		string defaultNamespace;
		static Hashtable typeTable;
		bool useOrder;
		bool isNullable;

		#endregion // Fields

		#region Constructors

		protected XmlSerializer ()
		{
		}

		public XmlSerializer (Type type)
			: this (type, null, null, null, null)
		{
		}

		[MonoTODO]
		public XmlSerializer (XmlTypeMapping xmltypemapping)
		{
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
			typeTable = typeTable;
		}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type", "XmlSerializer can't be constructed with a null type");

			this.type = type;
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

		[MonoTODO]
		public event XmlAttributeEventHandler UnknownAttribute;
		[MonoTODO]
		public event XmlElementEventHandler UnknownElement;
		[MonoTODO]
		public event XmlNodeEventHandler UnknownNode;
		[MonoTODO]
		public event UnreferencedObjectEventHandler UnreferencedObject;

		#endregion // Events

		#region Properties

		public bool UseOrder {
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
		[MonoTODO]
		public object Deserialize (Stream stream)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Deserialize (TextReader textReader)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Deserialize (XmlReader xmlReader)
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
			Serialize (xmlWriter, o);
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
			Type objType = o.GetType ();
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

			writer.WriteStartDocument ();
			
			object[] memberObj = (object[]) typeTable[objType];
			if (memberObj == null)
				throw new Exception ("Unknown Type " + objType + " encountered during Serialization");

			Hashtable memberTable = (Hashtable) memberObj[0];
			XmlAttributes xmlAttributes = (XmlAttributes) memberTable[""];

			//If we have been passed an XmlRoot, set it on the base class
			if (rootAttribute != null)
				xmlAttributes.XmlRoot = rootAttribute;
			
			if (xmlAttributes.XmlRoot != null) {
				isNullable = xmlAttributes.XmlRoot.IsNullable;
				if (xmlAttributes.XmlRoot.ElementName != null)
					rootName = xmlAttributes.XmlRoot.ElementName;
				rootNs	= xmlAttributes.XmlRoot.Namespace;
			}

			if (namespaces.GetPrefix (rootNs) != null)
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
			writer.WriteEndDocument ();
		}

		private void SerializeMembers (XmlWriter writer, object o, bool isRoot)
		{
			Type objType = o.GetType ();
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
					elementType  = propertyInfo.PropertyType;
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
			if (IsInbuiltType (type)) {
				string xmlValue = GetXmlValue (value);
				if (xmlValue != String.Empty && xmlValue != null)
					writer.WriteElementString (name, ns,  xmlValue);
			}
			else if (attrs.XmlText != null && value != null) {
				if (type == typeof (object[])) {
					// FIXME
				}
				else if (type == typeof (string[])) {
					// FIXME
				}
				else if (type == typeof (XmlNode)) {
					((XmlNode) value).WriteTo (writer);
				}
				else if (type == typeof (XmlNode[])) {
					XmlNode[] nodes = (XmlNode[]) value;
					foreach (XmlNode node in nodes)
						node.WriteTo (writer);
				}
			}
			else if (type.IsArray && value != null) {
				writer.WriteStartElement (name, ns);
				SerializeArray (writer, value);
				writer.WriteEndElement ();
			}
			else if (value is ICollection) {
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

				//Find a non indexer Count Property with return type of int
				PropertyInfo countInfo = type.GetProperty ("Count", flags, null, typeof (int), new Type[0], null);
				PropertyInfo itemInfo = type.GetProperty ("Item", flags, null, null, new Type[1] {typeof (int)}, null);
				int count = (int) countInfo.GetValue (value, null);

				if (count > 0) 
					for (int i = 0; i < count; i++) {
						object itemValue = itemInfo.GetValue (value, new object[1] {i});

						if (itemValue != null) {
							string itemName = attrs.GetElementName (itemValue.GetType (), name);
							string itemNs = attrs.GetElementNamespace (itemValue.GetType ());

							writer.WriteStartElement (itemName, itemNs);
							SerializeMembers (writer, itemValue, false);
							writer.WriteEndElement ();
						}
					}
			}
			else if (value is IEnumerable) {
				// FIXME
			}
			else if (type.IsEnum) {
				// FIXME
			}
			else if (value != null) { //Complex Type?
				string itemName = attrs.GetElementName (value.GetType (), name);
				string itemNs = attrs.GetElementNamespace (value.GetType ());
				writer.WriteStartElement (itemName, itemNs);
				SerializeMembers (writer, value, false);
				writer.WriteEndElement ();
			}
			else {
				// FIXME
			}
		}

		[MonoTODO]
		private void SerializeArray (XmlWriter writer, object o)
		{
			throw new NotImplementedException ();
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
				if (!HasDefaultConstructor (type))
					throw new Exception ("Can't Serialize Type " + type.Name + " since it does not have default Constructor");

				if (type.GetInterface ("ICollection") == typeof (System.Collections.ICollection)) {
					FillICollectionType (type);
					return;
				}
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
					if (!(propertyInfo.PropertyType.IsArray || Implements (propertyInfo.PropertyType, typeof (ICollection)) || 
						(propertyInfo.PropertyType != typeof (string) && Implements (propertyInfo.PropertyType, typeof (IEnumerable))))) {
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
			if (type.IsValueType || type == typeof (string) || type.IsPrimitive)
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

			if (value is Enum) {
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
			if (value is bool)
				return (bool) value ? "true" : "false";
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
