// TypeAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// <para>Specifies type attributes. This Enum matches the CorTypeAttr defined in 
	///                   CorHdr.h.</para>
	/// </summary>
	[Flags]
	public enum TypeAttributes {

		/// <summary>
		/// <para>Specifies type visibility information.</para>
		/// </summary>
		VisibilityMask = 7,

		/// <summary>
		/// <para> 
		///                   Specifies that the class is not public.</para>
		/// </summary>
		NotPublic = 0,

		/// <summary>
		/// <para> 
		///                   Specifies that the class is public.</para>
		/// </summary>
		Public = 1,

		/// <summary>
		/// <para> Specifies
		///                   that the class is nested with public visibility.</para>
		/// </summary>
		NestedPublic = 2,

		/// <summary>
		/// <para> 
		///                   Specifies that the class is nested with private visibility.</para>
		/// </summary>
		NestedPrivate = 3,

		/// <summary>
		/// <para>Specifies that the class is nested with family visibility, and is thus accessible
		///                   only by methods within its own type and any sub-types.</para>
		/// </summary>
		NestedFamily = 4,

		/// <summary>
		/// <para>Specifies that the class is nested with assembly visibility, and is thus accessible
		///                   only by methods within its assembly.</para>
		/// </summary>
		NestedAssembly = 5,

		/// <summary>
		/// <para>Specifies that the class is nested with assembly and family visibility, and is thus accessible
		///                   only by methods lying in the intersection of its family and assembly.</para>
		/// </summary>
		NestedFamANDAssem = 6,

		/// <summary>
		/// <para>Specifies that the class is nested with family or 
		///                   assembly visibility, and is thus accessible
		///                   only by methods lying in the union of its family and assembly.</para>
		/// </summary>
		NestedFamORAssem = 7,

		/// <summary>
		/// <para>Specifies class layout information.</para>
		/// </summary>
		LayoutMask = 24,

		/// <summary>
		/// <para>Specifies that class fields are automatically laid out by the Common Language Runtime.</para>
		/// </summary>
		AutoLayout = 0,

		/// <summary>
		/// <para>Specifies that class fields are laid out
		///                   sequentially, in the order that the fields were emitted to the metadata.</para>
		/// </summary>
		LayoutSequential = 8,

		/// <summary>
		/// <para>Specifies that class fields are laid out at the
		///                   specified offsets.</para>
		/// </summary>
		ExplicitLayout = 16,

		/// <summary>
		/// <para>Specifies class semantics information; the current class is contextful (else agile).</para>
		/// </summary>
		ClassSemanticsMask = 96,

		/// <summary>
		/// <para> Specifies that the type is a class.</para>
		/// </summary>
		Class = 0,

		/// <summary>
		/// <para>Specifies that the type is an interface.</para>
		/// </summary>
		Interface = 32,

		/// <summary>
		/// <para>Specifies that the type is a managed value type.</para>
		/// </summary>
		ValueType = 64,

		/// <summary>
		/// <para>Specifies that the type is never allocated from the garbage
		///                   collection heap. This is used for Interop.</para>
		/// </summary>
		UnmanagedValueType = 96,

		/// <summary>
		/// <para>Specifies that the type is abstract.</para>
		/// </summary>
		Abstract = 128,

		/// <summary>
		/// <para>Specifies that the class is concrete
		///                   and cannot be extended.</para>
		/// </summary>
		Sealed = 256,

		/// <summary>
		/// <para>Specifies that the type is an enumeration.</para>
		/// </summary>
		Enum = 512,

		/// <summary>
		/// <para>Specifies that the class is special in a way denoted by the
		///                   name.</para>
		/// </summary>
		SpecialName = 1024,

		/// <summary>
		/// <para>Specifies that the class or interface is imported from another
		///                   module.</para>
		/// </summary>
		Import = 4096,

		/// <summary>
		/// <para>Specifies that the class can be serialized.</para>
		/// </summary>
		Serializable = 8192,
		StringFormatMask = 196608,
		AnsiClass = 0,
		UnicodeClass = 65536,
		AutoClass = 131072,
		LateInit = 524288,
		ReservedMask = 264192,
		RTSpecialName = 2048,
		HasSecurity = 262144,
	} // TypeAttributes

} // System.Reflection
