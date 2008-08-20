//
// XsdDataContractExporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
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
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public class XsdDataContractExporter
	{
		ExportOptions options;
		KnownTypeCollection known_types;
		XmlSchemaSet schemas;
		Dictionary<QName, XmlSchemaType> generated_schema_types;

		static XmlSchema mstypes_schema;

		public XsdDataContractExporter ()
		{
		}

		public XsdDataContractExporter (XmlSchemaSet schemas)
		{
			this.schemas = schemas;
		}

		public XmlSchemaSet Schemas {
			get { 
				if (schemas == null) {
					schemas = new XmlSchemaSet ();
					schemas.Add (MSTypesSchema);
				}
				return schemas;
			}
		}

		public ExportOptions Options {
			get { return options; }
			set { options = value; }
		}

		public bool CanExport (ICollection<Type> types)
		{
			foreach (Type t in types)
				if (!CanExport (t))
					return false;
			return true;
		}

		public bool CanExport (ICollection<Assembly> assemblies)
		{
			foreach (Assembly a in assemblies)
				foreach (Module m in a.GetModules ())
					foreach (Type t in m.GetTypes ())
						if (!CanExport (t))
							return false;
			return true;
		}

		public bool CanExport (Type type)
		{
			return !KnownTypes.GetQName (type).IsEmpty;
		}

		public void Export (ICollection<Type> types)
		{
			foreach (Type t in types)
				Export (t);
		}

		public void Export (ICollection<Assembly> assemblies)
		{
			foreach (Assembly a in assemblies)
				foreach (Module m in a.GetModules ())
					foreach (Type t in m.GetTypes ())
						Export (t);
		}

		[MonoTODO]
		public void Export (Type type)
		{
			//FIXME: Which types to exclude?
			KnownTypes.Add (type);
			SerializationMap map = KnownTypes.FindUserMap (type);
			if (map == null)
				return;

			map.GetSchemaType (Schemas, GeneratedTypes);
			Schemas.Compile ();
		}

		[MonoTODO]
		public XmlQualifiedName GetRootElementName (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSchemaType GetSchemaType (Type type)
		{
			SerializationMap map = KnownTypes.FindUserMap (type);
			if (map == null)
				return null;

			return map.GetSchemaType (Schemas, GeneratedTypes);
		}

		public XmlQualifiedName GetSchemaTypeName (Type type)
		{
			QName qname = KnownTypes.GetQName (type);
			if (qname.Namespace == KnownTypeCollection.MSSimpleNamespace)
				//primitive type, mapping to XmlSchema ns
				return new QName (qname.Name, XmlSchema.Namespace);

			return qname;
		}

		KnownTypeCollection KnownTypes {
			get {
				if (known_types == null)
					known_types = new KnownTypeCollection ();
				return known_types;
			}
		}

		Dictionary<QName, XmlSchemaType> GeneratedTypes {
			get {
				if (generated_schema_types == null)
					generated_schema_types = new Dictionary<QName, XmlSchemaType> ();
				return generated_schema_types;
			}
		}
		
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


	}
}
#endif
