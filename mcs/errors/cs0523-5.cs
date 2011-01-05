// CS0523: Struct member `Foo.Handle' of type `Handle<Foo>' causes a cycle in the struct layout
// Line: 13
// NOTE: Not detected by csc only by runtime loader

using System;

struct Handle<T>
{
	public IntPtr Value;
}

struct Foo
{
	public readonly Handle<Foo> Handle;
}
