class BaseGeneric<T>
{
	public class InnerDerived
	{
		public InnerDerived (T t)
		{
		}
	}
	
	public class GenericInnerDerived<U>
	{
		public GenericInnerDerived (T t, U u)
		{
		}
	}
}

class BaseConcrete : BaseGeneric<string>
{
}

class Concrete_A : BaseGeneric<int>
{
}

class Concrete_B : BaseConcrete
{
	InnerDerived foo1;
}

class BaseGeneric_2<T, U>
{
	public class InnerDerived
	{
		public InnerDerived (T t, U u)
		{
		}
	}
}

class BaseGeneric_1<T> : BaseGeneric_2<T, string>
{
}

class Concrete_2 : BaseGeneric_1<bool>
{
}


class Program
{
    public static void Main ()
    {
		new Concrete_B.InnerDerived ("abc");
		new Concrete_A.InnerDerived (11);
		new Concrete_A.GenericInnerDerived<int> (1, 2);
		new Concrete_2.InnerDerived (false, "bb");
    }
}
