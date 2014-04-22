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

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class LightLambdaExpression : Expression {
        private readonly Expression _body;
        private readonly Type _retType;
        private readonly string _name;
        private readonly IList<ParameterExpression> _args;

        internal LightLambdaExpression(Type retType, Expression body, string name, IList<ParameterExpression> args) {
            _body = body;
            _name = name;
            _args = args;
            _retType = retType;
        }

        public Expression Body {
            get {
                return _body;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        public IList<ParameterExpression> Parameters {
            get {
                return _args;
            }
        }

        internal virtual LambdaExpression ReduceToLambdaWorker() {
            throw new InvalidOperationException();
        }

        public Delegate Compile() {
            return Compile(-1);
        }

        public Delegate Compile(int compilationThreshold) {
            return new LightCompiler(compilationThreshold).CompileTop(this).CreateDelegate();
        }

        public override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override Expression Reduce() {
            return ReduceToLambdaWorker();
        }

        public Type ReturnType {
            get {
                return _retType;
            }
        }
    }

    internal class TypedLightLambdaExpression : LightLambdaExpression {
        private readonly Type _delegateType;

        internal TypedLightLambdaExpression(Type retType, Type delegateType, Expression body, string name, IList<ParameterExpression> args)
            : base(retType, body, name, args) {
            _delegateType = delegateType;
        }

        internal override LambdaExpression ReduceToLambdaWorker() {
            return Expression.Lambda(
                _delegateType,
                Body,
                Name,
                Parameters
            );
        }

        public override Type Type {
            get { return _delegateType; }
        }
    }

    public class LightExpression<T> : LightLambdaExpression {
        internal LightExpression(Type retType, Expression body, string name, IList<ParameterExpression> args)
            : base(retType, body, name, args) {
        }

        public Expression<T> ReduceToLambda() {
            return Expression.Lambda<T>(Body, Name, Parameters);
        }

        public override Type Type {
            get { return typeof(T); }
        }

        public new T Compile() {
            return Compile(-1);
        }

        public new T Compile(int compilationThreshold) {
            return (T)(object)new LightCompiler(compilationThreshold).CompileTop(this).CreateDelegate();
        }

        internal override LambdaExpression ReduceToLambdaWorker() {
            return ReduceToLambda();
        }
    }

    public static partial class Utils {
        public static LightExpression<T> LightLambda<T>(Type retType, Expression body, string name, IList<ParameterExpression> args) {
            return new LightExpression<T>(retType, body, name, args);
        }

        public static LightLambdaExpression LightLambda(Type retType, Type delegateType, Expression body, string name, IList<ParameterExpression> args) {
            return new TypedLightLambdaExpression(retType, delegateType, body, name, args);
        }
    }

}
