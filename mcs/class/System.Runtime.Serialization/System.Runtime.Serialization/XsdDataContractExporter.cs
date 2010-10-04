//
// XsdDataContractExporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using QName = System.Xml.XmlQualifiedName;

//
// .NET exports almost empty schema for "http://www.w3.org/2001/XMLSchema" that
// contains only "schema" element which consists of a complexType with empty
// definition (i.e. <complexType/> ).
//

namespace System.Runtime.Serialization
{
	static class TypeExtension
	{
		public static T GetCustomAttribute<T> (this MemberInfo mi, bool inherit)
		{
			foreach (T att in mi.GetCustomAttributes (typeof (T), inherit))
				return att;
			return default (T);
		}
	}

	public class XsdDataContractExporter
	{
		class TypeImportInfo
		{
			public Type ClrType { get; set; }
			public QName RootElementName { get; set; }
			public XmlSchemaType SchemaType { get; set; }
			public QName  SchemaTypeName { get; set; }
		}

		static readonly List<TypeImportInfo> predefined_types;

		static XsdDataContractExporter ()
		{
			var l = new List<TypeImportInfo> ();
			predefined_types = l;
			if (!MSTypesSchema.IsCompiled)
				MSTypesSchema.Compile (null);
			foreach (XmlSchemaElement el in MSTypesSchema.Elements.Values) {
				var typeName = el.ElementSchemaType.QualifiedName;
				var info = new TypeImportInfo () {
					RootElementName = el.QualifiedName,
					SchemaType = typeName.Namespace == XmlSchema.Namespace ? null : el.ElementSchemaType,
					SchemaTypeName = typeName,
					ClrType = GetPredefinedTypeFromQName (typeName) };
				l.Add (info);
			}
		}

		static Type GetPredefinedTypeFromQName (QName qname)
		{
			switch (qname.Namespace) {
			case XmlSchema.Namespace:
				return KnownTypeCollection.GetPrimitiveTypeFromName (qname.Name);
			case KnownTypeCollection.MSSimpleNamespace:
				switch (qname.Name) {
				case "char":
					return typeof (char);
				case "duration":
					return typeof (TimeSpan);
				case "guid":
					return typeof (Guid);
				}
				break;
			}
			throw new Exception ("Should not happen");
		}

		static XmlSchema mstypes_schema;
		static XmlSchema MSTypesSchema {
			get {
				if (mstypes_schema == null) {
					Assembly a = Assembly.GetCallingAssembly ();
					Stream s = a.GetManifestResourceStream ("mstypes.schema");
					mstypes_schema= XmlSchema.Read (s, null);
				}
				return mstypes_schema;
			}
		}

		KnownTypeCollection known_types = new KnownTypeCollection ();
		List<TypeImportInfo> imported_types = new List<TypeImportInfo> ();

		public XsdDataContractExporter ()
			: this (new XmlSchemaSet ())
		{
		}
		
		public XsdDataContractExporter (XmlSchemaSet schemas)
		{
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
#if false // by default this is the only added schema. But it is pointless...
			var xs = new XmlSchema () { TargetNamespace = XmlSchema.Namespace };
			xs.Items.Add (new XmlSchemaElement () { Name = "schema", SchemaType = new XmlSchemaComplexType () });
			schemas.Add (xs);
#else // FIXME: it is added only when the included items are in use.
			schemas.Add (MSTypesSchema);
#endif
			Schemas = schemas;
		}
		
		public ExportOptions Options { get; set; }
		public XmlSchemaSet Schemas { get; private set; }

		// CanExport implementation

		public bool CanExport (ICollection<Assembly> assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("assemblies");
			foreach (var ass in assemblies)
				if (!CanExport (ass.GetTypes ()))
					return false;
			return true;
		}
		
		public bool CanExport (ICollection<Type> types)
		{
			if (types == null)
				throw new ArgumentNullException ("types");
			foreach (var type in types)
				if (!CanExport (type))
					return false;
			return true;
		}
		
		public bool CanExport (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (predefined_types.FirstOrDefault (i => i.ClrType == type) != null)
				return true;

			known_types.TryRegister (type);
			return known_types.FindUserMap (type) != null;
		}

		// Export implementation

		public void Export (ICollection<Assembly> assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("assemblies");
			foreach (var ass in assemblies)
				Export (ass.GetTypes ());
		}
		
