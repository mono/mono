namespace MyNamespace {
	public class MyClass {
		public string SomeMethod<T>() { return string.Empty; }

		#if V2
		public string SomeMethod() { return string.Empty; }
		#endif
	}
}
