using System;
using System.Collections.Generic;
using SCG = System.Collections.Generic;
using SCGL = System.Collections.Generic.List<string>;

class A<T>
{
	public class B
	{
		public int Foo;
	}
}

class X
{
	bool field;
	long Prop { get; set; }
	event Action ev;

	public static int Main ()
	{
		int res;
		var x = new X ();
		res = x.SimpleName (1);
		if (res != 0)
			return res;

		res = x.MemberAccess ();
		if (res != 0)
			return 20 + res;

		res = x.QualifiedName ();
		if (res != 0)
			return 40 + res;

		return 0;
	}

	static void GenMethod<T, U, V> ()
	{
	}

	int SimpleName<T> (T arg)
	{
		const object c = null;
		decimal d = 0;

		if (nameof (T) != "T")
			return 1;

		if (nameof (arg) != "arg")
			return 2;

		if (nameof (c) != "c")
			return 3;

		if (nameof (d) != "d")
			return 4;

		if (nameof (field) != "field")
			return 5;

		if (nameof (Prop) != "Prop")
			return 6;

		if (nameof (@Main) != "Main")
			return 7;

		if (nameof (ev) != "ev")
			return 8;

		if (nameof (Int32) != "Int32")
			return 9;

		if (nameof (Action) != "Action")
			return 10;

		if (nameof (List<bool>) != "List")
			return 11;

		if (nameof (GenMethod) != "GenMethod")
			return 12;

		return 0;
	}

	int MemberAccess ()
	{
		if (nameof (X.field) != "field")
			return 1;

		if (nameof (X.Prop) != "Prop")
			return 2;

		if (nameof (Console.WriteLine) != "WriteLine")
			return 3;

		if (nameof (System.Collections.Generic.List<long>) != "List")
			return 4;

		if (nameof (System.Collections) != "Collections")
			return 5;

		if (nameof (X.GenMethod) != "GenMethod")
			return 6;

		if (nameof (A<char>.B) != "B")
			return 7;

		if (nameof (A<ushort>.B.Foo) != "Foo")
			return 7;

		return 0;
	}

	int QualifiedName ()
	{
		if (nameof (global::System.Int32) != "Int32")
			return 1;

		if (nameof (SCG.List<short>) != "List")
			return 2;

		if (nameof (SCGL.Contains) != "Contains")
			return 3;

		return 0;
	}
}