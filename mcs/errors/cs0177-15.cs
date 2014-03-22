// CS0177: The out parameter `b' must be assigned to before control leaves the current method
// Line: 18

using System;

public class A
{
	private class C
	{
		public C(out string b)
		{
			b = "b";
		}
		public string D() {
			return "";
		}
	}
	private Func<String> E(out string b)
	{
		return new C(out b).D;
	}
}