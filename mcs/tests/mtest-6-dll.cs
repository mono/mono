// Compiler options: -t:library

using System;
public abstract class MyTestAbstract
{
	protected abstract string GetName();
	
	public MyTestAbstract()
	{
	}

	public void PrintName()
	{
		Console.WriteLine("Name=" + GetName());
	}
}
