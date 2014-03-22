using System;

public class C
{
	public C (out string b)
	{
		b = "";
	}
	public string D () {
		return "";
	}
}
public class A
{
	public Func<String> E (out string b)
	{
		return new C (out b).D;
	}
	public Func<String> F (out string b)
	{
		return new Func<String> (new C (out b).D);
	}
	static void Main () {
	}
}