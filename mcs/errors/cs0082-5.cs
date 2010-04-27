// CS0082: A member `Test.get_Value()' is already reserved
// Line: 7

public partial class Test
{
	public string get_Value () { return null; }
}

public partial class Test
{
	public string Value {
		get { }
	}
}
