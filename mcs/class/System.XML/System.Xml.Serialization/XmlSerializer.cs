//
// XmlSerializer.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;
using System.Collections;
using System.Reflection;

namespace System.Xml.Serialization
{	
	/// <summary>
	/// Summary description for XmlSerializer.
	/// </summary>
	public class XmlSerializer
	{
		private Type type;
		private XmlAttributeOverrides overrides;
		private Type[] extraTypes;
		private XmlRootAttribute rootAttribute;
		private string defaultNamespace;
		private Hashtable typeTable;
		private bool useOrder;
		
		private bool isNullable;

		public bool UseOrder
		{
			get{ return  useOrder; }
			set{ useOrder = value; }
		}

		#region constructors
		protected XmlSerializer ()
		{
		}

		public XmlSerializer (Type type)
			: this(type, null, null, null, null)
		{}

		[MonoTODO]
		public XmlSerializer (XmlTypeMapping xmltypemapping)
		{}

		public XmlSerializer (Type type, string defaultNamespace)
			: this(type, null, null, null, defaultNamespace)
		{}

		public XmlSerializer (Type type, Type[] extraTypes)
			: this(type, null, extraTypes, null, null)
		{}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
			: this(type, overrides, null, null, null)
		{}

		public XmlSerializer (Type type, XmlRootAttribute root)
			: this(type, null, null, root, null)
		{}

		internal XmlSerializer(Hashtable typeTable)
		{
			this.typeTable = typeTable;
		}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		{
			if(type == null)
				throw new ArgumentNullException("type", "XmlSerializer can't be consturcted with a null type");
			this.type = type;

			this.overrides = overrides;

			this.extraTypes = (extraTypes == null ? new Type[0] : extraTypes);
			
			this.rootAttribute	= root;
			
			this.defaultNamespace = defaultNamespace;

			if(typeTable == null)
				typeTable = new Hashtable();

			FillTypeTable(type);
		}
		#endregion
		#region Events
		[MonoTODO]
		public event XmlAttributeEventHandler UnknownAttribute;
		[MonoTODO]
		public event XmlElementEventHandler UnknownElement;
		[MonoTODO]
		public event XmlNodeEventHandler UnknownNode;
		[MonoTODO]
		public event UnreferencedObjectEventHandler UnreferencedObject;
		#endregion		
		#region Deserialize
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
		#endregion
		#region Serialize Delegates
		public void Serialize (Stream stream, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize(xmlWriter, o, null);
		}
		
		public void Serialize (TextWriter textWriter, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize(xmlWriter, o, null);
		}
		
		public void Serialize (XmlWriter xmlWriter, object o)
		{
			Serialize(xmlWriter, o);
		}
		
