class B : B2.IB
{
	public interface IA
	{
	}
	
	public static void Main ()
	{
	}
}

class B2 : A
{
	public interface IB
	{
	}
}

class A : G<int>
{
}

class G<T>
{
}