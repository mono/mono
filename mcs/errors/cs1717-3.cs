// CS1717: Assignment made to same variable; did you mean to assign something else?
// Line: 9
// Compiler options: -warnaserror -warn:3

class A
{    
    int i;
    
    void C ()
    {
	i = i;
    }
}