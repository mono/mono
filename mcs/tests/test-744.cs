class M
{
	sealed class Nested : C
	{
		protected override void Extra ()
		{
		}
	}
	
	public static void Main ()
	{
		new Nested ();
	}
}

abstract class A
{
	protected abstract void AMethod ();
}

abstract class B : A
{
	protected abstract void BMethod ();
}

abstract class C : B
{
	protected override void AMethod ()
	{
	}
	
	protected override void BMethod ()
	{
	}
	
	protected abstract void Extra ();
}
