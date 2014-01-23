// CS0761: Partial method declarations of `C.Foo<T>()' have inconsistent constraints for type parameter `T'
// Line: 8

partial class C
{
	partial void Foo<U> ();
	
	partial void Foo<T> () where T : class
	{
	}
}
