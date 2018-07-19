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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Reflection;
using System.Runtime.CompilerServices;

#if SILVERLIGHT
using System.Core;
#endif

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif
    using Compiler;

    /// <summary>
    /// Represents a dynamic operation.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.DynamicExpressionProxy))]
#endif
    public class DynamicExpression : Expression, IDynamicExpression
    {
        private readonly CallSiteBinder _binder;
        private readonly Type _delegateType;

        internal DynamicExpression(Type delegateType, CallSiteBinder binder) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == typeof(object) || GetType() != typeof(DynamicExpression));
            _delegateType = delegateType;
            _binder = binder;
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments) {
            if (returnType == typeof(object)) {
                return new DynamicExpressionN(delegateType, binder, arguments);
            } else {
                return new TypedDynamicExpressionN(returnType, delegateType, binder, arguments);
            }
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0) {
            if (returnType == typeof(object)) {
                return new DynamicExpression1(delegateType, binder, arg0);
            } else {
                return new TypedDynamicExpression1(returnType, delegateType, binder, arg0);
            }
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) {
            if (returnType == typeof(object)) {
                return new DynamicExpression2(delegateType, binder, arg0, arg1);
            } else {
                return new TypedDynamicExpression2(returnType, delegateType, binder, arg0, arg1);
            }
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) {
            if (returnType == typeof(object)) {
                return new DynamicExpression3(delegateType, binder, arg0, arg1, arg2);
            } else {
                return new TypedDynamicExpression3(returnType, delegateType, binder, arg0, arg1, arg2);
            }
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            if (returnType == typeof(object)) {
                return new DynamicExpression4(delegateType, binder, arg0, arg1, arg2, arg3);
            } else {
                return new TypedDynamicExpression4(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public override Type Type {
            get { return typeof(object); }
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Dynamic; }
        }

        /// <summary>
        /// Gets the <see cref="CallSiteBinder" />, which determines the runtime behavior of the
        /// dynamic site.
        /// </summary>
        public CallSiteBinder Binder {
            get { return _binder; }
        }

        /// <summary>
        /// Gets the type of the delegate used by the <see cref="CallSite" />.
        /// </summary>
        public Type DelegateType {
            get { return _delegateType; }
        }

        /// <summary>
        /// Gets the arguments to the dynamic operation.
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments {
            get { return GetOrMakeArguments(); }
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments() {
            throw ContractUtils.Unreachable;
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDynamic(this);
        }

        /// <summary>
        /// Makes a copy of this node replacing the args with the provided values.  The 
        /// number of the args needs to match the number of the current block.
        /// 
        /// This helper is provided to allow re-writing of nodes to not depend on the specific optimized
        /// subclass of DynamicExpression which is being used. 
        /// </summary>
        internal virtual DynamicExpression Rewrite(Expression[] args) {
            throw ContractUtils.Unreachable;
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the
        /// supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="arguments">The <see cref="Arguments" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public DynamicExpression Update(IEnumerable<Expression> arguments) {
            if (arguments == Arguments) {
                return this;
            }

            return Expression.MakeDynamic(DelegateType, Binder, arguments);
        }

        #region IArgumentProvider Members

        Expression IArgumentProvider.GetArgument(int index) {
            throw ContractUtils.Unreachable;
        }

        int IArgumentProvider.ArgumentCount {
            get { throw ContractUtils.Unreachable; }
        }

        #endregion

        #region Members that forward to Expression
#if !SILVERLIGHT || FEATURE_NETCORE
        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) {
            return Expression.Dynamic(binder, returnType, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) {
            return Expression.Dynamic(binder, returnType, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) {
            return Expression.Dynamic(binder, returnType, arg0);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) {
            return Expression.Dynamic(binder, returnType, arg0, arg1);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) {
            return Expression.Dynamic(binder, returnType, arg0, arg1, arg2);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <param name="arg3">The fourth argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static new DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return Expression.Dynamic(binder, returnType, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) {
            return Expression.MakeDynamic(delegateType, binder, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) {
            return Expression.MakeDynamic(delegateType, binder, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and one argument.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0) {
            return Expression.MakeDynamic(delegateType, binder, arg0);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and two arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) {
            return Expression.MakeDynamic(delegateType, binder, arg0, arg1);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and three arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) {
            return Expression.MakeDynamic(delegateType, binder, arg0, arg1, arg2);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and four arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <param name="arg3">The fourth argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static new DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return Expression.MakeDynamic(delegateType, binder, arg0, arg1, arg2, arg3);
        }
#endif // !SILVERLIGHT || FEATURE_NETCORE
        #endregion

        Expression IDynamicExpression.Rewrite(Expression[] args)
        {
            return this.Rewrite(args);
        }

        object IDynamicExpression.CreateCallSite()
        {
            return CallSite.Create(this.DelegateType, this.Binder);
        }
    }

    #region Specialized Subclasses

    internal class DynamicExpressionN : DynamicExpression, IArgumentProvider {
        private IList<Expression> _arguments;       // storage for the original IList or readonly collection.  See IArgumentProvider for more info.

        internal DynamicExpressionN(Type delegateType, CallSiteBinder binder, IList<Expression> arguments)
            : base(delegateType, binder) {
            _arguments = arguments;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(ref _arguments);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == ((IArgumentProvider)this).ArgumentCount);

            return Expression.MakeDynamic(DelegateType, Binder, args);
        }
    }

    internal class TypedDynamicExpressionN : DynamicExpressionN {
        private readonly Type _returnType;

        internal TypedDynamicExpressionN(Type returnType, Type delegateType, CallSiteBinder binder, IList<Expression> arguments)
            : base(delegateType, binder, arguments) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == returnType);
            _returnType = returnType;
        }

        public sealed override Type Type {
            get { return _returnType; }
        }
    }

    internal class DynamicExpression1 : DynamicExpression, IArgumentProvider {
        private object _arg0;               // storage for the 1st argument or a readonly collection.  See IArgumentProvider for more info.

        internal DynamicExpression1(Type delegateType, CallSiteBinder binder, Expression arg0)
            : base(delegateType, binder) {
            _arg0 = arg0;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 1;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 1);

            return Expression.MakeDynamic(DelegateType, Binder, args[0]);
        }
    }

    internal sealed class TypedDynamicExpression1 : DynamicExpression1 {
        private readonly Type _retType;

        internal TypedDynamicExpression1(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0)
            : base(delegateType, binder, arg0) {
            _retType = retType;
        }

        public sealed override Type Type {
            get { return _retType; }
        }
    }

    internal class DynamicExpression2 : DynamicExpression, IArgumentProvider {
        private object _arg0;                   // storage for the 1st argument or a readonly collection.  See IArgumentProvider for more info.
        private readonly Expression _arg1;      // storage for the 2nd argument

        internal DynamicExpression2(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 2;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 2);

            return Expression.MakeDynamic(DelegateType, Binder, args[0], args[1]);
        }
    }

    internal sealed class TypedDynamicExpression2 : DynamicExpression2 {
        private readonly Type _retType;

        internal TypedDynamicExpression2(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(delegateType, binder, arg0, arg1) {
            _retType = retType;
        }

        public sealed override Type Type {
            get { return _retType; }
        }
    }

    internal class DynamicExpression3 : DynamicExpression, IArgumentProvider {
        private object _arg0;                       // storage for the 1st argument or a readonly collection.  See IArgumentProvider for more info.
        private readonly Expression _arg1, _arg2;   // storage for the 2nd & 3rd arguments

        internal DynamicExpression3(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                case 2: return _arg2;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 3;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 3);

            return Expression.MakeDynamic(DelegateType, Binder, args[0], args[1], args[2]);
        }
    }

    internal sealed class TypedDynamicExpression3 : DynamicExpression3 {
        private readonly Type _retType;

        internal TypedDynamicExpression3(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
            : base(delegateType, binder, arg0, arg1, arg2) {
            _retType = retType;
        }

        public sealed override Type Type {
            get { return _retType; }
        }
    }

    internal class DynamicExpression4 : DynamicExpression, IArgumentProvider {
        private object _arg0;                               // storage for the 1st argument or a readonly collection.  See IArgumentProvider for more info.
        private readonly Expression _arg1, _arg2, _arg3;    // storage for the 2nd - 4th arguments

        internal DynamicExpression4(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                case 2: return _arg2;
                case 3: return _arg3;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 4;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 4);

            return Expression.MakeDynamic(DelegateType, Binder, args[0], args[1], args[2], args[3]);
        }
    }

    internal sealed class TypedDynamicExpression4 : DynamicExpression4 {
        private readonly Type _retType;

        internal TypedDynamicExpression4(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(delegateType, binder, arg0, arg1, arg2, arg3) {
            _retType = retType;
        }

        public sealed override Type Type {
            get { return _retType; }
        }
    }

    #endregion

    public partial class Expression {

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) {
            return MakeDynamic(delegateType, binder, (IEnumerable<Expression>)arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(MulticastDelegate))) throw Error.TypeMustBeDerivedFromSystemDelegate();

            var method = GetValidMethodForDynamic(delegateType);

            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(method, ExpressionType.Dynamic, ref args);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, args);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and one argument.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(MulticastDelegate))) throw Error.TypeMustBeDerivedFromSystemDelegate();

            var method = GetValidMethodForDynamic(delegateType);
            var parameters = method.GetParametersCached();

            ValidateArgumentCount(method, ExpressionType.Dynamic, 2, parameters);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg0, parameters[1]);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, arg0);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and two arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(MulticastDelegate))) throw Error.TypeMustBeDerivedFromSystemDelegate();

            var method = GetValidMethodForDynamic(delegateType);
            var parameters = method.GetParametersCached();

            ValidateArgumentCount(method, ExpressionType.Dynamic, 3, parameters);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg0, parameters[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg1, parameters[2]);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, arg0, arg1);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and three arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(MulticastDelegate))) throw Error.TypeMustBeDerivedFromSystemDelegate();

            var method = GetValidMethodForDynamic(delegateType);
            var parameters = method.GetParametersCached();

            ValidateArgumentCount(method, ExpressionType.Dynamic, 4, parameters);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg0, parameters[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg1, parameters[2]);
            ValidateDynamicArgument(arg2);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg2, parameters[3]);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, arg0, arg1, arg2);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" /> and four arguments.
        /// </summary>
        /// <param name="delegateType">The type of the delegate used by the <see cref="CallSite" />.</param>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <param name="arg3">The fourth argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.DelegateType">DelegateType</see>,
        /// <see cref="DynamicExpression.Binder">Binder</see>, and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(MulticastDelegate))) throw Error.TypeMustBeDerivedFromSystemDelegate();

            var method = GetValidMethodForDynamic(delegateType);
            var parameters = method.GetParametersCached();

            ValidateArgumentCount(method, ExpressionType.Dynamic, 5, parameters);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg0, parameters[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg1, parameters[2]);
            ValidateDynamicArgument(arg2);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg2, parameters[3]);
            ValidateDynamicArgument(arg3);
            ValidateOneArgument(method, ExpressionType.Dynamic, arg3, parameters[4]);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, arg0, arg1, arg2, arg3);
        }

        private static MethodInfo GetValidMethodForDynamic(Type delegateType) {
            var method = delegateType.GetMethod("Invoke");
            var pi = method.GetParametersCached();
            if (pi.Length == 0 || pi[0].ParameterType != typeof(CallSite)) throw Error.FirstArgumentMustBeCallSite();
            return method;
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) {
            return Dynamic(binder, returnType, (IEnumerable<Expression>)arguments);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg0.Type,
                    DelegateHelpers.NextTypeInfo(typeof(CallSite))
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0);
            }

            return DynamicExpression.Make(returnType, delegateType, binder, arg0);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg1.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg0.Type,
                        DelegateHelpers.NextTypeInfo(typeof(CallSite))
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1);
            }

            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg2.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg1.Type,
                        DelegateHelpers.GetNextTypeInfo(
                            arg0.Type,
                            DelegateHelpers.NextTypeInfo(typeof(CallSite))
                        )
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1, arg2);
            }

            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1, arg2);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arg0">The first argument to the dynamic operation.</param>
        /// <param name="arg1">The second argument to the dynamic operation.</param>
        /// <param name="arg2">The third argument to the dynamic operation.</param>
        /// <param name="arg3">The fourth argument to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);
            ValidateDynamicArgument(arg3);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg3.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg2.Type,
                        DelegateHelpers.GetNextTypeInfo(
                            arg1.Type,
                            DelegateHelpers.GetNextTypeInfo(
                                arg0.Type,
                                DelegateHelpers.NextTypeInfo(typeof(CallSite))
                            )
                        )
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1, arg2, arg3);
            }

            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// Creates a <see cref="DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="CallSiteBinder" />.
        /// </summary>
        /// <param name="binder">The runtime binder for the dynamic operation.</param>
        /// <param name="returnType">The result type of the dynamic expression.</param>
        /// <param name="arguments">The arguments to the dynamic operation.</param>
        /// <returns>
        /// A <see cref="DynamicExpression" /> that has <see cref="NodeType" /> equal to
        /// <see cref="ExpressionType.Dynamic">Dynamic</see> and has the
        /// <see cref="DynamicExpression.Binder">Binder</see> and
        /// <see cref="DynamicExpression.Arguments">Arguments</see> set to the specified values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DynamicExpression.DelegateType">DelegateType</see> property of the
        /// result will be inferred from the types of the arguments and the specified return type.
        /// </remarks>
        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.RequiresNotNull(returnType, "returnType");

            var args = arguments.ToReadOnly();
            ContractUtils.RequiresNotEmpty(args, "args");
            return MakeDynamic(binder, returnType, args);
        }

        private static DynamicExpression MakeDynamic(CallSiteBinder binder, Type returnType, ReadOnlyCollection<Expression> args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            for (int i = 0; i < args.Count; i++) {
                Expression arg = args[i];

                ValidateDynamicArgument(arg);
            }

            Type delegateType = DelegateHelpers.MakeCallSiteDelegate(args, returnType);

            // Since we made a delegate with argument types that exactly match,
            // we can skip delegate and argument validation

            switch (args.Count) {
                case 1: return DynamicExpression.Make(returnType, delegateType, binder, args[0]);
                case 2: return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1]);
                case 3: return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1], args[2]);
                case 4: return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1], args[2], args[3]);
                default: return DynamicExpression.Make(returnType, delegateType, binder, args);
            }
        }

        private static void ValidateDynamicArgument(Expression arg) {
            RequiresCanRead(arg, "arguments");
            var type = arg.Type;
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (type == typeof(void)) throw Error.ArgumentTypeCannotBeVoid();
        }
    }
}
