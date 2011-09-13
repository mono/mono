// CS0177: The out parameter `s' must be assigned to before control leaves the current method
// Line: 17

public class C
{
}

public struct S
{
	public C c;
}

public class Test
{
	void M (out S s)
	{
		var xx = s.c;
	}
}
