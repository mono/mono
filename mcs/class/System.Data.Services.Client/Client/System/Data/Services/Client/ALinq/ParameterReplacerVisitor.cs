//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.

namespace System.Data.Services.Client
{
    using System.Diagnostics;
    using System.Linq.Expressions;

    internal class ParameterReplacerVisitor : ExpressionVisitor
    {
        private Expression newExpression;

        private ParameterExpression oldParameter;

        private ParameterReplacerVisitor(ParameterExpression oldParameter, Expression newExpression)
        {
            this.oldParameter = oldParameter;
            this.newExpression = newExpression;
        }

        internal static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
        {
            Debug.Assert(expression != null, "expression != null");
            Debug.Assert(oldParameter != null, "oldParameter != null");
            Debug.Assert(newExpression != null, "newExpression != null");
            return new ParameterReplacerVisitor(oldParameter, newExpression).Visit(expression);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (p == this.oldParameter)
            {
                return this.newExpression;
            }
            else
            {
                return p;
            }
        }
    }
}