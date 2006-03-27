// InstrBase.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Reflection.Emit;

namespace Mono.ILASM {

	public abstract class InstrBase {

		private OpCode opcode;

		/// <summary>
		/// </summary>
		/// <param name="opcode"></param>
		public InstrBase (OpCode opcode)
		{
			this.opcode = opcode;
		}

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrBase (InstrToken tok) : this ((OpCode)tok.Value)
		{
		}


		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public InstrBase (ILToken tok) : this (tok as InstrToken)
		{
		}


		/// <summary>
		/// </summary>
		public OpCode Opcode {
			get {
				return opcode;
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="gen"></param>
		public abstract void Emit (ILGenerator ilgen, Class host);
	}
}
