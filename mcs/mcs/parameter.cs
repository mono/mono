//
// parameter.cs: Parameter definition.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc. 
// Copyright 2011 Xamarin Inc
//
//
using System;
using System.Text;

#if STATIC
using MetaType = IKVM.Reflection.Type;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using MetaType = System.Type;
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.CSharp {

	/// <summary>
	///   Abstract Base class for parameters of a method.
	/// </summary>
	public abstract class ParameterBase : Attributable
	{
		protected ParameterBuilder builder;

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
#if false
			if (a.Type == pa.MarshalAs) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					builder.SetMarshal (marshal);
				}
				return;
			}
#endif
			if (a.HasSecurityAttribute) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			if (a.Type == pa.Dynamic) {
				a.Error_MisusedDynamicAttribute ();
				return;
			}

			builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public ParameterBuilder Builder {
			get {
				return builder;
			}
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}
	}

	/// <summary>
	/// Class for applying custom attributes on the return type
	/// </summary>
	public class ReturnParameter : ParameterBase
	{
		MemberCore method;

		// TODO: merge method and mb
		public ReturnParameter (MemberCore method, MethodBuilder mb, Location location)
		{
			this.method = method;
			try {
				builder = mb.DefineParameter (0, ParameterAttributes.None, "");			
			}
			catch (ArgumentOutOfRangeException) {
				method.Compiler.Report.RuntimeMissingSupport (location, "custom attributes on the return type");
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant) {
				method.Compiler.Report.Warning (3023, 1, a.Location,
					"CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead");
			}

			// This occurs after Warning -28
			if (builder == null)
				return;

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.ReturnValue;
			}
		}

		/// <summary>
		/// Is never called
		/// </summary>
		public override string[] ValidAttributeTargets {
			get {
				return null;
			}
		}
	}

	public class ImplicitLambdaParameter : Parameter
	{
		public ImplicitLambdaParameter (string name, Location loc)
			: base (null, name, Modifier.NONE, null, loc)
		{
		}

		public override TypeSpec Resolve (IMemberContext ec, int index)
		{
			if (parameter_type == null)
				throw new InternalErrorException ("A type of implicit lambda parameter `{0}' is not set",
					Name);

			base.idx = index;
			return parameter_type;
		}

		public void SetParameterType (TypeSpec type)
		{
			parameter_type = type;
		}
	}

	public class ParamsParameter : Parameter {
		public ParamsParameter (FullNamedExpression type, string name, Attributes attrs, Location loc):
			base (type, name, Parameter.Modifier.PARAMS, attrs, loc)
		{
		}

		public override TypeSpec Resolve (IMemberContext ec, int index)
		{
			if (base.Resolve (ec, index) == null)
				return null;

			var ac = parameter_type as ArrayContainer;
			if (ac == null || ac.Rank != 1) {
				ec.Module.Compiler.Report.Error (225, Location, "The params parameter must be a single dimensional array");
				return null;
			}

			return parameter_type;
		}

		public override void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
			base.ApplyAttributes (mb, cb, index, pa);
			pa.ParamArray.EmitAttribute (builder);
		}
	}

	public class ArglistParameter : Parameter {
		// Doesn't have proper type because it's never chosen for better conversion
		public ArglistParameter (Location loc) :
			base (null, String.Empty, Parameter.Modifier.NONE, null, loc)
		{
			parameter_type = InternalType.Arglist;
		}

		public override void  ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
			// Nothing to do
		}

		public override bool CheckAccessibility (InterfaceMemberBase member)
		{
			return true;
		}

		public override TypeSpec Resolve (IMemberContext ec, int index)
		{
			return parameter_type;
		}
	}

	public interface IParameterData
	{
		Expression DefaultValue { get; }
		bool HasExtensionMethodModifier { get; }
		bool HasDefaultValue { get; }
		Parameter.Modifier ModFlags { get; }
		string Name { get; }
	}

	//
	// Parameter information created by parser
	//
	public class Parameter : ParameterBase, IParameterData, ILocalVariable // TODO: INamedBlockVariable
	{
		[Flags]
		public enum Modifier : byte {
			NONE    = 0,
			PARAMS  = 1 << 0,
			REF = 1 << 1,
			OUT = 1 << 2,
			This = 1 << 3,
			CallerMemberName = 1 << 4,
			CallerLineNumber = 1 << 5,
			CallerFilePath = 1 << 6,

			RefOutMask = REF | OUT,
			ModifierMask = PARAMS | REF | OUT | This,
			CallerMask = CallerMemberName | CallerLineNumber | CallerFilePath
		}

		static readonly string[] attribute_targets = new [] { "param" };

		FullNamedExpression texpr;
		Modifier modFlags;
		string name;
		Expression default_expr;
		protected TypeSpec parameter_type;
		readonly Location loc;
		protected int idx;
		public bool HasAddressTaken;

		TemporaryVariableReference expr_tree_variable;

		HoistedParameter hoisted_variant;

		public Parameter (FullNamedExpression type, string name, Modifier mod, Attributes attrs, Location loc)
		{
			this.name = name;
			modFlags = mod;
			this.loc = loc;
			texpr = type;

			// Only assign, attributes will be attached during resolve
			base.attributes = attrs;
		}

		#region Properties

		public Expression DefaultExpression {
			get {
				return default_expr;
			}
		}

		public DefaultParameterValueExpression DefaultValue {
			get {
				return default_expr as DefaultParameterValueExpression;
			}
			set {
				default_expr = value;
			}
		}

		Expression IParameterData.DefaultValue {
			get {
				var expr = default_expr as DefaultParameterValueExpression;
				return expr == null ? default_expr : expr.Child;
			}
		}

		bool HasOptionalExpression {
			get {
				return default_expr is DefaultParameterValueExpression;
			}
		}

		public Location Location {
			get {
				return loc;
			}
		}

		public Modifier ParameterModifier {
			get {
				return modFlags;
			}
		}

		public TypeSpec Type {
			get {
				return parameter_type;
			}
			set {
				parameter_type = value;
			}
		}

		public FullNamedExpression TypeExpression  {
			get {
				return texpr;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		#endregion

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.In && ModFlags == Modifier.OUT) {
				a.Report.Error (36, a.Location, "An out parameter cannot have the `In' attribute");
				return;
			}

			if (a.Type == pa.ParamArray) {
				a.Report.Error (674, a.Location, "Do not use `System.ParamArrayAttribute'. Use the `params' keyword instead");
				return;
			}

			if (a.Type == pa.Out && (ModFlags & Modifier.REF) != 0 &&
			    !OptAttributes.Contains (pa.In)) {
				a.Report.Error (662, a.Location,
					"Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither");
				return;
			}

			if (a.Type == pa.CLSCompliant) {
				a.Report.Warning (3022, 1, a.Location, "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead");
			} else if (a.Type == pa.DefaultParameterValue || a.Type == pa.OptionalParameter) {
				if (HasOptionalExpression) {
					a.Report.Error (1745, a.Location,
						"Cannot specify `{0}' attribute on optional parameter `{1}'",
						a.Type.GetSignatureForError ().Replace ("Attribute", ""), Name);
				}

				if (a.Type == pa.DefaultParameterValue)
					return;
			} else if (a.Type == pa.CallerMemberNameAttribute) {
				if ((modFlags & Modifier.CallerMemberName) == 0) {
					a.Report.Error (4022, a.Location,
						"The CallerMemberName attribute can only be applied to parameters with default value");
				}
			} else if (a.Type == pa.CallerLineNumberAttribute) {
				if ((modFlags & Modifier.CallerLineNumber) == 0) {
					a.Report.Error (4020, a.Location,
						"The CallerLineNumber attribute can only be applied to parameters with default value");
				}
			} else if (a.Type == pa.CallerFilePathAttribute) {
				if ((modFlags & Modifier.CallerFilePath) == 0) {
					a.Report.Error (4021, a.Location,
						"The CallerFilePath attribute can only be applied to parameters with default value");
				}
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}
		
		public virtual bool CheckAccessibility (InterfaceMemberBase member)
		{
			if (parameter_type == null)
				return true;

			return member.IsAccessibleAs (parameter_type);
		}

		bool IsValidCallerContext (MemberCore memberContext)
		{
			var m = memberContext as Method;
			if (m != null)
				return !m.IsPartialImplementation;

			return true;
		}

		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public virtual TypeSpec Resolve (IMemberContext rc, int index)
		{
			if (parameter_type != null)
				return parameter_type;

			if (attributes != null)
				attributes.AttachTo (this, rc);

			parameter_type = texpr.ResolveAsType (rc);
			if (parameter_type == null)
				return null;

			this.idx = index;

			if ((modFlags & Parameter.Modifier.RefOutMask) != 0 && parameter_type.IsSpecialRuntimeType) {
				rc.Module.Compiler.Report.Error (1601, Location, "Method or delegate parameter cannot be of type `{0}'",
					GetSignatureForError ());
				return null;
			}

			VarianceDecl.CheckTypeVariance (parameter_type,
				(modFlags & Parameter.Modifier.RefOutMask) != 0 ? Variance.None : Variance.Contravariant,
				rc);

			if (parameter_type.IsStatic) {
				rc.Module.Compiler.Report.Error (721, Location, "`{0}': static types cannot be used as parameters",
					texpr.GetSignatureForError ());
				return parameter_type;
			}

			if ((modFlags & Modifier.This) != 0 && (parameter_type.IsPointer || parameter_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)) {
				rc.Module.Compiler.Report.Error (1103, Location, "The extension method cannot be of type `{0}'",
					parameter_type.GetSignatureForError ());
			}

			return parameter_type;
		}

		void ResolveCallerAttributes (ResolveContext rc)
		{
			var pa = rc.Module.PredefinedAttributes;
			TypeSpec caller_type;
			Attribute callerMemberName = null, callerFilePath = null;

			foreach (var attr in attributes.Attrs) {
				var atype = attr.ResolveTypeForComparison ();
				if (atype == null)
					continue;

				if (atype == pa.CallerMemberNameAttribute) {
					caller_type = rc.BuiltinTypes.String;
					if (caller_type != parameter_type && !Convert.ImplicitReferenceConversionExists (caller_type, parameter_type)) {
						rc.Report.Error (4019, attr.Location,
							"The CallerMemberName attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'",
							caller_type.GetSignatureForError (), parameter_type.GetSignatureForError ());
					}

					if (!IsValidCallerContext (rc.CurrentMemberDefinition)) {
						rc.Report.Warning (4026, 1, attr.Location,
							"The CallerMemberName applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments",
							name);
					}

					modFlags |= Modifier.CallerMemberName;
					callerMemberName = attr;
					continue;
				}

				if (atype == pa.CallerLineNumberAttribute) {
					caller_type = rc.BuiltinTypes.Int;
					if (caller_type != parameter_type && !Convert.ImplicitStandardConversionExists (new IntConstant (caller_type, int.MaxValue, Location.Null), parameter_type)) {
						rc.Report.Error (4017, attr.Location,
							"The CallerLineNumberAttribute attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'",
							caller_type.GetSignatureForError (), parameter_type.GetSignatureForError ());
					}

					if (!IsValidCallerContext (rc.CurrentMemberDefinition)) {
						rc.Report.Warning (4024, 1, attr.Location,
							"The CallerLineNumberAttribute applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments",
							name);
					}

					modFlags |= Modifier.CallerLineNumber;
					continue;
				}

				if (atype == pa.CallerFilePathAttribute) {
					caller_type = rc.BuiltinTypes.String;
					if (caller_type != parameter_type && !Convert.ImplicitReferenceConversionExists (caller_type, parameter_type)) {
						rc.Report.Error (4018, attr.Location,
							"The CallerFilePath attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'",
							caller_type.GetSignatureForError (), parameter_type.GetSignatureForError ());
					}

					if (!IsValidCallerContext (rc.CurrentMemberDefinition)) {
						rc.Report.Warning (4025, 1, attr.Location,
							"The CallerFilePath applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments",
							name);
					}

					modFlags |= Modifier.CallerFilePath;
					callerFilePath = attr;
					continue;
				}
			}

			if ((modFlags & Modifier.CallerLineNumber) != 0) {
				if (callerMemberName != null) {
					rc.Report.Warning (7081, 1, callerMemberName.Location,
						"The CallerMemberNameAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerLineNumberAttribute",
						Name);
				}

				if (callerFilePath != null) {
					rc.Report.Warning (7082, 1, callerFilePath.Location,
						"The CallerFilePathAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerLineNumberAttribute",
						name);
				}
			}

			if ((modFlags & Modifier.CallerMemberName) != 0) {
				if (callerFilePath != null) {
					rc.Report.Warning (7080, 1, callerFilePath.Location,
						"The CallerMemberNameAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerFilePathAttribute",
						name);
				}

			}
		}

		public void ResolveDefaultValue (ResolveContext rc)
		{
			//
			// Default value was specified using an expression
			//
			if (default_expr != null) {
				((DefaultParameterValueExpression)default_expr).Resolve (rc, this);
				if (attributes != null)
					ResolveCallerAttributes (rc);

				return;
			}

			if (attributes == null)
				return;

			var pa = rc.Module.PredefinedAttributes;
			var def_attr = attributes.Search (pa.DefaultParameterValue);
			if (def_attr != null) {
				if (def_attr.Resolve () == null)
					return;

				var default_expr_attr = def_attr.GetParameterDefaultValue ();
				if (default_expr_attr == null)
					return;

				var dpa_rc = def_attr.CreateResolveContext ();
				default_expr = default_expr_attr.Resolve (dpa_rc);

				if (default_expr is BoxedCast)
					default_expr = ((BoxedCast) default_expr).Child;

				Constant c = default_expr as Constant;
				if (c == null) {
					if (parameter_type.BuiltinType == BuiltinTypeSpec.Type.Object) {
						rc.Report.Error (1910, default_expr.Location,
							"Argument of type `{0}' is not applicable for the DefaultParameterValue attribute",
							default_expr.Type.GetSignatureForError ());
					} else {
						rc.Report.Error (1909, default_expr.Location,
							"The DefaultParameterValue attribute is not applicable on parameters of type `{0}'",
							default_expr.Type.GetSignatureForError ());
					}

					default_expr = null;
					return;
				}

				if (TypeSpecComparer.IsEqual (default_expr.Type, parameter_type) ||
					(default_expr is NullConstant && TypeSpec.IsReferenceType (parameter_type) && !parameter_type.IsGenericParameter) ||
					parameter_type.BuiltinType == BuiltinTypeSpec.Type.Object ||
					parameter_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic) {
					return;
				}

				//
				// LAMESPEC: Some really weird csc behaviour which we have to mimic
				// User operators returning same type as parameter type are considered
				// valid for this attribute only
				//
				// struct S { public static implicit operator S (int i) {} }
				//
				// void M ([DefaultParameterValue (3)]S s)
				//
				var expr = Convert.ImplicitUserConversion (dpa_rc, default_expr, parameter_type, loc);
				if (expr != null && TypeSpecComparer.IsEqual (expr.Type, parameter_type)) {
					return;
				}
				
				rc.Report.Error (1908, default_expr.Location, "The type of the default value should match the type of the parameter");
				return;
			}

			var opt_attr = attributes.Search (pa.OptionalParameter);
			if (opt_attr != null) {
				default_expr = EmptyExpression.MissingValue;
			}
		}

		public bool HasDefaultValue {
			get { return default_expr != null; }
		}

		public bool HasExtensionMethodModifier {
			get { return (modFlags & Modifier.This) != 0; }
		}

		//
		// Hoisted parameter variant
		//
		public HoistedParameter HoistedVariant {
			get {
				return hoisted_variant;
			}
			set {
				hoisted_variant = value;
			}
		}

		public Modifier ModFlags {
			get { return modFlags & ~Modifier.This; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}

		public void Error_DuplicateName (Report r)
		{
			r.Error (100, Location, "The parameter name `{0}' is a duplicate", Name);
		}

		public virtual string GetSignatureForError ()
		{
			string type_name;
			if (parameter_type != null)
				type_name = parameter_type.GetSignatureForError ();
			else
				type_name = texpr.GetSignatureForError ();

			string mod = GetModifierSignature (modFlags);
			if (mod.Length > 0)
				return String.Concat (mod, " ", type_name);

			return type_name;
		}

		public static string GetModifierSignature (Modifier mod)
		{
			switch (mod) {
			case Modifier.OUT:
				return "out";
			case Modifier.PARAMS:
				return "params";
			case Modifier.REF:
				return "ref";
			case Modifier.This:
				return "this";
			default:
				return "";
			}
		}

		public void IsClsCompliant (IMemberContext ctx)
		{
			if (parameter_type.IsCLSCompliant ())
				return;

			ctx.Module.Compiler.Report.Warning (3001, 1, Location,
				"Argument type `{0}' is not CLS-compliant", parameter_type.GetSignatureForError ());
		}

		public virtual void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
			if (builder != null)
				throw new InternalErrorException ("builder already exists");

			var pattrs = ParametersCompiled.GetParameterAttribute (modFlags);
			if (HasOptionalExpression)
				pattrs |= ParameterAttributes.Optional;

			if (mb == null)
				builder = cb.DefineParameter (index, pattrs, Name);
			else
				builder = mb.DefineParameter (index, pattrs, Name);

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (HasDefaultValue && default_expr.Type != null) {
				//
				// Emit constant values for true constants only, the other
				// constant-like expressions will rely on default value expression
				//
				var def_value = DefaultValue;
				Constant c = def_value != null ? def_value.Child as Constant : default_expr as Constant;
				if (c != null) {
					if (c.Type.BuiltinType == BuiltinTypeSpec.Type.Decimal) {
						pa.DecimalConstant.EmitAttribute (builder, (decimal) c.GetValue (), c.Location);
					} else {
						builder.SetConstant (c.GetValue ());
					}
				} else if (default_expr.Type.IsStruct || default_expr.Type.IsGenericParameter) {
					//
					// Handles special case where default expression is used with value-type or type parameter
					//
					// void Foo (S s = default (S)) {}
					//
					builder.SetConstant (null);
				}
			}

			if (parameter_type != null) {
				if (parameter_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic) {
					pa.Dynamic.EmitAttribute (builder);
				} else if (parameter_type.HasDynamicElement) {
					pa.Dynamic.EmitAttribute (builder, parameter_type, Location);
				}
			}
		}

		public Parameter Clone ()
		{
			Parameter p = (Parameter) MemberwiseClone ();
			if (attributes != null)
				p.attributes = attributes.Clone ();

			return p;
		}

		public ExpressionStatement CreateExpressionTreeVariable (BlockContext ec)
		{
			if ((modFlags & Modifier.RefOutMask) != 0)
				ec.Report.Error (1951, Location, "An expression tree parameter cannot use `ref' or `out' modifier");

			expr_tree_variable = TemporaryVariableReference.Create (ResolveParameterExpressionType (ec, Location).Type, ec.CurrentBlock.ParametersBlock, Location);
			expr_tree_variable = (TemporaryVariableReference) expr_tree_variable.Resolve (ec);

			Arguments arguments = new Arguments (2);
			arguments.Add (new Argument (new TypeOf (parameter_type, Location)));
			arguments.Add (new Argument (new StringConstant (ec.BuiltinTypes, Name, Location)));
			return new SimpleAssign (ExpressionTreeVariableReference (),
				Expression.CreateExpressionFactoryCall (ec, "Parameter", null, arguments, Location));
		}

		public void Emit (EmitContext ec)
		{
			ec.EmitArgumentLoad (idx);
		}

		public void EmitAssign (EmitContext ec)
		{
			ec.EmitArgumentStore (idx);
		}

		public void EmitAddressOf (EmitContext ec)
		{
			if ((ModFlags & Modifier.RefOutMask) != 0) {
				ec.EmitArgumentLoad (idx);
			} else {
				ec.EmitArgumentAddress (idx);
			}
		}

		public TemporaryVariableReference ExpressionTreeVariableReference ()
		{
			return expr_tree_variable;
		}

		//
		// System.Linq.Expressions.ParameterExpression type
		//
		public static TypeExpr ResolveParameterExpressionType (IMemberContext ec, Location location)
		{
			TypeSpec p_type = ec.Module.PredefinedTypes.ParameterExpression.Resolve ();
			return new TypeExpression (p_type, location);
		}

		public void SetIndex (int index)
		{
			idx = index;
		}

		public void Warning_UselessOptionalParameter (Report Report)
		{
			Report.Warning (1066, 1, Location,
				"The default value specified for optional parameter `{0}' will never be used",
				Name);
		}
	}

	//
	// Imported or resolved parameter information
	//
	public class ParameterData : IParameterData
	{
		readonly string name;
		readonly Parameter.Modifier modifiers;
		readonly Expression default_value;

		public ParameterData (string name, Parameter.Modifier modifiers)
		{
			this.name = name;
			this.modifiers = modifiers;
		}

		public ParameterData (string name, Parameter.Modifier modifiers, Expression defaultValue)
			: this (name, modifiers)
		{
			this.default_value = defaultValue;
		}

		#region IParameterData Members

		public Expression DefaultValue {
			get { return default_value; }
		}

		public bool HasExtensionMethodModifier {
			get { return (modifiers & Parameter.Modifier.This) != 0; }
		}

		public bool HasDefaultValue {
			get { return default_value != null; }
		}

		public Parameter.Modifier ModFlags {
			get { return modifiers; }
		}

		public string Name {
			get { return name; }
		}

		#endregion
	}

	public abstract class AParametersCollection
	{
		protected bool has_arglist;
		protected bool has_params;

		// Null object pattern
		protected IParameterData [] parameters;
		protected TypeSpec [] types;

		public CallingConventions CallingConvention {
			get {
				return has_arglist ?
					CallingConventions.VarArgs :
					CallingConventions.Standard;
			}
		}

		public int Count {
			get { return parameters.Length; }
		}

		public TypeSpec ExtensionMethodType {
			get {
				if (Count == 0)
					return null;

				return FixedParameters [0].HasExtensionMethodModifier ?
					types [0] : null;
			}
		}

		public IParameterData [] FixedParameters {
			get {
				return parameters;
			}
		}

		public static ParameterAttributes GetParameterAttribute (Parameter.Modifier modFlags)
		{
			return (modFlags & Parameter.Modifier.OUT) != 0 ?
				ParameterAttributes.Out : ParameterAttributes.None;
		}

		// Very expensive operation
		public MetaType[] GetMetaInfo ()
		{
			MetaType[] types;
			if (has_arglist) {
				if (Count == 1)
					return MetaType.EmptyTypes;

				types = new MetaType[Count - 1];
			} else {
				if (Count == 0)
					return MetaType.EmptyTypes;

				types = new MetaType[Count];
			}

			for (int i = 0; i < types.Length; ++i) {
				types[i] = Types[i].GetMetaInfo ();

				if ((FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) == 0)
					continue;

				// TODO MemberCache: Should go to MetaInfo getter
				types [i] = types [i].MakeByRefType ();
			}

			return types;
		}

		//
		// Returns the parameter information based on the name
		//
		public int GetParameterIndexByName (string name)
		{
			for (int idx = 0; idx < Count; ++idx) {
				if (parameters [idx].Name == name)
					return idx;
			}

			return -1;
		}

		public string GetSignatureForDocumentation ()
		{
			if (IsEmpty)
				return string.Empty;

			StringBuilder sb = new StringBuilder ("(");
			for (int i = 0; i < Count; ++i) {
				if (i != 0)
					sb.Append (",");

				sb.Append (types [i].GetSignatureForDocumentation ());

				if ((parameters[i].ModFlags & Parameter.Modifier.RefOutMask) != 0)
					sb.Append ("@");
			}
			sb.Append (")");

			return sb.ToString ();
		}

		public string GetSignatureForError ()
		{
			return GetSignatureForError ("(", ")", Count);
		}

		public string GetSignatureForError (string start, string end, int count)
		{
			StringBuilder sb = new StringBuilder (start);
			for (int i = 0; i < count; ++i) {
				if (i != 0)
					sb.Append (", ");
				sb.Append (ParameterDesc (i));
			}
			sb.Append (end);
			return sb.ToString ();
		}

		public static bool HasSameParameterDefaults (AParametersCollection a, AParametersCollection b)
		{
			if (a == null)
				return b == null;

			for (int i = 0; i < a.Count; ++i) {
				if (a.FixedParameters [i].HasDefaultValue != b.FixedParameters [i].HasDefaultValue)
					return false;
			}

			return true;
		}

		public bool HasArglist {
			get { return has_arglist; }
		}

		public bool HasExtensionMethodType {
			get {
				if (Count == 0)
					return false;

				return FixedParameters [0].HasExtensionMethodModifier;
			}
		}

		public bool HasParams {
			get { return has_params; }
		}

		public bool IsEmpty {
			get { return parameters.Length == 0; }
		}

		public AParametersCollection Inflate (TypeParameterInflator inflator)
		{
			TypeSpec[] inflated_types = null;
			bool default_value = false;

			for (int i = 0; i < Count; ++i) {
				var inflated_param = inflator.Inflate (types[i]);
				if (inflated_types == null) {
					if (inflated_param == types[i])
						continue;

					default_value |= FixedParameters[i].HasDefaultValue;
					inflated_types = new TypeSpec[types.Length];
					Array.Copy (types, inflated_types, types.Length);
				} else {
					if (inflated_param == types[i])
						continue;

					default_value |= FixedParameters[i].HasDefaultValue;
				}

				inflated_types[i] = inflated_param;
			}

			if (inflated_types == null)
				return this;

			var clone = (AParametersCollection) MemberwiseClone ();
			clone.types = inflated_types;

			//
			// Default expression is original expression from the parameter
			// declaration context which can be of nested enum in generic class type.
			// In such case we end up with expression type of G<T>.E and e.g. parameter
			// type of G<int>.E and conversion would fail without inflate in this
			// context.
			//
			if (default_value) {
				clone.parameters = new IParameterData[Count];
				for (int i = 0; i < Count; ++i) {
					var fp = FixedParameters[i];
					clone.FixedParameters[i] = fp;

					if (!fp.HasDefaultValue)
						continue;

					var expr = fp.DefaultValue;

					if (inflated_types[i] == expr.Type)
						continue;

					var c = expr as Constant;
					if (c != null) {
						//
						// It may fail we are inflating before type validation is done
						//
						c = Constant.ExtractConstantFromValue (inflated_types[i], c.GetValue (), expr.Location);
						if (c == null)
							expr = new DefaultValueExpression (new TypeExpression (inflated_types[i], expr.Location), expr.Location);
						else
							expr = c;
					} else if (expr is DefaultValueExpression)
						expr = new DefaultValueExpression (new TypeExpression (inflated_types[i], expr.Location), expr.Location);

					clone.FixedParameters[i] = new ParameterData (fp.Name, fp.ModFlags, expr);
				}
			}

			return clone;
		}

		public string ParameterDesc (int pos)
		{
			if (types == null || types [pos] == null)
				return ((Parameter)FixedParameters [pos]).GetSignatureForError ();

			string type = types [pos].GetSignatureForError ();
			if (FixedParameters [pos].HasExtensionMethodModifier)
				return "this " + type;

			var mod = FixedParameters[pos].ModFlags & Parameter.Modifier.ModifierMask;
			if (mod == 0)
				return type;

			return Parameter.GetModifierSignature (mod) + " " + type;
		}

		public TypeSpec[] Types {
			get { return types; }
			set { types = value; }
		}
	}

	//
	// A collection of imported or resolved parameters
	//
	public class ParametersImported : AParametersCollection
	{
		public ParametersImported (IParameterData [] parameters, TypeSpec [] types, bool hasArglist, bool hasParams)
		{
			this.parameters = parameters;
			this.types = types;
			this.has_arglist = hasArglist;
			this.has_params = hasParams;
		}

		public ParametersImported (IParameterData[] param, TypeSpec[] types, bool hasParams)
		{
			this.parameters = param;
			this.types = types;
			this.has_params = hasParams;
		}
	}

	/// <summary>
	///   Represents the methods parameters
	/// </summary>
	public class ParametersCompiled : AParametersCollection
	{
		public static readonly ParametersCompiled EmptyReadOnlyParameters = new ParametersCompiled ();
		
		// Used by C# 2.0 delegates
		public static readonly ParametersCompiled Undefined = new ParametersCompiled ();

		private ParametersCompiled ()
		{
			parameters = new Parameter [0];
			types = TypeSpec.EmptyTypes;
		}

		private ParametersCompiled (IParameterData[] parameters, TypeSpec[] types)
		{
			this.parameters = parameters;
		    this.types = types;
		}
		
		public ParametersCompiled (params Parameter[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
				throw new ArgumentException ("Use EmptyReadOnlyParameters");

			this.parameters = parameters;
			int count = parameters.Length;

			for (int i = 0; i < count; i++){
				has_params |= (parameters [i].ModFlags & Parameter.Modifier.PARAMS) != 0;
			}
		}

		public ParametersCompiled (Parameter [] parameters, bool has_arglist) :
			this (parameters)
		{
			this.has_arglist = has_arglist;
		}
		
		public static ParametersCompiled CreateFullyResolved (Parameter p, TypeSpec type)
		{
			return new ParametersCompiled (new Parameter [] { p }, new TypeSpec [] { type });
		}
		
		public static ParametersCompiled CreateFullyResolved (Parameter[] parameters, TypeSpec[] types)
		{
			return new ParametersCompiled (parameters, types);
		}

		public static ParametersCompiled Prefix (ParametersCompiled parameters, Parameter p, TypeSpec type)
		{
			var ptypes = new TypeSpec [parameters.Count + 1];
			ptypes [0] = type;
			Array.Copy (parameters.Types, 0, ptypes, 1, parameters.Count);

			var param = new Parameter [ptypes.Length];
			param [0] = p;
			for (int i = 0; i < parameters.Count; ++i) {
				var pi = parameters [i];
				param [i + 1] = pi;
				pi.SetIndex (i + 1);
			}

			return ParametersCompiled.CreateFullyResolved (param, ptypes);
		}

		//
		// TODO: This does not fit here, it should go to different version of AParametersCollection
		// as the underlying type is not Parameter and some methods will fail to cast
		//
		public static AParametersCollection CreateFullyResolved (params TypeSpec[] types)
		{
			var pd = new ParameterData [types.Length];
			for (int i = 0; i < pd.Length; ++i)
				pd[i] = new ParameterData (null, Parameter.Modifier.NONE, null);

			return new ParametersCompiled (pd, types);
		}

		public static ParametersCompiled CreateImplicitParameter (FullNamedExpression texpr, Location loc)
		{
			return new ParametersCompiled (
				new[] { new Parameter (texpr, "value", Parameter.Modifier.NONE, null, loc) },
				null);
		}

		public void CheckConstraints (IMemberContext mc)
		{
			foreach (Parameter p in parameters) {
				//
				// It's null for compiler generated types or special types like __arglist
				//
				if (p.TypeExpression != null)
					ConstraintChecker.Check (mc, p.Type, p.TypeExpression.Location);
			}
		}

		//
		// Returns non-zero value for equal CLS parameter signatures
		//
		public static int IsSameClsSignature (AParametersCollection a, AParametersCollection b)
		{
			int res = 0;

			for (int i = 0; i < a.Count; ++i) {
				var a_type = a.Types[i];
				var b_type = b.Types[i];
				if (TypeSpecComparer.Override.IsEqual (a_type, b_type)) {
					if ((a.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != (b.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
						res |= 1;

					continue;
				}

				var ac_a = a_type as ArrayContainer;
				if (ac_a == null)
					return 0;

				var ac_b = b_type as ArrayContainer;
				if (ac_b == null)
					return 0;

				if (ac_a.Element is ArrayContainer || ac_b.Element is ArrayContainer) {
					res |= 2;
					continue;
				}

				if (ac_a.Rank != ac_b.Rank && TypeSpecComparer.Override.IsEqual (ac_a.Element, ac_b.Element)) {
					res |= 1;
					continue;
				}

				return 0;
			}

			return res;
		}

		public static ParametersCompiled MergeGenerated (CompilerContext ctx, ParametersCompiled userParams, bool checkConflicts, Parameter compilerParams, TypeSpec compilerTypes)
		{
			return MergeGenerated (ctx, userParams, checkConflicts,
				new Parameter [] { compilerParams },
				new TypeSpec [] { compilerTypes });
		}

		//
		// Use this method when you merge compiler generated parameters with user parameters
		//
		public static ParametersCompiled MergeGenerated (CompilerContext ctx, ParametersCompiled userParams, bool checkConflicts, Parameter[] compilerParams, TypeSpec[] compilerTypes)
		{
			Parameter[] all_params = new Parameter [userParams.Count + compilerParams.Length];
			userParams.FixedParameters.CopyTo(all_params, 0);

			TypeSpec [] all_types;
			if (userParams.types != null) {
				all_types = new TypeSpec [all_params.Length];
				userParams.Types.CopyTo (all_types, 0);
			} else {
				all_types = null;
			}

			int last_filled = userParams.Count;
			int index = 0;
			foreach (Parameter p in compilerParams) {
				for (int i = 0; i < last_filled; ++i) {
					while (p.Name == all_params [i].Name) {
						if (checkConflicts && i < userParams.Count) {
							ctx.Report.Error (316, userParams[i].Location,
								"The parameter name `{0}' conflicts with a compiler generated name", p.Name);
						}
						p.Name = '_' + p.Name;
					}
				}
				all_params [last_filled] = p;
				if (all_types != null)
					all_types [last_filled] = compilerTypes [index++];
				++last_filled;
			}
			
			ParametersCompiled parameters = new ParametersCompiled (all_params, all_types);
			parameters.has_params = userParams.has_params;
			return parameters;
		}

		//
		// Parameters checks for members which don't have a block
		//
		public void CheckParameters (MemberCore member)
		{
			for (int i = 0; i < parameters.Length; ++i) {
				var name = parameters[i].Name;
				for (int ii = i + 1; ii < parameters.Length; ++ii) {
					if (parameters[ii].Name == name)
						this[ii].Error_DuplicateName (member.Compiler.Report);
				}
			}
		}

		public bool Resolve (IMemberContext ec)
		{
			if (types != null)
				return true;
			
			types = new TypeSpec [Count];
			
			bool ok = true;
			Parameter p;
			for (int i = 0; i < FixedParameters.Length; ++i) {
				p = this [i];
				TypeSpec t = p.Resolve (ec, i);
				if (t == null) {
					ok = false;
					continue;
				}

				types [i] = t;
			}

			return ok;
		}

		public void ResolveDefaultValues (MemberCore m)
		{
			ResolveContext rc = null;
			for (int i = 0; i < parameters.Length; ++i) {
				Parameter p = (Parameter) parameters [i];

				//
				// Try not to enter default values resolution if there are is not any default value possible
				//
				if (p.HasDefaultValue || p.OptAttributes != null) {
					if (rc == null)
						rc = new ResolveContext (m);

					p.ResolveDefaultValue (rc);
				}
			}
		}

		// Define each type attribute (in/out/ref) and
		// the argument names.
		public void ApplyAttributes (IMemberContext mc, MethodBase builder)
		{
			if (Count == 0)
				return;

			MethodBuilder mb = builder as MethodBuilder;
			ConstructorBuilder cb = builder as ConstructorBuilder;
			var pa = mc.Module.PredefinedAttributes;

			for (int i = 0; i < Count; i++) {
				this [i].ApplyAttributes (mb, cb, i + 1, pa);
			}
		}

		public void VerifyClsCompliance (IMemberContext ctx)
		{
			foreach (Parameter p in FixedParameters)
				p.IsClsCompliant (ctx);
		}

		public Parameter this [int pos] {
			get { return (Parameter) parameters [pos]; }
		}

		public Expression CreateExpressionTree (BlockContext ec, Location loc)
		{
			var initializers = new ArrayInitializer (Count, loc);
			foreach (Parameter p in FixedParameters) {
				//
				// Each parameter expression is stored to local variable
				// to save some memory when referenced later.
				//
				StatementExpression se = new StatementExpression (p.CreateExpressionTreeVariable (ec), Location.Null);
				if (se.Resolve (ec)) {
					ec.CurrentBlock.AddScopeStatement (new TemporaryVariableReference.Declarator (p.ExpressionTreeVariableReference ()));
					ec.CurrentBlock.AddScopeStatement (se);
				}
				
				initializers.Add (p.ExpressionTreeVariableReference ());
			}

			return new ArrayCreation (
				Parameter.ResolveParameterExpressionType (ec, loc),
				initializers, loc);
		}

		public ParametersCompiled Clone ()
		{
			ParametersCompiled p = (ParametersCompiled) MemberwiseClone ();

			p.parameters = new IParameterData [parameters.Length];
			for (int i = 0; i < Count; ++i)
				p.parameters [i] = this [i].Clone ();

			return p;
		}
	}

	//
	// Default parameter value expression. We need this wrapper to handle
	// default parameter values of folded constants (e.g. indexer parameters).
	// The expression is resolved only once but applied to two methods which
	// both share reference to this expression and we ensure that resolving
	// this expression always returns same instance
	//
	public class DefaultParameterValueExpression : CompositeExpression
	{
		public DefaultParameterValueExpression (Expression expr)
			: base (expr)
		{
		}

		public void Resolve (ResolveContext rc, Parameter p)
		{
			var expr = Resolve (rc);
			if (expr == null) {
				this.expr = ErrorExpression.Instance;
				return;
			}

			expr = Child;

			if (!(expr is Constant || expr is DefaultValueExpression || (expr is New && ((New) expr).IsGeneratedStructConstructor))) {
				if (!(expr is ErrorExpression)) {
					rc.Report.Error (1736, Location,
						"The expression being assigned to optional parameter `{0}' must be a constant or default value",
						p.Name);
				}

				return;
			}

			var parameter_type = p.Type;
			if (type == parameter_type)
				return;

			var res = Convert.ImplicitConversionStandard (rc, expr, parameter_type, Location);
			if (res != null) {
				if (parameter_type.IsNullableType && res is Nullable.Wrap) {
					Nullable.Wrap wrap = (Nullable.Wrap) res;
					res = wrap.Child;
					if (!(res is Constant)) {
						rc.Report.Error (1770, Location,
							"The expression being assigned to nullable optional parameter `{0}' must be default value",
							p.Name);
						return;
					}
				}

				if (!expr.IsNull && TypeSpec.IsReferenceType (parameter_type) && parameter_type.BuiltinType != BuiltinTypeSpec.Type.String) {
					rc.Report.Error (1763, Location,
						"Optional parameter `{0}' of type `{1}' can only be initialized with `null'",
						p.Name, parameter_type.GetSignatureForError ());

					return;
				}

				this.expr = res;
				return;
			}

			rc.Report.Error (1750, Location,
				"Optional parameter expression of type `{0}' cannot be converted to parameter type `{1}'",
				type.GetSignatureForError (), parameter_type.GetSignatureForError ());

			this.expr = ErrorExpression.Instance;
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}
}
