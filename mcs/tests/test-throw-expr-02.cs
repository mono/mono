using System;

class Program
{
    public static int Main ()
    {
        Console.WriteLine (M (1));
        try {
            Console.WriteLine (M (null));
        } catch (Exception) {
            Console.WriteLine ("thrown");
            return 0;
        }

        return 1;
    }

    static string M (object data)
    {
        return data?.ToString () ?? throw null;
    }
}