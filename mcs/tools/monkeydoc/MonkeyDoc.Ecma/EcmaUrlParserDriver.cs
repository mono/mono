using System;
using System.IO;

namespace MonkeyDoc.Ecma
{
	public class EcmaUrlParserDriver
	{
		public static void Main (string[] args)
		{
			var input = new StringReader (args[0]);
			var lexer = new EcmaUrlTokenizer (input);
			var parser = new EcmaUrlParser ();

			Console.WriteLine (parser.yyparse (lexer));
		}
	}
}