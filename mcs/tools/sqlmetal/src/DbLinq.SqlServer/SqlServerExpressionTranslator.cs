using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.SqlServer
{
    class SqlServerExpressionTranslator : ExpressionTranslator
    {
        public override SelectExpression OuterExpression(SelectExpression e)
        {
            // Check for (from f in foo orderby f.Field select f).Count() trees
            // SQL Server doesn't support 'ORDER BY' for 'SELECT COUNT(*)'.
            if (e.Operands.Select(o => o as SpecialExpression)
                    .Where(o => o != null)
                    .Where(s => s.SpecialNodeType == SpecialExpressionType.Count)
                    .Any())
            {
                e.OrderBy.Clear();
            }
            return e;
        }
    }
}
