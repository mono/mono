interface IA
{
	string val { get; set; }
}

interface IAI : IA
{
	new int val { get; set; }
}

interface IAI2 : IAI { }

class AI2 : IAI2
{
	public int val { get { return 42; } set { } }
	string IA.val { get { return "13"; } set { } }

	public void stuff (IAI2 other)
	{
		val = other.val;
	}

	public static void Main ()
	{
	}
}
