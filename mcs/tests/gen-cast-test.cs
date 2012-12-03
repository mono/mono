using System;

class Stress {

	static string mode = "unchecked";
	
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

	static void var (string type, string name, string init)
	{
		w ("\t\t" + type + " " + name + " = (" + type + ") " + init + ";\n");
	}

	static void call (string type, string name)
	{
		w ("\t\treceive_" + type + " (" + mode + "((" + type + ") " + name + "));\n");
	}
	
	static void generate_emision ()
	{
		foreach (string type in types){
			w ("\tstatic void probe_" + type + "()\n\t{\n");
			var (type, "zero", "0");
			var (type, "min", type + ".MinValue");
			var (type, "max", type + ".MaxValue");
			wl ("");

			wl ("\t\tConsole.WriteLine (\"Testing: " + type + "\");\n");
			foreach (string t in types){
				wl ("\t\tConsole.WriteLine (\"   arg: " + t + " (" + type + ")\");\n");
				call (t, "zero");
				call (t, "min");
				call (t, "max");
			}
			
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
		foreach (string arg in args){
			if (arg == "-h" || arg == "--help"){
				Console.WriteLine ("-h, --help     Shows help");
				Console.WriteLine ("-c, --checked  Generate checked contexts");
				return;
			}
			if (arg == "--checked" || arg == "-c"){
				mode = "checked";
				continue;
			}
		}
		wl ("using System;\nclass Test {\n");

		generate_receptors ();
		generate_emision ();

		generate_main ();
			       
		wl ("}\n");
	}
}
