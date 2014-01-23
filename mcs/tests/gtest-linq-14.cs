using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public enum Style : ulong {
	Foo,
	Bar,
}

public class Person {
	public Style Style { get; set; }
}

public interface IObjectContainer {}

public static class Extensions {

	public static IMarker<T> Cast<T> (this IObjectContainer container)
	{
		return null;
	}

	public static IMarker<T> Where<T> (this IMarker<T> marker, Expression<Func<T, bool>> selector)
	{
		return null;
	}
}

public interface IMarker<T> : IEnumerable<T> {}

public class Program {

	public static void Main ()
	{
	}
	
	public static void Assert (Action a)
	{
	}

	public static void Test (IObjectContainer o, Style s)
	{
		Assert (delegate {
			var res = from Person p in o
			where 0 == s
			select p;
		});
	}
}
