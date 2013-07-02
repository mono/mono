//
// XsdDataContractImporterTest2.cs
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
	public class XsdDataContractImporterTest2
	{
		MetadataSet collectionsMetadata;
		MetadataSet customCollectionsMetadata;

		[SetUp]
		public void Setup ()
		{
			collectionsMetadata = WsdlHelper.GetMetadataSet ("collections.wsdl");
			customCollectionsMetadata = WsdlHelper.GetMetadataSet ("custom-collections.wsdl");
		}
		
		[Test]
		public void TestSimpleList ()
		{
			var options = new ImportOptions ();
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetSimpleList");
			Assert.That (method, Is.Not.Null, "#1");
			Assert.That (method.ReturnType, Is.Not.Null, "#2");
			
			Assert.That (method.ReturnType.ArrayRank, Is.EqualTo (1), "#3");
			Assert.That (method.ReturnType.BaseType, Is.EqualTo ("System.Int32"), "#4");
		}
		
		[Test]
		public void TestSimpleList2 ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof(LinkedList<>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetSimpleList");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.Generic.LinkedList`1"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (1), "#5");
			Assert.That (ret.TypeArguments [0].BaseType, Is.EqualTo ("System.Int32"), "#6");
		}

#if NET_4_0		
		[Test]
		public void TestSimpleList3 ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (Dictionary<,>));
			options.ReferencedCollectionTypes.Add (typeof (ObservableCollection<>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetSimpleList");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.ObjectModel.ObservableCollection`1"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (1), "#5");
			Assert.That (ret.TypeArguments [0].BaseType, Is.EqualTo ("System.Int32"), "#6");
		}
