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
			source = source.Resolve (ec);
			if (source == null)
				return null;
			
			target = target.ResolveLValue (ec, source);
			
			if (target == null)
				return null;

			Type target_type = target.Type;
			Type source_type = source.Type;

			type = target_type;
			eclass = ExprClass.Value;
			
			//
			// If we are doing a property assignment, then
			// set the `value' field on the property, and Resolve
			// it.
			//
			if (target is PropertyExpr){
				PropertyExpr property_assign = (PropertyExpr) target;
				
				if (!property_assign.VerifyAssignable ())
					return null;
				
				return this;
			}

			if (target is IndexerAccess){
				IndexerAccess ia = (IndexerAccess) target;

				if (!ia.VerifyAssignable (source))
					return null;

				return this;
			}
			
			if (source is New && target_type.IsSubclassOf (TypeManager.value_type)){
				New n = (New) source;

				n.ValueTypeVariable = target;

				return n;
			}
			
			if (target_type != source_type){
				source = ConvertImplicitRequired (ec, source, target_type, l);
				if (source == null)
					return null;
			}

			if (target.ExprClass != ExprClass.Variable){
				Report.Error (131, l,
					      "Left hand of an assignment must be a variable, " +
					      "a property or an indexer");
				return null;
			}

			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			ILGenerator ig = ec.ig;
			ExprClass eclass = target.ExprClass;
			
			if (eclass == ExprClass.Variable){

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

				((IStackStore) target).Store (ec);
			} else if (eclass == ExprClass.PropertyAccess){
				PropertyExpr pe = (PropertyExpr) target;

				if (is_statement){
					pe.Value = source;
					pe.Emit (ec);
				} else {
					LocalTemporary tempo;
					
					tempo = new LocalTemporary (ec, source.Type);

					pe.Value = tempo;
					source.Emit (ec);
					tempo.Store (ec);
					target.Emit (ec);

					tempo.Emit (ec);
				}
			} else if (eclass == ExprClass.IndexerAccess){
				
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




