//
// acc-modifiers2.cs: We use reflection to test that the flags are the correct ones
//

using System;
using System.Reflection;

 [AttributeUsage (AttributeTargets.Class)]
 public class TypeCheckAttribute : Attribute {

	 public TypeCheckAttribute ()
	 {
	 }
 }

 [AttributeUsage (AttributeTargets.Property)]
 public class PropertyCheckAttribute : Attribute {

	 public PropertyCheckAttribute ()
	 {
	 }
 }

 [AttributeUsage (AttributeTargets.Method)]
 public class AccessorCheckAttribute : Attribute {
	 MethodAttributes flags;

	 public AccessorCheckAttribute (MethodAttributes flags)
	 {
		 this.flags = flags;
	 }

	 public MethodAttributes Attributes {
		 get {
			 return flags;
		 }
	 }
 }

 public class Test {

	 public static int Main (string [] args)
	 {
		 Type t = typeof (A);
		 
		 foreach (PropertyInfo pi in t.GetProperties ()) {
			 object [] attrs = pi.GetCustomAttributes (typeof (PropertyCheckAttribute), true);
			 if (attrs == null)
				 return 0;
			 
			 MethodInfo get_accessor, set_accessor;
			 get_accessor = pi.GetGetMethod (true);
			 set_accessor = pi.GetSetMethod (true);
			 
			 if (get_accessor != null)
				 CheckFlags (pi, get_accessor);
			 if (set_accessor != null)
				 CheckFlags (pi, set_accessor);
		 }

		 return 0;
	 }

	 static void CheckFlags (PropertyInfo pi, MethodInfo accessor)
	 {
		 object [] attrs = accessor.GetCustomAttributes (typeof (AccessorCheckAttribute), true);
		 if (attrs == null)
			 return;

		 AccessorCheckAttribute accessor_attr = (AccessorCheckAttribute) attrs [0];
		 MethodAttributes accessor_flags = accessor.Attributes;

		 if ((accessor_flags & accessor_attr.Attributes) == accessor_attr.Attributes)
			 Console.WriteLine ("Test for {0}.{1} PASSED", pi.Name, accessor.Name);
		 else {
			 string message = String.Format ("Test for {0}.{1} INCORRECT: MethodAttributes should be {2}, but are {3}",
					 pi.Name, accessor.Name, accessor_attr.Attributes, accessor_flags);
			 throw new Exception (message);
		 }
	 }

 }

 [TypeCheck]
 public class A {

	 const MethodAttributes flags = MethodAttributes.HideBySig |
		 MethodAttributes.SpecialName;

	 [PropertyCheck]
	 public int Value1 {
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 get {
			 return 0;
		 }
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 set {
		 }
	 }

	 [PropertyCheck]
	 public int Value2 {
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 get {
			 return 0;
		 }
		 [AccessorCheck (flags | MethodAttributes.FamORAssem)]
		 protected internal set {
		 }
	 }

	 [PropertyCheck]
	 public int Value3 {
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 get {
			 return 0;
		 }
		 [AccessorCheck (flags | MethodAttributes.Family)]
		 protected set {
		 }
	 }

	 [PropertyCheck]
	 public int Value4 {
		 [AccessorCheck (flags | MethodAttributes.Assembly)]
		 internal get {
			 return 0;
		 }
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 set {
		 }
	 }

	 [PropertyCheck]
	 public int Value5 {
		 [AccessorCheck (flags | MethodAttributes.Public)]
		 get {
			 return 0;
		 }
		 [AccessorCheck (flags | MethodAttributes.Private)]
		 private set {
		 }
	 }

 }

