class A<T> where T : class {}
class B<T> : A<T> where T : class {}
class Test {
	internal static A<Test> x = new B<Test> ();
	public static void Main () { }
}
