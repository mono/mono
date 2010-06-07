// CS0534: `C1' does not implement inherited abstract member `A.M(int)'
// Line: 16

public abstract class A
{
	public abstract void M (int i);
}

internal class C0 : A
{
	public override void M (int i)
	{
	}
}

internal class C1 : A
{
}
