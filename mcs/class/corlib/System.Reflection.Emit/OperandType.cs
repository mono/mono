// OperandType.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {

	/// <summary>
	///  Describes the operand types of MSIL instructions.
	/// </summary>
	public enum OperandType {

		/// <summary>
		/// </summary>
		InlineBrTarget = 0,

		/// <summary>
		/// </summary>
		InlineField = 1,

		/// <summary>
		/// </summary>
		InlineI = 2,

		/// <summary>
		/// </summary>
		InlineI8 = 3,

		/// <summary>
		/// </summary>
		InlineMethod = 4,

		/// <summary>
		/// </summary>
		InlineNone = 5,

		/// <summary>
		/// </summary>
		InlinePhi = 6,

		/// <summary>
		/// </summary>
		InlineR = 7,

		/// <summary>
		/// </summary>
		InlineSig = 9,

		/// <summary>
		/// </summary>
		InlineString = 0x0A,

		/// <summary>
		/// </summary>
		InlineSwitch = 0x0B,

		/// <summary>
		/// </summary>
		InlineTok = 0x0C,

		/// <summary>
		/// </summary>
		InlineType = 0x0D,

		/// <summary>
		/// </summary>
		InlineVar = 0x0E,

		/// <summary>
		/// </summary>
		ShortInlineBrTarget = 0x0F,

		/// <summary>
		/// </summary>
		ShortInlineI = 0x10,

		/// <summary>
		/// </summary>
		ShortInlineR = 0x11,

		/// <summary>
		/// </summary>
		ShortInlineVar = 0x12
	}

}

