// CS0155: The type caught or thrown must be derived from System.Exception
// Line: 9

class Test
{
    public static void Main ()
    {
    	try {}
    	catch (int[]) {}
    }
}

