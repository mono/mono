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
		public void Select1Match ()
		{
			Type type = typeof (SingleIndexer);
			PropertyInfo [] props = type.GetProperties (BindingFlags.DeclaredOnly |
								    BindingFlags.Public |
								    BindingFlags.Instance);

			PropertyInfo prop = binder.SelectProperty (0, props, null, new Type [0], null);
			Assert.IsNull (prop, "empty");
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
	}
}

