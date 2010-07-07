// CS0252: Possible unintended reference comparison. Consider casting the left side expression to type `A' to get value comparison
// Line: 15
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
		return obj != this;
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
