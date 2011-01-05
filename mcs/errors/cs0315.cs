// CS0315: The type `int' cannot be used as type parameter `TEventArgs' in the generic type or method `System.EventHandler<TEventArgs>'. There is no boxing conversion from `int' to `System.EventArgs'
// Line: 5

class X {
	System.EventHandler <int> x;
	static void Main () {}
}


