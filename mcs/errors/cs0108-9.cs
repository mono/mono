// cs0108.cs: The new keyword is required on 'Outer.Inner' because it hides inherited member
// Line: 13
// Compiler options: -warnaserror -warn:2

public class Base
{
    public int Inner { set { } }
}

class Outer: Base
{
    public void M () {}
    
    public class Inner
    {
    }
}