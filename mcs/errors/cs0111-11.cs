// cs111.cs : Class `Blah' already contains a definition with the same return value and parameter types for method 'I.M'
// Line : 12

public interface I
{
    void M ();
}

public class Blah: I
{
        void I.M () {}
        void I.M () {}
}
