// CS0523: Struct member `S<T>.s' of type `S<T[]>' causes a cycle in the struct layout
// Line: 6

struct S<T>
{
	static S<T[]> s;
}
