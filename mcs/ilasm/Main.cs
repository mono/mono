using System;
using System.IO;

using Mono.ILASM;


public class ILAsmTest {
	private ILAsmTest() {}


	public static void Main (string [] args) {
		StreamReader reader = File.OpenText("test.il");
		ILTokenizer scanner = new ILTokenizer (reader);

		bool testScanner = !true;

		if (testScanner) {
			ILToken tok;
			while ((tok = scanner.NextToken) != ILToken.EOF) {
				Console.WriteLine (tok);
			}
		} else {
			ILParser parser = new ILParser ();
			parser.yyparse (new ScannerAdapter (scanner), new yydebug.yyDebugSimple ());

			CodeGen cg = parser.CodeGen;
			int n = cg.ClassCount;
			cg.Emit ();
		}
	}
}
