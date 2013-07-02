
// Make sure that the field and accessor methods of an automatic property have the CompilerGenerated attribute
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

public class Test
{
	public string Foo { get; set; }
	
	public static int Main ()
	{
		FieldInfo [] fields = typeof (Test).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		if (!(fields.Length > 0))
			return 1;
		object [] field_atts = fields[0].GetCustomAttributes (false);
		if (!(field_atts.Length > 0))
			return 2;
		if (field_atts[0].GetType() != typeof (CompilerGeneratedAttribute))
			return 3;
			
		if (fields [0].Name != "<Foo>k__BackingField")
			return 10;
		
		PropertyInfo property = typeof (Test).GetProperty ("Foo");
		MethodInfo get = property.GetGetMethod (false);
		object [] get_atts = get.GetCustomAttributes (false);
		if (!(get_atts.Length > 0))
			return 4;
		if (get_atts[0].GetType() != typeof (CompilerGeneratedAttribute))
			return 5;
			
		MethodInfo set = property.GetSetMethod (false);
		object [] set_atts = set.GetCustomAttributes (false);
		if (!(set_atts.Length > 0))
			return 6;
		if (set_atts[0].GetType() != typeof (CompilerGeneratedAttribute))
			return 7;

		return 0;
	}
}
