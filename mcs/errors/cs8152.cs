// CS8152: `C' does not implement interface member `IA.Foo()' and the best implementing candidate `C.Foo()' return type `void' does not return by reference
// Line: 11

interface IA
{
	ref char Foo ();
}

public class C : IA
{
	public void Foo ()
	{
	}
}
