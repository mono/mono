//
// System.Reflection.BinderTests - Tests Type.DefaultBinder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace MonoTests.System.Reflection
{
	enum MyEnum {
		Zero,
		One,
		Two
	}
	
	class SampleClass {
		public static void SampleMethod (object o) { }

		public Type this[decimal i] {
			get { return i.GetType (); }
		}

		public Type this[object i] {
			get { return i.GetType (); }
		}

	}
	
	class SingleIndexer {
		public Type this [int i] {
			get { return i.GetType (); }
		}
	}
	
	class MultiIndexer
	{
		public Type this[byte i] {
			get { return i.GetType (); }
		}

		public Type this[sbyte i] {
			get { return i.GetType (); }
		}

		public Type this[short i] {
			get { return i.GetType (); }
		}

		public Type this[ushort i] {
			get { return i.GetType (); }
		}

		public Type this[int i] {
			get { return i.GetType (); }
		}

		public Type this[uint i] {
			get { return i.GetType (); }
		}

		public Type this[long i] {
			get { return i.GetType (); }
		}

		public Type this[ulong i] {
			get { return i.GetType (); }
		}

		public Type this[float i] {
			get { return i.GetType (); }
		}

		public Type this[double i] {
			get { return i.GetType (); }
		}

		public Type this[decimal i] {
			get { return i.GetType (); }
		}

		public Type this[object i] {
			get { return i.GetType (); }
		}

		public Type this[Enum i] {
			get { return i.GetType (); }
		}
	}

	[TestFixture]
	public class BinderTest
	{
		Binder binder = Type.DefaultBinder;

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectPropertyTestNull1 ()
		{
			// The second argument is the one
			binder.SelectProperty (0, null, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectPropertyTestEmpty ()
		{
			// The second argument is the one
			binder.SelectProperty (0, new PropertyInfo [] {}, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (AmbiguousMatchException))]
		public void AmbiguousProperty1 () // Bug 58381
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo pi = type.GetProperty ("Item");
		}

		[Test]
		public void SelectAndInvokeAllProperties1 ()
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			// These don't cause an AmbiguousMatchException
			Type [] types = { typeof (byte), typeof (short),
					  typeof (int), typeof (long),
					  typeof (MyEnum) };

			/* MS matches short for sbyte!!! */
			/* MS matches int for ushort!!! */
			/* MS matches long for uint!!! */
			/** These do weird things under MS if used together and then in separate arrays *
			Type [] types = { typeof (ulong), typeof (float), typeof (double),
					  typeof (decimal), typeof (object) };
			*/

			MultiIndexer obj = new MultiIndexer ();

			foreach (Type t in types) {
				PropertyInfo prop = null;
				try {
					prop = binder.SelectProperty (0, props, null, new Type [] {t}, null);
				} catch (Exception e) {
					throw new Exception ("Type: " + t, e);
				}
				Type gotten = (Type) prop.GetValue (obj, new object [] {Activator.CreateInstance (t)});
				Assert.AreEqual (t, gotten);
			}
		}

		[Test]
		public void SelectAndInvokeAllProperties2 ()
		{
			Type type = typeof (MultiIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			Type [] types = { typeof (ushort), typeof (char) };

			MultiIndexer obj = new MultiIndexer ();
			PropertyInfo prop1 = binder.SelectProperty (0, props, null, new Type [] {types [0]}, null);
			PropertyInfo prop2 = binder.SelectProperty (0, props, null, new Type [] {types [1]}, null);
			Assert.AreEqual (prop1, prop2);
		}

		[Test]
		public void Select1Match2 ()
		{
			Type type = typeof (SingleIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);
			PropertyInfo prop = binder.SelectProperty (0, props, null, new Type [0], null);
			Assert.IsNull (prop, "empty");
		}
		
		[Test]
		public void Select1Match ()
		{
			Type type = typeof (SingleIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			PropertyInfo prop;
			
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (long) }, null);
			Assert.IsNull (prop, "long");
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (int) }, null);
			Assert.IsNotNull (prop, "int");
			prop = binder.SelectProperty (0, props, null, new Type [] { typeof (short) }, null);
			Assert.IsNotNull (prop, "short");
		}

		[Test]
		public void ArgNullOnMethod () // see bug 58846. We throwed nullref here.
		{
			Type type = typeof (SampleClass);
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
			type.InvokeMember ("SampleMethod", flags, null, null, new object[] { null });
		}

		[Test]
		public void ArgNullOnProperty ()
		{
			Type type = typeof (SampleClass);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			PropertyInfo prop = binder.SelectProperty (0, props, null, new Type [] {null}, null);
			Assert.IsNotNull (prop);
		}

		[Test] // bug #41691
		public void BindToMethodNamedArgs ()
		{
			Type t = typeof (Bug41691);

			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";

			object[] argValues = new object [] {"Hello", "World", "Extra", sw};
			string [] argNames = new string [] {"firstName", "lastName"};

			t.InvokeMember ("PrintName",
					BindingFlags.InvokeMethod,
					null,
					null,
					argValues,
					null,
					null,
					argNames);

			Assert.AreEqual ("Hello\nExtra\nWorld\n", sw.ToString ());
		}

		public class Bug41691
		{
			public static void PrintName (string lastName, string firstName, string extra, TextWriter output)
			{
				output.WriteLine (firstName);
				output.WriteLine (extra);
				output.WriteLine (lastName);
			}
		}

		[Test] // bug #42457
		public void GetMethodAmbiguity ()
		{
			object IntegerObject = 5;
			object IntArrayObject = new int[] {5, 2, 5};
			object StringArrayObject = new string [] {"One", "Two"};
			object [] IntParam = new object [] {IntegerObject};
			object [] IntArrayParam = new object [] {IntArrayObject};
			object [] StringArrayParam = new object [] {StringArrayObject};

			object be = this;
			Type betype = this.GetType ();

			string name1 = "Bug42457Method";
			string name2 = "Bug42457Method2";

			MethodInfo mi_obj = betype.GetMethod (name1, Type.GetTypeArray (IntParam));
			mi_obj.Invoke (be, IntParam);
			Assert.AreEqual (1, bug42457, "#1");
			MethodInfo mi_arr = betype.GetMethod (name1, Type.GetTypeArray (IntArrayParam));
			mi_arr.Invoke (be, IntArrayParam);
			Assert.AreEqual (2, bug42457, "#2");
			MethodInfo mi_str = betype.GetMethod (name1, Type.GetTypeArray (StringArrayParam));
			mi_str.Invoke (be, StringArrayParam);
			Assert.AreEqual (3, bug42457, "#3");

			MethodInfo m2_obj = betype.GetMethod (name2, Type.GetTypeArray (IntParam));
			m2_obj.Invoke (be, IntParam);
			Assert.AreEqual (1, bug42457_2, "#4");
			MethodInfo m2_arr = betype.GetMethod (name2, Type.GetTypeArray (IntArrayParam));
			m2_arr.Invoke (be, IntArrayParam);
			Assert.AreEqual (2, bug42457_2, "#5");
			MethodInfo m2_str = betype.GetMethod (name2, Type.GetTypeArray(StringArrayParam));
			m2_str.Invoke (be, StringArrayParam);
			Assert.AreEqual (3, bug42457_2, "#6");
		}

		static void MethodWithLongParam(long param)
		{
		}

		[Test]
		public void TestExactBinding ()
		{
			Type[] types = new Type[] { typeof(int) };
			Assert.AreEqual (null, typeof (BinderTest).GetMethod("MethodWithLongParam", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.ExactBinding,  null, types, null));
		}

		public void Bug42457Method (object thing)
		{
			bug42457 = 1;
		}

		public void Bug42457Method (Array thing)
		{
			bug42457 = 2;
		}

		public void Bug42457Method (string [] thing)
		{
			bug42457 = 3;
		}

		public void Bug42457Method2 (object thing)
		{
			bug42457_2 = 1;
		}

		public void Bug42457Method2 (Array thing)
		{
			bug42457_2 = 2;
		}

		public void Bug42457Method2 (string [] thing)
		{
			bug42457_2 = 3;
		}

		int bug42457, bug42457_2;

		[Test] // bug #77079
		public void GetMethodAvoidAmbiguity2 ()
		{
			Type tType = this.GetType ();
			Bug77079A a = new Bug77079C ();

			tType.InvokeMember ("Bug77079",
				BindingFlags.Public | BindingFlags.InvokeMethod | 
				BindingFlags.Instance,
				null, this, new object[] {a});
			Assert.AreEqual (2, bug77079);
		}

		int bug77079;

		public void Bug77079 (Bug77079A a)
		{
			bug77079 = 1;
		}

		public void Bug77079 (Bug77079B a)
		{
			bug77079 = 2;
		}

		public class Bug77079A
		{
		}

		public class Bug77079B : Bug77079A
		{
		}

		public class Bug77079C : Bug77079B
		{
		}

		[Test] // bug #76083
		public void GetMethodAvoidAmbiguity3 ()
		{
			Type[] types = new Type[] { typeof (Bug76083ArgDerived) };
			MethodInfo m = typeof (Bug76083Derived).GetMethod ("Foo", types);
			Assert.AreEqual (typeof (Bug76083Derived), m.DeclaringType);
		}

		public class Bug76083ArgBase {}
		public class Bug76083ArgDerived : Bug76083ArgBase {}

		public class Bug76083Base
		{
			public void Foo (Bug76083ArgBase a) {}
		}

		public class Bug76083Derived : Bug76083Base
		{
			public new void Foo (Bug76083ArgBase a) {}
		}
	}
}

