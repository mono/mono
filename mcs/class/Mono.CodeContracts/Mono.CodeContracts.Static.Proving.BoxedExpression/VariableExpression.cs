using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class VariableExpression : BoxedExpression
		{
				private readonly PathElement[] Path;
				private readonly object UnderlyingVar;
				public readonly object VarType;

				public VariableExpression(object var)
				: this (var, (LispList<PathElement>) null)
				{
				}

				public VariableExpression(object var, LispList<PathElement> path)
				{
						this.UnderlyingVar = var;
						this.Path = path != null ? path.AsEnumerable().ToArray() : null;
				}

				public VariableExpression(object var, LispList<PathElement> path, object type)
				: this (var, path)
				{
						this.VarType = type;
				}

				public VariableExpression(object var, PathElement[] path)
				{
						this.UnderlyingVar = var;
						this.Path = path;
				}

				public VariableExpression(object var, PathElement[] path, object type)
				: this (var, path)
				{
						this.VarType = type;
				}

				public override bool IsVariable
				{
						get { return true; }
				}

				public override object UnderlyingVariable
				{
						get { return this.UnderlyingVar; }
				}

				public override PathElement[] AccessPath
				{
						get { return this.Path; }
				}

				public override bool IsBooleanTyped
				{
						get { return this.Path != null && this.Path[this.Path.Length - 1]; }
				}

				public override bool TryGetType(out object type)
				{
						type = this.VarType;
						return type != null;
				}

				public override void AddFreeVariables(HashSet<BoxedExpression> set)
				{
						set.Add(this);
				}

				protected internal override BoxedExpression RecursiveSubstitute(BoxedExpression what, BoxedExpression replace)
				{
						var varExpr = what as VariableExpression;
						if (varExpr != null && varExpr.UnderlyingVar.Equals(this.UnderlyingVar))
								return replace;

						return this;
				}

				public override Result ForwardDecode<Data, Result, Visitor>(PC pc, Visitor visitor, Data data)
				{
						return visitor.Nop(pc, data);
				}

				public override bool Equals(object obj)
				{
						if (this == obj)
								return true;
						var boxedExpression = obj as BoxedExpression;
						if (boxedExpression != null && boxedExpression.IsVariable)
								return this.UnderlyingVar.Equals(boxedExpression.UnderlyingVariable);

						return false;
				}

				public override int GetHashCode()
				{
						return this.UnderlyingVariable != null ? 0 : this.UnderlyingVariable.GetHashCode();
				}

				public override BoxedExpression Substitute<Variable>(Func<Variable, BoxedExpression, BoxedExpression> map)
				{
						if (!(this.UnderlyingVar is Variable))
								return this;
						var variable = ((Variable)this.UnderlyingVar);
						return map(variable, this);
				}
		}
}

