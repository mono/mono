// Compiler options: -t:library

using System;

public class BaseClassLibrary
{
	public int i;
	public virtual void Print (int arg) { Console.WriteLine ("BaseClass.Print"); i = arg; }
}
