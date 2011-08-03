// CS0108: `Outer.Inner' hides inherited member `Base.Inner'. Use the new keyword if hiding was intended
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