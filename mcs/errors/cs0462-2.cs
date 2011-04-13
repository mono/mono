// CS0462: `B.this[int]' cannot override inherited members `A<T>.this[int]' and `A<T>.this[T]' because they have the same signature when used in type `B'
// Line: 12

abstract class A<T>
{
	public abstract int this[T t] { set; }
	public virtual bool this [int a] { set { } }
}

class B : A<int>
{
	public override int this [int a] { set {} }
}
