// CS0201: Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement
// Line: 9

class D
{
	void Foo ()
	{
		System.Threading.Tasks.TaskFactory m = null;
		m.StartNew (() => delegate { });
	}
}
