// AttributeTargets.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	/// <para> Enumerates the application elements to which it is valid to attach
	///       an attribute.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="T:System.AttributeTargets" /> is used as a parameter for <see cref="T:System.AttributeUsageAttribute" /> to enable an attribute to be
	///    associated with one or more kinds of application elements.</para>
	/// </remarks>
	/// <example>
	/// <para>The following code sample demonstrates how <see cref="T:System.AttributeTargets" /> can be
	///    used with <see cref="T:System.AttributeUsageAttribute" /> so that a user-defined attribute class, <paramref name="Author" />,
	///    may be applied to structures and classes. The <paramref name="Author" /> attribute is then
	///    applied to a class.</para>
	/// <code lang="C#">[AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Struct)]
	/// public class Author : Attribute {
	///    public Author(string Name) { this.name = Name; }
	///    string name;
	/// }
	/// [Author("John Q Public")]
	/// class JohnsClass {
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:System.AttributeUsageAttribute" />
	[Flags]
	public enum AttributeTargets {

		/// <summary><para>       Attribute can be applied to an assembly.
		///       </para></summary>
		Assembly = 0x00000001,

		/// <summary><para>       Attribute can be applied to a module.
		///       </para></summary>
		Module = 0x00000002,

		/// <summary><para>       Attribute can be applied to a class.
		///       </para></summary>
		Class = 0x00000004,

		/// <summary><para>       Attribute can be applied to a value type.
		///       </para></summary>
		Struct = 0x00000008,

		/// <summary><para>       Attribute can be applied to an enumeration.
		///       </para></summary>
		Enum = 0x00000010,

		/// <summary><para>       Attribute can be applied to a constructor.
		///       </para></summary>
		Constructor = 0x00000020,

		/// <summary><para>       Attribute can be applied to a method.
		///       </para></summary>
		Method = 0x00000040,

		/// <summary><para>       Attribute can be applied to a property.
		///       </para></summary>
		Property = 0x00000080,

		/// <summary><para>       Attribute can be applied to a field.
		///       </para></summary>
		Field = 0x00000100,

		/// <summary><para>       Attribute can be applied to an event.
		///       </para></summary>
		Event = 0x00000200,

		/// <summary><para>       Attribute can be applied to an interface.
		///       </para></summary>
		Interface = 0x00000400,

		/// <summary><para>       Attribute can be applied to a parameter.
		///       </para></summary>
		Parameter = 0x00000800,

		/// <summary><para>       Attribute can be applied to a delegate.
		///       </para></summary>
		Delegate = 0x00001000,

		/// <summary><para>Attribute can be applied to a Return value.</para></summary>
		ReturnValue = 0x00002000,

		/// <summary><para> Attribute can be applied to any element.
		///       </para></summary>
		All = Assembly | Class | Struct | Enum | Constructor | Method | Property | Field | Event | Interface | Parameter | Delegate | ReturnValue,
	} // AttributeTargets

} // System
