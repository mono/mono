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

namespace CIR {

	using System;
	using System.Reflection;
	using System.Collections;
	
	public class Parameter {
		public enum Modifier {
			NONE,
			REF,
			OUT,
			PARAMS,
		}

		public readonly string   Type;
		public readonly string   Name;
		public readonly Modifier ModFlags;
		public Attributes OptAttributes;
		
		public Parameter (string type, string name, Modifier mod, Attributes attrs)
		{
			Name = name;
			ModFlags = mod;
			Type = type = type;
			OptAttributes = attrs;
		}

		public ParameterAttributes Attributes {
			get {
				switch (ModFlags){
				case Modifier.NONE:
					return ParameterAttributes.None;
				case Modifier.REF:
					return ParameterAttributes.Retval;
				case Modifier.OUT:
					return ParameterAttributes.Out | ParameterAttributes.Retval;
				case Modifier.PARAMS:
					return 0;
				}
				
				return ParameterAttributes.None;
			}
		}
		
		string ModSignature ()
		{
			switch (ModFlags){
			case Modifier.NONE:
				return "";
			case Modifier.REF:
				return "&";
			case Modifier.OUT:
				return ">";
			case Modifier.PARAMS:
				return "";
			}
			// This should not happen.
			return (string) null;
		}

		// <summary>
		//   Returns the signature for this parameter evaluating it on the
		//   @tc context
		// </summary>
		public string GetSignature (TypeContainer tc)
		{
			Type t = tc.LookupType (Type, false);

			if (t == null)
				return "";
			
			return ModSignature () + t.FullName;
		}
	}

	public class Parameters {
		public Parameter [] FixedParameters;
		public readonly Parameter    ArrayParameter;
		string signature;
		Type [] types;
		
		public Parameters (Parameter [] fixed_parameters, Parameter array_parameter)
		{
			FixedParameters = fixed_parameters;
			ArrayParameter  = array_parameter;
		}

		public bool Empty {
			get {
				return (FixedParameters == null) && (ArrayParameter == null);
			}
		}
		
		public void ComputeSignature (TypeContainer tc)
		{
			signature = "";
			if (FixedParameters != null){
				for (int i = 0; i < FixedParameters.Length; i++){
					Parameter par = FixedParameters [i];
					
					signature += par.GetSignature (tc);
				}
			}
			//
			// Note: as per the spec, the `params' arguments (ArrayParameter)
			// are not used in the signature computation for a method
			//
		}

		public bool VerifyArgs (TypeContainer tc)
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
		
		// <summary>
		//    Returns the signature of the Parameters evaluated in
		//    the @tc environment
		// </summary>
		public string GetSignature (TypeContainer tc)
		{
			if (signature == null){
				VerifyArgs (tc);
				ComputeSignature (tc);
			}
			
			return signature;
		}
		
		// <summary>
		//    Returns the paramenter information based on the name
		// </summary>
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

			return null;
		}

		
		// <summary>
		//   Returns the argument types as an array
		// </summary>
		public Type [] GetParameterInfo (TypeContainer tc)
		{
			if (types != null)
				return types;
			
			if (FixedParameters == null)
				return null;
			
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc = FixedParameters.Length + extra;
			
			types = new Type [pc];

			if (!VerifyArgs (tc)){
				FixedParameters = null;
				return null;
			}
			
			foreach (Parameter p in FixedParameters){
				Type t = tc.LookupType (p.Type, false);

				if ((p.ModFlags & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					t = Type.GetType (t.FullName + "&");
				}
				types [i] = t;
				i++;
			}

			if (extra > 0)
				types [i] = Type.GetType ("System.Object");

			return types;
		}

		public CallingConventions GetCallingConvention ()
		{
			if (ArrayParameter != null)
				return CallingConventions.VarArgs;
			else
				return CallingConventions.Standard;
		}
	}
}
		
	

