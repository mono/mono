//
// driver.cs: Guides the compilation process through the different phases.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Reflection.Emit;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public class Jsc 
	{
		string filename;
		string assemblyName;
		ASTList program;
		SemanticAnaliser semAnalizer;
		CodeGenerator codeGen;

		public Jsc (string filename)
		{
			this.filename = filename;
			this.assemblyName = Path.GetFileNameWithoutExtension (filename);

			program = new ASTList ();
		}


		public void Run ()
		{
			this.GetAST (filename);
			// this.SemanticAnalysis ();
			this.GenerateCode ();

			this.codeGen.assemblyBuilder.Save (assemblyName + ".exe");
		}

			
		public void GenerateCode ()
		{
			this.codeGen = new CodeGenerator (assemblyName,
							  AssemblyBuilderAccess.RunAndSave);

			this.codeGen.EmitJScript0 (this.program);
			this.codeGen.EmitJScriptMain ();			
		}


		public void GetAST (string filename)
		{
			StreamReader reader = new StreamReader (filename);
			JScriptLexer lexer = new JScriptLexer (reader);
			JScriptParser parser = new JScriptParser (lexer);

			parser.program (program);
		}


		public static void Main (string [] args)
		{
			try {			
				Jsc compiler = new Jsc (args [0]);

				compiler.Run ();

			} catch (IndexOutOfRangeException) {
				Console.WriteLine ("Usage: [mono] mjs.exe filename.js");
			}
		}
	}
}
