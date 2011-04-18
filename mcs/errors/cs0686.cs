// CS0686: Accessor `C.Method.get' cannot implement interface member `I.get_Method()' for type `C'. Use an explicit interface implementation
// Line: 13

interface I
{
    int get_Method ();
}

class C: I
{
    public int Method
    {
	get { return -1; }
    }
}
