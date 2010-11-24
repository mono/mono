// Compiler options: -t:library

using System;

public class TestClass
{
	public class B : A<Nested>
	{
	}
	
	public abstract class A<T>
	{
		public static Comparison<A<T>> Compare;
	}
	
	public class Nested
	{
	}
}
