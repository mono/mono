// CS0159: The label `case null:' could not be found within the scope of the goto statement
// Line: 1

class y {
	static void Main ()
	{
		string x = null;

		switch (x){
			case "": goto case null;
		}
	}
}
		
