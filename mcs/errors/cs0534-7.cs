// CS0534: `B' does not implement inherited abstract member `A<int>.set_Prop(int)'
// Line: 13

abstract class A<T>
{
	public abstract void set_Prop (int value);

	public virtual T Prop {
		set { }
	}
}

class B : A<int>
{
	public override int Prop {
		set { }
	}
}
