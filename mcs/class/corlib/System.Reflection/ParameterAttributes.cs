// ParameterAttributes.cs
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
	///                Defines the attributes that may be associated with
	///                a parameter. These are defined in CorHdr.h.
	///             </summary>
	/// <remarks>
	/// <para>To get the <see langword="ParameterAttributes" /> value, 
	///                first get the <see langword="Type" />. From the <see langword="Type" />, get the
	///             <see langword="ParameterInfo" /> array. The <see langword="ParameterAttributes" /> 
	///             value is within the
	///             array.</para>
	/// <para>These enumerator values are dependent on optional metadata. Not
	///                all attributes are available from all compilers. See the appropriate compiler
	///                instructions to determine which enumerated values are available.</para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">using System;
	///             using System.Reflection;
	///             
	///             class paramatt
	///             {
	///                public static void mymethod ([in] string str1, [out] string str2, 
	///                   ref string str3)
	///                {
	///                }
	///                public static int Main(string[] args)
	///                {
	///                   Console.WriteLine("\nReflection.ParameterAttributes");
	///              
	///                   //Get the Type and the method.
	///              
	///                   Type Mytype = Type.GetType("paramatt");
	///                   MethodBase Mymethodbase = Mytype.GetMethod("mymethod");
	///              
	///                   //Display the method
	///                  Console.Write("\nMymethodbase = " + Mymethodbase);
	///              
	///                   //Get the ParameterInfo array
	///                   ParameterInfo[] Myarray = Mymethodbase.GetParameters();
	///              
	///                   //Get and display the attributes for the second parameter
	///                   ParameterAttributes Myparamattributes = Myarray[1].Attributes;
	///              
	///                   Console.Write("\nFor the second parameter:\nMyparamattributes = " 
	///                      + (int) Myparamattributes
	///                      + ", which is an "
	///                      + Enum.ToString(Type.GetType("System.Reflection.ParameterAttributes"),
	///                      Myparamattributes));
	///              
	///                   return 0;
	///                }
	///             }
	///             Produces the following output
	///             
	///             Reflection.ParameterAttributes
	///             
	///             Mymethodbase = Void mymethod (System.String, System.String,
	///             System.String ByRef)
	///             
	///             For the second parameter:
	///             Myparamattributes = 2, which is an Out
	///                </code>
	/// </example>
	[Flags]
	public enum ParameterAttributes {

		/// <summary>
		/// <para>Specifies that there is no parameter attribute.</para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>Specifies that the parameter is an input parameter. </para>
		/// </summary>
		In = 1,

		/// <summary>
		/// <para>Specifies that the parameter is an output parameter.</para>
		/// </summary>
		Out = 2,

		/// <summary>
		/// <para>Specifies that the parameter is a locale identifier (lcid).</para>
		/// </summary>
		Lcid = 4,

		/// <summary>
		/// <para>Specifies that the parameter is a return value.</para>
		/// </summary>
		Retval = 8,

		/// <summary>
		/// <para>Specifies that the parameter is optional.</para>
		/// </summary>
		Optional = 16,

		/// <summary>
		/// <para>Specifies that the parameter is reserved.</para>
		/// </summary>
		ReservedMask = 61440,

		/// <summary>
		/// <para>Specifies that the parameter has a default value.</para>
		/// </summary>
		HasDefault = 4096,

		/// <summary>
		/// <para>Specifies that the parameter has field marshaling information.</para>
		/// </summary>
		HasFieldMarshal = 8192,

		/// <summary>
		/// <para>Reserved.</para>
		/// </summary>
		Reserved3 = 16384,

		/// <summary>
		/// <para>Reserved.</para>
		/// </summary>
		Reserved4 = 32768,
	} // ParameterAttributes

} // System.Reflection
