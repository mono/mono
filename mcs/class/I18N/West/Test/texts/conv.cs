using System;
using System.IO;
using System.Text;

public class Test
{
	public static void Main (string [] args)
	{
		if (args.Length == 2) {
			string [] tmp = new string [4];
			tmp [0] = args [0] + "-utf8.txt";
			tmp [1] = "65001";
			tmp [2] = args [1];
			tmp [3] = args [0] + "-" + args [1] + ".txt";
			args = tmp;
		}
		if (args.Length < 4) {
			Console.WriteLine ("pass input-file input-encoding output-encoding output-file.");
			return;
		}
		Run (args);
	}

	static void Run (string [] args)
	{
		string s;
		using (StreamReader sr = new StreamReader (args [0],
			Encoding.GetEncoding (int.Parse (args [1])))) {
			s = sr.ReadToEnd ();
		}
		using (StreamWriter sw = new StreamWriter (args [3], false,
			Encoding.GetEncoding (int.Parse (args [2])))) {
			sw.Write (s);
		}
		string s2;
		using (StreamReader sr = new StreamReader (args [3],
			Encoding.GetEncoding (int.Parse (args [2])))) {
			s2 = sr.ReadToEnd ();
		}
		if (s != s2) {
			Console.WriteLine ("FAILURE");
			File.Delete (args [3]);
		}
	}
}

