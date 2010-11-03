using System;

public static class Assert
{
	public static int Errors {
		get { return errors; }
	}

	static int errors = 0;

	static void Error (string method, string text)
	{
		Console.WriteLine ("Assert failed: {0} ({1})", method, text);
		errors++;
	}

	public static void IsTrue (string text, bool b)
	{
		if (!b)
			Error ("IsTrue", text);
	}

	public static void IsFalse (string text, bool b)
	{
		if (b)
			Error ("IsFalse", text);
	}

	public static void IsNull<T> (string text, Nullable<T> nullable)
		where T : struct
	{
		if (nullable.HasValue)
			Error ("IsNull", text);
	}

	public static void IsNotNull<T> (string text, Nullable<T> nullable)
		where T : struct
	{
		if (!nullable.HasValue)
			Error ("IsNotNull", text);
	}

	public static void IsTrue (string text, Nullable<bool> b)
	{
		if (!b.HasValue || !b.Value)
			Error ("IsTrue", text);
	}

	public static void IsFalse (string text, Nullable<bool> b)
	{
		if (!b.HasValue || b.Value)
			Error ("IsFalse", text);
	}
}

class X
{
	static int Main ()
	{
		bool? a = null, b = false, c = true;
		bool? d = null, e = false, f = true;

		Assert.IsNull ("a", a);
		Assert.IsFalse ("b", b);
		Assert.IsTrue ("c", c);
		Assert.IsTrue ("a == d", a == d);
		Assert.IsTrue ("b == e", b == e);
		Assert.IsTrue ("c == f", c == f);

		Assert.IsFalse ("a != d", a != d);
		Assert.IsFalse ("a == b", a == b);
		Assert.IsTrue ("a != b", a != b);

		Assert.IsNull ("d & a", d & a);
		Assert.IsFalse ("d & b", d & b);
		Assert.IsNull ("d & c", d & c);
		Assert.IsFalse ("e & a", e & a);
		Assert.IsFalse ("e & b", e & b);
		Assert.IsFalse ("e & c", e & c);
		Assert.IsNull ("f & a", f & a);
		Assert.IsFalse ("f & b", f & b);
		Assert.IsTrue ("f & c", f & c);

		Assert.IsNull ("d | a", d | a);
		Assert.IsNull ("d | b", d | b);
		Assert.IsTrue ("d | c", d | c);
		Assert.IsNull ("e | a", e | a);
		Assert.IsFalse ("e | b", e | b);
		Assert.IsTrue ("e | c", e | c);
		Assert.IsTrue ("f | a", f | a);
		Assert.IsTrue ("f | b", f | b);
		Assert.IsTrue ("f | c", f | c);

		Assert.IsNull ("d ^ a", d ^ a);
		Assert.IsNull ("d ^ b", d ^ b);
		Assert.IsNull ("d ^ c", d ^ c);
		Assert.IsNull ("e ^ a", e ^ a);
		Assert.IsFalse ("e ^ b", e ^ b);
		Assert.IsTrue ("e ^ c", e ^ c);
		Assert.IsNull ("f ^ a", f ^ a);
		Assert.IsTrue ("f ^ b", f ^ b);
		Assert.IsFalse ("f ^ c", f ^ c);
		
		int? g = 3, h = null, i = 3, j = null;

		Assert.IsFalse ("g == null", g == null);
		Assert.IsTrue ("g != null", g != null);
		Assert.IsTrue ("h == null", h == null);
		Assert.IsFalse ("h != null", h != null);

		Assert.IsTrue ("g == i", g == i);
		Assert.IsFalse ("g != i", g != i);
		Assert.IsFalse ("g == j", g == j);
		Assert.IsTrue ("g != j", g != j);
		Assert.IsFalse ("h == i", h == i);
		Assert.IsTrue ("h != i", h != i);
		Assert.IsTrue ("h == j", h == j);
		Assert.IsFalse ("h != j", h != j);

		Console.WriteLine ("{0} errors.", Assert.Errors);
		return Assert.Errors;
	}
}
