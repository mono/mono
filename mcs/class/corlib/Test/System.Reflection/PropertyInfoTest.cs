//
// PropertyInfoTest.cs - NUnit Test Cases for PropertyInfo
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell 
//

using System;
using System.Reflection; 
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class PropertyInfoTest
	{
		[Test]
		public void GetAccessorsTest()
		{
			Type type = typeof(TestClass);
			PropertyInfo property = type.GetProperty ("ReadOnlyProperty");
        		MethodInfo[] methods = property.GetAccessors (true);

			Assert.AreEqual (1, methods.Length, "GetAccessors#1");
			Assert.IsNotNull (methods[0], "GetAccessors#2");
						
		}

		[Test]
		public void GetCustomAttributesInherited ()
		{
			Type derived = typeof (Derived);
			PropertyInfo p = derived.GetProperty ("P");

			Assert.AreEqual (2, p.GetCustomAttributes (true).Length);
		}
			
		class Base {
			[DesignOnly (true)]
			[Description ("Desc")]
			public virtual string P {
				get { return null; }
				set { }
			}
		}

		class Derived : Base {
			public override string P {
				get { return null; }
				set { }
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

		private class TestClass 
		{
			public string ReadOnlyProperty 
			{
				get { return string.Empty; }
			}
		}
	}
}
