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

namespace Mono.CSharp {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
		string ParameterName (int pos);
		string ParameterDesc (int pos);
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
			else {
				Type t = pi [pos].ParameterType;

				if (t.IsByRef)
					return t.GetElementType ();
				else
					return t;
			}
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
			
			sb.Append (TypeManager.CSharpName (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			int len = pi.Length;

			if (pos >= len - 1)
				if (last_arg_is_params)
					return Parameter.Modifier.PARAMS;
			
			Type t = pi [pos].ParameterType;
			if (t.IsByRef){
				if ((pi [pos].Attributes & ParameterAttributes.Out) != 0)
					return Parameter.Modifier.ISBYREF | Parameter.Modifier.OUT;
				else
					return Parameter.Modifier.ISBYREF | Parameter.Modifier.REF;
			}
			
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

		Parameter GetParameter (int pos)
		{
			Parameter [] fixed_pars = Parameters.FixedParameters;
			if (fixed_pars != null){
				int len = fixed_pars.Length;
				if (pos < len)
					return Parameters.FixedParameters [pos];
			}

			return Parameters.ArrayParameter;
		}

		public Type ParameterType (int pos)
		{
			if (param_types == null)
				return null;

			return GetParameter (pos).ParameterType;
		}


		public string ParameterName (int pos)
		{
			return GetParameter (pos).Name;
		}

		public string ParameterDesc (int pos)
		{
			string tmp = String.Empty;
			Parameter p = GetParameter (pos);

			//
			// We need to and for REF/OUT, because if either is set the
			// extra flag ISBYREF will be set as well
			//
			if ((p.ModFlags & Parameter.Modifier.REF) != 0)
				tmp = "ref ";
			else if ((p.ModFlags & Parameter.Modifier.OUT) != 0)
				tmp = "out ";
			else if (p.ModFlags == Parameter.Modifier.PARAMS)
				tmp = "params ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.CSharpName (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			Parameter.Modifier mod = GetParameter (pos).ModFlags;

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
