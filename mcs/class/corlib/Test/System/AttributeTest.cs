//
// AttributeTest.cs - NUnit Test Cases for the System.Attribute class
//
// Authors:
// 	Duco Fijma (duco@lorentz.xs4all.nl)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
//	(C) 2002 Duco Fijma
//	(c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Reflection;

namespace MonoTests.System
{

// Inner namespace for some test helper classes
using MonoTests.System.AttributeTestInternals;

namespace AttributeTestInternals
{

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
internal class MyCustomAttribute : Attribute {

	private string _info;

	public MyCustomAttribute (string info)
	{
		_info = info;
	}

	public string Info 
	{
		get {
			return _info;
		}
	}

}

[AttributeUsage(AttributeTargets.Class)]
internal class YourCustomAttribute : Attribute {
	
	private int _value;

	public YourCustomAttribute (int value) 
	{
		_value = value;
	}

	public int Value 
	{
		get {
			return _value;
		}
	}
}

[AttributeUsage(AttributeTargets.Class)]
internal class UnusedAttribute : Attribute {
}

[MyCustomAttribute("MyBaseClass"), YourCustomAttribute(37)]
internal class MyClass {
	int Value { get { return 42; }}
}

[MyCustomAttribute("MyDerivedClass")]
internal class MyDerivedClass : MyClass {
	public void Do () {}
}

} // Namespace MonoTests.System.AttributeTestInternals

[TestFixture]
public class AttributeTest : Assertion {
		
	public AttributeTest () {}

	public void TestIsDefined ()
	{
		AssertEquals ("A1", true, Attribute.IsDefined(typeof(MyDerivedClass), typeof(MyCustomAttribute)));
		AssertEquals ("A2", true, Attribute.IsDefined(typeof(MyDerivedClass), typeof(YourCustomAttribute)));
		AssertEquals ("A3", false, Attribute.IsDefined(typeof(MyDerivedClass), typeof(UnusedAttribute)));
		AssertEquals ("A4", true, Attribute.IsDefined(typeof(MyDerivedClass), typeof(MyCustomAttribute), true));
		AssertEquals ("A5", true, Attribute.IsDefined(typeof(MyDerivedClass), typeof(YourCustomAttribute), true));
		AssertEquals ("A6", false, Attribute.IsDefined(typeof(MyDerivedClass), typeof(UnusedAttribute), false));
		AssertEquals ("A7", true, Attribute.IsDefined(typeof(MyDerivedClass), typeof(MyCustomAttribute), false));
		AssertEquals ("A8", false, Attribute.IsDefined(typeof(MyDerivedClass), typeof(YourCustomAttribute), false));
		AssertEquals ("A9", false, Attribute.IsDefined(typeof(MyDerivedClass), typeof(UnusedAttribute), false));
	}

/*
	public static void TestIsDefaultAttribute () 
	{
		Console.WriteLine(">>>IsDefaultAttribute");

		Attribute a =  Attribute.GetCustomAttribute(typeof(MyClass), typeof(MyCustomAttribute));
		Console.WriteLine (a.IsDefaultAttribute() );
	}

	private static void WriteAttribute (Attribute a)
	{
		if (a == null) {
			Console.WriteLine ("NULL");
		}
		else {
			Console.WriteLine (a);
		}
	}
*/

	public void TestGetCustomAttribute ()
	{
		int i = 1;
		Type t = typeof(MyDerivedClass);
		try {
			AssertEquals ("A1",  "MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute(typeof(MyDerivedClass), typeof(MyCustomAttribute), false))).Info);
			i++;
			AssertEquals ("A2", null, ((YourCustomAttribute) (Attribute.GetCustomAttribute(typeof(MyDerivedClass), typeof(YourCustomAttribute), false))));
			i++;
			AssertEquals ("A3",  "MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute(typeof(MyDerivedClass), typeof(MyCustomAttribute)))).Info);
			i++;
			AssertNotNull ("A4a", Attribute.GetCustomAttribute(t, typeof(YourCustomAttribute)));
			i++;
			AssertEquals ("A4", 37, ((YourCustomAttribute) (Attribute.GetCustomAttribute(t, typeof(YourCustomAttribute)))).Value);
		} catch (Exception e) {
			Fail ("Unexpected exception thrown at i=" + i + " with t=" + t + ". e=" + e);
		}
	}
/*

	public static void WriteAttributes (Attribute[] attrs)
	{
		Console.WriteLine("length = {0}", attrs.Length);
		foreach (Attribute a in attrs) {
			WriteAttribute (a);
		}
	}


	private static void TestGetCustomAttributes ()
	{
		Console.WriteLine(">>>GetCustomAttributes");


		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass), typeof(MyCustomAttribute)));
		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass), typeof(MyCustomAttribute), true));
		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass), typeof(MyCustomAttribute), false));

		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass)));
		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass), true));
		WriteAttributes (Attribute.GetCustomAttributes(typeof(MyDerivedClass), false));
	}

*/

	/* Test for bug 54518 */
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PropTestAttribute : Attribute
	{
		public PropTestAttribute() {}
	}

	public class TestBase
	{
		public TestBase() {}
			
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
		public TestSub() {}
			
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
		object [] attrs;
		PropertyInfo [] props = typeof (TestSub).GetProperties (BindingFlags.Public | BindingFlags.Instance);
		
		foreach (PropertyInfo prop in props) {
			attrs = prop.GetCustomAttributes (typeof(PropTestAttribute), true);
			AssertEquals (prop.Name, true, attrs.Length > 0);
		}
	}
}

}
