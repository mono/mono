
[AttrB]
public class M {
	public static void Main()
	{
	}
}

[AttrB]
public class AttrA : System.Attribute {}

public class AttrB : AttrA {}
