// CS0082: A member `Test.set_Item(int, string)' is already reserved
// Line : 6

public class Test
{
	public string this [int i] {
		get { return ""; }
	}
	public void set_Item (int i, string s) { }
}
