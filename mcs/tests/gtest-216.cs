// bug #76382
// differentiate Foo() and Foo<>() on override resolution.
public interface Ret { }
public interface Ret<T> {}

public abstract class BaseClass
{
	public virtual Ret Foo () { return null; }
	public virtual Ret<T> Foo<T> () { return null; }

	public static void Main () {}
}

public class DerivedClass : BaseClass
{
	public override Ret Foo () { return null; }
	public override Ret<T> Foo<T> () { return null; }
}
