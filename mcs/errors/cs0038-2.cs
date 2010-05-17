// CS0038: Cannot access a nonstatic member of outer type `Outer' via nested type `Outer.Inner'
// Line: 33

public class Runner
{
	string msg;

	public Runner (string s)
	{
		msg = s;
	}

	public string Report ()
	{
		return msg;
	}
}

public class Outer
{
	private Runner r = new Runner ("Outer");

	public Runner Runner
	{
		get { return r; }
		set { r = value; }
	}

	class Inner
	{
		public string Check ()
		{
			return Runner.Report ();
		}
	}
}
