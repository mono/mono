using System;
public abstract class MyTestAbstract
{
	internal abstract string GetName();
	
	public MyTestAbstract()
	{
	}

	public void PrintName()
	{
		Console.WriteLine("Name=" + GetName());
	}
}
