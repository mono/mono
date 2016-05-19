//---------------------------------------------------------------------
// <copyright file="Funcletizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Determines which leaves of a LINQ expression tree should be evaluated locally before
    /// sending a query to the store. These sub-expressions may map to query parameters (e.g. local variables),
    /// to constants (e.g. literals 'new DateTime(2008, 1, 1)') or query sub-expression
    /// (e.g. 'context.Products'). Parameter expressions are replaced with QueryParameterExpression
    /// nodes. All other elements are swapped in place with either expanded expressions (for sub-queries)
    /// or constants. Where the expression includes mutable state that may influence the translation
    /// to a query, a Func(Of Boolean) delegate is returned indicating when a recompilation is necessary.
    /// </summary>
    internal sealed class Funcletizer
    {
        // Compiled query information
        private readonly ParameterExpression _rootContextParameter;
        private readonly ObjectContext _rootContext;
        private readonly ConstantExpression _rootContextExpression;
        private readonly ReadOnlyCollection<ParameterExpression> _compiledQueryParameters;
        private readonly Mode _mode;
        private readonly HashSet<Expression> _linqExpressionStack = new HashSet<Expression>();

        // Object parameters
        private static readonly string s_parameterPrefix = "p__linq__";
        private long _parameterNumber;

        private Funcletizer(
            Mode mode,
            ObjectContext rootContext, 
            ParameterExpression rootContextParameter, 
            ReadOnlyCollection<ParameterExpression> compiledQueryParameters)
        {
            _mode = mode;
            _rootContext = rootContext;
            _rootContextParameter = rootContextParameter;
            _compiledQueryParameters = compiledQueryParameters;
            if (null != _rootContextParameter && null != _rootContext)
            {
                _rootContextExpression = Expression.Constant(_rootContext);
            }
        }

        internal static Funcletizer CreateCompiledQueryEvaluationFuncletizer(
            ObjectContext rootContext,
            ParameterExpression rootContextParameter,
            ReadOnlyCollection<ParameterExpression> compiledQueryParameters)
        {
            EntityUtil.CheckArgumentNull(rootContext, "rootContext");
            EntityUtil.CheckArgumentNull(rootContextParameter, "rootContextParameter");
            EntityUtil.CheckArgumentNull(compiledQueryParameters, "compiledQueryParameters");

            return new Funcletizer(Mode.CompiledQueryEvaluation, rootContext, rootContextParameter, compiledQueryParameters);
        }

        internal static Funcletizer CreateCompiledQueryLockdownFuncletizer()
        {
            return new Funcletizer(Mode.CompiledQueryLockdown, null, null, null);
        }

        internal static Funcletizer CreateQueryFuncletizer(ObjectContext rootContext)
        {
            EntityUtil.CheckArgumentNull(rootContext, "rootContext");

            return new Funcletizer(Mode.ConventionalQuery, rootContext, null, null);
        }

        internal ObjectContext RootContext
        {
            get { return _rootContext; }
        }

        internal ParameterExpression RootContextParameter
        {
            get { return _rootContextParameter; }
        }

        internal ConstantExpression RootContextExpression
        {
            get { return _rootContextExpression; }
        }

        internal bool IsCompiledQuery
        {
            get { return _mode == Mode.CompiledQueryEvaluation || _mode == Mode.CompiledQueryLockdown; }
        }

        /// <summary>
        /// Performs funcletization on the given expression. Also returns a delegates that can be used
        /// to determine if the entire tree needs to be recompiled.
        /// </summary>
        internal Expression Funcletize(Expression expression, out Func<bool> recompileRequired)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            // Find all candidates for funcletization. Some sub-expressions are reduced to constants,
            // others are reduced to variables. The rules vary based on the _mode.
            Func<Expression, bool> isClientConstant;
            Func<Expression, bool> isClientVariable;

            expression = ReplaceRootContextParameter(expression);

            if (_mode == Mode.CompiledQueryEvaluation)
            {
                // We lock down closure expressions for compiled queries, so everything is either
                // a constant or a query parameter produced from the explicit parameters to the
                // compiled query delegate.
                isClientConstant = Nominate(expression, this.IsClosureExpression);
                isClientVariable = Nominate(expression, this.IsCompiledQueryParameterVariable);
            }
            else if (_mode == Mode.CompiledQueryLockdown)
            {
                // When locking down a compiled query, we can evaluate all closure expressions.
                isClientConstant = Nominate(expression, this.IsClosureExpression);
                isClientVariable = (exp) => false;
            }
            else
            {
                Debug.Assert(_mode == Mode.ConventionalQuery, "No other options...");

                // There are no variable parameters outside of compiled queries, so everything is
                // either a constant or a closure expression.
                isClientConstant = Nominate(expression, this.IsImmutable);
                isClientVariable = Nominate(expression, this.IsClosureExpression);
            }

            // Now rewrite given nomination functions
            FuncletizingVisitor visitor = new FuncletizingVisitor(this, isClientConstant, isClientVariable);
            Expression result = visitor.Visit(expression);
            recompileRequired = visitor.GetRecompileRequiredFunction();
            
            return result;
        }

        /// <summary>
        /// Replaces context parameter (e.g. 'ctx' in CompiledQuery.Compile(ctx => ctx.Products)) with constant
        /// containing the object context.
        /// </summary>
        private Expression ReplaceRootContextParameter(Expression expression)
        {
            if (null != _rootContextExpression)
            {
                return EntityExpressionVisitor.Visit(
                    expression, (exp, baseVisit) =>
                    exp == _rootContextParameter ? _rootContextExpression : baseVisit(exp));
            }
            else
            {
                return expression;
            }
        }

        /// <summary>
        /// Returns a function indicating whether the given expression and all of its children satisfy the 
        /// 'localCriterion'.
        /// </summary>
        private static Func<Expression, bool> Nominate(Expression expression, Func<Expression, bool> localCriterion)
        {
            EntityUtil.CheckArgumentNull(localCriterion, "localCriterion");
            HashSet<Expression> candidates = new HashSet<Expression>();
            bool cannotBeNominated = false;
            Func<Expression, Func<Expression, Expression>, Expression> visit = (exp, baseVisit) =>
                {
                    if (exp != null)
                    {
                        bool saveCannotBeNominated = cannotBeNominated;
                        cannotBeNominated = false;
                        baseVisit(exp);
                        if (!cannotBeNominated)
                        {
                            // everyone below me can be nominated, so
                            // see if this one can be also
                            if (localCriterion(exp))
                            {
                                candidates.Add(exp);
                            }
                            else
                            {
                                cannotBeNominated = true;
                            }
                        }
                        cannotBeNominated |= saveCannotBeNominated;
                    }
                    return exp;
                };
            EntityExpressionVisitor.Visit(expression, visit);
            return candidates.Contains;
        }

        private enum Mode
        {
            CompiledQueryLockdown,
            CompiledQueryEvaluation,
            ConventionalQuery,
        }

        /// <summary>
        /// Determines whether the node may be evaluated locally and whether 
        /// it is a constant. Assumes that all children are also client expressions.
        /// </summary>
        private bool IsImmutable(Expression expression)
        {
            if (null == expression) { return false; }
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    {
                        // support construction of primitive types
                        PrimitiveType primitiveType;
                        if (!ClrProviderManifest.Instance.TryGetPrimitiveType(TypeSystem.GetNonNullableType(expression.Type),
                            out primitiveType))
                        {
                            return false;
                        }
                        return true;
                    }
                case ExpressionType.Constant:
                    return true;
                case ExpressionType.NewArrayInit:
                    // allow initialization of byte[] 'literals'
                    return (typeof(byte[]) == expression.Type);
                case ExpressionType.Convert:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the node may be evaluated locally and whether 
        /// it is a variable. Assumes that all children are also variable client expressions.
        /// </summary>
        private bool IsClosureExpression(Expression expression)
        {
            if (null == expression) { return false; }
            if (IsImmutable(expression)) { return true; }
            if (ExpressionType.MemberAccess == expression.NodeType)
            {
                MemberExpression member = (MemberExpression)expression;
                if (member.Member.MemberType == MemberTypes.Property)
                {
                    return ExpressionConverter.CanFuncletizePropertyInfo((PropertyInfo)member.Member);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the node may be evaluated as a compiled query parameter.
        /// Assumes that all children are also eligible compiled query parameters.
        /// </summary>
        private bool IsCompiledQueryParameterVariable(Expression expression)
        {
            if (null == expression) { return false; }
            if (IsClosureExpression(expression)) { return true; }
            if (ExpressionType.Parameter == expression.NodeType)
            {
                ParameterExpression parameter = (ParameterExpression)expression;
                return _compiledQueryParameters.Contains(parameter);
            }
            return false;
        }

        /// <summary>
        /// Determine whether the given CLR type is legal for an ObjectParameter or constant
        /// DbExpression.
        /// </summary>
        private bool TryGetTypeUsageForTerminal(Type type, out TypeUsage typeUsage)
        {
            EntityUtil.CheckArgumentNull(type, "type");

            if (_rootContext.Perspective.TryGetTypeByName(TypeSystem.GetNonNullableType(type).FullName,
                false, // bIgnoreCase
                out typeUsage) &&
                (TypeSemantics.IsScalarType(typeUsage)))
            {
                return true;
            }

            typeUsage = null;
            return false;
        }

        /// <summary>
        /// Creates the next available parameter name.
        /// </summary>
        internal string GenerateParameterName()
        {
            // To avoid collisions with user parameters (the full set is not
            // known at this time) we plug together an 'unlikely' prefix and 
            // a number.
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}",
                s_parameterPrefix,
                _parameterNumber++);
        }

        /// <summary>
        /// Walks the expression tree and replaces client-evaluable expressions with constants
        /// or QueryParameterExpressions.
        /// </summary>
        private sealed class FuncletizingVisitor : EntityExpressionVisitor
        {
            private readonly Funcletizer _funcletizer;
            private readonly Func<Expression, bool> _isClientConstant;
            private readonly Func<Expression, bool> _isClientVariable;
            private readonly List<Func<bool>> _recompileRequiredDelegates = new List<Func<bool>>();

            internal FuncletizingVisitor(
                Funcletizer funcletizer,
                Func<Expression, bool> isClientConstant,
                Func<Expression, bool> isClientVariable)
            {
                EntityUtil.CheckArgumentNull(funcletizer, "funcletizer");
                EntityUtil.CheckArgumentNull(isClientConstant, "isClientConstant");
                EntityUtil.CheckArgumentNull(isClientVariable, "isClientVariable");
            
                _funcletizer = funcletizer;
                _isClientConstant = isClientConstant;
                _isClientVariable = isClientVariable;
            }

            /// <summary>
            /// Returns a delegate indicating (when called) whether a change has been identified
            /// requiring a complete recompile of the query.
            /// </summary>
            internal Func<bool> GetRecompileRequiredFunction()
            {
                // assign list to local variable to avoid including the entire Funcletizer
                // class in the closure environment
                ReadOnlyCollection<Func<bool>> recompileRequiredDelegates = _recompileRequiredDelegates.AsReadOnly();
                return () => recompileRequiredDelegates.Any(d => d());
            }

            internal override Expression Visit(Expression exp)
            {
                if (exp != null)
                {
                    if (!_funcletizer._linqExpressionStack.Add(exp))
                    {
                        // This expression is already in the stack.
                        throw EntityUtil.InvalidOperation(Strings.ELinq_CycleDetected);
                    }

                    try
                    {
                        if (_isClientConstant(exp))
                        {
                            return InlineValue(exp, false);
                        }
                        else if (_isClientVariable(exp))
                        {
                            TypeUsage queryParameterType;
                            if (_funcletizer.TryGetTypeUsageForTerminal(exp.Type, out queryParameterType))
                            {
                                DbParameterReferenceExpression parameterReference = queryParameterType.Parameter(_funcletizer.GenerateParameterName());
                                return new QueryParameterExpression(parameterReference, exp, _funcletizer._compiledQueryParameters);
                            }
                            else if (_funcletizer.IsCompiledQuery)
                            {
                                throw InvalidCompiledQueryParameterException(exp);
                            }
                            else
                            {
                                return InlineValue(exp, true);
                            }
                        }
                        return base.Visit(exp);
                    }
                    finally
                    {
                        _funcletizer._linqExpressionStack.Remove(exp);
                    }
                }
                return base.Visit(exp);
            }

            private static NotSupportedException InvalidCompiledQueryParameterException(Expression expression)
            {
                ParameterExpression parameterExp;
                if (expression.NodeType == ExpressionType.Parameter)
                {
                    parameterExp = (ParameterExpression)expression;
                }
                else
                {
                    // If this is a simple query parameter (involving a single delegate parameter) report the
                    // type of that parameter. Otherwise, report the type of the part of the parameter.
                    HashSet<ParameterExpression> parameters = new HashSet<ParameterExpression>();
                    EntityExpressionVisitor.Visit(expression, (exp, baseVisit) =>
                    {
                        if (null != exp && exp.NodeType == ExpressionType.Parameter)
                        {
                            parameters.Add((ParameterExpression)exp);
                        }
                        return baseVisit(exp);
                    });

                    if (parameters.Count != 1)
                    {
                        return EntityUtil.NotSupported(Strings.CompiledELinq_UnsupportedParameterTypes(expression.Type.FullName));
                    }
                    
                    parameterExp = parameters.Single();
                }

                if (parameterExp.Type.Equals(expression.Type))
                {
                    // If the expression type is the same as the parameter type, indicate that the parameter type is not valid.
                    return EntityUtil.NotSupported(Strings.CompiledELinq_UnsupportedNamedParameterType(parameterExp.Name, parameterExp.Type.FullName));                    
                }
                else
                {
                    // Otherwise, indicate that using the specified parameter to produce a value of the expression's type is not supported in compiled query
                    return EntityUtil.NotSupported(Strings.CompiledELinq_UnsupportedNamedParameterUseAsType(parameterExp.Name, expression.Type.FullName));
                }
            }
        
            /// <summary>
            /// Compiles a delegate returning the value of the given expression.
            /// </summary>
            private Func<object> CompileExpression(Expression expression)
            {
                Func<object> func = Expression
                    .Lambda<Func<object>>(TypeSystem.EnsureType(expression, typeof(object)))
                    .Compile();
                return func;
            }

            /// <summary>
            /// Inlines a funcletizable expression. Queries and lambda expressions are expanded
            /// inline. All other values become simple constants.
            /// </summary>
            private Expression InlineValue(Expression expression, bool recompileOnChange)
            {
                Func<object> getValue = null;
                object value = null;
                if (expression.NodeType == ExpressionType.Constant)
                {
                    value = ((ConstantExpression)expression).Value;
                }
                else
                {
                    bool fastPath = false;
                    //fastpath to process object query
                    if (expression.NodeType == ExpressionType.Convert)
                    {
                        var ue = (UnaryExpression)expression;
                        // The ObjectSet instance is wrapped inside Convert UnaryExpression in 
                        // ElinqQueryState.GetExpression(). The block below identifies such an 
                        // expression, makes sure the object query it contains is immutable and 
                        // extracts the reference to the object query.
                        if (!recompileOnChange
                            && ue.Operand.NodeType == ExpressionType.Constant
                            && typeof(IQueryable).IsAssignableFrom(ue.Operand.Type))
                        {
                            value = ((ConstantExpression)ue.Operand).Value;
                            fastPath = true;
                        }
                    }
                    if (!fastPath)
                    {
                        getValue = CompileExpression(expression);
                        value = getValue();
                    }
                }

                Expression result = null;
                ObjectQuery inlineQuery = value as ObjectQuery;
                if (inlineQuery != null)
                {
                    result = InlineObjectQuery(inlineQuery, expression.Type);
                }
                else
                {
                    LambdaExpression lambda = value as LambdaExpression;
                    if (null != lambda)
                    {
                        result = InlineExpression(Expression.Quote(lambda));
                    }
                    else
                    {
                        // everything else is just a constant...
                        result = expression.NodeType == ExpressionType.Constant
                            ? expression
                            : Expression.Constant(value, expression.Type);
                    }
                }

                if (recompileOnChange)
                {
                    AddRecompileRequiredDelegates(getValue, value);
                }

                return result;
            }

            private void AddRecompileRequiredDelegates(Func<object> getValue, object value)
            {
                // Build a delegate that returns true when the inline value has changed.
                // Outside of ObjectQuery, this amounts to a reference comparison.
                ObjectQuery originalQuery = value as ObjectQuery;
                if (null != originalQuery)
                {
                    // For inline queries, we need to check merge options as well (it's mutable)
                    MergeOption? originalMergeOption = originalQuery.QueryState.UserSpecifiedMergeOption;
                    if (null == getValue)
                    {
                        _recompileRequiredDelegates.Add(() => originalQuery.QueryState.UserSpecifiedMergeOption != originalMergeOption);
                    }
                    else
                    {
                        _recompileRequiredDelegates.Add(() =>
                        {
                            ObjectQuery currentQuery = getValue() as ObjectQuery;
                            return !object.ReferenceEquals(originalQuery, currentQuery) ||
                                currentQuery.QueryState.UserSpecifiedMergeOption != originalMergeOption;
                        });

                    }
                }
                else if (null != getValue)
                {
                    _recompileRequiredDelegates.Add(() => !object.ReferenceEquals(value, getValue()));
                }
            }

            /// <summary>
            /// Gets the appropriate LINQ expression for an inline ObjectQuery instance.
            /// </summary>
            private Expression InlineObjectQuery(ObjectQuery inlineQuery, Type expressionType)
            {
                EntityUtil.CheckArgumentNull(inlineQuery, "inlineQuery");

                Expression queryExpression;
                if (_funcletizer._mode == Mode.CompiledQueryLockdown)
                {
                    // In the lockdown phase, we don't chase down inline object queries because
                    // we don't yet know what the object context is supposed to be.
                    queryExpression = Expression.Constant(inlineQuery, expressionType);
                }
                else
                {
                    if (!object.ReferenceEquals(_funcletizer._rootContext, inlineQuery.QueryState.ObjectContext))
                    {
                        throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_UnsupportedDifferentContexts);
                    }

                    queryExpression = inlineQuery.GetExpression();

                    // If it's not an entity-sql (terminal) query, recursively process
                    if (!(inlineQuery.QueryState is EntitySqlQueryState))
                    {
                        queryExpression = InlineExpression(queryExpression);
                    }

                    queryExpression = TypeSystem.EnsureType(queryExpression, expressionType);
                }

                return queryExpression;
            }

            private Expression InlineExpression(Expression exp)
            {
                Func<bool> inlineExpressionRequiresRecompile;
                exp = _funcletizer.Funcletize(exp, out inlineExpressionRequiresRecompile);
                if (!_funcletizer.IsCompiledQuery)
                {
                    _recompileRequiredDelegates.Add(inlineExpressionRequiresRecompile);
                }
                return exp;
            }
        }
    }

    /// <summary>
    /// A LINQ expression corresponding to a query parameter.
    /// </summary>
    internal sealed class QueryParameterExpression : Expression
    {
        private readonly DbParameterReferenceExpression _parameterReference;
        private readonly Type _type;
        private readonly Expression _funcletizedExpression;
        private readonly IEnumerable<ParameterExpression> _compiledQueryParameters;
        private Delegate _cachedDelegate;

        internal QueryParameterExpression(
            DbParameterReferenceExpression parameterReference,
            Expression funcletizedExpression,
            IEnumerable<ParameterExpression> compiledQueryParameters)
        {
            EntityUtil.CheckArgumentNull(parameterReference, "parameterReference");
            EntityUtil.CheckArgumentNull(funcletizedExpression, "funcletizedExpression");
            _compiledQueryParameters = compiledQueryParameters ?? Enumerable.Empty<ParameterExpression>();
            _parameterReference = parameterReference;
            _type = funcletizedExpression.Type;
            _funcletizedExpression = funcletizedExpression;
            _cachedDelegate = null;
        }

        /// <summary>
        /// Gets the current value of the parameter given (optional) compiled query arguments.
        /// </summary>
        internal object EvaluateParameter(object[] arguments)
        {
            if (_cachedDelegate == null)
            {
                if (_funcletizedExpression.NodeType == ExpressionType.Constant)
                {
                    return ((ConstantExpression)_funcletizedExpression).Value;
                }
                ConstantExpression ce;
                if (TryEvaluatePath(_funcletizedExpression, out ce))
                {
                    return ce.Value;
                }
            }

            try
            {
                if (_cachedDelegate == null)
                {
                    // Get the Func<> type for the property evaluator
                    Type delegateType = TypeSystem.GetDelegateType(_compiledQueryParameters.Select(p => p.Type), _type);

                    // Now compile delegate for the funcletized expression
                    _cachedDelegate = Expression.Lambda(delegateType, _funcletizedExpression, _compiledQueryParameters).Compile();
                }
                return _cachedDelegate.DynamicInvoke(arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Create QueryParameterExpression based on this one, but with the funcletized expression
        /// wrapped by the given method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        internal QueryParameterExpression EscapeParameterForLike(Func<string, string> method)
        {
            Expression wrappedExpression = Expression.Invoke(Expression.Constant(method), this._funcletizedExpression);
            return new QueryParameterExpression(this._parameterReference, wrappedExpression, this._compiledQueryParameters);
        }

        /// <summary>
        /// Gets the parameter reference for the parameter.
        /// </summary>
        internal DbParameterReferenceExpression ParameterReference
        {
            get { return _parameterReference; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public override ExpressionType NodeType
        {
            get { return EntityExpressionVisitor.CustomExpression; }
        }

        private bool TryEvaluatePath(Expression expression, out ConstantExpression constantExpression)
        {
            MemberExpression me = expression as MemberExpression;
            constantExpression = null;
            if (me != null)
            {
                Stack<MemberExpression> stack = new Stack<MemberExpression>();
                stack.Push(me);
                while ((me = me.Expression as MemberExpression) != null)
                {
                    stack.Push(me);
                }
                me = stack.Pop();
                ConstantExpression ce = me.Expression as ConstantExpression;
                if (ce != null)
                {
                    object memberVal;
                    if (!TryGetFieldOrPropertyValue(me, ((ConstantExpression)me.Expression).Value, out memberVal))
                    {
                        return false;
                    }
                    if (stack.Count > 0)
                    {
                        foreach (var rec in stack)
                        {
                            if (!TryGetFieldOrPropertyValue(rec, memberVal, out memberVal))
                            {
                                return false;
                            }
                        }
                    }
                    constantExpression = Expression.Constant(memberVal, expression.Type);
                    return true;
                }
            }
            return false;
        }

        private bool TryGetFieldOrPropertyValue(MemberExpression me, object instance, out object memberValue)
        {
            bool result = false;
            memberValue = null;

            try
            {
                if (me.Member.MemberType == MemberTypes.Field)
                {
                    memberValue = ((FieldInfo)me.Member).GetValue(instance);
                    result = true;
                }
                else if (me.Member.MemberType == MemberTypes.Property)
                {
                    memberValue = ((PropertyInfo)me.Member).GetValue(instance, null);
                    result = true;
                }
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
