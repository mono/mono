//
// module.cs: Module handler
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2002 Rafael Teixeira
//
using System;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Mono.CSharp ;

namespace Mono.CSharp
{
	public class VBModule : Class
	{
		public new const int AllowedModifiers = Modifiers.PUBLIC |Modifiers.INTERNAL;

		public VBModule (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
			      Attributes attrs, Location l)
			: base (ns, parent, name, 0, attrs, l)

		{
			if (parent.Parent != null)
				Report.Error (30617, l,
					"'Module' statements can occur only at file or namespace level");

			// overwrite ModFlags
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.INTERNAL, l);

			// add specialized attribute

			Mono.CSharp.Attribute standardModuleAttribute = new Mono.CSharp.Attribute(null, Expression.StringToExpression ("Microsoft.VisualBasic.CompilerServices", l), "StandardModuleAttribute", null, l); 
			
			ArrayList al = new ArrayList();
			al.Add(standardModuleAttribute);
			if (attributes == null) {
				attributes = new Attributes(al);
			} else {
				attributes.AddAttributes(al);
			}
		}


		public override TypeAttributes TypeAttr 
		{
			get {
				return base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.Sealed;
			}
		}
	}
}
