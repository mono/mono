// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `T'
// Line: 7
// Compiler options: -unsafe

unsafe class Foo<T> where T : struct
{
	public T* Elements {
		get {
			return null;
		}
	}
}
