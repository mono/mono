/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_TASKS
using System.Threading.Tasks;
#endif

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

#if !FEATURE_DYNAMIC_EXPRESSION_VISITOR
#if FEATURE_CORE_DLR
namespace System.Linq.Expressions {
#else
namespace Microsoft.Scripting.Ast {
#endif
    public abstract class DynamicExpressionVisitor : ExpressionVisitor {
    }
}
#endif

namespace Microsoft.Scripting.Utils {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public static class DynamicUtils {
        /// <summary>
        /// Returns the list of expressions represented by the <see cref="DynamicMetaObject"/> instances.
        /// </summary>
        /// <param name="objects">An array of <see cref="DynamicMetaObject"/> instances to extract expressions from.</param>
        /// <returns>The array of expressions.</returns>
        public static Expression[] GetExpressions(DynamicMetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                DynamicMetaObject mo = objects[i];
                res[i] = mo != null ? mo.Expression : null;
            }

            return res;
        }

        /// <summary>
        /// Creates an instance of <see cref="DynamicMetaObject"/> for a runtime value and the expression that represents it during the binding process.
        /// </summary>
        /// <param name="argValue">The runtime value to be represented by the <see cref="DynamicMetaObject"/>.</param>
        /// <param name="parameterExpression">An expression to represent this <see cref="DynamicMetaObject"/> during the binding process.</param>
        /// <returns>The new instance of <see cref="DynamicMetaObject"/>.</returns>
        public static DynamicMetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicMetaObjectProvider ido = argValue as IDynamicMetaObjectProvider;
            if (ido != null) {
                return ido.GetMetaObject(parameterExpression);
            } else {
                return new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty, argValue);
            }
        }

        /// <summary>
        /// Produces an interpreted binding using the given binder which falls over to a compiled
        /// binding after hitCount tries.
        /// 
        /// This method should be called whenever an interpreted binding is required.  Sometimes it will
        /// return a compiled binding if a previous binding was produced and it's hit count was exhausted.
        /// In this case the binder will not be called back for a new binding - the previous one will
        /// be used.
        /// </summary>
        /// <typeparam name="T">The delegate type being used for the call site</typeparam>
        /// <param name="binder">The binder used for the call site</param>
        /// <param name="compilationThreshold">The number of calls before the binder should switch to a compiled mode.</param>
        /// <param name="args">The arguments that are passed for the binding (as received in a BindDelegate call)</param>
        /// <returns>A delegate which represents the interpreted binding.</returns>
        public static T/*!*/ LightBind<T>(this DynamicMetaObjectBinder/*!*/ binder, object[]/*!*/ args, int compilationThreshold) where T : class {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(args, "args");

            return GenericInterpretedBinder<T>.Instance.Bind(binder, compilationThreshold < 0 ? LightCompiler.DefaultCompilationThreshold : compilationThreshold, args);
        }

        private class GenericInterpretedBinder<T> where T : class {
            public static GenericInterpretedBinder<T>/*!*/ Instance = new GenericInterpretedBinder<T>();
            private readonly ReadOnlyCollection<ParameterExpression>/*!*/ _parameters;
            private readonly Expression/*!*/ _updateExpression;

            private GenericInterpretedBinder() {
                var invokeMethod = typeof(T).GetMethod("Invoke");
                var methodParams = invokeMethod.GetParameters();

                ReadOnlyCollectionBuilder<ParameterExpression> prms = new ReadOnlyCollectionBuilder<ParameterExpression>(methodParams.Length);
                ReadOnlyCollectionBuilder<Expression> invokePrms = new ReadOnlyCollectionBuilder<Expression>(methodParams.Length);
                for (int i = 0; i < methodParams.Length; i++) {
                    var param = Expression.Parameter(methodParams[i].ParameterType);
                    if (i == 0) {
                        invokePrms.Add(Expression.Convert(param, typeof(CallSite<T>)));
                    } else {
                        invokePrms.Add(param);
                    }
                    prms.Add(param);
                }

                _parameters = prms.ToReadOnlyCollection();

                _updateExpression = Expression.Block(
                    Expression.Label(CallSiteBinder.UpdateLabel),
                    Expression.Invoke(
                        Expression.Property(
                            invokePrms[0],
                            typeof(CallSite<T>).GetDeclaredProperty("Update")
                        ),
                        invokePrms.ToReadOnlyCollection()
                    )
                );
            }

            public T/*!*/ Bind(DynamicMetaObjectBinder/*!*/ binder, int compilationThreshold, object[] args) {
                if (CachedBindingInfo<T>.LastInterpretedFailure != null && CachedBindingInfo<T>.LastInterpretedFailure.Binder == binder) {
                    // we failed the rule because we have a compiled target available, return the compiled target
                    Debug.Assert(CachedBindingInfo<T>.LastInterpretedFailure.CompiledTarget != null);
                    var res = CachedBindingInfo<T>.LastInterpretedFailure.CompiledTarget;
                    CachedBindingInfo<T>.LastInterpretedFailure = null;
                    return res;
                }

                // we haven't produced a rule yet....
                var bindingInfo = new CachedBindingInfo<T>(binder, compilationThreshold);

                var targetMO = DynamicMetaObject.Create(args[0], _parameters[1]); // 1 is skipping CallSite
                DynamicMetaObject[] argsMO = new DynamicMetaObject[args.Length - 1];
                for (int i = 0; i < argsMO.Length; i++) {
                    argsMO[i] = DynamicMetaObject.Create(args[i + 1], _parameters[i + 2]);
                }
                var binding = binder.Bind(targetMO, argsMO);

                return CreateDelegate(binding, bindingInfo);
            }

            private T/*!*/ CreateDelegate(DynamicMetaObject/*!*/ binding, CachedBindingInfo<T>/*!*/ bindingInfo) {
                return Compile(binding, bindingInfo).LightCompile(Int32.MaxValue);
            }

            private Expression<T>/*!*/ Compile(DynamicMetaObject/*!*/ obj, CachedBindingInfo<T>/*!*/ bindingInfo) {
                var restrictions = obj.Restrictions.ToExpression();

                var body = Expression.Condition(
                    new InterpretedRuleHitCheckExpression(restrictions, bindingInfo),
                    AstUtils.Convert(obj.Expression, _updateExpression.Type),
                    _updateExpression
                );

                var res = Expression.Lambda<T>(
                    body,
                    "CallSite.Target",
                    true, // always compile the rules with tail call optimization
                    _parameters
                );

                bindingInfo.Target = res;
                return res;
            }

            /// <summary>
            /// Expression which reduces to the normal test but under the interpreter adds a count down
            /// check which enables compiling when the count down is reached.
            /// </summary>
            class InterpretedRuleHitCheckExpression : Expression, IInstructionProvider {
                private readonly Expression/*!*/ _test;
                private readonly CachedBindingInfo/*!*/ _bindingInfo;

                private static readonly MethodInfo InterpretedCallSiteTest = typeof(ScriptingRuntimeHelpers).GetMethod("InterpretedCallSiteTest");
                public InterpretedRuleHitCheckExpression(Expression/*!*/ test, CachedBindingInfo/*!*/ bindingInfo) {
                    Assert.NotNull(test, bindingInfo);

                    _test = test;
                    _bindingInfo = bindingInfo;
                }

                public override Expression Reduce() {
                    return _test;
                }

                protected override Expression VisitChildren(ExpressionVisitor visitor) {
                    var test = visitor.Visit(_test);
                    if (test != _test) {
                        return new InterpretedRuleHitCheckExpression(test, _bindingInfo);
                    }
                    return this;
                }

                public override bool CanReduce {
                    get { return true; }
                }

                public override ExpressionType NodeType {
                    get { return ExpressionType.Extension; }
                }

                public override Type Type {
                    get { return typeof(bool); }
                }

                #region IInstructionProvider Members

                public void AddInstructions(LightCompiler compiler) {
                    compiler.Compile(_test);
                    compiler.Instructions.EmitLoad(_bindingInfo);
                    compiler.EmitCall(InterpretedCallSiteTest);
                }

                #endregion
            }
        }
    }


    /// <summary>
    /// Base class for storing information about the binding that a specific rule is applicable for.
    /// 
    /// We have a derived generic class but this class enables us to refer to it w/o having the
    /// generic type information around.
    /// 
    /// This class tracks both the count down to when we should compile.  When we compile we
    /// take the Expression[T] that was used before and compile it.  While this is happening
    /// we continue to allow the interpreted code to run.  When the compilation is complete we
    /// store a thread static which tells us what binding failed and the current rule is no
    /// longer functional.  Finally the language binder will call us again and we'll retrieve
    /// and return the compiled overload.
    /// </summary>
    abstract class CachedBindingInfo {
        public readonly DynamicMetaObjectBinder/*!*/ Binder;
        public int CompilationThreshold;

        public CachedBindingInfo(DynamicMetaObjectBinder binder, int compilationThreshold) {
            Binder = binder;
            CompilationThreshold = compilationThreshold;
        }

        public abstract bool CheckCompiled();
    }

    class CachedBindingInfo<T> : CachedBindingInfo where T : class {
        public T CompiledTarget;
        public Expression<T> Target;

        [ThreadStatic]
        public static CachedBindingInfo<T> LastInterpretedFailure;

        public CachedBindingInfo(DynamicMetaObjectBinder binder, int compilationThreshold)
            : base(binder, compilationThreshold) {
        }

        public override bool CheckCompiled() {
            if (Target != null) {
                // start compiling the target if no one else has
                var lambda = Interlocked.Exchange(ref Target, null);
                if (lambda != null) {
#if FEATURE_TASKS
                    new Task(() => { CompiledTarget = lambda.Compile(); }).Start();
#else
                    ThreadPool.QueueUserWorkItem(x => { CompiledTarget = lambda.Compile(); });
#endif
                }
            }

            if (CompiledTarget != null) {
                LastInterpretedFailure = this;
                return false;
            }

            return true;
        }
    }
}
