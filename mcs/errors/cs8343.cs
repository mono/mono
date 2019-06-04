// CS8343: `S': ref structs cannot implement interfaces
// Line: 7
// Compiler options: -langversion:latest

using System;

public ref struct S : IDisposable
{
	public void Dispose ()
	{			
	}
}
