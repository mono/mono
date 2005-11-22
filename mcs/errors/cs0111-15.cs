// cs0111-15.cs: `I.set_Item(int[], params int[])' is already defined. Rename this member or use different parameter types
// Line : 7

interface I
{
	void set_Item (int[] a, params int[] b);
	int[] this [params int[] ii] { get; }
}
