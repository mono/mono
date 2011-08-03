// Compiler options: -doc:xml-061.xml /warnaserror /warn:4

class Test
{
	static void Main ()
	{
	}
}

///<summary>summary</summary>
public interface Interface
{
	///<summary>Problem!</summary>
	int this[int index]
	{
		get;
	}
}

