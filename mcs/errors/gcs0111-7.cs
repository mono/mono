// CS0111: A member `Blah.I.M<X>()' is already defined. Rename this member or use different parameter types
// Line : 12

public interface I
{
    void M<X> ();
}

public class Blah: I
{
        void I.M<X> () {}
        void I.M<X> () {}
}

