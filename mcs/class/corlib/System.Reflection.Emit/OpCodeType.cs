// OpCodeType.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {

	/// <summary>
	///  Describes the types of MSIL instructions.
	/// </summary>
	public enum OpCodeType {

		/// <summary>
		///  "Ignorable" instruction.
		///  Such instruction are used to supply
		///  additional information to particular
		///  MSIL processor.
		/// </summary>
		Annotation = 0,

		/// <summary>
		///  Denotes "shorthand" instruction.
		///  Such instructions take less space
		///  than their full-size equivalents
		///  (ex. ldarg.0 vs. ldarg 0).
		/// </summary>
		Macro = 1,

		/// <summary>
		///  Denotes instruction reserved for internal use.
		/// </summary>
		Nternal = 2,

		/// <summary>
		///  Denotes instruction to deal with objects.
		///  (ex. ldobj).
		/// </summary>
		Objmodel = 3,

		/// <summary>
		/// </summary>
		Prefix = 4,

		/// <summary>
		/// </summary>
		Primitive = 5
	}

}
