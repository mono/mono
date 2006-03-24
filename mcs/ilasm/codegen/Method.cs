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

		private ArrayList param_list;
		private ArrayList instructions;
		private ArrayList local_list;

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
				throw new InternalErrorException ("<null> instruction");
			}

			if (instructions == null) {
				this.instructions = new ArrayList ();
			}

			instructions.Add (instr);
		}

		public void AddLocal (DictionaryEntry local)
		{
			if (local_list == null)
				local_list = new ArrayList ();

			local_list.Add (local);
	
		}

		public void SetParamList (ArrayList param_list)
		{
			this.param_list = param_list;
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

		public MethodBuilder Builder {
			get {
				return method_builder;
			}
		}

		public void Resolve (Class host)
		{
			Type return_type = host.CodeGen.TypeManager[RetType];
			method_builder = host.TypeBuilder.DefineMethod (Name, Attrs, 
				CallConv, return_type, CreateTypeList (host.CodeGen.TypeManager));
		}

		/// <summary>
		/// </summary>
		/// <param name="tb"></param>
		public void Emit (Class host)
		{
			TypeBuilder tb = host.TypeBuilder;

			if (IsCtor) {
			} else {
				ILGenerator ilgen = method_builder.GetILGenerator ();

				if (local_list != null) {
					foreach (DictionaryEntry local in local_list) {
						Type local_type = host.CodeGen.TypeManager[(string)local.Key];
						if (local_type == null) {
							Console.WriteLine ("Could not find type: {0}", local.Key);
							return;
						}
						ilgen.DeclareLocal (local_type);
					}
				}

				if (instructions != null) {
					foreach (InstrBase instr in instructions)
						instr.Emit (ilgen, host);
				}
			}
		}

		private Type[] CreateTypeList (TypeManager type_manager)
		{
			if (param_list == null)
				return new Type[0];

			int count = param_list.Count;
			Type[] type_list = new Type[count];
			
			for (int i=0; i<count; i++) {
				type_list[i] = type_manager[(string) param_list[i]];
			}
		
			return type_list;
		}

	}

	
}
