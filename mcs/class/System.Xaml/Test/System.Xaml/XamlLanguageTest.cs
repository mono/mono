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
using System.Xml;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlLanguageTest
	{
		[Test]
		public void XamlNamepaces ()
		{
			var l = XamlLanguage.XamlNamespaces;
			Assert.AreEqual (1, l.Count, "#1");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, l [0], "#2");
		}

		[Test]
		public void XmlNamepaces ()
		{
			var l = XamlLanguage.XmlNamespaces;
			Assert.AreEqual (1, l.Count, "#1");
			Assert.AreEqual (XamlLanguage.Xml1998Namespace, l [0], "#2");
		}

		[Test]
		public void AllDirectives ()
		{
			var l = XamlLanguage.AllDirectives;
			Assert.AreEqual (24, l.Count, "count");
			Assert.IsTrue (l.Contains (XamlLanguage.Arguments), "#0");
			Assert.IsTrue (l.Contains (XamlLanguage.AsyncRecords), "#1");
			Assert.IsTrue (l.Contains (XamlLanguage.Base), "#2");
			Assert.IsTrue (l.Contains (XamlLanguage.Class), "#3");
			Assert.IsTrue (l.Contains (XamlLanguage.ClassAttributes), "#4");
			Assert.IsTrue (l.Contains (XamlLanguage.ClassModifier), "#5");
			Assert.IsTrue (l.Contains (XamlLanguage.Code), "#6");
			Assert.IsTrue (l.Contains (XamlLanguage.ConnectionId), "#7");
			Assert.IsTrue (l.Contains (XamlLanguage.FactoryMethod), "#8");
			Assert.IsTrue (l.Contains (XamlLanguage.FieldModifier), "#9");
			Assert.IsTrue (l.Contains (XamlLanguage.Initialization), "#10");
			Assert.IsTrue (l.Contains (XamlLanguage.Items), "#11");
			Assert.IsTrue (l.Contains (XamlLanguage.Key), "#12");
			Assert.IsTrue (l.Contains (XamlLanguage.Lang), "#13");
			Assert.IsTrue (l.Contains (XamlLanguage.Members), "#14");
			Assert.IsTrue (l.Contains (XamlLanguage.Name), "#15");
			Assert.IsTrue (l.Contains (XamlLanguage.PositionalParameters), "#16");
			Assert.IsTrue (l.Contains (XamlLanguage.Space), "#17");
			Assert.IsTrue (l.Contains (XamlLanguage.Subclass), "#18");
			Assert.IsTrue (l.Contains (XamlLanguage.SynchronousMode), "#19");
			Assert.IsTrue (l.Contains (XamlLanguage.Shared), "#20");
			Assert.IsTrue (l.Contains (XamlLanguage.TypeArguments), "#21");
			Assert.IsTrue (l.Contains (XamlLanguage.Uid), "#22");
			Assert.IsTrue (l.Contains (XamlLanguage.UnknownContent), "#23");
		}

		[Test]
		public void AllTypes ()
		{
			var l = XamlLanguage.AllTypes;
			Assert.AreEqual (21, l.Count, "count");
			Assert.IsTrue (l.Contains (XamlLanguage.Array), "#0");
			Assert.IsTrue (l.Contains (XamlLanguage.Boolean), "#1");
			Assert.IsTrue (l.Contains (XamlLanguage.Byte), "#2");
			Assert.IsTrue (l.Contains (XamlLanguage.Char), "#3");
			Assert.IsTrue (l.Contains (XamlLanguage.Decimal), "#4");
			Assert.IsTrue (l.Contains (XamlLanguage.Double), "#5");
			Assert.IsTrue (l.Contains (XamlLanguage.Int16), "#6");
			Assert.IsTrue (l.Contains (XamlLanguage.Int32), "#7");
			Assert.IsTrue (l.Contains (XamlLanguage.Int64), "#8");
			Assert.IsTrue (l.Contains (XamlLanguage.Member), "#9");
			Assert.IsTrue (l.Contains (XamlLanguage.Null), "#10");
			Assert.IsTrue (l.Contains (XamlLanguage.Object), "#11");
			Assert.IsTrue (l.Contains (XamlLanguage.Property), "#12");
			Assert.IsTrue (l.Contains (XamlLanguage.Reference), "#13");
			Assert.IsTrue (l.Contains (XamlLanguage.Single), "#14");
			Assert.IsTrue (l.Contains (XamlLanguage.Static), "#15");
			Assert.IsTrue (l.Contains (XamlLanguage.String), "#16");
			Assert.IsTrue (l.Contains (XamlLanguage.TimeSpan), "#17");
			Assert.IsTrue (l.Contains (XamlLanguage.Type), "#18");
			Assert.IsTrue (l.Contains (XamlLanguage.Uri), "#19");
			Assert.IsTrue (l.Contains (XamlLanguage.XData), "#20");
		}

		// directive property details

		[Test]
		public void Arguments ()
		{
			var d = XamlLanguage.Arguments;
			TestXamlDirectiveCommon (d, "Arguments", AllowedMemberLocations.Any, typeof (List<object>));
		}

		[Test]
		public void AsyncRecords ()
		{
			var d = XamlLanguage.AsyncRecords;
			TestXamlDirectiveCommon (d, "AsyncRecords", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Base ()
		{
			var d = XamlLanguage.Base;
			TestXamlDirectiveCommon (d, "base", XamlLanguage.Xml1998Namespace, AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Class ()
		{
			var d = XamlLanguage.Class;
			TestXamlDirectiveCommon (d, "Class", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void ClassAttributes ()
		{
			var d = XamlLanguage.ClassAttributes;
			TestXamlDirectiveCommon (d, "ClassAttributes", AllowedMemberLocations.MemberElement, typeof (List<Attribute>));
		}

		[Test]
		public void ClassModifier ()
		{
			var d = XamlLanguage.ClassModifier;
			TestXamlDirectiveCommon (d, "ClassModifier", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Code ()
		{
			var d = XamlLanguage.Code;
			TestXamlDirectiveCommon (d, "Code", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void ConnectionId ()
		{
			var d = XamlLanguage.ConnectionId;
			TestXamlDirectiveCommon (d, "ConnectionId", AllowedMemberLocations.Any, typeof (string));
		}

		[Test]
		public void FactoryMethod ()
		{
			var d = XamlLanguage.FactoryMethod;
			TestXamlDirectiveCommon (d, "FactoryMethod", AllowedMemberLocations.Any, typeof (string));
		}

		[Test]
		public void FieldModifier ()
		{
			var d = XamlLanguage.FieldModifier;
			TestXamlDirectiveCommon (d, "FieldModifier", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Initialization ()
		{
			var d = XamlLanguage.Initialization;
			// weird name
			TestXamlDirectiveCommon (d, "_Initialization", AllowedMemberLocations.Any, typeof (object));
		}

		[Test]
		public void Items ()
		{
			var d = XamlLanguage.Items;
			// weird name
			TestXamlDirectiveCommon (d, "_Items", AllowedMemberLocations.Any, typeof (List<object>));
		}

		[Test]
		public void Key ()
		{
			var d = XamlLanguage.Key;
			TestXamlDirectiveCommon (d, "Key", AllowedMemberLocations.Any, typeof (object));
		}

		[Test]
		public void Lang ()
		{
			var d = XamlLanguage.Lang;
			TestXamlDirectiveCommon (d, "lang", XamlLanguage.Xml1998Namespace, AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Members ()
		{
			var d = XamlLanguage.Members;
			TestXamlDirectiveCommon (d, "Members", AllowedMemberLocations.MemberElement, typeof (List<MemberDefinition>));
		}

		[Test]
		public void Name ()
		{
			var d = XamlLanguage.Name;
			TestXamlDirectiveCommon (d, "Name", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void PositionalParameters ()
		{
			var d = XamlLanguage.PositionalParameters;
			// weird name
			TestXamlDirectiveCommon (d, "_PositionalParameters", AllowedMemberLocations.Any, typeof (List<object>));
		}

		[Test]
		public void Subclass ()
		{
			var d = XamlLanguage.Subclass;
			TestXamlDirectiveCommon (d, "Subclass", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void SynchronousMode ()
		{
			var d = XamlLanguage.SynchronousMode;
			TestXamlDirectiveCommon (d, "SynchronousMode", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Shared ()
		{
			var d = XamlLanguage.Shared;
			TestXamlDirectiveCommon (d, "Shared", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Space ()
		{
			var d = XamlLanguage.Space;
			TestXamlDirectiveCommon (d, "space", XamlLanguage.Xml1998Namespace, AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void TypeArguments ()
		{
			var d = XamlLanguage.TypeArguments;
			TestXamlDirectiveCommon (d, "TypeArguments", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void Uid ()
		{
			var d = XamlLanguage.Uid;
			TestXamlDirectiveCommon (d, "Uid", AllowedMemberLocations.Attribute, typeof (string));
		}

		[Test]
		public void UnknownContent ()
		{
			var d = XamlLanguage.UnknownContent;
			// weird name
			TestXamlDirectiveCommon (d, "_UnknownContent", XamlLanguage.Xaml2006Namespace, AllowedMemberLocations.MemberElement, typeof (object), true);
		}

		void TestXamlDirectiveCommon (XamlDirective d, string name, AllowedMemberLocations allowedLocation, Type type)
		{
			TestXamlDirectiveCommon (d, name, XamlLanguage.Xaml2006Namespace, allowedLocation, type);
		}

		void TestXamlDirectiveCommon (XamlDirective d, string name, string ns, AllowedMemberLocations allowedLocation, Type type)
		{
			TestXamlDirectiveCommon (d, name, ns, allowedLocation, type, false);
		}

		void TestXamlDirectiveCommon (XamlDirective d, string name, string ns, AllowedMemberLocations allowedLocation, Type type, bool isUnknown)
		{
			Assert.AreEqual (allowedLocation, d.AllowedLocation, "#1");
			Assert.IsNull (d.DeclaringType, "#2");
			Assert.IsNotNull (d.Invoker, "#3");
			Assert.IsNull (d.Invoker.UnderlyingGetter, "#3-2");
			Assert.IsNull (d.Invoker.UnderlyingSetter, "#3-3");
			Assert.AreEqual (isUnknown, d.IsUnknown, "#4");
			Assert.IsTrue (d.IsReadPublic, "#5");
			Assert.IsTrue (d.IsWritePublic, "#6");
			Assert.AreEqual (name, d.Name, "#7");
			Assert.IsTrue (d.IsNameValid, "#8");
			Assert.AreEqual (ns, d.PreferredXamlNamespace, "#9");
			Assert.IsNull (d.TargetType, "#10");
			Assert.IsNotNull (d.Type, "#11");
			Assert.AreEqual (type, d.Type.UnderlyingType, "#11-2");

			// .NET returns StringConverter, but it should not premise that key must be string (it is object)
			if (name == "Key")
				;//Assert.IsNull (d.TypeConverter, "#12")
			else if (type.IsGenericType || name == "_Initialization" || name == "_UnknownContent")
				Assert.IsNull (d.TypeConverter, "#12");
			else
				Assert.IsNotNull (d.TypeConverter, "#12");
			Assert.IsNull (d.ValueSerializer, "#13");
			Assert.IsNull (d.DeferringLoader, "#14");
			Assert.IsNull (d.UnderlyingMember, "#15");
			Assert.IsFalse (d.IsReadOnly, "#16");
			Assert.IsFalse (d.IsWriteOnly, "#17");
			Assert.IsFalse (d.IsAttachable, "#18");
			Assert.IsFalse (d.IsEvent, "#19");
			Assert.IsTrue (d.IsDirective, "#20");
			Assert.IsNotNull (d.DependsOn, "#21");
			Assert.AreEqual (0, d.DependsOn.Count, "#21-2");
			Assert.IsFalse (d.IsAmbient, "#22");
			Assert.AreEqual (DesignerSerializationVisibility.Visible, d.SerializationVisibility, "#23");
		}

		// type property details

		// extension types
		[Test]
		public void Array ()
		{
			var t = XamlLanguage.Array;
			TestXamlTypeExtension (t, "ArrayExtension", typeof (ArrayExtension), typeof (Array), true);
			Assert.IsNotNull (t.ContentProperty, "#27");
			Assert.AreEqual ("Items", t.ContentProperty.Name, "#27-2");

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (2, l.Length, "#31");
			var items = l.First (m => m.Name == "Items");
			Assert.IsFalse (items == XamlLanguage.Items, "#31-2");
			l.First (m => m.Name == "Type");

			l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
		}

		[Test]
		public void Array_Items ()
		{
			var m = XamlLanguage.Array.GetMember ("Items");
			TestMemberCommon (m, "Items", typeof (IList), typeof (ArrayExtension), false);
		}

		[Test]
		public void Array_Type ()
		{
			var m = XamlLanguage.Array.GetMember ("Type");
			TestMemberCommon (m, "Type", typeof (Type), typeof (ArrayExtension), true);
		}

		[Test]
		public void Null ()
		{
			var t = XamlLanguage.Null;
			TestXamlTypeExtension (t, "NullExtension", typeof (NullExtension), typeof (object), true);
			Assert.IsNull (t.ContentProperty, "#27");

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#31");

			l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
		}

		[Test]
		public void Static ()
		{
			var t = XamlLanguage.Static;
			TestXamlTypeExtension (t, "StaticExtension", typeof (StaticExtension), typeof (object), false);
			Assert.IsNotNull (t.TypeConverter.ConverterInstance, "#25-2");
			Assert.IsNull (t.ContentProperty, "#27");

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (2, l.Length, "#31");
			l.First (m => m.Name == "Member");
			l.First (m => m.Name == "MemberType");

			l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
		}

		[Test]
		public void Static_Member ()
		{
			var m = XamlLanguage.Static.GetMember ("Member");
			TestMemberCommon (m, "Member", typeof (string), typeof (StaticExtension), true);
		}

		[Test]
		public void Static_MemberType ()
		{
			var m = XamlLanguage.Static.GetMember ("MemberType");
			TestMemberCommon (m, "MemberType", typeof (Type), typeof (StaticExtension), true);
		}

		[Test]
		public void Type ()
		{
			var t = XamlLanguage.Type;
			TestXamlTypeExtension (t, "TypeExtension", typeof (TypeExtension), typeof (Type), false);
			Assert.IsNotNull (t.TypeConverter.ConverterInstance, "#25-2");
			Assert.IsNull (t.ContentProperty, "#27");

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (2, l.Length, "#31");
			l.First (m => m.Name == "TypeName");
			l.First (m => m.Name == "Type");

			l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
		}

		[Test]
		public void Type_TypeName ()
		{
			var m = XamlLanguage.Type.GetMember ("TypeName");
			TestMemberCommon (m, "TypeName", typeof (string), typeof (TypeExtension), true);
		}

		[Test]
		public void Type_Type ()
		{
			var m = XamlLanguage.Type.GetMember ("Type");
			TestMemberCommon (m, "Type", typeof (Type), typeof (TypeExtension), true);
			Assert.AreNotEqual (XamlLanguage.Type, m.Type, "#1");
		}

		// primitive types

		[Test]
		public void Byte ()
		{
			var t = XamlLanguage.Byte;
			TestXamlTypePrimitive (t, "Byte", typeof (byte), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			*/
		}

		[Test]
		public void Char ()
		{
			var t = XamlLanguage.Char;
			TestXamlTypePrimitive (t, "Char", typeof (char), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (3, l.Length, "#32");
			l.First (m => m.Name == "UnicodeCategory");
			l.First (m => m.Name == "NumericValue");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Decimal ()
		{
			var t = XamlLanguage.Decimal;
			TestXamlTypePrimitive (t, "Decimal", typeof (decimal), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (2, l.Length, "#32");
			l.First (m => m.Name == "Bits");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Double ()
		{
			var t = XamlLanguage.Double;
			TestXamlTypePrimitive (t, "Double", typeof (double), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Int16 ()
		{
			var t = XamlLanguage.Int16;
			TestXamlTypePrimitive (t, "Int16", typeof (short), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Int32 ()
		{
			var t = XamlLanguage.Int32;
			TestXamlTypePrimitive (t, "Int32", typeof (int), false, false);

			try {
				t.Invoker.CreateInstance (new object [] {1});
				Assert.Fail ("Should expect .ctor() and fail");
			} catch (MissingMethodException) {
			}

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Int64 ()
		{
			var t = XamlLanguage.Int64;
			TestXamlTypePrimitive (t, "Int64", typeof (long), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Object ()
		{
			var t = XamlLanguage.Object;
			TestXamlTypePrimitive (t, "Object", typeof (object), true, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
			*/
		}

		[Test]
		public void Single ()
		{
			var t = XamlLanguage.Single;
			TestXamlTypePrimitive (t, "Single", typeof (float), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void String ()
		{
			var t = XamlLanguage.String;
			TestXamlTypePrimitive (t, "String", typeof (string), true, true);
			Assert.IsNotNull (XamlLanguage.AllTypes.First (tt => tt.Name == "String").ValueSerializer, "#x");
			Assert.IsNotNull (XamlLanguage.String.ValueSerializer, "#y");

			try {
				t.Invoker.CreateInstance (new object [] {"foo"});
				Assert.Fail ("Should expect .ctor() and fail");
			} catch (MissingMethodException) {
			}

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
			*/
		}

		[Test]
		public void TimeSpan ()
		{
			var t = XamlLanguage.TimeSpan;
			TestXamlTypePrimitive (t, "TimeSpan", typeof (TimeSpan), false, false);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#32");
			l.First (m => m.Name == "HashCodeOfPtr");
			*/
		}

		[Test]
		public void Uri ()
		{
			var t = XamlLanguage.Uri;
			TestXamlTypePrimitive (t, "Uri", typeof (Uri), true, true);

			/* Those properties are pointless regarding practical use. Those "members" does not participate in serialization.
			var l = t.GetAllAttachableMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#32");
			*/
		}

		// miscellaneous

		[Test]
		public void Member ()
		{
			var t = XamlLanguage.Member;
			TestXamlTypeCommon (t, "Member", typeof (MemberDefinition), true, true, false);
			Assert.IsNull (t.TypeConverter, "#25");
			// FIXME: test remaining members

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#31");
			l.First (m => m.Name == "Name");
		}

		[Test]
		public void Member_Name ()
		{
			var m = XamlLanguage.Member.GetMember ("Name");
			TestMemberCommon (m, "Name", typeof (string), typeof (MemberDefinition), true);
		}

		[Test]
		public void Property ()
		{
			var t = XamlLanguage.Property;
			TestXamlTypeCommon (t, "Property", typeof (PropertyDefinition), true);
			Assert.IsNull (t.TypeConverter, "#25");
			// FIXME: test remaining members

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (4, l.Length, "#31");
			l.First (m => m.Name == "Name");
			l.First (m => m.Name == "Type");
			l.First (m => m.Name == "Modifier");
			l.First (m => m.Name == "Attributes");
		}

		[Test]
		public void Property_Name ()
		{
			var m = XamlLanguage.Property.GetMember ("Name");
			TestMemberCommon (m, "Name", typeof (string), typeof (PropertyDefinition), true);
		}

		[Test]
		public void Property_Type ()
		{
			var m = XamlLanguage.Property.GetMember ("Type");
			TestMemberCommon (m, "Type", typeof (XamlType), typeof (PropertyDefinition), true);
		}

		[Test]
		public void Property_Modifier ()
		{
			var m = XamlLanguage.Property.GetMember ("Modifier");
			TestMemberCommon (m, "Modifier", typeof (string), typeof (PropertyDefinition), true);
		}

		[Test]
		public void Property_Attributes ()
		{
			var m = XamlLanguage.Property.GetMember ("Attributes");
			TestMemberCommon (m, "Attributes", typeof (IList<Attribute>), typeof (PropertyDefinition), false);
		}

		[Test]
		public void Reference ()
		{
			var t = XamlLanguage.Reference;
			TestXamlTypeCommon (t, "Reference", typeof (Reference), true);
			Assert.IsNull (t.TypeConverter, "#25");
			// FIXME: test remaining members

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (1, l.Length, "#31");
			l.First (m => m.Name == "Name");
		}

		[Test]
		public void Reference_Name ()
		{
			var m = XamlLanguage.Reference.GetMember ("Name");
			TestMemberCommon (m, "Name", typeof (string), typeof (Reference), true);
		}

		[Test]
		public void XData ()
		{
			var t = XamlLanguage.XData;
			TestXamlTypeCommon (t, "XData", typeof (XData), true);
			Assert.IsNull (t.TypeConverter, "#25");
			// FIXME: test remaining members

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (2, l.Length, "#31");
			l.First (m => m.Name == "Text");
			l.First (m => m.Name == "XmlReader");
		}

		[Test]
		public void XData_Text ()
		{
			var m = XamlLanguage.XData.GetMember ("Text");
			TestMemberCommon (m, "Text", typeof (string), typeof (XData), true);
		}

		[Test]
		public void XData_XmlReader ()
		{
			var m = XamlLanguage.XData.GetMember ("XmlReader");
			// it does not use XmlReader type ...
			TestMemberCommon (m, "XmlReader", typeof (object), typeof (XData), true);
		}

		// common test methods

		void TestXamlTypeCommon (XamlType t, string name, Type underlyingType, bool nullable)
		{
			TestXamlTypeCommon (t, name, underlyingType, nullable, false);
		}

		void TestXamlTypeCommon (XamlType t, string name, Type underlyingType, bool nullable, bool constructionRequiresArguments)
		{
			TestXamlTypeCommon (t, name, underlyingType, nullable, constructionRequiresArguments, true);
		}

		void TestXamlTypeCommon (XamlType t, string name, Type underlyingType, bool nullable, bool constructionRequiresArguments, bool isConstructible)
		{
			Assert.IsNotNull (t.Invoker, "#1");
			Assert.IsTrue (t.IsNameValid, "#2");
			Assert.IsFalse (t.IsUnknown, "#3");
			// FIXME: test names (some extension types have wrong name.
			//Assert.AreEqual (name, t.Name, "#4");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.PreferredXamlNamespace, "#5");
			Assert.IsNull (t.TypeArguments, "#6");
			Assert.AreEqual (underlyingType, t.UnderlyingType, "#7");
			Assert.AreEqual (constructionRequiresArguments, t.ConstructionRequiresArguments, "#8");
			Assert.IsFalse (t.IsArray, "#9");
			Assert.IsFalse (t.IsCollection, "#10");
			// FIXME: test here (very inconsistent with the spec)
			Assert.AreEqual (isConstructible, t.IsConstructible, "#11");
			Assert.IsFalse (t.IsDictionary, "#12");
			Assert.IsFalse (t.IsGeneric, "#13");
			Assert.IsFalse (t.IsNameScope, "#15");
			Assert.AreEqual (nullable, t.IsNullable, "#16");
			Assert.IsTrue (t.IsPublic, "#17");
			Assert.IsFalse (t.IsUsableDuringInitialization, "#18");
			Assert.IsFalse (t.IsWhitespaceSignificantCollection, "#19");
			Assert.IsFalse (t.IsXData, "#20");
			Assert.IsFalse (t.TrimSurroundingWhitespace, "#21");
			Assert.IsFalse (t.IsAmbient, "#22");
			Assert.IsNull (t.AllowedContentTypes, "#23");
			Assert.IsNull (t.ContentWrappers, "#24");
			// string is a special case.
			if (t == XamlLanguage.String)
				Assert.IsNotNull (t.ValueSerializer, "#26");
			else
				Assert.IsNull (t.ValueSerializer, "#26");
			//Assert.IsNull (t.DeferringLoader, "#28");
		}

		void TestXamlTypePrimitive (XamlType t, string name, Type underlyingType, bool nullable, bool constructorRequiresArguments)
		{
			TestXamlTypeCommon (t, name, underlyingType, nullable, constructorRequiresArguments);
			Assert.IsFalse (t.IsMarkupExtension, "#14");
			Assert.IsNotNull (t.TypeConverter, "#25");
			Assert.IsNull (t.ContentProperty, "#27");
			Assert.IsNull (t.MarkupExtensionReturnType, "#29");

			var l = t.GetAllMembers ().ToArray ();
			Assert.AreEqual (0, l.Length, "#31");
		}

		void TestXamlTypeExtension (XamlType t, string name, Type underlyingType, Type extReturnType, bool noTypeConverter)
		{
			TestXamlTypeCommon (t, name, underlyingType, true, false);
			Assert.IsTrue (t.IsMarkupExtension, "#14");
			if (noTypeConverter)
				Assert.IsNull (t.TypeConverter, "#25");
			else
				Assert.IsNotNull (t.TypeConverter, "#25");
			Assert.IsNotNull (t.MarkupExtensionReturnType, "#29");
			Assert.AreEqual (extReturnType, t.MarkupExtensionReturnType.UnderlyingType, "#29-2");
			Assert.IsNull (t.Invoker.SetMarkupExtensionHandler, "#31"); // orly?
		}

		void TestMemberCommon (XamlMember m, string name, Type type, Type declType, bool hasSetter)
		{
			Assert.IsNotNull (m, "#1");
			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (declType, m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.IsNotNull (m.Invoker.UnderlyingGetter, "#3-2");
			if (hasSetter)
				Assert.IsNotNull (m.Invoker.UnderlyingSetter, "#3-3");
			else
				Assert.IsNull (m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (m.IsUnknown, "#4");
			Assert.IsTrue (m.IsReadPublic, "#5");
			Assert.AreEqual (hasSetter, m.IsWritePublic, "#6");
			Assert.AreEqual (name, m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, m.PreferredXamlNamespace, "#9");
			// use declType here (mostly identical to targetType)
			Assert.AreEqual (new XamlType (declType, m.TargetType.SchemaContext), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (type, m.Type.UnderlyingType, "#11-2");
			// Property.Type is a special case here.
			if (name == "Type" && m.DeclaringType != XamlLanguage.Property)
				Assert.AreEqual (m.Type.TypeConverter, m.TypeConverter, "#12");
			// String type is a special case here.
			if (type == typeof (string))
				Assert.AreEqual (m.Type.ValueSerializer, m.ValueSerializer, "#13a");
			else
				Assert.IsNull (m.ValueSerializer, "#13b");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.IsNotNull (m.UnderlyingMember, "#15");
			Assert.AreEqual (!hasSetter, m.IsReadOnly, "#16");
			Assert.IsFalse (m.IsWriteOnly, "#17");
			Assert.IsFalse (m.IsAttachable, "#18");
			Assert.IsFalse (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}
	}
}
