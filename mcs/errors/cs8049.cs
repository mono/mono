// CS8049: Implemented interfaces cannot have arguments
// Line: 6
// Compiler options: -langversion:experimental

using System;

class ID () : IDisposable ()
{
	public void Dispose ()
	{
	}
}
