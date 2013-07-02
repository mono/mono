// CS0761: Partial method declarations of `C.Foo<U>()' have inconsistent constraints for type parameter `U'
// Line: 8

partial class C
{
	partial void Foo<T> () where T : new ();
	
	partial void Foo<U> ()
	{
	}
}
