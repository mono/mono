/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    // Expression fingerprint class
    // Contains information used for generalizing, comparing, and recreating Expression instances
    //
    // Since Expression objects are immutable and are recreated for every invocation of an expression
    // helper method, they can't be compared directly. Fingerprinting Expression objects allows
    // information about them to be abstracted away, and the fingerprints can be directly compared.
    // Consider the process of fingerprinting that all values (parameters, constants, etc.) are hoisted
    // and replaced with dummies. What remains can be decomposed into a sequence of operations on specific
    // types and specific inputs.
    //
    // Some sample fingerprints:
    //
    // 2 + 4 -> OP_ADD(CONST:int, CONST:int):int
    // 2 + 8 -> OP_ADD(CONST:int, CONST:int):int
    // 2.0 + 4.0 -> OP_ADD(CONST:double, CONST:double):double
    //
    // 2 + 4 and 2 + 8 have the same fingerprint, but 2.0 + 4.0 has a different fingerprint since its
    // underlying types differ.
    //
    // "Hello " + "world" -> OP_ADD(CONST:string, CONST:string):string
    // "Hello " + {model} -> OP_ADD(CONST:string, PARAM:string):string
    //
    // These string concatenations have different fingerprints since the inputs are provided differently:
    // one is a hoisted local, the other is a parameter.
    //
    // ({model} ?? "sample").Length -> MEMBER_ACCESS(String.Length, OP_COALESCE(PARAM:string, CONST:string):string):int
    // ({model} ?? "other sample").Length -> MEMBER_ACCESS(String.Length, OP_COALESCE(PARAM:string, CONST:string):string):int
    //
    // These expressions have the same fingerprint.
    internal abstract class ExpressionFingerprint {

        protected ExpressionFingerprint(Expression expression) {
            // since the fingerprints are cached potentially forever, don't keep a reference
            // to the original expression

            NodeType = expression.NodeType;
            Type = expression.Type;
        }

        // the type of expression node, e.g. OP_ADD, MEMBER_ACCESS, etc.
        public ExpressionType NodeType {
            get;
            private set;
        }

        // the CLR type resulting from this expression, e.g. int, string, etc.
        public Type Type {
            get;
            private set;
        }

        internal virtual void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            combiner.AddObject(NodeType);
            combiner.AddObject(Type);
        }

        public static ExpressionFingerprint Create(Expression expression, ParserContext parserContext) {
            {
                BinaryExpression binaryExpression = expression as BinaryExpression;
                if (binaryExpression != null) {
                    return BinaryExpressionFingerprint.Create(binaryExpression, parserContext);
                }
            }

            {
                ConditionalExpression conditionalExpression = expression as ConditionalExpression;
                if (conditionalExpression != null) {
                    return ConditionalExpressionFingerprint.Create(conditionalExpression, parserContext);
                }
            }

            {
                ConstantExpression constantExpression = expression as ConstantExpression;
                if (constantExpression != null) {
                    return ConstantExpressionFingerprint.Create(constantExpression, parserContext);
                }
            }

            {
                MemberExpression memberExpression = expression as MemberExpression;
                if (memberExpression != null) {
                    return MemberExpressionFingerprint.Create(memberExpression, parserContext);
                }
            }

            {
                MethodCallExpression methodCallExpression = expression as MethodCallExpression;
                if (methodCallExpression != null) {
                    return MethodCallExpressionFingerprint.Create(methodCallExpression, parserContext);
                }
            }

            {
                ParameterExpression parameterExpression = expression as ParameterExpression;
                if (parameterExpression != null) {
                    return ParameterExpressionFingerprint.Create(parameterExpression, parserContext);
                }
            }

            {
                UnaryExpression unaryExpression = expression as UnaryExpression;
                if (unaryExpression != null) {
                    return UnaryExpressionFingerprint.Create(unaryExpression, parserContext);
                }
            }

            // unknown expression
            return null;
        }

        public static ReadOnlyCollection<ExpressionFingerprint> Create(IEnumerable<Expression> expressions, ParserContext parserContext) {
            List<ExpressionFingerprint> fingerprints = new List<ExpressionFingerprint>();
            foreach (Expression expression in expressions) {
                ExpressionFingerprint fingerprint = Create(expression, parserContext);
                if (fingerprint == null && expression != null) {
                    // something couldn't be parsed properly
                    return null;
                }
                else {
                    fingerprints.Add(fingerprint);
                }
            }
            return new ReadOnlyCollection<ExpressionFingerprint>(fingerprints);
        }

        public override int GetHashCode() {
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddObject(GetType());
            AddToHashCodeCombiner(combiner);
            return combiner.CombinedHash;
        }

        public override bool Equals(object obj) {
            ExpressionFingerprint other = obj as ExpressionFingerprint;
            if (other == null) {
                return false;
            }

            return (this.NodeType == other.NodeType
                && this.Type == other.Type
                && this.GetType() == other.GetType());
        }

        protected static Expression ToExpression(ExpressionFingerprint fingerprint, ParserContext parserContext) {
            return (fingerprint != null) ? fingerprint.ToExpression(parserContext) : null;
        }

        protected static IEnumerable<Expression> ToExpression(IEnumerable<ExpressionFingerprint> fingerprints, ParserContext parserContext) {
            return from fingerprint in fingerprints select ToExpression(fingerprint, parserContext);
        }

        public abstract Expression ToExpression(ParserContext parserContext);

    }
}
