//
// AttributeTest.cs - NUnit Test Cases for the System.Attribute class
//
// Authors:
// 	Duco Fijma (duco@lorentz.xs4all.nl)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
//	(C) 2002 Duco Fijma
//	(c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Reflection;

namespace MonoTests.System
{
	using MonoTests.System.AttributeTestInternals;

	namespace AttributeTestInternals
	{
		[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		internal class MyCustomAttribute : Attribute
		{
			private string _info;

			public MyCustomAttribute (string info)
			{
				_info = info;
			}

			public string Info
			{
				get
				{
					return _info;
				}
			}
		}

		[AttributeUsage (AttributeTargets.Class)]
		internal class YourCustomAttribute : Attribute
		{
			private int _value;

			public YourCustomAttribute (int value)
			{
				_value = value;
			}

			public int Value
			{
				get
				{
					return _value;
				}
			}
		}

		[AttributeUsage (AttributeTargets.Class)]
		internal class UnusedAttribute : Attribute
		{
		}

		[MyCustomAttribute ("MyBaseClass"), YourCustomAttribute (37)]
		internal class MyClass
		{
			int Value { get { return 42; } }
		}

		[MyCustomAttribute ("MyDerivedClass")]
		internal class MyDerivedClass : MyClass
		{
			public void Do () { }
		}
	}

	[TestFixture]
	public class AttributeTest : Assertion
	{
		public AttributeTest () { }

		public void TestIsDefined ()
		{
			AssertEquals ("A1", true, Attribute.IsDefined (typeof(MyDerivedClass), typeof(MyCustomAttribute)));
			AssertEquals ("A2", true, Attribute.IsDefined (typeof(MyDerivedClass), typeof(YourCustomAttribute)));
			AssertEquals ("A3", false, Attribute.IsDefined (typeof(MyDerivedClass), typeof(UnusedAttribute)));
			AssertEquals ("A4", true, Attribute.IsDefined (typeof(MyDerivedClass), typeof(MyCustomAttribute), true));
			AssertEquals ("A5", true, Attribute.IsDefined (typeof(MyDerivedClass), typeof(YourCustomAttribute), true));
			AssertEquals ("A6", false, Attribute.IsDefined (typeof(MyDerivedClass), typeof(UnusedAttribute), false));
			AssertEquals ("A7", true, Attribute.IsDefined (typeof(MyDerivedClass), typeof(MyCustomAttribute), false));
			AssertEquals ("A8", false, Attribute.IsDefined (typeof(MyDerivedClass), typeof(YourCustomAttribute), false));
			AssertEquals ("A9", false, Attribute.IsDefined (typeof(MyDerivedClass), typeof(UnusedAttribute), false));
		}

		public void TestGetCustomAttribute ()
		{
			int i = 1;
			Type t = typeof(MyDerivedClass);
			try
			{
				AssertEquals ("A1", "MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(MyCustomAttribute), false))).Info);
				i++;
				AssertEquals ("A2", null, ((YourCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(YourCustomAttribute), false))));
				i++;
				AssertEquals ("A3", "MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(MyCustomAttribute)))).Info);
				i++;
				Console.WriteLine ("A4a");
				AssertNotNull ("A4a", Attribute.GetCustomAttribute (t, typeof(YourCustomAttribute)));
				i++;
				AssertEquals ("A4", 37, ((YourCustomAttribute) (Attribute.GetCustomAttribute (t, typeof(YourCustomAttribute)))).Value);
			}
			catch (Exception e)
			{
				Fail ("Unexpected exception thrown at i=" + i + " with t=" + t + ". e=" + e);
			}
		}

		/* Test for bug 54518 */
		[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
		public class PropTestAttribute : Attribute
		{
			public PropTestAttribute () { }
		}

		public class TestBase
		{
			public TestBase () { }

			[PropTest]
			public int PropBase1
			{
				get { return 0; }
				set { }
			}

			[PropTest]
			public string PropBase2
			{
				get { return ""; }
				set { }
			}
		}

		public class TestSub : TestBase
		{
			public TestSub () { }

			[PropTest]
			public int PropSub1
			{
				get { return 0; }
				set { }
			}

			[PropTest]
			public string PropSub2
			{
				get { return ""; }
				set { }
			}
		}

		[Test]
		public void BaseAttributes ()
		{
			object[] attrs;
			PropertyInfo[] props = typeof (TestSub).GetProperties (BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo prop in props)
			{
				attrs = prop.GetCustomAttributes (typeof(PropTestAttribute), true);
				AssertEquals (prop.Name, true, attrs.Length > 0);
			}
		}

		[Test]
		public void GetCustomAttributeOK ()
		{
			Attribute attribute = Attribute.GetCustomAttribute (typeof(ClassA),
				typeof(DerivedTestCustomAttributeInherit));
			AssertNotNull ("GetCustomAttributeNull", attribute);
		}

		[NUnit.Framework.Test]
		[ExpectedException (typeof(AmbiguousMatchException))]
		public void GetCustomAttributeAmbiguous ()
		{
			Attribute.GetCustomAttribute (typeof(ClassA), typeof(TestCustomAttribute));
		}

		[Test]
		public void GetCustomAttributeNull ()
		{
			Attribute attribute = Attribute.GetCustomAttribute (typeof(ClassA),
				typeof(DerivedTestCustomAttributeMultipleInherit));
			AssertNull ("GetCustomAttributeNull", attribute);
		}

		[Test]
		public void GetCustomAttributesTypeNoInherit ()
		{
			object[] attributes;

			attributes = Attribute.GetCustomAttributes (typeof(ClassA), false);
			AssertEquals ("GetCustomAttributesTypeNoInherit#1", 3, attributes.Length);

			AssertEquals ("GetCustomAttributesTypeNoInherit#2", 1, GetAttributeCount (
				attributes, typeof(TestCustomAttribute)));
			AssertEquals ("GetCustomAttributesTypeNoInherit#3", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultiple)));
			AssertEquals ("GetCustomAttributesTypeNoInherit#4", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeInherit)));

