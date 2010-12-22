// CS0612: `O2' is obsolete
// Line: 23
// Compiler options: -warnaserror

using System;

[Obsolete]
interface O2
{
}

#pragma warning disable 612
class A
{
	public virtual void Foo<T> () where T : O2
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
