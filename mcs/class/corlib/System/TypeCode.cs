// TypeCode.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	/// <para>Specifies the type code of an object.</para>
	/// </summary>
	/// <remarks>
	/// <para>To obtain the type code for a given object, use the 
	///                   object's GetTypeCode method. The TypeCode.Empty field represents a
	///                   null object reference and the TypeCode.Object field represents an object that
	///                   doesn't implement the IValue interface. TheTypeCode.DBNull field represents a
	///                   null database, which is distinct from a null reference. </para>
	/// <para>There are no type codes for "Missing", "Error", "IDispatch", and "IUnknown". 
	///                   These types of values are instead represented as classes. When the type code of
	///                   an object is TypeCode.Object, a call to <see cref="M:System.Type.IsInstanceOfType(System.Object)" /> can be used to determine if
	///                   the object is an instance of the Type.</para>
	/// <para>Note that when an object has a given type code, there is no guarantee that 
	///                   the object is an instance of the corresponding value type. For example, an
	///                   object with the type code TypeCode.Int32 might actually be an instance of a
	///                   nullable 32-bit integer type.</para>
	/// </remarks>
	public enum TypeCode {

		/// <summary>
		///                A null reference.
		///             </summary>
		Empty = 0,

		/// <summary>
		/// <para> An instance that isn't a value.</para>
		/// </summary>
		Object = 1,

		/// <summary>
		/// <para>A null database.</para>
		/// </summary>
		DBNull = 2,

		/// <summary>
		/// <para>A simple type representing boolean values of true
		///                   or false.</para>
		/// </summary>
		Boolean = 3,

		/// <summary>
		/// <para>An integral type representing unsigned 16-bit integers 
		///                   with values between 0 and 65535. The set of possible values for the <see cref="F:System.TypeCode.Char" />
		///                   type corresponds to the Unicode character set.</para>
		/// </summary>
		Char = 4,

		/// <summary>
		///                An integral type representing signed 8-bit
		///                integers with values between -128 and 127.
		///             </summary>
		SByte = 5,

		/// <summary>
		///                An integral type representing unsigned
		///                8-bit integers with values between 0 and 255.
		///             </summary>
		Byte = 6,

		/// <summary>
		///                An integral type representing signed 16-bit
		///                integers with values between -32768 and 32767.
		///             </summary>
		Int16 = 7,

		/// <summary>
		///                An integral type representing unsigned
		///                16-bit integers with values between 0 and 65535.
		///             </summary>
		UInt16 = 8,

		/// <summary>
		///                An integral type representing signed 32-bit
		///                integers with values between -2147483648 and 2147483647.
		///             </summary>
		Int32 = 9,

		/// <summary>
		///                An integral type representing unsigned
		///                32-bit integers with values between 0 and 4294967295.
		///             </summary>
		UInt32 = 10,

		/// <summary>
		/// <para>An integral type representing signed 64-bit integers 
		///                   with values
		///                   between -9223372036854775808 and 9223372036854775807.</para>
		/// </summary>
		Int64 = 11,

		/// <summary>
		///                An integral type representing unsigned
		///                64-bit integers with values between 0 and 18446744073709551615.
		///             </summary>
		UInt64 = 12,

		/// <summary>
		/// <para>A floating point type representing values ranging from 
		///                   approximately 1.5 x 10<superscript term="-45" /> to 3.4 x 10<superscript term="38" />
		///                   with a precision of 7
		///                   digits.</para>
		/// </summary>
		Single = 13,

		/// <summary>
		/// <para>A floating point type representing values ranging from 
		///                   approximately 5.0 x 10<superscript term="-324" /> to 1.7 x 10<superscript term="308" />
		///                   with a precision of
		///                   15-16 digits.</para>
		/// </summary>
		Double = 14,

		/// <summary>
		/// <para>A simple type representing values ranging from 1.0 x 
		///                   10<superscript term="-28" /> to approximately 7.9 x 10<superscript term="28" />
		///                   with 28-29 significant digits.</para>
		/// </summary>
		Decimal = 15,

		/// <summary>
		/// <para>A type representing a date and time value.</para>
		/// </summary>
		DateTime = 16,

		/// <summary>
		///                A sealed class type representing Unicode
		///                character strings.
		///             </summary>
		String = 18,
	} // TypeCode

} // System
