// CS0462: `B.M(int)' cannot override inherited members `A<T>.M(int)' and `A<T>.M(T)' because they have the same signature when used in type `B'
// Line: 12

abstract class A<T>
{
	public abstract void M (T t);
	public virtual void M (int t) { }
}

class B : A<int>
{
	public override void M (int t) { }
}
