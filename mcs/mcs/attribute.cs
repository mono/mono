//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//         Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc.
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
 		protected Attributes attributes;

		public Attributable (Attributes attrs)
		{
			if (attrs != null)
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

	public class Attribute : Expression
	{
		public readonly string ExplicitTarget;
		public AttributeTargets Target;

		// TODO: remove this member
		public readonly string    Name;
		public readonly Expression LeftExpr;
		public readonly string Identifier;

		ArrayList PosArguments;
		ArrayList NamedArguments;

		bool resolve_error;
		readonly bool nameEscaped;

		// It can contain more onwers when the attribute is applied to multiple fiels.
		protected Attributable[] owners;

		static readonly AttributeUsageAttribute DefaultUsageAttribute = new AttributeUsageAttribute (AttributeTargets.All);
		static Assembly orig_sec_assembly;
		public static readonly object[] EmptyObject = new object [0];

		// non-null if named args present after Resolve () is called
		PropertyInfo [] prop_info_arr;
		FieldInfo [] field_info_arr;
		object [] field_values_arr;
		object [] prop_values_arr;
		object [] pos_values;

		static PtrHashtable usage_attr_cache;
		// Cache for parameter-less attributes
		static PtrHashtable att_cache;
		
		public Attribute (string target, Expression left_expr, string identifier, object[] args, Location loc, bool nameEscaped)
		{
			LeftExpr = left_expr;
			Identifier = identifier;
			Name = LeftExpr == null ? identifier : LeftExpr + "." + identifier;
			if (args != null) {
				PosArguments = (ArrayList)args [0];
				NamedArguments = (ArrayList)args [1];				
			}
			this.loc = loc;
			ExplicitTarget = target;
			this.nameEscaped = nameEscaped;
		}

		static Attribute ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			usage_attr_cache = new PtrHashtable ();
			att_cache = new PtrHashtable ();
		}

		public virtual void AttachTo (Attributable owner)
		{
			if (this.owners == null) {
				this.owners = new Attributable[1] { owner };
				return;
			}

			// When the same attribute is attached to multiple fiels
			// we use this extra_owners as a list of owners. The attribute
			// then can be removed because will be emitted when first owner
			// is served
			Attributable[] new_array = new Attributable [this.owners.Length + 1];
			owners.CopyTo (new_array, 0);
			new_array [owners.Length] = owner;
			this.owners = new_array;
			owner.OptAttributes = null;
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

		public static void Error_AttributeArgumentNotValid (Location loc)
		{
			Report.Error (182, loc,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
		}
		
		static void Error_TypeParameterInAttribute (Location loc)
		{
			Report.Error (
				-202, loc, "Can not use a type parameter in an attribute");
		}

		public void Error_MissingGuidAttribute ()
		{
			Report.Error (596, Location, "The Guid attribute must be specified with the ComImport attribute");
		}

		public void Error_MisusedExtensionAttribute ()
		{
			Report.Error (1112, Location, "Do not use `{0}' directly. Use parameter modifier `this' instead", GetSignatureForError ());
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

		Attributable Owner {
			get {
				return owners [0];
			}
		}

		protected virtual TypeExpr ResolveAsTypeTerminal (Expression expr, IResolveContext ec, bool silent)
		{
			return expr.ResolveAsTypeTerminal (ec, silent);
		}

		Type ResolvePossibleAttributeType (string name, bool silent, ref bool is_attr)
		{
			IResolveContext rc = Owner.ResolveContext;

			TypeExpr te;
			if (LeftExpr == null) {
				te = ResolveAsTypeTerminal (new SimpleName (name, Location), rc, silent);
			} else {
				te = ResolveAsTypeTerminal (new MemberAccess (LeftExpr, name), rc, silent);
			}

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

		public override string GetSignatureForError ()
		{
			if (Type != null)
				return TypeManager.CSharpName (Type);

			return LeftExpr == null ? Identifier : LeftExpr.GetSignatureForError () + "." + Identifier;
		}

		public bool HasSecurityAttribute {
			get {
				return TypeManager.security_attr_type != null &&
				TypeManager.IsSubclassOf (type, TypeManager.security_attr_type);
			}
		}

		public bool IsValidSecurityAttribute ()
		{
			return HasSecurityAttribute && IsSecurityActionValid (false);
		}

		static bool IsValidArgumentType (Type t)
		{
			if (t.IsArray)
				t = TypeManager.GetElementType (t);

			return t == TypeManager.string_type ||
				TypeManager.IsPrimitiveType (t) ||
				TypeManager.IsEnumType (t) ||
				t == TypeManager.object_type ||
				t == TypeManager.type_type;
		}

		[Conditional ("GMCS_SOURCE")]
		void ApplyModuleCharSet ()
		{
			if (Type != TypeManager.dllimport_type)
				return;

			if (!CodeGen.Module.HasDefaultCharSet)
				return;

			const string CharSetEnumMember = "CharSet";
			if (NamedArguments == null) {
				NamedArguments = new ArrayList (1);
			} else {
				foreach (DictionaryEntry de in NamedArguments) {
					if ((string)de.Key == CharSetEnumMember)
						return;
				}
			}
			
			NamedArguments.Add (new DictionaryEntry (CharSetEnumMember,
				new Argument (Constant.CreateConstant (typeof (CharSet), CodeGen.Module.DefaultCharSet, Location))));
 		}

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

			if (PosArguments == null && NamedArguments == null) {
				object o = att_cache [Type];
				if (o != null) {
					resolve_error = false;
					return (CustomAttributeBuilder)o;
				}
			}

			Attributable owner = Owner;
			DeclSpace ds = owner.ResolveContext as DeclSpace;
			if (ds == null)
				ds = owner.ResolveContext.DeclContainer;
			
			EmitContext ec = new EmitContext (owner.ResolveContext, ds, owner.ResolveContext.DeclContainer,
				Location, null, typeof (Attribute), owner.ResolveContext.DeclContainer.ModFlags, false);
			ec.IsAnonymousMethodAllowed = false;

			ConstructorInfo ctor = ResolveConstructor (ec);
			if (ctor == null) {
				if (Type is TypeBuilder && 
				    TypeManager.LookupDeclSpace (Type).MemberCache == null)
					// The attribute type has been DefineType'd, but not Defined.  Let's not treat it as an error.
					// It'll be resolved again when the attached-to entity is emitted.
					resolve_error = false;
				return null;
			}

			ApplyModuleCharSet ();

			CustomAttributeBuilder cb;
			try {
				// SRE does not allow private ctor but we want to report all source code errors
				if (ctor.IsPrivate)
					return null;

				if (NamedArguments == null) {
					cb = new CustomAttributeBuilder (ctor, pos_values);

					if (pos_values.Length == 0)
						att_cache.Add (Type, cb);

					resolve_error = false;
					return cb;
				}

				if (!ResolveNamedArguments (ec)) {
					return null;
				}

				cb = new CustomAttributeBuilder (ctor, pos_values,
						prop_info_arr, prop_values_arr,
						field_info_arr, field_values_arr);

				resolve_error = false;
				return cb;
			}
			catch (Exception) {
				Error_AttributeArgumentNotValid (Location);
				return null;
			}
		}

		protected virtual ConstructorInfo ResolveConstructor (EmitContext ec)
		{
			if (PosArguments != null) {
				for (int i = 0; i < PosArguments.Count; i++) {
					Argument a = (Argument) PosArguments [i];

					if (!a.Resolve (ec, Location))
						return null;
				}
			}
			
			MethodGroupExpr mg = MemberLookupFinal (ec, ec.ContainerType,
				Type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Location) as MethodGroupExpr;

			if (mg == null)
				return null;

			mg = mg.OverloadResolve (ec, ref PosArguments, false, Location);
			if (mg == null)
				return null;
			
			ConstructorInfo constructor = (ConstructorInfo)mg;

			// TODO: move to OverloadResolve
			ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (constructor);
			if (oa != null && !Owner.ResolveContext.IsInObsoleteScope) {
				AttributeTester.Report_ObsoleteMessage (oa, mg.GetSignatureForError (), mg.Location);
			}

			if (PosArguments == null) {
				pos_values = EmptyObject;
				return constructor;
			}

			ParameterData pd = TypeManager.GetParameterData (constructor);

			int pos_arg_count = PosArguments.Count;
			pos_values = new object [pos_arg_count];
			for (int j = 0; j < pos_arg_count; ++j) {
				Argument a = (Argument) PosArguments [j];

				if (!a.Expr.GetAttributableValue (a.Type, out pos_values [j]))
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

			if (Type == TypeManager.attribute_usage_type && (int)pos_values [0] == 0) {
				Report.Error (591, Location, "Invalid value for argument to `System.AttributeUsage' attribute");
				return null;
			}

			if (Type == TypeManager.indexer_name_type || Type == TypeManager.conditional_attribute_type) {
				if (!Tokenizer.IsValidIdentifier ((string)pos_values [0])) {
					Report.Error (633, ((Argument)PosArguments[0]).Expr.Location,
						"The argument to the `{0}' attribute must be a valid identifier", GetSignatureForError ());
					return null;
				}
			}

			if (Type == TypeManager.methodimpl_attr_type && pos_values.Length == 1 &&
				pd.ParameterType (0) == TypeManager.short_type &&
				!System.Enum.IsDefined (typeof (MethodImplOptions), pos_values [0].ToString ())) {
				Error_AttributeEmitError ("Incorrect argument value.");
				return null;
			}

			return constructor;
		}

		protected virtual bool ResolveNamedArguments (EmitContext ec)
		{
			int named_arg_count = NamedArguments.Count;

			ArrayList field_infos = new ArrayList (named_arg_count);
			ArrayList prop_infos  = new ArrayList (named_arg_count);
			ArrayList field_values = new ArrayList (named_arg_count);
			ArrayList prop_values = new ArrayList (named_arg_count);

			ArrayList seen_names = new ArrayList(named_arg_count);
			
			foreach (DictionaryEntry de in NamedArguments) {
				string member_name = (string) de.Key;

				if (seen_names.Contains(member_name)) {
					Report.Error(643, Location, "'" + member_name + "' duplicate named attribute argument");
					return false;
				}				
				seen_names.Add(member_name);

				Argument a = (Argument) de.Value;
				if (!a.Resolve (ec, Location))
					return false;

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
						Report.SymbolRelatedToPreviousError (member.Type);
						Expression.ErrorIsInaccesible (Location, member.GetSignatureForError ());
						return false;
					}
				}

				if (member == null){
					Expression.Error_TypeDoesNotContainDefinition (Location, Type, member_name);
					return false;
				}
				
				if (!(member is PropertyExpr || member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return false;
				}

				if (a.Expr is TypeParameterExpr){
					Error_TypeParameterInAttribute (Location);
					return false;
				}

				ObsoleteAttribute obsolete_attr;

				if (member is PropertyExpr) {
					PropertyInfo pi = ((PropertyExpr) member).PropertyInfo;

					if (!pi.CanWrite || !pi.CanRead) {
						Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedArgument (member_name);
						return false;
					}

					if (!IsValidArgumentType (member.Type)) {
						Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedAgrumentType (member_name);
						return false;
					}

					object value;
					if (!a.Expr.GetAttributableValue (member.Type, out value))
						return false;

					PropertyBase pb = TypeManager.GetProperty (pi);
					if (pb != null)
						obsolete_attr = pb.GetObsoleteAttribute ();
					else
						obsolete_attr = AttributeTester.GetMemberObsoleteAttribute (pi);

					prop_values.Add (value);
					prop_infos.Add (pi);
					
				} else {
					FieldInfo fi = ((FieldExpr) member).FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return false;
					}

					if (!IsValidArgumentType (member.Type)) {
						Report.SymbolRelatedToPreviousError (fi);
						Error_InvalidNamedAgrumentType (member_name);
						return false;
					}

 					object value;
					if (!a.Expr.GetAttributableValue (member.Type, out value))
						return false;

					FieldBase fb = TypeManager.GetField (fi);
					if (fb != null)
						obsolete_attr = fb.GetObsoleteAttribute ();
					else
						obsolete_attr = AttributeTester.GetMemberObsoleteAttribute (fi);

 					field_values.Add (value);  					
					field_infos.Add (fi);
				}

				if (obsolete_attr != null && !Owner.ResolveContext.IsInObsoleteScope)
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, member.GetSignatureForError (), member.Location);
			}

			prop_info_arr = new PropertyInfo [prop_infos.Count];
			field_info_arr = new FieldInfo [field_infos.Count];
			field_values_arr = new object [field_values.Count];
			prop_values_arr = new object [prop_values.Count];

			field_infos.CopyTo  (field_info_arr, 0);
			field_values.CopyTo (field_values_arr, 0);

			prop_values.CopyTo  (prop_values_arr, 0);
			prop_infos.CopyTo   (prop_info_arr, 0);

			return true;
		}

		/// <summary>
		///   Get a string containing a list of valid targets for the attribute 'attr'
		/// </summary>
		public string GetValidTargets ()
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = GetAttributeUsage (Type).ValidOn;

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

#if NET_2_0
			if ((targets & AttributeTargets.GenericParameter) != 0)
				sb.Append ("type parameter, ");
#endif			
			return sb.Remove (sb.Length - 2, 2).ToString ();
		}

		/// <summary>
		/// Returns AttributeUsage attribute based on types hierarchy
		/// </summary>
		static AttributeUsageAttribute GetAttributeUsage (Type type)
		{
			AttributeUsageAttribute ua = usage_attr_cache [type] as AttributeUsageAttribute;
			if (ua != null)
				return ua;

			Class attr_class = TypeManager.LookupClass (type);

			if (attr_class == null) {
				object[] usage_attr = type.GetCustomAttributes (TypeManager.attribute_usage_type, true);
				ua = (AttributeUsageAttribute)usage_attr [0];
				usage_attr_cache.Add (type, ua);
				return ua;
			}

			Attribute a = null;
			if (attr_class.OptAttributes != null)
				a = attr_class.OptAttributes.Search (TypeManager.attribute_usage_type);

			if (a == null) {
				if (attr_class.TypeBuilder.BaseType != TypeManager.attribute_type)
					ua = GetAttributeUsage (attr_class.TypeBuilder.BaseType);
				else
					ua = DefaultUsageAttribute;
			} else {
				ua = a.GetAttributeUsageAttribute ();
			}

			usage_attr_cache.Add (type, ua);
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
			string[] valid_targets = Owner.ValidAttributeTargets;
			if (ExplicitTarget == null || ExplicitTarget == valid_targets [0]) {
				Target = Owner.AttributeTargets;
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
		protected virtual bool IsSecurityActionValid (bool for_assembly)
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

		static object GetValue (object value)
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
				FieldBase fm = attr as FieldBase;
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

		public bool IsInternalMethodImplAttribute {
			get {
				if (Type != TypeManager.methodimpl_attr_type)
					return false;

				MethodImplOptions options;
				if (pos_values[0].GetType () != typeof (MethodImplOptions))
					options = (MethodImplOptions)System.Enum.ToObject (typeof (MethodImplOptions), pos_values[0]);
				else
					options = (MethodImplOptions)pos_values[0];

				return (options & MethodImplOptions.InternalCall) != 0;
			}
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

		public override bool Equals (object obj)
		{
			Attribute a = obj as Attribute;
			if (a == null)
				return false;

			return Type == a.Type && Target == a.Target;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Emit attribute for Attributable symbol
		/// </summary>
		public void Emit (ListDictionary allEmitted)
		{
			CustomAttributeBuilder cb = Resolve ();
			if (cb == null)
				return;

			AttributeUsageAttribute usage_attr = GetAttributeUsage (Type);
			if ((usage_attr.ValidOn & Target) == 0) {
				Report.Error (592, Location, "The attribute `{0}' is not valid on this declaration type. " +
					      "It is valid on `{1}' declarations only",
					GetSignatureForError (), GetValidTargets ());
				return;
			}

			try {
				foreach (Attributable owner in owners)
					owner.ApplyAttributeBuilder (this, cb);
			}
			catch (Exception e) {
				Error_AttributeEmitError (e.Message);
				return;
			}

			if (!usage_attr.AllowMultiple && allEmitted != null) {
				if (allEmitted.Contains (this)) {
					ArrayList a = allEmitted [this] as ArrayList;
					if (a == null) {
						a = new ArrayList (2);
						allEmitted [this] = a;
					}
					a.Add (this);
				} else {
					allEmitted.Add (this, null);
				}
			}

			if (!RootContext.VerifyClsCompliance)
				return;

			// Here we are testing attribute arguments for array usage (error 3016)
			if (Owner.IsClsComplianceRequired ()) {
				if (PosArguments != null) {
					foreach (Argument arg in PosArguments) { 
						// Type is undefined (was error 246)
						if (arg.Type == null)
							return;

						if (arg.Type.IsArray) {
							Report.Error (3016, Location, "Arrays as attribute arguments are not CLS-compliant");
							return;
						}
					}
				}
			
				if (NamedArguments == null)
					return;
			
				foreach (DictionaryEntry de in NamedArguments) {
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

		private Expression GetValue () 
		{
			if (PosArguments == null || PosArguments.Count < 1)
				return null;

			return ((Argument) PosArguments [0]).Expr;
		}

		public string GetString () 
		{
			Expression e = GetValue ();
			if (e is StringConstant)
				return ((StringConstant)e).Value;
			return null;
		}

		public bool GetBoolean () 
		{
			Expression e = GetValue ();
			if (e is BoolConstant)
				return ((BoolConstant)e).Value;
			return false;
		}

		public Type GetArgumentType ()
		{
			TypeOf e = GetValue () as TypeOf;
			if (e == null)
				return null;
			return e.TypeArgument;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
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
					Expression left_expr, string identifier, object[] args, Location loc, bool nameEscaped):
			base (target, left_expr, identifier, args, loc, nameEscaped)
		{
			this.ns = ns;
			this.owners = new Attributable[1];
		}
		
		public override void AttachTo (Attributable owner)
		{
			if (ExplicitTarget == "assembly") {
				owners [0] = CodeGen.Assembly;
				return;
			}
			if (ExplicitTarget == "module") {
				owners [0] = CodeGen.Module;
				return;
			}
			throw new NotImplementedException ("Unknown global explicit target " + ExplicitTarget);
		}

		void Enter ()
		{
			// RootContext.ToplevelTypes has a single NamespaceEntry which gets overwritten
			// each time a new file is parsed.  However, we need to use the NamespaceEntry
			// in effect where the attribute was used.  Since code elsewhere cannot assume
			// that the NamespaceEntry is right, just overwrite it.
			//
			// Precondition: RootContext.ToplevelTypes == null

			if (RootContext.ToplevelTypes.NamespaceEntry != null)
				throw new InternalErrorException (Location + " non-null NamespaceEntry");

			RootContext.ToplevelTypes.NamespaceEntry = ns;
		}

		protected override bool IsSecurityActionValid (bool for_assembly)
		{
			return base.IsSecurityActionValid (true);
		}

		void Leave ()
		{
			RootContext.ToplevelTypes.NamespaceEntry = null;
		}

		protected override TypeExpr ResolveAsTypeTerminal (Expression expr, IResolveContext ec, bool silent)
		{
			try {
				Enter ();
				return base.ResolveAsTypeTerminal (expr, ec, silent);
			}
			finally {
				Leave ();
			}
		}

		protected override ConstructorInfo ResolveConstructor (EmitContext ec)
		{
			try {
				Enter ();
				return base.ResolveConstructor (ec);
			}
			finally {
				Leave ();
			}
		}

		protected override bool ResolveNamedArguments (EmitContext ec)
		{
			try {
				Enter ();
				return base.ResolveNamedArguments (ec);
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

			ListDictionary ld = Attrs.Count > 1 ? new ListDictionary () : null;

			foreach (Attribute a in Attrs)
				a.Emit (ld);

			if (ld == null || ld.Count == 0)
				return;

			foreach (DictionaryEntry d in ld) {
				if (d.Value == null)
					continue;

				foreach (Attribute collision in (ArrayList)d.Value)
					Report.SymbolRelatedToPreviousError (collision.Location, "");

				Attribute a = (Attribute)d.Key;
				Report.Error (579, a.Location, "The attribute `{0}' cannot be applied multiple times",
					a.GetSignatureForError ());
			}
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
		static PtrHashtable analyzed_types;
		static PtrHashtable analyzed_types_obsolete;
		static PtrHashtable analyzed_member_obsolete;
		static PtrHashtable analyzed_method_excluded;
		static PtrHashtable fixed_buffer_cache;

		static object TRUE = new object ();
		static object FALSE = new object ();

		static AttributeTester ()
		{
			Reset ();
		}

		private AttributeTester ()
		{
		}

		public static void Reset ()
		{
			analyzed_types = new PtrHashtable ();
			analyzed_types_obsolete = new PtrHashtable ();
			analyzed_member_obsolete = new PtrHashtable ();
			analyzed_method_excluded = new PtrHashtable ();
			fixed_buffer_cache = new PtrHashtable ();
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
			if (type.IsArray || type.IsByRef) {
				result = IsClsCompliant (TypeManager.GetElementType (type));
			} else if (TypeManager.IsNullableType (type)) {
				result = IsClsCompliant (TypeManager.GetTypeArguments (type) [0]);
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
			// Fixed buffer helper type is generated as value type
			if (!fi.FieldType.IsValueType)
				return null;

			FieldBase fb = TypeManager.GetField (fi);
			if (fb != null) {
				return fb as IFixedBuffer;
			}
			
			if (TypeManager.GetConstant (fi) != null)
				return null;

			object o = fixed_buffer_cache [fi];
			if (o == null) {
				if (TypeManager.fixed_buffer_attr_type == null)
					return null;

				if (!fi.IsDefined (TypeManager.fixed_buffer_attr_type, false)) {
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
				if (!GetClsCompliantAttributeValue (module, null)) {
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

		static bool GetClsCompliantAttributeValue (ICustomAttributeProvider attribute_provider, Assembly a) 
		{
			if (TypeManager.cls_compliant_attribute_type == null)
				return false;

			object[] cls_attr = attribute_provider.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (cls_attr.Length == 0) {
				if (a == null)
					return false;

				return GetClsCompliantAttributeValue (a, null);
			}
			
			return ((CLSCompliantAttribute)cls_attr [0]).IsCompliant;
		}

		static bool AnalyzeTypeCompliance (Type type)
		{
			type = TypeManager.DropGenericTypeArguments (type);
			DeclSpace ds = TypeManager.LookupDeclSpace (type);
			if (ds != null) {
				return ds.IsClsComplianceRequired ();
			}

			if (TypeManager.IsGenericParameter (type))
				return true;

			return GetClsCompliantAttributeValue (type, type.Assembly);
		}

		// Registers the core type as we assume that they will never be obsolete which
		// makes things easier for bootstrap and faster (we don't need to query Obsolete attribute).
		public static void RegisterNonObsoleteType (Type type)
		{
			analyzed_types_obsolete [type] = FALSE;
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
			if (TypeManager.HasElementType (type)) {
				result = GetObsoleteAttribute (TypeManager.GetElementType (type));
			} else if (TypeManager.IsGenericParameter (type) || TypeManager.IsGenericType (type))
				return null;
			else {
				DeclSpace type_ds = TypeManager.LookupDeclSpace (type);

				// Type is external, we can get attribute directly
				if (type_ds == null) {
					if (TypeManager.obsolete_attribute_type != null) {
						object [] attribute = type.GetCustomAttributes (TypeManager.obsolete_attribute_type, false);
						if (attribute.Length == 1)
							result = (ObsoleteAttribute) attribute [0];
					}
				} else {
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

			MemberInfo mi = TypeManager.GetPropertyFromAccessor (mb);
			if (mi != null)
				return GetMemberObsoleteAttribute (mi);

			mi = TypeManager.GetEventFromAccessor (mb);
			if (mi != null)
				return GetMemberObsoleteAttribute (mi);

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

			if ((mi.DeclaringType is TypeBuilder) || TypeManager.IsGenericType (mi.DeclaringType))
				return null;

			if (TypeManager.obsolete_attribute_type == null)
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

			if (oa.Message == null || oa.Message.Length == 0) {
				Report.Warning (612, 1, loc, "`{0}' is obsolete", member);
				return;
			}
			Report.Warning (618, 2, loc, "`{0}' is obsolete: `{1}'", member, oa.Message);
		}

		public static bool IsConditionalMethodExcluded (MethodBase mb)
		{
			object excluded = analyzed_method_excluded [mb];
			if (excluded != null)
				return excluded == TRUE ? true : false;

			if (TypeManager.conditional_attribute_type == null)
				return false;

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
			if (class_decl == null && TypeManager.conditional_attribute_type != null) {
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
				if (TypeManager.coclass_attr_type == null)
					return null;

				object[] o = type.GetCustomAttributes (TypeManager.coclass_attr_type, false);
				if (o.Length < 1)
					return null;
				return ((System.Runtime.InteropServices.CoClassAttribute)o[0]).CoClass;
			}

			if (tc.OptAttributes == null || TypeManager.coclass_attr_type == null)
				return null;

			Attribute a = tc.OptAttributes.Search (TypeManager.coclass_attr_type);
			if (a == null)
				return null;

			return a.GetCoClassAttributeValue ();
		}
	}
}
