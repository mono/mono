// CS0205: Cannot call an abstract base member `A.OnUpdate'
// Line: 17

public delegate int TestDelegate1 ();

public abstract class A
{
	public abstract event TestDelegate1 OnUpdate;
}

public class B : A
{
	public override event TestDelegate1 OnUpdate
	{
		add
		{
			base.OnUpdate += value;
		}
		remove
		{
			base.OnUpdate -= value;
		}
	}
}
