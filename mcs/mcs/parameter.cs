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
// FIXME: We should deprecate ParameterCollection as it is mostly slow
// to access (it uses an arraylist instead of a hashtable) and exposes
// no method to quickly locate parameters by name.
//
// Look at the implementation for GetParameterByName for an example.
//

namespace CIR {

	using System;

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
		
		public Parameter (string type, string name, Modifier mod)
		{
			Name = name;
			ModFlags = mod;
			Type = type = type;
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
		ParameterCollection fixed_parameters;
		Parameter array_parameter;
		string signature;
		
		public Parameters (ParameterCollection fixed_parameters, Parameter array_parameter)
		{
			this.fixed_parameters = fixed_parameters;
			this.array_parameter = array_parameter;
		}

		// <summary>
		//   Returns the fixed parameters element
		// </summary>
		public ParameterCollection FixedParameters {
			get {
				return fixed_parameters;
			}
		}

		// <summary>
		//   Returns the array parameter.
		// </summary>
		public Parameter ArrayParameter {
			get {
				return array_parameter;
			}
		}

		public void ComputeSignature (TypeContainer tc)
		{
			signature = "";
			if (fixed_parameters != null){
				for (int i = 0; i < fixed_parameters.Count; i++){
					Parameter par = (Parameter) fixed_parameters [i];
					
					signature += par.GetSignature (tc);
				}
			}
			//
			// Note: as per the spec, the `params' arguments (array_parameter)
			// are not used in the signature computation for a method
			//
		}

		// <summary>
		//    Returns the signature of the Parameters evaluated in
		//    the @tc environment
		// </summary>
		public string GetSignature (TypeContainer tc)
		{
			if (signature == null)
				ComputeSignature (tc);
			
			return signature;
		}
		
		// <summary>
		//    Returns the paramenter information based on the name
		// </summary>
		public Parameter GetParameterByName (string name)
		{
			if (fixed_parameters == null)
				return null;

			foreach (Parameter par in fixed_parameters)
				if (par.Name == name)
					return par;

			return null;
		}

		// <summary>
		//   Returns the argument types as an array
		// </summary>
		public Type [] GetTypes (TypeContainer tc)
		{
			int extra = (array_parameter != null) ? 1 : 0;
			Type [] types = new Type [fixed_parameters.Count + extra];
			int i = 0;
			
			foreach (Parameter p in fixed_parameters){
				types [i++] = tc.LookupType (p.Name, false);
			}

			if (extra > 0)
				types [i] = Type.GetType ("System.Object");

			return types;
		}
	}
}
		
	

