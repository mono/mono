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

namespace CIR {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
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
	}
}
