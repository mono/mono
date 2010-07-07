// CS0253: Possible unintended reference comparison. Consider casting the right side expression to type `A' to get value comparison
// Line: 16
// Compiler options: -warnaserror

using System;

class A
{
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
	
	public override bool Equals (object obj)
	{
		return this == obj;
	}
	
	public static bool operator == (A left, A right)
	{
		return true;
	}
	
	public static bool operator != (A left, A right)
	{
		return false;
	}
}
