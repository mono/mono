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

	public class Context
	{
		public Hashtable variables = new Hashtable ();
		public ILGenerator ig;
		public TypeBuilder type;

		public Context ()
		{}
	}
	

	public class Jsc 
	{
		AssemblyName assemblyName;
		AssemblyBuilder assemblyBuilder;
		ModuleBuilder moduleBuilder;
		MethodBuilder methodBuilder;
		
		Context context;
		string basename;
		
		Program mainProgram;
		string JSCRIPT_MODULE = "Jscript Module";

		public Jsc (string output)
		{
			basename = output;
			
			assemblyName = new AssemblyName ();

			assemblyName.Name = basename;

			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName,
											 AssemblyBuilderAccess.RunAndSave, ".");
			// FIXME: hard coded ".exe" extension
			moduleBuilder = assemblyBuilder.DefineDynamicModule (JSCRIPT_MODULE, basename + ".exe", false);
		
			context = new Context ();
		}


		public void  GetAST (string filename)
		{
			StreamReader reader = new StreamReader (filename);
			JScriptLexer lexer = new JScriptLexer (reader);
			JScriptParser parser = new JScriptParser (lexer);
			
			mainProgram = new Program ();
			parser.program (mainProgram);
		}


		private void EmitJScript0Type ()
		{
			context.type = moduleBuilder.DefineType ("JScript 0", TypeAttributes.Public | TypeAttributes.Class);
			context.type.SetParent (typeof (GlobalScope));
			context.type.SetCustomAttribute (new CustomAttributeBuilder
							 (typeof (CompilerGlobalScopeAttribute).GetConstructor (new Type [] {}), new object [] {}));
		}

		private void EmitJScript0Cons ()
		{
			ConstructorBuilder constructor;
			constructor = context.type.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard,
								      new Type [] {typeof (GlobalScope)});
			
			context.ig = constructor.GetILGenerator ();
			
			context.ig.Emit (OpCodes.Ldarg_0);
			context.ig.Emit (OpCodes.Ldarg_1);
			context.ig.Emit (OpCodes.Dup);
			context.ig.Emit (OpCodes.Ldfld,
					 typeof (ScriptObject).GetField ("engine"));
			context.ig.Emit (OpCodes.Call, 
					 typeof (GlobalScope).GetConstructor (new Type [] {typeof (GlobalScope), typeof (Microsoft.JScript.Vsa.VsaEngine)}));
			context.ig.Emit (OpCodes.Ret);				
		}


		private void EmitGlobalCode ()
		{
			methodBuilder = context.type.DefineMethod ("Global Code",
								   MethodAttributes.Public, 
								   typeof (object), 
								   null);
			
			context.ig = methodBuilder.GetILGenerator ();
			
			context.ig.Emit (OpCodes.Ldarg_0);
			
			context.ig.Emit (OpCodes.Ldfld,
					 typeof (ScriptObject).GetField ("engine"));
		
			context.ig.Emit (OpCodes.Ldarg_0);

			context.ig.Emit (OpCodes.Call, 
					 typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("PushScriptObject", new Type [] {typeof (Microsoft.JScript.ScriptObject)}));


			int size = mainProgram.SourceElements.Size;			
			
                        // Emit the statements from the program.
			for (int i = 0; i < size; i++) {				
				mainProgram.SourceElements.MoveNext ();
				((Statement) mainProgram.SourceElements.Current).Emit (context);
			}			

			context.ig.Emit (OpCodes.Ldsfld, 
					 typeof (Microsoft.JScript.Empty).GetField ("Value"));

			context.ig.Emit (OpCodes.Ldarg_0);

			context.ig.Emit (OpCodes.Ldfld, 
					 typeof (ScriptObject).GetField ("engine"));

			context.ig.Emit (OpCodes.Call,
					 typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("PopScriptObject"));

			context.ig.Emit (OpCodes.Pop);

			context.ig.Emit (OpCodes.Ret);		       
		}


		private void EmitJScript0 ()
		{
			EmitJScript0Type ();
			EmitJScript0Cons ();
			EmitGlobalCode ();

			Type t = context.type.CreateType ();
		}



		private void EmitJScriptMainType ()
		{
			context.type = moduleBuilder.DefineType ("JScript Main", TypeAttributes.Public);
			context.type.SetParent (typeof (System.Object));
		}

		private void EmitJScriptMainCons ()
		{
			// constructor for JScript Main
			ConstructorBuilder constructor;
			constructor = context.type.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard,
								      new Type [] {});
			context.ig = constructor.GetILGenerator ();
			context.ig.Emit (OpCodes.Ldarg_0);
			context.ig.Emit (OpCodes.Call, 
					 typeof (Object).GetConstructor (new Type [] {}));
			context.ig.Emit (OpCodes.Ret);
		}
			

		private void EmitJScriptMainFunction ()
		{
			// define Main for JScript Main 
			MethodBuilder methodBuilder;
			methodBuilder = context.type.DefineMethod ("Main",
							   MethodAttributes.Public | MethodAttributes.Static,
							   typeof (void),
							   new Type [] {typeof (String [])});
			
			methodBuilder.SetCustomAttribute (new CustomAttributeBuilder 
							  (typeof (STAThreadAttribute).GetConstructor (new Type [] {}),
							   new object [] {}));


			context.ig = methodBuilder.GetILGenerator ();

			// declare local vars
			context.ig.DeclareLocal (typeof (Microsoft.JScript.GlobalScope));

			context.ig.Emit (OpCodes.Ldc_I4_1);
			context.ig.Emit (OpCodes.Ldc_I4_1);
			context.ig.Emit (OpCodes.Newarr, typeof (string));
			context.ig.Emit (OpCodes.Dup);
			context.ig.Emit (OpCodes.Ldc_I4_0);
			context.ig.Emit (OpCodes.Ldstr,
					 "mscorlib, Version=1.0.3300.0, Culture=neutral, Pub" + "licKeyToken=b77a5c561934e089");
			context.ig.Emit (OpCodes.Stelem_Ref);
			context.ig.Emit (OpCodes.Call,
					 typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod  ("CreateEngineAndGetGlobalScope", new Type [] {typeof (bool), typeof (string [])}));
			
			context.ig.Emit (OpCodes.Stloc_0);
			context.ig.Emit (OpCodes.Ldloc_0);
			

			context.ig.Emit (OpCodes.Newobj,
					 assemblyBuilder.GetType ("JScript 0").GetConstructor (new Type [] {typeof (Microsoft.JScript.GlobalScope)})); 

			context.ig.Emit (OpCodes.Call, assemblyBuilder.GetType ("JScript 0").GetMethod ("Global Code", new Type [] {}));

			context.ig.Emit (OpCodes.Pop);
			context.ig.Emit (OpCodes.Ret);

			assemblyBuilder.SetEntryPoint (methodBuilder);

		}			

		private void EmitJScriptMain ()
		{			
			EmitJScriptMainType ();
			EmitJScriptMainCons ();
			EmitJScriptMainFunction ();

			Type t2 = context.type.CreateType ();
		}


		public void Emit (string outputFile)
		{
			EmitJScript0 ();
			EmitJScriptMain ();						
			assemblyBuilder.Save (outputFile);			
		}

		public static void Main (string [] args)
		{
			try {			
				string basename = Path.GetFileNameWithoutExtension (args [0]);
				Jsc compiler = new Jsc (basename);
				compiler.GetAST (args [0]);				
				// compiler.Emit (compiler.basename + ".exe");
			
			} catch (IndexOutOfRangeException) {
				Console.WriteLine ("Usage: [mono] mjsc.exe filename.js");
			}
		}
	}
}
