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

namespace Mono.MonoBASIC
{
	public class Utils
	{
		public static void AddSpecializedAttribute(ref Attributes attrs, string attributeName, ArrayList args, Location loc)
		{
			Mono.CSharp.Attribute specialAttr = new Mono.CSharp.Attribute(attributeName, args, loc);
			ArrayList al = new ArrayList();
			al.Add(specialAttr);
			AttributeSection asec = new AttributeSection(null, al);
			if (attrs == null)
				attrs = new Attributes(asec, loc);
			else
				attrs.AddAttribute(asec);
		}
	}
	
	/// <summary>
	/// Summary description for module.
	/// </summary>
	public class Module : Mono.CSharp.Class 
	{
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		public new const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.INTERNAL;

		public Module(TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, 0, null, l)
		{
			if (parent.Parent != null)
				Report.Error (30617, l,
					"'Module' statements can occur only at file or namespace level");

			// overwrite ModFlags
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.INTERNAL, l);

			// add specialized attribute
			Utils.AddSpecializedAttribute(ref attrs, "Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute", null, l);
			this.attributes = attrs;
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		public override TypeAttributes TypeAttr 
		{
			get 
			{
				return base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.Sealed;
			}
		}
	}
}
