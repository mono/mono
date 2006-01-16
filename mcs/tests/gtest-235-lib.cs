// Compiler options: /t:library
using System;

public class MyList<T>
{ 
	public void AddAll<U> ()
		where U : T
	{
	}
}
