// CodeGen.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {

	public class CodeGen {

		private string name;
		private string output_file;
		private AssemblyBuilder asmbld;
		private ModuleBuilder modbld;
		private TypeManager type_manager;
		
		private Types refTypes = new Types (); // FIXME: postpone init

		private ArrayList classes;


		/// <summary>
		/// </summary>
		/// <param name="output_file">The path of the output file</param>
		public CodeGen (string output_file)
		{
			this.output_file = output_file;
			this.type_manager = new TypeManager ();
		}


		/// <summary>
		/// </summary>
		public CodeGen ()
		{
		}


		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public void SetName (string name)
		{
			this.name = name;
			AppDomain appDomain = AppDomain.CurrentDomain;
			AssemblyName asmName = new AssemblyName();
			asmName.Name = name + "_asmname";
			asmbld = appDomain.DefineDynamicAssembly (asmName, AssemblyBuilderAccess.RunAndSave);

			// FIXME: exe/lib
			modbld = asmbld.DefineDynamicModule (name, name + ".exe");
		}

		public void SetEntryPoint (MethodInfo entry_point)
		{
			if (asmbld.EntryPoint != null)
				Console.WriteLine ("Multiple entry points defined.");

			asmbld.SetEntryPoint (entry_point);
		}


		/// <summary>
		/// </summary>
		public ModuleBuilder ModBuilder {
			get {
				return modbld;
			}
		}


		/// <summary>
		/// </summary>
		public Types RefTypes {
			get {
				return refTypes;
			}
		}

		public TypeManager TypeManager {
			get {
				return type_manager;
			}
		}

		/// <summary>
		/// </summary>
		public int ClassCount {
			get {
				return (classes == null) ? 0 : classes.Count;
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="clazz"></param>
		public void AddClass (Class clazz)
		{
			if (classes == null) classes = new ArrayList ();
			classes.Add (clazz);
		}


		/// <summary>
		/// </summary>
		public void Emit ()
		{
			if (ClassCount != 0) {
				foreach (Class c in classes) {
					c.Emit (this);
				}
			}

			asmbld.Save (output_file);
		}

	}

}

