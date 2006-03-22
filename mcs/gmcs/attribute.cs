//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//         Marek Safar (marek.safar@seznam.cz)
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
using System.Security; 
using System.Security.Permissions;
using System.Text;
using System.IO;

namespace Mono.CSharp {

	/// <summary>
	///   Base class for objects that can have Attributes applied to them.
	/// </summary>
	public abstract class Attributable {
		/// <summary>
		///   Attributes for this type
		/// </summary>
 		Attributes attributes;

		public Attributable (Attributes attrs)
		{
			OptAttributes = attrs;
		}

		public Attributes OptAttributes 
		{
			get {
				return attributes;
			}
			set {
				attributes = value;

				if (attributes != null) {
					attributes.AttachTo (this);
				}
			}
		}

		/// <summary>
		/// Use member-specific procedure to apply attribute @a in @cb to the entity being built in @builder
		/// </summary>
		public abstract void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb);

		/// <summary>
		/// Returns one AttributeTarget for this element.
		/// </summary>
		public abstract AttributeTargets AttributeTargets { get; }

		public abstract IResolveContext ResolveContext { get; }

		public abstract bool IsClsComplianceRequired ();

		/// <summary>
		/// Gets list of valid attribute targets for explicit target declaration.
		/// The first array item is default target. Don't break this rule.
		/// </summary>
		public abstract string[] ValidAttributeTargets { get; }
	};

	public class Attribute {
		public readonly string ExplicitTarget;
		public AttributeTargets Target;

		// TODO: remove this member
		public readonly string    Name;
		public readonly Expression LeftExpr;
		public readonly string Identifier;

		public readonly ArrayList Arguments;

		public readonly Location Location;

		public Type Type;

		bool resolve_error;
		readonly bool nameEscaped;
		Attributable owner;

		static AttributeUsageAttribute DefaultUsageAttribute = new AttributeUsageAttribute (AttributeTargets.All);
		static Assembly orig_sec_assembly;

		// non-null if named args present after Resolve () is called
		PropertyInfo [] prop_info_arr;
		FieldInfo [] field_info_arr;
		object [] field_values_arr;
		object [] prop_values_arr;
		object [] pos_values;

		static PtrHashtable usage_attr_cache = new PtrHashtable ();
		
		public Attribute (string target, Expression left_expr, string identifier, ArrayList args, Location loc, bool nameEscaped)
		{
			LeftExpr = left_expr;
			Identifier = identifier;
			Name = LeftExpr == null ? identifier : LeftExpr + "." + identifier;
			Arguments = args;
			Location = loc;
			ExplicitTarget = target;
			this.nameEscaped = nameEscaped;
		}

		public void AttachTo (Attributable owner)
		{
			this.owner = owner;
		}

		void Error_InvalidNamedArgument (string name)
		{
			Report.Error (617, Location, "`{0}' is not a valid named attribute argument. Named attribute arguments " +
				      "must be fields which are not readonly, static, const or read-write properties which are " +
				      "public and not static",
			      name);
		}

		void Error_InvalidNamedAgrumentType (string name)
		{
			Report.Error (655, Location, "`{0}' is not a valid named attribute argument because it is not a valid " +
				      "attribute parameter type", name);
		}

		static void Error_AttributeArgumentNotValid (string extra, Location loc)
		{
			Report.Error (182, loc,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression" + extra);
		}

		static void Error_AttributeArgumentNotValid (Location loc)
		{
			Error_AttributeArgumentNotValid ("", loc);
		}
		
		static void Error_TypeParameterInAttribute (Location loc)
		{
			Report.Error (
				-202, loc, "Can not use a type parameter in an attribute");
		}

		/// <summary>
		/// This is rather hack. We report many emit attribute error with same error to be compatible with
		/// csc. But because csc has to report them this way because error came from ilasm we needn't.
		/// </summary>
		public void Error_AttributeEmitError (string inner)
		{
			Report.Error (647, Location, "Error during emitting `{0}' attribute. The reason is `{1}'",
				      TypeManager.CSharpName (Type), inner);
		}

		public void Error_InvalidSecurityParent ()
		{
			Error_AttributeEmitError ("it is attached to invalid parent");
		}

		protected virtual FullNamedExpression ResolveAsTypeTerminal (Expression expr, EmitContext ec, bool silent)
		{
			return expr.ResolveAsTypeTerminal (ec, silent);
		}

		protected virtual FullNamedExpression ResolveAsTypeStep (Expression expr, EmitContext ec, bool silent)
		{
			return expr.ResolveAsTypeStep (ec, silent);
		}

		Type ResolvePossibleAttributeType (string name, bool silent, ref bool is_attr)
		{
			// It will disapear when IResolveContext will take a place
			DeclSpace container = owner.ResolveContext.DeclContainer;
			if (owner is TypeParameter)
				container = ((TypeParameter)owner).DeclSpace;
			EmitContext ec = new EmitContext (owner.ResolveContext, container, container,
					Location, null, null, container.ModFlags, false);
			//

			FullNamedExpression fn;
			if (LeftExpr == null) {
				fn = ResolveAsTypeTerminal (new SimpleName (name, Location), ec, silent);
			} else {
				fn = ResolveAsTypeStep (LeftExpr, ec, silent);
				if (fn == null)
					return null;
				fn = new MemberAccess (fn, name, Location).ResolveAsTypeTerminal (ec, silent);
			}

			TypeExpr te = fn as TypeExpr;
			if (te == null)
				return null;

			Type t = te.Type;
			if (TypeManager.IsSubclassOf (t, TypeManager.attribute_type)) {
				is_attr = true;
			} else if (!silent) {
				Report.SymbolRelatedToPreviousError (t);
				Report.Error (616, Location, "`{0}': is not an attribute class", TypeManager.CSharpName (t));
			}
			return t;
		}

		/// <summary>
		///   Tries to resolve the type of the attribute. Flags an error if it can't, and complain is true.
		/// </summary>
		void ResolveAttributeType ()
		{
			bool t1_is_attr = false;
			Type t1 = ResolvePossibleAttributeType (Identifier, true, ref t1_is_attr);

			bool t2_is_attr = false;
			Type t2 = nameEscaped ? null :
				ResolvePossibleAttributeType (Identifier + "Attribute", true, ref t2_is_attr);

			if (t1_is_attr && t2_is_attr) {
				Report.Error (1614, Location, "`{0}' is ambiguous between `{0}' and `{0}Attribute'. " +
					      "Use either `@{0}' or `{0}Attribute'", GetSignatureForError ());
				resolve_error = true;
				return;
			}

			if (t1_is_attr) {
				Type = t1;
				return;
			}

			if (t2_is_attr) {
				Type = t2;
				return;
			}

			if (t1 == null && t2 == null)
				ResolvePossibleAttributeType (Identifier, false, ref t1_is_attr);
			if (t1 != null)
				ResolvePossibleAttributeType (Identifier, false, ref t1_is_attr);
			if (t2 != null)
				ResolvePossibleAttributeType (Identifier + "Attribute", false, ref t2_is_attr);

			resolve_error = true;
		}

		public virtual Type ResolveType ()
		{
			if (Type == null && !resolve_error)
				ResolveAttributeType ();
			return Type;
		}

		public string GetSignatureForError ()
		{
			return LeftExpr == null ? Identifier : LeftExpr.GetSignatureForError () + "." + Identifier;
		}

		//
		// Given an expression, if the expression is a valid attribute-argument-expression
		// returns an object that can be used to encode it, or null on failure.
		//
		public static bool GetAttributeArgumentExpression (Expression e, Location loc, Type arg_type, out object result)
		{
			Constant constant = e as Constant;
			if (constant != null) {
				constant = constant.ToType (arg_type, loc);
				if (constant == null) {
					result = null;
					return false;
				}
				result = constant.GetTypedValue ();
				return true;
			} else if (e is TypeOf) {
				result = ((TypeOf) e).TypeArg;
				return true;
			} else if (e is ArrayCreation){
				result =  ((ArrayCreation) e).EncodeAsAttribute ();
				if (result != null)
					return true;
			} else if (e is EmptyCast) {
				Expression child = ((EmptyCast)e).Child;
				return GetAttributeArgumentExpression (child, loc, child.Type, out result);
			} else if (e is As) {
				As as_e = (As) e;
				return GetAttributeArgumentExpression (as_e.Expr, loc, as_e.ProbeType.Type, out result);
			}

			result = null;
			Error_AttributeArgumentNotValid (loc);
			return false;
		}

		bool IsValidArgumentType (Type t)
		{
			if (t.IsArray)
				t = t.GetElementType ();

			return TypeManager.IsPrimitiveType (t) ||
				TypeManager.IsEnumType (t) ||
				t == TypeManager.string_type ||
				t == TypeManager.object_type ||
				t == TypeManager.type_type;
		}

		// Cache for parameter-less attributes
		static PtrHashtable att_cache = new PtrHashtable ();

		public CustomAttributeBuilder Resolve ()
		{
			if (resolve_error)
				return null;

			resolve_error = true;

			if (Type == null) {
				ResolveAttributeType ();
				if (Type == null)
					return null;
			}

			if (Type.IsAbstract) {
				Report.Error (653, Location, "Cannot apply attribute class `{0}' because it is abstract", GetSignatureForError ());
				return null;
			}

			ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (Type);
			if (obsolete_attr != null) {
				AttributeTester.Report_ObsoleteMessage (obsolete_attr, TypeManager.CSharpName (Type), Location);
			}

			if (Arguments == null) {
				object o = att_cache [Type];
				if (o != null) {
					resolve_error = false;
					return (CustomAttributeBuilder)o;
				}
			}

			ConstructorInfo ctor = ResolveArguments ();
			if (ctor == null) {
				if (Type is TypeBuilder && 
				    TypeManager.LookupDeclSpace (Type).MemberCache == null)
					// The attribute type has been DefineType'd, but not Defined.  Let's not treat it as an error.
					// It'll be resolved again when the attached-to entity is emitted.
					resolve_error = false;
				return null;
			}

			CustomAttributeBuilder cb;

			try {
				if (prop_info_arr != null || field_info_arr != null) {
					cb = new CustomAttributeBuilder (
						ctor, pos_values,
						prop_info_arr, prop_values_arr,
						field_info_arr, field_values_arr);
				} else {
					cb = new CustomAttributeBuilder (
						ctor, pos_values);

					if (pos_values.Length == 0)
						att_cache.Add (Type, cb);
				}
			}
			catch (Exception) {
				Error_AttributeArgumentNotValid (Location);
				return null;
			}

			resolve_error = false;
			return cb;
		}

		protected virtual ConstructorInfo ResolveArguments ()
		{
			// Now we extract the positional and named arguments
			
			ArrayList pos_args = null;
			ArrayList named_args = null;
			int pos_arg_count = 0;
			int named_arg_count = 0;
			
			if (Arguments != null) {
				pos_args = (ArrayList) Arguments [0];
				if (pos_args != null)
					pos_arg_count = pos_args.Count;
				if (Arguments.Count > 1) {
					named_args = (ArrayList) Arguments [1];
					named_arg_count = named_args.Count;
				}
			}

			pos_values = new object [pos_arg_count];

			//
			// First process positional arguments 
			//

			EmitContext ec = new EmitContext (owner.ResolveContext, owner.ResolveContext.DeclContainer, owner.ResolveContext.DeclContainer,
					Location, null, null, owner.ResolveContext.DeclContainer.ModFlags, false);

			int i;
			for (i = 0; i < pos_arg_count; i++) {
				Argument a = (Argument) pos_args [i];
				Expression e;

				if (!a.Resolve (ec, Location))
					return null;

				e = a.Expr;

				object val;
				if (!GetAttributeArgumentExpression (e, Location, a.Type, out val))
					return null;

				pos_values [i] = val;

				if (i == 0 && Type == TypeManager.attribute_usage_type && (int)val == 0) {
					Report.Error (591, Location, "Invalid value for argument to 'System.AttributeUsage' attribute");
					return null;
				}
			}

			//
			// Now process named arguments
			//

			ArrayList field_infos = null;
			ArrayList prop_infos  = null;
			ArrayList field_values = null;
			ArrayList prop_values = null;
			Hashtable seen_names = null;

			if (named_arg_count > 0) {
				field_infos = new ArrayList (4);
				prop_infos  = new ArrayList (4);
				field_values = new ArrayList (4);
				prop_values = new ArrayList (4);

				seen_names = new Hashtable(4);
			}
			
			for (i = 0; i < named_arg_count; i++) {
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
					ec.ContainerType, Type, member_name,
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance,
					Location);

				if (member == null) {
					member = Expression.MemberLookup (ec.ContainerType, Type, member_name,
						MemberTypes.Field | MemberTypes.Property, BindingFlags.NonPublic | BindingFlags.Instance,
						Location);

					if (member != null) {
						Expression.ErrorIsInaccesible (Location, member.GetSignatureForError ());
						return null;
					}
				}

				if (member == null){
					Report.Error (117, Location, "`{0}' does not contain a definition for `{1}'",
						      TypeManager.CSharpName (Type), member_name);
					return null;
				}
				
				if (!(member is PropertyExpr || member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return null;
				}

				e = a.Expr;
				if (e is TypeParameterExpr){
					Error_TypeParameterInAttribute (Location);
					return null;
				}
					
				if (member is PropertyExpr) {
					PropertyExpr pe = (PropertyExpr) member;
					PropertyInfo pi = pe.PropertyInfo;

					if (!pi.CanWrite || !pi.CanRead) {
						Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (!IsValidArgumentType (pi.PropertyType)) {
						Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedAgrumentType (member_name);
						return null;
					}

					object value;
					if (!GetAttributeArgumentExpression (e, Location, pi.PropertyType, out value))
						return null;

					prop_values.Add (value);
					prop_infos.Add (pi);
					
				} else if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (!IsValidArgumentType (fi.FieldType)) {
						Report.SymbolRelatedToPreviousError (fi);
						Error_InvalidNamedAgrumentType (member_name);
						return null;
					}

 					object value;
 					if (!GetAttributeArgumentExpression (e, Location, fi.FieldType, out value))
  						return null;

 					field_values.Add (value);  					
					field_infos.Add (fi);
				}
			}

			Expression mg = Expression.MemberLookup (ec.ContainerType,
				Type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                Location);

			if (mg == null) {
				// FIXME: Punt the issue for now.
				if (Type is TypeBuilder)
					return null;
				throw new InternalErrorException ("Type " + Type + " doesn't have constructors");
			}

			MethodBase constructor = Invocation.OverloadResolve (
				ec, (MethodGroupExpr) mg, pos_args, false, Location);

			if (constructor == null) {
				return null;
			}


			// Here we do the checks which should be done by corlib or by runtime.
			// However Zoltan doesn't like it and every Mono compiler has to do it again.
			
			if (Type == TypeManager.guid_attr_type) {
				try {
					new Guid ((string)pos_values [0]);
				}
				catch (Exception e) {
					Error_AttributeEmitError (e.Message);
					return null;
				}
			}

			if (Type == TypeManager.methodimpl_attr_type &&
				pos_values.Length == 1 && ((Argument)pos_args [0]).Type == TypeManager.short_type &&
				!System.Enum.IsDefined (typeof (MethodImplOptions), pos_values [0])) {
					Error_AttributeEmitError ("Incorrect argument value.");
				return null;
			}

			//
			// Now we perform some checks on the positional args as they
			// cannot be null for a constructor which expects a parameter
			// of type object
			//

			ParameterData pd = TypeManager.GetParameterData (constructor);

			int last_real_param = pd.Count;
			if (pd.HasParams) {
				// When the params is not filled we need to put one
				if (last_real_param > pos_arg_count) {
					object [] new_pos_values = new object [pos_arg_count + 1];
					pos_values.CopyTo (new_pos_values, 0);
					new_pos_values [pos_arg_count] = new object [] {} ;
					pos_values = new_pos_values;
				}
				last_real_param--;
			}

			for (int j = 0; j < pos_arg_count; ++j) {
				Argument a = (Argument) pos_args [j];
				
				if (a.Expr is NullLiteral && pd.ParameterType (j) == TypeManager.object_type) {
					Error_AttributeArgumentNotValid (Location);
					return null;
				}

				object value = pos_values [j];
				if (value != null && a.Type != value.GetType () && TypeManager.IsPrimitiveType (a.Type)) {
					bool fail;
					pos_values [j] = TypeManager.ChangeType (value, a.Type, out fail);
					if (fail) {
						// TODO: Can failed ?
						throw new NotImplementedException ();
					}
				}

				if (j < last_real_param)
					continue;
				
				if (j == last_real_param) {
					object [] array = new object [pos_arg_count - last_real_param];
					array [0] = pos_values [j];
					pos_values [j] = array;
					continue;
				}

				object [] params_array = (object []) pos_values [last_real_param];
				params_array [j - last_real_param] = pos_values [j];
			}

			// Adjust the size of the pos_values if it had params
			if (last_real_param != pos_arg_count) {
				object [] new_pos_values = new object [last_real_param + 1];
				Array.Copy (pos_values, new_pos_values, last_real_param + 1);
				pos_values = new_pos_values;
			}

			if (named_arg_count > 0) {
				prop_info_arr = new PropertyInfo [prop_infos.Count];
				field_info_arr = new FieldInfo [field_infos.Count];
				field_values_arr = new object [field_values.Count];
				prop_values_arr = new object [prop_values.Count];

				field_infos.CopyTo  (field_info_arr, 0);
				field_values.CopyTo (field_values_arr, 0);

				prop_values.CopyTo  (prop_values_arr, 0);
				prop_infos.CopyTo   (prop_info_arr, 0);
			}

			return (ConstructorInfo) constructor;
		}

		/// <summary>
		///   Get a string containing a list of valid targets for the attribute 'attr'
		/// </summary>
		public string GetValidTargets ()
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = GetAttributeUsage ().ValidOn;

			if ((targets & AttributeTargets.Assembly) != 0)
				sb.Append ("assembly, ");

			if ((targets & AttributeTargets.Module) != 0)
				sb.Append ("module, ");

			if ((targets & AttributeTargets.Class) != 0)
				sb.Append ("class, ");

			if ((targets & AttributeTargets.Struct) != 0)
				sb.Append ("struct, ");

			if ((targets & AttributeTargets.Enum) != 0)
				sb.Append ("enum, ");

			if ((targets & AttributeTargets.Constructor) != 0)
				sb.Append ("constructor, ");

			if ((targets & AttributeTargets.Method) != 0)
				sb.Append ("method, ");

			if ((targets & AttributeTargets.Property) != 0)
				sb.Append ("property, indexer, ");

			if ((targets & AttributeTargets.Field) != 0)
				sb.Append ("field, ");

			if ((targets & AttributeTargets.Event) != 0)
				sb.Append ("event, ");

			if ((targets & AttributeTargets.Interface) != 0)
				sb.Append ("interface, ");

			if ((targets & AttributeTargets.Parameter) != 0)
				sb.Append ("parameter, ");

			if ((targets & AttributeTargets.Delegate) != 0)
				sb.Append ("delegate, ");

			if ((targets & AttributeTargets.ReturnValue) != 0)
				sb.Append ("return, ");

			if ((targets & AttributeTargets.GenericParameter) != 0)
				sb.Append ("type parameter, ");
			return sb.Remove (sb.Length - 2, 2).ToString ();
		}

		/// <summary>
		/// Returns AttributeUsage attribute for this type
		/// </summary>
		AttributeUsageAttribute GetAttributeUsage ()
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

			Attribute a = attr_class.OptAttributes == null
				? null
				: attr_class.OptAttributes.Search (TypeManager.attribute_usage_type);

			ua = a == null
				? DefaultUsageAttribute 
				: a.GetAttributeUsageAttribute ();

			usage_attr_cache.Add (Type, ua);
			return ua;
		}

		AttributeUsageAttribute GetAttributeUsageAttribute ()
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return DefaultUsageAttribute;

			AttributeUsageAttribute usage_attribute = new AttributeUsageAttribute ((AttributeTargets)pos_values [0]);

			object field = GetPropertyValue ("AllowMultiple");
			if (field != null)
				usage_attribute.AllowMultiple = (bool)field;

			field = GetPropertyValue ("Inherited");
			if (field != null)
				usage_attribute.Inherited = (bool)field;

			return usage_attribute;
		}

		/// <summary>
		/// Returns custom name of indexer
		/// </summary>
		public string GetIndexerAttributeValue ()
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return null;

			return pos_values [0] as string;
		}

		/// <summary>
		/// Returns condition of ConditionalAttribute
		/// </summary>
		public string GetConditionalAttributeValue ()
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return null;

			return (string)pos_values [0];
		}

		/// <summary>
		/// Creates the instance of ObsoleteAttribute from this attribute instance
		/// </summary>
		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return null;

			if (pos_values == null || pos_values.Length == 0)
				return new ObsoleteAttribute ();

			if (pos_values.Length == 1)
				return new ObsoleteAttribute ((string)pos_values [0]);

			return new ObsoleteAttribute ((string)pos_values [0], (bool)pos_values [1]);
		}

		/// <summary>
		/// Returns value of CLSCompliantAttribute contructor parameter but because the method can be called
		/// before ApplyAttribute. We need to resolve the arguments.
		/// This situation occurs when class deps is differs from Emit order.  
		/// </summary>
		public bool GetClsCompliantAttributeValue ()
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return false;

			return (bool)pos_values [0];
		}

		public Type GetCoClassAttributeValue ()
		{
			if (pos_values == null)
				Resolve ();

			if (resolve_error)
				return null;

			return (Type)pos_values [0];
		}

		public bool CheckTarget ()
		{
			string[] valid_targets = owner.ValidAttributeTargets;
			if (ExplicitTarget == null || ExplicitTarget == valid_targets [0]) {
				Target = owner.AttributeTargets;
				return true;
			}

			// TODO: we can skip the first item
			if (((IList) valid_targets).Contains (ExplicitTarget)) {
				switch (ExplicitTarget) {
					case "return": Target = AttributeTargets.ReturnValue; return true;
					case "param": Target = AttributeTargets.Parameter; return true;
					case "field": Target = AttributeTargets.Field; return true;
					case "method": Target = AttributeTargets.Method; return true;
					case "property": Target = AttributeTargets.Property; return true;
				}
				throw new InternalErrorException ("Unknown explicit target: " + ExplicitTarget);
			}
				
			StringBuilder sb = new StringBuilder ();
			foreach (string s in valid_targets) {
				sb.Append (s);
				sb.Append (", ");
			}
			sb.Remove (sb.Length - 2, 2);
			Report.Error (657, Location, "`{0}' is not a valid attribute location for this declaration. " +
				"Valid attribute locations for this declaration are `{1}'", ExplicitTarget, sb.ToString ());
			return false;
		}

		/// <summary>
		/// Tests permitted SecurityAction for assembly or other types
		/// </summary>
		public bool CheckSecurityActionValidity (bool for_assembly)
		{
			SecurityAction action = GetSecurityActionValue ();

			switch (action) {
			case SecurityAction.Demand:
			case SecurityAction.Assert:
			case SecurityAction.Deny:
			case SecurityAction.PermitOnly:
			case SecurityAction.LinkDemand:
			case SecurityAction.InheritanceDemand:
				if (!for_assembly)
					return true;
				break;

			case SecurityAction.RequestMinimum:
			case SecurityAction.RequestOptional:
			case SecurityAction.RequestRefuse:
				if (for_assembly)
					return true;
				break;

			default:
				Error_AttributeEmitError ("SecurityAction is out of range");
				return false;
			}

			Error_AttributeEmitError (String.Concat ("SecurityAction `", action, "' is not valid for this declaration"));
			return false;
		}

		System.Security.Permissions.SecurityAction GetSecurityActionValue ()
		{
			return (SecurityAction)pos_values [0];
		}

		/// <summary>
		/// Creates instance of SecurityAttribute class and add result of CreatePermission method to permission table.
		/// </summary>
		/// <returns></returns>
		public void ExtractSecurityPermissionSet (ListDictionary permissions)
		{
			Type orig_assembly_type = null;

			if (TypeManager.LookupDeclSpace (Type) != null) {
				if (!RootContext.StdLib) {
					orig_assembly_type = Type.GetType (Type.FullName);
				} else {
					string orig_version_path = Environment.GetEnvironmentVariable ("__SECURITY_BOOTSTRAP_DB");
					if (orig_version_path == null) {
						Error_AttributeEmitError ("security custom attributes can not be referenced from defining assembly");
						return;
					}

					if (orig_sec_assembly == null) {
						string file = Path.Combine (orig_version_path, Driver.OutputFile);
						orig_sec_assembly = Assembly.LoadFile (file);
					}

					orig_assembly_type = orig_sec_assembly.GetType (Type.FullName, true);
					if (orig_assembly_type == null) {
						Report.Warning (-112, 1, Location, "Self-referenced security attribute `{0}' " +
								"was not found in previous version of assembly");
						return;
					}
				}
			}

			SecurityAttribute sa;
			// For all non-selfreferencing security attributes we can avoid all hacks
			if (orig_assembly_type == null) {
				sa = (SecurityAttribute) Activator.CreateInstance (Type, pos_values);

				if (prop_info_arr != null) {
					for (int i = 0; i < prop_info_arr.Length; ++i) {
						PropertyInfo pi = prop_info_arr [i];
						pi.SetValue (sa, prop_values_arr [i], null);
					}
				}
			} else {
				// HACK: All security attributes have same ctor syntax
				sa = (SecurityAttribute) Activator.CreateInstance (orig_assembly_type, new object[] { GetSecurityActionValue () } );

				// All types are from newly created assembly but for invocation with old one we need to convert them
				if (prop_info_arr != null) {
					for (int i = 0; i < prop_info_arr.Length; ++i) {
						PropertyInfo emited_pi = prop_info_arr [i];
						PropertyInfo pi = orig_assembly_type.GetProperty (emited_pi.Name, emited_pi.PropertyType);

						object old_instance = pi.PropertyType.IsEnum ?
							System.Enum.ToObject (pi.PropertyType, prop_values_arr [i]) :
							prop_values_arr [i];

						pi.SetValue (sa, old_instance, null);
					}
				}
			}

			IPermission perm;
			perm = sa.CreatePermission ();
			SecurityAction action = GetSecurityActionValue ();

			// IS is correct because for corlib we are using an instance from old corlib
			if (!(perm is System.Security.CodeAccessPermission)) {
				switch (action) {
					case SecurityAction.Demand:
						action = (SecurityAction)13;
						break;
					case SecurityAction.LinkDemand:
						action = (SecurityAction)14;
						break;
					case SecurityAction.InheritanceDemand:
						action = (SecurityAction)15;
						break;
				}
			}

			PermissionSet ps = (PermissionSet)permissions [action];
			if (ps == null) {
				if (sa is PermissionSetAttribute)
					ps = new PermissionSet (sa.Unrestricted ? PermissionState.Unrestricted : PermissionState.None);
				else
					ps = new PermissionSet (PermissionState.None);

				permissions.Add (action, ps);
			} else if (!ps.IsUnrestricted () && (sa is PermissionSetAttribute) && sa.Unrestricted) {
				ps = ps.Union (new PermissionSet (PermissionState.Unrestricted));
				permissions [action] = ps;
			}
			ps.AddPermission (perm);
		}

		object GetValue (object value)
		{
			if (value is EnumConstant)
				return ((EnumConstant) value).GetValue ();
			else
				return value;				
		}

		public object GetPropertyValue (string name)
		{
			if (prop_info_arr == null)
				return null;

			for (int i = 0; i < prop_info_arr.Length; ++i) {
				if (prop_info_arr [i].Name == name)
					return prop_values_arr [i];
			}

			return null;
		}

		object GetFieldValue (string name)
		{
			int i;
			if (field_info_arr == null)
				return null;
			i = 0;
			foreach (FieldInfo fi in field_info_arr) {
				if (fi.Name == name)
					return GetValue (field_values_arr [i]);
				i++;
			}
			return null;
		}

		//
		// Theoretically, we can get rid of this, since FieldBuilder.SetCustomAttribute()
		// and ParameterBuilder.SetCustomAttribute() are supposed to handle this attribute.
		// However, we can't, since it appears that the .NET 1.1 SRE hangs when given a MarshalAsAttribute.
		//
		public UnmanagedMarshal GetMarshal (Attributable attr)
		{
			UnmanagedType UnmanagedType;
			if (!RootContext.StdLib || pos_values [0].GetType () != typeof (UnmanagedType))
				UnmanagedType = (UnmanagedType) System.Enum.ToObject (typeof (UnmanagedType), pos_values [0]);
			else
				UnmanagedType = (UnmanagedType) pos_values [0];

			object value = GetFieldValue ("SizeParamIndex");
			if (value != null && UnmanagedType != UnmanagedType.LPArray) {
				Error_AttributeEmitError ("SizeParamIndex field is not valid for the specified unmanaged type");
				return null;
			}

			object o = GetFieldValue ("ArraySubType");
			UnmanagedType array_sub_type = o == null ? (UnmanagedType) 0x50 /* NATIVE_MAX */ : (UnmanagedType) o;

			switch (UnmanagedType) {
			case UnmanagedType.CustomMarshaler: {
				MethodInfo define_custom = typeof (UnmanagedMarshal).GetMethod ("DefineCustom",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (define_custom == null) {
					Report.RuntimeMissingSupport (Location, "set marshal info");
					return null;
				}
				
				object [] args = new object [4];
				args [0] = GetFieldValue ("MarshalTypeRef");
				args [1] = GetFieldValue ("MarshalCookie");
				args [2] = GetFieldValue ("MarshalType");
				args [3] = Guid.Empty;
				return (UnmanagedMarshal) define_custom.Invoke (null, args);
			}
			case UnmanagedType.LPArray: {
				object size_const = GetFieldValue ("SizeConst");
				object size_param_index = GetFieldValue ("SizeParamIndex");

				if ((size_const != null) || (size_param_index != null)) {
					MethodInfo define_array = typeof (UnmanagedMarshal).GetMethod ("DefineLPArrayInternal",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (define_array == null) {
						Report.RuntimeMissingSupport (Location, "set marshal info");
						return null;
					}
				
					object [] args = new object [3];
					args [0] = array_sub_type;
					args [1] = size_const == null ? -1 : size_const;
					args [2] = size_param_index == null ? -1 : size_param_index;
					return (UnmanagedMarshal) define_array.Invoke (null, args);
				}
				else
					return UnmanagedMarshal.DefineLPArray (array_sub_type);
			}
			case UnmanagedType.SafeArray:
				return UnmanagedMarshal.DefineSafeArray (array_sub_type);

			case UnmanagedType.ByValArray:
				FieldMember fm = attr as FieldMember;
				if (fm == null) {
					Error_AttributeEmitError ("Specified unmanaged type is only valid on fields");
					return null;
				}
				return UnmanagedMarshal.DefineByValArray ((int) GetFieldValue ("SizeConst"));

			case UnmanagedType.ByValTStr:
				return UnmanagedMarshal.DefineByValTStr ((int) GetFieldValue ("SizeConst"));

			default:
				return UnmanagedMarshal.DefineUnmanagedMarshal (UnmanagedType);
			}
		}

		public CharSet GetCharSetValue ()
		{
			return (CharSet)System.Enum.Parse (typeof (CharSet), pos_values [0].ToString ());
		}

		public MethodImplOptions GetMethodImplOptions ()
		{
			if (pos_values [0].GetType () != typeof (MethodImplOptions))
				return (MethodImplOptions)System.Enum.ToObject (typeof (MethodImplOptions), pos_values [0]);
			return (MethodImplOptions)pos_values [0];
		}

		public LayoutKind GetLayoutKindValue ()
		{
			if (!RootContext.StdLib || pos_values [0].GetType () != typeof (LayoutKind))
				return (LayoutKind)System.Enum.ToObject (typeof (LayoutKind), pos_values [0]);

			return (LayoutKind)pos_values [0];
		}

		public object GetParameterDefaultValue ()
		{
			return pos_values [0];
		}

		/// <summary>
		/// Emit attribute for Attributable symbol
		/// </summary>
		public void Emit (ListDictionary emitted_attr)
		{
			CustomAttributeBuilder cb = Resolve ();
			if (cb == null)
				return;

			AttributeUsageAttribute usage_attr = GetAttributeUsage ();
			if ((usage_attr.ValidOn & Target) == 0) {
				Report.Error (592, Location, "Attribute `{0}' is not valid on this declaration type. " +
					      "It is valid on `{1}' declarations only",
					GetSignatureForError (), GetValidTargets ());
				return;
			}

			try {
				owner.ApplyAttributeBuilder (this, cb);
			}
			catch (Exception e) {
				Error_AttributeEmitError (e.Message);
				return;
			}

			if (!usage_attr.AllowMultiple) {
				ArrayList emitted_targets = (ArrayList)emitted_attr [Type];
				if (emitted_targets == null) {
					emitted_targets = new ArrayList ();
					emitted_attr.Add (Type, emitted_targets);
				} else if (emitted_targets.Contains (Target)) {
					Report.Error (579, Location, "Duplicate `{0}' attribute", GetSignatureForError ());
					return;
				}
				emitted_targets.Add (Target);
			}

			if (!RootContext.VerifyClsCompliance)
				return;

			// Here we are testing attribute arguments for array usage (error 3016)
			if (owner.IsClsComplianceRequired ()) {
				if (Arguments == null)
					return;

				ArrayList pos_args = (ArrayList) Arguments [0];
				if (pos_args != null) {
					foreach (Argument arg in pos_args) { 
						// Type is undefined (was error 246)
						if (arg.Type == null)
							return;

						if (arg.Type.IsArray) {
							Report.Error (3016, Location, "Arrays as attribute arguments are not CLS-compliant");
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
						Report.Error (3016, Location, "Arrays as attribute arguments are not CLS-compliant");
						return;
					}
				}
			}
		}
		
		public MethodBuilder DefinePInvokeMethod (TypeBuilder builder, string name,
							  MethodAttributes flags, Type ret_type, Type [] param_types)
		{
			if (pos_values == null)
				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve ();

			if (resolve_error)
				return null;
			
			string dll_name = (string)pos_values [0];

			// Default settings
			CallingConvention cc = CallingConvention.Winapi;
			CharSet charset = CodeGen.Module.DefaultCharSet;
			bool preserve_sig = true;
			string entry_point = name;
			bool best_fit_mapping = false;
			bool throw_on_unmappable = false;
			bool exact_spelling = false;
			bool set_last_error = false;

			bool best_fit_mapping_set = false;
			bool throw_on_unmappable_set = false;
			bool exact_spelling_set = false;
			bool set_last_error_set = false;

			MethodInfo set_best_fit = null;
			MethodInfo set_throw_on = null;
			MethodInfo set_exact_spelling = null;
			MethodInfo set_set_last_error = null;

			if (field_info_arr != null) {
				
				for (int i = 0; i < field_info_arr.Length; i++) {
					switch (field_info_arr [i].Name) {
					case "BestFitMapping":
						best_fit_mapping = (bool) field_values_arr [i];
						best_fit_mapping_set = true;
						break;
					case "CallingConvention":
						cc = (CallingConvention) field_values_arr [i];
						break;
					case "CharSet":
						charset = (CharSet) field_values_arr [i];
						break;
					case "EntryPoint":
						entry_point = (string) field_values_arr [i];
						break;
					case "ExactSpelling":
						exact_spelling = (bool) field_values_arr [i];
						exact_spelling_set = true;
						break;
					case "PreserveSig":
						preserve_sig = (bool) field_values_arr [i];
						break;
					case "SetLastError":
						set_last_error = (bool) field_values_arr [i];
						set_last_error_set = true;
						break;
					case "ThrowOnUnmappableChar":
						throw_on_unmappable = (bool) field_values_arr [i];
						throw_on_unmappable_set = true;
						break;
					default: 
						throw new InternalErrorException (field_info_arr [i].ToString ());
					}
				}
			}
			
			if (throw_on_unmappable_set || best_fit_mapping_set || exact_spelling_set || set_last_error_set) {
				set_best_fit = typeof (MethodBuilder).
					GetMethod ("set_BestFitMapping", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				set_throw_on = typeof (MethodBuilder).
					GetMethod ("set_ThrowOnUnmappableChar", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				set_exact_spelling = typeof (MethodBuilder).
					GetMethod ("set_ExactSpelling", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				set_set_last_error = typeof (MethodBuilder).
					GetMethod ("set_SetLastError", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				if ((set_best_fit == null) || (set_throw_on == null) ||
				    (set_exact_spelling == null) || (set_set_last_error == null)) {
					Report.Error (-1, Location,
								  "The ThrowOnUnmappableChar, BestFitMapping, SetLastError, " +
						      "and ExactSpelling attributes can only be emitted when running on the mono runtime.");
					return null;
				}
			}

			try {
				MethodBuilder mb = builder.DefinePInvokeMethod (
					name, dll_name, entry_point, flags | MethodAttributes.HideBySig | MethodAttributes.PinvokeImpl,
					CallingConventions.Standard, ret_type, param_types, cc, charset);

				if (preserve_sig)
					mb.SetImplementationFlags (MethodImplAttributes.PreserveSig);

				if (throw_on_unmappable_set)
					set_throw_on.Invoke (mb, 0, null, new object [] { throw_on_unmappable }, null);
				if (best_fit_mapping_set)
					set_best_fit.Invoke (mb, 0, null, new object [] { best_fit_mapping }, null);
				if (exact_spelling_set)
					set_exact_spelling.Invoke  (mb, 0, null, new object [] { exact_spelling }, null);
				if (set_last_error_set)
					set_set_last_error.Invoke  (mb, 0, null, new object [] { set_last_error }, null);
			
				return mb;
			}
			catch (ArgumentException e) {
				Error_AttributeEmitError (e.Message);
				return null;
			}
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
	public class GlobalAttribute : Attribute
	{
		public readonly NamespaceEntry ns;

		public GlobalAttribute (NamespaceEntry ns, string target, 
					Expression left_expr, string identifier, ArrayList args, Location loc, bool nameEscaped):
			base (target, left_expr, identifier, args, loc, nameEscaped)
		{
			this.ns = ns;
		}

		void Enter ()
		{
			// RootContext.Tree.Types has a single NamespaceEntry which gets overwritten
			// each time a new file is parsed.  However, we need to use the NamespaceEntry
			// in effect where the attribute was used.  Since code elsewhere cannot assume
			// that the NamespaceEntry is right, just overwrite it.
			//
			// Precondition: RootContext.Tree.Types == null

			if (RootContext.Tree.Types.NamespaceEntry != null)
				throw new InternalErrorException (Location + " non-null NamespaceEntry");

			RootContext.Tree.Types.NamespaceEntry = ns;
		}

		void Leave ()
		{
			RootContext.Tree.Types.NamespaceEntry = null;
		}

		protected override FullNamedExpression ResolveAsTypeStep (Expression expr, EmitContext ec, bool silent)
		{
			try {
				Enter ();
				return base.ResolveAsTypeStep (expr, ec, silent);
			}
			finally {
				Leave ();
			}
		}


		protected override FullNamedExpression ResolveAsTypeTerminal (Expression expr, EmitContext ec, bool silent)
		{
			try {
				Enter ();
				return base.ResolveAsTypeTerminal (expr, ec, silent);
			}
			finally {
				Leave ();
			}
		}

		protected override ConstructorInfo ResolveArguments ()
		{
			try {
				Enter ();
				return base.ResolveArguments ();
			}
			finally {
				Leave ();
			}
		}
	}

	public class Attributes {
		public readonly ArrayList Attrs;

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

		public void AttachTo (Attributable attributable)
		{
			foreach (Attribute a in Attrs)
				a.AttachTo (attributable);
		}

		/// <summary>
		/// Checks whether attribute target is valid for the current element
		/// </summary>
		public bool CheckTargets ()
		{
			foreach (Attribute a in Attrs) {
				if (!a.CheckTarget ())
					return false;
			}
			return true;
		}

		public Attribute Search (Type t)
		{
			foreach (Attribute a in Attrs) {
				if (a.ResolveType () == t)
					return a;
			}
			return null;
		}

		/// <summary>
		/// Returns all attributes of type 't'. Use it when attribute is AllowMultiple = true
		/// </summary>
		public Attribute[] SearchMulti (Type t)
		{
			ArrayList ar = null;

			foreach (Attribute a in Attrs) {
				if (a.ResolveType () == t) {
					if (ar == null)
						ar = new ArrayList ();
					ar.Add (a);
				}
			}

			return ar == null ? null : ar.ToArray (typeof (Attribute)) as Attribute[];
		}

		public void Emit ()
		{
			CheckTargets ();

			ListDictionary ld = new ListDictionary ();

			foreach (Attribute a in Attrs)
				a.Emit (ld);
		}

		public bool Contains (Type t)
		{
			return Search (t) != null;
		}
	}

	/// <summary>
	/// Helper class for attribute verification routine.
	/// </summary>
	sealed class AttributeTester
	{
		static PtrHashtable analyzed_types = new PtrHashtable ();
		static PtrHashtable analyzed_types_obsolete = new PtrHashtable ();
		static PtrHashtable analyzed_member_obsolete = new PtrHashtable ();
		static PtrHashtable analyzed_method_excluded = new PtrHashtable ();

		static PtrHashtable fixed_buffer_cache = new PtrHashtable ();

		static object TRUE = new object ();
		static object FALSE = new object ();

		private AttributeTester ()
		{
		}

		public enum Result {
			Ok,
			RefOutArrayError,
			ArrayArrayError
		}

		/// <summary>
		/// Returns true if parameters of two compared methods are CLS-Compliant.
		/// It tests differing only in ref or out, or in array rank.
		/// </summary>
		public static Result AreOverloadedMethodParamsClsCompliant (Type[] types_a, Type[] types_b) 
		{
			if (types_a == null || types_b == null)
				return Result.Ok;

			if (types_a.Length != types_b.Length)
				return Result.Ok;

			Result result = Result.Ok;
			for (int i = 0; i < types_b.Length; ++i) {
				Type aType = types_a [i];
				Type bType = types_b [i];

				if (aType.IsArray && bType.IsArray) {
					Type a_el_type = aType.GetElementType ();
					Type b_el_type = bType.GetElementType ();
					if (aType.GetArrayRank () != bType.GetArrayRank () && a_el_type == b_el_type) {
						result = Result.RefOutArrayError;
						continue;
					}

					if (a_el_type.IsArray || b_el_type.IsArray) {
						result = Result.ArrayArrayError;
						continue;
					}
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
					return Result.Ok;

				if (is_either_ref_or_out)
					result = Result.RefOutArrayError;
			}
			return result;
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
        
		/// <summary>
		/// Returns IFixedBuffer implementation if field is fixed buffer else null.
		/// </summary>
		public static IFixedBuffer GetFixedBuffer (FieldInfo fi)
		{
			FieldBase fb = TypeManager.GetField (fi);
			if (fb != null) {
				return fb as IFixedBuffer;
			}

			object o = fixed_buffer_cache [fi];
			if (o == null) {
				if (System.Attribute.GetCustomAttribute (fi, TypeManager.fixed_buffer_attr_type) == null) {
					fixed_buffer_cache.Add (fi, FALSE);
					return null;
				}
				
				IFixedBuffer iff = new FixedFieldExternal (fi);
				fixed_buffer_cache.Add (fi, iff);
				return iff;
			}

			if (o == FALSE)
				return null;

			return (IFixedBuffer)o;
		}

		public static void VerifyModulesClsCompliance ()
		{
			Module[] modules = RootNamespace.Global.Modules;
			if (modules == null)
				return;

			// The first module is generated assembly
			for (int i = 1; i < modules.Length; ++i) {
				Module module = modules [i];
				if (!IsClsCompliant (module)) {
					Report.Error (3013, "Added modules must be marked with the CLSCompliant attribute " +
						      "to match the assembly", module.Name);
					return;
				}
			}
		}

		public static Type GetImportedIgnoreCaseClsType (string name)
		{
			foreach (Assembly a in RootNamespace.Global.Assemblies) {
				Type t = a.GetType (name, false, true);
				if (t == null)
					continue;

				if (IsClsCompliant (t))
					return t;
			}
			return null;
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
			type = TypeManager.DropGenericTypeArguments (type);
			DeclSpace ds = TypeManager.LookupDeclSpace (type);
			if (ds != null) {
				return ds.IsClsComplianceRequired ();
			}

			if (type.IsGenericParameter)
				return true;

			object[] CompliantAttribute = type.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (CompliantAttribute.Length == 0) 
				return IsClsCompliant (type.Assembly);

			return ((CLSCompliantAttribute)CompliantAttribute[0]).IsCompliant;
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute when type is obsolete
		/// </summary>
		public static ObsoleteAttribute GetObsoleteAttribute (Type type)
		{
			object type_obsolete = analyzed_types_obsolete [type];
			if (type_obsolete == FALSE)
				return null;

			if (type_obsolete != null)
				return (ObsoleteAttribute)type_obsolete;

			ObsoleteAttribute result = null;
			if (type.IsByRef || type.IsArray || type.IsPointer) {
				result = GetObsoleteAttribute (TypeManager.GetElementType (type));
			} else if (type.IsGenericParameter || type.IsGenericType)
				return null;
			else {
				DeclSpace type_ds = TypeManager.LookupDeclSpace (type);

				// Type is external, we can get attribute directly
				if (type_ds == null) {
					object[] attribute = type.GetCustomAttributes (TypeManager.obsolete_attribute_type, false);
					if (attribute.Length == 1)
						result = (ObsoleteAttribute)attribute [0];
				} else {
					// Is null during corlib bootstrap
					if (TypeManager.obsolete_attribute_type != null)
						result = type_ds.GetObsoleteAttribute ();
				}
			}

			// Cannot use .Add because of corlib bootstrap
			analyzed_types_obsolete [type] = result == null ? FALSE : result;
			return result;
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute when method is obsolete
		/// </summary>
		public static ObsoleteAttribute GetMethodObsoleteAttribute (MethodBase mb)
		{
			IMethodData mc = TypeManager.GetMethod (mb);
			if (mc != null) 
				return mc.GetObsoleteAttribute ();

			// compiler generated methods are not registered by AddMethod
			if (mb.DeclaringType is TypeBuilder)
				return null;

			if (mb.IsSpecialName) {
				PropertyInfo pi = PropertyExpr.AccessorTable [mb] as PropertyInfo;
				if (pi != null) {
					// FIXME: This is buggy as properties from this assembly are included as well
					return null;
					//return GetMemberObsoleteAttribute (pi);
				}
			}

			return GetMemberObsoleteAttribute (mb);
		}

		/// <summary>
		/// Returns instance of ObsoleteAttribute when member is obsolete
		/// </summary>
		public static ObsoleteAttribute GetMemberObsoleteAttribute (MemberInfo mi)
		{
			object type_obsolete = analyzed_member_obsolete [mi];
			if (type_obsolete == FALSE)
				return null;

			if (type_obsolete != null)
				return (ObsoleteAttribute)type_obsolete;

			if ((mi.DeclaringType is TypeBuilder) || mi.DeclaringType.IsGenericType)
				return null;

			ObsoleteAttribute oa = System.Attribute.GetCustomAttribute (mi, TypeManager.obsolete_attribute_type, false)
				as ObsoleteAttribute;
			analyzed_member_obsolete.Add (mi, oa == null ? FALSE : oa);
			return oa;
		}

		/// <summary>
		/// Common method for Obsolete error/warning reporting.
		/// </summary>
		public static void Report_ObsoleteMessage (ObsoleteAttribute oa, string member, Location loc)
		{
			if (oa.IsError) {
				Report.Error (619, loc, "`{0}' is obsolete: `{1}'", member, oa.Message);
				return;
			}

			if (oa.Message == null) {
				Report.Warning (612, 1, loc, "`{0}' is obsolete", member);
				return;
			}
			Report.Warning (618, 2, loc, "`{0}' is obsolete: `{1}'", member, oa.Message);
		}

		public static bool IsConditionalMethodExcluded (MethodBase mb)
		{
			mb = TypeManager.DropGenericMethodArguments (mb);
			if ((mb is MethodBuilder) || (mb is ConstructorBuilder))
				return false;

			object excluded = analyzed_method_excluded [mb];
			if (excluded != null)
				return excluded == TRUE ? true : false;

			ConditionalAttribute[] attrs = mb.GetCustomAttributes (TypeManager.conditional_attribute_type, true)
				as ConditionalAttribute[];
			if (attrs.Length == 0) {
				analyzed_method_excluded.Add (mb, FALSE);
				return false;
			}

			foreach (ConditionalAttribute a in attrs) {
				if (RootContext.AllDefines.Contains (a.ConditionString)) {
					analyzed_method_excluded.Add (mb, FALSE);
					return false;
				}
			}
			analyzed_method_excluded.Add (mb, TRUE);
			return true;
		}

		/// <summary>
		/// Analyzes class whether it has attribute which has ConditionalAttribute
		/// and its condition is not defined.
		/// </summary>
		public static bool IsAttributeExcluded (Type type)
		{
			if (!type.IsClass)
				return false;

			Class class_decl = TypeManager.LookupDeclSpace (type) as Class;

			// TODO: add caching
			// TODO: merge all Type bases attribute caching to one cache to save memory
			if (class_decl == null) {
				object[] attributes = type.GetCustomAttributes (TypeManager.conditional_attribute_type, false);
				foreach (ConditionalAttribute ca in attributes) {
					if (RootContext.AllDefines.Contains (ca.ConditionString))
						return false;
				}
				return attributes.Length > 0;
			}

			return class_decl.IsExcluded ();
		}

		public static Type GetCoClassAttribute (Type type)
		{
			TypeContainer tc = TypeManager.LookupInterface (type);
			if (tc == null) {
				object[] o = type.GetCustomAttributes (TypeManager.coclass_attr_type, false);
				return ((System.Runtime.InteropServices.CoClassAttribute)o[0]).CoClass;
			}

			if (tc.OptAttributes == null)
				return null;

			Attribute a = tc.OptAttributes.Search (TypeManager.coclass_attr_type);
			if (a == null)
				return null;

			return a.GetCoClassAttributeValue ();
		}
	}
}
