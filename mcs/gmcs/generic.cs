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

		public string Name {
			get {
				return type_parameter;
			}
		}
		
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

	public class TypeArguments {
		ArrayList args;
		
		public TypeArguments ()
		{
			args = new ArrayList ();
		}

		public void Add (Expression type)
		{
			args.Add (type);
		}
	}
	
	public class ConstructedType : Expression {
		Expression container_type;
		string name;
		TypeArguments args;
		
		public ConstructedType (Expression container_type, string name, TypeArguments args, Location l)
		{
			loc = l;
			this.container_type = container_type;
			this.name = name;
			this.args = args;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			throw new Exception ("IMPLEMENT ME");
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("IMPLEMENT ME");
		}
	}
}
