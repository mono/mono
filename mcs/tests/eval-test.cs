using System;
using Mono.CSharp;

public class MyTest {
	static void Run (string id, string stmt)
	{
		if (!Evaluator.Run (stmt))
			Console.WriteLine ("Failed on test {0}", id);
	}

	static void Evaluate (string id, string expr, object expected)
	{
		try {
			object res = Evaluator.Evaluate (expr);
			if (res == null && expected == null)
				return;

			if (!expected.Equals (res)){
				Console.WriteLine ("Failed on test {2} Expecting {0}, got {1}", expected, res, id);
				throw new Exception ();
			}
		} catch {
			Console.WriteLine ("Failed on test {0}", id);
			throw;
		}
	}
	
	static void Main ()
	{
		Evaluator.Init (new string [0]); //new string [] { "-v", "-v" });
		//
		// This fails because of the grammar issue with the pointer type
		// Evaluate ("multiply", "1*2;", 2);
		//
		Run ("1",      "System.Console.WriteLine (100);");
		Run ("Length", "var a = new int [] {1,2,3}; var b = a.Length;");
		
		Evaluate ("CompareString", "\"foo\" == \"bar\";", false);
		Evaluate ("CompareInt", "var a = 1; a+2;", 3);

		Evaluator.Run ("using System; using System.Linq;");
		Run ("LINQ-1", "var a = new int[]{1,2,3};\nfrom x in a select x;");
		Run ("LINQ-2", "var a = from f in System.IO.Directory.GetFiles (\"/tmp\") where f == \"passwd\" select f;");

		Evaluator.ReferenceAssembly (typeof (MyTest).Assembly);
		Evaluate ("assembly reference test", "typeof (MyTest) != null;", true);

		Run ("LINQ-3", "var first_scope = new int [] {1,2,3};");
		Run ("LINQ-4", "var second_scope = from x in first_scope select x;");

		string prefix = "";
		string [] res = Evaluator.GetCompletions ("ConsoleK", out prefix);
		if (res [0] != "ey" || res [1] != "eyInfo"){
			Console.WriteLine (res [0]);
			Console.WriteLine (res [1]);
			throw new Exception ("Expected two completions ConsoleKey and ConsoleKeyInfo");
		}

		res = Evaluator.GetCompletions ("Converte", out prefix);
		if (res [0] != "r"){
			throw new Exception ("Expected one completion for Converter");
		}

		res = Evaluator.GetCompletions ("Sys", out prefix);
		if (res [0] != "tem"){
			throw new Exception ("Expected at least a conversion for System");
		}

		res = Evaluator.GetCompletions ("System.Int3", out prefix);
		if (res [0] != "2"){
			throw new Exception ("Expected completion to System.Int32");
		}

		res = Evaluator.GetCompletions ("new System.Text.StringBuilder () { Ca", out prefix);
		if (res [0] != "pacity"){
			throw new Exception ("Expected completion to Capacity");
		}

		res = Evaluator.GetCompletions ("new System.Text.StringBuilder () { ", out prefix);
		if (res.Length != 3){
			throw new Exception ("Epxected 4 completions (Capacity Chars Length MaxCapacity)");
		}

		// These should return "partial"
		object eval_result;
		bool result_set;
		string sres  = Evaluator.Evaluate ("1+", out eval_result, out result_set);
		if (result_set)
			throw new Exception ("No result should have been set");
		if (sres != "1+")
			throw new Exception ("The result should have been the input string, since we have a partial input");
		
	}
	
}
