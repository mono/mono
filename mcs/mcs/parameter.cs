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

		TypeRef  typeref;
		string   name;
		Modifier mod;
		
		public Parameter (TypeRef typeref, string name, Modifier mod)
		{
			this.name = name;
			this.mod = mod;
			this.typeref = typeref;
		}

		string ModSignature ()
		{
			switch (mod){
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
		
		public string Signature {
			get {
				return ModSignature (); // TYPEFIX: + Type.GetTypeEncoding (typeref.Type);
			}
		}
		
		public Type Type {
			get {
				return typeref.Type;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public Modifier ModFlags {
			get {
				return mod;
			}
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

		void compute_signature ()
		{
			signature = "";
			
			if (fixed_parameters != null){
				for (int i = 0; i < fixed_parameters.Count; i++){
					Parameter par = (Parameter) fixed_parameters [i];
					
					signature += par.Signature;
				}
			}

			//
			// Note: as per the spec, the `params' arguments (array_parameter)
			// are not used in the signature computation for a method
			//
		}
		
		public ParameterCollection FixedParameters {
			get {
				return fixed_parameters;
			}
		}

		public Parameter ArrayParameter {
			get {
				return array_parameter;
			}
		}

		// Removed for now, as we can not compute this at
		// boot strap.
		public string Signature {
			get {
				return signature;
			}
		}

		public Parameter GetParameterByName (string name)
		{
			if (fixed_parameters == null)
				return null;

			foreach (Parameter par in fixed_parameters)
				if (par.Name == name)
					return par;
			return null;
		}
	}
}
		
	
