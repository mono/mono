// PInvokeMap.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.InteropServices {


	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public enum PInvokeMap {

		/// <summary>
		/// </summary>
		NoMangle = 1,

		/// <summary>
		/// </summary>
		CharSetMask = 6,

		/// <summary>
		/// </summary>
		CharSetNotSpec = 0,

		/// <summary>
		/// </summary>
		CharSetAnsi = 2,

		/// <summary>
		/// </summary>
		CharSetUnicode = 4,

		/// <summary>
		/// </summary>
		CharSetAuto = 6,

		/// <summary>
		/// </summary>
		PinvokeOLE = 32,

		/// <summary>
		/// </summary>
		SupportsLastError = 64,

		/// <summary>
		/// </summary>
		CallConvMask = 1792,

		/// <summary>
		/// </summary>
		CallConvWinapi = 256,

		/// <summary>
		/// </summary>
		CallConvCdecl = 512,

		/// <summary>
		/// </summary>
		CallConvStdcall = 768,

		/// <summary>
		/// </summary>
		CallConvThiscall = 1024,

		/// <summary>
		/// </summary>
		CallConvFastcall = 1280,
	} // PInvokeMap

} // System.Runtime.InteropServices
