// CS0465: Introducing `Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?
// Line: 7
// Compiler options: -warnaserror -warn:1

class T
{	
    static void Finalize ()
    {
    }
}