			attributes = Attribute.GetCustomAttributes (typeof(ClassB), false);
			AssertEquals ("GetCustomAttributesTypeNoInherit#5", 4, attributes.Length);

			AssertEquals ("GetCustomAttributesTypeNoInherit#2", 1, GetAttributeCount (
				attributes, typeof(TestCustomAttribute)));
			AssertEquals ("GetCustomAttributesTypeNoInherit#3", 2, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultiple)));
			AssertEquals ("GetCustomAttributesTypeNoInherit#4", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultipleInherit)));
		}

		[Test]
		public void GetCustomAttributesTypeInherit ()
		{
			object[] attributes;

			attributes = Attribute.GetCustomAttributes (typeof(ClassA), true);

			AssertEquals ("GetCustomAttributesTypeInherit#1", 3, attributes.Length);

			AssertEquals ("GetCustomAttributesTypeInherit#2", 1, GetAttributeCount (
				attributes, typeof(TestCustomAttribute)));
			AssertEquals ("GetCustomAttributesTypeInherit#3", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultiple)));
			AssertEquals ("GetCustomAttributesTypeInherit#4", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeInherit)));

			attributes = Attribute.GetCustomAttributes (typeof(ClassB), true);
			AssertEquals ("GetCustomAttributesTypeInherit#5", 5, attributes.Length);

			AssertEquals ("GetCustomAttributesTypeInherit#6", 1, GetAttributeCount (
				attributes, typeof(TestCustomAttribute)));
			AssertEquals ("GetCustomAttributesTypeInherit#7", 2, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultiple)));
			AssertEquals ("GetCustomAttributesTypeInherit#8", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeInherit)));
			AssertEquals ("GetCustomAttributesTypeInherit#9", 1, GetAttributeCount (
				attributes, typeof(DerivedTestCustomAttributeMultipleInherit)));
		}

		private int GetAttributeCount (object[] attributes, Type attributeType)
		{
			int counter = 0;

			foreach (Attribute attribute in attributes)
			{
				if (attribute.GetType () == attributeType)
				{
					counter++;
				}
			}

			return counter;
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true)]
		private class TestCustomAttribute : Attribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = false)]
		private class DerivedTestCustomAttributeMultiple : TestCustomAttribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true)]
		private class DerivedTestCustomAttributeInherit : TestCustomAttribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = true)]
		private class DerivedTestCustomAttributeMultipleInherit : TestCustomAttribute
		{
		}

		[TestCustomAttribute]
		[DerivedTestCustomAttributeMultiple]
		[DerivedTestCustomAttributeInherit]
		private class ClassA
		{
		}

		[TestCustomAttribute ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultipleInherit ()]
		private class ClassB : ClassA
		{
		}

		[TestCustomAttribute ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultipleInherit ()]
		private class ClassC : ClassB
		{
		}
	}
}
