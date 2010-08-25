//
// PropertyInfoTest.cs - NUnit Test Cases for PropertyInfo
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004-2007 Gert Driesen
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection.Emit;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class PropertyInfoTest
	{
		[Test]
		public void GetAccessorsTest ()
		{
			Type type = typeof (TestClass);
			PropertyInfo property = type.GetProperty ("ReadOnlyProperty");

			MethodInfo [] methods = property.GetAccessors (true);
			Assert.AreEqual (1, methods.Length, "#A1");
			Assert.IsNotNull (methods [0], "#A2");
			Assert.AreEqual ("get_ReadOnlyProperty", methods [0].Name, "#A3");

			methods = property.GetAccessors (false);
			Assert.AreEqual (1, methods.Length, "#B1");
			Assert.IsNotNull (methods [0], "#B2");
			Assert.AreEqual ("get_ReadOnlyProperty", methods [0].Name, "#B3");

			property = typeof (Base).GetProperty ("P");

			methods = property.GetAccessors (true);
			Assert.AreEqual (2, methods.Length, "#C1");
			Assert.IsNotNull (methods [0], "#C2");
			Assert.IsNotNull (methods [1], "#C3");
			Assert.IsTrue (HasMethod (methods, "get_P"), "#C4");
			Assert.IsTrue (HasMethod (methods, "set_P"), "#C5");

			methods = property.GetAccessors (false);
			Assert.AreEqual (2, methods.Length, "#D1");
			Assert.IsNotNull (methods [0], "#D2");
			Assert.IsNotNull (methods [1], "#D3");
			Assert.IsTrue (HasMethod (methods, "get_P"), "#D4");
			Assert.IsTrue (HasMethod (methods, "set_P"), "#D5");

			methods = property.GetAccessors ();
			Assert.AreEqual (2, methods.Length, "#E1");
			Assert.IsNotNull (methods [0], "#E2");
			Assert.IsNotNull (methods [1], "#E3");
			Assert.IsTrue (HasMethod (methods, "get_P"), "#E4");
			Assert.IsTrue (HasMethod (methods, "set_P"), "#E5");

			property = typeof (TestClass).GetProperty ("Private",
				BindingFlags.NonPublic | BindingFlags.Instance);

			methods = property.GetAccessors (true);
			Assert.AreEqual (2, methods.Length, "#F1");
			Assert.IsNotNull (methods [0], "#F2");
			Assert.IsNotNull (methods [1], "#F3");
			Assert.IsTrue (HasMethod (methods, "get_Private"), "#F4");
			Assert.IsTrue (HasMethod (methods, "set_Private"), "#F5");

			methods = property.GetAccessors (false);
			Assert.AreEqual (0, methods.Length, "#G");

			methods = property.GetAccessors ();
			Assert.AreEqual (0, methods.Length, "#H");

#if NET_2_0
			property = typeof (TestClass).GetProperty ("PrivateSetter");

			methods = property.GetAccessors (true);
			Assert.AreEqual (2, methods.Length, "#H1");
			Assert.IsNotNull (methods [0], "#H2");
			Assert.IsNotNull (methods [1], "#H3");
			Assert.IsTrue (HasMethod (methods, "get_PrivateSetter"), "#H4");
			Assert.IsTrue (HasMethod (methods, "set_PrivateSetter"), "#H5");

			methods = property.GetAccessors (false);
			Assert.AreEqual (1, methods.Length, "#I1");
			Assert.IsNotNull (methods [0], "#I2");
			Assert.AreEqual ("get_PrivateSetter", methods [0].Name, "#I3");

			methods = property.GetAccessors ();
			Assert.AreEqual (1, methods.Length, "#J1");
			Assert.IsNotNull (methods [0], "#J2");
			Assert.AreEqual ("get_PrivateSetter", methods [0].Name, "#J3");
#endif
		}

		[Test]
		public void GetCustomAttributes ()
		{
			object [] attrs;
			PropertyInfo p = typeof (Base).GetProperty ("P");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#A1");
			Assert.AreEqual (typeof (ThisAttribute), attrs [0].GetType (), "#A2");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#A3");
			Assert.AreEqual (typeof (ThisAttribute), attrs [0].GetType (), "#A4");

			p = typeof (Base).GetProperty ("T");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (2, attrs.Length, "#B1");
			Assert.IsTrue (HasAttribute (attrs, typeof (ThisAttribute)), "#B2");
			Assert.IsTrue (HasAttribute (attrs, typeof (ComVisibleAttribute)), "#B3");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (2, attrs.Length, "#B41");
			Assert.IsTrue (HasAttribute (attrs, typeof (ThisAttribute)), "#B5");
			Assert.IsTrue (HasAttribute (attrs, typeof (ComVisibleAttribute)), "#B6");

			p = typeof (Base).GetProperty ("Z");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (0, attrs.Length, "#C1");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (0, attrs.Length, "#C2");
		}

		[Test]
		public void GetCustomAttributes_Inherited ()
		{
			object [] attrs;
			PropertyInfo p = typeof (Derived).GetProperty ("P");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (0, attrs.Length, "#A1");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (0, attrs.Length, "#A3");

			p = typeof (Derived).GetProperty ("T");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (2, attrs.Length, "#B1");
			Assert.IsTrue (HasAttribute (attrs, typeof (ThisAttribute)), "#B2");
			Assert.IsTrue (HasAttribute (attrs, typeof (ComVisibleAttribute)), "#B3");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (2, attrs.Length, "#B41");
			Assert.IsTrue (HasAttribute (attrs, typeof (ThisAttribute)), "#B5");
			Assert.IsTrue (HasAttribute (attrs, typeof (ComVisibleAttribute)), "#B6");

			p = typeof (Derived).GetProperty ("Z");

			attrs = p.GetCustomAttributes (false);
			Assert.AreEqual (0, attrs.Length, "#C1");
			attrs = p.GetCustomAttributes (true);
			Assert.AreEqual (0, attrs.Length, "#C2");
		}

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			Type derived = typeof (Derived);
			PropertyInfo pi = derived.GetProperty ("P");

			try {
				pi.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AccessorsReflectedType ()
		{
			PropertyInfo pi = typeof (Derived).GetProperty ("T");
			Assert.AreEqual (typeof (Derived), pi.GetGetMethod ().ReflectedType);
			Assert.AreEqual (typeof (Derived), pi.GetSetMethod ().ReflectedType);
		}

		[Test] // bug #399985
		public void SetValue_Enum ()
		{
			TestClass t = new TestClass ();
			PropertyInfo pi = t.GetType ().GetProperty ("Targets");
			pi.SetValue (t, AttributeTargets.Field, null);
			Assert.AreEqual (AttributeTargets.Field, t.Targets, "#1");
			pi.SetValue (t, (int) AttributeTargets.Interface, null);
			Assert.AreEqual (AttributeTargets.Interface, t.Targets, "#2");
		}

		public class ThisAttribute : Attribute
		{
		}

		class Base
		{
			[ThisAttribute]
			public virtual string P {
				get { return null; }
				set { }
			}

			[ThisAttribute]
			[ComVisible (false)]
			public virtual string T {
				get { return null; }
				set { }
			}

			public virtual string Z {
				get { return null; }
				set { }
			}
		}

		class Derived : Base
		{
			public override string P {
				get { return null; }
				set { }
			}
		}

		static void RunTest (Type t, bool use_getter) {
			var p = t.GetProperty ("Item");
			var idx = p.GetIndexParameters ();
			var m_args = t.GetMethod (use_getter ? "get_Item" : "set_Item").GetParameters ();

			Assert.AreEqual (2, idx.Length, "#1");

			Assert.AreEqual (typeof (double), idx [0].ParameterType, "#2");
			Assert.AreEqual (p, idx [0].Member, "#3");
			Assert.AreEqual ("a", idx [0].Name, "#4");
			Assert.AreEqual (0, idx [0].Position, "#5");
			Assert.AreEqual (m_args [0].MetadataToken, idx [0].MetadataToken, "#6");
			Assert.AreEqual (ParameterAttributes.None, idx [0].Attributes, "#7");

			Assert.AreEqual (typeof (string), idx [1].ParameterType, "#8");
			Assert.AreEqual (p, idx [1].Member, "#9");
			Assert.AreEqual ("b", idx [1].Name, "#10");
			Assert.AreEqual (1, idx [1].Position, "#11");
			Assert.AreEqual (m_args [1].MetadataToken, idx [1].MetadataToken, "#12");
			Assert.AreEqual (ParameterAttributes.None, idx [1].Attributes, "#13");

			var idx2 = p.GetIndexParameters ();

			//No interning exposed
			Assert.AreNotSame (idx, idx2, "#14");
			Assert.AreNotSame (idx [0], idx2 [1], "#15");
		}

		[Test]
		public void GetIndexParameterReturnsObjectsBoundToTheProperty ()
		{
			RunTest (typeof (TestA), false);
			RunTest (typeof (TestB), true);
		}

		public class TestA {
			public int this[double a, string b] {
				set {}
			}
		}

		public class TestB {
			public int this[double a, string b] {
				get { return 1; }
				set {}
			}
		}

		[Test]
		public void GetIndexParameterReturnedObjectsCustomAttributes () {
			var pa = typeof (TestC).GetProperty ("Item").GetIndexParameters () [0];
			Assert.IsTrue (pa.IsDefined (typeof (ParamArrayAttribute), false), "#1");

			var pb = typeof (TestD).GetProperty ("Item").GetIndexParameters () [0];
			Assert.IsTrue (pb.IsDefined (typeof (ParamArrayAttribute), false), "#2");

			Assert.AreEqual (1, Attribute.GetCustomAttributes (pa).Length, "#3");
			Assert.AreEqual (1, Attribute.GetCustomAttributes (pb).Length, "#4");

			Assert.AreEqual (0, pa.GetOptionalCustomModifiers ().Length, "#5");
			Assert.AreEqual (0, pb.GetRequiredCustomModifiers ().Length, "#6");
		}

		public class TestC {
			public int this[params double[] a] {
				get { return 99; }
			}
		}

		public class TestD {
			public int this[params double[] a] {
				set { }
			}
		}

		static string CreateTempAssembly ()
		{
			FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = Path.Combine (Path.GetTempPath (), "tmp" + num.ToString ("x") + ".dll");

				try {
					f = new FileStream (path, FileMode.CreateNew);
				} catch { }
			} while (f == null);

			f.Close ();


			return "tmp" + num.ToString ("x") + ".dll";
		}

		public class TestE {
			public int PropE {
				get { return 99; }
			}
		}

		[Test]
		public void ConstantValue () {
			/*This test looks scary because we can't generate a default value with C# */
			var assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.PropertyInfoTest";
			string an = CreateTempAssembly ();

			var assembly = Thread.GetDomain ().DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());
			var module = assembly.DefineDynamicModule ("module1", an);

			var tb = module.DefineType ("Test", TypeAttributes.Public);
			var prop = tb.DefineProperty ("Prop", PropertyAttributes.HasDefault, typeof (string), new Type [0]);

			var getter = tb.DefineMethod ("get_Prop", MethodAttributes.Public, typeof (string), new Type [0]);
			var ilgen = getter.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);

			var setter = tb.DefineMethod ("set_Prop", MethodAttributes.Public, null, new Type [1] { typeof (string) });
			setter.GetILGenerator ().Emit (OpCodes.Ret);

			prop.SetConstant ("test");
			prop.SetGetMethod (getter);
			prop.SetSetMethod (setter);

			tb.CreateType ();

			File.Delete (Path.Combine (Path.GetTempPath (), an));
			assembly.Save (an);

			var asm = Assembly.LoadFrom (Path.Combine (Path.GetTempPath (), an));
			var t = asm.GetType ("Test");
			var p = t.GetProperty ("Prop");
			Assert.AreEqual ("test", p.GetConstantValue (), "#1");

			File.Delete (Path.Combine (Path.GetTempPath (), an));

			var pa = typeof (TestE).GetProperty ("PropE");
			try {
				pa.GetConstantValue ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

#if NET_2_0
		public class A<T>
		{
			public string Property {
				get { return typeof (T).FullName; }
			}
		}

		public int? nullable_field;

		public int? NullableProperty {
			get { return nullable_field; }
			set { nullable_field = value; }
		}

		[Test]
		public void NullableTests ()
		{
			PropertyInfoTest t = new PropertyInfoTest ();

			PropertyInfo pi = typeof(PropertyInfoTest).GetProperty("NullableProperty");

			pi.SetValue (t, 100, null);
			Assert.AreEqual (100, pi.GetValue (t, null));
			pi.SetValue (t, null, null);
			Assert.AreEqual (null, pi.GetValue (t, null));
		}

		[Test]
		public void Bug77160 ()
		{
			object instance = new A<string> ();
			Type type = instance.GetType ();
			PropertyInfo property = type.GetProperty ("Property");
			Assert.AreEqual (typeof (string).FullName, property.GetValue (instance, null));
		}
#endif


		static bool HasAttribute (object [] attrs, Type attributeType)
		{
			foreach (object attr in attrs)
				if (attr.GetType () == attributeType)
					return true;
			return false;
		}

		static bool HasMethod (MethodInfo [] methods, string name)
		{
			foreach (MethodInfo method in methods)
				if (method.Name == name)
					return true;
			return false;
		}

		private class TestClass
		{
			private AttributeTargets _targets = AttributeTargets.Assembly;

			public AttributeTargets Targets {
				get { return _targets; }
				set { _targets = value; }
			}

			public string ReadOnlyProperty {
				get { return string.Empty; }
			}

			private string Private {
				get { return null; }
				set { }
			}

#if NET_2_0
			public string PrivateSetter {
				get { return null; }
				private set { }
			}
#endif
		}

		[Test] // bug #633671
		public void DeclaringTypeOfPropertyFromInheritedTypePointsToBase ()
		{
			var inherit1 = typeof(InheritsFromClassWithNullableDateTime);
			var siblingProperty = inherit1.GetProperty("Property1");

			Assert.AreEqual (typeof (ClassWithNullableDateTime), siblingProperty.DeclaringType, "#1");
			Assert.AreEqual (typeof (InheritsFromClassWithNullableDateTime), siblingProperty.ReflectedType, "#2");

			//The check is done twice since the bug is related to getting those 2 properties multiple times.
			Assert.AreEqual (typeof (ClassWithNullableDateTime), siblingProperty.DeclaringType, "#3");
			Assert.AreEqual (typeof (InheritsFromClassWithNullableDateTime), siblingProperty.ReflectedType, "#4");
		}
		
	
		public class ClassWithNullableDateTime
		{
			public DateTime? Property1 { get; set; }
		}
	
		public class InheritsFromClassWithNullableDateTime : ClassWithNullableDateTime
		{
		}
	}
}
