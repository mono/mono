using System;

class Stress {
	
	static string [] types = {
		"int",   "uint",
		"short", "ushort",
		"long",  "ulong",
		"sbyte", "byte", "char"
		};
	

	static void w (string s)
	{
		Console.Write (s);
	}

	static void wl (string s)
	{
		Console.WriteLine (s);
	}
	
	static void generate_receptors ()
	{
		foreach (string t in types){
			w ("\tstatic void receive_" + t + " (" + t + " a)\n\t{\n");
			w ("\t\tConsole.Write (\"        \");\n");
			w ("\t\tConsole.WriteLine (a);\n");
			w ("\t}\n\n");
		}
		
	}

	static void call (string type, string name)
	{
		w ("\t\treceive_" + type + " (checked ((" + type + ") var ));\n");
	}

	static void generate_emision ()
	{
		foreach (string type in types){
			w ("\tstatic void probe_" + type + "()\n\t{\n");
			if (type == "char")
				w ("\t\t" + type + " var = (char) 0;");
			else
				w ("\t\t" + type + " var = 0;");					
				
			wl ("");

			foreach (string t in types)
				call (t, "var");
			
			w ("\t}\n\n");
		}
	}
	
	static void generate_main ()
	{
		wl ("\tpublic static void Main ()\n\t{");

		foreach (string t in types){
			w ("\t\tprobe_" + t + " ();\n");
		}
		wl ("\t}");
	}

	public static void Main (string [] args)
	{
		wl ("using System;\nclass Test {\n");

		generate_receptors ();
		generate_emision ();

		generate_main ();
			       
		wl ("}\n");
	}
}