		public void Serialize (Stream stream, object o, XmlSerializerNamespaces namespaces)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize(xmlWriter, o, namespaces);
		}
		
		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize(xmlWriter, o, namespaces);
		}
		#endregion 

		public void Serialize (XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{
			Type objType = o.GetType();
			writer.WriteStartDocument();
			if(namespaces == null)
				namespaces = new XmlSerializerNamespaces();

			Hashtable memberTable = (Hashtable)((object[])typeTable[objType])[0];

			if(memberTable == null)
				throw new Exception("Unknown Type "+objType+" encounterd during Serialization");

			XmlAttributes attrs = (XmlAttributes)memberTable[""];
			//If we have been passed an XmlRoot, set it on the base class
			if(rootAttribute != null)
				attrs.XmlRoot = rootAttribute;
			//TODO: Handle the XmlRoot Attribute
			if(attrs.XmlRoot != null)
			{
				isNullable = attrs.XmlRoot.IsNullable;
			}
			//TODO: If the type has a xmlns member, get the namespaces.
			
			writer.WriteStartElement(objType.Name);
			SerializeMembers(writer, o, namespaces);
			writer.WriteEndElement();
		}

		private void SerializeMembers ( XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{
			Type objType = o.GetType();
			XmlAttributes XnsAttrs = (XmlAttributes)((object[])typeTable[objType])[1];			
			ArrayList attrList = (ArrayList)((object[])typeTable[objType])[2];
			ArrayList elemList = (ArrayList)((object[])typeTable[objType])[3];

			//Serialize the Attributes.
			foreach(XmlAttributes attrs in attrList)
			{
				MemberInfo member = attrs.MemberInfo;
				FieldInfo fInfo = member as FieldInfo;
				PropertyInfo propInfo = member as PropertyInfo;
				Type attributeType;
				object attributeValue;
				string attributeValString;
				string attributeName;
				string attributeNs;

				if(fInfo != null)
				{
					attributeType  = fInfo.FieldType;
					attributeValue = fInfo.GetValue(o);
				} 
				else if(propInfo != null)
				{
					attributeType  = propInfo.PropertyType;
					attributeValue = propInfo.GetValue(o,null);
				}
				else
					throw new Exception("Should never Happen. Neither field or property");

				attributeName = attrs.GetAttributeName(attributeType, member.Name);
				attributeNs	  = attrs.GetAttributeNamespace(attributeType);
			
				if(attributeValue is XmlQualifiedName)
				{
					XmlQualifiedName qname = (XmlQualifiedName) attributeValue;
					if(qname.IsEmpty)
						continue;
					writer.WriteStartAttribute(attributeName, attributeNs);
					writer.WriteQualifiedName(qname.Name, qname.Namespace);
					writer.WriteEndAttribute();
					continue;
				}
				attributeValString = GetXmlValue(attributeValue);
				
				if(attributeValString != GetXmlValue(attrs.XmlDefaultValue))
				{
					writer.WriteAttributeString(attributeName, attributeNs, attributeValString);
				}
			}

			//Serialize Elements
			foreach(XmlAttributes attrs in elemList)
			{
				MemberInfo member	= attrs.MemberInfo;
				FieldInfo fInfo		= member as FieldInfo;
				PropertyInfo propInfo = member as PropertyInfo;
				Type	elementType;
				object	elementValue;
				string	elementName;
				string	elementNs;

				if(fInfo != null)
				{
					elementType = fInfo.FieldType;
					elementValue = fInfo.GetValue(o);
				}
				else if(propInfo != null)
				{
					elementType  = propInfo.PropertyType;
					elementValue = propInfo.GetValue(o,null);
				}
				else throw new Exception("should never happpen. Element is neither field nor property");

				elementName = attrs.GetElementName(elementType, member.Name);
				elementNs	= attrs.GetElementNamespace(elementType);

				WriteElement(writer, namespaces, attrs, elementName, elementNs, elementType, elementValue);
			}
		}

		private void WriteElement(XmlWriter writer, XmlSerializerNamespaces namespaces, XmlAttributes attrs, 
			string name, string ns, Type type, Object value)
		{
			if(IsInbuiltType(type))
			{
				writer.WriteElementString(name, ns,  "" + GetXmlValue(value));
			}
			else if(attrs.XmlText != null && value != null)
			{
				if(type == typeof(object[]))
				{
				}
				else if(type == typeof(string[]))
				{
				}
				else if(type == typeof(XmlNode))
				{
					((XmlNode)value).WriteTo(writer);
				}
				else if(type == typeof(XmlNode[]))
				{
					XmlNode[] nodes = (XmlNode[])value;
					foreach(XmlNode node in nodes)
						node.WriteTo(writer);
				}
			}
			else if(type.IsArray && value != null)
			{
				writer.WriteStartElement(name, ns);
				SerializeArray(writer, value, namespaces);
				writer.WriteEndElement();
			}
			else if(value is ICollection)
			{
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
				//Find a non indexer Count Property with return type of int
				PropertyInfo countProp = type.GetProperty("Count", flags, null, typeof(int),new Type[0], null);
				PropertyInfo itemProp = type.GetProperty("Item", flags, null, null, new Type[1]{typeof(int)}, null);
				int count = (int)countProp.GetValue(value,null);
				object[] items = new object[1];

				if(count > 0)
				{
					for(int i=0;i<count;i++)
					{
						items[0] = i;
						object itemval = itemProp.GetValue(value, items);
						string itemName;
						string itemNs; 
						if(itemval != null)
						{
							itemName = attrs.GetElementName(itemval.GetType(), name);
							itemNs	= attrs.GetElementNamespace(itemval.GetType());
							writer.WriteStartElement(itemName, itemNs);
							SerializeMembers(writer, itemval, namespaces);
							writer.WriteEndElement();
						}
					}
				}
			}
			else if(value is IEnumerable)
			{

			}
			else if(type.IsEnum)
			{

			}
			else if(value != null) //Complex Type?
			{
				string itemName = attrs.GetElementName(value.GetType(), name);
				string itemNs	= attrs.GetElementNamespace(value.GetType());
				writer.WriteStartElement(itemName, itemNs);
				SerializeMembers(writer, value, namespaces);
				writer.WriteEndElement();
			}
			else
			{
			}
		}

		private void SerializeArray( XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{

		}

		/// <summary>
		/// If the type is a string, valuetype or primitive type we do not populate the TypeTable.
		/// If the type is an array, we populate the TypeTable with Element type of the array.
		/// If the type implements ICollection, it is handled differently. We do not care for its members.
		/// If the type implements IEnumberable, we check that it implements Add(). Don't care for members.
		/// </summary>
		private void FillTypeTable(Type type)
		{
			if(typeTable.Contains(type))
				return;

			//For value types and strings we don't need the members.
			//FIXME: We will need the enum types probably.
			if(IsInbuiltType(type))
				return;

			//Array, ICollection and IEnumberable are treated differenty
			if(type.IsArray)
			{
				FillArrayType(type);
				return;
			}
			else if(type.IsEnum)
			{
				FillEnum(type);
				return;
			}
			else
			{
				//There must be a public constructor
				if(!HasDefaultConstructor(type))
				{
					throw new Exception("Can't Serialize Type " + type.Name + " since it does not have default Constructor");
				}

				if(type.GetInterface("ICollection") == typeof(System.Collections.ICollection))
				{
					FillICollectionType(type);
					return;
				}
				
				if(type.GetInterface("IEnumerable") == typeof(System.Collections.IEnumerable))
				{
					FillIEnumerableType(type);
					return;
				}
			}
			
			//Add the Class to the hashtable.
			//Each value of the hashtable has two objects, one is the hashtable with key of membername (for deserialization)
			//Other is an Array of XmlSerializernames, Array of XmlAttributes & Array of XmlElements.
			Object[] memberObj = new Object[4];
			typeTable.Add(type,memberObj);

			Hashtable memberTable = new Hashtable();
			memberObj[0] = memberTable;
			memberTable.Add("", XmlAttributes.FromClass(type));

			memberObj[1] = null;

			ArrayList attrList = new ArrayList();
			memberObj[2] = attrList;

			ArrayList elemList = new ArrayList();
			memberObj[3] = elemList;

			//Get the graph of the members. Graph is nothing but the order
			//in which MS implementation serializes the members.
			MemberInfo[] minfo = GetGraph(type);

			foreach(MemberInfo member in minfo)
			{
				FieldInfo fInfo = (member as FieldInfo);
				PropertyInfo propInfo = (member as PropertyInfo);

				if(fInfo != null)
				{
					//If field is readOnly or const, do not serialize it.
					if(fInfo.IsLiteral || fInfo.IsInitOnly)
						continue;

					XmlAttributes attrs  = XmlAttributes.FromField(member,fInfo);

					//If XmlAttributes have XmlIgnore, ignore this member
					if(attrs.XmlIgnore)
						continue;
					//If this member is a XmlNs type, set the XmlNs object.
					if(attrs.Xmlns)
						memberObj[1] = attrs;
					//If the member is a attribute Type, Add to attribute list
					if(attrs.isAttribute)
						attrList.Add(attrs);
					else //Add to elements
					{
						elemList.Add(attrs);
					}
					//Add in the Hashtable.
					memberTable.Add(member.Name, attrs);
					
					Type fieldType = fInfo.FieldType;
					
					if(attrs.XmlAnyAttribute != null  || attrs.XmlText != null)
						continue;

					if(attrs.XmlElements.Count > 0)
					{
						foreach(XmlElementAttribute elem in attrs.XmlElements)
						{
							if(elem.Type != null)
								FillTypeTable(elem.Type);
							else
								FillTypeTable(fieldType);
						}
						continue;
					}

					if(!IsInbuiltType(fieldType))
						FillTypeTable(fieldType);
				} 
				else if(propInfo != null)
				{
					//If property is readonly or writeonly, do not serialize it.
					//Exceptions are properties whose return type is array, ICollection or IEnumerable
					//Indexers are not serialized unless the class Implements ICollection.
					if(!(propInfo.PropertyType.IsArray || Implements(propInfo.PropertyType,typeof(ICollection)) || 
						Implements(propInfo.PropertyType,typeof(IEnumerable))))
					{
						if(!(propInfo.CanRead && propInfo.CanWrite) || propInfo.GetIndexParameters().Length != 0)
							continue;
					}
					XmlAttributes attrs  = XmlAttributes.FromProperty(member,propInfo);
					//If XmlAttributes have XmlIgnore, ignore this member
					if(attrs.XmlIgnore)
						continue;

					//If this member is a XmlNs type, set the XmlNs object.
					if(attrs.Xmlns)
					{
						memberObj[1] = attrs;
						continue;
					}
					//If the member is a attribute Type, Add to attribute list
					if(attrs.isAttribute)
						attrList.Add(attrs);
					else  //Add to elements
					{
						elemList.Add(attrs);
					}

					//OtherWise add in the Hashtable.
					memberTable.Add(member.Name, attrs);

					Type propType = propInfo.PropertyType;
					
					if(attrs.XmlAnyAttribute != null || attrs.XmlText != null)
						continue;

					if(attrs.XmlElements.Count > 0)
					{
						foreach(XmlElementAttribute elem in attrs.XmlElements)
						{
							if(elem.Type != null)
								FillTypeTable(elem.Type);
							else
								FillTypeTable(propType);
						}
						continue;
					}

					if(!IsInbuiltType(propType))
						FillTypeTable(propType);
				}
			}
			//Sort the attributes for the members according to their Order
			//This is an extension to MS's Implementation and will be useful
			//if our reflection does not return the same order of elements
			//as MS .NET impl
			if(useOrder)
				BubbleSort(elemList,XmlAttributes.attrComparer);
		}

		private void FillArrayType(Type type)
		{
			if(!type.IsArray)
				throw new Exception("Should never happen. Type is not an array");

			if(type.GetArrayRank() != 1)
				throw new Exception("MultiDimensional Arrays are not Supported");

			Type arrayType = type.GetElementType();

			if(arrayType.IsArray)
			{
				FillArrayType(arrayType);
			}
			else if(!IsInbuiltType(arrayType))
			{
				FillTypeTable(arrayType);
			}
		}

		private void FillICollectionType(Type type)
		{
			//Must have an public Indexer that takes an integer and
			//a public Count Property which returns an int.

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			
			//Find a non indexer Count Property with return type of int
			PropertyInfo countProp = type.GetProperty("Count", flags, null, typeof(int),new Type[0], null);
			if(countProp == null || !countProp.CanRead)
				throw new Exception("Cannot Serialize "+type+" because it implements ICollectoion, but does not implement public Count property");
			//Find a indexer Item Property which takes an int
			PropertyInfo itemProp = type.GetProperty("Item", flags, null, null, new Type[1]{typeof(int)}, null);
			if(itemProp == null || !itemProp.CanRead || !itemProp.CanWrite)
				throw new Exception("Cannot Serialize "+type+" because it does not have a read/write indexer property that takes an int as argument");
		}

		private void FillIEnumerableType(Type type)
		{
			//Must implement a public Add method that takes a single parameter.
			//The Add method's parameter must be of the same type as is returned from 
			// the Current property on the value returned from GetEnumerator, or one of that type's bases.
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			MethodInfo enumMethod = type.GetMethod("GetEnumerator", flags, null, new Type[0], null);
			if(enumMethod == null)
				throw new Exception("Cannot serialize "+type+" because it does not implement GetEnumerator");

			Type returnType = enumMethod.ReturnType;

			while(returnType != null)
			{
				MethodInfo addMethod = type.GetMethod("Add", flags, null, new Type[1]{returnType},null);
				if(addMethod != null)
					return;
				returnType = returnType.BaseType;
			}
			
			throw new Exception("Cannot serialize "+type+" because it does not have a Add method which takes "
					+enumMethod.ReturnType+" or one of its base types.");
		}

		private void FillEnum(Type type)
		{
			Hashtable memberTable = new Hashtable();
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			typeTable.Add(type,memberTable);
			string[] names = Enum.GetNames(type);
			foreach(string name in names)
			{
				MemberInfo[] members = type.GetMember(name);
				if(members.Length != 1)
					throw new Exception("Should never happen. Enum member not present or more than one. "+name);
				XmlAttributes attrs = new XmlAttributes(members[0]);
				if(attrs.XmlIgnore)
					continue;
				if(attrs.XmlEnum != null)
				{
					memberTable.Add(members[0].Name, attrs.XmlEnum.Name);
				}
				else
				{
					memberTable.Add(members[0].Name, members[0].Name);
				}
			}
		}

		private bool HasDefaultConstructor(Type type)
		{
			ConstructorInfo defaultConstructor = type.GetConstructor(new Type[0]);
			if(defaultConstructor == null || defaultConstructor.IsAbstract || defaultConstructor.IsStatic 
				|| !defaultConstructor.IsPublic)
				return false;
			
			return true;
		}

		private bool IsInbuiltType(Type type)
		{
			if(type.IsEnum)
				return false;
			if(type.IsValueType || type == typeof(string) || type.IsPrimitive)
				return true;
			return false;
		}

		private static MemberInfo[] GetGraph(Type type)
		{
			ArrayList typeGraph = new ArrayList();
			GetGraph(type, typeGraph);
			return (MemberInfo[]) typeGraph.ToArray(typeof(MemberInfo));
		}

		private static void GetGraph(Type type, ArrayList typeGraph)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			if(type.BaseType == null)
				return;
			GetGraph(type.BaseType,typeGraph);

			typeGraph.AddRange(type.GetFields(flags));
			typeGraph.AddRange(type.GetProperties(flags));
		}

		private string GetXmlValue(object value)
		{
			if(value == null)
				return null;
			if(value is Enum)
			{
				Type type = value.GetType();
				if(typeTable.ContainsKey(type))
				{
					Hashtable memberTable = (Hashtable)(typeTable[type]);
					if(memberTable.ContainsKey(value.ToString()))
						return (string)memberTable[value.ToString()];
					else
						return null;
				}
				else
				{
					throw new Exception("Unknown Enumeration");
				}
			}
			if(value is bool)
			{
				return (bool)value ? "true" : "false";
			}
			if(value is XmlQualifiedName)
			{
				if(((XmlQualifiedName)value).IsEmpty)
					return null;
			}
			return (value==null) ? null : value.ToString();
		}

		private static void ProcessAttributes(XmlAttributes attrs, Hashtable memberTable)
		{
			if(attrs.XmlAnyAttribute != null)
			{
			}
			foreach(XmlAnyElementAttribute anyelem in attrs.XmlAnyElements) 
			{
				memberTable.Add(new XmlQualifiedName(anyelem.Name, anyelem.Namespace), attrs);
			}
			if(attrs.XmlArray != null)
			{
			}
			foreach(XmlArrayItemAttribute item in attrs.XmlArrayItems)
			{
				memberTable.Add(new XmlQualifiedName(item.ElementName, item.Namespace), attrs);
			}
			if(attrs.XmlAttribute != null)
			{
				memberTable.Add(new XmlQualifiedName(attrs.XmlAttribute.AttributeName,attrs.XmlAttribute.Namespace), attrs);
			}
			if(attrs.XmlChoiceIdentifier != null)
			{
			}
			foreach(XmlElementAttribute elem in attrs.XmlElements)
			{
				memberTable.Add(new XmlQualifiedName(elem.ElementName, elem.Namespace), attrs);
			}
			if(attrs.XmlEnum != null)
			{
			}
			if(attrs.XmlType != null)
			{
				memberTable.Add(new XmlQualifiedName(attrs.XmlType.TypeName, attrs.XmlType.Namespace), attrs);
			}
		}

		private bool Implements(Type type, Type interfaceType)
		{
			if(type.GetInterface(interfaceType.FullName) == interfaceType)
				return true;
			return false;
		}
		private static void BubbleSort(ArrayList array, IComparer comparer)
		{
			int len = array.Count;
			object obj1, obj2;
			for (int i=0; i < len; i++) 
			{
				for (int j=0; j < len -i -1; j++)
				{
					obj1 = array[j];
					obj2 = array[j+1];
					if (comparer.Compare( obj2 , obj1 ) < 0) 
					{
						array[j] = obj2;
						array[j+1] = obj1;
					}
				}
			}
		}
	}
}