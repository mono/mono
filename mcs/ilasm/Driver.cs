//
// Mono.ILASM.Driver
//    Main Command line interface for Mono ILasm Compiler
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Mono.ILASM {

	public class Driver {

		enum Target {
			Dll, 
			Exe
		}

		public static int Main (string[] args)
		{
			DriverMain driver = new DriverMain (args);
			driver.Run ();
			return 0;
		}

		private class DriverMain {
			
			private ArrayList il_file_list;
			private string output_file;
			private Target target = Target.Exe;
			private bool show_tokens = false;
			private bool scan_only = false;
			private CodeGen codegen;

			public DriverMain (string[] args)
			{
				il_file_list = new ArrayList ();
				ParseArgs (args);
			}
			
			public void Run () 
			{
				if (il_file_list.Count == 0) {
					Usage ();
					return;
				}
				if (output_file == null)
					output_file = CreateOutputFile ();
				codegen = new CodeGen (output_file, target == Target.Dll, true);	
				foreach (string file_path in il_file_list)
					ProcessFile (file_path);
				if (scan_only)
					return;
				codegen.ClassTable.CheckForUndefined ();
				codegen.PEFile.WritePEFile ();
			}

			private void ProcessFile (string file_path)
			{
				if (!File.Exists (file_path)) {
					Console.WriteLine ("File does not exist: {0}",
						file_path);
					Environment.Exit (2);
				}
				StreamReader reader = File.OpenText (file_path);
				ILTokenizer scanner = new ILTokenizer (reader);

				if (show_tokens) {
					scanner.NewTokenEvent += new NewTokenEvent (ShowToken);
				}
   				
				if (scan_only) {
					ILToken tok;
					while ((tok = scanner.NextToken) != ILToken.EOF) {
						Console.WriteLine (tok);
					}
					return;
				}

				ILParser parser = new ILParser (codegen);
				parser.yyparse (new ScannerAdapter (scanner), null);
			}

			public void ShowToken (object sender, NewTokenEventArgs args)
			{
				Console.WriteLine ("token: '{0}'", args.Token);
			}

			private void ParseArgs (string[] args)
			{
				string command_arg;
				foreach (string str in args) {
					if ((str[0] != '-') && (str[0] != '/')) {
						il_file_list.Add (str);
						continue;
					} 
					switch (GetCommand (str, out command_arg)) {
						case "out":
							output_file = command_arg;
							break;
						case "exe":
							target = Target.Exe;
							break;
						case "dll":
							target = Target.Dll;
							break;
						case "scan_only":
							scan_only = true;
							break;
						case "show_tokens":
							show_tokens = true;
							break;
						case "-about":
							if (str[0] != '-')
								break;
							About ();
							break;
						case "-version":
							if (str[0] != '-')
								break;
							Version ();
							break;
					}
				}
			}
			
			private string GetCommand (string str, out string command_arg)
			{
				int end_index = str.IndexOfAny (new char[] {':', '='}, 1);
                            	string command = str.Substring (1, 
					end_index == -1 ? str.Length - 1 : end_index - 1);
				
				if (end_index != -1) {
					command_arg = str.Substring (end_index+1);
				} else {
					command_arg = null;
				}

				return command.ToLower ();
			}
			
			/// <summary>
			///   Get the first file name and makes it into an output file name
			/// </summary>
			private string CreateOutputFile () 
			{
				string file_name = (string)il_file_list[0];
				int ext_index = file_name.LastIndexOf ('.');			
	
				if (ext_index == -1)
					ext_index = file_name.Length;
				
				return String.Format ("{0}.{1}", file_name.Substring (0, ext_index), 
					target.ToString ().ToLower ());
			}

			private void Usage ()
			{
				Console.WriteLine ("Mono ILasm compiler\n" +
					"ilasm [options] source-files\n" +
					"   --about            About the Mono ILasm compiler\n" +
					"   --version          Print the version number of the Mono ILasm compiler\n" +
					"   /out:file_name     Specifies output file.\n" +
					"   /exe               Compile to executable.\n" +
					"   /dll               Compile to library.\n" +
					"Options can be of the form -option or /option\n");
			}

			private void About ()
			{	
				Console.WriteLine (
					"For more information on Mono, visit the project Web site\n" +
					"   http://www.go-mono.com\n\n");
				Environment.Exit (0);
			}
			
			private void Version ()
			{
				string version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
				Console.WriteLine ("Mono ILasm compiler version {0}", version);
				Environment.Exit (0);
			}

		}
	}
}

