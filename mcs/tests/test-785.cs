// Compiler options: -warnaserror

abstract class Base
{
	public abstract int Prop
	{
		get;
		set;
	}

	public abstract int this[int i]
	{
		get;
	}

	public abstract void TestVoid ();
	public abstract void TestInt (int i);
}

abstract class DeriveVTable : Base
{
	public override int Prop
	{
		get { return 1; }
	}

	public override int this[int i]
	{
		get { return 1; }
	}

	public override void TestVoid ()
	{
	}

	public override void TestInt (int i)
	{
	}
}

abstract class NewVTable : DeriveVTable
{
	public new abstract int Prop
	{
		get;
	}

	public new int this[int i]
	{
		get { return 2; }
	}

	public new void TestVoid ()
	{
	}

	public new void TestInt (int i)
	{
	}

	public void Overload ()
	{
	}

	public void Overload (int i)
	{
	}

	public static void Main ()
	{
	}
}
