// CS0111: A member `Blah.I.M<U>(int)' is already defined. Rename this member or use different parameter types
// Line : 12

public interface I
{
    void M<T> (int i);
}

public class Blah: I
{
        void I.M<T> (int i) {}
        void I.M<U> (int i) {}
}
