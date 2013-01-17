// CS0619: `IB' is obsolete: `hint'
// Line: 20

using System;

public interface IA
{
	#pragma warning disable 619
	void Foo<T> () where T : IB;
	#pragma warning restore 619
}

[Obsolete ("hint", true)]
public interface IB
{
}

public class C : IA
{
	void IA.Foo<X> ()
	{
	}
}
