// Class.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {

	/// <summary>
	/// </summary>
	public class ClassName {

		private string name;
		private string assembly;
		private string module;


		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public ClassName (string name) {
			this.name = name;
			this.assembly = String.Empty;
			this.module = String.Empty;
		}


		public string Name {
			get {
				return name;
			}
		}


	}


	/// <summary>
	/// </summary>
	public class Class {

		private string name;

		// extends clause
		private ClassName baseClass;

		// implements clause
		private ArrayList interfaces;

		private ArrayList methods;

		private TypeBuilder tb;

		private CodeGen codgen;

		private TypeAttributes attrs;



		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public Class (string name)
		{
			this.name = name;
		}



		/// <summary>
		/// </summary>
		/// <param name="m"></param>
		public void AddMethod (Method m)
		{
			if (methods == null) methods = new ArrayList ();
			methods.Add (m);
		}


		/// <summary>
		/// </summary>
		public CodeGen CodeGen {
			get {
				return codgen;
			}
		}


		/// <summary>
		/// </summary>
		public TypeAttributes Attrs {
			get {
				return attrs;
			}
			set {
				attrs = value;
			}
		}


		/// <summary>
		/// </summary>
		public TypeBuilder TypeBuilder {
			get {
				if (tb == null && codgen != null) {
					tb = codgen.ModBuilder.DefineType (name, Attrs);
				}
				return tb;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="cg"></param>
		public void Emit (CodeGen cg)
		{
			codgen = cg;
			
			TypeBuilder.CreateType();
			cg.TypeManager[name] = TypeBuilder;

			if (methods != null) {
				foreach (Method m in methods) 
					m.Resolve (this);
			}

			if (methods != null) {
				foreach (Method m in methods) {
					m.Emit (this);
					if (m.IsEntryPoint)
						cg.SetEntryPoint (m.Builder);
				}
			}	
		}
		
		// This can be removed when System.Reflection.Emit.TypeBuilder.GetMethod is implemented
		// TODO: This function needs allot of work
		public MethodInfo GetMethod (string method_name, BindingFlags binding_flags,
			Type[] param_type_list)
		{
			foreach (Method method in methods) {
				if (method.Name != method_name)
					continue;
				ParameterInfo[] param_info = method.Builder.GetParameters ();
				if (param_info == null) {
					if (param_type_list.Length == 0)
						return method.Builder;
					else
						continue;
				}
				int size = param_info.Length;
				if (param_type_list.Length != size)
					continue;
				for (int i=0; i<size; i++) {
					if (param_type_list[i] != param_info[i].ParameterType)
						goto end;	
				}

				return method.Builder;
				end: continue;
			}
	
			return null;
		}
	}
}

