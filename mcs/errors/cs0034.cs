// CS0034: Operator `!=' is ambiguous on operands of type `Program.A' and `Program.B'
// Line: 36

using System;

class Program
{
	public class A
	{
		public static implicit operator string (A c)
		{
			return null;
		}
		
		public static implicit operator Delegate (A c)
		{
			return null;
		}
	}
	
	public class B
	{
		public static implicit operator string (B c)
		{
			return null;
		}
		
		public static implicit operator Delegate (B c)
		{
			return null;
		}
	}

	public static void Main (string [] args)
	{
		bool b = new A () != new B ();
	}
}
