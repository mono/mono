// FieldAttributes.cs
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
	/// <para>Specifies flags that describe the attributes of a field.</para>
	/// </summary>
	/// <remarks>
	/// <para>FieldAttributes uses this value from <see langword="FieldAccessMask" />
	///             to mask off only the parts of the attribute value that is the
	///             accessibility. For example, the following code snippet determines if Attributes has the public bit set:</para>
	/// <para>(Attributes &amp; FieldAttributes.FieldAccessMask) == 
	///                FieldAttributes.Public</para>
	/// <para>To get the FieldAttributes, first get the class Type. From the
	///                Type, get the FieldInfo. From the FieldInfo, get the Attributes.</para>
	/// <para>The enumerated
	///                value is a number representing the bitwise OR of the attributes implemented
	///                on the field.</para>
	/// </remarks>
	/// <example>
	/// <para>In this example, three fields are built and the FieldAttributes value is
	///                   displayed when it is exactly defined. A FieldAttributes may contain more than
	///                   one attribute, such as Public and Literal, as shown in the third field below.</para>
	/// <code lang="C#">//Make three fields
	///             //The first field is private
	///             using System;
	///             using System.Reflection;
	///             
	///             public class Myfielda
	///             {
	///                 private string field = "A private field";
	///                 public string Field{
	///                     get{return field;}
	///                     set{if(field!=value) {field=value + "---";}}
	///                 }
	///             }
	///             //The second field is public
	///             public class Myfieldb
	///             {
	///                 public string field = "B public field";
	///                 public string Field{
	///                     get{return field;}
	///                     set{if(field!=value) {field=value;}}
	///                 }
	///             }
	///             //The third field is public and literal, which is not exactly defined.
	///             public class Myfieldc
	///             {
	///                 public const string field = "C constant field";
	///                 public string Field{
	///                     get{return field;}
	///                 }
	///             }
	///             public class Myfieldattributes
	///             {
	///                 public static int Main()
	///                 {
	///                     Console.WriteLine ("\nReflection.FieldAttributes");
	///                     Myfielda Myfielda = new Myfielda();
	///                     Myfieldb Myfieldb = new Myfieldb();
	///                     Myfieldc Myfieldc = new Myfieldc();
	///             
	///                     //Get the Type and FieldInfo for each of the three fields
	///                     Type MyTypea = Type.GetType("Myfielda");
	///                     FieldInfo Myfieldinfoa = MyTypea.GetField("field",
	///                         BindingFlags.NonPublic);
	///                     Type MyTypeb = Type.GetType("Myfieldb");
	///                     FieldInfo Myfieldinfob = MyTypeb.GetField("field",
	///                         BindingFlags.NonPublic);
	///                     Type MyTypec = Type.GetType("Myfieldc");
	///                     FieldInfo Myfieldinfoc = MyTypec.GetField("field",
	///                         BindingFlags.NonPublic);
	///             
	///                     //For the first field;
	///                     //Get and Display the Name, field, and attributes
	///                     Console.Write ("\n{0} - ", MyTypea.FullName);
	///                     Console.Write ("{0}; ", Myfieldinfoa.GetValue(Myfielda));
	///                     FieldAttributes Myattributesa = Myfieldinfoa.Attributes;
	///             
	///                     //If the FieldAttributes is exactly defined,
	///                     // print it out, otherwise say it is not defined
	///                     if (IsDefined(typeof(FieldAttributes),
	///                         Myattributesa))
	///                         Console.Write ("It has a {0} field attribute.",
	///                             ToString(typeof(FieldAttributes),
	///                             Myattributesa));
	///                     else
	///                         Console.Write ("It is not exactly defined.");
	///             
	///                     //For the second field;
	///                     //Get and Display the Name, field, and attributes
	///                     Console.Write ("\n{0} - ", MyTypeb.FullName);
	///                     Console.Write ("{0}; ", Myfieldinfob.GetValue(Myfieldb));
	///                     FieldAttributes Myattributesb = Myfieldinfob.Attributes;
	///             
	///                     //If the FieldAttributes is exactly defined,
	///                     // print it out, otherwise say it is not defined
	///                     if (IsDefined(typeof(FieldAttributes),
	///                         Myattributesb))
	///                         Console.Write ("It has a {0} field attribute.",
	///                             ToString(typeof(FieldAttributes),
	///                                 Myattributesb));
	///                     else
	///                         Console.Write ("It is not exactly defined.");
	///             
	///                     //For the third field;
	///                     //Get and Display the Name, field, and attributes
	///                     Console.Write ("\n{0} - ", MyTypec.FullName);
	///                     Console.Write ("{0}; ", Myfieldinfoc.GetValue(Myfieldc));
	///                     FieldAttributes Myattributesc = Myfieldinfoc.Attributes;
	///             
	///                     //If the FieldAttributes is exactly defined,
	///                     // print it out, otherwise say it is not defined
	///                     if (IsDefined(typeof(FieldAttributes),
	///                         Myattributesc))
	///                         Console.Write ("It has a {0} field attribute.",
	///                             ToString(typeof(FieldAttributes),
	///                                 Myattributesc));
	///                     else
	///                         Console.Write ("It is not exactly defined.");
	///             
	///                     return 0;
	///                 }
	///             }
	///                </code>
	/// <para>This code produces the following output:</para>
	/// <para>Reflection.FieldAttributes</para>
	/// <para>Myfielda - A private field; it has a Private field attribute.</para>
	/// <para>Myfieldb - B public field; it has a Public field attribute.</para>
	/// <para>Myfieldc - C constant field; it is not exactly defined.</para>
	/// </example>
	public enum FieldAttributes {

		/// <summary>
		/// <para>
		///                   Specifies the access level of a given field.
		///                </para>
		/// </summary>
		FieldAccessMask = 7,

		/// <summary>
		/// <para>
		///                   Specifies that the field cannot be referenced.
		///                </para>
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible only by the parent type.
		///                </para>
		/// </summary>
		Private = 1,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible only by sub-types in this
		///                   assembly.
		///                </para>
		/// </summary>
		FamANDAssem = 2,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible throughout the assembly.
		///                </para>
		/// </summary>
		Assembly = 3,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible only by type and sub-types.
		///                </para>
		/// </summary>
		Family = 4,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible by sub-types anywhere, as well as
		///                   throughout this assembly.
		///                </para>
		/// </summary>
		FamORAssem = 5,

		/// <summary>
		/// <para>
		///                   Specifies that the field is accessible by any member for whom this scope
		///                   is visible.
		///                </para>
		/// </summary>
		Public = 6,

		/// <summary>
		/// <para>
		///                   Specifies that the field represents the defined type, or else it is
		///                   per-instance.
		///                </para>
		/// </summary>
		Static = 16,

		/// <summary>
		/// <para>
		///                   Specifies that the field is initialized only, and cannot be written after
		///                   initialization.
		///                </para>
		/// </summary>
		InitOnly = 32,

		/// <summary>
		/// <para>
		///                   Specifies that the field's value is a compile-time (static or early bound)
		///                   constant. No set accessor.
		///                </para>
		/// </summary>
		Literal = 64,

		/// <summary>
		/// <para>
		///                   Specifies that the field does not have to be serialized when the type is
		///                   remoted.
		///                </para>
		/// </summary>
		NotSerialized = 128,

		/// <summary>
		/// <para>
		///                   Specifies a special method, with the name describing how the method is
		///                   special.
		///                </para>
		/// </summary>
		SpecialName = 512,

		/// <summary>
		/// <para>
		///                   Specifies that the field's implementation is forwarded through PInvoke.
		///                </para>
		/// </summary>
		PinvokeImpl = 8192,
		ReservedMask = 54528,

		/// <summary>
		/// <para>Specifies that the Common Language Runtime (metadata internal APIs) should check the
		///                   name encoding.</para>
		/// </summary>
		RTSpecialName = 1024,

		/// <summary>
		/// <para>
		///                   Specifies that the field has marshalling information.
		///                </para>
		/// </summary>
		HasFieldMarshal = 4096,

		/// <summary>
		/// <para>
		///                   Specifies that the field has a security associate.
		///                </para>
		/// </summary>
		HasSecurity = 16384,

		/// <summary>
		/// <para>
		///                   Specifies that the field has a default value.
		///                </para>
		/// </summary>
		HasDefault = 32768,

		/// <summary>
		/// <para>
		///                   Specifies that the field has a Relative Virtual Address (RVA). The RVA is the
		///                   location of the method body in the current image, as an address relative to the
		///                   start of the image file in which it is located.
		///                </para>
		/// </summary>
		HasFieldRVA = 256,
	} // FieldAttributes

} // System.Reflection
