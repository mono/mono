// CS0307: The property `B.Get' cannot be used with type arguments
// Line: 14
public class B
{
	public virtual int Get {
		get { return 3; }
	}
}

public class A : B
{
	public override int Get {
		get {
			return base.Get<int>;
		}
	}

	public static void Main ()
	{
	}
}
