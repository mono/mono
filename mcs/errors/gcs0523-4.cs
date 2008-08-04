// CS0523: Struct member `S<T>.s' of type `S<int>' causes a cycle in the struct layout
// Line: 6

struct S<T>
{
	S<int> s;
}
