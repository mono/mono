// Method.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {


	public class MethodName {
		private static int methodCount = 0;

		private bool isCtor;
		private string name;
		
		/// <summary>
		/// </summary>
		public MethodName () : this ("M_" + (methodCount++))
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public MethodName (string name) : this (name, false)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <param name="ctor"></param>
		public MethodName (string name, bool ctor)
		{
			this.name = name;
			this.isCtor = ctor;
		}


		/// <summary>
		/// </summary>
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}


		/// <summary>
		/// </summary>
		public bool IsCtor {
			get {
				return isCtor;
			}
			set {
				isCtor = value;
			}
		}

	}



	/// <summary>
	/// </summary>
	public class Method {

		private MethodName name;
		private MethodAttributes attrs;
		private CallingConventions callConv;
		private string retType;
		private MethodBuilder method_builder;
		private bool entry_point = false;

		private ArrayList instructions;


		/// <summary>
		/// </summary>
		public Method ()
		{
			name = new MethodName ();
			attrs = 0;
		}


		/// <summary>
		/// </summary>
		public string Name {
			get {
				return name.Name;
			}
			set {
				name.Name = value;
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public void SetMethodName (MethodName name)
		{
			this.name = name;
		}


		/// <summary>
		/// </summary>
		public bool IsCtor {
			get {
				return name.IsCtor;
			}
			set {
				name.IsCtor = value;
			}
		}


		/// <summary>
		/// </summary>
		public string RetType {
			get {
				return retType;
			}
			set {
				retType = value;
			}
		}


		/// <summary>
		/// </summary>
		public MethodAttributes Attrs {
			get {
				return attrs;
			}
			set {
				attrs = value;
			}
		}


		/// <summary>
		/// </summary>
		public CallingConventions CallConv {
			get {
				return callConv;
			}
			set {
				callConv = value;
			}
		}


		/// <summary>
		/// </summary>
		public bool IsEntryPoint {
			get {
				return entry_point;
			}
			set {
				entry_point = value;
			}
		}		

		/// <summary>
		/// </summary>
		/// <param name="instr"></param>
		public void AddInstruction (InstrBase instr)
		{
			if (instr == null) {
				throw new System.NullReferenceException ("<null> instruction");
			}

			if (instructions == null) {
				this.instructions = new ArrayList ();
			}

			instructions.Add (instr);
		}


		/// <summary>
		/// </summary>
		public int InstrCount {
			get {
				return (instructions != null) ? instructions.Count : 0;
			}
		}



		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString ()
		{
			return String.Format ("IL.Method [Name: {0}, Attrs: {1}, CallConv: {2}, RetType: {3}, Instr: {4}]",
			                      Name, Attrs, CallConv, RetType, InstrCount);
		}

		public MethodInfo Info {
			get {
				return method_builder;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="tb"></param>
		public void Emit (Class host)
		{
			TypeBuilder tb = host.TypeBuilder;

			if (IsCtor) {
			} else {
				Type rt = host.CodeGen.RefTypes.Lookup (RetType);
				method_builder = tb.DefineMethod (Name, Attrs, CallConv, rt, null);
				ILGenerator ilgen = method_builder.GetILGenerator ();
				if (instructions != null) {
					foreach (InstrBase instr in instructions) {
						instr.Emit (ilgen);
					}
				}
			}
		}

	}

}
