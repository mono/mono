//
// Try.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004, Cesar Octavio Lopez Nataren
//

using System;
using Microsoft.JScript.Vsa;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public sealed class Try : AST {

		internal string catch_id;
		internal FieldBuilder field_info;
		internal LocalBuilder local_builder;
		internal Block guarded_block;
		internal Block catch_block;
		internal Block finally_block;

		internal Try (AST parent)
		{
			this.parent = parent;
			guarded_block = new Block (this);
			catch_block = new Block (this);
			finally_block = new Block (this);
		}

		public static Object JScriptExceptionValue (object e, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public static void PushHandlerScope (VsaEngine engine, string id, int scopeId)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (guarded_block != null)
				r &= guarded_block.Resolve (context);
			
			if (catch_block != null && catch_block.elems.Count > 0) {
				context.OpenBlock ();
				context.Enter (catch_id, this);
				r &= catch_block.Resolve (context);
			}

			if (finally_block != null && finally_block.elems.Count > 0)
				r &= finally_block.Resolve (context);

			context.CloseBlock ();
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t = typeof (object);
			bool not_inside_func = parent == null;
			
			ig.BeginExceptionBlock ();

			if (guarded_block != null)
				guarded_block.Emit (ec);

			if (not_inside_func)
				field_info = ec.type_builder.DefineField (mangle_id (catch_id), t, FieldAttributes.Public | FieldAttributes.Static);
			else
				local_builder = ig.DeclareLocal (t);

			if (catch_block != null && catch_block.elems.Count > 0) {
				ig.BeginCatchBlock (typeof (Exception));
				if (not_inside_func) {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
					ig.Emit (OpCodes.Call, typeof (Try).GetMethod ("JScriptExceptionValue"));
					ig.Emit (OpCodes.Stsfld, field_info);
				} else {
					ig.Emit (OpCodes.Ldarg_1);
					ig.Emit (OpCodes.Call, typeof (Try).GetMethod ("JScriptExceptionValue"));
					ig.Emit (OpCodes.Stloc, local_builder);
				}
				catch_block.Emit (ec);			
			}			
			if (finally_block != null && finally_block.elems.Count > 0) {
				ig.BeginFinallyBlock ();
				finally_block.Emit (ec);
			}
			ig.EndExceptionBlock ();
		}

		internal string mangle_id (string id)
		{
			return id + ":0";
		}
	}
}
