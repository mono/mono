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
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	// FIXME: uncomment TypeConverter tests
	public class XamlMemberTest
	{
		XamlSchemaContext sctx = new XamlSchemaContext (new XamlSchemaContextSettings ());
		EventInfo ass_load = typeof (AppDomain).GetEvent ("AssemblyLoad");
		PropertyInfo str_len = typeof (string).GetProperty ("Length");
		PropertyInfo sb_len = typeof (StringBuilder).GetProperty ("Length");
		MethodInfo dummy_add = typeof (XamlMemberTest).GetMethod ("DummyAddMethod");
		MethodInfo dummy_get = typeof (XamlMemberTest).GetMethod ("DummyGetMethod");
		MethodInfo dummy_set = typeof (XamlMemberTest).GetMethod ("DummySetMethod");
		MethodInfo dummy_set2 = typeof (Dummy).GetMethod ("DummySetMethod");

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEventInfoNullEventInfo ()
		{
			new XamlMember ((EventInfo) null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEventInfoNullSchemaContext ()
		{
			new XamlMember (ass_load, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPropertyInfoNullPropertyInfo ()
		{
			new XamlMember ((PropertyInfo) null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPropertyInfoNullSchemaContext ()
		{
			new XamlMember (str_len, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAddMethodNullName ()
		{
			new XamlMember (null, GetType ().GetMethod ("DummyAddMEthod"), sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAddMethodNullMethod ()
		{
			new XamlMember ("DummyAddMethod", null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAddMethodNullSchemaContext ()
		{
			new XamlMember ("DummyAddMethod", dummy_add, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorGetSetMethodNullName ()
		{
			new XamlMember (null, dummy_get, dummy_set, sctx);
		}

		[Test]
		public void ConstructorGetSetMethodNullGetMethod ()
		{
			new XamlMember ("DummyProp", null, dummy_set, sctx);
		}

		[Test]
		public void ConstructorGetSetMethodNullSetMethod ()
		{
			new XamlMember ("DummyProp", dummy_get, null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorGetSetMethodNullGetSetMethod ()
		{
			new XamlMember ("DummyProp", null, null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorGetSetMethodNullSchemaContext ()
		{
			new XamlMember ("DummyProp", dummy_get, dummy_set, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameTypeNullName ()
		{
			new XamlMember (null, new XamlType (typeof (string), sctx), false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameTypeNullType ()
		{
			new XamlMember ("Length", null, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddMethodInvalid ()
		{
			// It is not of expected kind of member here:
			// "Attached property setter and attached event adder methods must have two parameters."
			new XamlMember ("AssemblyLoad", ass_load.GetAddMethod (), sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodInvlaid ()
		{
			// It is not of expected kind of member here:
			// "Attached property getter methods must have one parameter and a non-void return type."
			new XamlMember ("Length", sb_len.GetGetMethod (), null, sctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetMethodInvalid ()
		{
			// It is not of expected kind of member here:
			// "Attached property setter and attached event adder methods must have two parameters."
			new XamlMember ("Length", null, sb_len.GetSetMethod (), sctx);
		}

		[Test]
		public void MethodsFromDifferentType ()
		{
			// allowed...
			var i = new XamlMember ("Length", dummy_get, dummy_set2, sctx);
			Assert.IsNotNull (i.DeclaringType, "#1");
			// hmm...
			Assert.AreEqual (GetType (), i.DeclaringType.UnderlyingType, "#2");
		}

		// default values.

		[Test]
		public void EventInfoDefaultValues ()
		{
			var m = new XamlMember (typeof (AppDomain).GetEvent ("AssemblyLoad"), sctx);

			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (typeof (AppDomain), m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.IsNull (m.Invoker.UnderlyingGetter, "#3-2");
			Assert.AreEqual (ass_load.GetAddMethod (), m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (m.IsUnknown, "#4");
			Assert.IsFalse (m.IsReadPublic, "#5");
			Assert.IsTrue (m.IsWritePublic, "#6");
			Assert.AreEqual ("AssemblyLoad", m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", m.PreferredXamlNamespace, "#9");
			Assert.AreEqual (new XamlType (typeof (AppDomain), sctx), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (typeof (AssemblyLoadEventHandler), m.Type.UnderlyingType, "#11-2");
//			Assert.IsNotNull (m.TypeConverter, "#12"); // EventConverter
			Assert.IsNull (m.ValueSerializer, "#13");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.AreEqual (ass_load, m.UnderlyingMember, "#15");
			Assert.IsFalse (m.IsReadOnly, "#16");
			Assert.IsTrue (m.IsWriteOnly, "#17");
			Assert.IsFalse (m.IsAttachable, "#18");
			Assert.IsTrue (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}

		[Test]
		public void PropertyInfoDefaultValues ()
		{
			var m = new XamlMember (typeof (string).GetProperty ("Length"), sctx);

			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (typeof (string), m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.AreEqual (str_len.GetGetMethod (), m.Invoker.UnderlyingGetter, "#3-2");
			Assert.AreEqual (str_len.GetSetMethod (), m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (m.IsUnknown, "#4");
			Assert.IsTrue (m.IsReadPublic, "#5");
			Assert.IsFalse (m.IsWritePublic, "#6");
			Assert.AreEqual ("Length", m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, m.PreferredXamlNamespace, "#9");
			Assert.AreEqual (new XamlType (typeof (string), sctx), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (typeof (int), m.Type.UnderlyingType, "#11-2");
			Assert.IsNotNull (m.TypeConverter, "#12");
			Assert.IsNull (m.ValueSerializer, "#13");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.AreEqual (str_len, m.UnderlyingMember, "#15");
			Assert.IsTrue (m.IsReadOnly, "#16");
			Assert.IsFalse (m.IsWriteOnly, "#17");
			Assert.IsFalse (m.IsAttachable, "#18");
			Assert.IsFalse (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}

		public void DummyAddMethod (object o, AssemblyLoadEventHandler h)
		{
		}

		public int DummyGetMethod (object o)
		{
			return 5;
		}

		public void DummySetMethod (object o, int v)
		{
		}

		public class Dummy
		{
			public int DummyGetMethod (object o)
			{
				return 5;
			}

			public void DummySetMethod (object o, int v)
			{
			}
		}

		[Test]
		public void AddMethodDefaultValues ()
		{
			var m = new XamlMember ("DummyAddMethod", dummy_add, sctx);

			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (GetType (), m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.IsNull (m.Invoker.UnderlyingGetter, "#3-2");
			Assert.AreEqual (dummy_add, m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (m.IsUnknown, "#4");
			Assert.IsFalse (m.IsReadPublic, "#5");
			Assert.IsTrue (m.IsWritePublic, "#6");
			Assert.AreEqual ("DummyAddMethod", m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			var ns = "clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name;
			Assert.AreEqual (ns, m.PreferredXamlNamespace, "#9");
			// since it is unknown.
			Assert.AreEqual (new XamlType (typeof (object), sctx), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (typeof (AssemblyLoadEventHandler), m.Type.UnderlyingType, "#11-2");
//			Assert.IsNotNull (m.TypeConverter, "#12");
			Assert.IsNull (m.ValueSerializer, "#13");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.AreEqual (dummy_add, m.UnderlyingMember, "#15");
			Assert.IsFalse (m.IsReadOnly, "#16");
			Assert.IsTrue (m.IsWriteOnly, "#17");
			Assert.IsTrue (m.IsAttachable, "#18");
			Assert.IsTrue (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}

		[Test]
		public void GetSetMethodDefaultValues ()
		{
			var m = new XamlMember ("DummyProp", dummy_get, dummy_set, sctx);

			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (GetType (), m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.AreEqual (dummy_get, m.Invoker.UnderlyingGetter, "#3-2");
			Assert.AreEqual (dummy_set, m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsFalse (m.IsUnknown, "#4");
			Assert.IsTrue (m.IsReadPublic, "#5");
			Assert.IsTrue (m.IsWritePublic, "#6");
			Assert.AreEqual ("DummyProp", m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			var ns = "clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name;
			Assert.AreEqual (ns, m.PreferredXamlNamespace, "#9");
			// since it is unknown.
			Assert.AreEqual (new XamlType (typeof (object), sctx), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (typeof (int), m.Type.UnderlyingType, "#11-2");
//			Assert.IsNotNull (m.TypeConverter, "#12");
			Assert.IsNull (m.ValueSerializer, "#13");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.AreEqual (dummy_get, m.UnderlyingMember, "#15");
			Assert.IsFalse (m.IsReadOnly, "#16");
			Assert.IsFalse (m.IsWriteOnly, "#17");
			Assert.IsTrue (m.IsAttachable, "#18");
			Assert.IsFalse (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}

		[Test]
		public void NameTypeDefaultValues ()
		{
			var m = new XamlMember ("Length", new XamlType (typeof (string), sctx), false);

			Assert.IsNotNull (m.DeclaringType, "#2");
			Assert.AreEqual (typeof (string), m.DeclaringType.UnderlyingType, "#2-2");
			Assert.IsNotNull (m.Invoker, "#3");
			Assert.IsNull (m.Invoker.UnderlyingGetter, "#3-2");
			Assert.IsNull (m.Invoker.UnderlyingSetter, "#3-3");
			Assert.IsTrue (m.IsUnknown, "#4");
			Assert.IsTrue (m.IsReadPublic, "#5");
			Assert.IsTrue (m.IsWritePublic, "#6");
			Assert.AreEqual ("Length", m.Name, "#7");
			Assert.IsTrue (m.IsNameValid, "#8");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, m.PreferredXamlNamespace, "#9");
			Assert.AreEqual (new XamlType (typeof (string), sctx), m.TargetType, "#10");
			Assert.IsNotNull (m.Type, "#11");
			Assert.AreEqual (typeof (object), m.Type.UnderlyingType, "#11-2");
			Assert.IsNull (m.TypeConverter, "#12");
			Assert.IsNull (m.ValueSerializer, "#13");
			Assert.IsNull (m.DeferringLoader, "#14");
			Assert.IsNull (m.UnderlyingMember, "#15");
			Assert.IsFalse (m.IsReadOnly, "#16");
			Assert.IsFalse (m.IsWriteOnly, "#17");
			Assert.IsFalse (m.IsAttachable, "#18");
			Assert.IsFalse (m.IsEvent, "#19");
			Assert.IsFalse (m.IsDirective, "#20");
			Assert.IsNotNull (m.DependsOn, "#21");
			Assert.AreEqual (0, m.DependsOn.Count, "#21-2");
			Assert.IsFalse (m.IsAmbient, "#22");
		}

		[Test]
		public void UnderlyingMember ()
		{
			Assert.IsTrue (new XamlMember (ass_load, sctx).UnderlyingMember is EventInfo, "#1");
			Assert.IsTrue (new XamlMember (str_len, sctx).UnderlyingMember is PropertyInfo, "#2");
			Assert.AreEqual (dummy_get, new XamlMember ("DummyProp", dummy_get, dummy_set, sctx).UnderlyingMember, "#3");
			Assert.AreEqual (dummy_add, new XamlMember ("DummyAddMethod", dummy_add, sctx).UnderlyingMember, "#4");
			Assert.IsNull (new XamlMember ("Length", new XamlType (typeof (string), sctx), false).UnderlyingMember, "#5");
		}

		[Test]
		public void EqualsTest ()
		{
			XamlMember m;
			var xt = XamlLanguage.Type;
			m = new XamlMember ("Type", xt, false);
			var type_type = xt.GetMember ("Type");
			Assert.AreNotEqual (m, xt.GetMember ("Type"), "#1"); // whoa!
			Assert.AreNotEqual (type_type, m, "#2"); // whoa!
			Assert.AreEqual (type_type, xt.GetMember ("Type"), "#3");
			Assert.AreEqual (type_type.ToString (), m.ToString (), "#4");

			Assert.AreEqual (xt.GetAllMembers ().FirstOrDefault (mm => mm.Name == "Type"), xt.GetAllMembers ().FirstOrDefault (mm => mm.Name == "Type"), "#5");
			Assert.AreEqual (xt.GetAllMembers ().FirstOrDefault (mm => mm.Name == "Type"), xt.GetMember ("Type"), "#6");

			// different XamlSchemaContext
			Assert.AreNotEqual (m, XamlLanguage.Type.GetMember ("Type"), "#7");
			Assert.AreNotEqual (XamlLanguage.Type.GetMember ("Type"), new XamlSchemaContext ().GetXamlType (typeof (Type)).GetMember ("Type"), "#7");
			Assert.AreEqual (XamlLanguage.Type.GetMember ("Type"), new XamlSchemaContext ().GetXamlType (typeof (TypeExtension)).GetMember ("Type"), "#8");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}_Initialization", XamlLanguage.Initialization.ToString (), "#1");

			// Wow. Uncomment this, and it will show .NET returns the XamlMember.ToString() results *inconsistently*.
			//Assert.AreEqual ("System.Windows.Markup.XData", XamlLanguage.XData.ToString (), "#2pre");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, XamlLanguage.XData.PreferredXamlNamespace, "#2pre2");

			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}XData.Text", XamlLanguage.XData.GetMember ("Text").ToString (), "#2");

			var pi = typeof (string).GetProperty ("Length");
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}String.Length", new XamlMember (pi, sctx).ToString (), "#3");

			Assert.AreEqual ("System.Xaml.XamlSchemaContext.FooBar", new XamlMember ("FooBar", typeof (XamlSchemaContext).GetMethod ("GetPreferredPrefix"), null, sctx).ToString (), "#4");

			Assert.AreEqual ("{urn:foo}bar", new XamlDirective ("urn:foo", "bar").ToString (), "#5");
		}
	}
}
