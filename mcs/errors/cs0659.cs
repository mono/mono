// CS0659: `E' overrides Object.Equals(object) but does not override Object.GetHashCode()
// Line: 13
// Compiler options: -warnaserror -warn:3

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
