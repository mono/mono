//
// A compilation test: accessing an event from a nested class.
// Bug 48710
//
using System;

public delegate void OnWhateverDelegate( string s );

class cls
{
	public event OnWhateverDelegate OnWhatever;

	class nestedcls
	{
		internal void CallParentDel( cls c, string s )
		{
			c.OnWhatever( s );
		}
	}
	internal void CallMyDel( string s)
	{
		(new nestedcls()).CallParentDel( this, s );
	}
}

class MonoEmbed 
{
	public static void Main() 
	{
		cls c = new cls();
		c.OnWhatever += new OnWhateverDelegate( Whatever );
		c.CallMyDel( "test" );
	}
	static void Whatever( string s )
	{
		Console.WriteLine( s );
	}
}



