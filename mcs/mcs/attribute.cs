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
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mono.CSharp {

	/// <summary>
	///   Base class for objects that can have Attributes applied to them.
	/// </summary>
	public abstract class Attributable {
		/// <summary>
		///   Attributes for this type
		/// </summary>
 		Attributes attributes;

		public Attributable(Attributes attrs)
		{
			attributes = attrs;
			if (attributes != null)
				attributes.CheckTargets (ValidAttributeTargets);
		}

		public Attributes OptAttributes 
		{
			get {
				return attributes;
			}
			set {
				attributes = value;
			}
		}

		/// <summary>
		/// Use member-specific procedure to apply attribute @a in @cb to the entity being built in @builder
		/// </summary>
		public abstract void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb);

		/// <summary>
		/// Returns combination of enabled AttributeTargets
		/// </summary>
		public abstract AttributeTargets AttributeTargets { get; }

		public abstract bool IsClsCompliaceRequired (DeclSpace ds);

		/// <summary>
		/// Gets list of valid attribute targets for explicit target declaration
		/// </summary>
		protected abstract string[] ValidAttributeTargets { get; }
	};

	public class Attribute {
		public readonly string Target;
		public readonly string    Name;
		public readonly ArrayList Arguments;

		public readonly Location Location;

		public Type Type;
		
		// Is non-null if type is AttributeUsageAttribute
		AttributeUsageAttribute usage_attribute;

		public AttributeUsageAttribute UsageAttribute {
			get {
				return usage_attribute;
			}
		}
		
		MethodImplOptions ImplOptions;
		UnmanagedType     UnmanagedType;
		CustomAttributeBuilder cb;
	
		// non-null if named args present after Resolve () is called
		PropertyInfo [] prop_info_arr;
		FieldInfo [] field_info_arr;
		object [] field_values_arr;
		object [] prop_values_arr;
		object [] pos_values;

		static PtrHashtable usage_attr_cache = new PtrHashtable ();
		
		public Attribute (string target, string name, ArrayList args, Location loc)
		{
			Name = name;
			Arguments = args;
			Location = loc;
			Target = target;
		}

		void Error_InvalidNamedArgument (string name)
		{
			Report.Error (617, Location, "'" + name + "' is not a valid named attribute " +
				      "argument. Named attribute arguments must be fields which are not " +
				      "readonly, static or const, or properties with a set accessor which "+
				      "are not static.");
		}

		static void Error_AttributeArgumentNotValid (Location loc)
		{
			Report.Error (182, loc,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
		}

		void Error_AttributeConstructorMismatch ()
		{
			Report.Error (-6, Location,
                                      "Could not find a constructor for this argument list.");
		}

		/// <summary>
		///   Tries to resolve the type of the attribute. Flags an error if it can't, and complain is true.
		/// </summary>
		protected virtual Type CheckAttributeType (DeclSpace ds, bool complain)
		{
			Type t1 = RootContext.LookupType (ds, Name, true, Location);

			// FIXME: Shouldn't do this for quoted attributes: [@A]
			Type t2 = RootContext.LookupType (ds, Name + "Attribute", true, Location);

			String err0616 = null;

			if (t1 != null && ! t1.IsSubclassOf (TypeManager.attribute_type)) {
				t1 = null;
				err0616 = "'" + Name + "': is not an attribute class";
			}
			if (t2 != null && ! t2.IsSubclassOf (TypeManager.attribute_type)) {
				t2 = null;
				err0616 = (err0616 != null) 
					? "Neither '" + Name + "' nor '" + Name + "Attribute' is an attribute class"
					: "'" + Name + "Attribute': is not an attribute class";
			}

			if (t1 != null && t2 != null) {
				Report.Error(1614, Location, "'" + Name + "': is ambiguous; " 
					     + " use either '@" + Name + "' or '" + Name + "Attribute'");
				return null;
			}
			if (t1 != null)
				return t1;
			if (t2 != null)
				return t2;

			if (err0616 != null) {
				Report.Error (616, Location, err0616);
				return null;
			}

			if (complain)
				Report.Error (246, Location, 
					      "Could not find attribute '" + Name 
					      + "' (are you missing a using directive or an assembly reference ?)");
			return null;
		}

		public Type ResolveType (DeclSpace ds, bool complain)
		{
			if (Type == null)
				Type = CheckAttributeType (ds, complain);
			return Type;
		}

		/// <summary>
		///   Validates the guid string
		/// </summary>
		bool ValidateGuid (string guid)
		{
			try {
				new Guid (guid);
				return true;
			} catch {
				Report.Error (647, Location, "Format of GUID is invalid: " + guid);
				return false;
			}
		}

		//
		// Given an expression, if the expression is a valid attribute-argument-expression
		// returns an object that can be used to encode it, or null on failure.
		//
		public static bool GetAttributeArgumentExpression (Expression e, Location loc, out object result)
		{
			if (e is Constant) {
				result = ((Constant) e).GetValue ();
				return true;
			} else if (e is TypeOf) {
				result = ((TypeOf) e).TypeArg;
				return true;
			} else if (e is ArrayCreation){
				result =  ((ArrayCreation) e).EncodeAsAttribute ();
				if (result != null)
					return true;
			} else if (e is EmptyCast) {
				result = e;
				if (((EmptyCast) e).Child is Constant) {
					result = ((Constant) ((EmptyCast)e).Child).GetValue();
				}
				return true;
			}

			result = null;
			Error_AttributeArgumentNotValid (loc);
			return false;
		}
		
		public CustomAttributeBuilder Resolve (EmitContext ec)
		{
			Type oldType = Type;
			
			// Sanity check.
			Type = CheckAttributeType (ec.DeclSpace, true);
			if (oldType == null && Type == null)
				return null;
			if (oldType != null && oldType != Type) {
				Report.Error (-6, Location,
					      "Attribute {0} resolved to different types at different times: {1} vs. {2}",
					      Name, oldType, Type);
				return null;
			}

			bool MethodImplAttr = false;
			bool MarshalAsAttr = false;
			bool GuidAttr = false;
			bool usage_attr = false;

			bool DoCompares = true;

                        //
                        // If we are a certain special attribute, we
                        // set the information accordingly
                        //
                        
			if (Type == TypeManager.attribute_usage_type)
				usage_attr = true;
			else if (Type == TypeManager.methodimpl_attr_type)
				MethodImplAttr = true;
			else if (Type == TypeManager.marshal_as_attr_type)
				MarshalAsAttr = true;
			else if (Type == TypeManager.guid_attr_type)
				GuidAttr = true;
			else
				DoCompares = false;

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

			pos_values = new object [pos_arg_count];

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

				object val;
				if (!GetAttributeArgumentExpression (e, Location, out val))
					return null;
				
				pos_values [i] = val;
				if (DoCompares){
					if (usage_attr)
						usage_attribute = new AttributeUsageAttribute ((AttributeTargets) pos_values [0]);
					else if (MethodImplAttr)
						this.ImplOptions = (MethodImplOptions) pos_values [0];
					else if (GuidAttr){
						//
						// we will later check the validity of the type
						//
						if (pos_values [0] is string){
							if (!ValidateGuid ((string) pos_values [0]))
								return null;
						}
						
					} else if (MarshalAsAttr)
						this.UnmanagedType =
						(System.Runtime.InteropServices.UnmanagedType) pos_values [0];
				}
			}

			//
			// Now process named arguments
			//

			ArrayList field_infos = null;
			ArrayList prop_infos  = null;
			ArrayList field_values = null;
			ArrayList prop_values = null;

			if (named_args.Count > 0) {
				field_infos = new ArrayList ();
				prop_infos  = new ArrayList ();
				field_values = new ArrayList ();
				prop_values = new ArrayList ();
			}

			Hashtable seen_names = new Hashtable();
			
			for (i = 0; i < named_args.Count; i++) {
				DictionaryEntry de = (DictionaryEntry) named_args [i];
				string member_name = (string) de.Key;
				Argument a  = (Argument) de.Value;
				Expression e;

				if (seen_names.Contains(member_name)) {
					Report.Error(643, Location, "'" + member_name + "' duplicate named attribute argument");
					return null;
				}				
				seen_names.Add(member_name, 1);
				
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
						Constant c;
						
						if (e.Type != pi.PropertyType){
							c = Const.ChangeType (Location, (Constant) e, pi.PropertyType);
							if (c == null)
								return null;
						} else
							c = (Constant) e;
						
						object o = c.GetValue ();
						prop_values.Add (o);
						
						if (usage_attribute != null) {
							if (member_name == "AllowMultiple")
								usage_attribute.AllowMultiple = (bool) o;
							if (member_name == "Inherited")
								usage_attribute.Inherited = (bool) o;
						}
						
					} else if (e is TypeOf) {
						prop_values.Add (((TypeOf) e).TypeArg);
					} else if (e is ArrayCreation) {
						prop_values.Add (((ArrayCreation) e).EncodeAsAttribute());
					} else {
						Error_AttributeArgumentNotValid (Location);
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
						Constant c = (Constant) e;;
						
						if (c.Type != fi.FieldType){
							c = Const.ChangeType (Location, (Constant) e, fi.FieldType);
							if (c == null)
								return null;
						} 
						
						object value = c.GetValue ();
						field_values.Add (value);
					} else if (e is TypeOf) {
						field_values.Add (((TypeOf) e).TypeArg);
					} else {
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
					field_infos.Add (fi);
				}
			}

			Expression mg = Expression.MemberLookup (
				ec, Type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                Location);

			if (mg == null) {
				Error_AttributeConstructorMismatch ();
				return null;
			}

			MethodBase constructor = Invocation.OverloadResolve (
				ec, (MethodGroupExpr) mg, pos_args, Location);

			if (constructor == null) {
				Error_AttributeConstructorMismatch ();
				return null;
			}

			//
			// Now we perform some checks on the positional args as they
			// cannot be null for a constructor which expects a parameter
			// of type object
			//

			ParameterData pd = Invocation.GetParameterData (constructor);

			int group_in_params_array = Int32.MaxValue;
			int pc = pd.Count;
			if (pc > 0 && pd.ParameterModifier (pc-1) == Parameter.Modifier.PARAMS)
				group_in_params_array = pc-1;

			for (int j = 0; j < pos_arg_count; ++j) {
				Argument a = (Argument) pos_args [j];
				
				if (a.Expr is NullLiteral && pd.ParameterType (j) == TypeManager.object_type) {
					Error_AttributeArgumentNotValid (Location);
					return null;
				}

				if (j < group_in_params_array)
					continue;
				
				if (j == group_in_params_array){
					object v = pos_values [j];
					int count = pos_arg_count - j;

					object [] array = new object [count];
					pos_values [j] = array;
					array [0] = v;
				} else {
					object [] array = (object []) pos_values [group_in_params_array];

					array [j - group_in_params_array] = pos_values [j];
				}
			}

			//
			// Adjust the size of the pos_values if it had params
			//
			if (group_in_params_array != Int32.MaxValue){
				int argc = group_in_params_array+1;
				object [] new_pos_values = new object [argc];

				for (int p = 0; p < argc; p++)
					new_pos_values [p] = pos_values [p];
				pos_values = new_pos_values;
			}

			try {
				if (named_args.Count > 0) {
					prop_info_arr = new PropertyInfo [prop_infos.Count];
					field_info_arr = new FieldInfo [field_infos.Count];
					field_values_arr = new object [field_values.Count];
					prop_values_arr = new object [prop_values.Count];

					field_infos.CopyTo  (field_info_arr, 0);
					field_values.CopyTo (field_values_arr, 0);

					prop_values.CopyTo  (prop_values_arr, 0);
					prop_infos.CopyTo   (prop_info_arr, 0);

					cb = new CustomAttributeBuilder (
						(ConstructorInfo) constructor, pos_values,
						prop_info_arr, prop_values_arr,
						field_info_arr, field_values_arr);
				}
				else
					cb = new CustomAttributeBuilder (
						(ConstructorInfo) constructor, pos_values);
			} catch (NullReferenceException) {
				// 
				// Don't know what to do here
				//
				Report.Warning (
				        -101, Location, "NullReferenceException while trying to create attribute." +
                                        "Something's wrong!");
			} catch (Exception e) {
				//
				// Sample:
				// using System.ComponentModel;
				// [DefaultValue (CollectionChangeAction.Add)]
				// class X { static void Main () {} }
				//
				Report.Warning (
					-23, Location,
					"The compiler can not encode this attribute in .NET due to\n" +
					"\ta bug in the .NET runtime.  Try the Mono runtime.\nThe error was: " + e.Message);
			}
			
			return cb;
		}

		/// <summary>
		///   Get a string containing a list of valid targets for the attribute 'attr'
		/// </summary>
		static string GetValidTargets (Attribute attr)
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = attr.GetAttributeUsage ().ValidOn;

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
				sb.Append ("'return' ");

			if ((targets & AttributeTargets.Struct) != 0)
				sb.Append ("'struct' ");

			return sb.ToString ();

		}

		public static void Error_AttributeNotValidForElement (Attribute a, Location loc)
		{
			Report.Error (
				592, loc, "Attribute '" + a.Name +
				"' is not valid on this declaration type. " +
				"It is valid on " + GetValidTargets (a) + "declarations only.");
		}


		/// <summary>
		/// Returns AttributeUsage attribute for this type
		/// </summary>
		public AttributeUsageAttribute GetAttributeUsage ()
		{
			AttributeUsageAttribute ua = usage_attr_cache [Type] as AttributeUsageAttribute;
			if (ua != null)
				return ua;

			Class attr_class = TypeManager.LookupClass (Type);

			if (attr_class == null) {
				object[] usage_attr = Type.GetCustomAttributes (TypeManager.attribute_usage_type, true);
				ua = (AttributeUsageAttribute)usage_attr [0];
				usage_attr_cache.Add (Type, ua);
				return ua;
			}
		
			return attr_class.AttributeUsage;
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
				Error_AttributeConstructorMismatch ();
				return null;
			}

			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count != 1){
				Error_AttributeConstructorMismatch ();
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch ();
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
				Error_AttributeConstructorMismatch ();
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch ();
				return null;
			}

			if (pos_args.Count == 2){
				Argument arg2 = (Argument) pos_args [1];
				if (!(arg2.Expr is BoolConstant)){
					Error_AttributeConstructorMismatch ();
					return null;
				}
				is_error = ((BoolConstant) arg2.Expr).Value;
			}

			return ((StringConstant) arg.Expr).Value;
		}

		public string IndexerName_GetIndexerName (EmitContext ec)
		{
			if (Arguments == null || Arguments [0] == null){
				Error_AttributeConstructorMismatch ();
				return null;
			}
			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count != 1) {
				Error_AttributeConstructorMismatch ();
				return null;
			}
			
			Argument arg = (Argument) pos_args [0];
			if (!arg.Resolve (ec, Location))
				return null;
			
			StringConstant sc = arg.Expr as StringConstant;
			if (sc == null){
				Error_AttributeConstructorMismatch ();
				return null;
			}
			
			return sc.Value;
		}

		/// <summary>
		/// Returns value of CLSCompliantAttribute contructor parameter but because the method can be called
		/// before ApplyAttribute. We need to resolve the arguments.
		/// This situation occurs when class deps is differs from Emit order.  
		/// </summary>
		public bool GetClsCompliantAttributeValue (DeclSpace ds)
		{
			if (pos_values == null) {
				EmitContext ec = new EmitContext (ds, ds, Location, null, null, 0, false);

				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve (ec);
			}

			// Some error occurred
			if (pos_values [0] == null)
				return false;

			return (bool)pos_values [0];
		}

		public object GetPositionalValue (int i)
		{
			return (pos_values == null) ? null : pos_values[i];
		}

		object GetFieldValue (string name)
                {
			int i;
			if (field_info_arr == null)
				return null;
			i = 0;
			foreach (FieldInfo fi in field_info_arr) {
				if (fi.Name == name)
					return field_values_arr [i];
				i++;
			}
			return null;
		}

		public UnmanagedMarshal GetMarshal ()
		{
			object o = GetFieldValue ("ArraySubType");
			UnmanagedType array_sub_type = o == null ? UnmanagedType.I4 : (UnmanagedType) o;
			
			switch (UnmanagedType) {
			case UnmanagedType.CustomMarshaler:
				MethodInfo define_custom = typeof (UnmanagedMarshal).GetMethod ("DefineCustom",
                                                                       BindingFlags.Static | BindingFlags.Public);
				if (define_custom == null)
					return null;
				
				object [] args = new object [4];
				args [0] = GetFieldValue ("MarshalTypeRef");
				args [1] = GetFieldValue ("MarshalCookie");
				args [2] = GetFieldValue ("MarshalType");
				args [3] = Guid.Empty;
				return (UnmanagedMarshal) define_custom.Invoke (null, args);
				
			case UnmanagedType.LPArray:				
				return UnmanagedMarshal.DefineLPArray (array_sub_type);
			
			case UnmanagedType.SafeArray:
				return UnmanagedMarshal.DefineSafeArray (array_sub_type);
			
			case UnmanagedType.ByValArray:
				return UnmanagedMarshal.DefineByValArray ((int) GetFieldValue ("SizeConst"));
			
			case UnmanagedType.ByValTStr:
				return UnmanagedMarshal.DefineByValTStr ((int) GetFieldValue ("SizeConst"));
			
			default:
				return UnmanagedMarshal.DefineUnmanagedMarshal (UnmanagedType);
			}
		}

		public bool IsInternalCall
		{
			get { return ImplOptions == MethodImplOptions.InternalCall; }
		}

		protected virtual bool CanIgnoreInvalidAttribute (Attributable ias)
		{
			return false;
		}

		/// <summary>
		/// Emit attribute for Attributable symbol
		/// </summary>
		public void Emit (EmitContext ec, Attributable ias, ListDictionary emitted_attr)
		{
			CustomAttributeBuilder cb = Resolve (ec);
			if (cb == null)
				return;

			AttributeUsageAttribute usage_attr = GetAttributeUsage ();
			if ((usage_attr.ValidOn & ias.AttributeTargets) == 0) {
				// The parser applies toplevel attributes both to the assembly and
				// to a top-level class, if any.  So, be silent about them.
				if (! CanIgnoreInvalidAttribute (ias))
					Error_AttributeNotValidForElement (this, Location);
				return;
			}

			ias.ApplyAttributeBuilder (this, cb);

			string emitted = emitted_attr [Type] as string;
			if (Target != null && emitted == Target && !usage_attr.AllowMultiple) {
				Report.Error (579, Location, "Duplicate '" + Name + "' attribute");
			}

			emitted_attr [Type] = Target;

			// Here we are testing attribute arguments for array usage (error 3016)
			if (ias.IsClsCompliaceRequired (ec.DeclSpace)) {
				if (Arguments == null)
					return;

				ArrayList pos_args = (ArrayList) Arguments [0];
				if (pos_args != null) {
					foreach (Argument arg in pos_args) { 
						// Type is undefined (was error 246)
						if (arg.Type == null)
							return;

						if (arg.Type.IsArray) {
							Report.Error_T (3016, Location);
							return;
						}
					}
				}
			
				if (Arguments.Count < 2)
					return;
			
				ArrayList named_args = (ArrayList) Arguments [1];
				foreach (DictionaryEntry de in named_args) {
					Argument arg  = (Argument) de.Value;

					// Type is undefined (was error 246)
					if (arg.Type == null)
						return;

					if (arg.Type.IsArray) {
						Report.Error_T (3016, Location);
						return;
					}
				}
			}
		}

		public object GetValue (EmitContext ec, Constant c, Type target)
		{
			if (Convert.ImplicitConversionExists (ec, c, target))
				return c.GetValue ();

			Convert.Error_CannotImplicitConversion (Location, c.Type, target);
			return null;
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

			ResolveType (ec.DeclSpace, true);
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
				Error_AttributeArgumentNotValid (Location);
				return null;
			}

			// Now we process the named arguments
			CallingConvention cc = CallingConvention.Winapi;
			CharSet charset = CharSet.Ansi;
			bool preserve_sig = true;
