// Instructions.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Reflection;
using System.Reflection.Emit;

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
		public override void Emit (ILGenerator ilgen)
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
		public override void Emit (ILGenerator ilgen)
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
		public override void Emit (ILGenerator ilgen) {
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
		public override void Emit (ILGenerator ilgen) {
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
		public override void Emit (ILGenerator ilgen) {
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
		public override void Emit (ILGenerator ilgen) {
			ilgen.Emit (Opcode, operand);
		}
	}



	/// <summary>
	///    void System.Console::WriteLine(string)
	/// </summary>
	public class InstrCall : InstrBase {
		
		private string return_type;
		private string assembly_name;
		private string calling_type;
		private string method_name;
		private string args;
	
		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrCall (OpCode op, string return_type, string assembly_name, 
				string calling_type, string method_name, string args) : base (op) {
			
			this.return_type = return_type;
			this.assembly_name = assembly_name;
			this.calling_type = calling_type;
			this.method_name = method_name;
			this.args = args;
		  	
		}


		/// <summary>
		/// </summary>
		/// <param name="ilgen"></param>
		public override void Emit (ILGenerator ilgen) {
			// Create MethodInfo
			Assembly assembly = Assembly.LoadWithPartialName (assembly_name);		
			Type type_to_call = assembly.GetType (calling_type);
			MethodInfo calling_method = type_to_call.GetMethod (method_name, TypeArray ());
			
			ilgen.Emit (Opcode, calling_method);
		
		}

		private Type[] TypeArray ()
		{
			string[] type_name_list = args.Split (',');
			Type[] type_list = new Type[type_name_list.Length];
			Types type_manager = new Types ();

			for (int i=0; i<type_name_list.Length; i++) {
				type_list[i] = type_manager.Lookup (type_name_list[i]);
			}

			return type_list;
		}

		
	}
}
