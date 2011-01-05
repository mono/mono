// CS0308: The non-generic type `BaseGeneric<T>.InnerDerived' cannot be used with the type arguments
// Line: 19

class BaseGeneric<T>
{
	public class InnerDerived
	{
	}
}

class BaseConcrete : BaseGeneric<string>
{
}

class Program
{
    static void Main ()
    {
        new BaseConcrete.InnerDerived<int>();
    }
}
