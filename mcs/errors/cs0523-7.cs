// CS0523: Struct member `S.value' of type `G<string>' causes a cycle in the struct layout
// Line: 11

struct G<T>
{
	public static S s;
}

struct S
{
	private G<string> value;
}
