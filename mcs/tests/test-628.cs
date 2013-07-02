namespace N1
{	
	public enum A
	{
		A_1, A_2, A_3
	}

	public class B
	{
		const A b = A.A_1;
	}
}

enum A {
a, b
}

class X {
	const A a = A.a;
}

public enum EX :byte {
	a, b
}


public class X2 {

	public enum Blah { A }

}

public class Y2 : X2 {

	Blah x;
	
}

public class Y {

	const EX myconst = EX.a;

	public static void Main ()
	{
	}
}



