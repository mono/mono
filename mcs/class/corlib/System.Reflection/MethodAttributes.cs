// MethodAttributes.cs
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
	///                   Specifies flags for method attributes. These flags are defined in corhdr.h.</para>
	/// </summary>
	/// <remarks>
	/// <para> 
	///                   This set of attributes is a combination of Enumeration
	///                   and bit flags.</para>
	/// <para> The enumerated
	///                   value is a number representing the bitwise OR of the attributes
	///                   implemented on the method.</para>
	/// <para> 
	///                   Accessibility information masks:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                      MemberAccessMask = 0x0007</term>
	/// </item>
	/// <item>
	/// <term>
	///                      PrivateScope = 0x0000</term>
	/// </item>
	/// <item>
	/// <term>
	///                      Private = 0x0001</term>
	/// </item>
	/// <item>
	/// <term>
	///                      FamANDAssem = 0x0002</term>
	/// </item>
	/// <item>
	/// <term>
	///                      Assembly = 0x0003</term>
	/// </item>
	/// <item>
	/// <term>
	///                      Family = 0x0004</term>
	/// </item>
	/// <item>
	/// <term>
	///                      FamORAssem = 0x0005</term>
	/// </item>
	/// <item>
	/// <term>
	///                      Public = 0x0006</term>
	/// </item>
	/// </list>
	/// <para> Method contract attributes masks:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   Static = 0x0010</term>
	/// </item>
	/// <item>
	/// <term>
	///                   Final = 0x0020</term>
	/// </item>
	/// <item>
	/// <term>
	///                   Virtual = 0x0040</term>
	/// </item>
	/// <item>
	/// <term>
	///                   HideBySig = 0x0080</term>
	/// </item>
	/// </list>
	/// <para> Vtable attributes
	///                masks:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   VtableLayoutMask = 0x0100</term>
	/// </item>
	/// <item>
	/// <term>
	///                   ReuseSlot = 0x0000</term>
	/// </item>
	/// <item>
	/// <term>
	///                   NewSlot = 0x0100</term>
	/// </item>
	/// </list>
	/// <para>Method implementation attributes:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   Abstract = 0x0400</term>
	/// </item>
	/// <item>
	/// <term>
	///                   SpecialName = 0x0800</term>
	/// </item>
	/// </list>
	/// <para>Interop attributes masks:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   PinvokeImpl = 0x2000</term>
	/// </item>
	/// <item>
	/// <term>
	///                   UnmanagedExport = 0x0008</term>
	/// </item>
	/// <item>
	/// <term>
	/// <para>RTSpecialName = 0x1000</para>
	/// </term>
	/// </item>
	/// </list>
	/// <para>Reserved flags for runtime use only:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                   ReservedMask = 0xd000</term>
	/// </item>
	/// <item>
	/// <term>
	///                   HasSecurity = 0x4000</term>
	/// </item>
	/// <item>
	/// <term>
	///                   RequireSecObject = 0x8000</term>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="C#">using System;
	///             using System.Reflection;
	///             
	///             class AttributesSample
	///             {
	///                public void Mymethod ( [in] int int1m, out string str2m, ref string str3m)
	///                {
	///                   str2m = "in Mymethod";
	///                }
	///             
	///                public static int Main(string[] args)
	///                {      
	///                   Console.WriteLine ("Reflection.MethodBase.Attributes Sample");
	///                   
	///                   // Get our type
	///                   Type MyType = Type.GetType("AttributesSample");
	///             
	///                   // Get the method Mymethod on our type
	///                   MethodBase Mymethodbase = MyType.GetMethod("Mymethod");
	///             
	///                   // Print out the method
	///                   Console.WriteLine("Mymethodbase = " + Mymethodbase);
	///             
	///                   // Get the MethodAttribute enumerated value
	///                   MethodAttributes Myattributes = Mymethodbase.Attributes;
	///             
	///                   // print out the flags set
	///                   PrintAttributes( typeof( System.Reflection.MethodAttributes ), (int) Myattributes );
	///                   return 0;
	///                }
	///             
	///             
	///                public static void PrintAttributes( Type attribType, int iAttribValue )
	///                {
	///                   if ( ! attribType.IsEnum ) { Console.WriteLine( "This type is not an enum" ); return; }
	///             
	///                   FieldInfo[] fields = attribType.GetFields(BindingFlags.Public | BindingFlags.Static);
	///                   for ( int i = 0; i &lt; fields.Length; i++ )
	///                   {
	///                      int fieldvalue = (Int32)fields[i].GetValue(null);
	///                      if ( (fieldvalue &amp; iAttribValue) == fieldvalue )
	///                      {
	///                         Console.WriteLine( "\t" + fields[i].Name );
	///                      }
	///                   }
	///                }
	///             }
	///             
	///             This code produces the following output:
	///             
	///             Reflection.MethodBase.Attributes Sample
	///             Mymethodbase = Void Mymethod (Int32, System.String ByRef, System.String ByRef)
	///                     PrivateScope
	///                     FamANDAssem
	///                     Family
	///                     Public 
	///                     HideBySig 
	///                     ReuseSlot
	///                </code>
	/// </example>
	public enum MethodAttributes {

		/// <summary>
		/// <para>
		///                   Retrieves accessibility information.
		///                </para>
		/// </summary>
		MemberAccessMask = 7,

		/// <summary>
		/// <para>
		///                   Indicates that the member cannot be referenced.
		///                </para>
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accessible only to the current
		///                   class and the parent class.
		///                </para>
		/// </summary>
		Private = 1,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accessible only to members of this class and
		///                   its subclasses.
		///                </para>
		/// </summary>
		FamANDAssem = 2,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accessible to any class of this
		///                   assembly.
		///                </para>
		/// </summary>
		Assembly = 3,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accessible only to members of
		///                   this class and its subclasses.
		///                </para>
		/// </summary>
		Family = 4,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accesssible to subclasses
		///                   anywhere, as well as to any class in the assembly.
		///                </para>
		/// </summary>
		FamORAssem = 5,

		/// <summary>
		/// <para>
		///                   Indicates that the method is accessible to any object for
		///                   which this object is in scope.
		///                </para>
		/// </summary>
		Public = 6,

		/// <summary>
		/// <para>
		///                   Indicates that the method is defined on the type;
		///                   otherwise, it is defined per instance.
		///                </para>
		/// </summary>
		Static = 16,

		/// <summary>
		/// <para>
		///                   Indicates that the method cannot be overriden.
		///                </para>
		/// </summary>
		Final = 32,

		/// <summary>
		/// <para>
		///                   Indicates that the method is virtual.
		///                </para>
		/// </summary>
		Virtual = 64,

		/// <summary>
		/// <para>
		///                   Indicates that the method hides by name and signature;
		///                   otherwise, by name only.
		///                </para>
		/// </summary>
		HideBySig = 128,

		/// <summary>
		/// <para>
		///                   Retrieves vtable
		///                   attributes.
		///                </para>
		/// </summary>
		VtableLayoutMask = 256,

		/// <summary>
		/// <para>
		///                   Indicates that the method will reuse an existing slot in
		///                   the vtable. This is
		///                   the default behavior.
		///                </para>
		/// </summary>
		ReuseSlot = 0,

		/// <summary>
		/// <para>
		///                   Indicates that the method always gets a new slot in the
		///                   vtable.
		///                </para>
		/// </summary>
		NewSlot = 256,

		/// <summary>
		/// <para>
		///                   Indicates that the class does not provide an implementation of
		///                   this method.
		///                </para>
		/// </summary>
		Abstract = 1024,

		/// <summary>
		/// <para>
		///                   Indicates that the method is special. The name describes how this method is special.
		///                </para>
		/// </summary>
		SpecialName = 2048,

		/// <summary>
		/// <para>
		///                   Indicates that the method implementation is forwarded
		///                   through PInvoke (Platform Invocation
		///                   Services).
		///                </para>
		/// </summary>
		PinvokeImpl = 8192,

		/// <summary>
		/// <para>
		///                   Indicates that the managed method is exported by thunk to
		///                   unmanaged code.
		///                </para>
		/// </summary>
		UnmanagedExport = 8,

		/// <summary>
		/// <para>Indicates that the Common Language Runtime checks the name encoding.</para>
		/// </summary>
		RTSpecialName = 4096,

		/// <summary>
		/// <para>
		///                   Indicates a reserved flag for runtime use only.
		///                </para>
		/// </summary>
		ReservedMask = 53248,

		/// <summary>
		/// <para>
		///                   Indicates that the
		///                   method has security associated with it. Reserved flag for runtime use only.
		///                </para>
		/// </summary>
		HasSecurity = 16384,

		/// <summary>
		/// <para>
		///                   Indicates that the
		///                   method calls another method containing security code. Reserved flag for runtime
		///                   use only.
		///                </para>
		/// </summary>
		RequireSecObject = 32768,
	} // MethodAttributes

} // System.Reflection
