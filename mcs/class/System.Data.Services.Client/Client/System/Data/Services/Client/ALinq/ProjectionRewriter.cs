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
    #region Namespaces.

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal class ProjectionRewriter : ExpressionVisitor
    {
        #region Private fields.

        private readonly ParameterExpression newLambdaParameter;

        private ParameterExpression oldLambdaParameter;

        private bool sucessfulRebind;

        #endregion Private fields.

        private ProjectionRewriter(Type proposedParameterType)
        {
            Debug.Assert(proposedParameterType != null, "proposedParameterType != null");
            this.newLambdaParameter = Expression.Parameter(proposedParameterType, "it");
        }

        #region Internal methods.

        internal static LambdaExpression TryToRewrite(LambdaExpression le, Type proposedParameterType)
        {
            LambdaExpression result;
            if (!ResourceBinder.PatternRules.MatchSingleArgumentLambda(le, out le) ||                ClientType.CheckElementTypeIsEntity(le.Parameters[0].Type) ||                !(le.Parameters[0].Type.GetProperties().Any(p => p.PropertyType == proposedParameterType)))            {
                result = le;
            }
            else
            {
                ProjectionRewriter rewriter = new ProjectionRewriter(proposedParameterType);
                result = rewriter.Rebind(le);
            }

            return result;
        }

        internal LambdaExpression Rebind(LambdaExpression lambda)
        {
            this.sucessfulRebind = true;
            this.oldLambdaParameter = lambda.Parameters[0];

            Expression body = this.Visit(lambda.Body);
            if (this.sucessfulRebind)
            {
                Type delegateType = typeof(Func<,>).MakeGenericType(new Type[] { newLambdaParameter.Type, lambda.Body.Type });
#if ASTORIA_LIGHT
                return ExpressionHelpers.CreateLambda(delegateType, body, new ParameterExpression[] { this.newLambdaParameter });
#else
                return Expression.Lambda(delegateType, body, new ParameterExpression[] { this.newLambdaParameter });
#endif
            }
            else
            {
                throw new NotSupportedException(Strings.ALinq_CanOnlyProjectTheLeaf);
            }
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression == this.oldLambdaParameter)
            {
                if (m.Type == this.newLambdaParameter.Type)
                {
                    return this.newLambdaParameter;
                }
                else
                {
                    this.sucessfulRebind = false;
                }
            }

            return base.VisitMemberAccess(m);
        }

        #endregion Internal methods.
    }
}
