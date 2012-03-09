// CS4008: Cannot await void method `X.Foo()'. Consider changing method return type to `Task'
// Line: 10

using System.Threading.Tasks;

class X
{
	static async void Test ()
	{
		await Foo ();
	}
	
	static async void Foo ()
	{
		await Task.FromResult (1);
	}
}
