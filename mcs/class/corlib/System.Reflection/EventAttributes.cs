// EventAttributes.cs
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
	///                   Specifies the attributes
	///                   of an event.
	///                </para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see langword="EventAttributes" />
	///                values may be combined using the bitwise OR operation to get the appropriate
	///                combination.
	///             </para>
	/// <para>
	///                These enums are defined in corhdr.h and are a combination of bits and
	///                enumerators.
	///             </para>
	/// </remarks>
	public enum EventAttributes {

		/// <summary>
		/// <para>
		///                   Specifies that the event has
		///                   no attributes.
		///                </para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>
		///                   Specifies that the event is special in a way described by the name.
		///                </para>
		/// </summary>
		SpecialName = 512,

		/// <summary>
		/// <para>Specifies a reserved flag for Common Language Runtime use only.</para>
		/// </summary>
		ReservedMask = 1024,

		/// <summary>
		/// <para>Specifies that the Common Language Runtime should check name encoding.</para>
		/// </summary>
		RTSpecialName = 1024,
	} // EventAttributes

} // System.Reflection