#if FIXME
			bool exact_spelling = false;
#endif
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

						try {
							if (member_name == "CallingConvention"){
								object val = GetValue (ec, c, typeof (CallingConvention));
								if (val == null)
									return null;
								cc = (CallingConvention) val;
							} else if (member_name == "CharSet"){
								charset = (CharSet) c.GetValue ();
							} else if (member_name == "EntryPoint")
								entry_point = (string) c.GetValue ();
							else if (member_name == "SetLastError")
								set_last_err = (bool) c.GetValue ();
#if FIXME
							else if (member_name == "ExactSpelling")
								exact_spelling = (bool) c.GetValue ();
#endif
							else if (member_name == "PreserveSig")
								preserve_sig = (bool) c.GetValue ();
						} catch (InvalidCastException){
							Error_InvalidNamedArgument (member_name);
							Error_AttributeArgumentNotValid (Location);
						}
					} else { 
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
				}
			}

			if (entry_point == null)
				entry_point = name;
			if (set_last_err)
				charset = (CharSet)((int)charset | 0x40);
			
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

		private Expression GetValue () 
		{
			if ((Arguments == null) || (Arguments.Count < 1))
				return null;
			ArrayList al = (ArrayList) Arguments [0];
			if ((al == null) || (al.Count < 1))
				return null;
			Argument arg = (Argument) al [0];
			if ((arg == null) || (arg.Expr == null))
				return null;
			return arg.Expr;
		}

		public string GetString () 
		{
			Expression e = GetValue ();
			if (e is StringLiteral)
				return (e as StringLiteral).Value;
			return null;
		}

		public bool GetBoolean () 
		{
			Expression e = GetValue ();
			if (e is BoolLiteral)
				return (e as BoolLiteral).Value;
			return false;
		}
	}
	

	/// <summary>
	/// For global attributes (assembly, module) we need special handling.
	/// Attributes can be located in the several files
	/// </summary>
	public class GlobalAttribute: Attribute
	{
		public readonly NamespaceEntry ns;

		public GlobalAttribute (TypeContainer container, string target, string name, ArrayList args, Location loc):
			base (target, name, args, loc)
		{
			ns = container.NamespaceEntry;
		}

		protected override Type CheckAttributeType (DeclSpace ds, bool complain)
		{
			NamespaceEntry old = ds.NamespaceEntry;
			if (old == null || old.NS == null || old.NS == Namespace.Root) 
				ds.NamespaceEntry = ns;
			return base.CheckAttributeType (ds, complain);
		}

		protected override bool CanIgnoreInvalidAttribute (Attributable ias)
		{
			// Ignore error if this attribute is shared between the Assembly
			// and a top-level class.  The parser couldn't figure out which entity
			// this attribute belongs to.  If this attribute is erroneous, it should
			// be caught when it is processed by the top-level class.

			return (Target == null && ias is CommonAssemblyModulClass);
		}
	}

	public class Attributes {
		public ArrayList Attrs;

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

		/// <summary>
		/// Checks whether attribute target is valid for the current element
		/// </summary>
		public void CheckTargets (string[] possible_targets)
		{
			foreach (Attribute a in Attrs) {
				if (a.Target == null)
					continue;

				if (((IList) possible_targets).Contains (a.Target))
					continue;

				StringBuilder sb = new StringBuilder ();
				foreach (string s in possible_targets) {
					sb.Append (s);
					sb.Append (", ");
				}
				sb.Remove (sb.Length - 2, 2);
				Report.Error_T (657, a.Location, a.Target, sb.ToString ());
			}
		}

		private Attribute Search (Type t, DeclSpace ds, bool complain)
		{
			foreach (Attribute a in Attrs) {
				if (a.ResolveType (ds, complain) == t)
					return a;
			}
			return null;
		}

		public Attribute Search (Type t, DeclSpace ds)
		{
			return Search (t, ds, true);
		}

		public void Emit (EmitContext ec, Attributable ias)
		{
			ListDictionary ld = new ListDictionary ();

			foreach (Attribute a in Attrs)
				a.Emit (ec, ias, ld);
		}

		public bool Contains (Type t, DeclSpace ds)
		{
			return Search (t, ds) != null;
		}

		public Attribute GetClsCompliantAttribute (DeclSpace ds)
		{
			return Search (TypeManager.cls_compliant_attribute_type, ds, false);
		}

		/// <summary>
		/// Pulls the IndexerName attribute from an Indexer if it exists.
		/// </summary>
		public string ScanForIndexerName (EmitContext ec)
		{
			Attribute a = Search (TypeManager.indexer_name_type, ec.DeclSpace);
			if (a == null)
				return null;

			// Remove the attribute from the list
			//TODO: It is very close to hack and it can crash here
			Attrs.Remove (a);

			return a.IndexerName_GetIndexerName (ec);
		}

	}

	/// <summary>
	/// Helper class for attribute verification routine.
	/// </summary>
	sealed class AttributeTester
	{
		static PtrHashtable analyzed_types = new PtrHashtable ();

		private AttributeTester ()
		{
		}

		/// <summary>
		/// Returns true if parameters of two compared methods are CLS-Compliant.
		/// It tests differing only in ref or out, or in array rank.
		/// </summary>
		public static bool AreOverloadedMethodParamsClsCompliant (Type[] types_a, Type[] types_b) 
		{
			if (types_a == null || types_b == null)
				return true;

			if (types_a.Length != types_b.Length)
				return true;

			for (int i = 0; i < types_b.Length; ++i) {
				Type aType = types_a [i];
				Type bType = types_b [i];

				if (aType.IsArray && bType.IsArray && aType.GetArrayRank () != bType.GetArrayRank () && aType.GetElementType () == bType.GetElementType ()) {
					return false;
				}

				Type aBaseType = aType;
				bool is_either_ref_or_out = false;

				if (aType.IsByRef || aType.IsPointer) {
					aBaseType = aType.GetElementType ();
					is_either_ref_or_out = true;
				}

				Type bBaseType = bType;
				if (bType.IsByRef || bType.IsPointer) 
				{
					bBaseType = bType.GetElementType ();
					is_either_ref_or_out = !is_either_ref_or_out;
				}

				if (aBaseType != bBaseType)
					continue;

				if (is_either_ref_or_out)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Goes through all parameters and test if they are CLS-Compliant.
		/// </summary>
		public static bool AreParametersCompliant (Parameter[] fixedParameters, Location loc)
		{
			if (fixedParameters == null)
				return true;

			foreach (Parameter arg in fixedParameters) {
				if (!AttributeTester.IsClsCompliant (arg.ParameterType)) {
					Report.Error_T (3001, loc, arg.GetSignatureForError ());
					return false;
				}
			}
			return true;
		}


		/// <summary>
		/// This method tests the CLS compliance of external types. It doesn't test type visibility.
		/// </summary>
		public static bool IsClsCompliant (Type type) 
		{
			if (type == null)
				return true;

			object type_compliance = analyzed_types[type];
			if (type_compliance != null)
				return type_compliance == TRUE;

			if (type.IsPointer) {
				analyzed_types.Add (type, null);
				return false;
			}

			bool result;
			if (type.IsArray || type.IsByRef)	{
				result = IsClsCompliant (TypeManager.GetElementType (type));
			} else {
				result = AnalyzeTypeCompliance (type);
			}
			analyzed_types.Add (type, result ? TRUE : FALSE);
			return result;
		}                

		static object TRUE = new object ();
		static object FALSE = new object ();

		/// <summary>
		/// Non-hierarchical CLS Compliance analyzer
		/// </summary>
		public static bool IsComplianceRequired (MemberInfo mi, DeclSpace ds)
		{
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (mi.DeclaringType);

			// Type is external, we can get attribute directly
			if (temp_ds == null) {
				object[] cls_attribute = mi.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
				return (cls_attribute.Length == 1 && ((CLSCompliantAttribute)cls_attribute[0]).IsCompliant);
			}

			string tmp_name;
			// Interface doesn't store full name
			if (temp_ds is Interface)
				tmp_name = mi.Name;
			else
				tmp_name = String.Concat (temp_ds.Name, ".", mi.Name);

			MemberCore mc = temp_ds.GetDefinition (tmp_name) as MemberCore;
			return mc.IsClsCompliaceRequired (ds);
		}

		public static void VerifyModulesClsCompliance ()
		{
			Module[] modules = TypeManager.Modules;
			if (modules == null)
				return;

			// The first module is generated assembly
			for (int i = 1; i < modules.Length; ++i) {
				Module module = modules [i];
				if (!IsClsCompliant (module)) {
					Report.Error_T (3013, module.Name);
					return;
				}
			}
		}

		static bool IsClsCompliant (ICustomAttributeProvider attribute_provider) 
		{
			object[] CompliantAttribute = attribute_provider.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (CompliantAttribute.Length == 0)
				return false;

			return ((CLSCompliantAttribute)CompliantAttribute[0]).IsCompliant;
		}

		static bool AnalyzeTypeCompliance (Type type)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (type);
			if (ds != null) {
				return ds.IsClsCompliaceRequired (ds.Parent);
			}

			object[] CompliantAttribute = type.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (CompliantAttribute.Length == 0) 
				return IsClsCompliant (type.Assembly);

			return ((CLSCompliantAttribute)CompliantAttribute[0]).IsCompliant;
		}
	}
}
