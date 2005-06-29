// cs0111-14.cs: `Test.set_Item(int, string)' is already defined. Rename this member or use different parameter types
// Line : 6

public class Test
{
	public string this [int i] {
		get { return ""; }
	}
	public void set_Item (int i, string s) { }
}
