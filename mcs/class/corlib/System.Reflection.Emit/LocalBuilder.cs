
//
// System.Reflection.Emit/LocalBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit {
	public sealed class LocalBuilder {
		private Type type;
		private string name;
		private ModuleBuilder module;
		private ISymbolWriter symbol_writer;
		internal int position;

		internal LocalBuilder (ModuleBuilder m, Type t)
		{
			this.module = m;
			this.symbol_writer = module.GetSymWriter ();
			this.type = t;
		}
		public void SetLocalSymInfo (string lname, int startOffset, int endOffset)
		{
			name = lname;

			if (symbol_writer == null)
				return;

			SignatureHelper sig_helper = SignatureHelper.GetLocalVarSigHelper (module);

			sig_helper.AddArgument (type);

			byte[] signature = sig_helper.GetSignature ();

			symbol_writer.DefineLocalVariable (name, FieldAttributes.Private,
							   signature, SymAddressKind.ILOffset,
							   position, 0, 0,
							   startOffset, endOffset);
		}

		public void SetLocalSymInfo (string lname)
		{
			SetLocalSymInfo (lname, 0, 0);
		}


		public Type LocalType
		{
			get {return type;}
		}
	}
}
