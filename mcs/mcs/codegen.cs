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
		
		public CodeGen (string name, string output)
		{
			AssemblyName an;
			
			an = new AssemblyName ();
			an.Name = "AssemblyName";
			current_domain = AppDomain.CurrentDomain;
			assembly_builder = current_domain.DefineDynamicAssembly (
										 an, AssemblyBuilderAccess.RunAndSave);
			
			module_builder = assembly_builder.DefineDynamicModule (name, output);
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
		
		public void Save (Report report, string name)
		{
			try {
				assembly_builder.Save (name);
			} catch (System.IO.IOException io){
				report.Error (16, "Coult not write to file `"+name+"', cause: " + io.Message);
			}
		}
	}

	public struct EmitContext {
		TypeContainer parent;
		public ILGenerator   ig;

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
		}

		public bool ConvertTo (Type target, Type source, bool verbose)
		{
			if (target == source)
				return true;

			if (verbose)
				parent.RootContext.Report.Error (
					31, "Can not convert to type bool");
			
			return false;
		}
		
		public void EmitBoolExpression (Expression e)
		{
			e.Resolve (parent);
			if (!ConvertTo (TypeManager.bool_type, e.Type, false)){
				parent.RootContext.Report.Error (
					31, "Can not convert the expression to a boolean");
				return;
			}
			
			e.Emit (this);
		}

		public void EmitExpression (Expression e)
		{
			e.Resolve (parent);
			e.Emit (this);     
		}
		
		public void EmitIf (If s)
		{
			Label false_target = ig.DefineLabel ();
			Label end;
			Statement false_stat = s.FalseStatement;

			EmitBoolExpression (s.Expr);
			ig.Emit (OpCodes.Brfalse, false_target);
			EmitStatement (s.TrueStatement);

			if (false_stat != null){
				end = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, end);
			
				ig.MarkLabel (false_target);
				EmitStatement (s.FalseStatement);

				if (false_stat != null)
					ig.MarkLabel (end);
			} else
				ig.MarkLabel (false_target);
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

		void EmitInvocation (Invocation inv)
		{
			inv.Resolve (parent);
			inv.Emit (this);
		}

		void EmitStatementExpression (StatementExpression s)
		{
			Expression e = s.Expr;

			Console.WriteLine ("Processing ExprStmt: " + e);

			e.Resolve (parent);
			e.Emit (this);

			Console.WriteLine ("TODO: Maybe we need a pop here?");
		}
		
		void EmitStatement (Statement s)
		{
			Console.WriteLine ("Emitting statement of type " + s.GetType ());
			
			if (s is If)
				EmitIf ((If) s);
			else if (s is Do)
				EmitDo ((Do) s);
			else if (s is While)
				EmitWhile ((While) s);
			else if (s is For)
				EmitFor ((For) s);
			else if (s is Return)
				EmitReturn ((Return) s);
			else if (s is Switch)
				EmitSwitch ((Switch) s);
			else if (s is Checked)
				EmitChecked ((Checked) s);
			else if (s is Unchecked)
				EmitUnChecked ((Unchecked) s);
			else if (s is Block)
				EmitBlock ((Block) s);
			else if (s is StatementExpression)
				EmitStatementExpression ((StatementExpression) s);
			else {
				Console.WriteLine ("Unhandled Statement type: " +
						   s.GetType ().ToString ());
			}
		}

		void EmitBlock (Block block)
		{
			foreach (Statement s in block.Statements){
				EmitStatement (s);
			}
		}
		
		public void EmitTopBlock (Block block)
		{
			block.EmitMeta (parent, ig, block);

			Console.WriteLine ("Emitting Top Block");
			EmitBlock (block);

			ig.Emit (OpCodes.Ret);
		}
	}
}
