// cs0659.cs: "'E' overrides Object.Equals(object o) but does not override Object.GetHashCode()
// Line: 13
// Compiler options: -warnaserror -warn:4

public class Base
{
    public override int GetHashCode ()
    {
        return 2;
    }
}

public class E: Base
{
    public override bool Equals (object o)
    {
        return true;
    }
}
