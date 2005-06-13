// cs0111-11.cs: Type `Blah' already defines a member called `I.M' with the same parameter types
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
