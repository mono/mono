// CS0165: Use of unassigned local variable `retval'
// Line: 9

class Test
{
	static string DoStuff (string msg)
	{
		string retval;

		switch (msg) {
		case "hello":
			retval = "goodbye";
			return retval;
		case "goodbye":
			return retval;
		case "other":
			retval = "other";
		case "":
			return msg;
		}
		return "";
	}
}