// PackingSize.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {

	/// <summary>
	///  Specifies the packing size (data alignment) of a type.
	/// </summary>
	[Flags]
	public enum PackingSize {

		/// <summary>
		///  The packing size is unspecified.
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// </summary>
		Size1 = 1,

		/// <summary>
		/// </summary>
		Size2 = 2,

		/// <summary>
		/// </summary>
		Size4 = 4,

		/// <summary>
		/// </summary>
		Size8 = 8,

		/// <summary>
		/// </summary>
		Size16 = 16
	}

}


