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
	public class Utils
	{
		public static void AddSpecializedAttribute(ref Attributes attrs, string attributeName, ArrayList args, Location loc)
		{
			Mono.CSharp.Attribute specialAttr = new Mono.CSharp.Attribute(null, attributeName, args, loc); // Sudha :  passed null for target
			ArrayList al = new ArrayList();
			al.Add(specialAttr);
			if (attrs == null) {
				attrs = new Attributes(al);
			} else {
				attrs.AddAttributes(al);
			}
		}
	}

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
			Utils.AddSpecializedAttribute(ref attributes, "Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute", null, l);
		}


		public override TypeAttributes TypeAttr 
		{
			get {
				return base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.Sealed;
			}
		}
	}
}
