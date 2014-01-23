//
// XmlFormatterDeserializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	internal class XmlFormatterDeserializer
	{
		KnownTypeCollection types;
		IDataContractSurrogate surrogate;
		DataContractResolver resolver, default_resolver; // new in 4.0.
		// 3.5 SP1 supports deserialization by reference (id->obj).
		// Though unlike XmlSerializer, it does not support forward-
		// reference resolution i.e. a referenced object must appear
		// before any references to it.
		Dictionary<string,object> references = new Dictionary<string,object> ();
		Dictionary<QName,Type> resolved_qnames = new Dictionary<QName,Type> ();

		public static object Deserialize (XmlReader reader, Type declaredType,
			KnownTypeCollection knownTypes, IDataContractSurrogate surrogate, DataContractResolver resolver, DataContractResolver defaultResolver,
			string name, string ns, bool verifyObjectName)
		{
			reader.MoveToContent ();
			if (verifyObjectName)
				if (reader.NodeType != XmlNodeType.Element ||
				    reader.LocalName != name ||
				    reader.NamespaceURI != ns)
					throw new SerializationException (String.Format ("Expected element '{0}' in namespace '{1}', but found {2} node '{3}' in namespace '{4}'", name, ns, reader.NodeType, reader.LocalName, reader.NamespaceURI));
//				Verify (knownTypes, declaredType, name, ns, reader);
			return new XmlFormatterDeserializer (knownTypes, surrogate, resolver, defaultResolver).Deserialize (declaredType, reader);
		}

		// Verify the top element name and namespace.
		private static void Verify (KnownTypeCollection knownTypes, Type type, string name, string Namespace, XmlReader reader)
		{
			QName graph_qname = new QName (reader.LocalName, reader.NamespaceURI);
			if (graph_qname.Name == name && graph_qname.Namespace == Namespace)
				return;

			// <BClass .. i:type="EClass" >..</BClass>
			// Expecting type EClass : allowed
			// See test Serialize1b, and Serialize1c (for
			// negative cases)

			// Run through inheritance heirarchy .. 
			for (Type baseType = type; baseType != null; baseType = baseType.BaseType)
				if (knownTypes.GetQName (baseType) == graph_qname)
					return;

			QName typeQName = knownTypes.GetQName (type);
			throw new SerializationException (String.Format (
				"Expecting element '{0}' from namespace '{1}'. Encountered 'Element' with name '{2}', namespace '{3}'",
				typeQName.Name, typeQName.Namespace, graph_qname.Name, graph_qname.Namespace));
		}

		private XmlFormatterDeserializer (
			KnownTypeCollection knownTypes,
			IDataContractSurrogate surrogate,
			DataContractResolver resolver,
			DataContractResolver defaultResolver)
		{
			this.types = knownTypes;
			this.surrogate = surrogate;
			this.resolver = resolver;
			this.default_resolver = defaultResolver;
		}

		public Dictionary<string,object> References {
			get { return references; }
		}

		XmlDocument document;
		
		XmlDocument XmlDocument {
			get { return (document = document ?? new XmlDocument ()); }
		}

		// This method handles z:Ref, xsi:nil and primitive types, and then delegates to DeserializeByMap() for anything else.

		public object Deserialize (Type type, XmlReader reader)
		{
			if (type == typeof (XmlElement))
				return XmlDocument.ReadNode (reader);
			else if (type == typeof (XmlNode [])) {
				reader.ReadStartElement ();
				var l = new List<XmlNode> ();
				for(; !reader.EOF && reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ())
					l.Add (XmlDocument.ReadNode (reader));
				reader.ReadEndElement ();
				return l.ToArray ();
			}
			QName graph_qname = null;
			
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>)) {
				Type internal_type = type.GetGenericArguments () [0];
				
				if (types.FindUserMap(internal_type) != null) {
					graph_qname = types.GetQName (internal_type);
				}
			}
			
			if (graph_qname == null)
				graph_qname = types.GetQName (type);
				
			string itype = reader.GetAttribute ("type", XmlSchema.InstanceNamespace);
			if (itype != null) {
				string [] parts = itype.Split (':');
				if (parts.Length > 1)
					graph_qname = new QName (parts [1], reader.LookupNamespace (reader.NameTable.Get (parts [0])));
				else
					graph_qname = new QName (itype, reader.LookupNamespace (String.Empty));
			}

			string label = reader.GetAttribute ("Ref", KnownTypeCollection.MSSimpleNamespace);
			if (label != null) {
				object o;
				if (!references.TryGetValue (label, out o))
					throw new SerializationException (String.Format ("Deserialized object with reference Id '{0}' was not found", label));
				reader.Skip ();
				return o;
			}

			bool isNil = reader.GetAttribute ("nil", XmlSchema.InstanceNamespace) == "true";

			if (isNil) {
				reader.Skip ();
				if (!type.IsValueType || type == typeof (void))
					return null;
				else if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>))
					return null;
				else 
					throw new SerializationException (String.Format ("Value type {0} cannot be null.", type));
			}

			if (resolver != null) {
				Type t;
				if (resolved_qnames.TryGetValue (graph_qname, out t))
					type = t;
				else { // i.e. resolve name only once.
					type = resolver.ResolveName (graph_qname.Name, graph_qname.Namespace, type, default_resolver) ?? type;
					resolved_qnames.Add (graph_qname, type);
					types.Add (type);
				}
			}

			if (KnownTypeCollection.GetPrimitiveTypeFromName (graph_qname) != null) {
				string id = reader.GetAttribute ("Id", KnownTypeCollection.MSSimpleNamespace);

				object ret = DeserializePrimitive (type, reader, graph_qname);

				if (id != null) {
					if (references.ContainsKey (id))
						throw new InvalidOperationException (String.Format ("Object with Id '{0}' already exists as '{1}'", id, references [id]));
					references.Add (id, ret);
				}
				return ret;
			}

			return DeserializeByMap (graph_qname, type, reader);
		}

		object DeserializePrimitive (Type type, XmlReader reader, QName qname)
		{
			bool isDateTimeOffset = false;
			// Handle DateTimeOffset type and DateTimeOffset?.
			if (type == typeof (DateTimeOffset))
				isDateTimeOffset = true;
			else if(type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>)) 
				isDateTimeOffset = type.GetGenericArguments () [0] == typeof (DateTimeOffset);	
			// It is the only exceptional type that does not serialize to string but serializes into complex element.
			if (isDateTimeOffset) {
				if (reader.IsEmptyElement) {
					reader.Read ();
					return default (DateTimeOffset);
				}
				reader.ReadStartElement ();
				reader.MoveToContent ();
				var date = reader.ReadElementContentAsDateTime ("DateTime", KnownTypeCollection.DefaultClrNamespaceSystem);
				var off = TimeSpan.FromMinutes (reader.ReadElementContentAsInt ("OffsetMinutes", KnownTypeCollection.DefaultClrNamespaceSystem));
				reader.MoveToContent ();
				reader.ReadEndElement ();
				return new DateTimeOffset (DateTime.SpecifyKind (date.ToUniversalTime () + off, DateTimeKind.Unspecified), off);
			}

			string value;
			if (reader.IsEmptyElement) {
				reader.Read (); // advance
				if (type.IsValueType)
					return Activator.CreateInstance (type);
				else
					// FIXME: Workaround for creating empty objects of the correct type.
					value = String.Empty;
			}
			else
				value = reader.ReadElementContentAsString ();
			return KnownTypeCollection.PredefinedTypeStringToObject (value, qname.Name, reader);
		}

		object DeserializeByMap (QName name, Type type, XmlReader reader)
		{
			SerializationMap map = null;
			// List<T> and T[] have the same QName, use type to find map work better.
			if(name.Name.StartsWith ("ArrayOf", StringComparison.Ordinal) || resolved_qnames.ContainsKey (name))
				map = types.FindUserMap (type);
			else
				map = types.FindUserMap (name); // use type when the name is "resolved" one. Otherwise use name (there are cases that type cannot be resolved by type).
			if (map == null && (name.Name.StartsWith ("ArrayOf", StringComparison.Ordinal) ||
			    name.Namespace == KnownTypeCollection.MSArraysNamespace ||
			    name.Namespace.StartsWith (KnownTypeCollection.DefaultClrNamespaceBase, StringComparison.Ordinal))) {
				var it = GetTypeFromNamePair (name.Name, name.Namespace);
				types.Add (it);
				map = types.FindUserMap (name);
			}
			if (map == null)
				throw new SerializationException (String.Format ("Unknown type {0} is used for DataContract with reference of name {1}. Any derived types of a data contract or a data member should be added to KnownTypes.", type, name));

			return map.DeserializeObject (reader, this);
		}

		Type GetTypeFromNamePair (string name, string ns)
		{
			Type p = KnownTypeCollection.GetPrimitiveTypeFromName (new QName (name, ns));
			if (p != null)
				return p;
			bool makeArray = false;
			if (name.StartsWith ("ArrayOf", StringComparison.Ordinal)) {
				name = name.Substring (7); // strip "ArrayOf"
				if (ns == KnownTypeCollection.MSArraysNamespace)
					return GetTypeFromNamePair (name, String.Empty).MakeArrayType ();
				makeArray = true;
			}

			string dnsb = KnownTypeCollection.DefaultClrNamespaceBase;
			string clrns = ns.StartsWith (dnsb, StringComparison.Ordinal) ?  ns.Substring (dnsb.Length) : ns;

			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies ()) {
				Type [] types;

				types = ass.GetTypes ();
				if (types == null)
					continue;

				foreach (var t in types) {
					// there can be null entries or exception throw to access the attribute - 
					// at least when some referenced assemblies could not be loaded (affects moonlight)
					if (t == null)
						continue;

					try {
						var dca = t.GetCustomAttribute<DataContractAttribute> (true);
						if (dca != null && dca.Name == name && dca.Namespace == ns)
							return makeArray ? t.MakeArrayType () : t;
					}
					catch (TypeLoadException tle) {
						Console.Error.WriteLine (tle);
						continue;
					}
					catch (FileNotFoundException fnfe) {
						Console.Error.WriteLine (fnfe);
						continue;
					}

					if (clrns != null && t.Name == name && t.Namespace == clrns)
						return makeArray ? t.MakeArrayType () : t;
				}
			}
			throw new XmlException (String.Format ("Type not found; name: {0}, namespace: {1}", name, ns));
		}
	}
}
#endif
