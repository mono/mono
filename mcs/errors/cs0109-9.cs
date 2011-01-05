// CS0109: The member `Derived.Action<T,U>' does not hide an inherited member. The new keyword is not required
// Line: 12
// Compiler options: -warnaserror -warn:4

public abstract class Base
{
	public delegate void Action<U> (U val);
}

public class Derived : Base
{
	public new delegate void Action<T, U> (U val);
}
