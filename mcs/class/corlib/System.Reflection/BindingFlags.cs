// BindingFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// <para> Specifies flags
	///       that control the binding and the invocation processes conducted by reflection.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by classes, such as <see cref="T:System.Reflection.Binder" />,
	/// <see cref="T:System.Reflection.Module" />, and <see cref="T:System.Reflection.ConstructorInfo" /> . <see cref="T:System.Reflection.BindingFlags" /> values are passed to 
	///    methods that
	///    invoke, create, get, set, and find
	///    members and
	///    types.</para>
	/// <para>To specify multiple <see cref="T:System.Reflection.BindingFlags" /> values, use the bit-wise 'OR' operator.</para>
	/// <para>
	/// <block subset="none" type="note">When using <see cref="T:System.Reflection.BindingFlags" />,
	/// specify <see cref="F:System.Reflection.BindingFlags.Instance" /> or <see cref="F:System.Reflection.BindingFlags.Static" />, and <see cref="F:System.Reflection.BindingFlags.Public" /> or <see cref="!:System.Reflection.BindingFlags.Private" />
	/// ; otherwise, methods will
	/// not return information as expected.</block>
	/// </para>
	/// </remarks>
	[Flags]
	public enum BindingFlags {

		/// <summary><para>Specifies that a member name will be handled in a case-insensitive
		///       manner when binding.</para></summary>
		IgnoreCase = 0x00000001,

		/// <summary><para>Specifies that only the members declared on a type will be searched when binding. When this
		///       field is specified, inherited members will not be searched.</para></summary>
		DeclaredOnly = 0x00000002,

		/// <summary><para> Specifies that instance members will be included in the binding search.
		///       </para><para><block subset="none" type="note">If this flag is specified, specify either <see cref="F:System.Reflection.BindingFlags.Public" /> or <see cref="F:System.Reflection.BindingFlags.NonPublic" /> as well, or no members will be
		///    returned by methods that use these flags.</block></para></summary>
		Instance = 0x00000004,

		/// <summary><para> Specifies that static members will be included in the binding search.
		///       </para><para><block subset="none" type="note">If this flag is specified, specify either <see cref="F:System.Reflection.BindingFlags.Public" /> or <see cref="F:System.Reflection.BindingFlags.NonPublic" /> as well, or no members will be
		///    returned by methods that use these flags.</block></para></summary>
		Static = 0x00000008,

		/// <summary><para> Specifies that public members will be included in the binding search.
		///       </para><para><block subset="none" type="note">If this flag is specified, specify either <see cref="F:System.Reflection.BindingFlags.Instance" /> or <see cref="F:System.Reflection.BindingFlags.Static" /> as well, or no
		///    members will be returned by methods that use these flags.</block></para></summary>
		Public = 0x00000010,

		/// <summary><para> Specifies that non-public members will be included in the binding search.
		///       </para><para><block subset="none" type="note">If this flag is specified, specify either <see cref="F:System.Reflection.BindingFlags.Instance" /> or <see cref="F:System.Reflection.BindingFlags.Static" /> as well, or no
		///    members will be returned by methods that use these flags.</block></para></summary>
		NonPublic = 0x00000020,

		/// <summary><para>Specifies that a method is to be invoked.</para></summary>
		InvokeMethod = 0x00000100,

		/// <summary><para> Specifies that an instance of
		///       the specified type is to be created.</para><para><block subset="none" type="note">The constructor 
		///       that matches parameters specified in the invocation of the method that uses this
		///       value is called. If <see cref="F:System.Reflection.BindingFlags.Static" /> or <see cref="F:System.Reflection.BindingFlags.Instance" /> and <see cref="F:System.Reflection.BindingFlags.Public" /> or <see cref="!:System.Reflection.BindingFlags.Nonpublic" /> flags are not specified, a method that uses
		///       this value will also use <see cref="F:System.Reflection.BindingFlags.Instance" /> and <see cref="F:System.Reflection.BindingFlags.Public" />
		///       .</block></para></summary>
		CreateInstance = 0x00000200,

		/// <summary><para> Specifies that the value of the specified field will be returned.
		///       </para></summary>
		GetField = 0x00000400,

		/// <summary><para> Specifies that the value of a field is to be set.
		///       </para></summary>
		SetField = 0x00000800,

		/// <summary><para> Specifies that the value of the specified property will be returned.
		///       </para></summary>
		GetProperty = 0x00001000,

		/// <summary><para>Specifies that the value of a property is to
		///       be set.</para></summary>
		SetProperty = 0x00002000,

		/// <summary><para>Specifies that, when binding to a method, types of the
		///       supplied arguments passed to the binder are required to be the same
		///       as the types of
		///       
		///       the corresponding parameters in the method
		///       declaration.</para></summary>
		ExactBinding = 0x00010000,

		/// <summary><para> Specifies that the system will not change the type of parameters when
		///       binding. An exception will be thrown if the caller supplies a
		///       non-null <see cref="T:System.Reflection.Binder" /> object.</para><block subset="none" type="note">This flag applies only to <see langword="MethodInfo.Invoke()" />
		/// .</block></summary>
		SuppressChangeType = 0x00020000,

		/// <summary><para>Specifies that the set of members whose parameter count matches the number of supplied arguments is to be returned by the
		///       method that is passed this flag. <block subset="none" type="note">This
		///       binding flag is used in conjunction with methods with parameters that have
		///       default values and methods with variable arguments. </block></para></summary>
		OptionalParamBinding = 0x00040000,
	} // BindingFlags

} // System.Reflection
