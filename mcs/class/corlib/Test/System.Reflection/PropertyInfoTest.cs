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


		private class TestClass 
		{
			public string ReadOnlyProperty 
			{
				get { return string.Empty; }
			}
		}
	}
}
