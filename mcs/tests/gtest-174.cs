// Compiler options: /r:gtest-174-lib.dll
public class B<T> {
	public static B<T> _N_constant_object = new B<T> ();
}

class M {
	public static void Main () {
		A<int> x = A<int>._N_constant_object;
		B<int> y = B<int>._N_constant_object;
	}
}
