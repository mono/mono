using System;

public abstract class Base
{
	public abstract T G<T> (T t) where T : IComparable;
}

class Derived : Base
{
	// CS0425: The constraints of type parameter `T' of method `G' must match the
	// constraints for type parameter `T' of method `Base.G'
	public override T G<T> (T t)
	{
		return t;
	}
}
