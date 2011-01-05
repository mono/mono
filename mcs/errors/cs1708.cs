// CS1708: `S.array': Fixed size buffers can only be accessed through locals or fields
// Line: 27
// Compiler options: -unsafe

using System;

unsafe struct S
{
    public fixed int array [2];
}

class C
{
    unsafe public S Get ()
    {
	return new S ();
    }
}

public class Tester 
{
    public static void Main() { }
    
    unsafe void setName()
    {
	C c = new C();
	c.Get ().array [1] = 44;
    }
}

