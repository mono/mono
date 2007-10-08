// CS1501: No overload for method `this' takes `2' arguments
// Line : 10

class C
{
	public bool this [int i] { get { return false; } set {} }
	
	void Foo ()
	{	C c = new C ();
		c [0, 0] = null;
	}
}