		public void Export (ICollection<Type> types)
		{
			if (types == null)
				throw new ArgumentNullException ("types");
			foreach (var type in types)
				Export (type);
		}
		
		public void Export (Type type)
		{
			if (ExportCore (type, true)) {
				// This reprocess is required to clean up compilation state.
				foreach (XmlSchema xs in Schemas.Schemas ())
					Schemas.Reprocess (xs);
				Schemas.Compile ();
			}
		}

		// returns true if it requires recompilcation
		bool ExportCore (Type type, bool rejectNonContract)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (predefined_types.FirstOrDefault (i => i.ClrType == type) != null) {
				if (Schemas.Contains (MSTypesSchema.TargetNamespace))
					return false; // exists
				Schemas.Add (MSTypesSchema);
				return false;
			}
			if (imported_types.FirstOrDefault (i => i.ClrType == type) != null)
				return false;

			known_types.TryRegister (type);
			var map = known_types.FindUserMap (type);
			if (map == null)
				return false;
			map.ExportSchemaType (this);
			return true;
		}
		
		internal void ExportDictionaryContractType (CollectionDataContractAttribute attr, Type type, Type dicType)
		{
			var qname = GetSchemaTypeName (type);

			var typeArgs = dicType.IsGenericType ? dicType.GetGenericArguments () : null;
			var keyType = typeArgs != null ? typeArgs [0] : typeof (object);
			var valueType = typeArgs != null ? typeArgs [1] : typeof (object);
			ExportCore (keyType, false);
			ExportCore (valueType, false);

			string keyName = "Key", valueName = "Value";
			if (attr != null) {
				keyName = attr.KeyName ?? keyName;
				valueName = attr.ValueName ?? valueName;
			}
			string itemName = attr != null && attr.ItemName != null ? attr.ItemName : "KeyValueOf" + keyName + valueName;

			var ct = CreateComplexType (qname, type);
			var appInfo = new XmlSchemaAppInfo ();
			var node = new XmlDocument ().CreateElement ("IsDictionary", KnownTypeCollection.MSSimpleNamespace);
			node.InnerText = "true";
			appInfo.Markup = new XmlNode [] { node };
			ct.Annotation = new XmlSchemaAnnotation ();
			ct.Annotation.Items.Add (appInfo);

			var seq = new XmlSchemaSequence ();
			ct.Particle = seq;
			var el = new XmlSchemaElement () { Name = itemName, MinOccurs = 0, MaxOccursString = "unbounded" };
			seq.Items.Add (el);

			var dictType = new XmlSchemaComplexType ();
			el.SchemaType = dictType;
			var dictSeq = new XmlSchemaSequence ();
			dictType.Particle = dictSeq;
			dictSeq.Items.Add (new XmlSchemaElement () { Name = keyName, SchemaTypeName = GetSchemaTypeName (keyType), IsNillable = true });
			dictSeq.Items.Add (new XmlSchemaElement () { Name = valueName, SchemaTypeName = GetSchemaTypeName (valueType), IsNillable = true });
		}
		
		internal void ExportListContractType (CollectionDataContractAttribute attr, Type type)
		{
			var qname = attr != null && attr.Name != null ? new QName (attr.Name, attr.Namespace ?? GetXmlNamespace (type)) : GetSchemaTypeName (type);

			var typeArgs = type.IsGenericType ? type.GetGenericArguments () : null;
			if (typeArgs != null && typeArgs.Length != 1)
				throw new InvalidDataContractException ("CollectionDataContractAttribute is applied to non-collection type.");

			var itemType = typeArgs != null ? typeArgs [0] : type.IsArray ? type.GetElementType () : typeof (object);
			bool nullable = !itemType.IsValueType;
			if (itemType.IsGenericType && itemType.GetGenericTypeDefinition () == typeof (Nullable<>)) {
				itemType = itemType.GetGenericArguments () [0];
				nullable = true;
			}
			ExportCore (itemType, false);

			var itemQName = GetSchemaTypeName (itemType);
			var itemName = attr != null && attr.ItemName != null ? attr.ItemName : itemQName.Name;

			var ct = CreateComplexType (qname, type);
			var seq = new XmlSchemaSequence ();
			ct.Particle = seq;
			var el = new XmlSchemaElement () { Name = itemName, MinOccurs = 0, MaxOccursString = "unbounded", SchemaTypeName = itemQName, IsNillable = nullable };
			seq.Items.Add (el);

			/*
			var arrayType = new XmlSchemaComplexType ();
			el.SchemaType = arrayType;
			var arraySeq = new XmlSchemaSequence ();
			arrayType.Particle = arraySeq;
			arraySeq.Items.Add (new XmlSchemaElement () { Name = itemName, SchemaTypeName = itemQName, IsNillable = true });
			*/
		}

