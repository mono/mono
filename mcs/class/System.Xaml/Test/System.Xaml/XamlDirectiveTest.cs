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
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlDirectiveTest
	{
		XamlSchemaContext sctx = new XamlSchemaContext (new XamlSchemaContextSettings ());

		[Test]
		public void ConstructorNameNull ()
		{
			// wow, it is allowed.
			var d = new XamlDirective (String.Empty, null);
			Assert.IsNull (d.Name, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamespaceNull ()
		{
			new XamlDirective (null, "Foo");
		}

		[Test]
		public void ConstructorNamespaceXamlNS ()
		{
			new XamlDirective (XamlLanguage.Xaml2006Namespace, "Foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorComplexParamsTypeNull ()
		{
			new XamlDirective (new string [] {"urn:foo"}, "Foo", null, null, AllowedMemberLocations.Any);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorComplexParamsNullNamespaces ()
		{
			var d = new XamlDirective (null, "Foo", new XamlType (typeof (object), sctx), null, AllowedMemberLocations.Any);
		}

		[Test]
		public void ConstructorComplexParamsEmptyNamespaces ()
		{
			var d = new XamlDirective (new string [0], "Foo", new XamlType (typeof (object), sctx), null, AllowedMemberLocations.Any);
		}

		[Test]
		public void ConstructorComplexParams ()
		{
			var d = new XamlDirective (new string [] {"urn:foo"}, "Foo", new XamlType (typeof (object), sctx), null, AllowedMemberLocations.Any);
		}

		[Test]
		public void DefaultValuesWithName ()
		{
			var d = new XamlDirective ("urn:foo", "Foo");
			Assert.AreEqual (AllowedMemberLocations.Any, d.AllowedLocation, "#1");
			Assert.IsNull (d.DeclaringType, "#2");
			Assert.IsNotNull (d.Invoker, "#3");
			Assert.IsNull (d.Invoker.UnderlyingGetter, "#3-2");
			Assert.IsNull (d.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsTrue (d.IsUnknown, "#4");
			Assert.IsTrue (d.IsReadPublic, "#5");
			Assert.IsTrue (d.IsWritePublic, "#6");
			Assert.AreEqual ("Foo", d.Name, "#7");
			Assert.IsTrue (d.IsNameValid, "#8");
			Assert.AreEqual ("urn:foo", d.PreferredXamlNamespace, "#9");
			Assert.IsNull (d.TargetType, "#10");
			Assert.IsNotNull (d.Type, "#11");
			Assert.AreEqual (typeof (object), d.Type.UnderlyingType, "#11-2");
			Assert.IsNull (d.TypeConverter, "#12");
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

		[Test]
		public void DefaultValuesWithComplexParams ()
		{
			var d = new XamlDirective (new string [0], "Foo", new XamlType (typeof (object), sctx), null, AllowedMemberLocations.Any);
			Assert.AreEqual (AllowedMemberLocations.Any, d.AllowedLocation, "#1");
			Assert.IsNull (d.DeclaringType, "#2");
			Assert.IsNotNull (d.Invoker, "#3");
			Assert.IsNull (d.Invoker.UnderlyingGetter, "#3-2");
			Assert.IsNull (d.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (d.IsUnknown, "#4"); // different from another test
			Assert.IsTrue (d.IsReadPublic, "#5");
			Assert.IsTrue (d.IsWritePublic, "#6");
			Assert.AreEqual ("Foo", d.Name, "#7");
			Assert.IsTrue (d.IsNameValid, "#8");
			Assert.AreEqual (null, d.PreferredXamlNamespace, "#9"); // different from another test (as we specified empty array above)
			Assert.IsNull (d.TargetType, "#10");
			Assert.IsNotNull (d.Type, "#11");
			Assert.AreEqual (typeof (object), d.Type.UnderlyingType, "#11-2");
			Assert.IsNull (d.TypeConverter, "#12");
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
	}
}
