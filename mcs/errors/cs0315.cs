// CS0315: The type `int' cannot be used as type parameter `TEventArgs' in the generic type or method `X.D<TEventArgs>'. There is no boxing conversion from `int' to `System.EventArgs'
// Line: 8

class X
{
	delegate void D<TEventArgs> () where TEventArgs : System.EventArgs;

	D<int> x;
}


