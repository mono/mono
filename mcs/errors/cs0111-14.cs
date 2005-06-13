// cs0111-14.cs: Type `Test' already defines a member called `set_Item' with the same parameter types
// Line : 6

public class Test
{
	public string this [int i] {
		get { return ""; }
	}
	public void set_Item (int i, string s) { }
}
