//
// driver.cs: Walks the AST from a JScript program, and generates CIL opcodes.
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Reflection.Emit;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public class Jsc 
	{
		AssemblyName assemblyName;
		AssemblyBuilder assemblyBuilder;
		ModuleBuilder moduleBuilder;
		MethodBuilder methodBuilder;
		
		string basename;
		
		string JSCRIPT_MODULE = "Jscript Module";

		public Jsc (string output)
		{
			basename = output;
			
			assemblyName = new AssemblyName ();

			assemblyName.Name = basename;

			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName,
											 AssemblyBuilderAccess.RunAndSave,
											 ".");
			// FIXME: hard coded ".exe" extension
			moduleBuilder = assemblyBuilder.DefineDynamicModule (JSCRIPT_MODULE, basename + ".exe", false);
		}

		public ASTList GetAST (string filename)
		{
			StreamReader reader = new StreamReader (filename);
			JScriptLexer lexer = new JScriptLexer (reader);
			JScriptParser parser = new JScriptParser (lexer);

			ASTList astList = new ASTList ();			
			parser.program (astList);

			return astList;
		}


		public static void Main (string [] args)
		{
			try {			
				string basename = Path.GetFileNameWithoutExtension (args [0]);
				Jsc compiler = new Jsc (basename);

				ASTList astList = compiler.GetAST (args [0]);

				Console.WriteLine (astList.ToString ());

			} catch (IndexOutOfRangeException) {
				Console.WriteLine ("Usage: [mono] mjs.exe filename.js");
			}
		}
	}
}
