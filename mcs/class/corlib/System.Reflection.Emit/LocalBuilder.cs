
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

		internal LocalBuilder (ModuleBuilder m, Type t)
		{
			this.module = m;
			this.type = t;
		}
		public void SetLocalSymInfo (string lname, int startOffset, int endOffset)
		{
			ISymbolWriter symbol_writer = module.GetSymWriter ();
			name = lname;

			if (symbol_writer == null)
				return;

			SignatureHelper sig_helper = SignatureHelper.GetLocalVarSigHelper (module);

			sig_helper.AddArgument (type);

			byte[] signature = sig_helper.GetSignature ();

			symbol_writer.DefineLocalVariable (name, FieldAttributes.Private,
							   signature, SymAddressKind.ILOffset,
							   (int)position, 0, 0,
							   startOffset, endOffset);
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
