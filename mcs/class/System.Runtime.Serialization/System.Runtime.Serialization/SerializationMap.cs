//
// SerializationMap.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
//	Duncan Mak (duncan@ximian.com)
//	Eyal Alaluf (eyala@mainsoft.com)
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
/*
	XmlFormatter implementation design inference:

	type definitions:
	- No XML Schema types are directly used. There are some maps from
	  xs:blahType to ms:blahType where the namespaceURI for prefix "ms" is
	  "http://schemas.microsoft.com/2003/10/Serialization/" .

	serializable types:
	- An object being serialized 1) must be of type System.Object, or
	  2) must be null, or 3) must have either a [DataContract] attribute
	  or a [Serializable] attribute to be serializable.
	- When the object is either of type System.Object or null, then the
	  XML type is "anyType".
	- When the object is [Serializable], then the runtime-serialization
	  compatible object graph is written.
	- Otherwise the serialization is based on contract attributes.
	  ([Serializable] takes precedence).

	type derivation:
	- For type A to be serializable, the base type B of A must be
	  serializable.
	- If a type which is [Serializable] and whose base type has a
	  [DataContract], then for base type members [DataContract] is taken.
	- It is vice versa i.e. if the base type is [Serializable] and the
	  derived type has a [DataContract], then [Serializable] takes place
	  for base members.

	known type collection:
	- It internally manages mapping store keyed by contract QNames.
	  KnownTypeCollection.Add() checks if the same QName contract already
	  exists (and raises InvalidOperationException if required).

*/
	internal abstract partial class SerializationMap
	{
		public const BindingFlags AllInstanceFlags =
			BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance;

		public readonly KnownTypeCollection KnownTypes;
		public readonly Type RuntimeType;
		public bool IsReference; // new in 3.5 SP1
		public List<DataMemberInfo> Members;
#if !NET_2_1
		XmlSchemaSet schema_set;
#endif
 
		//FIXME FIXME
		Dictionary<Type, QName> qname_table = new Dictionary<Type, QName> ();

		protected SerializationMap (
			Type type, QName qname, KnownTypeCollection knownTypes)
		{
			KnownTypes = knownTypes;
			RuntimeType = type;
			if (qname.Namespace == null)
				qname = new QName (qname.Name,
					KnownTypeCollection.DefaultClrNamespaceBase + type.Namespace);

			XmlName = qname;
			Members = new List<DataMemberInfo> ();
		}

		public virtual bool OutputXsiType {
			get { return true; }
		}

		public QName XmlName { get; set; }

		public CollectionDataContractAttribute GetCollectionDataContractAttribute (Type type)
		{
			object [] atts = type.GetCustomAttributes (
				typeof (CollectionDataContractAttribute), false);
			return atts.Length == 0 ? null : (CollectionDataContractAttribute) atts [0];
		}

		public DataMemberAttribute GetDataMemberAttribute (
			MemberInfo mi)
		{
			object [] atts = mi.GetCustomAttributes (
				typeof (DataMemberAttribute), false);
			if (atts.Length == 0)
				return null;
			return (DataMemberAttribute) atts [0];
		}

		bool IsPrimitive (Type type)
		{
			return (Type.GetTypeCode (type) != TypeCode.Object || type == typeof (object));
		}

#if !NET_2_1
		/* Returns the XmlSchemaType AND adds it to @schemas */
		public virtual XmlSchemaType GetSchemaType (XmlSchemaSet schemas, Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			if (IsPrimitive (RuntimeType))
				return null;

			if (generated_schema_types.ContainsKey (XmlName)) // Caching  
				return generated_schema_types [XmlName] as XmlSchemaType;

			XmlSchemaComplexType complex_type = null;

			complex_type = new XmlSchemaComplexType ();
			complex_type.Name = XmlName.Name;
			generated_schema_types [XmlName] = complex_type;

			if (RuntimeType.BaseType == typeof (object)) {
				complex_type.Particle = GetSequence (schemas, generated_schema_types);
			} else {
				//Has a non-System.Object base class
				XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension ();
				XmlSchemaComplexContent content = new XmlSchemaComplexContent ();

				complex_type.ContentModel = content;
				content.Content = extension;

				KnownTypes.Add (RuntimeType.BaseType);
				SerializationMap map = KnownTypes.FindUserMap (RuntimeType.BaseType);
				//FIXME: map == null ?
				map.GetSchemaType (schemas, generated_schema_types);

				extension.Particle = GetSequence (schemas, generated_schema_types);
				extension.BaseTypeName = GetQualifiedName (RuntimeType.BaseType);
			}
			
			XmlSchemaElement schemaElement = GetSchemaElement (XmlName, complex_type);
			XmlSchema schema = GetSchema (schemas, XmlName.Namespace);
			schema.Items.Add (complex_type);
			schema.Items.Add (schemaElement);
			schemas.Reprocess (schema);

			return complex_type;
		}

		/* Returns the <xs:sequence> for the data members */
		XmlSchemaSequence GetSequence (XmlSchemaSet schemas,
				Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			List<DataMemberInfo> members = GetMembers ();

			XmlSchema schema = GetSchema (schemas, XmlName.Namespace);
			XmlSchemaSequence sequence = new XmlSchemaSequence ();
			foreach (DataMemberInfo dmi in members) {
				// delegates are not supported.
				if (!dmi.MemberType.IsAbstract && typeof (System.Delegate).IsAssignableFrom (dmi.MemberType))
					continue;

				XmlSchemaElement element = new XmlSchemaElement ();
				element.Name = dmi.XmlName;

				KnownTypes.Add (dmi.MemberType);
				SerializationMap map = KnownTypes.FindUserMap (dmi.MemberType);
				if (map != null) {
					XmlSchemaType schema_type = map.GetSchemaType (schemas, generated_schema_types);
					if (schema_type is XmlSchemaComplexType)
						element.IsNillable = true;
				} else {
					//Primitive type
					if (dmi.MemberType == typeof (string))
						element.IsNillable = true;
				}

				element.MinOccurs = 0;

				element.SchemaTypeName = GetQualifiedName (dmi.MemberType);
				AddImport (schema, element.SchemaTypeName.Namespace);

				sequence.Items.Add (element);
			}

			schemas.Reprocess (schema);
			return sequence;
		}

		//FIXME: Replace with a dictionary ?
		void AddImport (XmlSchema schema, string ns)
		{
			if (ns == XmlSchema.Namespace || schema.TargetNamespace == ns)
				return;

			foreach (XmlSchemaObject o in schema.Includes) {
				XmlSchemaImport import = o as XmlSchemaImport;
				if (import == null)
					continue;
				if (import.Namespace == ns)
					return;
			}

			XmlSchemaImport imp = new XmlSchemaImport ();
			imp.Namespace = ns;
			schema.Includes.Add (imp);
		}
#endif

		//Returns list of data members for this type ONLY
		public virtual List<DataMemberInfo> GetMembers ()
		{
			throw new NotImplementedException (String.Format ("Implement me for {0}", this));
		}

#if !NET_2_1
		protected XmlSchemaElement GetSchemaElement (QName qname, XmlSchemaType schemaType)
		{
			XmlSchemaElement schemaElement = new XmlSchemaElement ();
			schemaElement.Name = qname.Name;
			schemaElement.SchemaTypeName = qname;

			if (schemaType is XmlSchemaComplexType)
				schemaElement.IsNillable = true;

			return schemaElement;
		}

		protected XmlSchema GetSchema (XmlSchemaSet schemas, string ns)
		{
			ICollection colln = schemas.Schemas (ns);
			if (colln.Count > 0) {
				if (colln.Count > 1)
					throw new Exception (String.Format (
						"More than 1 schema for namespace '{0}' found.", ns));
				foreach (object o in colln)
					//return colln [0]
					return (o as XmlSchema);
			}

			XmlSchema schema = new XmlSchema ();
			schema.TargetNamespace = ns;
			schema.ElementFormDefault = XmlSchemaForm.Qualified;
			schemas.Add (schema);

			return schema;
		}

		protected XmlQualifiedName GetQualifiedName (Type type)
		{
			if (qname_table.ContainsKey (type))
				return qname_table [type];

			QName qname = KnownTypes.GetQName (type);
			if (qname.Namespace == KnownTypeCollection.MSSimpleNamespace)
				qname = new QName (qname.Name, XmlSchema.Namespace);

			qname_table [type] = qname;
			return qname;
		}
#endif

		public virtual void Serialize (object graph,
			XmlFormatterSerializer serializer)
		{
			string label;
			if (serializer.TrySerializeAsReference (IsReference, graph, out label))
				return;
			else if (serializer.SerializingObjects.Contains (graph))
				throw new SerializationException (String.Format ("Circular reference of an object in the object graph was found: '{0}' of type {1}", graph, graph.GetType ()));
			serializer.SerializingObjects.Add (graph);

			if (label != null)
				serializer.Writer.WriteAttributeString ("z", "Id", KnownTypeCollection.MSSimpleNamespace, label);

			SerializeNonReference (graph, serializer);

			serializer.SerializingObjects.Remove (graph);
		}

		public virtual void SerializeNonReference (object graph,
			XmlFormatterSerializer serializer)
		{
			foreach (DataMemberInfo dmi in Members) {
				FieldInfo fi = dmi.Member as FieldInfo;
				PropertyInfo pi = fi == null ?
					(PropertyInfo) dmi.Member : null;
				Type type = fi != null ?
					fi.FieldType : pi.PropertyType;
				object value = fi != null ?
					fi.GetValue (graph) :
					pi.GetValue (graph, null);

				serializer.WriteStartElement (dmi.XmlName, dmi.XmlRootNamespace, dmi.XmlNamespace);
				serializer.Serialize (type, value);
				serializer.WriteEndElement ();
			}
		}

		public virtual object DeserializeObject (XmlReader reader, XmlFormatterDeserializer deserializer)
		{
			bool isEmpty = reader.IsEmptyElement;
			reader.ReadStartElement ();
			reader.MoveToContent ();

			object res;

			if (isEmpty)
				res = DeserializeEmptyContent (reader, deserializer);
			else
				res = DeserializeContent (reader, deserializer);

			reader.MoveToContent ();
			if (!isEmpty && reader.NodeType == XmlNodeType.EndElement)
				reader.ReadEndElement ();
			else if (!isEmpty && reader.NodeType != XmlNodeType.None) {
				var li = reader as IXmlLineInfo;
				throw new SerializationException (String.Format ("Deserializing type '{3}'. Expecting state 'EndElement'. Encountered state '{0}' with name '{1}' with namespace '{2}'.{4}",
					reader.NodeType,
					reader.Name,
					reader.NamespaceURI,
					RuntimeType.FullName,
					li != null && li.HasLineInfo () ? String.Format (" {0}({1},{2})", reader.BaseURI, li.LineNumber, li.LinePosition) : String.Empty));
			}
			return res;
		}

		// This is sort of hack. The argument reader already moved ahead of
		// the actual empty element.It's just for historical consistency.
		public virtual object DeserializeEmptyContent (XmlReader reader,
			XmlFormatterDeserializer deserializer)
		{
			return DeserializeContent (reader, deserializer, true);
		}

		public virtual object DeserializeContent (XmlReader reader,
			XmlFormatterDeserializer deserializer)
		{
			return DeserializeContent (reader, deserializer, false);
		}

		object DeserializeContent (XmlReader reader,
			XmlFormatterDeserializer deserializer, bool empty)
		{
			object instance = FormatterServices.GetUninitializedObject (RuntimeType);
			int depth = reader.NodeType == XmlNodeType.None ? reader.Depth : reader.Depth - 1;
			bool [] filled = new bool [Members.Count];
			int memberInd = -1, ordered = -1;
			while (!empty && reader.NodeType == XmlNodeType.Element && reader.Depth > depth) {
				DataMemberInfo dmi = null;
				int i = 0;
				for (; i < Members.Count; i++) { // unordered
					if (Members [i].Order >= 0)
						break;
					if (reader.LocalName == Members [i].XmlName &&
						(Members [i].XmlRootNamespace == null || reader.NamespaceURI == Members [i].XmlRootNamespace)) {
						memberInd = i;
						dmi = Members [i];
						break;
					}
				}
				for (i = Math.Max (i, ordered); i < Members.Count; i++) { // ordered
					if (dmi != null)
						break;
					if (reader.LocalName == Members [i].XmlName &&
						(Members [i].XmlRootNamespace == null || reader.NamespaceURI == Members [i].XmlRootNamespace)) {
						memberInd = i;
						ordered = i;
						dmi = Members [i];
						break;
					}
				}

				if (dmi == null) {
					reader.Skip ();
					continue;
				}
				SetValue (dmi, instance, deserializer.Deserialize (dmi.MemberType, reader));
				filled [memberInd] = true;
				reader.MoveToContent ();
			}
			for (int i = 0; i < Members.Count; i++)
				if (!filled [i] && Members [i].IsRequired)
					throw MissingRequiredMember (Members [i], reader);

			return instance;
		}

		// For now it could be private.
		protected Exception MissingRequiredMember (DataMemberInfo dmi, XmlReader reader)
		{
			var li = reader as IXmlLineInfo;
			return new ArgumentException (String.Format ("Data contract member {0} for the type {1} is required, but missing in the input XML.{2}",
				new QName (dmi.XmlName, dmi.XmlNamespace),
				RuntimeType,
				li != null && li.HasLineInfo () ? String.Format (" {0}({1},{2})", reader.BaseURI, li.LineNumber, li.LinePosition) : null));
		}

		// For now it could be private.
		protected void SetValue (DataMemberInfo dmi, object obj, object value)
		{
			try {
				if (dmi.Member is PropertyInfo)
					((PropertyInfo) dmi.Member).SetValue (obj, value, null);
				else
					((FieldInfo) dmi.Member).SetValue (obj, value);
			} catch (Exception ex) {
				throw new InvalidOperationException (String.Format ("Failed to set value of type {0} for property {1}", value != null ? value.GetType () : null, dmi.Member), ex);
			}
		}

		protected DataMemberInfo CreateDataMemberInfo (DataMemberAttribute dma, MemberInfo mi, Type type)
		{
			KnownTypes.Add (type);
			QName qname = KnownTypes.GetQName (type);
			string rootNamespace = KnownTypes.GetQName (mi.DeclaringType).Namespace;
			if (KnownTypeCollection.GetPrimitiveTypeFromName (qname.Name) != null)
				return new DataMemberInfo (mi, dma, rootNamespace, null);
			else
				return new DataMemberInfo (mi, dma, rootNamespace, qname.Namespace);
		}
	}

	internal partial class XmlSerializableMap : SerializationMap
	{
		public XmlSerializableMap (Type type, QName qname, KnownTypeCollection knownTypes)
			: base (type, qname, knownTypes)
		{
		}

		public override void Serialize (object graph, XmlFormatterSerializer serializer)
		{
			IXmlSerializable ixs = graph as IXmlSerializable;
			if (ixs == null)
				//FIXME: Throw what exception here?
				throw new SerializationException ();

			ixs.WriteXml (serializer.Writer);
		}

		public override object DeserializeObject (XmlReader reader, XmlFormatterDeserializer deserializer)
		{
#if NET_2_1
			IXmlSerializable ixs = Activator.CreateInstance (RuntimeType);
#else
			IXmlSerializable ixs = (IXmlSerializable) Activator.CreateInstance (RuntimeType, true);
#endif
			ixs.ReadXml (reader);
			return ixs;
		}

#if !NET_2_1
		// FIXME: verify return value sanity.
		public override XmlSchemaType GetSchemaType (XmlSchemaSet schemas, Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			return null;
		}
#endif
	}

	internal partial class SharedContractMap : SerializationMap
	{
		public SharedContractMap (
			Type type, QName qname, KnownTypeCollection knownTypes)
			: base (type, qname, knownTypes)
		{
		}

		internal void Initialize ()
		{
			Type baseType = RuntimeType;
			List <DataMemberInfo> members = new List <DataMemberInfo> ();
			object [] atts = baseType.GetCustomAttributes (
				typeof (DataContractAttribute), false);
			IsReference = atts.Length > 0 ? (((DataContractAttribute) atts [0]).IsReference) : false;

			while (baseType != null) {
				QName bqname = KnownTypes.GetQName (baseType);
					
				members = GetMembers (baseType, bqname, true);
				members.Sort (DataMemberInfo.DataMemberInfoComparer.Instance);
				Members.InsertRange (0, members);
				members.Clear ();

				baseType = baseType.BaseType;
			}
		}

		List<DataMemberInfo> GetMembers (Type type, QName qname, bool declared_only)
		{
			List<DataMemberInfo> data_members = new List<DataMemberInfo> ();
			BindingFlags flags = AllInstanceFlags;
			if (declared_only)
				flags |= BindingFlags.DeclaredOnly;

			foreach (PropertyInfo pi in type.GetProperties (flags)) {
				DataMemberAttribute dma =
					GetDataMemberAttribute (pi);
				if (dma == null)
					continue;
				KnownTypes.TryRegister (pi.PropertyType);
				var map = KnownTypes.FindUserMap (pi.PropertyType);
				if (!pi.CanRead || (!pi.CanWrite && !(map is ICollectionTypeMap)))
					throw new InvalidDataContractException (String.Format (
							"DataMember property '{0}' on type '{1}' must have both getter and setter.", pi, pi.DeclaringType));
				data_members.Add (CreateDataMemberInfo (dma, pi, pi.PropertyType));
			}

			foreach (FieldInfo fi in type.GetFields (flags)) {
				DataMemberAttribute dma =
					GetDataMemberAttribute (fi);
				if (dma == null)
					continue;
				data_members.Add (CreateDataMemberInfo (dma, fi, fi.FieldType));
			}

			return data_members;
		}

		public override List<DataMemberInfo> GetMembers ()
		{
			return Members;
		}
	}

	internal partial class DefaultTypeMap : SerializationMap
	{
		public DefaultTypeMap (Type type, KnownTypeCollection knownTypes)
			: base (type, KnownTypeCollection.GetStaticQName (type), knownTypes)
		{
			Members.AddRange (GetDefaultMembers ());
		}

		List<DataMemberInfo> GetDefaultMembers ()
		{
			var l = new List<DataMemberInfo> ();
			foreach (var mi in RuntimeType.GetMembers ()) {
				Type mt = null;
				FieldInfo fi = mi as FieldInfo;
				mt = fi == null ? null : fi.FieldType;
				PropertyInfo pi = mi as PropertyInfo;
				if (pi != null && pi.CanRead && pi.CanWrite && pi.GetIndexParameters ().Length == 0)
					mt = pi.PropertyType;
				if (mt == null)
					continue;
				if (mi.GetCustomAttributes (typeof (IgnoreDataMemberAttribute), false).Length != 0)
					continue;
				l.Add (new DataMemberInfo (mi, new DataMemberAttribute (), null, null));
			}
			l.Sort (DataMemberInfo.DataMemberInfoComparer.Instance);
			return l;
		}
	}

	// FIXME: it still needs to consider ItemName/KeyName/ValueName
	// (especially Dictionary collection is not likely considered yet.)
	internal partial class CollectionContractTypeMap : CollectionTypeMap
	{
		CollectionDataContractAttribute a;

		public CollectionContractTypeMap (
			Type type, CollectionDataContractAttribute a, Type elementType,
			QName qname, KnownTypeCollection knownTypes)
			: base (type, elementType, qname, knownTypes)
		{
			this.a = a;
			IsReference = a.IsReference;
		}

		internal override string CurrentNamespace {
			get { return XmlName.Namespace; }
		}
	}

	internal interface ICollectionTypeMap
	{
	}

	internal partial class CollectionTypeMap : SerializationMap, ICollectionTypeMap
	{
		Type element_type;
		internal QName element_qname;
		MethodInfo add_method;

		public CollectionTypeMap (
			Type type, Type elementType,
			QName qname, KnownTypeCollection knownTypes)
			: base (type, qname, knownTypes)
		{
			element_type = elementType;
			element_qname = KnownTypes.GetQName (element_type);
			var icoll = GetGenericCollectionInterface (RuntimeType);
			if (icoll != null) {
				if (RuntimeType.IsInterface) {
					add_method = RuntimeType.GetMethod ("Add", icoll.GetGenericArguments ());
				} else {
					var imap = RuntimeType.GetInterfaceMap (icoll);
					for (int i = 0; i < imap.InterfaceMethods.Length; i++)
						if (imap.InterfaceMethods [i].Name == "Add") {
							add_method = imap.TargetMethods [i];
							break;
						}
					if (add_method == null)
						add_method = type.GetMethod ("Add", icoll.GetGenericArguments ());
				}
			}
		}

		static Type GetGenericCollectionInterface (Type type)
		{
			foreach (var iface in type.GetInterfaces ())
				if (iface.IsGenericType && iface.GetGenericTypeDefinition () == typeof (ICollection<>))
					return iface;

			return null;
		}

		public override bool OutputXsiType {
			get { return false; }
		}

		internal virtual string CurrentNamespace {
			get {
				string ns = element_qname.Namespace;
				if (ns == KnownTypeCollection.MSSimpleNamespace)
					ns = KnownTypeCollection.MSArraysNamespace;
				return ns;
			}
		}

		public override void SerializeNonReference (object graph,
			XmlFormatterSerializer serializer)
		{

			foreach (object o in (IEnumerable) graph) {
				serializer.WriteStartElement (element_qname.Name, XmlName.Namespace, CurrentNamespace);
				serializer.Serialize (element_type, o);
				serializer.WriteEndElement ();
			}
		}

		object CreateInstance ()
		{
			if (RuntimeType.IsArray)
				return new ArrayList ();
			if (RuntimeType.IsInterface) {
				var icoll = GetGenericCollectionInterface (RuntimeType);
				if (icoll != null)
					return Activator.CreateInstance (typeof (List<>).MakeGenericType (RuntimeType.GetGenericArguments () [0])); // List<T>
				else // non-generic
					return new ArrayList ();
			}
#if NET_2_1 // FIXME: is it fine?
			return Activator.CreateInstance (RuntimeType);
#else
			return Activator.CreateInstance (RuntimeType, true);
#endif
		}

		public override object DeserializeEmptyContent (XmlReader reader, XmlFormatterDeserializer deserializer)
		{
			var instance = CreateInstance ();
			if (RuntimeType.IsArray)
				return ((ArrayList)instance).ToArray (element_type);
			else
				return instance;
		}

		public override object DeserializeContent (XmlReader reader, XmlFormatterDeserializer deserializer)
		{
			object instance = CreateInstance ();
			int depth = reader.NodeType == XmlNodeType.None ? reader.Depth : reader.Depth - 1;
			while (reader.NodeType == XmlNodeType.Element && reader.Depth > depth) {
				object elem = deserializer.Deserialize (element_type, reader);
				if (instance is IList)
					((IList)instance).Add (elem);
				else if (add_method != null)
					add_method.Invoke (instance, new object [] {elem});
				else
					throw new NotImplementedException (String.Format ("Type {0} is not supported", RuntimeType));
				reader.MoveToContent ();
			}
			if (RuntimeType.IsArray)
				return ((ArrayList)instance).ToArray (element_type);
			return instance;
		}

		public override List<DataMemberInfo> GetMembers ()
		{
			//Shouldn't come here at all!
			throw new NotImplementedException ();
		}
		
#if !NET_2_1
		public override XmlSchemaType GetSchemaType (XmlSchemaSet schemas, Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			if (generated_schema_types.ContainsKey (XmlName))
				return null;

			if (generated_schema_types.ContainsKey (XmlName))
				return generated_schema_types [XmlName];

			QName element_qname = GetQualifiedName (element_type);

			XmlSchemaComplexType complex_type = new XmlSchemaComplexType ();
			complex_type.Name = XmlName.Name;

			XmlSchemaSequence sequence = new XmlSchemaSequence ();
			XmlSchemaElement element = new XmlSchemaElement ();

			element.MinOccurs = 0;
			element.MaxOccursString = "unbounded";
			element.Name = element_qname.Name;

			KnownTypes.Add (element_type);
			SerializationMap map = KnownTypes.FindUserMap (element_type);
			if (map != null) {// non-primitive type
				map.GetSchemaType (schemas, generated_schema_types);
				element.IsNillable = true;
			}

			element.SchemaTypeName = element_qname;

			sequence.Items.Add (element);
			complex_type.Particle = sequence;

			XmlSchema schema = GetSchema (schemas, XmlName.Namespace);
			schema.Items.Add (complex_type);
			schema.Items.Add (GetSchemaElement (XmlName, complex_type));
			schemas.Reprocess (schema);

			generated_schema_types [XmlName] = complex_type;

			return complex_type;
		}
#endif
	}

	internal partial class DictionaryTypeMap : SerializationMap, ICollectionTypeMap
	{
		Type key_type, value_type;
		QName item_qname, key_qname, value_qname;
		MethodInfo add_method;
		CollectionDataContractAttribute a;

		public DictionaryTypeMap (
			Type type, CollectionDataContractAttribute a, KnownTypeCollection knownTypes)
			: base (type, QName.Empty, knownTypes)
		{
			this.a = a;

			key_type = typeof (object);
			value_type = typeof (object);

			var idic = GetGenericDictionaryInterface (RuntimeType);
			if (idic != null) {
				var imap = RuntimeType.GetInterfaceMap (idic);
				for (int i = 0; i < imap.InterfaceMethods.Length; i++)
					if (imap.InterfaceMethods [i].Name == "Add") {
						add_method = imap.TargetMethods [i];
						break;
					}
				var argtypes = idic.GetGenericArguments();
				key_type = argtypes [0];
				value_type = argtypes [1];
				if (add_method == null)
					add_method = type.GetMethod ("Add", argtypes);
			}

			XmlName = GetDictionaryQName ();
			item_qname = GetItemQName ();
			key_qname = GetKeyQName ();
			value_qname = GetValueQName ();
		}

		static Type GetGenericDictionaryInterface (Type type)
		{
			foreach (var iface in type.GetInterfaces ())
				if (iface.IsGenericType && iface.GetGenericTypeDefinition () == typeof (IDictionary<,>))
					return iface;

			return null;
		}

		string ContractNamespace {
			get { return a != null && !String.IsNullOrEmpty (a.Namespace) ? a.Namespace : KnownTypeCollection.MSArraysNamespace; }
		}

		public Type KeyType { get { return key_type; } }
		public Type ValueType { get { return value_type; } }

		internal virtual QName GetDictionaryQName ()
		{
			string name = a != null ? a.Name : null;
			string ns = a != null ? a.Namespace : null;
			if (RuntimeType.IsGenericType && RuntimeType.GetGenericTypeDefinition () != typeof (Dictionary<,>))
				name = name ?? KnownTypeCollection.GetDefaultName (RuntimeType);
			else
				name = "ArrayOf" + GetItemQName ().Name;
			ns = ns ?? KnownTypeCollection.MSArraysNamespace;

			return new QName (name, ns);
		}

		internal virtual QName GetItemQName ()
		{
			string name = a != null ? a.ItemName : null;
			string ns = a != null ? a.Namespace : null;

			name = name ?? "KeyValueOf" + KnownTypes.GetQName (key_type).Name + KnownTypes.GetQName (value_type).Name;
			ns = ns ?? (a != null ? ContractNamespace : KnownTypeCollection.MSArraysNamespace);

			return new QName (name, ns);
		}

		internal virtual QName GetKeyQName ()
		{
			string name = a != null ? a.KeyName : null;
			string ns = a != null ? a.Namespace : null;

			name = name ?? "Key";
			ns = ns ?? (a != null ? ContractNamespace : KnownTypeCollection.MSArraysNamespace);
			return new QName (name, ns);
		}

		internal virtual QName GetValueQName ()
		{
			string name = a != null ? a.ValueName : null;
			string ns = a != null ? a.Namespace : null;

			name = name ?? "Value";
			ns = ns ?? (a != null ? ContractNamespace : KnownTypeCollection.MSArraysNamespace);
			return new QName (name, ns);
		}

		internal virtual string CurrentNamespace {
			get {
				string ns = item_qname.Namespace;
				if (ns == KnownTypeCollection.MSSimpleNamespace)
					ns = KnownTypeCollection.MSArraysNamespace;
				return ns;
			}
		}

		Type pair_type;
		PropertyInfo pair_key_property, pair_value_property;

		public override void SerializeNonReference (object graph,
			XmlFormatterSerializer serializer)
		{
			if (add_method != null) { // generic
				if (pair_type == null) {
					pair_type = typeof (KeyValuePair<,>).MakeGenericType (add_method.DeclaringType.GetGenericArguments ());
					pair_key_property = pair_type.GetProperty ("Key");
					pair_value_property = pair_type.GetProperty ("Value");
				}
				foreach (object p in (IEnumerable) graph) {
					serializer.WriteStartElement (item_qname.Name, item_qname.Namespace, CurrentNamespace);
					serializer.WriteStartElement (key_qname.Name, key_qname.Namespace, CurrentNamespace);
					serializer.Serialize (pair_key_property.PropertyType, pair_key_property.GetValue (p, null));
					serializer.WriteEndElement ();
					serializer.WriteStartElement (value_qname.Name, value_qname.Namespace, CurrentNamespace);
					serializer.Serialize (pair_value_property.PropertyType, pair_value_property.GetValue (p, null));
					serializer.WriteEndElement ();
					serializer.WriteEndElement ();
				}
			} else { // non-generic
				foreach (DictionaryEntry p in (IEnumerable) graph) {
					serializer.WriteStartElement (item_qname.Name, item_qname.Namespace, CurrentNamespace);
					serializer.WriteStartElement (key_qname.Name, key_qname.Namespace, CurrentNamespace);
					serializer.Serialize (key_type, p.Key);
					serializer.WriteEndElement ();
					serializer.WriteStartElement (value_qname.Name, value_qname.Namespace, CurrentNamespace);
					serializer.Serialize (value_type, p.Value);
					serializer.WriteEndElement ();
					serializer.WriteEndElement ();
				}
			}
		}

		object CreateInstance ()
		{
			if (RuntimeType.IsInterface) {
				if (RuntimeType.IsGenericType && Array.IndexOf (RuntimeType.GetGenericTypeDefinition ().GetInterfaces (), typeof (IDictionary<,>)) >= 0) {
					var gargs = RuntimeType.GetGenericArguments ();
					return Activator.CreateInstance (typeof (Dictionary<,>).MakeGenericType (gargs [0], gargs [1])); // Dictionary<T>
				}
				else // non-generic
					return new Hashtable ();
			}
#if NET_2_1 // FIXME: is it fine?
			return Activator.CreateInstance (RuntimeType);
#else
			return Activator.CreateInstance (RuntimeType, true);
#endif
		}

		public override object DeserializeEmptyContent (XmlReader reader, XmlFormatterDeserializer deserializer)
		{
			return DeserializeContent (reader, deserializer);
		}

		public override object DeserializeContent(XmlReader reader, XmlFormatterDeserializer deserializer)
		{
			object instance = CreateInstance ();
			int depth = reader.NodeType == XmlNodeType.None ? reader.Depth : reader.Depth - 1;
			while (reader.NodeType == XmlNodeType.Element && reader.Depth > depth) {
				if (reader.IsEmptyElement)
					throw new XmlException (String.Format ("Unexpected empty element for dictionary entry: name {0}", reader.Name));
				// FIXME: sloppy parsing
				reader.ReadStartElement ();// item_qname.Name, item_qname.Namespace);
				reader.MoveToContent ();
				object key = deserializer.Deserialize (key_type, reader);
				reader.MoveToContent ();
				object val = deserializer.Deserialize (value_type, reader);
				reader.ReadEndElement (); // of pair

				if (instance is IDictionary)
					((IDictionary)instance).Add (key, val);
				else if (add_method != null)
					add_method.Invoke (instance, new object [] {key, val});
				else
					throw new NotImplementedException (String.Format ("Type {0} is not supported", RuntimeType));
			}
			return instance;
		}

		public override List<DataMemberInfo> GetMembers ()
		{
			//Shouldn't come here at all!
			throw new NotImplementedException ();
		}
		
#if !NET_2_1
		public override XmlSchemaType GetSchemaType (XmlSchemaSet schemas, Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			throw new NotImplementedException ();
		}
#endif
	}

	internal partial class SharedTypeMap : SerializationMap
	{
		public SharedTypeMap (
			Type type, QName qname, KnownTypeCollection knownTypes)
			: base (type, qname, knownTypes)
		{
		}

		public void Initialize ()
		{
			Members = GetMembers (RuntimeType, XmlName, false);
		}

		List<DataMemberInfo> GetMembers (Type type, QName qname, bool declared_only)
		{
			List<DataMemberInfo> data_members = new List<DataMemberInfo> ();
			BindingFlags flags = AllInstanceFlags;
			if (declared_only)
				flags |= BindingFlags.DeclaredOnly;
			
			foreach (FieldInfo fi in type.GetFields (flags)) {
				if (fi.GetCustomAttributes (
					typeof (NonSerializedAttribute),
					false).Length > 0)
					continue;

				if (fi.IsInitOnly)
					throw new InvalidDataContractException (String.Format ("DataMember field {0} must not be read-only.", fi));
				DataMemberAttribute dma = new DataMemberAttribute ();
				data_members.Add (CreateDataMemberInfo (dma, fi, fi.FieldType));
			}

			data_members.Sort (DataMemberInfo.DataMemberInfoComparer.Instance); // alphabetic order.

			return data_members;
		}

		// Does this make sense? I doubt.
		public override List<DataMemberInfo> GetMembers ()
		{
			return Members;
			//return GetMembers (RuntimeType, XmlName, true);
		}
	}

	internal partial class EnumMap : SerializationMap
	{
		List<EnumMemberInfo> enum_members;
		bool flag_attr;

		public EnumMap (
			Type type, QName qname, KnownTypeCollection knownTypes)
			: base (type, qname, knownTypes)
		{
			bool has_dc = false;
			object [] atts = RuntimeType.GetCustomAttributes (
				typeof (DataContractAttribute), false);
			if (atts.Length != 0)
				has_dc = true;
			flag_attr = type.GetCustomAttributes (typeof (FlagsAttribute), false).Length > 0;

			enum_members = new List<EnumMemberInfo> ();
			BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static;
			
			foreach (FieldInfo fi in RuntimeType.GetFields (flags)) {
				string name = fi.Name;
				if (has_dc) {
					EnumMemberAttribute ema =
						GetEnumMemberAttribute (fi);
					if (ema == null)
						continue;

					if (ema.Value != null)
						name = ema.Value;
				}

				enum_members.Add (new EnumMemberInfo (name, fi.GetValue (null)));
			}
		}

		private EnumMemberAttribute GetEnumMemberAttribute (
			MemberInfo mi)
		{
			object [] atts = mi.GetCustomAttributes (
				typeof (EnumMemberAttribute), false);
			if (atts.Length == 0)
				return null;
			return (EnumMemberAttribute) atts [0];
		}

#if !NET_2_1
		public override XmlSchemaType GetSchemaType (XmlSchemaSet schemas, Dictionary<QName, XmlSchemaType> generated_schema_types)
		{
			if (generated_schema_types.ContainsKey (XmlName))
				return generated_schema_types [XmlName];

			XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType ();
			simpleType.Name = XmlName.Name;

			XmlSchemaSimpleTypeRestriction simpleRestriction = new XmlSchemaSimpleTypeRestriction ();
			simpleType.Content = simpleRestriction;
			simpleRestriction.BaseTypeName = new XmlQualifiedName ("string", XmlSchema.Namespace);

			foreach (EnumMemberInfo emi in enum_members) {
				XmlSchemaEnumerationFacet e = new XmlSchemaEnumerationFacet ();
				e.Value = emi.XmlName;
				simpleRestriction.Facets.Add (e);
			}

			generated_schema_types [XmlName] = simpleType;
			
			XmlSchema schema = GetSchema (schemas, XmlName.Namespace);
			XmlSchemaElement element = GetSchemaElement (XmlName, simpleType);
			element.IsNillable = true;

			schema.Items.Add (simpleType);
			schema.Items.Add (element);

			return simpleType;
		}
#endif

		public override void Serialize (object graph,
			XmlFormatterSerializer serializer)
		{
			foreach (EnumMemberInfo emi in enum_members) {
				if (Enum.Equals (emi.Value, graph)) {
					serializer.Writer.WriteString (emi.XmlName);
					return;
				}
			}

			throw new SerializationException (String.Format (
				"Enum value '{0}' is invalid for type '{1}' and cannot be serialized.", graph, RuntimeType));
		}

		public override object DeserializeEmptyContent (XmlReader reader,
			XmlFormatterDeserializer deserializer)
		{
			if (!flag_attr)
				throw new SerializationException (String.Format ("Enum value '' is invalid for type '{0}' and cannot be deserialized.", RuntimeType));
			return Enum.ToObject (RuntimeType, 0);
		}

		public override object DeserializeContent (XmlReader reader,
			XmlFormatterDeserializer deserializer)
		{
			string value = reader.NodeType != XmlNodeType.Text ? String.Empty : reader.ReadContentAsString ();

			if (value != String.Empty) {
				foreach (EnumMemberInfo emi in enum_members)
					if (emi.XmlName == value)
						return emi.Value;
			}

			if (!flag_attr)
				throw new SerializationException (String.Format ("Enum value '{0}' is invalid for type '{1}' and cannot be deserialized.", value, RuntimeType));
			return Enum.ToObject (RuntimeType, 0);
		}
	}

	internal struct EnumMemberInfo
	{
		public readonly string XmlName;
		public readonly object Value;

		public EnumMemberInfo (string name, object value)
		{
			XmlName = name;
			Value = value;
		}
	}

	internal class DataMemberInfo //: KeyValuePair<int, MemberInfo>
	{
		public readonly int Order;
		public readonly bool IsRequired;
		public readonly string XmlName;
		public readonly MemberInfo Member;
		public readonly string XmlNamespace;
		public readonly string XmlRootNamespace;
		public readonly Type MemberType;

		public DataMemberInfo (MemberInfo member, DataMemberAttribute dma, string rootNamespce, string ns)
		{
			if (dma == null)
				throw new ArgumentNullException ("dma");
			Order = dma.Order;
			Member = member;
			IsRequired = dma.IsRequired;
			XmlName = dma.Name != null ? dma.Name : member.Name;
			XmlNamespace = ns;
			XmlRootNamespace = rootNamespce;
			if (Member is FieldInfo)
				MemberType = ((FieldInfo) Member).FieldType;
			else
				MemberType = ((PropertyInfo) Member).PropertyType;
		}

		public class DataMemberInfoComparer : IComparer<DataMemberInfo>
			, IComparer // see bug #76361
		{
			public static readonly DataMemberInfoComparer Instance
				= new DataMemberInfoComparer ();

			private DataMemberInfoComparer () {}

			public int Compare (object o1, object o2)
			{
				return Compare ((DataMemberInfo) o1,
					(DataMemberInfo) o2);
			}

			public int Compare (DataMemberInfo d1, DataMemberInfo d2)
			{
				if (d1.Order == d2.Order)
					return String.CompareOrdinal (d1.XmlName, d2.XmlName);

				return d1.Order - d2.Order;
			}
		}
	}
}
#endif
