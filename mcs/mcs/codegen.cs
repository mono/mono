//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {
	
	public class CodeGen {
		AppDomain current_domain;
		AssemblyBuilder assembly_builder;
		ModuleBuilder   module_builder;

		string Basename (string name)
		{
			int pos = name.LastIndexOf ("/");

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ("\\");
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		string TrimExt (string name)
		{
			int pos = name.LastIndexOf (".");

			return name.Substring (0, pos);
		}
		
		public CodeGen (string name, string output)
		{
			AssemblyName an;
			
			an = new AssemblyName ();
			an.Name = TrimExt (name);
			current_domain = AppDomain.CurrentDomain;
			assembly_builder = current_domain.DefineDynamicAssembly (
				an, AssemblyBuilderAccess.RunAndSave);

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			module_builder = assembly_builder.DefineDynamicModule (
				Basename (name), Basename (output));

		}
		
		public AssemblyBuilder AssemblyBuilder {
			get {
				return assembly_builder;
			}
		}
		
		public ModuleBuilder ModuleBuilder {
			get {
				return module_builder;
			}
		}
		
		public void Save (string name)
		{
			try {
				assembly_builder.Save (Basename (name));
			} catch (System.IO.IOException io){
				Report.Error (16, "Coult not write to file `"+name+"', cause: " + io.Message);
			}
		}
	}

	// <summary>
	//   An Emit Context is created for each body of code (from methods,
	//   properties bodies, indexer bodies or constructor bodies)
	// </summary>
	public class EmitContext {
		public TypeContainer TypeContainer;
		public ILGenerator   ig;
		public bool CheckState;

		// <summary>
		//   Whether we are emitting code inside a static or instance method
		// </summary>
		public bool IsStatic;

		// <summary>
		//   The value that is allowed to be returned or NULL if there is no
		//   return type.
		// </summary>
		public Type ReturnType;

		// <summary>
		//   Whether this is generating code for a constructor
		// </summary>
		public bool IsConstructor;
		
		// <summary>
		//   Keeps track of the Type to LocalBuilder temporary storage created
		//   to store structures (used to compute the address of the structure
		//   value on structure method invocations)
		// </summary>
		public Hashtable temporary_storage;

		public Block CurrentBlock;
		
		public EmitContext (TypeContainer parent, ILGenerator ig, Type return_type,
				    int code_flags, bool is_constructor)
		{
			this.ig = ig;

			TypeContainer = parent;
			CheckState = parent.RootContext.Checked;
			IsStatic = (code_flags & Modifiers.STATIC) != 0;
			ReturnType = return_type;
			IsConstructor = is_constructor;
			CurrentBlock = null;
			
			if (ReturnType == TypeManager.void_type)
				ReturnType = null;
		}

		public EmitContext (TypeContainer parent, ILGenerator ig, Type return_type, int code_flags)
			: this (parent, ig, return_type, code_flags, false)
		{
		}

		public void EmitTopBlock (Block block)
		{
			bool has_ret = false;
			
			if (block != null){
				int errors = Report.Errors;
				
				block.EmitMeta (TypeContainer, ig, block, 0);
				
				if (Report.Errors == errors){
					has_ret = block.Emit (this);
					
					if (Report.Errors == errors)
						block.UsageWarning ();
				}
			}

			if (!has_ret)
				ig.Emit (OpCodes.Ret);
		}

		// <summary>
		//   Returns a temporary storage for a variable of type t as 
		//   a local variable in the current body.
		// </summary>
		public LocalBuilder GetTemporaryStorage (Type t)
		{
			LocalBuilder location;
			
			if (temporary_storage == null)
				temporary_storage = new Hashtable ();

			location = (LocalBuilder) temporary_storage [t];
			if (location != null)
				return location;

			location = ig.DeclareLocal (t);
			temporary_storage.Add (t, location);

			return location;
		}

		//
		// Current loop begin and end labels.
		//
		public Label LoopBegin, LoopEnd;

		//
		// Whether we are inside a loop and break/continue are possible.
		// 
		public bool  InLoop;

	}
}
