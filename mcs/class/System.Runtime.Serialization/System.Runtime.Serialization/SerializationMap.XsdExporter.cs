//
// SerializationMap.XsdExporter.cs
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

namespace System.Runtime.Serialization
{
	internal abstract partial class SerializationMap
	{
		public abstract void ExportSchemaType (XsdDataContractExporter exporter);
	}
	
	internal partial class XmlSerializableMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			// .NET also expects a default constructor.
			var ixs = (IXmlSerializable) Activator.CreateInstance (RuntimeType, true);
			var xs = ixs.GetSchema ();
			if (xs != null)
				exporter.Schemas.Add (xs);
		}
	}
	
	internal partial class SharedContractMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportStandardComplexType (RuntimeType.GetCustomAttribute<DataContractAttribute> (false), this, Members);
		}
	}
	
	internal partial class DefaultTypeMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportStandardComplexType (null, this, Members);
		}
	}
	
	internal partial class CollectionContractTypeMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportListContractType (a, this);
		}
	}
	
	internal partial class CollectionTypeMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportListContractType (null, this);
		}
	}
	
	internal partial class DictionaryTypeMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportDictionaryContractType (a, this, GetGenericDictionaryInterface (RuntimeType));
		}
	}
	
	internal partial class SharedTypeMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportStandardComplexType (null, this, Members);
		}
	}
	
	internal partial class EnumMap
	{
		public override void ExportSchemaType (XsdDataContractExporter exporter)
		{
			exporter.ExportEnumContractType (RuntimeType.GetCustomAttribute<DataContractAttribute> (false), this);
		}
	}
}
