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

namespace Mono.CSharp {


	/// <summary>
	///   Represents a single method parameter
	/// </summary>
	public class Parameter {
		[Flags]
		public enum Modifier : byte {
			NONE   = 0,
			REF    = 1,
			OUT    = 2,
			PARAMS = 4,
		}

		public readonly string   TypeName;
		public readonly string   Name;
		public readonly Modifier ModFlags;
		public Attributes OptAttributes;
		public Type ParameterType;
		
		public Parameter (string type, string name, Modifier mod, Attributes attrs)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
			OptAttributes = attrs;
		}

		public bool Resolve (DeclSpace ds, Location l)
		{
			ParameterType = RootContext.LookupType (ds, TypeName, false, l);
			return ParameterType != null;
		}

		public Type ExternalType ()
		{
			if ((ModFlags & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
				string n = ParameterType.FullName + "&";
				Type t;
				
				t = Type.GetType (n);

				//
				// It is a type defined by the source code we are compiling
				//
				if (t == null){
					ModuleBuilder mb = RootContext.ModuleBuilder;

					t = mb.GetType (n);
				}

				return t;
			}

			return ParameterType;
		}
		
		public ParameterAttributes Attributes {
			get {
				switch (ModFlags){
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
		
		/// <summary>
		///   Returns the signature for this parameter evaluating it on the
		///   @tc context
		/// </summary>
		public string GetSignature (DeclSpace ds, Location loc)
		{
			if (ParameterType == null){
				if (!Resolve (ds, loc))
					return null;
			}

			return ExternalType ().FullName;
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
		public static Parameters GetEmptyReadOnlyParameters ()
		{
			if (empty_parameters == null)
				empty_parameters = new Parameters (null, null, Location.Null);
			
			return empty_parameters;
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

		public bool VerifyArgs ()
		{
			int count;
			int i, j;

			if (FixedParameters == null)
				return true;
			
			count = FixedParameters.Length;
			for (i = 0; i < count; i++){
				for (j = i + 1; j < count; j++){
					if (FixedParameters [i].Name != FixedParameters [j].Name)
						continue;
					Report.Error (
						100, "The parameter name `" + FixedParameters [i].Name +
						"' is a duplicate");
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

			if (FixedParameters == null)
				return null;

			int i = 0;
			foreach (Parameter par in FixedParameters){
				if (par.Name == name){
					idx = i;
					return par;
				}
				i++;
			}

			if (ArrayParameter != null)
				if (name == ArrayParameter.Name){
					idx = i;
					return ArrayParameter;
				}
			
			return null;
		}

		bool ComputeParameterTypes (DeclSpace ds)
		{
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc = FixedParameters.Length + extra;
			
			types = new Type [pc];
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return false;
			}
			
			foreach (Parameter p in FixedParameters){
				Type t = null;
				
				if (p.Resolve (ds, loc))
					t = p.ExternalType ();
				
				types [i] = t;
				i++;
			}

			if (extra > 0){
				if (ArrayParameter.Resolve (ds, loc))
					types [i] = ArrayParameter.ExternalType ();
			}

			return true;
		}
		
		/// <summary>
		///   Returns the argument types as an array
		/// </summary>
		static Type [] no_types = new Type [0];
		
		public Type [] GetParameterInfo (DeclSpace ds)
		{
			if (types != null)
				return types;
			
			if (FixedParameters == null)
				return no_types;

			if (ComputeParameterTypes (ds) == false)
				return null;
			
			return types;
		}

		/// <summary>
		///   Returns the type of a given parameter, and stores in the `is_out'
		///   boolean whether this is an out or ref parameter.
		///
		///   Note that the returned type will not contain any dereference in this
		///   case (ie, you get "int" for a ref int instead of "int&"
		/// </summary>
		public Type GetParameterInfo (DeclSpace ds, int idx, out bool is_out)
		{
			is_out = false;
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return null;
			}

			if (FixedParameters == null)
				return null;
			
			if (types == null)
				if (ComputeParameterTypes (ds) == false){
					is_out = false;
					return null;
				}

			//
			// If this is a request for the variable lenght arg.
			//
			if (idx == FixedParameters.Length){
				is_out = false;
				return types [idx];
			} 

			//
			// Otherwise, it is a fixed parameter
			//
			Parameter p = FixedParameters [idx];
			is_out = ((p.ModFlags & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0);

			return p.ParameterType;
		}

		public CallingConventions GetCallingConvention ()
		{
			// For now this is the only correc thing to do
			return CallingConventions.Standard;
		}
	}
}
		
	

