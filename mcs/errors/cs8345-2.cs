// CS8345: Field or auto-implemented property cannot be of type `S' unless it is an instance member of a ref struct
// Line: 11
// Compiler options: -langversion:latest

public ref struct S
{
}

ref struct Test
{
	static S field;
}