// CS8075: An expression tree cannot contain a collection initializer with extension method
// Line: 12

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

class Program
{
	static void Main()
	{
		Expression<Func<Stack<int>>> e = () => new Stack<int> { 42 };		
	}
}

static class X
{
	public static void Add<T>(this Stack<T> s, T value) => s.Push (value);
}