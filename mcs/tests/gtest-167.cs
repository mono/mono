// Conditional attribute class test
#define DEBUG

using System;
using System.Diagnostics;

[Conditional("DEBUG")]
public class TestAttribute : Attribute {}

[Conditional("RELEASE")]
public class TestNotAttribute : Attribute {}

[Conditional("A")]
[Conditional("DEBUG")]    
[Conditional("B")]
public class TestMultiAttribute : Attribute {}
    
// TestAttribute is included
[Test]				
class Class1 {}
    
// TestNotAttribute is not included
[TestNot] 			
class Class2 {}

// Is included    
[TestMulti]
class Class3 {}


public class TestClass
{
    public static int Main ()
    {
	if (Attribute.GetCustomAttributes (typeof (Class1)).Length != 1)
		return 1;

	if (Attribute.GetCustomAttributes (typeof (Class2)).Length != 0)
		return 1;

	if (Attribute.GetCustomAttributes (typeof (Class3)).Length != 1)
		return 1;
	
	Console.WriteLine ("OK");
	return 0;
    }
}
