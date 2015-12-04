//------------------------------------------------------------------------------
// <copyright file="CoordinatorScratchpad.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Objects.ELinq;
using System.Data.Query.InternalTrees;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common.Internal.Materialization
{
    /// <summary>
    /// Used in the Translator to aggregate information about a (nested) reader 
    /// coordinator. After the translator visits the columnMaps, it will compile
    /// the coordinator(s) which produces an immutable CoordinatorFactory that 
    /// can be shared amongst many query instances.
    /// </summary>
    internal class CoordinatorScratchpad
    {
        #region private state

        private readonly Type _elementType;
        private CoordinatorScratchpad _parent;
        private readonly List<CoordinatorScratchpad> _nestedCoordinatorScratchpads;
        /// <summary>
        /// Map from original expressions to expressions with detailed error handling.
        /// </summary>
        private readonly Dictionary<Expression, Expression> _expressionWithErrorHandlingMap;
        /// <summary>
        /// Expressions that should be precompiled (i.e. reduced to constants in 
        /// compiled delegates.
        /// </summary>
        private readonly HashSet<LambdaExpression> _inlineDelegates;

        #endregion

        #region constructor

        internal CoordinatorScratchpad(Type elementType)
        {
            _elementType = elementType;
            _nestedCoordinatorScratchpads = new List<CoordinatorScratchpad>();
            _expressionWithErrorHandlingMap = new Dictionary<Expression, Expression>();
            _inlineDelegates = new HashSet<LambdaExpression>();
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// For nested collections, returns the parent coordinator.
        /// </summary>
        internal CoordinatorScratchpad Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Gets or sets an Expression setting key values (these keys are used
        /// to determine when a collection has entered a new chapter) from the
        /// underlying store data reader.
        /// </summary>
        internal Expression SetKeys { get; set; }

        /// <summary>
        /// Gets or sets an Expression returning 'true' when the key values for 
        /// the current nested result (see SetKeys) are equal to the current key  
        /// values on the underlying data reader.
        /// </summary>
        internal Expression CheckKeys { get; set; }

        /// <summary>
        /// Gets or sets an expression returning 'true' if the current row in 
        /// the underlying data reader contains an element of the collection.
        /// </summary>
        internal Expression HasData { get; set; }

        /// <summary>
        /// Gets or sets an Expression yielding an element of the current collection
        /// given values in the underlying data reader.
        /// </summary>
        internal Expression Element { get; set; }

        /// <summary>
        /// Gets or sets an Expression initializing the collection storing results from this coordinator.
        /// </summary>
        internal Expression InitializeCollection { get; set; }

        /// <summary>
        /// Indicates which Shaper.State slot is home for this collection's coordinator.
        /// Used by Parent to pull out nested collection aggregators/streamers.
        /// </summary>
        internal int StateSlotNumber { get; set; }

        /// <summary>
        /// Gets or sets the depth of the current coordinator. A root collection has depth 0.
        /// </summary>
        internal int Depth { get; set; }

        /// <summary>
        /// List of all record types that we can return at this level in the query.
        /// </summary>
        private List<RecordStateScratchpad> _recordStateScratchpads;

        /// <summary>
        /// Allows sub-expressions to register an 'interest' in exceptions thrown when reading elements
        /// for this coordinator. When an exception is thrown, we rerun the delegate using the slower
        /// but more error-friendly versions of expressions (e.g. reader.GetValue + type check instead
        /// of reader.GetInt32())
        /// </summary>
        /// <param name="expression">The lean and mean raw version of the expression</param>
        /// <param name="expressionWithErrorHandling">The slower version of the same expression with better
        /// error handling</param>
        internal void AddExpressionWithErrorHandling(Expression expression, Expression expressionWithErrorHandling)
        {
            _expressionWithErrorHandlingMap[expression] = expressionWithErrorHandling;
        }

        /// <summary>
        /// Registers a lambda expression for pre-compilation (i.e. reduction to a constant expression)
        /// within materialization expression. Otherwise, the expression will be compiled every time
        /// the enclosing delegate is invoked.
        /// </summary>
        /// <param name="expression">Lambda expression to register.</param>
        internal void AddInlineDelegate(LambdaExpression expression)
        {
            _inlineDelegates.Add(expression);
        }

        /// <summary>
        /// Registers a coordinator for a nested collection contained in elements of this collection.
        /// </summary>
        internal void AddNestedCoordinator(CoordinatorScratchpad nested)
        {
            Debug.Assert(nested.Depth == this.Depth + 1, "can only nest depth + 1");
            nested._parent = this;
            _nestedCoordinatorScratchpads.Add(nested);
        }

        /// <summary>
        /// Use the information stored on the scratchpad to compile an immutable factory used
        /// to construct the coordinators used at runtime when materializing results.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal CoordinatorFactory Compile()
        {
            RecordStateFactory[] recordStateFactories;
            if (null != _recordStateScratchpads)
            {
                recordStateFactories = new RecordStateFactory[_recordStateScratchpads.Count];
                for (int i = 0; i < recordStateFactories.Length; i++)
                {
                    recordStateFactories[i] = _recordStateScratchpads[i].Compile();
                }
            }
            else
            {
                recordStateFactories = new RecordStateFactory[0];
            }

            CoordinatorFactory[] nestedCoordinators = new CoordinatorFactory[_nestedCoordinatorScratchpads.Count];
            for (int i = 0; i < nestedCoordinators.Length; i++)
            {
                nestedCoordinators[i] = _nestedCoordinatorScratchpads[i].Compile();
            }

            // compile inline delegates
            ReplacementExpressionVisitor replacementVisitor = new ReplacementExpressionVisitor(null, this._inlineDelegates);
            Expression element = new SecurityBoundaryExpressionVisitor().Visit(replacementVisitor.Visit(this.Element));

            // substitute expressions that have error handlers into a new expression (used
            // when a more detailed exception message is needed)
            replacementVisitor = new ReplacementExpressionVisitor(this._expressionWithErrorHandlingMap, this._inlineDelegates);
            Expression elementWithErrorHandling = new SecurityBoundaryExpressionVisitor().Visit(replacementVisitor.Visit(this.Element));

            CoordinatorFactory result = (CoordinatorFactory)Activator.CreateInstance(typeof(CoordinatorFactory<>).MakeGenericType(_elementType), new object[] {
                                                            this.Depth, 
                                                            this.StateSlotNumber, 
                                                            this.HasData, 
                                                            this.SetKeys, 
                                                            this.CheckKeys, 
                                                            nestedCoordinators, 
                                                            element,
                                                            elementWithErrorHandling,
                                                            this.InitializeCollection,
                                                            recordStateFactories
                                                            });
            return result;
        }

        /// <summary>
        /// Allocates a new RecordStateScratchpad and adds it to the list of the ones we're
        /// responsible for; will create the list if it hasn't alread been created.
        /// </summary>
        internal RecordStateScratchpad CreateRecordStateScratchpad()
        {
            RecordStateScratchpad recordStateScratchpad = new RecordStateScratchpad();

            if (null == _recordStateScratchpads)
            {
                _recordStateScratchpads = new List<RecordStateScratchpad>();
            }
            _recordStateScratchpads.Add(recordStateScratchpad);
            return recordStateScratchpad;
        }
        #endregion

        #region Nested types

        /// <summary>
        /// Visitor supporting (non-recursive) replacement of LINQ sub-expressions and
        /// compilation of inline delegates.
        /// </summary>
        private class ReplacementExpressionVisitor : EntityExpressionVisitor
        {
            // Map from original expressions to replacement expressions.
            private readonly Dictionary<Expression, Expression> _replacementDictionary;
            private readonly HashSet<LambdaExpression> _inlineDelegates;

            internal ReplacementExpressionVisitor(Dictionary<Expression, Expression> replacementDictionary,
                HashSet<LambdaExpression> inlineDelegates)
            {
                this._replacementDictionary = replacementDictionary;
                this._inlineDelegates = inlineDelegates;
            }

            internal override Expression Visit(Expression expression)
            {
                if (null == expression)
                {
                    return expression;
                }

                Expression result;

                // check to see if a substitution has been provided for this expression
                Expression replacement;
                if (null != this._replacementDictionary && this._replacementDictionary.TryGetValue(expression, out replacement))
                {
                    // once a substitution is found, we stop walking the sub-expression and
                    // return immediately (since recursive replacement is not needed or wanted)
                    result = replacement;
                }
                else
                {
                    // check if we need to precompile an inline delegate
                    bool preCompile = false;
                    LambdaExpression lambda = null;

                    if (expression.NodeType == ExpressionType.Lambda &&
                        null != _inlineDelegates)
                    {
                        lambda = (LambdaExpression)expression;
                        preCompile = _inlineDelegates.Contains(lambda);
                    }

                    if (preCompile)
                    {
                        // do replacement in the body of the lambda expression
                        Expression body = Visit(lambda.Body);

                        // compile to a delegate
                        result = Expression.Constant(Translator.Compile(body.Type, body));
                    }
                    else
                    {
                        result = base.Visit(expression);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Used to replace references to user expressions with compiled delegates
        /// which represent those expressions.
        /// </summary>
        /// <remarks>
        /// The materialization delegate used to be one big function, which included
        /// user-provided expressions in various places in the tree. Due to security reasons
        /// (Dev11 311339), we need to separate this delegate into two pieces: trusted code,
        /// run under a security assert, and untrusted code, run under the current AppDomain's
        /// permission set.
        /// 
        /// This visitor does that separation by compiling the untrusted code into delegates
        /// and re-inserting them back into the expression tree. When the untrusted code is
        /// run, it will run in another stack frame that does not have a security assert
        /// associated with it; therefore, any attempt to take advantage of MemberAccess
        /// reflection permissions will be blocked by the CLR.
        /// 
        /// The compiled user delegates accept two parameters, one of type DbDataReader
        /// to speed up access to the current reader, and the other of type object[],
        /// which contains all other values that they might require to correctly materialize an object. Most of these
        /// objects require the <see cref="Shaper"/>, so they must be run inside of trusted code.
        /// </remarks>
        private sealed class SecurityBoundaryExpressionVisitor : EntityExpressionVisitor
        {
            private static readonly MethodInfo s_userMaterializationFuncInvokeMethod = typeof(Func<DbDataReader, object[], object>).GetMethod("Invoke");
            private ParameterExpression _values = Expression.Parameter(typeof(object[]), "values");
            private ParameterExpression _reader = Expression.Parameter(typeof(DbDataReader), "reader");
            private List<Expression> _initializationArguments = new List<Expression>();
            private int _userExpressionDepth;

            /// <summary>
            /// Used to track the type of a constructor argument or member assignment
            /// when it could be a special type we create (e.g., CompensatingCollection{T}
            /// for collections and Grouping{K,V} for groups).
            /// </summary>
            private Type _userArgumentType;

            internal override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return exp;
                }

                var nex = exp as NewExpression;
                if (nex != null && _userExpressionDepth >= 1)
                {
                    // We are creating an internal type like CompensatingCollection<T> or Grouping<K, V>
                    // and at this particular point we are sure that the user isn't creating these
                    // since this.userArgumentType is not null.
                    if (_userArgumentType != null && !nex.Type.IsPublic && nex.Type.Assembly == typeof(SecurityBoundaryExpressionVisitor).Assembly)
                    {
                        return this.CreateInitializationArgumentReplacement(nex, _userArgumentType);
                    }

                    var constructorParameters = nex.Constructor.GetParameters();
                    var arguments = nex.Arguments;
                    var newArguments = new List<Expression>();
                    for (var i = 0; i < arguments.Count; ++i)
                    {
                        var argument = arguments[i];

                        // Visit this argument because it itself could be a user expression e.g.
                        // new { Argument = new SecureString { m_length = 32 } }
                        _userArgumentType = constructorParameters[i].ParameterType;
                        var visitedArgument = this.Visit(argument);

                        // If it hasn't changed, it's trusted code. (Untrusted code would have its
                        // Convert and MarkAsUserExpression expressions removed.)
                        if (visitedArgument == argument)
                        {
                            var convert = this.CreateInitializationArgumentReplacement(argument);

                            // Change the argument to access the values array.
                            newArguments.Add(convert);
                        }
                        else
                        {
                            newArguments.Add(visitedArgument);
                        }
                    }

                    nex = Expression.New(nex.Constructor, newArguments);

                    if (_userExpressionDepth == 1)
                    {
                        var userMaterializationFunc = Expression.Lambda<Func<DbDataReader, object[], object>>(nex, _reader, _values).Compile();

                        // Convert the constructor invocation into a func that runs without elevated permissions.
                        return Expression.Convert(
                            Expression.Call(
                                Expression.Constant(userMaterializationFunc),
                                s_userMaterializationFuncInvokeMethod,
                                Translator.Shaper_Reader,
                                Expression.NewArrayInit(typeof(object), _initializationArguments)),
                            nex.Type);
                    }

                    return nex;
                }

                return base.Visit(exp);
            }

            internal override Expression VisitConditional(ConditionalExpression c)
            {
                if (_userExpressionDepth >= 1 && _userArgumentType != null)
                {
                    var test = c.Test as MethodCallExpression;
                    var ifFalse = c.IfFalse as MethodCallExpression;

                    // We can optimize the path that checks for DbNull and then
                    // reads a value directly off the reader or invokes another user expression.
                    if (test != null && test.Object != null
                        && typeof(DbDataReader).IsAssignableFrom(test.Object.Type)
                        && test.Method.Name == "IsDBNull")
                    {
                        if (ifFalse != null && (ifFalse.Object != null && typeof(DbDataReader).IsAssignableFrom(ifFalse.Object.Type) || IsUserExpressionMethod(ifFalse.Method)))
                        {
                            return base.VisitConditional(c);
                        }
                    }

                    // If there's something more complicated then we have to replace it all.
                    // We can't just replace the false expression because it may not be evaluated
                    // if the test returns true.
                    return this.CreateInitializationArgumentReplacement(c);
                }

                return base.VisitConditional(c);
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                if (_userExpressionDepth >= 1)
                {
                    // Sometimes we will add expressions inside of a user expression that is actually
                    // our code, but we need to rewrite it since it accesses the shaper's reader to check if a column is null.
                    // e.g. Select(x => new { Y = new Entity { Name = x.Name } })
                    // -> new f<>__AnonymousType`1(IIF($shaper.Reader.IsDbNull(0), null, new Entity { Name = $shaper.Reader.GetString(0) }))
                    if (typeof(DbDataReader).IsAssignableFrom(m.Type))
                    {
                        var shaper = m.Expression as ParameterExpression;
                        if (shaper != null && shaper == Translator.Shaper_Parameter)
                        {
                            return _reader;
                        }
                    }
                }

                return base.VisitMemberAccess(m);
            }

            internal override Expression VisitMemberInit(MemberInitExpression init)
            {
                if (_userExpressionDepth >= 1)
                {
                    var newMemberInit = base.VisitMemberInit(init);

                    // Only compile into a delegate if this is the top-level user expression.
                    if (newMemberInit != init && _userExpressionDepth == 1)
                    {
                        var userMaterializationFunc = Expression.Lambda<Func<DbDataReader, object[], object>>(newMemberInit, _reader, _values).Compile();

                        // Convert the object initializer into a func that runs without elevated permissions.
                        return Expression.Convert(
                            Expression.Call(
                                Expression.Constant(userMaterializationFunc),
                                s_userMaterializationFuncInvokeMethod,
                                Translator.Shaper_Reader,
                                Expression.NewArrayInit(typeof(object), _initializationArguments)),
                            init.Type);
                    }
                    else
                    {
                        return newMemberInit;
                    }
                }

                return base.VisitMemberInit(init);
            }

            internal override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
            {
                if (_userExpressionDepth >= 1)
                {
                    var fieldInfo = assignment.Member as FieldInfo;
                    var propertyInfo = assignment.Member as PropertyInfo;
                    if (fieldInfo != null)
                    {
                        _userArgumentType = fieldInfo.FieldType;
                    }
                    else if (propertyInfo != null)
                    {
                        _userArgumentType = propertyInfo.PropertyType;
                    }
                }

                return base.VisitMemberAssignment(assignment);
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                var method = m.Method;
                if (IsUserExpressionMethod(method))
                {
                    Debug.Assert(
                        m.Arguments.Count == 1,
                        "m.Arguments.Count == 1",
                        "There should be one expression argument provided to the user expression marker.");

                    try
                    {
                        // Clear this type because we are about to process a user expression
                        _userArgumentType = null;

                        _userExpressionDepth++;
                        return this.Visit(m.Arguments[0]);
                    }
                    finally
                    {
                        _userExpressionDepth--;
                    }
                }
                else if (_userExpressionDepth >= 1)
                {
                    // If this method call is on a DbDataReader then we can replace it; otherwise,
                    // assume it's something on the shaper and extract the value into the values array.
                    if (m.Object != null && typeof(DbDataReader).IsAssignableFrom(m.Object.Type))
                    {
                        return base.VisitMethodCall(m);
                    }

                    return this.CreateInitializationArgumentReplacement(m);
                }

                return base.VisitMethodCall(m);
            }

            private Expression CreateInitializationArgumentReplacement(Expression original)
            {
                return this.CreateInitializationArgumentReplacement(original, original.Type);
            }

            private Expression CreateInitializationArgumentReplacement(Expression original, Type expressionType)
            {
                _initializationArguments.Add(Expression.Convert(original, typeof(object)));
                
                return Expression.Convert(
                    Expression.MakeBinary(ExpressionType.ArrayIndex, _values, Expression.Constant(_initializationArguments.Count - 1)),
                    expressionType);
            }

            private static bool IsUserExpressionMethod(MethodInfo method)
            {
                return method.IsGenericMethod && method.GetGenericMethodDefinition() == InitializerMetadata.UserExpressionMarker;
            }
        }
        #endregion
    }
}
