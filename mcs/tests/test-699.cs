// Compiler options: -r:test-699-lib.dll

public class D : C
{
	string _message = "";

	public D (string msg)
	{
		_message = msg;
	}

	public string message
	{
		get { return _message; }
	}

	public static void Main ()
	{
	}
}
