enum A
{
        A1,
        A2
}

enum B
{
        B1,
        B2
}

class C
{
        static void Main ()
        {
                A a = A.A1;
                System.Console.WriteLine (a == B.B1);
        }
}
