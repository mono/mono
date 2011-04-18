// CS0552: User-defined conversion `NoIDispose.implicit operator System.IDisposable(NoIDispose)' cannot convert to or from an interface type
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

