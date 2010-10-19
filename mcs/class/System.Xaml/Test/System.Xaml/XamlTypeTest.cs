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
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	// FIXME: enable DeferringLoader tests.
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
			Assert.AreEqual (0, t.GetAllMembers ().Count, "#8");
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
		[Ignore ("It results in NRE on .NET 4.0 RTM")]
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
			Assert.IsFalse (t.IsCollection, "#1.1-2");
			Assert.IsNull (t.KeyType, "#1.2");
			t = new XamlType (typeof (Hashtable), sctx);
			Assert.IsTrue (t.IsDictionary, "#2.1");
			Assert.IsFalse (t.IsCollection, "#2.1-2");
			Assert.IsNotNull (t.KeyType, "#2.2");
			Assert.AreEqual ("Object", t.KeyType.Name, "#2.3");
			t = new XamlType (typeof (Dictionary<int,string>), sctx);
			Assert.IsTrue (t.IsDictionary, "#3.1");
			Assert.IsFalse (t.IsCollection, "#3.1-2");
			Assert.IsNotNull (t.KeyType, "#3.2");
			Assert.AreEqual ("Int32", t.KeyType.Name, "#3.3");

			var ml = t.GetAllMembers ();
			Assert.AreEqual (2, ml.Count, "#3.4");
			Assert.IsTrue (ml.Any (mi => mi.Name == "Keys"), "#3.4-2");
			Assert.IsTrue (ml.Any (mi => mi.Name == "Values"), "#3.4-3");
			Assert.IsNotNull (t.GetMember ("Keys"), "#3.4-4");
			Assert.IsNotNull (t.GetMember ("Values"), "#3.4-5");
		}

		public class TestClass1
		{
		}
	
		class TestClass2
		{
			internal TestClass2 () {}
		}

		[Test]
		public void IsConstructible ()
		{
			// ... is it?
			Assert.IsTrue (new XamlType (typeof (int), sctx).IsConstructible, "#1");
			// ... is it?
			Assert.IsFalse (new XamlType (typeof (TestClass1), sctx).IsConstructible, "#2");
			Assert.IsFalse (new XamlType (typeof (TestClass2), sctx).IsConstructible, "#3");
			Assert.IsTrue (new XamlType (typeof (object), sctx).IsConstructible, "#4");
		}

		class AttachableClass
		{
			public event EventHandler<EventArgs> SimpleEvent;
			public void AddSimpleHandler (object o, EventHandler h)
			{
			}
		}

		// hmm, what can we use to verify this method?
		[Test]
		public void GetAllAttachableMembers ()
		{
			var xt = new XamlType (typeof (AttachableClass), sctx);
			var l = xt.GetAllAttachableMembers ();
			Assert.AreEqual (0, l.Count, "#1");
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
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNotNull (t.TypeConverter, "#25");
			Assert.IsTrue (t.TypeConverter.ConverterInstance is Int32Converter, "#25-2");
			Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNull (t.ContentProperty, "#27");
			//Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Test]
		public void DefaultValuesType2 ()
		{
			var t = new XamlType (typeof (Type), sctx);
			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsTrue (t.IsNameValid, "#2");
			Assert.IsFalse (t.IsUnknown, "#3");
			Assert.AreEqual ("Type", t.Name, "#4");
			// Note that Type is not a standard type. An instance of System.Type is usually represented as TypeExtension.
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.AreEqual (typeof (Type), t.UnderlyingType, "#7");
			Assert.IsTrue (t.ConstructionRequiresArguments, "#8"); // yes, true.
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsFalse (t.IsConstructible, "#11"); // yes, false.
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsTrue (t.IsNullable, "#16");
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsFalse (t.IsWhitespaceSignificantCollection, "#19");
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNotNull (t.TypeConverter, "#25"); // TypeTypeConverter
			Assert.IsNull (t.ValueSerializer, "#26");
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
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			Assert.IsNull (t.ValueSerializer, "#26");
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
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			Assert.IsNull (t.ValueSerializer, "#26");
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
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNotNull (t.ContentProperty, "#27");
			Assert.AreEqual ("Name", t.ContentProperty.Name, "#27-2");
			// Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");
		}

		[Test]
		public void DefaultValuesArgumentAttributed ()
		{
			var t = new XamlType (typeof (ArgumentAttributed), sctx);
			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsTrue (t.IsNameValid, "#2");
			Assert.IsFalse (t.IsUnknown, "#3");
			Assert.AreEqual ("ArgumentAttributed", t.Name, "#4");
			Assert.AreEqual ("clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name, t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.AreEqual (typeof (ArgumentAttributed), t.UnderlyingType, "#7");
			Assert.IsTrue (t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			Assert.IsTrue (t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.IsTrue (t.IsNullable, "#16");
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsFalse (t.IsWhitespaceSignificantCollection, "#19");
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			Assert.IsNull (t.TypeConverter, "#25");
			Assert.IsNull (t.ValueSerializer, "#26");
			Assert.IsNull (t.ContentProperty, "#27");
			// Assert.IsNull (t.DeferringLoader, "#28");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (sctx, t.SchemaContext, "#30");

			var members = t.GetAllMembers ();
			Assert.AreEqual (2, members.Count, "#31");
			string [] names = {"Arg1", "Arg2"};
			foreach (var member in members)
				Assert.IsTrue (Array.IndexOf (names, member.Name) >= 0, "#32");
		}

		[Test]
		public void TypeConverter ()
		{
			Assert.IsNull (new XamlType (typeof (List<object>), sctx).TypeConverter, "#1");
			Assert.IsNotNull (new XamlType (typeof (object), sctx).TypeConverter, "#2");
			Assert.IsTrue (new XamlType (typeof (Uri), sctx).TypeConverter.ConverterInstance is UriTypeConverter, "#3");
			Assert.IsTrue (new XamlType (typeof (TimeSpan), sctx).TypeConverter.ConverterInstance is TimeSpanConverter, "#4");
			Assert.IsNull (new XamlType (typeof (XamlType), sctx).TypeConverter, "#5");
			Assert.IsTrue (new XamlType (typeof (char), sctx).TypeConverter.ConverterInstance is CharConverter, "#6");
		}
		
		[Test]
		public void TypeConverter_Type ()
		{
			TypeConveter_TypeOrTypeExtension (typeof (Type));
		}
		
		[Test]
		public void TypeConverter_TypeExtension ()
		{
			TypeConveter_TypeOrTypeExtension (typeof (TypeExtension));
		}
		
		void TypeConveter_TypeOrTypeExtension (Type type)
		{
			var xtc = new XamlType (type, sctx).TypeConverter;
			Assert.IsNotNull (xtc, "#7");
			var tc = xtc.ConverterInstance;
			Assert.IsNotNull (tc, "#7-2");
			Assert.IsFalse (tc.CanConvertTo (typeof (Type)), "#7-3");
			Assert.IsFalse (tc.CanConvertTo (typeof (XamlType)), "#7-4");
			Assert.IsTrue (tc.CanConvertTo (typeof (string)), "#7-5");
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}TypeExtension", tc.ConvertToString (XamlLanguage.Type), "#7-6");
			Assert.IsFalse (tc.CanConvertFrom (typeof (Type)), "#7-7");
			Assert.IsFalse (tc.CanConvertFrom (typeof (XamlType)), "#7-8");
			// .NET returns true for type == typeof(Type) case here, which does not make sense. Disabling it now.
			//Assert.IsFalse (tc.CanConvertFrom (typeof (string)), "#7-9");
			try {
				tc.ConvertFromString ("{http://schemas.microsoft.com/winfx/2006/xaml}TypeExtension");
				Assert.Fail ("failure");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void GetXamlNamespaces ()
		{
			var xt = new XamlType (typeof (string), new XamlSchemaContext (null, null));
			var l = xt.GetXamlNamespaces ().ToList ();
			l.Sort ();
			Assert.AreEqual (2, l.Count, "#1-1");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", l [0], "#1-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, l [1], "#1-3");

			xt = new XamlType (typeof (TypeExtension), new XamlSchemaContext (null, null));
			l = xt.GetXamlNamespaces ().ToList ();
			l.Sort ();
			Assert.AreEqual (3, l.Count, "#2-1");
			Assert.AreEqual ("clr-namespace:System.Windows.Markup;assembly=System.Xaml", l [0], "#2-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, l [1], "#2-3");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, l [2], "#2-4"); // ??
		}
		
		[Test]
		public void GetAliasedProperty ()
		{
			XamlMember xm;
			var xt = new XamlType (typeof (SeverlyAliasedClass), new XamlSchemaContext (null, null));
			xm = xt.GetAliasedProperty (XamlLanguage.Key);
			Assert.IsNotNull (xm, "#1");
			xm = xt.GetAliasedProperty (XamlLanguage.Name);
			Assert.IsNotNull (xm, "#2");
			xm = xt.GetAliasedProperty (XamlLanguage.Uid);
			Assert.IsNotNull (xm, "#3");
			xm = xt.GetAliasedProperty (XamlLanguage.Lang);
			Assert.IsNotNull (xm, "#4");
		}

		[Test]
		public void GetAliasedPropertyOnAllTypes ()
		{
			foreach (var xt in XamlLanguage.AllTypes)
				foreach (var xd in XamlLanguage.AllDirectives)
					Assert.IsNull (xt.GetAliasedProperty (xd), xt.Name + " and " + xd.Name);
		}

		[DictionaryKeyProperty ("Key")]
		[RuntimeNameProperty ("RuntimeTypeName")]
		[UidProperty ("UUID")]
		[XmlLangProperty ("XmlLang")]
		public class SeverlyAliasedClass
		{
			public string Key { get; set; }
			public string RuntimeTypeName { get; set; }
			public string UUID { get; set; }
			public string XmlLang { get; set; }
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}String", XamlLanguage.String.ToString (), "#1");
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}TypeExtension", XamlLanguage.Type.ToString (), "#2");
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}ArrayExtension", XamlLanguage.Array.ToString (), "#3");
		}

		[Test]
		public void GetPositionalParameters ()
		{
			IList<XamlType> l;
			l = XamlLanguage.Type.GetPositionalParameters (1);
			Assert.IsNotNull (l, "#1");
			Assert.AreEqual (1, l.Count, "#2");
			Assert.AreEqual (typeof (Type), l [0].UnderlyingType, "#3"); // not TypeExtension but Type.
			Assert.AreEqual ("Type", l [0].Name, "#4");
		}

		[Test]
		public void GetPositionalParametersWrongCount ()
		{
			Assert.IsNull (XamlLanguage.Type.GetPositionalParameters (2), "#1");
		}

		[Test]
		public void GetPositionalParametersNoMemberExtension ()
		{
			// wow, so it returns some meaningless method parameters.
			Assert.IsNotNull (new XamlType (typeof (MyXamlType), sctx).GetPositionalParameters (3), "#1");
		}
	}

	class MyXamlType : XamlType
	{
		public MyXamlType (string fullName, IList<XamlType> typeArguments, XamlSchemaContext context)
			: base (fullName, typeArguments, context)
		{
		}
	}

	public class ArgumentAttributed
	{
		public ArgumentAttributed (string s1, string s2)
		{
			Arg1 = s1;
			Arg2 = s2;
		}

		[ConstructorArgument ("s1")]
		public string Arg1 { get; set; }

		[ConstructorArgument ("s2")]
		public string Arg2 { get; set; }
	}
}
