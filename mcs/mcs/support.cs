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

namespace CIR {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
		string ParameterDesc (int pos);
		Parameter.Modifier ParameterModifier (int pos);
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;

		public ReflectionParameters (ParameterInfo [] pi)
		{
			this.pi = pi;
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

			sb.Append (TypeManager.CSharpName (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (pi [pos].IsOut)
				return Parameter.Modifier.OUT;

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
		
		public InternalParameters (TypeContainer tc, Parameters parameters)
		{
			this.param_types = parameters.GetParameterInfo (tc);
			this.parameters = parameters;
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

			Parameter p = parameters.FixedParameters [pos];
			Type t = param_types [pos];
			string name = t.FullName;
			
			if (p.ModFlags == Parameter.Modifier.REF ||
			    p.ModFlags == Parameter.Modifier.OUT)
				t = Type.GetType (name.Substring (0, name.Length - 1));
			
			return t;
		}

		public string ParameterDesc (int pos)
		{
			string tmp = null;
			Parameter p = parameters.FixedParameters [pos];
			
			if (p.ModFlags == Parameter.Modifier.REF)
				tmp = "ref ";
			else if (p.ModFlags == Parameter.Modifier.OUT)
				tmp = "out ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.CSharpName (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			return parameters.FixedParameters [pos].ModFlags;
		}
		
	}
}
