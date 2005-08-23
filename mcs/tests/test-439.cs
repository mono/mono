using System;
public struct LayerMask
{
	private ushort mask;
	public static implicit operator int (LayerMask mask) { return (int)mask.mask; }
	public static implicit operator LayerMask (int intVal) 
	{ 
		LayerMask mask;
		mask.mask = unchecked ((ushort)intVal);
		return mask;
	}
}

class Test
{
	static private LayerMask test;
	static public void Main ()
	{
		LayerMask a = ~test;
		if (a != 0xFFFF) // LayerMask is an ushort internally
			throw new Exception ("");
	}
}
