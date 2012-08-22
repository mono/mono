using System;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public abstract class ContractExpression : BoxedExpression
		{
				public readonly string Tag;
				public readonly BoxedExpression Condition;
				public readonly APC Apc;
				public readonly object Provenance;

				public bool HasSourceContext
				{
						get{ return this.Apc.Block.SourceDocument(this.Apc) != null;}
				}

				public string SourceAssertionCondition
				{
						get{ return this.Apc.Block.SourceAssertionCondition(this.Apc);}
				}

				public string SourceDocument
				{
						get{ return this.Apc.Block.SourceDocument(this.Apc);}
				}

				public int SourceStartLine
				{
						get{ return this.Apc.Block.SourceStartLine(this.Apc);}
				}

				public int SourceStartColoumn
				{
						get{ return this.Apc.Block.SourceStartColoumn(this.Apc);}
				}

				public int SourceEndLine
				{
						get{ return this.Apc.Block.SourceStartLine(this.Apc);}
				}

				public int SourceEndColoumn
				{
						get{ return this.Apc.Block.SourceStartColoumn(this.Apc);}
				}

				public int SourceStartIndex
				{
						get{ return this.Apc.Block.SourceStartIndex(this.Apc);}
				}

				public int ILOffset
				{
						get{ return this.Apc.Block.ILOffset(this.Apc);}
				}

				public ContractExpression(BoxedExpression cond, string tag, APC apc, object provenance)
				{
						this.Tag = tag;
						this.Condition = cond;
						this.Apc = apc;
						this.Provenance = provenance;
				}

				protected override void AddFreeVariables(Set<>)

				public override int ComputeHashCode()
				{
						return this.Condition.GetHashCode();
				}

				internal abstract override Result ForwardDecode<Data, Result, Local, Parameter, Field, Method, Type, Visitor>(PC pc, Visitor visitor, Data data);

				public abstract override bool Equals(object obj);

				public abstract override string ToString();

				public abstract override BoxedExpression Substitude<Variable>(Func<Variable, BoxedExpression, BoxedExpression> map);  
		}
}

