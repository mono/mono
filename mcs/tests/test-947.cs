interface IA
{
	int Prop { get; }
	int this [int arg] { get; }
}

abstract class B : IA
{
    public long Prop => 4;

	int IA.Prop => 1;

	public long this [int arg] => 2;

	int IA.this [int arg] => 4;
}

class C : B, IA
{
	public static void Main ()
	{
	}

	public new string Prop {
		get { return ""; }
	}

	public new string this [int arg] => "2";
}