// CS0115: `C.P' is marked as an override but no accessible `set' accessor found to override
// Line: 11

abstract class B
{
	public virtual int P {
		get;
		private set;
	}
}

class C : B
{
	public override int P {
		get { return 5; }
		set { }
	}
}