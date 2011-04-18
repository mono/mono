// CS0019: Operator `-' cannot be applied to operands of type `A' and `B'
// Line : 20

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
                System.Console.WriteLine (A.A1 - B.B1);
        }
}
