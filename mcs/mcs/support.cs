//
// support.cs: Support routines to work around the fact that System.Reflection.Emit
// can not introspect types that are being constructed
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System.Reflection.Emit;
using System.Reflection;
using System;
using System.Text;

namespace Mono.CSharp {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
		string ParameterDesc (int pos);
		Parameter.Modifier ParameterModifier (int pos);
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		bool last_arg_is_params;
		
		public ReflectionParameters (ParameterInfo [] pi)
		{
			object [] a;
			
			this.pi = pi;

			int count = pi.Length-1;
			if (count > 0) {
				a = pi [count-1].GetCustomAttributes (TypeManager.param_array_type, false);
			
				if (a != null)
					if (a.Length != 0)
						last_arg_is_params = true;
			} 
		}
		       
		public Type ParameterType (int pos)
		{
			return pi [pos].ParameterType;
		}

		public string ParameterDesc (int pos)
		{
			StringBuilder sb = new StringBuilder ();

			if (pi [pos].IsOut)
				sb.Append ("out ");

			if (pi [pos].IsIn)
				sb.Append ("in ");

			if (pos == pi.Length - 1)
				sb.Append ("params ");
			
			sb.Append (TypeManager.CSharpName (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (pi [pos].IsOut)
				return Parameter.Modifier.OUT;

			if (pos == pi.Length-1) 
				if (last_arg_is_params)
					return Parameter.Modifier.PARAMS;

			return Parameter.Modifier.NONE;
		}

		public int Count {
			get {
				return pi.Length;
			}
		}
		
	}

	public class InternalParameters : ParameterData {
		Type [] param_types;

		Parameters parameters;
		
		public InternalParameters (Type [] param_types, Parameters parameters)
		{
			this.param_types = param_types;
			this.parameters = parameters;
		}

		public InternalParameters (TypeContainer tc, Parameters parameters)
			: this (parameters.GetParameterInfo (tc), parameters)
		{
		}

		public int Count {
			get {
				if (param_types == null)
					return 0;

				return param_types.Length;
			}
		}

		public Type ParameterType (int pos)
		{
			if (param_types == null)
				return null;

			int len = parameters.FixedParameters.Length;
			Parameter p;

			if (pos == len)
				p = parameters.ArrayParameter;
			else if (pos < len)
				p = parameters.FixedParameters [pos];
			else {
				p = parameters.ArrayParameter;
				pos = len;
			}

			//
			// Return the internal type.
			//
			return p.ParameterType;
		}

		public string ParameterDesc (int pos)
		{
			string tmp = null;
			Parameter p;

			if (pos >= parameters.FixedParameters.Length)
				p = parameters.ArrayParameter;
			else
				p = parameters.FixedParameters [pos];
			
			if (p.ModFlags == Parameter.Modifier.REF)
				tmp = "ref ";
			else if (p.ModFlags == Parameter.Modifier.OUT)
				tmp = "out ";
			else if (p.ModFlags == Parameter.Modifier.PARAMS)
				tmp = "params ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.CSharpName (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (pos >= parameters.FixedParameters.Length)
				return parameters.ArrayParameter.ModFlags;
			else
				return parameters.FixedParameters [pos].ModFlags;
		}
		
	}
}
