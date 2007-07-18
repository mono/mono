// Compiler options: -langversion:linq

delegate void E ();

public class C
{
	public static void Main ()
	{
		E e = () => { };
		e = (E)null;
		e = default (E);
	}
}
