//
// AttributeTest.cs - NUnit Test Cases for the System.Attribute class
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using NUnit.Framework;
using System;

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

public class AttributeTest : TestCase {
		
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

}

}
