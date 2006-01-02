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

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class PropertyInfoTest : Assertion 
	{
		[Test]
		public void GetAccessorsTest()
		{
			Type type = typeof(TestClass);
			PropertyInfo property = type.GetProperty ("ReadOnlyProperty");
        		MethodInfo[] methods = property.GetAccessors (true);

			AssertEquals ("GetAccessors#1", 1, methods.Length);
			AssertNotNull ("GetAccessors#2", methods[0]);
						
		}

#if NET_2_0
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
			AssertEquals (100, pi.GetValue (t, null));
			pi.SetValue (t, null, null);
			AssertEquals (null, pi.GetValue (t, null));
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
