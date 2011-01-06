// CS0108: `Derived.Action<U>' hides inherited member `Base.Action<U>'. Use the new keyword if hiding was intended
// Line: 12
// Compiler options: -warnaserror

public abstract class Base
{
	public delegate void Action<U> (U val);
}

public class Derived : Base
{
	public delegate void Action<U> (U i);
}
