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
			
			//If required, you can specify which field/Property of the 'type' to use as root.
			//This means only one field/Property of the baseType will be serialized.
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

			Hashtable memberTable = (Hashtable)((object[])typeTable[objType])[0];
			if(memberTable == null)
				throw new Exception("Unknown Type "+objType+" encounterd during Serialization");
			XmlAttributes attrs = (XmlAttributes)memberTable[""];
			//If we have been passed an XmlRoot, set it on the base class
			if(rootAttribute != null)
				attrs.XmlRoot = rootAttribute;
			//TODO: Handle the XmlRoot Attribute

			//TODO: If the type has a xmlns member, get the namespaces.
			
			writer.WriteStartElement(objType.Name);
			SerializeMembers(writer, o, namespaces);
			writer.WriteEndElement();
		}

		private void SerializeMembers ( XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{
			Type objType = o.GetType();
			ArrayList attrList = (ArrayList)((object[])typeTable[objType])[2];
			ArrayList elemList = (ArrayList)((object[])typeTable[objType])[3];

			//Serialize the Attributes.
			
			foreach(XmlAttributes attrs in attrList)
			{
				string memberName = attrs.ElementName;
				if(memberName == string.Empty)
					continue;

				MemberInfo member = attrs.MemberInfo;
				if((member.MemberType & MemberTypes.Field) != 0)
				{
					FieldInfo fInfo = attrs.FieldInfo;
					writer.WriteAttributeString(attrs.ElementName, null, ""+fInfo.GetValue(o));
				} 
				else if((member.MemberType & MemberTypes.Property) != 0)
				{
					PropertyInfo propInfo = attrs.PropertyInfo;
					writer.WriteAttributeString(attrs.ElementName, null, ""+propInfo.GetValue(o,null));
				}
			}

			//Serialize Elements
			foreach(XmlAttributes attrs in elemList)
			{
				string memberName = attrs.ElementName;
				if(memberName == string.Empty)
					continue;

				MemberInfo member = attrs.MemberInfo;
				if((member.MemberType & MemberTypes.Field) != 0)
				{
					FieldInfo fInfo = attrs.FieldInfo;
					if(IsInbuiltType(fInfo.FieldType))
					{
						writer.WriteElementString(memberName, "" + fInfo.GetValue(o));
					}
					else if(fInfo.FieldType.IsArray)
					{
						//						Type arrayType = fInfo.FieldType.GetElementType();
						//						Array arr = (Array)(fInfo.GetValue(o));
						//						if(arr != null)
						//						{
						//							for(int i=0;i < arr.Length; i++)
						//								writer.WriteElementString(arrayType.Name, "" + arr.GetValue(i));
						//						}
					}
					else//Complex Type?
					{
						writer.WriteStartElement(fInfo.Name);
						SerializeMembers(writer, fInfo.GetValue(o), namespaces);
						writer.WriteEndElement();
					}
				} 
				else if((member.MemberType & MemberTypes.Property) != 0)
				{
					PropertyInfo propInfo = attrs.PropertyInfo;
					if(IsInbuiltType(propInfo.PropertyType))
					{
						writer.WriteElementString(memberName, "" + propInfo.GetValue(o,null));
					}
					else if(propInfo.PropertyType.IsArray)
					{
					}
					else //Complex Type?
					{
						writer.WriteStartElement(propInfo.Name);
						SerializeMembers(writer, propInfo.GetValue(o,null), namespaces);
						writer.WriteEndElement();
					}
				}
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
				if((member.MemberType & MemberTypes.Field) != 0)
				{
					FieldInfo fInfo = type.GetField(member.Name);
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
						elemList.Add(attrs);

					//Add in the Hashtable.
					memberTable.Add(member.Name, attrs);
					
					Type fieldType = fInfo.FieldType;

					if(!IsInbuiltType(fieldType))
						FillTypeTable(fieldType);
				} 
				else if((member.MemberType & MemberTypes.Property) != 0)
				{
					PropertyInfo propInfo = type.GetProperty(member.Name);
					//If property is readonly or writeonly, do not serialize it.
					if(!(propInfo.CanRead && propInfo.CanWrite))
						continue;

					XmlAttributes attrs  = XmlAttributes.FromProperty(member,propInfo);
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
						elemList.Add(attrs);

					//OtherWise add in the Hashtable.
					memberTable.Add(member.Name, attrs);

					Type propType = propInfo.PropertyType;

					if(!IsInbuiltType(propType))
						FillTypeTable(propType);
				}
				continue;
			}
			//Sort the attributes for the members according to their Order
			if(useOrder)
				elemList.Sort(XmlAttributes.attrComparer);
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

		}

		private void FillIEnumerableType(Type type)
		{

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
			if(type.IsValueType || type == typeof(string) || type.IsPrimitive)
				return true;
			return false;
		}

		public static MemberInfo[] GetGraph(Type type)
		{
			ArrayList typeGraph = new ArrayList();
			GetGraph(type, typeGraph);
			return (MemberInfo[]) typeGraph.ToArray(typeof(MemberInfo));
		}

		public static void GetGraph(Type type, ArrayList typeGraph)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			if(type.BaseType == null)
				return;
			GetGraph(type.BaseType,typeGraph);

			typeGraph.AddRange(type.GetFields(flags));
			typeGraph.AddRange(type.GetProperties(flags));
		}
	}
}