// cs0108.cs: The new keyword is required on 'O.InnerAttribute()' because it hides inherited member
// Line: 12
// Compiler options: -t:library

using System;

public class Base
{
    public void InnerAttribute () {}
}

class O: Base
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InnerAttribute: Attribute {
    }        
}

class D {
	static void Main () {}
}
