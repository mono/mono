// Compiler options: -checked
//
// from bug #706877
//


public class Bug
{
    public static int Main()
    {
        try
        {
            long x = long.MaxValue;
            System.Console.WriteLine(x+1);
        }
        catch(System.OverflowException ex)
        {
            return 0;
        }
        return 1;
    }
}

