// PInvokeMap.cs
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
	///                   Defines the PInvoke attributes. These values are defined in
	///                   CorHdr.h.
	///                </para>
	/// </summary>
	/// <remarks>
	/// <para>
	///                   PInvoke
	///                   is Platform Invocation Services. PInvoke allows managed code to
	///                   call unmanaged functions that are implemented in a DLL.
	///                </para>
	/// </remarks>
	public enum PInvokeMap {

		/// <summary>
		/// <para>
		///                   Indicates the PInvoke is to use the member name as specified.
		///                </para>
		/// </summary>
		NoMangle = 1,

		/// <summary>
		/// <para>
		///                   Heuristic used in data type name mapping.
		///                </para>
		/// </summary>
		CharSetMask = 6,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CharSetNotSpec = 0,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CharSetAnsi = 2,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CharSetUnicode = 4,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CharSetAuto = 6,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		PinvokeOLE = 32,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		SupportsLastError = 64,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvMask = 1792,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvWinapi = 256,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvCdecl = 512,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvStdcall = 768,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvThiscall = 1024,

		/// <summary>
		/// <para>[To be supplied.]</para>
		/// </summary>
		CallConvFastcall = 1280,
	} // PInvokeMap

} // System.Runtime.InteropServices
