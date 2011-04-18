using System;

delegate void D<in T> ();

interface I<out T>
{
	event D<T> field;
}

class D : I<string>
{
	public event D<string> field;
	
	public static int Main ()
	{
		D<object> dd = () => {};
		
		D d = new D ();
		d.field += dd;
		d.field ();
		
		return 0;
	}
}
