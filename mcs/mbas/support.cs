//
// support.cs: Support routines to work around the fact that System.Reflection.Emit
// can not introspect types that are being constructed
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using System.Globalization;

namespace Mono.MonoBASIC {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
		string ParameterName (int pos);
		string ParameterDesc (int pos);
		Expression DefaultValue (int pos);
		Parameter.Modifier ParameterModifier (int pos);
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		bool last_arg_is_params = false;
		
		public ReflectionParameters (ParameterInfo [] pi)
		{
			object [] attrs;
			
			this.pi = pi;

			int count = pi.Length-1;

			if (count >= 0) {
				attrs = pi [count].GetCustomAttributes (TypeManager.param_array_type, true);

				if (attrs == null)
					return;
				
				if (attrs.Length == 0)
					return;

				last_arg_is_params = true;
			}
		}
		       
		public Type ParameterType (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length - 1].ParameterType;
			else
				if (pos >= pi.Length)
					return null;
				else {
					Type pt = pi [pos].ParameterType;
                              		if (pt.IsByRef)
						pt = pt.GetElementType();
					return pt;
				}
		}

		public Expression DefaultValue (int pos)
		{
			return null;
#if false
			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length - 1].ParameterType;
			else
				if (pos >= pi.Length)
					return null;
				else {
					Type pt = pi [pos].ParameterType;
                              		if (pt.IsByRef)
						pt = pt.GetElementType();
					return pt;
				}
#endif
		}

		public string ParameterName (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length - 1].Name;
			else 
				return pi [pos].Name;
		}

		public string ParameterDesc (int pos)
		{
			StringBuilder sb = new StringBuilder ();

			if (pi [pos].IsOut)
				sb.Append ("out ");

			if (pi [pos].IsIn)
				sb.Append ("in ");

			if (pos >= pi.Length - 1 && last_arg_is_params)
				sb.Append ("params ");
			
			sb.Append (TypeManager.MonoBASIC_Name (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			int len = pi.Length;
			Parameter.Modifier pm = Parameter.Modifier.NONE;

			if (pos >= len - 1)
				if (last_arg_is_params) {
					pm |= Parameter.Modifier.PARAMS;
					pos = len - 1;
				}
			
			Type t = pi [pos].ParameterType;
			if (t.IsByRef)
				pm |= Parameter.Modifier.ISBYREF | Parameter.Modifier.REF;

			if (pi [pos].IsOptional)
				pm |= Parameter.Modifier.OPTIONAL;
			
			return pm;
		}

		public int Count {
			get {
				return pi.Length;
			}
		}
		
	}

	public class InternalParameters : ParameterData {
		Type [] param_types;

		public readonly Parameters Parameters;
		
		public InternalParameters (Type [] param_types, Parameters parameters)
		{
			this.param_types = param_types;
			this.Parameters = parameters;
		}

		public InternalParameters (DeclSpace ds, Parameters parameters)
			: this (parameters.GetParameterInfo (ds), parameters)
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

			Parameter [] fixed_pars = Parameters.FixedParameters;
			if (fixed_pars != null && pos < fixed_pars.Length)
				return Parameters.FixedParameters [pos].ParameterType;
			else 
				return Parameters.ArrayParameter.ParameterType;
		}

		public Expression DefaultValue (int pos)
		{
			Parameter [] fixed_pars = Parameters.FixedParameters;
			if (fixed_pars != null && pos < fixed_pars.Length)
				return Parameters.FixedParameters [pos].ParameterInitializer;
			return null;
		}

		public string ParameterName (int pos)
		{
			Parameter p;

			if (pos >= Parameters.FixedParameters.Length)
				p = Parameters.ArrayParameter;
			else
				p = Parameters.FixedParameters [pos];

			return p.Name;
		}

		public string ParameterDesc (int pos)
		{
			string tmp = String.Empty;
			Parameter p;

			if (Parameters.FixedParameters == null || pos >= Parameters.FixedParameters.Length)
				p = Parameters.ArrayParameter;
			else
				p = Parameters.FixedParameters [pos];
			
			if (p.ModFlags == Parameter.Modifier.REF)
				tmp = "ref ";
			else if (p.ModFlags == Parameter.Modifier.OUT)
				tmp = "out ";
			else if (p.ModFlags == Parameter.Modifier.PARAMS)
				tmp = "params ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.MonoBASIC_Name (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			Parameter.Modifier mod;

			if (Parameters.FixedParameters == null) {
				if (Parameters.ArrayParameter != null) 
					mod = Parameters.ArrayParameter.ModFlags;
				else
					mod = Parameter.Modifier.NONE;
			} else if (pos >= Parameters.FixedParameters.Length)
				mod = Parameters.ArrayParameter.ModFlags;
			else
				mod = Parameters.FixedParameters [pos].ModFlags;

			if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0)
				mod |= Parameter.Modifier.ISBYREF;

			return mod;
		}
		
	}

	class PtrHashtable : Hashtable {
		class PtrComparer : IComparer {
			public int Compare (object x, object y)
			{
				if (x == y)
					return 0;
				else
					return 1;
			}
		}
		
		public PtrHashtable ()
		{
			comparer = new PtrComparer ();
			hcp = new CaseInsensitiveHashCodeProvider();
		}
	}
	

	public class CaseInsensitiveHashtable : Hashtable {
		public CaseInsensitiveHashtable() : base()
		{
			comparer = new CaseInsensitiveComparer();
			hcp = new CaseInsensitiveHashCodeProvider();
		}
	}

	//
	// Compares member infos based on their name and
	// also allows one argument to be a string
	//
	class MemberInfoCompare : IComparer {

		public int Compare (object a, object b)
		{
			if (a == null || b == null){
				Console.WriteLine ("Invalid information passed");
				throw new Exception ();
			}
			
			if (a is string)
				return String.Compare ((string) a, ((MemberInfo)b).Name);

			if (b is string)
				return String.Compare (((MemberInfo)a).Name, (string) b);

			return String.Compare (((MemberInfo)a).Name, ((MemberInfo)b).Name);
		}
	}

	struct Pair {
		public object First;
		public object Second;
		
		public Pair (object f, object s)
		{
			First = f;
			Second = s;
		}
	}
}
