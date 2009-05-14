using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
#if !MONO_STRICT
    public
#endif
    class StartIndexOffsetExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.StartIndexOffset;
        public bool StartsAtOne{get; private set;}
        public Expression InnerExpression { get; private set; }

        public StartIndexOffsetExpression(bool startsAtOne, Expression startExpression)
            : base(ExpressionType, typeof(int))
        {
            this.InnerExpression = startExpression;
            this.StartsAtOne = startsAtOne;
        }
        public override IEnumerable<Expression> Operands
        {
            get
            {
                return new Expression[] { this.InnerExpression };
            }
        }

        public override Expression Mutate(IList<Expression> newOperands)
        {
            this.InnerExpression = newOperands.First();
            return InnerExpression;
        }
    }
}
