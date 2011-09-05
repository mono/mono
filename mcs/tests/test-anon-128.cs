abstract class A {
	public abstract void Foo<T>() where T : struct;
}

class B : A {
	public delegate void Del();

	public override void Foo<T>() 
	{
		Del d=delegate(){Foo<T>();};
	}

	public static void Main(){}
}

