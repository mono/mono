//
// This bug exposes a problem when calling a struct constructor that is
// initialized from an instance constructor
//
using System;
public interface Interface
{
	int X{ get; }
}

public struct Struct : Interface
{
	public Struct( int x ) { }
	public int X { get { return 0; } }
}

public class User
{
	public User( Interface iface ) { }
}
public class Test
{
	User t;
	Test() { t=new User (new Struct(5)); }
	User t2=new User(new Struct(251));

	static int Main ()
	{
		Test tt = new Test ();

		return 0;
	}
}



