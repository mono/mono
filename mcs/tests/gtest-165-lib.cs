// Compiler options: -t:library

public class A<T>
{
	public int this [T arg] {
		get {
			return 1;
		}
	}
	
	public int this [string arg] {
		get {
			return 2;
		}
	}
}
