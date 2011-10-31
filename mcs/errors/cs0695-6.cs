// CS0695: `Test<T>.TestN' cannot implement both `C<T>.I' and `C<string>.I' because they may unify for some type parameter substitutions
// Line: 17

class C<T>
{
	public interface I
	{
	}
	
	public class N : C<string>
	{
	}
}

class Test<T> : C<T>
{
	class TestN : I, N.I
	{
	}
}
