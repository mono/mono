// Compiler options: -t:library

using System.Collections.Generic;

public interface I
{
	void Foo<T> (List<T> arg) where T : A;
}

public class A : AB, IB
{
}

public abstract class AB : IC
{
}

public interface IB
{
}

public interface IC
{
}