// Instructions.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
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


}
