using System;

class FlagsAttributeDemo
{
    // Define an Enum with FlagsAttribute.
    [FlagsAttribute] 
    enum MultiHue : short
    {
        Black = 0,
        Red = 1,
        Green = 2,
        Blue = 4
    };

    public static int Main( )
    {
        string s = ((MultiHue)7).ToString ();
        
        Console.WriteLine (s);
        if (s != "Red, Green, Blue")
            return 1;
        return 0;
    } 
}
