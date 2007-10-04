// Compiler options: -target:library

using System;
using System.Runtime.CompilerServices;

public interface IA {
	[SpecialName]
	int GetLength ();
}
