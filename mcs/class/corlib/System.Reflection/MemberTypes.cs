// MemberTypes.cs
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
	/// <para>Marks each type of member that is defined as a 
	///                   subclass of <see langword="MemberInfo" /> .</para>
	/// </summary>
	/// <remarks>
	/// <para>These enum values are returned by <see cref="P:System.Type.MemberType" /> and are useful in 
	///             <see langword="switch" /> statements. <see langword="MemberTypes" /> 
	///             matches the CorTypeAttr defined in CorHdr.h.</para>
	/// <para>To obtain the <see langword="MemberTypes" /> value for a
	///             method:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   First get a <see cref="T:System.Type" />.</term>
	/// </item>
	/// <item>
	/// <term>
	///                   From the <see langword="Type" />, get the
	///                <see langword="MemberInfo" /> array.</term>
	/// </item>
	/// <item>
	/// <term>
	/// <para>From the <see langword="MemberInfo" /> array, get the 
	///                <see langword="MemberType" /> 
	///                .</para>
	/// </term>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="C#">using System;
	///             using System.Reflection;
	///             
	///             class membertypesenum
	///             
	///             {
	///             
	///             public static int Main(string[] args)
	///             
	///             {
	///             
	///             Console.WriteLine ("\nReflection.MemberTypes");
	///             
	///             MemberTypes Mymembertypes;
	///             
	///             //Get the type
	///             
	///             Type Mytype = Type.GetType
	///             
	///             ("System.Reflection.ReflectionTypeLoadException");
	///             
	///             //Get the MemberInfo array
	///             
	///             MemberInfo[] Mymembersinfoarray = Mytype.GetMembers();
	///             
	///             //Get and display the name and the MemberType for each member
	///             
	///             foreach (MemberInfo Mymemberinfo in Mymembersinfoarray)
	///             
	///             {
	///             
	///             Console.Write ("\n" + Mymemberinfo.Name);
	///             
	///             Mymembertypes = Mymemberinfo.MemberType;
	///             
	///             Console.Write (" is a "
	///             
	///             + ToString(Type.GetType(
	///             
	///             "System.Reflection.MemberTypes"),Mymembertypes));
	///             
	///             }
	///             
	///             return 0;
	///             
	///             }
	///             
	///             }
	///                </code>
	/// <para>This code produces the following output:</para>
	/// <para>Reflection.MemberTypes</para>
	/// <para>GetType is a Method</para>
	/// <para>ToString is a Method</para>
	/// <para>.ctor is a Constructor</para>
	/// <para>Types is a Property</para>
	/// <para>LoaderExceptions is a Property</para>
	/// </example>
	public enum MemberTypes {

		/// <summary>
		/// <para> Specifies that the member is a constructor,
		///                   representing a <see cref="T:System.Reflection.ConstructorInfo" /> member. Hex value of 0x01.</para>
		/// </summary>
		Constructor = 1,

		/// <summary>
		/// <para>Specifies that the member is an event, representing an <see cref="T:System.Reflection.EventInfo" />
		///             member. Hex value of 0x02.</para>
		/// </summary>
		Event = 2,

		/// <summary>
		/// <para>Specifies that the member is a field, representing a <see cref="T:System.Reflection.FieldInfo" />
		///             member. Hex value of 0x04.</para>
		/// </summary>
		Field = 4,

		/// <summary>
		/// <para>Specifies that the member is a method, representing a <see cref="T:System.Reflection.MethodInfo" />
		///             member. Hex value of 0x08.</para>
		/// </summary>
		Method = 8,

		/// <summary>
		/// <para>Specifies that the member is a property, representing a <see cref="T:System.Reflection.PropertyInfo" />
		///             member. Hex value of 0x10.</para>
		/// </summary>
		Property = 16,

		/// <summary>
		/// <para>Specifies that the member is a type, representing a <see cref="F:System.Reflection.MemberTypes.TypeInfo" />
		///             member. Hex value of 0x20.</para>
		/// </summary>
		TypeInfo = 32,

		/// <summary>
		/// <para>Specifies that the member is a custom member type. Hex value of 0x40.</para>
		/// </summary>
		Custom = 64,

		/// <summary>
		/// <para>Specifies that the member is a nested type, extending <see cref="T:System.Reflection.MemberInfo" />.</para>
		/// </summary>
		NestedType = 128,

		/// <summary>
		///                Specifies all member types.
		///             </summary>
		All = 191,
	} // MemberTypes

} // System.Reflection
