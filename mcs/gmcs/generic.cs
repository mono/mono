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
using System.Text;
	
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

		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();

			int count = args.Count;
			for (int i = 0; i < count; i++){
				//
				// FIXME: Use TypeManager.CSharpname once we have the type
				//
				s.Append (args [i].ToString ());
				if (i+1 < count)
					s.Append (", ");
			}
			return s.ToString ();
		}

		public bool Resolve (EmitContext ec)
		{
			int count = args.Count;
			bool ok = true;
			
			for (int i = 0; i < count; i++){
				Expression e = ((Expression)args [i]).ResolveAsTypeTerminal (ec);
				if (e == null)
					ok = false;
				args [i] = e;
			}
			return ok;
		}
	}
	
	public class ConstructedType : Expression {
		Expression container_type;
		string name;
		TypeArguments args;
		
		public ConstructedType (string name, TypeArguments args, Location l)
		{
			loc = l;
			this.container_type = container_type;
			this.name = name;
			this.args = args;
			eclass = ExprClass.Type;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (args.Resolve (ec) == false)
				return null;

			//
			// Pretend there are not type parameters, until we get GetType support
			//
			return new SimpleName (name, loc).DoResolve (ec);
		}

		public override Expression ResolveAsTypeStep (EmitContext ec)
		{
			if (args.Resolve (ec) == false)
				return null;
			
			//
			// Pretend there are not type parameters, until we get GetType support
			//
			return new SimpleName (name, loc).ResolveAsTypeStep (ec);
		}
		
		public override void Emit (EmitContext ec)
		{
			//
			// Never reached for now
			//
			throw new Exception ("IMPLEMENT ME");
		}

		public override string ToString ()
		{
			if (container_type != null)
				return container_type.ToString () + "<" + args.ToString () + ">";
			else
				return "<" + args.ToString () + ">";
		}
	}
}
