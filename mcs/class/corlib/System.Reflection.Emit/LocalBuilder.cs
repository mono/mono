
//
// System.Reflection.Emit/LocalBuilder.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit {
	public sealed class LocalBuilder {
		//
		// These are kept in sync with reflection.h
		//
		#region Sync with reflection.h
		private Type type;
		private string name;
		#endregion
		
		//
		// Order does not matter after here
		//
		private ModuleBuilder module;
		internal uint position;
		internal ILGenerator ilgen;

		internal LocalBuilder (ModuleBuilder m, Type t, ILGenerator ilgen)
		{
			this.module = m;
			this.type = t;
			this.ilgen = ilgen;
		}
		public void SetLocalSymInfo (string lname, int startOffset, int endOffset)
		{
			this.name = lname;

			module.SymWriter_DefineLocalVariable (lname, this, FieldAttributes.Private,
							      (int) position, startOffset, endOffset);
		}

		public void SetLocalSymInfo (string lname)
		{
			SetLocalSymInfo (lname, 0, 0);
		}


		public Type LocalType
		{
			get {
				return type;
			}
		}
	}
}
