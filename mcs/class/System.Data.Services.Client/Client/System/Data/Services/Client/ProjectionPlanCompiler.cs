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
    using System.Reflection;

    #endregion Namespaces.

    internal class ProjectionPlanCompiler : ExpressionVisitor
    {
        #region Private fields.

        private readonly Dictionary<Expression, ExpressionAnnotation> annotations;

        private readonly ParameterExpression materializerExpression;

        private readonly Dictionary<Expression, Expression> normalizerRewrites;

        private int identifierId;

        private ProjectionPathBuilder pathBuilder;

        private bool topLevelProjectionFound;

        #endregion Private fields.

        #region Constructors.

        private ProjectionPlanCompiler(Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.annotations = new Dictionary<Expression, ExpressionAnnotation>(ReferenceEqualityComparer<Expression>.Instance);
            this.materializerExpression = Expression.Parameter(typeof(object), "mat");
            this.normalizerRewrites = normalizerRewrites;
            this.pathBuilder = new ProjectionPathBuilder();
        }

        #endregion Constructors.

        #region Internal methods.

        internal static ProjectionPlan CompilePlan(LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites)
        {
            Debug.Assert(projection != null, "projection != null");
            Debug.Assert(projection.Parameters.Count == 1, "projection.Parameters.Count == 1");
            Debug.Assert(
                projection.Body.NodeType == ExpressionType.Constant ||
                projection.Body.NodeType == ExpressionType.MemberInit ||
                projection.Body.NodeType == ExpressionType.MemberAccess ||
                projection.Body.NodeType == ExpressionType.Convert ||
                projection.Body.NodeType == ExpressionType.ConvertChecked ||
                projection.Body.NodeType == ExpressionType.New,
                "projection.Body.NodeType == Constant, MemberInit, MemberAccess, Convert(Checked) New");

            ProjectionPlanCompiler rewriter = new ProjectionPlanCompiler(normalizerRewrites);
#if TRACE_CLIENT_PROJECTIONS
            Trace.WriteLine("Projection: " + projection);
#endif

            Expression plan = rewriter.Visit(projection);
#if TRACE_CLIENT_PROJECTIONS
            Trace.WriteLine("Becomes: " + plan);
#endif

            ProjectionPlan result = new ProjectionPlan();
            result.Plan = (Func<object, object, Type, object>)((LambdaExpression)plan).Compile();
            result.ProjectedType = projection.Body.Type;
#if DEBUG
            result.SourceProjection = projection;
            result.TargetProjection = plan;
#endif
            return result;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            Expression original = this.GetExpressionBeforeNormalization(b);
            if (original == b)
            {
                return base.VisitBinary(b);
            }
            else
            {
                return this.Visit(original);
            }
        }

        internal override Expression VisitConditional(ConditionalExpression conditional)
        {
            Debug.Assert(conditional != null, "conditional != null");
            Expression original = this.GetExpressionBeforeNormalization(conditional);
            if (original != conditional)
            {
                return this.Visit(original);
            }

            var nullCheck = ResourceBinder.PatternRules.MatchNullCheck(this.pathBuilder.LambdaParameterInScope, conditional);
            if (!nullCheck.Match || !ClientType.CheckElementTypeIsEntity(nullCheck.AssignExpression.Type))
            {
                return base.VisitConditional(conditional);
            }

            return this.RebindConditionalNullCheck(conditional, nullCheck);
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            Expression original = this.GetExpressionBeforeNormalization(u);
            Expression result;
            if (original == u)
            {
                result = base.VisitUnary(u);
                UnaryExpression unaryResult = result as UnaryExpression;
                if (unaryResult != null)
                {
                    ExpressionAnnotation annotation;
                    if (this.annotations.TryGetValue(unaryResult.Operand, out annotation))
                    {
                        this.annotations[result] = annotation;
                    }
                }
            }
            else
            {
                result = this.Visit(original);
            }

            return result;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            Debug.Assert(m != null, "m != null");

            Expression result;
            Expression baseSourceExpression = m.Expression;

            if (ClientConvert.IsKnownNullableType(baseSourceExpression.Type))
            {
                result = base.VisitMemberAccess(m);
            }
            else
            {
                Expression baseTargetExpression = this.Visit(baseSourceExpression);
                ExpressionAnnotation annotation;
                if (this.annotations.TryGetValue(baseTargetExpression, out annotation))
                {
                    result = this.RebindMemberAccess(m, annotation);
                }
                else
                {
                    result = Expression.MakeMemberAccess(baseTargetExpression, m.Member);
                }
            }

            return result;
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            Debug.Assert(p != null, "p != null");

            Expression result;
            ExpressionAnnotation annotation;
            if (this.annotations.TryGetValue(p, out annotation))
            {
                result = this.RebindParameter(p, annotation);
            }
            else
            {
                result = base.VisitParameter(p);
            }

            return result;
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            this.pathBuilder.EnterMemberInit(init);
            
            Expression result = null;
            if (this.pathBuilder.CurrentIsEntity && init.Bindings.Count > 0)
            {
                result = this.RebindEntityMemberInit(init);
            }
            else
            {
                result = base.VisitMemberInit(init);
            }

            this.pathBuilder.LeaveMemberInit();
            return result;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            Debug.Assert(m != null, "m != null");

            Expression original = this.GetExpressionBeforeNormalization(m);
            if (original != m)
            {
                return this.Visit(original);
            }

            Expression result;
            if (this.pathBuilder.CurrentIsEntity)
            {
                Debug.Assert(
                    ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m) || ResourceBinder.PatternRules.MatchReferenceEquals(m),
                    "ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m) || ResourceBinder.PatternRules.MatchReferenceEquals(m) -- otherwise ProjectionAnalyzer should have blocked this for entities");
                if (m.Method.Name == "Select")
                {
                    result = this.RebindMethodCallForMemberSelect(m);
                }
                else if (m.Method.Name == "ToList")
                {
                    result = this.RebindMethodCallForMemberToList(m);
                }
                else
                {
                    Debug.Assert(m.Method.Name == "ReferenceEquals", "We don't know how to handle this method, ProjectionAnalyzer updated?");
                    result = base.VisitMethodCall(m);
                }
            }
            else
            {
                if (ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m))
                {
                    result = this.RebindMethodCallForNewSequence(m);
                }
                else
                {
                    result = base.VisitMethodCall(m);
                }
            }

            return result;
        }

        internal override NewExpression VisitNew(NewExpression nex)
        {
            Debug.Assert(nex != null, "nex != null");

            if (ResourceBinder.PatternRules.MatchNewDataServiceCollectionOfT(nex))
            {
                return this.RebindNewExpressionForDataServiceCollectionOfT(nex);
            }

            return base.VisitNew(nex);
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            Debug.Assert(lambda != null, "lambda != null");

            Expression result;
            if (!this.topLevelProjectionFound || lambda.Parameters.Count == 1 && ClientType.CheckElementTypeIsEntity(lambda.Parameters[0].Type))
            {
                this.topLevelProjectionFound = true;

                ParameterExpression expectedTypeParameter = Expression.Parameter(typeof(Type), "type" + this.identifierId);
                ParameterExpression entryParameter = Expression.Parameter(typeof(object), "entry" + this.identifierId);
                this.identifierId++;

                this.pathBuilder.EnterLambdaScope(lambda, entryParameter, expectedTypeParameter);
                ProjectionPath parameterPath = new ProjectionPath(lambda.Parameters[0], expectedTypeParameter, entryParameter);
                ProjectionPathSegment parameterSegment = new ProjectionPathSegment(parameterPath, null, null);
                parameterPath.Add(parameterSegment);
                this.annotations[lambda.Parameters[0]] = new ExpressionAnnotation() { Segment = parameterSegment };

                Expression body = this.Visit(lambda.Body);

                if (body.Type.IsValueType)
                {
                    body = Expression.Convert(body, typeof(object));
                }

                result = Expression.Lambda<Func<object, object, Type, object>>(
                    body,
                    this.materializerExpression,
                    entryParameter,
                    expectedTypeParameter);

                this.pathBuilder.LeaveLambdaScope();
            }
            else
            {
                result = base.VisitLambda(lambda);
            }

            return result;
        }

        #endregion Internal methods.

        #region Private methods.

        private static Expression CallMaterializer(string methodName, params Expression[] arguments)
        {
            return CallMaterializerWithType(methodName, null, arguments);
        }

        private static Expression CallMaterializerWithType(string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            Debug.Assert(methodName != null, "methodName != null");
            Debug.Assert(arguments != null, "arguments != null");

            MethodInfo method = typeof(AtomMaterializerInvoker).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Debug.Assert(method != null, "method != null - found " + methodName);
            if (typeArguments != null)
            {
                method = method.MakeGenericMethod(typeArguments);
            }

            return Expression.Call(method, arguments);
        }

        private Expression CallCheckValueForPathIsNull(Expression entry, Expression entryType, ProjectionPath path)
        {
            Expression result = CallMaterializer("ProjectionCheckValueForPathIsNull", entry, entryType, Expression.Constant(path, typeof(object)));
            this.annotations.Add(result, new ExpressionAnnotation() { Segment = path[path.Count - 1] });
            return result;
        }

        private Expression CallValueForPath(Expression entry, Expression entryType, ProjectionPath path)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(path != null, "path != null");

            Expression result = CallMaterializer("ProjectionValueForPath", this.materializerExpression, entry, entryType, Expression.Constant(path, typeof(object)));
            this.annotations.Add(result, new ExpressionAnnotation() { Segment = path[path.Count - 1] });
            return result;
        }

        private Expression CallValueForPathWithType(Expression entry, Expression entryType, ProjectionPath path, Type type)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(path != null, "path != null");
            
            Expression value = this.CallValueForPath(entry, entryType, path);
            Expression result = Expression.Convert(value, type);
            this.annotations.Add(result, new ExpressionAnnotation() { Segment = path[path.Count - 1] });
            return result;
        }

        private Expression RebindConditionalNullCheck(ConditionalExpression conditional, ResourceBinder.PatternRules.MatchNullCheckResult nullCheck)
        {
            Debug.Assert(conditional != null, "conditional != null");
            Debug.Assert(nullCheck.Match, "nullCheck.Match -- otherwise no reason to call this rebind method");

            Expression testToNullForProjection = this.Visit(nullCheck.TestToNullExpression);
            Expression assignForProjection = this.Visit(nullCheck.AssignExpression);
            ExpressionAnnotation testToNullAnnotation;
            if (!this.annotations.TryGetValue(testToNullForProjection, out testToNullAnnotation))
            {
                return base.VisitConditional(conditional);
            }

            ProjectionPathSegment testToNullSegment = testToNullAnnotation.Segment;

            Expression testToNullThroughMethod = this.CallCheckValueForPathIsNull(
                testToNullSegment.StartPath.RootEntry,
                testToNullSegment.StartPath.ExpectedRootType,
                testToNullSegment.StartPath);

            Expression test = testToNullThroughMethod;
            Expression iftrue = Expression.Constant(null, assignForProjection.Type);
            Expression iffalse = assignForProjection;
            Expression result = Expression.Condition(test, iftrue, iffalse);
            return result;
        }

        private Expression RebindEntityMemberInit(MemberInitExpression init)
        {
            Debug.Assert(init != null, "init != null");
            Debug.Assert(init.Bindings.Count > 0, "init.Bindings.Count > 0 -- otherwise this is just empty construction");

            Expression[] expressions;
            if (!this.pathBuilder.HasRewrites)
            {
                MemberAssignmentAnalysis propertyAnalysis = MemberAssignmentAnalysis.Analyze(
                    this.pathBuilder.LambdaParameterInScope,
                    ((MemberAssignment)init.Bindings[0]).Expression);
                expressions = propertyAnalysis.GetExpressionsToTargetEntity();
                Debug.Assert(expressions.Length != 0, "expressions.Length != 0 -- otherwise there is no correlation to parameter in entity member init");
            }
            else
            {
                expressions = MemberAssignmentAnalysis.EmptyExpressionArray;
            }

            Expression entryParameterAtMemberInit = this.pathBuilder.ParameterEntryInScope;
            List<string> propertyNames = new List<string>();
            List<Func<object, object, Type, object>> propertyFunctions = new List<Func<object, object, Type, object>>();
            Type projectedType = init.NewExpression.Type;
            Expression projectedTypeExpression = Expression.Constant(projectedType, typeof(Type));

            Expression entryToInitValue;            Expression expectedParamValue;            ParameterExpression entryParameterForMembers;            ParameterExpression expectedParameterForMembers;            string[] expressionNames = expressions.Skip(1).Select(e => ((MemberExpression)e).Member.Name).ToArray();
            if (expressions.Length <= 1)
            {
                entryToInitValue = this.pathBuilder.ParameterEntryInScope;
                expectedParamValue = this.pathBuilder.ExpectedParamTypeInScope;
                entryParameterForMembers = (ParameterExpression)this.pathBuilder.ParameterEntryInScope;
                expectedParameterForMembers = (ParameterExpression)this.pathBuilder.ExpectedParamTypeInScope;
            }
            else
            {
                entryToInitValue = this.GetDeepestEntry(expressions);
                expectedParamValue = projectedTypeExpression;
                entryParameterForMembers = Expression.Parameter(typeof(object), "subentry" + this.identifierId++);
                expectedParameterForMembers = (ParameterExpression)this.pathBuilder.ExpectedParamTypeInScope;

                ProjectionPath entryPath = new ProjectionPath(
                    (ParameterExpression)this.pathBuilder.LambdaParameterInScope, 
                    this.pathBuilder.ExpectedParamTypeInScope, 
                    this.pathBuilder.ParameterEntryInScope,
                    expressions.Skip(1));

                this.annotations.Add(entryToInitValue, new ExpressionAnnotation() { Segment = entryPath[entryPath.Count - 1] });
                this.annotations.Add(entryParameterForMembers, new ExpressionAnnotation() { Segment = entryPath[entryPath.Count - 1] });
                this.pathBuilder.RegisterRewrite(this.pathBuilder.LambdaParameterInScope, expressionNames, entryParameterForMembers);
            }

            for (int i = 0; i < init.Bindings.Count; i++)
            {
                MemberAssignment assignment = (MemberAssignment)init.Bindings[i];
                propertyNames.Add(assignment.Member.Name);

                LambdaExpression propertyLambda;

                if ((ClientType.CheckElementTypeIsEntity(assignment.Member.ReflectedType) &&
                     assignment.Expression.NodeType == ExpressionType.MemberInit))
                {
                    Expression nestedEntry = CallMaterializer(
                        "ProjectionGetEntry",
                        entryParameterAtMemberInit,
                        Expression.Constant(assignment.Member.Name, typeof(string)));
                    ParameterExpression nestedEntryParameter = Expression.Parameter(
                        typeof(object),
                        "subentry" + this.identifierId++);

                    ProjectionPath entryPath;
                    ExpressionAnnotation entryAnnotation;
                    if (this.annotations.TryGetValue(this.pathBuilder.ParameterEntryInScope, out entryAnnotation))
                    {
                        entryPath = new ProjectionPath(
                            (ParameterExpression)this.pathBuilder.LambdaParameterInScope,
                            this.pathBuilder.ExpectedParamTypeInScope,
                            entryParameterAtMemberInit);
                        entryPath.AddRange(entryAnnotation.Segment.StartPath);
                    }
                    else
                    {
                        entryPath = new ProjectionPath(
                            (ParameterExpression)this.pathBuilder.LambdaParameterInScope,
                            this.pathBuilder.ExpectedParamTypeInScope,
                            entryParameterAtMemberInit,
                            expressions.Skip(1));
                    }

                    ProjectionPathSegment nestedSegment = new ProjectionPathSegment(
                        entryPath,
                        assignment.Member.Name,
                        assignment.Member.ReflectedType);

                    entryPath.Add(nestedSegment);

                    string[] names = (entryPath.Where(m => m.Member != null).Select(m => m.Member)).ToArray();

                    this.annotations.Add(nestedEntryParameter, new ExpressionAnnotation() { Segment = nestedSegment });
                    this.pathBuilder.RegisterRewrite(this.pathBuilder.LambdaParameterInScope, names, nestedEntryParameter);
                    Expression e = this.Visit(assignment.Expression);
                    this.pathBuilder.RevokeRewrite(this.pathBuilder.LambdaParameterInScope, names);
                    this.annotations.Remove(nestedEntryParameter);

                    e = Expression.Convert(e, typeof(object));
                    ParameterExpression[] parameters =
                        new ParameterExpression[] 
                        {
                            this.materializerExpression,
                            nestedEntryParameter,
                            expectedParameterForMembers,
                        };
                    propertyLambda = Expression.Lambda(e, parameters);

                    Expression[] nestedParams =
                        new Expression[]
                        {
                            this.materializerExpression, 
                            nestedEntry,
                            expectedParameterForMembers,
                        };
                    var invokeParameters =
                        new ParameterExpression[] 
                        {
                            this.materializerExpression,
                            (ParameterExpression)entryParameterAtMemberInit,
                            expectedParameterForMembers,
                        };
                    propertyLambda = Expression.Lambda(Expression.Invoke(propertyLambda, nestedParams), invokeParameters);
                }
                else
                {
                    Expression e = this.Visit(assignment.Expression);
                    e = Expression.Convert(e, typeof(object));
                    ParameterExpression[] parameters =
                        new ParameterExpression[] 
                        {
                            this.materializerExpression,
                            entryParameterForMembers,
                            expectedParameterForMembers,
                        };
                    propertyLambda = Expression.Lambda(e, parameters);
                }

#if TRACE_CLIENT_PROJECTIONS
                Trace.WriteLine("Compiling lambda for " + assignment.Member.Name + ": " + propertyLambda);
#endif
                propertyFunctions.Add((Func<object, object, Type, object>) propertyLambda.Compile());
            }

            for (int i = 1; i < expressions.Length; i++)
            {
                this.pathBuilder.RevokeRewrite(this.pathBuilder.LambdaParameterInScope, expressionNames);
                this.annotations.Remove(entryToInitValue);
                this.annotations.Remove(entryParameterForMembers);
            }

            Expression reboundExpression = CallMaterializer(
                "ProjectionInitializeEntity",
                this.materializerExpression,
                entryToInitValue,
                expectedParamValue,
                projectedTypeExpression,
                Expression.Constant(propertyNames.ToArray()),
                Expression.Constant(propertyFunctions.ToArray()));

            return Expression.Convert(reboundExpression, projectedType);
        }

        private Expression GetDeepestEntry(Expression[] path)
        {
            Debug.Assert(path.Length > 1, "path.Length > 1");
            
            Expression result = null;
            int pathIndex = 1;
            do
            {
                result = CallMaterializer(
                    "ProjectionGetEntry",
                    result ?? this.pathBuilder.ParameterEntryInScope,
                    Expression.Constant(((MemberExpression)path[pathIndex]).Member.Name, typeof(string)));
                pathIndex++;
            }
            while (pathIndex < path.Length);

            return result;
        }

        private Expression GetExpressionBeforeNormalization(Expression expression)
        {
            Debug.Assert(expression != null, "expression != null");
            if (this.normalizerRewrites != null)
            {
                Expression original;
                if (this.normalizerRewrites.TryGetValue(expression, out original))
                {
                    expression = original;
                }
            }

            return expression;
        }

        private Expression RebindParameter(Expression expression, ExpressionAnnotation annotation)
        {
            Debug.Assert(expression != null, "expression != null");
            Debug.Assert(annotation != null, "annotation != null");

            Expression result;
            result = this.CallValueForPathWithType(
                annotation.Segment.StartPath.RootEntry,
                annotation.Segment.StartPath.ExpectedRootType,
                annotation.Segment.StartPath,
                expression.Type);

            ProjectionPath parameterPath = new ProjectionPath(
                annotation.Segment.StartPath.Root,
                annotation.Segment.StartPath.ExpectedRootType,
                annotation.Segment.StartPath.RootEntry);
            ProjectionPathSegment parameterSegment = new ProjectionPathSegment(parameterPath, null, null);
            parameterPath.Add(parameterSegment);
            this.annotations[expression] = new ExpressionAnnotation() { Segment = parameterSegment };

            return result;
        }

        private Expression RebindMemberAccess(MemberExpression m, ExpressionAnnotation baseAnnotation)
        {
            Debug.Assert(m != null, "m != null");
            Debug.Assert(baseAnnotation != null, "baseAnnotation != null");

            ProjectionPathSegment memberSegment;

            Expression baseSourceExpression = m.Expression;
            Expression result = this.pathBuilder.GetRewrite(baseSourceExpression);
            if (result != null)
            {
                Expression baseTypeExpression = Expression.Constant(baseSourceExpression.Type, typeof(Type));
                ProjectionPath nestedPath = new ProjectionPath(result as ParameterExpression, baseTypeExpression, result);
                ProjectionPathSegment nestedSegment = new ProjectionPathSegment(nestedPath, m.Member.Name, m.Type);
                nestedPath.Add(nestedSegment);
                result = this.CallValueForPathWithType(result, baseTypeExpression, nestedPath, m.Type);
            }
            else
            {
                memberSegment = new ProjectionPathSegment(baseAnnotation.Segment.StartPath, m.Member.Name, m.Type);
                baseAnnotation.Segment.StartPath.Add(memberSegment);
                result = this.CallValueForPathWithType(
                    baseAnnotation.Segment.StartPath.RootEntry,
                    baseAnnotation.Segment.StartPath.ExpectedRootType,
                    baseAnnotation.Segment.StartPath,
                    m.Type);
            }

            return result;
        }

        private NewExpression RebindNewExpressionForDataServiceCollectionOfT(NewExpression nex)
        {
            Debug.Assert(nex != null, "nex != null");
            Debug.Assert(
                ResourceBinder.PatternRules.MatchNewDataServiceCollectionOfT(nex),
                "Called should have checked that the 'new' was for our collection type");

            NewExpression result = base.VisitNew(nex);

            ExpressionAnnotation annotation = null;

            if (result != null)
            {
                ConstructorInfo constructorInfo = 
                    nex.Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First(
                        c => c.GetParameters().Length == 7 && c.GetParameters()[0].ParameterType == typeof(object));

                Type enumerable = typeof(IEnumerable<>).MakeGenericType(nex.Type.GetGenericArguments()[0]);

                if (result.Arguments.Count == 1 && result.Constructor == nex.Type.GetConstructor(new[] { enumerable }) &&
                    this.annotations.TryGetValue(result.Arguments[0], out annotation))
                {
                    result = Expression.New(
                        constructorInfo,
                        this.materializerExpression,
                        Expression.Constant(null, typeof(DataServiceContext)),
                        result.Arguments[0],
                        Expression.Constant(TrackingMode.AutoChangeTracking, typeof(TrackingMode)),
                        Expression.Constant(null, typeof(string)),
                        Expression.Constant(null, typeof(Func<EntityChangedParams, bool>)),
                        Expression.Constant(null, typeof(Func<EntityCollectionChangedParams, bool>)));
                }
                else if (result.Arguments.Count == 2 &&
                         this.annotations.TryGetValue(result.Arguments[0], out annotation))
                {
                    result = Expression.New(
                        constructorInfo, 
                        this.materializerExpression,
                        Expression.Constant(null, typeof(DataServiceContext)),
                        result.Arguments[0],                        result.Arguments[1],                        Expression.Constant(null, typeof(string)),
                        Expression.Constant(null, typeof(Func<EntityChangedParams, bool>)),
                        Expression.Constant(null, typeof(Func<EntityCollectionChangedParams, bool>)));
                }
                else if (result.Arguments.Count == 5 &&
                         this.annotations.TryGetValue(result.Arguments[0], out annotation))
                {
                    result = Expression.New(
                        constructorInfo, 
                        this.materializerExpression,
                        Expression.Constant(null, typeof(DataServiceContext)),
                        result.Arguments[0],                        result.Arguments[1],                        result.Arguments[2],                        result.Arguments[3],                        result.Arguments[4]);                }
                else if (result.Arguments.Count == 6 &&
                         typeof(DataServiceContext).IsAssignableFrom(result.Arguments[0].Type) &&
                         this.annotations.TryGetValue(result.Arguments[1], out annotation))
                {
                    result = Expression.New(
                        constructorInfo,
                        this.materializerExpression,
                        result.Arguments[0],                        result.Arguments[1],                        result.Arguments[2],                        result.Arguments[3],                        result.Arguments[4],                        result.Arguments[5]);                }
            }

            if (annotation != null)
            {
                this.annotations.Add(result, annotation);
            }

            return result;
        }

        private Expression RebindMethodCallForMemberSelect(MethodCallExpression call)
        {
            Debug.Assert(call != null, "call != null");
            Debug.Assert(call.Method.Name == "Select", "call.Method.Name == 'Select'");
            Debug.Assert(call.Object == null, "call.Object == null -- otherwise this isn't a call to a static Select method");
            Debug.Assert(call.Arguments.Count == 2, "call.Arguments.Count == 2 -- otherwise this isn't the expected Select() call on IQueryable");

            Expression result = null;
            Expression parameterSource = this.Visit(call.Arguments[0]);
            ExpressionAnnotation annotation;
            this.annotations.TryGetValue(parameterSource, out annotation);

            if (annotation != null)
            {
                Expression selectorExpression = this.Visit(call.Arguments[1]);
                Type returnElementType = call.Method.ReturnType.GetGenericArguments()[0];
                result = CallMaterializer(
                    "ProjectionSelect",
                    this.materializerExpression,
                    this.pathBuilder.ParameterEntryInScope,
                    this.pathBuilder.ExpectedParamTypeInScope,
                    Expression.Constant(returnElementType, typeof(Type)),
                    Expression.Constant(annotation.Segment.StartPath, typeof(object)),
                    selectorExpression);
                this.annotations.Add(result, annotation);
                result = CallMaterializerWithType(
                    "EnumerateAsElementType",
                    new Type[] { returnElementType },
                    result);
                this.annotations.Add(result, annotation);
            }

            if (result == null)
            {
                result = base.VisitMethodCall(call);
            }

            return result;
        }

        private Expression RebindMethodCallForMemberToList(MethodCallExpression call)
        {
            Debug.Assert(call != null, "call != null");
            Debug.Assert(call.Object == null, "call.Object == null -- otherwise this isn't a call to a static ToList method");
            Debug.Assert(call.Method.Name == "ToList", "call.Method.Name == 'ToList'");

            Debug.Assert(call.Arguments.Count == 1, "call.Arguments.Count == 1 -- otherwise this isn't the expected ToList() call on IEnumerable");

            Expression result = this.Visit(call.Arguments[0]);
            ExpressionAnnotation annotation;
            if (this.annotations.TryGetValue(result, out annotation))
            {
                result = this.TypedEnumerableToList(result, call.Type);
                this.annotations.Add(result, annotation);
            }

            return result;
        }

        private Expression RebindMethodCallForNewSequence(MethodCallExpression call)
        {
            Debug.Assert(call != null, "call != null");
            Debug.Assert(ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(call), "ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(call)");
            Debug.Assert(call.Object == null, "call.Object == null -- otherwise this isn't the supported Select or ToList methods");

            Expression result = null;

            if (call.Method.Name == "Select")
            {
                Debug.Assert(call.Arguments.Count == 2, "call.Arguments.Count == 2 -- otherwise this isn't the argument we expected");

                Expression parameterSource = this.Visit(call.Arguments[0]);
                ExpressionAnnotation annotation;
                this.annotations.TryGetValue(parameterSource, out annotation);

                if (annotation != null)
                {
                    Expression selectorExpression = this.Visit(call.Arguments[1]);
                    Type returnElementType = call.Method.ReturnType.GetGenericArguments()[0];
                    result = CallMaterializer(
                        "ProjectionSelect",
                        this.materializerExpression,
                        this.pathBuilder.ParameterEntryInScope,
                        this.pathBuilder.ExpectedParamTypeInScope,
                        Expression.Constant(returnElementType, typeof(Type)),
                        Expression.Constant(annotation.Segment.StartPath, typeof(object)),
                        selectorExpression);
                    this.annotations.Add(result, annotation);
                    result = CallMaterializerWithType(
                        "EnumerateAsElementType",
                        new Type[] { returnElementType },
                        result);
                    this.annotations.Add(result, annotation);
                }
            }
            else
            {
                Debug.Assert(call.Method.Name == "ToList", "call.Method.Name == 'ToList'");

                Expression source = this.Visit(call.Arguments[0]);
                ExpressionAnnotation annotation;
                if (this.annotations.TryGetValue(source, out annotation))
                {
                    result = this.TypedEnumerableToList(source, call.Type);
                    this.annotations.Add(result, annotation);
                }
            }

            if (result == null)
            {
                result = base.VisitMethodCall(call);
            }

            return result;
        }

        private Expression TypedEnumerableToList(Expression source, Type targetType)
        {
            Debug.Assert(source != null, "source != null");
            Debug.Assert(targetType != null, "targetType != null");

            Type enumeratedType = source.Type.GetGenericArguments()[0];
            Type listElementType = targetType.GetGenericArguments()[0];

            Expression result = CallMaterializerWithType(
                "ListAsElementType",
                new Type[] { enumeratedType, listElementType },
                this.materializerExpression,
                source);

            return result;
        }

        #endregion Private methods.

        #region Inner types.

        internal class ExpressionAnnotation
        {
            internal ProjectionPathSegment Segment 
            { 
                get; 
                set; 
            }
        }

        #endregion Inner types.
    }
}