#endif
		
		[Test]
		public void TestListOfFoo ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (List<>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetListOfFoo");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.Generic.List`1"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (1), "#5");
			Assert.That (ret.TypeArguments [0].BaseType, Is.EqualTo ("TestWCF.Model.Foo"), "#6");
		}
		
		[Test]
		public void TestListOfStringArray ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (List<>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetListOfStringArray");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.Generic.List`1"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (1), "#5");
			
			var baseType = ret.TypeArguments [0];
			Assert.That (baseType.BaseType, Is.EqualTo ("System.Collections.Generic.List`1"), "#6");
			Assert.That (baseType.TypeArguments.Count, Is.EqualTo (1), "#7");
			Assert.That (baseType.TypeArguments [0].BaseType, Is.EqualTo ("System.String"), "#8");
		}
		
		[Test]
		public void TestSimpleDictionary ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (List<>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetSimpleDictionary");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.Generic.Dictionary`2"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (2), "#5");
			
			var keyType = ret.TypeArguments [0];
			Assert.That (keyType.BaseType, Is.EqualTo ("System.Int32"), "#6");
			var valueType = ret.TypeArguments [1];
			Assert.That (valueType.BaseType, Is.EqualTo ("System.String"), "#7");
		}
		
		[Test]
		public void TestSimpleDictionary2 ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (SortedList<,>));
			
			var ccu = WsdlHelper.Import (collectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetSimpleDictionary");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("System.Collections.Generic.SortedList`2"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (2), "#5");
			
			var keyType = ret.TypeArguments [0];
			Assert.That (keyType.BaseType, Is.EqualTo ("System.Int32"), "#6");
			var valueType = ret.TypeArguments [1];
			Assert.That (valueType.BaseType, Is.EqualTo ("System.String"), "#7");
		}

		[Test]
		public void TestCustomCollection ()
		{
			var options = new ImportOptions ();
			
			var ccu = WsdlHelper.Import (customCollectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetCustomCollection");
			Assert.That (method, Is.Not.Null, "#1");

			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("TestWCF.Model.MyCollection"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (0), "#5");
		}

		[Test]
		public void TestCustomCollection2 ()
		{
			var options = new ImportOptions ();

			var ccu = WsdlHelper.Import (customCollectionsMetadata, options);
			
			var method = ccu.FindMethod ("MyServiceClient", "GetCustomCollection2");
			Assert.That (method, Is.Not.Null, "#1");
			
			var ret = method.ReturnType;
			Assert.That (ret, Is.Not.Null, "#2");
			
			Assert.That (ret.ArrayRank, Is.EqualTo (0), "#3");
			Assert.That (ret.BaseType, Is.EqualTo ("TestWCF.Model.MyCollectionOfdouble"), "#4");
			Assert.That (ret.TypeArguments.Count, Is.EqualTo (0), "#5");
		}

		[Test]
		public void TestCustomCollection3 ()
		{
			var options = new ImportOptions ();

			var ccu = WsdlHelper.Import (customCollectionsMetadata, options);
			
			var type = ccu.FindType ("MyCollection");
			Assert.That (type, Is.Not.Null, "#1a");
			Assert.That (type.BaseTypes.Count, Is.EqualTo (1), "#2a");
			
			var baseType = type.BaseTypes[0];
			Assert.That (baseType.BaseType, Is.EqualTo ("System.Collections.Generic.List`1"), "#3a");
			Assert.That (baseType.TypeArguments.Count, Is.EqualTo (1), "#4a");
			Assert.That (baseType.TypeArguments[0].BaseType, Is.EqualTo ("System.String"), "#5a");

			var attr = type.FindAttribute ("System.Runtime.Serialization.CollectionDataContractAttribute");
			Assert.That (attr, Is.Not.Null, "#6a");

			var nameArg = attr.FindArgument ("Name");
			Assert.That (nameArg, Is.Not.Null, "#7a");
			Assert.That (((CodePrimitiveExpression)nameArg.Value).Value, Is.EqualTo ("MyCollection"), "#8a");

			var nsArg = attr.FindArgument ("Namespace");
			Assert.That (nsArg, Is.Not.Null, "#9a");
			Assert.That (((CodePrimitiveExpression)nsArg.Value).Value, Is.EqualTo ("http://schemas.datacontract.org/2004/07/TestWCF.Model"), "#10a");
			
			var itemArg = attr.FindArgument ("ItemName");
			Assert.That (itemArg, Is.Not.Null);
			Assert.That (((CodePrimitiveExpression)itemArg.Value).Value, Is.EqualTo ("string"), "#11a");

			type = ccu.FindType ("MyCollectionOfdouble");
			Assert.That (type, Is.Not.Null, "#1b");
			Assert.That (type.BaseTypes.Count, Is.EqualTo (1), "#2b");

			baseType = type.BaseTypes[0];
			Assert.That (baseType.BaseType, Is.EqualTo ("System.Collections.Generic.List`1"), "#3b");
			Assert.That (baseType.TypeArguments.Count, Is.EqualTo (1), "#4b");
			Assert.That (baseType.TypeArguments[0].BaseType, Is.EqualTo ("System.Double"), "#5b");
			
			attr = type.FindAttribute ("System.Runtime.Serialization.CollectionDataContractAttribute");
			Assert.That (attr, Is.Not.Null, "#6b");
			
			nameArg = attr.FindArgument ("Name");
			Assert.That (nameArg, Is.Not.Null, "#7b");
			Assert.That (((CodePrimitiveExpression)nameArg.Value).Value, Is.EqualTo ("MyCollectionOfdouble"), "#8b");
			
			nsArg = attr.FindArgument ("Namespace");
			Assert.That (nsArg, Is.Not.Null, "#9b");
			Assert.That (((CodePrimitiveExpression)nsArg.Value).Value, Is.EqualTo ("http://schemas.datacontract.org/2004/07/TestWCF.Model"), "#10b");
			
			itemArg = attr.FindArgument ("ItemName");
			Assert.That (itemArg, Is.Not.Null);
			Assert.That (((CodePrimitiveExpression)itemArg.Value).Value, Is.EqualTo ("double"), "#11b");
		}

		[Test]
		public void TestCustomCollection4 ()
		{
			var options = new ImportOptions ();
			options.ReferencedCollectionTypes.Add (typeof (LinkedList<>));

			var ccu = WsdlHelper.Import (customCollectionsMetadata, options);
			
			var type = ccu.FindType ("MyCollection");
			Assert.That (type, Is.Not.Null, "#1a");
			Assert.That (type.BaseTypes.Count, Is.EqualTo (1), "#2a");
			
			var baseType = type.BaseTypes[0];
			Assert.That (baseType.BaseType, Is.EqualTo ("System.Collections.Generic.LinkedList`1"), "#3a");
			Assert.That (baseType.TypeArguments.Count, Is.EqualTo (1), "#4a");
			Assert.That (baseType.TypeArguments[0].BaseType, Is.EqualTo ("System.String"), "#5a");
			
			var attr = type.FindAttribute ("System.Runtime.Serialization.CollectionDataContractAttribute");
			Assert.That (attr, Is.Not.Null, "#6a");
			
			var nameArg = attr.FindArgument ("Name");
			Assert.That (nameArg, Is.Not.Null, "#7a");
			Assert.That (((CodePrimitiveExpression)nameArg.Value).Value, Is.EqualTo ("MyCollection"), "#8a");
			
			var nsArg = attr.FindArgument ("Namespace");
			Assert.That (nsArg, Is.Not.Null, "#9a");
			Assert.That (((CodePrimitiveExpression)nsArg.Value).Value, Is.EqualTo ("http://schemas.datacontract.org/2004/07/TestWCF.Model"), "#10a");
			
			var itemArg = attr.FindArgument ("ItemName");
			Assert.That (itemArg, Is.Not.Null);
			Assert.That (((CodePrimitiveExpression)itemArg.Value).Value, Is.EqualTo ("string"), "#11a");
			
			type = ccu.FindType ("MyCollectionOfdouble");
			Assert.That (type, Is.Not.Null, "#1b");
			Assert.That (type.BaseTypes.Count, Is.EqualTo (1), "#2b");
			
			baseType = type.BaseTypes[0];
			Assert.That (baseType.BaseType, Is.EqualTo ("System.Collections.Generic.LinkedList`1"), "#3b");
			Assert.That (baseType.TypeArguments.Count, Is.EqualTo (1), "#4b");
			Assert.That (baseType.TypeArguments[0].BaseType, Is.EqualTo ("System.Double"), "#5b");
			
			attr = type.FindAttribute ("System.Runtime.Serialization.CollectionDataContractAttribute");
			Assert.That (attr, Is.Not.Null, "#6b");
			
			nameArg = attr.FindArgument ("Name");
			Assert.That (nameArg, Is.Not.Null, "#7b");
			Assert.That (((CodePrimitiveExpression)nameArg.Value).Value, Is.EqualTo ("MyCollectionOfdouble"), "#8b");
			
			nsArg = attr.FindArgument ("Namespace");
			Assert.That (nsArg, Is.Not.Null, "#9b");
			Assert.That (((CodePrimitiveExpression)nsArg.Value).Value, Is.EqualTo ("http://schemas.datacontract.org/2004/07/TestWCF.Model"), "#10b");
			
			itemArg = attr.FindArgument ("ItemName");
			Assert.That (itemArg, Is.Not.Null);
			Assert.That (((CodePrimitiveExpression)itemArg.Value).Value, Is.EqualTo ("double"), "#11b");
		}
	}
}

#endif
