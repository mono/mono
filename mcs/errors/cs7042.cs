// CS7042: The DllImport attribute cannot be applied to a method that is generic or contained in a generic type
// Line: 9

using System.Runtime.InteropServices;

public class C<T>
{
	[DllImport ("my.dll")]
	static extern void Foo ();
}