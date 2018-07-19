// Compiler options: -langversion:latest

using System;

readonly struct S
{
	readonly int field;

	static int sf;
	static event Action e;
	static int Prop { get; set; }

	public static void Main ()
	{
	}
}