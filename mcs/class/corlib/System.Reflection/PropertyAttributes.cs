// PropertyAttributes.cs
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
	/// <para>Defines the attributes that may be associated
	///                   with a property. These
	///                   attribute values are defined in corhdr.h.</para>
	/// </summary>
	/// <remarks>
	/// <para>In the example shown below, three properties are built 
	///                   and the PropertyAttributes enumerated value is displayed. Note that the for the
	///                   read only property has no setter and thus cannot be changed by .Caption =
	///                   statement.</para>
	/// <para>To get the PropertyAttributes, first get the class Type. From the
	///                   Type, get the PropertyInfo. From the PropertyInfo, get the Attributes.</para>
	/// <para> The enumerated
	///                   value is a number representing the bitwise OR of the attributes implemented
	///                   on the method.</para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">using System;
	///             using System.Reflection;
	///             
	///             //Make three properties, one read-write, one default
	///             // and one read only. 
	///             public class Aproperty  
	///             // A read-write property
	///             {
	///                private string caption = "A Default caption";
	///                public string Caption{
	///                   get{return caption;}
	///                   set{
	///                      if (caption != value){caption = value;}
	///                   }
	///                }
	///             }
	///             public class Bproperty  
	///             // A default property
	///             {
	///                private string caption  = "B Default caption";
	///                public string this [int index]{
	///                   get {return "1";}
	///                }
	///                public string Caption{
	///              
	///                   get{return caption;}
	///                   set{
	///                      if (caption != value){caption = value;}
	///                   }
	///                }
	///             }
	///             public class Cproperty  
	///             // A read only property
	///             {
	///                private string caption = "C Default caption";
	///                public string Caption{
	///                   get{return caption;}
	///                   //No set- this is read only
	///                }
	///             }
	///              
	///             class propertyattributesenum
	///             {
	///                public static int Main(string[] args)
	///                {
	///                   Console.WriteLine("\nReflection.PropertyAttributes");
	///              
	///                   //Prove that a property exist and change its value
	///                   Aproperty Mypropertya = new Aproperty();
	///                   Bproperty Mypropertyb = new Bproperty();
	///                   Cproperty Mypropertyc = new Cproperty();
	///              
	///                  
	///             Console.Write("\n1. Mypropertya.Caption = " + Mypropertya.Caption );
	///                  
	///             Console.Write("\n1. Mypropertyb.Caption = " + Mypropertyb.Caption );
	///                  
	///             Console.Write("\n1. Mypropertyc.Caption = " + Mypropertyc.Caption );
	///              
	///                   //Can change only Mypropertya as Mypropertyb is read only
	///                   Mypropertya.Caption = "A- This is changed.";
	///                   Mypropertyb.Caption = "B- This is changed.";
	///                   //Note that Mypropertyc is not changed- it is read only
	///              
	///                   Console.Write("\n\n2. Mypropertya.Caption = " + Mypropertya.Caption );
	///              
	///                   Console.Write("\n2.Mypropertyb.Caption = " + Mypropertyb.Caption );
	///             
	///                   Console.Write("\n2. Mypropertyc.Caption = " + Mypropertyc.Caption );
	///              
	///                   //Get the PropertyAttributes Enumeration of the property.
	///                   //Get the type
	///                   Type MyTypea = Type.GetType("Aproperty");
	///                   Type MyTypeb = Type.GetType("Bproperty");
	///                   Type MyTypec = Type.GetType("Cproperty");
	///              
	///                   //Get the property attributes
	///                   PropertyInfo Mypropertyinfoa = MyTypea.GetProperty("Caption");
	///                   PropertyAttributes Myattributesa = Mypropertyinfoa.Attributes;
	///                   PropertyInfo Mypropertyinfob = MyTypeb.GetProperty("Item");
	///                   PropertyAttributes Myattributesb = Mypropertyinfob.Attributes;
	///                   PropertyInfo Mypropertyinfoc = MyTypec.GetProperty("Caption");
	///                   PropertyAttributes Myattributesc = Mypropertyinfoc.Attributes;
	///              
	///                   //Display the property attributes value
	///                  
	///                   Console.Write("\n\na- " + ToString(Type.GetType("System.Reflection.PropertyAttributes"), Myattributesa ));
	///              
	///                   Console.Write("\nb-" + ToString(Type.GetType("System.Reflection.PropertyAttributes"), Myattributesb ));
	///                  
	///                   Console.Write("\nc- " + ToString(Type.GetType("System.Reflection.PropertyAttributes"), Myattributesc ));
	///                   return 0;
	///                }
	///             }
	///             
	///             Produces the following output
	///             
	///             Reflection.PropertyAttributes
	///              
	///             1. Mypropertya.Caption = A Default caption
	///             1. Mypropertyb.Caption = B Default caption
	///             1. Mypropertyc.Caption = C Default caption
	///              
	///             2. Mypropertya.Caption = A- This is changed.
	///             2. Mypropertyb.Caption = B- This is changed.
	///             2. Mypropertyc.Caption = C Default caption
	///              
	///             a- None
	///             b- DefaultProperty
	///             c- None
	///                </code>
	/// </example>
	[Flags]
	public enum PropertyAttributes {

		/// <summary>
		/// <para>Specifies that no attributes are associated with a property.</para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>Specifies that the property is special, with the name describing how the
		///                   property is special.</para>
		/// </summary>
		SpecialName = 512,

		/// <summary>
		/// <para>Specifies a flag reserved for runtime use only.</para>
		/// </summary>
		ReservedMask = 62464,

		/// <summary>
		/// <para>Specifies that the metadata internal APIs check the name encoding.</para>
		/// </summary>
		RTSpecialName = 1024,

		/// <summary>
		/// <para>Specifies that the property has a default value.</para>
		/// </summary>
		HasDefault = 4096,
		Reserved2 = 8192,
		Reserved3 = 16384,
		Reserved4 = 32768,
	} // PropertyAttributes

} // System.Reflection
