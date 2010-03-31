// Compiler options: -target:library

public class B : A
{
	public override int Prop {
		get { return 3; }
	}
}

public class A
{
	public virtual int Prop {
		get { return 1; }
		set {}
	}
}
