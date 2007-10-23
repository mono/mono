// CS0111: A member `Blah.I.M()' is already defined. Rename this member or use different parameter types
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
