// CallingConventions.cs
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
	/// <para>
	///                   Defines the valid calling conventions for an enumeration.
	///                </para>
	/// </summary>
	/// <remarks>
	/// <para>
	///                   The "native calling convention" is
	///                   the set of rules governing the order and layout of arguments
	///                   passed to compiled methods. It also governs how the return value is passed,
	///                   what registers are used for arguments, and whether the called or the calling method removes
	///                   arguments from the stack.
	///                </para>
	/// </remarks>
	[Flags]
	public enum CallingConventions {

		/// <summary>
		/// <para>Specifies the default calling convention as determined by the Common Language Runtime.</para>
		/// </summary>
		Standard = 1,

		/// <summary>
		/// <para>
		///                   Specifies the calling convention for methods with variable arguments.
		///                </para>
		/// </summary>
		VarArgs = 2,

		/// <summary>
		/// <para>
		///                   Specifies that either the Standard or the VarArgs calling convention may be
		///                   used.
		///                </para>
		/// </summary>
		Any = 3,

		/// <summary>
		/// <para>
		///                   Specifies the calling convention for a non-static method or field.
		///                </para>
		/// </summary>
		HasThis = 32,

		/// <summary>
		/// <para>
		///                   Specifies that the <see langword="this" /> pointer is included in the argument
		///                   list of a non-static method. Required only for function pointers.
		///                </para>
		/// </summary>
		ExplicitThis = 64,
	} // CallingConventions

} // System.Reflection
