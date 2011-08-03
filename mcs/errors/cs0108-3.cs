// CS0108: `O.InnerAttribute' hides inherited member `Base.InnerAttribute()'. Use the new keyword if hiding was intended
// Line: 12
// Compiler options: -warnaserror -warn:2

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
