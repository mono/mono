//
// assign.cs: Assignment representation for the IL tree.
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
	public class Assign : ExpressionStatement {
		Expression target, source;
		Location l;
		
		public Assign (Expression target, Expression source, Location l)
		{
			this.target = target;
			this.source = source;
			this.l = l;
		}

		public Expression Target {
			get {
				return target;
			}

			set {
				target = value;
			}
		}

		public Expression Source {
			get {
				return source;
			}

			set {
				source = value;
			}
		}

		public override Expression DoResolve (EmitContext ec)
		{
			target = target.Resolve (ec);
			source = source.Resolve (ec);

			if (target == null || source == null)
				return null;

			//
			// If we are doing a property assignment, then
			// set the `value' field on the property, and Resolve
			// it.
			//
			if (target is PropertyExpr){
				PropertyExpr property_assign = (PropertyExpr) target;
				
				property_assign.Value = source;
				
				return property_assign.Resolve (ec);
			}

			if (source is New && target.Type.IsSubclassOf (TypeManager.value_type)){
				New n = (New) source;

				n.ValueTypeVariable = target;

				return source;
			}

			Type target_type = target.Type;
			
			Type source_type = source.Type;

			if (target_type != source_type){
				source = ConvertImplicitRequired (ec, source, target_type, l);
				if (source == null)
					return null;
			}
			
			if (!(target is LValue)){
				Report.Error (131, l, "Left hand of an assignment must be a variable, a property or an indexer");
				return null;
			}
			type = target_type;
			eclass = ExprClass.Value;
			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			ILGenerator ig = ec.ig;
			
			if (target.ExprClass == ExprClass.Variable){

				//
				// If it is an instance field, load the this pointer
				//
				if (target is FieldExpr){
					FieldExpr fe = (FieldExpr) target;
					
					if (!fe.FieldInfo.IsStatic)
						ig.Emit (OpCodes.Ldarg_0);
				}

				source.Emit (ec);

				if (!is_statement)
					ig.Emit (OpCodes.Dup);

				((LValue) target).Store (ec);
			} else if (target.ExprClass == ExprClass.PropertyAccess){
				// FIXME
				throw new Exception ("Can not assign to properties yet");
			} else if (target.ExprClass == ExprClass.IndexerAccess){
				// FIXME
				throw new Exception ("Can not assign to indexers yet");
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec, true);
		}
	}
}




