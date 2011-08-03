// Compiler options: -doc:xml-062.xml /warnaserror /warn:4

using System;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Maybe<T>
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparamref name="T"></typeparamref>
	public void Method ()
	{
	}
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public struct Tuple<T1, T2>
{
	/// <summary>
	///  Test
	///  <typeparamref name="TResult" />.
	///  <typeparamref name="T2" />.
	/// </summary>
	public TResult Match<TResult> (params Func<T1, T2, Maybe<TResult>>[] ms)
	{
		throw new InvalidOperationException ();
	}
}

class C
{
	static void Main ()
	{
	}
}
