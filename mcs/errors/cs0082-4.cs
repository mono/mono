// CS0082: A member `I.set_Item(int[], params int[])' is already reserved
// Line : 7

interface I
{
	void set_Item (int[] a, params int[] b);
	int[] this [params int[] ii] { get; }
}
