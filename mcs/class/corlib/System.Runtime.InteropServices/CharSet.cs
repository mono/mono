// CharSet.cs
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
	/// <para>Specifies which character set marshaled strings are required to use.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by the <see cref="T:System.Runtime.InteropServices.DllImportAttribute" /> to indicate the
	///    required modifications to the <see cref="T:System.String" /> arguments of an imported function.</para>
	/// <para>
	/// <block subset="none" type="note">See the <see cref="T:System.Runtime.InteropServices.DllImportAttribute" /> class overview for an example of
	/// the use of the <see cref="T:System.Runtime.InteropServices.CharSet" /> enumeration.</block>
	/// </para>
	/// </remarks>
	public enum CharSet {

		/// <summary><para> Specifies that strings will be marshaled in the ANSI character
		///       set.</para></summary>
		Ansi = 2,

		/// <summary><para> Specifies that strings will be marshaled in the Unicode character set.</para></summary>
		Unicode = 3,

		/// <summary><para> Specifies that strings will be automatically marshaled in the character set appropriate
		///       for the target system (either Unicode or ANSI).</para></summary>
		Auto = 4,
	} // CharSet

} // System.Runtime.InteropServices
