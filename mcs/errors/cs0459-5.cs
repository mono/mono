// CS0459: Cannot take the address of `this' because it is read-only
// Line: 11
// Compiler options: -unsafe -langversion:latest

readonly struct X
{
	unsafe void Test ()
	{
		fixed (X* x = &this) {

		}
	}
}
