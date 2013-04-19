//
// XsdDataContractExporterTest2.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if !MOBILE

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class XsdDataContractExporterTest2
	{
		internal const string MSArraysNamespace =
			"http://schemas.microsoft.com/2003/10/Serialization/Arrays";

		[Test]
		public void ExportList ()
		{
			var exporter = new XsdDataContractExporter ();
			Assert.That (exporter.CanExport (typeof(MyService)), Is.True, "#1");
			exporter.Export (typeof(MyService));

			var typeName = exporter.GetSchemaTypeName (typeof(MyService));
			var type = exporter.Schemas.GlobalTypes [typeName];

			Assert.That (type, Is.Not.Null, "#2");
			Assert.That (type, Is.InstanceOfType (typeof (XmlSchemaComplexType)), "#3");

			var complex = (XmlSchemaComplexType)type;
			Assert.That (complex.Annotation, Is.Null, "#4");

			var sequence = complex.Particle as XmlSchemaSequence;
			Assert.That (sequence, Is.Not.Null, "#5");
			Assert.That (sequence.Items.Count, Is.EqualTo (3), "#5a");
			Assert.That (sequence.Annotation, Is.Null, "#5b");
			Assert.That (sequence.MinOccursString, Is.Null, "#5c");
			Assert.That (sequence.MaxOccursString, Is.Null, "#5d");

			var list = GetElement (sequence, "list");
			Assert.That (list, Is.Not.Null, "#6");
			Assert.That (list.Annotation, Is.Null, "#6a");
			Assert.That (list.Name, Is.EqualTo ("list"), "#6b");
			Assert.That (list.ElementSchemaType, Is.InstanceOfType (typeof (XmlSchemaComplexType)), "#6c");

			var listElement = (XmlSchemaComplexType)list.ElementSchemaType;
			Assert.That (listElement.QualifiedName.Namespace, Is.EqualTo (MSArraysNamespace), "#6d");
			Assert.That (listElement.QualifiedName.Name, Is.EqualTo ("ArrayOfint"), "#6e");

			Assert.That (listElement.Particle, Is.InstanceOfType (typeof(XmlSchemaSequence)), "#7");
			var listSeq = (XmlSchemaSequence)listElement.Particle;
			Assert.That (listSeq.Items.Count, Is.EqualTo (1), "#7b");
			Assert.That (listSeq.Items[0], Is.InstanceOfType (typeof(XmlSchemaElement)), "#7c");
			Assert.That (listSeq.Annotation, Is.Null, "#7d");

			var listSeqElement = (XmlSchemaElement)listSeq.Items[0];
			Assert.That (listSeqElement.MaxOccursString, Is.EqualTo ("unbounded"), "#7e");
			Assert.That (listSeqElement.MinOccursString, Is.EqualTo ("0"), "#7f");

			var dict = GetElement (sequence, "dictionary");
			Assert.That (dict, Is.Not.Null, "#8");
			Assert.That (dict.Annotation, Is.Null, "#8a");
			Assert.That (dict.Name, Is.EqualTo ("dictionary"), "#8b");
			Assert.That (dict.ElementSchemaType, Is.InstanceOfType (typeof (XmlSchemaComplexType)), "#8c");
			
			var dictElement = (XmlSchemaComplexType)dict.ElementSchemaType;
			Assert.That (dictElement.QualifiedName.Namespace, Is.EqualTo (MSArraysNamespace), "#8d");
			Assert.That (dictElement.QualifiedName.Name, Is.EqualTo ("ArrayOfKeyValueOfstringdouble"), "#8e");

			Assert.That (dictElement.Particle, Is.InstanceOfType (typeof(XmlSchemaSequence)), "#9");
			var dictSeq = (XmlSchemaSequence)dictElement.Particle;
			Assert.That (dictSeq.Items.Count, Is.EqualTo (1), "#9b");
			Assert.That (dictSeq.Items[0], Is.InstanceOfType (typeof(XmlSchemaElement)), "#9c");
			Assert.That (dictSeq.Annotation, Is.Null, "#9d");
			
			var dictSeqElement = (XmlSchemaElement)dictSeq.Items[0];
			Assert.That (listSeqElement.MaxOccursString, Is.EqualTo ("unbounded"), "#9e");
			Assert.That (listSeqElement.MinOccursString, Is.EqualTo ("0"), "#9f");


			var custom = GetElement (sequence, "customCollection");
			Assert.That (custom, Is.Not.Null, "#10");
			Assert.That (custom.Annotation, Is.Null, "#10a");
			Assert.That (custom.Name, Is.EqualTo ("customCollection"), "#10b");
			Assert.That (custom.ElementSchemaType, Is.InstanceOfType (typeof (XmlSchemaComplexType)), "#10c");
			
			var customElement = (XmlSchemaComplexType)custom.ElementSchemaType;
			var customEQN = customElement.QualifiedName;
			Assert.That (customEQN.Namespace, Is.EqualTo (typeName.Namespace), "#10d");
			Assert.That (customEQN.Name.StartsWith ("XsdDataContractExporterTest2.MyCollectionOfstring", StringComparison.InvariantCultureIgnoreCase),
			             Is.True, "#10e");

			Assert.That (customElement.Particle, Is.InstanceOfType (typeof(XmlSchemaSequence)), "#11");
			var customSeq = (XmlSchemaSequence)customElement.Particle;
			Assert.That (customSeq.Items.Count, Is.EqualTo (1), "#11b");
			Assert.That (customSeq.Items[0], Is.InstanceOfType (typeof(XmlSchemaElement)), "#11c");
			Assert.That (customSeq.Annotation, Is.Null, "#11d");
			
			var customSeqElement = (XmlSchemaElement)customSeq.Items[0];
			Assert.That (customSeqElement.MaxOccursString, Is.EqualTo ("unbounded"), "#11e");
			Assert.That (customSeqElement.MinOccursString, Is.EqualTo ("0"), "#11f");
		}

		static XmlSchemaElement GetElement (XmlSchemaSequence sequence, string name)
		{
			foreach (XmlSchemaElement item in sequence.Items) {
				if (item.Name.Equals (name))
					return item;
			}

			return null;
		}

		[ServiceContract]
		public class MyService
		{
			[DataMember]
			public List<int> list;

			[DataMember]
			public Dictionary<string,double> dictionary;

			[DataMember]
			public MyCollection<string> customCollection;
		}

		[CollectionDataContract]
		public class MyCollection<T> : List<T>
		{
		}
	}
}

#endif
