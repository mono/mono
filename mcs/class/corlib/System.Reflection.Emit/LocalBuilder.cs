
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
		private bool is_pinned;
		#endregion
		
		//
		// Order does not matter after here
		//
		internal ushort position;
		internal ILGenerator ilgen;

		internal LocalBuilder (Type t, ILGenerator ilgen)
		{
			this.type = t;
			this.ilgen = ilgen;
		}
		public void SetLocalSymInfo (string lname, int startOffset, int endOffset)
		{
			this.name = lname;

			SignatureHelper sighelper = SignatureHelper.GetLocalVarSigHelper (ilgen.module);
			sighelper.AddArgument (type);
			byte[] signature = sighelper.GetSignature ();

			ilgen.sym_writer.DefineLocalVariable (lname, FieldAttributes.Private,
								  signature, SymAddressKind.ILOffset,
								  (int) position, 0, 0,
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
		
		internal void MakePinned ()
		{
			is_pinned = true;
		}
	}
}
