// CS0592: The attribute `MyAttributeAttribute' is not valid on this declaration type. It is valid on `interface' declarations only
// Line : 8

using System;

public interface ITest
{
	[MyAttribute]
	void Method();
}

[AttributeUsage (AttributeTargets.Interface)]
public class MyAttributeAttribute : Attribute
{
}

public class Foo {

        public static void Main ()
        {
        }
}


