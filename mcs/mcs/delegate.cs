//
// delegate.cs: Delegate Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {
	
	public class Delegate {

		public readonly string Name;
		public readonly string ReturnType;
		public int    mod_flags;
		public Parameters Parameters;
		public Attributes OptAttributes;
		public TypeBuilder TypeBuilder;

		public readonly RootContext RootContext;
		
		Location loc;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (RootContext rc, string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs, Location loc)
		{
			this.RootContext = rc;
			this.Name       = name;
			this.ReturnType = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			Parameters      = param_list;
			OptAttributes   = attrs;
			this.loc        = loc;
		}

		public void DefineDelegate (object parent_builder)
		{
			TypeAttributes attr;
			
			if (parent_builder is ModuleBuilder) {
				ModuleBuilder builder = (ModuleBuilder) parent_builder;
				
				attr = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineType (Name, attr, TypeManager.delegate_type);
								  
			} else {
				TypeBuilder builder = (TypeBuilder) parent_builder;
				
				attr = TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineNestedType (Name, attr, TypeManager.delegate_type);

			}

			RootContext.TypeManager.AddDelegateType (Name, TypeBuilder);
		}

		public void Populate (TypeContainer parent)
		{

			Type [] const_arg_types = new Type [2];

			const_arg_types [0] = TypeManager.object_type;

			// FIXME : How do I specify a "native int" or void * here ?
			// This is surely not right !
			const_arg_types [1] = TypeManager.int32_type;
			
			ConstructorBuilder cb = TypeBuilder.DefineConstructor (
						    MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
						    MethodAttributes.HideBySig | MethodAttributes.Public,
						    CallingConventions.Standard,
						    const_arg_types);
			
			cb.SetImplementationFlags (MethodImplAttributes.Runtime);
			
			// Here the various methods like Invoke, BeginInvoke etc are defined

			Type [] param_types = Parameters.GetParameterInfo (parent);
			Type ret_type = parent.LookupType (ReturnType, false);

			MethodBuilder mb = TypeBuilder.DefineMethod ("Invoke", 
					       MethodAttributes.Public | MethodAttributes.HideBySig,		     
					       Parameters.GetCallingConvention (),
					       ret_type,		     
					       param_types);

			mb.SetImplementationFlags (MethodImplAttributes.Runtime);

			// FIXME : The asynchronous ones BeginInvoke, EndInvoke come here
		}
		
		public void CloseDelegate ()
		{
			TypeBuilder.CreateType ();
		}
		
		public int ModFlags {
			get {
				return mod_flags;
			}
		}
		

	}
	
}
