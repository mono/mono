//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

	public class Attribute {
		
		public readonly string    Name;
		public readonly ArrayList Arguments;

		Location Location;
		
		public Attribute (string name, ArrayList args, Location loc)
		{
			Name = name;
			Arguments = args;
			Location = loc;
		}

		public CustomAttributeBuilder Resolve (EmitContext ec)
		{
			string name = Name;
			
			if (Name.IndexOf ("Attribute") == -1)
				name = Name + "Attribute";
			
			Type attribute_type = ec.TypeContainer.LookupType (name, false);

			if (attribute_type == null) {
				Report.Error (246, Location, "Could not find attribute '" + Name + "' (are you" +
					      " missing a using directive or an assembly reference ?)");
				return null;
			}
			
			// Now we extract the positional and named arguments
			
			// FIXME : For now we handle only positional arguments

			if (Arguments.Count != 1)
				Console.WriteLine ("Warning : Cannot handle named arguments in attributes yet");
				
			ArrayList pos_args = (ArrayList) Arguments [0];

			foreach (Argument a in pos_args) {
				if (!(a.Expr is Literal)) {
					Report.Error (182, Location,
						      "An attribute argument must be a constant expression, typeof " +
						      "expression or array creation expression");
					return null;
				}
				if (!a.Resolve (ec))
					return null;
			}

			Expression mg = Expression.MemberLookup (ec, attribute_type, ".ctor", false,
								 MemberTypes.Constructor,
								 BindingFlags.Public | BindingFlags.Instance,
								 Location);

			if (mg == null) {
				Report.Error (-6, Location,
					      "Could not find a constructor for this argument list.");
				return null;
			}

			MethodBase constructor = Invocation.OverloadResolve (ec, (MethodGroupExpr) mg, pos_args, Location);

			if (constructor == null) {
				Report.Error (-6, Location, "Could not find a constructor for this argument list.");
				return null;
			}

			object [] values = new object [pos_args.Count];

			for (int i = 0; i < pos_args.Count; i++) {
				Expression e = ((Argument) pos_args [i]).Expr;
				values [i] = ((Literal) e).GetValue ();
			}

			CustomAttributeBuilder cb = new CustomAttributeBuilder ((ConstructorInfo) constructor, values); 

			return cb;
		}

	}
	
	public class AttributeSection {
		
		public readonly string    Target;
		public readonly ArrayList Attributes;
		
		public AttributeSection (string target, ArrayList attrs)
		{
			Target = target;
			Attributes = attrs;
		}
		
	}

	public class Attributes {

		public ArrayList AttributeSections;

		public Attributes (AttributeSection a)
		{
			AttributeSections = new ArrayList ();
			AttributeSections.Add (a);

		}

		public void AddAttribute (AttributeSection a)
		{
			if (a != null)
				AttributeSections.Add (a);
		}
		
	}
}
