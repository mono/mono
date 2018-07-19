// CS0162: Unreachable code detected
// Line: 12
// Compiler options: -warnaserror -warn:2

using System;

class X
{
    void Test ()
    {
        var x = true ? throw new NullReferenceException () : 1;
        x = 2;
        return;
    }

	static void Main () 
	{
	}
}
