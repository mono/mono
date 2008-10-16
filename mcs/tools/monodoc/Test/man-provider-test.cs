using System;
using System.IO;
using Monodoc;

class Test {
	public static void Main (string[] args)
	{
		foreach (string file in args) {
			using (FileStream f = new FileStream (file, FileMode.Open, FileAccess.Read))
				Console.WriteLine (ManHelpSource.GetTextFromStream (f));
		}
	}
}

