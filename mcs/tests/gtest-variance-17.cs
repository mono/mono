using System;

interface InterfaceContravariat<in T>
{
	T Prop { set; }
	T this[int arg] { set; }
}

interface InterfaceCovariant<out T>
{
	T Prop { get; }
	T this[int arg] { get; }
}

class A : InterfaceContravariat<int>, InterfaceCovariant<long>
{
	public static int Main ()
	{
		return 0;
	}

	int InterfaceContravariat<int>.Prop
	{
		set { throw new NotImplementedException (); }
	}

	int InterfaceContravariat<int>.this[int arg]
	{
		set { throw new NotImplementedException (); }
	}

	long InterfaceCovariant<long>.Prop
	{
		get { throw new NotImplementedException (); }
	}

	long InterfaceCovariant<long>.this[int arg]
	{
		get { throw new NotImplementedException (); }
	}
}
