// GCHandleType.cs
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
	/// <para> Represents the types of handles the
	///    <see cref="T:System.Runtime.InteropServices.GCHandle" /> class can allocate.</para>
	/// </summary>
	public enum GCHandleType {

		/// <summary><para>A <see cref="F:System.Runtime.InteropServices.GCHandleType.Normal" /> is an opaque 
		///    handle, and the address of the object it references cannot be resolved through
		///    it. The <see cref="F:System.Runtime.InteropServices.GCHandleType.Normal" /> also prevents the
		///    collection of the referenced object by the GC.</para></summary>
		Normal = 2,

		/// <summary><para> Similar to <see cref="F:System.Runtime.InteropServices.GCHandleType.Normal" />, but allows the
		///  address of the object, which the current <see cref="T:System.Runtime.InteropServices.GCHandle" />
		///  
		///  represents to be taken.</para></summary>
		Pinned = 3,
	} // GCHandleType

} // System.Runtime.InteropServices
