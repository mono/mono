// Compiler options: -warnaserror -warn:4

using System;

public delegate void GenericEventHandler<U, V>(U u, V v);

public class GenericEventNotUsedTest<T>
{
	event GenericEventHandler<GenericEventNotUsedTest<T>, T> TestEvent;

	public void RaiseTestEvent(T t)
	{
		TestEvent(this, t);
	}
}

public interface IFoo {
		event EventHandler blah;
}

public static class TestEntry
{
	public static void Main()
	{
	}
}

