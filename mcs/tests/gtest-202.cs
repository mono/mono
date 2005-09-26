public class Generic<T>
{
	private T[,] container = new T[1,1];
	
	public T this [int row, int col]
	{
		get {
			return container[row, col];
		}
		set {
			container[row, col] = value;
		}
	}
}

public struct Fault
{
	public static void Main ()
	{
		Generic<Fault> gen = new Generic<Fault> ();
		gen[0, 0] = new Fault ();
		System.Console.WriteLine (gen[0, 0].ToString ());
	}
	
	public override string ToString ()
	{
		return "Hi!";
	}
}
