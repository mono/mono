// CS0269: Use of unassigned out parameter `a'
// Line: 9

public class A
{
	void Test(out A a)
	{
		a.ToString ();
		a = null;
	}
}
