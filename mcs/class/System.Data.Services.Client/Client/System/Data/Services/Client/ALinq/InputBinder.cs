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
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.

    internal sealed class InputBinder : DataServiceExpressionVisitor
    {
        #region Private fields.

        private readonly HashSet<ResourceExpression> referencedInputs = new HashSet<ResourceExpression>(EqualityComparer<ResourceExpression>.Default);

        private readonly ResourceExpression input;

        private readonly ResourceSetExpression inputSet;
        
        private readonly ParameterExpression inputParameter;

        #endregion Private fields.

        private InputBinder(ResourceExpression resource, ParameterExpression setReferenceParam)
        {
            this.input = resource;
            this.inputSet = resource as ResourceSetExpression;
            this.inputParameter = setReferenceParam;
        }

        internal static Expression Bind(Expression e, ResourceExpression currentInput, ParameterExpression inputParameter, List<ResourceExpression> referencedInputs)
        {
            Debug.Assert(e != null, "Expression cannot be null");
            Debug.Assert(currentInput != null, "A current input resource set is required");
            Debug.Assert(inputParameter != null, "The input lambda parameter is required");
            Debug.Assert(referencedInputs != null, "The referenced inputs list is required");

            InputBinder binder = new InputBinder(currentInput, inputParameter);
            Expression result = binder.Visit(e);
            referencedInputs.AddRange(binder.referencedInputs);
            return result;
        }
                
        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (this.inputSet == null ||
                !this.inputSet.HasTransparentScope)
            {
                return base.VisitMemberAccess(m);
            }

            ParameterExpression innerParamRef = null;
            Stack<PropertyInfo> nestedAccesses = new Stack<PropertyInfo>();
            MemberExpression memberRef = m;
            while (memberRef != null &&
                   memberRef.Member.MemberType == MemberTypes.Property &&
                   memberRef.Expression != null)
            {
                nestedAccesses.Push((PropertyInfo)memberRef.Member);

                if (memberRef.Expression.NodeType == ExpressionType.Parameter)
                {
                    innerParamRef = (ParameterExpression)memberRef.Expression;
                }

                memberRef = memberRef.Expression as MemberExpression;
            }

            if (innerParamRef != this.inputParameter || nestedAccesses.Count == 0)
            {
                return m;
            }

            ResourceExpression target = this.input;
            ResourceSetExpression targetSet = this.inputSet;
            bool transparentScopeTraversed = false;

            while (nestedAccesses.Count > 0)
            {
                if (targetSet == null || !targetSet.HasTransparentScope)
                {
                    break;
                }

                PropertyInfo currentProp = nestedAccesses.Peek();

                if (currentProp.Name.Equals(targetSet.TransparentScope.Accessor, StringComparison.Ordinal))
                {
                    target = targetSet;
                    nestedAccesses.Pop();
                    transparentScopeTraversed = true;
                    continue;
                }

                Expression source;
                if (!targetSet.TransparentScope.SourceAccessors.TryGetValue(currentProp.Name, out source))
                {
                    break;
                }

                transparentScopeTraversed = true;
                nestedAccesses.Pop();
                Debug.Assert(source != null, "source != null -- otherwise ResourceBinder created an accessor to nowhere");
                InputReferenceExpression sourceReference = source as InputReferenceExpression;
                if (sourceReference == null)
                {
                    targetSet = source as ResourceSetExpression;
                    if (targetSet == null || !targetSet.HasTransparentScope)
                    {
                        target = (ResourceExpression)source;
                    }
                }
                else
                {
                    targetSet = sourceReference.Target as ResourceSetExpression;
                    target = targetSet;
                }
            }

            if (!transparentScopeTraversed)
            {
                return m;
            }

            Expression result = this.CreateReference(target);
            while (nestedAccesses.Count > 0)
            {
                result = Expression.Property(result, nestedAccesses.Pop());
            }

            return result;
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if ((this.inputSet == null || !this.inputSet.HasTransparentScope) &&
               p == this.inputParameter)
            {
                return this.CreateReference(this.input);
            }
            else
            {
                return base.VisitParameter(p);
            }
        }

        private Expression CreateReference(ResourceExpression resource)
        {
            this.referencedInputs.Add(resource);
            return resource.CreateReference();
        }
    }
}

