using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.CodeContracts.Rewrite.Extensions;

namespace Mono.CodeContracts.Rewrite.AstVisitors {
	abstract class ExprVisitor {

		public virtual Expr Visit (Expr e)
		{
		
			switch (e.ExprType) {
			case ExprType.Block:
				return this.VisitBlock ((ExprBlock) e);
			case ExprType.Nop:
				return this.VisitNop ((ExprNop) e);
			case ExprType.Call:
				return this.VisitCall ((ExprCall) e);
			case ExprType.CompareEqual:
				return this.VisitCompareEqual ((ExprCompareEqual) e);
			case ExprType.CompareLessThan:
				return this.VisitCompareLessThan ((ExprCompareLessThan) e);
			case ExprType.CompareGreaterThan:
				return this.VisitCompareGreaterThan ((ExprCompareGreaterThan) e);
			case ExprType.Add:
				return this.VisitAdd ((ExprAdd) e);
			case ExprType.Sub:
				return this.VisitSub ((ExprSub) e);
			case ExprType.LoadArg:
				return this.VisitLoadArg ((ExprLoadArg) e);
			case ExprType.LoadConstant:
				return this.VisitLoadConstant ((ExprLoadConstant) e);
			case ExprType.Return:
				return this.VisitReturn ((ExprReturn) e);
			case ExprType.Box:
				return this.VisitBox ((ExprBox) e);
			case ExprType.Conv:
				return this.VisitConv ((ExprConv) e);
			default:
				throw new NotSupportedException ("Cannot handle: " + e.ExprType);
			}
		}

		protected virtual Expr VisitCollection (IEnumerable<Expr> collection, Expr e, Func<IEnumerable<Expr>, Expr> fnCreateNew)
		{
			int index = 0;
			List<Expr> exprs = null;
			foreach (var expr in collection) {
				Expr exprVisited = this.Visit (expr);
				if (exprs != null || exprVisited != expr) {
					if (exprs == null) {
						exprs = new List<Expr> (collection.Take (index));
					}
					exprs.Add (exprVisited);
				}
				index++;
			}
			if (exprs == null) {
				return e;
			} else {
				return fnCreateNew(exprs);
			}
		}

		protected Expr VisitCollection (Expr e, Func<IEnumerable<Expr>, Expr> fnCreateNew, params Expr [] collection)
		{
			return this.VisitCollection (collection, e, fnCreateNew);
		}

		protected virtual Expr VisitBlock (ExprBlock e)
		{
			return this.VisitCollection (e.Exprs, e, exprs => new ExprBlock (e.MethodInfo, exprs));
		}

		protected virtual Expr VisitNop (ExprNop e)
		{
			return e;
		}

		protected virtual Expr VisitLoadArg (ExprLoadArg e)
		{
			return e;
		}

		protected virtual Expr VisitLoadConstant (ExprLoadConstant e)
		{
			return e;
		}

		protected virtual Expr VisitCompareLessThan (ExprCompareLessThan e)
		{
			return this.VisitCollection (e, exprs => new ExprCompareLessThan (e.MethodInfo, exprs.First (), exprs.Second (), e.Signage), e.Left, e.Right);
		}

		protected virtual Expr VisitCompareGreaterThan (ExprCompareGreaterThan e)
		{
			return this.VisitCollection (e, exprs => new ExprCompareGreaterThan (e.MethodInfo, exprs.First (), exprs.Second (), e.Signage), e.Left, e.Right);
		}

		protected virtual Expr VisitCompareEqual (ExprCompareEqual e)
		{
			return this.VisitCollection (e, exprs => new ExprCompareEqual (e.MethodInfo, exprs.First (), exprs.Second ()), e.Left, e.Right);
		}

		protected virtual Expr VisitAdd (ExprAdd e)
		{
			return this.VisitCollection (e, exprs => new ExprAdd (e.MethodInfo, exprs.First (), exprs.Second (), e.Signage, e.Overflow), e.Left, e.Right);
		}

		protected virtual Expr VisitSub (ExprSub e)
		{
			return this.VisitCollection (e, exprs => new ExprSub (e.MethodInfo, exprs.First (), exprs.Second (), e.Signage, e.Overflow), e.Left, e.Right);
		}

		protected virtual Expr VisitCall (ExprCall e)
		{
			return this.VisitCollection (e.Parameters, e, exprs => new ExprCall (e.MethodInfo, e.Method, exprs));
		}

		protected virtual Expr VisitReturn (ExprReturn e)
		{
			return e;
		}

		protected virtual Expr VisitBox (ExprBox e)
		{
			return this.VisitCollection (e, exprs => new ExprBox (e.MethodInfo, exprs.First ()), e.ExprToBox);
		}

		protected virtual Expr VisitConv (ExprConv e)
		{
			return this.VisitCollection (e, exprs => new ExprConv (e.MethodInfo, exprs.First (), e.ConvToType), e.ExprToConvert);
		}
	}
}
