//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mono.CSharp {

	public class Attribute {
		
		public readonly string    Name;
		public readonly ArrayList Arguments;

		Location Location;

		public Type Type;
		
		//
		// The following are only meaningful when the attribute
		// being emitted is one of the builtin ones
		//
		public AttributeTargets Targets;
		public bool AllowMultiple;
		public bool Inherited;

		public bool UsageAttr = false;
		
		public Attribute (string name, ArrayList args, Location loc)
		{
			Name = name;
			Arguments = args;
			Location = loc;
		}

		void error617 (string name)
		{
			Report.Error (617, Location, "'" + name + "' is not a valid named attribute " +
				      "argument. Named attribute arguments must be fields which are not " +
				      "readonly, static or const, or properties with a set accessor which "+
				      "are not static.");
		}

		void error182 ()
		{
			Report.Error (182, Location,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
		}

		public CustomAttributeBuilder Resolve (EmitContext ec)
		{
			string name = Name;

			UsageAttr = false;

			if (Name.IndexOf ("Attribute") == -1)
				name = Name + "Attribute";
			else if (Name.LastIndexOf ("Attribute") == 0)
				name = Name + "Attribute";
			
			Type = ec.TypeContainer.LookupType (name, false);

			if (Type == null) {
				Report.Error (246, Location, "Could not find attribute '" + Name + "' (are you" +
					      " missing a using directive or an assembly reference ?)");
				return null;
			}

			if (Type == TypeManager.attribute_usage_type)
				UsageAttr = true;
			
			// Now we extract the positional and named arguments
			
			ArrayList pos_args = new ArrayList ();
			ArrayList named_args = new ArrayList ();
			
			if (Arguments != null) {
				pos_args = (ArrayList) Arguments [0];
				if (Arguments.Count > 1)
					named_args = (ArrayList) Arguments [1];
			}
				
			object [] pos_values = new object [pos_args.Count];

			//
			// First process positional arguments 
			//
			
			int i;
			for (i = 0; i < pos_args.Count; i++) {

				Argument a = (Argument) pos_args [i];

				if (!a.Resolve (ec, Location))
					return null;

				Expression e = Expression.Reduce (ec, a.Expr);

				if (e is Literal) {
					pos_values [i] = ((Literal) e).GetValue ();

					if (UsageAttr)
						this.Targets = (AttributeTargets) pos_values [0];
				} else { 
					error182 ();
					return null;
				}
			}

			//
			// Now process named arguments
			//

			ArrayList field_infos = new ArrayList ();
			ArrayList prop_infos  = new ArrayList ();
			ArrayList field_values = new ArrayList ();
			ArrayList prop_values = new ArrayList ();

			for (i = 0; i < named_args.Count; i++) {

				DictionaryEntry de = (DictionaryEntry) named_args [i];

				string member_name = (string) de.Key;
				Argument a  = (Argument) de.Value;

				if (!a.Resolve (ec, Location))
					return null;

				Expression member = Expression.MemberLookup (ec, Type, member_name, false,
									     MemberTypes.Field | MemberTypes.Property,
									     BindingFlags.Public | BindingFlags.Instance,
									     Location);

				if (member == null || !(member is PropertyExpr || member is FieldExpr)) {
					error617 (member_name);
					return null;
				}

				if (member is PropertyExpr) {
					PropertyExpr pe = (PropertyExpr) member;
					PropertyInfo pi = pe.PropertyInfo;

					if (!pi.CanWrite) {
						error617 (member_name);
						return null;
					}

					Expression e = Expression.Reduce (ec, a.Expr);
					
					if (e is Literal) {
						object o = ((Literal) e).GetValue ();
						prop_values.Add (o);
						
						if (UsageAttr) {
							if (member_name == "AllowMultiple")
								this.AllowMultiple = (bool) o;
							if (member_name == "Inherited")
								this.Inherited = (bool) o;
						}
						
					} else { 
						error182 ();
						return null;
					}
					
					prop_infos.Add (pi);
					
				} else if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						error617 (member_name);
						return null;
					}

					Expression e = Expression.Reduce (ec, a.Expr);
					
					if (e is Literal)
						field_values.Add (((Literal) e).GetValue ());
					else { 
						error182 ();
						return null;
					}
					
					field_infos.Add (fi);
				}
			}
			
			Expression mg = Expression.MemberLookup (ec, Type, ".ctor", false,
								 MemberTypes.Constructor,
								 BindingFlags.Public | BindingFlags.Instance,
								 Location);

			if (mg == null) {
				Report.Error (-6, Location, "Could not find a constructor for this argument list.");
				return null;
			}

			MethodBase constructor = Invocation.OverloadResolve (ec, (MethodGroupExpr) mg, pos_args, Location);

			if (constructor == null) {
				Report.Error (-6, Location, "Could not find a constructor for this argument list.");
				return null;
			}
			
			PropertyInfo [] prop_info_arr = new PropertyInfo [prop_infos.Count];
			FieldInfo [] field_info_arr = new FieldInfo [field_infos.Count];
			object [] field_values_arr = new object [field_values.Count];
			object [] prop_values_arr = new object [prop_values.Count];

			field_infos.CopyTo  (field_info_arr, 0);
			field_values.CopyTo (field_values_arr, 0);

			prop_values.CopyTo  (prop_values_arr, 0);
			prop_infos.CopyTo   (prop_info_arr, 0);
			
			CustomAttributeBuilder cb = new CustomAttributeBuilder ((ConstructorInfo) constructor, pos_values,
										prop_info_arr, prop_values_arr,
										field_info_arr, field_values_arr); 
			
			return cb;
		}

		static string GetValidPlaces (Attribute attr)
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = 0;
			
			TypeContainer a = TypeManager.LookupAttr (attr.Type);

			if (a == null) {
				System.Attribute [] attrs = System.Attribute.GetCustomAttributes (attr.Type);
				
				foreach (System.Attribute tmp in attrs)
					if (tmp is AttributeUsageAttribute) 
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
			} else
				targets = a.Targets;

			
			if ((targets & AttributeTargets.Assembly) != 0)
				sb.Append ("'assembly' ");

			if ((targets & AttributeTargets.Class) != 0)
				sb.Append ("'class' ");

			if ((targets & AttributeTargets.Constructor) != 0)
				sb.Append ("'constructor' ");

			if ((targets & AttributeTargets.Delegate) != 0)
				sb.Append ("'delegate' ");

			if ((targets & AttributeTargets.Enum) != 0)
				sb.Append ("'enum' ");

			if ((targets & AttributeTargets.Event) != 0)
				sb.Append ("'event' ");

			if ((targets & AttributeTargets.Field) != 0)
				sb.Append ("'field' ");

			if ((targets & AttributeTargets.Interface) != 0)
				sb.Append ("'interface' ");

			if ((targets & AttributeTargets.Method) != 0)
				sb.Append ("'method' ");

			if ((targets & AttributeTargets.Module) != 0)
				sb.Append ("'module' ");

			if ((targets & AttributeTargets.Parameter) != 0)
				sb.Append ("'parameter' ");

			if ((targets & AttributeTargets.Property) != 0)
				sb.Append ("'property' ");

			if ((targets & AttributeTargets.ReturnValue) != 0)
				sb.Append ("'return value' ");

			if ((targets & AttributeTargets.Struct) != 0)
				sb.Append ("'struct' ");

			return sb.ToString ();

		}

		public static void Error592 (Attribute a, Location loc)
		{
			Report.Error (592, loc, "Attribute '" + a.Name + "' is not valid on this declaration type. " +
				      "It is valid on " + GetValidPlaces (a) + "declarations only.");
		}

		public static bool CheckAttribute (Attribute a, object element)
		{
			TypeContainer attr = TypeManager.LookupAttr (a.Type);
			AttributeTargets targets = 0;
			
			if (attr == null) {
				System.Attribute [] attrs = System.Attribute.GetCustomAttributes (a.Type);

				foreach (System.Attribute tmp in attrs)
					if (tmp is AttributeUsageAttribute) 
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
			} else
				targets = attr.Targets;

			if (element is Class) {
				if ((targets & AttributeTargets.Class) != 0)
					return true;
				else
					return false;
				
			} else if (element is Struct) {
				if ((targets & AttributeTargets.Struct) != 0)
					return true;
				else
					return false;
			} else if (element is Constructor) {
				if ((targets & AttributeTargets.Constructor) != 0)
					return true;
				else
					return false;
			} else if (element is Delegate) {
				if ((targets & AttributeTargets.Delegate) != 0)
					return true;
				else
					return false;
			} else if (element is Enum) {
				if ((targets & AttributeTargets.Enum) != 0)
					return true;
				else
					return false;
			} else if (element is Event) {
				if ((targets & AttributeTargets.Event) != 0)
					return true;
				else
					return false;
			} else if (element is Field) {
				if ((targets & AttributeTargets.Field) != 0)
					return true;
				else
					return false;
			} else if (element is Interface) {
				if ((targets & AttributeTargets.Interface) != 0)
					return true;
				else
					return false;
			} else if (element is Method) {
				if ((targets & AttributeTargets.Method) != 0)
					return true;
				else
					return false;
			} else if (element is Parameter) {
				if ((targets & AttributeTargets.Parameter) != 0)
					return true;
				else
					return false;
			} else if (element is Property) {
				if ((targets & AttributeTargets.Property) != 0)
					return true;
				else
					return false;
			}

			return false;
		}
	}
	
	public class AttributeSection {
		
		public readonly string    Target;
		public readonly ArrayList Attributes;
		
		public AttributeSection (string target, ArrayList attrs)
		{
			Target = target;
			Attributes = attrs;
		}
		
	}

	public class Attributes {

		public ArrayList AttributeSections;

		public Attributes (AttributeSection a)
		{
			AttributeSections = new ArrayList ();
			AttributeSections.Add (a);

		}

		public void AddAttribute (AttributeSection a)
		{
			if (a != null)
				AttributeSections.Add (a);
		}
	}
}
