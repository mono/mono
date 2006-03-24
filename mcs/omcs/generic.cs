//
// generic.cs: Support classes for generics
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Collections;

namespace Mono.CSharp {

	//
	// Tracks the constraints for a type parameter
	//
	class Constraints {
		string type_parameter;
		ArrayList constraints;
		
		//
		// type_parameter is the identifier, constraints is an arraylist of
		// Expressions (with types) or `true' for the constructor constraint.
		// 
		public Constraints (string type_parameter, ArrayList constraints)
		{
			this.type_parameter = type_parameter;
			this.constraints = constraints;
		}
	}

	//
	// This type represents a generic type parameter reference.
	//
	// These expressions are born in a fully resolved state.
	//
	public class TypeParameterExpr : TypeExpr {
		string type_parameter;
		
		public TypeParameterExpr (string type_parameter, Location l)
			: base (typeof (object), l)
		{
			this.type_parameter = type_parameter;
		}

		public override string ToString ()
		{
			return "TypeParameter[" + type_parameter + "]";
		}

		public void Error_CannotUseAsUnmanagedType (Location loc)
		{
			Report.Error (-203, loc, "Can not use type parameter as unamanged type");
		}
	}
}
