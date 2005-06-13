// gcs0702.cs: Bound cannot be special class `System.Array'
// Line: 8

using System;

class Foo<T>
	where T : Array
{
}
