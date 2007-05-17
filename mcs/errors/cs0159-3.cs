// cs0159-3.cs: No such label `case null:' within the scope of the goto statement
// Line: 10

class y {
	static void Main ()
	{
		string x = null;

		switch (x){
			case "": goto case null;
		}
	}
}
		
