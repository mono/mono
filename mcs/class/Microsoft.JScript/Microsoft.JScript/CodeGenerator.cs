//
// CodeGenerator.cs: The actual IL code generator for JScript .Net programs.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.CompilerServices;
	using Microsoft.JScript.Vsa;

	internal sealed class CodeGenerator : Visitor
	{
		internal AssemblyName assemblyName;
		internal AssemblyBuilder assemblyBuilder;
		internal ModuleBuilder moduleBuilder;
		internal TypeBuilder typeBuilder;
		internal ILGenerator ilGen;

		internal string MODULE_NAME = "JScript Module";
		internal string GLOBAL_SCOPE = "Microsoft.JScript.GlobalScope";
	
		internal CodeGenerator (string assemName, AssemblyBuilderAccess access)
		{
			assemblyName = new AssemblyName ();
			assemblyName.Name = assemName;

			AppDomain appDomain = AppDomain.CurrentDomain;
			
			this.assemblyBuilder = appDomain.DefineDynamicAssembly (assemblyName, 
										access);

			this.moduleBuilder = assemblyBuilder.DefineDynamicModule (MODULE_NAME,
										  assemblyName.Name + ".exe", false);
		}


		internal void EmitJScript0Type ()
		{
			this.typeBuilder = moduleBuilder.DefineType ("JScript 0",
								     TypeAttributes.Class |
								     TypeAttributes.Public);

			this.typeBuilder.SetParent (typeof (Microsoft.JScript.GlobalScope));

			CustomAttributeBuilder attr;
			Type t = typeof (CompilerGlobalScopeAttribute);
			attr = new CustomAttributeBuilder (t.GetConstructor (new Type [] {}), 
									     new object [] {});
			this.typeBuilder.SetCustomAttribute (attr);
		}


		internal void EmitJScript0Cons ()
		{
			ConstructorBuilder consBuilder;

			consBuilder = typeBuilder.DefineConstructor (MethodAttributes.Public,
							             CallingConventions.Standard,
								     new Type [] {typeof (Microsoft.JScript.GlobalScope)});

			this.ilGen = consBuilder.GetILGenerator ();

			this.ilGen.Emit (OpCodes.Ldarg_0);
			this.ilGen.Emit (OpCodes.Ldarg_1);
			this.ilGen.Emit (OpCodes.Dup);
			this.ilGen.Emit (OpCodes.Ldfld, 
					 typeof (Microsoft.JScript.ScriptObject).GetField ("engine"));
			this.ilGen.Emit (OpCodes.Call,
					 typeof (Microsoft.JScript.GlobalScope).GetConstructor (new Type [] {typeof (Microsoft.JScript.GlobalScope), typeof (Microsoft.JScript.Vsa.VsaEngine)}));
			this.ilGen.Emit (OpCodes.Ret);
		}


		private void EmitJScript0GlobalCode (ASTList program)
		{
			MethodBuilder methodBuilder;
			methodBuilder = this.typeBuilder.DefineMethod ("Global Code",
									MethodAttributes.Public, 
									typeof (object), 
									null);
                        
			this.ilGen = methodBuilder.GetILGenerator ();
                        
			this.ilGen.Emit (OpCodes.Ldarg_0);

			this.ilGen.Emit (OpCodes.Ldfld,
					 typeof (Microsoft.JScript.ScriptObject).GetField ("engine"));
                
			this.ilGen.Emit (OpCodes.Ldarg_0);

			this.ilGen.Emit (OpCodes.Call, 
					 typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("PushScriptObject", new Type [] {typeof (Microsoft.JScript.ScriptObject)}));


			program.Visit (this, null);
			
			
			this.ilGen.Emit (OpCodes.Ldsfld, 
					 typeof (Microsoft.JScript.Empty).GetField ("Value"));

			this.ilGen.Emit (OpCodes.Ldarg_0);

			this.ilGen.Emit (OpCodes.Ldfld, 
					 typeof (Microsoft.JScript.ScriptObject).GetField ("engine"));

			this.ilGen.Emit (OpCodes.Call,
					typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("PopScriptObject"));

			this.ilGen.Emit (OpCodes.Pop);
			this.ilGen.Emit (OpCodes.Ret);                       
                }



	
		public void EmitJScript0 (ASTList program)
		{
			this.EmitJScript0Type ();
			this.EmitJScript0Cons ();
			this.EmitJScript0GlobalCode (program);
			
			this.typeBuilder.CreateType ();
		}					
									    

		//
		// JScript Main class construction
		//

		private void EmitJScriptMainType ()
                {
			this.typeBuilder = moduleBuilder.DefineType ("JScript Main", 
								TypeAttributes.Public);

			this.typeBuilder.SetParent (typeof (System.Object));
                }


                private void EmitJScriptMainCons ()
                {
                        ConstructorBuilder consBuilder;
                        consBuilder = typeBuilder.DefineConstructor (MethodAttributes.Public, 
								      CallingConventions.Standard,
                                                                      new Type [] {});

			this.ilGen = consBuilder.GetILGenerator ();
			this.ilGen.Emit (OpCodes.Ldarg_0);
			this.ilGen.Emit (OpCodes.Call, 
                                         typeof (Object).GetConstructor (new Type [] {}));
			this.ilGen.Emit (OpCodes.Ret);
                }
                        

                private void EmitJScriptMainFunction ()
                {
			MethodBuilder methodBuilder;
			methodBuilder = typeBuilder.DefineMethod ("Main", 
								   MethodAttributes.Public | 
								   MethodAttributes.Static, 
								   typeof (void),
								   new Type [] {typeof (String [])});
                        
                        methodBuilder.SetCustomAttribute (new CustomAttributeBuilder 
                                                          (typeof (STAThreadAttribute).GetConstructor (new Type [] {}),
                                                           new object [] {}));


                        this.ilGen = methodBuilder.GetILGenerator ();

                        // declare local vars
                        this.ilGen.DeclareLocal (typeof (Microsoft.JScript.GlobalScope));

                        this.ilGen.Emit (OpCodes.Ldc_I4_1);
                        this.ilGen.Emit (OpCodes.Ldc_I4_1);
                        this.ilGen.Emit (OpCodes.Newarr, typeof (string));
                        this.ilGen.Emit (OpCodes.Dup);
                        this.ilGen.Emit (OpCodes.Ldc_I4_0);
                        this.ilGen.Emit (OpCodes.Ldstr,
                                         "mscorlib, Version=1.0.3300.0, Culture=neutral, Pub" + "licKeyToken=b77a5c561934e089");
                        this.ilGen.Emit (OpCodes.Stelem_Ref);
                        this.ilGen.Emit (OpCodes.Call,
                                         typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod  ("CreateEngineAndGetGlobalScope", new Type [] {typeof (bool), typeof (string [])}));
                        
                        this.ilGen.Emit (OpCodes.Stloc_0);
                        this.ilGen.Emit (OpCodes.Ldloc_0);
                        

                        this.ilGen.Emit (OpCodes.Newobj,
                                         assemblyBuilder.GetType ("JScript 0").GetConstructor (new Type [] {typeof (Microsoft.JScript.GlobalScope)})); 

                        this.ilGen.Emit (OpCodes.Call, assemblyBuilder.GetType ("JScript 0").GetMethod ("Global Code", new Type [] {}));

                        this.ilGen.Emit (OpCodes.Pop);
                        this.ilGen.Emit (OpCodes.Ret);

                        this.assemblyBuilder.SetEntryPoint (methodBuilder);

                }                        

                public void EmitJScriptMain ()
                {                        
                        this.EmitJScriptMainType ();
                        this.EmitJScriptMainCons ();
                        this.EmitJScriptMainFunction ();

                        Type t2 = this.typeBuilder.CreateType ();
                }




		// 
		// Visitor methods, that Emit IL OpCodes for each type of 
		// language constructs.
		//

		public object VisitASTList (ASTList prog, object obj)
		{
			int size = prog.elems.Count;

			for (int i = 0; i < size; i++)
				((AST) prog.elems [i]).Visit (this, obj);

			return null;
		}


		public object VisitVariableDeclaration (VariableDeclaration decl, 
							object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitFunctionDeclaration (FunctionDeclaration decl,
							object args)
		{
			throw new NotImplementedException ();
		}

		
		public object VisitBlock (Block b, object args)
		{
			throw new NotImplementedException ();
		}

		
		public object VisitEval (Eval e, object args)
		{	
			throw new NotImplementedException ();
		}

		
		public object VisitForIn (ForIn forIn, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitFunctionExpression (FunctionExpression fexp,	
						       object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitImport (Import imp, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitPackage (Package pkg, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitScriptBlock (ScriptBlock sblock, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitThrow (Throw t, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitTry (Try t, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitWith (With w, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitPrint (Print p, object args)
		{
			StringLiteral sl = p.Exp as StringLiteral;

			sl.Visit (this, args);

			this.ilGen.Emit (OpCodes.Call,
					 typeof (Microsoft.JScript.ScriptStream).GetMethod ("WriteLine", new Type [] {typeof (string)}));

			return null;
		}


		//
		// Literals
		//

		public object VisitArrayLiteral (ArrayLiteral al, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitStringLiteral (StringLiteral sl, object args)
		{
			this.ilGen.Emit (OpCodes.Ldstr, sl.Str);

			return null;
		}
	}
}
