// LayoutKind.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.InteropServices {


	/// <summary>
	/// <para> Indicates the physical memory layout of objects exported
	///       to unmanaged code.</para>
	/// </summary>
	public enum LayoutKind {

		/// <summary><para> Indicates that object members are laid out sequentially, in the
		///  order in which they appear in that object's type definition.</para></summary>
		Sequential = 0,

		/// <summary><para> Indicates that the precise position of each member of an
		///  object is explicitly controlled in unmanaged memory. Each member of the exported
		///  class or structure is required to
		///  use the <see cref="T:System.Runtime.InteropServices.FieldOffsetAttribute" /> to indicate the position of that
		///  field within the type.</para><para><block subset="none" type="note">See the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> class overview for an example of
		///  the use of the <see cref="T:System.Runtime.InteropServices.FieldOffsetAttribute" />.</block></para></summary>
		Explicit = 2,

		/// <summary><para> Indicates that the appropriate layout of members of an 
		///  object is automatically chosen. <block subset="none" type="note">The layout in this case is implementation defined.</block></para></summary>
		Auto = 3,
	} // LayoutKind

} // System.Runtime.InteropServices
