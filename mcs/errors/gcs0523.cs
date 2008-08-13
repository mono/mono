// CS0523: Struct member `A<T>.a' of type `A<T>' causes a cycle in the struct layout
// Line: 6

struct A<T>
{
	A<T> a;
}
