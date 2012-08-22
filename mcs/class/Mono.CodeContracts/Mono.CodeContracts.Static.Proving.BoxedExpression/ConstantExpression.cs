using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class ConstantExpression : BoxedExpression
		{
				public readonly TypeNode Type;
				public readonly object Value;
				private readonly bool is_boolean;

				public ConstantExpression(object value, TypeNode type)
				: this (value, type, false)
				{
				}

				public ConstantExpression(object value, TypeNode type, bool isBoolean)
				{
						this.Value = value;
						this.Type = type;
						this.is_boolean = isBoolean;
				}

				public override bool IsBooleanTyped
				{
						get { return this.is_boolean; }
				}

				public override bool IsConstant
				{
						get { return true; }
				}

				public override object Constant
				{
						get { return this.Value; }
				}

				public override object ConstantType
				{
						get { return this.Type; }
				}

				public override bool IsNull
				{
						get
						{
								if (this.Value == null)
										return true;

								var conv = this.Value as IConvertible;
								if (conv != null)
								{
										try
										{
												if (conv.ToInt32(null) == 0)
														return true;
										}
										catch
										{
												return false;
										}
								}

								return false;
						}
				}

				public override void AddFreeVariables(HashSet<BoxedExpression> set)
				{
				}

				public override BoxedExpression Substitute<Variable1>(Func<Variable1, BoxedExpression, BoxedExpression> map)
				{
						return this;
				}

				public override Result ForwardDecode<Data, Result, Visitor>(PC pc, Visitor visitor, Data data)
				{
						if (this.Value == null)
								return visitor.LoadNull(pc, Dummy.Value, data);

						return visitor.LoadConst(pc, this.Type, this.Value, Dummy.Value, data);
				}
		}
}

