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
			
			if (methods != null) {
				foreach (Method m in methods) {
					m.Emit (this);
					if (m.IsEntryPoint)
						cg.SetEntryPoint (m.Info);
				}
			}
			
			TypeBuilder.CreateType();
		}

	}
}

