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

		public int Count {
			get {
				return pi.Length;
			}
		}
		
	}

	public class InternalParameters : ParameterData {
		Type [] pars;
		
		public InternalParameters (Type [] pars)
		{
			this.pars = pars;
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
					
			return pars [pos];
		}

		public string ParameterDesc (int pos)
		{
			return TypeManager.CSharpName (ParameterType (pos));
		}
	}
}
