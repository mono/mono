public abstract class A
{
	public abstract void Bind (string [] args);
}

public class B : A
{
	public override void Bind (params string [] args)
	{
	}

	public static int Main ()
	{
		var m = typeof (B).GetMethod ("Bind");
		var p = m.GetParameters ();
		var ca = p[0].GetCustomAttributes (false);
		if (ca.Length != 0)
			return 1;
		
		return 0;
	}
}
