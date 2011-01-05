// CS0108: `BaseConcrete.InnerDerived<T>()' hides inherited member `BaseGeneric<string>.InnerDerived'. Use the new keyword if hiding was intended
// Line: 14
// Compiler options: -warn:2 -warnaserror

class BaseGeneric<T>
{
	public class InnerDerived
	{
	}
}

class BaseConcrete : BaseGeneric<string>
{
	public void InnerDerived<T> ()
	{
	}
}
