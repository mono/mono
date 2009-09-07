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
		//
		// Holds all attributes attached to this element
		//
 		protected Attributes attributes;

		public void AddAttributes (Attributes attrs, IMemberContext context)
		{
			if (attrs == null)
				return;

			if (attributes == null)
				attributes = attrs;
			else
				throw new NotImplementedException ();

			attributes.AttachTo (this, context);
		}

		public Attributes OptAttributes {
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
		public abstract void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa);

		/// <summary>
		/// Returns one AttributeTarget for this element.
		/// </summary>
		public abstract AttributeTargets AttributeTargets { get; }

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
		readonly ATypeNameExpression expression;

		Arguments PosArguments;
		Arguments NamedArguments;

		bool resolve_error;
		readonly bool nameEscaped;

		//
		// An attribute can be attached to multiple targets (e.g. multiple fields)
		//
		protected Attributable[] targets;

		//
		// A member context for the attribute, it's much easier to hold it here
		// than trying to pull it during resolve
		//
		IMemberContext context;

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

		public Attribute (string target, ATypeNameExpression expr, Arguments[] args, Location loc, bool nameEscaped)
		{
			this.expression = expr;
			if (args != null) {
				PosArguments = args [0];
				NamedArguments = args [1];				
			}
			this.loc = loc;
			ExplicitTarget = target;
			this.nameEscaped = nameEscaped;
		}

		public Attribute Clone ()
		{
			Attribute a = new Attribute (ExplicitTarget, expression, null, loc, nameEscaped);
			a.PosArguments = PosArguments;
			a.NamedArguments = NamedArguments;
			return a;
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

		//
		// When the same attribute is attached to multiple fiels
		// we use @target field as a list of targets. The attribute
		// has to be resolved only once but emitted for each target.
		//
		public virtual void AttachTo (Attributable target, IMemberContext context)
		{
			if (this.targets == null) {
				this.targets = new Attributable[] { target };
				this.context = context;
				return;
			}

			// Resize target array
			Attributable[] new_array = new Attributable [this.targets.Length + 1];
			targets.CopyTo (new_array, 0);
			new_array [targets.Length] = target;
			this.targets = new_array;

			// No need to update context, different targets cannot have
			// different contexts, it's enough to remove same attributes
			// from secondary members.

			target.OptAttributes = null;
		}

		static void Error_InvalidNamedArgument (ResolveContext rc, NamedArgument name)
		{
			rc.Report.Error (617, name.Name.Location, "`{0}' is not a valid named attribute argument. Named attribute arguments " +
				      "must be fields which are not readonly, static, const or read-write properties which are " +
				      "public and not static",
			      name.Name.Value);
		}

		static void Error_InvalidNamedArgumentType (ResolveContext rc, NamedArgument name)
		{
			rc.Report.Error (655, name.Name.Location,
				"`{0}' is not a valid named attribute argument because it is not a valid attribute parameter type",
				name.Name.Value);
		}

		public static void Error_AttributeArgumentNotValid (ResolveContext rc, Location loc)
		{
			rc.Report.Error (182, loc,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
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
				return targets [0];
			}
		}

		protected virtual TypeExpr ResolveAsTypeTerminal (Expression expr, IMemberContext ec)
		{
			return expr.ResolveAsTypeTerminal (ec, false);
		}

		Type ResolvePossibleAttributeType (ATypeNameExpression expr, ref bool is_attr)
		{
			TypeExpr te = ResolveAsTypeTerminal (expr, context);
			if (te == null)
				return null;

			Type t = te.Type;
			if (TypeManager.IsSubclassOf (t, TypeManager.attribute_type)) {
				is_attr = true;
			} else {
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
			SessionReportPrinter resolve_printer = new SessionReportPrinter ();
			ReportPrinter prev_recorder = context.Compiler.Report.SetPrinter (resolve_printer);

			bool t1_is_attr = false;
			bool t2_is_attr = false;
			Type t1, t2;
			ATypeNameExpression expanded = null;

			try {
				t1 = ResolvePossibleAttributeType (expression, ref t1_is_attr);

				if (nameEscaped) {
					t2 = null;
				} else {
					expanded = (ATypeNameExpression) expression.Clone (null);
					expanded.Name += "Attribute";

					t2 = ResolvePossibleAttributeType (expanded, ref t2_is_attr);
				}

				resolve_printer.EndSession ();
			} finally {
				context.Compiler.Report.SetPrinter (prev_recorder);
			}

			if (t1_is_attr && t2_is_attr) {
				Report.Error (1614, Location, "`{0}' is ambiguous between `{1}' and `{2}'. Use either `@{0}' or `{0}Attribute'",
					GetSignatureForError (), expression.GetSignatureForError (), expanded.GetSignatureForError ());
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

			resolve_printer.Merge (prev_recorder);
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

			return expression.GetSignatureForError ();
		}

		public bool HasSecurityAttribute {
			get {
				PredefinedAttribute pa = PredefinedAttributes.Get.Security;
				return pa.IsDefined && TypeManager.IsSubclassOf (type, pa.Type);
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

		// TODO: Don't use this ambiguous value
		public string Name {
			get { return expression.Name; }
		}

		void ApplyModuleCharSet ()
		{
			if (Type != PredefinedAttributes.Get.DllImport)
				return;

			if (!RootContext.ToplevelTypes.HasDefaultCharSet)
				return;

			const string CharSetEnumMember = "CharSet";
			if (NamedArguments == null) {
				NamedArguments = new Arguments (1);
			} else {
				foreach (NamedArgument a in NamedArguments) {
					if (a.Name.Value == CharSetEnumMember)
						return;
				}
			}
			
			NamedArguments.Add (new NamedArgument (new LocatedToken (loc, CharSetEnumMember),
				Constant.CreateConstant (typeof (CharSet), RootContext.ToplevelTypes.DefaultCharSet, Location)));
 		}

		Report Report {
			get { return context.Compiler.Report; }
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
				AttributeTester.Report_ObsoleteMessage (obsolete_attr, TypeManager.CSharpName (Type), Location, Report);
			}

			if (PosArguments == null && NamedArguments == null) {
				object o = att_cache [Type];
				if (o != null) {
					resolve_error = false;
					return (CustomAttributeBuilder)o;
				}
			}

			ResolveContext rc = new ResolveContext (context, ResolveContext.Options.ConstantScope);
			ConstructorInfo ctor = ResolveConstructor (rc);
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

				if (!ResolveNamedArguments (rc)) {
					return null;
				}

				cb = new CustomAttributeBuilder (ctor, pos_values,
						prop_info_arr, prop_values_arr,
						field_info_arr, field_values_arr);

				resolve_error = false;
				return cb;
			}
			catch (Exception) {
				Error_AttributeArgumentNotValid (rc, Location);
				return null;
			}
		}

		protected virtual ConstructorInfo ResolveConstructor (ResolveContext ec)
		{
			if (PosArguments != null) {
				bool dynamic;
				PosArguments.Resolve (ec, out dynamic);
				if (dynamic)
					throw new NotImplementedException ("dynamic");
			}

			MethodGroupExpr mg = MemberLookupFinal (ec, ec.CurrentType,
				Type, ConstructorInfo.ConstructorName, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Location) as MethodGroupExpr;

			if (mg == null)
				throw new NotImplementedException ();

			mg = mg.OverloadResolve (ec, ref PosArguments, false, Location);
			if (mg == null)
				return null;
			
			ConstructorInfo constructor = (ConstructorInfo)mg;
			if (PosArguments == null) {
				pos_values = EmptyObject;
				return constructor;
			}

			AParametersCollection pd = TypeManager.GetParameterData (constructor);

			if (!PosArguments.GetAttributableValue (ec, out pos_values))
				return null;

			// Here we do the checks which should be done by corlib or by runtime.
			// However Zoltan doesn't like it and every Mono compiler has to do it again.
			PredefinedAttributes pa = PredefinedAttributes.Get;
			if (Type == pa.Guid) {
				try {
					new Guid ((string)pos_values [0]);
				}
				catch (Exception e) {
					Error_AttributeEmitError (e.Message);
					return null;
				}
			}

			if (Type == pa.AttributeUsage && (int)pos_values [0] == 0) {
				ec.Report.Error (591, Location, "Invalid value for argument to `System.AttributeUsage' attribute");
				return null;
			}

			if (Type == pa.IndexerName || Type == pa.Conditional) {
				string v = pos_values [0] as string;
				if (!Tokenizer.IsValidIdentifier (v) || Tokenizer.IsKeyword (v)) {
					ec.Report.Error (633, PosArguments [0].Expr.Location,
						"The argument to the `{0}' attribute must be a valid identifier", GetSignatureForError ());
					return null;
				}
			}

			if (Type == pa.MethodImpl && pos_values.Length == 1 &&
				pd.Types [0] == TypeManager.short_type &&
				!System.Enum.IsDefined (typeof (MethodImplOptions), pos_values [0].ToString ())) {
				Error_AttributeEmitError ("Incorrect argument value.");
				return null;
			}

			return constructor;
		}

		protected virtual bool ResolveNamedArguments (ResolveContext ec)
		{
			int named_arg_count = NamedArguments.Count;

			ArrayList field_infos = new ArrayList (named_arg_count);
			ArrayList prop_infos  = new ArrayList (named_arg_count);
			ArrayList field_values = new ArrayList (named_arg_count);
			ArrayList prop_values = new ArrayList (named_arg_count);

			ArrayList seen_names = new ArrayList (named_arg_count);
			
			foreach (NamedArgument a in NamedArguments) {
				string name = a.Name.Value;
				if (seen_names.Contains (name)) {
					ec.Report.Error (643, a.Name.Location, "Duplicate named attribute `{0}' argument", name);
					continue;
				}			
	
				seen_names.Add (name);

				a.Resolve (ec);

				Expression member = Expression.MemberLookup (ec.Compiler,
					ec.CurrentType, Type, name,
					MemberTypes.All,
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
					Location);

				if (member == null) {
					member = Expression.MemberLookup (ec.Compiler, ec.CurrentType, Type, name,
						MemberTypes.All, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
						Location);

					if (member != null) {
						ec.Report.SymbolRelatedToPreviousError (member.Type);
						Expression.ErrorIsInaccesible (Location, member.GetSignatureForError (), ec.Report);
						return false;
					}
				}

				if (member == null){
					Expression.Error_TypeDoesNotContainDefinition (ec, Location, Type, name);
					return false;
				}
				
				if (!(member is PropertyExpr || member is FieldExpr)) {
					Error_InvalidNamedArgument (ec, a);
					return false;
				}

				ObsoleteAttribute obsolete_attr;

				if (member is PropertyExpr) {
					PropertyInfo pi = ((PropertyExpr) member).PropertyInfo;

					if (!pi.CanWrite || !pi.CanRead || pi.GetGetMethod ().IsStatic) {
						ec.Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedArgument (ec, a);
						return false;
					}

					if (!IsValidArgumentType (member.Type)) {
						ec.Report.SymbolRelatedToPreviousError (pi);
						Error_InvalidNamedArgumentType (ec, a);
						return false;
					}

					object value;
					if (!a.Expr.GetAttributableValue (ec, member.Type, out value))
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

					if (fi.IsInitOnly || fi.IsStatic) {
						Error_InvalidNamedArgument (ec, a);
						return false;
					}

					if (!IsValidArgumentType (member.Type)) {
						ec.Report.SymbolRelatedToPreviousError (fi);
						Error_InvalidNamedArgumentType (ec, a);
						return false;
					}

 					object value;
					if (!a.Expr.GetAttributableValue (ec, member.Type, out value))
						return false;

					FieldBase fb = TypeManager.GetField (fi);
					if (fb != null)
						obsolete_attr = fb.GetObsoleteAttribute ();
					else
						obsolete_attr = AttributeTester.GetMemberObsoleteAttribute (fi);

 					field_values.Add (value);  					
					field_infos.Add (fi);
				}

				if (obsolete_attr != null && !context.IsObsolete)
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, member.GetSignatureForError (), member.Location, Report);
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
			PredefinedAttribute pa = PredefinedAttributes.Get.AttributeUsage;

			if (attr_class == null) {
				if (!pa.IsDefined)
					return new AttributeUsageAttribute (0);

				object[] usage_attr = type.GetCustomAttributes (pa.Type, true);
				ua = (AttributeUsageAttribute)usage_attr [0];
				usage_attr_cache.Add (type, ua);
				return ua;
			}

			Attribute a = null;
			if (attr_class.OptAttributes != null)
				a = attr_class.OptAttributes.Search (pa);

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
						// FIXME: We are missing return type filter
						// TODO: pi can be null
						PropertyInfo pi = orig_assembly_type.GetProperty (emited_pi.Name);

						object old_instance = TypeManager.IsEnumType (pi.PropertyType) ?
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

		//
		// Theoretically, we can get rid of this, since FieldBuilder.SetCustomAttribute()
		// and ParameterBuilder.SetCustomAttribute() are supposed to handle this attribute.
		// However, we can't, since it appears that the .NET 1.1 SRE hangs when given a MarshalAsAttribute.
		//
#if !NET_2_0
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

		static object GetValue (object value)
		{
			if (value is EnumConstant)
				return ((EnumConstant) value).GetValue ();
			else
				return value;				
		}
		
#endif

		public CharSet GetCharSetValue ()
		{
			return (CharSet)System.Enum.Parse (typeof (CharSet), pos_values [0].ToString ());
		}

		public bool HasField (string fieldName)
		{
			if (field_info_arr == null)
				return false;

			foreach (FieldInfo fi in field_info_arr) {
				if (fi.Name == fieldName)
					return true;
			}

			return false;
		}

		public bool IsInternalMethodImplAttribute {
			get {
				if (Type != PredefinedAttributes.Get.MethodImpl)
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
				foreach (Attributable target in targets)
					target.ApplyAttributeBuilder (this, cb, PredefinedAttributes.Get);
			} catch (Exception e) {
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
				if (PosArguments != null)
					PosArguments.CheckArrayAsAttribute (context.Compiler);
			
				if (NamedArguments == null)
					return;

				NamedArguments.CheckArrayAsAttribute (context.Compiler);
			}
		}

		private Expression GetValue () 
		{
			if (PosArguments == null || PosArguments.Count < 1)
				return null;

			return PosArguments [0].Expr;
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

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
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

		public GlobalAttribute (NamespaceEntry ns, string target, ATypeNameExpression expression,
					Arguments[] args, Location loc, bool nameEscaped):
			base (target, expression, args, loc, nameEscaped)
		{
			this.ns = ns;
		}
		
		public override void AttachTo (Attributable target, IMemberContext context)
		{
			if (ExplicitTarget == "assembly") {
				base.AttachTo (CodeGen.Assembly, context);
				return;
			}

			if (ExplicitTarget == "module") {
				base.AttachTo (RootContext.ToplevelTypes, context);
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

		protected override TypeExpr ResolveAsTypeTerminal (Expression expr, IMemberContext ec)
		{
			try {
				Enter ();
				return base.ResolveAsTypeTerminal (expr, ec);
			}
			finally {
				Leave ();
			}
		}

		protected override ConstructorInfo ResolveConstructor (ResolveContext ec)
		{
			try {
				Enter ();
				return base.ResolveConstructor (ec);
			}
			finally {
				Leave ();
			}
		}

		protected override bool ResolveNamedArguments (ResolveContext ec)
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

		public void AttachTo (Attributable attributable, IMemberContext context)
		{
			foreach (Attribute a in Attrs)
				a.AttachTo (attributable, context);
		}

		public Attributes Clone ()
		{
			ArrayList al = new ArrayList (Attrs.Count);
			foreach (Attribute a in Attrs)
				al.Add (a.Clone ());

			return new Attributes (al);
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

		public Attribute Search (PredefinedAttribute t)
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
		public Attribute[] SearchMulti (PredefinedAttribute t)
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

				Report report = RootContext.ToplevelTypes.Compiler.Report;
				foreach (Attribute collision in (ArrayList)d.Value)
					report.SymbolRelatedToPreviousError (collision.Location, "");

				Attribute a = (Attribute)d.Key;
				report.Error (579, a.Location, "The attribute `{0}' cannot be applied multiple times",
					a.GetSignatureForError ());
			}
		}

		public bool Contains (PredefinedAttribute t)
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
		public static Result AreOverloadedMethodParamsClsCompliant (AParametersCollection pa, AParametersCollection pb) 
		{
			Type [] types_a = pa.Types;
			Type [] types_b = pb.Types;
			if (types_a == null || types_b == null)
				return Result.Ok;

			if (types_a.Length != types_b.Length)
				return Result.Ok;

			Result result = Result.Ok;
			for (int i = 0; i < types_b.Length; ++i) {
				Type aType = types_a [i];
				Type bType = types_b [i];

				if (aType.IsArray && bType.IsArray) {
					Type a_el_type = TypeManager.GetElementType (aType);
					Type b_el_type = TypeManager.GetElementType (bType);
					if (aType.GetArrayRank () != bType.GetArrayRank () && a_el_type == b_el_type) {
						result = Result.RefOutArrayError;
						continue;
					}

					if (a_el_type.IsArray || b_el_type.IsArray) {
						result = Result.ArrayArrayError;
						continue;
					}
				}

				if (aType != bType)
					return Result.Ok;

				const Parameter.Modifier out_ref_mod = (Parameter.Modifier.OUTMASK | Parameter.Modifier.REFMASK);
				if ((pa.FixedParameters[i].ModFlags & out_ref_mod) != (pb.FixedParameters[i].ModFlags & out_ref_mod))
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
				analyzed_types.Add (type, FALSE);
				return false;
			}

			bool result;
			if (type.IsArray) {
				result = IsClsCompliant (TypeManager.GetElementType (type));
			} else if (TypeManager.IsNullableType (type)) {
				result = IsClsCompliant (TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (type) [0]));
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
			if (TypeManager.IsReferenceType (fi.FieldType))
				return null;

			FieldBase fb = TypeManager.GetField (fi);
			if (fb != null) {
				return fb as IFixedBuffer;
			}
			
			if (TypeManager.GetConstant (fi) != null)
				return null;

			object o = fixed_buffer_cache [fi];
			if (o == null) {
				PredefinedAttribute pa = PredefinedAttributes.Get.FixedBuffer;
				if (!pa.IsDefined)
					return null;

				if (!fi.IsDefined (pa.Type, false)) {
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

		public static void VerifyModulesClsCompliance (CompilerContext ctx)
		{
			Module[] modules = GlobalRootNamespace.Instance.Modules;
			if (modules == null)
				return;

			// The first module is generated assembly
			for (int i = 1; i < modules.Length; ++i) {
				Module module = modules [i];
				if (!GetClsCompliantAttributeValue (module, null)) {
					ctx.Report.Error (3013, "Added modules must be marked with the CLSCompliant attribute " +
						      "to match the assembly", module.Name);
					return;
				}
			}
		}

		public static Type GetImportedIgnoreCaseClsType (string name)
		{
			foreach (Assembly a in GlobalRootNamespace.Instance.Assemblies) {
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
			PredefinedAttribute pa = PredefinedAttributes.Get.CLSCompliant;
			if (!pa.IsDefined)
				return false;

			object[] cls_attr = attribute_provider.GetCustomAttributes (pa.Type, false);
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
			} else if (TypeManager.IsGenericParameter (type))
				result = null;	// TODO: throw new NotSupportedException ()
			else if (TypeManager.IsGenericType (type) && !TypeManager.IsGenericTypeDefinition (type)) {
				return GetObsoleteAttribute (TypeManager.DropGenericTypeArguments (type));
			} else {
				DeclSpace type_ds = TypeManager.LookupDeclSpace (type);

				// Type is external, we can get attribute directly
				if (type_ds == null) {
					PredefinedAttribute pa = PredefinedAttributes.Get.Obsolete;
					if (pa.IsDefined) {
						object[] attribute = type.GetCustomAttributes (pa.Type, false);
						if (attribute.Length == 1)
							result = (ObsoleteAttribute) attribute[0];
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

			PredefinedAttribute pa = PredefinedAttributes.Get.Obsolete;
			if (!pa.IsDefined)
				return null;

			ObsoleteAttribute oa = System.Attribute.GetCustomAttribute (mi, pa.Type, false)
				as ObsoleteAttribute;
			analyzed_member_obsolete.Add (mi, oa == null ? FALSE : oa);
			return oa;
		}

		/// <summary>
		/// Common method for Obsolete error/warning reporting.
		/// </summary>
		public static void Report_ObsoleteMessage (ObsoleteAttribute oa, string member, Location loc, Report Report)
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

		public static bool IsConditionalMethodExcluded (MethodBase mb, Location loc)
		{
			object excluded = analyzed_method_excluded [mb];
			if (excluded != null)
				return excluded == TRUE ? true : false;

			PredefinedAttribute pa = PredefinedAttributes.Get.Conditional;
			if (!pa.IsDefined)
				return false;

			ConditionalAttribute[] attrs = mb.GetCustomAttributes (pa.Type, true)
				as ConditionalAttribute[];
			if (attrs.Length == 0) {
				analyzed_method_excluded.Add (mb, FALSE);
				return false;
			}

			foreach (ConditionalAttribute a in attrs) {
				if (loc.CompilationUnit.IsConditionalDefined (a.ConditionString)) {
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
		public static bool IsAttributeExcluded (Type type, Location loc)
		{
			if (!type.IsClass)
				return false;

			Class class_decl = TypeManager.LookupDeclSpace (type) as Class;

			// TODO: add caching
			// TODO: merge all Type bases attribute caching to one cache to save memory
			PredefinedAttribute pa = PredefinedAttributes.Get.Conditional;
			if (class_decl == null && pa.IsDefined) {
				object[] attributes = type.GetCustomAttributes (pa.Type, false);
				foreach (ConditionalAttribute ca in attributes) {
					if (loc.CompilationUnit.IsConditionalDefined (ca.ConditionString))
						return false;
				}
				return attributes.Length > 0;
			}

			return class_decl.IsExcluded ();
		}

		public static Type GetCoClassAttribute (Type type)
		{
			TypeContainer tc = TypeManager.LookupInterface (type);
			PredefinedAttribute pa = PredefinedAttributes.Get.CoClass;
			if (tc == null) {
				if (!pa.IsDefined)
					return null;

				object[] o = type.GetCustomAttributes (pa.Type, false);
				if (o.Length < 1)
					return null;
				return ((System.Runtime.InteropServices.CoClassAttribute)o[0]).CoClass;
			}

			if (tc.OptAttributes == null)
				return null;

			Attribute a = tc.OptAttributes.Search (pa);
			if (a == null)
				return null;

			return a.GetCoClassAttributeValue ();
		}
	}

	public class PredefinedAttributes
	{
		// Core types
		public readonly PredefinedAttribute ParamArray;
		public readonly PredefinedAttribute Out;

		// Optional types
		public readonly PredefinedAttribute Obsolete;
		public readonly PredefinedAttribute DllImport;
		public readonly PredefinedAttribute MethodImpl;
		public readonly PredefinedAttribute MarshalAs;
		public readonly PredefinedAttribute In;
		public readonly PredefinedAttribute IndexerName;
		public readonly PredefinedAttribute Conditional;
		public readonly PredefinedAttribute CLSCompliant;
		public readonly PredefinedAttribute Security;
		public readonly PredefinedAttribute Required;
		public readonly PredefinedAttribute Guid;
		public readonly PredefinedAttribute AssemblyCulture;
		public readonly PredefinedAttribute AssemblyVersion;
		public readonly PredefinedAttribute ComImport;
		public readonly PredefinedAttribute CoClass;
		public readonly PredefinedAttribute AttributeUsage;
		public readonly PredefinedAttribute DefaultParameterValue;
		public readonly PredefinedAttribute OptionalParameter;

		// New in .NET 2.0
		public readonly PredefinedAttribute DefaultCharset;
		public readonly PredefinedAttribute TypeForwarder;
		public readonly PredefinedAttribute FixedBuffer;
		public readonly PredefinedAttribute CompilerGenerated;
		public readonly PredefinedAttribute InternalsVisibleTo;
		public readonly PredefinedAttribute RuntimeCompatibility;
		public readonly PredefinedAttribute DebuggerHidden;
		public readonly PredefinedAttribute UnsafeValueType;

		// New in .NET 3.5
		public readonly PredefinedAttribute Extension;

		// New in .NET 4.0
		public readonly PredefinedAttribute Dynamic;

		//
		// Optional types which are used as types and for member lookup
		//
		public readonly PredefinedAttribute DefaultMember;
		public readonly PredefinedAttribute DecimalConstant;
		public readonly PredefinedAttribute StructLayout;
		public readonly PredefinedAttribute FieldOffset;

		public static PredefinedAttributes Get = new PredefinedAttributes ();

		private PredefinedAttributes ()
		{
			ParamArray = new PredefinedAttribute ("System", "ParamArrayAttribute");
			Out = new PredefinedAttribute ("System.Runtime.InteropServices", "OutAttribute");

			Obsolete = new PredefinedAttribute ("System", "ObsoleteAttribute");
			DllImport = new PredefinedAttribute ("System.Runtime.InteropServices", "DllImportAttribute");
			MethodImpl = new PredefinedAttribute ("System.Runtime.CompilerServices", "MethodImplAttribute");
			MarshalAs = new PredefinedAttribute ("System.Runtime.InteropServices", "MarshalAsAttribute");
			In = new PredefinedAttribute ("System.Runtime.InteropServices", "InAttribute");
			IndexerName = new PredefinedAttribute ("System.Runtime.CompilerServices", "IndexerNameAttribute");
			Conditional = new PredefinedAttribute ("System.Diagnostics", "ConditionalAttribute");
			CLSCompliant = new PredefinedAttribute ("System", "CLSCompliantAttribute");
			Security = new PredefinedAttribute ("System.Security.Permissions", "SecurityAttribute");
			Required = new PredefinedAttribute ("System.Runtime.CompilerServices", "RequiredAttributeAttribute");
			Guid = new PredefinedAttribute ("System.Runtime.InteropServices", "GuidAttribute");
			AssemblyCulture = new PredefinedAttribute ("System.Reflection", "AssemblyCultureAttribute");
			AssemblyVersion = new PredefinedAttribute ("System.Reflection", "AssemblyVersionAttribute");
			ComImport = new PredefinedAttribute ("System.Runtime.InteropServices", "ComImportAttribute");
			CoClass = new PredefinedAttribute ("System.Runtime.InteropServices", "CoClassAttribute");
			AttributeUsage = new PredefinedAttribute ("System", "AttributeUsageAttribute");
			DefaultParameterValue = new PredefinedAttribute ("System.Runtime.InteropServices", "DefaultParameterValueAttribute");
			OptionalParameter = new PredefinedAttribute ("System.Runtime.InteropServices", "OptionalAttribute");

			DefaultCharset = new PredefinedAttribute ("System.Runtime.InteropServices", "DefaultCharSetAttribute");
			TypeForwarder = new PredefinedAttribute ("System.Runtime.CompilerServices", "TypeForwardedToAttribute");
			FixedBuffer = new PredefinedAttribute ("System.Runtime.CompilerServices", "FixedBufferAttribute");
			CompilerGenerated = new PredefinedAttribute ("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
			InternalsVisibleTo = new PredefinedAttribute ("System.Runtime.CompilerServices", "InternalsVisibleToAttribute");
			RuntimeCompatibility = new PredefinedAttribute ("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute");
			DebuggerHidden = new PredefinedAttribute ("System.Diagnostics", "DebuggerHiddenAttribute");
			UnsafeValueType = new PredefinedAttribute ("System.Runtime.CompilerServices", "UnsafeValueTypeAttribute");

			Extension = new PredefinedAttribute ("System.Runtime.CompilerServices", "ExtensionAttribute");

			Dynamic = new PredefinedAttribute ("System.Runtime.CompilerServices", "DynamicAttribute");

			DefaultMember = new PredefinedAttribute ("System.Reflection", "DefaultMemberAttribute");
			DecimalConstant = new PredefinedAttribute ("System.Runtime.CompilerServices", "DecimalConstantAttribute");
			StructLayout = new PredefinedAttribute ("System.Runtime.InteropServices", "StructLayoutAttribute");
			FieldOffset = new PredefinedAttribute ("System.Runtime.InteropServices", "FieldOffsetAttribute");
		}

		public void Initialize ()
		{
			foreach (FieldInfo fi in GetType ().GetFields (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
				((PredefinedAttribute) fi.GetValue (this)).Resolve (true);
			}
		}

		public static void Reset ()
		{
			Get = new PredefinedAttributes ();
		}
	}

	public class PredefinedAttribute
	{
		Type type;
		CustomAttributeBuilder cab;
		ConstructorInfo ctor;
		readonly string ns, name;

		public PredefinedAttribute (string ns, string name)
		{
			this.ns = ns;
			this.name = name;
		}

		public static bool operator == (Type type, PredefinedAttribute pa)
		{
			return type == pa.type;
		}

		public static bool operator != (Type type, PredefinedAttribute pa)
		{
			return type != pa.type;
		}

		public ConstructorInfo Constructor {
			get { return ctor; }
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			throw new NotSupportedException ();
		}

		public void EmitAttribute (ConstructorBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (MethodBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (PropertyBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (FieldBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (TypeBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (AssemblyBuilder builder)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public void EmitAttribute (ParameterBuilder builder, Location loc)
		{
			if (ResolveBuilder ())
				builder.SetCustomAttribute (cab);
		}

		public bool IsDefined {
			get { return type != null && type != typeof (PredefinedAttribute); }
		}

		public bool Resolve (bool canFail)
		{
			if (type != null) {
				if (IsDefined)
					return true;
				if (canFail)
					return false;
			}

			type = TypeManager.CoreLookupType (RootContext.ToplevelTypes.Compiler, ns, name, Kind.Class, !canFail);
			if (type == null) {
				type = typeof (PredefinedAttribute);
				return false;
			}

			return true;
		}

		bool ResolveBuilder ()
		{
			if (cab != null)
				return true;

			//
			// Handle all parameter-less attributes as optional
			//
			if (!Resolve (true))
				return false;

			ConstructorInfo ci = TypeManager.GetPredefinedConstructor (type, Location.Null, Type.EmptyTypes);
			if (ci == null)
				return false;

			cab = new CustomAttributeBuilder (ci, new object[0]);
			return true;
		}

		public bool ResolveConstructor (Location loc, params Type[] argType)
		{
			if (ctor != null)
				throw new InternalErrorException ("Predefined ctor redefined");

			if (!Resolve (false))
				return false;

			ctor = TypeManager.GetPredefinedConstructor (type, loc, argType);
			return ctor != null;
		}

		public Type Type {
			get { return type; }
		}
	}
}
