// UnmanagedType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.InteropServices {


	/// <summary>
	/// <para>
	///             	  Identifies how parameters or fields should be marshaled to unmanaged
	///             	  code.
	///                </para>
	/// </summary>
	public enum UnmanagedType {

		/// <summary>
		/// <para>
		///             	  4-byte Boolean value (<see langword="true " />!= 0, <see langword="false " />=
		///             	  0).
		///                </para>
		/// </summary>
		Bool = 2,

		/// <summary>
		/// <para>
		///             	  1-byte signed integer.
		///                </para>
		/// </summary>
		I1 = 3,

		/// <summary>
		/// <para>
		///             	  1-byte unsigned integer.
		///                </para>
		/// </summary>
		U1 = 4,

		/// <summary>
		/// <para>
		///             	  2-byte signed integer.
		///                </para>
		/// </summary>
		I2 = 5,

		/// <summary>
		/// <para>
		///             	  2-byte unsigned integer.
		///                </para>
		/// </summary>
		U2 = 6,

		/// <summary>
		/// <para>
		///             	  4-byte signed integer.
		///                </para>
		/// </summary>
		I4 = 7,

		/// <summary>
		/// <para>
		///             	  4-byte unsigned integer.
		///                </para>
		/// </summary>
		U4 = 8,

		/// <summary>
		/// <para>
		///             	  8-byte signed integer.
		///                </para>
		/// </summary>
		I8 = 9,

		/// <summary>
		/// <para>
		///             	  8-byte unsigned integer.
		///                </para>
		/// </summary>
		U8 = 10,

		/// <summary>
		/// <para>
		///             	  4-byte floating point number.
		///                </para>
		/// </summary>
		R4 = 11,

		/// <summary>
		/// <para>
		///             	  8-byte floating point number.
		///                </para>
		/// </summary>
		R8 = 12,

		/// <summary>
		/// <para>
		///             	  Unicode character string that is a length prefixed double
		///             	  byte.
		///                </para>
		/// </summary>
		BStr = 19,

		/// <summary>
		/// <para>
		///             	  A single byte ANSI character string.
		///                </para>
		/// </summary>
		LPStr = 20,

		/// <summary>
		/// <para>
		///             	  A double byte Unicode character string.
		///                </para>
		/// </summary>
		LPWStr = 21,

		/// <summary>
		/// <para>
		///             	  A platform independent character string, ANSI on Win9x, Unicode on WinNT.
		///                </para>
		/// </summary>
		LPTStr = 22,

		/// <summary>
		/// <para>Used for in-line fixed length character arrays that 
		///                   appear within a structure. The character type used with
		///                <see langword="ByValTStr" /> is determined by the <see cref="T:System.Runtime.InteropServices.CharSet" /> argument of the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> 
		///                applied to the containing structure.</para>
		/// </summary>
		ByValTStr = 23,

		/// <summary>
		/// <para>A COM <see langword="IUnknown" /> pointer. This only applies to a generic object, not a derived class.</para>
		/// </summary>
		IUnknown = 25,

		/// <summary>
		/// <para>A COM IDispatch pointer. This only applies to a generic object, not a derived class.</para>
		/// </summary>
		IDispatch = 26,

		/// <summary>
		/// <para>
		///             	  A C-style structure, used to marshal managed formatted classes and value
		///             	  types.
		///                </para>
		/// </summary>
		Struct = 27,

		/// <summary>
		/// <para>A COM interface pointer. The <see cref="T:System.Guid" /> of the interface is obtained from the class
		///                metadata.</para>
		/// </summary>
		Interface = 28,

		/// <summary>
		///                SafeArrays are self-describing arrays that carry the
		///                type, rank and bounds of the associated array data.
		///             </summary>
		SafeArray = 29,

		/// <summary>
		/// <para>
		///             	  A fixed length array. The UnmanagedFormatAttribute must contain the count of
		///             	  elements in the array. The UnmanagedFormatAttribute may optionally contain the
		///             	  unmanaged type of the elements when it is necessary to differentiate among
		///             	  string types.
		///                </para>
		/// </summary>
		ByValArray = 30,

		/// <summary>
		/// <para>
		///             	  A platform independent signed integer. 4-bytes on 32 bit Windows, 8-bytes on
		///             	  64 bit Windows.
		///                </para>
		/// </summary>
		SysInt = 31,

		/// <summary>
		/// <para>
		///             	  Hardware natural sized unsigned integer
		///                </para>
		/// </summary>
		SysUInt = 32,

		/// <summary>
		/// <para>
		///             	  VB specific.
		///                </para>
		/// </summary>
		VBByRefStr = 34,

		/// <summary>
		/// <para>
		///             	  ANSI character string that is a length prefixed, single byte.
		///                </para>
		/// </summary>
		AnsiBStr = 35,

		/// <summary>
		/// <para>
		///             	  A length prefixed platform independent char string. ANSI on Windows 9x,
		///             	  Unicode on Windows NT.
		///                </para>
		/// </summary>
		TBStr = 36,

		/// <summary>
		/// <para>
		///             	  8-byte unsigned integer.
		///                </para>
		/// </summary>
		VariantBool = 37,

		/// <summary>
		/// <para>
		///             	  A function pointer.
		///                </para>
		/// </summary>
		FunctionPtr = 38,

		/// <summary>
		/// <para>
		///             	  An un-typed 4-byte pointer.
		///                </para>
		/// </summary>
		LPVoid = 39,

		/// <summary>
		/// <para>Dynamic type that determines the <see cref="T:System.Type" /> of an object at 
		///                runtime and marshals the object as that <see cref="T:System.Type" />
		///                .</para>
		/// </summary>
		AsAny = 40,

		/// <summary>
		/// <para>
		///             	  Size agnostic floating point number.
		///                </para>
		/// </summary>
		RPrecise = 41,

		/// <summary>
		/// <para>An array whose length is determined at runtime by the size of the actual
		///                   marshaled array. Optionally followed by the unmanaged type of the elements
		///                   within the array when it is necessary to differentiate among string types. When
		///                   marshaling from managed to unmanaged, the size of the array is determined
		///                   dynamically. When marshaling from unmanaged to managed, the size is always
		///                   assumed to be 1.</para>
		/// </summary>
		LPArray = 42,

		/// <summary>
		/// <para>
		///             	  A pointer to a C-style structure. Used to marshal managed formatted classes
		///             	  and value types.
		///                </para>
		/// </summary>
		LPStruct = 43,

		/// <summary>
		/// <para>
		///             	  Custom marshaler native type. This must be followed 
		///             	  by a string of the following format:
		///             	  "Native type name\0Custom marshaler type name\0Optional cookie\0"
		///             	  Or
		///             	  "{Native type GUID}\0Custom marshaler type name\0Optional cookie\0"
		///                </para>
		/// </summary>
		CustomMarshaler = 44,

		/// <summary>
		/// <para>This native type associated with an <see cref="F:System.Runtime.InteropServices.UnmanagedType.I4" /> or a <see cref="F:System.Runtime.InteropServices.UnmanagedType.U4" /> will cause the parameter
		///                to be exported as an HRESULT in the exported typelib.</para>
		/// </summary>
		Error = 45,

		/// <summary>
		/// <para>First invalid element type. </para>
		/// </summary>
		NativeTypeMax = 80,
	} // UnmanagedType

} // System.Runtime.InteropServices
