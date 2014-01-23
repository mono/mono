// CS0029: Cannot implicitly convert type `char' to `char*'
// Line: 15
// Compiler options: -unsafe

unsafe struct MyStruct
{
	public fixed char Name[32];
}

unsafe class MainClass
{
	public static void Main ()
	{
		var str = new MyStruct ();
		str.Name = default (char);
	}
}