		internal void ExportEnumContractType (DataContractAttribute attr, Type type)
		{
			var qname = attr != null && attr.Name != null ? new QName (attr.Name, attr.Namespace ?? GetXmlNamespace (type)) : GetSchemaTypeName (type);
			var st = CreateSimpleType (qname, type);
			if (type.GetCustomAttribute<FlagsAttribute> (false) != null) {
				var list = new XmlSchemaSimpleTypeList ();
				var sct = new XmlSchemaSimpleType ();
				sct.Content = CreateEnumMembers (type, attr != null);
				list.ItemType = sct;
				st.Content = list;
			}
			else
				st.Content = CreateEnumMembers (type, attr != null);
		}

		XmlSchemaSimpleTypeRestriction CreateEnumMembers (Type type, bool expectAttribute)
		{
			var r = new XmlSchemaSimpleTypeRestriction () { BaseTypeName = GetSchemaTypeName (typeof (string)) };
			foreach (var mi in type.GetFields (BindingFlags.Public | BindingFlags.Static)) {
				var ema = expectAttribute ? mi.GetCustomAttribute<EnumMemberAttribute> (false) : null;
				if (expectAttribute && ema == null)
					continue;
				var xe = new XmlSchemaEnumerationFacet () { Value = ema != null && ema.Value != null ? ema.Value : mi.Name };
				r.Facets.Add (xe);
			}
			return r;
		}

		internal void ExportStandardComplexType (DataContractAttribute attr, Type type, List<DataMemberInfo> members)
		{
			var qname = attr != null && attr.Name != null ? new QName (attr.Name, attr.Namespace ?? GetXmlNamespace (type)) : GetSchemaTypeName (type);
			var ct = CreateComplexType (qname, type);

			if (type.BaseType != null && type.BaseType != typeof (object)) {
				ExportCore (type.BaseType, false);
				var xcc = new XmlSchemaComplexContent ();
				ct.ContentModel = xcc;
				var xcce = new XmlSchemaComplexContentExtension ();
				xcc.Content = xcce;
				xcce.BaseTypeName = GetSchemaTypeName (type.BaseType);
				xcce.Particle = CreateMembersSequence (type, members, attr != null);
			}
			else
				ct.Particle = CreateMembersSequence (type, members, attr != null);
		}

		XmlSchemaSimpleType CreateSimpleType (QName qname, Type type)
		{
			var xs = GetSchema (qname.Namespace);

			var el = new XmlSchemaElement () { Name = qname.Name, IsNillable = true };
			el.SchemaTypeName = qname;
			xs.Items.Add (el);
			var st = new XmlSchemaSimpleType () { Name = qname.Name };
			xs.Items.Add (st);
			imported_types.Add (new TypeImportInfo () { RootElementName = qname, SchemaType = st, SchemaTypeName = qname,  ClrType = type });

			return st;
		}

		XmlSchemaComplexType CreateComplexType (QName qname, Type type)
		{
			var xs = GetSchema (qname.Namespace);

			var el = new XmlSchemaElement () { Name = qname.Name, IsNillable = true };
			el.SchemaTypeName = qname;
			xs.Items.Add (el);
			var ct = new XmlSchemaComplexType () { Name = qname.Name };
			xs.Items.Add (ct);
			imported_types.Add (new TypeImportInfo () { RootElementName = qname, SchemaType = ct, SchemaTypeName = qname,  ClrType = type });

			return ct;
		}

		static int CompareMembers (MemberInfo m1, MemberInfo m2)
		{
			var a1 = m1.GetCustomAttribute<DataMemberAttribute> (false);
			var a2 = m2.GetCustomAttribute<DataMemberAttribute> (false);
			return a1.Order == a2.Order ? String.CompareOrdinal (a1.Name ?? m1.Name, a2.Name ?? m2.Name) : a1.Order - a2.Order;
		}

