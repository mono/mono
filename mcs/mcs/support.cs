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
		string ParameterModifier (int pos);
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

		public string ParameterModifier (int pos)
		{
			if (pi [pos].IsOut)
				return "OUT";

			return "NONE";
		}

		public int Count {
			get {
				return pi.Length;
			}
		}
		
	}

	public class InternalParameters : ParameterData {
		Type [] pars;

		Parameters parameters;
		
		public InternalParameters (Type [] pars, Parameters parameters)
		{
			this.pars = pars;
			this.parameters = parameters;
		}

		public int Count {
			get {
				if (pars == null)
					return 0;

				return pars.Length;
			}
		}

		public Type ParameterType (int pos)
		{
			if (pars == null)
				return null;

			Type t = pars [pos];
			string name = t.FullName;
			
			if (name.IndexOf ("&") != -1)
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

		public string ParameterModifier (int pos)
		{
			return parameters.FixedParameters [pos].ModFlags.ToString ();
		}
		
	}
}
