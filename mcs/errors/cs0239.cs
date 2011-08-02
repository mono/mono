// CS0239: `X.MyMethod()': cannot override inherited member `Bar.MyMethod()' because it is sealed
// Line : 25

using System;

public class Foo {

	public virtual void MyMethod ()
	{
		Console.WriteLine ("This is me !");
	}
}
	      
public class Bar : Foo {

	public sealed override void MyMethod ()
	{

	}

} 

public class X : Bar {

	public override void MyMethod ()
	{

	}
	
	public static void Main ()
	{

	}
}