		// FIXME: use members parameter to determine which members are to be exported.
		XmlSchemaSequence CreateMembersSequence (Type type, List<DataMemberInfo> dataMembers, bool expectContract)
		{
			var seq = new XmlSchemaSequence ();
			var members = new List<MemberInfo> ();
			var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			if (expectContract)
				flags |= BindingFlags.NonPublic;

			foreach (var mi in type.GetFields (flags))
				if (!expectContract || mi.GetCustomAttribute<DataMemberAttribute> (false) != null)
					members.Add (mi);
			foreach (var mi in type.GetProperties (flags))
				if ((!expectContract || mi.GetCustomAttribute<DataMemberAttribute> (false) != null) && mi.GetIndexParameters ().Length == 0)
					members.Add (mi);

			if (expectContract)
				members.Sort (CompareMembers);

			foreach (var mi in members) {
				var dma = mi.GetCustomAttribute<DataMemberAttribute> (false);
				var fi = mi as FieldInfo;
				var pi = mi as PropertyInfo;
				var mt = fi != null ? fi.FieldType : pi.PropertyType;
				bool nullable = !mt.IsValueType;
				if (mt.IsGenericType && mt.GetGenericTypeDefinition () == typeof (Nullable<>)) {
					mt = mt.GetGenericArguments () [0];
					nullable = true;
				}
				ExportCore (mt, false);

				var name = dma != null && dma.Name != null ? dma.Name : mi.Name;
				var xe = new XmlSchemaElement () { Name = name, IsNillable = nullable };
				xe.SchemaTypeName = GetSchemaTypeName (mt);
				seq.Items.Add (xe);
			}
			return seq;
		}

		XmlSchema GetSchema (string ns)
		{
			foreach (XmlSchema xs in Schemas.Schemas (ns))
				return xs;
			var nxs = new XmlSchema () { ElementFormDefault = XmlSchemaForm.Qualified };
			if (!String.IsNullOrEmpty (ns))
				nxs.TargetNamespace = ns;
			Schemas.Add (nxs);
			return nxs;
		}

		string GetXmlTypeName (Type type)
		{
			var qname = KnownTypeCollection.GetPrimitiveTypeName (type);
			return qname.Equals (QName.Empty) ? type.Name : qname.Name;
		}

		string GetXmlNamespace (Type type)
		{
			foreach (ContractNamespaceAttribute a in type.Assembly.GetCustomAttributes (typeof (ContractNamespaceAttribute), false))
				if (a.ClrNamespace == type.Namespace)
					return a.ContractNamespace;
			return KnownTypeCollection.DefaultClrNamespaceBase + type.Namespace;
		}

		// get mapping info (either exported or predefined).

		public QName GetRootElementName (Type type)
		{
			var info = predefined_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null)
				return info.RootElementName;
			info = imported_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null && info.RootElementName != null)
				return info.RootElementName;

			return GetSchemaTypeName (type);
		}
		
		public XmlSchemaType GetSchemaType (Type type)
		{
			var info = predefined_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null)
				return info.SchemaType;
			info = imported_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null)
				return info.SchemaType;

			return null;
		}
		
		public QName GetSchemaTypeName (Type type)
		{
			var info = predefined_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null)
				return info.SchemaTypeName;
			info = imported_types.FirstOrDefault (i => i.ClrType == type);
			if (info != null && info.SchemaTypeName != null)
				return info.SchemaTypeName;

			var cdca = type.GetCustomAttribute<CollectionDataContractAttribute> (false);
			if (cdca != null)
				return new QName (cdca.Name ?? GetXmlTypeName (type), cdca.Namespace ?? GetXmlNamespace (type));
			var dca = type.GetCustomAttribute<DataContractAttribute> (false);
			if (dca != null)
				return new QName (dca.Name ?? GetXmlTypeName (type), dca.Namespace ?? GetXmlNamespace (type));

			if (type.IsArray) {
				var item = GetSchemaTypeName (type.GetElementType ());
				if (item.Namespace == XmlSchema.Namespace)
					return new QName ("ArrayOf" + item.Name, KnownTypeCollection.MSArraysNamespace);
				return new QName ("ArrayOf" + item.Name, item.Namespace);
			}

			return new QName (type.Name, KnownTypeCollection.DefaultClrNamespaceBase + type.Namespace);
		}
	}
}
