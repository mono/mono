//
// anonymous.cs: Support for anonymous methods
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//
// (C) 2003 Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public class AnonymousMethod : Expression {
		// An array list of AnonymousMethodParameter or null
		Parameters parameters;
		Block block;
		
		public AnonymousMethod (Parameters parameters, Block block, Location l)
		{
			this.parameters = parameters;
			this.block = block;
			loc = l;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			//
			// Set class type, set type
			//

			eclass = ExprClass.Value;

			//
			// This hack means `The type is not accessible
			// anywhere', we depend on special conversion
			// rules.
			// 
			type = typeof (AnonymousMethod);
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}
	}
}
	
