using System;

class MainClass
{
	public static void Main()
	{
		Child test = new Child();
	}
}

class Parent 
{
	protected virtual string Property { get; }
}

class Child : Parent
{
	protected override string Property { get; }

	public Child () 
	{
		new AnotherClass{ field = Property = "success" };
		Console.WriteLine(Property);
	}
}

class AnotherClass 
{
	public string field;
}