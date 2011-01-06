// CS0111: A member `C.I<int>.Prop' is already defined. Rename this member or use different parameter types
// Line: 12

interface I<T>
{
	T Prop { get; set; }
}

class C : I<int>
{
	int I<int>.Prop { get; set; }
	int I<int>.Prop { get; set; }
}
