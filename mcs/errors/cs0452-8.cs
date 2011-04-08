// CS0452: The type `ulong' must be a reference type in order to use it as type parameter `T' in the generic type or method `C<T>'
// Line: 10

public class C<T> where T : class
{
	public int this [params C<ulong>[] args] {
		set {}
	}
}

