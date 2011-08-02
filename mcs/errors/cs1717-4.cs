// CS1717: Assignment made to same variable; did you mean to assign something else?
// Line: 9
// Compiler options: -warnaserror -warn:3

class A
{    
    static int Value;
    
    void D ()
    {
	A.Value = Value;
    }
}