// Compiler options: -unsafe

struct Obsolete {
	int a;
}
struct A {
	int a, b;
}

class MainClass {
        unsafe public static void Main ()
        {
                System.Console.WriteLine (sizeof (Obsolete));
        }
}


