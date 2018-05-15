// Compiler options: -langversion:7.2

public class B
{
	private protected enum E
	{
	}

	public int Index { get; protected private set; }

	internal string S1 { get; private protected set; }

	protected string S2 { get; private protected set; }

	private protected int field;

	public static void Main ()
	{
	}
}