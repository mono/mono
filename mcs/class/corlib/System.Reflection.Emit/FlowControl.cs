// FlowControl.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com



namespace System.Reflection.Emit {

	/// <summary>
	///  Describes how an instruction alters the flow of control.
	/// </summary>
	public enum FlowControl {

		/// <summary>
		/// Branch instruction (ex: br, leave).
		/// </summary>
		Branch = 0,

		/// <summary>
		///  Break instruction (ex: break).
		/// </summary>
		Break = 1,

		/// <summary>
		///  Call instruction (ex: jmp, call, callvirt).
		/// </summary>
		Call = 2,

		/// <summary>
		///  Conditional branch instruction (ex: brtrue, brfalse).
		/// </summary>
		Cond_Branch = 3,

		/// <summary>
		///  Changes the behaviour of or provides additional
		///  about a subsequent instruction. 
		///  (ex: prefixes such as volatile, unaligned).
		/// </summary>
		Meta = 4,

		/// <summary>
		///  Transition to the next instruction.
		/// </summary>
		Next = 5,

		/// <summary>
		///  Annotation for ann.phi instruction.
		/// </summary>
		Phi = 6,

		/// <summary>
		///  Return instruction.
		/// </summary>
		Return = 7,

		/// <summary>
		///  Throw instruction.
		/// </summary>
		Throw = 8
	}

}

