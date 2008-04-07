// CS0428: Cannot convert method group `Count' to non-delegate type `int'. Consider using parentheses to invoke the method
// Line: 11

using System.Linq;

public class A
{
	public A ()
	{
		string [] test = new string [5];
		A [] array = new A [test.Count];
	}
}
