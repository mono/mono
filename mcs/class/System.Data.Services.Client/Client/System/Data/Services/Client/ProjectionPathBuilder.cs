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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal class ProjectionPathBuilder
    {
        #region Private fields.

        private readonly Stack<bool> entityInScope;

        private readonly List<MemberInitRewrite> rewrites;

        private readonly Stack<ParameterExpression> parameterExpressions;

        private readonly Stack<Expression> parameterExpressionTypes;

        private readonly Stack<Expression> parameterEntries;

        private readonly Stack<Type> parameterProjectionTypes;

        #endregion Private fields.

        #region Constructors.

        internal ProjectionPathBuilder()
        {
            this.entityInScope = new Stack<bool>();
            this.rewrites = new List<MemberInitRewrite>();
            this.parameterExpressions = new Stack<ParameterExpression>();
            this.parameterExpressionTypes = new Stack<Expression>();
            this.parameterEntries = new Stack<Expression>();
            this.parameterProjectionTypes = new Stack<Type>();
        }

        #endregion Constructors.

        #region Internal properties.

        internal bool CurrentIsEntity
        {
            get { return this.entityInScope.Peek(); }
        }

        internal Expression ExpectedParamTypeInScope
        {
            get
            {
                Debug.Assert(this.parameterExpressionTypes.Count > 0, "this.parameterExpressionTypes.Count > 0");
                return this.parameterExpressionTypes.Peek();
            }
        }

        internal bool HasRewrites
        {
            get { return this.rewrites.Count > 0; }
        }

        internal Expression LambdaParameterInScope
        {
            get
            {
                return this.parameterExpressions.Peek();
            }
        }

        internal Expression ParameterEntryInScope
        {
            get
            {
                return this.parameterEntries.Peek();
            }
        }

        #endregion Internal properties.

        #region Methods.

        public override string ToString()
        {
            string result = "ProjectionPathBuilder: ";
            if (this.parameterExpressions.Count == 0)
            {
                result += "(empty)";
            }
            else
            {
                result +=
                    "entity:" + this.CurrentIsEntity +
                    " param:" + this.ParameterEntryInScope;
            }

            return result;
        }

        internal void EnterLambdaScope(LambdaExpression lambda, Expression entry, Expression expectedType)
        {
            Debug.Assert(lambda != null, "lambda != null");
            Debug.Assert(lambda.Parameters.Count == 1, "lambda.Parameters.Count == 1");

            ParameterExpression param = lambda.Parameters[0];
            Type projectionType = lambda.Body.Type;
            bool isEntityType = ClientType.CheckElementTypeIsEntity(projectionType);

            this.entityInScope.Push(isEntityType);
            this.parameterExpressions.Push(param);
            this.parameterExpressionTypes.Push(expectedType);
            this.parameterEntries.Push(entry);
            this.parameterProjectionTypes.Push(projectionType);
        }

        internal void EnterMemberInit(MemberInitExpression init)
        {
            bool isEntityType = ClientType.CheckElementTypeIsEntity(init.Type);
            this.entityInScope.Push(isEntityType);
        }

        internal Expression GetRewrite(Expression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            List<string> names = new List<string>();
            while (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression member = (MemberExpression)expression;
                names.Add(member.Member.Name);
                expression = member.Expression;
            }

            Expression result = null;
            foreach (var rewrite in this.rewrites)
            {
                if (rewrite.Root != expression)
                {
                    continue;
                }

                if (names.Count != rewrite.MemberNames.Length)
                {
                    continue;
                }

                bool match = true;
                for (int i = 0; i < names.Count && i < rewrite.MemberNames.Length; i++)
                {
                    if (names[names.Count - i - 1] != rewrite.MemberNames[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    result = rewrite.RewriteExpression;
                    break;
                }
            }

            return result;
        }

        internal void LeaveLambdaScope()
        {
            this.entityInScope.Pop(); 
            this.parameterExpressions.Pop();
            this.parameterExpressionTypes.Pop();
            this.parameterEntries.Pop();
            this.parameterProjectionTypes.Pop();
        }

        internal void LeaveMemberInit()
        {
            this.entityInScope.Pop();
        }

        internal void RegisterRewrite(Expression root, string[] names, Expression rewriteExpression)
        {
            Debug.Assert(root != null, "root != null");
            Debug.Assert(names != null, "names != null");
            Debug.Assert(rewriteExpression != null, "rewriteExpression != null");

            this.rewrites.Add(new MemberInitRewrite() { Root = root, MemberNames = names, RewriteExpression = rewriteExpression });
            this.parameterEntries.Push(rewriteExpression);
        }

        internal void RevokeRewrite(Expression root, string[] names)
        {
            Debug.Assert(root != null, "root != null");

            for (int i = 0; i < this.rewrites.Count; i++)
            {
                if (this.rewrites[i].Root != root)
                {
                    continue;
                }

                if (names.Length != this.rewrites[i].MemberNames.Length)
                {
                    continue;
                }

                bool match = true;
                for (int j = 0; j < names.Length; j++)
                {
                    if (names[j] != this.rewrites[i].MemberNames[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    this.rewrites.RemoveAt(i);
                    this.parameterEntries.Pop();
                    return;
                }
            }
        }

        #endregion Methods.

        #region Inner types.

        internal class MemberInitRewrite
        {
            internal string[] MemberNames 
            { 
                get; 
                set; 
            }

            internal Expression Root 
            { 
                get; 
                set; 
            }
         
            internal Expression RewriteExpression
            { 
                get; 
                set; 
            }
        }

        #endregion Inner types.
    }
}
