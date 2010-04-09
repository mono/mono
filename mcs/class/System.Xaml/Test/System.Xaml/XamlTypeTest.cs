//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	// FIXME: enable AllowedContentTypes, ContentWrappers, DeferringLoader and ValueSerializer tests.
	[TestFixture]
	public class XamlTypeTest
	{
		XamlSchemaContext sctx = new XamlSchemaContext (null, null);

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNullType ()
		{
			new XamlType (null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNullSchemaContext ()
		{
			new XamlType (typeof (int), null);
		}

		[Test]
		public void ConstructorSimpleType ()
		{
			var t = new XamlType (typeof (int), sctx);
			Assert.AreEqual ("Int32", t.Name, "#1");
			Assert.AreEqual (typeof (int), t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("ValueType", t.BaseType.Name, "#3-2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", t.BaseType.PreferredXamlNamespace, "#3-3");
			// It is likely only for primitive types such as int.
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.PreferredXamlNamespace, "#4");

			t = new XamlType (typeof (XamlXmlReader), sctx);
			Assert.AreEqual ("XamlXmlReader", t.Name, "#11");
			Assert.AreEqual (typeof (XamlXmlReader), t.UnderlyingType, "#12");
			Assert.IsNotNull (t.BaseType, "#13");
			Assert.AreEqual (typeof (XamlReader), t.BaseType.UnderlyingType, "#13-2");
			Assert.AreEqual ("clr-namespace:System.Xaml;assembly=System.Xaml", t.BaseType.PreferredXamlNamespace, "#13-3");
			Assert.AreEqual ("clr-namespace:System.Xaml;assembly=System.Xaml", t.PreferredXamlNamespace, "#14");
		}

		[Test]
		public void ConstructorNullTypeInvoker ()
		{
			// allowed.
			new XamlType (typeof (int), sctx, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamesNullName ()
		{
			new XamlType (String.Empty, null, null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamesNullSchemaContext ()
		{
			new XamlType ("System", "Int32", null, null);
		}

		[Test]
		public void ConstructorNames ()
		{
			// null typeArguments is allowed.
			new XamlType ("System", "Int32", null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNullName ()
		{
			new MyXamlType (null, null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNullSchemaContext ()
		{
			new MyXamlType ("System.Int32", null, null);
		}

		[Test]
		public void ConstructorNameInvalid ()
		{
			// ... all allowed.
			new XamlType (String.Empty, ".", null, sctx);
			new XamlType (String.Empty, "<>", null, sctx);
			new XamlType (String.Empty, "", null, sctx);
		}

		[Test]
		public void ConstructorNameWithFullName ()
		{
			// null typeArguments is allowed.
			var t = new MyXamlType ("System.Int32", null, sctx);
			Assert.AreEqual ("System.Int32", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.IsNull (t.BaseType.BaseType, "#3-4");
			Assert.AreEqual (String.Empty, t.PreferredXamlNamespace, "#4");
			Assert.IsFalse (t.IsArray, "#5");
			Assert.IsFalse (t.IsGeneric, "#6");
			Assert.IsTrue (t.IsPublic, "#7");
		}

		[Test]
		public void NoSuchTypeByName ()
		{
			var t = new MyXamlType ("System.NoSuchType", null, sctx);
			Assert.AreEqual ("System.NoSuchType", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.AreEqual (String.Empty, t.PreferredXamlNamespace, "#4");
		}

		[Test]
		public void NoSuchTypeByNames ()
		{
			var t = new XamlType ("urn:foo", "System.NoSuchType", null, sctx);
			Assert.AreEqual ("System.NoSuchType", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.AreEqual ("urn:foo", t.PreferredXamlNamespace, "#4");
		}

		[Test]
		[Ignore ("It results in NRE on .NET 4.0 RC")]
		public void EmptyTypeArguments ()
		{
			var t1 = new MyXamlType ("System.Int32", null, sctx);
			var t2 = new MyXamlType ("System.Int32", new XamlType [0], sctx);
			Assert.IsTrue (t1 == t2, "#1");
			Assert.IsTrue (t1.Equals (t2), "#2");
		}

		[Test]
		public void EmptyTypeArguments2 ()
		{
			var t1 = new XamlType ("System", "Int32", null, sctx);
			var t2 = new XamlType ("System", "Int32", new XamlType [0], sctx);
			Assert.IsNull (t1.TypeArguments, "#1");
			Assert.IsNull (t2.TypeArguments, "#2");
			Assert.IsTrue (t1 == t2, "#3");
			Assert.IsTrue (t1.Equals (t2), "#4");
		}

		[Test]
		public void EqualityAcrossConstructors ()
		{
			var t1 = new XamlType (typeof (int), sctx);
			var t2 = new XamlType (t1.PreferredXamlNamespace, t1.Name, null, sctx);
			// not sure if it always returns false for different .ctor comparisons...
			Assert.IsFalse (t1 == t2, "#3");
		}

		[Test]
		public void ArrayAndCollection ()
		{
			var t = new XamlType (typeof (int), sctx);
			Assert.IsFalse (t.IsArray, "#1.1");
			Assert.IsFalse (t.IsCollection, "#1.2");
			Assert.IsNull (t.ItemType, "#1.3");
			t = new XamlType (typeof (ArrayList), sctx);
			Assert.IsFalse (t.IsArray, "#2.1");
			Assert.IsTrue (t.IsCollection, "#2.2");
			Assert.IsNotNull (t.ItemType, "#2.3");
			Assert.AreEqual ("Object", t.ItemType.Name, "#2.4");
			t = new XamlType (typeof (int []), sctx);
			Assert.IsTrue (t.IsArray, "#3.1");
			// why?
			Assert.IsFalse (t.IsCollection, "#3.2");
			Assert.IsNotNull (t.ItemType, "#3.3");
			Assert.AreEqual (typeof (int), t.ItemType.UnderlyingType, "#3.4");
		}

		[Test]
		public void Dictionary ()
		{
			var t = new XamlType (typeof (int), sctx);
			Assert.IsFalse (t.IsDictionary, "#1.1");
			Assert.IsNull (t.KeyType, "#1.2");
			t = new XamlType (typeof (Hashtable), sctx);
			Assert.IsTrue (t.IsDictionary, "#2.1");
			Assert.IsNotNull (t.KeyType, "#2.2");
			Assert.AreEqual ("Object", t.KeyType.Name, "#2.3");
			t = new XamlType (typeof (Dictionary<int,string>), sctx);
			Assert.IsTrue (t.IsDictionary, "#3.1");
			Assert.IsNotNull (t.KeyType, "#3.2");
			Assert.AreEqual ("Int32", t.KeyType.Name, "#3.3");
		}

		public class TestClass1
		{
		}
	
		class TestClass2
		{
			internal TestClass2 () {}
		}

		[Test]
		[Category ("NotWorking")]
		public void IsConstructible ()
		{
			// ... is it?
			Assert.IsTrue (new XamlType (typeof (int), sctx).IsConstructible, "#1");
			// ... is it?
			Assert.IsFalse (new XamlType (typeof (TestClass1), sctx).IsConstructible, "#2");
			Assert.IsFalse (new XamlType (typeof (TestClass2), sctx).IsConstructible, "#3");
			Assert.IsTrue (new XamlType (typeof (object), sctx).IsConstructible, "#4");
		}

		[Test]
		public void DefaultValuesType ()
		{
			var t = new XamlType (typeof (int), sctx);
			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsTrue (t.IsNameValid, "#2");
			Assert.IsFalse (t.IsUnknown, "#3");
			Assert.AreEqual ("Int32", t.Name, "#4");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.AreEqual (typeof (int), t.UnderlyingType, "#7");
			Assert.IsFalse (t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsTrue (t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsFalse (t.IsNullable, "#16");
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsFalse (t.IsWhitespaceSignificantCollection, "#19");
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			//Assert.IsNull (t.AllowedContentTypes, "#23");
			//Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNotNull (t.TypeConverter, "#25");
			Assert.IsTrue (t.TypeConverter.ConverterInstance is Int32Converter, "#25-2");
			//Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNull (t.ContentProperty, "#27");
			//Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Test]
		public void DefaultValuesName ()
		{
			var t = new XamlType ("urn:foo", ".", null, sctx);

			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsFalse (t.IsNameValid, "#2");
			Assert.IsTrue (t.IsUnknown, "#3");
			Assert.AreEqual (".", t.Name, "#4");
			Assert.AreEqual ("urn:foo", t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.IsNull (t.UnderlyingType, "#7");
			Assert.IsFalse (t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsTrue (t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsTrue (t.IsNullable, "#16"); // different from int
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsTrue (t.IsWhitespaceSignificantCollection, "#19"); // somehow true ...
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			//Assert.IsNull (t.AllowedContentTypes, "#23");
			//Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			//Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNull (t.ContentProperty, "#27");
			//Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Test]
		public void DefaultValuesCustomType ()
		{
			var t = new MyXamlType ("System.Int32", null, sctx);

			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsFalse (t.IsNameValid, "#2");
			Assert.IsTrue (t.IsUnknown, "#3");
			Assert.AreEqual ("System.Int32", t.Name, "#4");
			Assert.AreEqual (String.Empty, t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.IsNull (t.UnderlyingType, "#7");
			Assert.IsFalse (t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsTrue (t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsTrue (t.IsNullable, "#16"); // different from int
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsTrue (t.IsWhitespaceSignificantCollection, "#19"); // somehow true ...
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			//Assert.IsNull (t.AllowedContentTypes, "#23");
			//Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			//Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNull (t.ContentProperty, "#27");
			//Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Ambient]
		[ContentProperty ("Name")]
		[WhitespaceSignificantCollection]
		[UsableDuringInitialization (true)]
		public class TestClass3
		{
			public TestClass3 (string name)
			{
				Name = name;
			}
			
			public string Name { get; set; }
		}

		[Test]
		public void DefaultValuesSeverlyAttributed ()
		{
			var t = new XamlType (typeof (TestClass3), sctx);
			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsFalse (t.IsNameValid, "#2"); // see #4
			Assert.IsFalse (t.IsUnknown, "#3");
			Assert.AreEqual ("XamlTypeTest+TestClass3", t.Name, "#4");
			Assert.AreEqual ("clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name, t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.AreEqual (typeof (TestClass3), t.UnderlyingType, "#7");
			Assert.IsTrue (t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsFalse (t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsTrue (t.IsNullable, "#16");
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsTrue (t.IsUsableDuringInitialization, "#18");
			Assert.IsTrue (t.IsWhitespaceSignificantCollection, "#19");
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsTrue (t.IsAmbient, "#22");
			// Assert.IsNull (t.AllowedContentTypes, "#23");
			// Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			// Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNotNull (t.ContentProperty, "#27");
			Assert.AreEqual ("Name", t.ContentProperty.Name, "#27-2");
			// Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Test]
		public void TypeConverter ()
		{
			Assert.IsNull (new XamlType (typeof (List<object>), sctx).TypeConverter, "#1");
			Assert.IsNotNull (new XamlType (typeof (object), sctx).TypeConverter, "#2");
			Assert.IsTrue (new XamlType (typeof (Uri), sctx).TypeConverter.ConverterInstance is UriTypeConverter, "#3");
			Assert.IsTrue (new XamlType (typeof (TimeSpan), sctx).TypeConverter.ConverterInstance is TimeSpanConverter, "#4");
		}
	}

	class MyXamlType : XamlType
	{
		public MyXamlType (string fullName, IList<XamlType> typeArguments, XamlSchemaContext context)
			: base (fullName, typeArguments, context)
		{
		}
	}
}
