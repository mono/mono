// CS0165: Use of unassigned local variable `trial'
// Line: 18

using System;

class Test
{
	public static void Main (string[] args)
	{
		bool trial;
		string action = "add_trial";

		switch (action) {
		case "add_trial":
			trial = true;
			goto case "add_to_project";
		case "add_to_project":
			Console.WriteLine (trial);
			break;
		case "test":
			break;
		}
	}
}