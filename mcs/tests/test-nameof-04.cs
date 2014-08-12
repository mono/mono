interface IA
{
	void M (int arg);
	int Prop { get; set; }
}

interface IB
{
	void M (string arg);
	void M<T> (T arg);
	int Prop { get; set; }
}

interface I : IA, IB
{
	void Extra (string method = nameof (M), string prop = nameof (Prop));
}

class Ambiguous
{
	public static int Main ()
	{
		string res;
		res = nameof (I.M);
		if (res != "M")
			return 1;

		res = nameof (I.Prop);
		if (res != "Prop")
			return 2;

		return 0;
	}
}