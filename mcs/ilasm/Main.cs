using System;
using System.IO;

using Mono.ILASM;


public class ILAsmTest {
	private ILAsmTest() {}


	public static int Main (string [] args) {

		if (args.Length != 1) {
			Console.WriteLine ("Usage : ilasm [filename]");
			return 1;
		}
		
		StreamReader reader = File.OpenText (args [0]);
		ILTokenizer scanner = new ILTokenizer (reader);

		bool testScanner = true;

		if (testScanner) {
			ILToken tok;
			while ((tok = scanner.NextToken) != ILToken.EOF) {
				Console.WriteLine (tok);
			}
		} else {
			ILParser parser = new ILParser (new CodeGen ());
			parser.yyparse (new ScannerAdapter (scanner), new yydebug.yyDebugSimple ());

			CodeGen cg = parser.CodeGen;
			int n = cg.ClassCount;
			cg.Emit ();
		}

		return 0;
	}
}
