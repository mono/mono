// cs0552.cs: user-defined conversion to/from interface
// Line: 12
//
using System;
using System.IO;

//
//
// Implicit conversion to an interface is not permitted
//
class NoIDispose {
	public static implicit operator IDisposable (NoIDispose a)
	{
		return a.x;
	}
}

