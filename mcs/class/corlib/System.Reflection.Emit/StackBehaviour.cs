// StackBehaviour.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {

	/// <summary>
	///  Describes how values are pushed onto or popped off a stack.
	/// </summary>
	public enum StackBehaviour {

		/// <summary>
		/// </summary>
		Pop0 = 0,

		/// <summary>
		/// </summary>
		Pop1 = 1,

		/// <summary>
		/// </summary>
		Pop1_pop1 = 2,

		/// <summary>
		/// </summary>
		Popi = 3,

		/// <summary>
		/// </summary>
		Popi_pop1 = 4,

		/// <summary>
		/// </summary>
		Popi_popi = 5,

		/// <summary>
		/// </summary>
		Popi_popi8 = 6,

		/// <summary>
		/// </summary>
		Popi_popi_popi = 7,

		/// <summary>
		/// </summary>
		Popi_popr4 = 8,

		/// <summary>
		/// </summary>
		Popi_popr8 = 9,

		/// <summary>
		/// </summary>
		Popref = 0x0A,

		/// <summary>
		/// </summary>
		Popref_pop1 = 0x0B,

		/// <summary>
		/// </summary>
		Popref_popi = 0x0C,

		/// <summary>
		/// </summary>
		Popref_popi_popi = 0x0D,

		/// <summary>
		/// </summary>
		Popref_popi_popi8 = 0x0E,

		/// <summary>
		/// </summary>
		Popref_popi_popr4 = 0x0F,

		/// <summary>
		/// </summary>
		Popref_popi_popr8 = 0x10,

		/// <summary>
		/// </summary>
		Popref_popi_popref = 0x11,

		/// <summary>
		/// </summary>
		Push0 = 0x12,

		/// <summary>
		/// </summary>
		Push1 = 0x13,

		/// <summary>
		/// </summary>
		Push1_push1 = 0x14,

		/// <summary>
		/// </summary>
		Pushi = 0x15,

		/// <summary>
		/// </summary>
		Pushi8 = 0x16,

		/// <summary>
		/// </summary>
		Pushr4 = 0x17,

		/// <summary>
		/// </summary>
		Pushr8 = 0x18,

		/// <summary>
		/// </summary>
		Pushref = 0x19,

		/// <summary>
		/// </summary>
		Varpop = 0x1A,

		/// <summary>
		/// </summary>
		Varpush = 0x1B
	}

}
