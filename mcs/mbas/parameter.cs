//
// parameter.cs: Parameter definition.
//
// Author: Miguel de Icaza (miguel@gnu.org)
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

namespace Mono.MonoBASIC {


	/// <summary>
	///   Represents a single method parameter
	/// </summary>
	public class Parameter : Attributable {
		[Flags]
		public enum Modifier : byte {
			NONE    = 0,
			VAL 	= 0,
			REF     = 1,
			OUT     = 2,
			PARAMS  = 4,
			// This is a flag which says that it's either REF or OUT.
			ISBYREF = 8,
			OPTIONAL = 16
		}

		public readonly Expression TypeName;
		public readonly Modifier ModFlags;
		public readonly string Name;
		public Type parameter_type;
		public readonly Expression ParameterInitializer;
		public readonly bool IsOptional;

		public Parameter (Expression type, string name, Modifier mod, Attributes attrs)
			: base (attrs)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
			ParameterInitializer = null;
			IsOptional = false;
		}

		public Parameter (Expression type, string name, Modifier mod, Attributes attrs, Expression pi)
			: base (attrs)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
			ParameterInitializer = pi;
			IsOptional = false;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			throw new Exception ("FIXME: I am just a placeholder implementation");
		}
		
		public Parameter (Expression type, string name, Modifier mod, Attributes attrs, Expression pi, bool opt)
			: base (attrs)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
			ParameterInitializer = pi;
			IsOptional = opt;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}
		
		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public bool Resolve (DeclSpace ds, Location l)
		{
			parameter_type = ds.ResolveType (TypeName, false, l);

			if (parameter_type == TypeManager.void_type){
				Report.Error (1536, l, "`void' parameter is not permitted");
				return false;
			}
			
			return parameter_type != null;
		}

		public Type ExternalType (DeclSpace ds, Location l)
		{
			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0){
				string n = parameter_type.FullName + "&";

				Type t = RootContext.LookupType (ds, n, false, l);

				return t;
			}
			
			return parameter_type;
		}

		public Type ParameterType {
			get {
				return parameter_type;
			}
		}
		
		public ParameterAttributes Attributes {
			get {
				int flags = ((int) ModFlags) & ~((int) Parameter.Modifier.ISBYREF);
				switch ((Modifier) flags) {
				case Modifier.NONE:
					return ParameterAttributes.None;
				case Modifier.REF:
					return ParameterAttributes.None;
				case Modifier.OUT:
					return ParameterAttributes.Out;
				case Modifier.OPTIONAL:
					return ParameterAttributes.Optional;
				case Modifier.PARAMS:
					return 0;
				}
				
				return ParameterAttributes.None;
			}
		}
		
		/// <summary>
		///   Returns the signature for this parameter evaluating it on the
		///   @tc context
		/// </summary>
		public string GetSignature (DeclSpace ds, Location loc)
		{
			if (parameter_type == null){
				if (!Resolve (ds, loc))
					return null;
			}

			return ExternalType (ds, loc).FullName;
		}
	}

	/// <summary>
	///   Represents the methods parameters
	/// </summary>
	public class Parameters {
		public Parameter [] FixedParameters;
		public readonly Parameter ArrayParameter;
		string signature;
		Type [] types;
		Location loc;
		
		static Parameters empty_parameters;
		
		public Parameters (Parameter [] fixed_parameters, Parameter array_parameter, Location l)
		{
			FixedParameters = fixed_parameters;
			ArrayParameter  = array_parameter;
			loc = l;
		}
	
		/// <summary>
		///   This is used to reuse a set of empty parameters, because they
		///   are common
		/// </summary>
		public static Parameters EmptyReadOnlyParameters {
			get {
				if (empty_parameters == null)
					empty_parameters = new Parameters (null, null, Location.Null);
			
				return empty_parameters;
			}
		}
		
		public bool HasOptional()
		{
			bool res = false;

			foreach (Parameter p in FixedParameters) 
			{
				if (p.IsOptional) 
				{
					res = true;
					break;
				}
			}
			return (res);
		}
		
		/// <summary>
		///   Returns the number of standard (i.e. non-optional) parameters
		/// </summary>		
		public int CountStandardParams()
		{
			int res = 0;
			if (FixedParameters == null)
				return 0;

			foreach (Parameter p in FixedParameters) {
				if (!p.IsOptional)
					res++;
			}
			return (res);
		}
		
		/// <summary>
		///   Returns the number of optional parameters
		/// </summary>		
		public int CountOptionalParams()
		{
			int res = 0;
			if (FixedParameters == null)
				return 0;
							
			foreach (Parameter p in FixedParameters) {
				if (p.IsOptional)
					res++;
			}
			return (res);
		}		

		public Expression GetDefaultValue (int i)
		{
			Parameter p = FixedParameters[i];
			if (p.IsOptional)
				return p.ParameterInitializer;
			else
				return null;
		}

		public void AppendParameter (Parameter p)
		{
			if (FixedParameters != null) 
			{
				Parameter [] pa = new Parameter [FixedParameters.Length+1];
				FixedParameters.CopyTo (pa, 0);
				pa[FixedParameters.Length] = p;
				FixedParameters = pa;		
			}
			else
			{
				FixedParameters = new Parameter [1];
				FixedParameters[0] = p;
			}
		}

		public void PrependParameter (Parameter p)
		{
			Parameter [] pa = new Parameter [FixedParameters.Length+1];
			FixedParameters.CopyTo (pa, 1);
			pa[0] = p;
			FixedParameters = pa;		
		}
		
		public Parameters Copy (Location l)
		{
			Parameters p = new Parameters (null, null, l);
			p.FixedParameters = new Parameter[this.FixedParameters.Length];
			this.FixedParameters.CopyTo (p.FixedParameters, 0);
			
			return (p);
			
		}
		
		public bool Empty {
			get {
				return (FixedParameters == null) && (ArrayParameter == null);
			}
		}
		
		public void ComputeSignature (DeclSpace ds)
		{
			signature = "";
			if (FixedParameters != null){
				for (int i = 0; i < FixedParameters.Length; i++){
					Parameter par = FixedParameters [i];
					
					signature += par.GetSignature (ds, loc);
				}
			}
			//
			// Note: as per the spec, the `params' arguments (ArrayParameter)
			// are not used in the signature computation for a method
			//
		}

		static void Error_DuplicateParameterName (string name)
		{
			Report.Error (
				100, "The parameter name `" + name + "' is a duplicate");
		}
		
		public bool VerifyArgs ()
		{
			int count;
			int i, j;

			if (FixedParameters == null)
				return true;
			
			count = FixedParameters.Length;
			string array_par_name = ArrayParameter != null ? ArrayParameter.Name : null;
			for (i = 0; i < count; i++){
				string base_name = FixedParameters [i].Name;
				
				for (j = i + 1; j < count; j++){
					if (base_name != FixedParameters [j].Name)
						continue;
					Error_DuplicateParameterName (base_name);
					return false;
				}

				if (base_name == array_par_name){
					Error_DuplicateParameterName (base_name);
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		///    Returns the signature of the Parameters evaluated in
		///    the @tc environment
		/// </summary>
		public string GetSignature (DeclSpace ds)
		{
			if (signature == null){
				VerifyArgs ();
				ComputeSignature (ds);
			}
			
			return signature;
		}
		
		/// <summary>
		///    Returns the paramenter information based on the name
		/// </summary>
		public Parameter GetParameterByName (string name, out int idx)
		{
			idx = 0;
			int i = 0;

			if (FixedParameters != null){
				foreach (Parameter par in FixedParameters){
					if (par.Name == name){
						idx = i;
						return par;
					}
					i++;
				}
			}

			if (ArrayParameter != null){
				if (name == ArrayParameter.Name){
					idx = i;
					return ArrayParameter;
				}
			}
			
			return null;
		}

		bool ComputeParameterTypes (DeclSpace ds)
		{
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc;

			if (FixedParameters == null)
				pc = extra;
			else
				pc = extra + FixedParameters.Length;

			types = new Type [pc];
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return false;
			}

			bool failed = false;
			if (FixedParameters != null){
				foreach (Parameter p in FixedParameters){
					Type t = null;
					
					if (p.Resolve (ds, loc))
						t = p.ExternalType (ds, loc);
					else
						failed = true;
					
					types [i] = t;
					i++;
				}
			}
			
			if (extra > 0){
				if (ArrayParameter.Resolve (ds, loc))
					types [i] = ArrayParameter.ExternalType (ds, loc);
				else 
					failed = true;
			}

			if (failed){
				types = null;
				return false;
			}

			return true;
		}

		//
		// This variant is used by Delegates, because they need to
		// resolve/define names, instead of the plain LookupType
		//
		public bool ComputeAndDefineParameterTypes (DeclSpace ds)
		{
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc;

			if (FixedParameters == null)
				pc = extra;
			else
				pc = extra + FixedParameters.Length;
			
			types = new Type [pc];
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return false;
			}

			bool ok_flag = true;
			
			if (FixedParameters != null){
				foreach (Parameter p in FixedParameters){
					Type t = null;
					
					if (p.Resolve (ds, loc))
						t = p.ExternalType (ds, loc);
					else
						ok_flag = false;
					
					types [i] = t;
					i++;
				}
			}
			
			if (extra > 0){
				if (ArrayParameter.Resolve (ds, loc))
					types [i] = ArrayParameter.ExternalType (ds, loc);
				else
					ok_flag = false;
			}

			//
			// invalidate the cached types
			//
			if (!ok_flag){
				types = null;
			}
			
			return ok_flag;
		}
		
		/// <summary>
		///   Returns the argument types as an array
		/// </summary>
		static Type [] no_types = new Type [0];
		
		public Type [] GetParameterInfo (DeclSpace ds)
		{
			if (types != null)
				return types;
			
			if (FixedParameters == null && ArrayParameter == null)
				return no_types;

			if (ComputeParameterTypes (ds) == false){
				types = null;
				return null;
			}

			return types;
		}

		/// <summary>
		///   Returns the type of a given parameter, and stores in the `is_out'
		///   boolean whether this is an out or ref parameter.
		///
		///   Note that the returned type will not contain any dereference in this
		///   case (ie, you get "int" for a ref int instead of "int&"
		/// </summary>
		public Type GetParameterInfo (DeclSpace ds, int idx, out Parameter.Modifier mod)
		{
			mod = Parameter.Modifier.NONE;
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return null;
			}

			if (FixedParameters == null && ArrayParameter == null)
				return null;
			
			if (types == null)
				if (ComputeParameterTypes (ds) == false)
					return null;

			//
			// If this is a request for the variable lenght arg.
			//
			int array_idx = (FixedParameters != null ? FixedParameters.Length : 0);
			if (idx == array_idx)
				return types [idx];

			//
			// Otherwise, it is a fixed parameter
			//
			Parameter p = FixedParameters [idx];
			mod = p.ModFlags;

			if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0)
				mod |= Parameter.Modifier.ISBYREF;

			return p.ParameterType;
		}

		public CallingConventions GetCallingConvention ()
		{
			// For now this is the only correc thing to do
			return CallingConventions.Standard;
		}
	}
}
