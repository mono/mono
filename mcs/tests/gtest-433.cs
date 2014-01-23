// Compiler options: -r:gtest-433-lib.dll

using C1 = Blah.Class1;
using C2 = Blah.Class2;
using Cit = Blah.Class2.Citrus;

public class M 
{
    public static void Main() 
    {
        // access an internal type
        C1 a = new C1();
        a.Test();

        C2 b = new C2();
        // access an internal member of a public type
        b.Test();

        Cit.Lime.ToString ();
    }
}
