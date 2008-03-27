using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace System.Linq.jvm
{
    internal class ExpressionValidator : ExpressionVisitor
    {
        LambdaExpression _exp;

        internal ExpressionValidator(LambdaExpression exp)
        {
            _exp = exp;
        }

        protected override void Visit(Expression expression)
        {
            if (expression == null)
            {
                return;
            }
            if (expression.NodeType == ExpressionType.Power)
            {
                VisitBinary((BinaryExpression)expression);
            }
            else
            {
                base.Visit(expression);
            }
        }

        protected override void VisitParameter(ParameterExpression parameter)
        {
            foreach (ParameterExpression pe in _exp.Parameters)
            {
                if (pe.Name.Equals(parameter.Name) &&
                    !Object.ReferenceEquals(parameter, pe))
                {
                    throw new InvalidOperationException("Lambda Parameter not in scope");
                }
            }
        }

        internal void Validate()
        {
            Visit(_exp);
        }
    }
}
