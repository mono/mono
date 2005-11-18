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
		public readonly Location Location;

		protected ParameterBase (Attributes attrs, Location loc)
			: base (attrs)
		{
			Location = loc;
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

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			builder.SetCustomAttribute (cb);
		}

		public override bool IsClsComplianceRequired(DeclSpace ds)
		{
			return false;
		}
	}

	/// <summary>
	/// Class for applying custom attributes on the return type
	/// </summary>
	public class ReturnParameter : ParameterBase {
		public ReturnParameter (MethodBuilder mb, Location location):
			base (null, location)
		{
			try {
				builder = mb.DefineParameter (0, ParameterAttributes.None, "");			
			}
			catch (ArgumentOutOfRangeException) {
				Report.RuntimeMissingSupport (Location, "custom attributes on the return type");
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
		public ImplicitParameter (MethodBuilder mb, Location loc):
			base (null, loc)
		{
			builder = mb.DefineParameter (1, ParameterAttributes.None, "");			
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
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

	public class ParamsParameter : Parameter {
		public ParamsParameter (Expression type, string name, Attributes attrs, Location loc):
			base (type, name, Parameter.Modifier.PARAMS, attrs, loc)
		{
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!base.Resolve (ec))
				return false;

			if (!parameter_type.IsArray || parameter_type.GetArrayRank () != 1) {
				Report.Error (225, Location, "The params parameter must be a single dimensional array");
				return false;
			}
			return true;
		}

		public override void ApplyAttributes (EmitContext ec, MethodBuilder mb, ConstructorBuilder cb, int index)
		{
			base.ApplyAttributes (ec, mb, cb, index);

			CustomAttributeBuilder a = new CustomAttributeBuilder (
				TypeManager.cons_param_array_attribute, new object [0]);
				
			builder.SetCustomAttribute (a);
		}
	}

	public class ArglistParameter : Parameter {
		// Doesn't have proper type because it's never choosed for better conversion
		public ArglistParameter () :
			base (typeof (ArglistParameter), "", Parameter.Modifier.ARGLIST, null, Location.Null)
		{
		}

		public override bool Resolve (EmitContext ec)
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
			OUTMASK = 64
		}

		static string[] attribute_targets = new string [] { "param" };

		public Expression TypeName;
		public readonly Modifier ModFlags;
		public readonly string Name;
		protected Type parameter_type;

		EmitContext ec;  // because ApplyAtrribute doesn't have ec
		
		public Parameter (Expression type, string name, Modifier mod, Attributes attrs, Location loc)
			: base (attrs, loc)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
		}

		public Parameter (Type type, string name, Modifier mod, Attributes attrs, Location loc)
			: base (attrs, loc)
		{
			Name = name;
			ModFlags = mod;
			parameter_type = type;
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

			if (a.Type == TypeManager.out_attribute_type && (ModFlags & Modifier.REF) != 0 &&
			    !OptAttributes.Contains (TypeManager.in_attribute_type, ec)) {
				Report.Error (662, a.Location,
					"Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither");
				return;
			}

			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				Report.Warning (3022, 1, a.Location, "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead");
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public virtual bool Resolve (EmitContext ec)
		{
			if (parameter_type != null)
				return true;

			this.ec = ec;

			TypeExpr texpr = TypeName.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return false;

			parameter_type = texpr.ResolveType (ec);
			
			if (parameter_type.IsAbstract && parameter_type.IsSealed) {
				Report.Error (721, Location, "`{0}': static types cannot be used as parameters", GetSignatureForError ());
				return false;
			}

			if (parameter_type == TypeManager.void_type){
				Report.Error (1536, Location, "Invalid parameter type 'void'");
				return false;
			}

			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0){
				if (parameter_type == TypeManager.typed_reference_type ||
				    parameter_type == TypeManager.arg_iterator_type){
					Report.Error (1601, Location, "Method or delegate parameter cannot be of type `{0}'",
						GetSignatureForError ());
					return false;
				}
			}
			
			return true;
		}

		public Type ExternalType ()
		{
			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0)
				return TypeManager.GetReferenceType (parameter_type);
			
			return parameter_type;
		}

		public Type ParameterType {
			get {
				return parameter_type;
			}
		}
		
		public ParameterAttributes Attributes {
			get {
				switch (ModFlags) {
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

			string mod = GetModifierSignature (ModFlags);
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

		public virtual void ApplyAttributes (EmitContext ec, MethodBuilder mb, ConstructorBuilder cb, int index)
		{
			if (mb == null)
				builder = cb.DefineParameter (index, Attributes, Name);
			else 
				builder = mb.DefineParameter (index, Attributes, Name);
		
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
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

		private Parameters ()
		{
			FixedParameters = new Parameter[0];
			types = new Type [0];
		}
		
		public Parameters (Parameter [] parameters)
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
		
		public bool Resolve (EmitContext ec)
		{
			if (types != null)
				return true;

			types = new Type [count];
			
			if (ec != null && !VerifyArgs ()){
				return false;
			}

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
		public void ApplyAttributes (EmitContext ec, MethodBase builder)
		{
			if (count == 0)
				return;

			MethodBuilder mb = builder as MethodBuilder;
			ConstructorBuilder cb = builder as ConstructorBuilder;

			for (int i = 0; i < FixedParameters.Length; i++) {
				FixedParameters [i].ApplyAttributes (ec, mb, cb, i + 1);
			}
		}

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
				if (HasArglist) {
					if (sb.Length > 1)
						sb.Append (", ");
					sb.Append ("__arglist");
				}
			}

			sb.Append (')');
			return sb.ToString ();
		}

		public Type[] Types {
			get {
				return types;
			}
		}

		Parameter this [int pos]
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

				return FixedParameters [count - 1] is ParamsParameter;
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

		#endregion
	}
}
