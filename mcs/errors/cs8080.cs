// CS8080: `C.P': Auto-implemented properties must override all accessors of the overridden property
// Line: 11

abstract class B
{
	public virtual int P { get; private set; }
}

class C : B
{
	public override int P { get; }
}