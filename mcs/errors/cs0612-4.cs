// CS0612: `O1' is obsolete
// Line: 23
// Compiler options: -warnaserror

using System;

[Obsolete]
class O1
{
}

#pragma warning disable 612
class A
{
	public virtual void Foo<T> () where T : O1
	{
	}
}
#pragma warning restore 612

class B : A
{
	public override void Foo<U> ()
	{
	}
}
