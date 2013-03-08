// CS0131: The left-hand side of an assignment must be a variable, a property or an indexer
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
		var str = new MyStruct();
		str.Name = null;
	}
}
