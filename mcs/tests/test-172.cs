//
// This test excercises a few new optimizations that were entered in the context
// of Binary operators and EmitBranchable.
//
// There are a number of cases:
//
//   EmitBranchable can be called with an `onTrue' (false for if, true for loops).
//
//   The == and != operators handle the Null compares specially
using System;

class X {
	static int one = 1;
	static int two = 2;
	static int three = 3;
	static int four = 4;
	static bool t = true;
	static bool f = false;

	static int fcalls = 0;
	static int tcalls = 0;
	
	static bool ff ()
	{
		fcalls++;
		return false;
	}

	static bool tt ()
	{
		tcalls++;
		return true;
	}
	
	static int test_if ()
	{
		//
		// Ands in the if context
		//
		if (f && f)
			return 1;
		if (t && f)
			return 2;
		if (f && t)
			return 3;
		
		if (one < two && f)
			return 4;
		if (one > two && t)
			return 5;
		if (one < two && t)
			Console.WriteLine ("");

		if (ff () && ff ())
			return 6;
		if (fcalls != 1)
			return 10;
		
		if (tt () && tt ()){
			if (tcalls != 2)
				return 11;
			
			if (tt () && ff ())
				return 8;
			if (tcalls != 3)
				return 12;
			if (fcalls != 2)
				return 13;
			if (ff () && tt ())
				return 9;
			if (fcalls != 3)
				return 14;
			if (tcalls != 3)
				return 15;
		} else
			return 7;

		if (one < two && four > three){
			if (one == one && two != three){
				
			} else 
				return 20;
		} else
			return 21;

		if (one == two || two != two)
			return 22;

		object o = null;

		if (o == null || false){
			o = 1;

			if (o != null || false)
				o = null;
			else
				return 23;

			if (true || o == null){
				if (o != null || o == null){
					if (o == null && o != null)
						return 25;
					if (o == null && one == two)
						return 26;
					if (one == one && o != null)
						return 27;
					o = 1;
					if (two == two && o == null)
						return 28;
					return 0;
				}
				return 25;
			}
			return 26;
		}
		return 27;
	}

	//
	// This tests emitbranchable with an `onTrue' set to tru
	//
	static int test_while ()
	{
		int count = 0;
		
		while (t && t){
			count++;
			break;
		}

		if (count != 1)
			return 1;

		while (f || t){
			count++; break;
		}
		if (count != 2)
			return 2;
		while (f || f){
			count++; break;
		}
		if (count != 2)
			return 3;

		while (one < two && two > one){
			count++;
			break;
		}
		if (count != 3)
			return 4;

		while (one < one || two > one){
			count++;
			break;
		}
		if (count != 4)
			return 5;

		while (one < one || two > two){
			count++;
			break;
		}
		if (count != 4)
			return 6;
		
		while (one < two && t){
			count++;
			break;
		}
		if (count != 5)
			return 7;

		while (one < one || t){
			count++;
			break;
		}
		if (count != 6)
			return 8;

		while (one < one || f){
			count++;
			break;
		}

		if (count != 6)
			return 9;
		
		return 0;
	}
	
	static int test_inline ()
	{
		bool lt = t || f;

		if (!lt)
			return 1;

		return 0;
	}
	
	public static int Main ()
	{
		int v;
		object o = null;

		if (o == null || false)
			o = 1;
		else
			o = 2;

		Console.WriteLine ("V: "+ o);
		
		v = test_if ();
		if (v != 0)
			return v;
		v = test_inline ();
		if (v != 0)
			return 30 + v;
		
		v = test_while ();
		if (v != 0)
			return 90 + v;
		
		Console.WriteLine ("test ok");
		return 0;
	}
}
	
