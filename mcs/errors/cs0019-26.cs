// CS0019: Operator `&' cannot be applied to operands of type `C.Flags' and `int'
// Line: 16

using System;

class C
{
	enum Flags {
		Removed	= 0
	}
	
	public int	_enumFlags;
		
	internal void Close()
	{	
		if ((Flags.Removed & _enumFlags) == Flags.Removed)
			Console.WriteLine ("error");
	}
}
