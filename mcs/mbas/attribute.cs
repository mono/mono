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
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mono.MonoBASIC {

	public class Attribute {
		public readonly string ExplicitTarget;
		public readonly string    Name;
		public readonly ArrayList Arguments;

		Location Location;

		public Type Type;
		
		//
		// The following are only meaningful when the attribute
		// being emitted is one of the builtin ones
		//
		AttributeTargets Targets;
		bool AllowMultiple;
		bool Inherited;

		bool UsageAttr = false;
		
		MethodImplOptions ImplOptions;
		UnmanagedType     UnmanagedType;
		CustomAttributeBuilder cb;

		public Attribute (string name, ArrayList args, Location loc)
		{
			Name = name;
			Arguments = args;
			Location = loc;
		}
		
		public Attribute (string target, string name, ArrayList args, Location loc) 
		{
			ExplicitTarget = target;
		}

		void Error_InvalidNamedArgument (string name)
		{
			Report.Error (617, Location, "'" + name + "' is not a valid named attribute " +
				      "argument. Named attribute arguments must be fields which are not " +
				      "readonly, static or const, or properties with a set accessor which "+
				      "are not static.");
		}

		void Error_AttributeArgumentNotValid ()
		{
			Report.Error (182, Location,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
		}

		static void Error_AttributeConstructorMismatch (Location loc)
		{
			Report.Error (
					-6, loc,
					"Could not find a constructor for this argument list.");
		}
		
		private Type CheckAttributeType (EmitContext ec) {
			Type t;
			bool isattributeclass = true;
			
			t = RootContext.LookupType (ec.DeclSpace, Name, true, Location);
			if (t != null) {
				isattributeclass = t.IsSubclassOf (TypeManager.attribute_type);
				if (isattributeclass)
					return t;
			}
			t = RootContext.LookupType (ec.DeclSpace, Name + "Attribute", true, Location);
			if (t != null) {
				if (t.IsSubclassOf (TypeManager.attribute_type))
					return t;
			}
			if (!isattributeclass) {
				Report.Error (616, Location, "'" + Name + "': is not an attribute class");
				return null;
			}
			if (t != null) {
				Report.Error (616, Location, "'" + Name + "Attribute': is not an attribute class");
				return null;
			}
			Report.Error (
				246, Location, "Could not find attribute '" + Name + "' (are you" +
				" missing a using directive or an assembly reference ?)");
			return null;
		}

		public Type ResolveType (EmitContext ec)
		{
			Type = CheckAttributeType (ec);
			return Type;
		}

		
		public CustomAttributeBuilder Resolve (EmitContext ec)
		{
			if (Type == null)
				Type = CheckAttributeType (ec);
			if (Type == null)
				return null;

			bool MethodImplAttr = false;
			bool MarshalAsAttr = false;

			UsageAttr = false;

			if (Type == TypeManager.attribute_usage_type)
				UsageAttr = true;
			if (Type == TypeManager.methodimpl_attr_type)
				MethodImplAttr = true;
			if (Type == TypeManager.marshal_as_attr_type)
				MarshalAsAttr = true;

			// Now we extract the positional and named arguments
			
			ArrayList pos_args = new ArrayList ();
			ArrayList named_args = new ArrayList ();
			int pos_arg_count = 0;
			
			if (Arguments != null) {
				pos_args = (ArrayList) Arguments [0];
				if (pos_args != null)
					pos_arg_count = pos_args.Count;
				if (Arguments.Count > 1)
					named_args = (ArrayList) Arguments [1];
			}

			object [] pos_values = new object [pos_arg_count];

			//
			// First process positional arguments 
			//

			int i;
			for (i = 0; i < pos_arg_count; i++) {
				Argument a = (Argument) pos_args [i];
				Expression e;

				if (!a.Resolve (ec, Location))
					return null;

				e = a.Expr;

				if (e is Constant) {
					pos_values [i] = ((Constant) e).GetValue ();
				} else if (e is TypeOf) {
					pos_values [i] = ((TypeOf) e).TypeArg;
				} else {
					Error_AttributeArgumentNotValid ();
					return null;
				}
				
				if (UsageAttr)
					this.Targets = (AttributeTargets) pos_values [0];
				
				if (MethodImplAttr)
					this.ImplOptions = (MethodImplOptions) pos_values [0];
				
				if (MarshalAsAttr)
					this.UnmanagedType =
					(System.Runtime.InteropServices.UnmanagedType) pos_values [0];
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
				Expression e;
				
				if (!a.Resolve (ec, Location))
					return null;

				Expression member = Expression.MemberLookup (
					ec, Type, member_name,
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance,
					Location);

				if (member == null || !(member is PropertyExpr || member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return null;
				}

				e = a.Expr;
				if (member is PropertyExpr) {
					PropertyExpr pe = (PropertyExpr) member;
					PropertyInfo pi = pe.PropertyInfo;

					if (!pi.CanWrite) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (e is Constant) {
						object o = ((Constant) e).GetValue ();
						prop_values.Add (o);
						
						if (UsageAttr) {
							if (member_name == "AllowMultiple")
								this.AllowMultiple = (bool) o;
							if (member_name == "Inherited")
								this.Inherited = (bool) o;
						}
						
					} else if (e is TypeOf) {
						prop_values.Add (((TypeOf) e).TypeArg);
					} else {
						Error_AttributeArgumentNotValid ();
						return null;
					}
					
					prop_infos.Add (pi);
					
				} else if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					//
					// Handle charset here, and set the TypeAttributes
					
					if (e is Constant){
						object value = ((Constant) e).GetValue ();
						
						field_values.Add (value);
					} else if (e is TypeOf) {
						field_values.Add (((TypeOf) e).TypeArg);
					} else {
						Error_AttributeArgumentNotValid ();
						return null;
					}
					
					field_infos.Add (fi);
				}
			}

			Expression mg = Expression.MemberLookup (
				ec, Type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance, Location);

			if (mg == null) {
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			MethodBase constructor = Invocation.OverloadResolve (
				ec, (MethodGroupExpr) mg, pos_args, Location);

			if (constructor == null) {
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			//
			// Now we perform some checks on the positional args as they
			// cannot be null for a constructor which expects a parameter
			// of type object
			//

			ParameterData pd = Invocation.GetParameterData (constructor);

			for (int j = 0; j < pos_arg_count; ++j) {
				Argument a = (Argument) pos_args [j];
				
				if (a.Expr is NullLiteral && pd.ParameterType (j) == TypeManager.object_type) {
					Error_AttributeArgumentNotValid ();
					return null;
				}
			}
			
			PropertyInfo [] prop_info_arr = new PropertyInfo [prop_infos.Count];
			FieldInfo [] field_info_arr = new FieldInfo [field_infos.Count];
			object [] field_values_arr = new object [field_values.Count];
			object [] prop_values_arr = new object [prop_values.Count];

			field_infos.CopyTo  (field_info_arr, 0);
			field_values.CopyTo (field_values_arr, 0);

			prop_values.CopyTo  (prop_values_arr, 0);
			prop_infos.CopyTo   (prop_info_arr, 0);

			try {
				cb = new CustomAttributeBuilder (
					(ConstructorInfo) constructor, pos_values,
					prop_info_arr, prop_values_arr,
					field_info_arr, field_values_arr); 

			} catch (NullReferenceException) {
				// 
				// Don't know what to do here
				//
			} catch {
				//
				// Sample:
				// using System.ComponentModel;
				// [DefaultValue (CollectionChangeAction.Add)]
				// class X { static void Main () {} }
				//
				Report.Warning (
					-23, Location,
					"The compiler can not encode this attribute in .NET due to\n" +
					"\ta bug in the .NET runtime.  Try the Mono runtime");
			}
			
			return cb;
		}

		static string GetValidPlaces (Attribute attr)
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = 0;
			
			TypeContainer a = TypeManager.LookupAttr (attr.Type);

			if (a == null) {
				
				System.Attribute [] attrs = null;
				
				try {
					attrs = System.Attribute.GetCustomAttributes (attr.Type);
					
				} catch {
					Report.Error (-20, attr.Location, "Cannot find attribute type " + attr.Name +
						      " (maybe you forgot to set the usage using the" +
						      " AttributeUsage attribute ?).");
					return null;
				}
					
				foreach (System.Attribute tmp in attrs)
					if (tmp is AttributeUsageAttribute) {
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
						break;
					}
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

		public static void Error_AttributeNotValidForElement (Attribute a, Location loc)
		{
			Report.Error (
				592, loc, "Attribute '" + a.Name +
				"' is not valid on this declaration type. " +
				"It is valid on " + GetValidPlaces (a) + "declarations only.");
		}

		public static bool CheckAttribute (Attribute a, object element)
		{
			TypeContainer attr = TypeManager.LookupAttr (a.Type);
			AttributeTargets targets = 0;

			
			if (attr == null) {

				System.Attribute [] attrs = null;
				
				try {
					attrs = System.Attribute.GetCustomAttributes (a.Type);

				} catch {
					Report.Error (-20, a.Location, "Cannot find attribute type " + a.Name +
						      " (maybe you forgot to set the usage using the" +
						      " AttributeUsage attribute ?).");
					return false;
				}
					
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
			} else if (element is Event /*|| element is InterfaceEvent*/) {
				if ((targets & AttributeTargets.Event) != 0)
					return true;
				else
					return false;
			} else if (element is Field || element is FieldBuilder) {
				if ((targets & AttributeTargets.Field) != 0)
					return true;
				else
					return false;
			} else if (element is Interface) {
				if ((targets & AttributeTargets.Interface) != 0)
					return true;
				else
					return false;
			} else if (element is Method || element is Accessor) {
				if ((targets & AttributeTargets.Method) != 0)
					return true;
				else
					return false;
			} else if (element is ParameterBuilder) {
				if ((targets & AttributeTargets.Parameter) != 0)
					return true;
				else
					return false;
			} else if (element is Property || element is Indexer /*||
				   element is InterfaceProperty || element is InterfaceIndexer*/) {
				if ((targets & AttributeTargets.Property) != 0)
					return true;
				else
					return false;
			} else if (element is AssemblyBuilder){
				if ((targets & AttributeTargets.Assembly) != 0)
					return true;
				else
					return false;
			}

			return false;
		}

		//
		// This method should be invoked to pull the IndexerName attribute from an
		// Indexer if it exists.
		//
		public static string ScanForIndexerName (EmitContext ec, Attributes opt_attrs)
		{
			if (opt_attrs == null)
				return null;

			foreach (Attribute a in opt_attrs.Attrs) {
				if (a.ResolveType (ec) == null)
					return null;
					
				if (a.Type != TypeManager.indexer_name_type)
					continue;

				//
				// So we have found an IndexerName, pull the data out.
				//
				if (a.Arguments == null || a.Arguments [0] == null){
					Error_AttributeConstructorMismatch (a.Location);
					return null;
				}
				ArrayList pos_args = (ArrayList) a.Arguments [0];
				if (pos_args.Count == 0){
					Error_AttributeConstructorMismatch (a.Location);
					return null;
				}
					
				Argument arg = (Argument) pos_args [0];
				if (!arg.Resolve (ec, a.Location))
					return null;
					
				Expression e = arg.Expr;
				if (!(e is StringConstant)){
					Error_AttributeConstructorMismatch (a.Location);
					return null;
				}

				//
				// Remove the attribute from the list
				//
				opt_attrs.Attrs.Remove (a);

				return (((StringConstant) e).Value);
			}
			return null;
		}

		//
		// This pulls the condition name out of a Conditional attribute
		//
		public string Conditional_GetConditionName ()
		{
			//
			// So we have a Conditional, pull the data out.
			//
			if (Arguments == null || Arguments [0] == null){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count != 1){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			return ((StringConstant) arg.Expr).Value;
		}

		//
		// This pulls the obsolete message and error flag out of an Obsolete attribute
		//
		public string Obsolete_GetObsoleteMessage (out bool is_error)
		{
			is_error = false;
			//
			// So we have an Obsolete, pull the data out.
			//
			if (Arguments == null || Arguments [0] == null)
				return "";

			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count == 0)
				return "";
			else if (pos_args.Count > 2){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			if (pos_args.Count == 2){
				Argument arg2 = (Argument) pos_args [1];
				if (!(arg2.Expr is BoolConstant)){
					Error_AttributeConstructorMismatch (Location);
					return null;
				}
				is_error = ((BoolConstant) arg2.Expr).Value;
			}

			return ((StringConstant) arg.Expr).Value;
		}

		//
		// Applies the attributes to the `builder'.
		//							    		
		public static void ApplyAttributes (EmitContext ec, object builder, object kind,
						    Attributes opt_attrs, Location loc)
		{
			if (opt_attrs == null)
				return;

			foreach (Attribute a in opt_attrs.Attrs) {
				CustomAttributeBuilder cb = a.Resolve (ec);

				if (cb == null)
					continue;

				if (!(kind is TypeContainer))
					if (!CheckAttribute (a, kind)) {
							Error_AttributeNotValidForElement (a, loc);
							return;
					}

				if (kind is Method || kind is Accessor) {
					if (a.Type == TypeManager.methodimpl_attr_type) {
						if (a.ImplOptions == MethodImplOptions.InternalCall)
								((MethodBuilder) builder).SetImplementationFlags (MethodImplAttributes.InternalCall |	MethodImplAttributes.Runtime);
					} else if (a.Type != TypeManager.dllimport_type){
							((MethodBuilder) builder).SetCustomAttribute (cb);
					}
				} else if (kind is Constructor) {
						((ConstructorBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is Field) {
						((FieldBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is Property || kind is Indexer) {
						((PropertyBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is Event) {
						((MyEventBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is ParameterBuilder) {
					if (a.Type == TypeManager.marshal_as_attr_type) {
						UnmanagedMarshal marshal = UnmanagedMarshal.DefineUnmanagedMarshal (a.UnmanagedType);
						((ParameterBuilder) builder).SetMarshal (marshal);
					} else 
						((ParameterBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is Enum) {
					((TypeBuilder) builder).SetCustomAttribute (cb); 
				} else if (kind is TypeContainer) {
					TypeContainer tc = (TypeContainer) kind;
						
					if (a.UsageAttr) {
						tc.Targets = a.Targets;
						tc.AllowMultiple = a.AllowMultiple;
						tc.Inherited = a.Inherited;
							
					} else if (a.Type == TypeManager.default_member_type) {
						if (tc.Indexers != null) {
							Report.Error (646, loc, "Cannot specify the DefaultMember attribute on" + " a type containing an indexer");
							return;
						}
					} else {
						if (!CheckAttribute (a, kind)) {
							Error_AttributeNotValidForElement (a, loc);
								return;
						}
					}

					try {
						((TypeBuilder) builder).SetCustomAttribute (cb);
					} catch (System.ArgumentException) {
						Report.Warning (-21, loc, 	"The CharSet named property on StructLayout\n"+"\tdoes not work correctly on Microsoft.NET\n"+"\tYou might want to remove the CharSet declaration\n"+"\tor compile using the Mono runtime instead of the\n"+"\tMicrosoft .NET runtime");
					}
				} else if (kind is Interface) {
					Interface iface = (Interface) kind;
					if (!CheckAttribute (a, kind)) {
						Error_AttributeNotValidForElement (a, loc);
						return;
					}
					((TypeBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is AssemblyBuilder){
					((AssemblyBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is ModuleBuilder) {
					((ModuleBuilder) builder).SetCustomAttribute (cb);
				} else if (kind is FieldBuilder) {
					((FieldBuilder) builder).SetCustomAttribute (cb);
				} else
					throw new Exception ("Unknown kind: " + kind);
			}
		}

		public MethodBuilder DefinePInvokeMethod (EmitContext ec, TypeBuilder builder, string name,
							  MethodAttributes flags, Type ret_type, Type [] param_types)
		{
			//
			// We extract from the attribute the information we need 
			//

			if (Arguments == null) {
				Console.WriteLine ("Internal error : this is not supposed to happen !");
				return null;
			}

			Type = CheckAttributeType (ec);
			if (Type == null)
				return null;
			
			ArrayList named_args = new ArrayList ();
			
			ArrayList pos_args = (ArrayList) Arguments [0];
			if (Arguments.Count > 1)
				named_args = (ArrayList) Arguments [1];
			

			string dll_name = null;
			
			Argument tmp = (Argument) pos_args [0];

			if (!tmp.Resolve (ec, Location))
				return null;
			
			if (tmp.Expr is Constant)
				dll_name = (string) ((Constant) tmp.Expr).GetValue ();
			else { 
				Error_AttributeArgumentNotValid ();
				return null;
			}

			// Now we process the named arguments
			CallingConvention cc = CallingConvention.Winapi;
			CharSet charset = CharSet.Ansi;
			bool preserve_sig = true;
			bool exact_spelling = false;
			bool set_last_err = false;
			string entry_point = null;

			for (int i = 0; i < named_args.Count; i++) {

				DictionaryEntry de = (DictionaryEntry) named_args [i];

				string member_name = (string) de.Key;
				Argument a  = (Argument) de.Value;

				if (!a.Resolve (ec, Location))
					return null;

				Expression member = Expression.MemberLookup (
					ec, Type, member_name, 
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance,
					Location);

				if (member == null || !(member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return null;
				}

				if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (a.Expr is Constant) {
						Constant c = (Constant) a.Expr;
						
						if (member_name == "CallingConvention")
							cc = (CallingConvention) c.GetValue ();
						else if (member_name == "CharSet")
							charset = (CharSet) c.GetValue ();
						else if (member_name == "EntryPoint")
							entry_point = (string) c.GetValue ();
						else if (member_name == "SetLastError")
							set_last_err = (bool) c.GetValue ();
						else if (member_name == "ExactSpelling")
							exact_spelling = (bool) c.GetValue ();
						else if (member_name == "PreserveSig")
							preserve_sig = (bool) c.GetValue ();
					} else { 
						Error_AttributeArgumentNotValid ();
						return null;
					}
					
				}
			}

			if (entry_point == null)
				entry_point = name;
			
			MethodBuilder mb = builder.DefinePInvokeMethod (
				name, dll_name, entry_point, flags | MethodAttributes.HideBySig,
				CallingConventions.Standard,
				ret_type,
				param_types,
				cc,
				charset);

			if (preserve_sig)
				mb.SetImplementationFlags (MethodImplAttributes.PreserveSig);
			
			return mb;
		}			
	}
	
	public class Attributes {
		public ArrayList Attrs;
		public Location Location;
		
		public Attributes (Attribute a)
		{
			Attrs = new ArrayList ();
			Attrs.Add (a);
		}

		public Attributes (ArrayList attrs)
		{
			Attrs = attrs;
		}

		public void AddAttributes (ArrayList attrs)
		{
			Attrs.AddRange (attrs);
		}

		public bool Contains (Type t)
		{
			foreach (Attribute a in Attrs){
					if (a.Type == t)
						return true;
				}
			return false;
		}			
	}
}
