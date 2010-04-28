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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.
    
    internal class ResourceBinder : DataServiceExpressionVisitor
    {
        internal static Expression Bind(Expression e)
        {
            Debug.Assert(e != null, "e != null");

            ResourceBinder rb = new ResourceBinder();
            Expression boundExpression = rb.Visit(e);
            VerifyKeyPredicates(boundExpression);
            VerifyNotSelectManyProjection(boundExpression);
            return boundExpression;
        }

        internal static bool IsMissingKeyPredicates(Expression expression)
        {
            ResourceExpression re = expression as ResourceExpression;
            if (re != null)
            {
                if (IsMissingKeyPredicates(re.Source))
                {
                    return true;
                }

                if (re.Source != null)
                {
                    ResourceSetExpression rse = re.Source as ResourceSetExpression;
                    if ((rse != null) && !rse.HasKeyPredicate)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static void VerifyKeyPredicates(Expression e)
        {
            if (IsMissingKeyPredicates(e))
            {
                throw new NotSupportedException(Strings.ALinq_CantNavigateWithoutKeyPredicate);
            }
        }

        internal static void VerifyNotSelectManyProjection(Expression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            ResourceSetExpression resourceSet = expression as ResourceSetExpression;
            if (resourceSet != null)
            {
                ProjectionQueryOptionExpression projection = resourceSet.Projection;
                if (projection != null)
                {
                    Debug.Assert(projection.Selector != null, "projection.Selector != null -- otherwise incorrectly constructed");
                    MethodCallExpression call = StripTo<MethodCallExpression>(projection.Selector.Body);
                    if (call != null && call.Method.Name == "SelectMany")
                    {
                        throw new NotSupportedException(Strings.ALinq_UnsupportedExpression(call));
                    }
                }
                else if (resourceSet.HasTransparentScope)
                {
                    throw new NotSupportedException(Strings.ALinq_UnsupportedExpression(resourceSet));
                }
            }
        }

        private static Expression AnalyzePredicate(MethodCallExpression mce)
        {
            Debug.Assert(mce != null, "mce != null -- caller couldn't have know the expression kind otherwise");
            Debug.Assert(mce.Method.Name == "Where", "mce.Method.Name == 'Where' -- otherwise this isn't a predicate");

            ResourceSetExpression input;
            LambdaExpression le;
            if (!TryGetResourceSetMethodArguments(mce, out input, out le))
            {
                ValidationRules.RequireNonSingleton(mce.Arguments[0]);
                return mce;
            }


            List<Expression> conjuncts = new List<Expression>();
            AddConjuncts(le.Body, conjuncts);

            Dictionary<ResourceSetExpression, List<Expression>> predicatesByTarget = new Dictionary<ResourceSetExpression, List<Expression>>(ReferenceEqualityComparer<ResourceSetExpression>.Instance);
            List<ResourceExpression> referencedInputs = new List<ResourceExpression>();
            foreach (Expression e in conjuncts)
            {
                Expression reboundPredicate = InputBinder.Bind(e, input, le.Parameters[0], referencedInputs);
                if (referencedInputs.Count > 1)
                {
                    return mce;
                }

                ResourceSetExpression boundTarget = (referencedInputs.Count == 0 ? input : referencedInputs[0] as ResourceSetExpression);
                if (boundTarget == null)
                {
                    return mce;
                }

                List<Expression> targetPredicates = null;
                if (!predicatesByTarget.TryGetValue(boundTarget, out targetPredicates))
                {
                    targetPredicates = new List<Expression>();
                    predicatesByTarget[boundTarget] = targetPredicates;
                }

                targetPredicates.Add(reboundPredicate);
                referencedInputs.Clear();
            }

            conjuncts = null;
            List<Expression> inputPredicates;
            if (predicatesByTarget.TryGetValue(input, out inputPredicates))
            {
                predicatesByTarget.Remove(input);
            }
            else
            {
                inputPredicates = null;
            }

            foreach (KeyValuePair<ResourceSetExpression, List<Expression>> predicates in predicatesByTarget)
            {
                ResourceSetExpression target = predicates.Key;
                List<Expression> clauses = predicates.Value;

                Dictionary<PropertyInfo, ConstantExpression> keyValues;
                if (!ExtractKeyPredicate(target, clauses, out keyValues) ||
                    clauses.Count > 0)
                {
                    return mce;
                }

                Debug.Assert(!target.HasQueryOptions, "Navigation source had query options?");

                SetKeyPredicate(target, keyValues);
            }

            if (inputPredicates != null)
            {
                Dictionary<PropertyInfo, ConstantExpression> inputKeyValues;
                if (ExtractKeyPredicate(input, inputPredicates, out inputKeyValues))
                {
                    if (input.HasSequenceQueryOptions)
                    {
                        Expression predicateFilter = BuildKeyPredicateFilter(input.CreateReference(), inputKeyValues);
                        inputPredicates.Add(predicateFilter);
                    }
                    else
                    {
                        SetKeyPredicate(input, inputKeyValues);
                    }
                }

                if (inputPredicates.Count > 0)
                {
                    if (input.KeyPredicate != null)
                    {
                        Expression predicateFilter = BuildKeyPredicateFilter(input.CreateReference(), input.KeyPredicate);
                        inputPredicates.Add(predicateFilter);
                        input.KeyPredicate = null;
                    }

                    int start;
                    Expression newFilter;
                    if (input.Filter != null)
                    {
                        start = 0;
                        newFilter = input.Filter.Predicate;
                    }
                    else
                    {
                        start = 1;
                        newFilter = inputPredicates[0];
                    }

                    for (int idx = start; idx < inputPredicates.Count; idx++)
                    {
                        newFilter = Expression.And(newFilter, inputPredicates[idx]);
                    }

                    AddSequenceQueryOption(input, new FilterQueryOptionExpression(mce.Method.ReturnType, newFilter));
                }
            }

            return input;        }

        private static void SetKeyPredicate(ResourceSetExpression rse, Dictionary<PropertyInfo, ConstantExpression> keyValues)
        {
            Debug.Assert(rse != null, "rse != null");
            Debug.Assert(keyValues != null, "keyValues != null");

            if (rse.KeyPredicate == null)
            {
                rse.KeyPredicate = new Dictionary<PropertyInfo, ConstantExpression>(EqualityComparer<PropertyInfo>.Default);
            }

            foreach(var g in keyValues)
            {
                if (rse.KeyPredicate.Keys.Contains(g.Key))
                {
                    throw Error.NotSupported(Strings.ALinq_CanOnlyApplyOneKeyPredicate);
                }

                rse.KeyPredicate.Add(g.Key, g.Value);
            }
        }

        private static bool CollectionContentsEqual<T>(ICollection<T> left, ICollection<T> right, IEqualityComparer<T> comparer) where T : class
        {
            Debug.Assert(left != null, "left != null");
            Debug.Assert(right != null, "right != null");
            Debug.Assert(comparer != null, "comparer != null");

            if (left.Count != right.Count)
            {
                return false;
            }

            if (left.Count == 1)
            {
                return comparer.Equals(left.First(), right.First());
            }
            else
            {
#if ASTORIA_LIGHT                
                HashSet<T> leftElements = new HashSet<T>(comparer);
                foreach (var l in left)
                {
                    leftElements.Add(l);
                }
#else
                HashSet<T> leftElements = new HashSet<T>(left, comparer);
#endif
                foreach (T rightElement in right)
                {
                    if (!leftElements.Contains(rightElement))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static bool ExtractKeyPredicate(
            ResourceSetExpression target, 
            List<Expression> predicates, 
            out Dictionary<PropertyInfo, ConstantExpression> keyValues)
        {
            Debug.Assert(target != null, "target != null");
            Debug.Assert(predicates != null, "predicates != null");

            keyValues = null;
            List<Expression> nonKeyPredicates = null;

            foreach (Expression predicate in predicates)
            {
                PropertyInfo property;
                ConstantExpression constantValue;
                if (PatternRules.MatchKeyComparison(predicate, out property, out constantValue))
                {
                    if (keyValues == null)
                    {
                        keyValues = new Dictionary<PropertyInfo, ConstantExpression>(EqualityComparer<PropertyInfo>.Default);
                    }
                    else if (keyValues.ContainsKey(property))
                    {
                        throw Error.NotSupported(Strings.ALinq_CanOnlyApplyOneKeyPredicate);
                    }

                    keyValues.Add(property, constantValue);
                }
                else
                {
                    if (nonKeyPredicates == null)
                    {
                        nonKeyPredicates = new List<Expression>();
                    }

                    nonKeyPredicates.Add(predicate);
                }
            }

            Debug.Assert(keyValues != null || nonKeyPredicates != null, "No key predicates or non-key predicates found?");
            if (keyValues != null)
            {
                var properties = PatternRules.GetKeyProperties(target.CreateReference().Type);
                if (!CollectionContentsEqual(properties, keyValues.Keys, PropertyInfoEqualityComparer.Instance))
                {
                    keyValues = null;
                    return false;
                }
            } 

            if (keyValues != null)
            {
                predicates.Clear();

                if (nonKeyPredicates != null)
                {
                    predicates.AddRange(nonKeyPredicates);
                }
            }

            return keyValues != null;
        }

        private static Expression BuildKeyPredicateFilter(InputReferenceExpression input, Dictionary<PropertyInfo, ConstantExpression> keyValuesDictionary)
        {
            Debug.Assert(input != null, "input != null");
            Debug.Assert(keyValuesDictionary != null, "keyValuesDictionary != null");
            Debug.Assert(keyValuesDictionary.Count > 0, "At least one key property is required in a key predicate");

            Expression retExpr = null;
            foreach (KeyValuePair<PropertyInfo, ConstantExpression> keyValue in keyValuesDictionary)
            {
                Expression clause = Expression.Equal(Expression.Property(input, keyValue.Key), keyValue.Value);
                if (retExpr == null)
                {
                    retExpr = clause;
                }
                else
                {
                    retExpr = Expression.And(retExpr, clause);
                }
            }

            return retExpr;
        }

        private static void AddConjuncts(Expression e, List<Expression> conjuncts)
        {
            Debug.Assert(conjuncts != null, "conjuncts != null");
            if (PatternRules.MatchAnd(e))
            {
                BinaryExpression be = (BinaryExpression)e;
                AddConjuncts(be.Left, conjuncts);
                AddConjuncts(be.Right, conjuncts);
            }
            else
            {
                conjuncts.Add(e);
            }
        }

        internal bool AnalyzeProjection(MethodCallExpression mce, SequenceMethod sequenceMethod, out Expression e)
        {
            Debug.Assert(mce != null, "mce != null");
            Debug.Assert(
                sequenceMethod == SequenceMethod.Select || sequenceMethod == SequenceMethod.SelectManyResultSelector,
                "sequenceMethod == SequenceMethod.Select(ManyResultSelector)");

            e = mce;

            bool matchMembers = sequenceMethod == SequenceMethod.SelectManyResultSelector;
            ResourceExpression source = this.Visit(mce.Arguments[0]) as ResourceExpression;
            if (source == null)
            {
                return false;
            }

            if (sequenceMethod == SequenceMethod.SelectManyResultSelector)
            {
                Expression collectionSelector = mce.Arguments[1];
                if (!PatternRules.MatchParameterMemberAccess(collectionSelector))
                {
                    return false;
                }

                Expression resultSelector = mce.Arguments[2];
                LambdaExpression resultLambda;
                if (!PatternRules.MatchDoubleArgumentLambda(resultSelector, out resultLambda))
                {
                    return false;
                }

                if (ExpressionPresenceVisitor.IsExpressionPresent(resultLambda.Parameters[0], resultLambda.Body))
                {
                    return false;
                }

                List<ResourceExpression> referencedExpressions = new List<ResourceExpression>();
                LambdaExpression collectionLambda = StripTo<LambdaExpression>(collectionSelector);
                Expression collectorReference = InputBinder.Bind(collectionLambda.Body, source, collectionLambda.Parameters[0], referencedExpressions);
                collectorReference = StripCastMethodCalls(collectorReference);
                MemberExpression navigationMember;
                if (!PatternRules.MatchPropertyProjectionSet(source, collectorReference, out navigationMember))
                {
                    return false;
                }

                collectorReference = navigationMember;

                ResourceExpression resultSelectorSource = CreateResourceSetExpression(mce.Method.ReturnType, source, collectorReference, TypeSystem.GetElementType(collectorReference.Type));

                if (!PatternRules.MatchMemberInitExpressionWithDefaultConstructor(resultSelectorSource, resultLambda) &&
                    !PatternRules.MatchNewExpression(resultSelectorSource, resultLambda))
                {
                    return false;
                }

#if ASTORIA_LIGHT
                resultLambda = ExpressionHelpers.CreateLambda(resultLambda.Body, new ParameterExpression[] { resultLambda.Parameters[1] });
#else
                resultLambda = Expression.Lambda(resultLambda.Body, new ParameterExpression[] { resultLambda.Parameters[1] });
#endif

                ResourceExpression resultWithProjection = resultSelectorSource.CreateCloneWithNewType(mce.Type);
                bool isProjection;
                try
                {
                    isProjection = ProjectionAnalyzer.Analyze(resultLambda, resultWithProjection, false);
                }
                catch (NotSupportedException)
                {
                    isProjection = false;
                }

                if (!isProjection)
                {
                    return false;
                }

                e = resultWithProjection;
                ValidationRules.RequireCanProject(resultSelectorSource);
            }
            else
            {
                LambdaExpression lambda;
                if (!PatternRules.MatchSingleArgumentLambda(mce.Arguments[1], out lambda))
                {
                    return false;
                }

                lambda = ProjectionRewriter.TryToRewrite(lambda, source.ResourceType);

                ResourceExpression re = source.CreateCloneWithNewType(mce.Type);

                if (!ProjectionAnalyzer.Analyze(lambda, re, matchMembers))
                {
                    return false;
                }

                ValidationRules.RequireCanProject(source);
                e = re;
            }

            return true;
        }

        internal static Expression AnalyzeNavigation(MethodCallExpression mce)
        {
            Debug.Assert(mce != null, "mce != null");
            Expression input = mce.Arguments[0];
            LambdaExpression le;
            ResourceExpression navSource;
            Expression boundProjection;
            MemberExpression navigationMember;

            if (!PatternRules.MatchSingleArgumentLambda(mce.Arguments[1], out le))
            {
                return mce;
            }         
            else if (PatternRules.MatchIdentitySelector(le))
            {
                return input;
            }
            else if (PatternRules.MatchTransparentIdentitySelector(input, le))
            {
                return RemoveTransparentScope(mce.Method.ReturnType, (ResourceSetExpression)input);
            }
            else if (IsValidNavigationSource(input, out navSource) &&
                TryBindToInput(navSource, le, out boundProjection) &&
                PatternRules.MatchPropertyProjectionSingleton(navSource, boundProjection, out navigationMember))
            {
                boundProjection = navigationMember;
                return CreateNavigationPropertySingletonExpression(mce.Method.ReturnType, navSource, boundProjection);
            }

            return mce;
        }

        private static bool IsValidNavigationSource(Expression input, out ResourceExpression sourceExpression)
        {
            ValidationRules.RequireCanNavigate(input);
            sourceExpression = input as ResourceExpression;
            return sourceExpression != null;
        }

        internal static Expression AnalyzeSelectMany(MethodCallExpression mce)
        {
            Debug.Assert(mce != null, "mce != null");

            if (mce.Arguments.Count != 2 && mce.Arguments.Count != 3)
            {
                return mce;
            }

            ResourceExpression input;
            if (!IsValidNavigationSource(mce.Arguments[0], out input))
            {
                return mce;
            }

            LambdaExpression collectorSelector;
            if (!PatternRules.MatchSingleArgumentLambda(mce.Arguments[1], out collectorSelector))
            {
                return mce;
            }

            List<ResourceExpression> referencedInputs = new List<ResourceExpression>();
            Expression navPropRef = InputBinder.Bind(collectorSelector.Body, input, collectorSelector.Parameters[0], referencedInputs);
            Type resourceType = TypeSystem.GetElementType(navPropRef.Type);

            navPropRef = StripCastMethodCalls(navPropRef);

            ResourceSetExpression rse = null;
            MemberExpression navigationMember;
            if (PatternRules.MatchPropertyProjectionSet(input, navPropRef, out navigationMember))
            {
                navPropRef = navigationMember;
                rse = CreateResourceSetExpression(mce.Method.ReturnType, input, navPropRef, resourceType);
            }
            else
            {
                return mce;
            }

            if (mce.Arguments.Count == 3)
            {
                return AnalyzeSelectManySelector(mce, rse);
            }
            else
            {
                return rse;
            }
        }

        private static Expression AnalyzeSelectManySelector(MethodCallExpression selectManyCall, ResourceSetExpression sourceResourceSet)
        {
            Debug.Assert(selectManyCall != null, "selectManyCall != null");

            LambdaExpression selector = StripTo<LambdaExpression>(selectManyCall.Arguments[2]);
            
            Expression result;
            ResourceSetExpression.TransparentAccessors transparentScope;
            if (PatternRules.MatchTransparentScopeSelector(sourceResourceSet, selector, out transparentScope))
            {
                sourceResourceSet.TransparentScope = transparentScope;
                result = sourceResourceSet;
            }
            else if (PatternRules.MatchIdentityProjectionResultSelector(selector))
            {
                result = sourceResourceSet;
            }
            else if (PatternRules.MatchMemberInitExpressionWithDefaultConstructor(sourceResourceSet, selector) || PatternRules.MatchNewExpression(sourceResourceSet, selector))
            {
#if ASTORIA_LIGHT
                selector = ExpressionHelpers.CreateLambda(selector.Body, new ParameterExpression[] { selector.Parameters[1] }); 
#else
                selector = Expression.Lambda(selector.Body, new ParameterExpression[] { selector.Parameters[1] });
#endif
                if (!ProjectionAnalyzer.Analyze(selector, sourceResourceSet, false))
                {
                    result = selectManyCall;
                }
                else
                {
                    result = sourceResourceSet;
                }
            }
            else
            {
                result = selectManyCall;
            }

            return result;
        }

        internal static Expression ApplyOrdering(MethodCallExpression mce, ResourceSetExpression input, Expression selector, bool descending, bool thenBy)
        {
            List<OrderByQueryOptionExpression.Selector> selectors;
            if (!thenBy)
            {
                selectors = new List<OrderByQueryOptionExpression.Selector>();
                AddSequenceQueryOption(input, new OrderByQueryOptionExpression(mce.Type, selectors));
            }
            else
            {
                Debug.Assert(input.OrderBy != null, "input.OrderBy != null");
                selectors = input.OrderBy.Selectors;
            }

            selectors.Add(new OrderByQueryOptionExpression.Selector(selector, descending));

            return input;
        }

#if !ASTORIA_LIGHT
        private static Expression LimitCardinality(MethodCallExpression mce, int maxCardinality)
        {
            Debug.Assert(mce != null, "mce != null");
            Debug.Assert(maxCardinality > 0, "Cardinality must be at least 1");

            if (mce.Arguments.Count != 1)
            {
                return mce;
            }

            ResourceSetExpression rse = mce.Arguments[0] as ResourceSetExpression;
            if (rse != null)
            {
                if (!rse.HasKeyPredicate &&                    (ResourceExpressionType)rse.NodeType != ResourceExpressionType.ResourceNavigationProperty)
                {
                    if (rse.Take == null || (int)rse.Take.TakeAmount.Value > maxCardinality)
                    {
                        AddSequenceQueryOption(rse, new TakeQueryOptionExpression(mce.Type, Expression.Constant(maxCardinality)));
                    }
                }
                return mce.Arguments[0];
            }
            else if (mce.Arguments[0] is NavigationPropertySingletonExpression)
            {
                return mce.Arguments[0];
            }

            return mce;
        }

#endif

        private static Expression AnalyzeCast(MethodCallExpression mce)
        {
            ResourceExpression re = mce.Arguments[0] as ResourceExpression;
            if (re != null)
            {
                return re.CreateCloneWithNewType(mce.Method.ReturnType);
            }

            return mce;
        }

        private static Expression StripConvert(Expression e)
        {
            UnaryExpression ue = e as UnaryExpression;

            if (ue != null &&
                ue.NodeType == ExpressionType.Convert &&
                ue.Type.IsGenericType && 
                (ue.Type.GetGenericTypeDefinition() == typeof(DataServiceQuery<>) ||
                 ue.Type.GetGenericTypeDefinition() == typeof(DataServiceQuery<>.DataServiceOrderedQuery)))
            {
                e = ue.Operand;
                ResourceExpression re = e as ResourceExpression;
                if (re != null)
                {
                    e = re.CreateCloneWithNewType(ue.Type);
                }
            }

            return e;
        }

        private static Expression AnalyzeExpand(MethodCallExpression mce)
        {
            Expression obj = StripConvert(mce.Object);
            ResourceExpression re = obj as ResourceExpression;
            if (re == null)
            {
                return mce;
            }

            ValidationRules.RequireCanExpand(re);
            ConstantExpression ce = StripTo<ConstantExpression>(mce.Arguments[0]);
            string path = (string)ce.Value;
            if (!re.ExpandPaths.Contains(path))
            {
                re.ExpandPaths.Add(path);
            }

            return re;
        }
        
        private static Expression AnalyzeAddCustomQueryOption(MethodCallExpression mce)
        {
            Expression obj = StripConvert(mce.Object);
            ResourceExpression re = obj as ResourceExpression;
            if (re == null)
            {
                return mce;
            }

            ValidationRules.RequireCanAddCustomQueryOption(re);

            ConstantExpression name = StripTo<ConstantExpression>(mce.Arguments[0]);
            ConstantExpression value = StripTo<ConstantExpression>(mce.Arguments[1]);

            if (((string)name.Value).Trim() == UriHelper.DOLLARSIGN + UriHelper.OPTIONEXPAND)
            {
                ValidationRules.RequireCanExpand(re);
                re.ExpandPaths = re.ExpandPaths.Union(new string[] { (string) value.Value }, StringComparer.Ordinal).ToList();
            }
            else
            {
                ValidationRules.RequireLegalCustomQueryOption(mce.Arguments[0], re);
                re.CustomQueryOptions.Add(name, value);
            }

            return re;
        }

        private static Expression AnalyzeAddCountOption(MethodCallExpression mce, CountOption countOption)
        {
            Expression obj = StripConvert(mce.Object);
            ResourceExpression re = obj as ResourceExpression;
            if (re == null)
            {
                return mce;
            }

            ValidationRules.RequireCanAddCount(re);
            ValidationRules.RequireNonSingleton(re);
            re.CountOption = countOption;

            return re;
        }

        private static ResourceSetExpression CreateResourceSetExpression(Type type, ResourceExpression source, Expression memberExpression, Type resourceType)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(source != null, "source != null");
            Debug.Assert(memberExpression != null, "memberExpression != null");
            Debug.Assert(resourceType != null, "resourceType != null");

            Type elementType = TypeSystem.GetElementType(type);
            Debug.Assert(elementType != null, "elementType != null -- otherwise the set isn't going to act like a collection");
            Type expressionType = typeof(IOrderedQueryable<>).MakeGenericType(elementType);

            ResourceSetExpression newResource = new ResourceSetExpression(expressionType, source, memberExpression, resourceType, source.ExpandPaths.ToList(), source.CountOption, source.CustomQueryOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), null);
            source.ExpandPaths.Clear();
            source.CountOption = CountOption.None;
            source.CustomQueryOptions.Clear();
            return newResource;
        }

        private static NavigationPropertySingletonExpression CreateNavigationPropertySingletonExpression(Type type, ResourceExpression source, Expression memberExpression)
        {
            NavigationPropertySingletonExpression newResource = new NavigationPropertySingletonExpression(type, source, memberExpression, memberExpression.Type, source.ExpandPaths.ToList(), source.CountOption, source.CustomQueryOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), null);
            source.ExpandPaths.Clear();
            source.CountOption = CountOption.None;
            source.CustomQueryOptions.Clear();
            return newResource;
        }

        private static ResourceSetExpression RemoveTransparentScope(Type expectedResultType, ResourceSetExpression input)
        {
            ResourceSetExpression newResource = new ResourceSetExpression(expectedResultType, input.Source, input.MemberExpression, input.ResourceType, input.ExpandPaths, input.CountOption, input.CustomQueryOptions, input.Projection);
            
            newResource.KeyPredicate = input.KeyPredicate;
            foreach (QueryOptionExpression queryOption in input.SequenceQueryOptions)
            {
                newResource.AddSequenceQueryOption(queryOption);
            }

            newResource.OverrideInputReference(input);

            return newResource;
        }

        internal static Expression StripConvertToAssignable(Expression e)
        {
            Debug.Assert(e != null, "e != null");
            
            Expression result;
            UnaryExpression unary = e as UnaryExpression;
            if (unary != null && PatternRules.MatchConvertToAssignable(unary))
            {
                result = unary.Operand;
            }
            else
            {
                result = e;
            }

            return result;
        }

        internal static T StripTo<T>(Expression expression) where T : Expression
        {
            Debug.Assert(expression != null, "expression != null");

            Expression result;
            do
            {
                result = expression;
                expression = expression.NodeType == ExpressionType.Quote ? ((UnaryExpression)expression).Operand : expression;
                expression = StripConvertToAssignable(expression);
            }
            while (result != expression);

            return result as T;
        }

        internal override Expression VisitResourceSetExpression(ResourceSetExpression rse)
        {
            Debug.Assert(rse != null, "rse != null");

            if ((ResourceExpressionType)rse.NodeType == ResourceExpressionType.RootResourceSet)
            {

                return new ResourceSetExpression(rse.Type, rse.Source, rse.MemberExpression, rse.ResourceType, null, CountOption.None, null, null);
            }

            return rse;
        }

        private static bool TryGetResourceSetMethodArguments(MethodCallExpression mce, out ResourceSetExpression input, out LambdaExpression lambda)
        {
            input = null;
            lambda = null;

            input = mce.Arguments[0] as ResourceSetExpression;
            if (input != null &&
                PatternRules.MatchSingleArgumentLambda(mce.Arguments[1], out lambda))
            {
                return true;
            }

            return false;
        }

        private static bool TryBindToInput(ResourceExpression input, LambdaExpression le, out Expression bound)
        {
            List<ResourceExpression> referencedInputs = new List<ResourceExpression>();
            bound = InputBinder.Bind(le.Body, input, le.Parameters[0], referencedInputs);
            if (referencedInputs.Count > 1 || (referencedInputs.Count == 1 && referencedInputs[0] != input))
            {
                bound = null;
            }

            return bound != null;
        }

        private static Expression AnalyzeResourceSetMethod(MethodCallExpression mce, Func<MethodCallExpression, ResourceSetExpression, Expression, Expression> sequenceMethodAnalyzer)
        {
            ResourceSetExpression input;
            LambdaExpression le;
            if (!TryGetResourceSetMethodArguments(mce, out input, out le))
            {
                return mce;
            }

            Expression lambdaBody;
            if (!TryBindToInput(input, le, out lambdaBody))
            {
                return mce;
            }

            return sequenceMethodAnalyzer(mce, input, lambdaBody);
        }

        private static Expression AnalyzeResourceSetConstantMethod(MethodCallExpression mce, Func<MethodCallExpression, ResourceExpression, ConstantExpression, Expression> constantMethodAnalyzer)
        {
            ResourceExpression input = (ResourceExpression)mce.Arguments[0];
            ConstantExpression constantArg = StripTo<ConstantExpression>(mce.Arguments[1]);
            if (null == constantArg)
            {
                return mce;
            }

            return constantMethodAnalyzer(mce, input, constantArg);
        }

        private static Expression AnalyzeCountMethod(MethodCallExpression mce)
        {
            ResourceExpression re = (ResourceExpression)mce.Arguments[0];
            if (re == null)
            {
                return mce;
            }

            ValidationRules.RequireCanAddCount(re);
            ValidationRules.RequireNonSingleton(re);
            re.CountOption = CountOption.ValueOnly;

            return re;
        }

        private static void AddSequenceQueryOption(ResourceExpression target, QueryOptionExpression qoe)
        {
            ValidationRules.RequireNonSingleton(target);
            ResourceSetExpression rse = (ResourceSetExpression)target;

            switch (qoe.NodeType)
            {
                case (ExpressionType)ResourceExpressionType.FilterQueryOption:
                    if (rse.Skip != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("filter", "skip"));
                    }
                    else if (rse.Take != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("filter", "top"));
                    }
                    else if (rse.Projection != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("filter", "select"));
                    }
                    break;
                case (ExpressionType)ResourceExpressionType.OrderByQueryOption:
                    if (rse.Skip != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("orderby", "skip"));
                    }
                    else if (rse.Take != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("orderby", "top"));
                    }
                    else if (rse.Projection != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("orderby", "select"));
                    }
                    break;
                case (ExpressionType)ResourceExpressionType.SkipQueryOption:
                    if (rse.Take != null)
                    {
                        throw new NotSupportedException(Strings.ALinq_QueryOptionOutOfOrder("skip", "top"));
                    }
                    break;
                default:
                    break;
            }

            rse.AddSequenceQueryOption(qoe);
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            Expression e = base.VisitBinary(b);
            if (PatternRules.MatchStringAddition(e))
            {
                BinaryExpression be = StripTo<BinaryExpression>(e);
                MethodInfo mi = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string)});
                return Expression.Call(mi, new Expression[] {be.Left, be.Right});
            }

            return e;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression e = base.VisitMemberAccess(m);
            MemberExpression me = StripTo<MemberExpression>(e);
            PropertyInfo pi;
            MethodInfo mi;
            if (me != null &&
                PatternRules.MatchNonPrivateReadableProperty(me, out pi) &&
                TypeSystem.TryGetPropertyAsMethod(pi, out mi))
            {
                return Expression.Call(me.Expression, mi);
            }

            return e;
        }

        internal override Expression VisitMethodCall(MethodCallExpression mce)
        {
            Expression e;

            SequenceMethod sequenceMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(mce.Method, out sequenceMethod))
            {
                if (sequenceMethod == SequenceMethod.Select ||
                    sequenceMethod == SequenceMethod.SelectManyResultSelector)
                {
                    if (this.AnalyzeProjection(mce, sequenceMethod, out e))
                    {
                        return e;
                    }
                }
            }

            e = base.VisitMethodCall(mce);
            mce = e as MethodCallExpression;

            if (mce != null)
            {
                if (ReflectionUtil.TryIdentifySequenceMethod(mce.Method, out sequenceMethod))
                {
                    switch (sequenceMethod)
                    {
                        case SequenceMethod.Where:
                            return AnalyzePredicate(mce);
                        case SequenceMethod.Select:
                            return AnalyzeNavigation(mce);
                        case SequenceMethod.SelectMany:
                        case SequenceMethod.SelectManyResultSelector:
                            {
                                Expression result = AnalyzeSelectMany(mce);
                                return result;
                            }

                        case SequenceMethod.Take:
                            return AnalyzeResourceSetConstantMethod(mce, (callExp, resource, takeCount) => { AddSequenceQueryOption(resource, new TakeQueryOptionExpression(callExp.Type, takeCount)); return resource; });
                        case SequenceMethod.Skip:
                            return AnalyzeResourceSetConstantMethod(mce, (callExp, resource, skipCount) => { AddSequenceQueryOption(resource, new SkipQueryOptionExpression(callExp.Type, skipCount)); return resource; });
                        case SequenceMethod.OrderBy:
                            return AnalyzeResourceSetMethod(mce, (callExp, resource, selector) => ApplyOrdering(callExp, resource, selector,false, false));
                        case SequenceMethod.ThenBy:
                            return AnalyzeResourceSetMethod(mce, (callExp, resource, selector) => ApplyOrdering(callExp, resource, selector, false,true));
                        case SequenceMethod.OrderByDescending:
                            return AnalyzeResourceSetMethod(mce, (callExp, resource, selector) => ApplyOrdering(callExp, resource, selector, true, false));
                        case SequenceMethod.ThenByDescending:
                             return AnalyzeResourceSetMethod(mce, (callExp, resource, selector) => ApplyOrdering(callExp, resource, selector,true, true));
#if !ASTORIA_LIGHT      
                        case SequenceMethod.First:
                        case SequenceMethod.FirstOrDefault:
                            return LimitCardinality(mce, 1);
                        case SequenceMethod.Single:
                        case SequenceMethod.SingleOrDefault:
                            return LimitCardinality(mce, 2);
#endif
                        case SequenceMethod.Cast:
                            return AnalyzeCast(mce);
                        case SequenceMethod.LongCount:
                        case SequenceMethod.Count:
                            return AnalyzeCountMethod(mce);
                        default:
                            throw Error.MethodNotSupported(mce);
                    }
                }
                else if (mce.Method.DeclaringType.IsGenericType &&
                    mce.Method.DeclaringType.GetGenericTypeDefinition() == typeof(DataServiceQuery<>))
                {
                    Type t = typeof(DataServiceQuery<>).MakeGenericType(mce.Method.DeclaringType.GetGenericArguments()[0]);

                    if (mce.Method == t.GetMethod("Expand", new Type[] { typeof(string) }))
                    {
                        return AnalyzeExpand(mce);
                    }
                    else if (mce.Method == t.GetMethod("AddQueryOption", new Type[] { typeof(string), typeof(object) }))
                    {
                        return AnalyzeAddCustomQueryOption(mce);
                    }
                    else if (mce.Method == t.GetMethod("IncludeTotalCount"))
                    {
                        return AnalyzeAddCountOption(mce, CountOption.InlineAll);
                    }
                    else
                    {
                        throw Error.MethodNotSupported(mce);
                    }
                }
                return mce;
            }

            return e;
        }

 
        private static Expression StripCastMethodCalls(Expression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            MethodCallExpression call = StripTo<MethodCallExpression>(expression);
            while (call != null && ReflectionUtil.IsSequenceMethod(call.Method, SequenceMethod.Cast))
            {
                expression = call.Arguments[0];
                call = StripTo<MethodCallExpression>(expression);
            }

            return expression;
        }



        internal static class PatternRules
        {
            internal static bool MatchConvertToAssignable(UnaryExpression expression)
            {
                Debug.Assert(expression != null, "expression != null");

                if (expression.NodeType != ExpressionType.Convert &&
                    expression.NodeType != ExpressionType.ConvertChecked &&
                    expression.NodeType != ExpressionType.TypeAs)
                {
                    return false;
                }

                return expression.Type.IsAssignableFrom(expression.Operand.Type);
            }

            internal static bool MatchParameterMemberAccess(Expression expression)
            {
                Debug.Assert(expression != null, "lambda != null");

                LambdaExpression lambda = StripTo<LambdaExpression>(expression);
                if (lambda == null || lambda.Parameters.Count != 1)
                {
                    return false;
                }

                ParameterExpression parameter = lambda.Parameters[0];
                Expression body = StripCastMethodCalls(lambda.Body);
                MemberExpression memberAccess = StripTo<MemberExpression>(body);
                while (memberAccess != null)
                {
                    if (memberAccess.Expression == parameter)
                    {
                        return true;
                    }

                    memberAccess = StripTo<MemberExpression>(memberAccess.Expression);
                }

                return false;
            }

            internal static bool MatchPropertyAccess(Expression e, out MemberExpression member, out Expression instance, out List<string> propertyPath)
            {
                instance = null;
                propertyPath = null;

                MemberExpression me = StripTo<MemberExpression>(e);
                member = me;
                while (me != null)
                {
                    PropertyInfo pi;
                    if (MatchNonPrivateReadableProperty(me, out pi))
                    {
                        if (propertyPath == null)
                        {
                            propertyPath = new List<string>();
                        }

                        propertyPath.Insert(0, pi.Name);
                        e = me.Expression;
                        me = StripTo<MemberExpression>(e);
                    }
                    else
                    {
                        me = null;
                    }
                }

                if (propertyPath != null)
                {
                    instance = e;
                    return true;
                }

                return false;
            }

            internal static bool MatchConstant(Expression e, out ConstantExpression constExpr)
            {
                constExpr = e as ConstantExpression;
                return constExpr != null;
            }

            internal static bool MatchAnd(Expression e)
            {
                BinaryExpression be = e as BinaryExpression;
                return (be != null && (be.NodeType == ExpressionType.And || be.NodeType == ExpressionType.AndAlso));
            }

            internal static bool MatchNonPrivateReadableProperty(Expression e, out PropertyInfo propInfo)
            {
                MemberExpression me = e as MemberExpression;
                if (me == null)
                {
                    propInfo = null;
                    return false;
                }

                return MatchNonPrivateReadableProperty(me, out propInfo);
            }

           internal static bool MatchNonPrivateReadableProperty(MemberExpression me, out PropertyInfo propInfo)
            {
                Debug.Assert(me != null, "me != null");

                propInfo = null;

                if (me.Member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo pi = (PropertyInfo)me.Member;
                    if (pi.CanRead && !TypeSystem.IsPrivate(pi))
                    {
                        propInfo = pi;
                        return true;
                    }
                }

                return false;
            }

           internal static bool MatchKeyProperty(Expression expression, out PropertyInfo property)
            {
                property = null;

                 PropertyInfo pi;
                if (!PatternRules.MatchNonPrivateReadableProperty(expression, out pi))
                {
                    return false;
                }

                if (GetKeyProperties(pi.ReflectedType).Contains(pi, PropertyInfoEqualityComparer.Instance))
                {
                    property = pi;
                    return true;
                }

                return false;
            }

            internal static List<PropertyInfo> GetKeyProperties(Type type)
            {
                Debug.Assert(type != null, "type != null");
                ClientType clientType = ClientType.Create(type, false );
                var result = new List<PropertyInfo>();
                foreach (var property in clientType.Properties)
                {
                    if (property.KeyProperty)
                    {
                        result.Add(property.DeclaringType.GetProperty(property.PropertyName));
                    }
                }

                return result;
            }

            internal static bool MatchKeyComparison(Expression e, out PropertyInfo keyProperty, out ConstantExpression keyValue)
            {
                if (PatternRules.MatchBinaryEquality(e))
                {
                    BinaryExpression be = (BinaryExpression)e;
                    if ((PatternRules.MatchKeyProperty(be.Left, out keyProperty) && PatternRules.MatchConstant(be.Right, out keyValue)) ||
                        (PatternRules.MatchKeyProperty(be.Right, out keyProperty) && PatternRules.MatchConstant(be.Left, out keyValue)))
                    {
                        return keyValue.Value != null;
                    }
                }

                keyProperty = null;
                keyValue = null;
                return false;
            }

           internal static bool MatchReferenceEquals(Expression expression)
            {
                Debug.Assert(expression != null, "expression != null");
                MethodCallExpression call = expression as MethodCallExpression;
                if (call == null)
                {
                    return false;
                }

                return call.Method == typeof(object).GetMethod("ReferenceEquals");
            }

            internal static bool MatchResource(Expression expression, out ResourceExpression resource)
            {
                resource = expression as ResourceExpression;
                return resource != null;
            }

               internal static bool MatchDoubleArgumentLambda(Expression expression, out LambdaExpression lambda)
            {
                return MatchNaryLambda(expression, 2, out lambda);
            }

            internal static bool MatchIdentitySelector(LambdaExpression lambda)
            {
                Debug.Assert(lambda != null, "lambda != null");

                ParameterExpression parameter = lambda.Parameters[0];
                return parameter == StripTo<ParameterExpression>(lambda.Body);
            }

            internal static bool MatchSingleArgumentLambda(Expression expression, out LambdaExpression lambda)
            {
                return MatchNaryLambda(expression, 1, out lambda);
            }

            internal static bool MatchTransparentIdentitySelector(Expression input, LambdaExpression selector)
            {
                if (selector.Parameters.Count != 1)
                {
                    return false;
                }

                ResourceSetExpression rse = input as ResourceSetExpression;
                if (rse == null || rse.TransparentScope == null)
                {
                    return false;
                }

                Expression potentialRef = selector.Body;
                ParameterExpression expectedTarget = selector.Parameters[0];

                MemberExpression propertyMember;
                Expression paramRef;
                List<string> refPath;
                if (!MatchPropertyAccess(potentialRef, out propertyMember, out paramRef, out refPath))
                {
                    return false;
                }

                Debug.Assert(refPath != null, "refPath != null -- otherwise MatchPropertyAccess should not have returned true");
                return paramRef == expectedTarget && refPath.Count == 1 && refPath[0] == rse.TransparentScope.Accessor;
            }

            internal static bool MatchIdentityProjectionResultSelector(Expression e)
            {
                LambdaExpression le = (LambdaExpression)e;
                return (le.Body == le.Parameters[1]);
            }

            internal static bool MatchTransparentScopeSelector(ResourceSetExpression input, LambdaExpression resultSelector, out ResourceSetExpression.TransparentAccessors transparentScope)
            {
                transparentScope = null;

                if (resultSelector.Body.NodeType != ExpressionType.New)
                {
                    return false;
                }
                
                NewExpression ne = (NewExpression)resultSelector.Body;
                if (ne.Arguments.Count < 2)
                {
                    return false;
                }

                if (ne.Type.BaseType != typeof(object))
                {
                    return false;
                }

                ParameterInfo[] constructorParams = ne.Constructor.GetParameters();
                if (ne.Members.Count != constructorParams.Length)
                {
                    return false;
                }

                ResourceSetExpression inputSourceSet = input.Source as ResourceSetExpression;
                int introducedMemberIndex = -1;
                ParameterExpression collectorSourceParameter = resultSelector.Parameters[0];
                ParameterExpression introducedRangeParameter = resultSelector.Parameters[1];
                MemberInfo[] memberProperties = new MemberInfo[ne.Members.Count];
                PropertyInfo[] properties = ne.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Dictionary<string, Expression> sourceAccessors = new Dictionary<string, Expression>(constructorParams.Length - 1, StringComparer.Ordinal);
                for (int i = 0; i < ne.Arguments.Count; i++)
                {
                    Expression argument = ne.Arguments[i];
                    MemberInfo member = ne.Members[i];

                    if (!ExpressionIsSimpleAccess(argument, resultSelector.Parameters))
                    {
                        return false;
                    }

                    if (member.MemberType == MemberTypes.Method)
                    {
                        member = properties.Where(property => property.GetGetMethod() == member).FirstOrDefault();
                        if (member == null)
                        {
                            return false;
                        }
                    }

                    if (member.Name != constructorParams[i].Name)
                    {
                        return false;
                    }

                    memberProperties[i] = member;

                    ParameterExpression argumentAsParameter = StripTo<ParameterExpression>(argument);
                    if (introducedRangeParameter == argumentAsParameter)
                    {
                        if (introducedMemberIndex != -1)
                        {
                            return false;
                        }

                        introducedMemberIndex = i;
                    }
                    else if (collectorSourceParameter == argumentAsParameter)
                    {
                        sourceAccessors[member.Name] = inputSourceSet.CreateReference();
                    }
                    else
                    {
                        List<ResourceExpression> referencedInputs = new List<ResourceExpression>();
                        Expression boundArgument = InputBinder.Bind(argument, inputSourceSet, resultSelector.Parameters[0], referencedInputs);
                        if (referencedInputs.Count != 1)
                        {
                            return false;
                        }

                        sourceAccessors[member.Name] = referencedInputs[0].CreateReference();
                    }
                }

                if (introducedMemberIndex == -1)
                {
                    return false;
                }

                string resultAccessor = memberProperties[introducedMemberIndex].Name;
                transparentScope = new ResourceSetExpression.TransparentAccessors(resultAccessor, sourceAccessors);

                return true;
            }

            internal static bool MatchPropertyProjectionSet(ResourceExpression input, Expression potentialPropertyRef, out MemberExpression navigationMember)
            {
                return MatchNavigationPropertyProjection(input, potentialPropertyRef, true, out navigationMember);
            }

            internal static bool MatchPropertyProjectionSingleton(ResourceExpression input, Expression potentialPropertyRef, out MemberExpression navigationMember)
            {
                return MatchNavigationPropertyProjection(input, potentialPropertyRef, false, out navigationMember);
            }

            private static bool MatchNavigationPropertyProjection(ResourceExpression input, Expression potentialPropertyRef, bool requireSet, out MemberExpression navigationMember)
            {
                if (PatternRules.MatchNonSingletonProperty(potentialPropertyRef) == requireSet)
                {
                    Expression foundInstance;
                    List<string> propertyNames;
                    if (MatchPropertyAccess(potentialPropertyRef, out navigationMember, out foundInstance, out propertyNames))
                    {
                        if (foundInstance == input.CreateReference())
                        {
                            return true;
                        }
                    }
                }

                navigationMember = null;
                return false;
            }

            internal static bool MatchMemberInitExpressionWithDefaultConstructor(Expression source, LambdaExpression e)
            {
                MemberInitExpression mie = StripTo<MemberInitExpression>(e.Body);
                ResourceExpression resource;
                return MatchResource(source, out resource) && (mie != null) && (mie.NewExpression.Arguments.Count == 0);
            }

            internal static bool MatchNewExpression(Expression source, LambdaExpression e)
            {
                ResourceExpression resource;
                return MatchResource(source, out resource) && (e.Body is NewExpression);
            }

            internal static bool MatchNot(Expression expression)
            {
                Debug.Assert(expression != null, "expression != null");
                return expression.NodeType == ExpressionType.Not;
            }

            internal static bool MatchNonSingletonProperty(Expression e)
            {
                return (TypeSystem.FindIEnumerable(e.Type) != null) && 
                    e.Type != typeof(char[]) &&
                    e.Type != typeof(byte[]);
            }

            internal static MatchNullCheckResult MatchNullCheck(Expression entityInScope, ConditionalExpression conditional)
            {
                Debug.Assert(conditional != null, "conditional != null");

                MatchNullCheckResult result = new MatchNullCheckResult();
                MatchEqualityCheckResult equalityCheck = MatchEquality(conditional.Test);
                if (!equalityCheck.Match)
                {
                    return result;
                }

                Expression assignedCandidate;
                if (equalityCheck.EqualityYieldsTrue)
                {
                    if (!MatchNullConstant(conditional.IfTrue))
                    {
                        return result;
                    }

                    assignedCandidate = conditional.IfFalse;
                }
                else
                {
                    if (!MatchNullConstant(conditional.IfFalse))
                    {
                        return result;
                    }

                    assignedCandidate = conditional.IfTrue;
                }

                Expression memberCandidate;
                if (MatchNullConstant(equalityCheck.TestLeft))
                {
                    memberCandidate = equalityCheck.TestRight;
                }
                else if (MatchNullConstant(equalityCheck.TestRight))
                {
                    memberCandidate = equalityCheck.TestLeft;
                }
                else
                {
                    return result;
                }

                Debug.Assert(assignedCandidate != null, "assignedCandidate != null");
                Debug.Assert(memberCandidate != null, "memberCandidate != null");

                MemberAssignmentAnalysis assignedAnalysis = MemberAssignmentAnalysis.Analyze(entityInScope, assignedCandidate);
                if (assignedAnalysis.MultiplePathsFound)
                {
                    return result;
                }

                MemberAssignmentAnalysis memberAnalysis = MemberAssignmentAnalysis.Analyze(entityInScope, memberCandidate);
                if (memberAnalysis.MultiplePathsFound)
                {
                    return result;
                }

                Expression[] assignedExpressions = assignedAnalysis.GetExpressionsToTargetEntity();
                Expression[] memberExpressions = memberAnalysis.GetExpressionsToTargetEntity();
                if (memberExpressions.Length > assignedExpressions.Length)
                {
                    return result;
                }

                for (int i = 0; i < memberExpressions.Length; i++)
                {
                    Expression assigned = assignedExpressions[i];
                    Expression member = memberExpressions[i];
                    if (assigned == member)
                    {
                        continue;
                    }

                    if (assigned.NodeType != member.NodeType || assigned.NodeType != ExpressionType.MemberAccess)
                    {
                        return result;
                    }

                    if (((MemberExpression)assigned).Member != ((MemberExpression)member).Member)
                    {
                        return result;
                    }
                }

                result.AssignExpression = assignedCandidate;
                result.Match = true;
                result.TestToNullExpression = memberCandidate;
                return result;
            }

            internal static bool MatchNullConstant(Expression expression)
            {
                Debug.Assert(expression != null, "expression != null");
                ConstantExpression constant = expression as ConstantExpression;
                if (constant != null && constant.Value == null)
                {
                    return true;
                }

                return false;
            }

            internal static bool MatchBinaryExpression(Expression e)
            {
                return (e is BinaryExpression);
            }

            internal static bool MatchBinaryEquality(Expression e)
            {
                return (PatternRules.MatchBinaryExpression(e) && ((BinaryExpression)e).NodeType == ExpressionType.Equal);
            }

            internal static bool MatchStringAddition(Expression e)
            {
                if (e.NodeType == ExpressionType.Add)
                {
                    BinaryExpression be = e as BinaryExpression;
                    return be != null &&
                        be.Left.Type == typeof(string) &&
                        be.Right.Type == typeof(string);
                }
                return false;
            }

            internal static bool MatchNewDataServiceCollectionOfT(NewExpression nex)
            {
                return nex.Type.IsGenericType && WebUtil.IsDataServiceCollectionType(nex.Type.GetGenericTypeDefinition());
            }

            internal static MatchEqualityCheckResult MatchEquality(Expression expression)
            {
                Debug.Assert(expression != null, "expression != null");

                MatchEqualityCheckResult result = new MatchEqualityCheckResult();
                result.Match = false;
                result.EqualityYieldsTrue = true;

                while (true)
                {
                    if (MatchReferenceEquals(expression))
                    {
                        MethodCallExpression call = (MethodCallExpression)expression;
                        result.Match = true;
                        result.TestLeft = call.Arguments[0];
                        result.TestRight = call.Arguments[1];
                        break;
                    }
                    else if (MatchNot(expression))
                    {
                        result.EqualityYieldsTrue = !result.EqualityYieldsTrue;
                        expression = ((UnaryExpression)expression).Operand;
                    }
                    else
                    {
                        BinaryExpression test = expression as BinaryExpression;
                        if (test == null)
                        {
                            break;
                        }

                        if (test.NodeType == ExpressionType.NotEqual)
                        {
                            result.EqualityYieldsTrue = !result.EqualityYieldsTrue;
                        }
                        else if (test.NodeType != ExpressionType.Equal)
                        {
                            break;
                        }

                        result.TestLeft = test.Left;
                        result.TestRight = test.Right;
                        result.Match = true;
                        break;
                    }
                }

                return result;
            }

            private static bool ExpressionIsSimpleAccess(Expression argument, ReadOnlyCollection<ParameterExpression> expressions)
            {
                Debug.Assert(argument != null, "argument != null");
                Debug.Assert(expressions != null, "expressions != null");

                Expression source = argument;
                MemberExpression member;
                do
                {
                    member = source as MemberExpression;
                    if (member != null)
                    {
                        source = member.Expression;
                    }
                }
                while (member != null);

                ParameterExpression parameter = source as ParameterExpression;
                if (parameter == null)
                {
                    return false;
                }

                return expressions.Contains(parameter);
            }

            private static bool MatchNaryLambda(Expression expression, int parameterCount, out LambdaExpression lambda)
            {
                lambda = null;

                LambdaExpression le = StripTo<LambdaExpression>(expression);
                if (le != null && le.Parameters.Count == parameterCount)
                {
                    lambda = le;
                }

                return lambda != null;
            }

            internal struct MatchNullCheckResult
            {
                internal Expression AssignExpression;

                internal bool Match;

                internal Expression TestToNullExpression;
            }

            internal struct MatchEqualityCheckResult
            {
                internal bool EqualityYieldsTrue;

                internal bool Match;

                internal Expression TestLeft;

                internal Expression TestRight;
            }
        }

        private static class ValidationRules
        {
            internal static void RequireCanNavigate(Expression e)
            {
                ResourceSetExpression resourceSet = e as ResourceSetExpression;
                if (resourceSet != null && resourceSet.HasSequenceQueryOptions)
                {
                    throw new NotSupportedException(Strings.ALinq_QueryOptionsOnlyAllowedOnLeafNodes);
                }

                ResourceExpression resource;
                if (PatternRules.MatchResource(e, out resource) && resource.Projection != null)
                {
                    throw new NotSupportedException(Strings.ALinq_ProjectionOnlyAllowedOnLeafNodes);
                }
            }

            internal static void RequireCanProject(Expression e)
            {
                ResourceExpression re = (ResourceExpression)e;
                if (!PatternRules.MatchResource(e, out re))
                {
                    throw new NotSupportedException(Strings.ALinq_CanOnlyProjectTheLeaf);
                }

                if (re.Projection != null)
                {
                    throw new NotSupportedException(Strings.ALinq_ProjectionCanOnlyHaveOneProjection);
                }

                if (re.ExpandPaths.Count > 0)
                {
                    throw new NotSupportedException(Strings.ALinq_CannotProjectWithExplicitExpansion);
                }
            }

            internal static void RequireCanExpand(Expression e)
            {
                ResourceExpression re = (ResourceExpression)e;
                if (!PatternRules.MatchResource(e, out re))
                {
                    throw new NotSupportedException(Strings.ALinq_CantExpand);
                }

                if (re.Projection != null)
                {
                    throw new NotSupportedException(Strings.ALinq_CannotProjectWithExplicitExpansion);
                }
            }

            internal static void RequireCanAddCount(Expression e)
            {
                ResourceExpression re = (ResourceExpression)e;
                if (!PatternRules.MatchResource(e, out re))
                {
                    throw new NotSupportedException(Strings.ALinq_CannotAddCountOption);
                }

                if (re.CountOption != CountOption.None)
                {
                    throw new NotSupportedException(Strings.ALinq_CannotAddCountOptionConflict);
                }
            }

            internal static void RequireCanAddCustomQueryOption(Expression e)
            {
                ResourceExpression re = (ResourceExpression)e;
                if (!PatternRules.MatchResource(e, out re))
                {
                    throw new NotSupportedException(Strings.ALinq_CantAddQueryOption);
                }
            }

            internal static void RequireNonSingleton(Expression e)
            {
                ResourceExpression re = e as ResourceExpression;
                if (re != null && re.IsSingleton)
                {
                    throw new NotSupportedException(Strings.ALinq_QueryOptionsOnlyAllowedOnSingletons);
                }
            }

            internal static void RequireLegalCustomQueryOption(Expression e, ResourceExpression target)
            {
                string name = ((string)(e as ConstantExpression).Value).Trim();

                if (name[0] == UriHelper.DOLLARSIGN)
                {
                    if (target.CustomQueryOptions.Any(c => (string)c.Key.Value == name))
                    {
                        throw new NotSupportedException(Strings.ALinq_CantAddDuplicateQueryOption(name));
                    }

                    ResourceSetExpression rse = target as ResourceSetExpression;
                    if (rse != null)
                    {
                        switch (name.Substring(1))
                        {
                            case UriHelper.OPTIONFILTER:
                                if (rse.Filter != null)
                                {
                                    throw new NotSupportedException(Strings.ALinq_CantAddAstoriaQueryOption(name));
                                }
                                break;
                            case UriHelper.OPTIONORDERBY:
                                if (rse.OrderBy != null)
                                    throw new NotSupportedException(Strings.ALinq_CantAddAstoriaQueryOption(name));
                                break;
                            case UriHelper.OPTIONEXPAND:
                                break;
                            case UriHelper.OPTIONSKIP:
                                if (rse.Skip != null)
                                    throw new NotSupportedException(Strings.ALinq_CantAddAstoriaQueryOption(name));
                                break;
                            case UriHelper.OPTIONTOP:
                                if (rse.Take != null)
                                    throw new NotSupportedException(Strings.ALinq_CantAddAstoriaQueryOption(name));
                                break;
                            case UriHelper.OPTIONCOUNT:
                                if (rse.CountOption != CountOption.None)
                                    throw new NotSupportedException(Strings.ALinq_CantAddAstoriaQueryOption(name));
                                break;
                            default:
                                throw new NotSupportedException(Strings.ALinq_CantAddQueryOptionStartingWithDollarSign(name));
                        }
                    }
                }
            }
        }


        private sealed class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            private PropertyInfoEqualityComparer() { }

            internal static readonly PropertyInfoEqualityComparer Instance = new PropertyInfoEqualityComparer();

            #region IEqualityComparer<TypeUsage> Members

            public bool Equals(PropertyInfo left, PropertyInfo right)
            {
                if (object.ReferenceEquals(left, right)) { return true; }

                if (null == left || null == right) { return false; }

                return object.ReferenceEquals(left.DeclaringType, right.DeclaringType) && left.Name.Equals(right.Name);
            }

            public int GetHashCode(PropertyInfo obj)
            {
                Debug.Assert(obj != null, "obj != null");
                return obj.GetHashCode();
            }

            #endregion
        }

        private sealed class ExpressionPresenceVisitor : DataServiceExpressionVisitor
        {
            #region Private fields.

            private readonly Expression target;

            private bool found;

            #endregion Private fields.

            private ExpressionPresenceVisitor(Expression target)
            {
                Debug.Assert(target != null, "target != null");
                this.target = target;
            }

            internal static bool IsExpressionPresent(Expression target, Expression tree)
            {
                Debug.Assert(target != null, "target != null");
                Debug.Assert(tree != null, "tree != null");

                ExpressionPresenceVisitor visitor = new ExpressionPresenceVisitor(target);
                visitor.Visit(tree);
                return visitor.found;
            }

            internal override Expression Visit(Expression exp)
            {
                Expression result;
                
                if (this.found || object.ReferenceEquals(this.target, exp))
                {
                    this.found = true;
                    result = exp;
                }
                else
                {
                    result = base.Visit(exp);
                }

                return result;
            }
        }
    }
}
