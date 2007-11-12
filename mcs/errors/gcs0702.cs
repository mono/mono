// CS0702: A constraint cannot be special class `System.Array'
// Line: 8

using System;

class Foo<T>
	where T : Array
{
}
