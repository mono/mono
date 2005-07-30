// cs0136-6.cs: A local variable named `top' cannot be declared in this scope because it would give a different meaning to `top', which is already used in a `child' scope
// Line: 19

using System.Collections;

class Symbol
{
}

class X
{
		Symbol top;
	
		internal int Enter (Symbol key, object value)
		{
 			if (key != null) {
				top = key;					
			}
			Hashtable top = new Hashtable ();
			return top.Count;
		}
		
		public static void Main () {}
}
