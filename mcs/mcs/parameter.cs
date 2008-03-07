//
// parameter.cs: Parameter definition.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;

namespace Mono.CSharp {

	/// <summary>
	///   Abstract Base class for parameters of a method.
	/// </summary>
	public abstract class ParameterBase : Attributable {

		protected ParameterBuilder builder;

		protected ParameterBase (Attributes attrs)
			: base (attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					builder.SetMarshal (marshal);
				}
				return;
			}

			if (a.HasSecurityAttribute) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			builder.SetCustomAttribute (cb);
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}
	}

	/// <summary>
	/// Class for applying custom attributes on the return type
	/// </summary>
	public class ReturnParameter : ParameterBase {
		public ReturnParameter (MethodBuilder mb, Location location):
			base (null)
		{
			try {
				builder = mb.DefineParameter (0, ParameterAttributes.None, "");			
			}
			catch (ArgumentOutOfRangeException) {
				Report.RuntimeMissingSupport (location, "custom attributes on the return type");
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				Report.Warning (3023, 1, a.Location, "CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead");
			}

			// This occurs after Warning -28
			if (builder == null)
				return;

			base.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.ReturnValue;
			}
		}

		public override IResolveContext ResolveContext {
			get {
				throw new NotSupportedException ();
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

	/// <summary>
	/// Class for applying custom attributes on the implicit parameter type
	/// of the 'set' method in properties, and the 'add' and 'remove' methods in events.
	/// </summary>
	/// 
	// TODO: should use more code from Parameter.ApplyAttributeBuilder
	public class ImplicitParameter : ParameterBase {
		public ImplicitParameter (MethodBuilder mb):
			base (null)
		{
			builder = mb.DefineParameter (1, ParameterAttributes.None, "");			
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}

		public override IResolveContext ResolveContext {
			get {
				throw new NotSupportedException ();
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
			: base ((Type)null, name, Modifier.NONE, null, loc)
		{
		}

		public override bool Resolve (IResolveContext ec)
		{
			if (parameter_type == null)
				throw new InternalErrorException ("A type of implicit lambda parameter `{0}' is not set",
					Name);

			return true;
		}
	}

	public class ParamsParameter : Parameter {
		public ParamsParameter (Expression type, string name, Attributes attrs, Location loc):
			base (type, name, Parameter.Modifier.PARAMS, attrs, loc)
		{
		}

		public override bool Resolve (IResolveContext ec)
		{
			if (!base.Resolve (ec))
				return false;

			if (!parameter_type.IsArray || parameter_type.GetArrayRank () != 1) {
				Report.Error (225, Location, "The params parameter must be a single dimensional array");
				return false;
			}
			return true;
		}

		public override void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index)
		{
			base.ApplyAttributes (mb, cb, index);

			CustomAttributeBuilder ca = TypeManager.param_array_attr;
			if (ca == null) {
				ConstructorInfo ci = TypeManager.GetPredefinedConstructor (TypeManager.param_array_type, Location, Type.EmptyTypes);
				if (ci == null)
					return;

				ca = new CustomAttributeBuilder (ci, new object [0]);
				if (ca == null)
					return;

				TypeManager.param_array_attr = ca;
			}
				
			builder.SetCustomAttribute (ca);
		}
	}

	public class ArglistParameter : Parameter {
		// Doesn't have proper type because it's never chosen for better conversion
		public ArglistParameter () :
			base (typeof (ArglistParameter), String.Empty, Parameter.Modifier.ARGLIST, null, Location.Null)
		{
		}

		public override bool CheckAccessibility (InterfaceMemberBase member)
		{
			return true;
		}

		public override bool Resolve (IResolveContext ec)
		{
			return true;
		}

		public override string GetSignatureForError ()
		{
			return "__arglist";
		}
	}

	/// <summary>
	///   Represents a single method parameter
	/// </summary>
	public class Parameter : ParameterBase {
		[Flags]
		public enum Modifier : byte {
			NONE    = 0,
			REF     = REFMASK | ISBYREF,
			OUT     = OUTMASK | ISBYREF,
			PARAMS  = 4,
			// This is a flag which says that it's either REF or OUT.
			ISBYREF = 8,
			ARGLIST = 16,
			REFMASK	= 32,
			OUTMASK = 64,
			This	= 128
		}

		static string[] attribute_targets = new string [] { "param" };

		Expression TypeName;
		public Modifier modFlags;
		public string Name;
		public bool IsCaptured;
		protected Type parameter_type;
		public readonly Location Location;

		IResolveContext resolve_context;
		LocalVariableReference expr_tree_variable;
		static TypeExpr parameter_expr_tree_type;

		Variable var;
		public Variable Variable {
			get { return var; }
		}

#if GMCS_SOURCE
		public bool IsTypeParameter;
		GenericConstraints constraints;
#else
	    	public bool IsTypeParameter {
			get {
				return false;
			}
			set {
				if (value)
					throw new Exception ("You can not se TypeParameter in MCS");
			}
		}
#endif
		
		public Parameter (Expression type, string name, Modifier mod, Attributes attrs, Location loc)
			: this (type.Type, name, mod, attrs, loc)
		{
			if (type == TypeManager.system_void_expr)
				Report.Error (1536, loc, "Invalid parameter type `void'");
			
			TypeName = type;
		}

		public Parameter (Type type, string name, Modifier mod, Attributes attrs, Location loc)
			: base (attrs)
		{
			Name = name;
			modFlags = mod;
			parameter_type = type;
			Location = loc;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.in_attribute_type && ModFlags == Modifier.OUT) {
				Report.Error (36, a.Location, "An out parameter cannot have the `In' attribute");
				return;
			}

			if (a.Type == TypeManager.param_array_type) {
				Report.Error (674, a.Location, "Do not use `System.ParamArrayAttribute'. Use the `params' keyword instead");
				return;
			}

			if (a.Type == TypeManager.out_attribute_type && (ModFlags & Modifier.REF) == Modifier.REF &&
			    TypeManager.in_attribute_type != null && !OptAttributes.Contains (TypeManager.in_attribute_type)) {
				Report.Error (662, a.Location,
					"Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither");
				return;
			}

			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				Report.Warning (3022, 1, a.Location, "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead");
			}

			// TypeManager.default_parameter_value_attribute_type is null if !NET_2_0, or if System.dll is not referenced
			if (a.Type == TypeManager.default_parameter_value_attribute_type) {
				object val = a.GetParameterDefaultValue ();
				if (val != null) {
					Type t = val.GetType ();
					if (t.IsArray || TypeManager.IsSubclassOf (t, TypeManager.type_type)) {
						if (parameter_type == TypeManager.object_type) {
							if (!t.IsArray)
								t = TypeManager.type_type;

							Report.Error (1910, a.Location, "Argument of type `{0}' is not applicable for the DefaultValue attribute",
								TypeManager.CSharpName (t));
						} else {
							Report.Error (1909, a.Location, "The DefaultValue attribute is not applicable on parameters of type `{0}'",
								TypeManager.CSharpName (parameter_type)); ;
						}
						return;
					}
				}

				if (parameter_type == TypeManager.object_type ||
				    (val == null && !TypeManager.IsValueType (parameter_type)) ||
				    (val != null && TypeManager.TypeToCoreType (val.GetType ()) == parameter_type))
					builder.SetConstant (val);
				else
					Report.Error (1908, a.Location, "The type of the default value should match the type of the parameter");
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}
		
		public virtual bool CheckAccessibility (InterfaceMemberBase member)
		{
			if (IsTypeParameter)
				return true;

			return member.IsAccessibleAs (parameter_type);
		}

		public override IResolveContext ResolveContext {
			get {
				return resolve_context;
			}
		}

		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public virtual bool Resolve (IResolveContext ec)
		{
			// HACK: to resolve attributes correctly
			this.resolve_context = ec;

			if (parameter_type != null)
				return true;

			TypeExpr texpr = TypeName.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return false;

			parameter_type = texpr.Type;

#if GMCS_SOURCE
			TypeParameterExpr tparam = texpr as TypeParameterExpr;
			if (tparam != null) {
				IsTypeParameter = true;
				constraints = tparam.TypeParameter.Constraints;
				return true;
			}
#endif

			if ((parameter_type.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
				Report.Error (721, Location, "`{0}': static types cannot be used as parameters", 
					texpr.GetSignatureForError ());
				return false;
			}

			if ((modFlags & Parameter.Modifier.ISBYREF) != 0){
				if (parameter_type == TypeManager.typed_reference_type ||
				    parameter_type == TypeManager.arg_iterator_type){
					Report.Error (1601, Location, "Method or delegate parameter cannot be of type `{0}'",
						GetSignatureForError ());
					return false;
				}
			}

			if ((modFlags & Modifier.This) != 0 && parameter_type.IsPointer) {
				Report.Error (1103, Location, "The type of extension method cannot be `{0}'",
					TypeManager.CSharpName (parameter_type));
				return false;
			}
			
			return true;
		}

		public void ResolveVariable (ToplevelBlock toplevel, int idx)
		{
			if (toplevel.RootScope != null)
				var = toplevel.RootScope.GetCapturedParameter (this);
			if (var == null)
				var = new ParameterVariable (this, idx);
		}

		public Type ExternalType ()
		{
			if ((modFlags & Parameter.Modifier.ISBYREF) != 0)
				return TypeManager.GetReferenceType (parameter_type);
			
			return parameter_type;
		}

		public bool HasExtensionMethodModifier {
			get { return (modFlags & Modifier.This) != 0; }
		}

		public Modifier ModFlags {
			get { return modFlags & ~Modifier.This; }
		}

		public Type ParameterType {
			get {
				return parameter_type;
			}
			set {
				parameter_type = value;
				IsTypeParameter = false;
			}
		}

#if GMCS_SOURCE
		public GenericConstraints GenericConstraints {
			get {
				return constraints;
			}
		}
#endif

		ParameterAttributes Attributes {
			get {
				return (modFlags & Modifier.OUT) == Modifier.OUT ?
					ParameterAttributes.Out : ParameterAttributes.None;
			}
		}

		// TODO: should be removed !!!!!!!
		public static ParameterAttributes GetParameterAttributes (Modifier mod)
		{
			int flags = ((int) mod) & ~((int) Parameter.Modifier.ISBYREF);
			switch ((Modifier) flags) {
			case Modifier.NONE:
				return ParameterAttributes.None;
			case Modifier.REF:
				return ParameterAttributes.None;
			case Modifier.OUT:
				return ParameterAttributes.Out;
			case Modifier.PARAMS:
				return 0;
			}
				
			return ParameterAttributes.None;
		}
		
		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}

		public virtual string GetSignatureForError ()
		{
			string type_name;
			if (parameter_type != null)
				type_name = TypeManager.CSharpName (parameter_type);
			else
				type_name = TypeName.GetSignatureForError ();

			string mod = GetModifierSignature (modFlags);
			if (mod.Length > 0)
				return String.Concat (mod, ' ', type_name);

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
				case Modifier.ARGLIST:
					return "__arglist";
				case Modifier.This:
					return "this";
				default:
					return "";
			}
		}

		public void IsClsCompliant ()
		{
			if (AttributeTester.IsClsCompliant (ExternalType ()))
				return;

			Report.Error (3001, Location, "Argument type `{0}' is not CLS-compliant", GetSignatureForError ());
		}

		public virtual void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index)
		{
#if !GMCS_SOURCE || !MS_COMPATIBLE
			// TODO: It should use mb.DefineGenericParameters
			if (mb == null)
				builder = cb.DefineParameter (index, Attributes, Name);
			else 
				builder = mb.DefineParameter (index, Attributes, Name);
#endif

			if (OptAttributes != null)
				OptAttributes.Emit ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected class ParameterVariable : Variable
		{
			public readonly Parameter Parameter;
			public readonly int Idx;
			public readonly bool IsRef;

			public ParameterVariable (Parameter par, int idx)
			{
				this.Parameter = par;
				this.Idx = idx;
				this.IsRef = (par.ModFlags & Parameter.Modifier.ISBYREF) != 0;
			}

			public override Type Type {
				get { return Parameter.ParameterType; }
			}

			public override bool HasInstance {
				get { return false; }
			}

			public override bool NeedsTemporary {
				get { return false; }
			}

			public override void EmitInstance (EmitContext ec)
			{
			}

			public override void Emit (EmitContext ec)
			{
				int arg_idx = Idx;
				if (!ec.MethodIsStatic)
					arg_idx++;

				ParameterReference.EmitLdArg (ec.ig, arg_idx);
			}

			public override void EmitAssign (EmitContext ec)
			{
				int arg_idx = Idx;
				if (!ec.MethodIsStatic)
					arg_idx++;

				if (arg_idx <= 255)
					ec.ig.Emit (OpCodes.Starg_S, (byte) arg_idx);
				else
					ec.ig.Emit (OpCodes.Starg, arg_idx);
			}

			public override void EmitAddressOf (EmitContext ec)
			{
				int arg_idx = Idx;

				if (!ec.MethodIsStatic)
					arg_idx++;

				if (IsRef) {
					if (arg_idx <= 255)
						ec.ig.Emit (OpCodes.Ldarg_S, (byte) arg_idx);
					else
						ec.ig.Emit (OpCodes.Ldarg, arg_idx);
				} else {
					if (arg_idx <= 255)
						ec.ig.Emit (OpCodes.Ldarga_S, (byte) arg_idx);
					else
						ec.ig.Emit (OpCodes.Ldarga, arg_idx);
				}
			}
		}

		public Parameter Clone ()
		{
			Parameter p = new Parameter (parameter_type, Name, ModFlags, attributes, Location);
			p.IsTypeParameter = IsTypeParameter;

			return p;
		}

		public ExpressionStatement CreateExpressionTreeVariable (EmitContext ec)
		{
			if ((modFlags & Modifier.ISBYREF) != 0)
				Report.Error (1951, Location, "An expression tree parameter cannot use `ref' or `out' modifier");

			LocalInfo variable = ec.CurrentBlock.AddTemporaryVariable (
				ResolveParameterExpressionType (ec, Location), Location);
			variable.Resolve (ec);

			expr_tree_variable = new LocalVariableReference (
				ec.CurrentBlock, variable.Name, Location, variable, false);

			ArrayList arguments = new ArrayList (2);
			arguments.Add (new Argument (new TypeOf (
				new TypeExpression (parameter_type, Location), Location)));
			arguments.Add (new Argument (new StringConstant (Name, Location)));
			return new Assign (ExpressionTreeVariableReference (),
				Expression.CreateExpressionFactoryCall ("Parameter", null, arguments, Location));
		}

		public Expression ExpressionTreeVariableReference ()
		{
			return expr_tree_variable;
		}

		//
		// System.Linq.Expressions.ParameterExpression type
		//
		public static TypeExpr ResolveParameterExpressionType (EmitContext ec, Location location)
		{
			if (parameter_expr_tree_type != null)
				return parameter_expr_tree_type;

			Type p_type = TypeManager.parameter_expression_type;
			if (p_type == null) {
				p_type = TypeManager.CoreLookupType ("System.Linq.Expressions", "ParameterExpression", Kind.Class, true);
				TypeManager.parameter_expression_type = p_type;
			}

			parameter_expr_tree_type = new TypeExpression (p_type, location).
				ResolveAsTypeTerminal (ec, false);

			return parameter_expr_tree_type;
		}
	}

	/// <summary>
	///   Represents the methods parameters
	/// </summary>
	public class Parameters : ParameterData {
		// Null object pattern
		public Parameter [] FixedParameters;
		public readonly bool HasArglist;
		Type [] types;
		int count;

		public static readonly Parameters EmptyReadOnlyParameters = new Parameters ();
		static readonly Parameter ArgList = new ArglistParameter ();

#if GMCS_SOURCE
//		public readonly TypeParameter[] TypeParameters;
#endif

		private Parameters ()
		{
			FixedParameters = new Parameter[0];
			types = new Type [0];
		}

		private Parameters (Parameter[] parameters, Type[] types)
		{
			FixedParameters = parameters;
			this.types = types;
			count = types.Length;
		}
		
		public Parameters (params Parameter[] parameters)
		{
			if (parameters == null)
				throw new ArgumentException ("Use EmptyReadOnlyPatameters");

			FixedParameters = parameters;
			count = parameters.Length;
		}

		public Parameters (Parameter[] parameters, bool has_arglist):
			this (parameters)
		{
			HasArglist = has_arglist;
		}
		
		public static Parameters CreateFullyResolved (Parameter p)
		{
			return new Parameters (new Parameter [] { p }, new Type [] { p.ParameterType });
		}
		
		public static Parameters CreateFullyResolved (Parameter[] parameters, Type[] types)
		{
			return new Parameters (parameters, types);
		}

		/// <summary>
		/// Use this method when you merge compiler generated argument with user arguments
		/// </summary>
		public static Parameters MergeGenerated (Parameters userParams, params Parameter[] compilerParams)
		{
			Parameter[] all_params = new Parameter [userParams.count + compilerParams.Length];
			Type[] all_types = new Type[all_params.Length];
			userParams.FixedParameters.CopyTo(all_params, 0);
			userParams.Types.CopyTo (all_types, 0);

			int last_filled = userParams.Count;
			foreach (Parameter p in compilerParams) {
				for (int i = 0; i < last_filled; ++i) {
					while (p.Name == all_params [i].Name) {
						p.Name = '_' + p.Name;
					}
				}
				all_params [last_filled] = p;
				all_types [last_filled] = p.ParameterType;
				++last_filled;
			}
			
			return new Parameters (all_params, all_types);
		}

		public bool Empty {
			get {
				return count == 0;
			}
		}

		public int Count {
			get {
				return HasArglist ? count + 1 : count;
			}
		}

		//
		// The property can be used after parameter types were resolved.
		//
		public Type ExtensionMethodType {
			get {
				if (count == 0)
					return null;

				return FixedParameters [0].HasExtensionMethodModifier ?
					types [0] : null;
			}
		}

		public bool HasExtensionMethodType {
			get {
				if (count == 0)
					return false;

				return FixedParameters [0].HasExtensionMethodModifier;
			}
		}


		bool VerifyArgs ()
		{
			if (count < 2)
				return true;

			for (int i = 0; i < count; i++){
				string base_name = FixedParameters [i].Name;
				for (int j = i + 1; j < count; j++){
					if (base_name != FixedParameters [j].Name)
						continue;

					Report.Error (100, FixedParameters [i].Location,
						"The parameter name `{0}' is a duplicate", base_name);
					return false;
				}
			}
			return true;
		}
		
		
		/// <summary>
		///    Returns the paramenter information based on the name
		/// </summary>
		public Parameter GetParameterByName (string name, out int idx)
		{
			idx = 0;

			if (count == 0)
				return null;

			int i = 0;

			foreach (Parameter par in FixedParameters){
				if (par.Name == name){
					idx = i;
					return par;
				}
				i++;
			}
			return null;
		}

		public Parameter GetParameterByName (string name)
		{
			int idx;

			return GetParameterByName (name, out idx);
		}
		
		public bool Resolve (IResolveContext ec)
		{
			if (types != null)
				return true;

			types = new Type [count];
			
			if (!VerifyArgs ())
				return false;

			bool ok = true;
			Parameter p;
			for (int i = 0; i < FixedParameters.Length; ++i) {
				p = FixedParameters [i];
				if (!p.Resolve (ec)) {
					ok = false;
					continue;
				}
				types [i] = p.ExternalType ();
			}

			return ok;
		}

		public void ResolveVariable (ToplevelBlock toplevel)
		{
			for (int i = 0; i < FixedParameters.Length; ++i) {
				Parameter p = FixedParameters [i];
				p.ResolveVariable (toplevel, i);
			}
		}

		public CallingConventions CallingConvention
		{
			get {
				if (HasArglist)
					return CallingConventions.VarArgs;
				else
					return CallingConventions.Standard;
			}
		}

		// Define each type attribute (in/out/ref) and
		// the argument names.
		public void ApplyAttributes (MethodBase builder)
		{
			if (count == 0)
				return;

			MethodBuilder mb = builder as MethodBuilder;
			ConstructorBuilder cb = builder as ConstructorBuilder;

			for (int i = 0; i < FixedParameters.Length; i++) {
				FixedParameters [i].ApplyAttributes (mb, cb, i + 1);
			}
		}

#if MS_COMPATIBLE
		public ParameterData InflateTypes (Type[] genArguments, Type[] argTypes)
		{
			Parameters p = Clone ();
			for (int i = 0; i < count; ++i) {
				if (types[i].IsGenericType) {
					Type[] gen_arguments_open = new Type [types[i].GetGenericTypeDefinition ().GetGenericArguments ().Length];
					Type[] gen_arguments = types[i].GetGenericArguments ();
					for (int ii = 0; ii < gen_arguments_open.Length; ++ii) {
						if (gen_arguments[ii].IsGenericParameter) {
							Type t = argTypes[gen_arguments[ii].GenericParameterPosition];
							gen_arguments_open[ii] = t;
						} else
							gen_arguments_open[ii] = gen_arguments[ii];
					}

					p.FixedParameters [i].ParameterType = p.types[i] =
						types[i].GetGenericTypeDefinition ().MakeGenericType (gen_arguments_open);
					continue;
				}

				if (types[i].IsGenericParameter) {
					Type gen_argument = argTypes[types[i].GenericParameterPosition];
					p.FixedParameters[i].ParameterType = p.types[i] = gen_argument;
					continue;
				}
			}

			return p;
		}
#endif

		public void VerifyClsCompliance ()
		{
			foreach (Parameter p in FixedParameters)
				p.IsClsCompliant ();
		}

		public string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ("(");
			if (count > 0) {
				for (int i = 0; i < FixedParameters.Length; ++i) {
					sb.Append (FixedParameters[i].GetSignatureForError ());
					if (i < FixedParameters.Length - 1)
						sb.Append (", ");
				}
			}

			if (HasArglist) {
				if (sb.Length > 1)
					sb.Append (", ");
				sb.Append ("__arglist");
			}

			sb.Append (')');
			return sb.ToString ();
		}

		public Type[] Types {
			get {
				return types;
			}
			//
			// Dangerous, used by implicit lambda parameters
			// only to workaround bad design
			//
			set {
				types = value;
			}
		}

		public Parameter this [int pos]
		{
			get {
				if (pos >= count && (HasArglist || HasParams)) {
					if (HasArglist && (pos == 0 || pos >= count))
						return ArgList;
					pos = count - 1;
				}

				return FixedParameters [pos];
			}
		}

		#region ParameterData Members

		public Type ParameterType (int pos)
		{
			return this [pos].ExternalType ();
		}

		public bool HasParams {
			get {
				if (count == 0)
					return false;

				for (int i = count; i != 0; --i) {
					if ((FixedParameters [i - 1].ModFlags & Parameter.Modifier.PARAMS) != 0)
						return true;
				}
				return false;
			}
		}

		public string ParameterName (int pos)
		{
			return this [pos].Name;
		}

		public string ParameterDesc (int pos)
		{
			return this [pos].GetSignatureForError ();
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			return this [pos].ModFlags;
		}

		public Expression CreateExpressionTree (EmitContext ec, Location loc)
		{
			ArrayList initializers = new ArrayList (count);
			foreach (Parameter p in FixedParameters) {
				//
				// Each parameter expression is stored to local variable
				// to save some memory when referenced later.
				//
				StatementExpression se = new StatementExpression (p.CreateExpressionTreeVariable (ec));
				if (se.Resolve (ec))
					ec.CurrentBlock.AddScopeStatement (se);
				
				initializers.Add (p.ExpressionTreeVariableReference ());
			}

			return new ArrayCreation (
				Parameter.ResolveParameterExpressionType (ec, loc),
				"[]", initializers, loc);
		}

		public Parameters Clone ()
		{
			Parameter [] parameters_copy = new Parameter [FixedParameters.Length];
			int i = 0;
			foreach (Parameter p in FixedParameters)
				parameters_copy [i++] = p.Clone ();
			Parameters ps = new Parameters (parameters_copy, HasArglist);
			if (types != null)
				ps.types = (Type[])types.Clone ();
			return ps;
		}
		
		#endregion
	}
}
