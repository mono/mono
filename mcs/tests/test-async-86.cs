using System;
using System.Threading.Tasks;

public class Program
{
	string Data { 
		get {
			return data;
		}
		set {
			++setter_called;
			data = value;
		}
	}

	int setter_called;
	string data = "init-";

	string this [string arg] {
		get {
			return i_data;
		}
		set {
			++i_setter_called;
			i_data = value;
		}
	}

	int i_setter_called;
	string i_data = "init2-";

	public static int Main()
	{
		var p = new Program ();
		p.TestProperty ().Wait ();
		if (p.data != "init-nxa123z") {
			return 1;
		}

		if (p.setter_called != 1)
			return 2;

		p.TestIndexer ().Wait ();

		if (p.i_data != "init2-nxa123z") {
			return 3;
		}

		if (p.i_setter_called != 1)
			return 4;

		return 0;
	}

	async Task TestProperty ()
	{
		Data += "n" + await StringValue () + "a" + 123.ToString () + "z";
	}

	async Task TestIndexer ()
	{
		string arg = "foo";
		this[arg] += "n" + await StringValue () + "a" + 123.ToString () + "z";
	}

	async Task<string> StringValue ()
	{
		await Task.Yield ();
		return "x";
	}
}