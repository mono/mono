// Instructions.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Mono.ILASM {


	/// <summary>
	/// </summary>
	public class InstrNone : InstrBase {

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrNone (OpCode op) : base (op)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host)
		{
			ilgen.Emit (this.Opcode);
		}
	}


	/// <summary>
	/// </summary>
	public class InstrVar : InstrBase {

		private object operand;

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrVar (OpCode op, object operand) : base (op)
		{
			this.operand = operand;
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host)
		{
			if (operand is string) {
				ilgen.Emit (Opcode, operand as string);
			} else if (operand is Int32) {
				ilgen.Emit (Opcode, (Int32)operand);
			}
		}
	}


	/// <summary>
	/// </summary>
	public class InstrI : InstrBase {

		private int operand;

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrI (OpCode op, int operand) : base (op) {
			this.operand = operand;
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
				ilgen.Emit (Opcode, operand);
		}
	}


	/// <summary>
	/// </summary>
	public class InstrI8 : InstrBase {

		private long operand;

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrI8 (OpCode op, long operand) : base (op) {
			this.operand = operand;
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
			ilgen.Emit (Opcode, operand);
		}
	}


	/// <summary>
	/// </summary>
	public class InstrR : InstrBase {

		private double operand;

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrR (OpCode op, double operand) : base (op) {
			this.operand = operand;
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
			if (Opcode.Name.IndexOf (".r4") != -1) {
				ilgen.Emit (Opcode, (float) operand);
			} else {
				ilgen.Emit (Opcode, operand);
			}
		}
	}



	/// <summary>
	/// </summary>
	public class InstrString : InstrBase {

		private string operand;

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrString (OpCode op, string operand) : base (op) {
			this.operand = operand;
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
			ilgen.Emit (Opcode, operand);
		}
	}


	/// <summary>
	///   Base class for instructions that call methods
	/// </summary>
	public abstract class InstrCallBase : InstrBase {
		
		private string return_type;
		private string binding_flags;
		private string calling_type;
		private string method_name;
		private ArrayList arg_list;
	
		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrCallBase (OpCode op, string binding_flags, string return_type, 
				string calling_type, string method_name, ArrayList arg_list) : base (op) {

			this.binding_flags = binding_flags;
			this.return_type = return_type;
			this.calling_type = calling_type;
			this.method_name = method_name;
			this.arg_list = arg_list;
		  	
		}

		protected Type[] CreateArgsTypeArray (CodeGen code_gen)
		{
			if (arg_list == null)
				return new Type[0];

			int size = arg_list.Count;
			Type[] type_array = new Type[size];

			for (int i=0; i<size; i++) {
				type_array[i] = code_gen.TypeManager[(string)arg_list[i]];
			}

			return type_array;
		}

		protected BindingFlags CreateBindingFlags ()
		{	
			if ((binding_flags == null) || (binding_flags == String.Empty))
				return (BindingFlags.Static | BindingFlags.Public);
	
			BindingFlags return_flags = BindingFlags.Public;
			
			switch (binding_flags) {
			case "instance":
				return_flags |= BindingFlags.Instance;
				break;
			}			

			return return_flags;
		}
		

	}

	/// <summary>
	///   call void System.Console::WriteLine(string)
	/// </summary>
	public class InstrCall : InstrCallBase {
	
		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrCall (OpCode op, string binding_flags, string return_type, string calling_type, 
			string method_name, ArrayList arg_list) : base (op, binding_flags, 
				return_type, calling_type, method_name, arg_list) {

		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
			Type type_to_call = host.CodeGen.TypeManager[calling_type];
			MethodInfo calling_method;
			
			if (type_to_call == host.TypeBuilder) {
				calling_method = host.GetMethod (method_name, CreateBindingFlags (), 
					CreateArgsTypeArray (host.CodeGen));
			} else {
				calling_method = type_to_call.GetMethod (method_name, CreateBindingFlags (), 
					null, CreateArgsTypeArray (host.CodeGen), null);
			}

			if (calling_method == null) {
				Console.WriteLine ("Method does not exist: {0}.{1}", type_to_call, method_name);
				return;
			}

			ilgen.Emit (Opcode, calling_method);
		
		}
		
	}

		/// <summary>
	/// </summary>
	public class InstrNewobj : InstrCallBase {
	
		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrNewobj (OpCode op, string binding_flags, string return_type, string calling_type, 
			string method_name, ArrayList arg_list) : base (op, binding_flags,
				return_type, calling_type, method_name, arg_list) {

		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen, Class host) {
			Type type_to_call = host.CodeGen.TypeManager[calling_type];
			ConstructorInfo calling_constructor = null;

			calling_constructor = type_to_call.GetConstructor (CreateArgsTypeArray (host.CodeGen));
			
			if (calling_constructor == null) {
				Console.WriteLine ("Constructor does not exist for: {0}", type_to_call);
				return;
			}

			ilgen.Emit (Opcode, calling_constructor);
		}
	}
}

