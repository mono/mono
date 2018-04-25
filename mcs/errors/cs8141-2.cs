// CS8141:
// Line: 11

public interface I<T>
{
	int this[T arg] { get; set; }
}

public class C : I<(int a, int b)>
{
	public int this[(int c, int d) arg] {
		get {
			return 1;
		}
		set {

		}
	}
}
