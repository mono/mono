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
                        // Do everything in Invariant
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                        DriverMain driver = new DriverMain (args);
                                if (!driver.Run ()) {
                                        Console.WriteLine ();
                                        Console.WriteLine ("***** FAILURE *****");
                                        return 1;
                                }
                        Console.WriteLine ("Operation completed successfully");
                        return 0;
                }

                private class DriverMain {

                        private ArrayList il_file_list;
                        private Report report;
                        private string output_file;
                        private Target target = Target.Exe;
                        private string target_string = "exe";
                        private bool quiet = false;
                        private bool show_tokens = false;
                        private bool show_method_def = false;
                        private bool show_method_ref = false;
                        private bool show_parser = false;
                        private bool scan_only = false;
			private bool debugging_info = false;
                        private CodeGen codegen;

                        public DriverMain (string[] args)
                        {
                                il_file_list = new ArrayList ();
                                ParseArgs (args);
                                report = new Report (quiet);
                        }

                        public bool Run ()
                        {
                                        if (il_file_list.Count == 0)
                                                Usage ();
                                        if (output_file == null)
                                                output_file = CreateOutputFile ();
                                        codegen = new CodeGen (output_file, target == Target.Dll, true, debugging_info, report);
                                        foreach (string file_path in il_file_list)
                                                ProcessFile (file_path);
                                        if (scan_only)
                                                return true;

                                        if (report.ErrorCount > 0)
                                                return false;
                                        codegen.Write ();
                                return true;
                        }

                        private void ProcessFile (string file_path)
                        {
                                if (!File.Exists (file_path)) {
                                        Console.WriteLine ("File does not exist: {0}",
                                                file_path);
                                        Environment.Exit (2);
                                }
                                report.AssembleFile (file_path, null,
                                                target_string, output_file);
                                StreamReader reader = File.OpenText (file_path);
                                ILTokenizer scanner = new ILTokenizer (reader);

                                if (show_tokens)
                                        scanner.NewTokenEvent += new NewTokenEvent (ShowToken);
                                //if (show_method_def)
                                //        MethodTable.MethodDefinedEvent += new MethodDefinedEvent (ShowMethodDef);
                                //if (show_method_ref)
                                //       MethodTable.MethodReferencedEvent += new MethodReferencedEvent (ShowMethodRef);

                                if (scan_only) {
                                        ILToken tok;
                                        while ((tok = scanner.NextToken) != ILToken.EOF) {
                                                Console.WriteLine (tok);
                                        }
                                        return;
                                }

                                ILParser parser = new ILParser (codegen, scanner);
				codegen.BeginSourceFile (file_path);
                                try {
                                        if (show_parser)
                                                parser.yyparse (new ScannerAdapter (scanner),
                                                                new yydebug.yyDebugSimple ());
                                        else
                                                parser.yyparse (new ScannerAdapter (scanner),  null);
                                } catch (ILTokenizingException ilte) {
                                        report.Error (file_path + "(" + ilte.Location.line + ") : error : " +
                                                        "syntax error at token '" + ilte.Token + "'.");
                                } catch {
                                        Console.WriteLine ("Error at: " + scanner.Reader.Location);
                                        throw;
                                } finally {
					codegen.EndSourceFile ();
				}
                        }

                        public void ShowToken (object sender, NewTokenEventArgs args)
                        {
                                Console.WriteLine ("token: '{0}'", args.Token);
                        }
                        /*
                        public void ShowMethodDef (object sender, MethodDefinedEventArgs args)
                        {
                                Console.WriteLine ("***** Method defined *****");
                                Console.WriteLine ("-- signature:   {0}", args.Signature);
                                Console.WriteLine ("-- name:        {0}", args.Name);
                                Console.WriteLine ("-- return type: {0}", args.ReturnType);
                                Console.WriteLine ("-- is in table: {0}", args.IsInTable);
                                Console.WriteLine ("-- method atts: {0}", args.MethodAttributes);
                                Console.WriteLine ("-- impl atts:   {0}", args.ImplAttributes);
                                Console.WriteLine ("-- call conv:   {0}", args.CallConv);
                        }

                        public void ShowMethodRef (object sender, MethodReferencedEventArgs args)
                        {
                                Console.WriteLine ("***** Method referenced *****");
                                Console.WriteLine ("-- signature:   {0}", args.Signature);
                                Console.WriteLine ("-- name:        {0}", args.Name);
                                Console.WriteLine ("-- return type: {0}", args.ReturnType);
                                Console.WriteLine ("-- is in table: {0}", args.IsInTable);
                        }
                        */
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
                                        case "output":
                                                output_file = command_arg;
                                                break;
                                        case "exe":
                                                target = Target.Exe;
                                                target_string = "exe";
                                                break;
                                        case "dll":
                                                target = Target.Dll;
                                                target_string = "dll";
                                                break;
                                        case "quiet":
                                                quiet = true;
                                                break;
                                        case "debug":
                                        case "deb":
						if (str[0] != '-')
							break;
						debugging_info = true;
						break;
                                        // Stubs to stay commandline compatible with MS 
                                        case "listing":
                                        case "nologo":
                                        case "clock":
                                        case "error":
                                        case "subsystem":
                                        case "flags":
                                        case "alignment":
                                        case "base":
                                        case "key":
                                        case "resource":
                                                break;
                                        case "scan_only":
                                                scan_only = true;
                                                break;
                                        case "show_tokens":
                                                show_tokens = true;
                                                break;
                                        case "show_method_def":
                                                show_method_def = true;
                                                break;
                                        case "show_method_ref":
                                                show_method_ref = true;
                                                break;
                                        case "show_parser":
                                                show_parser = true;
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
                                        default:
                                                if (str [0] == '-')
                                                        break;
                                                il_file_list.Add (str);
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
                                        target_string);
                        }

                        private void Usage ()
                        {
                                Console.WriteLine ("Mono ILasm compiler\n" +
                                        "ilasm [options] source-files\n" +
                                        "   --about            About the Mono ILasm compiler\n" +
                                        "   --version          Print the version number of the Mono ILasm compiler\n" +
                                        "   /output:file_name  Specifies output file.\n" +
                                        "   /exe               Compile to executable.\n" +
                                        "   /dll               Compile to library.\n" +
                                        "   /debug             Include debug information.\n" +
                                        "Options can be of the form -option or /option\n");
                                Environment.Exit (1);
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

