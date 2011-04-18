// CS1624: The body of `X.this[int].set' cannot be an iterator block because `void' is not an iterator interface type
// Line: 15
using System;
using System.Collections;

class X
{
	IEnumerator this [int u]
	{
	    get {
		yield return 1;
		yield return 2;
		yield return 3;
	    }
	    set
	    {
		yield return 3;		
	    }	    
	}
}
