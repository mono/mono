public class X<T> {
	public static int i {
		get { return 0; }
		private set { }
	}
	public static int myMain ()
	{
		return i++;
	}
}
public class Y {
	public static int Main ()
	{
		return X<Y>.myMain ();
	}
}
