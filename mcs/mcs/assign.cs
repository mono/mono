//
// assign.cs: Assignment representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Reflection.Emit;

namespace CIR {
	public class Assign : Expression {
		Expression target, source;
		
		public Assign (Expression target, Expression source)
		{
			this.target = target;
			this.source = source;
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

		public override Expression Resolve (TypeContainer tc)
		{
			target = target.Resolve (tc);
			source = source.Resolve (tc);

			if (target == null || source == null)
				return null;
			
			return this;
		}

		void EmitLocalAssign (LocalVariableReference lv, ILGenerator ig)
		{
			VariableInfo vi = lv.VariableInfo;
			int idx = vi.Idx;
					
			switch (idx){
			case 0:
				ig.Emit (OpCodes.Stloc_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Stloc_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Stloc_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Stloc_3);
				break;
				
			default:
				if (idx < 255)
					ig.Emit (OpCodes.Stloc_S, idx);
				else
					ig.Emit (OpCodes.Stloc, idx);
				break;
			}
		}

		public void EmitParameterAssign (ParameterReference pr, ILGenerator ig)
		{
			int idx = pr.Idx;
			
			if (idx < 255)
				ig.Emit (OpCodes.Starg_S, idx);
			else
				ig.Emit (OpCodes.Starg, idx);
		}

		public void EmitFieldAssign (FieldExpr field, ILGenerator ig)
		{
			if (field.IsStatic)
				ig.Emit (OpCodes.Stsfld, field.FieldInfo);
			else
				ig.Emit (OpCodes.Stfld, field.FieldInfo);
		}
		
		public override void Emit (EmitContext ec)
		{
			if (target.ExprClass == ExprClass.Variable){
				source.Emit (ec);

				if (target is LocalVariableReference){
					EmitLocalAssign ((LocalVariableReference) target, ec.ig);
				} else if (target is ParameterReference){
					EmitParameterAssign ((ParameterReference) target, ec.ig);
				} else if (target is FieldExpr){
					EmitFieldAssign ((FieldExpr) target, ec.ig);
				}
			} 
		}
	}
}



