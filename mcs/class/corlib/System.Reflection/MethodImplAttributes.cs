// MethodImplAttributes.cs
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
	/// <para> Specifies flags for the attributes of a method
	///                   implementation.</para>
	/// </summary>
	/// <remarks>
	/// <para>The attributes are combined using the bitwise OR operation as
	///                   follows:</para>
	/// <para>code impl mask</para>
	/// <para>CodeTypeMask = 0x0003</para>
	/// <para>IL =0x0000</para>
	/// <para>Native = 0x0001</para>
	/// <para>OPTIL = 0x0002</para>
	/// <para>Runtime = 0x0003</para>
	/// <para>managed mask</para>
	/// <para>ManagedMask = 0x0004</para>
	/// <para>Unmanaged = 0x0004</para>
	/// <para>Managed = 0x0000</para>
	/// <para>implementation info and interop</para>
	/// <para>ForwardRef = 0x0010</para>
	/// <para>OLE = 0x0080</para>
	/// <para>InternalCall=0x1000</para>
	/// <para>Synchronized = 0x0020</para>
	/// <para>NoInlining = 0x0008</para>
	/// <para>OneWay = 0x0040</para>
	/// <para>MaxMethodImplVal = 0xFFFF</para>
	/// </remarks>
	/// <seealso cref="N:System.Reflection" />
	public enum MethodImplAttributes {

		/// <summary>
		/// <para>Specifies flags about code type.</para>
		/// </summary>
		CodeTypeMask = 3,

		/// <summary>
		/// <para>Specifies that the method implementation is in Microsoft intermediate language (MSIL).</para>
		/// </summary>
		IL = 0,

		/// <summary>
		/// <para>Specifies that the method implementation is native.</para>
		/// </summary>
		Native = 1,

		/// <summary>
		/// <para>Specifies that the method implementation is in Optimized Intermediate Language (OPTIL).</para>
		/// </summary>
		OPTIL = 2,

		/// <summary>
		/// <para>Specifies that the method implementation is provided by the runtime.</para>
		/// </summary>
		Runtime = 3,

		/// <summary>
		/// <para>Specifies whether the code is managed or unmanaged.</para>
		/// </summary>
		ManagedMask = 4,

		/// <summary>
		/// <para>Specifies that the method implementation is unmanaged, otherwise
		///                   managed.</para>
		/// </summary>
		Unmanaged = 4,

		/// <summary>
		/// <para>Specifies that the method implementation is managed, otherwise unmanaged.</para>
		/// </summary>
		Managed = 0,

		/// <summary>
		/// <para>Specifies that the method is not defined.</para>
		/// </summary>
		ForwardRef = 16,

		/// <summary>
		/// <para>Specifies that the method signature is mangled to return an HRESULT, with a 
		///                   retval as a parameter.</para>
		/// </summary>
		OLE = 128,

		/// <summary>
		/// <para>Specifies an internal call.</para>
		/// </summary>
		InternalCall = 4096,

		/// <summary>
		/// <para>Specifies that the method is single-threaded through the body.</para>
		/// </summary>
		Synchronized = 32,

		/// <summary>
		/// <para>Specifies that the method may not be inlined.</para>
		/// </summary>
		NoInlining = 8,

		/// <summary>
		/// <para>Specifies that the method has a <see langword="void" /> return value and [in] 
		///                parameters only.</para>
		/// </summary>
		OneWay = 64,

		/// <summary>
		/// <para>Specifies a range check value.</para>
		/// </summary>
		MaxMethodImplVal = 65535,
	} // MethodImplAttributes

} // System.Reflection
