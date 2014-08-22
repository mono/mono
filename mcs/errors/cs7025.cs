// CS7025: Inconsistent accessibility: event type `System.Action<I>' is less accessible than event `C.E'
// Line: 8

using System;

public class C
{
	public event Action<I> E;
}

interface I
{
}