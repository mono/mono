//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {
	
	public class CodeGen {
		AppDomain current_domain;
		AssemblyBuilder assembly_builder;
		ModuleBuilder   module_builder;

		string Basename (string name)
		{
			int pos = name.LastIndexOf ("/");

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ("\\");
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		string TrimExt (string name)
		{
			int pos = name.LastIndexOf (".");

			return name.Substring (0, pos);
		}
		
		public CodeGen (string name, string output)
		{
			AssemblyName an;
			
			an = new AssemblyName ();
			an.Name = TrimExt (name);
			current_domain = AppDomain.CurrentDomain;
			assembly_builder = current_domain.DefineDynamicAssembly (
				an, AssemblyBuilderAccess.RunAndSave);

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			module_builder = assembly_builder.DefineDynamicModule (
				Basename (name), Basename (output));
		}
		
		public AssemblyBuilder AssemblyBuilder {
			get {
				return assembly_builder;
			}
		}
		
		public ModuleBuilder ModuleBuilder {
			get {
				return module_builder;
			}
		}
		
		public void Save (string name)
		{
			try {
				assembly_builder.Save (Basename (name));
			} catch (System.IO.IOException io){
				Report.Error (16, "Coult not write to file `"+name+"', cause: " + io.Message);
			}
		}
	}

	public struct EmitContext {
		public TypeContainer parent;
		public ILGenerator   ig;
		Block current_block;
		
		// FIXME: FIXME: FIXME!
		// This structure has to be kept small.  We need to figure
		// out ways of moving the CheckState somewhere else
		//
		// tracks the state of checked/unchecked arithmetic
		// generation.
		//
		public bool CheckState;
		
		public EmitContext (TypeContainer parent, ILGenerator ig)
		{
			this.parent = parent;
			this.ig = ig;
			CheckState = false;
			current_block = null;
		}

		static bool GetEnumeratorFilter (MemberInfo m, object criteria)
		{
			if (m == null)
				return false;

			if (!(m is MethodInfo))
				return false;

			if (m.Name != "GetEnumerator")
				return false;

			MethodInfo mi = (MethodInfo) m;

			if (mi.ReturnType != TypeManager.ienumerator_type)
				return false;

			Type [] args = TypeManager.GetArgumentTypes (mi);
			if (args == null)
				return true;

			if (args.Length == 0)
				return true;

			return false;
		}

		// <summary>
		//   This filter is used to find the GetEnumerator method
		//   on which IEnumerator operates
		// </summary>
		static MemberFilter FilterEnumerator;
		
		static EmitContext ()
		{
			FilterEnumerator = new MemberFilter (GetEnumeratorFilter);
		}
		
		public bool ConvertTo (Type target, Type source, bool verbose)
		{
			if (target == source)
				return true;

			if (verbose)
				Report.Error (
					31, "Can not convert to type bool");
			
			return false;
		}
		
		public bool EmitBoolExpression (Expression e)
		{
			e = e.Resolve (parent);

			if (e == null)
				return false;

			if (e.Type != TypeManager.bool_type)
				e = Expression.ConvertImplicit (parent, e, TypeManager.bool_type,
								new Location (-1));

			if (e == null){
				Report.Error (
					31, "Can not convert the expression to a boolean");
				return false;
			}
			
			e.Emit (this);

			return true;
		}

		public void EmitExpression (Expression e)
		{
			e = e.Resolve (parent);

			if (e != null)
				e.Emit (this);     
		}

		//
		// Emits an If statement.  Returns true if the last opcode
		// emitted was a ret opcode.
		//
		public bool EmitIf (If s)
		{
			Label false_target = ig.DefineLabel ();
			Label end;
			Statement false_stat = s.FalseStatement;
			bool is_ret;
			
			if (!EmitBoolExpression (s.Expr))
				return false;
			
			ig.Emit (OpCodes.Brfalse, false_target);
			is_ret = EmitStatement (s.TrueStatement);

			if (false_stat != null){
				end = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, end);
			
				ig.MarkLabel (false_target);
				is_ret = EmitStatement (s.FalseStatement);

				if (false_stat != null)
					ig.MarkLabel (end);
			} else
				ig.MarkLabel (false_target);

			return is_ret;
		}

		public void EmitDo (Do s)
		{
			Label loop = ig.DefineLabel ();

			ig.MarkLabel (loop);
			EmitStatement (s.EmbeddedStatement);
			EmitBoolExpression (s.Expr);
			ig.Emit (OpCodes.Brtrue, loop);
		}

		public void EmitWhile (While s)
		{
			Label while_eval = ig.DefineLabel ();
			Label exit = ig.DefineLabel ();
			
			ig.MarkLabel (while_eval);
			EmitBoolExpression (s.Expr);
			ig.Emit (OpCodes.Brfalse, exit);
			EmitStatement (s.Statement);
			ig.Emit (OpCodes.Br, while_eval);
			ig.MarkLabel (exit);
		}

		public void EmitFor (For s)
		{
			Statement init = s.InitStatement;
			Statement incr = s.Increment;
			Label loop = ig.DefineLabel ();
			Label exit = ig.DefineLabel ();
			
			if (! (init is EmptyStatement))
				EmitStatement (init);

			ig.MarkLabel (loop);
			EmitBoolExpression (s.Test);
			ig.Emit (OpCodes.Brfalse, exit);
			EmitStatement (s.Statement);
			if (!(incr is EmptyStatement))
				EmitStatement (incr);
			ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (exit);
		}

		void error1579 (Location l, Type t)
		{
			Report.Error (1579, l,
				      "foreach statement cannot operate on variables of type `" +
				      t.FullName + "' because that class does not provide a " +
				      " GetEnumerator method or it is inaccessible");
		}
			
		MethodInfo ProbeCollectionType (Foreach f, Type t)
		{
			MemberInfo [] mi;

			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public,
							FilterEnumerator, null);

			if (mi == null){
				error1579 (f.Location, t);
				return null;
			}

			if (mi.Length == 0){
				error1579 (f.Location, t);
				return null;
			}

			return (MethodInfo) mi [0];
		}
		
		void EmitForeach (Foreach f)
		{
			Expression e = f.Expr;
			MethodInfo get_enum;
			LocalBuilder enumerator, disposable;
			Type var_type;
			
			e = e.Resolve (parent);
			if (e == null)
				return;

			var_type = parent.LookupType (f.Type, false);
			if (var_type == null)
				return;
			
			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(e.ExprClass == ExprClass.Variable || e.ExprClass == ExprClass.Value)){
				error1579 (f.Location, e.Type);
				return;
			}
			
			if ((get_enum = ProbeCollectionType (f, e.Type)) == null)
				return;

			Expression empty = new EmptyExpression ();
			Expression conv;

			conv = Expression.ConvertExplicit (parent, empty, var_type, f.Location);
			if (conv == null)
				return;
			
			enumerator = ig.DeclareLocal (TypeManager.ienumerator_type);
			disposable = ig.DeclareLocal (TypeManager.idisposable_type);
			
			//
			// Instantiate the enumerator

			Label end = ig.DefineLabel ();
			Label end_try = ig.DefineLabel ();
			Label loop = ig.DefineLabel ();

			//
			// FIXME: This code does not work for cases like:
			// foreach (int a in ValueTypeVariable){
			// }
			//
			// The code should emit an ldarga instruction
			// for the ValueTypeVariable rather than a ldarg
			//
			if (e.Type.IsValueType){
				ig.Emit (OpCodes.Call, get_enum);
			} else {
				e.Emit (this);
				ig.Emit (OpCodes.Callvirt, get_enum);
			}
			ig.Emit (OpCodes.Stloc, enumerator);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			Label l = ig.BeginExceptionBlock ();
			ig.MarkLabel (loop);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.bool_movenext_void);
			ig.Emit (OpCodes.Brfalse, end_try);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.object_getcurrent_void);
			conv.Emit (this);
			f.Variable.Store (this);
			EmitStatement (f.Statement);
			ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (end_try);

			// The runtime provides this for us.
			// ig.Emit (OpCodes.Leave, end);

			//
			// Now the finally block
			//
			Label end_finally = ig.DefineLabel ();
			
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
			ig.Emit (OpCodes.Stloc, disposable);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Brfalse, end_finally);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
			ig.MarkLabel (end_finally);

			// The runtime generates this anyways.
			// ig.Emit (OpCodes.Endfinally);

			ig.EndExceptionBlock ();
			ig.MarkLabel (end);
		}

		void EmitReturn (Return s)
		{
			Expression ret_expr = s.Expr;
			
			if (ret_expr != null)
				EmitExpression (ret_expr);
			ig.Emit (OpCodes.Ret);
		}

		void EmitSwitch (Switch s)
		{
		}

		void EmitChecked (Checked s)
		{
			bool previous_state = CheckState;

			CheckState = true;
			EmitBlock (s.Block);
			CheckState = previous_state;
		}

		void EmitUnChecked (Unchecked s)
		{
			bool previous_state = CheckState;

			CheckState = false;
			EmitBlock (s.Block);
			CheckState = previous_state;
		}

		void EmitStatementExpression (StatementExpression s)
		{
			ExpressionStatement e = s.Expr;
			Expression ne;
			
			ne = e.Resolve (parent);
			if (ne != null){
				if (ne is ExpressionStatement)
					((ExpressionStatement) ne).EmitStatement (this);
				else {
					ne.Emit (this);
					ig.Emit (OpCodes.Pop);
				}
			}
		}

		//
		// Emits the statemets `s'.
		//
		// Returns true if the statement had a `ret' opcode embedded
		//
		bool EmitStatement (Statement s)
		{
			// Console.WriteLine ("Emitting statement of type " + s.GetType ());
			
			if (s is If)
				EmitIf ((If) s);
			else if (s is Do)
				EmitDo ((Do) s);
			else if (s is While)
				EmitWhile ((While) s);
			else if (s is For)
				EmitFor ((For) s);
			else if (s is Return){
				EmitReturn ((Return) s);
				return true;
			} else if (s is Switch)
				EmitSwitch ((Switch) s);
			else if (s is Checked)
				EmitChecked ((Checked) s);
			else if (s is Unchecked)
				EmitUnChecked ((Unchecked) s);
			else if (s is Block)
				return EmitBlock ((Block) s);
			else if (s is StatementExpression)
				EmitStatementExpression ((StatementExpression) s);
			else if (s is Foreach)
				EmitForeach ((Foreach) s);
			else if (s is EmptyStatement) {

			} else {
				Console.WriteLine ("Unhandled Statement type: " +
						   s.GetType ().ToString ());
			}

			return false;
		}

		bool EmitBlock (Block block)
		{
			bool is_ret = false;
			Block last_block = current_block;
			
			current_block = block;
			foreach (Statement s in block.Statements){
				is_ret = EmitStatement (s);
			}
			current_block = last_block;

			return is_ret;
		}
		
		public void EmitTopBlock (Block block)
		{
			bool has_ret = false;
			
			if (block != null){
				int errors = Report.Errors;
				
				block.EmitMeta (parent, ig, block, 0);
				
				if (Report.Errors == errors){
					has_ret = EmitBlock (block);
					
					if (Report.Errors == errors)
						block.UsageWarning ();
				}
			}

			if (!has_ret)
				ig.Emit (OpCodes.Ret);
		}
	}
}
