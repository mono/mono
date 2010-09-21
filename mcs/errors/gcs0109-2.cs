// CS0109: The member `Derived<U>.Action' does not hide an inherited member. The new keyword is not required
// Line: 12
// Compiler options: -warnaserror -warn:4

public abstract class Base
{
	public delegate void Action<U>(U val);
}

public class Derived<U> : Base
{
	new internal Action<U> Action;
}
