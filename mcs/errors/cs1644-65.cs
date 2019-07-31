// CS1644: Feature `expression body property accessor' cannot be used because it is not part of the C# 6.0 language specification 
// Line: 11
// Compiler options: -langversion:6

using System;

class C
{
	public int this[int i]
	{
		get => i;
	}
}