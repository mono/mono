// CS4012: Parameters or local variables of type `S' cannot be declared in async methods or iterators
// Line: 16
// Compiler options: -langversion:latest

using System;
using System.Threading.Tasks;

public ref struct S
{
}

class C
{
	public async void Test ()
	{
		var tr = new S ();
		await Task.Factory.StartNew (() => 6);
	}
}